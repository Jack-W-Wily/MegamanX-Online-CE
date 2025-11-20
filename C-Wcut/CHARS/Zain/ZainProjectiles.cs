using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;





public class ZainSaberProj : Projectile {

	float flameCreateTime = 1;
	
	public ZainSaberProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 300, 6, player, "zain_projslash", 60, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		fadeSprite = "zsaber_shot_fade";
		reflectable = true;
		projId = (int)ProjIds.ZainSaberProj;
		useGravity = true;
		hitSound = "kofhtsnd_superslash";
		collider.wallOnly = true;
		destroyOnHit = false;
		maxTime = 2;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;
		flameCreateTime += Global.spf;
		if (flameCreateTime > 0.1f) {
			flameCreateTime = 0;
			new Anim(pos, "torpedo_smoke", xDir, null, true);
		}
		var hit = Global.level.checkTerrainCollisionOnce(this, vel.x * Global.spf, 0, null);
		if (hit?.gameObject is Wall && hit?.hitData?.normal != null && !(hit.hitData.normal.Value.isAngled())) {
			new ZainPillar(new ElectricSpark(), pos, xDir,owner, owner.getNextActorNetId(), sendRpc: true);
			destroySelf();
		}
	}
	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
	}
}



public class ZainPillar : Projectile {
	Player player;
	public ZainPillar(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 2, player, "zain_pillar_proj", Global.superFlinch, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ZainPillar;
		destroyOnHit = false;
		maxTime = 5f;
		this.player = player;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
	}

	public override bool shouldDealDamage(IDamagable damagable) {
	
		return true;
	}

}


