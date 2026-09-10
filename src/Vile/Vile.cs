using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MMXOnline;

public class Vile : Character {
	public const float maxCalldownMechCooldown = 120;
	public float vulcanLingerTime;
	public const int callNewMechCost = 5;
	public const int mk5AltCost = 2;

	public float mechBusterCooldown;
	public bool usedAmmoLastFrame;
	public bool isShootingGizmo;
	public bool wasShootingVulcan;
	public bool isShootingVulcan => vulcanLingerTime > 0;
	public bool hasFrozenCastle;
	public bool hasSpeedDevil;
	public bool summonedGoliath;

	public bool phase2;
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

	public float vileAmmoRechargeCooldown;
	public bool isShootingLongshotGizmo;
	public int longshotGizmoCount;
	
	public float gizmoCooldown;
	public bool lastFrameWeaponLeftHeld;
	public bool lastFrameWeaponRightHeld;
	public int cannonAimNum;
	public float calldownMechCooldown;
	public VileAmmoWeapon energy = new();
	public VileCannon cannonWeapon;
	public VileLoadout loadout;
	public VileVulcan vulcanWeapon;

	public VileBall vileBall;
	public VileMissile missileWeapon;
	public RocketPunch rocketPunchWeapon;
	public VileNapalm napalmWeapon;
	public VileNapalm grenadeWeapon;
	public VileCutter cutterWeapon;
	public VileFlamethrower flamethrowerWeapon;
	public VileLaser laserWeapon;
	public MechMenuWeapon rideMenuWeapon;

	// For Classic Weapon Compatibility
	public List<Actor> junkShieldProjs = new();
	public LoopingSound? junkShieldSound;
	public Projectile? sWheel;
	public HardKnuckleVProj? HardKnuckleVProj;
	public bool armless;
	public ChargeEffect NoiseCrushVEffect;
	public bool hasChargedNoiseCrushV = false;
	public float NoiseCrushVAnimTime;
	public LoopingSound? chargedNoiseCrushVSound;
	public bool usedDoubleJump;
	public bool boughtSuperAdaptorOnce;
	public float timeSinceLastShoot;
	public bool isSlideColliding;

	public Weapon downAirSpWeapon;
	public Weapon airSpWeapon;
	public Weapon downSpWeapon;
	public float deadCooldown;
	public const float maxdeadCooldown = 60;
	public float[] chargeTimeEx = new float[3];
	public VileWeaponSystem weaponSystem;

	// When firing missile, you can't shoot cannon until it reaches 0
	public float aiAttackCooldown;
	public bool canAirDashReset;

	public Vile(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, bool mk2VileOverride = false, bool mk5VileOverride = false,
		VileLoadout? loadout = null,
		int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn, heartTanks, isATrans
	) {
		mk2VileOverride = false;
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
		loadout ??= player.loadout.vileLoadout.clone();
		this.loadout = loadout;

		weaponSystem = setupWeaponSystem();

		rideMenuWeapon = new MechMenuWeapon(VileMechMenuType.All);
		hasFrozenCastle = player.frozenCastle;
		hasSpeedDevil = player.speedDevil;
	}

	public (Sprite? spr, Point drawPos, Point shootPos) getCannonSprite() {
		string vilePrefix = "vava_";
		if (isVileMK2) { vilePrefix = "vilemk2_"; }
		if (isVileMK5) { vilePrefix = "vilemk5_"; }
		string cannonSprite = vilePrefix + "cannon";

		for (int i = 0; i < currentFrame.POIs.Length; i++) {
			string tag = currentFrame.POITags[i] ?? "";
			if (tag == "") {
				continue;
			}
			int frameIndexToDraw = tag.ToLower() switch {
				"cannon1" or "cannon1b" => 0,
				"cannon2" or "cannon2b" => 1,
				"cannon3" or "cannon3b" => 2,
				"cannon4" or "cannon4b" => 3,
				"cannon5" or "cannon5b" => 4,
				"cannon" => cannonAimNum,
				_ => -1
			};
			if (frameIndexToDraw != cannonAimNum) {
				continue;
			}
			Point poi = currentFrame.POIs[i];
			Sprite retSprite = new Sprite(cannonSprite);
			int dir = getShootXDirSynced();
			Point altPOI = (
				retSprite.animData.frames.ElementAtOrDefault(cannonAimNum)?.POIs?.FirstOrDefault() ??
				Point.zero
			);
			altPOI.x *= dir;

			Point drawPos = new Point(poi.x * dir + pos.x, poi.y + pos.y);
			Point shootPos = drawPos + altPOI;

			return (retSprite, drawPos, shootPos);
		}
		return (null, pos, getCenterPos());
	}





	public virtual Point setCannonAim(Point shootDir) {
		float shootY = -shootDir.y;
		float shootX = MathF.Abs(shootDir.x);
		float ratio = shootY / shootX;
		if (ratio > 1.25f) cannonAimNum = 3;
		else if (ratio <= 1.25f && ratio > 0.75f) cannonAimNum = 2;
		else if (ratio <= 0.75f && ratio > 0.25f) cannonAimNum = 1;
		else if (ratio <= 0.25f && ratio > -0.25f) cannonAimNum = 0;
		else cannonAimNum = 4;

		return getCannonSprite().shootPos;
	}

