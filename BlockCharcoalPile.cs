using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StoneRoad
{
	public class BlockCharcoalPile : BlockLayeredSlowDig
	{

		public override float RandomSoundPitch(IWorldAccessor world)
		{
			return (float)world.Rand.NextDouble() * 0.24f + 0.88f;
		}
	}
}
