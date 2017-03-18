using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Wumpus.Common
{
	public class UserConfig : ConfigBase
	{
		public ulong UID { get; protected set; }
		public DateTime LastUpdate { get; protected set; } = DateTime.UtcNow;

		public override string FileName =>
			Path.Combine("dist", "users", $"{UID}.json");

		public Dictionary<ulong, uint> Wumpoints { get; set; } = new Dictionary<ulong, uint>();

		public UserConfig(ulong UID)
		{
			this.UID = UID;
		}

		public override async Task Behaviour()
		{
			await Task.Run(() =>
			{
				if (!File.Exists(FilePath))
				{
					LastUpdate = DateTime.UtcNow;
					this.SaveJson<UserConfig>();
				}
			});
		}

		public UserConfig GivePoints(ulong GUID, uint points)
		{
			uint givePoints =
				HasPoints(GUID)
				? points + Wumpoints[GUID]
				: points;

			SetPoints(GUID, givePoints);
			return this;
		}

		public UserConfig TakePoints(ulong GUID, uint points)
		{
			uint value;
			if (Wumpoints.TryGetValue(GUID, out value))
				SetPoints(GUID, value - points);
			return this;
		}

		public UserConfig SetPoints(ulong GUID, uint points)
		{
			Wumpoints[GUID] = Math.Max(0, points);
			this.SaveJson<UserConfig>();
			return this;
		}

		public static Task<IReadOnlyCollection<UserConfig>> GetAll()
		{
			var path = Path.Combine(AppContext.BaseDirectory, "dist", "users");
			var files = Directory.GetFiles(path, "*.json").ToList();
			var configs = new List<UserConfig>();
			files.ForEach(x => configs.Add(Load<UserConfig>(File.ReadAllText(x))));
			return Task.FromResult<IReadOnlyCollection<UserConfig>>(configs.AsReadOnly());
		}

		public bool HasPoints(ulong GUID) =>
			Wumpoints.ContainsKey(GUID) && Wumpoints[GUID] > 0;
	}
}
