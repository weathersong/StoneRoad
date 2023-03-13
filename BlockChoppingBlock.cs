using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StoneRoad
{
	public class BlockChoppingBlock : Block
	{

		/*
		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer).Append(new WorldInteraction[] {
				new WorldInteraction()
				{
					ActionLangCode = "stoneroad:blockhelp-choppingblock-rightclick",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = null
				}
			});
		}
		*/

		public MeshData GenMesh(ICoreClientAPI capi, ITexPositionSource texture)
		{
			Shape shape;
			string shapePath = "stoneroad:shapes/block/stump";
			shape = capi.Assets.TryGet(shapePath + ".json").ToObject<Shape>();
			ITesselatorAPI tesselator = capi.Tesselator;

			tesselator.TesselateShape(shapePath, shape, out var mesh, texture, new Vec3f(0, 0, 0));

			var rotate = this.Shape.rotateY;
			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, rotate * GameMath.DEG2RAD, 0); //orient based on direction 
			return mesh;
		}

		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
		{
			if (world.BlockAccessor.GetBlockEntity(pos) is BEChoppingBlock be)
				be.OnBreak(); //empty the inventory onto the ground
			base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
		}

		public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
		{
			var facing = SuggestedHVOrientation(byPlayer, blockSel)[0].ToString();
			bool placed;
			placed = base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
			if (placed)
			{
				var block = this.api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.Default);
				var newPath = block.Code.Path;
				newPath = newPath.Replace("north", facing);
				block = this.api.World.GetBlock(block.CodeWithPath(newPath));
				this.api.World.BlockAccessor.SetBlock(block.BlockId, blockSel.Position);
			}
			return placed;
		}

		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BEChoppingBlock be)
				return be.OnInteract(byPlayer, blockSel);
			return base.OnBlockInteractStart(world, byPlayer, blockSel);
		}

//		public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
//		{
//			Cuboidf[] result = base.GetCollisionBoxes(blockAccessor, pos);
////			if (blockAccessor.GetBlockEntity(pos) is BEChoppingBlock be && !be.IsInventoryEmpty)
////			{
//				result.Append(new Cuboidf(0, 0, 0, 1, 1, 1));
////			}
//			return result;
//		}

	}

}
