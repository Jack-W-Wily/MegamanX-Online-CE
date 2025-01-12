using System;

namespace MMXOnline;

public class PlasmaGunWC : AxlWeaponWC {
	public PlasmaGunWC() {
		shootSounds = [ "plasmaGun", "" ];
		isTwoHanded = true;
		fireRate = 14;
		altFireRate = 2;
		index = (int)WeaponIds.PlasmaGun;
		weaponBarBaseIndex = 36;
		weaponSlotIndex = 56;
		killFeedIndex = 71;
		sprite = "axl_arm_plasmagun";
		maxSwapCooldown = 60;
		maxAmmo = 4;
		ammo = 4;

		maxAmmo = 2;
		ammo = maxAmmo;
		maxSwapCooldown = 20 * 2;
		autoFire = true;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		ushort netIdEffect = axl.player.getNextActorNetId();
		// Effect.
		new Anim(pos, "plasmagun_effect", 1, netIdEffect, true, sendRpc: true) {
			byteAngle = byteAngle,
			host = axl
		};
		// Projectile itself.
		new PlasmaGunProj(this, pos, 1, axl.player, bulletDir, netId, sendRpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {

	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 1;
		}
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 0;
	}
}
