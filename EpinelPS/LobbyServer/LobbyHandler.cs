using ASodium;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;
using Paseto.Builder;
using Paseto;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer
{
    public static class LobbyHandler
    {
        public static readonly Dictionary<string, LobbyMsgHandler> Handlers = new Dictionary<string, LobbyMsgHandler>();
        static LobbyHandler()
        {
            foreach (System.Type type in typeof(LobbyMsgHandler).Assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(PacketPathAttribute), true).Length > 0)
                {
                    var attrib = (PacketPathAttribute?)Attribute.GetCustomAttribute(type, typeof(PacketPathAttribute));
                    if (attrib == null)
                    {
                        Console.WriteLine("WARNING: Failed to get attribute for " + type.FullName);
                        continue;
                    }


                    var instance = Activator.CreateInstance(type);
                    if (instance is LobbyMsgHandler handler)
                    {
                        Handlers.Add(attrib.Url, handler);
                    }
                    else
                    {
                        Console.WriteLine($"WARNING: Type {type.FullName} has PacketPathAttribute but does not implement LobbyMsgHandler");
                    }
                }
            }
        }
        public static async Task DispatchSingle(HttpContext ctx)
        {
            LobbyMsgHandler? handler = null;
            string path = ctx.Request.Path.Value.Replace("/v1", "");
            foreach (var item in Handlers)
            {
                if (path == item.Key)
                {
                    handler = item.Value;
                }
            }

            if (handler == null)
            {
                Console.WriteLine("404: " + path);
                ctx.Response.StatusCode = 404;
            }
            else
            {
                handler.Reset();
                await handler.HandleAsync(ctx);
                return;
            }
        }

        /// <summary>
        /// Private key, Token
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static GameClientInfo GenGameClientTok(ByteString publicKey, ulong userid)
        {
            var token = Rng.RandomString(381);

            var info = new GameClientInfo() { ClientPublicKey = publicKey.ToArray() };

            var box = SodiumKeyExchange.CalculateServerSharedSecret(JsonDb.ServerPublicKey, JsonDb.ServerPrivateKey, publicKey.ToArray());

            info.Keys = box;
            info.ClientAuthToken = token;

            if (userid == 0)
                throw new Exception("expected user account");

            info.UserId = userid;

            return info;
        }

        public static GameClientInfo? GetInfo(string token)
        {
            var encryptionToken = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                             .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                             .Decode(token, new PasetoTokenValidationParameters() { ValidateLifetime = true });

            var p = ((System.Text.Json.JsonElement)encryptionToken.Paseto.Payload["data"]).GetString() ?? throw new Exception("auth token cannot be null");

            return JsonConvert.DeserializeObject<GameClientInfo>(p ?? throw new Exception("data cannot be null"));
        }

        public static void Init()
        {
            // By calling this function, we force .NET to initialize handler dictanary to catch errors early on.
        }

        public static NetUserData CreateNetUserDataFromUser(User user)
        {
            NetUserData ret = new()
            {
                Level = user.userPointData.UserLevel,
                Exp = user.userPointData.ExperiencePoint,
                CostumeLv = 1,
                Frame = 1,
                Icon = user.ProfileIconId,
                IconPrism = user.ProfileIconIsPrism,
                InfraCoreExp = user.InfraCoreExp,
                InfraCoreLv = user.InfraCoreLvl,
            };


            // Restore completed tutorials.
            foreach (var item in user.ClearedTutorialData)
            {
                int groupId = item.Value.GroupId;
                int version = item.Value.VersionGroup;

                ret.Tutorials.Add(new NetTutorialData() { GroupId = groupId, LastClearedTid = item.Key, LastClearedVersion = version });
            }

            return ret;
        }
        public static NetWholeUserData CreateWholeUserDataFromDbUser(User user)
        {
            var ret = new NetWholeUserData()
            {
                Level = user.userPointData.UserLevel,
                Frame = user.ProfileFrame,
                Icon = user.ProfileIconId,
                IconPrism = user.ProfileIconIsPrism,
				UserTitleId = user.TitleId,
                Nickname = user.Nickname,
                Usn = (long)user.ID,
                LastActionAt = DateTimeOffset.UtcNow.Ticks,
                Server = 1001
            };

            return ret;
        }
    }

    public class GameClientInfo
    {
        /// <summary>
        /// Client public key generated by game client
        /// </summary>
        public byte[] ClientPublicKey = [];
        /// <summary>
        /// Authentication token
        /// </summary>
        public string ClientAuthToken = "";
        /// <summary>
        /// Rx/Tx key pair
        /// </summary>
        public SodiumKeyExchangeSharedSecretBox Keys = new();
        /// <summary>
        /// User ID of the user
        /// </summary>
        public ulong UserId;
    }
}
