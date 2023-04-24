using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace StoneRoad
{
	internal class BEUncuredPlank : BlockEntity
	{
		private ICoreAPI coreAPI;
		//private long tickListener;

		//private double previousHourChecked;
		//private double thisHourChecked;

		//public double TimeRemaining; 
		//public float CureChance;

		//private Random rnd;

		public override void Initialize(ICoreAPI api)
		{
			if (api.Side == EnumAppSide.Server)
			{
				coreAPI = api;

				//tickListener = api.World.RegisterGameTickListener(HourlyTicker, (int)(3600000 / api.World.Calendar.SpeedOfTime));
				this.MarkDirty(false);
			}

			//rnd = new Random();

			base.Initialize(api);
		}

		public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
		{
			//TimeRemaining = tree.GetDouble("timeremaining");
			//CureChance = tree.GetFloat("curechance");
			//previousHourChecked = tree.GetDouble("previoushourchecked", worldAccessForResolve.Calendar.TotalHours);

			base.FromTreeAttributes(tree, worldAccessForResolve);
		}

		public override void ToTreeAttributes(ITreeAttribute tree)
		{
			//tree.SetDouble("timeremaining", TimeRemaining);
			//tree.SetFloat("curechance", CureChance);

			//if (previousHourChecked != 0)
			//{
			//	tree.SetDouble("previoushourchecked", previousHourChecked);
			//}
			//else
			//{
			//	tree.SetDouble("previoushourchecked", Api.World.Calendar.TotalHours);
			//	previousHourChecked = Api.World.Calendar.TotalHours;
			//}

			base.ToTreeAttributes(tree);
		}

		public override void OnBlockBroken(IPlayer player)
		{
			base.OnBlockBroken(player);

			//if (Api.Side == EnumAppSide.Server)
			//	Api.World.UnregisterGameTickListener(tickListener);
		}

		public override void OnBlockRemoved()
		{
			base.OnBlockRemoved();

			//if (Api.Side == EnumAppSide.Server)
			//	Api.World.UnregisterGameTickListener(tickListener);
		}

		public override void OnBlockUnloaded()
		{
			base.OnBlockUnloaded();

			//Api.World.UnregisterGameTickListener(tickListener);
		}

		private void HourlyTicker(float deltaTime)
		{
			//thisHourChecked = coreAPI.World.Calendar.TotalHours;

			//TimeRemaining -= (thisHourChecked - previousHourChecked);

			//if (TimeRemaining <= 0)
			//{
			//	if (rnd.Next(100) < CureChance)
			//		coreAPI.World.BlockAccessor.ExchangeBlock(coreAPI.World.BlockAccessor.GetBlock(new AssetLocation("stoneroad", "uncuredplank-cured-" + Block.LastCodePart())).Id, this.Pos);
			//	else
			//		coreAPI.World.BlockAccessor.ExchangeBlock(coreAPI.World.BlockAccessor.GetBlock(new AssetLocation("stoneroad", "uncuredplank-warped-" + Block.LastCodePart())).Id, this.Pos);
			//	OnBlockRemoved();
			//	coreAPI.World.BlockAccessor.RemoveBlockEntity(this.Pos);
			//}

			//previousHourChecked = thisHourChecked;
			//this.MarkDirty(true);
		}

	}

}
