﻿using System;
using System.Collections.Generic;
namespace MMXOnline;

public class FlameMammoth : Maverick {
	public FlameMStompWeapon stompWeapon = new();

	public FlameMammoth(
		Player player, Point pos, Point destPos, int xDir, ushort? netId, bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		player, pos, destPos, xDir, netId, ownedByLocalPlayer
	) {
		stateCooldowns.Add(typeof(MShoot), new MaverickStateCooldown(false, true, 0.5f));
		stateCooldowns.Add(typeof(FlameMOilState), new MaverickStateCooldown(false, true, 0.5f));

		awardWeaponId = WeaponIds.FireWave;
		weakWeaponId = WeaponIds.StormTornado;
		weakMaverickWeaponId = WeaponIds.StormEagle;

		weapon = new Weapon(WeaponIds.FlameMGeneric, 100);

		netActorCreateId = NetActorCreateId.FlameMammoth;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		armorClass = ArmorClass.Heavy;
		canStomp = true;
	}

	public override void update() {
		base.update();
		if (aiBehavior == MaverickAIBehavior.Control) {
			if (state is MIdle or MRun or MLand) {
				if (shootPressed()) {
					changeState(getShootState(false));
				} else if (specialPressed()) {
					changeState(new FlameMOilState());
				} else if (input.isPressed(Control.Dash, player)) {
					changeState(new MammothGrabState());
				}
				
				
				
			} else if (state is MJump || state is MFall) {
				if (input.isPressed(Control.Dash, player)) {
					changeState(new FlameMJumpPressState());
				}
			}
		}  else {
			if ((state is MIdle or MRun or MLand or BoomerKDashState)) {
				foreach (var enemyPlayer in Global.level.players) {
					if (enemyPlayer.character == null || enemyPlayer == player) continue;
					var chr = enemyPlayer.character;
					if (!chr.canBeDamaged(player.alliance, player.id, null)) return;
					if (isFacing(chr) && getCenterPos().distanceTo(chr.getCenterPos()) < 10) {
					getRandomAttackState();
					}
				}
			}
		}
	
	}

	public override string getMaverickPrefix() {
		return "flamem";
	}

	public MaverickState getShootState(bool isAI) {
		var shootState = new MShoot((Point pos, int xDir) => {
			playSound("flamemShoot", sendRpc: true);
			new FlameMFireballProj(
				new FlameMFireballWeapon(), pos, xDir,
				player.input.isHeld(Control.Down, player), player, player.getNextActorNetId(), rpc: true
			);
		}, null);
		if (isAI) {
			shootState.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.5f);
		}
		return shootState;
	}

	public override MaverickState[] aiAttackStates() {
		return new MaverickState[]
		{
				getShootState(true),
				new FlameMOilState(),
				new MJumpStart(),
		};
	}

	public override MaverickState getRandomAttackState() {
		var attacks = new MaverickState[] {
				getShootState(true),
				new FlameMOilState(),
				new MJumpStart(),
		};
		return attacks.GetRandomItem();
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Fall,
		Grab,
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"flamem_fall" => MeleeIds.Fall,
			"flamem_grab" => MeleeIds.Grab,
			_ => MeleeIds.None
		});
		
	}

	// This can be called from a RPC, so make sure there is no character conditionals here.
	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Fall => new GenericMeleeProj(
				stompWeapon, pos, ProjIds.FlameMStomp, player,
				6, Global.defFlinch, addToLevel: addToLevel
			),
				MeleeIds.Grab => new GenericMeleeProj(
				stompWeapon, pos, ProjIds.FlameMSlam, player,
				0, 0, addToLevel: addToLevel
			),
			_ => null
		};
	}

	public override void updateProjFromHitbox(Projectile proj) {
		if (proj.projId == (int)ProjIds.FlameMStomp) {
			float damage = Helpers.clamp(MathF.Floor(vel.y / 75), 1, 6);
			if (vel.y > 300) damage += 2;
			proj.damager.damage = damage;
		}
	}

}