	public override void preUpdate() {
		base.preUpdate();

		if (isVileMK1) altSoundId = AltSoundIds.X1;
		else if (isVileMK2 || isVileMK5) altSoundId = AltSoundIds.X3;

		if (!ownedByLocalPlayer) return;

		if (!isShootingGizmo && !isShootingVulcan && !usedAmmoLastFrame) {
			energy.addAmmo(0.25f * speedMul, player);
		}
		usedAmmoLastFrame = false;

		if (isShootingVulcan) {
			string targeSprite = charState.shootSpriteEx;
			if (targeSprite == "") {
				targeSprite = grounded ? "shoot" : "shoot_fall";
			}
			if (getSprite(sprite.name) != charState.shootSpriteEx) {
				changeSpriteFromName(charState.shootSpriteEx, false);
			}
			wasShootingVulcan = true;
		} else if (wasShootingVulcan) {
			changeSpriteFromName(charState.sprite, resetFrame: false);
			wasShootingVulcan = false;
		}
		shootAnimTime = vulcanLingerTime;

		Helpers.decrementFrames(ref calldownMechCooldown);
		Helpers.decrementFrames(ref mechBusterCooldown);
		//Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref vulcanLingerTime);
		Helpers.decrementFrames(ref deadCooldown);
		addWeaponHealAmmo();

		if ((grounded || charState is LadderClimb or LadderEnd or WallSlide) && vileHoverTime > 0) {
			vileHoverTime -= Global.spf * 6;
			if (vileHoverTime < 0) vileHoverTime = 0;
		}
	}

	public int vileStyle = 0;


	VileGrab grabWeapon;
	public virtual void VileWeaponUpdate() {
		if (vileStyle > 3) {
			vileStyle = 0;
		}
		if (player.input.isPressed(Control.WeaponLeft, player)) {
			vileStyle ++;
		}

		if (Options.main.vileLoadout.cannon == 0) {
			cannonWeapon = new FrontRunner();
		}
		if (Options.main.vileLoadout.vulcan == 0) {
			vulcanWeapon = new CherryBlast();
		}
		if (Options.main.vileLoadout.rocketPunch == 0) {
			grabWeapon = new ViolentCrusher();
		}
		if (Options.main.vileLoadout.flamethrower == 0) {
			flamethrowerWeapon = new WildHorseKick();
		}
		if (Options.main.vileLoadout.ball == 0) {
			airSpWeapon = new ExplosiveRound();
		}
		if (Options.main.vileLoadout.napalm == 0) {
			grenadeWeapon = new BumptyBoom();
		}
		if (Options.main.vileLoadout.missile == 0) {
			missileWeapon = new HumerusCrush();
		}
		if (Options.main.vileLoadout.cutter == 0) {
			cutterWeapon = new QuickHomesick();
		}




		if (Options.main.vileLoadout.cannon == 1) {
			cannonWeapon = new FatBoy();
		}
		if (Options.main.vileLoadout.vulcan == 1) {
			vulcanWeapon = new DistanceNeedler();
		}
		if (Options.main.vileLoadout.rocketPunch == 1) {
			grabWeapon = new SpringSnatcher();
		}
		if (Options.main.vileLoadout.flamethrower == 1) {
			flamethrowerWeapon = new SeaDragonRage();
		}
		if (Options.main.vileLoadout.ball == 1) {
			airSpWeapon = new SpreadShot();
		}
		if (Options.main.vileLoadout.napalm == 1) {
			grenadeWeapon = new RumblingBang();
		}
		if (Options.main.vileLoadout.missile == 1) {
			missileWeapon = new HumerusCrush();
		}
		if (Options.main.vileLoadout.cutter == 1) {
			cutterWeapon = new MaroonedTomahawk();
		}





		if (Options.main.vileLoadout.cannon == 2) {
			cannonWeapon = new TridentLine();
		}
		if (Options.main.vileLoadout.vulcan == 2) {
			vulcanWeapon = new BuckshotDance();
		}
		if (Options.main.vileLoadout.rocketPunch == 2) {
			grabWeapon = new SpeedyViper();
		}
		if (Options.main.vileLoadout.flamethrower == 2) {
			flamethrowerWeapon = new DragonsWrath();
		}
		if (Options.main.vileLoadout.ball == 2) {
			airSpWeapon = new PeaceOutRoller();
		}
		if (Options.main.vileLoadout.napalm == 2) {
			grenadeWeapon = new RumblingBang();
		}
		if (Options.main.vileLoadout.missile == 2) {
			missileWeapon = new PopcornDemon();
		}
		if (Options.main.vileLoadout.cutter == 2) {
			cutterWeapon = new ParasiteSword();
		}
		
	}

