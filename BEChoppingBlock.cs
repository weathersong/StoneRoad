using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static StoneRoad.BEWoodRack;

namespace StoneRoad
{
	public class BEChoppingBlock : BlockEntityDisplayCase
	{
		private BlockFacing ownFacing;
		public override string InventoryClassName => "woodrack";
		public override InventoryBase Inventory => this.inventory;
		public bool IsInventoryEmpty => this.inventory[0].Empty;

		private AssetLocation logSound;

		public BEChoppingBlock()
		{
			this.inventory = new InventoryGeneric(1, null, null);
			this.meshes = new MeshData[1];
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);

			this.inventory.LateInitialize("choppingblock" + "-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);
			this.logSound = new AssetLocation("game", "sounds/block/planks");

			this.ownFacing = BlockFacing.FromCode(api.World.BlockAccessor.GetBlock(this.Pos).LastCodePart());
		}

		internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
		{
			ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

			string playerPath = "";
			ItemStack playerStack = playerSlot.Itemstack;
			if (playerStack?.Collectible != null)
				playerPath = playerStack.Collectible.Code.Path;

			string blockPath = "";
			ItemStack itemStack = inventory[0]?.Itemstack;
			if (itemStack != null && itemStack.Collectible != null)
				blockPath = itemStack.Collectible.Code.Path;

			// Place down a log for chopping
			if (playerStack?.Collectible is BlockLog && IsInventoryEmpty)
			{
				Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

				BEChoppingBlock be = ToggleContentState();
				if(be != null)
					return be.TryPut(playerSlot);
			}

			// Place down a halflog
			else if (playerStack?.Collectible is BlockLogHalf && IsInventoryEmpty)
			{
				Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

				BEChoppingBlock be = ToggleContentState();
				if (be != null)
					return be.TryPut(playerSlot);
			}

			// Place down a stripped log
			else if (playerPath.StartsWith("strippedlog-"))
			{
				Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

				BEChoppingBlock be = ToggleContentState();
				if (be != null)
					return be.TryPut(playerSlot);
			}

			// Chop a placed log
			else if (playerPath.StartsWith("axe-") && itemStack?.Collectible is BlockLog blockLogToChop)
			{
				// what wood type is it?
				//string wood = blockToChop.Attributes["wood"].ToString(); // Code.Path = log-placed-larch-ud
				string wood = blockLogToChop.Variant["wood"];
				// damage axe
				playerStack.Collectible.DamageItem(this.Api.World, byPlayer.Entity, playerSlot, 5);
				// particles
				blockLogToChop.SpawnBlockBrokenParticles(blockSel.Position);
				// sound
				this.Api.World.PlaySoundAt(blockLogToChop.Sounds?.GetBreakSound(byPlayer), this.Pos.X, this.Pos.Y, this.Pos.Z, byPlayer);
				// swap to empty block - this also empties inventory
				ToggleContentState();
				//inventory[0].TakeOutWhole();
				// drop halves
				//AssetLocation dropAsset = new AssetLocation("game:firewood"); // just firewood works too
				AssetLocation dropAsset = new AssetLocation("stoneroad", $"loghalf-{wood}-north");
				if (dropAsset != null)
					for (int i = 0; i < 2 ; i++)
						this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetBlock(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.0 + (i / 10), 0.5));

				return true;
			}

			// Chop a half log
			else if (playerPath.StartsWith("axe-") && itemStack?.Collectible is BlockLogHalf blockHalfLogToChop)
			{
				playerStack.Collectible.DamageItem(this.Api.World, byPlayer.Entity, playerSlot, 5);
				blockHalfLogToChop.SpawnBlockBrokenParticles(blockSel.Position);
				Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
				ToggleContentState();
				AssetLocation dropAsset = new AssetLocation("firewood");
				if (dropAsset != null)
					for (int i = 0; i < 2; i++)
						this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetItem(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.0 + (i / 10), 0.5));

				return true;
			}

