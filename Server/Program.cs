using System.Text.Json;
using TetrLoader.JsonClass;
using TetrLoader;
using TetrLoader.Enum;

using System.Net;
using System.Net.Sockets;
using System.Text;

class TCPServer
{
    static void Main()
    {
        TcpListener? server = null;
        try
        {
            // Set the IP address and port number
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            int port = int.TryParse(Environment.GetEnvironmentVariable("TETRIO_PARSER_PORT"), out int parsedPort) ? parsedPort : 8080;

            // Create a TcpListener
            server = new TcpListener(ipAddress, port);

            // Start listening for client requests
            server.Start();

            Console.WriteLine($"Server listening on {ipAddress}:{port}");

            // Enter the listening loop
            while (true)
            {
                // Accept the TcpClient connection
                TcpClient client = server.AcceptTcpClient();

                // Process the client request
                try
                {
                    HandleClientRequest(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
                client.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical Error: {ex}");
        }
        finally
        {
            // Stop the server if an exception occurs
            server?.Stop();
        }
    }

    static void HandleClientRequest(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new(stream, Encoding.UTF8);
        StreamWriter writer = new(stream, Encoding.UTF8);


        string replayString = reader.ReadLine()!;

        var IsMulti = Util.IsMulti(ref replayString);
        var replayData = ReplayLoader.ParseReplay(ref replayString, IsMulti ? ReplayKind.TTRM : ReplayKind.TTR);

        int version;
        bool work()
        {
            if (IsMulti)
            {
                var fullDataString = (replayData as ReplayDataTTRM)?.data?[0]?.replays[0]?.events?.FirstOrDefault(ev => ev.type == EventType.Full)?.data?.ToString();
                if (fullDataString == null) return false;
                JsonDocument json = JsonDocument.Parse(fullDataString!);
                json.RootElement.GetProperty("options").TryGetProperty("version", out JsonElement versionElement);
                version = int.Parse(versionElement.ToString());
                if (version == 15)
                {
                    foreach (var player in (replayData as ReplayDataTTRM)!.data!)
                    {
                        foreach (var replay in player.replays)
                        {
                            if (replay.events == null) continue;
                            foreach (var ev in replay.events)
                            {
                                if (ev.type != EventType.Ige) continue;
                                var dataString = ev.data!.ToString();
                                JsonDocument dataJson = JsonDocument.Parse(dataString!);
                                dataJson.RootElement.TryGetProperty("type", out JsonElement typeElement);
                                if (typeElement.ToString() is not "ige") continue;
                                dataJson.RootElement.GetProperty("data").TryGetProperty("type", out JsonElement dataTypeElement);
                                if (!(dataTypeElement.ToString() is "interaction" or "interaction_confirm" or "attack")) continue;
                                return !dataJson.RootElement.GetProperty("data").TryGetProperty("lines", out JsonElement _);
                            }
                        }
                    }
                    return true;
                }
                if (version < 15)
                {
                    return false;
                }
            }
            else
            {
                var fullDataString = (replayData as ReplayDataTTR)?.data?.events?.FirstOrDefault(ev => ev.type == EventType.Full)?.data?.ToString();
                if (fullDataString == null) return false;
                JsonDocument json = JsonDocument.Parse(fullDataString!);
                json.RootElement.GetProperty("options").TryGetProperty("version", out JsonElement versionElement);
                version = int.Parse(versionElement.ToString());
                if (version < 15)
                {
                    return false;
                }
            }
            return true;
        }
        if (work())
        {
            writer.WriteLine("true");
            writer.Flush();
        }
        else
        {
            writer.WriteLine("false");
            writer.Flush();
            return;
        }


        var numGames = replayData.GetGamesCount();
        var usernames = replayData.GetUsernames();

        writer.WriteLine(string.Join(' ', usernames));
        writer.WriteLine(numGames.ToString());
        writer.Flush();

        var numUsernames = ushort.Parse(reader.ReadLine()!);


        for (int i = 0; i < numUsernames; i++)
        {
            var usernameString = reader.ReadLine()!;
            var username = usernameString.Trim(new char[] { '\uFEFF', '\u200B' });
            for (int j = 0; j < numGames; j++)
            {
                List<TetrLoader.JsonClass.Event.Event> events;
                try
                {
                    events = replayData.GetReplayEvents(username, j);
                }
                catch (Exception)
                {
                    writer.WriteLine("CORRUPT");
                    writer.Flush();
                    continue;
                }

                if (IsMulti)
                {
                    (replayData as ReplayDataTTRM)?.ProcessReplayData(replayData as ReplayDataTTRM, events);
                }
                var env = new TetrEnvironment.Environment(events, replayData.GetGameType());
                while (env.NextFrame()) { }
                writer.WriteLine(JsonSerializer.Serialize(env.CustomStatsLog));
                writer.Flush();
            }
        }
    }
}