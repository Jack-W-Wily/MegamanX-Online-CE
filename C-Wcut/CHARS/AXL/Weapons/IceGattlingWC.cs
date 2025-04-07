using System;

namespace MMXOnline;

public class IceGattlingWC : AxlWeaponWC {
	public static IceGattlingWC netWeapon = new();
	public float targetFireRate;
	public float fireRateReduceCooldown = 0;
	public float minFireRate = 5;
	public float maxFireRate = 9;
	public float animFrames;
	public float fireRateStacks;

	public IceGattlingWC() {
		shootSounds = [ "iceGattling", "gaeaShield" ];
		isTwoHanded = true;
		fireRate = minFireRate;
		throwIndex = (int)ThrowID.IceGattling;
		altFireRate = 24;
		index = (int)WeaponIds.IceGattling;
		weaponBarBaseIndex = (int)WeaponBarIndex.IceGattling;
		weaponSlotIndex = (int)SlotIndex.IGattling;
		killFeedIndex = 72;

		sprite = "axl_arm_icegattling";
		flashSprite = "x8_axl_bullet_flash3";
		chargedFlashSprite = "x8_axl_bullet_cflash3";

		maxAmmo = 22;
		ammo = maxAmmo;
		maxSwapCooldown = 20 * 5;
	}

	public override void update() {
		base.update();
		// Fire rate auto-reduction.
		if (fireRateReduceCooldown <= 0) {
			if (targetFireRate < maxFireRate) {
				targetFireRate++;
			} else {
				targetFireRate = 14;
			}
			fireRateReduceCooldown = 4;
			fireRateStacks = 0;
		} else {
			fireRateReduceCooldown -= Global.speedMul;
		}
		// Animation.
		if (targetFireRate < maxFireRate) {
			animFrames += Global.speedMul;
			if (animFrames >= targetFireRate / 3f) {
				spriteFrameIndex++;
				animFrames = 0;
				if (spriteFrameIndex > 2) { spriteFrameIndex = 0; }
			}
		}
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new IceGattlingWCProj(axl, pos, byteAngle, netId, sendRpc: true);
		if (targetFireRate > minFireRate && fireRateStacks >= 1) {
			targetFireRate -= 1;
			if (targetFireRate < minFireRate) { targetFireRate = minFireRate; }
		} else {
			fireRateStacks++;
		}
		fireRateReduceCooldown = 18;
	}

	public override float getFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return MathF.Ceiling(targetFireRate * 0.75f);
		}
		return targetFireRate;
	}

	public override float getAltFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 20;
		}
		return altFireRate;
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		new IceGattlingAltWCProj(axl, pos, byteAngle, netId, sendRpc: true);
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 8;
		}
		return 11;
	}
}

public class IceGattlingWCProj : Projectile {
	float sparkleTime;
	Anim spark;
	public IceGattlingWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "x8_axl_igattling_proj", netProjId, player
	) {
		fadeSprite = "x8_axl_igattling_fade";
		projId = (int)ProjIds.IceGattlingWC;
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
	public override void update() {
		base.update();
		sparkleTime += speedMul;
		if (sparkleTime > 2	) {
			sparkleTime = 0;
			spark = new Anim(pos, "x8_axl_igattling_sparkles", 1, null, true);
			spark.useGravity = true;
		}
	}


	

	public static Projectile rpcInvoke(ProjParameters args) {
		return new IceGattlingWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
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

public class IceGattlingAltWCProj : Projectile {
	public IceGattlingAltWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "frosttowercharged_proj", netProjId, player
	) {
		projId = (int)ProjIds.IceGattlingAltWC;
		weapon = IceGattlingWC.netWeapon;
		damager.damage = 2f;
		damager.flinch = Global.halfFlinch;
		sprite.frameIndex = 7;
		xScale = 0.5f;
		yScale = 0.5f;

		vel = Point.createFromByteAngle(byteAngle) * 400;
		this.byteAngle = byteAngle + 64;
		maxTime = 0.35f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		playSound("iceBreak");
		Anim.createGibEffect("hyoroga_proj_pieces", getCenterPos(), owner);
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new IceGattlingAltWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
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
