using System;

namespace MMXOnline;

public class FlameBurnerWC : AxlWeaponWC {
	public FlameBurnerWC() {
		shootSounds = [ "flameBurner", "circleBlaze" ];
		isTwoHanded = true;
		fireRate = 5;
		altFireRate = 14;
		index = (int)WeaponIds.FlameBurner;
		weaponBarBaseIndex = 38;
		weaponSlotIndex = 58;
		killFeedIndex = 73;
		sprite = "axl_arm_flameburner";

		maxAmmo = 16;
		ammo = maxAmmo;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle + Helpers.randomRange(0, 10) - 5);
		Point bulletDir1 = Point.createFromByteAngle(byteAngle - Helpers.randomRange(2, 16));
		Point bulletDir2 = Point.createFromByteAngle(byteAngle + Helpers.randomRange(2, 16));
		new FlameBurnerProj(this, pos, 1, axl.player, bulletDir1, axl.player.getNextActorNetId(), sendRpc: true);
		new FlameBurnerProj(this, pos, 1, axl.player, bulletDir2, axl.player.getNextActorNetId(), sendRpc: true);
		new FlameBurnerProj(this, pos, 1, axl.player, bulletDir, axl.player.getNextActorNetId(), sendRpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		new CircleBlazeProj(this, pos, 1, axl.player, bulletDir, netId, sendRpc: true);
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 0.75f;
		}
		return 1;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 8;
		}
		return 16;
	}
}
