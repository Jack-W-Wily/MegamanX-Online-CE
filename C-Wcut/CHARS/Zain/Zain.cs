using System;
using System.Collections.Generic;



namespace MMXOnline;

public class Zain : Character {
	public Zain(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.Zain;
		ShouldExplode = true;
		spriteFrameToSounds["zain_run/3"] = "ridewalk";
		spriteFrameToSounds["zain_run/0"] = "ridewalk";
		spriteFrameToSounds["zain_land/1"] = "ridewalk";
	}

	private float CounterTimer;

	private float CounterCooldown;

	private float SlashCooldown;



	public float ZainCounters;

	public override void landingCode(bool useSound = true) {
		base.landingCode(useSound);
		shakeCamera(sendRpc: true);
		playSound("crash", sendRpc: true);

	}

	public override bool normalCtrl() {

		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT());
			return true;
		}
		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);	
		}
		return base.normalCtrl();
	}

	public override bool attackCtrl() {
		if (player.input.isHeld(Control.Down, player) && sprite.name.Contains("spinslash")) {

		}
		if (player.input.isBPressed(player) && charState is not Dash) {

			if (grounded) {
				changeState(new ZainJab(), true);
			} else {
				changeState(new ZainAirDunk(), true);
			}
		}

		if (player.input.isAPressed(player)) {
			changeState(new ZainProjSwingState(grounded, shootProj: false), forceChange: true);
		}

		if (player.input.isL2Held(player) && player.input.isAPressed(player)) {
			changeState(new ZainGrabStab(), forceChange: true);
		}

		return base.attackCtrl();
	}



	public override void update() {
		base.update();


		if (ZainCounters == 0) player.superAmmo = 0;
		if (ZainCounters == 1) player.superAmmo = 4;
		if (ZainCounters == 2) player.superAmmo = 8;
		if (ZainCounters == 3) player.superAmmo = 12;
		if (ZainCounters == 4) player.superAmmo = 18;
		if (ZainCounters == 5) player.superAmmo = 20;
		if (ZainCounters == 6) player.superAmmo = 24;
		if (ZainCounters == 7) player.superAmmo = 28;
		if (ZainCounters == 8) player.superAmmo = 32;

		//Cooldowns
		//Helpers.decrementTime(ref CounterTimer);
		Helpers.decrementTime(ref CounterCooldown);
		Helpers.decrementTime(ref SlashCooldown);
		if (ZainCounters > 8) ZainCounters = 8;
		if (ZainCounters <= 0) {
			ZainCounters = 0;
			counterCooldown = 1;
		}
		//KillingSpreeThemes
		//if (KillingSpree == 3){
		//		if (musicSource == null) {
		//			addMusicSource("RequiemNitanchouDiesIrae", getCenterPos(), true); 
		///		}
		//} 

		if (player.input.isBHeld(player)
		&& (charState.attackCtrl || charState.bonusAttackCtrl) &&
		player.input.isHeld(Control.Dash, player) &&
		ZainCounters > 1
		) {
			changeState(new ZainDashParryState(), true);
			ZainCounters -= 2;
		}

		if ((charState.attackCtrl || charState.bonusAttackCtrl) && ZainCounters > 0 &&
		player.input.isR2Pressed(player)) {
			changeState(new ZainKokuSlash(grounded, shootProj: false), forceChange: true);
			ZainCounters -= 1;
		}
		if ((charState.attackCtrl || charState.bonusAttackCtrl) && ZainCounters > 3 &&
		player.input.isPressed(Control.Special2, player)) {

			if (player.input.isHeld(Control.Down, player)) {
				changeState(new ZainShinGroundStab(), true);
			} else if (player.input.isHeld(Control.Up, player)) {
				changeState(new ZainParryShinStartState(), true);
			} else {
				changeState(new ZainShinProjSwingState(grounded, shootProj: true), forceChange: true);

			}
			ZainCounters -= 4;
		}



		bool hadokenCheck2 = player.input.checkHadoken(player, xDir, Control.Special1);


		//	if ((charState.attackCtrl || charState.bonusAttackCtrl)  && ZainCounters > 1 &&
		//	hadokenCheck2)
		//	{	

		//		ZainCounters -= 2;
		//	}

		if ((charState.attackCtrl || charState.bonusAttackCtrl)
		 && player.input.isPressed(Control.WeaponLeft, player)
		 && player.input.isHeld(Control.Up, player)) {
			changeState(new ZainParryStartState(), true);
		}

		if (player.input.isPressed(Control.WeaponLeft, player)
		   && (charState.attackCtrl || charState.bonusAttackCtrl)
		   && !player.input.isHeld(Control.Up, player)
		   ) {
			if (unpoAbsorbedProj != null) {
				changeState(new ZainUPParryProjState(unpoAbsorbedProj, true, false), true);
				unpoAbsorbedProj = null;
			} else {
				changeState(new ZainUPParryStartState(), true);
			}
		}

		if (player.input.isPressed(Control.Special1, player) && genericParryCooldown == 0 &&
			 (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is XUPPunchState || charState is XUPGrabState)
		   ) {
			if (unpoAbsorbedProj != null) {
				changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
				unpoAbsorbedProj = null;
			}
		}

		if (ZainCounters >= 8) return;

		player.vileAmmo += Global.spf * 15;
		if (player.vileAmmo > player.vileMaxAmmo) {
			player.vileAmmo = 0;
			ZainCounters += 1;
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


	public override bool isToughGuyHyperMode() {
		return isAttacking();
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



	// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {

		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new SonicSlicer(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}

		if (sprite.name.Contains("rising")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.BlockableLaunch,
			player, 2f, 0, 10f, null, isShield: true, isDeflectShield: true, addToLevel: true);
		}
		if (sprite.name.Contains("spinslash")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.ZSaberRollingSlash, player,
				1, 10, 5f, isDeflectShield: true,
				isZSaberClang: true
			, addToLevel: true);
		}
		if (sprite.name.Contains("super_slash")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber3, player, 4f, 60, 15f, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("projswing_air")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber3, player, 4f, 30, 15f, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("jab")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.UPPunch, player, 2f, 10, 15f, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("parry_start")) {
			return new GenericMeleeProj(new SilkShot(), centerPoint,
			 ProjIds.ForceGrabState, player, 1f, 0, 15f, isZSaberClang: true
			 , addToLevel: true);
		}

		if (sprite.name.Contains("parry_dash")) {
			return new GenericMeleeProj(new SilkShot(), centerPoint,
			 ProjIds.ForceGrabState, player, 1f, 0, 15f, isZSaberClang: true
			 , addToLevel: true);
		}

		if (sprite.name.Contains("stabgrab") && !sprite.name.Contains("end")) {
			return new GenericMeleeProj(new SilkShot(), centerPoint,
			 ProjIds.ForceGrabState, player, 2f, 0, 15f, isZSaberClang: false
			 , addToLevel: true);
		}

		if (sprite.name.Contains("slash") && !sprite.name.Contains("uppercut")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber2, player, 3f, 20, 15f, isZSaberClang: true
			 , addToLevel: true);
		}
		if (sprite.name.Contains("uppercut")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber1, player, 3f, 20, 15f, isZSaberClang: true
			, addToLevel: true);
		}

		if (sprite.name.Contains("grab")
		&& !sprite.name.Contains("2") && !sprite.name.Contains("stab")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber1, player, 3f, 20, 15f, isZSaberClang: false
			 , addToLevel: true);

		}

		if (sprite.name.Contains("grab") && sprite.name.Contains("2")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.MechFrogStompShockwave, player, 1f, 0, 15f, isZSaberClang: true
			 , addToLevel: true);
		}

		if (sprite.name.Contains("groundstab")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber3, player, 3f, 20, 15f, isZSaberClang: true
			, addToLevel: true);
		}


		if (sprite.name.Contains("stabgrab_end")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.HeavyPush, player, 4f, 30, 15f, isZSaberClang: true
			 , addToLevel: true);
		}

		if (sprite.name.Contains("air_dunk")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.MechFrogGroundPound, player, 2f, 20, 15f, isZSaberClang: true
			 , addToLevel: true);
		}


		if (sprite.name.Contains("projswing") && !sprite.name.Contains("air")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.MechFrogGroundPound, player, 5f, 20, 15f, isZSaberClang: true
			 , addToLevel: true);
		}
		if (sprite.name.Contains("parry")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.MechFrogStompShockwave, player, 1f, 0, 15f
			, addToLevel: true);
		}
		if (sprite.name.Contains("thrust")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.SpreadShot, player, 2f, 0, 15f
			, addToLevel: true);
		}
		return null;
	}


	public override bool canDash() {
		return flag == null;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		//	if ((Options.main.enableSkins == true)
		//		&& Global.sprites.ContainsKey("zainalt_" + spriteName)){		
		//		return "zainalt_" + spriteName;
		//		}
		return "zain_" + spriteName;
	}



	public float AIHellBarrageCD;

	public bool AIStart;

	public bool isBoss;

	public override void aiAttack(Actor? target) {
		int Vattack = Helpers.randomRange(1, 7);
		Helpers.decrementFrames(ref AIHellBarrageCD);
		bool isTargetInAir = pos.y <= target?.pos.y - 20;
		bool isTargetClose = target?.getCenterPos().distanceTo(getCenterPos()) < 50;
		bool isWishinRangedMoves = target?.getCenterPos().distanceTo(getCenterPos()) < 120;
		bool isFacingTarget = (pos.x < target?.pos.x && xDir == 1) || (pos.x >= target?.pos.x && xDir == -1);
		if (Global.level.is1v1()) {
			isBoss = true;
		}
		if (isBoss) {
			player.superAmmo = player.superMaxAmmo;
		}



		if (!charState.isGrabbedState && !player.isDead && !isInvulnerableAttack()
					&& aiAttackCooldown <= 0 && charState.attackCtrl ) {

			if (isTargetClose  && grounded && !isTargetInAir) {
				switch (Vattack) {
					case 1 when isFacingTarget:

						changeState(new ZainJab());
						break;
					case 2 when isFacingTarget && ZainCounters > 0:
						changeState(new ZainKokuSlash(grounded, false));
						ZainCounters -= 1;
						break;
					case 3 when isFacingTarget && ZainCounters > 0:
						changeState(new ZainKokuRising(grounded, false));
						player.press(Control.Up);
						ZainCounters -= 1;
						break;
					case 4 when isFacingTarget && ZainCounters > 0:
						changeState(new ZainKokuStab(grounded, false));
						player.press(Control.Down);
						ZainCounters -= 1;
						break;
					case 5 when isFacingTarget:
						changeState(new ZainGrabStab());
						break;
					case 6 when isFacingTarget:
						changeState(new ZainProjSwingState(grounded, false));
						break;
					case 7 when isFacingTarget:
						changeState(new ZainGroundStab());
						break;
				}
			}

			if (isTargetClose  && !grounded || isTargetInAir && isTargetClose) {
				switch (Vattack) {
					case 1 when isFacingTarget:

							changeState(new ZainKokuRising(grounded, false));
						break;
					case 2 when isFacingTarget && ZainCounters > 0:
							changeState(new ZainKokuRising(grounded, false));
						ZainCounters -= 1;
						break;
					case 3 when isFacingTarget && ZainCounters > 0:
							changeState(new ZainKokuRising(grounded, false));
						player.press(Control.Up);
						ZainCounters -= 1;
						break;
					case 4 when isFacingTarget && ZainCounters > 0:
							changeState(new ZainKokuRising(grounded, false));
						player.press(Control.Down);
						ZainCounters -= 1;
						break;
					case 5 when isFacingTarget:
							changeState(new ZainKokuRising(grounded, false));
						break;
					case 6 when isFacingTarget:
							changeState(new ZainKokuRising(grounded, false));
						break;
					case 7 when isFacingTarget:
							changeState(new ZainKokuRising(grounded, false));
						break;
				}
			}



			if (!isTargetClose && isWishinRangedMoves && grounded) {
				switch (Vattack) {
					case 1 when isFacingTarget && ZainCounters >= 2:
						changeState(new ZainDashParryState());
		if (bonusHealth > 0)	ZainCounters -= 2;
						break;
					case 2 when isFacingTarget:
						changeState(new ZainBossJumpStart());
						break;
					case 3 when isFacingTarget:
						changeState(new ClaudioBossDash());
						break;
					case 4 when isFacingTarget:
						changeState(new ZainBossJump());
						break;
					case 5 when isFacingTarget && ZainCounters >= 4:
						changeState(new ZainParryShinStartState(), true);
			if (bonusHealth > 0)	ZainCounters -= 4;
						addHealth(10);
						break;
					case 6 when isFacingTarget && ZainCounters >= 4:
						changeState(new ZainShinGroundStab(), true);
		if (bonusHealth > 0)	ZainCounters -= 4;
						break;
					case 7 when isFacingTarget && ZainCounters >= 4:
						changeState(new ZainShinProjSwingState(grounded, shootProj: true), forceChange: true);
		if (bonusHealth > 0)	ZainCounters -= 4;
						break;
				}
			}

			aiAttackCooldown = Helpers.randomRange(0, 20);
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
					if (grounded) {
						if (aiDodgeCD == 0 && !isDashing) {
							if (Helpers.randomRange(0, 2) == 1){
								changeState(new ZainUPParryStartState(), true);
							} else if (Helpers.randomRange(0, 2) == 2) {
								changeState(new ZainParryStartState(), true);
							} else {
								changeState(new ZainParryShinStartState(), true);
							}
							aiDodgeCD = Helpers.randomRange(0, 30);

						}
					}
				}
			}
		}

		base.aiDodge(target);
	}

}

