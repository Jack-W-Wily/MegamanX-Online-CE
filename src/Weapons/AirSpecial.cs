﻿using System;

namespace MMXOnline;

public enum AirSpecialType {
	Kuuenzan,
	FSplasher,
	Hyoroga
}

public class KuuenzanWeapon : Weapon {
	public KuuenzanWeapon(Player player) : base() {
		damager = new Damager(player, 3, 0, 0.5f);
		index = (int)WeaponIds.Kuuenzan;
		weaponBarBaseIndex = 21;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 48;
		killFeedIndex = 121;
		type = (int)AirSpecialType.Kuuenzan;
		displayName = "Kuuenzan";
		description = new string[] { "Standard spin attack in the air." };
	}

	public static Weapon getWeaponFromIndex(Player player, int index) {
		if (index == (int)AirSpecialType.Kuuenzan) return new KuuenzanWeapon(player);
		else if (index == (int)AirSpecialType.FSplasher) return new FSplasherWeapon(player);
		else if (index == (int)AirSpecialType.Hyoroga) return new HyorogaWeapon(player);
		else throw new Exception("Invalid Zero air special weapon index!");
	}
}

public class FSplasherWeapon : Weapon {
	public FSplasherWeapon(Player player) : base() {
		damager = new Damager(player, 3, Global.defFlinch, 0.06f);
		index = (int)WeaponIds.FSplasher;
		killFeedIndex = 109;
		type = (int)AirSpecialType.FSplasher;
		displayName = "Hisuishou";
		description = new string[] { "An aerial dash attack forward.", "Also provides a good speed boost." };
	}

	public override void attack(Character character) {
		if (character.dashedInAir > 0) return;
		if (shootTime > 0) return;
		shootTime = 1;
		character.changeState(new FSplasherState(), true);
	}
}

public class FSplasherState : CharState {
	public float dashTime = 0;
	public Projectile fSplasherProj;
	Zero zero;

	public FSplasherState() : base("dash", "") {
		enterSound = "fsplasher";
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as Zero;

		character.isDashing = true;
		character.useGravity = false;
		character.vel = new Point(0, 0);
		character.dashedInAir++;
		fSplasherProj = new FSplasherProj(
			zero.zeroAirSpecialWeapon, character.pos, character.xDir,
			player, player.getNextActorNetId(), sendRpc: true
		);
	}

	public override void onExit(CharState newState) {
		character.useGravity = true;
		if (fSplasherProj != null) {
			fSplasherProj.destroySelf();
			fSplasherProj = null;
		}
		zero.zeroAirSpecialWeapon.shootTime = 1;
		base.onExit(newState);
	}

	public override bool canEnter(Character character) {
		if (!base.canEnter(character)) return false;
		return character.flag == null;
	}

	public override void update() {
		base.update();

		float upSpeed = 0;
		var inputDir = player.input.getInputDir(player);
		if (inputDir.y < 0) upSpeed = -1;
		else if (inputDir.y > 0) upSpeed = 1;
		else upSpeed = 0;

		if (fSplasherProj != null) {
			fSplasherProj.incPos(character.deltaPos);
		}

		CollideData collideData = Global.level.checkCollisionActor(character, character.xDir, upSpeed);
		if (collideData != null) {
			character.changeState(new Fall(), true);
			return;
		}

		float modifier = 1f;
		dashTime += Global.spf;
		if (dashTime > 0.6) {
			character.changeState(new Fall());
			return;
		}

		var move = new Point(0, 0);
		move.x = character.getDashSpeed() * character.xDir * modifier;
		move.y = upSpeed * 100;
		character.move(move);
		if (stateTime > 0.1) {
			stateTime = 0;
		}
	}
}

public class FSplasherProj : Projectile {
	public FSplasherProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, xDir, 0, 2, player, "fsplasher_proj", 0, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.FSplasher;
		destroyOnHit = false;
		shouldVortexSuck = false;
		shouldShieldBlock = false;
		canBeLocal = false;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}
}

