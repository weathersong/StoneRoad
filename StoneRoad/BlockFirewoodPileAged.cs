using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StoneRoad
{
	// A direct copy of https://github.com/anegostudios/vssurvivalmod/blob/master/Legacy/Block/BlockFirewoodPile.cs

	public class BlockFirewoodPileAged : Block, IBlockItemPile
	{
		Cuboidf[][] CollisionBoxesByFillLevel;

		public BlockFirewoodPileAged()
		{
			CollisionBoxesByFillLevel = new Cuboidf[5][];

			for (int i = 0; i <= 4; i++)
			{
				CollisionBoxesByFillLevel[i] = new Cuboidf[] { new Cuboidf(0, 0, 0, 1, i * 0.25f, 1) };
			}
		}

		public int FillLevel(IBlockAccessor blockAccessor, BlockPos pos)
		{
			BlockEntity be = blockAccessor.GetBlockEntity(pos);
			if (be is BEFirewoodPileAged)
			{
				return (int)Math.Ceiling(((BEFirewoodPileAged)be).OwnStackSize / 8.0);
			}

			return 1;
		}

		public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
		{
			return CollisionBoxesByFillLevel[FillLevel(blockAccessor, pos)];
		}

		public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
		{
			return CollisionBoxesByFillLevel[FillLevel(blockAccessor, pos)];
		}

		public override BlockDropItemStack[] GetDropsForHandbook(ItemStack handbookStack, IPlayer forPlayer)
		{
			return new BlockDropItemStack[0];
		}

		public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
		{
			// Handled by BlockEntityItemPile
			return new ItemStack[0];
		}


		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{
			BlockEntity be = world.BlockAccessor.GetBlockEntity(blockSel.Position);
			if (be is BEFirewoodPileAged)
			{
				BEFirewoodPileAged pile = (BEFirewoodPileAged)be;
				return pile.OnPlayerInteract(byPlayer);
			}

			return false;
		}

		public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
		{
			var beg = world.BlockAccessor.GetBlockEntity(pos) as BEFirewoodPileAged;
			if (beg != null)
			{
				return beg.inventory.FirstNonEmptySlot?.Itemstack.Clone();
			}

			return null;
		}

		public bool Construct(ItemSlot slot, IWorldAccessor world, BlockPos pos, IPlayer player)
		{
			Block block = world.BlockAccessor.GetBlock(pos);
			if (!block.IsReplacableBy(this)) return false;
			Block belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
			if (!belowBlock.CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(), BlockFacing.UP) && (belowBlock != this || FillLevel(world.BlockAccessor, pos.DownCopy()) != 4)) return false;

			world.BlockAccessor.SetBlock(BlockId, pos);

			BlockEntity be = world.BlockAccessor.GetBlockEntity(pos);
			if (be is BEFirewoodPileAged)
			{
				BEFirewoodPileAged pile = (BEFirewoodPileAged)be;
				if (player == null || player.WorldData.CurrentGameMode != EnumGameMode.Creative)
				{
					pile.inventory[0].Itemstack = (ItemStack)slot.TakeOut(player.Entity.Controls.CtrlKey ? pile.BulkTakeQuantity : pile.DefaultTakeQuantity);
					slot.MarkDirty();
				}
				else
				{
					pile.inventory[0].Itemstack = (ItemStack)slot.Itemstack.Clone();
					pile.inventory[0].Itemstack.StackSize = Math.Min(pile.inventory[0].Itemstack.StackSize, pile.MaxStackSize);
				}

				pile.MarkDirty();
				world.BlockAccessor.MarkBlockDirty(pos);
				world.PlaySoundAt(pile.soundLocation, pos.X, pos.Y, pos.Z, player, true);
			}

			return true;
		}

		public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
		{
			Block belowBlock = world.BlockAccessor.GetBlock(pos.DownCopy());
			if (!belowBlock.CanAttachBlockAt(world.BlockAccessor, this, pos.DownCopy(), BlockFacing.UP) && (belowBlock != this || FillLevel(world.BlockAccessor, pos.DownCopy()) < 4))
			{
				world.BlockAccessor.BreakBlock(pos, null);
			}
		}

		public override bool CanAttachBlockAt(IBlockAccessor blockAccessor, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
		{
			BEFirewoodPileAged be = blockAccessor.GetBlockEntity(pos) as BEFirewoodPileAged;
			if (be != null)
			{
				return be.OwnStackSize == be.MaxStackSize;
			}

			return base.CanAttachBlockAt(blockAccessor, block, pos, blockFace, attachmentArea);
		}

		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			return new WorldInteraction[]
			{
				new WorldInteraction()
				{
					ActionLangCode = "game:blockhelp-firewoodpile-addlog",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = "shift",
					Itemstacks = new ItemStack[] { new ItemStack(world.GetItem(new AssetLocation("stoneroad", "firewood-aged")), 2) }
				},
				new WorldInteraction()
				{
					ActionLangCode = "game:blockhelp-firewoodpile-removelog",
					MouseButton = EnumMouseButton.Right,
					HotKeyCode = null
				},

				new WorldInteraction()
				{
					ActionLangCode = "game:blockhelp-firewoodpile-8addlog",
					MouseButton = EnumMouseButton.Right,
					HotKeyCodes = new string[] {"ctrl", "shift" },
					Itemstacks = new ItemStack[] { new ItemStack(world.GetItem(new AssetLocation("stoneroad", "firewood-aged")), 8) }
				},
				new WorldInteraction()
				{
					ActionLangCode = "game:blockhelp-firewoodpile-8removelog",
					HotKeyCode = "ctrl",
					MouseButton = EnumMouseButton.Right
				}
			}.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
		}
	}

}
