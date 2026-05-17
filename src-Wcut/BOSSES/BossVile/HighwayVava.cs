
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class HighwayVAVA : Vile {


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

	




	public float stockedTime;

	public VileCannonWC cannonWeapon;
	public MechMenuWeapon rideMenuWeapon;

	public HighwayVAVA(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
		) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn) {


		charId = CharIds.VAVA1;

		ShouldExplode = true;

		if (charState is WarpIn) player.superAmmo = 0;
		
		
		cannonWeapon = new VileCannonWC(0);
		
		rideMenuWeapon = new MechMenuWeapon(VileMechMenuType.All);


		vileForm = 0;
		hasFrozenCastle = player.frozenCastle;
		hasSpeedDevil = player.speedDevil;
		if (player.isAI) {
		}
	}

	public bool isMadjoey => Global.level.levelData.name == "st_cybermaze_test";
	public bool isImortalVile => Global.level.levelData.name == "st_x_x1_highway";

	public override bool normalCtrl() {
		if (player.input.isL2Held(player) && grounded) {
			changeState(new BlockWCUT(), true);

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
		if (player.input.checkShoryuken2(player, xDir, Control.Special1) && player.superAmmo >= 16){
			changeState(new VavaBurensen1(), true);	
			player.superAmmo -= 16;
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

			SpecialMoves();
		
		if (!player.input.checkHadoken(player, xDir, Control.Shoot)
		&& !player.input.checkShoryuken(player, xDir, Control.Shoot)
		&& charState is not VAVAKamae) {
			if (player.input.isAPressed(player)) {
				if (grounded) {
					if (player.input.isHeld(Control.Up, player) && player.input.isLeftOrRightHeld(player)) {
						if (player.vileAmmo >= 14) {
						changeState(new InfinityGigAttack(), true);
						}			
					}
					if (player.input.isHeld(Control.Up, player) && !player.input.isLeftOrRightHeld(player)) {
						if (player.vileAmmo >= 14) {
						changeState(new SpoiledBratPunch(), true);
						}
					}
					
					if (player.input.isLeftOrRightHeld(player)) {
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
							changeState(new VAVAUpperCutPunch(), true);
						}
					}
				} else {
					if (player.input.isHeld(Control.Up, player) && player.input.isLeftOrRightHeld(player)) {
						if (player.vileAmmo >= 14) {
						changeState(new InfinityGigAttack(), true);
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
					if (player.vileAmmo > 20) {
						changeState(new WildHorseKickState(), true);
						player.vileAmmo -= 20;
					}
				} else if (player.input.isHeld(Control.Down, player)) {
					
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
							changeState(new PeaceOutRollerAttack());
							player.vileAmmo -= 15;
							}
						} else {
							changeState(new ExplosiveRoundState(), true);
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

		if (!player.input.checkHadoken(player, xDir, Control.R2)
		&& !player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not Vava1GizmoDash
		) {
			// finally added Tridentline
			if (player.input.isR2Pressed(player)) {
				if (downPressedTimes >= 2 && player.vileAmmo > 15) {
					changeState(new Vava1TridentLine(grounded), true);
					downPressedTimes = 0;
					player.vileAmmo -= 15;
				} else {
					if (CannonCD == 0) {
						shoot(0);
						CannonCD = 0.35f;
					}
				}
			}
		}	
		


		return base.attackCtrl();
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
	
	public override void update() {
		base.update();


	
			phase2 = true;
		
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);
		Helpers.decrementTime(ref CrimsonphantomCD);
		Helpers.decrementTime(ref grabCooldown);
		Helpers.decrementTime(ref mechBusterCooldown);
		Helpers.decrementTime(ref gizmoCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref CannonCD);

	

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
		Point cannonSpritePOI = nullablePos ?? Point.zero;

		return poiPos.addxy(cannonSpritePOI.x * getShootXDir(), cannonSpritePOI.y);
	}


	public override bool canDash() {
		return flag == null;
	}

	public override string getSprite(string spriteName) {
		if (isMadjoey) {
			return "madjoey_" + spriteName;
		}
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
		GodPress,
		DeadLiftEX,
	}


	// VAva melee stuff
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"vilemk2_block"  => MeleeIds.Blocking,
			"vilemk2_deadlift"   => MeleeIds.DeadLiftEX,
			"vilemk2_kamae" or "vava_kamae_dash" or "vava_kamae_backdash" => MeleeIds.KamaeBlock,
			"vilemk2_jab_1" => MeleeIds.Jab,
			"vilemk2_jab_2" => MeleeIds.Jab2,
			"vilemk2_punch_2" => MeleeIds.UpperCut,
			"vilemk2_gizmo_dash_grab" => MeleeIds.GizmoGrab,
			"vilemk2_kamae_unblockable" or "vava_kamae_unblockable_land" => MeleeIds.KamaeUnB,
			"vilemk2_kamae_kote" => MeleeIds.Kote,
			"vilemk2_spring_grab" => MeleeIds.Grab,
			"vilemk2_dash_grab" => MeleeIds.Grabmk2dash,
			"vilemk2_hoticecle" => MeleeIds.HotIcecle,
			"vilemk2_green_eyed_lamp" => MeleeIds.GreenEyedLamp,
			"vilemk2_burensen_1" => MeleeIds.BurensenStart,
			"vilemk2_burensen_2" => MeleeIds.BurensenStomp,
			"vilemk2_ragingdemon_dash" => MeleeIds.RagingDemon,
			"vilemk2_hyperdash_end" => MeleeIds.BurensenENDCPU,
			"vilemk2_hyperdash_attack" => MeleeIds.GodPress,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockingProjID, player, damage: 0.0f,
				flinch: 0, hitCooldown: 0, isShield: false, isReflectShield: false,
				isDeflectShield: true, ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel),

			(int)MeleeIds.Grab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 0, 0, 0, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.GizmoGrab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GizmoGrab, player,
				 0, 0, 0, isReflectShield: false,
				clashTier: ClashTier.Weak, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Grabmk2dash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.VileMK2Grab, player,
				 0, 0, 0, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.UpperCut => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.SpinningBlade, player,
				 2, 40, 42, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.GodPress => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.ForceGrabState, player,
				 2, 0, 0, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.KamaeBlock => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab1, player,
			 0.25f, 5, 10, isReflectShield: true,
			clashTier: ClashTier.Weak, isZSaberEffect: true,
			addToLevel: addToLevel
			),
			(int)MeleeIds.Jab => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab1, player,
			 1, 20, 25, isReflectShield: true,
			clashTier: ClashTier.Weak, isZSaberEffect: true,
			addToLevel: addToLevel
			),
			(int)MeleeIds.Jab2 => new GenericMeleeProj(
			new KRMelee(), projPos, ProjIds.VJab2, player,
			 1, 26, 20, isReflectShield: true,
			clashTier: ClashTier.Weak, isZSaberEffect: true,
			addToLevel: addToLevel
			),


			(int)MeleeIds.KamaeUnB => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MechFrogStompShockwave, player,
				3, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.DeadLiftEX => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockableLaunch, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.Kote => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				3, 30, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.BurensenStart => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenStart, player,
				2, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.BurensenStomp => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenStomp, player,
				1, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.BurensenEND => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenEND, player,
				2, 0, 30, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.BurensenENDCPU => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BurensenEND, player,
				4, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.RagingDemon => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.RagingDemon, player,
				5, 0, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.HotIcecle => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Hyouretsuzan2, player,
				3, 30, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.GreenEyedLamp => new GenericMeleeProj(
				new RyuenjinWeapon(), projPos, ProjIds.Ryuenjin, player,
				3, 30, 20, isReflectShield: true,
				ShouldClang: false, isZSaberEffect: true,
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
		if (OverDrive) factor = 1.5f; // this means during OverDrive he gets a chargespeed buff
		chargeTime += Global.speedMul * factor;
	}

	public override float getRunSpeed() {
		
		return 0 * getRunDebuffs();
	}




	// Shoots stuff. VAVA(WCUT)
	public override void shoot(int chargeLevel) {


		if (chargeLevel == 0) {
			stopCharge();
			if (player.vileAmmo > 9) {
				changeState(new Vava1Stunshot(grounded, false), true);
				player.vileAmmo -= 10;
			}
		} else if (chargeLevel == 1) {
			cannonWeapon.type = (int)VileCannonType.FrontRunner;
		//		cannonWeapon.vavaShoot(0, this);
			stopCharge();
		} else if (chargeLevel == 2) {
			cannonWeapon.type = (int)VileCannonType.FatBoy;
		//	cannonWeapon.vavaShoot(0, this);
			stopCharge();
		} else if (chargeLevel == 3) {
			cannonWeapon.type = (int)VileCannonType.FatBoy;
		//	cannonWeapon.vavaShoot(0, this);
			stopCharge();
		} else if (chargeLevel >= 4) {
			cannonWeapon.type = (int)VileCannonType.FatBoy;
		//	cannonWeapon.vavaShoot(0, this);
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
					int raIndex = 1;
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
		if (visible && phase2) {
			// Position to draw the sprite to.
			float auraSize = 1 + omegaAura.twitch + omegaAura.grow;
			float drawX = pos.x + x + (float)xDir * currentFrame.offset.x * auraSize;
			float drawY = pos.y + y + (float)yDir * currentFrame.offset.y * auraSize + 1;

			float auraAlpha = 0.1f;

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


	
	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();

		customData.Add(Helpers.boolArrayToByte([
			hasFrozenCastle,
			hasSpeedDevil,
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
		OverDrive = boolData[2];
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
	
			isBossVile = true;
		
			if (isImortalVile) {
			health = 100;
			if (linkedRideArmor != null) {
				linkedRideArmor.health = 100;
			}
		}

		if (!AIStart && charState.attackCtrl) {
			if (isBossVile) {		
					int raIndex = 0;
					linkedRideArmor = new RideArmor(player, pos, raIndex, 0, player.getNextActorNetId(), true, sendRpc: true);
					changeState(new CallDownMech(linkedRideArmor, true), true);
				AIStart = true;
			}
			
		} else {

					if (!charState.isGrabbedState && !player.isDead && !isInvulnerableAttack()
						&& aiAttackCooldown <= 0 && charState.attackCtrl || charState is InRideArmor && aiAttackCooldown <= 0) {

				if (charState is Dash or AirDash && isFacingTarget && isBossVile) {
					charState.isGrabbing = true;
					charState.superArmor = true; // yes Cry Gsu I'm adding the annoying SuperArmor
					changeSpriteFromName("dash_grab", true);
				}


				if (charState is InRideArmor && linkedRideArmor != null) {

					if (musicSource == null && isMadjoey) {
						addMusicSource("boss_X1", getCenterPos(), true);
						linkedRideArmor.neutralId = 1;
			} else {
						if (musicSource == null) {
						addMusicSource("fake", getCenterPos(), true);
						}
					}
	
					switch (Vattack) {
						case 1 when isFacingTarget && isTargetClose:
							linkedRideArmor.changeState(new RAAIDashAttack(Control.Dash, false), true);

							break;
						case 2 when isFacingTarget && isTargetClose:

							linkedRideArmor.changeState(new RAAIDashAttack(Control.Dash, false), true);

							break;
						case 3 when isFacingTarget:
							linkedRideArmor.changeState(new RAAIDashAttack(Control.Dash, false), true);

							break;
						case 4 when isFacingTarget:
							linkedRideArmor.changeState(new RAJump());

							break;
						case 5 when isFacingTarget && !isMadjoey:
							linkedRideArmor.changeState(new RAAIDashAttack(Control.Dash, false), true);

							break;
						case 6 when isFacingTarget && !isMadjoey:

							Point shootVel = getVileShootVel(true);
							new StunShotProj(
						pos, xDir, MathF.Round(shootVel.byteAngle), this,
							player, player.getNextActorNetId(), rpc: true
							);


							break;
						case 7 when isFacingTarget:
							new VileBombProj(
							pos, xDir, 0, this, player,
							player.getNextActorNetId(), rpc: true);
							new VileBombProj(
							pos, xDir, 1, this, player,
							player.getNextActorNetId(), rpc: true);
							break;
					}
				}  else  {
					switch (Vattack) {
						case 1 when isFacingTarget&& linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
							break;
						case 2 when isFacingTarget && linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
							break;
						case 3 when isFacingTarget && linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
							break;
						case 4 when isFacingTarget && linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
							break;
						case 5 when isFacingTarget && linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
							break;
						case 6 when isFacingTarget && linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
							break;
						case 7 when isFacingTarget && linkedRideArmor != null:
							changeState(new CallDownMech(linkedRideArmor, true), true);
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
			(charState.attackCtrl && charState is not InRideArmor)) {
				//Projectile is not 
				if (!(proj.projId == (int)ProjIds.RollingShieldCharged || proj.projId == (int)ProjIds.RollingShield
					|| proj.projId == (int)ProjIds.MagnetMine || proj.projId == (int)ProjIds.FrostShield || proj.projId == (int)ProjIds.FrostShieldCharged
					|| proj.projId == (int)ProjIds.FrostShieldAir || proj.projId == (int)ProjIds.FrostShieldChargedPlatform || proj.projId == (int)ProjIds.FrostShieldPlatform)
				) {
					if (Helpers.randomRange(0, 1) == 1) {
						if (aiDodgeCD == 0 && linkedRideArmor != null) {
							changeState(new CallDownMech(linkedRideArmor, true), true);
						}
					} else {
						if (!(proj.projId == (int)ProjIds.SwordBlock) && grounded
								&& aiBlocktime <= 0) {
							turnToInput(player.input, player);
							changeState(new BlockWCUT(), true);;
							aiBlocktime = Helpers.randomRange(0, 60);
						}
					}
				}
			}
		}
	
		base.aiDodge(target);
	}
	

}
