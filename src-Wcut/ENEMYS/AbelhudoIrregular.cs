using System;
using System.Collections.Generic;

namespace MMXOnline;

public class AbelhudoIrregular : Maverick {
	public VelGMeleeWeapon meleeWeapon = new();

	public AbelhudoIrregular(
		Player player, Point pos, int xDir,
		ushort? netId, bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		player, pos, xDir, netId, ownedByLocalPlayer
	) {
		stateCooldowns = new() {
			{ typeof(MShoot), new(45, true) }
		};
		canClimbWall = true;
		maxHealth = 6;
		awardWeaponId = WeaponIds.Buster;
		weakWeaponId = WeaponIds.ShotgunIce;
		weakMaverickWeaponId = WeaponIds.ChillPenguin;
		dismantleTypeDeath = false;
		shouldDealColisionDmg = true;
		noHurtKnockback = true;
		weapon = new Weapon(WeaponIds.VelGGeneric, 101);

		netActorCreateId = NetActorCreateId.AbelhudoIrregular;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		armorClass = ArmorClass.Light;
		height = 24;
	}

	public bool healthvalueOnce = false;


	
	public override void creditMaverickKill(Player killer, Player assister, int? weaponIndex) {
		if (killer != null && killer != player) {
			if (Helpers.randomRange(0,5) == 0) {
				new SmallHealthPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			} else if (Helpers.randomRange(0,5) == 1) {
				new LargeHealthPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			} else if (Helpers.randomRange(0,5) == 2) {
				new SmallAmmoPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}  else if (Helpers.randomRange(0,5) == 3) {
				new LargeAmmoPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}    else if (Helpers.randomRange(0,5) == 4) {
				
			}  	else {
			killer.awardCurrency();
			}

			if (Global.level.gameMode is Arena) {
				killer.addKill();
			}
		}
	}

	public float WheelerCooldown;



	public override void onDestroy() {
		base.onDestroy();
		if (pathBlocker != null) {
			pathBlocker.destroySelf();
		}
	}



	public override void update() {
		base.update();
		Helpers.decrementTime(ref WheelerCooldown);

		if (pathBlocker == null){
		pathBlocker = new SubBossPathBlocker(weapon, pos.addxy(100 * xDir, 0), 
		xDir, player, player.getNextActorNetId(), rpc: true);
		}

		if (WheelerCooldown == 0) {
			WheelerCooldown = 12;
			playSound("viralSigmaShoot", sendRpc: true);

			if (Helpers.randomRange(0, 3) == 0) {
				new ViralSigmaShootProj(new MechaniloidWeapon(player, MechaniloidType.Bird), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			} else if (Helpers.randomRange(0, 2) == 1) {
					new ViralSigmaShootProj(new MechaniloidWeapon(player, MechaniloidType.Tank), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			
			} else {
					new ViralSigmaShootProj(new MechaniloidWeapon(player, MechaniloidType.BallWalker), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			
			}
			new EnemyWheeler(new XBuster(), pos, xDir, player, player.getNextActorNetId(), true);

		}

		if (!healthvalueOnce) {
			healthvalueOnce = true;
			health = 32;
		}



		if (aiBehavior == MaverickAIBehavior.Control) {

		}
	}

	public override string getMaverickPrefix() {
		return "enemy_belhudo_irregular";
	}

	public override float getRunSpeed() {
		return 35f * getRunDebuffs();
	}

	public MaverickState getShootState(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
				new TorpedoProjMech2(pos, xDir, this, player, player.getNextActorNetId(), rpc: true);
				
		}, "torpedo");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}

	
	public MaverickState getShootState2(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
				new TorpedoProjMech2(pos, xDir, this, player, player.getNextActorNetId(), rpc: true);
				
		}, "torpedo");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}






	public override MaverickState[] aiAttackStates() {
		float enemyDist = 199;
		if (target != null) {
			enemyDist = MathF.Abs(target.pos.x - pos.x);
		}
	
		return [
			getShootState(false),
			getShootState2(false),
			getShootState2(true),
		];
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Pounce,
	}



	



	// This can be called from a RPC, so make sure there is no character conditionals here.
	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return  new GenericMeleeProj(
				meleeWeapon, pos, ProjIds.EnemySubBossColision, player,
				3, Global.superFlinch, addToLevel: true);
			
			
		
	}

}




