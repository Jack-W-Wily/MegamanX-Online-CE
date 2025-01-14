using System;

namespace MMXOnline;

public class BlueBulletProj : Projectile {
	public BlueBulletProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "axl_bullet_blue", netProjId, player
	) {
		fadeSprite = "axl_bullet_fade";
		projId = (int)ProjIds.BlueBullet;
		weapon = AxlBulletWC.netWeapon;
		damager.damage = 1;
		damager.flinch = Global.miniFlinch;
		reflectable = true;
		destroyOnHitWall = true;

		vel = Point.createFromByteAngle(byteAngle) * 60 * 6;
		this.byteAngle = byteAngle;
		maxTime = 16f / 60f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public static BlueBulletProj newWithDir(
		Actor owner, Point pos,
		int xDir, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) {
		return new BlueBulletProj(owner, pos, (xDir < 0 ? 128 : 0), netProjId, sendRpc, player);
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new BlueBulletProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
	}
}

public class AxlMeleeBullet : Projectile {
	public AxlMeleeBullet(
		Actor owner, Point pos,
		int xDir, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "axl_meleeshot_proj", netProjId, player
	) {
		weapon = AxlBulletWC.netWeapon;
		projId = (int)ProjIds.AxlMeleeBullet;
		this.xDir = xDir;

		damager.damage = 1;
		damager.hitCooldown = 30;
		setIndestructableProperties();
		reflectable = false;
		destroyOnHit = false;
		maxTime = 0.1f;
		isMelee = true;
		
		if (sendRpc) {
			rpcCreate(pos, owningActor, ownerPlayer, netProjId, xDir);
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (owningActor != null) {
			incPos(owner.character.deltaPos);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new AxlMeleeBullet(
			args.owner, args.pos, args.xDir, args.netId, player: args.player
		);
	}
}
