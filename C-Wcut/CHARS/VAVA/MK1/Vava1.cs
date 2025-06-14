
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class VAVA1 : Character {


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

	

	
	public float stockedTime;

	public bool canSpecialCancel = false;


	public VAVA1(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
		) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn) {


		charId = CharIds.VAVA1;


		if (charState is WarpIn) superBarAmmo = 0;
		VileLoadout vileLoadout = player.loadout.vileLoadout;

		vulcanWeapon = new Vulcan((VulcanType)vileLoadout.vulcan);
		cannonWeapon = new VileCannon((VileCannonType)vileLoadout.cannon);
		missileWeapon = new VileMissile((VileMissileType)vileLoadout.missile);
		rocketPunchWeapon = new RocketPunch((RocketPunchType)vileLoadout.rocketPunch);
		napalmWeapon = new Napalm((NapalmType)vileLoadout.napalm);
		grenadeWeapon = new VileBall((VileBallType)vileLoadout.ball);
		cutterWeapon = new VileCutter((VileCutterType)vileLoadout.cutter);
		flamethrowerWeapon = vileLoadout.flamethrower switch {
			-1 => new NoneFlamethrower(),
			1 => new SeaDragonRage(),
			2 => new DragonsWrath(),
			_ => new WildHorseKick()
		};
		laserWeapon = new VileLaser((VileLaserType)vileLoadout.laser);
		rideMenuWeapon = new MechMenuWeapon(VileMechMenuType.All);
		hasFrozenCastle = player.frozenCastle;
		hasSpeedDevil = player.speedDevil;

	}



	public override bool normalCtrl() {

		if (player.input.isHeld(Control.L2, player) && grounded){
			changeState(new BlockWCUT());
		
		}
			if (player.input.isPressed(Control.Special2, player)
			&& player.currency > 4
			) {
				player.currency -= 5;
			}

		return base.normalCtrl();
	}

	public override bool attackCtrl() {

		if (player.input.isPressed(Control.Shoot, player)) {
			changeState(new VAVAJab1(), true);
		}

		

		if (player.input.isPressed(Control.Special1, player)) {		
			
		}

		if (player.input.isHeld(Control.L2, player)){
		}


		if (player.input.isPressed(Control.R2, player)) {
				shoot(0);
			}


		return base.attackCtrl();
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
					if (tryUseVileAmmo(missileWeapon.vileAmmo)) {
					
					}
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




	public override void update() {
		base.update();
		// For the special cancels to work
		if (charState.attackCtrl ||
		charState.normalCtrl || charState.spcCancel
		|| charState.wiffCancel
		) {
			canSpecialCancel = true;
		} else {
			canSpecialCancel = false;
		}

		if (overDriveTimer > 0) {
			OverDrive = true;
		} else {
			OverDrive = false;
		}
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);



		if (canSpecialCancel) {

			if (player.input.checkHadoken(player, xDir, Control.Shoot)) {
				changeState(new VAVAKamae(), true);

			}

		}
		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
			}
		}







		if (superBarAmmo >= superBarMaxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				superBarAmmo = Helpers.clampMax(superBarAmmo + 1, superBarMaxAmmo);
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
		Helpers.decrementTime(ref grabCooldown);
		Helpers.decrementTime(ref mechBusterCooldown);
		Helpers.decrementTime(ref gizmoCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);


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
	

	public override bool canDash() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "vava_" + spriteName;
	}


	public enum MeleeIds {
		None = -1,

		Blocking,
		Jab,
		Grab,
		KamaeUnB,

		HotIcecle,

		Kote,
	}


	// VAva melee stuff
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"vava_block" or "vava_kamae" or "vava_kamae_dash" or "vava_kamae_backdash" => MeleeIds.Blocking, 
			"vava_jab_1"  or "vava_jab_2" => MeleeIds.Jab, 
			"vava_kamae_unblockable"  or "vava_kamae_unblockable_land" => MeleeIds.KamaeUnB, 
			"vava_kamae_kote" => MeleeIds.Kote,
			"vava_hoticecle" => MeleeIds.HotIcecle,


			_ => MeleeIds.None
		});
	}

public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), 	projPos,ProjIds.BlockingProjID,	player, damage: 0.0f, 
				flinch: 0, hitCooldown: 0, 	isShield: false,isReflectShield: false,
				isDeflectShield: true,	isZSaberClang: false,	isZSaberEffect: false,
				addToLevel: addToLevel ),

			(int)MeleeIds.Grab => new KRGenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 0,0,0, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
				(int)MeleeIds.Jab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				 1, Global.halfFlinch,20, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.KamaeUnB => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MechFrogStompShockwave, player,
				3, 0, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.Kote => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				3, 30, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.HotIcecle => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Hyouretsuzan2, player,
				3, 30, 20, isReflectShield: true,
				isZSaberClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			_ => null
		};
		return proj;
	}



	// Ammo section
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return (superBarAmmo < superBarMaxAmmo);
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
		return player.input.isHeld(Control.R2, player);
	}

	public override void increaseCharge() {
		float factor = 1;
		if (OverDrive) factor = 1.5f; // this means during OverDrive he gets a chargespeed buff
		chargeTime += Global.speedMul * factor;
	}

	public override float getRunSpeed() {
		float runSpeed = 90;
		if (OverDrive) { // this means during OverDrive he gets a speed buff
			runSpeed *= 1.15f;
		}
		return runSpeed * getRunDebuffs();
	}




	// Shoots stuff.
	public void shoot(int chargeLevel) {


		if (chargeLevel == 0) {
			stopCharge();
		} else if (chargeLevel == 1) {
		
			stopCharge();
		} else if (chargeLevel == 2) {
			
			stopCharge();
		} else if (chargeLevel == 3) {
		
			stopCharge();
		} else if (chargeLevel >= 4) {
			
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
			if (player.input.isPressed(Control.Special1, player) || player.input.isPressed(Control.WeaponLeft, player)) {
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
		return isVileMK5 && linkedRideArmor?.character == this;
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
					int raIndex = player.selectedRAIndex;
					if (isVileMK5 && raIndex == 4) raIndex++;
					linkedRideArmor = new RideArmor(player, pos, raIndex, 0, player.getNextActorNetId(), true, sendRpc: true);
					if (linkedRideArmor.raNum == 4) summonedGoliath = true;
					if (isVileMK5) {
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

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add(Helpers.boolArrayToByte([
			OverDrive,
		]));
		return customData;
	}
	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];
		bool[] flags = Helpers.byteToBoolArray(data[0]);
		OverDrive = flags[0];
	}




}
