using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StoneRoad
{

	// Originally based on Primitive Survival's (Meat) Smoker, credit to Spear&Fang!

	public class BlockStraighteningRack : Block, IIgnitable
	{
		public StoneRoadMod SRMod;
		public double LumberStraighteningHours;
		public float LumberStraighteningCurePctChance;

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);

			//hard - coded defaults, but config defaults should always exist
			LumberStraighteningHours = 22;
			LumberStraighteningCurePctChance = 60;

			// these are re-read OnLoaded because mod config may have been changed by player
			SRMod = api.ModLoader.GetModSystem<StoneRoadMod>();
			if (SRMod != null && SRMod.Config != null)
			{
				LumberStraighteningHours = SRMod.Config.LumberStraighteningHours;
				LumberStraighteningCurePctChance = SRMod.Config.LumberStraighteningCurePctChance;
			}
		}

		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			BEStraighteningRack be = world.BlockAccessor.GetBlockEntity(selection.Position) as BEStraighteningRack;

			if (be?.State == BEStraighteningRack.StraightenRackStates.Steaming)
				return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
			else if (be?.WoodSlot.StackSize == 4 && be?.Inventory[0].Empty == false)
			{
				return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(new WorldInteraction[] {
				new WorldInteraction()
				{
					ActionLangCode = "blockhelp-forge-ignite",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = "shift"
				},
				new WorldInteraction()
				{
					ActionLangCode = "stoneroad:blockhelp-straightenrack-rightclick",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = null
				}
				});
			}
			else
			{
				return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(new WorldInteraction[] {
				new WorldInteraction()
				{
					ActionLangCode = "stoneroad:blockhelp-straightenrack-rightclick",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = null
				}
				});
			}
		}

		public Vintagestory.GameContent.EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
		{
			BEStraighteningRack be = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BEStraighteningRack;
			if (!be.CanIgnite())
				return Vintagestory.GameContent.EnumIgniteState.NotIgnitablePreventDefault;
			return secondsIgniting > 4 ? Vintagestory.GameContent.EnumIgniteState.IgniteNow : Vintagestory.GameContent.EnumIgniteState.Ignitable;
		}

		public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
		{
			handling = EnumHandling.PreventDefault;
			BEStraighteningRack be = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BEStraighteningRack;
			be?.TryIgnite();
		}

		public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, int count)
		{
			Shape shape;
			ITesselatorAPI tesselator = capi.Tesselator;
			shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

			int glow = 0;
			if (shapePath.Contains("lit"))
				glow = 200;
			tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0, 0, 0), glow);

			if (shapePath.Contains("log") || shapePath.Contains("lit"))
			{
				if (count == 1)
					mesh.Translate(0f, 0f, 0f);
				else if (count == 2)
					mesh.Translate(0.13f, 0f, 0f);
				else if (count == 3)
					mesh.Translate(0f, 0.125f, 0f);
				else
					mesh.Translate(0.13f, 0.125f, 0f);
			}
			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction 
			return mesh;
		}

		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
		{
			if (world.BlockAccessor.GetBlockEntity(pos) is BEStraighteningRack be)
				be.OnBreak(); // special inventory drop consideration because of firewood
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
			if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEStraighteningRack be)
				return be.OnInteract(byPlayer, blockSel);
			return base.OnBlockInteractStart(world, byPlayer, blockSel);
		}

		public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
		{
			if (world.BlockAccessor.GetBlockEntity(pos) is BEStraighteningRack be)
			{
				StringBuilder sb = new StringBuilder();
				switch (be.State)
				{
					case BEStraighteningRack.StraightenRackStates.Starting:
					default:
						return base.GetPlacedBlockInfo(world, pos, forPlayer);
					case BEStraighteningRack.StraightenRackStates.Steaming:
						be.GetProgressInfo(sb);
						return sb.ToString();
					case BEStraighteningRack.StraightenRackStates.Done:
						be.GetContentsInfo(sb);
						return sb.ToString() + "\n" + Lang.Get("stoneroad:blockdesc-straightenrack-done");
				}
			}

			return base.GetPlacedBlockInfo(world, pos, forPlayer);
		}

		public EnumIgniteState OnTryIgniteStack(EntityAgent byEntity, BlockPos pos, ItemSlot slot, float secondsIgniting)
		{
			throw new NotImplementedException();
		}
	}
}
