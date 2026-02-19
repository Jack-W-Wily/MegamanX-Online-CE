using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MMXOnline;

public class ZeroEND : Zero {

	public const int BlackZeroCost = 10;
	// Hypermode stuff.
	public bool isViral;
	public int awakenedPhase;
	public bool isAwakened => (awakenedPhase != 0);
	public bool isGenmuZero => (awakenedPhase >= 2);
	public bool isBlack;
	public int hyperMode;

	// Hypermode timers.
	public static readonly float maxBlackZeroTime = 20 * 60;
	public float hyperModeTimer;
	public float scrapDrainCounter = 120;
	public bool hyperOvertimeActive;

	// Hypermode effects stuff.
	public int awakenedAuraFrame;
	public float awakenedAuraAnimTime;
	public byte hypermodeBlink;

	// Weapons.
	public ZSaber meleeWeapon = new();
	public PZeroParryWeapon parryWeapon = new();
	public AwakenedAura awakenedAuraWeapon = new();
	public ZSaberProjSwing saberSwingWeapon = new();
	public ZeroBuster busterWeapon = new();

	// Loadout weapons.
	public Weapon groundSpecial = new RaijingekiWeapon();
	public Weapon airSpecial = new KuuenzanWeapon();
	public Weapon uppercutA  = new RisingFangWeapon();
	public Weapon uppercutS  = new RyuenjinWeapon();
	public Weapon downThrustA  = new RakukojinWeapon();
	public Weapon downThrustS = new HyouretsuzanWeapon();
	public Weapon gigaAttack = new RekkohaWeapon();
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
	public ZeroEND(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, ZeroLoadout? loadout = null,
		int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, player.loadout.zeroLoadout ,heartTanks, isATrans
	) {
		charId = CharIds.ZeroEND;
		// Loadout stuff.
		isBlack = player.blackZarzo;
		groundSpecial = new RaijingekiWeapon();
		airSpecial = KuuenzanWeapon.getWeaponFromIndex(0);
		uppercutA = RyuenjinWeapon.getWeaponFromIndex(2);
		uppercutS = RyuenjinWeapon.getWeaponFromIndex(0);
		downThrustA = HyouretsuzanWeapon.getWeaponFromIndex(1);
		spriteFrameToSounds["zarzo_run/4"] = "zerowalkx4";
		spriteFrameToSounds["zarzo_run/9"] = "zerowalkx4";
		downThrustS = HyouretsuzanWeapon.getWeaponFromIndex(0);

		gigaAttackSelected =0;
		gigaAttack = new RekkohaWeapon();
		
		hyperMode = 0;
		
		altCtrlsLength = 2;
		altSoundId = AltSoundIds.X3;
	}

	// State overdrive.
	public override CharState getAirJumpState() => new Jump() { sprite = "kuuenbu" };

	public override void preUpdate() {
		base.preUpdate();
		if (grounded && charState is not ZeroUppercut) {
			airRisingUses = 0;
		}
	}
	
	public int Hypermode() {
		if (player.input.isHeld(Control.Up, player)) {
			return 1;
		}
		return 0;
	}



	public override int baselineMaxHealth() {
		return 24;
	}


