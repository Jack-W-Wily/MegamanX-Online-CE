using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace MMXOnline;

public class RayGunWC : AxlWeaponWC {
	public static RayGunWC netWeapon = new();

	public RayGunWC() {
		shootSounds = ["raygun", "splashLaser"];
		fireRate = 6;
		altFireRate = 8;
		sprite = "axl_arm_raygun";
		flashSprite = "axl_raygun_flash";
		chargedFlashSprite = "axl_raygun_flash";
		index = (int)WeaponIds.RayGun;
		weaponBarBaseIndex = 30;
		weaponBarIndex = 28;
		weaponSlotIndex = 34;
		killFeedIndex = 33;
		canHealAmmo = false;

		maxAmmo = 16;
		ammo = maxAmmo;
		maxSwapCooldown = 60 * 4;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new RayGunWCProj(axl, pos, byteAngle, netId, sendRpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new SplashLaserProj(this, pos, axl.player, bulletDir, netId, sendRpc: true);
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 1;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 1;
	}
}

public class RayGunWCProj : Projectile {
	float len = 0;
	float lenDelay = 0;
	const float maxLen = 50;
	
	public RayGunWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "spiralmagnum_proj", netProjId, player
	) {
		weapon = RayGunWC.netWeapon;
		projId = (int)ProjIds.RayGun;
		damager.damage = 0.5f;
		damager.hitCooldown = 1;

		vel = Point.createFromByteAngle(byteAngle) * 400;
		this.byteAngle = byteAngle;
		maxTime = 0.25f;
		reflectable = true;
		destroyOnHitWall = true;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public void updateAngle() {
		angle = vel.angle;
	}

	public override void update() {
		base.update();
		if (lenDelay > 0.01f) {
			len += Global.spf * 300;
			if (len > maxLen) len = maxLen;
		}
		lenDelay += Global.spf;
	}

	public void reflectSide() {
		vel.x *= -1;
		len = 0;
		lenDelay = 0;
		updateAngle();
	}

	public override void onReflect() {
		reflectSide();
		time = 0;
	}

	public override void onDeflect() {
		base.onDeflect();
		len = 0;
		lenDelay = 0;
		updateAngle();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -600));
		}
	}

	public override void render(float x, float y) {
		var normVel = vel.normalize();
		float xOff1 = -(normVel.x * len);
		float yOff1 = -(normVel.y * len);
		float sin = MathF.Sin(Global.time * 42.5f);

		DrawWrappers.DrawLine(
			pos.x + xOff1, pos.y + yOff1, pos.x, pos.y,
			new Color(74, 78, 221), 4 + sin, zIndex - 4, true
		);
		DrawWrappers.DrawLine(
			pos.x + xOff1, pos.y + yOff1, pos.x, pos.y,
			new Color(61, 113, 255), 2 + sin, zIndex - 2, true
		);
		DrawWrappers.DrawLine(
			pos.x + xOff1, pos.y + yOff1, pos.x, pos.y,
			new Color(215, 244, 255), 1 + sin, zIndex, true
		);
	}
}
