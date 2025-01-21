using System;
using System.Collections.Generic;

namespace MMXOnline;

public class Zero : Character {
	// Hypermode stuff.
	public bool isViral;
	public int awakenedPhase;
	public bool isAwakened => (awakenedPhase != 0);
	public bool isGenmuZero => (awakenedPhase >= 2);
	public bool isBlack;
	public int hyperMode;

	// Hypermode timers.
	public static readonly float maxBlackZeroTime = 40 * 60;
	public float hyperModeTimer;
	public float scrapDrainCounter = 120;
	public bool hyperOvertimeActive;

	// Hypermode effects stuff.
	public int awakenedAuraFrame;
	public float awakenedAuraAnimTime;
	public byte hypermodeBlink;
	public Sprite auraSprite = new Sprite("zero_awakened_aura");
	public Sprite auraSprite2 = new Sprite("zero_awakened_aura2");

	// Weapons.
	public ZSaber meleeWeapon = new();
	public PZeroParryWeapon parryWeapon = new();
	public AwakenedAura awakenedAuraWeapon = new();
	public ZSaberProjSwing saberSwingWeapon = new();
	public ZeroBuster busterWeapon = new();

	// Loadout weapons.
	public Weapon groundSpecial;
	public Weapon airSpecial;
	public Weapon uppercutA;
	public Weapon uppercutS;
	public Weapon downThrustA;
	public Weapon downThrustS;
	public Weapon gigaAttack;
	public int gigaAttackSelected;

	// Inputs.
	public int shootPressTime;
	public int specialPressTime;
	public int swingPressTime;
	public bool shootPressed => (shootPressTime > 0);
	public bool specialPressed => (specialPressTime > 0);
	public bool swingPressed => (swingPressTime > 0);

	// Cooldowns.
	public float dashAttackCooldown;
	public float hadangekiCooldown;
	public float genmureiCooldown;
	public int airRisingUses;

	// Hypermode stuff.
	public float donutTimer;
	public int donutsPending;
	public int freeBusterShots;

	// Triple Slash damage.
	public float zeroTripleStartTime;
	public float zeroTripleSlashEndTime;

	// AI stuff.
	public bool isWildDance;
	public float aiBlocktime;
	public float aiAttackCooldown;

	// Creation code.
	public Zero(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer, bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.BusterZero;
		// Loadout stuff.
		ZeroLoadout zeroLoadout = player.loadout.zeroLoadout;

		groundSpecial = new RaijingekiWeapon();
		airSpecial = KuuenzanWeapon.getWeaponFromIndex(zeroLoadout.airSpecial);
		uppercutA = RyuenjinWeapon.getWeaponFromIndex(zeroLoadout.uppercutA);
		uppercutS = RyuenjinWeapon.getWeaponFromIndex(zeroLoadout.uppercutS);
		downThrustA =  HyouretsuzanWeapon.getWeaponFromIndex(zeroLoadout.downThrustA);
		downThrustS = HyouretsuzanWeapon.getWeaponFromIndex(zeroLoadout.downThrustS);

		gigaAttackSelected = zeroLoadout.gigaAttack;
		gigaAttack = zeroLoadout.gigaAttack switch {
			1 => new CFlasher(),
			2 => new RekkohaWeapon(),
			_ => new RakuhouhaWeapon(),
		};

		hyperMode = zeroLoadout.hyperMode;
		altCtrlsLength = 2;
		spriteFrameToSounds["zero_run/5"] = "zerowalkx4";
		spriteFrameToSounds["zero_run/11"] = "zerowalkx4";
	}



	public override void preUpdate() {
		base.preUpdate();
		if (grounded && charState is not ZeroUppercut) {
			airRisingUses = 0;
		}
	}

