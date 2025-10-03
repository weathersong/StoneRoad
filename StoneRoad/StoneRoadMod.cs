using System;
using System.Reflection;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using HarmonyLib;

namespace StoneRoad
{
	public class StoneRoadMod : ModSystem
	{

		const string LogHeader = "STONE_ROAD_2_2";
		const string ConfigFilename = "StoneRoadConfig.json";

		private ICoreAPI CoreApi;
		private ICoreServerAPI ServerApi;
		private ICoreClientAPI ClientApi;

		private Harmony Harmony;

		public StoneRoadConfig Config;

		#region STARTUP

		public override void StartPre(ICoreAPI api)
		{
			base.StartPre(api);

			CoreApi = api;
			Config = new StoneRoadConfig();

			LoadConfig();
		}

		public override void Start(ICoreAPI api)
		{
			base.Start(api);

			LogDebug("Registering classes...");

			// Chopping block
			api.RegisterBlockClass("BlockChoppingBlock", typeof(BlockChoppingBlock));
			api.RegisterBlockEntityClass("BEChoppingBlock", typeof(BEChoppingBlock));

			// Log halves
			api.RegisterBlockClass("BlockLogHalf", typeof(BlockLogHalf));

			// Wood drying rack
			api.RegisterBlockClass("BlockWoodRack", typeof(BlockWoodRack));
			api.RegisterBlockEntityClass("BEWoodRack", typeof(BEWoodRack));

			// Aged firewood
			// Removed in 1.18 - use the vanilla version now instead.

			// Straightening rack
			api.RegisterBlockClass("BlockStraighteningRack", typeof(BlockStraighteningRack));
			api.RegisterBlockEntityClass("BEStraighteningRack", typeof(BEStraighteningRack));

			// Charcoal
			// // BlockCharcoalPit is the "firepit" block, not to be confused with (char)coal *piles*.
			// // Firepit itself is Harmony patched (FirepitPatch.cs) to spawn *this* block, not the vanilla BlockCharcoalPit, which this is based upon:
			api.RegisterBlockClass("BlockCharcoalPit", typeof(BlockCharcoalPit));
			api.RegisterBlockEntityClass("BECharcoalPit", typeof(BECharcoalPit));

			// Hand-juiced fruit
			api.RegisterItemClass("ItemJuicyFruit", typeof(ItemJuicyFruit));

			LogDebug($"Applying Harmony patches...");
			Harmony = new Harmony("net.weathersong.stoneroad.harmonypatches");
			Harmony.PatchAll(Assembly.GetExecutingAssembly());

		}

		public override void StartServerSide(ICoreServerAPI api)
		{
			base.StartServerSide(api);

			ServerApi = api;

		}

		public override void StartClientSide(ICoreClientAPI api)
		{
			base.StartClientSide(api);

			ClientApi = api;

		}

		#endregion

		#region UTIL_FUNCTIONS

		private void LoadConfig()
		{
			try
			{
				Config = CoreApi.LoadModConfig<StoneRoadConfig>(ConfigFilename);
				if (Config == null)
				{
					LogNotif("No config file found. Using defaults, and creating a default config file.");
					Config = DefaultConfig();
					CoreApi.StoreModConfig(Config, ConfigFilename);
				}
				else
				{
					// Extra sanity checks / warnings on particular values:
					// ...
					LogNotif("Config loaded.");
					// In case this was an old version of the config, store again anyway so that it's updated.
					CoreApi.StoreModConfig(Config, ConfigFilename);
				}
			}
			catch (Exception ex)
			{
				LogError($"Problem loading the mod's config file, using defaults. Check the config file for typos! Error details: {ex.Message}");
				Config = DefaultConfig();
			}
		}

		private StoneRoadConfig DefaultConfig()
		{
			StoneRoadConfig defaultConfig = new StoneRoadConfig();
			defaultConfig.ResetToDefaults();

			return defaultConfig;
		}

		public void LogNotif(string msg)
		{
			CoreApi?.Logger.Notification($"[{LogHeader}] {msg}");
		}

		public void LogWarn(string msg)
		{
			CoreApi?.Logger.Warning($"[{LogHeader}] {msg}");
		}

		public void LogError(string msg)
		{
			CoreApi?.Logger.Error($"[{LogHeader}] {msg}");
		}

		public void LogDebug(string msg)
		{
			if (Config == null || Config.DebugLogging)
				CoreApi?.Logger.Debug($"[{LogHeader}] {msg}");
		}

		public void MessagePlayer(IPlayer toPlayer, string msg)
		{
			ServerApi.SendMessage(toPlayer, GlobalConstants.GeneralChatGroup, msg, EnumChatType.OwnMessage);
		}

		public string BlockPosString(BlockPos pos)
		{
			return $"{pos.X} {pos.Y} {pos.Z}";
		}

		public string EntityPosString(SyncedEntityPos pos)
		{
			return $"{pos.X:#.00} {pos.Y:#.00} {pos.Z:#.00}";
		}

		#endregion

	}

}
