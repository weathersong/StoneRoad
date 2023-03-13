using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace StoneRoad
{
	internal class BlockUncuredPlank : Block
	{
		//private StoneRoadMod srMod;
		//private double conversionHours;
		//public float conversionPct;

		private WorldInteraction[] pickupInteraction = null;

		public BlockUncuredPlank() { }

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);

			// hard-coded defaults, but config defaults should always exist
			//conversionHours = 110;
			//conversionPct = 10;

			//srMod = api.ModLoader.GetModSystem<StoneRoadMod>();
			//if (srMod != null && srMod.Config != null)
			//{
			//	conversionHours = srMod.Config.LumberDryingHours;
			//	conversionPct = srMod.Config.LumberDryingCurePctChance;
			//}

			//this.pickupInteraction = ObjectCacheUtil.GetOrCreate<WorldInteraction[]>(api, "plankPickUp", () => new WorldInteraction[]
			//{
			//	new WorldInteraction
			//	{
			//		ActionLangCode = "stoneroad:blockhelp-take-plank",
			//		MouseButton = EnumMouseButton.Right
			//	}
			//});
		}

		//public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		//{
		//	return ArrayExtensions.Append<WorldInteraction>(this.pickupInteraction, base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
		//}

		//public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
		//{
		//	if (world.BlockAccessor.GetBlockEntity(pos) is BEUncuredPlank uncuredPlankEntity && this.FirstCodePart(1) == "raw")
		//	{
		//		if (uncuredPlankEntity.TimeRemaining < 1.5)
		//			return base.GetPlacedBlockInfo(world, pos, forPlayer) + "\n" + Lang.Get("stoneroad:blockdesc-uncuredplank-cure-1-hour");
		//		else
		//			return base.GetPlacedBlockInfo(world, pos, forPlayer) + "\n" + Lang.Get("stoneroad:blockdesc-uncuredplank-cure-x-hours", (int)(uncuredPlankEntity.TimeRemaining + 0.5));
		//	}
		//	else if (this.FirstCodePart(1) == "warped")
		//		return base.GetPlacedBlockInfo(world, pos, forPlayer) + "\n" + Lang.Get("stoneroad:blockdesc-warpedplank-next-step");
		//	else if (this.FirstCodePart(1) == "cured")
		//		return base.GetPlacedBlockInfo(world, pos, forPlayer) + "\n" + Lang.Get("stoneroad:blockdesc-curedplank-ready");

		//	return base.GetPlacedBlockInfo(world, pos, forPlayer);
		//}

		public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
		{
			base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

			if (this.FirstCodePart(1) == "raw")
			{
				dsc.Append("\n" + Lang.Get("stoneroad:blockdesc-uncuredplank-first-step"));
				//if (inSlot.Itemstack.Attributes.HasAttribute("timeremaining"))
				//	dsc.Append( "\n" + Lang.Get("stoneroad:blockdesc-uncuredplank-cure-x-hours-when-placed", (int)(inSlot.Itemstack.Attributes.GetDouble("timeremaining") + 0.5)) );
				//else
				//	dsc.Append( "\n" + Lang.Get("stoneroad:blockdesc-uncuredplank-cure-x-hours-when-placed", conversionHours) );
			}
			else if (this.FirstCodePart(1) == "warped")
				dsc.Append("\n" + Lang.Get("stoneroad:blockdesc-warpedplank-next-step"));
			//else if (this.FirstCodePart(1) == "cured")
			//	dsc.Append("\n" + Lang.Get("stoneroad:blockdesc-curedplank-next-step"));

		}

		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			return true;

			/*
			if (byPlayer.Entity.Controls.Sneak)
				return false;

			if (this.FirstCodePart(1) == "raw")
			{
				if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEUncuredPlank uncuredPlankEntity)
				{
					ItemStack rawPlank = new ItemStack(world.GetBlock(new AssetLocation("stoneroad", "uncuredplank-raw-" + this.LastCodePart())));
					rawPlank.Attributes.SetDouble("timeremaining", uncuredPlankEntity.TimeRemaining);
					rawPlank.Attributes.SetFloat("curechance", conversionPct);

					if (!byPlayer.Entity.TryGiveItemStack(rawPlank))
					{
						world.SpawnItemEntity(rawPlank, blockSel.Position.ToVec3d());
					}

					world.BlockAccessor.SetBlock(0, blockSel.Position);
					world.BlockAccessor.RemoveBlockEntity(blockSel.Position); // BEUncuredPlank removed

					return true;
				}
			}
			else if (this.FirstCodePart(1) == "warped")
			{
				ItemStack warpedPlank = new ItemStack(world.GetBlock(new AssetLocation("stoneroad", "uncuredplank-warped-" + this.LastCodePart())));

				if (!byPlayer.Entity.TryGiveItemStack(warpedPlank))
				{
					world.SpawnItemEntity(warpedPlank, blockSel.Position.ToVec3d());
				}

				world.BlockAccessor.SetBlock(0, blockSel.Position);

				return true;
			}
			else if (this.FirstCodePart(1) == "cured")
			{
				ItemStack curedPlank = new ItemStack(world.GetBlock(new AssetLocation("stoneroad", "uncuredplank-cured-" + this.LastCodePart())));

				if (!byPlayer.Entity.TryGiveItemStack(curedPlank))
				{
					world.SpawnItemEntity(curedPlank, blockSel.Position.ToVec3d());
				}

				world.BlockAccessor.SetBlock(0, blockSel.Position);

				return true;
			}

			return false;
			*/
		}

		public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack)
		{
			base.OnBlockPlaced(world, blockPos, byItemStack);

			//if (byItemStack == null)
			//	return;

			//if (world.BlockAccessor.GetBlockEntity(blockPos) is BEUncuredPlank uncuredPlankEntity)
			//{
			//	uncuredPlankEntity.TimeRemaining = byItemStack.Attributes.GetDouble("timeremaining", conversionHours);
			//	uncuredPlankEntity.CureChance = conversionPct;
			//}
		}

	}

}
