using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Wumpus.Common
{
    public abstract class ConfigBase : IConfig
	{
		[JsonIgnore]
	    public abstract string FileName { get; }
		[JsonIgnore]
	    public virtual string FilePath => 
			Path.Combine(AppContext.BaseDirectory, FileName);

	    protected ConfigBase()
	    {
	    }

	    public Task<T> Maintain<T>() where T: ConfigBase
	    {
		    return Task.Run(async () =>
		    {
			    if (!File.Exists(FilePath))
				    // Existance check is redundant, it's only created if it doesn't exist
				    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));

			    await Behaviour();

			    return Load<T>();
		    });
	    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public virtual async Task Behaviour()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			
	    }

	    public virtual void SaveJson<T>() where T: ConfigBase =>
		    File.WriteAllText(FilePath, ToJson<T>());

		public virtual T Load<T>() where T : ConfigBase =>
			Load<T>(File.ReadAllText(FilePath));

		public static T Load<T>(string path) where T: ConfigBase =>
			JsonConvert.DeserializeObject<T>(path);

		public virtual string ToJson<T>() where T: ConfigBase =>
			JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
