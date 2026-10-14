using System.Collections.Generic;

namespace UpgradePlanner
{
	/// <summary>Which tab of the window a plan is on, and so which of its halves a drag uses.</summary>
	internal enum PlanMode { ChangeMaterial, ReplaceType }

	/// <summary>How the buildings to replace are picked by material.</summary>
	internal enum MaterialMode { Unset, Any, Specific }

	/// <summary>
	/// The player's choices on both tabs of the window, and which tab is showing. Kept for the
	/// whole session, so selecting the tool again resumes the last plan; presets keep plans across
	/// sessions. Every change bumps <see cref="Version"/>, so what is worked out from a plan (the
	/// tool's highlight and overlay) knows to work it out again.
	/// </summary>
	internal sealed class UpgradePlan
	{
		public static readonly UpgradePlan Current = new UpgradePlan();

		private PlanMode mode = PlanMode.ChangeMaterial;

		public UpgradePlan()
		{
			ChangeMaterial = new ChangeMaterialPlan(Changed);
			ReplaceType = new ReplaceTypePlan(Changed);
		}

		public ChangeMaterialPlan ChangeMaterial { get; }
		public ReplaceTypePlan ReplaceType { get; }

		public int Version { get; private set; }

		public PlanMode Mode
		{
			get => mode;
			set
			{
				if (mode == value)
					return;
				mode = value;
				Changed();
			}
		}

		/// <summary>The tab showing, whose choices a drag uses.</summary>
		public IPlanTab Active => mode == PlanMode.ChangeMaterial ? (IPlanTab)ChangeMaterial : ReplaceType;

		public bool IsComplete => Active.IsComplete;

		/// <summary>Back to nothing chosen on the tab showing.</summary>
		public void Clear() => Active.Clear();

		/// <summary>What a drag does, or what is still to choose, for the window's footer.</summary>
		public string Summary() => Active.Summary();

		/// <summary>The types a drag can touch now, every variant included; empty when nothing is chosen.</summary>
		public List<BuildingDef> TouchedDefs() => Active.TouchedDefs();

		/// <summary>What a drag does to each building, as the plan stands; null until enough is chosen to say.</summary>
		public Matcher Compile() => Active.Compile();

		/// <summary>The tab showing, saved under a name.</summary>
		public PlanPreset ToPreset(string name)
		{
			var preset = new PlanPreset { Name = name, Mode = mode.ToString() };
			Active.Save(preset);
			return preset;
		}

		/// <summary>Takes on a saved plan, on its own tab; the other tab's choices are left alone.</summary>
		public void Load(PlanPreset preset)
		{
			if (preset == null)
				return;
			Mode = preset.Tab;
			Active.Load(preset);
		}

		private void Changed() => Version++;
	}

	/// <summary>One tab's half of the plan.</summary>
	internal interface IPlanTab
	{
		/// <summary>Everything a drag needs is chosen.</summary>
		bool IsComplete { get; }
		void Clear();
		string Summary();
		List<BuildingDef> TouchedDefs();
		Matcher Compile();
		void Save(PlanPreset preset);
		void Load(PlanPreset preset);
	}

	/// <summary>What a drag does to one building: the def built in its place and its materials, by an order over it or a rebuild.</summary>
	internal struct Upgrade
	{
		public BuildingDef Target;
		public List<Tag> Materials;
		/// <summary>The game does not let this building be built over: it is deconstructed and rebuilt in place.</summary>
		public bool Rebuild;
	}

	/// <summary>
	/// A plan worked out for the buildings a drag or the highlight looks at: which buildings it
	/// takes, and what each becomes. Made from the plan as it stands, so the per-building checks are
	/// lookups; made again when the plan changes.
	/// </summary>
	internal abstract class Matcher
	{
		/// <summary>The types a drag can touch, every variant included.</summary>
		public readonly List<BuildingDef> Defs;
		/// <summary>The grid layers those types sit on, their object and tile layers.</summary>
		public readonly List<ObjectLayer> Layers = new List<ObjectLayer>();

		protected Matcher(List<BuildingDef> defs)
		{
			Defs = defs;
			foreach (BuildingDef def in defs)
			{
				if (!Layers.Contains(def.ObjectLayer))
					Layers.Add(def.ObjectLayer);
				if (def.TileLayer != ObjectLayer.NumLayers && !Layers.Contains(def.TileLayer))
					Layers.Add(def.TileLayer);
			}
		}

		private readonly Dictionary<string, BuildingDef> variants = new Dictionary<string, BuildingDef>();

		/// <summary>
		/// The def to build for a family in these materials (<see cref="Variants.Resolve"/>), worked out
		/// once per family and materials: it can run other mods' build tool patches.
		/// </summary>
		protected BuildingDef Resolve(Family family, List<Tag> materials)
		{
			if (family.Members.Count == 1)
				return family.Representative;
			string key = family.PrefabID + "|" + string.Join("|", materials);
			if (!variants.TryGetValue(key, out BuildingDef def))
				variants[key] = def = Variants.Resolve(family, materials);
			return def;
		}

		/// <summary>True if the plan takes this building (whether or not it is complete enough to say what it becomes).</summary>
		public abstract bool Matches(BuildingComplete building);

		/// <summary>What the plan makes of this building; false when it leaves it alone or is not complete.</summary>
		public abstract bool TryPlan(BuildingComplete building, out Upgrade upgrade);
	}
}
