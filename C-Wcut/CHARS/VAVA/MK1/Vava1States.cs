using System;
using SFML.Graphics;

namespace MMXOnline;





public class VAVAJab1 : CharState {


	public VAVAJab1() : base("jab_1") {
		wiffCancel = true;
		enterSound = "punch2";
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		if (character.frameIndex > 3) {
			if (player.input.isPressed(Control.Shoot, player)) {
				character.changeState(new VAVAJab2(), true);
		}
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}


public class VAVAJab2 : CharState {


	public VAVAJab2() : base("jab_2") {
		wiffCancel = true;
		enterSound = "punch2";
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}






public class VAVAKamae : CharState {


	public VAVAKamae() : base("kamae") {

	}

	public override void update() {
		base.update();

		if (character.frameIndex > 0) {
			if (player.input.isPressed(Control.Down, player)) {
				character.changeToIdleOrFall();
			}


			if (player.input.isPressed(Control.Shoot, player)) {
				character.changeState(new VKamaeUnblockableStart(), true);
			}

			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new VKamaeHotIcecle(), true);
			}

			if (player.input.isPressed(Control.R2, player)) {
				character.changeState(new VKote(), true);
			}

			if (character.xDir == 1) {
				if (player.input.isPressed(Control.Left, player) && player.input.checkDoubleTap(Control.Left)) {
					character.changeState(new VKamaeBDash(), true);
				}
				if (player.input.isPressed(Control.Right, player) && player.input.checkDoubleTap(Control.Right)) {
					character.changeState(new VKamaeDash(), true);
				}

			}
			if (character.xDir == -1) {
				if (player.input.isPressed(Control.Left, player) && player.input.checkDoubleTap(Control.Left)) {
					character.changeState(new VKamaeDash(), true);
				}
				if (player.input.isPressed(Control.Right, player) && player.input.checkDoubleTap(Control.Right)) {
				character.changeState(new VKamaeBDash(), true);
				}
				
			}
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}





public class VKamaeUnblockableStart : CharState {
	public VKamaeUnblockableStart() : base("kamae_unblockable_start") {
		enterSound = "vileJump";
	}

	public override void update() {
		base.update();

		if (character.isAnimOver()) {
			character.vel.y = -character.getJumpPower() * 0.75f;
			character.changeState(new VUnblockable(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class VUnblockable : CharState {
	public VUnblockable() : base("kamae_unblockable") {
		enterSound = "swordswipeGG";
	}

	public override void update() {
		base.update();

		if (character.grounded && stateTime > 0.05f) {
			character.changeState(new VUnblockableLand(), true);
		}

		if (Global.level.checkTerrainCollisionOnce(character, 0, -1) != null && character.vel.y < 0) {
			character.vel.y = 0;
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




public class VKamaeHotIcecle : CharState {


	public VKamaeHotIcecle() : base("hoticecle") {
		wiffCancel = true;
		enterSound = "genocideCutter1";
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}



public class VUnblockableLand : CharState {


	public VUnblockableLand() : base("kamae_unblockable_land") {
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




public class VKamaeDash : CharState {


	public VKamaeDash() : base("kamae_dash") {
		immuneToWind = true;
		enterSound = "GDash";
	}

	public override void update() {
		base.update();

		character.move(new Point(character.xDir * 350, 0));

		if (player.input.isPressed(Control.Shoot, player)) {
				character.changeState(new VKamaeUnblockableStart(), true);
			}

			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new VKamaeHotIcecle(), true);
			}

			if (player.input.isPressed(Control.R2, player)) {
				character.changeState(new VKote(), true);
			}

		if (stateTime > 0.2f) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.slideVel = character.xDir * character.getDashSpeed() * 0.9f;
	}
}


public class VKamaeBDash : CharState {


	public VKamaeBDash() : base("kamae_backdash") {
		immuneToWind = true;
		enterSound = "GDash";
	}

	public override void update() {
		base.update();

			character.move(new Point(character.xDir * -350, 0));

		if (player.input.isPressed(Control.Shoot, player)) {
				character.changeState(new VKamaeUnblockableStart(), true);
			}

			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new VKamaeHotIcecle(), true);
			}

			if (player.input.isPressed(Control.R2, player)) {
				character.changeState(new VKote(), true);
			}

		if (stateTime > 0.2f) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.slideVel = character.xDir * character.getDashSpeed() * 0.9f;
	}
}



public class VKote : CharState {


	public VKote() : base("kamae_kote") {
		superArmor = true;
		immuneToWind = true;
		enterSound = "distortion_d";
	}

	public override void update() {
		base.update();

		if (character.frameIndex < 3) {
			character.move(new Point(character.xDir * 350, 0));
		}



		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel.y = 0;

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;

	}
}



public class CallDownMechWC : CharState {
	VAVA1 vile = null!;
	RideArmor rideArmor;
	bool isNew;

	public CallDownMechWC(RideArmor rideArmor, bool isNew, string transitionSprite = "") : base("call_down_mech", "", "", transitionSprite) {
		this.rideArmor = rideArmor;
		this.isNew = isNew;
		superArmor = true;
	}

	public override void update() {
		base.update();
		if (rideArmor == null || rideArmor.destroyed || stateTime > 4) {
			character.changeToIdleOrFall();
			return;
		}

		if (rideArmor.rideArmorState is not RACalldown) {
			/*
			if (character.isVileMK5)
			{
				if (stateTime > 0.75f)
				{
					character.changeToIdleOrFall();
				}
				return;
			}
			*/

			if (vile.isVileMK5 != true && MathF.Abs(character.pos.x - rideArmor.pos.x) < 10) {
				rideArmor.putCharInRideArmor(character);
			} else {
				character.changeToIdleOrFall();
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		rideArmor.changeState(new RACalldown(character.pos, isNew), true);
		rideArmor.xDir = character.xDir;
		vile = character as VAVA1 ?? throw new NullReferenceException();
	}
}

public class VileReviveWC : CharState {
	public float radius = 200;
	Anim? drDopplerAnim;
	bool isMK5;
	public VAVA1 vile = null!;

	public VileReviveWC(bool isMK5) : base(isMK5 ? "revive_to5" : "revive") {
		invincible = true;
		this.isMK5 = isMK5;
	}

	public override void update() {
		base.update();
		if (radius >= 0) {
			radius -= Global.spf * 150;
		}
		if (character.frameIndex < 2) {
			if (Global.frameCount % 4 < 2) {
				character.addRenderEffect(RenderEffectType.Flash);
			} else {
				character.removeRenderEffect(RenderEffectType.Flash);
			}
		} else {
			character.removeRenderEffect(RenderEffectType.Flash);
		}
		if (character.frameIndex == 7 && !once) {
			character.playSound("ching");
			player.health = 1;
			character.addHealth(player.maxHealth);
			once = true;
		}
		if (character.ownedByLocalPlayer) {
			if (character.isAnimOver()) {
				setFlags();
				character.changeState(character.getFallState(), true);
			}
		} else if (character?.sprite?.name != null) {
			if (!character.sprite.name.EndsWith("_revive") && !character.sprite.name.EndsWith("_revive_to5") && radius <= 0) {
				setFlags();
				character.changeState(character.getFallState(), true);
			}
		}
	}

	public void setFlags() {
		if (!isMK5) {
			vile.vileForm = 1;
		} else {
			vile.vileForm = 2;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as VAVA1 ?? throw new NullReferenceException();
		//character.setzIndex(ZIndex.Foreground);
		character.playSound("revive");
		character.addMusicSource("demo_X3", character.getCenterPos(), false, loop: false);
		if (!isMK5) {
			drDopplerAnim = new Anim(character.pos.addxy(30 * character.xDir, -15), "drdoppler", -character.xDir, null, false);
			drDopplerAnim.blink = true;
		} else {
			drDopplerAnim = new Anim(character.pos.addxy(30 * character.xDir, -15), "vilemk5_lumine", character.xDir, null, false);
			drDopplerAnim.blink = true;
			if (vile.linkedRideArmor != null) {
				vile.linkedRideArmor.ownedByMK5 = true;
			}
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		setFlags();
		character.removeRenderEffect(RenderEffectType.Flash);
		Global.level.delayedActions.Add(new DelayedAction(() => { character.destroyMusicSource(); }, 0.75f));

		drDopplerAnim?.destroySelf();
		if (character != null) {
			character.invulnTime = 0.5f;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		if (!character.ownedByLocalPlayer) return;

		if (radius <= 0) return;
		Point pos = character.getCenterPos();
		DrawWrappers.DrawCircle(pos.x + x, pos.y + y, radius, false, Color.White, 5, character.zIndex + 1, true, Color.White);
	}
}

public class VileHoverWC : CharState {
	public SoundWrapper? soundh;
	public Point flyVel;
	float flyVelAcc = 500;
	float flyVelMaxSpeed = 200;
	public float fallY;
	Vile vile = null!;

	public VileHoverWC(string transitionSprite = "") : base("hover", "hover_shoot", transitionSprite) {
		exitOnLanding = true;
		attackCtrl = true;
		normalCtrl = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (character.flag != null) {
			character.changeToIdleOrFall();
			return;
		}

		if (vile.vileHoverTime > vile.vileMaxHoverTime) {
			vile.vileHoverTime = vile.vileMaxHoverTime;
			character.changeToIdleOrFall();
			return;
		}

		if (character.charState is not VileHoverWC) return;

		if (Global.level.checkTerrainCollisionOnce(character, 0, -character.getYMod()) != null && character.vel.y * character.getYMod() < 0) {
			character.vel.y = 0;
		}

		Point move = getHoverMove();

		if (!character.sprite.name.EndsWith("cannon_air") || character.sprite.time > 0.1f) {
			if (MathF.Abs(move.x) > 75 && !character.isUnderwater()) {
				sprite = "hover_forward";
				character.changeSpriteFromNameIfDifferent("hover_forward", false);
			} else {
				sprite = "hover";
				character.changeSpriteFromNameIfDifferent("hover", false);
			}
		}

		if (move.magnitude > 0) {
			character.move(move);
		}

		if (character.isUnderwater()) {
			character.frameIndex = 0;
			character.frameSpeed = 0;
		}
		if (base.player.input.isHeld("jump", base.player) && !once) {
			once = true;
			soundh = character.playSound("vileHover", forcePlay: false, sendRpc: false);
		}
	}

	public Point getHoverMove() {
		bool isSoftLocked = character.isSoftLocked();
		bool isJumpHeld = !isSoftLocked && player.input.isHeld(Control.Jump, player) && character.pos.y > -5;

		var inputDir = isSoftLocked ? Point.zero : player.input.getInputDir(player);
		inputDir.y = isJumpHeld ? -1 : 0;

		if (inputDir.x > 0) character.xDir = 1;
		if (inputDir.x < 0) character.xDir = -1;

		if (inputDir.y == 0 || character.gravityWellModifier > 1) {
			if (character.frameIndex >= character.sprite.loopStartFrame) {
				character.frameIndex = character.sprite.loopStartFrame;
				character.frameSpeed = 0;
			}
			character.addGravity(ref fallY);
		} else {
			character.frameSpeed = 1;
			fallY = Helpers.lerp(fallY, 0, Global.spf * 10);
			vile.vileHoverTime += Global.spf;
		}

		if (inputDir.isZero()) {
			flyVel = Point.lerp(flyVel, Point.zero, Global.spf * 5f);
		} else {
			float ang = flyVel.angleWith(inputDir);
			float modifier = Math.Clamp(ang / 90f, 1, 2);

			flyVel.inc(inputDir.times(Global.spf * flyVelAcc * modifier));
			if (flyVel.magnitude > flyVelMaxSpeed) {
				flyVel = flyVel.normalize().times(flyVelMaxSpeed);
			}
		}

		var hit = character.checkCollision(flyVel.x * Global.spf, flyVel.y * Global.spf);
		if (hit != null && !hit.isGroundHit()) {
			flyVel = flyVel.subtract(flyVel.project(hit.getNormalSafe()));
		}

		return flyVel.addxy(0, fallY);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		character.useGravity = false;
		if (vile.hasSpeedDevil) {
			flyVelMaxSpeed *= 1.1f;
			flyVelAcc *= 1.1f;
		}

		float flyVelX = 0;
		if (character.deltaPos.x != 0) {
			flyVelX = character.xDir * character.getDashOrRunSpeed() * 0.5f;
		}

		float flyVelY = 0;
		if (character.vel.y < 0) {
			flyVelY = character.vel.y;
		}

		flyVel = new Point(flyVelX, flyVelY);
		if (flyVel.magnitude > flyVelMaxSpeed) flyVel = flyVel.normalize().times(flyVelMaxSpeed);

		if (character.vel.y > 0) {
			fallY = character.vel.y;
		}

		character.isDashing = false;
		character.stopMoving();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.sprite.restart();
		character.stopMoving();
		if (soundh != null && !soundh.deleted) {
			soundh.sound?.Stop();
			soundh = null;
		}
		RPC.stopSound.sendRpc("vileHover", character.netId);

	}
}







