using System.Collections.Generic;

namespace MMXOnline;

public class RisingFire : Weapon {

	public static RisingFire netWeapon = new();

	public RisingFire() {
		shootSounds = new string[] { "ryuenjin", "ryuenjin", "ryuenjin", "ryuenjin", "ryuenjin" };
		fireRate = 45;
		index = (int)WeaponIds.RisingFire;
		weaponBarBaseIndex = (int)WeaponBarIndex.RisingFire;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = (int)SlotIndex.RFire;
		killFeedIndex = 184;
		type = index;
		displayName = "Dragon Install";
		weaknessIndex = (int)WeaponIds.DoubleCyclone;
		//hasCustomAnim = true;
		damage = "2+1-1/2+1-1";
		hitcooldown = "0.5";
		flinch = "0/13-26";
		flinchCD = hitcooldown;
		effect = "Burns upper enemies. \nC: Resets airdashes count.";
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		var player = character.player;
		if (chargeLevel > 2) {
			character.changeState(new RisingFireChargedState(), true);
		}
		else if (player.input.isHeld(Control.Up , player)) {
			character.changeState(new RisingFireState(), true);	
		} else if (player.input.isHeld(Control.Down , player)) {
			if (character.grounded) {
			character.vel.y = -character.getJumpPower() * 0.5f;
			}
			character.changeState(new DragoonDiveKick(), true);
		} else {
			character.changeState(new DragoonHadoukenX(), true);
		}
	}
}


public class RisingFireState : CharState {
	private bool fired;

	public RisingFireState()
		: base("risingfire")
	{
		superArmor = false;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		
		if (character.currentFrame.getBusterOffset() != null && !fired) {
			Point shootPos = character.getFirstPOI() ?? character.getShootPos();
			int xDir = character.getShootXDir();
			Player player = character.player;

			if (!character.isUnderwater()) {
				new RisingFireProj(new RisingFire(), shootPos, xDir, player, player.getNextActorNetId(), true);
			} else {
				new RisingFireWaterProj(new RisingFire(), shootPos, xDir, player, player.getNextActorNetId(), true);
			}
			
			fired = true;
		}

		if (character.isAnimOver()) character.changeToIdleOrFall();
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel = new Point();
		character.useGravity = false;
		
		bool air = !character.grounded || character.vel.y < 0;
		defaultSprite = sprite;
		landSprite = "risingfire";
		if (air) {
			sprite = "risingfire_air";
			defaultSprite = sprite;
		}
		character.changeSpriteFromName(sprite, true);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}

public class RisingFireProj : Projectile {
	public RisingFireProj(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 1.5f, player, "risingfire_proj", 
		Global.defFlinch, 0, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 4f;
		projId = (int)ProjIds.RisingFire;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		fadeSprite = "explosion";
		destroyOnHit = true;
		vel.y = -275;
		
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new RisingFireProj(
			RisingFire.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		
		var randomSpeedvalue = Helpers.randomRange(0, 10);
		if (isUnderwater()) destroySelf();

		if (time > 0.5f ) {
			angle = 130 * xDir;
			useGravity = true;
			vel.y += 5;
			vel.x += randomSpeedvalue * xDir;
		}
	}
}

public class RisingFireChargedState : CharState {
	private bool jumpedYet;
	private bool fired = false;

	private float timeInWall;

	private Projectile? proj;

