namespace MMXOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



public class GBDKick : Weapon {
	public GBDKick() : base() {
		fireRate = 60;
		index = (int)WeaponIds.GBDKick;
		killFeedIndex = 92;
	}
}


public class EnfrenteMeuDisco : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	ShieldBoomerangProj proj;

	public EnfrenteMeuDisco(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
	{
	airMove = true;
	
	}

	public override void update()
	{
	

		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		
		if (proj == null && character.frameIndex >= 3 && character.ownedByLocalPlayer){
		character.playSound("spinningBlade", forcePlay: false, sendRpc: true);
		proj = new ShieldBoomerangProj(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	
		
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class EnfrenteMeuDisco2 : CharState {

	private bool shot;
	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	ShieldBoomerangProj2 proj;

	public EnfrenteMeuDisco2(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
	{
		airMove = true;
	
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}


		if (proj == null && character.frameIndex >= 3 && character.ownedByLocalPlayer){
		character.playSound("spinningBlade", forcePlay: false, sendRpc: true);
		proj = new ShieldBoomerangProj2(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}











public class ShieldBoomerang : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public ShieldBoomerang() : base() {
		index = (int)WeaponIds.ShieldBoomerang;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
			displayName = "Shield Boomerang";
			description = new string[] { "Shield Boomerang" };
			killFeedIndex = 126;
		
		}
}


public class ShieldBoomerangProj : Projectile {
	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.55f;
	public float turnSpeed = 300;
	public float maxAngleDist = 280;
	public float soundCooldown;

	public ShieldBoomerangProj(ShieldBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "meudisco_proj", 8, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ShieldBoomerang;
		destroyOnHit = false;
		
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
	
		if (other.gameObject is Pickup && pickup == null) {
			pickup = other.gameObject as Pickup;
			if (!pickup.ownedByLocalPlayer) {
				pickup.takeOwnership();
				RPC.clearOwnership.sendRpc(pickup.netId);
			}
		}

		var character = other.gameObject as Character;
		if (time > returnTime && character != null && character.player == damager.owner) {
			if (pickup != null) {
				pickup.changePos(character.getCenterPos());
			}
			destroySelf();
			character.player.vileAmmo = Helpers.clampMax(character.player.vileAmmo + 8, character.player.vileMaxAmmo);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		if (pickup != null) {
			pickup.useGravity = true;
			pickup.collider.isTrigger = false;
		}
	}

	

	public override void update() {
		base.update();
		if (!owner.isGBD)destroySelf();
		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
			playSound("cutter", sendRpc: true);
		}

		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} 
			 if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} else {
				destroySelf();
			}
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}




public class ShieldBoomerangProj2 : Projectile {
	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.15f;
	public float turnSpeed = 300;
	public float maxAngleDist = 180;
	public float soundCooldown;

	public ShieldBoomerangProj2(ShieldBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "meudisco2_proj", 8, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ShieldBoomerang2;
		destroyOnHit = false;
		
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		
		if (other.gameObject is Pickup && pickup == null) {
			pickup = other.gameObject as Pickup;
			if (!pickup.ownedByLocalPlayer) {
				pickup.takeOwnership();
				RPC.clearOwnership.sendRpc(pickup.netId);
			}
		}

		var character = other.gameObject as Character;
		if (time > returnTime && character != null && character.player == damager.owner) {
			if (pickup != null) {
				pickup.changePos(character.getCenterPos());
			}
			if (time > 1.5f) destroySelf();
			character.player.vileAmmo = Helpers.clampMax(character.player.vileAmmo + 8, character.player.vileMaxAmmo);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		if (pickup != null) {
			pickup.useGravity = true;
			pickup.collider.isTrigger = false;
		}
	}

	

	public override void update() {
		base.update();
		if (!owner.isGBD)destroySelf();
		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
			playSound("cutter", sendRpc: true);
		}

		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} 
			 if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} else {
				destroySelf();
			}
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}




public class GBDSpear1 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpear1(string transitionSprite = "")
		: base("spear_1", "", "", transitionSprite)
	{
		airMove = true;
	
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




public class GBDSpearUp : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpearUp(string transitionSprite = "")
		: base("spear_up", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

public class GBDSpearRising : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpearRising(string transitionSprite = "")
		: base("spear_rising", "", "", transitionSprite)
	{
	airMove = true;
	
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class GBDSpearSpin : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpearSpin(string transitionSprite = "")
		: base("spear_spin", "", "", transitionSprite)
	{
		airMove = true;
	
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		if (player.input.isPressed(Control.Shoot, player)&& character.sprite.frameIndex > 5){
		character.sprite.frameIndex = 4;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




