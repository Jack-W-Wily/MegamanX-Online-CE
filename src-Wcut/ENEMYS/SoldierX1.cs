using System;
using System.Collections.Generic;

namespace MMXOnline;

public class SoldierX1 : Maverick {
	public SigmaMenuWeapon meleeWeapon = new();

	public Sprite ridesprite;

	public SoldierX1(
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
		dismantleTypeDeath = true;
		weapon = new Weapon(WeaponIds.VelGGeneric, 101);

		netActorCreateId = NetActorCreateId.SoldierX1;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		ridesprite = new Sprite("enemy_soldier_ra_idle");


		spriteFrameToSounds["neutralra_run/2"] = "ridewalk";
		spriteFrameToSounds["neutralra_run/6"] = "ridewalk2";

		spriteFrameToSounds["enemy_soldier_run/2"] = "run";
		spriteFrameToSounds["enemy_soldier_run/6"] = "run";

		spriteFrameToSounds["enemy_charger_soldier_run/2"] = "run";
		spriteFrameToSounds["enemy_charger_soldier_run/6"] = "run";

		armorClass = ArmorClass.Light;
		height = 24;
	}




	
	public bool isBiker;

	public bool isDriver;



	
	
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

	
	public bool healthvalueOnce = false;

	public bool LostVeicle;


	public override void update() {
		base.update();

		if (isRideArmor && health > 0){
		ridesprite.visible = true;
		} else {
		ridesprite.visible = false;
		}
		ridesprite.update();


		if (!LostVeicle && (isBiker || isDriver) && sprite.name.Contains("grabbed")) {
			
			isDriver = false;
			LostVeicle = true;

			if (isBiker) {
			playSound("rcExplode");
			Anim.createGibEffect("ridechaser_piece", getCenterPos(), netOwner, GibPattern.Radial);
			new ExplodeDieEffect(player ?? netOwner, getCenterPos(), getCenterPos(), "empty", 1, zIndex, false, 35, 0.5f, false);
			isBiker = false;
			}
		} 

		if (!LostVeicle && (isBiker || isDriver) && health < 4 && Options.main.Difficulty > 1) {
			health = 10;
			
			isDriver = false;
			LostVeicle = true;
			new ExplodeDieEffect(player ?? netOwner, getCenterPos(), getCenterPos(), "empty", 1, zIndex, false, 35, 0.5f, false);
			playSound("rcExplode");
			if (isBiker) {
			
			Anim.createGibEffect("ridechaser_piece", getCenterPos(), netOwner, GibPattern.Radial);
			isBiker = false;
			
			}
			changeState(new MJumpStart(), true);
		}

		if (!LostVeicle && isCharger && health < 4 && Options.main.Difficulty > 0) {
			health = 10;
			isCharger = false;
			LostVeicle = true;
			
			playSound("rcExplode");
			new ExplodeDieEffect(player ?? netOwner, getCenterPos(), getCenterPos(), "empty", 1, zIndex, false, 35, 0.5f, false);
			
			changeState(new MJumpStart(), true);
		}


		if (isRideArmor && health < 4 && Options.main.Difficulty > 0) {
			health = 10;
			isRideArmor = false;
			if (Options.main.Difficulty > 1){
			isCharger = true;
			}
			playSound("rcExplode");
			new ExplodeDieEffect(player ?? netOwner, getCenterPos(), getCenterPos(), "empty", 1, zIndex, false, 35, 0.5f, false);
			
			changeState(new MJumpStart(), true);
		}


		if (!healthvalueOnce) {
			healthvalueOnce = true;
			if (isRideArmor){
			health = 20;
			} 
			else if (isCharger){
			health = 15;
			} else {
			health = 10;
			}
		}
		if (aiBehavior == MaverickAIBehavior.Control) {
			if (state is MIdle or MRun or MLand or MGuard) {
				if (shootPressed()) {
					changeState(getShootState(false));
				} else if (specialPressed()) {
					changeState(getShootState2(false));
				} else if (input.isPressed(Control.Dash, player)) {
					changeState(new VelGPounceStartState());
				}
			} else if (state is MJump || state is MFall) {
				if (input.isPressed(Control.Dash, player)) {
					changeState(new VelGPounceStartState());
				}
			}
		}
	}

