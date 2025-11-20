using System;
using System.Collections.Generic;

namespace MMXOnline;

public class HighmaxHomingProj : Projectile, IDamagable {
	public Actor? target;
	public float smokeTime = 0;
	public float maxSpeed = 150;
	public HighmaxHomingProj(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, float? angle = null, bool rpc = false
	) : base(
		pos, xDir, owner, "highmax_spark_proj", netId, player
	) {
		weapon = LaunchOctopus.netWeapon;
		damager.damage = 1;
		damager.flinch = Global.halfFlinch;
		vel = new Point(150 * xDir, 0);
		fadeSprite = "explosion";
		fadeSound = "explosion";
		hitSound = "htsnd_common_x4";
		maxTime = 2f;
		projId = (int)ProjIds.LaunchOTorpedo;
		fadeOnAutoDestroy = true;
		reflectableFBurner = true;
		customAngleRendering = true;
		this.angle = this.xDir == -1 ? 180 : 0;
		if (angle != null) {
			this.angle = angle.Value + (this.xDir == -1 ? 180 : 0);
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		canBeLocal = false;
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new TorpedoProjChargedOcto(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}
	bool homing = true;
	public void reflect(float reflectAngle) {
		angle = reflectAngle;
		target = null;
	}
	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}
	public override void update() {
		base.update();
		if (ownedByLocalPlayer && homing) {
			if (target != null) {
				if (!Global.level.gameObjects.Contains(target)) {
					target = null;
				}
			}
			if (target != null) {
				if (time < 3f) {
					var dTo = pos.directionTo(target.getCenterPos()).normalize();
					var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
					destAngle = Helpers.to360(destAngle);
					if (angle != null) angle = Helpers.lerpAngle((float)angle, destAngle, Global.spf * 3);
				}
			}
			if (time >= 0.15) {
				target = Global.level.getClosestTarget(pos, damager.owner.alliance, true, aMaxDist: Global.screenW * 0.75f);
			} else if (time < 0.15) {
				//this.vel.x += this.xDir * Global.spf * 300;
			}
			if (angle != null) {
				vel.x = Helpers.cosd((float)angle) * maxSpeed;
				vel.y = Helpers.sind((float)angle) * maxSpeed;
			}
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.2) {
			smokeTime = 0;
			if (homing) new Anim(pos, "torpedo_smoke", 1, null, true);
		}
	}
	public override void renderFromAngle(float x, float y) {
		var angle = this.angle;
		var xDir = 1;
		var yDir = 1;
		var frameIndex = 0;
		float normAngle = 0;
		if (angle < 90) {
			xDir = 1;
			yDir = -1;
			normAngle = (float)angle;
		}
		if (angle >= 90 && angle < 180) {
			xDir = -1;
			yDir = -1;
			normAngle = 180 - (float)angle;
		} else if (angle >= 180 && angle < 270) {
			xDir = -1;
			yDir = 1;
			normAngle = (float)angle - 180;
		} else if (angle >= 270 && angle < 360) {
			xDir = 1;
			yDir = 1;
			normAngle = 360 - (float)angle;
		}

		if (normAngle < 18) frameIndex = 0;
		else if (normAngle >= 18 && normAngle < 36) frameIndex = 1;
		else if (normAngle >= 36 && normAngle < 54) frameIndex = 2;
		else if (normAngle >= 54 && normAngle < 72) frameIndex = 3;
		else if (normAngle >= 72 && normAngle < 90) frameIndex = 4;

		sprite.draw(frameIndex, pos.x + x, pos.y + y, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex, actor: this);
	}
	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		if (damage > 0) {
			destroySelf();
		}
	}
	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return damager.owner.alliance != damagerAlliance;
	}
	public bool isInvincible(Player attacker, int? projId) {
		return false;
	}
	public bool canBeHealed(int healerAlliance) {
		return false;
	}
	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) {
	}
	public bool isPlayableDamagable() {
		return false;
	}
}



public class HighmaxStunShot : Projectile {
	public HighmaxStunShot(
		Point pos, int xDir, float byteAngle,
		Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "highmax_spark_proj" , netId, player
	) {
		weapon = VileMK2StunShot.netWeapon;
		fadeSprite = "vile_stun_shot_fade";
		projId = (int)ProjIds.HighmaxStunShot;
		maxTime = 0.75f;
		destroyOnHit = true;
		damager.damage = 1;
		damager.hitCooldown = 9;
		hitSound = "htsnd_common_x4";
		byteAngle = byteAngle % 256;
		this.byteAngle = byteAngle;
		vel.x = 225 * Helpers.cosb(byteAngle);
		vel.y = 225 * Helpers.sinb(byteAngle);	
		if (rpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netId, byteAngle);
		}
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new HighmaxStunShot(
			args.pos, args.xDir, args.byteAngle, args.owner, args.player, args.netId
		);
	}
}




public class DesmumeProj1 : Projectile {
	public DesmumeProj1(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 150, 1, player, "highmax_punch_proj", 20, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DesmumeProj1;
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		hitSound = "kofhtsnd_lightning1";
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (grounded) {
			destroySelf();
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		Point destroyPos = other?.hitData?.hitPoint ?? pos;
		changePos(destroyPos);
		destroySelf();
	}

	public override void onDestroy() {
		if (!ownedByLocalPlayer) return;
		new DesmumeProj2(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
	}
}

public class DesmumeProj2 : Projectile {
	float flameCreateTime = 1;
	public DesmumeProj2(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 100, 1f, player, "highmax_punch_proj", 10, 1f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 2;
		projId = (int)ProjIds.DesmumeProj2;
		useGravity = false;
		collider.wallOnly = true;
		destroyOnHit = false;
		shouldShieldBlock = false;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}



	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -50));
		}
	}


	public override void update() {
		base.update();

		if (ownedByLocalPlayer && owner.input.isPressed(Control.Special1, owner) && owner.input.isHeld(Control.Down, owner)) {
			new DesmumeProj4(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
			destroySelf();
		}
		var hit = Global.level.checkCollisionActorOnce(this, vel.x * Global.spf, 0, null);
		if (hit?.gameObject is Wall && hit?.hitData?.normal != null && !(hit.hitData.normal.Value.isAngled()) 
		|| owner.input.isPressed(Control.Special1, owner) && owner.input.isHeld(Control.Up, owner)) {
			if (ownedByLocalPlayer) {
				new DesmumeProj3(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
			}
			destroySelf();
		}
	}
}


public class DesmumeProj3 : Projectile {
	public DesmumeProj3(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 0, 2, player, "highmax_punch_proj", 10, 0.5f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 1f;
		projId = (int)ProjIds.DesmumeProj3;
		vel = new Point(0, -200);
		destroyOnHit = false;
		hitSound = "kofhtsnd_lightning1";
		shouldShieldBlock = false;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (isUnderwater()) {
			destroySelf(disableRpc: true);
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}



public class DesmumeProj4 : Projectile {
	public DesmumeProj4(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 0, 2, player, "highmax_punch_proj", 10, 0.5f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 1f;
		projId = (int)ProjIds.DesmumeProj4;
		vel = new Point(0, 200);
		destroyOnHit = false;
		shouldShieldBlock = false;
		hitSound = "kofhtsnd_lightning1";
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (isUnderwater()) {
			destroySelf(disableRpc: true);
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}
