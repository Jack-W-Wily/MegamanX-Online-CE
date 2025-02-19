using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;

public class BusterZeroMelee : CharState {
	bool fired;
	public BusterZero zero = null!;

	public BusterZeroMelee() : base("projswing") {
		landSprite = "projswing";
		airSprite = "projswing_air";
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("zerosaberx3", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as BusterZero ?? throw new NullReferenceException();
		if (!character.grounded || character.vel.y < 0) {
			sprite = "projswing_air";
			character.changeSpriteFromName(sprite, true);
		}
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		zero.zSaberCooldown = 36;
	}
}


public class BusterZeroMeleeWall : CharState {
	bool fired;
	public BusterZero zero = null!;
	public int wallDir;
	public Collider wallCollider;

	public BusterZeroMeleeWall(int wallDir, Collider wallCollider) : base("wall_slide_attack") {
		this.wallDir = wallDir;
		this.wallCollider = wallCollider;
		useGravity = false;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("zerosaberx3", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeState(new WallSlide(wallDir, wallCollider) { enterSound = "" });
			character.sprite.frameIndex = character.sprite.totalFrameNum - 1;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as BusterZero ?? throw new NullReferenceException();
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		useGravity = true;
		zero.zSaberCooldown = 36;
	}
}

public class BusterZeroDoubleBuster : CharState {
	public bool fired1;
	public bool fired2;
	public bool isSecond;
	public bool isPinkCharge;
	public bool shootPressedAgain;
	public int startStockLevel;
	BusterZero zero = null!;

	public BusterZeroDoubleBuster(bool isSecond, int startstockLevel) : base("doublebuster") {
		this.isSecond = isSecond;
		this.startStockLevel = startstockLevel;
		useDashJumpSpeed = true;
		airMove = true;
		superArmor = false;
		canStopJump = true;
		canJump = true;
		landSprite = "doublebuster";
		airSprite = "doublebuster_air";
	}

	public override void update() {
		base.update();
		if (player.input.isPressed(Control.Shoot, player)) {
			shootPressedAgain = true;
		}
		if (!fired1 && character.frameIndex == 3) {
			fired1 = true;
			character.playSound("buster3X3", sendRpc: true);
			new DZBuster3Proj(
				character.getShootPos(), character.getShootXDir(),
				zero, player, player.getNextActorNetId(), rpc: true
			);
			zero.stockedTime = 0;
		}
		if (!fired2 && character.frameIndex == 7) {
			fired2 = true;
			if (!isPinkCharge) {
				zero.stockedBusterLv = 0;
				character.playSound("buster3X3", sendRpc: true);
				new DZBuster3Proj(
					character.getShootPos(), character.getShootXDir(),
					zero, player, player.getNextActorNetId(), rpc: true
				);
			} else {
				zero.stockedBusterLv = 0;
				character.playSound("buster2X3", sendRpc: true);
				new DZBuster2Proj(
					character.getShootPos(), character.getShootXDir(),
					zero, player, player.getNextActorNetId(), rpc: true
				);
			}
			zero.stockedTime = 0;
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else if (!isSecond && character.frameIndex >= 4 && !shootPressedAgain) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as BusterZero ?? throw new NullReferenceException();
		// For the starting buster;
		if (startStockLevel is 1 or 3) {
			isPinkCharge = true;
		}
		// Non-full charge.
		if (isPinkCharge) {
			zero.stockedBusterLv = 1;
			isPinkCharge = true;
		}
		// Full charge.
		else {
			// We add Z-Saber charge if we fire the full charge and we were at 0 charge before.
			if (startStockLevel == 4 || !isSecond) {
				zero.stockedSaber = true;
			}
			zero.stockedBusterLv = 2;
		}
		if (!character.grounded || character.vel.y < 0) {
			sprite = "doublebuster_air";
			character.changeSpriteFromName(sprite, true);
		}
		// For halfway shot.
		if (startStockLevel <= 2) {
			character.frameIndex = 4;
			fired1 = true;
		}
	}

	public override void onExit(CharState newState) {
		zero.stockedTime = 0;
		base.onExit(newState);
		// We check if we fired the second shot. If not we add the stocked charge.
		if (!fired2) {
			if (isPinkCharge) {
				zero.stockedBusterLv = 1;
			} else {
				zero.stockedBusterLv = 2;
				zero.stockedSaber = true;
			}
		}
		if (!fired1) {
			if (isPinkCharge) {
				zero.stockedBusterLv = 3;
			} else {
				zero.stockedBusterLv = 4;
				zero.stockedSaber = true;
			}
		}
	}
}

public class BusterZeroHadangeki : CharState {
	bool fired;
	public BusterZero zero = null!;

