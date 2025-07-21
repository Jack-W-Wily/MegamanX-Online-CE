using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;


public class ZainSaberProj : Projectile {
	public ZainSaberProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 300, 6, player, "zain_projslash", 60, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		fadeSprite = "zsaber_shot_fade";
		reflectable = true;
		projId = (int)ProjIds.ZainSaberProj;
	
		
	
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		
		if (time > 0.5) {
			destroySelf(fadeSprite);
		}
	}
}
