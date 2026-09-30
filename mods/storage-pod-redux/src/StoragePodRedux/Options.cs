using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace StoragePodRedux
{
	public enum CoolPodMode
	{
		[Option("Auto", "Enabled, unless Ronivan's Legacy - Industrial Revolution is enabled with its Fridge Pod turned on.")]
		Auto,
		[Option("Enabled", "Always add the Cool Pod.")]
		Enabled,
		[Option("Disabled", "Never add the Cool Pod.")]
		Disabled,
	}

	[JsonObject(MemberSerialization.OptIn)]
	[ConfigFile(SharedConfigLocation: true)]
	[RestartRequired]
	public sealed class Options : SingletonOptions<Options>
	{
		[Option("Storage Pod", "Add the Storage Pod building. Takes effect after a restart.", "Storage Pod")]
		[JsonProperty]
		public bool StoragePodEnabled { get; set; } = true;

		[Option("Capacity (kg)", "How much the Storage Pod holds.", "Storage Pod", Format = "F0")]
		[Limit(100, 50000)]
		[JsonProperty]
		public float StoragePodCapacity { get; set; } = 5000f;

		[Option("Stores food", "Let the Storage Pod hold food as well as other solids. It is not refrigerated.", "Storage Pod")]
		[JsonProperty]
		public bool StoragePodStoresFood { get; set; } = false;

		[Option("Cool Pod", "Add the Cool Pod building. Takes effect after a restart.", "Cool Pod")]
		[JsonProperty]
		public CoolPodMode CoolPod { get; set; } = CoolPodMode.Auto;

		[Option("Capacity (kg)", "How much food the Cool Pod holds.", "Cool Pod", Format = "F0")]
		[Limit(10, 5000)]
		[JsonProperty]
		public float CoolPodCapacity { get; set; } = 50f;
	}
}
