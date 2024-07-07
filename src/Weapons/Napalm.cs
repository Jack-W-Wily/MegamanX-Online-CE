using System;
using System.Collections.Generic;

namespace MMXOnline;

public enum NapalmType {
	NoneBall = -1,
	RumblingBang,
	FireGrenade,
	SplashHit,
	NoneFlamethrower,
}

public class Napalm : Weapon {
	public float vileAmmoUsage;
	public Napalm(NapalmType napalmType) : base() {
		index = (int)WeaponIds.Napalm;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 30;
		type = (int)napalmType;

		if (napalmType == NapalmType.NoneBall) {
			displayName = "None(BALL)";
			description = new string[] { "Do not equip a Napalm.", "BALL will be used instead." };
			killFeedIndex = 126;
		} else if (napalmType == NapalmType.NoneFlamethrower) {
			displayName = "None(FLAMETHROWER)";
			description = new string[] { "Do not equip a Napalm.", "FLAMETHROWER will be used instead." };
			killFeedIndex = 126;
		} else if (napalmType == NapalmType.RumblingBang) {
			displayName = "Rumbling Bang";
			vileAmmoUsage = 8;
			rateOfFire = 2f;
			description = new string[] { "This napalm sports a wide horizontal", "range but cannot attack upward." };
			vileWeight = 3;
		}
		if (napalmType == NapalmType.FireGrenade) {
			displayName = "Flame Round";
			vileAmmoUsage = 16;
			rateOfFire = 4f;
			description = new string[] { "This napalm travels along the", "ground, laying a path of fire." };
			killFeedIndex = 54;
			vileWeight = 3;
		}
		if (napalmType == NapalmType.SplashHit) {
			displayName = "Splash Hit";
			vileAmmoUsage = 16;
			rateOfFire = 3f;
			description = new string[] { "This napalm can attack foes above,", "but has a narrow horizontal range." };
			killFeedIndex = 79;
			vileWeight = 3;
		}
	}

	public override void vileShoot(WeaponIds weaponInput, Vile vile) {
		if (type == (int)NapalmType.NoneBall || type == (int)NapalmType.NoneFlamethrower) return;
		if (shootTime == 0) {
			if (weaponInput == WeaponIds.Napalm) {
				if (vile.tryUseVileAmmo(vileAmmoUsage)) {
					vile.changeState(new NapalmAttack(NapalmAttackType.Napalm), true);
				}
			} else if (weaponInput == WeaponIds.VileFlamethrower) {
				var ground = Global.level.raycast(vile.pos, vile.pos.addxy(0, 25), new List<Type>() { typeof(Wall) });
				if (ground == null) {
					if (vile.tryUseVileAmmo(vileAmmoUsage)) {
						vile.setVileShootTime(this);
						vile.changeState(new AirBombAttack(true), true);
					}
				}
			} else if (weaponInput == WeaponIds.VileBomb) {
				var ground = Global.level.raycast(vile.pos, vile.pos.addxy(0, 25), new List<Type>() { typeof(Wall) });
				if (ground == null) {
					if (vile.player.vileAmmo >= vileAmmoUsage) {
						vile.setVileShootTime(this);
						vile.changeState(new AirBombAttack(true), true);
					}
				}
			}
		}
	}
}

public class NapalmGrenadeProj : Projectile {
	bool exploded;
	public NapalmGrenadeProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 150, 2, player, "napalm_grenade", 0, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.NapalmGrenade;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		shouldShieldBlock = false;
	}

	public override void update() {
		base.update();
		if (grounded) {
			explode();
		}
	}

	public override void onHitWall(CollideData other) {
		xDir *= -1;
		explode();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (ownedByLocalPlayer) explode();
	}

	public void explode() {
		if (exploded) return;
		exploded = true;
		if (ownedByLocalPlayer) {
			for (int i = -3; i <= 3; i++) {
				new NapalmPartProj(weapon, pos.addxy(0, 0), 1, owner, owner.getNextActorNetId(), false, i * 10, rpc: true);
				new NapalmPartProj(weapon, pos.addxy(0, 0), 1, owner, owner.getNextActorNetId(), true, i * 10, rpc: true);
			}
		}
		destroySelf();
	}
}






