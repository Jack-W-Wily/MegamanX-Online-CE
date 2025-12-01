using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



// Adding a Weapon
public class KRMelee : Weapon {
	public static KRMelee netWeapon = new();

	public KRMelee() : base() {
		fireRate = 45;// frames
		index = (int)WeaponIds.KRMelee;// Make sure to add to "WeaponIds" on Enums.cs for it to work
		killFeedIndex = 167;//what sprite will appear in the kill index
	}
}




// this is a projectile 
public class OrochinagiChargedProj : Projectile {
	public OrochinagiChargedProj(
		Point pos, int xDir, bool isOD, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "kr_orochinagi_proj", netId, player
	) {
		weapon = FireWave.netWeapon; // weapon tied to it will also be compatible with weakness system
		damager.damage = 3;
		damager.flinch = 10;
		//damager.hitcooldown = (inset value); for cooldowns, this one has none since it's autodestroyed on hit
		vel = new Point(150 * xDir, 0);
		fadeOnAutoDestroy = true;
		// you can add pretty much every thing you see on Projectile.CS's bools in this area
		fadeSprite = "kr_orochinagi_proj_fade";
		reflectable = false;
		projId = (int)ProjIds.OrochinagiProj;
		maxTime = 0.5f;
		if (isOD) {
			damager.damage = 4;
			damager.flinch = 30;
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir, isOD ? (byte)1 : (byte)0);
		}
	}



	// To add damage effects
	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (!damagable.isPlayableDamagable()) { return; }
		if (damagable is not Actor actor || !actor.ownedByLocalPlayer) {
			return;
		}
	
		if (damagable is Character chr) {
			chr.burnTime = 2; // this is where the burn DOT effect enters
		}
	
	}


	// for Online display, make sure you add this to RPCCreatePojEX.cs
	/*
	public static Dictionary<int, ProjCreate> functs = new Dictionary<int, ProjCreate> {
	//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	{ (int)ProjIds.OrochinagiProj, OrochinagiChargedProj.rpcInvoke },
	//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	*/
	public static Projectile rpcInvoke(ProjParameters args) {
		return new OrochinagiChargedProj(
			args.pos, args.xDir, args.extraData[0] == 1, args.owner, args.player, args.netId
		);
	}
}


public class YamiBaraiProj : Projectile {
	public YamiBaraiProj(
		Point pos, int xDir, bool isOD, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "kr_shiki_yami_barai_proj", netId, player
	) {
		weapon = FireWave.netWeapon;
		damager.damage = 1;
		damager.flinch = 16;
		vel = new Point(250 * xDir, 0);
		fadeOnAutoDestroy = true;
		fadeSprite = "explosion";
		reflectable = true;
		projId = (int)ProjIds.YamiBaraiProj;
		maxTime = 0.5f;
		if (isOD) {
			damager.damage = 2;
			genericShader = player.zeroPaletteShader;
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir, isOD ? (byte)1 : (byte)0);
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (!damagable.isPlayableDamagable()) { return; }
		if (damagable is not Actor actor || !actor.ownedByLocalPlayer) {
			return;
		}
	
		if (damagable is Character chr) {
			chr.burnTime = 1; 
		}
	
	}


	public static Projectile rpcInvoke(ProjParameters args) {
		return new OrochinagiChargedProj(
			args.pos, args.xDir, args.extraData[0] == 1, args.owner, args.player, args.netId
		);
	}
}