	public override void update() {


		if (isViral && flag == null && (
			 charState is AirDash)) {
			changeState(new FSplasherState(), true);
			
		}


		if (charState is Crouch){
				if (gigaAttack.ammo >= 28 &&
				player.input.isPressed(Control.WeaponRight, player) ) {
					gigaAttack.addAmmo(-28, player);

					if (isAwakened){
					changeState(new GenmureiState(), true);	
					}
					if (isBlack){
					changeState(new ZeroFinalStart(), true);	
					}

					if (isViral){
					changeState(new Rakuhouha(new DarkHoldWeapon()), true);	
					} 
					if (!isViral && !isBlack && !isAwakened) {
						changeState(new Rakuhouha(new RakuhouhaWeapon()), true);
					}
				
				} 
		}

	



		// Hypermode effects.



		if (isAwakened) {
			updateAwakenedAura();
		}
		// Hypermode music.
		if (!Global.level.isHyper1v1()) {
			if (isBlack) {
				if (musicSource == null) {
					addMusicSource("zero_X3", getCenterPos(), true);
				}
			} else if (isAwakened) {
				if (musicSource == null) {
					addMusicSource("XvsZeroV2_megasfc", getCenterPos(), true);
				}
			} else if (isViral && ownedByLocalPlayer) {
				if (musicSource == null) {
					addMusicSource("introStageZeroX5_megasfc", getCenterPos(), true);
				}
			} else {
				destroyMusicSource();
			}
		}
		if (!ownedByLocalPlayer) {
			base.update();
			return;
		}

		// Local update starts here.
		inputUpdate();
		Helpers.decrementFrames(ref donutTimer);
		Helpers.decrementFrames(ref hadangekiCooldown);
		Helpers.decrementFrames(ref genmureiCooldown);
		Helpers.decrementFrames(ref dashAttackCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		airSpecial.update();
		gigaAttack.update();
		gigaAttack.charLinkedUpdate(this, true);
		base.update();

		// Hypermode timer.
		if (hyperModeTimer > 0) {
			hyperModeTimer -= Global.speedMul;
			if (hyperModeTimer <= 180) {
				hypermodeBlink = (byte)MathInt.Ceiling(hyperModeTimer - 180);
			}
			if (hyperModeTimer <= 0) {
				hypermodeBlink = 0;
				hyperModeTimer = 0;
				if (hyperOvertimeActive && isAwakened && player.currency >= 4) {
					awakenedPhase = 2;
					heal(player, (float)maxHealth * 2, true);
					gigaAttack.addAmmoPercentHeal(100);
				} else {
					awakenedPhase = 0;
					isBlack = false;
					float oldAmmo = gigaAttack.ammo;
					gigaAttack = gigaAttackSelected switch {
						1 => new CFlasher(),
						2 => new RekkohaWeapon(),
						_ => new RakuhouhaWeapon(),
					};
					gigaAttack.ammo = oldAmmo;
				}
				hyperOvertimeActive = false;
			}
		}
		// Genmu Zero scrap drain.
		else if (awakenedPhase == 2) {
			if (scrapDrainCounter > 0) {
				scrapDrainCounter--;
			} else {
				scrapDrainCounter = 320;
				player.currency--;
				if (player.currency < 0) {
					player.currency = 0;
					awakenedPhase = 0;
					isBlack = false;
					hyperOvertimeActive = false;
				}
			}
		}
		// For the shooting animation.
		if (shootAnimTime > 0) {
			shootAnimTime -= Global.speedMul;
			if (shootAnimTime <= 0) {
				shootAnimTime = 0;
				changeSpriteFromName(charState.defaultSprite, false);
				if (charState is WallSlide) {
					frameIndex = sprite.totalFrameNum - 1;
				}
			}
		}
		// For the donuts.
		if (donutsPending > 0 && donutTimer <= 0) {
			shootDonutProj(donutsPending * 9);
			donutsPending--;
			donutTimer = 9;
		}
		// Charge and release charge logic.
		if (!isInDamageSprite()){
		if (isAwakened) {
			chargeLogic(shootDonuts);
		} else {
			chargeLogic(shoot);
		}
		}
	}

	// Flags.
	public bool hypermodeActive() {
		return isBlack || isAwakened || isViral;
	}

	// Shoot logic and stuff.
	public override bool canShoot() {
//		return (!charState.invincible && !isInvulnerable() &&
//			(charState.attackCtrl || (charState.altCtrls.Length >= 2 && charState.altCtrls[1]))
//		);
		return true;
	}


	public override int getMaxChargeLevel() {
		return isBlack ? 4 : 3;
	}
	
	public override bool canCharge() {
		return ( !isInvulnerable
			(charState.attackCtrl || getChargeLevel() > 0) &&
			(gigaAttack.ammo >= 6 || freeBusterShots > 0) &&
			donutsPending == 0
		);
	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Shoot, player)
		||  player.input.isHeld(Control.Special1, player)
		|| player.input.isHeld(Control.WeaponRight, player)
		||  player.input.isHeld(Control.WeaponLeft, player)
		
		
		;
	}

	public void shoot(int chargeLevel) {
		if (player.currency <= 0 && freeBusterShots <= 0) { return; }
		if (chargeLevel == 0) { return; }
		int currencyUse = 0;

		// Cancel non-invincible states.
		if (!charState.attackCtrl && !charState.invincible) {
			changeToIdleOrFall();
		}
		// Shoot anim and vars.
		setShootAnim();
		Point shootPos = getShootPos();
		int xDir = getShootXDir();

		// Shoot stuff.
		if (chargeLevel == 1) {
			currencyUse = 1;
			playSound("buster2X3", sendRpc: true);
			new ZBuster2Proj(
				shootPos, xDir, 0, player, player.getNextActorNetId(), rpc: true
			);
		} else if (chargeLevel == 2) {
			currencyUse = 1;
			playSound("buster3X3", sendRpc: true);
			new ZBuster3Proj(
				shootPos, xDir, 0, player, player.getNextActorNetId(), rpc: true
			);
		} else if (chargeLevel == 3 || chargeLevel >= 4) {

			if (!player.input.isHeld(Control.Down, player)){
				currencyUse = 1;

				if (!isBlack){
				playSound("buster4", sendRpc: true);
				new DZBuster3Proj(
				shootPos, xDir, player, player.getNextActorNetId(), rpc: true
				);
				} else {
				changeState(new ZeroDoubleBuster(false, false), true);
		
				}
				} else {
			changeState(new TBreakerState(false), true);
			}
		}
		if (currencyUse > 0) {
			if (freeBusterShots > 0) {
				freeBusterShots--;
			} else if (player.currency > 0) {
				gigaAttack.ammo -= 6;
				//player.currency--;
			}
		}
	}

	public void setShootAnim() {
		string shootSprite = getSprite(charState.shootSprite);
		if (!Global.sprites.ContainsKey(shootSprite)) {
			if (grounded) { shootSprite = "zero_shoot"; }
			else { shootSprite = "zero_fall_shoot"; }
		}
		if (shootAnimTime == 0) {
			changeSprite(shootSprite, false);
		} else if (charState is Idle) {
			frameIndex = 0;
			frameTime = 0;
		}
		if (charState is LadderClimb) {
			if (player.input.isHeld(Control.Left, player)) {
				this.xDir = -1;
			} else if (player.input.isHeld(Control.Right, player)) {
				this.xDir = 1;
			}
		}
		shootAnimTime = DefaultShootAnimTime;
	}

	public void shootDonuts(int chargeLevel) {
		if (player.currency <= 0 && freeBusterShots <= 0) { return; }
		if (chargeLevel == 0) { return; }
		int currencyUse = 0;

		// Cancel non-invincible states.
		if (!charState.attackCtrl && !charState.invincible) {
			changeToIdleOrFall();
		}
		// Shoot anim and vars.
		setShootAnim();
		shootDonutProj(0);
		if (chargeLevel >= 2) {
			donutTimer = 9;
			donutsPending = (chargeLevel - 1);
		}
		currencyUse = 0;
		if (currencyUse > 0) {
			if (freeBusterShots > 0) {
				freeBusterShots--;
			} else if (player.currency > 0) {
			//	player.currency--;
				gigaAttack.ammo -= 6;
			}
		}
	}