public class NapalmPartProj : Projectile {
	int times;
	float xDist;
	float maxXDist;
	float napalmTime;
	float timeOffset;
	float napalmPeriod = 0.5f;
	public NapalmPartProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool isTimeOffset, float xDist, bool rpc = false) :
		base(weapon, pos, xDir, 0, 1, player, "napalm_part", 0, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.Napalm;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir, isTimeOffset ? (byte)1 : (byte)0);
		}
		useGravity = true;
		collider.wallOnly = true;
		destroyOnHit = false;
		shouldShieldBlock = false;
		gravityModifier = 0.25f;
	//	frameIndex = Helpers.randomRange(0, sprite.frames.Count - 1);
		if (isTimeOffset) {
			timeOffset = napalmPeriod * 0.5f;
		}
		maxXDist = xDist;
		visible = false;
	}

	public override void update() {
		base.update();

		if (isUnderwater()) {
			destroySelf(disableRpc: true);
			return;
		}

		if (time < timeOffset) return;
		else visible = true;

		napalmTime += Global.spf;

		if (!Options.main.lowQualityParticles()) {
			alpha = 2 * (napalmPeriod - napalmTime);
			xScale = 1 + (napalmTime * 2);
			yScale = 1 + (napalmTime * 2);
		}

		if (xDist < MathF.Abs(maxXDist)) {
			xDist += MathF.Abs(maxXDist * 0.25f);
			move(new Point(maxXDist * 0.25f, 0), useDeltaTime: false);
		}

		if (napalmTime > napalmPeriod) {
			napalmTime = 0;
			times++;
			if (times >= 8) {
				destroySelf(disableRpc: false);
			}
		}
	}
}

public enum NapalmAttackType {
	Napalm,
	Ball,
	Flamethrower,
}

public class NapalmAttack : CharState {
	bool shot;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;

	public NapalmAttack(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		return napalmAttackType == NapalmAttackType.Flamethrower ? "crouch_flamethrower" : "crouch_nade";
	}

