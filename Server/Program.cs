// See https://aka.ms/new-console-template for more information
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using System.Text.Json;
using TetrEnvironment;
using TetrLoader;
using TetrLoader.Enum;

namespace SimpleWebServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    new WebHostBuilder()
        .UseKestrel(options =>
        {
            options.ListenAnyIP(8080); // Change the port number here
            options.Limits.MaxRequestBodySize = null;
        })
        .UseStartup<Startup>();
    }


    public class Startup
    {

        public void Configure(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                if (context.Request.Method == "POST")
                {

                    string content = await new System.IO.StreamReader(context.Request.Body).ReadToEndAsync();
                    var IsMulti = Util.IsMulti(ref content);
                    var replayData = ReplayLoader.ParseReplay(content, IsMulti ? ReplayKind.TTRM : ReplayKind.TTR);
                    var replayCount = replayData.GetGamesCount();
                    var failLoads = 0;
                    Dictionary<string, List<List<CustomStats>>> playerLogs = new();
                    Replay replay = new Replay(replayData);

                    var usernames = replayData.GetUsernames();

                    for (int i = 0; i < replayData.GetGamesCount(); i++)
                    {
                        try
                        {
                            replay.LoadGame(i);
                            while (replay.NextFrame()) { }
                            foreach (var env in replay.Environments)
                            {
                                var username = IsMulti ? env.Username ?? "" : usernames[0];
                                if (!playerLogs.ContainsKey(username)) playerLogs.Add(username, new());
                                List<List<CustomStats>> value;
                                playerLogs.TryGetValue(username, out value);
                                value.Add(env.CustomStatsLog);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            failLoads += 1;
                        }
                    }

                    Dictionary<string, object> output = new();
                    output.Add("brokenGames", failLoads);
                    output.Add("playerLogs", playerLogs);
                    output.Add("totalGames", replayCount);

                    context.Response.ContentType = "application/json";
                    string outputString = JsonSerializer.Serialize(output);
                    await context.Response.WriteAsync(outputString);
                }
                else
                {
                    context.Response.StatusCode = 405; // Method Not Allowed
                    await context.Response.WriteAsync("Only POST requests are supported.");
                }
            });
        }
    }
}
