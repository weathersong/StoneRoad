using System;
using System.Collections.Generic;
using System.Text;

namespace StoneRoad
{
	public class StoneRoadConfig
	{
		public double LumberDryingHours;
		public int LumberDryingCurePctChance;

		public double LumberSteamingHours;
		public int LumberSteamingCurePctChance;

		public float CharcoalPitBurnHours;

		public bool DebugLogging;

		public StoneRoadConfig()
		{

		}

		public void ResetToDefaults()
		{

			LumberDryingHours = 110;
			LumberDryingCurePctChance = 5;

			LumberSteamingHours = 22;
			LumberSteamingCurePctChance = 60;

			CharcoalPitBurnHours = 60;

			DebugLogging = true;
		}

	}
}
