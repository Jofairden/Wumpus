using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Diagnostics;
using Discord.WebSocket;
using Wumpus.Common;

namespace Wumpus.Modules
{
	public class DefaultModule : ModuleBase<SocketCommandContext>
	{
		private readonly CommandService _service;
		private readonly IDependencyMap _map;

		public DefaultModule(CommandService service, IDependencyMap map) 
		{
			_service = service;
			_map = map;
		}

		[Command("leaveall")]
		[RequireContext(ContextType.DM)]
		[RequireOwner]
		public async Task Leaveall()
		{
			foreach (var guild in Context.Client.Guilds)
			{
				await guild.LeaveAsync();
			};
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

		[Group("wumpi"), Name("wumpi")]
		public class Wumpi : ModuleBase<SocketCommandContext>
		{
			private readonly CommandService _service;
			private readonly IDependencyMap _map;

			public Wumpi(CommandService service, IDependencyMap map)
			{
				_service = service;
				_map = map;
			}

			[Command("gift")]
			[RequireUserPermission(GuildPermission.Administrator)]
			public async Task Gift(double points)
			{
				var guild = Context.Guild;
				var configs = await UserConfig.GetAll();
				uint givePoints =
					points > uint.MaxValue
					? uint.MaxValue
					: (uint)points;
				foreach (var config in configs)
				{
					config.GivePoints(Context.Guild.Id, givePoints);
				}

				await ReplyAsync($"Gifted {points} wumpoints to {configs.Count} users.");
			}

			[Command("gift")]
			[RequireUserPermission(GuildPermission.Administrator)]
			public async Task Gift(IGuildUser user, double points)
			{
				var config = await new UserConfig(user.Id).Maintain<UserConfig>();
				uint givePoints = 
					points > uint.MaxValue
					? uint.MaxValue
					: (uint)points;
				config.GivePoints(user.Guild.Id, givePoints);
				await ReplyAsync($"Gifted {givePoints} wumpoints to {user.Username}");
			}

			[Command("wealth")]
			public async Task Wealth()
			{
				var configs = await UserConfig.GetAll();
				var filtered = configs
					.Where(x => x.Wumpoints.ContainsKey(Context.Guild.Id))
					.OrderByDescending(x => x.Wumpoints[Context.Guild.Id])
					.Take(11);

				await ReplyAsync($"Showing top 10 wealthiest people\n\n" +
								string.Join("\n", filtered.Select(x => $"{Format.Bold(Context.Guild.GetUser(x.UID)?.Username ?? "Not found")} with `{x.Wumpoints[Context.Guild.Id]}` wumpoints")));
			}
		}
	}
}
