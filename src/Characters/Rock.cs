namespace MMXOnline;

public class Rock : Character {
	public Rock(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
		charId = CharIds.Rock;
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		return "rock_" + spriteName;
	}
}


public class Reploid : Character {
	public Reploid(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "tt_csnail_noshell_" + spriteName;
	}
}

public class NightmareGhost : Character {
	public NightmareGhost(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}

	public override void update() {
		
		base.update();
		if (player.input.isPressed(Control.Special1, player) && !isInvisible() &&
				  (charState is Dash || charState is AirDash)) {
				charState.isGrabbing = true;
				changeSpriteFromName("grab", true);
			}
		}

	public override Projectile getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile proj = null;
		 if (sprite.name.Contains("run")) {
			proj = new GenericMeleeProj(new XUPGrab(), centerPoint, ProjIds.UPGrab, player, 0, 0, 0);
		}
		return proj;
	}

	public override bool canDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "nghost_" + spriteName;
	}
}
