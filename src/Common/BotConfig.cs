using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wumpus.Common
{
    public class BotConfig : ConfigBase
    {
		public string BotToken { get; protected set; }
		public string TestToken { get; protected set; }
		public override string FileName =>
			"dist/config/botconfig.json";

	    public BotConfig(string botToken = "", string testToken = "")
	    {
		    BotToken = botToken;
		    TestToken = testToken;
	    }

		public override async Task Behaviour()
		{
			if (!File.Exists(FilePath))
			{
				string botToken = BotToken;
				string testToken = TestToken;

				// Don't hate on goto! :smirk:
				ReqToken1:
				if (!string.IsNullOrEmpty(BotToken)) goto ReqToken2;
				await Console.Out.WriteAsync("Bot token: ");
				botToken = Console.ReadLine();
				if (string.IsNullOrEmpty(botToken))
				{
					await Console.Out.WriteLineAsync("Token cannot be null or empty.");
					goto ReqToken1;
				}
				ReqToken2:
				if (!string.IsNullOrEmpty(BotToken)) goto SetTokens;
				await Console.Out.WriteAsync("Test bot token: ");
				testToken = Console.ReadLine();
				if (string.IsNullOrEmpty(testToken))
				{
					await Console.Out.WriteLineAsync("Token cannot be null or empty.");
					goto ReqToken2;
				}

				SetTokens:
				BotToken = botToken;
				TestToken = testToken;

				this.SaveJson<BotConfig>();
			}
			await Console.Out.WriteLineAsync("Config loaded");
		}
	}
}