    public RisingFireChargedState() : base("risingfire_charged") {
		superArmor = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		
		int xDir = character.xDir;
		Point pos = character.pos;
		Player player = character.player;

		if (character.sprite.frameIndex >= 1 && !jumpedYet) {
			jumpedYet = true;
			character.vel.y = -character.getJumpPower();
			character.useGravity = true;
		} 
		
		if (character.vel.y < 0) character.move(new Point(character.xDir * 165, 0f));

		if (character.currentFrame.getBusterOffset() != null) {
			Point poi = character.currentFrame.POIs[0];
			Point firePos = character.pos.addxy(poi.x * (float)character.xDir, poi.y);

			if (proj == null) {
				if (!character.isUnderwater()){
					proj = new RisingFireProjChargedStart(
						new RisingFire(), pos, xDir, player, player.getNextActorNetId(), true
					);
				} else {
					proj = new RisingFireProjChargedStart(
						new RisingFire(), pos, xDir, player, player.getNextActorNetId(), true
					);
				}
			//	proj.releasePlasma = player.hasPlasma();
			}
			else proj.changePos(firePos);
		}

		else if (character.sprite.frameIndex == 3 && proj != null) {
			proj.destroySelf();
			proj = null!;
		}
		
		CollideData? wallAbove = Global.level.checkTerrainCollisionOnce(character, 0, -10);
		
		if (wallAbove != null && wallAbove.gameObject is Wall) {
			timeInWall++;
			if (timeInWall > 6) {
				character.vel.y = 1;
				character.changeToIdleOrFall();
				return;
			}
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		if (character.frameIndex > 3 && !fired) {
			fired = true;
			releaseProj();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.dashedInAir = 0;
		character.stopMoving();
		character.useGravity = false;
		if (!character.grounded) {
			character.frameIndex = 2;
			character.frameTime = 2;
		}
	}
	

	public override void onExit(CharState? newState) {
		base.onExit(newState);
			character.useGravity = true;
		if (proj != null) {
			proj.destroySelf();
			if (!fired) releaseProj();
		} 
	}

	void releaseProj() {
		Projectile? rf;
		Point shootPos = character.getShootPos();
		int xDir = character.xDir;

		if (!character.isUnderwater()) {
			rf = new RisingFireProjCharged(
				new RisingFire(), shootPos, xDir, player, 
				player.getNextActorNetId(), rpc: true
			);
		} else {
			rf = new RisingFireWaterProjCharged(
				new RisingFire(), shootPos, xDir, player, 
				player.getNextActorNetId(), rpc: true
			);
		}
		
		
	}
}

public class RisingFireProjChargedStart : Projectile {
	public RisingFireProjChargedStart(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0f, 2f, player, "risingfire_proj_charged",
		Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.6f;
		projId = (int)ProjIds.RisingFireChargedStart;
		shouldShieldBlock = false;
		destroyOnHit = false;
		shouldVortexSuck = false;
		canBeLocal = false;
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new RisingFireProjChargedStart(
			RisingFire.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();

		if (isUnderwater()) destroySelf();
	}
}




public class DragoonHadoukenX : CharState {
	bool fired = false;


	public DragoonHadoukenX() : base("hadouken", "", "", "") {
	superArmor = true;
		spcCancel = true;
	}

	public override void update() {
		base.update();

      if (character.frameIndex >= 3 && !fired) {
			fired = true;

			Weapon weapon = new HadoukenWeapon(player);
	
			new FlameHadouken(character.pos.addxy(10,-18), character.xDir,character, player, player.getNextActorNetId(), true);
			
			character.playSound("speedBurner", sendRpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopCharge();
	}

	public override void onExit(CharState? newState) {
	base.onExit(newState);
	}
}






public class FlameHadouken : Projectile {
	float groundSpawnTime;
	float airSpawnTime;
	int groundSpawns;
	public FlameHadouken(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "haduken_flame", netId, player	
	) {
		weapon = SpeedBurner.netWeapon;
		damager.damage = 3;
		damager.flinch = 30;
		vel = new Point(275 * xDir, 0);
		maxTime = 0.6f;
		projId = (int)ProjIds.FlameHadouken;
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new FlameHadouken(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void update() {
		base.update();
		if (sprite.name == "speedburner_start") {
			if (isAnimOver()) {
				changeSprite("speedburner_proj", true);
			}
		}
		Helpers.decrementTime(ref groundSpawnTime);
		Helpers.decrementTime(ref airSpawnTime);

		if (airSpawnTime == 0) {
			var anim = new Anim(
				pos.addxy(Helpers.randomRange(-10, 10),
				Helpers.randomRange(-10, 10)), "speedburner_dust", xDir, null, true
			);
			anim.vel.x = 50 * xDir;
			anim.vel.y = 10;
			airSpawnTime = 0.05f;
		}
		if (!ownedByLocalPlayer) {
			return;
		}
		CollideData? hit = Global.level.raycast(pos, pos.addxy(0, 18), [typeof(Wall)]);

		if (hit != null && groundSpawnTime == 0) {
			Point spawnPos = pos.addxy((groundSpawns * -15 + 10) * xDir, 0);
			spawnPos.y = hit.hitData.hitPoint?.y - 1 ?? pos.y;
			new SpeedBurnerProjGround(
				spawnPos, xDir, this, damager.owner, damager.owner.getNextActorNetId(), rpc: true
			);
			groundSpawns++;

			groundSpawnTime = 0.075f;
		}
	}
}


public class RisingFireProjCharged : Projectile {
	public RisingFireProjCharged(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 2, player, "risingfire_proj_charged", 
		Global.defFlinch, 0.2f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.6f;
		damager.flinch = 30;
		isLiftProjectile = true;;
		isShield = true;
		projId = (int)ProjIds.RisingFireCharged;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		vel.y = -275;
		if (isUnderwater()) destroySelf();

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new RisingFireProjCharged(
			RisingFire.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (isUnderwater()) destroySelf();
	}
}

public class RisingFireWaterProj : Projectile {
	public RisingFireWaterProj(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player, "risingfire_proj_water", 
		0, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.6f;
		projId = (int)ProjIds.RisingFireUnderwater;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		vel.y = -275;
		if (!isUnderwater()) destroySelf();
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}
	
	public static Projectile rpcInvoke(ProjParameters arg) {
		return new RisingFireWaterProj(
			RisingFire.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (!isUnderwater()) destroySelf();
	}
}

public class RisingFireWaterProjCharged : Projectile {
	public RisingFireWaterProjCharged(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player, "risingfire_proj_water", 
		Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.6f;
		projId = (int)ProjIds.RisingFireUnderwaterCharged;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		vel.y = -275;
		if (!isUnderwater()) destroySelf();
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new RisingFireWaterProjCharged(
			RisingFire.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (!isUnderwater()) destroySelf();
	}
}
