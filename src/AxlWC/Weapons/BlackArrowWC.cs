namespace MMXOnline;

public class BlackArrowtWC : AxlWeaponWC {
	public BlackArrowtWC() {
		shootSounds = [ "blackArrow", "blackArrow", "blackArrow", "blackArrow" ];
		fireRate = 24;
		altFireCooldown = 48;
		index = (int)WeaponIds.BlackArrow;
		weaponBarBaseIndex = 33;
		weaponSlotIndex = 53;
		killFeedIndex = 68;
		sprite = "axl_arm_blackarrow";
		maxAmmo = 8;
		ammo = maxAmmo;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new BlackArrowProj(this, pos, axl.player, bulletDir, netId, rpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		Point bulletDir1 = Point.createFromByteAngle(byteAngle - 22);
		Point bulletDir2 = Point.createFromByteAngle(byteAngle + 22);
		new BlackArrowProj2(this, pos, axl.player, bulletDir1, axl.player.getNextActorNetId(), rpc: true);
		new BlackArrowProj2(this, pos, axl.player, bulletDir, axl.player.getNextActorNetId(), rpc: true);
		new BlackArrowProj2(this, pos, axl.player, bulletDir2, axl.player.getNextActorNetId(), rpc: true);
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return maxAmmo;
	}
}
