﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class Vile : Character {
	public const float maxCalldownMechCooldown = 2;
	public float grabCooldown;
	public bool vulcanActive;
	public float vulcanLingerTime;
	public const int callNewMechCost = 3;
	float mechBusterCooldown;
	public bool usedAmmoLastFrame;
	public int buckshotDanceNum;
	public int VileMode = 0;
	public float vileAmmoRechargeCooldown;
	public bool isShootingLongshotGizmo;
	public int longshotGizmoCount;
	public float gizmoCooldown;
	public bool hasFrozenCastle;
	public bool hasSpeedDevil;
	public bool summonedGoliath;
	public int vileForm;
	public bool isVileMK1 { get { return vileForm == 0; } }
	public bool isVileMK2 { get { return vileForm == 1; } }
	public bool isVileMK5 { get { return vileForm == 2; } }
	public float vileHoverTime;
	public float vileMaxHoverTime = 6;

	public const decimal frozenCastlePercent = 0.125m;
	public const float speedDevilRunSpeed = 110;
	public const int frozenCastleCost = 3;
	public const int speedDevilCost = 3;
	public bool lastFrameWeaponLeftHeld;
	public bool lastFrameWeaponRightHeld;
	public int cannonAimNum;
	
	public float calldownMechCooldown;

	public VileCannon cannonWeapon;
	public Vulcan vulcanWeapon;
	public VileMissile missileWeapon;
	public RocketPunch rocketPunchWeapon;
	public Napalm napalmWeapon;
	public VileBall grenadeWeapon;
	public VileCutter cutterWeapon;
	public VileFlamethrower flamethrowerWeapon;
	public VileLaser laserWeapon;
	public MechMenuWeapon rideMenuWeapon;


	//Statecooldowns
	public float dodgeRollCooldown;
	public float HyperDashCooldown;
	public float GizmoSpreadCD;
	public float AirSplashHitCD;
	public float ModeCD;
	public float AirBombCD;
	public float BumptyBoomCD;
	public const float maxDodgeRollCooldown = 0.8f;
	//

	public Vile(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, bool mk2VileOverride = false, bool mk5VileOverride = false
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
		charId = CharIds.Vile;
		if (isWarpIn) {
			if (mk5VileOverride) {
				vileForm = 2;
			} else if (mk2VileOverride) {
				vileForm = 1;
			}
			if (player.vileFormToRespawnAs == 2 || Global.quickStartVileMK5 == true) {
				vileForm = 2;
			} else if (player.vileFormToRespawnAs == 1 || Global.quickStartVileMK2 == true) {
				vileForm = 1;
			}
		}
		VileLoadout vileLoadout = player.loadout.vileLoadout;

		vulcanWeapon = new Vulcan((VulcanType)vileLoadout.vulcan);
		cannonWeapon = new VileCannon((VileCannonType)vileLoadout.cannon);
		missileWeapon = new VileMissile((VileMissileType)vileLoadout.missile);
		rocketPunchWeapon = new RocketPunch((RocketPunchType)vileLoadout.rocketPunch);
		napalmWeapon = new Napalm((NapalmType)vileLoadout.napalm);
		spriteFrameToSounds["vile_run/4"] = "vileWalk";
		spriteFrameToSounds["vile_run/8"] = "vileWalk";
		spriteFrameToSounds["vilemk2_run/2"] = "vileMk2Walk";
		spriteFrameToSounds["vilemk2_run/5"] = "vileMk2Walk";
		spriteFrameToSounds["vilemk5_run/4"] = "vileMk5Walk";
		spriteFrameToSounds["vilemk5_run/7"] = "vileMk5Walk";
		spriteFrameToSounds["vilemk5_slashrun/3"] = "vileMk5Walk";

		chargeSound = new LoopingSound("charge_start_vile", "charge_loop_vile", this);


		grenadeWeapon = new VileBall((VileBallType)vileLoadout.ball);
		cutterWeapon = new VileCutter((VileCutterType)vileLoadout.cutter);
		flamethrowerWeapon = vileLoadout.flamethrower switch {
			1 => new SeaDragonRage(),
			2 => new DragonsWrath(),
			_ => new WildHorseKick()
		};
		laserWeapon = new VileLaser((VileLaserType)vileLoadout.laser);
		rideMenuWeapon = new MechMenuWeapon(VileMechMenuType.All);

		//Vile Cooldowns
		stateCooldowns.Add(typeof(AirSplashHitGranadeLaunch), new CharStateCooldown(false, true, 1f));
		hasFrozenCastle = player.frozenCastle;
		hasSpeedDevil = player.speedDevil;
	}

	public Sprite? getCannonSprite(out Point poiPos, out int zIndexDir) {
		poiPos = getCenterPos();
		zIndexDir = 0;

		string vilePrefix = "vile_";
	//	if (isVileMK2) vilePrefix = "vilemk2_";
	//	if (isVileMK5) vilePrefix = "vilemk5_";
		string cannonSprite = vilePrefix + "cannon";
		for (int i = 0; i < currentFrame.POIs.Length; i++) {
			var poi = currentFrame.POIs[i];
			var tag = currentFrame.POITags[i] ?? "";
			zIndexDir = tag.EndsWith("b") ? -1 : 1;
			int? frameIndexToDraw = null;
			if (tag.StartsWith("cannon1") && cannonAimNum == 0) frameIndexToDraw = 0;
			if (tag.StartsWith("cannon2") && cannonAimNum == 1) frameIndexToDraw = 1;
			if (tag.StartsWith("cannon3") && cannonAimNum == 2) frameIndexToDraw = 2;
			if (tag.StartsWith("cannon4") && cannonAimNum == 3) frameIndexToDraw = 3;
			if (tag.StartsWith("cannon5") && cannonAimNum == 4) frameIndexToDraw = 4;
			if (frameIndexToDraw != null) {
				poiPos = new Point(pos.x + (poi.x * getShootXDirSynced()), pos.y + poi.y);
				return new Sprite(cannonSprite);
			}
		}
		return null;
	}

	public Point setCannonAim(Point shootDir) {
		float shootY = -shootDir.y;
		float shootX = MathF.Abs(shootDir.x);
		float ratio = shootY / shootX;
		if (ratio > 1.25f) cannonAimNum = 3;
		else if (ratio <= 1.25f && ratio > 0.75f) cannonAimNum = 2;
		else if (ratio <= 0.75f && ratio > 0.25f) cannonAimNum = 1;
		else if (ratio <= 0.25f && ratio > -0.25f) cannonAimNum = 0;
		else cannonAimNum = 4;

		var cannonSprite = getCannonSprite(out Point poiPos, out _);
		Point? nullablePos = cannonSprite?.animData.frames?.ElementAtOrDefault(cannonAimNum)?.POIs?.FirstOrDefault();
		if (nullablePos == null) {
		}
		Point cannonSpritePOI = nullablePos ?? Point.zero;

		return poiPos.addxy(cannonSpritePOI.x * getShootXDir(), cannonSpritePOI.y);
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) {
			return;
		}

		if (!isInDamageSprite()){
		chargeLogic(shoot);
		}


		if (isVileMK5) {
				if (musicSource == null) {
					addMusicSource("MegaloVava", getCenterPos(), true);
				}
		}

		if (charState is Dash)vileHoverTime += 0.12f;
		if (charState is AirDash)vileHoverTime += 0.03f;


		if (charState is Dash || charState is AirDash){
			if (vileHoverTime > vileMaxHoverTime) {
			vileHoverTime = vileMaxHoverTime;
			changeToIdleOrFall();
			return;
			}
		}

		// >>>>>>>>>>>>>>>>>>>>>>>>>>
		//Special moves
		bool repuA = player.input.checkHadoken(player, xDir, Control.Shoot);
		bool hadoukenA = player.input.checkHadoken(player, xDir, Control.Shoot);
		bool shoryukenA = player.input.checkShoryuken(player, xDir, Control.Shoot);
		bool repuS = player.input.checkHadoken(player, xDir, Control.Special1);
		bool hadoukenS = player.input.checkHadoken(player, xDir, Control.Special1);
		bool shoryukenS = player.input.checkShoryuken(player, xDir, Control.Special1);
		bool repuL = player.input.checkHadoken(player, xDir, Control.WeaponLeft);
		bool hadoukenL = player.input.checkHadoken(player, xDir, Control.WeaponLeft);
		bool shoryukenL = player.input.checkShoryuken(player, xDir, Control.WeaponLeft);
		bool PressL = player.input.isPressed(Control.WeaponLeft, player);
		bool PressA = player.input.isPressed(Control.Shoot, player);
		bool PressS = player.input.isPressed(Control.Special1, player);
		
		bool HoldA = player.input.isHeld(Control.Shoot, player);
		bool HoldR = player.input.isHeld(Control.WeaponRight, player);
	
		if (PressA && charState is VileStompState && frameIndex > 2) {
			changeState(new VileSuperKickState(), true);
		}
		if (PressA && charState is VileChainGrabState && frameIndex > 2) {
			changeState(new VilePunch1(), true);
		}
		if (VileMode == 1){
		if (shoryukenS && HyperDashCooldown == 0) {
			changeState(new SplashHitState(), true);
			HyperDashCooldown = 1.5f;
		}
		}

		
		if (shoryukenA || charState is GoGetterRightAttack && player.input.isHeld(Control.Down,player)
		&& player.input.isPressed(Control.Shoot,player)) {
			changeState(new VileChainGrabState(), true);
		}

		// vileteleport
		if (charState is VileDodge &&
		linkedRideArmor != null &&
		player.input.isPressed(Control.Up,player)){
		changeState(new VileTeleport(linkedRideArmor.pos), true);
		}



		


		if (player.input.isHeld(Control.Up,player) && !grounded &&
				  player.input.isPressed(Control.Taunt,player) && parryCooldown == 0 &&
				  (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is SwordBlock)
			  ) {
					changeState(new GlobalParryState(), true);	
			}
		
		if (linkedRideArmor != null && linkedRideArmor.raNum == 0 &&
		player.input.isPressed(Control.AxlCrouch,player)){
		linkedRideArmor.changeSprite("ridearmor_attack", true);
		}
		// blow up ride

		if (linkedRideArmor != null && player.input.isHeld(Control.Down,player)
		&& player.input.isPressed(Control.Taunt,player)){
			linkedRideArmor.explode(shrapnel: true);
			shakeCamera(sendRpc: true);				
			new NecroBurstProj(
				new VileLaser(VileLaserType.NecroBurst), linkedRideArmor.pos,
				xDir, player, player.getNextActorNetId(), rpc: true);
			if (linkedRideArmor.raNum >= 4){
			new GigaCrushProj(
				new GigaCrush(), linkedRideArmor.pos, xDir,
				player, player.getNextActorNetId(), rpc: true
			);
			}
			playSound("necroburst", sendRpc: true);
		}
		//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

		if ((grounded || charState is LadderClimb || charState is LadderEnd || charState is WallSlide) && vileHoverTime > 0) {
			vileHoverTime -= Global.spf * 6;
			if (vileHoverTime < 0) vileHoverTime = 0;
		}

		bool isShootingVulcan = vulcanLingerTime <= 0.1;
		if (isShootingVulcan) {
			vileAmmoRechargeCooldown = 0.15f;
		}

		if (vileAmmoRechargeCooldown > 0) {
			Helpers.decrementTime(ref vileAmmoRechargeCooldown);
		} else if (usedAmmoLastFrame) {
			usedAmmoLastFrame = false;
		} else if (!isShootingLongshotGizmo && !isShootingVulcan) {
			player.vileAmmo += Global.spf * 15;
			if (player.vileAmmo > player.vileMaxAmmo) {
				player.vileAmmo = player.vileMaxAmmo;
			}
		}


		if (player.vileAmmo >= player.vileMaxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				player.vileAmmo = Helpers.clampMax(player.vileAmmo + 1, player.vileMaxAmmo);
				playSound("heal", forcePlay: true);
			}
		}

		if (vulcanLingerTime <= 0.1f && vulcanWeapon.shootCooldown == 0f) {
			vulcanLingerTime += Global.spf;
			if (vulcanLingerTime > 0.1f && sprite.name.EndsWith("shoot")) {
				changeSpriteFromName(charState.sprite, resetFrame: false);
			}
		}
		cannonWeapon.update();
		vulcanWeapon.update();
		missileWeapon.update();
		rocketPunchWeapon.update();
		napalmWeapon.update();
		grenadeWeapon.update();
		cutterWeapon.update();
		laserWeapon.update();
		flamethrowerWeapon.update();

		if (calldownMechCooldown > 0) {
			calldownMechCooldown -= Global.spf;
			if (calldownMechCooldown < 0) calldownMechCooldown = 0;
		}
		//Vilestatecds
		Helpers.decrementTime(ref dodgeRollCooldown);
		Helpers.decrementTime(ref HyperDashCooldown);
		Helpers.decrementTime(ref AirSplashHitCD);
		Helpers.decrementTime(ref AirBombCD);
		Helpers.decrementTime(ref BumptyBoomCD);
		Helpers.decrementTime(ref GizmoSpreadCD);
		Helpers.decrementTime(ref ModeCD);
		Helpers.decrementTime(ref grabCooldown);
		Helpers.decrementTime(ref mechBusterCooldown);
		Helpers.decrementTime(ref gizmoCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);


		if (player.input.isPressed(Control.WeaponLeft, player) 
		&& ModeCD == 0
		&& VileMode == 0){
		VileMode = 1;
		ModeCD = 0.01f;
		addDamageText("Beatdown", 0);	
			

			
		playSound("vileModule", sendRpc: false);	
		}
		if (player.input.isPressed(Control.WeaponLeft, player)
		&&	ModeCD == 0
	 && VileMode == 1){
		VileMode = 0;
			ModeCD = 0.01f;
		addDamageText("Trash Metal", 3);	
		
		playSound("vileModule", sendRpc: false);
		}


		if (charState is not VileStationaryHover &&
			dodgeRollCooldown == 0 && player.canControl) {
		 if (!isInDamageSprite() &&
			player.input.isPressed(Control.Dash, player)
		  && player.input.checkDoubleTap(Control.Dash)) {
				changeState(new VileDodge(), true);
				rideArmorPlatform = null;
			}
		}
		if (charState is not VileStationaryHover && charState is not VileHover &&
				player.input.isHeld(Control.AimAngleUp, player) &&
			vileForm == 2 && player.canControl){
			changeState(new VileStationaryHover(), true);

	
			}


			if (charState is not VileStationaryHover &&
				player.input.isHeld(Control.Down, player) &&
			player.input.isPressed(Control.Dash, player) &&  HyperDashCooldown == 0){
			changeState(new VileDashChargeState(), true);
			playSound("vilehyperdashstart", true);
			HyperDashCooldown = 2f;
			}

		if (charState is InRideChaser) {
			return;
		}
		RideArmorAttacks();
		RideLinkMK5();
		// GMTODO: Consider a better way here instead of a hard-coded deny list
		// Gacel: Done, now it uses attackCtrl
		if (!charState.attackCtrl || charState is VileMK2GrabState) {
			return;
		}
		
	}


	// Vile attacks
	public override bool attackCtrl() {


		bool WPLeftPressed = player.input.isPressed(Control.WeaponLeft, player);
		bool WPRightPressed = player.input.isPressed(Control.WeaponRight, player);
		bool shootPressed = player.input.isPressed(Control.Shoot, player);
		bool specialPressed = player.input.isPressed(Control.Special1, player);
		bool shootHeld = player.input.isHeld(Control.Shoot, player);
		bool WeaponRightHeld = player.input.isHeld(Control.WeaponRight, player);
	



		if (specialPressed) {
		
			airDownAttacks();
			return normalAttacks();
		}

		if (shootHeld){
		if (grabCooldown == 0 && VileMode == 1){
			dashGrabSpecial();
			}
		}
		if (shootPressed) {
			normalAttacks2();

			
		}
		if (WeaponRightHeld && VileMode == 0) {
			vulcanWeapon.vileShoot(0, this);
		}


		if (player.input.isLeftOrRightHeld(player) &&
			WPRightPressed && player.loadout.vileLoadout.cannon == 2) {
			changeState(new VavaVSlashRun(), true);
		}
		
		return base.attackCtrl();
	}

	public bool normalAttacks2() {
			bool LeftorRightHeld = player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player);
		bool UpHeld = player.input.isHeld(Control.Up, player);
		bool DownHeld = player.input.isHeld(Control.Down, player);
		if (VileMode == 1 && charState is not AirDash &&
		charState is not Dash){
	if (DownHeld) {
		if (player.vileAmmo > 4)	
		changeState(new SpoiledBratPunch(), true);
		player.vileAmmo -= 4;
			return true;			
		}
		if (UpHeld) {
			if (player.vileAmmo > 4)	
		changeState(new InfinityGigAttack(), true);
		player.vileAmmo -= 4;
			return true;			
		}
		if (LeftorRightHeld && !UpHeld) {
			if (player.vileAmmo > 4)	
			changeState(new GoGetterRightAttack(), true);
			player.vileAmmo -= 4;
			return true;			
		}
			//if (rocketPunchWeapon.type > -1) {
			//	rocketPunchWeapon.vileShoot(WeaponIds.RocketPunch, this);
			//}
		return true;
		}

		if (VileMode == 0){
		if (cutterWeapon.shootCooldown < cutterWeapon.fireRate * 0.75f) {
		
				cannonWeapon.vileShoot(0, this);
		}
		}
		
			return false;
	}

		public bool airDownAttacks() {
		bool HeldDown = player.input.isHeld(Control.Down, player);
		bool dashorairdash = charState is Dash || charState is AirDash;
		if (!grounded && !dashorairdash) {
			if (!HeldDown && AirBombCD == 0 && VileMode == 0){
				AirBombCD = 0.5f;	
				changeState(new ExplosiveRoundState(), true);
				player.vileAmmo -= 4;			
			}
			if (!HeldDown && AirBombCD == 0 && VileMode == 1){
				AirBombCD = 0.5f;	
				changeState(new SpreadShotKnee(), true);		
			}
			  
			 if (HeldDown && AirSplashHitCD == 0 && VileMode == 0){
				AirSplashHitCD = 0.5f;	
				changeState(new PeaceOutRollerAttack(), true);
				player.vileAmmo -= 8;			
			}
			if (HeldDown && AirSplashHitCD == 0 && VileMode == 1){
				flamethrowerWeapon.vileShoot(WeaponIds.VileFlamethrower, this);			
			}
		return true;
		}
		return false;
	}

	public bool normalAttacks() {
		bool LeftorRightHeld = player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player);
		bool UpHeld = player.input.isHeld(Control.Up, player);
		
	
			if (player.input.isHeld(Control.Down, player) 
			&& BumptyBoomCD == 0 && VileMode == 0 && grounded){
				BumptyBoomCD = 0.8f;	
				changeState(new BumptyBoomGranadeLaunch(), true);
				player.vileAmmo -= 8;			
			}

			if (player.input.isHeld(Control.Up, player) 
			&& AirSplashHitCD == 0 && VileMode == 1){
				AirSplashHitCD = 0.8f;
				changeState(new AirSplashHitGranadeLaunch(), true);
			player.vileAmmo -= 8;			
			}

			if (player.input.isHeld(Control.Up, player) 
			&& AirSplashHitCD == 0 && VileMode == 0 && (vileForm == 1|| player.loadout.vileLoadout.cannon == 1)){
				AirSplashHitCD = 1.5f;
				changeState(new AirFireNadeLaunch(), true);
			player.vileAmmo -= 8;			
			}


		if (cutterWeapon.shootCooldown < cutterWeapon.fireRate * 0.75f
		&& VileMode == 1 && (grounded || charState is AirDash)) {
		
			cutterWeapon.vileShoot(WeaponIds.VileCutter, this);
		}
		if (cutterWeapon.shootCooldown < cutterWeapon.fireRate * 0.75f
		&& VileMode == 0 && (grounded || charState is AirDash)) {	
			missileWeapon.vileShoot(WeaponIds.ElectricShock, this);			
		}

		return false;
	}

	public bool dashGrabSpecial() {
		if (charState is Dash || charState is AirDash) {
				charState.isGrabbing = true;
			if (getChargeLevel() == 2){
				charState.superArmor = true; //peakbalance
					stopCharge();
				}
				if (getChargeLevel() > 2){
				invulnTime = 0.5f;
					stopCharge();
				}
				changeSpriteFromName("dash_grab", true);	
			return true;
		}
		return false;
	} 
	public bool RideArmorAttacks() {
		var raState = charState as InRideArmor;
		bool Goliath = rideArmor?.raNum == 4;
		bool stunShotPressed = player.input.isPressed(Control.Special1, player);
		bool HeldDown = player.input.isHeld(Control.Down, player);
		bool goliathShotPressed = player.input.isPressed(Control.WeaponLeft, player) || player.input.isPressed(Control.WeaponRight, player);
		bool raStates = rideArmor?.rideArmorState is RAIdle || rideArmor?.rideArmorState is RAJump || rideArmor?.rideArmorState is RAFall || rideArmor?.rideArmorState is RADash;
		if (rideArmor != null && raState != null && !raState.isHiding) {
			if (raStates) {
				if (Goliath && Options.main.swapGoliathInputs) {
					bool oldStunShotPressed = stunShotPressed;
					stunShotPressed = goliathShotPressed;
					goliathShotPressed = oldStunShotPressed;
				}
				if (stunShotPressed && !HeldDown) {
				//	if (tryUseVileAmmo(missileWeapon.vileAmmo)) {
						missileWeapon.vileShoot(WeaponIds.ElectricShock, this);
			//		}
				}
				if (goliathShotPressed) {
					if (Goliath && !rideArmor.isAttacking() && mechBusterCooldown == 0) {
						rideArmor.changeState(new RAGoliathShoot(rideArmor.grounded), true);
						mechBusterCooldown = 1;
					}
				}
			}
			player.gridModeHeld = false;
			player.gridModePos = new Point();
			return true;
		}
		return false;
	}
	public override bool normalCtrl() {
		if (sprite.name.EndsWith("cannon_air") && isAnimOver()) {
			changeSpriteFromName("fall", true);
		}
			
		if (charState.attackCtrl
		 && charState is not Dash && grounded && 
		!player.input.isHeld(Control.WeaponRight, player)&&
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);
			changeState(new SwordBlock());
			return true;
		}

		if (!grounded &&
			canVileHover() &&
			player.input.isPressed(Control.Jump, player) &&
			charState is not VileHover
		) {
			changeState(new VileHover(), true);
			return true;
		}
		return base.normalCtrl();
	}
	public void shoot(int chargeLevel) {
		if (chargeLevel == 1) {
			player.vileAmmo -= 10;
			changeState(new CannonAttackFrunner(false, player.character.grounded), true);
		}
		if (chargeLevel == 2) {
			player.vileAmmo -= 15;
			changeState(new CannonAttackFatBoy(false, player.character.grounded));
		}

		if (chargeLevel == 3) {
			laserWeapon.vileShoot(WeaponIds.VileLaser, this);
		}
		if (chargeLevel == 4 && (isVileMK5 || player.loadout.vileLoadout.cannon  == 2)) {
			changeState(new HexaInvoluteState(), true);
		} 
	}
	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Special1, player)
		|| player.input.isHeld(Control.Shoot,player);
	}
	public override bool canCharge() {
		return !isInvulnerableAttack() && charState is not Die && invulnTime == 0;
	}

	
	public override int getMaxChargeLevel() {
		if (isVileMK5 || player.loadout.vileLoadout.cannon  == 2){
			return 4;
		}
		return  3;
	}
	public override bool canShoot() {
		if (isInvulnerableAttack()) return false;
		if (invulnTime > 0) return false;
		if (!player.canControl) return false;
		return base.canShoot();
	}
	public override void chargeLogic(Action<int> shootFunct) {
		if (chargeButtonHeld() && flag == null && player.vileAmmo >= laserWeapon.getAmmoUsage(0)) {
			if (canCharge()) {
				increaseCharge();
			}
		}
		else if (canShoot()) {
			int chargeLevel = getChargeLevel();
			if (isCharging()) {
				if (chargeLevel >= 1) {
					shootFunct(chargeLevel);
				}
			}
			stopCharge();
		}
		chargeGfx();
	}
	public void RideLinkMK5() {
		if ((isVileMK5 ||player.loadout.vileLoadout.cannon  == 2) &&  linkedRideArmor != null &&
		(player.input.isPressed(Control.Special2, player) &&
			player.input.isHeld(Control.Down, player) 
			|| charState is GenericStun
			|| sprite.name.Contains("lose")
			|| sprite.name.Contains("hurt")
			|| sprite.name.Contains("knocked")
			|| sprite.name.Contains("grabbed"))
		) {
			if (linkedRideArmor.rideArmorState is RADeactive) {
				linkedRideArmor.manualDisabled = false;
				linkedRideArmor.changeState(new RAIdle("ridearmor_activating"), true);
			} else {
				linkedRideArmor.manualDisabled = true;
				linkedRideArmor.changeState(new RADeactive(), true);
				Global.level.gameMode.setHUDErrorMessage(
					player, "Deactivated Ride Armor.",
					playSound: false, resetCooldown: true
				);
			}
		}
		// Vile V Ride control.
		if (!(isVileMK5 ||player.loadout.vileLoadout.cannon  == 2) || linkedRideArmor == null) {
			if (player.input.isPressed(Control.Special2, player) &&
				rideMenuWeapon != null && calldownMechCooldown == 0 &&
				(!alreadySummonedNewMech || linkedRideArmor != null)
			) {
				onMechSlotSelect(rideMenuWeapon);
				return;
			}
		//Ride Menu
		} else if (player.input.isPressed(Control.Special2, player) && !player.input.isHeld(Control.Down, player)) {
			onMechSlotSelect(rideMenuWeapon);
			return;
		}
		if (rideMenuWeapon?.isMenuOpened == true) {
			if (player.input.isPressed(Control.Special1, player) || player.input.isPressed(Control.WeaponLeft, player)) {
				rideMenuWeapon.isMenuOpened = false;
			}
		}

		if ((isVileMK5 ||player.loadout.vileLoadout.cannon  == 2) && linkedRideArmor != null) {
			if (canLinkMK5()) {
				if (linkedRideArmor.character == null) {
					linkedRideArmor.linkMK5(this);
				}
			} else {
				if (linkedRideArmor.character != null) {
					linkedRideArmor.unlinkMK5();
				}
			}
		}
	}
	public bool canLinkMK5() {
		if (linkedRideArmor == null) return false;
		if (linkedRideArmor.rideArmorState is RADeactive && linkedRideArmor.manualDisabled) return false;
		if (linkedRideArmor.pos.distanceTo(pos) > Global.screenW * 0.75f) return false;
		return charState is not Die && charState is not VileRevive && charState is not CallDownMech && charState is not HexaInvoluteState;
	}

	public bool isVileMK5Linked() {
		return (isVileMK5 ||player.loadout.vileLoadout.cannon  == 2) && linkedRideArmor?.character == this;
	}

	public bool canVileHover() {
		return (isVileMK5 ||player.loadout.vileLoadout.cannon  == 2) && player.vileAmmo > 0 && flag == null;
	}

	public override bool canTurn() {
		if (rideArmorPlatform != null) {
			return false;
		}
		return base.canTurn();
	}

	public override bool canWallClimb() {
		if (charState is VileHover) {
			return !player.input.isHeld(Control.Jump, player);
		}
		return base.canWallClimb();
	}

	public override bool canUseLadder() {
		if (charState is VileHover) {
			return !player.input.isHeld(Control.Jump, player);
		}
		return base.canWallClimb();
	}

	public override Point getDashDustEffectPos(int xDir) {
		float dashXPos = -30;
		return pos.addxy(dashXPos * xDir + (5 * xDir), -4);
	}

	public override void onMechSlotSelect(MechMenuWeapon mmw) {
		if (linkedRideArmor == null) {
			if (!mmw.isMenuOpened) {
				mmw.isMenuOpened = true;
				return;
			}
		}

		if (player.isAI) {
			calldownMechCooldown = maxCalldownMechCooldown;
		}
		if (linkedRideArmor == null) {
			if (alreadySummonedNewMech) {
				Global.level.gameMode.setHUDErrorMessage(player, "Can only summon a mech once per life");
			} else if (canAffordRideArmor()) {
				if (!(charState is Idle || charState is Run || charState is Crouch)) return;
				if (player.selectedRAIndex == 4 && player.currency < 10) {
					if (isVileMK2 || player.loadout.vileLoadout.cannon == 1) {
						Global.level.gameMode.setHUDErrorMessage(
							player, $"Goliath armor requires 10 {Global.nameCoins}"
						);
					} else {
						Global.level.gameMode.setHUDErrorMessage(
							player, $"Devil Bear armor requires 10 {Global.nameCoins}"
						);
					}
				} else {
					alreadySummonedNewMech = true;
					if (linkedRideArmor != null) linkedRideArmor.selfDestructTime = 1000;
					buyRideArmor();
					mmw.isMenuOpened = false;
					int raIndex = player.selectedRAIndex;
					if (isVileMK5 && raIndex == 4) raIndex++;
					linkedRideArmor = new RideArmor(player, pos, raIndex, 0, player.getNextActorNetId(), true, sendRpc: true);
					if (linkedRideArmor.raNum == 4) summonedGoliath = true;
					if (isVileMK5 || player.loadout.vileLoadout.cannon  == 2 ) {
						linkedRideArmor.ownedByMK5 = true;
						linkedRideArmor.zIndex = zIndex - 1;
					}
					changeState(new CallDownMech(linkedRideArmor, true), true);
				}
			} else {
				if (player.selectedRAIndex == 4 && player.currency < 10) {
					if (isVileMK2) Global.level.gameMode.setHUDErrorMessage(
						player, $"Goliath armor requires 10 {Global.nameCoins}"
					);
					else Global.level.gameMode.setHUDErrorMessage(
						player, $"Devil Bear armor requires 10 {Global.nameCoins}"
					);
				} else {
					cantAffordRideArmorMessage();
				}
			}
		} else {
		//	if (!(charState is Idle || charState is Run || charState is Crouch)) return;
			changeState(new CallDownMech(linkedRideArmor, false), true);
		}
	}

	public bool tryUseVileAmmo(float ammo, bool isVulcan = false) {
		if (isVulcan) {
			usedAmmoLastFrame = true;
		}
		if (player.vileAmmo > ammo - 0.1f) {
			usedAmmoLastFrame = true;
			if (weaponHealAmount == 0) {
				player.vileAmmo -= ammo;
				if (player.vileAmmo < 0) player.vileAmmo = 0;
			}
			return true;
		}
		return false;
	}
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}
	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}
	public override bool canAddAmmo() {
		return player.vileAmmo < player.vileMaxAmmo;
	}

	private void cantAffordRideArmorMessage() {
		if (Global.level.is1v1()) {
			Global.level.gameMode.setHUDErrorMessage(player, "Ride Armor requires 16 HP");
		} else {
			Global.level.gameMode.setHUDErrorMessage(
				player, "Ride Armor requires " + callNewMechCost + " " + Global.nameCoins
			);
		}
	}

	public Point getVileShootVel(bool aimable) {
		Point vel = new Point(1, 0);
		if (!aimable) {
			return vel;
		}

		if (rideArmor != null) {
			if (player.input.isHeld(Control.Up, player)) {
				vel = new Point(1, -0.5f);
			} else {
				vel = new Point(1, 0.5f);
			}
		} else if (charState is VileMK2GrabState) {
			vel = new Point(1, -0.75f);
		} else if (player.input.isHeld(Control.Up, player)) {
			if (!canVileAim60Degrees() || (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		} else if (player.input.isHeld(Control.Down, player) && player.character.charState is not Crouch && charState is not MissileAttack) {
			vel = new Point(1, 0.5f);
		} else if (player.input.isHeld(Control.Down, player) && player.input.isLeftOrRightHeld(player) && player.character.charState is Crouch) {
			vel = new Point(1, 0.5f);
		}

		if (charState is RisingSpecterState) {
			vel = new Point(1, -0.75f);
		}

		/*
		if (charState is CutterAttackState)
		{
			vel = new Point(1, -3);
		}
		*/

		return vel;
	}

	public bool canVileAim60Degrees() {
		return charState is MissileAttack || charState is Idle || charState is CannonAttack;
	}

	public Point? getVileMK2StunShotPos() {
		if (charState is InRideArmor) {
			return pos.addxy(xDir * -8, -12);
		}

		var headPos = getHeadPos();
		if (headPos == null) return null;
		return headPos.Value.addxy(-xDir * 5, 3);
	}

	public void setVileShootTime(Weapon weapon, float modifier = 1f, Weapon? targetCooldownWeapon = null) {
		targetCooldownWeapon = targetCooldownWeapon ?? weapon;
		if (isVileMK2) {
			float innerModifier = 1f;
			if (weapon is VileMissile) innerModifier = 0.3333f;
			weapon.shootCooldown = MathF.Ceiling(targetCooldownWeapon.fireRate * innerModifier * modifier);
		} else {
			weapon.shootCooldown = MathF.Ceiling(targetCooldownWeapon.fireRate * modifier);
		}
	}

	public override Projectile? getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile? proj = null;

		if (sprite.name.Contains("crouch_start")) {
			proj = new GenericMeleeProj(new VileStomp(), centerPoint, ProjIds.VileStomp, player, 0, 0, 0);
		}
		if (sprite.name.Contains("vilemk5_stomp")) {
			proj = new GenericMeleeProj(new VileStomp(), 
			centerPoint, ProjIds.VileStomp2, player, 0.3f, 0, 5f);
		}

		if (sprite.name.Contains("air_bomb_attack")) {
			proj = new GenericMeleeProj( new MechFrogStompWeapon(player), 
			centerPoint, ProjIds.MechFrogStompShockwave, player, 0, 30, 0);
		}
		if (sprite.name.Contains("violentcrusher_grab")) {
			proj = new GenericMeleeProj( new MechFrogStompWeapon(player), 
			centerPoint, ProjIds.MechFrogStompShockwave, player, 3, 0, 10);
		}

		if (sprite.name.Contains("dash_grab")) {
			proj = new GenericMeleeProj(new VileMK2Grab(), centerPoint, ProjIds.VileMK2Grab, player, 0, 0, 0);
		}

		if (sprite.name.Contains("block")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true
			);
		}


		if (sprite.name.Contains("slashrun")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.ForceGrabState, player,
				2f, 0,  15f, isDeflectShield: true, ShouldClang : true,
				isPushProjectile : true,
				 isZSaberEffect2 : true
			);
		}


		if (sprite.name.Contains("punch")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				1f, 25,  15f, isDeflectShield: true, ShouldClang : true
			);
		}
			if (sprite.name.Contains("kick") &&  !sprite.name.Contains("kick_3") &&  !sprite.name.Contains("super")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				1, 25,  15f, isDeflectShield: true, ShouldClang : true
			);
		}
		if (sprite.name.EndsWith("superkick_up") 
		) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileAirRaidPlusKnock, player,
				2, 0,  15f, isDeflectShield: true
			);
		}
		if (sprite.name.EndsWith("superkick") 
		) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileSuperKick, player,
				2, 0,  15f, isDeflectShield: true
			);
		}


		if (sprite.name.Contains("hyperdash") && !sprite.name.Contains("2")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileSuperKick, player,
				2, 0,  10f, isDeflectShield: true, ShouldClang : true
			);
		}

			if (sprite.name.Contains("hyperdash") && sprite.name.Contains("2")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.ForceGrabState, player,
				0.5f, 0,  10f, isDeflectShield: true, ShouldClang : true
			);
		}

		if (sprite.name.Contains("kick_3")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileAirRaidPlusKnock, player,
				2, 0,  15f, isDeflectShield: true
			);
		}

			if (sprite.name.Contains("spring_grab") && 	VileMode == 1) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileAirRaidStart, player,
				0, 0,  15f, isDeflectShield: true
			);
		}

		if (sprite.name.Contains("spring_grab") && 	VileMode == 0) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileMK2Grab, player,
				0, 0,  15f, isDeflectShield: true
			);
		}



		
		return proj;
	}

	public override bool isSoftLocked() {
		if (isShootingLongshotGizmo) {
			return true;
		}
		if (isVileMK5 && linkedRideArmor != null && player.input.isHeld(Control.WeaponLeft, player)) {
			return true;
		}
		if (sprite.name.EndsWith("_idle_shoot") && sprite.frameTime < 6) {
			return true;
		}
		return base.isSoftLocked();
	}

	public override bool canChangeWeapons() {
		if (isShootingLongshotGizmo) {
			return false;
		}
		return base.canChangeWeapons();
	}

	public override bool canEnterRideArmor() {
		if (isVileMK5) {
			return false;
		}
		return base.canEnterRideArmor();
	}

	public override void changeSprite(string spriteName, bool resetFrame) {
		cannonAimNum = 0;
		base.changeSprite(spriteName, resetFrame);
	}

	public override string getSprite(string spriteName) {
		if (player.loadout.vileLoadout.cannon == 2) {

			if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("vilemk5alt_" + spriteName)){		
			return "vilemk5alt_" + spriteName;
			}
			return "vilemk5_" + spriteName;
		}


		if (player.loadout.vileLoadout.cannon == 1) {

			if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("vilemk2alt_" + spriteName)){		
			return "vilemk2alt_" + spriteName;
			}
			return "vilemk2_" + spriteName;
		}

		if (player.loadout.vileLoadout.cannon == 0) {

			if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("vilealt_" + spriteName)){		
			return "vilealt_" + spriteName;
			}
			return "vile_" + spriteName;
		}

		return "vile_" + spriteName;
	}

	public override void changeToIdleOrFall(string transitionSprite = "") {
		if (!grounded && charState.wasVileHovering && canVileHover()) {
			changeState(new VileHover(), true);
			return;
		}
		base.changeToIdleOrFall(transitionSprite);
	}

	public override float getLabelOffY() {
		if (sprite.name.Contains("_ra_")) {
			return 25;
		}
		return 50;
	}

	public override void render(float x, float y) {
		if (hasSpeedDevil) {
			addRenderEffect(RenderEffectType.SpeedDevilTrail);
		} else {
			removeRenderEffect(RenderEffectType.SpeedDevilTrail);
		}
		if (currentFrame.POIs.Length > 0) {
			Sprite? cannonSprite = getCannonSprite(out Point poiPos, out int zIndexDir);
			cannonSprite?.draw(
				cannonAimNum, poiPos.x, poiPos.y, getShootXDirSynced(),
				1, getRenderEffectSet(), alpha, 1, 1, zIndex + zIndexDir,
				getShaders(), actor: this
			);
		}

		if (player.isMainPlayer && vileHoverTime > 0 && charState is not HexaInvoluteState) {
			float healthPct = Helpers.clamp01((vileMaxHoverTime - vileHoverTime) / vileMaxHoverTime);
			float sy = -27;
			float sx = 20;
			if (xDir == -1) sx = 90 - 20;
			drawFuelMeter(healthPct, sx, sy);
		}
		base.render(x, y);
	}

	public override Point getAimCenterPos() {
		if (sprite.name.Contains("_ra_")) {
			return pos.addxy(0, -10);
		}
		return pos.addxy(0, -24);
	}

	public override Collider getGlobalCollider() {
		var rect = new Rect(0, 0, 18, 42);
		if (sprite.name.Contains("_ra_")) {
			rect.y2 = 20;
		}
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override Collider getDashingCollider() {
		Rect rect = new Rect(0, 0, 18, 30);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override Collider getCrouchingCollider() {
		Rect rect = new Rect(0, 0, 18, 30);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override Collider getRaCollider() {
		var rect = new Rect(0, 0, 18, 22);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> shaders = base.getShaders();

		if (hasFrozenCastle && player.frozenCastleShader != null) {
			shaders.Add(player.frozenCastleShader);
		}

		return shaders;
	}

	public override float getRunSpeed() {
		if (hasSpeedDevil) {
			return base.getRunSpeed() * 1.1f;
		}
		return base.getRunSpeed();
	}

	public override float getDashSpeed() {
		float dashSpeed = 3.45f * 60f;

		if (hasSpeedDevil) {
			dashSpeed *= 1.1f;
		}
	
		return dashSpeed * getRunDebuffs();
	}

	public override Point getParasitePos() {
		if (sprite.name.Contains("_ra_")) {
			if (sprite.name.Contains("_ra_hide")) {
				pos.addxy(0, -6 + 22 * (sprite.frameIndex / (float)sprite.totalFrameNum));
			}
			return pos.addxy(0, -6);
		}
		return pos.addxy(0, -24);
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();

		customData.Add(Helpers.boolArrayToByte([
			hasFrozenCastle,
			hasSpeedDevil
		]));

		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];

		// Per-character data.
		bool[] boolData = Helpers.byteToBoolArray(data[0]);
		hasFrozenCastle = boolData[0];
		hasSpeedDevil = boolData[1];
	}


	public override bool isToughGuyHyperMode() {
		return sprite.name.Contains("grab") 
		|| charState is VileStationaryHover
		|| charState is VileHover ;
	}

	public virtual bool isStatusImmune() {
		return charState is HexaInvoluteState ;
	}



public float aiAttackCooldown;
	public override void aiAttack(Actor? target) {
		int Vattack = Helpers.randomRange(1, 9);
		bool isFacingTarget = (pos.x < target?.pos.x && xDir == 1) || (pos.x >= target?.pos.x && xDir == -1);
		if (!charState.isGrabbedState && !player.isDead && !isInvulnerableAttack()
			&& !(charState is VileRevive or HexaInvoluteState or NecroBurstAttack
			or StraightNightmareAttack or RisingSpecterState or VileMK2GrabState 
			or GenericStun or Hurt or Die) && aiAttackCooldown <= 0) {
			if (isVileMK2 && charState is Dash or AirDash && isFacingTarget) {
				player.press(Control.Special1);
			}
			switch (Vattack) {
				case 1 when isFacingTarget:
					cannonWeapon.vileShoot(WeaponIds.FrontRunner, this);
					break;
				case 2 when isFacingTarget:
					rocketPunchWeapon.vileShoot(WeaponIds.RocketPunch, this);
					break;
				case 3 when !grounded:
					grenadeWeapon.vileShoot(WeaponIds.VileBomb, this);
					break;
				case 4 when isFacingTarget:
					missileWeapon.vileShoot(WeaponIds.ElectricShock, this);
					break;
				case 5 when isFacingTarget:
					cutterWeapon.vileShoot(WeaponIds.VileCutter, this);
					break;
				case 6 when grounded:
					napalmWeapon.vileShoot(WeaponIds.Napalm, this);
					break;
				case 7 when charState is Fall:
					flamethrowerWeapon.vileShoot(WeaponIds.VileFlamethrower, this);
					break;
				case 8 when player.vileAmmo >= 24 && !player.isMainPlayer && isFacingTarget:
					laserWeapon.vileShoot(WeaponIds.VileLaser, this);
					break;
				case 9 when isVileMK5 && player.vileAmmo >= 20 && !player.isMainPlayer:
					changeState(new HexaInvoluteState(), true);
					break;
			}
			aiAttackCooldown = 20;
		}
		base.aiAttack(target);
	}
	public override void aiUpdate() {
		if (!player.isMainPlayer) {
			if (player.canReviveVile() && isVileMK1) {
				player.reviveVile(false);
			}
			if (isVileMK2 && player.canReviveVile()) {
				player.reviveVile(true);
			}
		}
		base.aiUpdate();
	}
}
