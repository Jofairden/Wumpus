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
		public ulong? UID { get; protected set; } = null;
	    public DateTime LastUpdate { get; protected set; } = DateTime.UtcNow;
		public override string FileName => 
			$"dist{Path.DirectorySeparatorChar}users{Path.DirectorySeparatorChar}{UID}.json";

		public Dictionary<ulong, int> Wumpoints { get; private set; } = new Dictionary<ulong, int>();

	    public UserConfig(ulong UID)
	    {
			this.UID = UID;
	    }

	    public override async Task Behaviour()
	    {
		    await Task.Run(() =>
		    {
			    if (!UID.HasValue) return;
			    if (!File.Exists(FilePath))
			    {
					LastUpdate = DateTime.UtcNow;
					this.SaveJson<UserConfig>();
				}			
		    });
	    }

	    public UserConfig GivePoints(ulong GUID, int points)
	    {
			int givePoints = points;
		    if (HasPoints(GUID))
			    givePoints += Wumpoints[GUID];

		    SetPoints(GUID, givePoints);
			return this;
	    }

	    public UserConfig TakePoints(ulong GUID, int points) =>
		    GivePoints(GUID, -points);

	    public UserConfig SetPoints(ulong GUID, int points)
	    {
			Wumpoints[GUID] = Math.Max(0, points);
		    this.SaveJson<UserConfig>();
			return this;
	    }

	    public static IReadOnlyCollection<UserConfig> GetAll()
	    {
		    var path = Path.Combine(AppContext.BaseDirectory, $"dist{Path.DirectorySeparatorChar}users");
		    var files = Directory.GetFiles(path, "*.json").ToList();
		    var configs = new List<UserConfig>();
		    files.ForEach(x => configs.Add(Load<UserConfig>(File.ReadAllText(x))));
		    return configs.AsReadOnly();
	    }

	    public bool HasPoints(ulong GUID) =>
		    Wumpoints.ContainsKey(GUID) && Wumpoints[GUID] > 0;
    }
}
