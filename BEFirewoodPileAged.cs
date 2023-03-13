using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StoneRoad
{
	// Copied from (1.17's) BEFireWoodPile: https://github.com/anegostudios/vssurvivalmod/blob/9f2f42da65ac74d1aff50f480c6c38769c87890a/Legacy/BlockEntity/BEFireWoodPile.cs

	public class BEFirewoodPileAged : BlockEntityItemPile, IBlockEntityItemPile
	{
		internal AssetLocation soundLocation = new AssetLocation("sounds/block/planks");

		public override AssetLocation SoundLocation { get { return soundLocation; } }

		public override string BlockCode
		{
			get { return "firewoodpile-aged"; }
		}

		public override int DefaultTakeQuantity
		{
			get { return 2; }
		}

		public override int BulkTakeQuantity
		{
			get { return 8; }
		}

		public override int MaxStackSize { get { return 32; } }

		MeshData[] meshes
		{
			get
			{
				return ObjectCacheUtil.GetOrCreate(Api, "firewoodpile-meshes", () =>
				{
					MeshData[] meshes = new MeshData[17];

					Block block = Api.World.BlockAccessor.GetBlock(Pos);

					Shape shape = Shape.TryGet(Api, "shapes/block/wood/firewoodpile.json");

					ITesselatorAPI mesher = ((ICoreClientAPI)Api).Tesselator;

					for (int j = 0; j <= 16; j++)
					{
						mesher.TesselateShape(block, shape, out meshes[j], null, j);
					}

					return meshes;
				});
			}
		}

		public override void Initialize(ICoreAPI api)
		{
			base.Initialize(api);
			RandomizeSoundPitch = true;
		}

		public override bool OnTesselation(ITerrainMeshPool meshdata, ITesselatorAPI tesselator)
		{
			lock (inventoryLock)
			{
				int index = Math.Min(16, (int)Math.Ceiling(inventory[0].StackSize / 2.0));

				meshdata.AddMeshData(meshes[index]);
			}

			return true;
		}
	}

}
