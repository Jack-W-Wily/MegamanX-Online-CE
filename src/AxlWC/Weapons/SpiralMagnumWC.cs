using System;

namespace MMXOnline;

public class  SpiralMagnumWC : AxlWeaponWC {
	public static SpiralMagnumWC netWeapon = new();

	public  SpiralMagnumWC() {
		shootSounds = [ "spiralMagnum", "acidBurst" ];
		fireRate = 30;
		altFireRate = 40;
		index = (int)WeaponIds.SpiralMagnum;
		throwIndex = (int)ThrowID.SpiralMagnum;
		weaponBarBaseIndex = (int)WeaponBarIndex.SpiralMagnum;
		weaponSlotIndex = (int)SlotIndex.SMagnum;
		killFeedIndex = 69;

		sprite = "axl_arm_spiralmagnum";
		flashSprite = "x8_axl_bullet_flash";
		chargedFlashSprite = "x8_axl_bullet_cflash";

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
		new FormidAcidProj(axl, pos, byteAngle, netId, sendRpc: true);
	}

	public override float getFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 20;
		}
		return fireRate;
	}

	public override float getAltFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 30;
		}
		return altFireRate;
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 2;
		}
		return 3;
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
		pos, 1, owner, "x8_axl_magnum_proj", netProjId, player
	) {
		projId = (int)ProjIds.SpiralMagnumWC;
		weapon = SpiralMagnumWC.netWeapon;
		damager.damage = 2.5f;
		damager.flinch = Global.halfFlinch;

		destroyOnHit = true;
		reflectable = false;

		vel = Point.createFromByteAngle(byteAngle) * 600;
		this.byteAngle = byteAngle;
		maxTime = 0.325f;

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
	
	public static Projectile rpcInvoke(ProjParameters args) {
		return new SpiralMagnumWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
	}
}

public class FormidAcidProj : Projectile {
	bool acidSplashOnce;
	int bounces = 0;
	int type;
	bool once;
	float yBounce;
	float xDistTraveled;

	public FormidAcidProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "tseahorse_proj_acid", netProjId, player
	) {
		projId = (int)ProjIds.FormicAcidWC;
		weapon = SpiralMagnumWC.netWeapon;
		damager.damage = 1;
		damager.flinch = Global.miniFlinch;

		destroyOnHit = true;
		vel = Point.createFromByteAngle(byteAngle) * 300;
		this.byteAngle = byteAngle;
		maxTime = 1.25f;
		useGravity = true;
		fadeSound = "acidBurst";
		vel.y *= 1.5f;
		yBounce = MathF.Abs(vel.y) * -1;
		if (yBounce > -150) {
			yBounce = -150;
		}

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public override void update() {
		base.update();
		xDistTraveled += MathF.Abs(deltaPos.x);
		if (xDistTraveled > 200) {
			destroySelf();
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		if (!acidSplashOnce) {
			acidSplashEffect(null, ProjIds.FormicAcidSmallWC);
		}
	}

	public override void onHitWall(CollideData other) {
		acidSplashEffect(other, ProjIds.FormicAcidSmallWC);
		bounces++;
		if (bounces > 3) {
			destroySelf();
			return;
		}
		var normal = other.hitData.normal ?? new Point(0, -1);
		if (normal.isSideways()) {
			vel.x *= -1;
			incPos(new Point(5 * MathF.Sign(vel.x), 0));
		} else {
			vel.y = MathF.Sign(vel.y) * yBounce;
			if (vel.y == 0) vel.y = yBounce;
			incPos(new Point(0, 5 * MathF.Sign(vel.y)));
		}
		playSound("acidBurst");
	}

	public override void onHitDamagable(IDamagable damagable) {
		if (ownedByLocalPlayer) {
			if (!acidSplashOnce) {
				acidSplashOnce = true;
				acidSplashParticles(pos, false, 1, 1, ProjIds.TSeahorseAcid2);
				acidFadeEffect();
			}
		}
		base.onHitDamagable(damagable);
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new FormidAcidProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
	}
}
