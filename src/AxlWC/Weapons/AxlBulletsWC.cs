using System;

namespace MMXOnline;

public class AxlBulletWC : AxlWeaponWC {
	public static AxlBulletWC netWeapon = new();

	private bool wasSpecialHeld;
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
		ushort netId = axl.player.getNextActorNetId();
		new AxlBulletWCProj(axl, pos, byteAngle, netId, sendRpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new CopyShotWCProj(axl, pos, chargeLevel, byteAngle, netId, sendRpc: true);
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
				axl.charState is Dash or AirDash || 
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

public class AxlBulletWCProj : Projectile {
	public AxlBulletWCProj(
		Actor owner, Point pos,
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

		vel = Point.createFromByteAngle(byteAngle) * 500;
		this.byteAngle = byteAngle;
		maxTime = 12f / 60f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		new AxlBulletWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
		return null!;
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
		fadeSprite = "buster4_fade";
		fadeOnAutoDestroy = true;
		projId = (int)ProjIds.CopyShotWC;
		weapon = AxlBulletWC.netWeapon;
		damager.damage = 2;
		reflectable = true;

		vel = Point.createFromByteAngle(byteAngle) * 500;
		this.byteAngle = byteAngle;
		maxTime = 0.225f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}

		if (chargeLevel == 2) {
			damager.damage = 3;
			vel *= 1.25f;
			maxTime /= 1.25f;
			xScale = 1.25f;
			yScale = 1.25f;
		}
		else if (chargeLevel >= 3) {
			damager.damage = 4;
			vel *= 1.5f;
			maxTime /= 1.5f;
			xScale = 1.5f;
			yScale = 1.5f;
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		new CopyShotWCProj(
			args.owner, args.pos, args.extraData[0], args.byteAngle, args.netId, player: args.player
		);
		return null!;
	}
}

