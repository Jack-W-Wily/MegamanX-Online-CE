using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;

/*
in Order for this to work as intended I added this to Damager.cs's applydamage section

			if (character.sprite.name.Contains("block") && damage > 0 && !isArmorPiercing(projId)) {
				if (!hitFromBehind(character, damagingActor, owner, projId)) {
					damage--;
					flinch = 0;
					if (damage < 3) {
						damage = 0;
						character.playSound("m10ding");
					}
				}
			}
*/


public class BlockWCUT : CharState {


	public const float maxBlockTime = 4;

	public float blockTime;


	public BlockWCUT() : base("block") {
		exitOnAirborne = true;
		attackCtrl = true;
		normalCtrl = true;
		stunResistant = true;
		immuneToWind = true;
		blockTime = maxBlockTime;
	}

	public override void update() {
		base.update();
		blockTime -= Global.spf;
		bool isHoldingGuard = (
			player.input.isHeld(Control.L2, player)
		);
		if (blockTime == 0) {
			character.changeState(new BlockBreak(character.xDir), true);
		}
		if (!isHoldingGuard) {
			character.changeToIdleOrFall();
			return;
		}
		if (Global.level.gameMode.isOver) {
			if (Global.level.gameMode.playerWon(player)) {
				if (!character.sprite.name.Contains("_win")) {
					character.changeSpriteFromName("win", true);
				}
			} else {
				if (!character.sprite.name.Contains("lose")) {
					character.changeSpriteFromName("lose", true);
				}
			}
		}
	}
}



public class BlockBreak : CharState {
	public int hurtDir;
	public float hurtSpeed;

	public BlockBreak(int dir) : base("land") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}
		/*
		if (this.character.isAnimOver()) {
			this.character.changeToIdleOrFall();
		}
		*/
		if (hurtSpeed == 0) {
			character.changeToIdleOrFall();
		}
	}
}

public class Clang : CharState {
	public int hurtDir;
	public float hurtSpeed;

	public Clang(int dir) : base("clang") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}
		/*
		if (this.character.isAnimOver()) {
			this.character.changeToIdleOrFall();
		}
		*/
		if (hurtSpeed == 0) {
			character.changeToIdleOrFall();
		}
	}
}




public class HurtByEnemy : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public HurtByEnemy(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
		flinchTime = 0.5f;
		enterSound = "hurt";
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -100;
		character.invulnTime = 0.3f;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (player.character.canCharge() && player.input.isHeld(Control.Shoot, player)) {
			player.character.increaseCharge();
		}

		if (stateTime >= flinchTime) {
			character.changeToIdleOrFall();
			character.invulnTime = 0.2f;
		}
	}
}




public class PushedOver : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public PushedOver(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 300;
		flinchTime = 0.5f;
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -300;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (player.character.canCharge() && player.input.isHeld(Control.Shoot, player)) {
			player.character.increaseCharge();
		}

		if (stateTime >= flinchTime) {
			character.changeState(new KnockedDown(-character.xDir), true);
		}
	}
}



public class PushedOver2 : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public PushedOver2(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 300;
		flinchTime = 0.5f;
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -300;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (player.character.canCharge() && player.input.isHeld(Control.Shoot, player)) {
			player.character.increaseCharge();
		}

		if (stateTime >= flinchTime) {
			character.changeState(new Idle(), true);
		}
	}
}




public class LaunchedState : GenericGrabbedState {
	public Character grabbedChar;
	//private bool once;
	public bool launched;
	float launchTime;
	bool once;
	public LaunchedState(Character grabber) : base(grabber, 1, "") {
		customUpdate = true;
		superArmor = true;
	}


	public override void update() {
		base.update();

		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				character.changeToIdleOrFall();
				return;
			}

			for (int i = 1; i <= 4; i++) {
				CollideData collideData = Global.level.checkTerrainCollisionOnce(character, 0, -10 * i, autoVel: true);
				if (!character.grounded && collideData != null && collideData.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
					if (!once) {
						once = true;
						character.applyDamage(2, player, character, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
						character.playSound("crash", sendRpc: true);
						character.shakeCamera(sendRpc: true);
					}
				}
			}

		}

		if (!launched) {
			launched = true;
			character.unstickFromGround();
			character.vel.y = -600;
		}
	}
}



public class LaunchedFowardState : CharState {


	public LaunchedFowardState() : base("hurt") {
		superArmor = false;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

	
		character.move(new Point(character.xDir * -350, 0));

		CollideData? collideData = Global.level.checkTerrainCollisionOnce(character, -character.xDir, 0);
		if (collideData != null && collideData.isSideWallHit() && character.ownedByLocalPlayer ||
		(character.vel.x == 0 || character.grounded) && stateTime > 0.2f ) {
			character.applyDamage(2, player, character, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
			character.changeToIdleOrFall();
			character.playSound("hurt", sendRpc: true);
			character.shakeCamera(sendRpc: true);
			return;
		} else if (stateTime > 3f) {
			character.changeToIdleOrFall();
			character.shakeCamera(sendRpc: true);
			return;
		}

	
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = true;
		character.vel.y = -character.getJumpPower() * 0.75f;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}


