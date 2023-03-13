using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace StoneRoad
{
	public class ItemCharcoalPoor : ItemPileable
	{
		protected override AssetLocation PileBlockCode => new AssetLocation("stoneroad", "charcoalpoorpile");
	}
}
