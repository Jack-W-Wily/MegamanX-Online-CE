namespace MMXOnline;

public class AxlBulletWC : AxlWeaponWC {
	public bool wasSpecialHeld;
	private bool specialActive;

	public AxlBulletWC() {
		shootSounds = ["axlBullet", "axlBulletCharged", "axlBulletCharged", "axlBulletCharged"];
		index = (int)WeaponIds.AxlBullet;
		weaponBarBaseIndex = 28;
		weaponBarIndex = 28;
		weaponSlotIndex = 28;
		killFeedIndex = 28;
		sprite = "axl_arm_pistol";
		flashSprite = "axl_pistol_flash";
		chargedFlashSprite = "axl_pistol_flash_charged";
		altFireRate = 18;
		displayName = "Axl Bullets";
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
	public override bool attackCtrl(AxlWC axl) {
		Point inputDir = axl.player.input.getInputDir(axl.player);
		bool specialPressed = wasSpecialHeld && !axl.player.input.isHeld(Control.Special1, axl.player);
		// Shoryken does not use negative edge at all.
		if (axl.player.input.checkShoryuken(axl.player, axl.xDir, Control.Special1) && axl.axlWeapon.ammo > 0) {
			axl.changeState(new RainStorm(), true);
			return true;
		}
		// Negative edge inputs.
		if (axl.grounded && axl.axlWeapon.ammo > 0 &&
			inputDir.y == 1 && (
				axl.charState is Dash ||
				axl.player.input.isPressed(Control.Dash, axl.player)
			)
		) {
			axl.changeState(new RisingBarrage(), true);
			return true;
		}
		if (specialPressed && inputDir.y == -1 && ammo > 0) {
			axl.changeState(new TailShot(), true);
			return true;
		}
		if (specialPressed && (inputDir.y == 1 || ammo == 0)) {
			axl.changeState(new OcelotSpin(), true);
			return true;
		}
		if (specialPressed && ammo > 0) {
			axl.vel.y = -axl.getJumpPower() * 2f;
			axl.changeState(new EvasionBarrage(), true);
			return true;
		}
		return false;
	}

	// Negative edge input shenanigans.
	public override void preAxlUpdate(AxlWC axl, bool isSelected) {
		if (!isSelected) {
			wasSpecialHeld = false;
			specialActive = false;
			return;
		}
		if (!specialActive) {
			specialActive = axl.player.input.isPressed(Control.Special1, axl.player);
		}
	}
	public override void postAxlUpdate(AxlWC axl, bool isSelected) {
		if (specialActive && isSelected) {
			wasSpecialHeld = axl.player.input.isHeld(Control.Special1, axl.player);
		}
	}
}
