using System;

namespace MMXOnline;

public class IceGattlingWC : AxlWeaponWC {
	public static IceGattlingWC netWeapon = new();

	public float targetFireRate;
	public float fireRateReduceCooldown = 0;
	public float minFireRate = 4;
	public float maxFireRate = 14;
	public float animFrames;
	public float fireRateStacks;

	public IceGattlingWC() {
		shootSounds = [ "iceGattling", "gaeaShield" ];
		isTwoHanded = true;
		fireRate = minFireRate;
		altFireRate = 60;
		index = (int)WeaponIds.IceGattling;
		weaponBarBaseIndex = 37;
		weaponSlotIndex = 57;
		killFeedIndex = 72;

		sprite = "axl_arm_icegattling";
		flashSprite = "axl_pistol_flash";
		chargedFlashSprite = "axl_pistol_flash";

		maxAmmo = 28;
		ammo = maxAmmo;
		maxSwapCooldown = 60 * 5;
	}

	public override void update() {
		base.update();
		// Fire rate auto-reduction.
		if (fireRateReduceCooldown <= 0) {
			if (fireRate < maxFireRate) {
				fireRate++;
			} else {
				fireRate = 14;
			}
			fireRateReduceCooldown = 4;
			fireRateStacks = 0;
		} else {
			fireRateReduceCooldown -= Global.speedMul;
		}
		// Animation.
		if (fireRate < maxFireRate) {
			animFrames += Global.speedMul;
			if (animFrames >= fireRate / 3f) {
				spriteFrameIndex++;
				animFrames = 0;
				if (spriteFrameIndex > 2) { spriteFrameIndex = 0; }
			}
		}
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new IceGattlingWCProj(axl, pos, byteAngle, netId, sendRpc: true);
		if (fireRate > minFireRate && fireRateStacks >= 2) {
			fireRate -= 1;
			if (fireRate < minFireRate) { fireRate = minFireRate; }
		} else {
			fireRateStacks++;
		}
		fireRateReduceCooldown = 18;
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {

	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return maxAmmo;
	}
}

public class IceGattlingWCProj : Projectile {
	public IceGattlingWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "icegattling_proj", netProjId, player
	) {
		projId = (int)ProjIds.IceGattling;
		weapon = IceGattlingWC.netWeapon;
		damager.damage = 0.5f;
		reflectable = true;

		vel = Point.createFromByteAngle(byteAngle) * 400;
		this.byteAngle = byteAngle;
		maxTime = 0.4f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}
}
