using System;
using System.Collections.Generic;
using System.Text;

namespace StoneRoad
{
	public class StoneRoadConfig
	{
		public double LumberDryingHours;
		public int LumberDryingCurePctChance;

		public double LumberStraighteningHours;
		public int LumberStraighteningCurePctChance;

		public float CharcoalPitBurnHours;

		public int AxeChopLogCost;
		public int AxeStripLogCost;
		public int AxeSplitFirewoodCost;

		public bool DebugLogging;

		public StoneRoadConfig()
		{

		}

		public void ResetToDefaults()
		{
			LumberDryingHours = 110;
			LumberDryingCurePctChance = 5;

			LumberStraighteningHours = 22;
			LumberStraighteningCurePctChance = 60;

			CharcoalPitBurnHours = 60;

			AxeChopLogCost = 4;
			AxeStripLogCost = 4;
			AxeSplitFirewoodCost = 2;

			DebugLogging = true;
		}

	}
}
