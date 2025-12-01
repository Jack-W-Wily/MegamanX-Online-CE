using System;
using System.Collections.Generic;

namespace MMXOnline;

public class FreezeCrackerV : Weapon {
	public static FreezeCrackerV netWeapon = new();

	public FreezeCrackerV() : base() {
		displayName = "FREEZE CRACKER";
		index = (int)VAVA2WeaponID.FreezeCrackerV;
		killFeedIndex = 0;

		fireRate = 45;
		maxAmmo = 20;
		ammo = maxAmmo;

	}

	public override void vileShoot(WeaponIds weaponInput, Vile vile) {
		
			Point shootPos = vile.getShootPos();
			int xDir = vile.getShootXDir();
			Player player = vile.player;
			int input = player.input.getYDir(player);

			new FreezeCrackerVProj(vile, shootPos, xDir, player.getNextActorNetId(), 0, input);
			vile.playSound("buster2", sendRpc: true);
		
	}
}

public class FreezeCrackerVProj : Projectile {

	public int type;
	int input;
	public float sparkleTime = 0;
	Anim? sparkle;
	float projSpeed = 300;
	Actor ownChr = null!;


	public FreezeCrackerVProj(
		Actor owner, Point pos, int xDir, ushort? netProjId, 
		int type, int input = 0, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "freeze_cracker_start", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.FreezeCrackerV;
		maxTime = 0.6f;
		fadeSprite = "freeze_cracker_start";
		this.type = type;
		this.input = input;
		damager.damage = 2;
		damager.hitCooldown = 6;
		ownChr = owner;

		if (type == 1) {

			canBeLocal = false;
			changeSprite("freeze_cracker_proj", false);
			reflectable = true;
			int dir = input * 32;
			float ang = xDir > 0 ? dir : -dir + 128;
			base.vel = Point.createFromByteAngle(ang) * projSpeed;
		}

		if (rpc) {
			byte[] extraArgs = new byte[] { (byte)type };

			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
		}
	}


	public override void update() {
		base.update();

		sparkleTime += Global.speedMul;
		if (sparkleTime >= 3) {
			sparkleTime = 0;

			if (ownedByLocalPlayer) sparkle = new Anim(
				pos, "freeze_cracker_sparkles", 1, damager.owner.getNextActorNetId(), true
			) { useGravity = true, gravityModifier = 0.5f };

		}

		if (type == 0 && isAnimOver() && ownedByLocalPlayer) {
			time = 0;
			new FreezeCrackerVProj(
				ownChr, pos, xDir, damager.owner.getNextActorNetId(true), 1, input, rpc: true
			);
			destroySelfNoEffect();
		}
	}

	public void onHit() {
		if (!ownedByLocalPlayer) {
			destroySelf();
			//destroySelfNoEffect(disableRpc: true, true);
			return;
		}

		if (type == 1) {
			playSound("ding", true, true);
			destroySelf();

			for (int i = 0; i < 6; i++) {
				new FreezeCrackerVPieceProj(
					ownChr, pos, xDir, damager.owner.getNextActorNetId(true), i, rpc: true);
			}
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		//if (!ownedByLocalPlayer) return;
		if (other.gameObject.collider == null) return;
		if (!other.gameObject.collider.isClimbable) return;
		onHit();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		destroySelf();
	}



	public static Projectile rpcInvoke(ProjParameters arg) {
		return new FreezeCrackerVProj(
			arg.owner, arg.pos, arg.xDir, arg.netId, 
			arg.extraData[0], altPlayer: arg.player
		);
	}
}


public class FreezeCrackerVPieceProj : Projectile {


	public FreezeCrackerVPieceProj(
		Actor owner, Point pos, int xDir, ushort? netProjId, 
		int type, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "freeze_cracker_piece", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.FreezeCrackerVPiece;
		maxTime = 0.4f;
		reflectable = true;
		damager.damage = 2;
		damager.hitCooldown = 15;

		base.vel = Point.createFromByteAngle(type * 42.5f) * 240;

		if (rpc) {
			byte[] extraArgs = new byte[] { (byte)type };

			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new FreezeCrackerVPieceProj(
			arg.owner, arg.pos, arg.xDir, arg.netId, 
			arg.extraData[0], altPlayer: arg.player
		);
	}
}
