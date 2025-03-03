using System;
using System.Collections.Generic;
using SFML.Graphics;
namespace MMXOnline;

public class AxlBulletWC : AxlWeaponWC {
	public static AxlBulletWC netWeapon = new();
	public float ammoUsedSinceBlue = 4;

	private bool wasSpecialHeld;
	private bool specialActive;

	public AxlBulletWC() {
		throwIndex = (int)ThrowID.AxlBullet;
		shootSounds = ["axlBullet", "axlBulletCharged", "axlBulletCharged", "axlBulletCharged"];
		index = (int)WeaponIds.AxlBullet;
		weaponSlotIndex = (int)SlotIndex.Abullet;
		killFeedIndex = 28;
		weaponBarBaseIndex = (int)WeaponBarIndex.AxlBullet;
		sprite = "axl_arm_pistol";
		flashSprite = "x8_axl_bullet_flash";
		chargedFlashSprite = "";
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
		ushort netIdEffect = axl.player.getNextActorNetId();
		if(chargeLevel >= 2){
			new CopyShotWCProj(axl, pos, chargeLevel, byteAngle, netId, sendRpc: true);
			new Anim(pos, "x8_axl_bullet_cflash4", 1, netIdEffect, true, sendRpc: true) {
			byteAngle = byteAngle, host = axl};
		}
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 0;
	}

	public override void onAmmoChange(float amount) {
		if (amount <= 0) {
			ammoUsedSinceBlue -= amount;
		} else {
			ammoUsedSinceBlue = 1600;
		}
	}

	public override bool attackCtrl(AxlWC axl) {
		if(axl.isCharging()) return false;
		Point inputDir = axl.player.input.getInputDir(axl.player);
		bool specialPressed = wasSpecialHeld && !axl.player.input.isHeld(Control.Special1, axl.player);
		// Shoryken does not use negative edge at all.
		if (axl.autoChargeCooldown <= 0 && axl.player.input.checkShoryuken(axl.player, axl.xDir, Control.Special1) && ammo > 0) {
			axl.changeState(new RainStorm(), true);
			return true;
		}
		// Negative edge inputs.
		if (axl.autoChargeCooldown <= 0 && axl.grounded && ammo > 0 && inputDir.y == -1 && axl.charState is not RisingBarrage && (
				axl.charState is Dash or AirDash ||
				axl.player.input.isPressed(Control.Dash, axl.player)
			)
		) {
			axl.changeState(new RisingBarrage(), true);
			return true;
		}
		if (axl.autoChargeCooldown <= 0 && specialPressed && inputDir.y == -1 && ammo > 0) {
			if (axl.grounded) {
				axl.changeState(new TailShot(), true);
			} else {
				axl.changeState(new AxlRainDrop(), true);
			}
			return true;
		}
		if (axl.autoChargeCooldown <= 0 && specialPressed && axl.grounded && inputDir.y == 1 && axl.charState is not OcelotSpin) {
			axl.changeState(new OcelotSpin(), true);
			return true;
		}
		if (axl.autoChargeCooldown <= 0 && specialPressed && ammo > 0 && axl.sprite.name.Contains("dash")) {
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
		pos, 1, owner, "x8_axl_bullet_proj", netProjId, player
	) {
		fadeSprite = "x8_axl_bullet_fade";
		projId = (int)ProjIds.AxlBulletWC;
		weapon = AxlBulletWC.netWeapon;
		damager.damage = 1;
		reflectable = true;
		destroyOnHitWall = true;

		vel = Point.createFromByteAngle(byteAngle) * 8.5f * 60f;
		this.byteAngle = byteAngle;
		maxTime = 14f / 60f;

		if (type >= 1) {
			changeSprite("x8_axl_bullet2_proj", true);
			fadeSprite = "x8_axl_bullet2_fade";
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


		public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -800));
		}
	}

}

public class CopyShotWCProj : Projectile {
	public List<Point> lastPoses = new List<Point>();
	public Sprite sprite;
	public Sprite trailSprite;

	public CopyShotWCProj(
		Actor owner, Point pos, int chargeLevel,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "x8_axl_cshot_proj", netProjId, player
	) {
		sprite = new Sprite("x8_axl_cshot_proj");
		//trailSprite = new Sprite("");
		damager.damage = 1;
		damager.flinch = Global.miniFlinch;
		fadeSprite = "x8_axl_cshot_fade";
		fadeOnAutoDestroy = true;
		projId = (int)ProjIds.CopyShotWC;
		weapon = AxlBulletWC.netWeapon;
		reflectable = true;
		vel = Point.createFromByteAngle(byteAngle) * 500;
		this.byteAngle = byteAngle;
		maxTime = 0.225f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle, [(byte)chargeLevel]);
		}}
	public override void update() {
		base.update();
		lastPoses.Add(pos);
		if (lastPoses.Count > 5) lastPoses.RemoveAt(0);
	}
	/*public override void render(float x, float y) {
		base.render(x, y);
		sprite.draw(frameIndex, pos.x + x, pos.y + y, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex);
		if (Options.main.lowQualityParticles()) return;

		for (int i = lastPoses.Count - 1; i >= 1; i--) {
			Point head = lastPoses[i];
			Point outerTail = lastPoses[i - 1];
			Point innerTail = lastPoses[i - 1];
			if (i == 1) {
				innerTail = innerTail.add(head.directionToNorm(innerTail).times(5));
			}
	
			/DrawWrappers.DrawLine(head.x, head.y, outerTail.x, outerTail.y, new Color(255, 75, 3), 1, 0, true);
			DrawWrappers.DrawLine(head.x, head.y, innerTail.x, innerTail.y, new Color(255, 75, 3), 1, 1, true);
			DrawWrappers.DrawLine(head.x, head.y + 10, innerTail.x, innerTail.y + 10, new Color(255, 75, 3), 1, 1, true);

		}
	}*/
		public static Projectile rpcInvoke(ProjParameters args) {
		return new CopyShotWCProj(
			args.owner, args.pos, args.extraData[0], args.byteAngle, args.netId, player: args.player
		);
	}


		public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -800));
		}
	}
	
}

