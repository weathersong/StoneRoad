using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using static StoneRoad.BEWoodRack;

namespace StoneRoad
{
	public class BlockWoodRack : Block
	{
		public StoneRoadMod SRMod;
		public double LumberDryingHours;
		public float LumberDryingCurePctChance;

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);

			//hard - coded defaults, but config defaults should always exist
			LumberDryingHours = 110;
			LumberDryingCurePctChance = 10;

			// these are re-read OnLoaded because mod config may have been changed by player
			SRMod = api.ModLoader.GetModSystem<StoneRoadMod>();
			if (SRMod != null && SRMod.Config != null)
			{
				LumberDryingHours = SRMod.Config.LumberDryingHours;
				LumberDryingCurePctChance = SRMod.Config.LumberDryingCurePctChance;
			}
		}

		//public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		//{
		//	return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(new WorldInteraction[] {
		//		new WorldInteraction()
		//		{
		//			ActionLangCode = "stoneroad:blockhelp-woodrack-rightclick",
		//			MouseButton = EnumMouseButton.Right,
		//			HotKeyCode = null
		//		}
		//		});
		//}

		public MeshData GenMesh(ICoreClientAPI capi, ITexPositionSource texture)
		{
			Shape shape;
			string shapePath = "stoneroad:shapes/block/woodrack";
			shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
			ITesselatorAPI tesselator = capi.Tesselator;

			tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0, 0, 0));

			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Shape.rotateY * GameMath.DEG2RAD, 0);
			return mesh;
		}

		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
		{
			if (world.BlockAccessor.GetBlockEntity(pos) is BEWoodRack be)
				be.OnBreak(); //empty the inventory onto the ground
			base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
		}

		public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
		{
			string facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
			bool placed;
			placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
			if (placed)
			{
				Block block = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
				string newPath = block.Code.Path;
				newPath = newPath.Replace("north", facing);
				block = api.World.GetBlock(block.CodeWithPath(newPath));
				api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
			}
			return placed;
		}

		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEWoodRack be)
				return be.OnInteract(byPlayer, blockSel);
			return base.OnBlockInteractStart(world, byPlayer, blockSel);
		}

		public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
		{
			if (world.BlockAccessor.GetBlockEntity(pos) is BEWoodRack be)
			{
				StringBuilder sb = new StringBuilder();
				switch (be.State)
				{
					case BEWoodRack.WoodRackStates.Starting:
					default:
						return base.GetPlacedBlockInfo(world, pos, forPlayer);
					case BEWoodRack.WoodRackStates.Drying:
						be.GetProgressInfo(sb);
						return sb.ToString();
					case BEWoodRack.WoodRackStates.Done:
						return Lang.Get("stoneroad:blockdesc-woodrack-done");
				}
			}

			return base.GetPlacedBlockInfo(world, pos, forPlayer);
		}

	}
}
