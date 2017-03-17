using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wumpus.Common
{
    public class BotConfig
    {
		[JsonIgnore]
		public static string FileName { get; protected set; } = "dist/config/botconfig.json";
		private static string FilePath => Path.Combine(AppContext.BaseDirectory, FileName);
	    public string BotToken { get; set; } = "";
	    public string TestToken { get; set; } = "";

		public static async Task Maintain()
		{
			// No config file, try create.
			if (!File.Exists(FilePath))
			{
				// Existance check is redundant, it's only created if it doesn't exist
				Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

				// Don't hate on goto!
				ReqToken1:
				await Console.Out.WriteAsync("Bot token: ");
				var token = Console.ReadLine();
				if (string.IsNullOrEmpty(token))
				{
					await Console.Out.WriteLineAsync("Token cannot be null or empty.");
					goto ReqToken1;
				}
				ReqToken2:
				await Console.Out.WriteAsync("Test bot token: ");
				var testToken = Console.ReadLine();
				if (string.IsNullOrEmpty(testToken))
				{
					await Console.Out.WriteLineAsync("Token cannot be null or empty.");
					goto ReqToken2;
				}

				// Create new config, save it with read token.
				new BotConfig { BotToken = token, TestToken = testToken}.SaveJson();
			}
			await Console.Out.WriteLineAsync("Config loaded");
		}

		public void SaveJson() =>
			File.WriteAllText(FilePath, ToJson());

		public static BotConfig Load() =>
			JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText(FilePath));

		public string ToJson() => 
			JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
