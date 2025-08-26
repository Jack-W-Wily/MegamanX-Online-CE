using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;






public class ZainKokuStab : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	bool once;

	bool once1;
	public ZainKokuStab(
		bool grounded, bool shootProj
	) : base(
		grounded ? "projswing" : "projswing_air", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "slash";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
		useDashJumpSpeed = true;
		bonusAttackCtrl = true;
	}


		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

		 if (player.input.isHeld(Control.Down, player)
		&& character.grounded){
		    character.changeSpriteFromName("thrust", true);
		}

	}

	public override void update() {
		base.update();

		

		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_1", forcePlay: false, sendRpc: true);
			if (shootProj) {
				new ZainSaberProj(
					new ZSaber(), character.pos.addxy(30 * character.xDir, -20),
					character.xDir, player, player.getNextActorNetId(), rpc: true
				);
			}
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "projswing_air";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}







public class ZainKokuRising : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	bool once;

	bool once1;
	public ZainKokuRising(
		bool grounded, bool shootProj
	) : base(
		grounded ? "projswing" : "projswing_air", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "slash";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
		useDashJumpSpeed = true;
		bonusAttackCtrl = true;
	}


		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		   character.changeSpriteFromName("rising", true);
			character.dashedInAir++;
			float ySpeedMod = 1.5f;
			character.vel.y = (0f - character.getJumpPower()) * ySpeedMod;
	}

	public override void update() {
		base.update();

		

		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_1", forcePlay: false, sendRpc: true);
			if (shootProj) {
				new ZainSaberProj(
					new ZSaber(), character.pos.addxy(30 * character.xDir, -20),
					character.xDir, player, player.getNextActorNetId(), rpc: true
				);
			}
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "projswing_air";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}


public class ZainBossJumpStart : CharState {
	public ZainBossJumpStart() : base("jump") {
		enterSound = "vileJump";
	}

	public override void update() {
		base.update();

		if (character.isAnimOver()) {
			character.vel.y = -character.getJumpPower() * 0.75f;
			character.changeState(new ZainBossJump(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class ZainBossJump : CharState {

	bool dropDown;
	public ZainBossJump() : base("projswing_air") {
		enterSound = "swordswipeGG";
		superArmor = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();

		if (character.grounded && stateTime > 0.05f) {
			character.changeState(new ZainBossJumpLand(), true);
		}

		if (Global.level.checkTerrainCollisionOnce(character, 0, -1) != null && character.vel.y < 0) {
			character.vel.y = 0;
		}
		if (!player.isAI && !character.player.input.isAHeld(player) && !dropDown) {
			dropDown = true;
			character.vel.y = 0;
			character.frameIndex = 8;
		}
		if (player.isAI && stateTime == Helpers.randomRange(0,2) && !dropDown) {
			dropDown = true;
			character.vel.y = 0;
			character.frameIndex = 8;
		}
		character.move(new Point(character.xDir * 300, 0));
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}




public class ZainBossJumpLand : CharState {


	public ZainBossJumpLand() : base("projswing") {
		enterSound = "crash";
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.shakeCamera(sendRpc: true);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}


