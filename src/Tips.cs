using System.Collections.Generic;

namespace MMXOnline;

public class Tips {
	public static List<string[]> xTipsPool = new List<string[]>()
	{
		new string[]{
			"You can upgrade and customize armor in",
			"ARMOR UPGRADE MENU, each armor piece has its own effect."
		}
		};

	public static List<string[]> zeroTipsPool = new List<string[]>()
	{
		
		};

	public static List<string[]> vileTipsPool = new List<string[]>()
		{
		};

	public static List<string[]> axlTipsPool = new List<string[]>()
		{
		};

	public static List<string[]> sigmaTipsPool = new List<string[]>()
		{
		};

	public static List<string[]> tipsPool = new List<string[]>()
		{
			new string[]
			{
				"Hold UP KEY to block, blocking reflects any",
				"damage below 3."
			},
			new string[]{
				"Some attacks like grabs and knockdowns",
				"can break block."	
			},
			new string[]
			{
				"In CTF, hold the DASH key to drop the flag.",
				"Drop time is 2 seconds."
			},
		};

	public static List<string[]> raceTipsPool = new List<string[]>()
		{
			new string[]
			{
				"In RACE mode, you can push the camera left or right",
				"in a Ride Chaser by holding WEAPON L or WEAPON R.",
				"Use this to see further ahead or behind."
			},
			new string[]
			{
				"In RACE mode, you cannot exit a Ride Chaser",
				"once you have entered it."
			},
			new string[]
			{
				"In RACE mode, you can damage other Ride Chasers",
				"with your Ride Chaser gun by holding SHOOT."
			},
		};

	public static string[] getRandomTip(int charNum) {
		var tipsPool = new List<string[]>(Tips.tipsPool);
		if (Global.level.isRace()) tipsPool = Tips.raceTipsPool;
		else if (charNum == (int)CharIds.X) tipsPool.AddRange(Tips.xTipsPool);
		else if (charNum == (int)CharIds.Zero) tipsPool.AddRange(Tips.zeroTipsPool);
		else if (charNum == (int)CharIds.Vile) tipsPool.AddRange(Tips.vileTipsPool);
		else if (charNum == (int)CharIds.AxlWC) tipsPool.AddRange(Tips.axlTipsPool);
		else if (charNum == (int)CharIds.Sigma) tipsPool.AddRange(Tips.sigmaTipsPool);
		return tipsPool.GetRandomItem();
	}
}
