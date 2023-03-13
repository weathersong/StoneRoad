using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace StoneRoad
{
	internal class BlockLogHalf : Block
	{
		//public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
		//{
		//	return Drops[0].ResolvedItemstack.Clone();
		//}

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
	}

}
