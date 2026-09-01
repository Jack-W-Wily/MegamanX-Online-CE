using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class VAVAV : Vile {




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

	public bool isVavaV = true;

	public bool isVava2 = false;

	public int ResitDeathTimes = 0;

	
	public float selfDamageCooldown;

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

	public VileCannonWC cannonWeapon;
	public VileVulcan vulcanWeapon;
	public VileMissile missileWeapon;
	public RocketPunch rocketPunchWeapon;
	public VileNapalm napalmWeapon;
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


	public float AiCD;
	public float AirBombCD;
	public float BumptyBoomCD;
	public const float maxDodgeRollCooldown = 0.8f;
	//

	public VAVAV(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
		) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn) {

		charId = CharIds.Vile;
	
		VileLoadout loadout = player.loadout.vileLoadout;



		spriteFrameToSounds["vilemk5_run/2"] = "hornetWalk";
		spriteFrameToSounds["vilemk5_run/7"] = "hornetWalk";
		spriteFrameToSounds["vilemk5_slashrun/3"] = "vileMk5Walk";
		spriteFrameToSounds["vilemk5_air_bomb_attack/4"] = "fireNappalmMK2";
		spriteFrameToSounds["vilemk5_crouch_nade/2"] = "fireNappalmMK2";
		chargeSound = new LoopingSound("charge_start_vile", "charge_loop_vile", this);


		
		loadout ??= player.loadout.vileLoadout.clone();
		loadout = loadout;

		vulcanWeapon = new DistanceNeedler();
		cannonWeapon = new VileCannonWC(0);
		missileWeapon = new ElectricShock();
		rocketPunchWeapon = new InfinityGig();
		napalmWeapon = loadout.napalm switch {
			1 => new FireGrenade(),
			2 => new SplashHit(),
			3 => new NoneNapalm(),
			_ => new RumblingBang()
		};
		grenadeWeapon = loadout.ball switch {
			1 => new SpreadShot(),
			2 => new PeaceOutRoller(),
			3 => new NoneBall(),
			_ => new ExplosiveRound()
		};
		cutterWeapon = new MaroonedTomahawk();
		flamethrowerWeapon =  new SeaDragonRage();
		downSpWeapon = loadout.downSpWeapon switch {
			0 => napalmWeapon,
			1 => grenadeWeapon,
			2 => flamethrowerWeapon,
			_ => napalmWeapon,
		};
		airSpWeapon = loadout.airSpWeapon switch {
			0 => napalmWeapon,
			1 => grenadeWeapon,
			2 => flamethrowerWeapon,
			_ => napalmWeapon,
		};
		downAirSpWeapon = loadout.downAirSpWeapon switch {
			0 => napalmWeapon,
			1 => grenadeWeapon,
			2 => flamethrowerWeapon,
			_ => napalmWeapon,
		};
		laserWeapon = loadout.laser switch {
			1 => new NecroBurst(),
			2 => new StraightNightmare(),
			3 => new NoneLaser(),
			_ => new RisingSpecter()
		};
		rideMenuWeapon = new MechMenuWeapon(VileMechMenuType.All);

		hasFrozenCastle = player.frozenCastle;
		hasSpeedDevil = player.speedDevil;
	}

	public Sprite? getCannonSprite(out Point poiPos, out int zIndexDir) {
		poiPos = getCenterPos();
		zIndexDir = 0;

		string vilePrefix = "vava_";
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

	public override Point setCannonAim(Point shootDir) {
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
		Point cannonSpritePOI = nullablePos ?? pos.addxy(20 * xDir, -35);

		return poiPos.addxy(cannonSpritePOI.x * getShootXDir(), cannonSpritePOI.y);
	}


	public float deathsmoketime;

	public float spawnTime = 0;

	public int radius = 26;
	
	
	public override void update() {
			base.update();
		if (!ownedByLocalPlayer) {
			return;
		}


		Supers();
	
		


		if (ResitDeathTimes > 0) {

			selfDamageCooldown += Global.spf;
			if (selfDamageCooldown >= 1 && !isInDamageSprite()
			&& !isAttacking() && !sprite.name.Contains("grab")) {
				selfDamageCooldown = 0;
				applyDamage(0.5f * ResitDeathTimes, player, this, null, (int)ProjIds.SelfDmg);
			}

			deathsmoketime += Global.spf;
			if (deathsmoketime > 0.1f) {
				deathsmoketime = 0;

				if (ResitDeathTimes > 0) {
					new DashDustAnim(
						pos.addxy(xDir * -8, -12), player.getNextActorNetId(), true, true
					);
				}
				if (ResitDeathTimes > 2) {
					new DashDustAnim(
						pos.addxy(xDir * 8, -20), player.getNextActorNetId(), true, true
					);
				}

				if (ResitDeathTimes > 3) {
					spawnTime += Global.spf;
					if (spawnTime >= 0.2f) {

						if (ResitDeathTimes == 4) spawnTime = 0;
						if (ResitDeathTimes == 5) spawnTime = 0.1f;
						int randX = Helpers.randomRange(-radius, radius);
						int randY = Helpers.randomRange(-radius, radius);
						var randomPos = pos.addxy(randX, randY);
						new Anim(randomPos, "explosion", 1, player.getNextActorNetId(), true, sendRpc: true);
						playSound("explosion", sendRpc: true);
					}
				}



			}

			if (ResitDeathTimes > 1) {
				oilTime = 1;
			}
		}



		if (!isInDamageSprite()) {
			chargeLogic(shoot);
		}


		if (isVileMK5) {
			if (musicSource == null) {
				addMusicSource("MegaloVava", getCenterPos(), true);
			}
		}

		if (charState is Dash) vileHoverTime += 0.12f;
		if (charState is AirDash) vileHoverTime += 0.03f;


		if (charState is Dash || charState is AirDash) {
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
		


	

		// vileteleport
		if (charState is VileDodge &&
		linkedRideArmor != null &&
		player.input.isPressed(Control.Up, player)) {
			changeState(new VileTeleport(linkedRideArmor.pos), true);
		}






		if (player.input.isHeld(Control.Up, player) && !isInDamageSprite() &&
				  player.input.isPressed(Control.WeaponLeft,player) && GenericParryCD == 0
			) {
			GenericParryCD = 0.2f;
			changeState(new GlobalParryState(), true);
		}

		if (linkedRideArmor != null && linkedRideArmor.raNum == 0 &&
		player.input.isPressed(Control.AxlCrouch, player)) {
			linkedRideArmor.changeSprite("ridearmor_attack", true);
		}
		// blow up ride

		if (linkedRideArmor != null && player.input.isHeld(Control.Down, player)
		&& player.input.isPressed(Control.Taunt, player)) {
			linkedRideArmor.explode(shrapnel: true);
			shakeCamera(sendRpc: true);

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
	

		if (player.input.isPressed(Control.WeaponLeft, player)
		&& ModeCD == 0
		&& VileMode == 0) {
			VileMode = 1;
			ModeCD = 0.01f;
			addDamageText("Beatdown", 0);



			playSound("vileModule", sendRpc: false);
		}
		if (player.input.isPressed(Control.WeaponLeft, player)
		&& ModeCD == 0
	 && VileMode == 1) {
			VileMode = 0;
			ModeCD = 0.01f;
			addDamageText("Trash Metal", 3);

			playSound("vileModule", sendRpc: false);
		}


		if (charState is not VileStationaryHover &&
			GenericDodgeCD == 0 && player.canControl) {
			if (!isInDamageSprite() &&
			   player.input.isPressed(Control.Dash, player)
			 && player.input.checkDoubleTap(Control.Dash)) {
				changeState(new VileDodge(), true);
				rideArmorPlatform = null;
				GenericDodgeCD = 1;
			}
		}
		if (charState is not VileStationaryHover && charState is not VileHover &&
				player.input.isHeld(Control.AimAngleUp, player) &&
			vileForm == 2 && player.canControl) {
			changeState(new VileStationaryHover(), true);


		}


		if (charState is not VileStationaryHover &&
			player.input.isHeld(Control.Down, player) &&
		player.input.isPressed(Control.Dash, player) && HyperDashCooldown == 0) {
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

		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);
		Helpers.decrementTime(ref CrimsonphantomCD);
		Helpers.decrementTime(ref grabCooldown);
		Helpers.decrementTime(ref mechBusterCooldown);
		Helpers.decrementTime(ref gizmoCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref CannonCD);

	}

		public override bool canAirDash() {
		return true;
	}


	public override bool spcCancel() {
		

		// Dash Cancel
		if (player.dashPressed(out string dashControl) && player.vileAmmo > 15) {
			if (grounded) {
				changeState(new Dash(dashControl), true);
			} else {
				changeState(new AirDash(dashControl), true);
			}
			player.vileAmmo -= 16;
			playSound("distortion_d", sendRpc: true);
				return true;
		}

		// JumpCancel
		if (player.input.isPressed(Control.Jump, player) && canJump()) {
				vel.y = -getJumpPower();
				isDashing = true;
				changeState(getJumpState());
				return true;
		} 

		


		SpecialMoves();


		return base.spcCancel();
	}


public bool Supers() {
		if (player.input.checkShoryuken2(player, xDir, Control.Special1) && player.superAmmo >= 32
		
		){
			changeState(new VavaBurensen1(), true);	
			player.superAmmo = 0;
			playSound("chingX4");
		}

		if (player.input.checkShoryuken(player, xDir, Control.R2) && player.superAmmo >= 32) {
			changeState(new RisingSpecterStart());
			player.superAmmo = 0;
		}
		


		return !isInDamageSprite();
	}


	public bool SpecialMoves() {
		
		if (player.input.isHeld(Control.Down, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isAPressed(player) 
		){
			changeState(new VAVAKamae(), true);	
			return true;
		}

		if (player.input.isBPressed(player) && player.input.isHeld(Control.Down, player)
		&& player.input.isLeftOrRightHeld(player)
		&& !grounded
		&& player.vileAmmo > 15){
			changeState(new GreenEyedLampState(), true);	
			player.vileAmmo -= 15;
			return true;
		}

		if (player.input.isHeld(Control.Up, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isR2Pressed(player) 
		&& sprite.name.Contains("dash")
		&& !sprite.name.Contains("end")
		){
			changeState(new OvosFritosStart(), true);	
			return true;
		}

		if (player.input.isHeld(Control.Down, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isR2Pressed(player) 
		&& sprite.name.Contains("dash")
		&& !sprite.name.Contains("end")
		){
			changeState(new Vava1GizmoDash(), true);	
			return true;
		}


		if (player.input.isHeld(Control.Up, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isR2Pressed(player) 
		&& (upPressedTimes > 2 || charState is VKote || charState is VAVAUpperCutPunch)
		){
			changeState(new OvosFritosStart(), true);	
			upPressedTimes = 0;
			return true;
		}

		if (player.input.isHeld(Control.Down, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isR2Pressed(player) 
		&& downPressedTimes > 2
		){
			changeState(new Vava1GizmoDash(), true);	
			downPressedTimes = 0;
			return true;
		}


		if (player.vileAmmo >= 15 && canDash() && player.speedDevil &&
			downPressedTimes >= 2 && player.input.isHeld(Control.Down, player) && player.input.isHeld(Control.Dash, player)) {
			changeState(new VileDashChargeState());
			player.vileAmmo -= 15;
			return true;
		}

		return !sprite.name.Contains("hurt") ||
		!sprite.name.Contains("frozen") ||
		!sprite.name.Contains("grabbed") || 
		!sprite.name.Contains("knocked") || 
		!sprite.name.Contains("launched") || 
		!sprite.name.Contains("thrown") || 
		!sprite.name.Contains("die") || 
		!sprite.name.Contains("lose") ||
		!sprite.name.Contains("stunned");
	}

	public override bool attackCtrl() {
		bool WeaponRightHeld = player.input.isPressed(Control.WeaponRight, player);

		SpecialMoves();
		if (WeaponRightHeld && player.vileAmmo > 0) {
		getVulcanMoves();
		}
		if (!player.input.checkHadoken(player, xDir, Control.Shoot)
		&& !player.input.checkShoryuken(player, xDir, Control.Shoot)
		&& charState is not VAVAKamae) {
			if (player.input.isAPressed(player)) {
				if (grounded) {
					if (player.input.isHeld(Control.Up, player) && player.input.isLeftOrRightHeld(player)) {
						if (player.vileAmmo >= 10) {
							changeState(new InfinityGigAttack(), true);
							player.vileAmmo -= 10;
						}			
					}
					 else if (player.input.isHeld(Control.Up, player) && !player.input.isLeftOrRightHeld(player)) {
						if (upPressedTimes >= 2) {
							if (player.vileAmmo >= 20) {
								changeState(new EgotisticalPillAttack(), true);
								upPressedTimes = 0;
							}
						} else {
							if (player.vileAmmo >= 14) {
								changeState(new SpoiledBratPunch(), true);
							}
						}
					}
					
				 	else if (player.input.isLeftOrRightHeld(player)) {
						if (!player.input.isHeld(Control.Down, player)) {
							if (player.vileAmmo >= 8) {
								changeState(new GoGetterRightAttack(), true);
							}
						}
					} else {
						if (!player.input.isHeld(Control.Down, player)) {
							if (charState is not InfinityGigAttack or SpoiledBratPunch) {
								changeState(new VAVAJab1(), true);
							}
						} else {
							if (downPressedTimes >= 2 && player.vileAmmo >= 26) {
								changeState(new VAVAGoldenRight(), true);
								player.vileAmmo -= 26;
								downPressedTimes = 0;
							} else {
								changeState(new VAVAUpperCutPunch(), true);
							}
						}
					}
				} else {
					if (player.input.isHeld(Control.Up, player) && player.input.isLeftOrRightHeld(player)) {
							if (player.vileAmmo >= 6) {
							changeState(new InfinityGigAttack(), true);
							player.vileAmmo -= 6;
						}			
					} else {
						if (player.vileAmmo >= 4) {
							changeState(new SpoiledBratPunch(), true);

						}
					}
				}
			}
		}
		

		if (player.input.isBPressed(player)) {
			if (grounded) {
				if (player.input.isHeld(Control.Up, player) && player.vileAmmo >= 6) {
					changeState(new WildHorseKickState(), true);
					player.vileAmmo -= 6;
				} else if (player.input.isHeld(Control.Down, player)) {
					if (downPressedTimes >= 2) {
						if (player.vileAmmo >= 15) {
							changeState(new RumblingBangLaunch(), true);
							player.vileAmmo -= 15;
						}
					} else {
						if (player.vileAmmo >= 25) {
							changeState(new BumptyBoomGranadeLaunch(), true);
							player.vileAmmo -= 25;
						}
						}
				} else {
					if (player.input.isLeftOrRightHeld(player)) {
						changeState(new VavaVSlashRun(), true);
					} else {
                        	changeState(new VavaKneeAttack(), true);
					
                    
					}
					
				}
				
			} else {
				if (player.input.isHeld(Control.Down, player)) {
					if (player.vileAmmo > 8 && charState is not GreenEyedLampState)
					changeState(new SeaDragonRageState(), true);
				}else {
					if (player.vileAmmo > 10 && charState is not GreenEyedLampState) {
						if (player.input.isHeld(Control.Up, player)) {
							if (player.vileAmmo > 15){
								if (!player.input.isL2Held(player)) {
									if (getChargeLevel() > 2) {
										changeState(new SwordBouqueteLaunch());
										stopCharge();
									} else {
										changeState(new VavaWindCoil());
									}
								} else {
									if (getChargeLevel() > 2) {
										changeState(new BurningDriveState());
										stopCharge();
									} else {
										changeState(new TerriotiralPowState());
									}
										
								}
							player.vileAmmo -= 15;
							}
						} else {
								if (!player.input.isL2Held(player)) {
									changeState(new MetalBladeKnee());
								} else {
									changeState(new AirSplashHitGranadeLaunch(), true);			
									player.vileAmmo -= 10;
								}
						
							
						}
					}
				}
			}
		}

		if (player.input.isL2Held(player)) {
			if (player.input.isAPressed(player)) {

				int upOrDown = player.input.getYDir(player);
				changeState(new VavaVstrikeChain(), true);
				new StrikeChainProj(getShootPos(), xDir, this, player, player.getNextActorNetId(), upOrDown, true);
			
			}
			if (player.input.isPressed(Control.Dash, player) && CrimsonphantomCD == 0) {
				changeState(new CrimsonPhantomState(grounded), true);
				CrimsonphantomCD = 0.3f;
			}
		}


		 getCannonMoves();
		return base.attackCtrl();
	}

public float CannonCD;


	public void getCannonMoves() {
		if (!player.input.checkHadoken(player, xDir, Control.R2)
		&& !player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not Vava1GizmoDash
		&& charState is not OvosFritos
		&& charState is not OvosFritosStart
		) {
			// finally added Tridentline
			if (player.input.isR2Pressed(player)) {
				if (downPressedTimes >= 2 && player.vileAmmo > 15) {			
						changeState(new Vava1TridentLine(grounded), true);
						player.vileAmmo -= 15;
					downPressedTimes = 0;
				} else {
					if (CannonCD == 0) {
						shoot(0);
						CannonCD = 0.35f;
					}
				}
			}
					}
		
	}
	
	public void getVulcanMoves() {
		var vile = this;
			
				if (player.vileAmmo >= 7) {
						player.vileAmmo -= 7;
				
					playSound("vileMissile", true);
					new TorpedoProjX(pos.addxy(-xDir * 5, -32), xDir, this, player, player.getNextActorNetId(true), rpc: true);
		
					player.vileAmmo -= 15;
				}
		} 
			

	
	

	public bool getFlamethrowerMoves() {
		if (getChargeLevel() > 2) {
			changeState(new BurningDriveState());
			stopCharge();
		}  else
		if (player.input.isHeld(Control.Down, player) && !player.input.isLeftOrRightHeld(player)) {
			if (player.vileAmmo > 8)
				changeState(new SeaDragonRageState(), true);
			return true;
		} else 

		if (player.input.isHeld(Control.Down, player) && player.input.isLeftOrRightHeld(player)) {
			if (player.vileAmmo > 8)
				changeState(new GreenEyedLampState(), true);
			return true;
		} else

		if (player.input.isHeld(Control.Up, player)) {
            	changeState(new DragonsWrathState(), true);
        } else {
            changeState(new WildHorseKickState(), true);
        }
		
							


		return false;
	}




	public bool RideArmorAttacks() {
		var raState = charState as InRideArmor;
		bool Goliath = rideArmor?.raNum == 4;
		bool stunShotPressed = player.input.isBPressed(player);
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



	//Bonus VAVA CD Stuff
	public float CrimsonphantomCD;

	public float ShieldHealthCD;

	public float ThirstTimer;


	public bool dashGrabSpecial() {
		if (charState is Dash || charState is AirDash) {
			charState.isGrabbing = true;
			if (getChargeLevel() == 2) {
				charState.superArmor = true; //peakbalance
				stopCharge();
			}
			if (getChargeLevel() > 2) {
				invulnTime = 0.5f;
				stopCharge();
			}
			if (!sprite.name.Contains("grab")){
			changeSpriteFromName("dash_grab", true);
			}
			charState.sprite = "dash_grab";
			return true;
		}
		return false;
	}




	public override bool normalCtrl() {
		if (sprite.name.EndsWith("cannon_air") && isAnimOver()) {
			changeSpriteFromName("fall", true);
		}

		if (player.input.isL2Held(player) &&
			!isAttacking() &&
			charState is not BlockWCUT
		) {
			
			changeState(new BlockWCUT(), true);
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

	public override bool chargeButtonHeld() {
		return player.input.isR2Held(player);
		
	}


	
	// Shoots stuff. VAVA(WCUT)
	public override void shoot(int chargeLevel) {


		if (chargeLevel == 0) {
			stopCharge();
			if (player.vileAmmo > 9) {
				if (!player.input.isL2Held(player)) {
					changeState(new Vava1Stunshot(grounded, false), true);
				} else {
					changeState(new VavaTomahawk(), true);
					invulnTime = 0.2f;
					var tomahawk1 = new VileMaroonedTomahawk(
					getShootPos(), xDir, this, player,
					player.getNextActorNetId(), rpc: true
						);
					var tomahawk2 = new VileMaroonedTomahawk(
				getShootPos(), xDir, this, player,
				player.getNextActorNetId(), rpc: true
					);
					tomahawk2.vel.y = -30;
				}
				
				player.vileAmmo -= 10;
			}
		} else if (chargeLevel == 1) {
			if (!player.input.isL2Held(player)) {
			player.loadout.vileLoadout.cannon = (int)VileCannonType.FrontRunner;
			cannonWeapon.type = (int)VileCannonType.FrontRunner;
			cannonWeapon.vavaShoot(0, this);
			} else {
                new VileParasiteSword(
				getShootPos(), xDir, this, player,
				player.getNextActorNetId(), rpc: true);
				player.vileAmmo -= 20;
            }
			stopCharge();
		} else if (chargeLevel == 2) {
			if (!player.input.isL2Held(player)) {
			player.loadout.vileLoadout.cannon = (int)VileCannonType.FatBoy;
			cannonWeapon.type = (int)VileCannonType.FatBoy;
			cannonWeapon.vavaShoot(0, this);
			} else {
				new MetalCrescent(
			getShootPos(), xDir, this, player,
			player.getNextActorNetId(), rpc: true);
				Global.level.delayedActions.Add(new DelayedAction(() => {
					new MetalCrescent(
				getShootPos(), xDir, this, player,
				player.getNextActorNetId(), rpc: true);
				}, 0.15f));
				Global.level.delayedActions.Add(new DelayedAction(() => {
					new MetalCrescent(
				getShootPos(), xDir, this, player,
				player.getNextActorNetId(), rpc: true);
				}, 0.25f));
				player.vileAmmo -= 20;
			}
			stopCharge();
		} else if (chargeLevel == 3) {
			changeState(new Vava1FatBoy(false, false), true);
			player.vileAmmo -= 30;
			stopCharge();
		} else if (chargeLevel >= 4) {
				if (player.input.isHeld(Control.Down, player)) {
				changeState(new NecroBurstAttack(grounded), true);
			} else if (player.input.isHeld(Control.Up, player)) {
				changeState(new NervousGhostState(grounded), true);
			} else if (player.input.isLeftOrRightHeld(player)) {
				changeState(new StraightNightmareAttack(grounded), true);
			}
			stopCharge();
		}
		if (chargeLevel >= 1) {
			stopCharge();
		}
		player.syncLoadout();
	}




	public override bool canCharge() {
		return !isInvulnerableAttack() && charState is not Die && invulnTime == 0;
	}


	public override int getMaxChargeLevel() {
		return 4;
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
		} else if (canShoot()) {
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
		if ((isVileMK5 || isVavaV) && linkedRideArmor != null &&
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
		if (!(isVileMK5 || isVavaV) || linkedRideArmor == null) {
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

		if ((isVileMK5 || isVavaV) && linkedRideArmor != null) {
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
		return (isVileMK5 || isVavaV) && linkedRideArmor?.character == this;
	}

	public bool canVileHover() {
		return (isVileMK5 || isVavaV) && player.vileAmmo > 0 && flag == null;
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
					if (isVileMK2 || isVava2) {
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
					if (isVileMK5 || isVavaV) {
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

	public override Point getVileShootVel(bool aimable) {
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
		} else if (charState is ShoulderCannon or Vava1TridentLine) {
			if (frameIndex == 12) vel = new Point(1, 0.5f);
			if (frameIndex == 15) vel = new Point(1, -0.5f);
		} else if (player.input.isHeld(Control.Up, player)) {
			if (!canVileAim60Degrees() || (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		} else if (player.input.isHeld(Control.Down, player) && charState is not Crouch) {
			vel = new Point(1, 0.5f);
		} else if (player.input.isHeld(Control.Down, player) && player.input.isLeftOrRightHeld(player) && charState is Crouch) {
			vel = new Point(1, 0.5f);
		}

		if (charState is NervousGhostState) {
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
			proj = new GenericMeleeProj(new VileStomp(), centerPoint, ProjIds.VileStomp, player, 0, 0, 0
			, addToLevel : true);
		}

			if (sprite.name.Contains("drop_kick")) {
			proj = new GenericMeleeProj(new VileStomp(), centerPoint, ProjIds.DropSlide, player, 1, 0, 0
			, addToLevel : true);
		}


		if (sprite.name.Contains("burensen_2")) {
			proj = new GenericMeleeProj(new VileStomp(), centerPoint, ProjIds.BurensenStomp, player, 1, 0, 0
			, addToLevel : true);
		}

		if (sprite.name.Contains("execution")) {
			proj = new GenericMeleeProj(new VileStomp(), centerPoint, ProjIds.BlockableLaunch, player, 4, 0, 0
			, addToLevel : true);
		}




		if (sprite.name.Contains("vilemk5_stomp")) {
			proj = new GenericMeleeProj(new VileStomp(),
			centerPoint, ProjIds.VileStomp2, player, 0.3f, 0, 5f, addToLevel : true);
		}

		if (sprite.name.Contains("air_bomb_attack")) {
			proj = new GenericMeleeProj(new MechFrogStompWeapon(),
			centerPoint, ProjIds.MechFrogStompShockwave, player, 0, 0, 0, addToLevel : true);
		}
		if (sprite.name.Contains("violentcrusher_grab")) {
			proj = new GenericMeleeProj(new MechFrogStompWeapon(),
			centerPoint, ProjIds.MechFrogStompShockwave, player, 3, 0, 10, addToLevel : true);
		}


		if (sprite.name.Contains("burensen_1")) {
			proj = new GenericMeleeProj(	new KRMelee(), centerPoint, ProjIds.BurensenStart, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: true, hitSound : "kofhtsnd_clamp1", isJuggleProjectile : true);
		}

		if (sprite.name.Contains("dash_grab")) {
			proj = new GenericMeleeProj(new VileMK2Grab(), centerPoint, 
			ProjIds.VileMK2Grab2, player, 0, 0, 120, addToLevel : true);
		}

		if (sprite.name.Contains("unpo_grab") || sprite.name.Contains("ovos_fritos")) {
			proj = new GenericMeleeProj(new MechFrogStompWeapon(), centerPoint, 
			ProjIds.newUpGrab, player, 0, 0, 0, addToLevel : true);
		}


		if (sprite.name.Contains("strike_chain") ) {
			if (sprite.name.Contains("grounded") ){
			proj = new GenericMeleeProj(new VileMK2Grab(), centerPoint, 
			ProjIds.VileMK2Grab2, player, 0, 0, 120, addToLevel : true);
			} else {
			proj = new GenericMeleeProj(new VileMK2Grab(), centerPoint, 
			ProjIds.newUpGrab, player, 0.5f, 0, 0, addToLevel : true);
			}
		}


		if (sprite.name.Contains("hoticecle")) {
			proj = new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.Hyouretsuzan2, player,
				2f, 0, 10f, isDeflectShield: true, clashTier: ClashTier.Weak
			, addToLevel : true);
		}



		if (sprite.name.Contains("block") && !sprite.name.Contains("kamae")) {
			proj = new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true, isShield: true
			, addToLevel : true);
		}


		if (sprite.name.Contains("slashrun")) {
			proj = new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.ForceGrabState, player,
				2f,0, 15, isDeflectShield: true, clashTier: ClashTier.Weak,
				isPushProjectile: true,
				 isZSaberEffect2: true
			, addToLevel : true);
		}


		if (sprite.name.Contains("jab")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				1f, 25, 15f, isDeflectShield: true, clashTier: ClashTier.Weak, hitSound : "htsnd_slash_deep1"
			, addToLevel : true);
		}

		if (sprite.name.Contains("knee")) {
			proj = new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				1f, 25, 5f, isDeflectShield: true, clashTier: ClashTier.Weak, hitSound : "htsnd_slash_deep3"
			, addToLevel : true);
		}

		if (sprite.name.Contains("green_eyed_lamp")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.BlockableWeakLaunch, player,
				1f, 35, 5f, isDeflectShield: true, clashTier: ClashTier.Weak
			, addToLevel : true);
		}

		if (sprite.name.Contains("punch_1")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				1f, 25, 15f, isDeflectShield: true, clashTier: ClashTier.Weak, hitSound : "htsnd_slash_deep2"
			, addToLevel : true);
		}

		if (sprite.name.Contains("punch_2")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.BlockableWeakLaunch, player,
				2, 0, 15f, isDeflectShield: true, clashTier: ClashTier.Weak, hitSound : "htsnd_slash_deep3"
			, addToLevel : true);
		}
		if (sprite.name.Contains("kick")  && !sprite.name.Contains("drop") && !sprite.name.Contains("kick_3") && !sprite.name.Contains("super")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				1, 25, 15f, isDeflectShield: true, clashTier: ClashTier.Weak, hitSound : "kofhtsnd_clamp2"
			, addToLevel : true);
		}
		if (sprite.name.EndsWith("superkick_up")
		) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileAirRaidPlusKnock, player,
				2, 0, 15f, isDeflectShield: true, hitSound : "kofhtsnd_clamp2"
			, addToLevel : true);
		}
		if (sprite.name.EndsWith("superkick")
		) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.BurensenEND, player,
				2, 0, 15f, isDeflectShield: true, hitSound : "kofhtsnd_megapunch1"
			, addToLevel : true);
		}


		if (sprite.name.Contains("hyperdash_attack") && !sprite.name.Contains("b")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.HeavyPush, player,
				2, 0, 10f, isDeflectShield: true, clashTier: ClashTier.Weak
			, addToLevel : true);
		}

		if (sprite.name.Contains("hyperdash_attack") && sprite.name.Contains("b")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.ForceGrabState, player,
				0.5f, 0, 10f, isDeflectShield: true, clashTier: ClashTier.Weak
			, addToLevel : true);
		}
		
		if (sprite.name.Contains("hyperdash_end")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.BurensenEND, player,
				2, 0, 15f, isDeflectShield: true
			, addToLevel : true);
		}

		if (sprite.name.Contains("kick_3")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.VileAirRaidPlusKnock, player,
				2, 0, 15f, isDeflectShield: true
			, addToLevel: true);
		}


		if (sprite.name.Contains("kote")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.HeavyPush, player,
				2, 0, 15f, isDeflectShield: true
			, addToLevel: true);
		}

		if (sprite.name.Contains("unblockable")) {
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.MechFrogStompShockwave, player,
				2, 0, 15f, isDeflectShield: true
			, addToLevel: true);
		}


		
		if (sprite.name.Contains("spring_grab")) {
		
			return new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.GizmoGrab, player,
				0, 0, 15f, isDeflectShield: true
			, addToLevel: true);
		
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
				if (VileMode == 1
			&& Global.sprites.ContainsKey("vilemk5_b_" + spriteName)) {
			return "vilemk5_b_" + spriteName;
		}
			return "vilemk5_" + spriteName;
		

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
		if (flag != null || !isDashing) {
			return getRunSpeed();
		}
		float dashSpeed = 210;

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



	public override bool isToughGuyHyperMode() {
		return sprite.name.Contains("grab")
		|| charState is VileStationaryHover
		|| charState is VileHover
		|| charState is VavaVSlashRun;
	}

	public virtual bool isStatusImmune() {
		return charState is HexaInvoluteState;
	}

}