	public bool isCharger;

	public bool isRideArmor;

	public override string getMaverickPrefix() {
		if (isRideArmor) {
			return "neutralra";
		}
		if (isCharger) {
			return "enemy_charger_soldier";
		}
		if (isBiker) {
			return "enemy_biker_soldier";
		}
		if (isDriver) {
			return "enemy_driver_soldier";
		}
		return "enemy_soldier";
	}

	public override float getRunSpeed() {
		if (isBiker) {
			return 100 * getRunDebuffs();
		}
		return 65f * getRunDebuffs();
	}




	public Point? getRaPoi(out string tag) {
		tag = "";
		if (sprite.getCurrentFrame().POIs.Length > 0) {
			for (int i = 0; i < sprite.getCurrentFrame().POITags.Length; i++) {
				tag = sprite.getCurrentFrame().POITags[i];
				
					return getFirstPOIOffsetOnly(i);
				
			}
		}
		return null;
	}

	public override void render(float x, float y) {
		base.render(x, y);
		var raPOI = getRaPoi(out string tag);
		if (raPOI != null) {
			Sprite sprite = ridesprite;
			sprite.draw(sprite.frameIndex, pos.x + (xDir * raPOI.Value.x), pos.y + raPOI.Value.y, xDir, 1, null, 1, 1, 1, zIndex + 100, useFrameOffsets: true);
		}
	}
	
	public MaverickState getShootState(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
			new FakeZeroBuster2Proj(
				pos, xDir, this, player.getNextActorNetId(), sendRpc: true
			);
		}, "busterX2");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}


	
	public MaverickState getShootState2(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
			new FakeZeroBuster2Proj(
				pos, xDir, this, player.getNextActorNetId(), sendRpc: true
			);
		}, "busterX2");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}


	public override MaverickState[] strikerStates() {
		return [
			new VelGShootFireState(),
			new VelGShootIceState(),
			new VelGPounceStartState(),
		];
	}
	


	public MaverickState getBonusState() {
		var mshoot = state;
		   
		if (Options.main.Difficulty > 1) {
			if (isCharger || isRideArmor){
			mshoot = new KastStompState();
			} 
		} else {
			mshoot = new MIdle();
		}
		return mshoot;
	}



	public override MaverickState[] aiAttackStates() {
		float enemyDist = 199;
		if (target != null && attackgeneralCooldown == 0) {
			enemyDist = MathF.Abs(target.pos.x - pos.x);
		}
		if (attackgeneralCooldown == 0){
			if (Options.main.Difficulty == 0) {
				attackgeneralCooldown = 2;
			}
			if (Options.main.Difficulty == 1) {
				attackgeneralCooldown = 0.6f;
			}
		}
		//if (enemyDist > 50) {
		//	return [new VelGPounceStartState()];
		//}


		



		if (isRideArmor) {
			return [
			new GenericWCUTRunStateM(),
			new GenericDashStateMaverick(),
			new SparkMPunchState(),
			new VelGPounceStartState(),
			];
		}
		else if (isBiker) {
			return [
			new GenericDashStateMaverick(),
			new GenericDashStateMaverick(),
			new VelGPounceStartState(),
			];
		}
		else if (isCharger) {
			return [
			new GenericWCUTRunStateM(),
			new SparkMDashPunchState(),
			new SparkMPunchState()
		];
		}
		else {
		return [
			new GenericWCUTRunStateM(),
			getShootState(false),
			new GenericDashStateMaverick(),
			getShootState(true),
		];
		}
		
		return [];
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Pounce,
		DashPunch,
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"neutralra_attack" => MeleeIds.Pounce,
			"neutralra_attack_dash" => MeleeIds.Pounce,
			"neutralra_attack_air" => MeleeIds.Pounce,

			"enemy_driver_soldier_dash" => MeleeIds.Pounce,
			"enemy_biker_soldier_pounce" => MeleeIds.Pounce,
			"enemy_biker_soldier_dash" => MeleeIds.DashPunch,
			"enemy_charger_soldier_punch" => MeleeIds.Pounce,
			"enemy_charger_soldier_dash_punch" => MeleeIds.DashPunch,
			_ => MeleeIds.None
		});
	}

	// This can be called from a RPC, so make sure there is no character conditionals here.
	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Pounce => new GenericMeleeProj(
				meleeWeapon, pos, ProjIds.VelGMelee, player,
				3, Global.defFlinch, addToLevel: addToLevel, hitSound : "htsnd_punch_2"
			),
			MeleeIds.DashPunch => new GenericMeleeProj(
				meleeWeapon, pos, ProjIds.HeavyPush, player,
				3, 0, addToLevel: addToLevel , hitSound : "kofhtsnd_knock1"
			),
			_ => null
		};
	}

}







