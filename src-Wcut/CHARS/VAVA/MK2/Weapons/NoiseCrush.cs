using System;
using System.Collections.Generic;

namespace MMXOnline;

public class NoiseCrushV : Weapon {
	public static NoiseCrushV netWeapon = new();

	public NoiseCrushV() : base() {
		displayName = "NOISE CRUSH";

		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 0;
		maxAmmo = 14;
		ammo = maxAmmo;


	}



	public override float getAmmoUsage(int chargeLevel) {
		return 0;
	}

	public override void vileShootOld(WeaponIds weaponInput, Vile vile) {
		{
			Point shootPos = vile.getShootPos();
			int xDir = vile.getShootXDir();
			Player player = vile.player;


			if (vile.getChargeLevel() >= 3) {
				vile.playSound("noise_crush_charged");
				new NoiseCrushVChargedProj(vile, shootPos, xDir, 0, player.getNextActorNetId(), true);
				new NoiseCrushVChargedProj(vile, shootPos.addxy(6 * -xDir, 0), xDir, 0, player.getNextActorNetId(), true);
				new NoiseCrushVChargedProj(vile, shootPos.addxy(12 * -xDir, 0), xDir, 1, player.getNextActorNetId(), true);
				new NoiseCrushVChargedProj(vile, shootPos.addxy(18 * -xDir, 0), xDir, 2, player.getNextActorNetId(), true);
				new NoiseCrushVChargedProj(vile, shootPos.addxy(24 * -xDir, 0), xDir, 3, player.getNextActorNetId(), true);
				vile.hasChargedNoiseCrushV = false;
				vile.NoiseCrushVAnimTime = 0;
			} else {
				new NoiseCrushVProj(vile, shootPos, xDir, 0, player.getNextActorNetId(), true, true);
				new NoiseCrushVProj(vile, shootPos.addxy(4 * -xDir, 0), xDir, 0, player.getNextActorNetId(true), rpc: true);
				new NoiseCrushVProj(vile, shootPos.addxy(8 * -xDir, 0), xDir, 1, player.getNextActorNetId(true), rpc: true);
				new NoiseCrushVProj(vile, shootPos.addxy(12 * -xDir, 0), xDir, 1, player.getNextActorNetId(true), rpc: true);
				new NoiseCrushVProj(vile, shootPos.addxy(16 * -xDir, 0), xDir, 2, player.getNextActorNetId(true), rpc: true);
				vile.playSound("noise_crush", sendRpc: true);
				addAmmo(-1, player);
			}
		}
	}
}


public class NoiseCrushVProj : Projectile {

	public int type;
	public int bounces = 0;
	public bool isMain;

	public NoiseCrushVProj(
		Actor owner, Point pos, int xDir, int type, ushort? netProjId,
		bool isMain = false, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "noise_crush_top", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.NoiseCrushV;
		maxTime = 0.75f;
		this.type = type;
		this.isMain = isMain;
		//improve fade sprite
		fadeSprite = "noise_crush_fade";
		fadeOnAutoDestroy = true;
		canBeLocal = false;

		vel.x = 240 * xDir;
		damager.damage = 1;
		damager.hitCooldown = 12;

		if (type == 1) changeSprite("noise_crush_middle", true);
		else if (type == 2) {
			changeSprite("noise_crush_bottom", true);
		}
		if (rpc) {
			byte[] extraArgs = new byte[] { (byte)type };

			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new NoiseCrushVProj(
			arg.owner, arg.pos, arg.xDir, arg.extraData[0], 
			arg.netId, altPlayer: arg.player
		);
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);

		if (bounces < 4) {

			if (other.isSideWallHit()) {
				vel.x *= -1;
				xDir *= -1;
				incPos(new Point(5 * MathF.Sign(vel.x), 0));
				bounces++;
				time = 0;
			}
		}
	}
}


	public class NoiseCrushVChargedProj : Projectile {

		public int type;

		public NoiseCrushVChargedProj(
			Actor owner, Point pos, int xDir, int type,
			ushort? netProjId, bool rpc = false, Player? altPlayer = null
		) : base(
			pos, xDir, owner, "noise_crush_charged_top", netProjId, altPlayer
		) {

			projId = (int)VAVA2ProjIds.NoiseCrushVCharged;
			maxTime = 1f;
			this.type = type;
			fadeSprite = "noise_crush_fade";

			vel.x = 240 * xDir;
			damager.damage = 3;
			damager.hitCooldown = 20;

			if (type == 1) changeSprite("noise_crush_charged_middle", true);
			else if (type == 2) changeSprite("noise_crush_charged_middle2", true);
			else if (type == 3) {
				changeSprite("noise_crush_charged_bottom", true);
			}

			if (rpc) {
				byte[] extraArgs = new byte[] { (byte)type };

				rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
			}
		}

		public static Projectile rpcInvoke(ProjParameters arg) {
			return new NoiseCrushVChargedProj(
				arg.owner, arg.pos, arg.xDir,
				arg.extraData[0], arg.netId, altPlayer: arg.player
			);
		}
	}