	public override void update() {
		base.update();
		VileWeaponUpdate();
		Supers();

		player.vileAmmo = energy.ammo;

		if (overDriveTimer > 0){
			ShouldDrawAura = true;
		} else {
			ShouldDrawAura = false;
		}

		if (!ownedByLocalPlayer) return;

		// Update the weapon system.
		// And subweapons by extension.
		weaponSystem.update();
		weaponSystem.charLinkedUpdate(this, false);
		Helpers.decrementTime(ref mk2GrabCooldown);
		rideArmorAttacks();
		rideLinkPenta();
		if (isVileMK2) mk2Buffs = true;
		// GMTODO: Consider a better way here instead of a hard-coded deny list
		// Gacel: Done, now it uses attackCtrl
		if (!charState.attackCtrl || charState is VileMK2GrabState) {
			chargeLogic(null);
		} else {
			chargeLogic(shoot);
		}

		if (charState is WallKick or WallSlide || grounded) {
			canAirDashReset = true;
		}
	}



	

	public float HyperDashCooldown;

	public virtual void Supers() {
		Helpers.decrementTime(ref HyperDashCooldown);

		if (!isInDamageSprite() && downPressedTimes > 1 && hasSpeedDevil && 

			player.input.isHeld(Control.Down, player) &&
		player.input.isPressed(Control.Dash, player) && HyperDashCooldown == 0) {
			changeState(new VileDashChargeState(), true);
			playSound("vilehyperdashstart", true);
			HyperDashCooldown = 2f;
		}


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
		


		
	}

