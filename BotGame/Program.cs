using System.Text.Json;
using ChessChallenge.Application;
using Raylib_cs;

Raylib.InitWindow(100, 100, "Chess Coding Challenge - BOT");
Raylib.SetTargetFPS(60);

var cc = new ChallengeController(false);
var botTypeA = ChallengeController.PlayerType.MyBot;
var botTypeB = ChallengeController.PlayerType.EvilBot;
cc.StartNewBotMatch(botTypeA, botTypeB);

while (cc.TotalGameCount - 1 >= cc.botMatchGameIndex)
{
    cc.Update();
}

var dirName = Directory.GetCurrentDirectory(); // Starting Dir
var fileInfo = new FileInfo(dirName);
var parentDir = fileInfo.Directory.Parent.Parent;
var parentDirName = parentDir.FullName;
var path = Path.Combine(@"..\..", parentDirName, "data");

if (!Directory.Exists(path))
{
    Directory.CreateDirectory(path);
}

if (cc.BotStatsA.NumWins > cc.BotStatsB.NumWins)
{
}

var filename = $"{botTypeA}_{botTypeB}";
cc.BotStatsA.BotName = filename;
var jsonString = JsonSerializer.Serialize(cc.BotStatsA);
File.WriteAllText(Path.Combine(path, $"{filename}.json"), jsonString);

var filename2 = $"{botTypeB}_{botTypeA}";
cc.BotStatsB.BotName = filename2;
var jsonString2 = JsonSerializer.Serialize(cc.BotStatsB);
File.WriteAllText(Path.Combine(path, $"{filename2}.json"), jsonString2);
    