	public BusterZeroHadangeki() : base("projswing") {
		landSprite = "projswing";
		airSprite = "projswing_air";
		airMove = true;
		useDashJumpSpeed = true;
		superArmor = false;
		canStopJump = true;
		canJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 6 && !fired) {
			character.playSound("zerosaberx3", sendRpc: true);
			zero.stockedSaber = false;
			fired = true;
			new DZHadangekiProj(
				character.pos.addxy(30 * character.xDir, -20), character.xDir,
				zero.isBlackZero, zero, player, player.getNextActorNetId(), rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as BusterZero ?? throw new NullReferenceException();
		if (!character.grounded || character.vel.y < 0) {
			sprite = "projswing_air";
			defaultSprite = sprite;
			character.changeSpriteFromName(sprite, true);
		}
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		zero.zSaberCooldown = 36;
		zero.stockedTime = 0;
	}
}

public class BusterZeroHadangekiWall : CharState {
	bool fired;
	public BusterZero zero = null!;
	public int wallDir;
	public Collider wallCollider;

	public BusterZeroHadangekiWall(int wallDir, Collider wallCollider) : base("wall_slide_attack") {
		this.wallDir = wallDir;
		this.wallCollider = wallCollider;
		useDashJumpSpeed = true;
		superArmor = true;
		useGravity = false;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 4 && !fired) {
			character.playSound("zerosaberx3", sendRpc: true);
			zero.stockedSaber = false;
			fired = true;
			new DZHadangekiProj(
				character.pos.addxy(30 * -wallDir, -20), -wallDir,
				zero.isBlackZero, zero, player, player.getNextActorNetId(), rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeState(new WallSlide(wallDir, wallCollider) { enterSound = "" });
			character.sprite.frameIndex = character.sprite.totalFrameNum - 1;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as BusterZero ?? throw new NullReferenceException();
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		useGravity = true;
		zero.zSaberCooldown = 36;
	}
}

public class HyperBusterZeroStart : CharState {
	public float radius = 200;
	public float time;
	BusterZero zero = null!;
	Anim? LightX3;

	public HyperBusterZeroStart() : base("hyper_start") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (time == 0) {
			if (radius >= 0) {
				radius -= Global.spf * 200;
			} else {
				time = Global.spf;
				radius = 0;
				zero.isBlackZero = true;
				character.playSound("ching");
				character.fillHealthToMax();
			}
		} else {
			time += Global.spf;
			if (time >= 1) {
				character.changeToLandingOrFall();
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as BusterZero ?? throw new NullReferenceException();
		character.useGravity = false;
		character.vel = new Point();
		if (zero == null) {
			throw new NullReferenceException();
		}
		LightX3 = new Anim(
				character.pos.addxy(50 * character.xDir, 0f),
				"LightX3", -character.xDir,
				player.getNextActorNetId(),
				destroyOnEnd: false, sendRpc: true
			);
		LightX3.fadeIn = true;
		character.player.currency -= Player.zBusterZeroHyperCost;
		character.playSound("blackzeroentry", forcePlay: false, sendRpc: true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		LightX3?.destroySelf();
		character.useGravity = true;
		if (character != null) {
			character.invulnTime = 0.5f;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		Point pos = character.getCenterPos();
		DrawWrappers.DrawCircle(
			pos.x + x, pos.y + y, radius, false, Color.White, 5, character.zIndex + 1, true, Color.White
		);
	}
}
