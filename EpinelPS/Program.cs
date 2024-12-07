using DnsClient;
using EpinelPS.Database;
using EpinelPS.LobbyServer;
using EpinelPS.LobbyServer.Msgs.Stage;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;
using Google.Api;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EpinelPS
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine($"EpinelPS v{Assembly.GetExecutingAssembly().GetName().Version} - https://github.com/EpinelPS/EpinelPS/");
                Console.WriteLine("Targeting Game Version " + GameConfig.Root.GameMaxVer);
                Console.WriteLine("Initializing database");
                JsonDb.Save();

                GameData.Instance.GetAllCostumes(); // force static data to be loaded

                Console.WriteLine("Register handlers");
                LobbyHandler.Init();

                Console.WriteLine("Starting ASP.NET core on port 443");
                new Thread(() =>
                {
                    var builder = WebApplication.CreateBuilder(args);

                    // Configure HTTPS
                    var httpsConnectionAdapterOptions = new HttpsConnectionAdapterOptions
                    {
                        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
                        ClientCertificateMode = ClientCertificateMode.AllowCertificate,
                        ServerCertificate = new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + @"site.pfx")
                    };

                    builder.WebHost.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.Listen(IPAddress.Any, 443,
                            listenOptions =>
                            {
                                listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                                listenOptions.UseHttps(AppDomain.CurrentDomain.BaseDirectory + @"site.pfx", "");
                            });

                        // TODO
                        serverOptions.AllowSynchronousIO = true;
                    });


                    // Add services to the container.

                    builder.Services.AddControllersWithViews();
                    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                    builder.Services.AddEndpointsApiExplorer();
                    builder.Services.AddRouting();

                    builder.Logging.ClearProviders();
                    builder.Logging.AddColorConsoleLogger(configuration =>
                    {
                        // Replace warning value from appsettings.json of "Cyan"
                        configuration.LogLevelToColorMap[LogLevel.Warning] = ConsoleColor.Yellow;
                        // Replace warning value from appsettings.json of "Red"
                        configuration.LogLevelToColorMap[LogLevel.Error] = ConsoleColor.DarkRed;
                    });


                    var app = builder.Build();

                    app.UseDefaultFiles();
                    app.UseStaticFiles();

                    // Configure the HTTP request pipeline.
                    if (app.Environment.IsDevelopment())
                    {

                    }

                    app.UseHttpsRedirection();

                    app.UseAuthorization();
                    app.UseHttpsRedirection();
                    app.UseRouting();
                    app.MapControllerRoute(
               name: "default",
               pattern: "/admin/{controller=Admin}/{action=Dashboard}/{id?}");

                    app.MapControllers();

                    app.MapPost("/$batch", HandleBatchRequests);
                    app.MapGet("/prdenv/{**all}", AssetDownloadUtil.HandleReq);
                    app.MapGet("/PC/{**all}", AssetDownloadUtil.HandleReq);
                    app.MapGet("/media/{**all}", AssetDownloadUtil.HandleReq);

                    // NOTE: pub prefixes shows public (production server), local is local server (does not have any effect), dev is development server, etc.
                    // It does not have any effect, except for the publisher server, which adds a watermark?

                    app.MapGet("/route/{**all}", () => @"{
          ""Config"": [
            {
              ""VersionRange"": {
                ""From"": ""{GameMinVer}"",
                ""To"": ""{GameMaxVer}"",
                ""PackageName"": ""com.proximabeta.nikke""
              },
              ""Route"": [
                {
                  ""WorldId"": 81,
                  ""Name"": ""pub:live-jp"",
                  ""Url"": ""https://jp-lobby.nikke-kr.com/"",
                  ""Description"": ""JAPAN"",
                  ""Tags"": []
                },
                {
                  ""WorldId"": 82,
                  ""Name"": ""pub:live-na"",
                  ""Url"": ""https://us-lobby.nikke-kr.com/"",
                  ""Description"": ""NA"",
                  ""Tags"": []
                },
                {
                  ""WorldId"": 83,
                  ""Name"": ""pub:live-kr"",
                  ""Url"": ""https://kr-lobby.nikke-kr.com/"",
                  ""Description"": ""KOREA"",
                  ""Tags"": []
                },
                {
                  ""WorldId"": 84,
                  ""Name"": ""pub:live-global"",
                  ""Url"": ""https://global-lobby.nikke-kr.com/"",
                  ""Description"": ""GLOBAL"",
                  ""Tags"": []
                },
                {
                  ""WorldId"": 85,
                  ""Name"": ""pub:live-sea"",
                  ""Url"": ""https://sea-lobby.nikke-kr.com/"",
                  ""Description"": ""SEA"",
                  ""Tags"": []
                }
              ]
            },
            {
              ""VersionRange"": {
                ""From"": ""{GameMinVer}"",
                ""To"": ""{GameMaxVer}"",
                ""PackageName"": ""com.gamamobi.nikke""
              },
              ""Route"": [
                {
                  ""WorldId"": 91,
                  ""Name"": ""pub:live-hmt"",
                  ""Url"": ""https://hmt-lobby.nikke-kr.com/"",
                  ""Description"": ""HMT"",
                  ""Tags"": []
                }
              ]
            }
          ]
        }".Replace("{GameMinVer}", GameConfig.Root.GameMinVer).Replace("{GameMaxVer}", GameConfig.Root.GameMaxVer));

                    app.MapGet("/", () => {
                        return $"EpinelPS v{Assembly.GetExecutingAssembly().GetName().Version} - https://github.com/EpinelPS/EpinelPS/";
                    });

                    app.Run();
                }).Start();

                CliLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }

        private static void CliLoop()
        {
            ulong selectedUser = 0;
            string prompt = "# ";
            while (true)
            {
                Console.Write(prompt);

                var input = Console.ReadLine();
                var args = input.Split(' ');
                if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                {

                }
                else if (input == "?" || input == "help")
                {
                    Console.WriteLine("EpinelPS CLI");
                    Console.WriteLine();
                    Console.WriteLine("Commands:");
                    Console.WriteLine("  help - show this help");
                    Console.WriteLine("  ls /users - show all users");
                    Console.WriteLine("  cd (user id) - select user by id");
                    Console.WriteLine("  rmuser - delete selected user");
                    Console.WriteLine("  ban - ban selected user from game");
                    Console.WriteLine("  unban - unban selected user from game");
                    Console.WriteLine("  exit - exit server application");
                    Console.WriteLine("  completestage (chapter num)-(stage number) - complete selected stage and get rewards (and all previous ones). Example completestage 15-1. Note that the exact stage number cleared may not be exact.");
		    Console.WriteLine("  sickpulls (requires selecting user first) allows for all characters to have equal chances of getting pulled");
                    Console.WriteLine("  SetLevel (level) - Set all characters' level (between 1 and 999 takes effect on game and server restart)");
                    Console.WriteLine("  SetSkillLevel (level) - Set all characters' skill levels between 1 and 10 (takes effect on game and server restart)");
                    Console.WriteLine("  addallcharacters - Add all missing characters to the selected user with default levels and skills (takes effect on game and server restart)");
                    Console.WriteLine("  SetCoreLevel (core level / 0-3 sets stars) - Set all characters' grades based on the input (from 0 to 11)");

				}
                else if (input == "ls /users")
                {
                    Console.WriteLine("Id,Username,Nickname");
                    foreach (var item in JsonDb.Instance.Users)
                    {
                        Console.WriteLine($"{item.ID},{item.Username},{item.Nickname}");
                    }
                }
                else if (input.StartsWith("cd"))
                {
                    if (args.Length == 2)
                    {
                        if (ulong.TryParse(args[1], out ulong id))
                        {
                            // check if user id exists
                            var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == id);
                            if (user != null)
                            {
                                selectedUser = user.ID;
                                Console.WriteLine("Selected user: " + user.Username);
                                prompt = "/users/" + user.Username + "# ";
                            }
                            else
                            {
                                Console.WriteLine("User not found");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Argument #1 should be a number");
                            Console.WriteLine("Usage: chroot (user id)");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Incorrect number of arguments for chroot");
                        Console.WriteLine("Usage: chroot (user id)");
                    }
                }
				else if (input == "addallcharacters")
				{
					if (selectedUser == 0)
					{
						Console.WriteLine("No user selected");
					}
					else
					{
						var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
						if (user == null)
						{
							Console.WriteLine("Selected user does not exist");
							selectedUser = 0;
							prompt = "# ";
						}
						else
						{
							// Group characters by name_code and always add those with grade_core_id == 11, 103, and include grade_core_id == 201
							var allCharacters = GameData.Instance.characterTable.Values
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
								}
							}

							Console.WriteLine("Added all missing characters to user " + user.Username);
							JsonDb.Save();
						}
					}
				}
				else if (input.StartsWith("SetCoreLevel"))
				{
					if (selectedUser == 0)
					{
						Console.WriteLine("No user selected");
					}
					else
					{
						var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
						if (user == null)
						{
							Console.WriteLine("Selected user does not exist");
							selectedUser = 0;
							prompt = "# ";
						}
						else if (args.Length == 2 && int.TryParse(args[1], out int inputGrade) && inputGrade >= 0 && inputGrade <= 11)
						{
							foreach (var character in user.Characters)
							{
								// Get current character's Tid
								int tid = character.Tid;

								// Get the character data from the character table
								if (!GameData.Instance.characterTable.TryGetValue(tid, out var charData))
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
									var newCharData = GameData.Instance.characterTable.Values.FirstOrDefault(c =>
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
									var newCharData = GameData.Instance.characterTable.Values.FirstOrDefault(c =>
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
						}
						else
						{
							Console.WriteLine("Invalid argument. Core level must be between 0 and 11.");
						}
					}
					//code above WILL change tids in user.characters so this will update them in representation team
					foreach (var user in JsonDb.Instance.Users)
					{
						// Check if RepresentationTeamData exists and has slots
						if (user.RepresentationTeamData != null && user.RepresentationTeamData.Slots != null)
						{
							// Iterate through RepresentationTeamData slots
							foreach (var slot in user.RepresentationTeamData.Slots)
							{
								// Find the character in user's character list that matches the slot's Csn
								var correspondingCharacter = user.Characters.FirstOrDefault(c => c.Csn == slot.Csn);

								if (correspondingCharacter != null)
								{
									// Update the Tid value if it differs
									if (slot.Tid != correspondingCharacter.Tid)
									{
										slot.Tid = correspondingCharacter.Tid;
									}
								}
							}
						}
					}

					// Save the updated data
					JsonDb.Save();

				}



				else if (input == "sickpulls")
				{
					if (selectedUser == 0)
					{
						Console.WriteLine("No user selected");
					}
					else
					{
						var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
						if (user == null)
						{
							Console.WriteLine("Selected user does not exist");
							selectedUser = 0;
							prompt = "# ";
						}
						else
						{
							// Check current value of sickpulls and toggle it
							bool currentSickPulls = EpinelPS.Database.JsonDb.IsSickPulls(user);
							if (currentSickPulls)
							{
								user.sickpulls = false;
								Console.WriteLine("sickpulls is now set to false for user " + user.Username);
							}
							else
							{
								user.sickpulls = true;
								Console.WriteLine("sickpulls is now set to true for user " + user.Username);
							}

							// Save the changes to the database
							JsonDb.Save();
						}
					}
				}
				else if (input.StartsWith("SetLevel"))
				{
					if (selectedUser == 0)
					{
						Console.WriteLine("No user selected");
					}
					else
					{
						var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
						if (user == null)
						{
							Console.WriteLine("Selected user does not exist");
							selectedUser = 0;
							prompt = "# ";
						}
						else if (args.Length == 2 && int.TryParse(args[1], out int level) && level >= 1 && level <= 999)
						{
							foreach (var character in user.Characters)
							{
								character.Level = level;
							}
							Console.WriteLine("Set all characters' level to " + level);
							JsonDb.Save();
						}
						else
						{
							Console.WriteLine("Invalid argument. Level must be between 1 and 999.");
						}
					}
						//code above WILL change levels in user.characters so this will update them in representation team
					foreach (var user in JsonDb.Instance.Users)
					{
						// Check if RepresentationTeamData exists and has slots
						if (user.RepresentationTeamData != null && user.RepresentationTeamData.Slots != null)
						{
							// Iterate through RepresentationTeamData slots
							foreach (var slot in user.RepresentationTeamData.Slots)
							{
								// Find the character in user's character list that matches the slot's Csn
								var correspondingCharacter = user.Characters.FirstOrDefault(c => c.Csn == slot.Csn);

								if (correspondingCharacter != null)
								{
									// Update the Level value if it differs
									if (slot.Level != correspondingCharacter.Level)
									{
										slot.Level = correspondingCharacter.Level;
									}
								}
							}
						}
					}

						// Save the updated data
						JsonDb.Save();
				}
				else if (input.StartsWith("SetSkillLevel"))
				{
					if (selectedUser == 0)
					{
						Console.WriteLine("No user selected");
					}
					else
					{
						var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
						if (user == null)
						{
							Console.WriteLine("Selected user does not exist");
							selectedUser = 0;
							prompt = "# ";
						}
						else if (args.Length == 2 && int.TryParse(args[1], out int skillLevel) && skillLevel >= 1 && skillLevel <= 10)
						{
							foreach (var character in user.Characters)
							{
								character.UltimateLevel = skillLevel;
								character.Skill1Lvl = skillLevel;
								character.Skill2Lvl = skillLevel;
							}
							Console.WriteLine("Set all characters' skill levels to " + skillLevel);
							JsonDb.Save();
						}
						else
						{
							Console.WriteLine("Invalid argument. Skill level must be between 1 and 10.");
						}
					}
				}

                else if (input.StartsWith("rmuser"))
                {
                    if (selectedUser == 0)
                    {
                        Console.WriteLine("No user selected");
                    }
                    else
                    {
                        var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
                        if (user == null)
                        {
                            Console.WriteLine("Selected user does not exist");
                            selectedUser = 0;
                            prompt = "# ";
                        }
                        else
                        {
                            Console.Write("Are you sure you want to delete user " + user.Username + "? (y/n) ");
                            var confirm = Console.ReadLine();
                            if (confirm == "y")
                            {
                                JsonDb.Instance.Users.Remove(user);
                                JsonDb.Save();
                                Console.WriteLine("User deleted");
                                selectedUser = 0;
                                prompt = "# ";
                            }
                            else
                            {
                                Console.WriteLine("User not deleted");
                            }
                        }
                    }
                }
				else if (input.StartsWith("completestage"))
				{
					if (selectedUser == 0)
					{
						Console.WriteLine("No user selected");
					}
					else
					{
						var user = JsonDb.Instance.Users.FirstOrDefault(x => x.ID == selectedUser);
						if (user == null)
						{
							Console.WriteLine("Selected user does not exist");
							selectedUser = 0;
							prompt = "# ";
						}
						else
						{
							if (args.Length == 2)
							{
								var input2 = args[1];
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
										Console.WriteLine("Chapter and stage number must be valid integers");
									}
								}
								catch (Exception ex)
								{
									Console.WriteLine("Exception: " + ex.ToString());
								}
							}
							else
							{
								Console.WriteLine("Invalid argument length, must be 1");
							}
						}
					}
				}




                else if (input == "exit")
                {
                    Environment.Exit(0);
                }
                else if (input == "ban")
                {
                    Console.WriteLine("Not implemented");
                }
                else if (input == "unban")
                {
                    Console.WriteLine("Not implemented");
                }
                else
                {
                    Console.WriteLine("Unknown command");
                }
            }
        }
        private static string LauncherEndpoint = Encoding.UTF8.GetString(Convert.FromBase64String("L25pa2tlX2xhdW5jaGVy"));


        private static async Task HandleBatchRequests(HttpContext ctx)
        {
            var theBytes = await PacketDecryption.DecryptOrReturnContentAsync(ctx);

            // this actually uses gzip compression, unlike other requests.

            using MemoryStream streamforparser = new(theBytes.Contents);
            StreamContent content = new(streamforparser);
            content.Headers.Remove("Content-Type");
            content.Headers.TryAddWithoutValidation("Content-Type", (string?)ctx.Request.Headers["Content-Type"]);

            // we have the form contents, 
            var multipart = await content.ReadAsMultipartAsync();

            HttpClient cl = new();

            // TODO: the server returns different boundary each time, looks like a GUID
            List<byte> response = [.. Encoding.UTF8.GetBytes("--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n")];

            int i = 0;
            foreach (var item in multipart.Contents)
            {
                i++;
                response.AddRange(Encoding.UTF8.GetBytes("Content-Type: application/http\r\n"));
                response.AddRange(Encoding.UTF8.GetBytes($"Content-ID: {item.Headers.NonValidated["Content-ID"]}\r\n"));
                response.AddRange(Encoding.UTF8.GetBytes("\r\n"));

                var bin = await item.ReadAsByteArrayAsync();
                try
                {
                    var res = await SendReqLocalAndReadResponseAsync(bin);

                    if (res != null)
                    {
                        List<byte> ResponseWithBytes =
    [
        .. Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n"),
                            .. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                            .. Encoding.UTF8.GetBytes($"Content-Length: {res.Length}\r\n"),
                            .. Encoding.UTF8.GetBytes($"\r\n"),
                            .. res,
                        ];
                        response.AddRange([.. ResponseWithBytes]);
                    }
                    else
                    {
                        List<byte> ResponseWithBytes =
    [                   .. Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n"),
                            //.. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                            .. Encoding.UTF8.GetBytes($"Content-Length: 0\r\n"),
                            .. Encoding.UTF8.GetBytes($"\r\n"),
                        ];
                        response.AddRange([.. ResponseWithBytes]);
                    }
                }
                catch(Exception ex)
                {
                    List<byte> ResponseWithBytes =
   [                   .. Encoding.UTF8.GetBytes("HTTP/1.1 500 Internal Server Error\r\n"),
                            //.. Encoding.UTF8.GetBytes($"Content-Type: application/octet-stream+protobuf\r\n"),
                            .. Encoding.UTF8.GetBytes($"Content-Length: 0\r\n"),
                            .. Encoding.UTF8.GetBytes($"\r\n"),
                        ];
                    response.AddRange([.. ResponseWithBytes]);

                    Console.WriteLine("Exception during batch request: " + ex.ToString());
                }

                // add boundary, also include http newline if there is binary content

                if (i == multipart.Contents.Count)
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a--\r\n"));
                else
                    response.AddRange(Encoding.UTF8.GetBytes("\r\n--f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\r\n"));

            }

            var responseBytes = response.ToArray();
            ctx.Response.ContentType = "multipart/mixed; boundary=\"f5d5cf4d-5627-422f-b3c6-532f1a0cbc0a\"";
            ctx.Response.Body.Write(responseBytes);
        }
        public static string GetCachePathForPath(string path)
        {
            return AppDomain.CurrentDomain.BaseDirectory + "cache/" + path;
        }
        private static (string key, string value) GetHeader(string line)
        {
            var pieces = line.Split([':'], 2);

            return (pieces[0].Trim(), pieces[1].Trim());
        }
        private static async Task<byte[]?> SendReqLocalAndReadResponseAsync(byte[] bytes)
        {
            int line = 0;
            var bodyStartStr = Encoding.UTF8.GetString(bytes);

            string method;
            string url = "";
            string httpVer;
            string authToken = "";
            List<NameValueHeaderValue> headers = [];

            int currentByte = 0;

            foreach (var item in bodyStartStr.Split("\r\n"))
            {
                if (line == 0)
                {
                    var parts = item.Split(" ");
                    method = parts[0];
                    url = parts[1];
                    httpVer = parts[2];
                }
                else if (item == null || string.IsNullOrEmpty(item))
                {
                    currentByte += 2;
                    break;
                }
                else
                {
                    var (key, value) = GetHeader(item);
                    headers.Add(new NameValueHeaderValue(key, value));

                    if (key == "Authorization")
                    {
                        authToken = value.Replace("Bearer ", "");
                    }
                }
                currentByte += 2 + item.Length;
                line++;
            }
            byte[] body;
            if (currentByte == bytes.Length)
            {
                // empty body
                body = [];
            }
            else
            {
                body = bytes.Skip(currentByte).ToArray();
            }

            if (!url.StartsWith("/v1/"))
            {
                throw new NotImplementedException("handler for " + url + " not implemented");
            }

            url = url.Replace("/v1", "");

            // find appropriate handler
            Console.WriteLine("BATCH: /v1" + url);

            foreach (var item in LobbyHandler.Handlers)
            {
                if (item.Key == url)
                {
                    item.Value.Reset();
                    item.Value.Contents = body;
                    await item.Value.HandleAsync(authToken);
                    return item.Value.ReturnBytes;
                }
            }

            var fg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("HANDLER NOT FOUND: " + url);
            Console.ForegroundColor = fg;
            return null;
        }
    }
}