	public void shootDonutProj(int time) {
		setShootAnim();
		Point shootPos = getShootPos();
		int xDir = getShootXDir();

		new ShingetsurinProj(
			shootPos, xDir,
			time / 60f, player, player.getNextActorNetId(), rpc: true
		);
		playSound("shingetsurinx5", forcePlay: false, sendRpc: true);
		shootAnimTime = DefaultShootAnimTime;
	}

	public void updateAwakenedAura() {
		awakenedAuraAnimTime += Global.speedMul;
		if (awakenedAuraAnimTime > 4) {
			awakenedAuraAnimTime = 0;
			awakenedAuraFrame++;
			if (awakenedAuraFrame > 3) {
				awakenedAuraFrame = 0;
			}
		}
	}

	// To make combo attacks easier Zero inputs have a leitency if 6 frames.
	public void inputUpdate() {
		if (shootPressTime > 0) {
			shootPressTime--;
		}
		if (specialPressTime > 0) {
			specialPressTime--;
		}
		if (swingPressTime > 0) {
			swingPressTime--;
		}
		if (player.input.isPressed(Control.Shoot, player)) {
			shootPressTime = 6;
		}
		if (player.input.isPressed(Control.Special1, player)) {
			specialPressTime = 6;
		}
		if (player.input.isPressed(Control.WeaponRight, player) && isAwakened) {
			swingPressTime = 6;
		}
	}

