using System;
using System.Collections.Generic;

namespace MMXOnline;

public class ArrowSlash : Weapon {
	public static ArrowSlash netWeapon = new();

	public ArrowSlash() : base() {
	killFeedIndex = 0;
	
	}
}


public class ArrowSlashProj : Projectile {
	float timeMoving;

	public ArrowSlashProj(
		Actor owner, Point pos, int xDir, ushort? netProjId,
		bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "vilemk2_proj_slash", netProjId, altPlayer
	) {
		maxTime = 0.2f;
		projId = (int)VAVA2ProjIds.SlashClawV;
		fadeSprite = "vilemk2_proj_slash_2";
		damager.damage = 2;
		damager.flinch = 20;

		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new ArrowSlashProj(
			arg.owner, arg.pos, arg.xDir, arg.netId, altPlayer: arg.player
		);
	}


	public override void update() {
		base.update();


		timeMoving += Global.spf;
		base.vel.x = 240 * xDir;
		

		if (timeMoving >= Global.spf * 8 && base.vel.y > -120) base.vel.y -= 5;

		damager.damage = getDamageIncrease();

	}

	int getDamageIncrease() {
		int finalDamage;
		finalDamage = (int)(time / (20f / 60f)) + 1;
		if (finalDamage >= 3) damager.flinch = Global.halfFlinch;
		return finalDamage;
	}
}

public class SlashClawVState : CharState {

	bool fired;
	Vile vile = null!;

	public SlashClawVState() : base("punch_1", "", "", "") {
		airMove = true;
	}

	
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}

	public override void update() {
		base.update();

		if (!fired) {
			new ArrowSlashProj(vile, vile.getCenterPos(), vile.xDir, player.getNextActorNetId(), true);
			fired = true;
			vile.playSound("slash_claw", sendRpc: true);
		}

		if (vile.isAnimOver()) vile.changeToIdleOrFall();
	}
}




