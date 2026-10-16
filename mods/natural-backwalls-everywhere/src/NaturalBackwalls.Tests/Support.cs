using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProcGen;

namespace NaturalBackwalls.Tests
{
	/// <summary>Synthetic worldgen data for the planning tests, and a few lookups.</summary>
	internal static class Data
	{
		public static SubWorld SubWorld(SubWorld.ZoneType zone, string backwallNoise, params string[] biomes)
		{
			var sub = new SubWorld();
			Set(sub, "biomes", biomes.Select(b => new WeightedBiome(b, 1f)).ToList());
			Set(sub, "zoneType", zone);
			if (backwallNoise != null)
				Set(sub, "backwallNoise", backwallNoise);
			return sub;
		}

		private static void Set(SubWorld sub, string property, object value)
		{
			typeof(SubWorld).GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetSetMethod(true).Invoke(sub, new[] { value });
		}

		public static ElementBandConfiguration Bands(params (string element, float size)[] bands)
		{
			var config = new ElementBandConfiguration();
			foreach (var (element, size) in bands)
				config.Add(new ElementGradient(element, size, null));
			return config;
		}

		public static List<(string element, float size)> Read(ElementBandConfiguration config)
		{
			return config.Select(g => (g.content, g.bandSize)).ToList();
		}

		public static CloneInToNewWorld_Patch.SubworldPlan Plan(SubWorld sub, Options options, IDictionary<string, ElementBandConfiguration> bands = null, params string[] startBiomes)
		{
			var subworlds = new Dictionary<string, SubWorld> { { "subworlds/test/Sub", sub } };
			var plans = CloneInToNewWorld_Patch.Plan(subworlds, bands ?? new Dictionary<string, ElementBandConfiguration>(), new HashSet<string>(startBiomes), options, out _);
			return plans.SingleOrDefault();
		}
	}
}
