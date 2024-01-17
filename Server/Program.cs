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
        var replayData = ReplayLoader.ParseReplay(replayString, IsMulti ? ReplayKind.TTRM : ReplayKind.TTR);

        int version;
        if (IsMulti)
        {
            var fullDataString = (replayData as ReplayDataTTRM)?.data?[0]?.replays[0]?.events?.First(ev => ev.type == EventType.Full)?.data?.ToString();
            JsonDocument json = JsonDocument.Parse(fullDataString!);
            json.RootElement.GetProperty("options").TryGetProperty("version", out JsonElement versionElement);
            version = int.Parse(versionElement.ToString());
            if (version < 16)
            {
                writer.WriteLine("false");
                writer.Flush();
                return;
            }
        }
        else
        {
            var fullDataString = (replayData as ReplayDataTTR)?.data?.events?.First(ev => ev.type == EventType.Full)?.data?.ToString();
            JsonDocument json = JsonDocument.Parse(fullDataString!);
            json.RootElement.GetProperty("options").TryGetProperty("version", out JsonElement versionElement);
            version = int.Parse(versionElement.ToString());
            if (version < 15)
            {
                writer.WriteLine("false");
                writer.Flush();
                return;
            }
        }

        writer.WriteLine("true");
        writer.Flush();

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

                var events = replayData.GetReplayEvents(username, j);
                if (events == null)
                {
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