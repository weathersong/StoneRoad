﻿using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace StoneRoad
{
	public class BlockCharcoalPit : Block, IIgnitable
	{
		//Vec3f[] basePos;
		WorldInteraction[] interactions;

		public override void OnLoaded(ICoreAPI api)
		{
			base.OnLoaded(api);

			interactions = ObjectCacheUtil.GetOrCreate(api, "charcoalpitInteractions", () =>
			{
				List<ItemStack> canIgniteStacks = BlockBehaviorCanIgnite.CanIgniteStacks(api, true);

				return new WorldInteraction[]
				{
					new WorldInteraction()
					{
						ActionLangCode = "blockhelp-firepit-ignite",
						MouseButton = EnumMouseButton.Right,
						HotKeyCode = "shift",
						Itemstacks = canIgniteStacks.ToArray(),
						GetMatchingStacks = (wi, bs, es) => {
							BECharcoalPit becp = api.World.BlockAccessor.GetBlockEntity(bs.Position) as BECharcoalPit;
							if (becp?.Lit == false)
							{
								return wi.Itemstacks;
							}
							return null;
						}
					}
				};
			});
		}

		public Vintagestory.GameContent.EnumIgniteState OnTryIgniteBlock(EntityAgent byEntity, BlockPos pos, float secondsIgniting)
		{
			BECharcoalPit becp = api.World.BlockAccessor.GetBlockEntity(pos) as BECharcoalPit;
			if (becp == null || becp.Lit) return Vintagestory.GameContent.EnumIgniteState.NotIgnitablePreventDefault;

			return secondsIgniting > 3 ? Vintagestory.GameContent.EnumIgniteState.IgniteNow : Vintagestory.GameContent.EnumIgniteState.Ignitable;
		}

		public void OnTryIgniteBlockOver(EntityAgent byEntity, BlockPos pos, float secondsIgniting, ref EnumHandling handling)
		{
			BECharcoalPit becp = api.World.BlockAccessor.GetBlockEntity(pos) as BECharcoalPit;

			if (becp != null && !becp.Lit) becp.IgniteNow();

			handling = EnumHandling.PreventDefault;
		}

		public override bool ShouldReceiveClientParticleTicks(IWorldAccessor world, IPlayer player, BlockPos pos, out bool isWindAffected)
		{
			bool val = base.ShouldReceiveClientParticleTicks(world, player, pos, out _);
			isWindAffected = true;

			return val;
		}

		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
		}

		public EnumIgniteState OnTryIgniteStack(EntityAgent byEntity, BlockPos pos, ItemSlot slot, float secondsIgniting)
		{
			throw new NotImplementedException();
		}
	}

}