public class GenericDashStateMaverick : MaverickState {
	public float dustTime;
	public GenericDashStateMaverick() : base("dash") {
		enterSound = "dash";
	}

	public override void update() {
		base.update();
		if (player == null) return;

		var move = new Point(250 * maverick.xDir, 0);

		var hitGround = Global.level.checkTerrainCollisionOnce(maverick, move.x * Global.spf * 5, 20);
		if (hitGround == null) {
			maverick.changeState(new MIdle());
			return;
		}

		var hitWall = Global.level.checkTerrainCollisionOnce(maverick, move.x * Global.spf * 2, -5);
		if (hitWall?.isSideWallHit() == true) {
		
			maverick.changeState(new MIdle());
			return;
		}

		maverick.move(move);

		if (stateTime > 0.6) {
			maverick.changeState(new MIdle());
			return;
		}

		dustTime += Global.spf;
		if (dustTime > 0.1) {
			dustTime = 0;
			new Anim(maverick.pos.addxy(0, -4), "dust", maverick.xDir, player.getNextActorNetId(), true, sendRpc: true);
		}
	}
}







public class GenericWCUTRunStateM : MaverickState {
	
	bool isDone;
	Character otherChar;
	float moveAmount;
	float maxMoveAmount;


	public GenericWCUTRunStateM( ) : 
	base( "run"
	) {
		
		
		normalCtrl = true;
		attackCtrl = true;
	}

	public string getSpriteName() {
		
		return "run";
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		var character = maverick;
		character.useGravity = true;
		character.vel.y = 0;
		
		maverick.changeSpriteFromName(getSpriteName(), true);
		sprite = getSpriteName();
		MaverickState mState = maverick.getRandomAttackState();
		if (player.input.isHeld(Control.Down, player)) {
			
			maverick.changeState(mState, true);
		}
	}

	public override void onExit(MaverickState? newState) {
		base.onExit(newState);
		var character = maverick;
		character.useGravity = true;
	}

	public override void update() {
		base.update();

		var jumpZones = Global.level.getTerrainTriggerList(
						maverick, Point.zero, typeof(JumpZone)
					);

		

		foreach (var otherPlayer in Global.level.players) {
					if (otherPlayer.character == null) continue;
					if (otherPlayer == player) continue;
					if (otherPlayer.character.isInvulnerable()) continue;
					if (Global.level.gameMode.isTeamMode && otherPlayer.alliance != player.alliance) continue;
					if (otherPlayer.character.getCenterPos().distanceTo(maverick.getCenterPos()) > ParasiticBomb.carryRange) continue;
					otherChar = otherPlayer.character;
					
					break;
		}
		
		if (jumpZones.Count > 0
			&& maverick.grounded ) {
			maverick.changeSpriteFromName("jump", true);
			maverick.vel.y = -maverick.getJumpPower();
		}
		
		if (maverick.sprite.name.Contains("jump")) {
			if (maverick.vel.y > 0) {
				maverick.changeSpriteFromName("fall", true);
			}
		}

		if (maverick.sprite.name.Contains("fall") && maverick.grounded) {
			maverick.changeSpriteFromName("run", true);
		}
		
		
		var character = maverick;
		var move = new Point(80 * maverick.xDir, 0);

		if (otherChar != null){
		if (!once) {
				once = true;
				maxMoveAmount = character.getCenterPos().distanceTo(otherChar.getCenterPos()) * 1.5f;
			}
			if (otherChar.pos.x < character.pos.x) {
			character.xDir = -1;
			} else {
			character.xDir = 1;}
		}
		maverick.move(move);

		moveAmount += Global.spf;
		if (moveAmount > maxMoveAmount) {
			character.changeToIdleOrFall();
			return;
		}
	}
}