#region weapons
public class FlameMFireballWeapon : Weapon {
	public FlameMFireballWeapon() {
		index = (int)WeaponIds.FlameMFireball;
		killFeedIndex = 100;
	}
}

public class FlameMStompWeapon : Weapon {
	public FlameMStompWeapon() {
		index = (int)WeaponIds.FlameMStomp;
		killFeedIndex = 100;
	}
}

public class FlameMOilWeapon : Weapon {
	public FlameMOilWeapon() {
		index = (int)WeaponIds.FlameMOil;
		killFeedIndex = 100;
	}
}

public class FlameMOilFireWeapon : Weapon {
	public FlameMOilFireWeapon() {
		index = (int)WeaponIds.FlameMOilFire;
		killFeedIndex = 100;
	}
}

#endregion

#region projectiles
public class FlameMFireballProj : Projectile {
	public FlameMFireballProj(
		Weapon weapon, Point pos, int xDir, bool isShort,
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 250, 2, player, "flamem_proj_fireball",
		0, 0.01f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FlameMFireball;
		fadeSprite = "flamem_anim_fireball_fade";
		maxTime = 0.75f;
		useGravity = true;
		gravityModifier = 0.5f;
		collider.wallOnly = true;
		if (isShort) {
			vel.x *= 0.5f;
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;

		if (isUnderwater()) {
			destroySelf();
			return;
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		destroySelf();
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is FlameMOilSpillProj oilSpill && oilSpill.ownedByLocalPlayer) {
			playSound("flamemOilBurn", sendRpc: true);
			new FlameMBigFireProj(
				new FlameMOilFireWeapon(), oilSpill.pos, oilSpill.xDir,
				oilSpill.angle ?? 0, owner, owner.getNextActorNetId(), rpc: true
			);
			// oilSpill.time = 0;
			oilSpill.destroySelf(doRpcEvenIfNotOwned: true);
			destroySelf();
		}
	}
}

public class FlameMOilProj : Projectile {
	public FlameMOilProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 175, 0, player, "flamem_proj_oilball",
		0, 0.01f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FlameMOil;
		maxTime = 0.75f;
		useGravity = true;
		vel.y = -150;
		collider.wallOnly = true;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		if (!ownedByLocalPlayer) return;
		if (!destroyed) {
			new FlameMOilSpillProj(
				new FlameMOilWeapon(), other.getHitPointSafe(), 1,
				other.getNormalSafe().angle + 90, owner, owner.getNextActorNetId(), rpc: true
			);
			playSound("flamemOil", sendRpc: true);
			destroySelf();
		}
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;

		if (isUnderwater()) {
			destroySelf();
			return;
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is FlameMBigFireProj bigFire && bigFire.ownedByLocalPlayer && !destroyed) {
			playSound("flamemOilBurn", sendRpc: true);
			bigFire.reignite();
			destroySelf();
		}
	}
}

public class FlameMOilSpillProj : Projectile {
	public FlameMOilSpillProj(
		Weapon weapon, Point pos, int xDir,
		float angle, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "flamem_proj_oilspill",
		0, 0f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FlameMOilSpill;
		maxTime = 8f;
		this.angle = angle;
		destroyOnHit = false;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		moveWithMovingPlatform();
		if (!ownedByLocalPlayer) return;

		if (isUnderwater()) {
			destroySelf();
			return;
		}
	}
}

public class FlameMBigFireProj : Projectile {
	public FlameMBigFireProj(
		Weapon weapon, Point pos, int xDir, float angle, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 2, player, "flamem_proj_bigfire",
		Global.defFlinch, 0.15f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FlameMOilFire;
		maxTime = 8;
		this.angle = angle;
		destroyOnHit = false;
		shouldShieldBlock = false;
		shouldVortexSuck = false;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		moveWithMovingPlatform();

		if (!ownedByLocalPlayer) return;
		if (isUnderwater()) {
			destroySelf();
			return;
		}
	}

