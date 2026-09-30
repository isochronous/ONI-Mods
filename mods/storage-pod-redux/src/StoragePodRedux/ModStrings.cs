using STRINGS;

namespace StoragePodRedux
{
	public static class ModStrings
	{
		private const string Prefabs = "STRINGS.BUILDINGS.PREFABS.";

		public static class StoragePod
		{
			public static readonly string Name = UI.FormatAsLink("Storage Pod", StoragePodConfig.ID.ToUpperInvariant());
			public const string Desc = "A sealed pod that keeps a lot of material in very little space, and does not care what it is bolted to.";
			public static readonly string Effect = "Stores the " + UI.FormatAsLink("Solid", "ELEMENTS_SOLID") + " resources of your choosing.\n\nCompact, and can be built anywhere.";
		}

		public static class CoolPod
		{
			public static readonly string Name = UI.FormatAsLink("Cool Pod", CoolPodConfig.ID.ToUpperInvariant());
			public const string Desc = "A small refrigerated pod that fits where a fridge will not.";
			public static readonly string Effect = "Stores " + UI.FormatAsLink("Food", "FOOD") + " and slows its spoilage.\n\nCompact, and can be built anywhere. Uses less power once its contents are cold.";
		}

		public static void Register()
		{
			Add(StoragePodConfig.ID, StoragePod.Name, StoragePod.Desc, StoragePod.Effect);
			Add(CoolPodConfig.ID, CoolPod.Name, CoolPod.Desc, CoolPod.Effect);
		}

		private static void Add(string id, string name, string desc, string effect)
		{
			string key = Prefabs + id.ToUpperInvariant() + ".";
			Strings.Add(key + "NAME", name);
			Strings.Add(key + "DESC", desc);
			Strings.Add(key + "EFFECT", effect);
		}
	}
}
