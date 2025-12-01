
using System;
using System.Collections.Generic;

namespace MMXOnline;




public class MetalBladeKnee : CharState {
	int bombNum;

	Vile vile = null!;

	public MetalBladeKnee(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;

			int input = player.input.getYDir(player);
				new MetalBladeProj(vile,  poi.Value, vile.xDir, player.getNextActorNetId(), 0, input);
				 vile.playSound("sparkShock", sendRpc: true);
		
			player.vileAmmo -= 15;

		
			character.playSound("FireNappalmMK2", forcePlay: false, sendRpc: true);

		}

		if (stateTime > 0.25f) {
			character.changeToIdleOrFall();
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class MetalBladeProj : Projectile {

	public int type;
	int input;

	float projSpeed = 300;
	Actor ownChr = null!;


	public MetalBladeProj(
		Actor owner, Point pos, int xDir, ushort? netProjId, 
		int type, int input = 0, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "vilemk5_metalblade_proj", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.MetalBladeV;
		maxTime = 0.6f;
		fadeSprite = "explosion";
		this.type = type;
		this.input = input;
		damager.damage = 2;
		damager.flinch = 20;
		damager.hitCooldown = 6;
		ownChr = owner;
		destroyOnHit = true;

		if (type == 1) {

			canBeLocal = false;
			changeSprite("vilemk5_metalblade_proj", false);
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

		

		if (type == 0 && ownedByLocalPlayer) {
			time = 0;
			new MetalBladeProj(
				ownChr, pos, xDir, damager.owner.getNextActorNetId(true), 1, input, rpc: true
			);
			destroySelfNoEffect();
		}
	}




	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		destroySelf();
	}



	public static Projectile rpcInvoke(ProjParameters arg) {
		return new MetalBladeProj(
			arg.owner, arg.pos, arg.xDir, arg.netId, 
			arg.extraData[0], altPlayer: arg.player
		);
	}
}


