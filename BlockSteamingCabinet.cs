using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace StoneRoad
{

	// Using Primitive Survival's Smoker as a reference

	public class BlockSteamingCabinet : Block
	{
		public StoneRoadMod SRMod;
		public double LumberSteamingHours;
		public float LumberSteamingCurePctChance;

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);

			//hard - coded defaults, but config defaults should always exist
			LumberSteamingHours = 22;
			LumberSteamingCurePctChance = 60;

			// these are re-read OnLoaded because mod config may have been changed by player
			SRMod = api.ModLoader.GetModSystem<StoneRoadMod>();
			if (SRMod != null && SRMod.Config != null)
			{
				LumberSteamingHours = SRMod.Config.LumberSteamingHours;
				LumberSteamingCurePctChance = SRMod.Config.LumberSteamingCurePctChance;
			}
		}

		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			var be = world.BlockAccessor.GetBlockEntity(selection.Position) as BESteamingCabinet;

			if (be?.State == "lit")
			{ return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer); }
			else if (be?.State == "closed" && be?.WoodSlot.StackSize == 4 && be?.Inventory[0].Empty == false)
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
					ActionLangCode = "stoneroad:blockhelp-steamcabinet-rightclick",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = null
				}
				});
			}
			else if (be?.State != "lit")
			{
				return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(new WorldInteraction[] {
				new WorldInteraction()
				{
					ActionLangCode = "stoneroad:blockhelp-steamcabinet-rightclick",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = null
				}
				});
			}
			else
			{
				return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
			}
		}

		public override EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
		{
			var be = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BESteamingCabinet;
			if (!be.CanIgnite())
			{ return EnumIgniteState.NotIgnitablePreventDefault; }
			return secondsIgniting > 4 ? EnumIgniteState.IgniteNow : EnumIgniteState.Ignitable;
		}

		public override void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
		{
			handling = EnumHandling.PreventDefault;
			var be = byEntity.World.BlockAccessor.GetBlockEntity(pos) as BESteamingCabinet;
			be?.TryIgnite();
		}

		public MeshData GenMesh(ICoreClientAPI capi, string shapePath, ITexPositionSource texture, string state, int count)
		{
			Shape shape;
			ITesselatorAPI tesselator = capi.Tesselator;
			shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();

			int glow = 0;
			if (shapePath.Contains("lit"))
				glow = 200;
			tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0, 0, 0), glow);

			float rotate = Shape.rotateY;
			if (state == "open" && shapePath.Contains("door"))
			{
				rotate -= 100;
				mesh.Translate(0.2f, 0f, 0.8f);
			}
			if (shapePath.Contains("log") || shapePath.Contains("lit"))
			{
				if (count == 1)
					mesh.Translate(-0.06f, -0.08f, -0.12f);
				else if (count == 2)
					mesh.Translate(-0.06f, -0.08f, 0.01f);
				else if (count == 3)
					mesh.Translate(-0.1f, 0.05f, -0.12f);
				else
					mesh.Translate(-0.08f, 0.05f, 0.01f);
			}
			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, rotate * GameMath.DEG2RAD, 0); //orient based on direction 
			return mesh;
		}

		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
		{
			if (world.BlockAccessor.GetBlockEntity(pos) is BESteamingCabinet be)
			{ be.OnBreak(); } //empty the inventory onto the ground
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
			if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BESteamingCabinet be)
				return be.OnInteract(byPlayer, blockSel);
			return base.OnBlockInteractStart(world, byPlayer, blockSel);
		}
	}
}