	public override void update() {
		base.update();

		if (napalmAttackType == NapalmAttackType.Napalm) {
			if (!shot && character.sprite.frameIndex == 2) {
				shot = true;
				vile.setVileShootTime(player.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				Projectile proj;
				if (napalmAttackType == NapalmAttackType.Napalm) {
					if (player.vileNapalmWeapon.type == (int)NapalmType.RumblingBang) {
						proj = new NapalmGrenadeProj(player.vileNapalmWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
					} else if (player.vileNapalmWeapon.type == (int)NapalmType.FireGrenade) {
						proj = new MK2NapalmGrenadeProj(player.vileNapalmWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
					} else if (player.vileNapalmWeapon.type == (int)NapalmType.SplashHit) {
						proj = new SplashHitGrenadeProj(player.vileNapalmWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
					}
				}
			}
		} else if (napalmAttackType == NapalmAttackType.Ball) {
			if (player.vileBallWeapon.type == (int)VileBallType.ExplosiveRound) {
				if (shootCount < 3 && character.sprite.frameIndex == 2) {
					if (!vile.tryUseVileAmmo(player.vileBallWeapon.vileAmmoUsage)) {
						character.changeState(new Crouch(""), true);
						return;
					}
					shootCount++;
					vile.setVileShootTime(player.vileBallWeapon);
					var poi = character.sprite.getCurrentFrame().POIs[0];
					poi.x *= character.xDir;
					Projectile proj = new VileBombProj(player.vileBallWeapon, character.pos.add(poi), character.xDir, player, 0, character.player.getNextActorNetId(), rpc: true);
					proj.vel = new Point(character.xDir * 150, -200);
					proj.maxTime = 0.6f;
					character.sprite.frameIndex = 0;
				}
			} else if (player.vileBallWeapon.type == (int)VileBallType.SpreadShot) {
				shootTime += Global.spf;
				var poi = character.getFirstPOI();
				if (shootTime > 0.06f && poi != null && shootCount <= 4) {
					if (!vile.tryUseVileAmmo(player.vileBallWeapon.vileAmmoUsage)) {
						character.changeState(new Crouch(""), true);
						return;
					}
					shootTime = 0;
					character.sprite.frameIndex = 1;
					Point shootDir = Point.createFromAngle(-45).times(150);
					if (shootCount == 1) shootDir = Point.createFromAngle(-22.5f).times(150);
					if (shootCount == 2) shootDir = Point.createFromAngle(0).times(150);
					if (shootCount == 3) shootDir = Point.createFromAngle(22.5f).times(150);
					if (shootCount == 4) shootDir = Point.createFromAngle(45f).times(150);
					new StunShotProj(player.vileBallWeapon, poi.Value, character.xDir, 1, character.player, character.player.getNextActorNetId(), new Point(shootDir.x * character.xDir, shootDir.y), rpc: true);
					shootCount++;
				}
			} else if (player.vileBallWeapon.type == (int)VileBallType.PeaceOutRoller) {
				if (!shot && character.sprite.frameIndex == 2) {
					if (!vile.tryUseVileAmmo(player.vileBallWeapon.vileAmmoUsage)) {
						character.changeState(new Crouch(""), true);
						return;
					}
					shot = true;
					vile.setVileShootTime(player.vileBallWeapon);
					var poi = character.sprite.getCurrentFrame().POIs[0];
					poi.x *= character.xDir;
					Projectile proj = new PeaceOutRollerProj(player.vileBallWeapon, character.pos.add(poi), character.xDir, player, 0, character.player.getNextActorNetId(), rpc: true);
					proj.vel = new Point(character.xDir * 150, -200);
					proj.gravityModifier = 1;
				}
			}
		} else {
			shootTime += Global.spf;
			var poi = character.getFirstPOI();
			if (shootTime > 0.06f && poi != null) {
				if (!vile.tryUseVileAmmo(2)) {
					character.changeState(new Crouch(""), true);
					return;
				}
				shootTime = 0;
				character.playSound("flamethrower");
				new FlamethrowerProj(player.vileFlamethrowerWeapon, poi.Value, character.xDir, true, player, player.getNextActorNetId(), sendRpc: true);
			}

			if (character.loopCount > 4) {
				character.changeState(new Crouch(""), true);
				return;
			}
		}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}
}



public class RunblingBangAttack : CharState {
	bool shot;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;

	public RunblingBangAttack(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		return napalmAttackType == NapalmAttackType.Flamethrower ? "crouch_flamethrower" : "crouch_nade";
	}

	public override void update() {
		base.update();

		if (napalmAttackType == NapalmAttackType.Napalm) {
			if (!shot && character.sprite.frameIndex == 2) {
				shot = true;
				vile.setVileShootTime(player.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				Projectile proj;
				proj = new NapalmGrenadeProj(player.vileNapalmWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
				
			}
			
		} 
		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}


public class FireNadeAttack : CharState {
	bool shot;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;
	bool jumpedYet;
	float timeInWall;
	bool isUnderwater;
	Anim anim;
	float projTime;

	public FireNadeAttack(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		return napalmAttackType == NapalmAttackType.Flamethrower ? "air_bomb_attack" : "air_bomb_attack";
	}

	public override void update() {
		base.update();
		if (character.sprite.frameIndex >= 0 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			character.vel.y = -character.getJumpPower() * 1.5f;
			if (!isUnderwater) {
				character.playSound("FireNappalmMK2", sendRpc: true);
			}
		}
		if (character.sprite.frameIndex == 2 && character.currentFrame.POIs.Count > 0) {
			character.move(new Point(character.xDir * 150, 0));
		}
		if (napalmAttackType == NapalmAttackType.Napalm) {
			if (!shot && character.sprite.frameIndex == 2) {
				shot = true;
				vile.setVileShootTime(player.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				Projectile proj;
				proj = new MK2NapalmGrenadeProj(player.vileNapalmWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
				
			}
			if (character.loopCount > 0) {
				character.changeState(new Idle(), true);
				return;
			}
		} 
		if (stateTime >1f || character.isAnimOver()) {
			character.changeState(new Idle(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}


public class SplashHitAttack : CharState {
	bool shot;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;

	public SplashHitAttack(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
		airMove = true;
		airSprite = "air_bomb_attack";
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		return napalmAttackType == NapalmAttackType.Flamethrower ?  "crouch_nade" : "crouch_nade";
	}

	public override void update() {
		base.update();
		if (napalmAttackType == NapalmAttackType.Napalm) {
			if (!shot && character.sprite.frameIndex == 2) {
				shot = true;
				vile.setVileShootTime(player.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				Projectile proj;
					proj = new SplashHitGrenadeProj(player.vileNapalmWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
			
				
			}
		} 
		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (character.sprite.name.Contains("air")){
		character.useGravity = false;
		character.stopMoving();
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}

public class HoutenjinWeapon : Weapon {
	public HoutenjinWeapon(Player player) : base() {
		
		index = (int)WeaponIds.Houtenjin;
		killFeedIndex = 113;
	}
}




public class HoutenjinWeaponF : Weapon {
	public HoutenjinWeaponF(Player player) : base() {
		
		index = (int)WeaponIds.HoutenjinF;
		killFeedIndex = 113;
	}
}


public class HoutenjinStartState : CharState {
	public HoutenjinStartState() : base("houtenjin_prepare", "", "", "") {
		superArmor = true;
	}

	

	public override void update() {
		base.update();
		CrystalHunterCharged proj;
		if (stateTime < 0.1f){
		proj =  new CrystalHunterCharged(character.pos, 
		player, player.getNextActorNetId(), 
		player.ownedByLocalPlayer, 
		overrideTime: 0.25f, sendRpc: true);		
		}
		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (stateTime > 1) {
		Actor counterAttackTarget = null;
		counterAttackTarget =  Global.level.getClosestTarget(character.pos, player.alliance, true, aMaxDist: 250);;
		character.playSound("zeroParry", sendRpc: true);
		character.changeState(new HoutenjinState(counterAttackTarget), true);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.parryCooldown = character.maxParryCooldown;
	}


	

	public bool canParry(Actor damagingActor) {
		var proj = damagingActor as Projectile;
		if (proj == null) {
			return false;
		}
		return character.frameIndex == 0;
	}
}

public class HoutenjinState : CharState {
	bool shot;
	
	float shootTime;
	int shootCount;

	Actor counterAttackTarget;
	Point counterAttackPos;
	NapalmAttackType napalmAttackType;

	public HoutenjinState(Actor counterAttackTarget, string transitionSprite = "") :
		base(getSprite(), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite() {
		return"houtenjin";
	}

	public override void update() {
		base.update();

		if (counterAttackTarget != null) {
			character.turnToPos(counterAttackPos);
			float dist = character.pos.distanceTo(counterAttackPos);
			if (dist < 350) {
				if (character.frameIndex >= 1 && !once) {
					if (dist > 5) {
						var destPos = Point.lerp(character.pos, counterAttackPos, Global.spf * 5);
						character.changePos(destPos);
					}
				}
			}
		}
		else{
		character.slideVel = character.xDir * character.getDashSpeed();	
		}
		if (napalmAttackType == NapalmAttackType.Napalm) {
			if (!shot && character.sprite.frameIndex == 12) {
				shot = true;
				vile.setVileShootTime(player.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				Projectile proj;
					proj = new SplashHitProj(new Napalm(NapalmType.SplashHit)
				, character.pos, character.xDir,
				player, player.getNextActorNetId(), sendRpc: true
			);
				
			}
		} 
		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class DragonsWrath : CharState {
	bool shot;
	bool isGrounded = false;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;
	
	public DragonsWrath(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		//if (isGrounded){
		return "crouch_flamethrower";
		//}
		//return "flamethrower";
	}

	public override void update() {
		base.update();
			if (character.grounded){
			isGrounded = true;
			}


			if (!character.grounded){
			 character.useGravity = false;
			 character.changeSpriteFromName("flamethrower", false);
			}
			shootTime += Global.spf;
			var poi = character.getFirstPOI();
			if (shootTime > 0.06f && poi != null) {
				if (!vile.tryUseVileAmmo(2)) {
					character.changeState(new Crouch(""), true);
					return;
				}
				shootTime = 0;
				character.playSound("flamethrower");
				if (!character.grounded){
				new FlamethrowerProj(new VileFlamethrower(VileFlamethrowerType.DragonsWrath), poi.Value, character.xDir, false, player, player.getNextActorNetId(), sendRpc: true);
				} 
				else {	new FlamethrowerProj(new VileFlamethrower(VileFlamethrowerType.DragonsWrath), poi.Value, character.xDir, true, player, player.getNextActorNetId(), sendRpc: true);
				}
			}

			if (character.loopCount > 4) {
				character.changeState(new Crouch(""), true);
				return;
			}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class SeaDragonRageAttack : CharState {
	bool shot;
	bool isGrounded = false;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;
	
	public SeaDragonRageAttack(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		//if (isGrounded){
		return "crouch_flamethrower";
		//}
		//return "flamethrower";
	}

	public override void update() {
		base.update();
			if (character.grounded){
			isGrounded = true;
			}


			if (!character.grounded){
			 character.useGravity = false;
			 character.changeSpriteFromName("flamethrower", false);
			}
			shootTime += Global.spf;
			var poi = character.getFirstPOI();
			if (shootTime > 0.06f && poi != null) {
				if (!vile.tryUseVileAmmo(2)) {
					character.changeState(new Crouch(""), true);
					return;
				}
				shootTime = 0;
				character.playSound("flamethrower");
				if (!character.grounded){
				new FlamethrowerProj(new VileFlamethrower(VileFlamethrowerType.SeaDragonRage), poi.Value, character.xDir, false, player, player.getNextActorNetId(), sendRpc: true);
				} 
				else {	new FlamethrowerProj(new VileFlamethrower(VileFlamethrowerType.SeaDragonRage), poi.Value, character.xDir, true, player, player.getNextActorNetId(), sendRpc: true);
				}
			}

			if (character.loopCount > 4) {
				character.changeState(new Crouch(""), true);
				return;
			}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class WildHorseKick : CharState {
	bool shot;
	bool isGrounded = false;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;
	
	public WildHorseKick(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
		airMove = true;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		//if (isGrounded){
		return "idle_flamethrower";
		//}
		//return "flamethrower";
	}

	public override void update() {
		base.update();
			if (character.grounded){
			isGrounded = true;
			}
			shootTime += Global.spf;
			var poi = character.getFirstPOI();
			if (shootTime > 0.06f && poi != null) {
				if (!vile.tryUseVileAmmo(2)) {
					character.changeState(new Crouch(""), true);
					return;
				}
				shootTime = 0;
				character.playSound("flamethrower");
				if (player.input.isHeld(Control.WeaponLeft, player) && !character.grounded){
				new FlamethrowerProj(new VileFlamethrower(VileFlamethrowerType.WildHorseKick), poi.Value, character.xDir, false, player, player.getNextActorNetId(), sendRpc: true);
				} 
				else {	new FlamethrowerProj(new VileFlamethrower(VileFlamethrowerType.WildHorseKick), poi.Value, character.xDir, true, player, player.getNextActorNetId(), sendRpc: true);
				}
			}

			if (character.loopCount > 4) {
				character.changeState(new Crouch(""), true);
				return;
			}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class HotIcecleAttack : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public HotIcecleAttack(string transitionSprite = "")
		: base("hoticecle", "", "", transitionSprite)
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
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("dynamoslash", sendRpc: true);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		character.specialState = (int)SpecialStateIds.AxlRoll;	
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.specialState = (int)SpecialStateIds.None;	
    }
}



public class MaceBashAttack : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public MaceBashAttack(string transitionSprite = "")
		: base("macestomp", "", "", transitionSprite)
	{
	}

	public override void update()
	{
		if (stateTime < 0.8f){
			character.specialState = (int)SpecialStateIds.AxlRoll;	
		} 
		if (stateTime > 0.7f){	
			character.specialState = (int)SpecialStateIds.None;
		}
		
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

		if (character.grounded){
		character.slideVel = character.xDir * character.getDashSpeed();	
		}
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("dynamoslash", sendRpc: true);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.specialState = (int)SpecialStateIds.None;
    }
}




public class MK2NapalmGrenadeProj : Projectile {
	public MK2NapalmGrenadeProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 150, 1, player, "napalm_grenade2", 0, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.NapalmGrenade2;
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
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
		new MK2NapalmProj(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
	}
}

public class MK2NapalmProj : Projectile {
	float flameCreateTime = 1;
	public MK2NapalmProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 100, 1f, player, "napalm2_proj", 0, 1f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 2;
		projId = (int)ProjIds.Napalm2;
		useGravity = true;
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
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -50));
		}
	}


	public override void update() {
		base.update();
		if (ownedByLocalPlayer) {
			flameCreateTime += Global.spf;
			if (flameCreateTime > 0.1f) {
				flameCreateTime = 0;
				new MK2NapalmFlame(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
			}
		}

		var hit = Global.level.checkCollisionActor(this, vel.x * Global.spf, 0, null);
		if (hit?.gameObject is Wall && hit?.hitData?.normal != null && !(hit.hitData.normal.Value.isAngled())) {
			if (ownedByLocalPlayer) {
				new MK2NapalmWallProj(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
			}
			destroySelf();
		}
	}
}


public class MK2NapalmFlame : Projectile {
	public MK2NapalmFlame(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 0, 1, player, "napalm2_flame", 0, 1f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.Napalm2Flame;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		useGravity = true;
		collider.wallOnly = true;
		destroyOnHit = true;
		shouldShieldBlock = false;
		gravityModifier = 0.25f;
	}

	public override void update() {
		base.update();
		if (isUnderwater()) {
			destroySelf(disableRpc: true);
			return;
		}
		if (loopCount > 8) {
			destroySelf(disableRpc: true);
			return;
		}
	}
	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
	}
}

public class MK2NapalmWallProj : Projectile {
	public MK2NapalmWallProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 0, 2, player, "napalm2_wall", 0, 0.5f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 1f;
		projId = (int)ProjIds.Napalm2Wall;
		vel = new Point(0, -200);
		destroyOnHit = false;
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
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}


public class SplashHitGrenadeProj : Projectile {
	bool exploded;
	public SplashHitGrenadeProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 150, 2, player, "napalm_sh_grenade", 0, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.NapalmGrenadeSplashHit;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		shouldShieldBlock = false;
	}

	public override void update() {
		base.update();
		if (grounded) {
			explode();
		}
	}

	public override void onHitWall(CollideData other) {
		explode();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		explode();
	}

	public void explode() {
		if (exploded) return;
		exploded = true;
		if (ownedByLocalPlayer) {
			var hit = Global.level.raycast(pos.addxy(0, -10), pos.addxy(0, 100), new List<Type>() { typeof(Wall) });
			new SplashHitProj(
				weapon, hit?.getHitPointSafe() ?? pos, xDir,
				owner, owner.getNextActorNetId(), sendRpc: true
			);
		}
		destroySelf();
	}
}

public class SplashHitProj : Projectile {
	Player player;
	public SplashHitProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 1, player, "napalm_sh_proj", 0, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.NapalmSplashHit;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		maxTime = 0.7f;
		this.player = player;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
	}

	public override bool shouldDealDamage(IDamagable damagable) {
		if (damagable is Actor actor && MathF.Abs(pos.x - actor.pos.x) > 40) {
			return false;
		}
		return true;
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -600));
		}
	}
}
