using System;
using System.Linq;

namespace MMXOnline;

public class EarlyAxlProj : Projectile {
	public EarlyAxlProj(
		Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		ZeroBuster.netWeapon, pos, xDir, 350, 1, player, "early_axl_bullet_proj",
		Global.defFlinch, 0, netProjId, player.ownedByLocalPlayer
	) {
	//	fadeOnAutoDestroy = true;
		fadeSprite = "early_axl_bullet_fade";
		reflectable = true;
		projId = (int)ProjIds.ZBuster3;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}
}
public class ESpiralMagnumProj : Projectile {
	
	public Anim exhaust;
	public ESpiralMagnumProj(
		Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		ZeroBuster.netWeapon, pos, xDir, 350, 2, player, "early_axl_magnum_proj",
		Global.defFlinch, 0.1f, netProjId, player.ownedByLocalPlayer
	) {
		destroyOnHit = false;
		maxTime = 0.2f;
	//	fadeOnAutoDestroy = true;
		//fadeSprite = "early_axl_bullet_fade";
		reflectable = true;
		projId = (int)ProjIds.ZBuster3;
		exhaust = new Anim(pos, "early_axl_magnum_spiral", xDir, null, true);
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}
		public override void update() {
		base.update();
		exhaust.pos = pos;
		exhaust.xDir = xDir;
	}
	/*public override void onDestroy() {
		base.onDestroy();
		exhaust?.destroySelf();
	}*/
}

#region anims
public class FlashAnim : Anim {
	//int type;
	public FlashAnim(Point pos, int type, ushort? netId = null, bool sendRpc = false, bool ownedByLocalPlayer = true) :
		base(pos, "early_axl_bullet_flash", 1, netId, false, sendRpc, ownedByLocalPlayer) {
		if(type == 1){
			changeSprite("early_axl_bullet_flash2", true);
		}if(type == 2){
			changeSprite("early_axl_bullet_flash3", true);
		}
	}

	public override void update() {
		base.update();
		if (isAnimOver()) {
			destroySelf();
		}
	}}
	
public class EarlyAxlSpiralMagnumShell : Anim {
	public int bounces;
	float angularVel = 0;
	bool stopped;
	float bounceCooldown;
	float timeNoYVel;
	public EarlyAxlSpiralMagnumShell(Point pos, int xDir, ushort? netId = null, bool sendRpc = false, bool ownedByLocalPlayer = true) :
		base(pos, "early_axl_magnum_shell", 1, netId, false, sendRpc, ownedByLocalPlayer) {
		vel = new Point(xDir * 75, -150);
		collider.wallOnly = true;
		useGravity = true;
		angle = 0;
		if (xDir == -1) angularVel = -300;
		else angularVel = 300;
		ttl = 4;
		
	}

	public override void update() {
		base.update();
		Helpers.decrementTime(ref bounceCooldown);
		if (MathF.Abs(vel.y) < 1) {
			timeNoYVel += Global.spf;
			if (timeNoYVel > 0.15f) {
				vel = new Point();
				stopped = true;
			}
		}
		if (!stopped && angle != null) {
			angle += angularVel * Global.spf;
			angle = Helpers.to360(angle.Value);
		} else {
			angle = 0;
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (other.gameObject is not Wall) return;
		if (stopped) return;
		if (MathF.Abs(vel.y) < 1) {
			playSound("dingX2", sendRpc: true);
			vel = new Point();
			stopped = true;
			return;
		}
		if (bounces > 0 && !stopped) {
			vel = new Point();
			stopped = true;
			return;
		}
		if (bounceCooldown > 0) return;

		bounces++;
		bounceCooldown = 0.5f;
		var normal = other.hitData.normal ?? new Point(0, -1);

		if (normal.isSideways()) {
			vel.x *= -0.5f;
			incPos(new Point(5 * MathF.Sign(vel.x), 0));
		} else {
			vel.y *= -0.5f;
			if (vel.y < -300) vel.y = -300;
			incPos(new Point(0, 5 * MathF.Sign(vel.y)));
		}
		playSound("dingX2", sendRpc: true);
	}
}
	
#endregion