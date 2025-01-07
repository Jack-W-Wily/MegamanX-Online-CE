using System;

namespace MMXOnline;

public class  SpiralMagnumWC : AxlWeaponWC {
	public static SpiralMagnumWC netWeapon = new();

	public  SpiralMagnumWC() {
		shootSounds = [ "spiralMagnum", "sniperMissile" ];
		fireRate = 45;
		altFireRate = 90;
		index = (int)WeaponIds.SpiralMagnum;
		weaponBarBaseIndex = 34;
		weaponSlotIndex = 54;
		killFeedIndex = 69;

		sprite = "axl_arm_spiralmagnum2";
		flashSprite = "axl_pistol_flash";
		chargedFlashSprite = "axl_pistol_flash";

		maxAmmo = 10;
		ammo = maxAmmo;
		maxSwapCooldown = 60 * 4;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		var bullet = new SpiralMagnumWCProj(
			axl, pos, byteAngle, netId, sendRpc: true
		);
		new SpiralMagnumShell(
			pos,
			-axl.armDir,
			axl.player.getNextActorNetId(),
			sendRpc: true
		);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new SniperMissileProj(this, pos, axl.player, bulletDir, netId, rpc: true);
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 4;
	}
}


public class SpiralMagnumWCProj : Projectile {
	public Point destroyPos;
	public float distTraveled;

	public int passCount = 0;
	public int powerDecrements = 0;
	public bool isScoped;
	float dist;
	float maxDist;
	bool doubleDamageBonus;
	bool isHyper;
	bool playedSoundOnce;

	public SpiralMagnumWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "spiralmagnum_proj", netProjId, player
	) {
		projId = (int)ProjIds.SpiralMagnum;
		weapon = SpiralMagnumWC.netWeapon;
		damager.damage = 2;
		damager.hitCooldown = 15;

		destroyOnHit = false;
		reflectable = true;

		vel = Point.createFromByteAngle(byteAngle) * 600;
		this.byteAngle = byteAngle;
		maxTime = 0.4f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public bool playZing() {
		if (playedSoundOnce) return false;
		var mainCharPos = new Point(Global.level.camCenterX, Global.level.camCenterY);
		var ownerChar = owner?.character;
		if (Global.level.mainPlayer?.character != null && Global.level.mainPlayer.character == ownerChar) return false;
		if (ownerChar != null && ownerChar.getCenterPos().distanceTo(mainCharPos) < Global.screenW / 2) return false;
		if (mainCharPos.distanceTo(pos) > Global.screenW / 2) {
			return false;
		}
		return true;
	}

	public override void onStart() {
		base.onStart();
		new AssassinBulletTrailAnim(pos, this);
	}

	public override void update() {
		base.update();
		if (!playedSoundOnce && playZing()) {
			playedSoundOnce = true;
			playSound("zing1");
		}
	}

	public void increasePassCount(int amount) {
		if (!ownedByLocalPlayer) return;

		bool damageChanged = false;
		if (doubleDamageBonus) {
			doubleDamageBonus = false;
			damager.damage /= 2;
			damageChanged = true;
		}
		passCount += amount;
		if (passCount >= 5) {
			passCount = 0;
			powerDecrements++;
			damager.damage *= 0.5f;
			damageChanged = true;
			vel = vel.times(0.5f);
		}

		if (damageChanged) {
			updateDamager();
		}

		if (powerDecrements > 2) {
			destroySelf();
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		increasePassCount(1);
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		increasePassCount(1);
	}
}
