using System;

namespace MMXOnline;

public class BlastLauncherWC : AxlWeaponWC {
	public static BlastLauncherWC netWeapon = new();

	public BlastLauncherWC() {
		shootSounds = [ "grenadeShoot", "rocketShoot" ];
		index = (int)WeaponIds.BlastLauncher;
		weaponBarBaseIndex = (int)WeaponBarIndex.BlastLauncher;
		weaponSlotIndex = (int)SlotIndex.BLauncher;
		killFeedIndex = 29;
		fireRate = 28;
		altFireRate = 14;
		throwIndex = (int)ThrowID.BlastLauncher;
		sprite = "axl_arm_blastlauncher";
		flashSprite = "x8_axl_bullet_flash";
		chargedFlashSprite = "x8_axl_bullet_cflash";
		isTwoHanded = true;

		maxAmmo = 10;
		ammo = maxAmmo;
		maxSwapCooldown = 20 * 4;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new BlastLauncherWCProj(axl, pos, byteAngle, netId, sendRpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new GreenSpinnerWCProj(axl, pos, byteAngle, netId, sendRpc: true);
	}

	public override float getFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 14;
		}
		return fireRate;
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return maxAmmo / 2f;
		}
		return maxAmmo;
	}
}


public class BlastLauncherWCProj : Projectile, IDamagable {
	Player player;
	float health = 3;

	public BlastLauncherWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "x8_axl_blauncher_proj", netProjId, player
	) {
		weapon = BlastLauncherWC.netWeapon;
		this.byteAngle = byteAngle;
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		vel.x = 300 * bulletDir.x;
		vel.y = 300 * bulletDir.y;
		projId = (int)ProjIds.BlastLauncherWC;
		damager.damage = 2;
		damager.hitCooldown = 15;
		useGravity = true;
		collider.wallOnly = true;
		destroyOnHit = true;
		shouldShieldBlock = false;
		reflectableFBurner = true;
		maxTime = 2;
		this.player = ownerPlayer;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new BlastLauncherWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
	}

	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}

	public override void update() {
		base.update();
		updateAngle();
		if (MathF.Abs(vel.y) < 0.5f && grounded) {
			vel.y = 0;
			vel.x *= 0.5f;
		}
		if (MathF.Abs(vel.x) < 1) {
			vel.x = 0;
		}
	}

	public void updateAngle() {
		if (vel.magnitude > 50) {
			angle = MathF.Atan2(vel.y, vel.x) * 180 / MathF.PI;
		}
		xDir = 1;
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (other.gameObject is Wall wall) {
			Point? normal = other.hitData.normal;
			if (normal != null) {
				if (normal.Value.x != 0) vel.x *= -0.5f;
				if (normal.Value.y != 0) vel.y *= -0.5f;
			}
		}
	}

	public override void onDestroy() {
		if (!ownedByLocalPlayer) return;
		if (health > 0) {
			detonate();
		}
	}

	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		health -= damage;
		if (health < 0) {
			health = 0;
			destroySelf();
		}
	}
	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return owner.alliance != damagerAlliance;
	}
	public void detonate() {
		//playSound("detonate", sendRpc: true);
		if (player != null) {
			new GrenadeExplosionProj(
				weapon, pos, xDir, player, 0, null, Math.Sign(vel.x), player.getNextActorNetId()
			);
		}
		destroySelfNoEffect();
	}
	public bool isInvincible(Player attacker, int? projId) { return false; }
	public bool canBeHealed(int healerAlliance) { return false; }
	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) { }
	public bool isPlayableDamagable() { return false; }
}

public class GreenSpinnerWCProj : Projectile {
	public GreenSpinnerWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "x8_axl_green_spinner", netProjId, player
	) {
		weapon = BlastLauncherWC.netWeapon;
		projId = (int)ProjIds.GreenSpinnerWC;
		this.byteAngle = byteAngle;
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		vel.x = 400 * bulletDir.x;
		vel.y = 400 * bulletDir.y;
		collider.wallOnly = true;
		destroyOnHit = false;
		maxTime = 0.35f;
		shouldShieldBlock = false;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new GreenSpinnerWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (ownedByLocalPlayer) {
			var damagable = other.gameObject as IDamagable;
			if (damagable != null && damagable.canBeDamaged(owner.alliance, owner.id, projId)) {
				destroySelf();
				return;
			}
		}
		var wall = other.gameObject as Wall;
		if (wall != null) {
			destroySelf(disableRpc: true);
			return;
		}
	}

	public override void onDestroy() {
		if (!ownedByLocalPlayer) return;
		var netId = owner.getNextActorNetId();
		if (angle != null) {
			new GreenSpinnerExplosionProj(weapon, pos, xDir, owner, angle.Value, null, Math.Sign(vel.x), netId);
		}
	}
}
