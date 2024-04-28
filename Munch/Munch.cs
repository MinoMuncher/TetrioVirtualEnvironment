using System.Text.Json;
using ProcessMunch.Exception;
using TetrEnvironment;
using TetrLoader;
using TetrLoader.Enum;
using TetrLoader.JsonClass;
using Environment = TetrEnvironment.Environment;

namespace Munch;

public class Munch
{
	public static IEnumerable<List<CustomStats>?> ProcessReplay(string username, string replayJson)
	{
		var isMulti = Util.IsMulti(ref replayJson);
		var replayData = ReplayLoader.ParseReplay(ref replayJson, isMulti ? ReplayKind.TTRM : ReplayKind.TTR);
		//同一ファイル内で違うゲームバージョンは存在しないものとする
		var version = replayData.GetVersion(0);

		if (version <= 15)
			throw new Exception("not supported");

		var numGames = replayData.GetGamesCount();
		if (!replayData.GetUsernames().Contains(username))
			throw new Exception("存在しないユーザー");

		for (var gameIndex = 0; gameIndex < numGames; gameIndex++)
		{
			var events = replayData.GetReplayEvents(username, gameIndex);

			if (isMulti)
				(replayData as ReplayDataTTRM)?.ProcessReplayData(replayData as ReplayDataTTRM, events);

			Environment env;
			try
			{
				env = new Environment(events, replayData.GetGameType());

				while (env.NextFrame())
				{
				}
			}
			catch
			{
				//TODO: username gameIndex (gameid)
				Console.WriteLine($"failed to parse, skipped:{username}/{gameIndex}");
				continue;
			}

			yield return env.CustomStatsLog;
		}
	}
}