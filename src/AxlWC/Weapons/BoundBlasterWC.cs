using System;

namespace MMXOnline;

public class BoundBlasterWC : AxlWeaponWC {
	public BoundBlasterWC() {
		shootSounds = [ "boundBlaster", "movingWheel" ];
		fireRate = 9;
		altFireRate = 24;
		index = (int)WeaponIds.BoundBlaster;
		weaponBarBaseIndex = 35;
		weaponSlotIndex = 55;
		killFeedIndex = 70;

		sprite = "axl_arm_boundblaster";
		flashSprite = "axl_pistol_flash";
		chargedFlashSprite = "axl_pistol_flash_charged";

		maxAmmo = 16;
		ammo = maxAmmo;
		maxSwapCooldown = 20 * 4;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new BoundBlasterProj(this, pos, Helpers.byteToDegree(byteAngle), axl.player, netId, rpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new MovingWheelProj(this, pos, axl.armDir, axl.player, netId, rpc: true);
	}

	public override float getFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 6;
		}
		return fireRate;
	}

	public override float getAltFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 14;
		}
		return altFireRate;
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 1;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 8;
	}
}
