using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace UpgradePlanner
{
	/// <summary>
	/// The def to build for a family and a choice of materials. A family of one def is that def.
	/// For a family of variants the mod that made them decides, in a prefix on BuildTool.Activate
	/// that swaps the def for the chosen material: those prefixes are run here with the same
	/// arguments, so the swap is theirs, not a guess at their naming. Failing that, a finished
	/// building of the family with these exact materials shows which variant it was; failing that,
	/// the listed def.
	/// </summary>
	internal static class Variants
	{
		public static BuildingDef Resolve(Family family, IList<Tag> materials)
		{
			if (family == null)
				return null;
			if (family.Members.Count == 1)
				return family.Representative;
			BuildingDef swapped = RunActivatePrefixes(family.Representative, materials);
			if (swapped != null && family.Contains(swapped))
				return swapped;
			foreach (BuildingComplete building in Components.BuildingCompletes.Items)
				if (building != null && family.Contains(building.Def) && Materials.Same(Materials.Of(building), materials))
					return building.Def;
			return family.Representative;
		}

		private static BuildingDef RunActivatePrefixes(BuildingDef def, IList<Tag> materials)
		{
			try
			{
				MethodInfo activate = AccessTools.Method(typeof(BuildTool), "Activate", new[] { typeof(BuildingDef), typeof(IList<Tag>) });
				HarmonyLib.Patches patches = activate != null ? Harmony.GetPatchInfo(activate) : null;
				if (patches == null)
					return null;
				BuildingDef current = def;
				foreach (Patch patch in patches.Prefixes)
				{
					MethodInfo method = patch.PatchMethod;
					ParameterInfo[] parameters = method.GetParameters();
					var args = new object[parameters.Length];
					int defIndex = -1;
					for (int i = 0; i < parameters.Length; i++)
					{
						switch (parameters[i].Name)
						{
						case "__instance": args[i] = BuildTool.Instance; break;
						case "def": args[i] = current; defIndex = i; break;
						case "selected_elements": args[i] = materials; break;
						default: args[i] = DefaultOf(parameters[i].ParameterType); break;
						}
					}
					try
					{
						method.Invoke(null, args);
					}
					catch (Exception e)
					{
						Debug.Log("[UpgradePlanner] Build tool prefix " + method.DeclaringType?.FullName + "." + method.Name + " refused a dry run: " + e.GetBaseException().Message);
						continue;
					}
					if (defIndex >= 0 && args[defIndex] is BuildingDef result)
						current = result;
				}
				return current;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[UpgradePlanner] Could not ask the build tool's patches for a variant: " + e.Message);
				return null;
			}
		}

		private static object DefaultOf(Type type)
		{
			if (type.IsByRef)
				type = type.GetElementType();
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}
	}
}
