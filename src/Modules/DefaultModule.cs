using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Diagnostics;

namespace Wumpus.Modules
{
	public class DefaultModule : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService _service;

		public DefaultModule(CommandService service) 
		{
			_service = service;
		}

		[Command("ping"), Alias("status")]
		public async Task Ping()
		{
			var sw = Stopwatch.StartNew();
			int latency = Context.Client.Latency;
			var status =
				(Context.Client.ConnectionState == ConnectionState.Disconnected || latency > 500)
					? "BAD"
					: (Context.Client.ConnectionState == ConnectionState.Connecting || latency > 250)
						? "DECENT"
						: "GOOD";
			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = $"My connection is {status}"
			};

			var reply = await ReplyAsync("Calculating...", false, builder.Build());
			await reply.ModifyAsync(p =>
			{
				p.Content = $"Latency: {latency}" +
							$"\nMessage response time: {sw.ElapsedMilliseconds} ms" +
							$"\nDelta: {sw.ElapsedMilliseconds - latency} ms";
			});
		}

		[Command("help")]
		public async Task HelpAsync()
		{
			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = $"Commands I've found that are usable by {Context.User.Mention}"
			};

			foreach (var module in _service.Modules)
			{
				string description = null;
				foreach (var cmd in module.Commands)
				{
					var result = await cmd.CheckPreconditionsAsync(Context);
					if (result.IsSuccess)
						description += $"{cmd.Aliases.First()}\n";
				}

				if (!string.IsNullOrWhiteSpace(description))
				{
					builder.AddField(x =>
					{
						x.Name = module.Name;
						x.Value = description;
						x.IsInline = false;
					});
				}
			}

			await ReplyAsync("", false, builder.Build());
		}

		[Command("help")]
		public async Task HelpAsync(string command)
		{
			var result = _service.Search(Context, command);

			if (!result.IsSuccess)
			{
				await ReplyAsync($"Sorry, I couldn't find a command like **{command}**");
				return;
			}

			var builder = new EmbedBuilder()
			{
				Color = new Color(114, 137, 218),
				Description = $"Commands I've found like {Format.Bold(command)}"
			};

			foreach (var match in result.Commands)
			{
				var cmd = match.Command;

				builder.AddField(x =>
				{
					x.Name = string.Join(", ", cmd.Aliases);
					x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
							  $"Remarks: {cmd.Remarks}";
					x.IsInline = false;
				});
			}

			await ReplyAsync("", false, builder.Build());
		}
	}
}
