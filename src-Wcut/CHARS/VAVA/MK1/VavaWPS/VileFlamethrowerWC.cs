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


			if (!player.input.isHeld(Control.Up,player)){
			new FlamethrowerSeaDragonRage(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
			} else {
				new FlamethrowerDragonsWrath(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
			}

			

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
			if (!player.input.isHeld(Control.Up,player)){
			new FlamethrowerWildHorseKick(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
			} else {
				new FlamethrowerDragonsWrath(
					poiPos, character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
			}
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

		
			

			if (character.isAnimOver()) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}









public class BurningDriveProj : Projectile {
	public BurningDriveProj(
		Point pos, int xDir, Actor owner, Player player, ushort? netId,
		float damage = 6, int flinch = 26, bool rpc = false
	) : base(
		pos, xDir, owner, "burningdrive_proj", netId, player
	) {
		damager.damage = damage;
		damager.flinch = flinch;
		damager.hitCooldown = 30;
		reflectable = false;
		setIndestructableProperties();
		maxTime = 10f / 60f;
		projId = (int)ProjIds.BurningDriveProj;
		isMelee = true;
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		if (ownerPlayer?.character != null) {
			ownerActor = ownerPlayer.character;
		}
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new BurningDriveProj(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void preUpdate() {
		base.preUpdate();
		if (frameIndex % 2 == 1) {
			alpha = 0.125f;
		} else {
			alpha = 1f;
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (owner?.character != null) {
			incPos(owner.character.deltaPos);
		}
	}
}




public class BurningDriveState : CharState {
	int bombNum;

	Vile vile = null!;

	public BurningDriveState(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			var proj = new BurningDriveProj(
					poi.Value, character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				);
			proj.vel = new Point(character.xDir * 100, 0);
			character.playSound("flamemOilBurn", forcePlay: false, sendRpc: true);

		}

		if (stateTime > 0.25f) {
			character.changeToIdleOrFall();
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


