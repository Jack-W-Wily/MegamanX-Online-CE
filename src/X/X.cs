using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class MegamanX : Character {
	// Shoot variables.
	public float shootCooldown;
	public int lastShootPressed;
	public float xSaberCooldown;
	public XBuster specialBuster;
	public int specialButtonMode;

	// Armor variables.
	public ArmorId bodyArmor;
	public ArmorId armArmor;
	public ArmorId legArmor;
	public ArmorId helmetArmor;
	
	public ArmorId hyperBodyActive;
	public ArmorId hyperArmActive;
	public ArmorId hyperLegActive;
	public ArmorId hyperHelmetActive;

	public ArmorId hyperBodyArmor;
	public ArmorId hyperArmArmor;
	public ArmorId hyperLegArmor;
	public ArmorId hyperHelmetArmor;

	public const int headArmorCost = 1;
	public const int bodyArmorCost = 1;
	public const int armArmorCost = 1;
	public const int bootsArmorCost = 1;

	public float headbuttAirTime = 0;
	public int hyperChargeTarget;
	public bool stockedBuster;
	public bool stockedMaxBuster;
	public float noDamageTime;
	public float rechargeHealthTime;

	// Shoto moves.
	public float hadoukenCooldownTime;
	public float maxHadoukenCooldownTime = 60;
	public float shoryukenCooldownTime;
	public float maxShoryukenCooldownTime = 60;

	// HyperX stuff.
	public bool hasSeraphArmor;
	public bool hasFullHyperMaxArmor => (
		hyperBodyArmor == ArmorId.Max &&
		hyperArmArmor == ArmorId.Max &&
		hyperLegActive == ArmorId.Max &&
		hyperHelmetArmor == ArmorId.Max
	);
	public bool usedChips;

	// Giga-attacks and armor weapons.
	public Weapon? gigaWeapon;
	public HyperNovaStrike? seraphNovaStrike;
	public ItemTracer? itemTracer;
	public float barrierCooldown;
	public float barrierActiveTime;
	public Sprite barrierAnim = new Sprite("barrier_start");
	public bool stockedCharge;
	public bool stockedX3Charge;
	public bool stockedSaber;

	// Weapon-specific.
	public RollingShieldProjCharged? chargedRollingShieldProj;
	public List<BubbleSplashProjCharged> chargedBubbles = new();
	public StrikeChainProj? strikeChainProj;
	public StrikeChainProjCharged? strikeChainChargedProj;
	public GravityWellProjCharged? chargedGravityWell;
	public SpinningBladeProjCharged? chargedSpinningBlade;
	public FrostShieldProjCharged? chargedFrostShield;
	public TornadoFangProjCharged? chargedTornadoFang;
	public GravityWellProj? linkedGravityWell;
	public TriadThunderProj? linkedTriadThunder;
	public BeeSwarm? chargedParasiticBomb;
	public List<MagnetMineProj> magnetMines = new();
	public List<RaySplasherTurret> rayTurrets = new();
	public RaySplasher? shootingRaySplasher = new();

	// Chamaleon Sting.
	public float stingActiveTime;
	public int cStingPaletteIndex;
	public float cStingPaletteTime;
	// Other.
	public float WeaknessCooldown;

	// Creation code.
	bool lastFrameSpecialHeld;
	bool lastShotWasSpecialBuster;
	public float upPunchCooldown;
	public Projectile? unpoAbsorbedProj;

	float hyperChargeAnimTime;
	float hyperChargeAnimTime2 = 0.125f;
	const float maxHyperChargeAnimTime = 0.25f;

	public bool boughtUltimateArmorOnce;
	public bool boughtGoldenArmorOnce;

	public bool stockedCharge;
	public bool stockedXSaber;

	public bool stockedX3Buster;

	public float xSaberCooldown;
	public float stockedChargeFlashTime;

	public BeeSwarm? beeSwarm;

	public float parryCooldown;
	public float maxParryCooldown = 30;

	public bool stingActive;
	public bool UltrastingActive;
	public bool isHyperChargeActive;

	public Sprite hyperChargePartSprite =  new Sprite("hypercharge_part_1");
	public Sprite hyperChargePart2Sprite =  new Sprite("hypercharge_part_1");

	public bool isShootingSpecialBuster;

	public XBuster staticBusterWeapon = new();
	public XBuster specialBuster;
	public float WeaknessT;

	public MegamanX(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.X;
		specialBuster = new XBuster();
	}

	// Updates at the start of the frame.
	public override void preUpdate() {
		base.preUpdate();
		Helpers.decrementFrames(ref barrierActiveTime);

		// Max armor barrier sprite.
		if (barrierActiveTime > 0) {
			barrierAnim.update();
			if (barrierAnim.name == "barrier_start" && barrierAnim.isAnimOver()) {
				if (player.hasChip(1)) {
					barrierAnim = new Sprite("barrier2");
				} else {
					barrierAnim = new Sprite("barrier");
				}
			}
		}

		Helpers.decrementFrames(ref WeaknessCooldown);
		if (!ownedByLocalPlayer) {
			return;
	public bool canShootSpecialBuster() {
		if (isHyperX && (charState is Dash || charState is AirDash)) {
			return false;
		}
		return isSpecialBuster() &&
			player.weapon is not XBuster &&
			!stingActive &&
			player.armorFlag == 0 &&
			streamCooldown == 0;
	}

	public bool canShootSpecialBusterOnBuster() {
		return isSpecialBuster() && !stingActive && player.armorFlag == 0;
	}

	public void refillUnpoBuster() {
		if (player.weapons.Count > 0) player.weapons[0].ammo = player.weapons[0].maxAmmo;
	}

	public override void update() {
		fgMotion = false;
		base.update();

		if (stockedCharge) {
			addRenderEffect(RenderEffectType.ChargePink, 0.033333f, 0.1f);
		}
		if (stockedXSaber) {
			addRenderEffect(RenderEffectType.ChargeGreen, 0.05f, 0.1f);
		}
		if (stockedX3Buster) {
			if (player.weapon is not XBuster) {
				stockedX3Buster = false;
			} else {
				addRenderEffect(RenderEffectType.ChargeOrange, 0.05f, 0.1f);
			}
		}

		stingActive = stingChargeTime > 0;
		if (stingActive) {
			addRenderEffect(RenderEffectType.Invisible);
		} else {
			removeRenderEffect(RenderEffectType.Invisible);
		}
		UltrastingActive = UltraStingChargeTime > 0;
		if (UltrastingActive) {
			addRenderEffect(RenderEffectType.StealthModeBlue);
		} else {
			removeRenderEffect(RenderEffectType.StealthModeBlue);
		}

		if (isHyperX) {
			if (musicSource == null) {
				addMusicSource("introStageBreisX4_JX", getCenterPos(), true);
			} 
		} else destroyMusicSource();
		
		if (cStingPaletteTime > 5) {
			cStingPaletteTime = 0;
			cStingPaletteIndex++;
		}
		cStingPaletteTime++;

		if (headbuttAirTime > 0) {
			headbuttAirTime += Global.spf;
		}
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref barrierCooldown);
		Helpers.decrementFrames(ref xSaberCooldown);

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
	}

	// General update.
	public override void update() {
		base.update();

		if (!ownedByLocalPlayer) {
			return;
		}
		gigaWeapon?.update();
		seraphNovaStrike?.update();
		itemTracer?.update();
		shootingRaySplasher?.burstLogic(this);
	}

	// Late updates. Before render.
	public override void postUpdate() {
		base.postUpdate();

		if (stockedSaber) {
			addRenderEffect(RenderEffectType.ChargeGreen, 2, 6);
		}
		else if (stockedX3Charge) {
			addRenderEffect(RenderEffectType.ChargeOrange, 2, 6);
		}
		else if (stockedBuster) {
			addRenderEffect(RenderEffectType.ChargePink, 2, 6);
		}
	}

	// Shoots stuff.
	public void shoot(int chargeLevel) {
		// Changes to shoot animation.
		setShootAnim();
		shootAnimTime = DefaultShootAnimTime;

		// Calls the weapon shoot function.
		player.weapon.shoot(this, [chargeLevel]);
		// Sets up global shoot cooldown to the weapon shootCooldown.
		shootCooldown = player.weapon.shootCooldown;

		// Stop charge if this was a charge shot.
		if (chargeLevel >= 1) {
			stopCharge();
		}
	}

	// Movement related stuff.
	public override float getDashSpeed() {
		if (flag != null || !isDashing) {
			return getRunSpeed();
		}
		float dashSpeed = 3.5f * 60;
		if (legArmor == ArmorId.Light && charState is Dash) {
			dashSpeed *= 1.15f;
		} else if (legArmor == ArmorId.Giga && charState is AirDash) {
			dashSpeed *= 1.15f;
		}
		return dashSpeed * getRunDebuffs();

		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				player.weapon.ammo = Helpers.clampMax(player.weapon.ammo + 1, player.weapon.maxAmmo);
				if (!player.hasArmArmor(3)) {
					playSound("heal", forcePlay: true);
				} else {
					playSound("healX3", forcePlay: true);
				}
			}
		}

		if (player.hasChip(2) && !isInvisible() && totalChipHealAmount < maxTotalChipHealAmount) {
			noDamageTime += Global.speedMul;
			if ((player.health < player.maxHealth || player.hasSubtankCapacity()) && noDamageTime > 240) {
				Helpers.decrementFrames(ref rechargeHealthTime);
				if (rechargeHealthTime <= 0) {
					rechargeHealthTime = 60;
					addHealth(1);
					totalChipHealAmount++;
				}
			}
		}

		// Fast Hyper Activation.
		quickArmorUpgrade();

		//Fast Chip Activation.
		if (charState is not Die &&
			player.input.isPressed(Control.Special1, player) &&
			player.hasAllX3Armor() && !player.hasGoldenArmor() && !player.hasUltimateArmor()) {
			if (player.input.isHeld(Control.Down, player)) {
				player.setChipNum(0, false);
				Global.level.gameMode.setHUDErrorMessage(
					player, "Equipped foot chip.", playSound: false, resetCooldown: true
				);
			} else if (player.input.isHeld(Control.Up, player)) {
				player.setChipNum(2, false);
				Global.level.gameMode.setHUDErrorMessage(
					player, "Equipped head chip.", playSound: false, resetCooldown: true
				);
			} else if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				player.setChipNum(3, false);
				Global.level.gameMode.setHUDErrorMessage(
					player, "Equipped arm chip.", playSound: false, resetCooldown: true
				);
			} else {
				player.setChipNum(1, false);
				Global.level.gameMode.setHUDErrorMessage(
					player, "Equipped body chip.", playSound: false, resetCooldown: true
				);
			}
		}

		Helpers.decrementTime(ref upPunchCooldown);

			if (charState.attackCtrl && player.input.isPressed(Control.Special1, player)
			&& !player.input.isHeld(Control.Down, player) && !player.input.isHeld(Control.Up, player)) {
				if (unpoShotCount <= 0) {
					upPunchCooldown = 0.5f;
					changeState(new XUPPunchState(grounded), true);
					return;
				}
			} 
			if (player.input.isPressed(Control.Special1, player) && !isInvisible() &&
				  (charState is Dash || charState is AirDash)) {
				charState.isGrabbing = true;
				changeSpriteFromName("unpo_grab_dash", true);
			} 
			if (charState.attackCtrl && parryCooldown == 0 && player.input.isPressed(Control.Special1, player) 
				 && player.input.isHeld(Control.Down, player)) {
				if (unpoAbsorbedProj != null) {
					changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
					unpoAbsorbedProj = null;
					return;
				} else {
					changeState(new XUPParryStartState(), true);
				}
			}
		

		if (charState.attackCtrl &&
			(isSpecialSaber() || isHyperX) && canShoot() &&
			canChangeWeapons() && player.armorFlag == 0 &&
			player.input.isPressed(Control.Special1, player) &&
			!isAttacking() && !isInvisible() &&
			!charState.isGrabbing
		) {
			if (xSaberCooldown == 0) {
				//xSaberCooldown = 1f;
				changeState(new X6SaberState(grounded), true);
				return;
			}
		}

		if (isHyperX) {
			if (charState is not XUPGrabState
				and not XUPParryMeleeState
				and not XUPParryProjState
				and not Hurt
				and not GenericStun
				and not VileMK2Grabbed
				and not GenericGrabbedState
			) {
				unpoTime += Global.speedMul;
				UPDamageCooldown += Global.speedMul;
				if (UPDamageCooldown > unpoDamageMaxCooldown) {
					UPDamageCooldown = 0;
					applyDamage(1, player, this, null, null);
				}
			}

			unpoShotCount = MathInt.Floor(player.weapon.ammo / player.weapon.getAmmoUsage(0));
		}

		//Giga Helmet Scan.
		if (charState.attackCtrl && player.hasHelmetArmor(2) && scannerCooldown <= 0 && canScan()) {
			Point scanPos;
			Point? headPos = getHeadPos();
			if (headPos != null) {
				scanPos = headPos.Value.addxy(xDir * 10, 0);
			} else {
				scanPos = getCenterPos().addxy(0, -10);
			}
			CollideData hit = Global.level.raycast(
				scanPos, scanPos.addxy(getShootXDir() * 150, 0), new List<Type>() { typeof(Actor) }
			);
			if (hit?.gameObject is Character chr &&
				chr.player.alliance != player.alliance &&
				!chr.player.scanned &&
				!chr.isStealthy(player.alliance)
			) {
				new ItemTracer().getProjectile(scanPos, getShootXDir(), player, 0, player.getNextActorNetId());
			} else if (player.input.isPressed(Control.Special1, player)) {
				new ItemTracer().getProjectile(scanPos, getShootXDir(), player, 0, player.getNextActorNetId());
			}
		}

		if (!isHyperX) {
			player.changeWeaponControls();
		}

		if (string.IsNullOrEmpty(charState.shootSprite)) {
			shotgunIceChargeTime = 0;
		}

		if (shotgunIceChargeTime > 0 && ownedByLocalPlayer) {
			changeSprite("mmx_" + charState.shootSprite, true);
			shotgunIceChargeTime -= Global.spf;
			var busterPos = getShootPos().addxy(xDir * 10, 0);
			if (shotgunIceChargeCooldown == 0) {
				new ShotgunIceProjCharged(
					player.weapon, busterPos, xDir,
					player, shotgunIceChargeMod % 2, false,
					player.getNextActorNetId(), rpc: true
				);
				shotgunIceChargeMod++;
			}
			shotgunIceChargeCooldown += Global.spf;
			if (shotgunIceChargeCooldown > 0.1) {
				shotgunIceChargeCooldown = 0;
			}
			if (shotgunIceChargeTime < 0) {
				shotgunIceChargeTime = 0;
				shotgunIceChargeCooldown = 0;
				changeSprite("mmx_" + charState.defaultSprite, true);
			}
		}

		if (isShootingRaySplasher && ownedByLocalPlayer) {
			changeSprite("mmx_" + charState.shootSprite, true);

			if (raySplasherCooldown > 0) {
				raySplasherCooldown += Global.spf;
				if (raySplasherCooldown >= 0.03f) {
					raySplasherCooldown = 0;
				}
			} else {
				var busterPos = getShootPos();
				if (raySplasherCooldown2 == 0) {
					player.weapon.addAmmo(-0.15f, player);
					raySplasherCooldown2 = 0.03f;
					new RaySplasherProj(
						player.weapon, busterPos,
						getShootXDir(), raySplasherMod % 3, (raySplasherMod / 3) % 3,
						player, player.getNextActorNetId(), rpc: true
					);
					raySplasherMod++;
					if (raySplasherMod % 3 == 0) {
						if (raySplasherMod >= 21) {
							setShootRaySplasher(false);
							changeSprite("mmx_" + charState.defaultSprite, true);
						} else {
							raySplasherCooldown = Global.spf;
						}
					}
				} else {
					raySplasherCooldown2 -= Global.spf;
					if (raySplasherCooldown2 <= 0) {
						raySplasherCooldown2 = 0;
					}
				}
			}
		}
	}
	public override void chargeGfx() {
		if (ownedByLocalPlayer) {
			chargeEffect.stop();
		}
		if (isCharging()) {
			chargeSound.play();
			int chargeType = 2;
			if (player.hasArmArmor(3) && !player.hasGoldenArmor()) {
				chargeType = 1;
			}
			if (player.hasGoldenArmor()) {
				chargeType = 3;
			}
			
			int level = isHyperX ? unpoShotCount : getChargeLevel();
			var renderGfx = RenderEffectType.ChargeBlue;
			renderGfx = level switch {
				1 => RenderEffectType.ChargeBlue,
				2 => RenderEffectType.ChargeYellow,
				3 when (chargeType == 1) => RenderEffectType.ChargeOrange,
				3 when (chargeType == 3) => RenderEffectType.ChargeGreen,
				3 => RenderEffectType.ChargePink,
				_ => RenderEffectType.ChargeOrange
			};
			addRenderEffect(renderGfx, 0.033333f, 0.1f);			
			chargeEffect.update(level, chargeType);
		}
	}

	public override bool canAirDash() {
		return dashedInAir == 0 || (dashedInAir == 1 && hyperLegArmor == ArmorId.Max);
	}

	// Handles Bubble Splash Charged jump height
	public override float getJumpPower() {
		float jumpModifier = 0;
		jumpModifier += (chargedBubbles.Count / 6.0f) * 50;

		return jumpModifier + base.getJumpPower();
	}

	
	public override float getGravity() {
			float modifier = 1;
		if (chargedBubbles.Count > 0) {
			if (isUnderwater()) {
				modifier = 1 - (0.01f * chargedBubbles.Count);
			} else {
				modifier = 1 - (0.05f * chargedBubbles.Count);
	public override bool normalCtrl() {

		if (charState.attackCtrl && charState is not Dash && grounded && 
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);
			changeState(new SwordBlock());
			return true;
		}

		if (!grounded) {
			if (player.dashPressed(out string dashControl) && canAirDash() && canDash() && flag == null) {
				CharState dashState;
				if (player.input.isHeld(Control.Up, player) && player.hasBootsArmor(3)) {
					dashState = new UpDash(Control.Dash);
				} else {
					dashState = new AirDash(dashControl);
				}
				if (!isDashing) {
					changeState(dashState);
					return true;
				} else if (player.hasChip(0)) {
					changeState(dashState);
					return true;
				}
			}
			if (player.input.isPressed(Control.Jump, player) &&
				canJump() && isUnderwater() &&
				chargedBubbles.Count > 0 && flag == null
			) {
				vel.y = -getJumpPower();
				changeState(new Jump());
				return true;
			}
			if (!player.isAI && player.hasUltimateArmor() &&
				player.input.isPressed(Control.Jump, player) &&
				canJump() && !isDashing && canAirDash() && flag == null
			) {
				dashedInAir++;
				changeState(new XHover(), true);
			}
		}
		return base.normalCtrl();
	}

	public override bool attackCtrl() {
		bool shootPressed = player.input.isPressed(Control.Shoot, player);
		bool shootHeld = player.input.isHeld(Control.Shoot, player);
		bool specialPressed = player.input.isPressed(Control.Special1, player);
		
		if (isHyperX) {
			if (shootPressed && upPunchCooldown <= 0 && unpoShotCount <= 0 ) {
				upPunchCooldown = 30;
				changeState(new XUPPunchState(grounded), true);
				return true;
			} 
			else if (specialPressed && charState is Dash or AirDash) {
				charState.isGrabbing = true;
				changeSpriteFromName("unpo_grab_dash", true);
				return true;
			} 
			else if ( player.input.isWeaponLeftOrRightPressed(player) && parryCooldown <= 0 ) {
				if (unpoAbsorbedProj != null) {
					changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
					unpoAbsorbedProj = null;
					return true;
				} else {
					changeState(new XUPParryStartState(), true);
					return true;
				}
			}
		}

		if ( (isSpecialSaber() || isHyperX) && !hasBusterProj() &&
			canChangeWeapons() && player.armorFlag == 0 &&
			specialPressed && !stingActive && player.input.isHeld(Control.Up, player)
			&& charState is not Dash && charState is not AirDash
			
		) {
			if (xSaberCooldown == 0) {
			//	xSaberCooldown = 60;
				changeState(new X6SaberState(grounded), true);
				return true;
			}
		}

		bool hadokenCheck = false;
		bool shoryukenCheck = false;
		if (hasHadoukenEquipped()) {
			hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		}
		if (hasShoryukenEquipped()) {
			shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		}
		if (player.isX && hadokenCheck && canUseFgMove()) {
			if (!player.hasAllItems()) player.currency -= 3;
			player.fgMoveAmmo = 0;
			changeState(new Hadouken(), true);
			return true;
		}
		if (player.isX && shoryukenCheck && canUseFgMove()) {
			if (!player.hasAllItems()) player.currency -= 3;
			player.fgMoveAmmo = 0;
			changeState(new Shoryuken(isUnderwater()), true);
			return true;
		}

		Point inputDir = player.input.getInputDir(player);
		int oldSlot, newSlot;

		if (Global.level.is1v1() && player.weapons.Count == 10) {
			if (player.weaponSlot != 9) {
				player.weapons[9].update();
			}
		}
		return base.getGravity() * modifier;
	}

	// Attack related stuff.
	public void setShootAnim() {
		string shootSprite = getSprite(charState.shootSprite);
		if (!Global.sprites.ContainsKey(shootSprite)) {
			if (grounded) { shootSprite = getSprite("shoot"); } else { shootSprite = getSprite("fall_shoot"); }
		}
		if (shootAnimTime == 0) {
			changeSprite(shootSprite, false);
		} else if (charState is Idle) {
			frameIndex = 0;
			frameTime = 0;
		}
	}
	
	public override bool canShoot() {
		if (isInvulnerableAttack() ||
			hasLastingProj() ||
			shootCooldown > 0 ||
			invulnTime > 0 ||
			linkedTriadThunder != null
		) {
			return false;
		}
		return charState.attackCtrl;
	}

	public override bool canCharge() {
		return !isInvulnerableAttack() || hasLastingProj();
	}

	public override bool canChangeWeapons() {
		if (hasLastingProj()) {
			return false;
		}
		return base.canChangeWeapons();
	}

	public override void onWeaponChange(Weapon oldWeapon, Weapon newWeapon) {
		stingActiveTime = 0;
	}

	public void activateMaxBarrier(bool isFlinchOrStun) {
		if (!ownedByLocalPlayer ||
			barrierActiveTime > 0 ||
			barrierCooldown > 0
		) {
			return;
		}
		if (isFlinchOrStun) {
			barrierActiveTime = 90;
		} else {
			barrierActiveTime = 45;
		}
	}

	public bool hasHadoukenEquipped() {
		return !Global.level.is1v1() &&
		player.hasArmArmor(1) && player.hasBootsArmor(1) &&
		player.hasHelmetArmor(1) && player.hasBodyArmor(1);
	}

	public bool hasShoryukenEquipped() {
		return !Global.level.is1v1() &&
		player.hasArmArmor(2) && player.hasBootsArmor(2) &&
		player.hasHelmetArmor(2) && player.hasBodyArmor(2);
	}

	public bool hasFgMoveEquipped() {
		return hasHadoukenEquipped() || hasShoryukenEquipped();
	}

	public bool canAffordFgMove() {
		return player.currency >= 3 || player.hasAllItems();
	}

	public bool canUseFgMove() {
		return 
			!isInvulnerableAttack() && 
			chargedRollingShieldProj == null && 
			stingActiveTime == 0 && canAffordFgMove() && 
			hadoukenCooldownTime == 0 && player.weapon is XBuster && 
			player.fgMoveAmmo >= player.fgMoveMaxAmmo && grounded;
	}

	public bool hasLastingProj() {
		return (
			chargedSpinningBlade != null ||
			chargedFrostShield != null ||
			chargedTornadoFang != null ||
			strikeChainProj != null ||
			strikeChainChargedProj != null
		);
	}

	public void removeLastingProjs() {
		chargedSpinningBlade?.destroySelf();
		chargedSpinningBlade = null;
		chargedFrostShield?.destroySelf();
		chargedFrostShield = null;
		chargedTornadoFang?.destroySelf();
		chargedTornadoFang = null;
		strikeChainProj?.destroySelf();
		strikeChainProj = null;
		strikeChainChargedProj?.destroySelf();
		strikeChainChargedProj = null;
	}
	
	public void popAllBubbles() {
		for (int i = chargedBubbles.Count - 1; i >= 0; i--) {
			chargedBubbles[i].destroySelf();
		}
		chargedBubbles.Clear();
	}

	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"mmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"mmx_shoryuken" => MeleeIds.Shoryuken,
			"mmx_beam_saber" or "mmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"mmx_beam_saber2" => MeleeIds.ZSaber,
			"mmx_beam_saber_air2" => MeleeIds.ZSaberAir,
			"mmx_nova_strike" or "mmx_nova_strike_down" or "mmx_nova_strike_up" => MeleeIds.NovaStrike,
			// Light Helmet.
			"mmx_jump" or "mmx_jump_shoot" or "mmx_wall_kick" or "mmx_wall_kick_shoot"
			when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LigthHeadbutt,
			// Light Helmet when it up-dashes.
			"mmx_up_dash" or "mmx_up_dash_shoot"
			when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LigthHeadbuttEX,
			// Nothing.
			"mmx_beam_saber" or
			"mmx_beam_saber_air" => MeleeIds.X3Saber,
			"mmx_beam_saber2" or
			"mmx_beam_saber_air2" => MeleeIds.X6Saber,
			"mmx_nova_strike" or
			"mmx_nova_strike_down" or
			"mmx_nova_strike_up" => MeleeIds.NovaStrike,
			"mmx_unpo_grab_dash" => MeleeIds.UPGrab,
			"mmx_unpo_punch" or
			"mmx_unpo_punch_2" or
			"mmx_unpo_air_punch" => MeleeIds.UPPunch,
			"mmx_unpo_parry_start" => MeleeIds.UPParryBlock,

			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		return id switch {
			(int)MeleeIds.SpeedBurnerCharged => new GenericMeleeProj(
				SpeedBurner.netWeapon, projPos, ProjIds.SpeedBurnerCharged, player,
				4, Global.defFlinch, 0.5f
			),
			(int)MeleeIds.LigthHeadbutt => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.halfFlinch, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.LigthHeadbuttEX => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				4, Global.defFlinch, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.X6Saber => new GenericMeleeProj(
				new ZXSaber(player), projPos, ProjIds.X6Saber, player,
				1, 4 , 0.1f, isDeflectShield : true
			(int)MeleeIds.Shoryuken => new GenericMeleeProj(
				ShoryukenWeapon.netWeapon, projPos, ProjIds.Shoryuken, player,
				Damager.ohkoDamage, Global.defFlinch, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.MaxZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.XSaber, player,
				4, Global.defFlinch, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.ZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				3, 0, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.ZSaberAir => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, 0, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.NovaStrike => new GenericMeleeProj(
				HyperNovaStrike.netWeapon, projPos, ProjIds.NovaStrike, player,
				4, Global.defFlinch, 0.5f, addToLevel: addToLevel
			),
			_ => null
		};
	}

	/* public override Projectile? getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile? proj = null;

		if (sprite.name.Contains("beam_saber") && sprite.name.Contains("2")) {
			float overrideDamage = 3;
			if (!grounded) overrideDamage = 2;
			proj = new GenericMeleeProj(new XSaber(player), centerPoint, ProjIds.X6Saber, player, damage: overrideDamage, flinch: 20);
		} else if (sprite.name.Contains("beam_saber")) {
			proj = new GenericMeleeProj(new XSaber(player), centerPoint, ProjIds.XSaber, player);
		} else if (sprite.name.Contains("nova_strike")) {
			proj = new GenericMeleeProj(new NovaStrike(player), centerPoint, ProjIds.NovaStrike, player);
		} else if (sprite.name.Contains("speedburner")) {
			proj = new GenericMeleeProj(new SpeedBurner(player), centerPoint, ProjIds.SpeedBurnerCharged, player);
		} else if (sprite.name.Contains("shoryuken")) {
			proj = new GenericMeleeProj(new ShoryukenWeapon(player), centerPoint, ProjIds.Shoryuken, player);
		} else if (sprite.name.Contains("unpo_grab_dash")) {
			proj = new GenericMeleeProj(new XUPGrab(), centerPoint, ProjIds.UPGrab, player, 0, 0, 0);
		} else if (sprite.name.Contains("unpo_punch")) {
			proj = new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player,  2, 20, 0.15f);
		} else if (sprite.name.Contains("unpo_air_punch")) {
			proj = new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, damage: 3, flinch: Global.halfFlinch);
		} else if (sprite.name.Contains("unpo_parry_start")) {
			proj = new GenericMeleeProj(new XUPParry(), centerPoint, ProjIds.UPParryBlock, player, 0, 0, 0);
		}

			if (sprite.name.Contains("block")) {
			proj = new GenericMeleeProj(
				new VileStomp(), centerPoint, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true
			);
		}

		return proj;
	} */

	public enum MeleeIds {
		None = -1,
		SpeedBurnerCharged,
		LigthHeadbutt,
		LigthHeadbuttEX,
		Shoryuken,
		MaxZSaber,
		ZSaber,
		ZSaberAir,
		NovaStrike,
	}

	// Other overrides.
	public override void onFlinchOrStun(CharState newState) {
		strikeChainProj?.destroySelf();
		strikeChainChargedProj?.destroySelf();
		// Remove all linked stuff on stun.
		if (newState is not Hurt hurtState) {
			removeLastingProjs();
		}
		// Reset P-Bomb on flinch.
		else {
			chargedParasiticBomb?.reset(hurtState.isMiniFlinch());
		}
		base.onFlinchOrStun(newState);
	}

	public override bool changeState(CharState newState, bool forceChange = false) {
		bool hasChanged = base.changeState(newState, forceChange);
		if (!hasChanged || !ownedByLocalPlayer) {
			return hasChanged;
		}
		if (!newState.canUseShootAnim() && charState is not Hurt) {
			removeLastingProjs();
		}
		return true;
	}

	public override void getHealthNameOffsets(out bool shieldDrawn, ref float healthPct) {
		shieldDrawn = false;
		if (rideArmor != null) {
			shieldDrawn = true;
			healthPct = rideArmor.health / rideArmor.maxHealth;
		}
		else if (chargedRollingShieldProj != null) {
			shieldDrawn = true;
			healthPct = player.weapon.ammo / player.weapon.maxAmmo;
		}
	}

	public override void destroySelf(
		string spriteName = "", string fadeSound = "",
		bool disableRpc = false, bool doRpcEvenIfNotOwned = false,
		bool favorDefenderProjDestroy = false
	) {

		removeLastingProjs();
		chargedRollingShieldProj?.destroySelfNoEffect();
		strikeChainProj?.destroySelf();
		strikeChainChargedProj?.destroySelf();
		chargedParasiticBomb?.destroy();

		for (int i = magnetMines.Count - 1; i >= 0; i--) {
			magnetMines[i].destroySelf();
		}
		magnetMines.Clear();
		for (int i = rayTurrets.Count - 1; i >= 0; i--) {
			rayTurrets[i].destroySelf();
		}
		rayTurrets.Clear();

		base.destroySelf(spriteName, fadeSound, disableRpc, doRpcEvenIfNotOwned, favorDefenderProjDestroy);
	}

	public override string getSprite(string spriteName) {
		return "mmx_" + spriteName;
	}

	public override void render(float x, float y) {
		if (!shouldRender(x, y)) {
			return;
		}
		base.render(x, y);
		if (stingActive || UltrastingActive) {
			return false;
		}
		return damaged;
	}

	public override bool isStealthy(int alliance) {
		return (player.alliance != alliance && (stingActive || UltrastingActive));
	}

	public override bool isCCImmuneHyperMode() {
		return false;
	}

	public bool shouldShowHyperBusterCharge() {
		return player.weapon is HyperCharge hb && hb.canShootIncludeCooldown(player) || flag != null;
	}

	public override bool isInvulnerable(bool ignoreRideArmorHide = false, bool factorHyperMode = false) {
		bool invul = base.isInvulnerable(ignoreRideArmorHide, factorHyperMode);
		if (stingActive) {
			return !factorHyperMode;
		}
		return invul;
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();

		int weaponIndex = player.weapon.index;
		if (weaponIndex == (int)WeaponIds.HyperCharge) {
			weaponIndex = player.weapons[player.hyperChargeSlot].index;
		}
		customData.Add((byte)weaponIndex);
		customData.Add((byte)MathF.Ceiling(player.weapon?.ammo ?? 0));

		customData.AddRange(BitConverter.GetBytes(player.armorFlag));

		customData.Add(Helpers.boolArrayToByte([
			stingActive,
			isHyperX,
			isHyperChargeActive,
			hasUltimateArmor
		]));

		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];

		// Per-player data.
		player.changeWeaponFromWi(data[0]);
		if (player.weapon != null) {
			player.weapon.ammo = data[1];
		}
		player.armorFlag = BitConverter.ToUInt16(data[2..4]);

		bool[] boolData = Helpers.byteToBoolArray(data[4]);
		stingActive = boolData[0];
		isHyperX = boolData[1];
		isHyperChargeActive = boolData[2];
		hasUltimateArmor = boolData[3];
	}
}