	public override bool attackCtrl() {

		if (	this is not VAVA1
			and not VAVA2
			and not VAVAV
			and not MysteriousMaverick
			and not HighwayVAVA
			and not FinalVava
			and not Vava2Goliath
			
		
		
		
		) {
			bool specialPressed = player.input.isPressed(Control.Special1, player);
			bool shootHeld = player.input.isHeld(Control.Shoot, player);

			bool WeaponRightHeld = (
				player.input.isHeld(Control.WeaponRight, player) &&
				(!isATrans || !player.input.isHeld(Control.Up, player))
			);

			if (specialPressed) {
				if (player.input.isHeld(Control.Down, player)) {
					grenadeWeapon?.vileShoot(this);
				} else if (player.input.isHeld(Control.Up, player)) {
					flamethrowerWeapon?.vileShoot(this);
				} else {
					airSpWeapon?.vileShoot(this);
				}
			}

			if (player.input.isPressed(Control.WeaponLeft,player)) {
				if (player.input.isHeld(Control.Down, player)) {
					cutterWeapon?.vileShoot(this);
				}// else if (player.input.isLeftOrRightHeld(player)) {
					
				//}
				 else {
					grabWeapon?.vileShoot(this);
				}
			}

		
			if (player.input.isAPressed(player) && player.input.isL2Held(player)) {
				changeState(new VileChainGrabState());
			}
			if (player.input.isPressed(Control.R2,player) && player.input.isL2Held(player)) {
				changeState(new Vava1GizmoDash());
			}
			if (player.input.isPressed(Control.Dash,player) && player.input.isL2Held(player)) {
				changeState(new CrimsonPhantomState(grounded));
			}


			if (player.input.isAPressed(player) && !player.input.isL2Held(player)) {
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


			if (player.input.isR2Pressed(player) //&& cannonWeapon.type > -1
			) {
				cannonWeapon?.vileShoot(this);	
			}
			if (WeaponRightHeld && vulcanWeapon.type > -3) {
				vulcanWeapon?.vileShoot(this);
			}
			}
		return base.attackCtrl();
	}

	public bool normalAttacks() {
		if (!grounded) {
			return false;
		}
		if (!player.input.isPressed(Control.Special1, player)) {
			return false;
		}
		bool leftorRightHeld = player.input.getXDir(player) != 0;
		bool downHeld = player.input.getYDir(player) == 1;
		if (downHeld) {
			downSpWeapon?.vileShoot(this);
			return true;
		}
		if (!sprite.name.Contains("grab")) {
			return weaponSystem.shootLogic(this);
		}
		return false;
	}

	public bool mk2Buffs = false;

	public float mk2GrabCooldown;

	public bool dashGrabSpecial() {
		if (!player.input.isHeld(Control.Special1, player)) {
			return false;
		}
		if (mk2GrabCooldown > 0) return false;

		if (isDashing && (isVileMK2 || mk2Buffs) &&
			charState is Dash or AirDash { stop: false }
		) {
			charState.isGrabbing = true;
			charState.superArmor = true; //peakbalance
			changeSpriteFromName("dash_grab", true);
			return true;
		}
		return false;
	}

	public bool rideArmorAttacks() {
		bool goliath = rideArmor?.raNum == 4;
		bool stunShotPressed = player.input.isPressed(Control.Special1, player);
		bool HeldDown = player.input.isHeld(Control.Down, player);
		bool goliathShotPressed = (
			player.input.isPressed(Control.WeaponLeft, player) ||
			player.input.isPressed(Control.WeaponRight, player)
		);
		bool raStates = rideArmor?.rideArmorState is RAIdle or RAJump or RAFall or RADash;
		if (rideArmor != null && charState is InRideArmor raState && !raState.isHiding) {
			if (raStates) {
				if (goliath && Options.main.swapGoliathInputs) {
					(goliathShotPressed, stunShotPressed) = (stunShotPressed, goliathShotPressed);
				}
				Weapon rideWeapon = weaponSystem.rideWeapon;
				if (stunShotPressed && !HeldDown && rideWeapon.shootCooldown <= 0) {
					rideWeapon?.vileShoot(this);
				}
				if (goliathShotPressed) {
					if (goliath && !rideArmor.isAttacking() && mechBusterCooldown <= 0) {
						rideArmor.changeState(new RAGoliathShoot(rideArmor.grounded), true);
						mechBusterCooldown = 60;
					}
				}
			}
			player.gridModeHeld = false;
			player.gridModePos = new Point();
			return true;
		}
		return false;
	}

	public int airDashReset = 1;
	public override bool normalCtrl() {


		if (player.input.isL2Held(player)) {
			changeState(new BlockWCUT(), true);

		}


		if (player.dashPressed(out string dashControl) && flag == null &&
		!grounded && airDashReset == 0) {
			changeState(new AirDash(dashControl));
			airDashReset = 1;
			return true;
		}

		if (sprite.name.EndsWith("cannon_air") && isAnimOver()) {
			changeSpriteFromName("fall", true);
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
	public virtual void shoot(int chargeLevel) {
		if (chargeLevel >= 3) {
			laserWeapon?.vileShoot(this);
		}
		if (chargeLevel == 4 && isVileMK5) {
			changeState(new HexaInvoluteState(), true);
		} else if (chargeLevel >= 3) {
			weaponSystem.chargeWeapon.vileShoot(this);
		}
	}

	public override bool chargeButtonHeld() {
	//	if (currentWeapon is AssassinBulletChar) return player.input.isHeld(Control.Up, player);
		return player.input.isR2Held(player);
	}

	public override bool canCharge() {
		return (
			!isInvulnerable(true) &&
			alive && invulnTime == 0 &&
			charState is not VileRevive and not Die and not HexaInvoluteState
		);
	}

	public override int getMaxChargeLevel() {
		return 4;
	}

	public override bool canShoot() {
		if (isInvulnerableAttack() || invulnTime > 0) {
			return false;
		}
		return base.canShoot();
	}

	public void rideLinkPenta() {
		//Do not use if dead
		if (!alive) {
			rideMenuWeapon.isMenuOpened = false;
			return;
		}
		// Deactivation code.
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
			// Ride Menu
		} else if (!oldATrans &&
			player.input.isPressed(Control.Special2, player) &&
			!player.input.isHeld(Control.Down, player)
		) {
			onMechSlotSelect(rideMenuWeapon);
			return;
		}
		// Menu controls.
		if (rideMenuWeapon?.isMenuOpened == true) {
			if (player.input.isBPressed(player) || player.input.isPressed(Control.WeaponLeft, player)) {
				rideMenuWeapon.isMenuOpened = false;
			}
		}
		// Link code.
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
		return isVileMK5 && linkedRideArmor?.character == this;
	}

	public bool canVileHover() {
		return isVileMK5 && energy.ammo > 0 && flag == null;
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


	public override bool canAffordRideArmor() {
		if (isVileMK5 && !mk2Buffs) {
			return player.currency >= Vile.mk5AltCost;
		}
		if (Global.level.is1v1()) {
			return health > Math.Floor(maxHealth / 2);
		}
		return player.currency >= Vile.callNewMechCost;
	}

	public override void buyRideArmor() {
		if (Global.level.is1v1()) {
			health -= Math.Floor(maxHealth / 2);
			return;
		}
		if (isVileMK5 && !mk2Buffs) {
			player.currency -= mk5AltCost * (player.selectedRAIndex >= 4 ? 2 : 1);
			return;
		}
		player.currency -= callNewMechCost * (player.selectedRAIndex >= 4 ? 2 : 1);
	}


	public override void onMechSlotSelect(MechMenuWeapon mmw) {
		//Do not use if dead
		if (!alive) return;
		if (linkedRideArmor == null) {
			if (!mmw.isMenuOpened) {
				mmw.isMenuOpened = true;
				return;
			}
		}
		if (player.isAI) {
			calldownMechCooldown = maxCalldownMechCooldown;
		}
		if (linkedRideArmor != null) {
			if (charState is not (Idle or Run or Crouch)) {
				return;
			}
			changeState(new CallDownMech(linkedRideArmor, false), true);
			return;
		}
		if (alreadySummonedNewMech) {
			Global.level.gameMode.setHUDErrorMessage(player, "Can only summon a mech once per life");
			return;
		}
		if (!canAffordRideArmor()) {
			if (player.selectedRAIndex == 4 && player.currency < 10) {
				if (isVileMK2) {
					int randomString = Helpers.randomRange(0, 1);
					string brownOrGoliath = randomString == 0 ? "Brown Bear" : "Goliath";

					Global.level.gameMode.setHUDErrorMessage(
						player, $"{brownOrGoliath} armor requires 10 {Global.nameCoins}"
					);
				} else {
					Global.level.gameMode.setHUDErrorMessage(
					  player, $"Devil Bear armor requires 10 {Global.nameCoins}"
				  );
				}
			} else {
				cantAffordRideArmorMessage();
			}
			return;
		}
		if (charState is not (Idle or Run or Crouch)) {
			return;
		}
		alreadySummonedNewMech = true;
		buyRideArmor();
		mmw.isMenuOpened = false;
		int raIndex = player.selectedRAIndex;
		if (isVileMK5 && raIndex == 4) {
			raIndex++;
		}
		linkedRideArmor = new RideArmor(
			player, pos, raIndex, 0, player.getNextActorNetId(), true, sendRpc: true
		);
		if (linkedRideArmor.raNum == 4) summonedGoliath = true;
		if (isVileMK5) {
			linkedRideArmor.ownedByMK5 = true;
			linkedRideArmor.zIndex = zIndex - 1;
		}
		changeState(new CallDownMech(linkedRideArmor, true), true);
	}

	public bool tryUseVileAmmo(float ammo, bool isVulcan = false) {
		// Do not drain if negative, use ammo regen for that.
		if (ammo < 0) {
			return true;
		}
		if (weaponHealAmount > 0) {
			return true;
		}
		if (isVulcan) {
			usedAmmoLastFrame = true;
		}
		if (energy.ammo >= ammo) {
			usedAmmoLastFrame = true;
			energy.addAmmo(-ammo, player);
			return true;
		}
		return false;
	}

	public override void addAmmo(float amount) {
		if (amount < 0) {
			energy.addAmmo(amount, player);
			return;
		}
		player.vileAmmo += amount;
		weaponHealAmount += amount;
	}
	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}
	public override bool canAddAmmo() {
		return energy.ammo < energy.maxAmmo;
	}
	public void addWeaponHealAmmo() {
		if (energy.ammo >= energy.maxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && alive) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				energy.addAmmo(1, player);
				if (isVileMK1) {
					playSound("heal", forcePlay: true, true);
				} else {
					playSound("healX3", forcePlay: true, true);
				}
			}
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

	public virtual Point getVileShootVel(bool aimable) {
		Point vel = new Point(1, 0);
		if (!aimable) return vel;
		bool isHeldUp = player.input.isHeld(Control.Up, player);
		bool isHeldDown = player.input.isHeld(Control.Down, player);
		bool isLeftOrRightHeld = player.input.isLeftOrRightHeld(player);
		bool isRideArmor = rideArmor != null;
		bool isCrouchState = charState is Crouch;
		if (isRideArmor) {
			if (isHeldUp) vel = new Point(1, -0.5f);
			else vel = new Point(1, 0.5f);
		} else if (charState is ShoulderCannon or Vava1TridentLine) {
			if (frameIndex == 12) vel = new Point(1, 0.5f);
			if (frameIndex == 15) vel = new Point(1, -0.5f);
		}
		else if (!isCrouchState) {
			if (!canVileAim60Degrees()) return vel;
			if (isHeldUp) {
				if (isLeftOrRightHeld) vel = new Point(1, -0.75f);
				else vel = new Point(1, -3);
			} else if (isHeldDown) {
				if (isLeftOrRightHeld) vel = new Point(1, 0.75f);
				else vel = new Point(1, 3);
			}
		} else if (isCrouchState) {
			if (isHeldUp) vel = new Point(1, -0.5f);
			if (isLeftOrRightHeld) vel = new Point(1, 0.5f);
		} 

		/*
		if (rideArmor != null) {
			if (player.input.isHeld(Control.Up, player)) {
				vel = new Point(1, -0.5f);
			} else {
				vel = new Point(1, 0.5f);
			}
		} else if (charState is VileMK2GrabState) {
			vel = new Point(1, -0.75f); //This code was from old times when you could shoot cannon on GrabState
		} else if (player.input.isHeld(Control.Up, player)) {
			if (!canVileAim60Degrees() || (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		} else if (player.input.isHeld(Control.Down, player) && charState is not Crouch && charState is not MissileAttack) {
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

	public Point getVileMK2StunShotPos() {
		if (charState is InRideArmor) {
			return pos.addxy(xDir * -8, -12);
		}
		return pos.addxy(-xDir * 5, -32);
	}

	public void setVileShootTime(Weapon weapon, float modifier = 1f, Weapon? targetCooldownWeapon = null) {
		targetCooldownWeapon ??= weapon;
		if (isVileMK2 || isVileMK5) {
			float innerModifier = 1f;
			if (weapon is VileMissile) innerModifier = isVileMK2 ? 0.3333f : 0.6666f;
			weapon.shootCooldown = MathF.Ceiling(targetCooldownWeapon.fireRate * innerModifier * modifier);
		} else {
			weapon.shootCooldown = MathF.Ceiling(targetCooldownWeapon.fireRate * modifier);
		}
	}


	
	public enum MeleeIds {
		None = -1,
		Blocking,
		KamaeBlock,
		Jab,
		Jab2,
		
		VavaKneeAttack,
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

		AirRaid,


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
			"vava_knee" => MeleeIds.VavaKneeAttack,
			"vava_jab_1" => MeleeIds.Jab,
			"vava_jab_2" or "vava_punch_1" or "vava_kick" or "vava_kick_2" => MeleeIds.Jab2,
			"vava_punch_2" => MeleeIds.UpperCut,
			"vava_gizmo_dash_grab" => MeleeIds.GizmoGrab,
			"vava_kamae_unblockable" or "vava_kamae_unblockable_land"  or "vava_air_bomb_attack"=> MeleeIds.KamaeUnB,
			"vava_kamae_kote" => MeleeIds.Kote,
			"vava_spring_grab" => MeleeIds.Grab,
			"vava_dash_grab" => MeleeIds.Grabmk2dash,
			"vava_hoticecle" => MeleeIds.HotIcecle,
			"vava_drop_kick"=> MeleeIds.DropKick,
			"vava_cannon_execution" => MeleeIds.CannonExecution,
			"vava_green_eyed_lamp" => MeleeIds.GreenEyedLamp,
			"vava_burensen_1" => MeleeIds.BurensenStart,
			"vava_burensen_2" or "vava_stomp" => MeleeIds.BurensenStomp,
			"vava_ragingdemon_dash" => MeleeIds.RagingDemon,
			"vava_burensen_finish" when !player.isAI => MeleeIds.BurensenEND,
			"vava_burensen_finish" or "vava_hyperdash_attack" when player.isAI => MeleeIds.BurensenENDCPU,
			"vava_hyperdash_attack" or "vava_missile_stance" =>  MeleeIds.SpeedDemon,

			"vava_superkick"  => MeleeIds.BurensenENDCPU,
			"vava_superkick_up"  => MeleeIds.AirRaid,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockingProjID, player, damage: 0.0f,
				flinch: 0, hitCooldown: 0, isShield: false, isReflectShield: false,
				isDeflectShield: true, ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitspark : "empty"


				),

			
			(int)MeleeIds.StompStart => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.VileStomp, player,
				 0, 0, 0, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.Grab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 0, 0, 0, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound: "kofhtsnd_grab1"
			),

			(int)MeleeIds.GizmoGrab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GizmoGrab, player,
				 0, 0, 0, isReflectShield: false,
				clashTier: ClashTier.Weak, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound: "kofhtsnd_grab1"
			),
			(int)MeleeIds.Grabmk2dash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.VileMK2Grab, player,
				 0, 0, 0, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound: "kofhtsnd_grab1"
			),
			(int)MeleeIds.UpperCut => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.SpinningBlade, player,
				 2, 40, 32, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "htsnd_punch_3", isJuggleProjectile : true
			),
			(int)MeleeIds.VavaKneeAttack => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.SpinningBlade, player,
				 2, 20, 12, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp1", isJuggleProjectile : true
			),
			(int)MeleeIds.GrabNonFlinchAT => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.SpinningBlade, player,
				 1, 0, 10, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_punch1", isJuggleProjectile : true
			),
			(int)MeleeIds.DropKick => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.DropSlide, player,
				 2, 0, 42, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, isJuggleProjectile : false, hitSound : "kofhtsnd_punch1"
			),
			(int)MeleeIds.CannonExecution => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				 2, 0, 42, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, isJuggleProjectile : true, hitSound: "kofhtsnd_knock1"
			),
			(int)MeleeIds.KamaeBlock => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab1, player,
			 0.25f, 5, 8, isReflectShield: true,
			clashTier: ClashTier.Weak, isZSaberEffect: true,
			addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.Jab => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab1, player,
			 1, 12, 15, isReflectShield: true,
			clashTier: ClashTier.Weak, isZSaberEffect: true,
			addToLevel: addToLevel, hitSound : "htsnd_punch_1"
			),
			(int)MeleeIds.Jab2 => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab2, player,
			 1, 26, 8, isReflectShield: true,
			clashTier: ClashTier.Weak, isZSaberEffect: true,
			addToLevel: addToLevel, hitSound : "htsnd_punch_2", isJuggleProjectile : true
			),


			(int)MeleeIds.KamaeUnB => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MechFrogStompShockwave, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true
			),

			(int)MeleeIds.DeadLiftEX => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true
			),


			(int)MeleeIds.GoldenRight => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				3, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "dbzclang", isJuggleProjectile : true
			),




			(int)MeleeIds.Kote => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				2, 40, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp2", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenStart => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenStart, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp1", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenStomp => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenStomp, player,
				1, 0, 5, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp2", isJuggleProjectile : false
			),


			(int)MeleeIds.SpeedDemon => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.HeavyPush, player,
				2, 0, 30, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_clamp2", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenEND => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenEND, player,
				2, 0, 30, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_megapunch1", isJuggleProjectile : true
			),

			(int)MeleeIds.BurensenENDCPU => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenEND, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, hitSound : "kofhtsnd_megapunch1", isJuggleProjectile : true
			),

			(int)MeleeIds.RagingDemon => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.RagingDemon, player,
				5, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel, isJuggleProjectile : true
			),

			(int)MeleeIds.HotIcecle => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Hyouretsuzan2, player,
				2, 30, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "htsnd_glass", isJuggleProjectile : true, isLiftProjectile : true
			),

			(int)MeleeIds.GreenEyedLamp => new GenericMeleeProj(
				new RyuenjinWeapon(), projPos, ProjIds.Ryuenjin, player,
				2, 30, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true,  hitSound : "htsnd_punch_3"
			),

			(int)MeleeIds.AirRaid => new GenericMeleeProj(
				new RyuenjinWeapon(), projPos, ProjIds.VileAirRaidPlusKnock, player,
				1, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true, hitSound: "htsnd_lighting"
			),

			_ => null
		};
		return proj;
	}



	public override bool isSoftLocked() {
		if (isShootingGizmo) {
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
		if (isShootingGizmo) {
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
	//	if (isVileMK5) {
	//		return "vilemk5_" + spriteName;
	//	}
	//	if (isVileMK2) {
	//		return "vilemk2_" + spriteName;
	//	}
		return "vava_" + spriteName;
	}

	public override void changeToIdleOrFall(string transitionSprite = "", string transShootSprite = "") {
		if (!grounded && charState.wasVileHovering && canVileHover()) {
			changeState(new VileHover(), true);
			return;
		}
		base.changeToIdleOrFall(transitionSprite, transShootSprite);
	}

	public override float getLabelOffY() {
		if (sprite.name.Contains("_ra_")) {
			return 25;
		}
		return 50;
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
	

	public override void render(float x, float y) {

		if (visible && ShouldDrawAura  ) {
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

		if (hasSpeedDevil && visible) {
			addRenderEffect(RenderEffectType.SpeedDevilTrail);
		} else {
			removeRenderEffect(RenderEffectType.SpeedDevilTrail);
		}
		if (currentFrame.POIs.Length > 0) {
			(Sprite? cannonSprite, Point drawPos, _) = getCannonSprite();
			cannonSprite?.draw(
				cannonAimNum, drawPos.x, drawPos.y, getShootXDirSynced(),
				1, getRenderEffectSet(), alpha, 1, 1, zIndex + 1,
				getShaders(), actor: this
			);
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
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;


		if (this is not VAVA2 and not VAVAV and not FinalVava and not VAVA1){
			if (player.skinSlot == 0) {
				palette = player.vilePal0;
			}
			if (player.skinSlot == 1) {
				palette = player.nightmareZeroShader;
			}
			if (player.skinSlot == 2) {
				palette = player.nightmareZeroShader2;
			}
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


/*
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
	}*/

	public override Point getParasitePos() {
		if (sprite.name.Contains("_ra_")) {
			if (sprite.name.Contains("_ra_hide")) {
				pos.addxy(0, -6 + 22 * (sprite.frameIndex / (float)sprite.totalFrameNum));
			}
			return pos.addxy(0, -6);
		}
		return pos.addxy(0, -24);
	}

	public override void onDeath() {
		base.onDeath();
		player.lastDeathWasVileMK2 = isVileMK2;
		player.lastDeathWasVileV = isVileMK5;
		deadCooldown = maxdeadCooldown;
	}

	public VileWeaponSystem setupWeaponSystem() {
		Weapon? vulcanWeapon = loadout.vulcan switch {
			0 => new CherryBlast(),
			1 => new DistanceNeedler(),
			2 => new BuckshotDance(),
			_ => null
		};
		Weapon? cannonWeapon = loadout.cannon switch {
			1 => new FatBoy(),
			2 => new LongShotGizmo(),
			0 => new FrontRunner(),
			_ => null
		};
		Weapon? missileWeapon = loadout.missile switch {
			0 => new ElectricShock(),
			1 => new HumerusCrush(),
			2 => new PopcornDemon(),
			_ => null
		};
		Weapon? rocketPunchWeapon = loadout.rocketPunch switch {
			0 => new GoGetterRight(),
			1 => new SpoiledBrat(),
			2 => new InfinityGig(),
			_ => null
		};
		Weapon? napalmWeapon = loadout.napalm switch {
			0 => new RumblingBang(),
			1 => new FireGrenade(),
			2 => new SplashHit(),
			_ => null
		};
		Weapon? grenadeWeapon = loadout.napalm switch {
			0 => new ExplosiveRound(),
			1 => new SpreadShot(),
			2 => new PeaceOutRoller(),
			_ => null
		};
		Weapon? cutterWeapon = loadout.cutter switch {
			0 => new QuickHomesick(),
			1 => new ParasiteSword(),
			2 => new MaroonedTomahawk(),
			_ => null
		};
		Weapon? flamethrowerWeapon = loadout.flamethrower switch {
			0 => new WildHorseKick(),
			1 => new SeaDragonRage(),
			2 => new DragonsWrath(),
			_ => null
		};
		Weapon? downSpWeapon = loadout.downSpWeapon switch {
			0 => napalmWeapon,
			1 => grenadeWeapon,
			2 => flamethrowerWeapon,
			_ => napalmWeapon,
		};
		Weapon? airSpWeapon = loadout.airSpWeapon switch {
			0 => napalmWeapon,
			1 => grenadeWeapon,
			2 => flamethrowerWeapon,
			_ => napalmWeapon,
		};
		Weapon? downAirSpWeapon = loadout.downAirSpWeapon switch {
			0 => napalmWeapon,
			1 => grenadeWeapon,
			2 => flamethrowerWeapon,
			_ => napalmWeapon,
		};
		Weapon? laserWeapon = loadout.laser switch {
			0 => new RisingSpecter(),
			1 => new NecroBurst(),
			2 => new StraightNightmare(),
			_ => null
		};
		// Assing weapons to specific slots.
		Weapon?[] shootWps = [cannonWeapon, null, null, null];
		Weapon?[] specialWps = [missileWeapon, rocketPunchWeapon, null, downSpWeapon];
		Weapon?[] airSpecialWps = [airSpWeapon, null, null, downAirSpWeapon];
		Weapon?[] altWps = [vulcanWeapon, null, cutterWeapon, null];

		return new VileWeaponSystem(
			altWps, shootWps, specialWps,
			altWps, shootWps, airSpecialWps, [
				laserWeapon ?? new EmptyWeapon(),
				missileWeapon ?? new ElectricShock(),
				napalmWeapon ?? new RumblingBang()
			]
		);
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();

		customData.Add(Helpers.boolArrayToByte([
			hasFrozenCastle,
			hasSpeedDevil,
			ShouldDrawAura,
			OverDrive
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
		ShouldDrawAura = boolData[2];
		OverDrive = boolData[3];
	}

	public bool ShouldDrawAura;
	
	//public float aiAttackCooldown;

	/*
	public override void aiAttack(Actor? target) {
		int vattack = Helpers.randomRange(1, 7);
		bool isFacingTarget = (pos.x * xDir < target?.pos.x * xDir);
		if (isFacingTarget && charState is Dash or AirDash && charState.isGrabbing == true) {
			return;
		}

		if (canShoot() && charState.attackCtrl && aiAttackCooldown <= 0) {
			if (isVileMK2 && charState is Dash or AirDash && isFacingTarget) {
				player.press(Control.Special1);
				aiAttackCooldown = 20;
				return;
			}
			if (!isFacingTarget && canTurn() && charState.normalCtrl) {
				if (xDir == 1) {
					player.press(Control.Left);
				} else {
					player.press(Control.Right);
				}
			}

			bool shotWeapon = weaponSystem.shootRandomWeapon(this);
			if (shotWeapon) {
				aiAttackCooldown = 20;
			}
		}
	}
	*/

	public override void aiUpdate(Actor? target) {
		base.aiUpdate(target);
		if (!player.isMainPlayer) {
			if (player.canReviveVile() && isVileMK1) {
				player.reviveVile(false);
			}
			if (isVileMK2 && player.canReviveVile()) {
				player.reviveVile(true);
			}
		}
		if (!player.isMainPlayer) {
			if (player.currency >= 3 && !player.frozenCastle) {
				player.frozenCastle = true;
				hasFrozenCastle = true;
				player.currency -= Vile.frozenCastleCost;
			}
			if (player.currency >= 3 && !player.speedDevil) {
				player.speedDevil = true;
				hasSpeedDevil = true;
				player.currency -= Vile.speedDevilCost;
			}
		}
	}
}


public class VileAmmoWeapon : Weapon {
	public VileAmmoWeapon() {
		index = (int)WeaponIds.VileLaser;
		weaponSlotIndex = 32;
		weaponBarBaseIndex = 39;
		weaponBarIndex = 32;
		allowSmallBar = true;
		drawRoundedDown = true;

		maxAmmo = 32;
		ammo = maxAmmo;
		drawCooldown = false;
	}
}

// Vile was here.
// The way it was before made it so.
/*
░░░░░░▄▄▄▄▄▄▄▄▄░░░
░░░░░█░░░░░░░░░█░░
░░░░█░░░░░░░█░█░█░
░░░█░░░░░░░░░▀░░█░
░░████░▀█████████░
░░█░░█▄▄░░░░██░░█░
░░░███░░█░░░██░░█░
░░░░░██░░█░░██░░█░
░░░░░░█▒▒▒█░██░█░░
░░░░░░░▀▀▀▀████░░░
░░░░░░░░░░░░░░░░░░
*/