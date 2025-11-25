
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class VAVA1 : Vile {


	public float aiAttackCooldown;
	public const float maxCalldownMechCooldown = 2;
	public float grabCooldown = 1;
	public bool vulcanActive;
	public float vulcanLingerTime;
	public const int callNewMechCost = 5;
	float mechBusterCooldown;
	public bool usedAmmoLastFrame;
	public int buckshotDanceNum;
	public float vileAmmoRechargeCooldown;
	public bool isShootingLongshotGizmo;
	public int longshotGizmoCount;
	public float gizmoCooldown;
	public bool hasFrozenCastle;
	public bool hasSpeedDevil;
	public bool summonedGoliath;
	public int vileForm;

	public bool phase2;
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

	public float CannonCD;

	

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




	public VileSaveData saveData {
		get {
			return VileSaveData.vileSaveData;
		}
	}


	public float stockedTime;




	public VAVA1(
				Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, bool mk2VileOverride = false, bool mk5VileOverride = false,
		VileLoadout? loadout = null,
		int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn, false , false , player.loadout.vileLoadout, heartTanks, isATrans
	) {


		charId = CharIds.VAVA1;

		ShouldExplode = true;
	
		if (charState is WarpIn) player.superAmmo = 0;

		spriteFrameToSounds["vava_run/4"] = "vileWalk";
		spriteFrameToSounds["vava_run/8"] = "vileWalk";
		chargeSound = new LoopingSound("charge_start_vile", "charge_loop_vile", this);



		loadout ??= player.loadout.vileLoadout.clone();
		this.loadout = loadout;

		vulcanWeapon = loadout.vulcan switch {
			1 => new DistanceNeedler(),
			2 => new BuckshotDance(),
			3 => new NoneVulcan(),
			_ => new CherryBlast()
		};
		cannonWeapon = new VileCannonWC(0);
		missileWeapon = loadout.missile switch {
			1 => new HumerusCrush(),
			2 => new PopcornDemon(),
			3 => new NoneMissile(),
			_ => new ElectricShock()
		};
		rocketPunchWeapon = loadout.rocketPunch switch {
			1 => new SpoiledBrat(),
			2 => new InfinityGig(),
			3 => new NoneRocketPunch(),
			_ => new GoGetterRight()
		};
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
		cutterWeapon = loadout.cutter switch {
			1 => new ParasiteSword(),
			2 => new MaroonedTomahawk(),
			3 => new NoneCutter(),
			_ => new QuickHomesick()
		};
		flamethrowerWeapon = loadout.flamethrower switch {
			1 => new SeaDragonRage(),
			2 => new DragonsWrath(),
			3 => new NoneFlamethrower(),
			_ => new WildHorseKick()
		};
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



	public override bool normalCtrl() {
		if (player.input.isL2Held(player) && grounded) {
			changeState(new BlockWCUT());

		}
	//	if (player.input.isPressed(Control.Special2, player)
	//	&& player.currency > 4
	//	) {
	//		player.currency -= 5;
	//	}

		return base.normalCtrl();
	}


	

	public override bool spcCancel() {
		

		// Dash Cancel
		if (player.dashPressed(out string dashControl)) {
			if (grounded) {
				changeState(new Dash(dashControl), true);
			} else {
				changeState(new AirDash(dashControl), true);
			}
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
		if (player.input.checkShoryuken2(player, xDir, Control.Special1) && player.superAmmo >= 32){
			changeState(new VavaBurensen1(), true);	
			player.superAmmo = 0;
			playSound("chingX4");
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

		if (player.input.isHeld(Control.Down, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isR2Pressed(player) 
		&& sprite.name.Contains("dash")
		&& !sprite.name.Contains("end")
		){
			changeState(new Vava1GizmoDash(), true);	
			return true;
		}

		if (player.vileAmmo >= 15 && canDash() &&
			downPressedTimes >= 2 && player.input.isHeld(Control.Down, player) && player.input.isHeld(Control.Dash, player)) {
			changeState(new VileDashChargeState());
			player.vileAmmo -= 15;
			return true;
		}

		return false ;
	}

	public override bool attackCtrl() {
		bool WeaponRightHeld = player.input.isHeld(Control.WeaponRight, player);

		SpecialMoves();
		if (WeaponRightHeld) {
			if (player.input.isHeld(Control.Up, player)) {
                if (player.vileAmmo >= 15) {
					changeState(new VavaDistantNeedler(), true);
					player.vileAmmo -= 15;
				}
            }
			if (charState is Crouch) {
				
					changeState(new VavaZipZapper(), true);
					
				
			} else {
				vulcanWeapon.vileShoot(0, this);
			}
		}
		if (!player.input.checkHadoken(player, xDir, Control.Shoot)
		&& !player.input.checkShoryuken(player, xDir, Control.Shoot)
		&& charState is not VAVAKamae) {
			if (player.input.isAPressed(player)) {
				if (grounded) {
					if (player.input.isHeld(Control.Up, player) && player.input.isLeftOrRightHeld(player)) {
						if (player.vileAmmo >= 25) {
							changeState(new InfinityGigAttack(), true);
							player.vileAmmo -= 25;
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
							if (player.vileAmmo >= 25) {
							changeState(new InfinityGigAttack(), true);
							player.vileAmmo -= 25;
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
				if (player.input.isHeld(Control.Up, player)) {
					changeState(new WildHorseKickState(), true);
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
					} else {
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
										changeState(new PeaceOutRollerAttack());
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
									changeState(new ExplosiveRoundState());
								} else {
									changeState(new AirSplashHitGranadeLaunch(), true);			
								}
						
							player.vileAmmo -= 10;
						}
					}
				}
			}
		}

		if (player.input.isL2Held(player)) {
			if (player.input.isAPressed(player)) {
				changeState(new Vava1GrabStartState(), true);
			}
			if (player.input.isPressed(Control.Dash, player) && CrimsonphantomCD == 0) {
				changeState(new CrimsonPhantomState(grounded), true);
				CrimsonphantomCD = 0.3f;
			}
		}


		 getCannonMoves();
		return base.attackCtrl();
	}

	public bool getCannonMoves() {
		if (!player.input.checkHadoken(player, xDir, Control.R2)
		&& !player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not Vava1GizmoDash
		) {
			// finally added Tridentline
			if (player.input.isR2Pressed(player)) {
				if (downPressedTimes >= 2 && player.vileAmmo > 15) {
					if (saveData.vavaDataSave.Tridentline) {
						changeState(new Vava1TridentLine(grounded), true);
						player.vileAmmo -= 15;
					}
					downPressedTimes = 0;
				} else {
					if (CannonCD == 0) {
						shoot(0);
						CannonCD = 0.35f;
					}
				}
			}
			return true;
		}
		return false;
	}
	
	public bool getVulcanMoves() {
			if (player.input.isHeld(Control.Up, player)) {
				if (player.vileAmmo >= 15) {
					changeState(new VavaDistantNeedler(), true);
					player.vileAmmo -= 15;
				}
			}
			if (charState is Crouch) {
				changeState(new VavaZipZapper(), true);
			} else {
				vulcanWeapon.vileShoot(0, this);
			}

		return false;
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
	
	public override void update() {
		base.update();
		// DisrespectFactor


		
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

			// vileteleport
		if (charState is VileDodge &&
		linkedRideArmor != null &&
		player.input.isPressed(Control.Up, player)) {
			changeState(new VileTeleport(linkedRideArmor.pos), true);
		}



		bool PressA = player.input.isPressed(Control.Shoot, player);
		
		if (PressA && charState is VileStompState && frameIndex > 2) {
			changeState(new VileSuperKickState(), true);
		}
		if (PressA && charState is VileChainGrabState && frameIndex > 2) {
			changeState(new VilePunch1(), true);
		}


		if (charState is VAVAJab1 or VAVAJab2 or VAVAUpperCutPunch){
		if (player.input.isHeld(Control.Down, player)
		&& player.input.isLeftOrRightHeld(player)
		&& player.input.isBPressed(player)  && grounded
		){
			changeState(new VMissiLeStance(), true);	
		}
		}


	
	
		if (inCombatCooldown > 0) {
            if (!isInDamageSprite() && charState is not Taunt) {
			
                if (player.input.isPressed(Control.Taunt, player)) {
                    if (Helpers.randomRange(0, 1 ) == 0) {
                        changeState(new ZainParryShinStartState(), true);
                    } else {
                        changeState(new GlobalParryState(), true);
                    }
                }
            }
        }


		if (overDriveTimer > 0) {
			OverDrive = true;
			// test
			saveData.vavaDataSave.Tridentline = true;
			saveData.saveToFile();
		} else {
			OverDrive = false;
		}
		if (isVileMK5) {
			phase2 = true;
		}
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);
		Helpers.decrementTime(ref CrimsonphantomCD);
		Helpers.decrementTime(ref grabCooldown);
		Helpers.decrementTime(ref mechBusterCooldown);
		Helpers.decrementTime(ref gizmoCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref CannonCD);



		if (player.isAI && health < 5 && !phase2 && !isWarpIn() && isBossVile && AIStart) {
			changeState(new VAVAPhase2Start(false), true);
			stopMoving();
			bonusHealth = 60;
		}

		// Hypermode music.
		if (!Global.level.isHyper1v1()) {
			if (phase2 && ownedByLocalPlayer) {
				if (musicSource == null) {
					addMusicSource("fortressBoss_X1", getCenterPos(), true);
				}
			} else {
				destroyMusicSource();
			}
		}

		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
			}
		}

		if (player.superAmmo >= player.superMaxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				player.superAmmo = Helpers.clampMax(player.superAmmo + 1, player.superMaxAmmo);
				playSound("healX3", forcePlay: true, true);
			}
		}

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
				if (isVileMK1) {
					playSound("heal", forcePlay: true, true);
				} else {
					playSound("healX3", forcePlay: true, true);
				}
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


		if (charState is InRideChaser) {
			return;
		}
		RideArmorAttacks();
		RideLinkMK5();

		Supers();
		if (!charState.attackCtrl || charState is VileMK2GrabState) {
			return;
		}
		chargeLogic(shoot);
	}







	public Sprite? getCannonSprite(out Point poiPos, out int zIndexDir) {
		poiPos = getCenterPos();
		zIndexDir = 0;

		string vilePrefix = "vava_";
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


	public override bool canDash() {
		return flag == null;
	}

	public override string getSprite(string spriteName) {
		return "vava_" + spriteName;
	}


	public enum MeleeIds {
		None = -1,
		Blocking,
		KamaeBlock,
		Jab,
		Jab2,
		UpperCut,
		Grab,
		Grabmk2dash,
		KamaeUnB,
		HotIcecle,
		BurensenStart,
		BurensenStomp,
		BurensenEND,
		GreenEyedLamp,
		BurensenENDCPU,
		RagingDemon,
		Kote,

		GizmoGrab,
		DropKick,
		CannonExecution,
		DeadLiftEX,
		GoldenRight,
		StompStart,
		GrabNonFlinchAT,

		SpeedDemon,
	}


	// VAva melee stuff
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"vava_crouch_start"  => MeleeIds.StompStart,
			"vava_block"  => MeleeIds.Blocking,
			"vava_grab_attack" or "vava_grab_down"  => MeleeIds.GrabNonFlinchAT,
			"vava_deadlift" => MeleeIds.DeadLiftEX,
			"vava_golden_right" => MeleeIds.GoldenRight,
			"vava_kamae" or "vava_kamae_dash" or "vava_kamae_backdash" => MeleeIds.KamaeBlock,
			"vava_jab_1" => MeleeIds.Jab,
			"vava_jab_2" => MeleeIds.Jab2,
			"vava_punch_2" => MeleeIds.UpperCut,
			"vava_gizmo_dash_grab" => MeleeIds.GizmoGrab,
			"vava_kamae_unblockable" or "vava_kamae_unblockable_land" => MeleeIds.KamaeUnB,
			"vava_kamae_kote" => MeleeIds.Kote,
			"vava_spring_grab" => MeleeIds.Grab,
			"vava_dash_grab" => MeleeIds.Grabmk2dash,
			"vava_hoticecle" => MeleeIds.HotIcecle,
			"vava_drop_kick" => MeleeIds.DropKick,
			"vava_cannon_execution" => MeleeIds.CannonExecution,
			"vava_green_eyed_lamp" => MeleeIds.GreenEyedLamp,
			"vava_burensen_1" => MeleeIds.BurensenStart,
			"vava_burensen_2" or "vava_stomp" => MeleeIds.BurensenStomp,
			"vava_ragingdemon_dash" => MeleeIds.RagingDemon,
			"vava_burensen_finish" when !player.isAI => MeleeIds.BurensenEND,
			"vava_burensen_finish" or "vava_hyperdash_attack" when player.isAI => MeleeIds.BurensenENDCPU,
			"vava_hyperdash_attack" or "vava_missile_stance" =>  MeleeIds.SpeedDemon,

			"vava_superkick"  => MeleeIds.BurensenENDCPU,
			
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockingProjID, player, damage: 0.0f,
				flinch: 0, hitCooldown: 0, isShield: false, isReflectShield: false,
				isDeflectShield: true, isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel


				),

			
			(int)MeleeIds.StompStart => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.VileStomp, player,
				 0, 0, 0, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.Grab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 0, 0, 0, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.GizmoGrab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GizmoGrab, player,
				 0, 0, 0, isReflectShield: false,
				isZSaberClang: true, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Grabmk2dash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.VileMK2Grab, player,
				 0, 0, 0, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.UpperCut => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.SpinningBlade, player,
				 2, 40, 42, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "htsnd_punch_3", isJuggleProjectile : true
			),
			(int)MeleeIds.GrabNonFlinchAT => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.SpinningBlade, player,
				 1, 0, 10, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_punch1", isJuggleProjectile : true
			),
			(int)MeleeIds.DropKick => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MechFrogGroundPound, player,
				 2, 40, 42, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, isJuggleProjectile : true
			),
			(int)MeleeIds.CannonExecution => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				 2, 0, 42, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, isJuggleProjectile : true
			),
			(int)MeleeIds.KamaeBlock => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab1, player,
			 0.25f, 5, 10, isReflectShield: true,
			isZSaberClang: true, isZSaberEffect: true,
			addToLevel: addToLevel, hitSound : "htsnd_slash1", isJuggleProjectile : true
			),
			(int)MeleeIds.Jab => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab1, player,
			 1, 20, 25, isReflectShield: true,
			isZSaberClang: true, isZSaberEffect: true,
			addToLevel: addToLevel, hitSound : "htsnd_punch_1", isJuggleProjectile : true
			),
			(int)MeleeIds.Jab2 => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab2, player,
			 1, 26, 20, isReflectShield: true,
			isZSaberClang: true, isZSaberEffect: true,
			addToLevel: addToLevel, hitSound : "htsnd_punch_2", isJuggleProjectile : true
			),


			(int)MeleeIds.KamaeUnB => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MechFrogStompShockwave, player,
				3, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true
			),

			(int)MeleeIds.DeadLiftEX => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				2, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true
			),


			(int)MeleeIds.GoldenRight => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				3, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "dbzclang", isJuggleProjectile : true
			),




			(int)MeleeIds.Kote => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				3, 40, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp2", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenStart => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenStart, player,
				2, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp1", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenStomp => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenStomp, player,
				1, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp2", isJuggleProjectile : true
			),


			(int)MeleeIds.SpeedDemon => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.HeavyPush, player,
				2, 0, 30, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp2", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenEND => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenEND, player,
				2, 0, 30, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_megapunch1", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenENDCPU => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenEND, player,
				4, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_megapunch1", isJuggleProjectile : true
			),

			(int)MeleeIds.RagingDemon => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.RagingDemon, player,
				5, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, isJuggleProjectile : true
			),

			(int)MeleeIds.HotIcecle => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Hyouretsuzan2, player,
				3, 30, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "htsnd_glass", isJuggleProjectile : true
			),

			(int)MeleeIds.GreenEyedLamp => new GenericMeleeProj(
				new RyuenjinWeapon(), projPos, ProjIds.Ryuenjin, player,
				3, 30, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true
			),

			_ => null
		};
		return proj;
	}



	// Ammo section
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
		 player.vileAmmo += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return (player.superAmmo < player.superMaxAmmo);
	}

	public override bool canCharge() {
		return !isInvulnerableAttack();
	}

	public override void chargeGfx() {
		if (ownedByLocalPlayer) {
			chargeEffect.stop();
		}
		if (isCharging()) {
			chargeSound.play();
			int chargeType = 0;
			chargeEffect.update(getChargeLevel(), chargeType);
		}
	}

	public override int getMaxChargeLevel() {
		return 4;
	}




	public override bool chargeButtonHeld() {
		return player.input.isR2Held(player);
	}

	public override void increaseCharge() {
		float factor = 1;
		if (OverDrive) factor = 1.2f; // this means during OverDrive he gets a chargespeed buff
		chargeTime += Global.speedMul * factor;
	}

	public override float getRunSpeed() {
		float runSpeed = 90;
		if (OverDrive) { // this means during OverDrive he gets a speed buff
			runSpeed *= 1.15f;
		}
		return runSpeed * getRunDebuffs();
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
			cannonWeapon.type = (int)VileCannonType.FatBoy;
			cannonWeapon.vavaShoot(0, this);
			stopCharge();
		} else if (chargeLevel >= 4) {
				if (player.input.isHeld(Control.Down, player)) {
				changeState(new NecroBurstAttack(grounded), true);
			} else if (player.input.isHeld(Control.Up, player)) {
				changeState(new RisingSpecterState(grounded), true);
			} else if (player.input.isLeftOrRightHeld(player)) {
				changeState(new StraightNightmareAttack(grounded), true);
			}
			stopCharge();
		}
		if (chargeLevel >= 1) {
			stopCharge();
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



	public void RideLinkMK5() {
		if (isVileMK5 && linkedRideArmor != null &&
			player.input.isPressed(Control.Special2, player) &&
			player.input.isHeld(Control.Down, player)
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
		if (!isVileMK5 || linkedRideArmor == null) {
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
			if (player.input.isBPressed(player) || player.input.isPressed(Control.WeaponLeft, player)) {
				rideMenuWeapon.isMenuOpened = false;
			}
		}

		if (isVileMK5 && linkedRideArmor != null) {
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
		return false;
	}

	public bool canVileHover() {
		return isVileMK5 && player.vileAmmo > 0 && flag == null;
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
					if (isVileMK2) {
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
					int raIndex = 6;
					linkedRideArmor = new RideArmor(player, pos, raIndex, 0, player.getNextActorNetId(), true, sendRpc: true);
					
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
			if (!(charState is Idle || charState is Run || charState is Crouch)) return;
			changeState(new CallDownMech(linkedRideArmor, false), true);
		}
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


	(float twitch, float grow, int time) omegaAura = new(0.015f, 0, 0);

	void updateOmegaAura() {
		omegaAura.twitch -= 0.05f;
		if (omegaAura.twitch < 0.05)
			omegaAura.twitch = 0.15f;

		if (omegaAura.time >= 0 && omegaAura.time < 50)
			omegaAura.grow += 0.0025f;
		else if (omegaAura.time >= 55 && omegaAura.time < 105)
			omegaAura.grow -= 0.0025f;

		omegaAura.time++;
		if (omegaAura.time > 110) {
			omegaAura.time = 0;
		}
	}
	
	
	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;


		
		if (player.skinSlot == 1) {
			palette = player.nightmareZeroShader;
		}
		
		if (palette != null) {
			shaders.Add(palette);
		}
		if (shaders.Count == 0) {
			return baseShaders;
		}
		shaders.AddRange(baseShaders);
		return shaders;
	}


	public override void render(float x, float y) {
		addRenderEffect(RenderEffectType.SpeedDevilTrail);
		if (currentFrame.POIs.Length > 0) {
			Sprite? cannonSprite = getCannonSprite(out Point poiPos, out int zIndexDir);
			cannonSprite?.draw(
				cannonAimNum, poiPos.x, poiPos.y, getShootXDirSynced(),
				1, getRenderEffectSet(), alpha, 1, 1, zIndex + zIndexDir,
				getShaders(), actor: this
			);
		}


		// For drawing the growing aura that LastStand and Eigengrau Zero uses.
		if (visible && (OverDrive || phase2) ) {
			// Position to draw the sprite to.
			float auraSize = 1 + omegaAura.twitch + omegaAura.grow;
			float drawX = pos.x + x + (float)xDir * currentFrame.offset.x * auraSize;
			float drawY = pos.y + y + (float)yDir * currentFrame.offset.y * auraSize + 1;

			float auraAlpha = 0.75f;

			// Draw aura.
			Global.sprites[sprite.name].draw(
				sprite.frameIndex,
				drawX, drawY,
				xDir, yDir,
				null, auraAlpha,
				auraSize,
				auraSize,
				zIndex - 1,
				player.omegaAuraShader
			);
			updateOmegaAura();
		}


		if (player.isMainPlayer && isVileMK5 && vileHoverTime > 0 && charState is not HexaInvoluteState) {
			float healthPct = Helpers.clamp01((vileMaxHoverTime - vileHoverTime) / vileMaxHoverTime);
			float sy = -27;
			float sx = 20;
			if (xDir == -1) sx = 90 - 20;
			drawFuelMeter(healthPct, sx, sy);
		}
		base.render(x, y);
	}

	public bool dashGrabSpecial() {
		if (charState is Dash || charState is AirDash) {
			charState.isGrabbing = true;
			charState.superArmor = true; //peakbalance
			changeSpriteFromName("dash_grab", true);
			return true;
		}
		return false;
	}



	public float AIHellBarrageCD;

	public bool AIStart;

	public bool isBossVile;

	public override void aiAttack(Actor? target) {
		int Vattack = Helpers.randomRange(1, 7);
		Helpers.decrementFrames(ref AIHellBarrageCD);
		bool isTargetInAir = pos.y > target?.pos.y - 20;
		bool isTargetClose = target?.getCenterPos().distanceTo(getCenterPos()) < 50;
		bool isWishinRangedMoves = target?.getCenterPos().distanceTo(getCenterPos()) < 120;
		bool isFacingTarget = (pos.x < target?.pos.x && xDir == 1) || (pos.x >= target?.pos.x && xDir == -1);
		if (Global.level.is1v1()) {
			isBossVile = true;
		}
		if (isBossVile) {
			player.superAmmo = player.superMaxAmmo;
		} else {
			AIStart = true;
		}

		if (!AIStart && charState.attackCtrl) {
			if (isBossVile) {
				changeState(new VB1(), true);
				AIStart = true;
			}
			
		} else {

			if (!charState.isGrabbedState && !player.isDead && !isInvulnerableAttack()
						&& aiAttackCooldown <= 0 && charState.attackCtrl) {

				if (charState is Dash or AirDash && isFacingTarget && isBossVile) {
					charState.isGrabbing = true;
					charState.superArmor = true; // yes Cry Gsu I'm adding the annoying SuperArmor
					changeSpriteFromName("dash_grab", true);
				}


				if (isTargetClose && grounded) {
					switch (Vattack) {
						case 1 when isFacingTarget && player.superAmmo >= player.superMaxAmmo:
							changeState(new VavaBurensen1());
							player.superAmmo -= 32;
							break;
						case 2 when isFacingTarget:
							changeState(new SpoiledBratPunch());
							break;
						case 3 when isFacingTarget:
							if (isBossVile) {
								changeState(new InfinityGigAttackBossVer());
							} else {
								changeState(new VAVAUpperCutPunch());
							}
							break;
						case 4 when isFacingTarget:
							if (phase2) {
								changeState(new RagingDemonStart());
							} else {
								changeState(new Vava1GrabStartState());
							}
							break;
						case 5 when isFacingTarget:
							changeState(new VKamaeHotIcecle());
							break;
						case 6 when isFacingTarget:
							changeState(new VAVAKamae());
							break;
						case 7 when isFacingTarget:
							changeState(new VavaBurensen1());
							break;
					}
				}

				if (!grounded) {
					switch (Vattack) {
						case 1 when isFacingTarget:
							if (!isBossVile) {
								changeState(new ExplosiveRoundState());
							} else {
								changeState(new ExplosiveRoundStateBoss());
							}
							break;
						case 2 when isFacingTarget:
							changeState(new SpoiledBratPunch());
							break;
						case 3 when isFacingTarget:
							if (isBossVile) {
								changeState(new InfinityGigAttackBossVer());
							} else {
								changeState(new InfinityGigAttack());
							}
							break;
						case 4 when isFacingTarget:
							if (isBossVile) {
								changeState(new SpreadShotKnee());
							} else {
								changeState(new SeaDragonRageState());
							}
							break;
						case 5 when isFacingTarget:
							changeState(new PeaceOutRollerAttack());
							break;
						case 6 when isFacingTarget:
							changeState(new VKamaeUnblockableStart());
							break;
						case 7 when isFacingTarget:
							changeState(new GreenEyedLampState());
							break;
					}
				}

				if (!isTargetClose && grounded && isWishinRangedMoves) {
					switch (Vattack) {
						case 1 when isFacingTarget:
							if (isBossVile) {
								changeState(new ShoulderCannon(grounded));
							} else {
								shoot(0);
							}
							break;
						case 2 when isFacingTarget:
							changeState(new SpoiledBratPunch());
							break;
						case 3 when isFacingTarget:
							if (isBossVile) {
								changeState(new InfinityGigAttackBossVer());
							} else {
								changeState(new InfinityGigAttack());
							}
							break;
						case 4 when isFacingTarget:
							changeState(new VKamaeUnblockableStart());
							break;
						case 5 when isFacingTarget:
							changeState(new VKamaeDash());
							break;
						case 6 when isFacingTarget:
							changeState(new VileDashChargeState());
							break;
						case 7 when isFacingTarget:
							if (isBossVile) {
								changeState(new PopcornHell(grounded));
							} else {
								shoot(1);
							}
							break;
					}
				}

				aiAttackCooldown = Helpers.randomRange(0, 30);
			}

			if (charState is VAVAKamae or VKamaeBDash or VKamaeDash && charState.stateTime > 0.2f) {
				if (Helpers.randomRange(0, 4) == 0) {
					changeState(new VKamaeHotIcecle());
				}
				if (Helpers.randomRange(0, 4) == 1) {
					changeState(new VKamaeBDash());
				}
				if (Helpers.randomRange(0, 4) == 2) {
					changeState(new VKamaeDash());
				}
				if (Helpers.randomRange(0, 4) == 3) {
					changeState(new VKamaeUnblockableStart());
				}
				if (Helpers.randomRange(0, 4) == 4) {
					changeState(new VKote());
				}
			}
		}
		base.aiAttack(target);
	}

	public float aiBlocktime;

	public float aiDodgeCD;
	public override void aiDodge(Actor? target) {
		Helpers.decrementFrames(ref aiBlocktime);
		Helpers.decrementFrames(ref aiDodgeCD);
		foreach (GameObject gameObject in getCloseActors(64, true, false, false)) {
			if (gameObject is Projectile proj && proj.damager.owner.alliance != player.alliance &&
			(charState.attackCtrl || charState is ShoulderCannon or PopcornHell)) {
				//Projectile is not 
				if (!(proj.projId == (int)ProjIds.RollingShieldCharged || proj.projId == (int)ProjIds.RollingShield
					|| proj.projId == (int)ProjIds.MagnetMine || proj.projId == (int)ProjIds.FrostShield || proj.projId == (int)ProjIds.FrostShieldCharged
					|| proj.projId == (int)ProjIds.FrostShieldAir || proj.projId == (int)ProjIds.FrostShieldChargedPlatform || proj.projId == (int)ProjIds.FrostShieldPlatform)
				) {
					if (Helpers.randomRange(0, 1) == 1) {
						if (aiDodgeCD == 0) {
							if (phase2) {
								aiDodgeCD = Helpers.randomRange(0, 20);
							} else {
								aiDodgeCD = Helpers.randomRange(0, 60);
							}
							if (Helpers.randomRange(0, 1) == 1) {
								changeState(new CrimsonPhantomState(grounded), true);
							} else {
								changeState(new CrimsonPhantomState2(grounded), true);
							}
						}
					} else {
						if (!(proj.projId == (int)ProjIds.SwordBlock) && grounded
								&& aiBlocktime <= 0) {
							turnToInput(player.input, player);
							changeState(new BlockWCUT(), true);
							aiBlocktime = Helpers.randomRange(0, 60);
						}
					}
				}
			}
		}
	
		base.aiDodge(target);
	}
	

}