	public override void update() {
		hyperMode = 0;
		// Hypermode effects.
		if (isAwakened) {
			updateAwakenedAura();
		}
		
		
		if (player.blackZarzo) {
			hyperProgress = 0;
		}

	


	
	
		
		if (!ownedByLocalPlayer) {
			base.update();
			return;
		}
		player.superAmmo = gigaAttack.ammo;

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
		if (sprite.name == "bzero_attack_dash") charState.invincible = true;
		
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
						1 => new RekkohaWeapon(),
						2 => new RekkohaWeapon(),
						_ => new RekkohaWeapon(),
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
				scrapDrainCounter = 120;
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
				if (sprite.name == getSprite(charState.shootSpriteEx)) {
					changeSpriteFromName(charState.defaultSprite, false);
					if (charState is WallSlide) {
						frameIndex = sprite.totalFrameNum - 1;
					}
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
		if (isAwakened) {
			chargeLogic(shootDonuts);
		} else if (isBlack) {
			chargeLogic(shootb);
		} else {
			
			chargeLogic(shoot);
		}
	}

	// Flags.
	public bool hypermodeActive() {
		return isBlack || isAwakened || isViral;
	}

	// Shoot logic and stuff.
	public override bool canShoot() {
		return (!charState.invincible && !isInvulnerable() &&
			(charState.attackCtrl || (charState.altCtrls.Length >= 2 && charState.altCtrls[1]))
		);
	}

	public override int getMaxChargeLevel() {
		return isBlack ? 4 : 3;
	}
	
	public override bool canCharge() {
		return (!isInvulnerable
			(charState.attackCtrl || getChargeLevel() > 0) &&
			(player.superAmmo > 0 || freeBusterShots > 0) &&
			donutsPending == 0
		);
	}

	public override bool chargeButtonHeld() {
		return player.input.isAHeld(player) || player.input.isBHeld(player) || player.input.isR2Held(player);
	}

	
	public int stockedBusterLv;
	public bool stockedSaber;
	public float stockedTime;

	
	public override void shoot(int chargeLevel) {
		
			
			string shootSprite = getSprite(charState.shootSpriteEx);



			if (!Global.sprites.ContainsKey(shootSprite)) {
				if (grounded) { shootSprite = "zero_shoot"; } else { shootSprite = "zero_fall_shoot"; }
			}
			if (shootAnimTime == 0) {
				changeSprite(shootSprite, false);
			} else if (charState is Idle && !charState.inTransition()) {
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
			Point shootPos = getShootPos();
			int xDir = getShootXDir();
			if (gigaAttack.ammo > 5){
			if (chargeLevel == 0 ) {
				playSound("busterX3", sendRpc: true);
				var lemon = new DZBusterProj(
					shootPos, xDir, this, player, player.getNextActorNetId(), rpc: true
				);

			} else if (chargeLevel == 1) {
				playSound("buster2X3", sendRpc: true);
				new DZBuster2Proj(
					shootPos, xDir, this, player, player.getNextActorNetId(), rpc: true
				);

			} else if (chargeLevel == 2) {
				playSound("buster3X3", sendRpc: true);
				new DZBuster3Proj(
					shootPos, xDir, this, player, player.getNextActorNetId(), rpc: true
				);

			} else if (chargeLevel == 3) {
				if (charState is WallSlide) {
					shoot(2);
					stockedBusterLv = 1;
					
					return;
				} else {
					shootAnimTime = 0;
					changeState(new ZeroDoubleBuster(false, false), true);
				}
			} else if (chargeLevel >= 4) {
				if (charState is WallSlide) {
					shoot(2);
					stockedBusterLv = 2;
					stockedSaber = true;

					return;
				} else {
					shootAnimTime = 0;
					changeState(new ZeroDoubleBuster(false, false), true);
				}
			}
			if (chargeLevel >= 1) {
				stopCharge();
				gigaAttack.ammo -=6;
			}
			}
		
	}

	public override void shootb(int chargeLevel) {
		
			
			string shootSprite = getSprite(charState.shootSpriteEx);
			if (!Global.sprites.ContainsKey(shootSprite)) {
				if (grounded) { shootSprite = "zero_shoot"; } else { shootSprite = "zero_fall_shoot"; }
			}
			if (shootAnimTime == 0) {
				changeSprite(shootSprite, false);
			} else if (charState is Idle && !charState.inTransition()) {
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
			Point shootPos = getShootPos();
			int xDir = getShootXDir();

			if (chargeLevel == 0) {
				playSound("busterX3", sendRpc: true);
				var lemon = new DZBusterProj(
					shootPos, xDir, this, player, player.getNextActorNetId(), rpc: true
				);

			} else if (chargeLevel == 1) {
				playSound("buster2X3", sendRpc: true);
				new DZBuster2Proj(
					shootPos, xDir, this, player, player.getNextActorNetId(), rpc: true
				);

			} else if (chargeLevel == 2) {
				playSound("buster3X3", sendRpc: true);
				new DZBuster3Proj(
					shootPos, xDir, this, player, player.getNextActorNetId(), rpc: true
				);

			} else if (chargeLevel == 3) {
				if (charState is WallSlide) {
					shoot(2);
					stockedBusterLv = 1;
					
					return;
				} else {
					shootAnimTime = 0;
					changeState(new ZeroDoubleBuster(false, false), true);
				}
			} else if (chargeLevel >= 4) {
				if (charState is WallSlide) {
					shoot(2);
					stockedBusterLv = 2;
					stockedSaber = true;

					return;
				} else {
					shootAnimTime = 0;
					changeState(new ZeroDoubleBuster(false, false), true);
				}
			}
			if (chargeLevel >= 1) {
				stopCharge();
				gigaAttack.ammo -=6;
			}
		
	}

	public void setShootAnim() {
		string shootSprite = getSprite(charState.shootSpriteEx);
		if (!Global.sprites.ContainsKey(shootSprite)) {
			if (grounded) { shootSprite = "zero_shoot"; }
			else { shootSprite = "zero_fall_shoot"; }
		}
		if (shootAnimTime == 0) {
			changeSprite(shootSprite, false);
		} else if (charState is Idle && !charState.inTransition()) {
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
		currencyUse = 1;
		if (currencyUse > 0) {
			if (freeBusterShots > 0) {
				freeBusterShots--;
			} else if (player.currency > 0) {
				player.currency--;
			}
		}
	}

	public void shootDonutProj(int time) {
		setShootAnim();
		Point shootPos = getShootPos();
		int xDir = getShootXDir();

		new ShingetsurinProj(
			shootPos, xDir,
			time / 60f, this, player, player.getNextActorNetId(), rpc: true
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
		if (player.input.isAPressed(player)) {
			shootPressTime = 6;
		}
		if (player.input.isBPressed(player)) {
			specialPressTime = 6;
		}
		if (player.input.isPressed(Control.WeaponRight, player) && isAwakened) {
			swingPressTime = 6;
		}
	}

	// Non-attacks like guard and hypermode activation.
	public override bool normalCtrl() {
		// Hypermode activation.
		int cost = 5;
		if (isAwakened) {
			cost = 4;
		}

		if (player.input.isHeld(Control.Special2, player) && player.superAmmo < player.superMaxAmmo ){
		changeState(new AwakenedTaunt(),true);
		}


			if (player.input.isPressed(Control.WeaponLeft, player)
			&& player.currency > 4
			) {
				player.currency -= 5;
				changeState(new OverDriveStart(), true);
				/* 
				changeState() you'll be using this for every custom action your character does
				*/
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
		else if (hyperProgress >= 1 && player.currency >= 5) {
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
		// Guard!
		if (
				player.input.isL2Held(player)
			)
		{

			changeState(new BlockWCUT(), true);
			return true;
		} 


		for (int i = 1; i <= 4; i++ ) {
				CollideData? collideData = Global.level.checkTerrainCollisionOnce(this, 0, -12 * i, autoVel: true);
			if (collideData != null && collideData.gameObject is Wall wall
				&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()
			) {
				if (player.input.isHeld(Control.Up, player)) {
					changeState(new HyorogaStartState(), true);
				}
			}
		}


		return false;
	}

	public override bool attackCtrl() {
		// To prevent XDiego skillcheck we check if we are shooting donuts.
		// If we are doing so we do not attack.
		if (donutsPending != 0) {
			return false;
		}

		if (player.input.isL2Held(player) && player.input.isAPressed(player)) {
			changeState(new ZeroGrabStart(), forceChange: true);
		}
		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);	
		}
		if (grounded && player.superAmmo == player.superMaxAmmo &&
		downPressedTimes >= 2 && player.input.isR2Held(player)) {
			if (isAwakened) {
				changeState(new GenmureiState(), true);
			} else if (isBlack) {
				playSound("dynamoting", forcePlay: false, sendRpc: true);
			changeState(new Idle());
			playSound("dynamoUltraCross1", forcePlay: false, sendRpc: true);

			}
			else {
				changeState(new DarkHoldShootState(new DarkHoldWeapon()), true);
			}
			downPressedTimes = 0;
			gigaAttack.ammo -= 32;
			return true;
		}



		if (isAwakened && swingPressTime > 0 && hadangekiCooldown == 0) {
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
		if (!player.input.isL2Held(player)) {
			if (grounded && vel.y >= 0) {
				return groundAttacks();
			} else {
				return airAttacks();
			}

		}
		return base.attackCtrl();
	}



	public override bool spcCancel() {
		// Uppercuts.
		if (player.input.isHeld(Control.Up, player) && charState is not ZeroUppercut) {
			if (player.input.isR2Pressed(player)) {

				changeState(new ZeroUppercut(RisingType.Denjin, true), true);
			}
			if (player.input.isBPressed(player)) {
				changeState(new ZeroUppercut(RisingType.Ryuenjin, false), true);
			}
			if (player.input.isAPressed(player)) {
				changeState(new ZeroUppercut(RisingType.RisingFang, true), true);
			}
			return true;
		}
		// Dash Cancel
		else if (player.dashPressed(out string dashControl)) {
			if (grounded) {
				changeState(new Dash(dashControl), true);
			} else {
				changeState(new AirDash(dashControl), true);
			}
				return true;
		}



		return base.spcCancel();
	}

	public override bool groundAttacks() {
		int yDir = player.input.getYDir(player);
		// Giga attacks.
		if (yDir == 1 && specialPressed && downPressedTimes >= 2) {
			if (gigaAttack.shootCooldown <= 0 && gigaAttack.ammo >= gigaAttack.getAmmoUsage(0)) {
				gigaAttack.shoot(this, []);
				return true;
			}
			if (!shootPressed) {
				return true;
			}
		}
		if (yDir == 1 && specialPressed && downPressedTimes <= 2) {
			if (yDir == 1 && specialPressed && gigaAttack.ammo >= 16) {
				
					changeState(new ShinMessenkouState(new ShinMessenkou()), true);
		
				
				gigaAttack.ammo -= 16;
			}

		}
		// Uppercuts.
		if (yDir == -1 && charState is not ZeroUppercut) {
			if (player.input.isR2Pressed(player)) {

				changeState(new ZeroUppercut(RisingType.Denjin, true), true);
			}
			if (player.input.isBPressed(player)) {
				changeState(new ZeroUppercut(RisingType.Ryuenjin, false), true);
			}
			if (player.input.isAPressed(player)) {
				changeState(new ZeroUppercut(RisingType.RisingFang, true), true);
			}
			return true;
		}
		// Dash attacks.
		if (isDashing && (shootPressed || specialPressed)) {
			// Do nothing if we dashed already.
			if (dashAttackCooldown > 0) {
				return false;
			}
			dashAttackCooldown = 60;
		
			if (specialPressTime > shootPressTime) {
					slideVel = xDir * getDashSpeed();
				changeState(new ZeroShippuugaState(), true);
				return true;
			}
			if (player.superAmmo > 10){
			changeState(new ZeroDashSlashState(), true);
			gigaAttack.ammo -= 10;
			} else {
        	changeState(new ZeroAirSlashState(), true && stockedBusterLv == 0);
            }
			if (specialPressTime == 0){
		
			slideVel = xDir * getDashSpeed() * 2;
			}
			return true;
		}
		// Use special if pressed first.
		if (specialPressed && specialPressTime > shootPressTime) {
			if (!OverDrive) {
				groundSpecial.attack(this);
					Global.level.delayedActions.Add(new DelayedAction(() => {

					playSound("buster3X3", sendRpc: true);
						new ZBuster3Proj2(
							getShootPos(), xDir, this, player, player.getNextActorNetId(), rpc: true
						);
				}, 0.25f));

			} else {
				groundSpecial.attack2(this);
			}
		}
		// Regular slashes.
		if (shootPressed) {
			// Crounch variant.

			if (!isCharging()) {
				if (shootPressed) {
					if (yDir == 1  && stockedBusterLv == 0) {
					if (charState is not ZeroCrouchSlashState) {
						changeState(new ZeroCrouchSlashState(), true);
					}
					return true;
				}
				if (charState is not ZeroSlash1State or ZeroSlash2State or ZeroSlash3State) {
					changeState(new ZeroSlash1State(), true && stockedBusterLv == 0);
				}
					if (stockedBusterLv >= 1) {
						if (charState is WallSlide) {
							int chargeLevel = stockedBusterLv;
							if (stockedBusterLv >= 3) {
								stockedBusterLv -= 2;
								chargeLevel = stockedBusterLv;
							} else {
								stockedBusterLv = 0;
							}
							shoot(chargeLevel);

							return true;
						}
						changeState(new ZeroDoubleBuster(true, true), true);
						stockedBusterLv = 0;
						return true;
					}
					if (stockedSaber) {
						if (charState is WallSlide wsState) {
							changeState(new AwakenedZeroHadangekiWall(wsState.wallDir, wsState.wallCollider), true);
							return true;
						}
						changeState(new AwakenedZeroHadangeki(), true);
						return true;
					}

				}
			} else {
				if (yDir == 1) {
					if (charState is not ZeroCrouchSlashState) {
						changeState(new ZeroCrouchSlashState(), true);
					}
					return true;
				}
				if (charState is not ZeroSlash1State or ZeroSlash2State or ZeroSlash3State) {
					changeState(new ZeroSlash1State(), true);
				}
				return true;
			}
		}
			return false;

	}


	public bool airAttacks() {
		int yDir = player.input.getYDir(player); 
		if (yDir == -1 && airRisingUses == 0 && flag == null && (
			(uppercutA.type == (int)RisingType.RisingFang && shootPressed) ||
			(uppercutS.type == (int)RisingType.RisingFang && specialPressed)
		)) {
			changeState(new ZeroUppercut(RisingType.RisingFang, isUnderwater()), true);
			dashedInAir++;
			airRisingUses++;
			return true;
		}
		if (yDir == 1 && (shootPressed || specialPressed)) {
			// Weapon type to use.
			int weaponType = downThrustA.type;
			// If special was pressed first.
			if (specialPressTime > shootPressTime) {
				weaponType = downThrustS.type;
			}
			changeState(new ZeroDownthrust(weaponType), true);
			return true;
		}
		// Air attack.
		if (specialPressed) {
			if (airSpecial.type == 0 && charState is not ZeroRollingSlashtate) {
				if (Options.main.swapAirAttacks == false) {
					changeState(new ZeroRollingSlashtate(), true);
				} else {
					changeState(new ZeroAirSlashState(), true);
				}
			}
			if (airSpecial.type != 0) {
				airSpecial.attack(this);
			}
			return true;
		}
		// Air attack.
		if (shootPressed) {
			if (charState is WallSlide wallSlide) {
				changeState(new ZeroMeleeWall(wallSlide.wallDir, wallSlide.wallCollider), true);
			} else {
				if (Options.main.swapAirAttacks == false) {
					changeState(new ZeroAirSlashState(), true);
				} else {
					changeState(new ZeroRollingSlashtate(), true);
				}
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
		if (isBlack) {
			runSpeed *= 1.15f;
		}
		return runSpeed * getRunDebuffs();
	}

	public override float getDashSpeed() {
		if (flag != null || !isDashing) {
			return getRunSpeed();
		}
		float dashSpeed = 210;
		if (isBlack) {
			dashSpeed *= 1.15f;
		}
		return dashSpeed * getRunDebuffs();
	}


	public override string getSprite(string spriteName) {
		if (Global.sprites.ContainsKey("bzero_" + spriteName)) {
			return "bzero_" + spriteName;
		}
		return "zarzo_" + spriteName;
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
		return  isGenmuZero;
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
		proj.ownerActor = this;

		// Damage based on tripleSlash time.
		if (meleeId == (int)MeleeIds.HuhSlash) {
			float timeSinceStart = zeroTripleSlashEndTime - zeroTripleStartTime;
			float overrideDamage = 1;
			int overrideFlinch = Global.superFlinch;
			if (timeSinceStart < 0.5f) {
				overrideDamage = 1;
			}
			proj.damager.damage = overrideDamage;
			proj.damager.flinch = overrideFlinch;
		}
		// Damage based on fall speed.
		else if (meleeId == (int)MeleeIds.Rakukojin) {
			float damage = 1 + Helpers.clamp(MathF.Floor(deltaPos.y * 0.8f), 0, 10);
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
		DrillCrush,
		// Others
		LadderSlash,
		WallSlash,
		Gokumonken,
		Hadangeki,

		Grab,
		GrabEX,
		GrabEnd,
		AwakenedAura
	}

	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"zarzo_grab_start" => MeleeIds.Grab,
			"zarzo_grab_ex" => MeleeIds.GrabEX,
			"zarzo_grab_ex_end" => MeleeIds.GrabEnd,
			// Ground
			"bzero_attack" => MeleeIds.HuSlash,
			"bzero_attack2" => MeleeIds.HaSlash,
			"bzero_attack3" => MeleeIds.HuhSlash,
			"bzero_attack_crouch" => MeleeIds.CrouchSlash,
			// Dash
			"bzero_hyouretsuzan_land" or "bzero_hyouretsuzan_fall" => MeleeIds.DashSlash,
			"zarzo_attack_dash2" => MeleeIds.Shippuuga,
			// Air
			"bzero_attack_air" or "bzero_attack_air_ground" => MeleeIds.AirSlash,
			"zarzo_attack_air2" or "bzero_attack_air2" or "zarzo_cmoon" => MeleeIds.RollingSlash,
			"zarzo_hyoroga_attack"  => MeleeIds.Hyoroga,
			// Ground Speiclas
			"bzero_raijingeki2" => MeleeIds.Ryuenjin,
			"bzero_attack_dash" or "bzero_raijingeki" => MeleeIds.RaijingekiWeak,
			"zarzo_nuclear" => MeleeIds.Dairettsui,
			"zarzo_spear" => MeleeIds.Suiretsusen,
			// Up Specials
		//	"zarzo_ryuenjin" => MeleeIds.Ryuenjin,
			"zarzo_eblade" => MeleeIds.Denjin,
			"zarzo_rising" => MeleeIds.RisingFang,
			// Down specials
			"bzero_ryuenjin"=> MeleeIds.Hyouretsuzan,
			"zarzo_quakeblazer_start" or "zarzo_quakeblazer_fall" => MeleeIds.Danchien,
			"bzero_rakukojin_start" or "bzero_rakukojin_fall" => MeleeIds.Rakukojin,
			// Others.
			"zarzo_ladder_attack" => MeleeIds.LadderSlash,
			"zarzo_wall_slide_attack" => MeleeIds.WallSlash,
			"zarzo_block" => MeleeIds.Gokumonken,
			"zarzo_projswing" => MeleeIds.Hadangeki,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		return id switch {

			(int)MeleeIds.Grab => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ForceGrabState, player, 0, 0, 40, isReflectShield: true,
				isZSaberEffect2: false, isZSaberClang: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.GrabEX => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ForceGrabState, player, 1, 0, 5, isReflectShield: true,
				isZSaberEffect2: false, isZSaberClang: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.GrabEnd => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.BurensenEND, player, 2, 0, 15, isReflectShield: true,
				isZSaberEffect2: false, isZSaberClang: false,
				addToLevel: addToLevel
			),
			// Ground
			(int)MeleeIds.HuSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaber1, player, 0.5f, Global.miniFlinch, 15, isReflectShield: true,
				isZSaberEffect2: true, isZSaberClang: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.HaSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaber2, player, 0.5f, Global.halfFlinch, 15, isReflectShield: true,
				isZSaberEffect2B: true, isZSaberClang: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.HuhSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaber3, player,
				0.5f, Global.defFlinch, 5, isReflectShield: true,
				isZSaberEffect: true, isZSaberClang: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.CrouchSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberCrouch, player, 3, Global.halfFlinch, 15, isReflectShield: true,
				isZSaberEffect: true, isZSaberClang: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			// Dash
			(int)MeleeIds.DashSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberDash, player, 2, Global.halfFlinch, 15, isReflectShield: true,
				isZSaberEffect: true, isZSaberClang: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.Shippuuga => new GenericMeleeProj(
				ShippuugaWeapon.staticWeapon, projPos, ProjIds.Shippuuga, player, 2, Global.defFlinch, 15,
				isZSaberEffect: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			// Air
			(int)MeleeIds.AirSlash => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.ZSaberAir, player, 1, Global.miniFlinch, 5, isReflectShield: true,
				isZSaberEffect: true, isZSaberClang: true,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.RollingSlash =>  new GenericMeleeProj(
				KuuenzanWeapon.staticWeapon, projPos, ProjIds.ZSaberRollingSlash, player,
				1, 10, 5, isDeflectShield: true,
				isZSaberEffect2: true, isZSaberClang: true,
				addToLevel: addToLevel, isJuggleProjectile : true, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.Hyoroga => new GenericMeleeProj(
				HyorogaWeapon.staticWeapon, projPos, ProjIds.HyorogaSwing, player, 4, Global.superFlinch, 15,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			// Ground Specials
			(int)MeleeIds.Raijingeki => new GenericMeleeProj(
				RaijingekiWeapon.staticWeapon, projPos, ProjIds.Raijingeki, player, 0.5f, Global.defFlinch, 4,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.RaijingekiWeak => new GenericMeleeProj(
				Raijingeki2Weapon.staticWeapon, projPos, ProjIds.ElectricShock, player, 0.5f, 0, 4,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.Dairettsui => new GenericMeleeProj(
				TBreakerWeapon.staticWeapon, projPos, ProjIds.TBreaker, player, 3, Global.defFlinch, 10,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.Suiretsusen => new GenericMeleeProj(
				SuiretsusenWeapon.staticWeapon, projPos, ProjIds.SuiretsusanProj, player, 6, Global.defFlinch, 45,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			// Up Specials
			(int)MeleeIds.Ryuenjin => new GenericMeleeProj(
				RyuenjinWeapon.staticWeapon, projPos, ProjIds.Ryuenjin, player, 2, Global.defFlinch, 15,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.Denjin => new GenericMeleeProj(
				DenjinWeapon.staticWeapon, projPos, ProjIds.Denjin, player, 1, 30, 6,
				addToLevel: addToLevel, isJuggleProjectile : true
			),
			(int)MeleeIds.RisingFang => new GenericMeleeProj(
				RisingFangWeapon.staticWeapon, projPos, ProjIds.RisingFang, player, 2, 0, 30,
				isZSaberEffect: true,
				addToLevel: addToLevel, isJuggleProjectile : true, hitSound : "htsnd_slash1"
			),
			// Down specials
			(int)MeleeIds.Hyouretsuzan => new GenericMeleeProj(
				HyouretsuzanWeapon.staticWeapon, projPos, ProjIds.Hyouretsuzan2, player, 4, 12, 30,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Danchien => new GenericMeleeProj(
				DanchienWeapon.staticWeapon, projPos, ProjIds.QuakeBlazer, player, 2, 0, 30,
				addToLevel: addToLevel
			),
			(int)MeleeIds.DrillCrush => new GenericMeleeProj(
				RakukojinWeapon.staticWeapon, projPos, ProjIds.Rakukojin, player, 1, 12, 4,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Rakukojin => new GenericMeleeProj(
				RakukojinWeapon.staticWeapon, projPos, ProjIds.Rakukojin, player, 4, 12, 30,
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
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			(int)MeleeIds.AwakenedAura => new GenericMeleeProj(
				awakenedAuraWeapon, projPos, ProjIds.AwakenedAura, player,
				2, 0, 30,
				addToLevel: addToLevel
			),
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
					float damage = 0;
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
						ownerActor = this
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
				proj.damager.flinch = Global.defFlinch;
			}
		}
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
			palette = player.viralZeroShader;
		}
		if (palette != null && hypermodeBlink > 0) {
			float blinkRate = MathInt.Ceiling(hypermodeBlink / 30f);
			palette = ((Global.frameCount % (blinkRate * 2) >= blinkRate) ? null : palette);
		}

	
	if (Global.isOnFrameCycle(4)) {
			switch (getChargeLevel()) {
				case 1:
					palette = Player.ZeroBlueC;
					break;
				case 2:
					palette = Player.ZeroBlueC;
					break;
				case 3:
					palette = Player.ZeroPinkC;
					break;
				case 4:
					palette = Player.ZeroGreenC;
					break;
			}
			if (stockedSaber || stockedBusterLv == 2) {
				palette = Player.ZeroGreenC;
			}
			if (stockedBusterLv == 1) {
				palette = Player.ZeroPinkC;
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


		// For drawing the growing aura that LastStand and Eigengrau Zero uses.
		if (visible && OverDrive) {
			// Position to draw the sprite to.
			float auraSize = 1 + omegaAura.twitch + omegaAura.grow;
			float drawX = pos.x + x + (float)xDir * currentFrame.offset.x * auraSize;
			float drawY = pos.y + y + (float)yDir * currentFrame.offset.y * auraSize + 1;

			float auraAlpha2 = 0.75f;
			

			// Draw aura.
			Global.sprites[sprite.name].draw(
				sprite.frameIndex,
				drawX, drawY,
				xDir, yDir,
				null, auraAlpha2,
				auraSize,
				auraSize,
				zIndex - 1,
				player.omegaAuraShaderRed
			);
			updateOmegaAura();
		}



		if (isViral && visible) {
			addRenderEffect(RenderEffectType.Trail);
		} else {
			removeRenderEffect(RenderEffectType.Trail);
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
			OverDrive,
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
		OverDrive = flags[5];

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
			if (!(sprite.name == "zarzo_attack" || sprite.name == "zarzo_attack3" || sprite.name == "zarzo_attack2")) {
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
							gigaAttack.shoot(this, []);
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
					case 0 when !isViral && gigaAttack.shootCooldown <= 0:
						gigaAttack.shoot(this, []);
						break;
					case 1 when isAwakened && genmureiCooldown <= 0:
						changeState(new GenmureiState(), true);
						break;
					case 2 when isAwakened && hadangekiCooldown <= 0:
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
						if (gigaAttack.ammo >= gigaAttack.getAmmoUsage(0)) {
							gigaAttack.shoot(this, []);
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
			if (sprite.name == "zarzo_attack3") { 
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
								if (gigaAttack.shootCooldown <= 0 &&
									gigaAttack.ammo >= gigaAttack.getAmmoUsage(0)
								) {
									gigaAttack.shoot(this, []);
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
			if (charState is RakuhouhaState && sprite.frameIndex >= 16 ||
				charState is RekkohaState && sprite.frameIndex >= 14) {
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
			if (sprite.name == "zarzo_attack_dash2" && sprite.frameIndex >= 7) {
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
			if (sprite.name == "zarzo_attack_dash2" && sprite.frameIndex >= 7) {
				changeState(new ZeroSlash1State(), true);
				stopMoving();
			}
			if (sprite.name == "zarzo_attack3" && sprite.frameIndex >= 6) {
				changeState(new ZeroDashSlashState(), true);
				slideVel = xDir * getDashSpeed() * 2f;
			}
			if (sprite.name == "zarzo_attack_dash" && sprite.frameIndex >= 3) {
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
