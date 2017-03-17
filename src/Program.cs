using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Wumpus.Common;

namespace Wumpus
{
	public class Program
	{
		public static void Main(string[] args) => 
			new Program().Start().GetAwaiter().GetResult();

		// Settings
		public const string version = "r-1.0";
		private const ulong clientid = 292315897467633666;
		private const ulong permissions = 536345663;
		private const string oath2Url = "https://discordapp.com/api/oauth2/authorize";

		// Api
		private DiscordSocketClient _client;
		private CommandHandler _commandHandler;

		public async Task Start()
		{
			// Start bot
			Console.Title = $"Wumpus Bot for Discord by Jofairden";

			await BotConfig.Maintain();

			await Console.Out.WriteLineAsync($"Start date: {DateTime.UtcNow}");
			await Console.Out.WriteLineAsync($"{oath2Url}?client_id={clientid}&scope=bot");

			// Config client
			_client = new DiscordSocketClient(new DiscordSocketConfig()
			{
				LogLevel = LogSeverity.Verbose,
				AlwaysDownloadUsers = true,
				MessageCacheSize = 1000, // per channel
			});

			// Register events
			_client.Log += Client_Log;
			_client.JoinedGuild += Client_JoinedGuild;
			_client.Ready += Client_Ready;
			_client.LatencyUpdated += Client_LatencyUpdated;

			// Login and start
			await _client.LoginAsync(TokenType.Bot, BotConfig.Load().BotToken);
			await _client.StartAsync();

			_commandHandler = new CommandHandler();
			await _commandHandler.Install(_client);

			// Never end app, let bot run.
			await Task.Delay(-1);
		}

		private async Task Client_LatencyUpdated(int older, int newer)
		{
			if (_client == null) return;

			var newStatus =
				(_client.ConnectionState == ConnectionState.Disconnected || newer > 500)
					? UserStatus.DoNotDisturb
					: (_client.ConnectionState == ConnectionState.Connecting || newer > 250)
						? UserStatus.Idle
						: UserStatus.Online;

			await _client.SetStatusAsync(newStatus);
		}

		private async Task Client_Ready()
		{
			await Console.Out.WriteLineAsync($"Wumpus bot ready.");
		}

		private async Task Client_JoinedGuild(SocketGuild guild)
		{
			var dmCh = await guild.Owner.CreateDMChannelAsync();
			await dmCh.SendMessageAsync(
				$"Hey there! I am Wumpus. I just joined your guild, `{Format.Sanitize(guild.Name)}`" +
				$"\n" +
				$"Before we proceed, I will explain to you the things I can help you with. " +
				$"Because you are the server owner, you can enable or disable some of my modules. " +
				$"My standard module `wumpi earnings` should be enabled by default. " +
				$"With this module, users in your server generate `wumpi` for their activity in your server. " +
				$"They can spend their earnings in the wumpushop, buying things such as a role with a certain color for a day. " +
				$"Along with this module comes a wealth metrics system. " +
				$"Users can see the wealthiest members of your guild, those who have the most earnings, those who spend the most and much more!" +
				$"\n\n" +
				$"Are you excited yet?! " +
				$"Shoot me a message saying `wumpus help wumpi` if you want to know more." +
				$"\n\n" +
				$"In case you ever lose me, here's a link to invite me: <https://discordapp.com/api/oauth2/authorize?client_id=292315897467633666&scope=bot>" +
				$"\n" +
				$"Remember that certain modules require me to have certain permissions to function properly." +
				$"\n" +
				$"My status represents my response time. The connection is GOOD if my status is Online (green), it's DECENT if my status is IDLE (yellow) and it's BAD if my status is DND (red)" +
				$"You can also call me using `{_client.CurrentUser.Mention} ping` (or status instead of ping) and I will try to tell you my current status." +
				$"\n" +
				$"Remember that I listen to commands {Format.Bold("if I'm mentioned")}, always mention me before typing a command. This is so that I will always work with other bots.");
		}

		private async Task Client_Log(LogMessage message)
		{
			await Console.Out.WriteLineAsync(
				$"[{message.Severity}] [{message.Source}]: {message.Exception?.ToString() ?? message.Message}");
		}
	}
}
