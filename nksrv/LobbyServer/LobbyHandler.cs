using ASodium;
using EmbedIO;
using Google.Protobuf;
using nksrv.Utils;
using Swan.Logging;
using static Google.Rpc.Context.AttributeContext.Types;

namespace nksrv.LobbyServer
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
                        Logger.Error("WARNING: Failed to get attribute for " + type.FullName);
                        continue;
                    }


                    var instance = Activator.CreateInstance(type);
                    if (instance is LobbyMsgHandler handler)
                    {
                        Handlers.Add(attrib.Url, handler);
                    }
                    else
                    {
                        Logger.Error($"WARNING: Type {type.FullName} has PacketPathAttribute but does not implement LobbyMsgHandler");
                    }
                }
            }
        }
        public static async Task DispatchSingle(IHttpContext ctx)
        {
            //var x = new RedirectionHandler();
            //await x.HandleAsync(ctx);
            //return;
            LobbyMsgHandler? handler = null;
            foreach (var item in Handlers)
            {
                if (ctx.RequestedPath == item.Key)
                {
                    handler = item.Value;
                }
            }

            if (handler == null)
            {
                ctx.Response.StatusCode = 404;
                //Logger.Error("HTTPS: No handler for /v1/" + ctx.RequestedPath);
            }
            else
            {
                // todo move everClass1.csything to its own handler
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
        public static GameClientInfo GenGameClientTok(ByteString publicKey, string authToken)
        {
            var token = Rng.RandomString(381);

            var info = new GameClientInfo() { ClientPublicKey = publicKey.ToArray() };


            var box = SodiumKeyExchange.CalculateServerSharedSecret(JsonDb.ServerPublicKey, JsonDb.ServerPrivateKey, publicKey.ToArray());

            info.Keys = box;
            info.ClientAuthToken = token;

            // look up user id
            foreach (var user in JsonDb.Instance.LauncherAccessTokens)
            {
                if (user.Token == authToken)
                {
                    info.UserId = user.UserID;
                }
            }
            if (info.UserId == 0)
                throw new Exception("expected user account");

            JsonDb.Instance.GameClientTokens.Add(token, info);
            JsonDb.Save();
            return info;
        }

        public static GameClientInfo? GetInfo(string token)
        {
            return JsonDb.Instance.GameClientTokens[token];
        }

        public static void Init()
        {
            // By calling this function, we force .NET to initialize handler dictanary to catch errors early on.
        }

        public static NetUserData CreateNetUserDataFromUser(User user)
        {
            NetUserData ret = new()
            {
                Lv = 1,
                CommanderRoomJukebox = 5,
                CostumeLv = 1,
                Frame = 1,
                Icon = user.ProfileIconId,
                IconPrism = user.ProfileIconIsPrism,
                LobbyJukebox = 2,
                InfraCoreExp = user.InfraCoreExp,
                InfraCoreLv = user.InfraCoreLvl,
            };


            // Restore completed tutorials.
            foreach (var item in user.ClearedTutorialData)
            {
                int groupId = item.Value.GroupId;
                int version = item.Value.VersionGroup;

                ret.Tutorials.Add(new NetTutorialData() { GroupId = groupId, LastClearedTid = groupId, LastClearedVersion = version });
            }

            return ret;
        }
        public static NetWholeUserData CreateWholeUserDataFromDbUser(User user)
        {
            var ret = new NetWholeUserData()
            {
                Lv = 1,
                Frame = 1,
                Icon = user.ProfileIconId,
                IconPrism = user.ProfileIconIsPrism,
                Nickname = user.Nickname,
                Usn = (long)user.ID,
                LastActionAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                
            };

            return ret;
        }
    }

    public class GameClientInfo
    {
        /// <summary>
        /// Client public key generated by game client
        /// </summary>
        public byte[] ClientPublicKey;
        /// <summary>
        /// Authentication token
        /// </summary>
        public string ClientAuthToken;
        /// <summary>
        /// Rx/Tx key pair
        /// </summary>
        public SodiumKeyExchangeSharedSecretBox Keys;
        /// <summary>
        /// User ID of the user
        /// </summary>
        public ulong UserId;

    }
}
