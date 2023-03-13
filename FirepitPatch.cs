using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using HarmonyLib;

namespace StoneRoad
{
	[HarmonyPatch(typeof(BlockFirepit))]
	internal class FirepitPatch
	{
		[HarmonyPatch("TryConstruct")]
		[HarmonyPrefix]
		private static bool TryConstruct(ref bool __result, IWorldAccessor world, BlockPos pos, CollectibleObject obj, IPlayer player, BlockFirepit __instance)
		{
			int stage = __instance.Stage;

			if (obj.Attributes?.IsTrue("firepitConstructable") != true) return false;

			if (stage == 5) return false;

			if (stage == 4 && world.BlockAccessor.GetBlock(pos.DownCopy()).Code.Path.Contains("firewoodpile"))
			{
				Block charcoalPitBlock = world.GetBlock(new AssetLocation("stoneroad", "charcoalpit"));
				if (charcoalPitBlock != null)
				{
					world.BlockAccessor.SetBlock(charcoalPitBlock.BlockId, pos);

					BECharcoalPit be = world.BlockAccessor.GetBlockEntity(pos) as BECharcoalPit;
					be?.Init(player);

					(player as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

					__result = true;
					return false;
				}
			}

			return true;
		}
	}

}