			// Chop a stripped log
			else if (playerPath.StartsWith("axe-") && blockPath.StartsWith("strippedlog-"))
			{
				string wood = "";
				Block blockToChop = itemStack.Collectible as Block;
				if (blockToChop != null)
					wood = blockToChop.Variant["wood"];
				playerStack.Collectible.DamageItem(this.Api.World, byPlayer.Entity, playerSlot, 5);
				blockToChop?.SpawnBlockBrokenParticles(blockSel.Position);
				Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
				ToggleContentState();
				AssetLocation dropAsset = new AssetLocation("stoneroad", $"uncuredplank-raw-{wood}");
				if (dropAsset != null)
					for (int i = 0; i < 8; i++)
						this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetItem(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.0 + (i / 10), 0.5));

				return true;
			}

			// Strip a log - this is all AncientTools stuff, the adze, the bark, and the stripped log
			else if (playerPath.StartsWith("adze-") && itemStack?.Collectible is BlockLog blockLogToStrip)
			{
				string wood = blockLogToStrip.Variant["wood"];
				playerStack.Collectible.DamageItem(this.Api.World, byPlayer.Entity, playerSlot, 3);
				blockLogToStrip.SpawnBlockBrokenParticles(blockSel.Position);
				this.Api.World.PlaySoundAt(blockLogToStrip.Sounds?.GetBreakSound(byPlayer), this.Pos.X, this.Pos.Y, this.Pos.Z, byPlayer);
				ToggleContentState();
				// drop bark if possible
				AssetLocation dropAsset = new AssetLocation("ancienttools", $"bark-{wood}");
				if (dropAsset != null)
					for (int i = 0; i < 4; i++)
						this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetItem(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.5 + (i / 10), 0.5));
				// stripped log
				dropAsset = new AssetLocation("ancienttools", $"strippedlog-{wood}-ud");
				if (dropAsset != null)
					this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetBlock(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.0, 0.5));

				return true;
			}

			// Strip a half log
			else if (playerPath.StartsWith("adze-") && itemStack?.Collectible is BlockLogHalf blockHalfLogToStrip)
			{
				string wood = blockHalfLogToStrip.Variant["wood"];
				playerStack.Collectible.DamageItem(this.Api.World, byPlayer.Entity, playerSlot, 5);
				blockHalfLogToStrip.SpawnBlockBrokenParticles(blockSel.Position);
				Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
				ToggleContentState();
				AssetLocation dropAsset = new AssetLocation("ancienttools", $"bark-{wood}");
				if (dropAsset != null)
					for (int i = 0; i < 2; i++)
						this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetItem(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.5 + (i / 10), 0.5));
				dropAsset = new AssetLocation("stoneroad", $"uncuredplank-raw-{wood}");
				if (dropAsset != null)
					for (int i = 0; i < 4; i++)
						this.Api.World.SpawnItemEntity(new ItemStack(this.Api.World.GetItem(dropAsset)), this.Pos.ToVec3d().Add(0.5, 1.0 + (i / 10), 0.5));

				return true;
			}

			// Take a placed log back
			else if (!IsInventoryEmpty)
			{
				if (TryTake(byPlayer, this.inventory[0]))
				{
					this.Api.World.PlaySoundAt(this.logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					ToggleContentState();
					return true;
				}
			}

			return false;
		}

		internal void OnBreak()
		{

		}

		public override void OnBlockPlaced(ItemStack byItemStack = null)
		{
			this.MarkDirty(true);
			base.OnBlockPlaced(byItemStack);
		}

		private bool TryPut(ItemSlot playerSlot)
		{
			ItemSlot targetSlot = inventory[0];
			int moved = playerSlot.TryPutInto(Api.World, targetSlot);
			if (moved > 0)
				MarkDirty(true);
			return moved > 0;
		}

		private bool TryTake(IPlayer byPlayer, ItemSlot itemSlot)
		{
			if (itemSlot.StackSize == 0)
				return false;

			ItemStack tempStack = itemSlot.Itemstack.Clone();

			if (!byPlayer.InventoryManager.TryGiveItemstack(tempStack)) //player has no free slots
				Api.World.SpawnItemEntity(tempStack, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));
			itemSlot.Itemstack = null;
			MarkDirty(true);

			return true;
		}

		// Changes the block from -empty- to -log- and vice versa. Courtesy return of the new block's entity.
		private BEChoppingBlock ToggleContentState()
		{
			Block block = this.Block;

			string path = block.Code.Path;
			path = path.Contains("-empty-") ? path.Replace("empty", "log") : path.Replace("log", "empty");

			block = Api.World.GetBlock(block.CodeWithPath(path));
			BlockPos pos = this.Pos.Copy();
			Api.World.BlockAccessor.SetBlock(block.BlockId, pos);

			this.MarkDirty(true);

			return Api.World.BlockAccessor.GetBlockEntity(pos) as BEChoppingBlock;
		}

		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
		{
			base.FromTreeAttributes(tree, worldForResolve);
			//this.State = tree.GetString("state");
			//this.IsBurning = tree.GetInt("burning") > 0;
			//this.burningUntilTotalDays = tree.GetDouble("burningUntilTotalDays");
			//this.burningStartTotalDays = tree.GetDouble("burningStartTotalDays");
			if (this.Api != null)
			{
				if (this.Api.Side == EnumAppSide.Client)
				{ this.Api.World.BlockAccessor.MarkBlockDirty(this.Pos); }
			}
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			//tree.SetString("state", this.State);
			//tree.SetInt("burning", this.IsBurning ? 1 : 0);
			//tree.SetDouble("burningUntilTotalDays", this.burningUntilTotalDays);
			//tree.SetDouble("burningStartTotalDays", this.burningStartTotalDays);
		}

		public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
		{
			if (IsInventoryEmpty)
				sb.AppendLine(Lang.Get("stoneroad:blockdesc-choppingblock-empty"));
			else
				sb.AppendLine(Lang.Get("stoneroad:blockdesc-choppingblock-log"));
		}

		private MeshData GenBlockMesh(ItemStack lumberSlot)
		{
			nowTesselatingObj = lumberSlot.Block;
			MeshData mesh = null;

			if (lumberSlot == null || lumberSlot.Block == null || lumberSlot.Block.Shape == null)
				return null;

			try {
				capi.Tesselator.TesselateBlock(lumberSlot.Block, out mesh);
			} catch { return mesh; }

			mesh?.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

			if (lumberSlot.Collectible is BlockLog)
			{
				mesh.Rotate(new Vec3f(0f, 0f, 0f), 0, 5 * GameMath.DEG2RAD, 0);
				mesh.Translate(-0.05f, 0.33f, 0f);
				mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
			}
			else if (lumberSlot.Collectible is BlockLogHalf)
			{
				mesh.Rotate(new Vec3f(0f, 0f, 0f), 0, 5 * GameMath.DEG2RAD, 0);
				mesh.Translate(-0.05f, 0.33f, -0.25f);
				mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
			}
			else if (lumberSlot.Collectible.Code.Path.StartsWith("strippedlog-"))
			{
				mesh.Rotate(new Vec3f(0f, 0f, 0f), 0, 5 * GameMath.DEG2RAD, 0);
				mesh.Translate(-0.05f, 0.33f, 0f);
				mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);
			}

			// Chopping block's own orientation
			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, this.Block.Shape.rotateY * GameMath.DEG2RAD, 0);

			return mesh;
		}

		public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
		{
			MeshData mesh;

			if (this.Api.World.BlockAccessor.GetBlock(this.Pos, BlockLayersAccess.Default) is BlockChoppingBlock block)
			{
				var texture = tesselator.GetTexSource(block);

				// Base model
				mesh = block.GenMesh(this.Api as ICoreClientAPI, texture);
				mesher.AddMeshData(mesh);

				if (this.inventory != null && !IsInventoryEmpty)
				{
					mesh = GenBlockMesh(inventory[0].Itemstack);
					if (mesh != null)
						mesher.AddMeshData(mesh);
				}
			}

			return true;
		}

	}

}