public class TorpedoProjMech2 : Projectile, IDamagable {
	public Actor? target;
	public float smokeTime = 0;
	public float maxSpeed = 150;
	public TorpedoProjMech2(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, float? angle = null, bool rpc = false
	) : base(
		pos, xDir, owner, "frog_torpedo", netId, player	
	) {
		weapon = RideArmor.netWeapon;
		damager.damage = 2;
		vel = new Point(1 * xDir, 0);
		fadeSprite = "explosion";
		fadeSound = "explosion";
		maxTime = 2f;
		projId = (int)ProjIds.MechTorpedo2;
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
		return new TorpedoProjMech2(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}
	bool homing = true;
	public void checkLandFrogTorpedo() {
		
			useGravity = false;
			homing = true;
		
	}

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
		checkLandFrogTorpedo();
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
					angle = Helpers.lerpAngle(angle, destAngle, Global.spf * 3);
				}
			}
			if (time >= 0.15) {
				target = Global.level.getClosestTarget(pos, damager.owner.alliance, true, aMaxDist: Global.screenW * 0.75f);
			} else if (time < 0.15) {
				//this.vel.x += this.xDir * Global.spf * 300;
			}
			vel.x = Helpers.cosd(angle) * maxSpeed;
			vel.y = Helpers.sind(angle) * maxSpeed;
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
			normAngle = angle;
		}
		if (angle >= 90 && angle < 180) {
			xDir = -1;
			yDir = -1;
			normAngle = 180 - angle;
		} else if (angle >= 180 && angle < 270) {
			xDir = -1;
			yDir = 1;
			normAngle = angle - 180;
		} else if (angle >= 270 && angle < 360) {
			xDir = 1;
			yDir = 1;
			normAngle = 360 - angle;
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




public class EnemyWheeler : Projectile, IDamagable {
	int started;
	float soundTime;
	float startMaxTime = 5f;
	int hitCount;
	float hitCooldown;
	public EnemyWheeler(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 0, 3, player, "enemy_wheeler", Global.defFlinch, 1, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.EnemyWheeler;
		
		maxTime = startMaxTime;
		useGravity = true;
		collider.isTrigger = false;
		collider.wallOnly = true;
		damager.damage = 2;
		damager.flinch = 0;
		destroyOnHit = false;
		//xScale = 0.75f;
		//yScale = 0.75f;
		angle = 0;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (hitCooldown > 0 || started == 0) return;
		hitCooldown = 0.75f;
		speed *= 0.66f;
		damager.damage--;
		damager.flinch /= 2;
		hitCount++;
		if (damager.damage <= 1) {
			damager.damage = 1;
			damager.flinch = 0;
		}
		updateDamager();
		if (hitCount >= 3) {
			//destroySelf();
		}
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



	public override void update() {
		base.update();
		Helpers.decrementTime(ref hitCooldown);
		if (started == 0) {
			if (frameIndex > 0) frameIndex = 0;
			if (grounded) {
				started = 1;
				damager.damage = 3;
				if (isDefenderFavored()) damager.damage = 4;
				damager.flinch = Global.defFlinch;
				destroyOnHit = false;
				maxTime = startMaxTime;
				speed = 250;
				updateDamager();
			}
		}
		if (started == 1) {
			vel.x = xDir * speed;
			angle += xDir * speed * 3 * Global.spf;
			if (Global.level.checkTerrainCollisionOnce(this, 0, -1) == null) {
				var collideData = Global.level.checkTerrainCollisionOnce(this, xDir, 0, vel);
				if (collideData != null && collideData.hitData != null && !((Point)collideData.hitData.normal).isAngled()) {
					xDir *= -1;
					maxTime = startMaxTime;
					startMaxTime -= 0.2f;
				}
			}
			soundTime += Global.spf;
			if (soundTime > 0.15f) {
				soundTime = 0;
				//playSound("spinWheelLoop");
			}
		}
	}
}







public class SubBossPathBlocker : Projectile {

	Wall wall;
	public SubBossPathBlocker(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "subboss_path_blocker", 
		Global.halfFlinch, 1f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 9999f;
		projId = (int)ProjIds.EnemySubBossColision;
		setIndestructableProperties();
		fadeSprite = "explosion";
		fadeOnAutoDestroy = true;
		//isShield = true;
		//isReflectShield = true;
		collider.isClimbable = false;
		collider.wallOnly = false;
		isStatic = true;
		
		var rect = collider.shape.getRect().getPoints();
		wall = new Wall("Collision Shape", new List<Point>()
		{
				rect[0].add(new Point(0, 0)),
				rect[1].add(new Point(0, 0)),
				rect[2].add(new Point(0, 0)),
				rect[3].add(new Point(0, 0)),
			});

		Global.level.addGameObject(wall);
		
		if (player.character != null) zIndex = player.character.zIndex - 10;
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new HighmaxWallProj(
			LightningWeb.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void onDestroy() {
		base.onDestroy();
		if (wall != null) Global.level.removeGameObject(wall);
	}
}