	public void reignite() {
		frameIndex = 0;
		frameTime = 0;
		time = 0;
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is FlameMOilSpillProj oilSpill && oilSpill.ownedByLocalPlayer && frameIndex >= 4) {
			playSound("flamemOilBurn", sendRpc: true);
			new FlameMBigFireProj(
				new FlameMOilFireWeapon(), oilSpill.pos, oilSpill.xDir,
				oilSpill.angle ?? 0,
				owner, owner.getNextActorNetId(), rpc: true
			);
			// oilSpill.time = 0;
			oilSpill.destroySelf();
		}
	}
}


public class FlameMStompShockwave : Projectile {
	public FlameMStompShockwave(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "flamem_proj_shockwave", 0, 1f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.75f;
		projId = (int)ProjIds.FlameMStompShockwave;
		destroyOnHit = false;
		shouldShieldBlock = false;
		shouldVortexSuck = false;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onStart() {
		base.onStart();
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;

		if (isAnimOver()) {
			destroySelf();
		}
	}
}

#endregion

#region states

public class FlameMOilState : MaverickState {
	public FlameMOilState() : base("shoot2") {
	}

	public override bool canEnter(Maverick maverick) {
		return base.canEnter(maverick);
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.stopMoving();
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (maverick.frameIndex == 6 && !once) {
			once = true;
			var poi = maverick.getFirstPOI().Value;
			new FlameMOilProj(new FlameMOilWeapon(), poi, maverick.xDir, player, player.getNextActorNetId(), rpc: true);
		}

		if (maverick.isAnimOver()) {
			maverick.changeState(new MIdle());
		}
	}
}

public class FlameMJumpPressState : MaverickState {
	public FlameMJumpPressState() : base("fall") {
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (maverick.grounded) {
			landingCode();
		}
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.vel = new Point(0, 300);
	}
}



public class MammothGrabState : MaverickState {
	private Character grabbedChar;
	float timeWaiting;
	bool grabbedOnce;
	public MammothGrabState() : base("grab") {
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
	}

	public override void update() {
		base.update();

		

		if (grabbedChar != null && grabbedChar.sprite.name.EndsWith("_grabbed")
		&& !maverick.sprite.name.Contains("finisher")) {
			grabbedOnce = true;
					maverick.changeSpriteFromName("grab_finisher", true);	
	
		}

		if (maverick.isAnimOver()) {
			maverick.changeState(new MIdle());
		}
	}

	public override bool trySetGrabVictim(Character grabbed) {
		if (grabbedChar == null) {
			grabbedChar = grabbed;
			return true;
		}
		return false;
	}
}


public class FlameMSlamWeapon : Weapon {
	public FlameMSlamWeapon(Player player) {
		index = (int)WeaponIds.FlameMSlam;
		killFeedIndex = 97;
		damager = new Damager(player, 8, Global.defFlinch, 0.5f);
	}
}


public class MammothSlammed : GenericGrabbedState {
	public Character? grabbedChar;
	public bool launched;
	float launchTime;
	public MammothSlammed(FlameMammoth grabber) : base(grabber, 1, "") {
		customUpdate = true;
	}

	public override void update() {
		base.update();
		if (!character.ownedByLocalPlayer) { return; }

		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				character.changeToIdleOrFall();
				return;
			}
			if (launchTime > 0.1f && character.grounded) {
				new FlameMSlamWeapon((grabber as Maverick).player).applyDamage(character, false, character, (int)ProjIds.MechFrogStompShockwave);
				character.playSound("crash", sendRpc: true);
				character.shakeCamera(sendRpc: true);
			}
		}

		if (grabber.sprite?.name.EndsWith("grab_finisher") == true) {
			if (grabber.frameIndex < 2) {
				trySnapToGrabPoint(true);
			} else if (!launched) {
				launched = true;
				character.unstickFromGround();
				character.vel.y = 600;
			}
		} else {
			notGrabbedTime += Global.spf;
		}

		if (notGrabbedTime > 0.5f) {
			character.changeToIdleOrFall();
		}
	}
}

#endregion
