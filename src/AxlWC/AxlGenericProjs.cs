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

		vel = Point.createFromByteAngle(byteAngle) * 350;
		this.byteAngle = byteAngle;
		maxTime = 16f / 60f;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		new BlueBulletProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
		return null!;
	}
}
