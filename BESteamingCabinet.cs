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

namespace StoneRoad
{

	public class BESteamingCabinet : BlockEntityDisplayCase
	{
		static SimpleParticleProperties breakSparks;
		static SimpleParticleProperties smallMetalSparks;
		static SimpleParticleProperties smoke;

		//first 4 for planks, last 1 for firewood
		private readonly int maxSlots = 5;
		protected static readonly Random Rnd = new Random();
		public string State { get; protected set; }
		private readonly long particleTick;

		private BlockFacing ownFacing;
		private double burningUntilTotalDays;
		private double burningStartTotalDays;
		public override string InventoryClassName => "steamcabinet";
		public override InventoryBase Inventory => this.inventory;
		private int FirstFreeSlot => GetFirstFreeSlot();

		private AssetLocation doorOpenSound;
		private AssetLocation doorCloseSound;
		private AssetLocation logSound;

		private Random rnd;

		static BESteamingCabinet()
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

		public BESteamingCabinet()
		{
			inventory = new InventoryGeneric(maxSlots, null, null);
			meshes = new MeshData[maxSlots];
		}

		public bool IsBurning { get; private set; }

		public ItemSlot WoodSlot => inventory[4];

		public ItemStack WoodStack
		{
			get => inventory[4].Itemstack;
			set => inventory[4].Itemstack = value;
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			inventory.LateInitialize("steamcabinet" + "-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);
			RegisterGameTickListener(OnGameTick, 500);
			doorOpenSound = new AssetLocation("game", "sounds/block/chestopen");
			doorCloseSound = new AssetLocation("game", "sounds/block/chestclose");
			logSound = new AssetLocation("game", "sounds/block/planks");

			rnd = new Random();

			ownFacing = BlockFacing.FromCode(api.World.BlockAccessor.GetBlock(Pos).LastCodePart());
		}


		public override void OnBlockUnloaded()
		{
			base.OnBlockUnloaded();
			UnregisterGameTickListener(particleTick);
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
			for (int i = 0; i < 4; i++)
			{
				if (inventory[i] == null || inventory[i].Itemstack == null) // might have only a partial load
					continue;
				ItemStack thisStack = inventory[i].Itemstack;
				if (thisStack.Collectible.Code.Path.StartsWith("uncuredplank-warped-"))
				{
					float lumberSteamingCurePctChance = Block is BlockSteamingCabinet block ? block.LumberSteamingCurePctChance : 100;
					if (rnd.Next(100) < lumberSteamingCurePctChance)
						inventory[i].Itemstack = new ItemStack( Api.World.GetItem(new AssetLocation( $"plank-{thisStack.Collectible.Variant["wood"]}" )) );
					// else stays warped, steam again
				}
			}
			IsBurning = false;
			burningUntilTotalDays = 0;
			State = "closed";
			MarkDirty(true);
		}

		public bool TryIgnite()
		{
			if (!CanIgnite() || IsBurning)
				return false;

			IsBurning = true;
			State = "lit";

			BlockSteamingCabinet block = Block as BlockSteamingCabinet;
			double lumberSteamingHours = block.LumberSteamingHours;

			burningUntilTotalDays = Api.World.Calendar.TotalDays + (lumberSteamingHours / 24.0);
			burningStartTotalDays = Api.World.Calendar.TotalDays;
			MarkDirty();
			return true;
		}

		public bool CanIgnite()
		{
			return !IsBurning && WoodSlot.StackSize == 4 && this.inventory[0].StackSize > 0;
		}

		// Returns the first available empty inventory slot, returns -1 if all slots are full
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
			} while (slot < maxSlots && !found);
			if (!found)
				slot = -1;
			return slot;
		}

		internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
		{
			ItemSlot playerSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

			string playerPath = "";
			if (playerSlot.Itemstack?.Collectible != null)
				playerPath = playerSlot.Itemstack.Collectible.Code.Path;

			string cabinetPath = "";
			if (inventory[0].Itemstack?.Collectible != null)
				cabinetPath = inventory[0].Itemstack.Collectible.Code.Path;

			if (State == "open")
			{
				// put in firewood
				if (playerPath == "firewood")
				{
					//if the firewood slot is empty or less than full add one
					if ((WoodSlot.Empty || WoodStack?.StackSize < 4) && TryPut(playerSlot, WoodSlot))
					{
						Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
						return true;
					}
				}
				// put in raw lumber
				else if (playerPath.Contains("uncuredplank-warped"))
				{
					int i = FirstFreeSlot;
					if (i > -1 && i < 4 && TryPut(playerSlot, inventory[i]))
					{
						Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
						return true;
					}
				}
				// holding a torch shortcuts the take-one-out below
				else if (playerPath.Contains("torch"))
				{

				}
				// take out one - this needs to be always allowed since contents can be mixed-state
				else if (cabinetPath != "")
				{
					int i = FirstFreeSlot - 1;
					if (i == -2)
						i = 3; // wrap to the last *plank* slot, not the wood slot
					if (i > -1 && i < 4 && TryTake(byPlayer, this.inventory[i]))
					{
						Api.World.PlaySoundAt(logSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
						return true;
					}
				}
			}
			else if (State == "closed")
			{
				Api.World.PlaySoundAt(doorOpenSound, blockSel.Position.X, blockSel.Position.Y - 1, blockSel.Position.Z, byPlayer);
				State = "open";
				MarkDirty(true);
				return true;
			}

			if (State == "open")
			{
				//close the door
				Api.World.PlaySoundAt(doorCloseSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
				State = "closed";
				MarkDirty(true);
				return true;
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
			this.State = "closed";
			this.MarkDirty(true);
			base.OnBlockPlaced(byItemStack);
		}

		private bool TryPut(ItemSlot playerSlot, ItemSlot targetSlot)
		{
			var moved = playerSlot.TryPutInto(this.Api.World, targetSlot);
			if (moved > 0)
			{
				this.MarkDirty(true);
				return moved > 0;
			}
			return false;
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
			State = tree.GetString("state");
			IsBurning = tree.GetInt("burning") > 0;
			burningUntilTotalDays = tree.GetDouble("burningUntilTotalDays");
			burningStartTotalDays = tree.GetDouble("burningStartTotalDays");
			if (Api?.Side == EnumAppSide.Client)
				Api.World.BlockAccessor.MarkBlockDirty(Pos);
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			base.ToTreeAttributes(tree);
			tree.SetString("state", State);
			tree.SetInt("burning", IsBurning ? 1 : 0);
			tree.SetDouble("burningUntilTotalDays", burningUntilTotalDays);
			tree.SetDouble("burningStartTotalDays", burningStartTotalDays);
		}

		public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
		{
			for (int i = 0; i < 4; i++)
				if (!inventory[i].Empty)
					if (inventory[i].Itemstack.Collectible.Code.Path.StartsWith("uncuredplank-"))
						sb.AppendLine(Lang.Get("stoneroad:item-" + inventory[i].Itemstack.Collectible.Code.Path));
					else
						sb.AppendLine(Lang.Get("item-" + inventory[i].Itemstack.Collectible.Code.Path));

			int WoodCount = WoodSlot.Empty ? 0 : WoodStack.StackSize;
			sb.AppendLine($"{WoodCount}/4 {Lang.Get("item-firewood")}");

			double percentComplete = Math.Round((Api.World.Calendar.TotalDays - burningStartTotalDays) / (burningUntilTotalDays - burningStartTotalDays) * 100, 0);

			if (0 <= percentComplete && percentComplete < 100 && IsBurning)
				sb.AppendLine("" + percentComplete + "% " + Lang.Get("stoneroad:blockhelp-steamcabinet-complete"));

			sb.AppendLine();
			//sb.AppendLine(string.Format("DEBUG: {3} , Current total days: {0} , BurningStart total days: {1} , BurningUntil total days: {2}", this.Api.World.Calendar.TotalDays, this.burningStartTotalDays, this.burningUntilTotalDays, this.burning));
		}

		private MeshData GenLumberMesh(ItemStack lumberStack, int count)
		{
			this.nowTesselatingObj = lumberStack.Block;
			MeshData mesh = null;

			if (lumberStack?.Block?.Shape != null)
			{
				try	{
					capi.Tesselator.TesselateBlock(lumberStack.Block, out mesh);
				} catch { return mesh; }
				mesh?.RenderPassesAndExtraBits.Fill((short)EnumChunkRenderPass.BlendNoCull);

				float radX = -10 * GameMath.DEG2RAD;
				float radY = 90 * GameMath.DEG2RAD;
				float radZ = -90 * GameMath.DEG2RAD;
				// Rear left
				if (count == 0)
				{
					mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), radX, radY, radZ);
					mesh.Translate(0.15f, 0.33f, -0.75f);
				}
				// Rear right
				else if (count == 1)
				{
					mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), -radX, radY, radZ);
					mesh.Translate(0.25f, 0.5f, -0.15f);
				}
				// Fore left
				else if (count == 2)
				{
					mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), radX, radY, radZ);
					mesh.Translate(-0.25f, 0.33f, -0.75f);
				}
				// Fore right
				else
				{
					mesh.Rotate(new Vec3f(0.5f, 0.5f, 0.5f), -radX, radY, radZ);
					mesh.Translate(-0.15f, 0.5f, -0.15f);
				}

				mesh.Scale(new Vec3f(0.5f, 0, 0.5f), 0.75f, 0.75f, 0.75f);

				mesh.Rotate(new Vec3f(0.5f, 0, 0.5f), 0, Block.Shape.rotateY * GameMath.DEG2RAD, 0);
			}
			return mesh;
		}

		public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
		{
			MeshData mesh;
			string shapeBase = "stoneroad:shapes/block/steamcabinet/steamcabinetblock-placed";
			int i = 0;
			if (Api.World.BlockAccessor.GetBlock(Pos, BlockLayersAccess.Default) is BlockSteamingCabinet block)
			{
				ITexPositionSource texture = tesselator.GetTexSource(block);
				// Base model
				mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase, texture, State, i);
				mesher.AddMeshData(mesh);

				// Door next
				shapeBase = shapeBase.Replace("placed", "door");
				mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase, texture, State, i);
				mesher.AddMeshData(mesh);

				if (this.inventory != null)
				{
					// Wood 
					if (!WoodSlot.Empty)
					{
						if (State == "lit")
						{ shapeBase = shapeBase.Replace("door", "lit"); }
						else
						{ shapeBase = shapeBase.Replace("door", "log"); }
						for (i = 1; i <= WoodStack.StackSize; i++)
						{
							mesh = block.GenMesh(Api as ICoreClientAPI, shapeBase, texture, State, i);
							mesher.AddMeshData(mesh);
						}
					}

					// Contents
					// dont render if lit or closed
					if (State != "lit")
						for (i = 0; i < 4; i++)
							if (!this.inventory[i].Empty)
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
