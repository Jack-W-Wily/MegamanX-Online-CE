using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;


public class XRacer : Character {
	public XRacer(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.XRacer;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "xracer_" + spriteName;
	}
}





public class ZeroIrisRacer : Character {
	public ZeroIrisRacer(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.ZeroIrisRacer;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "zeroirisracer_" + spriteName;
	}
}



public class GBDRacer : Character {
	public GBDRacer(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.GBDRacer;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "gbdracer_" + spriteName;
	}
}




public class VileRacerMK1 : Character {
	public VileRacerMK1(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.VileRacerMK1;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "vilemk1racer_" + spriteName;
	}
}





public class VileRacerMK2 : Character {
	public VileRacerMK2(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.VileRacerMk2;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "vilemk2racer_" + spriteName;
	}
}





public class VileRacerMKV : Character {
	public VileRacerMKV(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.VileRacerMKV;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "vilemk5racer_" + spriteName;
	}
}





public class SigmaRacerX1 : Character {
	public SigmaRacerX1(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.SigmaRacerX1;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "sigma1racer_" + spriteName;
	}
}



public class SigmaRacerX2 : Character {
	public SigmaRacerX2(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.SigmaRacerX2;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
	
		return "sigma2racer_" + spriteName;
	}
}



public class DopplerRacer : Character {
	public DopplerRacer(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.DopplerRacer;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		
		
		return "dopplerracer_" + spriteName;
	}
}
