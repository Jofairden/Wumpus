using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
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

			// Proceed command handling
			int argPos = 0;
			bool isAsDM = (msg.Channel as SocketDMChannel) != null;
			bool hasMention = msg.HasMentionPrefix(_client.CurrentUser, ref argPos);
			if (isAsDM || hasMention)
			{
				// We don't want whitespaces to be handled, the api does, so we have our own little handling here
				var trimmedString = 
					!hasMention
					// Trim the start, take until we hit a whitespace (or not)
					? new string(context.Message.Content.TrimStart().TakeWhile(c => !char.IsWhiteSpace(c)).ToArray())
					// There's a mention, skip until we reach the argPos, then we trim the start.
					: new string(context.Message.Content.SkipWhile((x, i) => i < argPos).ToArray())?.TrimStart() ?? "Error in command handler";

				var result = await _service.ExecuteAsync(context, trimmedString);

				if (!result.IsSuccess)
					await context.Channel.SendMessageAsync(result.ToString());
			}
		}
	}
}
