using System;

namespace MMXOnline;

public class BlastLauncherWC : AxlWeaponWC {
	public static BlastLauncherWC netWeapon = new();

	public BlastLauncherWC() {
		shootSounds = [ "grenadeShoot", "rocketShoot" ];
		index = (int)WeaponIds.BlastLauncher;
		weaponBarBaseIndex = 29;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 29;
		killFeedIndex = 29;
		fireRate = 45;
		altFireRate = 48;

		sprite = "axl_arm_blastlauncher";
		flashSprite = "axl_pistol_flash_charged";
		chargedFlashSprite = "axl_pistol_flash_charged";
		isTwoHanded = true;

		maxAmmo = 10;
		ammo = maxAmmo;
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

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
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
		pos, 1, owner, "axl_grenade", netProjId, player
	) {
		weapon = BlastLauncherWC.netWeapon;
		this.byteAngle = byteAngle;
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		vel.x = 300 * bulletDir.x;
		vel.y = 300 * bulletDir.y;
		projId = (int)ProjIds.BlastLauncherGrenadeProj;
		useGravity = true;
		collider.wallOnly = true;
		destroyOnHit = false;
		shouldShieldBlock = false;
		reflectableFBurner = true;
		maxTime = 2;
		player = this.player;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
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
		if (ownedByLocalPlayer) {
			var damagable = other.gameObject as IDamagable;
			if (damagable != null && damagable.canBeDamaged(owner.alliance, owner.id, projId) && !vel.isZero()) {
				destroySelf();
				return;
			}
		}
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
		detonate();
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
		pos, 1, owner, "axl_rocket", netProjId, player
	) {
		weapon = BlastLauncherWC.netWeapon;
		projId = (int)ProjIds.GreenSpinner;
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
		if (time >= maxTime) return;
		var netId = owner.getNextActorNetId();
		if (angle != null)
		new GreenSpinnerExplosionProj(weapon, pos, xDir, owner, angle.Value, null, Math.Sign(vel.x), netId);
	}
}
