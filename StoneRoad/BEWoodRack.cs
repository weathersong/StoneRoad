﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StoneRoad
{
	public class BEWoodRack : BlockEntityDisplayCase, ITexPositionSource
	{
		// Each slot accomodates 1 board or 2 firewood
		private readonly int maxSlots = 8;

		protected static readonly Random Rnd = new Random();

		private BlockFacing ownFacing;

		protected InventoryGeneric inventory;
		public override InventoryBase Inventory => this.inventory;
		public override string InventoryClassName => "woodrack";
		private int FirstFreeSlot => GetFirstFreeSlot();

		public enum WoodRackStates
		{
			Starting = 0, Drying = 1, Done = 2
		}
		public WoodRackStates State;
		public bool IsStarting => State == WoodRackStates.Starting;
		public bool IsDrying => State == WoodRackStates.Drying;
		public bool IsDone => State == WoodRackStates.Done;

		private double dryingUntilTotalDays;
		private double dryingStartTotalDays;
		private Random rnd;

		private AssetLocation logSound;

		public BEWoodRack()
		{
			State = WoodRackStates.Starting;
			inventory = new InventoryGeneric(maxSlots, null, null);
			//meshes = new MeshData[maxSlots];
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			inventory.LateInitialize("woodrack" + "-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);
			RegisterGameTickListener(OnGameTick, 500);
			logSound = new AssetLocation("game", "sounds/block/planks");

			rnd = new Random();

			ownFacing = BlockFacing.FromCode(api.World.BlockAccessor.GetBlock(Pos).LastCodePart());
		}

		private void OnGameTick(float dt)
		{
			if (Api.Side == EnumAppSide.Server && IsDrying && dryingUntilTotalDays < Api.World.Calendar.TotalDays)
				DoConvert();
		}

		private void CheckStarting()
		{
			if (State == WoodRackStates.Starting && FirstFreeSlot == -1)
			{
				State = WoodRackStates.Drying;
				BlockWoodRack block = Block as BlockWoodRack;
				double lumberDryingHours = block.LumberDryingHours;
				dryingUntilTotalDays = Api.World.Calendar.TotalDays + (lumberDryingHours / 24.0);
				dryingStartTotalDays = Api.World.Calendar.TotalDays;
				MarkDirty();
			}
		}

		private void DoConvert()
		{
			for (int i = 0; i < maxSlots; i++)
			{
				string stackPath = inventory[i].Itemstack.Collectible.Code.Path;
				// firewood
				if (stackPath == "firewood")
				{
					// 1.18: Use the vanilla aged firewood.
					// This isn't just for convenience, but code even as deep as ItemDryGrass refers to ItemFirewood (which is the same class for aged) for firepit / charcoal pit construction...
					//inventory[i].Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation("stoneroad", "firewood-aged")), 2);
					inventory[i].Itemstack = new ItemStack(Api.World.GetItem(new AssetLocation("agedfirewood")), 2);
				}
				// raw lumber
				else if (stackPath.StartsWith("uncuredplank-raw-"))
				{
					float lumberDryingCurePctChance = Block is BlockWoodRack block ? block.LumberDryingCurePctChance : 100;
					if (rnd.Next(100) < lumberDryingCurePctChance)
						inventory[i].Itemstack = new ItemStack( Api.World.GetItem(new AssetLocation("plank-" + stackPath.Replace("uncuredplank-raw-", ""))) );
					else
						inventory[i].Itemstack = new ItemStack( Api.World.GetItem(new AssetLocation("stoneroad", stackPath.Replace("raw", "warped"))) );
				}
			}
			State = WoodRackStates.Done;
			dryingUntilTotalDays = 0;
			MarkDirty();
		}

		private int GetFirstFreeSlot()
		{
			int slot = 0;
			bool found = false;
			do {
				if (inventory[slot].Empty)
					found = true;
				else
					slot++;
			} while (slot < maxSlots && !found);
			if (!found)
				slot = -1;
			return slot;
		}

		internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
		{
			if (IsDrying)
				return false;

			ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

			string playerPath = "";
			if (playerSlot.Itemstack?.Collectible != null)
				playerPath = playerSlot.Itemstack.Collectible.Code.Path;

			// Put in firewood
			if (IsStarting && playerPath == "firewood")
			{
				if (playerSlot.Itemstack.StackSize < 2)
					return false;
				int i = FirstFreeSlot;
				if (i > -1 && TryPut(playerSlot, inventory[i], 2))
				{
					Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					CheckStarting();
					return true;
				}
			}

			// Put in raw lumber
			else if (IsStarting && playerPath.Contains("uncuredplank-raw"))
			{
				int i = FirstFreeSlot;
				if (i > -1 && TryPut(playerSlot, inventory[i]))
				{
					Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					CheckStarting();
					return true;
				}
			}

			// Take something out
			else if (IsDone || FirstFreeSlot > -1)
			{
				int i = FirstFreeSlot - 1;
				if (i == -2)
					i = maxSlots - 1;
				if (i > -1 && TryTake(byPlayer, inventory[i]))
				{
					Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					if (IsDone && FirstFreeSlot == 0)
						State = WoodRackStates.Starting;
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
			MarkDirty(true); // orientation
			base.OnBlockPlaced(byItemStack);
		}

		private bool TryPut(ItemSlot playerSlot, ItemSlot targetSlot, int quantity = 1)
		{
			int moved = playerSlot.TryPutInto(Api.World, targetSlot, quantity);
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

		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
		{
			base.FromTreeAttributes(tree, worldForResolve);
			State = (WoodRackStates) tree.GetInt("state");
			dryingUntilTotalDays = tree.GetDouble("dryingUntilTotalDays");
			dryingStartTotalDays = tree.GetDouble("dryingStartTotalDays");
			if (Api?.Side == EnumAppSide.Client)
				Api.World.BlockAccessor.MarkBlockDirty(Pos);
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			tree.SetInt("state", (int)State);
			tree.SetDouble("dryingUntilTotalDays", dryingUntilTotalDays);
			tree.SetDouble("dryingStartTotalDays", dryingStartTotalDays);
		}

		public void GetContentsInfo(StringBuilder sb)
		{
			// Seems superfluous for the drying rack
			/*
			for (int i = 0; i < maxSlots - 1; i++)
				if (!inventory[i].Empty)
					if (inventory[i].Itemstack.Collectible.Code.Path.StartsWith("uncuredplank-"))
						sb.AppendLine(Lang.Get("stoneroad:item-" + inventory[i].Itemstack.Collectible.Code.Path));
					else
						sb.AppendLine(Lang.Get("item-" + inventory[i].Itemstack.Collectible.Code.Path));
			*/
		}

		public void GetProgressInfo(StringBuilder sb)
		{
			double hoursPassed = (Api.World.Calendar.TotalDays - dryingStartTotalDays) * 24;
			BlockWoodRack block = Block as BlockWoodRack;
			double lumberDryingHours = block.LumberDryingHours;

			if (lumberDryingHours - hoursPassed < 1.1)
				sb.AppendLine( Lang.Get("stoneroad:Done in about an hour.") );
			else
			{
				string timePassedText = hoursPassed > 24 ?
					Lang.Get("{0} days", Math.Round(hoursPassed / Api.World.Calendar.HoursPerDay, 1)) :
					Lang.Get("{0} hours", Math.Round(hoursPassed));
				string timeTotalText = lumberDryingHours > 24 ?
					Lang.Get("{0} days", Math.Round(lumberDryingHours / Api.World.Calendar.HoursPerDay, 1)) :
					Lang.Get("{0} hours", Math.Round(lumberDryingHours));
				sb.AppendLine(Lang.Get("stoneroad:Drying for {0} / {1}", timePassedText, timeTotalText));
			}
		}

		public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
		{
			// When base.GetPlacedBlockInfo is called

			GetContentsInfo(sb);
			if (sb.Length > 0)
				sb.AppendLine();

			// Then the blockdesc goes down here.
		}

		private MeshData GenBlockMesh(ItemStack lumberSlot, int index)
		{
			this.nowTesselatingObj = lumberSlot.Block;
			MeshData mesh = null;

			if (lumberSlot == null || lumberSlot.Block == null || lumberSlot.Block.Shape == null)
				return null;

			try	{
				capi.Tesselator.TesselateBlock(lumberSlot.Block, out mesh);
			} catch { return mesh; }

			mesh?.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

			float radX = 90 * GameMath.DEG2RAD;
			float radY = 0 * GameMath.DEG2RAD;
			float radZ = 90 * GameMath.DEG2RAD;
			mesh.Rotate(new Vec3f(0f, 0f, 0f), radX, radY, radZ);

			mesh.Translate((0.2f * ((index % 4) + 1)) + 0.025f, (index / 4 * 0.44f) + 0.75f, 0f);

			//mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);

			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Block.Shape.rotateY * GameMath.DEG2RAD, 0);

			return mesh;
		}

		private MeshData GetItemMesh(string shapePath, ITexPositionSource texture, int index)
		{
			IAsset asset = capi.Assets.TryGet(shapePath + ".json");
			Shape shape = asset.ToObject<Shape>();
			MeshData mesh = null;

			try
			{
				capi.Tesselator.TesselateShape(shapePath, shape, out mesh, texture, new Vec3f(0, 0, 0));
			}
			catch { return mesh; }

			mesh.Translate( 0.25f * (index % 4) + (0.1f * (index % 4)) - 0.15f, (index / 4 * 0.87f) + 0.25f, 0f );

			mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.5f, 0.5f, 0.9f);

			mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Block.Shape.rotateY * GameMath.DEG2RAD, 0); //orient based on direction 

			return mesh;
		}

		public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
		{
			MeshData mesh;

			if (Api.World.BlockAccessor.GetBlock(Pos, BlockLayersAccess.Default) is BlockWoodRack block)
			{
				// Note the texture source is this block, acting as a kind of texture atlas.
				// Any textures that must be on added meshes (ie GetItemMesh) must have corresponding textures on *this* block.
				// For example, firewood2's textures (bark, firewoodfront, firewoodside) must all be in the textures{} block of shapes/woodrack.json!
				var texture = tesselator.GetTextureSource(block);

				// Base model
				mesh = block.GenMesh(Api as ICoreClientAPI, texture);
				mesher.AddMeshData(mesh);

				if (inventory != null)
					for (int i = 0; i < maxSlots; i++)
						if (!inventory[i].Empty)
						{
							string shapePath = inventory[i].Itemstack.Collectible.Code.Path;
							if (shapePath == "firewood")
								mesh = GetItemMesh("stoneroad:shapes/item/firewood2", texture, i);
							else if (shapePath == "agedfirewood")
								mesh = GetItemMesh("stoneroad:shapes/item/agedfirewood2", texture, i);
							else
							{
								// planks-as-items are a bit tricky because of the wood variant, so there's a "fake" version that looks the same, but is a block
								//mesh = GenBlockMesh(inventory[i].Itemstack, i);
								AssetLocation plankBlock = new AssetLocation("stoneroad", $"plankblock-{inventory[i].Itemstack.Collectible.Variant["wood"] }");
								ItemStack tempStack = new ItemStack(Api.World.GetBlock(plankBlock));
								mesh = GenBlockMesh(tempStack, i);
							}
							if (mesh != null)
								mesher.AddMeshData(mesh);
						}
			}

			return true;
		}

	}

}
