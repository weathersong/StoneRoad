using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static StoneRoad.BEWoodRack;
using static System.Net.Mime.MediaTypeNames;

namespace StoneRoad
{

	public class BEStraighteningRack : BlockEntityDisplayCase, ITexPositionSource
	{
		static SimpleParticleProperties breakSparks;
		static SimpleParticleProperties smallMetalSparks;
		static SimpleParticleProperties smoke;

		//first 12 for planks, last 1 for firewood
		private readonly int maxSlots = 13;
		protected static readonly Random Rnd = new Random();

		public enum StraightenRackStates
		{
			Starting = 0, Steaming = 1, Done = 2
		}
		public StraightenRackStates State;

		//private readonly long particleTick;

		private BlockFacing ownFacing;

		private double burningUntilTotalDays;
		private double burningStartTotalDays;

		protected InventoryGeneric inventory;
		public override InventoryBase Inventory => this.inventory;
		public override string InventoryClassName => "straightenrack";
		private int FirstFreeSlot => GetFirstFreeSlot();

		private AssetLocation logSound;

		private Random rnd;

		static BEStraighteningRack()
		{
			smallMetalSparks = new SimpleParticleProperties(
				2, 5,
				ColorUtil.ToRgba(255, 255, 233, 83),
				new Vec3d(), new Vec3d(),
				new Vec3f(-3f, 8f, -3f),
				new Vec3f(3f, 12f, 3f),
				0.1f,
				1f,
				0.25f, 0.25f,
				EnumParticleModel.Quad
			)
			{
				WithTerrainCollision = false,
				VertexFlags = 128
			};
			smallMetalSparks.AddPos.Set(1 / 16f, 0, 1 / 16f);
			smallMetalSparks.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.QUADRATIC, -0.5f);
			smallMetalSparks.AddPos.Set(4 / 16.0, 3 / 16.0, 4 / 16.0);
			smallMetalSparks.ParticleModel = EnumParticleModel.Cube;
			smallMetalSparks.LifeLength = 0.04f;
			smallMetalSparks.MinQuantity = 1;
			smallMetalSparks.AddQuantity = 1;
			smallMetalSparks.MinSize = 0.2f;
			smallMetalSparks.MaxSize = 0.2f;
			smallMetalSparks.GravityEffect = 0f;

			breakSparks = new SimpleParticleProperties(
				40, 80,
				ColorUtil.ToRgba(255, 255, 233, 83),
				new Vec3d(), new Vec3d(),
				new Vec3f(-1f, 0.5f, -1f),
				new Vec3f(2f, 1.5f, 2f),
				0.5f,
				1f,
				0.25f, 0.25f
			)
			{
				VertexFlags = 128
			};
			breakSparks.AddPos.Set(4 / 16f, 4 / 16f, 4 / 16f);
			breakSparks.SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -0.25f);