	// Non-attacks like guard and hypermode activation.
	public override bool normalCtrl() {
		// Hypermode activation.
		int cost = Player.zeroHyperCost;
		if (isAwakened) {
			cost = 4;
		}
		if (player.currency >= cost &&
			player.input.isHeld(Control.Special2, player) &&
			charState is not HyperZeroStart and not WarpIn && (
				!isViral && !isAwakened && !isBlack ||
				isAwakened && !hyperOvertimeActive && player.currency >= 2
			)
		) {
			hyperProgress += Global.spf;
		} else {
			hyperProgress = 0;
		}
		if (hyperProgress >= 1 && (isViral || isAwakened || isBlack)) {
			hyperProgress = 0;
			hyperOvertimeActive = true;
			Global.level.gameMode.setHUDErrorMessage(player, "Overtime mode active");
		}
		else if (hyperProgress >= 1 && player.currency >= Player.zeroHyperCost) {
			hyperProgress = 0;
			changeState(new HyperZeroStart(), true);
			return true;
		}
		// If we changed state this frame. Return.
		// This is to prevent jumping guard shenanigans.
		bool changedState = base.normalCtrl();
		if (changedState) {
			return true;
		}
		// Guard! (You can thank Axl for this mess)
		if (charState.attackCtrl && charState is not Dash && grounded && 
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);
			changeState(new SwordBlock());
			return true;
		} 
		
		
		return false;
	}

	public override bool attackCtrl() {
		// To prevent XDiego skillcheck we check if we are shooting donuts.
		// If we are doing so we do not attack.

	
		if (donutsPending != 0) {
			return false;
		}
		if (isAwakened && swingPressTime > 0 && hadangekiCooldown == 0 && isGenmuZero) {
			hadangekiCooldown = 60;
			if (charState is WallSlide wallSlide) {
				changeState(new AwakenedZeroHadangekiWall(wallSlide.wallDir, wallSlide.wallCollider), true);
				return true;
			}
			if (isDashing && grounded) {
				slideVel = xDir * getDashSpeed() * 0.9f;
			}
			if (grounded && vel.y >= 0 && isGenmuZero) {
				if (genmureiCooldown == 0) {
					genmureiCooldown = 120;
					changeState(new GenmureiState(), true);
					return true;
				}
			} else {
				changeState(new AwakenedZeroHadangeki(), true);
				return true;
			}
		}
		if (grounded && vel.y >= 0) {
			return groundAttacks();
		} else {
			return airAttacks();
		}
	}

	public bool groundAttacks() {
		int yDir = player.input.getYDir(player);
		// Giga attacks.
		if (isBlack && player.input.isPressed(Control.WeaponLeft,player)){
		
		changeState(new SaberParryStartState(), true);
		}
		if (yDir != 1 && yDir != -1){
			if (gigaAttack.ammo >= 10 &&
				player.input.isPressed(Control.WeaponRight, player) ) {
					gigaAttack.addAmmo(-10, player);
					changeState(new AwakenedZeroHadangeki(), true);
					return true;
				} 
		}


		
		if (yDir == 1) {
				
				 if (gigaAttack.shootCooldown <= 0 && gigaAttack.ammo >= 14 &&
				player.input.isPressed(Control.Special1, player)) {
					gigaAttack.addAmmo(-14, player);
					
					if (!isAwakened){
					changeState(new Rakuhouha(new CFlasher()), true);
					} else {
					changeState(new Rakuhouha(new ShinMessenkou()), true);		
					}
					return true;
				}
			//	 if (gigaAttack.shootCooldown <= 0 && gigaAttack.ammo >= 10 &&
			///	player.input.isPressed(Control.WeaponLeft, player)) {
			//		gigaAttack.addAmmo(-10, player);
			//		changeState(new ZeroRocks( new FakeZeroWeapon(player)), true);
			//		return true;
			//	}
			
			if (!shootPressed) {
				return true;
			}
		}
		// Uppercuts.
		if (yDir == -1 && (shootPressed)) {
		
			int weaponType = (int)RisingType.RisingFang;
		
			changeState(new ZeroUppercut(weaponType, isUnderwater()), true);
			return true;
		}
		if (yDir == -1 && (specialPressed)) {
		
			int weaponType = (int)RisingType.Ryuenjin;
		
			changeState(new ZeroUppercut(weaponType, isUnderwater()), true);
			return true;
		}
		if (yDir == -1 && (player.input.isPressed(Control.WeaponRight, player))) {
		
			int weaponType = (int)RisingType.Denjin;
		
			changeState(new ZeroUppercut(weaponType, isUnderwater()), true);
			return true;
		}
		// Dash attacks.
		if (isDashing && (shootPressed || specialPressed)) {
			// Do nothing if we dashed already.
			if (dashAttackCooldown > 0) {
				return false;
			}
			dashAttackCooldown = 60;
			slideVel = xDir * getDashSpeed();
			if (specialPressTime > shootPressTime) {
				if (!isViral){
				changeState(new ZeroShippuugaState(), true);
				} else {
				changeState(new SuiretsusanState(false), true);
				 
				}
				return true;
			}
			changeState(new ZeroDashSlashState(), true);
			return true;
		}
		// Use special if pressed first.
		if (specialPressed && specialPressTime > shootPressTime) {
			groundSpecial.attack(this);
		}
		// Regular slashes.
		if (shootPressed) {
			// Crounch variant.
			if (yDir == 1) {
				changeState(new ZeroCrouchSlashState(), true);
				return true;
			}
			changeState(new ZeroSlash1State(), true);
			return true;
		}
		return false;
	}

	public bool airAttacks() {
		int yDir = player.input.getYDir(player);



		if (yDir == -1 && airRisingUses == 0 && flag == null && (
			 shootPressed)) {
			changeState(new ZeroUppercut(RisingType.RisingFang, isUnderwater()), true);
			dashedInAir++;
			return true;
		}
		



		
		if (yDir == 1 && (shootPressed || specialPressed)) {
			// Weapon type to use.
			int weaponType = (int)ZeroDownthrustType.QuakeBlazer;
			// If special was pressed first.
			if (specialPressTime > shootPressTime) {
				weaponType = (int)ZeroDownthrustType.Hyouretsuzan;
			}
			changeState(new ZeroDownthrust(weaponType), true);
			return true;
		}
		// Air attack.
		if (specialPressed) {

			if (!isViral){
			if (charState is not ZeroRollingSlashtate) {
				changeState(new ZeroRollingSlashtate(), true);
			}
			} else {
			if (charState is not CMoonState){
			changeState(new CMoonState(), true);
			}
			}
			return true;
		}
		// Air attack.
		if (shootPressed) {
			if (charState is WallSlide wallSlide) {
				changeState(new ZeroMeleeWall(wallSlide.wallDir, wallSlide.wallCollider), true);
			} else {
				changeState(new ZeroAirSlashState(), true);
			}
			return true;
		}
		return false;
	}

	public override bool altCtrl(bool[] ctrls) {
		if (charState is ZeroGenericMeleeState zgms) {
			zgms.altCtrlUpdate(ctrls);
		}
		return base.altCtrl(ctrls);
	}

	// This is to prevent accidental combo activation between attacks.

	// This is to prevent accidental combo activation between attacks.
	public override bool changeState(CharState newState, bool forceChange = false) {
		// Save old state.
		CharState oldState = charState;
		// Base function call.
		bool hasChanged = base.changeState(newState, forceChange);
		if (!hasChanged) {
			return false;
		}
		if (!newState.attackCtrl || newState.attackCtrl != oldState.attackCtrl) {
			shootPressTime = 0;
			specialPressTime = 0;
		}
		return true;
	}

	// Movement and stuff.
	
	// Double jump.
	public override bool canAirJump() {
		return dashedInAir == 0;
	}

	public override float getRunSpeed() {
		float runSpeed = Physics.WalkSpeed;
		if (isBlack || isViral) {
			runSpeed *= 1.15f;
		}
		return runSpeed * getRunDebuffs();
	}

	public override float getDashSpeed() {
		if (flag != null || !isDashing) {
			return getRunSpeed();
		}
		float dashSpeed = 210;
		if (isBlack || isViral) {
			dashSpeed *= 1.15f;
		}
		return dashSpeed * getRunDebuffs();
	}
	public override string getSprite(string spriteName) {
		return "zero_" + spriteName;
	}

	// Simple giga ammo logic.
	public override void addAmmo(float amount) {
		gigaAttack.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		gigaAttack.addAmmoPercentHeal(amount);
	}

	public override bool canAddAmmo() {
		return (gigaAttack.ammo < gigaAttack.maxAmmo);
	}

	public override bool isToughGuyHyperMode() {
		return isGenmuZero;
	}

	// Melee projectiles.
	public override Projectile? getProjFromHitbox(Collider hitbox, Point centerPoint) {
		int meleeId = getHitboxMeleeId(hitbox);
		if (meleeId == -1) {
			return null;
		}
		Projectile? proj = getMeleeProjById(meleeId, centerPoint);
		if (proj == null) {
			return null;
		}
		// Assing data variables.
		proj.meleeId = meleeId;
		proj.owningActor = this;

		// Damage based on tripleSlash time.
		if (meleeId == (int)MeleeIds.HuhSlash) {
			float timeSinceStart = zeroTripleSlashEndTime - zeroTripleStartTime;
			float overrideDamage = 2;
			int overrideFlinch = Global.defFlinch;
			if (timeSinceStart < 0.5f) {
				overrideDamage = 1;
			}
			proj.damager.damage = overrideDamage;
			proj.damager.flinch = overrideFlinch;
		}
		// Damage based on fall speed.
		else if (meleeId == (int)MeleeIds.Rakukojin) {
			float damage = 3 + Helpers.clamp(MathF.Floor(deltaPos.y * 0.8f), 0, 10);
			proj.damager.damage = damage;
		}
		updateProjFromHitbox(proj);
		return proj;
	}

	public enum MeleeIds {
		None = -1,
		// Ground
		HuSlash,
		HaSlash,
		HuhSlash,
		CrouchSlash,
		// Dash
		DashSlash,
		Shippuuga,
		// Air
		AirSlash,
		RollingSlash,
		Hyoroga,
		// Ground Specials
		Raijingeki,
		RaijingekiWeak,
		Dairettsui,
		Suiretsusen,
		// Up Specials
		Ryuenjin,
		Denjin,
		RisingFang,
		// Down specials
		Hyouretsuzan,
		Danchien,
		Rakukojin,
		// Others
		LadderSlash,
		WallSlash,
		Gokumonken,
		Hadangeki,
		ZeroFinal,
		AwakenedAura
	}

	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			// Ground
			"zero_attack" => MeleeIds.HuSlash,
			"zero_attack2" => MeleeIds.HaSlash,
			"zero_attack3" => MeleeIds.HuhSlash,
			"zero_attack_crouch" => MeleeIds.CrouchSlash,
			// Dash
			"zero_attack_dash" => MeleeIds.DashSlash,
			"zero_attack_dash2" => MeleeIds.Shippuuga,
			// Air
			"zero_attack_air" => MeleeIds.AirSlash,
			"zero_attack_air2" => MeleeIds.RollingSlash,
			"zero_cmoon" => MeleeIds.RollingSlash,
			"zero_hyoroga_attack"  => MeleeIds.Hyoroga,
			// Ground Speiclas
			"zero_raijingeki" => MeleeIds.Raijingeki,
			"zero_raijingeki2" => MeleeIds.RaijingekiWeak,
			"zero_tbreaker" => MeleeIds.Dairettsui,
			"zero_spear" => MeleeIds.Suiretsusen,
			// Up Specials
			"zero_ryuenjin" => MeleeIds.Ryuenjin,
			"zero_eblade" => MeleeIds.Denjin,
			"zero_rising" => MeleeIds.RisingFang,
			// Down specials
			"zero_hyouretsuzan_start" or "zero_hyouretsuzan_fall" => MeleeIds.Hyouretsuzan,
			"zero_quakeblazer_start" or "zero_quakeblazer_fall" => MeleeIds.Danchien,
			"zero_rakukojin_start" or "zero_rakukojin_fall" => MeleeIds.Rakukojin,
			// Others.
			"zero_ladder_attack" => MeleeIds.LadderSlash,
			"zero_wall_slide_attack" => MeleeIds.WallSlash,
			"zero_block" => MeleeIds.Gokumonken,
			"zero_final_start" => MeleeIds.ZeroFinal,
			"zero_final_end" => MeleeIds.Dairettsui,
			"zero_projswing" => MeleeIds.Hadangeki,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		return id switch {
			// Ground
			(int)MeleeIds.HuSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaber1, player, 2, Global.miniFlinch,  10f, isReflectShield: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.HaSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaber2, player, 1, Global.miniFlinch, 10f, isReflectShield: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.HuhSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaber3, player,
				1, Global.halfFlinch, 5, isReflectShield: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.CrouchSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberCrouch, player, 2, 8, 5, isReflectShield: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			// Dash
			(int)MeleeIds.DashSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberDash, player, 2, 10, 5, isReflectShield: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.ZeroFinal => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.VileAirRaidStart, player, 2, 20, 5, isReflectShield: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Shippuuga => new GenericMeleeProj(
				ShippuugaWeapon.staticWeapon, projPos, ProjIds.Shippuuga, player, 2, Global.defFlinch, 5f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			// Air
			(int)MeleeIds.AirSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberAir, player, 2, Global.miniFlinch, 7f, isReflectShield: true,
				isJuggleProjectile: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.RollingSlash =>  new GenericMeleeProj(
				KuuenzanWeapon.staticWeapon, projPos, ProjIds.ZSaberRollingSlash, player,
				1, 10, 10, isDeflectShield: true,
				isJuggleProjectile: true,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Hyoroga => new GenericMeleeProj(
				HyorogaWeapon.staticWeapon, projPos, ProjIds.HyorogaSwing, player, 4, 0, 10f,
				
				addToLevel: addToLevel
			),
			// Ground Specials
			(int)MeleeIds.Raijingeki => new GenericMeleeProj(
				RaijingekiWeapon.staticWeapon, projPos, ProjIds.Raijingeki, player, 1, 0, 6f,
				addToLevel: addToLevel
			),
			(int)MeleeIds.RaijingekiWeak => new GenericMeleeProj(
				Raijingeki2Weapon.staticWeapon, projPos, ProjIds.Raijingeki2, player, 1, 0, 6f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Dairettsui => new GenericMeleeProj(
				TBreakerWeapon.staticWeapon, projPos, ProjIds.TBreaker, player, 5, Global.defFlinch, 5f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Suiretsusen => new GenericMeleeProj(
				SuiretsusenWeapon.staticWeapon, projPos, ProjIds.SuiretsusanProj, player, 2, Global.defFlinch, 5f,
				addToLevel: addToLevel
			),
			// Up Specials
			(int)MeleeIds.Ryuenjin => new GenericMeleeProj(
				RyuenjinWeapon.staticWeapon, projPos, ProjIds.Ryuenjin, player, 3, Global.defFlinch, 55f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Denjin => new GenericMeleeProj(
				DenjinWeapon.staticWeapon, projPos, ProjIds.Denjin, player, 1, Global.defFlinch, 5f,
				ShouldClang : true,
				isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.RisingFang => new GenericMeleeProj(
				RisingFangWeapon.staticWeapon, projPos, ProjIds.RisingFang, player, 1, 20, 25f,
				ShouldClang : true,
				isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			// Down specials
			(int)MeleeIds.Hyouretsuzan => new GenericMeleeProj(
				HyouretsuzanWeapon.staticWeapon, projPos, ProjIds.Hyouretsuzan2, player, 1, 12, 10f,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Danchien => new GenericMeleeProj(
				DanchienWeapon.staticWeapon, projPos, ProjIds.QuakeBlazer, player, 2, 10, 60f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Rakukojin => new GenericMeleeProj(
				RakukojinWeapon.staticWeapon, projPos, ProjIds.Rakukojin, player, 2, 12, 60f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			// Others
			(int)MeleeIds.LadderSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberLadder, player, 3, 0, 15, isReflectShield: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.WallSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberslide, player, 3, 0, 15, isReflectShield: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Gokumonken => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.SwordBlock, player, 0, 0, 0, isDeflectShield: true,
				addToLevel: addToLevel
			) {
				highPiority = true
			},
			(int)MeleeIds.Hadangeki => new GenericMeleeProj(
				saberSwingWeapon, projPos, ProjIds.ZSaberProjSwing, player,
				3, Global.defFlinch, 30, isReflectShield: true,
				addToLevel: addToLevel
			),
			/* AZ nerf
			(int)MeleeIds.AwakenedAura => new GenericMeleeProj(
				awakenedAuraWeapon, projPos, ProjIds.AwakenedAura, player,
				0, 0, 80,
				addToLevel: addToLevel
			),*/
			_ => null
		};
	}

	// Awakened aura.
	public override Dictionary<int, Func<Projectile>> getGlobalProjs() {
		if (isAwakened && globalCollider != null) {
			Dictionary<int, Func<Projectile>> retProjs = new() {
				[(int)ProjIds.AwakenedAura] = () => {
					playSound("awakenedaura", forcePlay: true, sendRpc: true); 
					Point centerPoint = globalCollider.shape.getRect().center();
					float damage = 2;
					int flinch = 0;
					if (isGenmuZero) {
						damage = 0;
						flinch = Global.defFlinch;
					}
					Projectile proj = new GenericMeleeProj(
						awakenedAuraWeapon, centerPoint,
						ProjIds.AwakenedAura, player, damage, flinch, 30
					) {
						globalCollider = globalCollider.clone(),
						meleeId = (int)MeleeIds.AwakenedAura,
						owningActor = this
					};
					return proj;
				}
			};
			return retProjs;
		}
		return base.getGlobalProjs();
	}

	public override void updateProjFromHitbox(Projectile proj) {
		if (proj.projId == (int)ProjIds.AwakenedAura) {
			if (isGenmuZero) {
				proj.damager.damage = 0;
				proj.damager.flinch =0;// Global.defFlinch;
			}
		}
	}
	
	// Shader and display.
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;
		if (isBlack) {
			palette = player.zeroPaletteShader;
			palette?.SetUniform("palette", 1);
			palette?.SetUniform("paletteTexture", Global.textures["hyperZeroPalette"]);
		}
		if (isAwakened) {
			palette = player.zeroAzPaletteShader;
		}
		if (isViral) {
			palette = player.nightmareZeroShader;
		}
		if (palette != null && hypermodeBlink > 0) {
			float blinkRate = MathInt.Ceiling(hypermodeBlink / 30f);
			palette = ((Global.frameCount % (blinkRate * 2) >= blinkRate) ? null : palette);
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

	public override Point getParasitePos() {
		if (sprite.name.Contains("_ra_")) {
			return pos.addxy(0, -6);
		}
		return pos.addxy(0, -20);
	}

	public override float getLabelOffY() {
		if (sprite.name.Contains("_ra_")) {
			return 25;
		}
		return 45;
	}
	
	public override void render(float x, float y) {
		if (isViral && visible) {
			addRenderEffect(RenderEffectType.Trail);
		} else {
			removeRenderEffect(RenderEffectType.Trail);
		}
		if (isBlack && visible) {
			addRenderEffect(RenderEffectType.SpeedDevilTrail);
		} else {
			removeRenderEffect(RenderEffectType.SpeedDevilTrail);
		}
		float auraAlpha = 1;
		if (isAwakened && visible && hypermodeBlink > 0) {
			float blinkRate = MathInt.Ceiling(hypermodeBlink / 2f);
			bool blinkActive = Global.frameCount % (blinkRate * 2) >= blinkRate;
			if (!blinkActive) {
				auraAlpha = 0.5f;
			}
		}
		if (isAwakened && visible) {
			float xOff = 0;
			int auraXDir = 1;
			float yOff = 5;
			Sprite auraSprite = this.auraSprite;
			if (sprite.name.Contains("dash")) {
				auraSprite = auraSprite2;
				auraXDir = xDir;
				yOff = 8;
			}
			var shaders = new List<ShaderWrapper>();
			if (isGenmuZero &&
				Global.frameCount % Global.normalizeFrames(6) > Global.normalizeFrames(3) &&
				Global.shaderWrappers.ContainsKey("awakened")
			) {
				shaders.Add(Global.shaderWrappers["awakened"]);
			}
			auraSprite.draw(
				awakenedAuraFrame,
				pos.x + x + (xOff * auraXDir),
				pos.y + y + yOff, auraXDir,
				1, null, auraAlpha, 1, 1,
				zIndex - 1, shaders: shaders
			);
		}
		base.render(x, y);
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add((byte)MathF.Floor(gigaAttack.ammo));

		customData.Add(Helpers.boolArrayToByte([
			hypermodeBlink > 0,
			isAwakened,
			isGenmuZero,
			isBlack,
			isViral,
		]));
		if (hypermodeBlink > 0) {
			customData.Add(hypermodeBlink);
		}

		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];

		// Per-player data.
		gigaAttack.ammo = data[0];
		bool[] flags = Helpers.byteToBoolArray(data[1]);
		awakenedPhase = (flags[2] ? 2 : (flags[1] ? 1 : 0));
		isBlack = flags[3];
		isViral = flags[4];

		if (flags[0]) {
			hypermodeBlink = data[2];
		}
	}

	public override void aiAttack(Actor? target) {
		bool isTargetInAir = pos.y > target?.pos.y - 20;
		bool isTargetClose = pos.x < target?.pos.x - 10;
		bool isFacingTarget = (pos.x < target?.pos.x && xDir == 1) || (pos.x >= target?.pos.x && xDir == -1);
		if (player.currency >= Player.zeroHyperCost && !isInvulnerable() &&
		   charState is not (HyperZeroStart or LadderClimb) && !hypermodeActive() && !player.isMainPlayer
		) {
			changeState(new HyperZeroStart(), true);
		}
		if (health > 4) {
			isWildDance = false;
		}
		ComboAttacks();
		WildDance(target);
		if (charState.attackCtrl && !player.isDead && sprite.name != null && 
			!isWildDance && !isInvulnerable() && aiAttackCooldown <= 0 && isFacingTarget) {
			int ZSattack = Helpers.randomRange(0, 11);
			if (!(sprite.name == "zero_attack" || sprite.name == "zero_attack3" || sprite.name == "zero_attack2")) {
				switch (ZSattack) {
					//Randomizador
					case 0 when grounded:
						changeState(new ZeroSlash1State(), true);
						break;
					case 1 when grounded:
						changeState(new ZeroUppercut(uppercutA.type, isUnderwater()), true);
						break;
					case 2 when grounded:
						changeState(new ZeroUppercut(uppercutS.type, isUnderwater()), true);
						break;
					case 3 when grounded && canCrouch():
						changeState(new ZeroCrouchSlashState(), true);
						break;
					case 4 when charState is Dash:
						changeState(new ZeroShippuugaState(), true);
						slideVel = xDir * getDashSpeed() * 2f;
						break;
					case 5 when grounded:
						if (gigaAttack.shootCooldown <= 0 && gigaAttack.ammo >= gigaAttack.getAmmoUsage(0)) {
							if (gigaAttack is RekkohaWeapon) {
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rekkoha(gigaAttack), true);
							} else {
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rakuhouha(gigaAttack), true);
							}
						}
						break;
					case 6 when charState is Fall or Jump:
						changeState(new ZeroRollingSlashtate(), true);
						break;
					case 7 when charState is Fall or Jump:
						changeState(new ZeroAirSlashState(), true);
						break;
					case 8 when charState is Fall:
						changeState(new ZeroDownthrust(downThrustA.type), true);
						break;
					case 9 when charState is Fall:
						changeState(new ZeroDownthrust(downThrustS.type), true);
						break;
					case 10 when charState is Dash:
						changeState(new ZeroDashSlashState(), true);
						slideVel = xDir * getDashSpeed() * 2f;
						break;
					case 11 when grounded:
						groundSpecial.attack(this);
						break;
				}
			}
			if (hypermodeActive() && !player.isMainPlayer) {
				switch (Helpers.randomRange(0, 54)) {
					case 0 when !isViral:
						changeState(new Rakuhouha(gigaAttack), true);
						break;
					case 1 when isAwakened:
						changeState(new GenmureiState(), true);
						break;
					case 2 when isAwakened:
						changeState(new AwakenedZeroHadangeki(), true);
						break;
				}
			}
			aiAttackCooldown = 18;
		}
		base.aiAttack(target);
	}

	public override void aiDodge(Actor? target) {
		Helpers.decrementFrames(ref aiBlocktime);
		foreach (GameObject gameObject in getCloseActors(64, true, false, false)) {
			if (gameObject is Projectile proj&& proj.damager.owner.alliance != player.alliance && charState.attackCtrl) {
				//Projectile is not 
				if (!(proj.projId == (int)ProjIds.RollingShieldCharged || proj.projId == (int)ProjIds.RollingShield
					|| proj.projId == (int)ProjIds.MagnetMine || proj.projId == (int)ProjIds.FrostShield || proj.projId == (int)ProjIds.FrostShieldCharged
					|| proj.projId == (int)ProjIds.FrostShieldAir || proj.projId == (int)ProjIds.FrostShieldChargedPlatform || proj.projId == (int)ProjIds.FrostShieldPlatform)
				) {
					if (gigaAttack.shootCooldown <= 0 && grounded) {
						switch (gigaAttack) {
							case RekkohaWeapon when gigaAttack.ammo >= 28:
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rekkoha(gigaAttack), true);
								break;
							case CFlasher when gigaAttack.ammo >= 7:
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rakuhouha(new CFlasher()), true);
								break;
							case RakuhouhaWeapon when gigaAttack.ammo >= 14:
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rakuhouha(new RakuhouhaWeapon()), true);
								break;
							case DarkHoldWeapon when gigaAttack.ammo >= 14 && isViral:
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rakuhouha(new DarkHoldWeapon()), true);
								break;
							case ShinMessenkou when gigaAttack.ammo >= 14 && isAwakened:
								gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
								changeState(new Rakuhouha(new ShinMessenkou()), true);
								break;
						}
					} else if (!(proj.projId == (int)ProjIds.SwordBlock) && grounded
					&& aiBlocktime <= 0) {
						turnToInput(player.input, player);
						changeState(new SwordBlock(), true);
						aiBlocktime = 40;
					}
				}
			}
		}
		base.aiDodge(target);
	}
	public void ComboAttacks() {
		if (!(charState is HyperZeroStart or DarkHoldState or Hurt) &&
			sprite.name != null && !player.isMainPlayer && !isWildDance
		) { //least insane else if chain be like:		
			if (sprite.name == "zero_attack3") { 
				switch (Helpers.randomRange(1, 2)) {
					case 1 when sprite.frameIndex >= 10:
						switch (Helpers.randomRange(1, 5)) {
							case 1:
								groundSpecial.attack(this);
								break;
							case 2:
								changeState(new ZeroCrouchSlashState(), true);
								break;
							case 3:
								if (gigaAttack.shootCooldown <= 0 && gigaAttack.ammo >= gigaAttack.getAmmoUsage(0)) {
									if (gigaAttack is RekkohaWeapon) {
										gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
										changeState(new Rekkoha(gigaAttack), true);
									} else {
										gigaAttack.addAmmo(-gigaAttack.getAmmoUsage(0), player);
										changeState(new Rakuhouha(gigaAttack), true);
									}
								}
								break;
							case 4:
								changeState(new ZeroShippuugaState(), true);
								slideVel = xDir * getDashSpeed() * 2f;
								break;
							case 5:
								changeState(new ZeroDashSlashState(), true);
								slideVel = xDir * getDashSpeed() * 2f;
								break;
						}
						break;
					case 2 when sprite.frameIndex >= 7:
						switch (Helpers.randomRange(1, 3)) {
							case 1:
								changeState(new ZeroUppercut(RisingType.Denjin, true), true);
								break;
							case 2 when !isUnderwater():
								changeState(new ZeroUppercut(RisingType.Ryuenjin, false), true);
								break;
							case 3:
								changeState(new ZeroUppercut(RisingType.RisingFang, true), true);
								break;
						}
						break;
				}
			}
			if (sprite.name == "zero_ryuenjin" && sprite.frameIndex >= 9 ||
				sprite.name == "zero_eblade" && sprite.frameIndex >= 11 ||
				sprite.name == "zero_rising" && sprite.frameIndex >= 5) {
				switch (Helpers.randomRange(1, 5)) {
					case 1:
						changeState(new ZeroDownthrust(ZeroDownthrustType.Hyouretsuzan), true);
						break;
					case 2:
						changeState(new ZeroDownthrust(ZeroDownthrustType.Rakukojin), true);
						break;
					case 3:
						changeState(new ZeroDownthrust(ZeroDownthrustType.QuakeBlazer), true);
						break;
					case 4:
						changeState(new ZeroRollingSlashtate(), true);
						break;
					case 5:
						changeState(new ZeroAirSlashState(), true);
						break;
				}
			}
			if (sprite.name == "zero_raijingeki" && sprite.frameIndex >= 26 ||
				sprite.name == "zero_tbreaker" && sprite.frameIndex >= 9 ||
				sprite.name == "zero_spear" && sprite.frameIndex >= 12) {
				switch (Helpers.randomRange(1, 3)) {
					case 1:
						changeState(new ZeroDashSlashState(), true);
						slideVel = xDir * getDashSpeed() * 2f;
						break;
					case 2:
						changeState(new ZeroShippuugaState(), true);
						slideVel = xDir * getDashSpeed() * 2f;
						break;
					case 3:
						changeState(new FSplasherState(), true);
						break;
				}
			}
			if (charState is Rakuhouha && sprite.frameIndex >= 16 ||
				charState is Rekkoha && sprite.frameIndex >= 14) {
				switch (Helpers.randomRange(1, 3)) {
					case 1:
						changeState(new ZeroDashSlashState(), true);
						slideVel = xDir * getDashSpeed() * 2f;
						break;
					case 2:
						changeState(new ZeroShippuugaState(), true);
						slideVel = xDir * getDashSpeed() * 2f;
						break;
					case 3:
						changeState(new FSplasherState(), true);
						break;
				}
			}
			if (sprite.name == "zero_attack_dash2" && sprite.frameIndex >= 7) {
				switch (Helpers.randomRange(1, 3)) {
					case 1:
						changeState(new ZeroSlash1State(), true);
						break;
					case 2:
						switch (Helpers.randomRange(1, 3)) {
							case 1:
								changeState(new ZeroUppercut(RisingType.Denjin, true), true);
								break;
							case 2 when !isUnderwater():
								changeState(new ZeroUppercut(RisingType.Ryuenjin, false), true);
								break;
							case 3:
								changeState(new ZeroUppercut(RisingType.RisingFang, true), true);
								break;
						}
						break;
					case 3:
						changeState(new ZeroCrouchSlashState(), true);
						break;
				}
			}
		}
	}
	public void WildDance(Actor? target) {
			if (health <= 4 && target != null && !player.isMainPlayer) {
				if (isFacing(target) && sprite.name != null && grounded) {
					WildDanceMove();
					player.clearAiInput();
					isWildDance = true;
				}
			if (health > 4) {
				isWildDance = false;
			}
		}
	}
	public void WildDanceMove() {
		if (charState.attackCtrl && !isInvulnerableAttack() && charState.attackCtrl) {
			changeState(new ZeroShippuugaState(), true);
			slideVel = xDir * getDashSpeed() * 2f;
		}
		if (!charState.attackCtrl) {
			if (sprite.name == "zero_attack_dash2" && sprite.frameIndex >= 7) {
				changeState(new ZeroSlash1State(), true);
				stopMoving();
			}
			if (sprite.name == "zero_attack3" && sprite.frameIndex >= 6) {
				changeState(new ZeroDashSlashState(), true);
				slideVel = xDir * getDashSpeed() * 2f;
			}
			if (sprite.name == "zero_attack_dash" && sprite.frameIndex >= 3) {
				playSound("gigaCrushAmmoFull");
				switch (Helpers.randomRange(1, 3)) {
					case 1:
						changeState(new ZeroUppercut(RisingType.Denjin, true), true);
						break;
					case 2 when !isUnderwater():
						changeState(new ZeroUppercut(RisingType.Ryuenjin, false), true);
						break;
					case 3:
						changeState(new ZeroUppercut(RisingType.RisingFang, true), true);
						break;
				}
			}
		}
	}

}
