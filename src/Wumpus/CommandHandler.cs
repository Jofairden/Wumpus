using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Wumpus
{
	public class CommandHandler
	{
		private DiscordSocketClient _client;
		private CommandService _service;

		// Install
		public async Task Install(DiscordSocketClient c)
		{
			_client = c;
			_service = new CommandService();                           

			await _service.AddModulesAsync(Assembly.GetEntryAssembly());

			_client.MessageReceived += HandleCommand;
		}

		// Handle command
		private async Task HandleCommand(SocketMessage s)
		{
			var msg = s as SocketUserMessage;
			if (msg == null || s.IsWebhook || s.Author.IsBot)
				return;

			var context = new SocketCommandContext(_client, msg); 

			int argPos = 0;
			bool isAsDM = (msg.Channel as SocketDMChannel) != null;
			bool hasMention = msg.HasMentionPrefix(_client.CurrentUser, ref argPos);
			if (isAsDM || hasMention)
			{
				//typeof(SocketUserMessage).GetProperty("Content", (BindingFlags)36).SetMethod.Invoke(context.Message, new object[] { context.Message.Content.Trim() });
				var result = await _service.ExecuteAsync(context, argPos);

				if (!result.IsSuccess)
					await context.Channel.SendMessageAsync(result.ToString());
			}
		}
	}
}