			smoke = new SimpleParticleProperties(
				1, 1, ColorUtil.ToRgba(128, 110, 110, 110), new Vec3d(), new Vec3d(),
				new Vec3f(-0.2f, 0.3f, -0.2f), new Vec3f(0.2f, 0.3f, 0.2f), 2, 0, 0.5f, 1f, EnumParticleModel.Quad
			)
			{
				SelfPropelled = true,
				OpacityEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, -255),
				SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEAR, 2)
			};
		}

		public BEStraighteningRack()
		{
			inventory = new InventoryGeneric(maxSlots, null, null);
			//meshes = new MeshData[maxSlots];
			State = StraightenRackStates.Starting;
		}

		public bool IsBurning { get; private set; }

		public ItemSlot WoodSlot => inventory[maxSlots-1];

		public ItemStack WoodStack
		{
			get => inventory[maxSlots - 1].Itemstack;
			set => inventory[maxSlots - 1].Itemstack = value;
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			inventory.LateInitialize("straightenrack" + "-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);
			RegisterGameTickListener(OnGameTick, 500);
			logSound = new AssetLocation("game", "sounds/block/planks");

			rnd = new Random();

			ownFacing = BlockFacing.FromCode(api.World.BlockAccessor.GetBlock(Pos).LastCodePart());
		}


		public override void OnBlockUnloaded()
		{
			base.OnBlockUnloaded();
			//UnregisterGameTickListener(particleTick);
		}

		private void EmitParticles()
		{
			if (Api.World.Rand.Next(5) > 0)
			{
				smoke.MinPos.Set(Pos.X + 0.5 - 2 / 16.0, Pos.Y + 10 / 16f, Pos.Z + 0.5 - 2 / 16.0);
				smoke.AddPos.Set(4 / 16.0, 0, 4 / 16.0);
				Api.World.SpawnParticles(smoke, null);
			}

			if (Api.World.Rand.Next(3) == 0)
			{
				var dir = ownFacing.Normalf;
				var particlePos = smallMetalSparks.MinPos;
				particlePos.Set(Pos.X + 0.5, Pos.Y, Pos.Z + 0.5);
				particlePos.Sub(dir.X * (6 / 16.0) + 2 / 16f, 0, dir.Z * (6 / 16.0) + 2 / 16f);

				smallMetalSparks.MinPos = particlePos;
				smallMetalSparks.MinVelocity = new Vec3f(-0.5f - dir.X, -0.3f, -0.5f - dir.Z);
				smallMetalSparks.AddVelocity = new Vec3f(1f - dir.X, 0.6f, 1f - dir.Z);
				Api.World.SpawnParticles(smallMetalSparks, null);
			}
		}

		private void OnGameTick(float dt)
		{
			if (IsBurning)
				EmitParticles();
			if (!IsBurning)
				return;

			if (Api.Side == EnumAppSide.Server && burningUntilTotalDays < Api.World.Calendar.TotalDays)
				DoConvert();
		}

		private void DoConvert()
		{
			WoodStack = null;
			for (int i = 0; i < maxSlots-1; i++)
			{
				if (inventory[i] == null || inventory[i].Itemstack == null) // might have only a partial load
					continue;
				ItemStack thisStack = inventory[i].Itemstack;
				if (thisStack.Collectible.Code.Path.StartsWith("uncuredplank-warped-"))
				{
					float lumberStraighteningCurePctChance = Block is BlockStraighteningRack block ? block.LumberStraighteningCurePctChance : 100;
					if (rnd.Next(100) < lumberStraighteningCurePctChance)
						inventory[i].Itemstack = new ItemStack( Api.World.GetItem(new AssetLocation( $"plank-{thisStack.Collectible.Variant["wood"]}" )) );
					// else stays warped, steam again
				}
			}
			IsBurning = false;
			burningUntilTotalDays = 0;
			State = StraightenRackStates.Done;
			MarkDirty(true);
		}

		public bool TryIgnite()
		{
			if (!CanIgnite() || IsBurning)
				return false;

			IsBurning = true;
			State = StraightenRackStates.Steaming;

			BlockStraighteningRack block = Block as BlockStraighteningRack;
			double lumberStraighteningHours = block.LumberStraighteningHours;

			burningUntilTotalDays = Api.World.Calendar.TotalDays + (lumberStraighteningHours / 24.0);
			burningStartTotalDays = Api.World.Calendar.TotalDays;
			MarkDirty();
			return true;
		}

		public bool CanIgnite()
		{
			return !IsBurning && WoodSlot.StackSize == 4 && this.inventory[0].StackSize > 0;
		}

		// Returns the first available empty LUMBER inventory slot (the last inventory slot is reserved for wood), returns -1 if all slots are full
		private int GetFirstFreeSlot()
		{
			int slot = 0;
			bool found = false;
			do
			{
				if (inventory[slot].Empty)
					found = true;
				else
					slot++;
			} while (slot < maxSlots-1 && !found);
			if (!found)
				slot = -1;
			return slot;
		}

		internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
		{
			// Break block if you want to interrupt steaming
			if (State == StraightenRackStates.Steaming)
				return false;

			ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

			string playerPath = "";
			if (playerSlot.Itemstack?.Collectible != null)
				playerPath = playerSlot.Itemstack.Collectible.Code.Path;

			string cabinetPath = "";
			if (inventory[0].Itemstack?.Collectible != null)
				cabinetPath = inventory[0].Itemstack.Collectible.Code.Path;

			// Put in (aged or non) firewood
			if (playerPath.StartsWith("firewood"))
			{
				//if the firewood slot is empty or less than full add one
				if ((WoodSlot.Empty || WoodStack?.StackSize < 4) && TryPut(playerSlot, WoodSlot))
				{
					Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					return true;
				}
			}
			// Put in *warped* lumber
			else if (playerPath.Contains("uncuredplank-warped"))
			{
				int i = FirstFreeSlot;
				if (i > -1 && TryPut(playerSlot, inventory[i]))
				{
					Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					return true;
				}
			}
			// Holding a torch shortcuts the take-one-out below
			else if (playerPath.Contains("torch"))
			{

			}
			// Take out one, regardless of state
			else if (cabinetPath != "")
			{
				int i = FirstFreeSlot - 1;
				if (i == -2)
					i = maxSlots-2; // wrap to the last *lumber* slot, not the wood slot
				if (i > -1 && TryTake(byPlayer, inventory[i]))
				{
					Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
					if (State == StraightenRackStates.Done && FirstFreeSlot == 0)
						State = StraightenRackStates.Starting;
					return true;
				}
			}

			return false;
		}

		internal void OnBreak()
		{
			for (int index = maxSlots - 2; index >= 0; index--)
			{
				if (!this.inventory[index].Empty)
				{
					int stackSize = this.inventory[index].StackSize;
					if (stackSize > 0)
					{
						ItemStack stack = inventory[index].TakeOut(1);
						if (stack.Collectible.FirstCodePart() == "uncuredplank")
						{
							double d = index / 10;
							Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5 + d, 0.5));
						}
					}
					MarkDirty(true);
				}
			}

			//if lit DON'T drop the wood
			if (!IsBurning)
			{
				for (int i = 1; i <= WoodSlot.StackSize; i++)
				{
					ItemStack stack = WoodSlot.TakeOut(1);
					double d = i / 10;
					Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5 + d, 0.5));
				}
				MarkDirty(true);
			}
			else
			{
				WoodStack = null;
				MarkDirty(true);
			}
		}

		public override void OnBlockPlaced(ItemStack byItemStack = null)
		{
			State = StraightenRackStates.Starting;
			MarkDirty(true);
			base.OnBlockPlaced(byItemStack);
		}

		private bool TryPut(ItemSlot playerSlot, ItemSlot targetSlot)
		{
			int moved = playerSlot.TryPutInto(this.Api.World, targetSlot);
			if (moved > 0)
				MarkDirty(true);
			return moved > 0;
		}

		private bool TryTake(IPlayer byPlayer, ItemSlot itemSlot)
		{
			if (itemSlot.StackSize == 0)
				return false;

			ItemStack tempStack;
			if (itemSlot.Itemstack.Collectible.Code.Path.StartsWith("uncuredplank-"))
				tempStack = new ItemStack( Api.World.GetItem(new AssetLocation("stoneroad", itemSlot.Itemstack.Collectible.Code.Path)) );
			else
				tempStack = new ItemStack(Api.World.GetItem(new AssetLocation(itemSlot.Itemstack.Collectible.Code.Path)));

			if (!byPlayer.InventoryManager.TryGiveItemstack(tempStack)) //player has no free slots
				Api.World.SpawnItemEntity(tempStack, byPlayer.Entity.Pos.XYZ.Add(0, 0.5, 0));

			itemSlot.Itemstack = null;
			MarkDirty(true);
			return true;
		}

		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
		{
			base.FromTreeAttributes(tree, worldForResolve);
			State = (StraightenRackStates)tree.GetInt("state");
			IsBurning = tree.GetInt("burning") > 0;
			burningUntilTotalDays = tree.GetDouble("burningUntilTotalDays");
			burningStartTotalDays = tree.GetDouble("burningStartTotalDays");
			if (Api?.Side == EnumAppSide.Client)
				Api.World.BlockAccessor.MarkBlockDirty(Pos);
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			tree.SetInt("state", (int)State);
			tree.SetInt("burning", IsBurning ? 1 : 0);
			tree.SetDouble("burningUntilTotalDays", burningUntilTotalDays);
			tree.SetDouble("burningStartTotalDays", burningStartTotalDays);
		}

		public void GetContentsInfo(StringBuilder sb)
		{
			for (int i = 0; i < maxSlots - 1; i++)
				if (!inventory[i].Empty)
					if (inventory[i].Itemstack.Collectible.Code.Path.StartsWith("uncuredplank-"))
						sb.AppendLine(Lang.Get("stoneroad:item-" + inventory[i].Itemstack.Collectible.Code.Path));
					else
						sb.AppendLine(Lang.Get("item-" + inventory[i].Itemstack.Collectible.Code.Path));

			int WoodCount = WoodSlot.Empty ? 0 : WoodStack.StackSize;
			if (WoodCount > 0)
				sb.AppendLine($"{WoodCount}/4 {Lang.Get("item-firewood")}");
		}

		public void GetProgressInfo(StringBuilder sb)
		{
			double hoursPassed = (Api.World.Calendar.TotalDays - burningStartTotalDays) * 24;
			BlockStraighteningRack block = Block as BlockStraighteningRack;
			double lumberStraighteningHours = block.LumberStraighteningHours;

			if (lumberStraighteningHours - hoursPassed < 1.1)
				sb.AppendLine(Lang.Get("stoneroad:Done in about an hour."));
			else
			{
				string timePassedText = hoursPassed > 24 ?
					Lang.Get("{0} days", Math.Round(hoursPassed / Api.World.Calendar.HoursPerDay, 1)) :
					Lang.Get("{0} hours", Math.Round(hoursPassed));
				string timeTotalText = lumberStraighteningHours > 24 ?
					Lang.Get("{0} days", Math.Round(lumberStraighteningHours / Api.World.Calendar.HoursPerDay, 1)) :
					Lang.Get("{0} hours", Math.Round(lumberStraighteningHours));
				sb.AppendLine(Lang.Get("stoneroad:Steaming for {0} / {1}", timePassedText, timeTotalText));
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

		private MeshData GenLumberMesh(ItemStack lumberStack, int index)
		{
			this.nowTesselatingObj = lumberStack.Block;
			MeshData mesh = null;

			if (lumberStack?.Block?.Shape != null)
			{
				try	{
					capi.Tesselator.TesselateBlock(lumberStack.Block, out mesh);
				} catch { return mesh; }
				mesh?.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

				float radX = 90 * GameMath.DEG2RAD;
				float radY = 0 * GameMath.DEG2RAD;
				float radZ = 90 * GameMath.DEG2RAD;
				mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), radX, radY, radZ);
				mesh.Translate(
					-0.8125f + (0.0625f * (index / 6)) + (index % 6 * 0.125f), // spacing is regular but the offset is different for lower and upper racks
					0f + (0.3125f * (index / 6)), // upper rack is 5/16ths above the lower
					0.025f
				);

				//mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 1f, 1f, 1f);

				mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Block.Shape.rotateY * GameMath.DEG2RAD, 0);
			}
			return mesh;
		}

		public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
		{
			MeshData mesh;
			string shapeBase = "stoneroad:shapes/block/straightenrack/straightenrackblock-placed";
			int i = 0;
			if (Api.World.BlockAccessor.GetBlock(Pos, BlockLayersAccess.Default) is BlockStraighteningRack block)
			{
				ITexPositionSource texture = tesselator.GetTextureSource(block);
				// Base model
				mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase, texture, i);
				mesher.AddMeshData(mesh);

				if (inventory != null)
				{
					// Wood 
					if (!WoodSlot.Empty)
					{
						if (State == StraightenRackStates.Steaming)
							shapeBase = shapeBase.Replace("placed", "lit");
						else
							shapeBase = shapeBase.Replace("placed", "log");
						for (i = 1; i <= WoodStack.StackSize; i++)
						{
							mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase, texture, i);
							mesher.AddMeshData(mesh);
						}
					}

					// Contents
					for (i = 0; i < maxSlots-1; i++)
						if (!inventory[i].Empty)
						{
							// planks-as-items are a bit tricky because of the wood variant, so there's a "fake" version that looks the same, but is a block
							//mesh = GenBlockMesh(inventory[i].Itemstack, i);
							AssetLocation plankBlock = new AssetLocation("stoneroad", $"plankblock-{inventory[i].Itemstack.Collectible.Variant["wood"]}");
							ItemStack tempStack = new ItemStack(Api.World.GetBlock(plankBlock));
							mesh = GenLumberMesh(tempStack, i);
							if (mesh != null)
								mesher.AddMeshData(mesh);
						}
				}

			}

			return true;
		}

	}

}
