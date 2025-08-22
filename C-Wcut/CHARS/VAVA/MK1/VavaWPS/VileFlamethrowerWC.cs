using System;
using System.Collections.Generic;

namespace MMXOnline;




public class DragonsWrathState : VileState {
	bool isGrounded;
	public float shootTime;
	public Point shootPOI = new Point(-1, -1);
	public Point groundShotPOI = new Point(12, -11);

	public DragonsWrathState() : base("flamethrower") {
		useGravity = false;
		useDashJumpSpeed = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();
		character.turnToInput(player.input, player);

		shootTime += Global.speedMul;
		if (shootTime >= 4) {
			if (!vile.tryUseVileAmmo(2)) {
				character.changeToIdleOrFall();
				return;
			}
			shootTime = 0;
			character.playSound("flamethrower");
			Point poiPos;
			if (!isGrounded) {
				poiPos = character.getPOIPos(shootPOI);
			} else {
				poiPos = (character.getFirstPOI() ?? character.getPOIPos(groundShotPOI));
			}
		
			new FlamethrowerSeaDragonRage(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
		}

		if (character.loopCount >= 5 || !player.input.isBHeld(player)) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
		character.vel = new Point();
		if (character.grounded && character.vel.y >= 0) {
			character.changeSpriteFromName("crouch_flamethrower", true);
			isGrounded = true;
		}
	}
}

public class SeaDragonRageState : VileState {
	bool isGrounded;
	public float shootTime;
	public Point shootPOI = new Point(-1, -1);
	public Point groundShotPOI = new Point(12, -11);

	public SeaDragonRageState() : base("flamethrower") {
		useGravity = false;
		useDashJumpSpeed = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();
		character.turnToInput(player.input, player);

		shootTime += Global.speedMul;
		if (shootTime >= 4) {
			player.vileAmmo -= 2;
			if (!vile.tryUseVileAmmo(2)) {
				character.changeToIdleOrFall();
				return;
			}
			shootTime = 0;
			character.playSound("flamethrower");
			Point poiPos;
			if (!isGrounded) {
				poiPos = character.getPOIPos(shootPOI);
			} else {
				poiPos = (character.getFirstPOI() ?? character.getPOIPos(groundShotPOI));
			}
		
			new FlamethrowerSeaDragonRage(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
		}

		if (character.loopCount >= 5 || !player.input.isBHeld(player)) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
		character.vel = new Point();
		if (character.grounded && character.vel.y >= 0) {
			character.changeSpriteFromName("crouch_flamethrower", true);
			isGrounded = true;
		}
	}
}



public class WildHorseKickState : VileState {
	bool isGrounded;
	public float shootTime;
	public Point shootPOI = new Point(-1, -1);
	public Point groundShotPOI = new Point(12, -11);

	public WildHorseKickState() : base("flamethrower") {
		useGravity = false;
		useDashJumpSpeed = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();
		character.turnToInput(player.input, player);

		shootTime += Global.speedMul;
		if (shootTime >= 4) {
			player.vileAmmo -= 2;
			if (!vile.tryUseVileAmmo(2)) {
				character.changeToIdleOrFall();
				return;
			}
			shootTime = 0;
			character.playSound("flamethrower");
			Point poiPos;
			if (!isGrounded) {
				poiPos = character.getPOIPos(shootPOI);
			} else {
				poiPos = (character.getFirstPOI() ?? character.getPOIPos(groundShotPOI));
			}
		
			new FlamethrowerWildHorseKick(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
		}

		if (character.loopCount >= 5 || !player.input.isBHeld(player)) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
		character.vel = new Point();
		if (character.grounded && character.vel.y >= 0) {
			character.changeSpriteFromName("crouch_flamethrower", true);
			isGrounded = true;
		}
	}
}


public class GreenEyedLampState : CharState {
	Character vile;

	public GreenEyedLampState() : base("green_eyed_lamp", "", "") {
		enterSound = "ryuenjin";
		useDashJumpSpeed = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();

		
			

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}
