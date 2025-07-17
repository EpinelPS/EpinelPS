using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using DnsClient;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.LobbyServer.Stage;
using EpinelPS.Models.Admin;
using Google.Protobuf;

namespace EpinelPS.Utils
{
    public class AdminCommands
    {
        private static HttpClient client;

        private static string serverUrl = "global-lobby.nikke-kr.com";
        private static string connectingServer = serverUrl;
        private static string? serverIp;
        private static string? staticDataUrl;
        private static string? resourcesUrl;
        static AdminCommands()
        {
            // Use TLS 1.1 so that tencents cloudflare knockoff wont complain
            if (!OperatingSystem.IsLinux())
            {
                var handler = new SocketsHttpHandler
                {
                    ConnectCallback = async (context, cancellationToken) =>
                    {
                        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                        try
                        {
                            await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);

                            var sslStream = new SslStream(new NetworkStream(socket, ownsSocket: true));

                            // When using HTTP/2, you must also keep in mind to set options like ApplicationProtocols
                            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                            {
                                TargetHost = connectingServer,
                                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls11

                            }, cancellationToken);

                            return sslStream;
                        }
                        catch
                        {
                            socket.Dispose();
                            throw;
                        }
                    }
                };

                client = new(handler);
            }
            else
            {
                client = new();
            }
            client.DefaultRequestHeaders.Add("Accept", "application/octet-stream+protobuf");
        }
        public static RunCmdResponse CompleteStage(ulong userId, string input2)
        {
            var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == userId);
            if (user == null) return new RunCmdResponse() { error = "invalid user ID" };

            try
            {
                var chapterParsed = int.TryParse(input2.Split('-')[0], out int chapterNumber);
                var stageParsed = int.TryParse(input2.Split('-')[1], out int stageNumber);

                if (chapterParsed && stageParsed)
                {
                    Console.WriteLine($"Chapter number: {chapterNumber}, Stage number: {stageNumber}");

                    // Complete main stages
                    for (int i = 0; i <= chapterNumber; i++)
                    {
                        var stages = GameData.Instance.GetStageIdsForChapter(i, true);
                        int target = 1;
                        foreach (var item in stages)
                        {
                            if (!user.IsStageCompleted(item, true))
                            {
                                Console.WriteLine("Completing stage " + item);
                                ClearStage.CompleteStage(user, item, true);
                            }

                            if (i == chapterNumber && target == stageNumber)
                            {
                                break;
                            }

                            target++;
                        }
                    }

                    // Process scenario and regular stages
                    Console.WriteLine($"Processing stages for chapters 0 to {chapterNumber}");

                    for (int chapter = 0; chapter <= chapterNumber; chapter++)
                    {
                        Console.WriteLine($"Processing chapter: {chapter}");

                        var stages = GameData.Instance.GetScenarioStageIdsForChapter(chapter)
                            .Where(stageId => GameData.Instance.IsValidScenarioStage(stageId, chapterNumber, stageNumber))
                            .ToList();

                        Console.WriteLine($"Found {stages.Count} stages for chapter {chapter}");

                        foreach (var stage in stages)
                        {
                            if (!user.CompletedScenarios.Contains(stage))
                            {
                                user.CompletedScenarios.Add(stage);
                                Console.WriteLine($"Added stage {stage} to CompletedScenarios");
                            }
                            else
                            {
                                Console.WriteLine($"Stage {stage} is already completed");
                            }
                        }
                    }

                    // Save changes to user data
                    JsonDb.Save();
                }
                else
                {
                    return new RunCmdResponse() { error = "Chapter and stage number must be valid integers" };
                }
            }
            catch (Exception ex)
            {
                return new RunCmdResponse() { error = "Exception: " + ex.ToString() };
            }

            return RunCmdResponse.OK;
        }

        public static RunCmdResponse AddAllCharacters(User user)
        {
            // Group characters by name_code and always add those with grade_core_id == 11, 103, and include grade_core_id == 201
            var allCharacters = GameData.Instance.CharacterTable.Values
                .GroupBy(c => c.name_code)  // Group by name_code to treat same name_code as one character                     3999 = marian
                .SelectMany(g => g.Where(c => c.grade_core_id == 1 || c.grade_core_id == 101 || c.grade_core_id == 201 || c.name_code == 3999))  // Always add characters with grade_core_id == 11 and 103
                .ToList();

            foreach (var character in allCharacters)
            {
                if (!user.HasCharacter(character.id))
                {
                    user.Characters.Add(new Database.Character()
                    {
                        CostumeId = 0,
                        Csn = user.GenerateUniqueCharacterId(),
                        Grade = 0,
                        Level = 1,
                        Skill1Lvl = 1,
                        Skill2Lvl = 1,
                        Tid = character.id,  // Tid is the character ID
                        UltimateLevel = 1
                    });

                    user.BondInfo.Add(new() { NameCode = character.name_code, Lv = 1 });
                    user.AddTrigger(TriggerType.ObtainCharacter, 1, character.name_code);
                    user.AddTrigger(TriggerType.ObtainCharacterNew, 1);
                }
            }

            JsonDb.Save();

            return RunCmdResponse.OK;
        }

        public static RunCmdResponse AddAllMaterials(User user, int amount)
        {
            foreach (var tableItem in GameData.Instance.itemMaterialTable.Values)
            {
                ItemData? item = user.Items.FirstOrDefault(i => i.ItemType == tableItem.id);

                if (item == null)
                {
                    user.Items.Add(new ItemData
                    {
                        Isn = user.GenerateUniqueItemId(),
                        ItemType = tableItem.id,
                        Level = 1,
                        Exp = 1,
                        Count = amount
                    });
                }
                else
                {
                    item.Count += amount;
                }
            }

            Console.WriteLine($"Added {amount} of all materials to user " + user.Username);
            JsonDb.Save();
            return RunCmdResponse.OK;
        }

        public static RunCmdResponse FinishAllTutorials(User user)
        {
            foreach (var tutorial in GameData.Instance.TutorialTable.Values)
            {
                if (!user.ClearedTutorialData.ContainsKey(tutorial.id))
                {
                    user.ClearedTutorialData.Add(tutorial.id, tutorial);
                }
            }

            Console.WriteLine("Finished all tutorials for user " + user.Username);
            JsonDb.Save();
            return RunCmdResponse.OK;
        }

        public static RunCmdResponse SetCoreLevel(User user, int inputGrade)
        {
            if (!(inputGrade >= 0 && inputGrade <= 11)) return new RunCmdResponse() { error = "core level out of range, must be between 0-12" };

            foreach (var character in user.Characters)
            {
                // Get current character's Tid
                int tid = character.Tid;

                // Get the character data from the character table
                if (!GameData.Instance.CharacterTable.TryGetValue(tid, out var charData))
                {
                    Console.WriteLine($"Character data not found for Tid {tid}");
                    continue;
                }

                int currentGradeCoreId = charData.grade_core_id;
                int nameCode = charData.name_code;
                string originalRare = charData.original_rare;

                // Skip characters with original_rare == "R"
                if (originalRare == "R" || nameCode == 3999)
                {
                    continue;
                }

                // Now handle normal SR and SSR characters
                int maxGradeCoreId = 0;

                // If the character is "SSR", it can have a grade_core_id from 1 to 11
                if (originalRare == "SSR")
                {
                    maxGradeCoreId = 11;  // SSR characters can go from 1 to 11

                    // Calculate the new grade_core_id within the bounds
                    int newGradeCoreId = Math.Min(inputGrade + 1, maxGradeCoreId);  // +1 because inputGrade starts from 0 for SSRs

                    // Find the character with the same name_code and new grade_core_id
                    var newCharData = GameData.Instance.CharacterTable.Values.FirstOrDefault(c =>
                        c.name_code == nameCode && c.grade_core_id == newGradeCoreId);

                    if (newCharData != null)
                    {
                        // Update the character's Tid and Grade
                        character.Tid = newCharData.id;
                        character.Grade = inputGrade;
                    }

                }

                // If the character is "SR", it can have a grade_core_id from 101 to 103
                else if (originalRare == "SR")
                {
                    maxGradeCoreId = 103;  // SR characters can go from 101 to 103

                    // Start from 101 and increment based on inputGrade (inputGrade 0 -> grade_core_id 101)
                    int newGradeCoreId = Math.Min(101 + inputGrade, maxGradeCoreId);  // Starts at 101

                    // Find the character with the same name_code and new grade_core_id
                    var newCharData = GameData.Instance.CharacterTable.Values.FirstOrDefault(c =>
                        c.name_code == nameCode && c.grade_core_id == newGradeCoreId);

                    if (newCharData != null)
                    {
                        // Update the character's Tid and Grade
                        character.Tid = newCharData.id;
                        character.Grade = inputGrade;
                    }

                }
            }

            Console.WriteLine($"Core level of all characters have been set to {inputGrade}");
            JsonDb.Save();

            return RunCmdResponse.OK;
        }


        public static RunCmdResponse SetCharacterLevel(User user, int level)
        {
            if (level > 999 || level <= 0) return new RunCmdResponse() { error = "level must be between 1-999" };
            foreach (var character in user.Characters)
            {
                character.Level = level;
            }
            Console.WriteLine("Set all characters' level to " + level);
            JsonDb.Save();
            return RunCmdResponse.OK;
        }

        public static RunCmdResponse SetSkillLevel(User user, int skillLevel)
        {
            if (skillLevel > 10 || skillLevel < 0) return new RunCmdResponse() { error = "level must be between 1-10" };
            foreach (var character in user.Characters)
            {
                character.UltimateLevel = skillLevel;
                character.Skill1Lvl = skillLevel;
                character.Skill2Lvl = skillLevel;
            }
            Console.WriteLine("Set all characters' skill levels to " + skillLevel);
            JsonDb.Save();
            return RunCmdResponse.OK;
        }

        public static RunCmdResponse AddCharacter(User user, int characterId)
        {
            if (!user.HasCharacter(characterId))
            {
                user.Characters.Add(new Database.Character()
                {
                    CostumeId = 0,
                    Csn = user.GenerateUniqueCharacterId(),
                    Grade = 0,
                    Level = 1,
                    Skill1Lvl = 1,
                    Skill2Lvl = 1,
                    Tid = characterId,
                    UltimateLevel = 1
                });

                Console.WriteLine($"Added character {characterId} to user {user.Username}");
                JsonDb.Save();
                return RunCmdResponse.OK;
            }
            else
            {
                return new RunCmdResponse() { error = $"User {user.Username} already has character {characterId}" };
            }
        }

        public static RunCmdResponse AddItem(User user, int itemId, int amount)
        {
            ItemData? item = user.Items.FirstOrDefault(i => i.ItemType == itemId);

            if (item == null)
            {
                user.Items.Add(new ItemData
                {
                    Isn = user.GenerateUniqueItemId(),
                    ItemType = itemId,
                    Level = 1,
                    Exp = 1,
                    Count = amount
                });
            }
            else
            {
                item.Count += amount;

                if (item.Count < 0)
                {
                    item.Count = 0;
                }
            }

            Console.WriteLine($"Added {amount} of item {itemId} to user {user.Username}");
            JsonDb.Save();
            return RunCmdResponse.OK;
        }

        internal static async Task<RunCmdResponse> UpdateResources()
        {
            Logging.WriteLine("updating static data and resource info...", LogType.Info);
            if (serverIp == null || staticDataUrl == null || resourcesUrl == null)
            {
                serverIp = await AssetDownloadUtil.GetIpAsync(serverUrl);
                staticDataUrl = $"https://{serverIp}/v1/staticdatapack";
                resourcesUrl = $"https://{serverIp}/v1/resourcehosts2";
            }

            if (serverIp == null)
                return new RunCmdResponse() { error = "failed to get real server ip, check internet connection" };

            // Get latest static data info from server
            ResStaticDataPackInfo? staticData = await FetchProtobuf<ResStaticDataPackInfo, ReqStaticDataPackInfo>(staticDataUrl);
            if (staticData == null)
            {
                Logging.WriteLine("failed to fetch static data", LogType.Error);
                return new RunCmdResponse() { error = "failed to fetch static data" };
            }

            // Get latest static data info from server
            ResStaticDataPackInfoV2? staticData2 = await FetchProtobuf<ResStaticDataPackInfoV2, ReqStaticDataPackInfoV2>(staticDataUrl.Replace("staticdatapack", "get-static-data-pack-info"),
                new ReqStaticDataPackInfoV2()
                {
                    Type = StaticDataPackType.Mpk
                });
            if (staticData2 == null)
            {
                Logging.WriteLine("failed to fetch static data (2)", LogType.Error);
                return new RunCmdResponse() { error = "failed to fetch static data (2)" };
            }


            ResGetResourceHosts2? resources = await FetchProtobuf<ResGetResourceHosts2, ReqGetResourceHosts2>(resourcesUrl);
            if (resources == null)
            {
                Logging.WriteLine("failed to fetch resource data", LogType.Error);
                return new RunCmdResponse() { error = "failed to fetch resource data" };
            }

            GameConfig.Root.ResourceBaseURL = resources.BaseUrl;
            GameConfig.Root.StaticData.Salt1 = staticData.Salt1.ToBase64();
            GameConfig.Root.StaticData.Salt2 = staticData.Salt2.ToBase64();
            GameConfig.Root.StaticData.Version = staticData.Version;
            GameConfig.Root.StaticData.Url = staticData.Url;

            GameConfig.Root.StaticDataMpk.Salt1 = staticData2.Salt1.ToBase64();
            GameConfig.Root.StaticDataMpk.Salt2 = staticData2.Salt2.ToBase64();
            GameConfig.Root.StaticDataMpk.Version = staticData2.Version;
            GameConfig.Root.StaticDataMpk.Url = staticData2.Url;
            GameConfig.Save();

            return RunCmdResponse.OK;
        }

        private static async Task<T?> FetchProtobuf<T, I>(string url, IMessage? input = null) where T : IMessage, new() where I : IMessage, new()
        {
            byte[] inputBytes = [];

            if(input != null)
            {
                using MemoryStream ms = new();
                CodedOutputStream stream = new CodedOutputStream(ms);
                input.WriteTo(stream);
                stream.Flush();

                inputBytes = ms.ToArray();
            }

            ByteArrayContent staticDataContent = new(inputBytes);
            client.DefaultRequestHeaders.Host = serverUrl;
            staticDataContent.Headers.Add("Content-Type", "application/octet-stream+protobuf");
            connectingServer = serverUrl;
            HttpResponseMessage? staticDataHttpResponse = await client.PostAsync(url, staticDataContent);
            if (staticDataHttpResponse == null)
            {
                Console.WriteLine($"failed to post {url}");
                return default;
            }

            if (!staticDataHttpResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"POST {url} failed with {staticDataHttpResponse.StatusCode}");
                return default;
            }

            byte[] staticDataHttpResponseBytes = await staticDataHttpResponse.Content.ReadAsByteArrayAsync();

            // Parse response
            T response = new();
            response.MergeFrom(new CodedInputStream(staticDataHttpResponseBytes));


            return response;
        }
    }
}
