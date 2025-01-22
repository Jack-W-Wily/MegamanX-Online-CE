using System;

namespace MMXOnline;

public class AxlBulletWC : AxlWeaponWC {
	public static AxlBulletWC netWeapon = new();
	public float ammoUsedSinceBlue = 4;

	private bool wasSpecialHeld;
	private bool specialActive;

	public AxlBulletWC() {
		shootSounds = ["axlBullet", "axlBulletCharged", "axlBulletCharged", "axlBulletCharged"];
		index = (int)WeaponIds.AxlBullet;
		weaponSlotIndex = 28;
		killFeedIndex = 28;
		weaponBarBaseIndex = (int)WeaponBarIndex.AxlBullet;
		sprite = "axl_arm_pistol";
		flashSprite = "axl_pistol_flash";
		chargedFlashSprite = "axl_pistol_flash_charged";
		altFireRate = 18;
		displayName = "Axl Bullets";
		maxAmmo = 16;
		ammo = maxAmmo;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		int type = 0;
		// Random values and WA versions.
		int treshold = 4;
		int randRange = 0;
		if (axl.isWhite) {
			treshold = 2;
			randRange = 1;
		}
		// Pseudo-random to guarantee at least every 4 shots.
		if (ammoUsedSinceBlue >= treshold || Helpers.randomRange(0, 3) <= randRange || ammo <= 1) {
			// if less than 4 shots were fire increase the threshold..
			if (ammoUsedSinceBlue < 4) {
				ammoUsedSinceBlue *= -1;
			} else {
				ammoUsedSinceBlue = 0;
			}
			// Activate crit.
			type = 1;
		}
		new AxlBulletWCProj(axl, pos, type, byteAngle, netId, sendRpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new CopyShotWCProj(axl, pos, chargeLevel, byteAngle, netId, sendRpc: true);
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return chargeLevel switch {
			1 => 2,
			2 => 3,
			3 => 4,
			_ => 4,
		};
	}

	public override void onAmmoChange(float amount) {
		if (amount <= 0) {
			ammoUsedSinceBlue -= amount;
		} else {
			ammoUsedSinceBlue = 1600;
		}
	}

	public override bool attackCtrl(AxlWC axl) {
		Point inputDir = axl.player.input.getInputDir(axl.player);
		bool specialPressed = wasSpecialHeld && !axl.player.input.isHeld(Control.Special1, axl.player);
		// Shoryken does not use negative edge at all.
		if (axl.player.input.checkShoryuken(axl.player, axl.xDir, Control.Special1) && ammo > 0) {
			axl.changeState(new RainStorm(), true);
			return true;
		}
		// Negative edge inputs.
		if (axl.grounded && ammo > 0 && inputDir.y == -1 && axl.charState is not RisingBarrage && (
				axl.charState is Dash or AirDash ||
				axl.player.input.isPressed(Control.Dash, axl.player)
			)
		) {
			axl.changeState(new RisingBarrage(), true);
			return true;
		}
		if (specialPressed && inputDir.y == -1 && ammo > 0) {
			if (axl.grounded) {
				axl.changeState(new TailShot(), true);
			} else {
				axl.changeState(new AxlRainDrop(), true);
			}
			return true;
		}
		if (specialPressed && axl.grounded && inputDir.y == 1 && axl.charState is not OcelotSpin) {
			axl.changeState(new OcelotSpin(), true);
			return true;
		}
		if (specialPressed && ammo > 0) {
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

	public override void axlUpdate(AxlWC axl, bool isSelected) {
		if (ammo >= maxAmmo) {
			ammoUsedSinceBlue = 1600;
		}
	}

	public override void postAxlUpdate(AxlWC axl, bool isSelected) {
		if (specialActive && isSelected) {
			wasSpecialHeld = axl.player.input.isHeld(Control.Special1, axl.player);
		}
	}
}

public class AxlBulletWCProj : Projectile {
	public AxlBulletWCProj(
		Actor owner, Point pos, int type,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "axl_bullet", netProjId, player
	) {
		fadeSprite = "axl_bullet_fade";
		projId = (int)ProjIds.AxlBulletWC;
		weapon = AxlBulletWC.netWeapon;
		damager.damage = 1;
		reflectable = true;
		destroyOnHitWall = true;

		vel = Point.createFromByteAngle(byteAngle) * 8.5f * 60f;
		this.byteAngle = byteAngle;
		maxTime = 14f / 60f;

		if (type >= 1) {
			changeSprite("axl_bullet_blue", true);
			damager.damage = 2;
			damager.flinch = Global.halfFlinch;
		}

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle, (byte)type);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new AxlBulletWCProj(
			args.owner, args.pos, args.extraData[0], args.byteAngle, args.netId, player: args.player
		);
	}
}

public class CopyShotWCProj : Projectile {
	public CopyShotWCProj(
		Actor owner, Point pos, int chargeLevel,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "axl_bullet_charged", netProjId, player
	) {
		fadeSprite = "axl_bullet_charged_fade";
		fadeOnAutoDestroy = true;
		projId = (int)ProjIds.CopyShotWC;
		weapon = AxlBulletWC.netWeapon;
		damager.damage = 2;
		damager.flinch = Global.halfFlinch;
		reflectable = true;

		vel = Point.createFromByteAngle(byteAngle) * 500;
		this.byteAngle = byteAngle;
		maxTime = 0.225f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle, [(byte)chargeLevel]);
		}

		if (chargeLevel == 2) {
			damager.damage = 3;
			damager.flinch = Global.defFlinch;
			vel *= 1.25f;
			maxTime /= 1.25f;
			xScale = 1.25f;
			yScale = 1.25f;
		} else if (chargeLevel >= 3) {
			damager.damage = 4;
			damager.flinch = Global.superFlinch;
			vel *= 1.5f;
			maxTime /= 1.5f;
			xScale = 1.5f;
			yScale = 1.5f;
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new CopyShotWCProj(
			args.owner, args.pos, args.extraData[0], args.byteAngle, args.netId, player: args.player
		);
	}
}