public class HyorogaWeapon : Weapon {
	public HyorogaWeapon(Player player) : base() {
		damager = new Damager(player, 3, Global.defFlinch, 0.06f);
		index = (int)WeaponIds.Hyoroga;
		killFeedIndex = 108;
		type = (int)AirSpecialType.Hyoroga;
		displayName = "Hyoroga";
		description = new string[] { "Cling to ceilings and rain down icicles with ATTACK." };
	}

	public override void attack(Character character) {
		//if (character.charState is Fall) return;
		for (int i = 1; i <= 4; i++) {
			CollideData collideData = Global.level.checkCollisionActor(character, 0, -10 * i, autoVel: true);
			if (collideData != null && collideData.gameObject is Wall wall && !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
				character.changeState(new HyorogaStartState(), true);
				return;
			}
		}
	}
}

public class HyorogaStartState : CharState {
	public HyorogaStartState() : base("hyoroga_rise") {
	}

	public override void update() {
		base.update();

		if (player.input.isPressed(Control.Down, player)) character.changeToIdleOrFall();
		if (character.sprite.name.Contains("hyoroga_rise")) {
			if (character.deltaPos.isCloseToZero()) {
				sprite =  "hyoroga_start";
				character.changeSpriteFromName("hyoroga_start", true);
				character.gravityModifier = -1;
				character.useGravity = true;
			}
		} else if (character.sprite.name.Contains("hyoroga_start")) {
			if (character.isAnimOver()) {
				character.changeState(new HyorogaState(), true);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel = new Point(0, 0);
		character.gravityModifier = -1;
		character.dashedInAir = 0;
		if (character is Zero zero) {
			zero.quakeBlazerBounces = 0;
		}
		character.specialState = (int)SpecialStateIds.HyorogaStart;
	}

	public override void onExit(CharState newState) {
		character.useGravity = true;
		character.gravityModifier = 1;
		base.onExit(newState);
		character.specialState = (int)SpecialStateIds.None;
	}
}

public class HyorogaState : CharState {
	float shootCooldown;
	Zero zero;

	public HyorogaState() : base("hyoroga", "hyoroga_shoot", "hyoroga_attack") {
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel = new Point(0, 0);
		character.useGravity = false;
		character.gravityModifier = 0;

		zero = character as Zero;
	}

	public override void onExit(CharState newState) {
		character.useGravity = true;
		character.gravityModifier = 1;
		base.onExit(newState);
	}

	public override void update() {
		base.update();
		Helpers.decrementTime(ref shootCooldown);

		var pois = character.sprite.getCurrentFrame().POIs;
		if (character.sprite.name.Contains("hyoroga_attack")) {
			if (pois != null && pois.Count > 0 && shootCooldown == 0) {
				var poi = character.getFirstPOIOrDefault();
				new HyorogaProj(
					zero.zeroAirSpecialWeapon, poi, new Point(0, 1),
					player, player.getNextActorNetId(), sendRpc: true
				);
				new HyorogaProj(
					zero.zeroAirSpecialWeapon, poi, new Point(0.375f, 1),
					player, player.getNextActorNetId(), sendRpc: true
				);
				new HyorogaProj(
					zero.zeroAirSpecialWeapon, poi, new Point(-0.375f, 1),
					player, player.getNextActorNetId(), sendRpc: true
				);
				//shootCooldown = 1f;
			}
		} else {
			character.turnToInput(player.input, player);
		}

		if (player.input.isPressed(Control.Jump, player)) {
			character.changeState(new Fall(), true);
		}
	}
}

public class HyorogaProj : Projectile {
	public HyorogaProj(Weapon weapon, Point pos, Point vel, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 3, player, "hyoroga_proj", Global.halfFlinch, 0.15f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.HyorogaProj;
		destroyOnHit = true;
		this.vel = vel.times(250);
		maxTime = 0.4f;
		canBeLocal = false; // TODO: Implement local.

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		destroySelf();
	}

	public override void onDestroy() {
		base.onDestroy();
		playSound("iceBreak");
		Anim.createGibEffect("hyoroga_proj_pieces", getCenterPos(), owner);
	}
}
