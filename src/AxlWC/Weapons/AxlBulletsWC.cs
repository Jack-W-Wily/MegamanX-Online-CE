namespace MMXOnline;

public class AxlBulletWC : AxlWeaponWC {
	public AxlBulletWC() {
		shootSounds = [ "axlBullet", "axlBulletCharged", "axlBulletCharged", "axlBulletCharged" ];
		index = (int)WeaponIds.AxlBullet;
		weaponBarBaseIndex = 28;
		weaponBarIndex = 28;
		weaponSlotIndex = 28;
		killFeedIndex = 28;
		sprite = "axl_arm_pistol";
		flashSprite = "axl_pistol_flash";
		chargedFlashSprite = "axl_pistol_flash_charged";
		altFireCooldown = 18;
		displayName = "Axl Bullets";
		canHealAmmo = true;		
		maxAmmo = 20;
		ammo = maxAmmo;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new AxlBulletProj(this, pos, axl.player, bulletDir, netId);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new CopyShotProj(this, pos, chargeLevel, axl.player, bulletDir, netId);
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return chargeLevel switch {
			1 => 4,
			2 => 6,
			3 => 8,
			_ => 8,
		};
	}
}
