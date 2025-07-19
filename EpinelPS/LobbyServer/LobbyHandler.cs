using ASodium;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;
using Paseto.Builder;
using Paseto;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.LobbyServer.Event.EventStory;
using System.Text.Json;

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
                        Logging.WriteLine("WARNING: Failed to get attribute for " + type.FullName, LogType.Warning);
                        continue;
                    }


                    var instance = Activator.CreateInstance(type);
                    if (instance is LobbyMsgHandler handler)
                    {
                        Handlers.Add(attrib.Url, handler);
                    }
                    else
                    {
                        Logging.WriteLine($"WARNING: Type {type.FullName} has PacketPathAttribute but does not implement LobbyMsgHandler", LogType.Warning);
                    }
                }
            }
        }
        public static async Task DispatchSingle(HttpContext ctx)
        {
            LobbyMsgHandler? handler = null;

            string fullPath = ctx.Request.Path.Value ?? throw new Exception();
            string path = fullPath.Replace("/v1", "");
            
            foreach (var item in Handlers)
            {
                if (path == item.Key)
                {
                    handler = item.Value;
                }
            }

            if (handler == null)
            {
                Logging.WriteLine($"Error: Implementation for {path} not found", LogType.Error);
                //ctx.Response.StatusCode = 404;

                // to prevent "reloading" of the game for now, return empty response
                // this may cause more problems later on
                
                await new EmptyHandler().HandleAsync(ctx);
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
                             .Decode(token, new PasetoTokenValidationParameters() { ValidateLifetime = true }) ?? throw new Exception("failed to decrypt");
            var elem = (encryptionToken.Paseto.Payload["data"] as System.Text.Json.JsonElement?) ?? throw new Exception("expected data field in auth token");

            var p = elem.GetString() ?? throw new Exception("auth token cannot be null");

            return JsonSerializer.Deserialize<GameClientInfo>(p ?? throw new Exception("data cannot be null"));
        }

        public static void Init()
        {
            // By calling this function, we force .NET to initialize handler dictanary to catch errors early on.
        }

        public static NetUserData CreateNetUserDataFromUser(User user)
        {
            NetUserData ret = new()
            {
                Lv = user.userPointData.UserLevel,
                Exp = user.userPointData.ExperiencePoint,
                CostumeLv = 1,
                Frame = user.ProfileFrame,
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
                Lv = user.userPointData.UserLevel,
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
        public byte[] ClientPublicKey { get; set; } = [];
        /// <summary>
        /// Authentication token
        /// </summary>
        public string ClientAuthToken { get; set; } = "";
        /// <summary>
        /// Rx/Tx key pair
        /// </summary>
        public SodiumKeyExchangeSharedSecretBox Keys { get; set; } = new();
        /// <summary>
        /// User ID of the user
        /// </summary>
        public ulong UserId{ get; set; }
    }
}
