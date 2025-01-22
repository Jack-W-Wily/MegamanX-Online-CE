using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class XAnother : Character {

	// Armor variables.
	public ArmorId chestArmor;
	public ArmorId armArmor;
	public ArmorId legArmor;
	public ArmorId helmetArmor;
	
	public ArmorId hyperChestArmor => (hyperChestActive ? helmetArmor : ArmorId.None);
	public ArmorId hyperArmArmor => (hyperArmActive ? armArmor : ArmorId.None);
	public ArmorId hyperLegArmor => (hyperLegActive ? legArmor : ArmorId.None);
	public ArmorId hyperHelmetArmor => (hyperHelmetActive ? helmetArmor : ArmorId.None);

	public bool hyperChestActive;
	public bool hyperArmActive;
	public bool hyperLegActive;
	public bool hyperHelmetActive;

	public bool hasUltimateArmor;

	public bool hasShadowArmor;

	public bool hasGaeaArmor;

	public bool hasXFire;

	public bool hasFullHyperMaxArmor => (
		hyperChestArmor == ArmorId.Max &&
		hyperArmArmor == ArmorId.Max &&
		hyperLegArmor == ArmorId.Max &&
		hyperHelmetArmor == ArmorId.Max
	);
	public bool hasAnyArmor => (
		chestArmor != 0 ||
		armArmor != 0 ||
		legArmor != 0 ||
		helmetArmor != 0
	);
	public bool hasAnyHyperArmor => (
		hyperChestActive ||
		hyperArmActive ||
		hyperLegActive ||
		hyperHelmetActive
	);
	public bool usedChips;

	// Shoot variables.


	public int cStingPaletteIndex;
	public float cStingPaletteTime;

	
	public int lastShootPressed;
	public float specialSaberCooldown;
	public XBuster specialBuster;
	public int specialButtonMode;

	public bool UnlockZsaber;







	public float shootCooldown;
	public int unpoShotCount;
	public float upPunchCooldown;
	public float xSaberCooldown;
	public float parryCooldown;
	public float maxParryCooldown = 30;
	
	float UPDamageCooldown;
	public float unpoDamageMaxCooldown = 2;

	// Shoto moves.
	public float hadoukenCooldownTime;
	public float maxHadoukenCooldownTime = 60;
	public float shoryukenCooldownTime;
	public float maxShoryukenCooldownTime = 60;




	public float headbuttAirTime = 0;
	public int hyperChargeTarget;
	public float noDamageTime;

	public float rechargeHealthTime;

		// Giga-attacks and armor weapons.
	public Weapon? gigaWeapon;
	public HyperNovaStrike? hyperNovaStrike;
	public ItemTracer itemTracer = new();
	public float barrierCooldown;
	public float barrierActiveTime;
	public Sprite barrierAnim = new Sprite("barrier_start");
	public bool stockedCharge;
	public float stockedChargeFlashTime;
	public bool stockedX3Charge;
	public bool stockedSaber;
	public bool hyperChargeActive;
	public bool stockedBuster;
	public bool stockedMaxBuster;

		// Weapon-specific.
	public RollingShieldProjCharged? chargedRollingShieldProj;
	public List<BubbleSplashProjCharged> chargedBubbles = new();
	public StrikeChainProj? strikeChainProj;
	public StrikeChainProjCharged? strikeChainChargedProj;
	public StrikeChainSemiCharged? strikeChainSemiChargedProj;
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
	public int stingPaletteIndex;
	public float stingPaletteTime;
	// Other.
	public float weaknessCooldown;


	public Weapon gigaAttack;
	public int gigaAttackSelected;


	public Projectile? unpoAbsorbedProj;
	public XAnother(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.XAnother;
		weapons = XLoadoutSetup.getLoadout(player);
		// Link X-Buster or create one.
		XBuster? tempBuster = weapons.Find((Weapon w) => w is XBuster) as XBuster;
		if (tempBuster != null) {
			specialBuster = tempBuster;
		} else {
			specialBuster = new XBuster();
		}



			gigaAttackSelected = player.loadout.zeroLoadout.gigaAttack;
		gigaAttack = player.loadout.zeroLoadout.gigaAttack switch {
			1 => new CFlasher(),
			2 => new RekkohaWeapon(),
			_ => new RakuhouhaWeapon(),
		};


	}




public override bool normalCtrl() {
		
	if (gigaAttack.ammo >= 5 &&
				player.input.isPressed(Control.Dash, player) &&
				player.input.isHeld(Control.Up, player) 
				
			) {
				gigaAttack.ammo -= 5;
				changeState(new UpDash(Control.Dash));
				return true;
			}

	if (charState is Dash or AirDash &&
		player.input.isHeld(Control.Down, player)
		&& gigaAttack.ammo >= 3
		){
			gigaAttack.ammo -= 3;
			changeState(new XIceSlide());
				slideVel = xDir * getDashSpeed() * 2;
			return true;
		}

	if (charState is not Dash && grounded && 
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);

			if (player.weapon is not FireWave){
			changeState(new SwordBlock());
			}
			return true;
		}
		if (!player.isAI && hasUltimateArmor &&
				player.input.isPressed(Control.Jump, player) &&
				player.input.isHeld(Control.Up,player)&&
				canJump() && !isDashing && canAirDash() && flag == null
			) {
				dashedInAir++;
				changeState(new XHover(), true);
				return true;
			}
		return base.normalCtrl();
	}



// Updates at the start of the frame.
	public override void preUpdate() {
		base.preUpdate();
		Helpers.decrementFrames(ref barrierActiveTime);

		// Max armor barrier sprite.
		if (barrierActiveTime > 0) {
			barrierAnim.update();
			if (barrierAnim.name == "barrier_start" && barrierAnim.isAnimOver()) {
				if (hyperChestArmor == ArmorId.Max) {
					barrierAnim = new Sprite("barrier2");
				} else {
					barrierAnim = new Sprite("barrier");
				}
			}
		}

		Helpers.decrementFrames(ref weaknessCooldown);
		if (!ownedByLocalPlayer) {
			return;
		}
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref barrierCooldown);
		Helpers.decrementFrames(ref specialSaberCooldown);

		// For the shooting animation.
		if (shootAnimTime > 0) {
			shootAnimTime -= speedMul;
			if (shootAnimTime <= 0) {
				shootAnimTime = 0;
				if (sprite.name == getSprite(charState.shootSprite) ||
					sprite.name == getSprite("shoot") ||
					sprite.name == getSprite("jump_shoot") ||
					sprite.name == getSprite("fall_shoot")
				) {
					changeSpriteFromName(charState.defaultSprite, false);
					if (charState is WallSlide) {
						frameIndex = sprite.totalFrameNum - 1;
					}
				}
			}
		}
	}


	public override int getMaxChargeLevel() {
		if (hasUltimateArmor){
		return 4;
		}
		return 3;
	}



	public override void update() {
		base.update();


		if (gigaAttack.ammo >= 28 && !hasUltimateArmor &&
		player.input.isPressed(Control.Special2, player)){
		hasUltimateArmor = true;
		player.addNovaStrike();
		addHealth(5);
		//player.currency -= 5;
		changeSpriteFromName("warp_in", true);

		}
	if (cStingPaletteTime > 5) {
			cStingPaletteTime = 0;
			cStingPaletteIndex++;
		}
		cStingPaletteTime++;



		if (!ownedByLocalPlayer) {
			return;
		}
		// Shotos

		

	
		bool hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		bool shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		
		if ( hadokenCheck && gigaAttack.ammo >= 14) {
			gigaAttack.ammo -= 14;
			changeState(new Hadouken(), true);
		}
		if (shoryukenCheck && gigaAttack.ammo >= 14) {
			 gigaAttack.ammo -= 14;
			changeState(new Shoryuken(isUnderwater()), true);
		}

		if (gigaAttack.ammo > 0 && player.input.isHeld(Control.Down,player) &&
		charState is Jump && !sprite.name.Contains("headbutt")){
		 gigaAttack.ammo -= 1;
		changeSpriteFromName("headbutt", false);
		}

		if (gigaAttack.ammo > 0 && player.input.isPressed(Control.Dash,player) &&
		sprite.name.Contains("unpo")){
		 gigaAttack.ammo -= 1;
		changeState(new XlightKick(), true);
		}



		//>>>>>>>>>>>>>>>>>

			player.fgMoveAmmo += Global.speedMul;
		if (player.fgMoveAmmo > player.fgMoveMaxAmmo) player.fgMoveAmmo = player.fgMoveMaxAmmo;



		gigaWeapon?.update();
		hyperNovaStrike?.update();
		itemTracer?.update();
		shootingRaySplasher?.burstLogic2(this);

		// Charge and release charge logic.
		if (!isInDamageSprite() && !sprite.name.Contains("block")){
		chargeLogic(shoot);
		}
		player.changeWeaponControls();
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref upPunchCooldown);
		Helpers.decrementFrames(ref parryCooldown);
		Helpers.decrementFrames(ref xSaberCooldown);


		

		if (musicSource == null && hasUltimateArmor) {
			addMusicSource("XvsZeroV2_megasfc", getCenterPos(), true);
		}
		
		if (!ownedByLocalPlayer) return;

			if (!isCharging() && currentWeapon != null && (
				player.input.isPressed(Control.Shoot, player))
		) {
			if (currentWeapon.shootCooldown <= 0) {
				shoot(0);
			}
		}



		if (charState is not XUPGrabState
			and not XUPParryMeleeState
			and not XUPParryProjState
			and not Hurt
			and not GenericStun
			and not VileMK2Grabbed
			and not GenericGrabbedState
		) {
			UPDamageCooldown += Global.spf;
			if (hasUltimateArmor &&
				UPDamageCooldown > unpoDamageMaxCooldown) {
				UPDamageCooldown = 0;
				addAmmo(2);
				//applyDamage(1f, player, this, null, null);
			}
		}
		unpoShotCount = 0;
		if (player.weapon != null) {
			unpoShotCount = MathInt.Floor(player.weapon.ammo / player.weapon.getAmmoUsage(0));
		}
	}

	public override bool attackCtrl() {
		if (player.input.isPressed(Control.Special1, player) && helmetArmor == ArmorId.Giga &&
			itemTracer.shootCooldown == 0
		) {
			itemTracer.shoot(this, [0, hyperHelmetArmor == ArmorId.Giga ? 1 : 0]);
			itemTracer.shootCooldown = itemTracer.fireRate;
		}
		
		if (player.input.isPressed(Control.Shoot, player) && stockedMaxBuster) {
			shoot(1, specialBuster, false);
			return true;
		}
		if (player.input.isPressed(Control.Shoot, player) && stockedBuster) {
			shoot(1, currentWeapon, true);
			return true;
		}
		if (!isCharging() && currentWeapon != null && (
				player.input.isPressed(Control.Shoot, player) ||
				currentWeapon.isStream && getChargeLevel() < 2 &&
				player.input.isHeld(Control.Shoot, player)
			)
		) {
			if (currentWeapon.shootCooldown <= 0) {
				shoot(0);
				return true;
			}
		}
		if (player.input.isPressed(Control.Special1, player)
		&& UnlockZsaber) {
			changeState(new X6SaberState(grounded), true);
			
		}
		if (player.input.isPressed(Control.Special1, player)
		&& !UnlockZsaber &&   charState is not AirDash
		&& charState is not Dash
		&& !player.input.isHeld(Control.Down, player)) {
			changeState(new XUPPunchState(grounded), true);
			
		}


		 if (player.input.isPressed(Control.Special1, player) &&
				  (charState is Dash || charState is AirDash)) {
				charState.isGrabbing = true;
				changeSpriteFromName("unpo_grab_dash", true);
			}  
			if  ( gigaAttack.ammo >= 5 && parryCooldown == 0 &&
				 player.input.isPressed(Control.Special1, player) &&
				 player.input.isHeld(Control.Down, player)		 ) {
					gigaAttack.ammo -= 5;
					changeState(new XUPParryStartState(), true);
				}
			
		
		return base.attackCtrl();
	}

	// Shoots stuff.
	public void shoot(int chargeLevel) {
		shoot(chargeLevel, currentWeapon, false);
	}

	public void shoot(int chargeLevel, Weapon weapon, bool busterStock) {
		// Check if can shoot.
		if (shootCooldown > 0 ||
			weapon == null ||
			!weapon.canShoot(chargeLevel, player) ||
			weapon.shootCooldown > 0
		) {
			return;
		}
		// Calls the weapon shoot function.
		bool useCrossShotAnim = false;
		if (chargeLevel >= 3 && armArmor == ArmorId.Giga || busterStock) {
			if (!busterStock) {
				stockedBuster = true;
			} else if (chargeLevel < 3) {
				stockedBuster = false;
			}
			if (charState.normalCtrl && charState.attackCtrl) {
				useCrossShotAnim = true;
			} else {
				chargeLevel = 3;
			}
		}
		// Changes to shoot animation and gets sound.
		setShootAnim();
		shootAnimTime = DefaultShootAnimTime;
		string shootSound = weapon.shootSounds[chargeLevel];
		// Shoot.
		if (useCrossShotAnim) {
			changeState(new X2ChargeShot(null, busterStock ? 1 : 0), true);
			stopCharge();
			return;
		} else {
			weapon.shoot(this, [chargeLevel, busterStock ? 1 : 0]);
		}
		// Sets up global shoot cooldown to the weapon shootCooldown.
		if (!stockedBuster || busterStock || weapon.fireRate <= 12) {
			weapon.shootCooldown = weapon.fireRate;
		} else {
			weapon.shootCooldown = 12;
		}
		shootCooldown = weapon.shootCooldown;
		// Add ammo.
		weapon.addAmmo(-weapon.getAmmoUsageEX(chargeLevel, this), player);
		// Play sound if any.
		if (shootSound != "") {
			playSound(shootSound, sendRpc: true);
		}
		// Stop charge if this was a charge shot.
		if (chargeLevel >= 1) {
			stopCharge();
		}
	}

	// Movement related stuff.
	public override float getRunSpeed() {
		if (charState is XHover) {
			return 2 * 60 * getRunDebuffs();;
		}
		return base.getRunSpeed();
	}

	public override float getDashSpeed() {
		if (flag != null || !isDashing) {
			return getRunSpeed();
		}
		float dashSpeed = 3.5f * 60;
		return dashSpeed * getRunDebuffs();
	}

	public override bool canAirDash() {
		return dashedInAir == 0 || (dashedInAir < 2 && hyperLegArmor == ArmorId.Max);
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
		} else if (shootSprite == getSprite("shoot")) {
			frameIndex = 0;
			frameTime = 0;
		}
	}

	public override Point getShootPos() {
		Point? busterOffsetPos = currentFrame.getBusterOffset();
		if (busterOffsetPos == null) {
			return getCenterPos();
		}
		Point busterOffset = busterOffsetPos.Value;
		if (armArmor == ArmorId.Max && sprite.needsX3BusterCorrection()) {
			if (busterOffset.x > 0) { busterOffset.x += 4; }
			else if (busterOffset.x < 0) { busterOffset.x -= 4; }
		}
		busterOffset.x *= xDir;
		if (currentWeapon is RollingShield && charState is Dash) {
			busterOffset.y -= 2;
		}
		return pos.add(busterOffset);
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
		return !isInvulnerableAttack() && !hasLastingProj();
	}

	public override void increaseCharge() {
		if (armArmor == ArmorId.Light) {
			chargeTime += speedMul * 1.5f;
			return;
		}
		chargeTime += speedMul;
	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Shoot, player);
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

	public override bool canAddAmmo() {
		if (weapons.Count == 0) { return false; }
		bool hasEmptyAmmo = false;
		foreach (Weapon weapon in weapons) {
			if (weapon.canHealAmmo && weapon.ammo < weapon.maxAmmo) {
				return true;
			}
		}
		return hasEmptyAmmo;
	}

	public override void addAmmo(float amount) {
		getRefillTargetWeapon()?.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		getRefillTargetWeapon()?.addAmmoPercentHeal(amount);
	}

	public Weapon? getRefillTargetWeapon() {
		if (currentWeapon != null && currentWeapon.canHealAmmo && currentWeapon.ammo < currentWeapon.maxAmmo) {
			return currentWeapon;
		}
		foreach (Weapon weapon in weapons) {
			if (weapon is GigaCrush or HyperNovaStrike or HyperCharge && weapon.ammo < weapon.maxAmmo) {
				return weapon;
			}
		}
		Weapon? targetWeapon = null;
		float targetAmmo = Int32.MaxValue;
		foreach (Weapon weapon in weapons) {
			if (!weapon.canHealAmmo) {
				continue;
			}
			if (weapon != currentWeapon &&
				weapon.ammo < weapon.maxAmmo &&
				weapon.ammo < targetAmmo
			) {
				targetWeapon = weapon;
				targetAmmo = targetWeapon.ammo;
			}
		}
		return targetWeapon;
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


	
	public bool hasLastingProj() {
		return (
			chargedSpinningBlade != null ||
			chargedFrostShield != null ||
			chargedTornadoFang != null ||
			strikeChainProj != null ||
			strikeChainSemiChargedProj != null ||
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
		strikeChainSemiChargedProj?.destroySelf();
		strikeChainSemiChargedProj = null;
		strikeChainChargedProj = null;
	}
	
	public void popAllBubbles() {
		for (int i = chargedBubbles.Count - 1; i >= 0; i--) {
			chargedBubbles[i].destroySelf();
		}
		chargedBubbles.Clear();
	}




	public bool hasHadoukenEquipped() {
		return true;
	}

	public bool hasShoryukenEquipped() {
		return true;
	}

	public bool hasFgMoveEquipped() {
		return hasHadoukenEquipped() || hasShoryukenEquipped();
	}

	public bool canAffordFgMove() {
		return player.currency >= 3;
	}

	public bool canUseFgMove() {
		return canAffordFgMove(); //(
	//		!isInvulnerableAttack() && 
	//		 canAffordFgMove() && 
	//		hadoukenCooldownTime == 0 && 
	//		player.fgMoveAmmo >= player.fgMoveMaxAmmo
//		);
	}

	public override string getSprite(string spriteName) {
		return "rmx_" + spriteName;
	}

	
	
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
		XBlock,
		UPGrab,
		UPPunch,

		UPDash,

		IceSlide,

		LightKick,

		UPParryBlock,
	}



	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"rmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"rmx_shoryuken" => MeleeIds.Shoryuken,
			"rmx_block" => MeleeIds.XBlock,
			"rmx_beam_saber" or "rmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"rmx_beam_saber2"  => MeleeIds.ZSaber,
			"rmx_beam_saber_air2"  => MeleeIds.ZSaberAir,
			"rmx_nova_strike" or "rmx_nova_strike_down" or "rmx_nova_strike_up" => MeleeIds.NovaStrike,
			"rmx_unpo_grab_dash" => MeleeIds.UPGrab,
			"rmx_unpo_punch" or "rmx_unpo_air_punch" or "rmx_unpo_punch_2" => MeleeIds.UPPunch,
			"rmx_unpo_parry_start" => MeleeIds.UPParryBlock,
			"rmx_up_dash"  => MeleeIds.UPDash,
			"rmx_sice_slide"  => MeleeIds.IceSlide,
			// Light Helmet.
			"rmx_headbutt"  => MeleeIds.LigthHeadbutt,

			"rmx_kick_lightarmor"  => MeleeIds.LightKick,
			// Light Helmet when it up-dashes.
		//	"rmx_up_dash" or "rmx_up_dash_shoot"
		//	when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LigthHeadbuttEX,
			// Nothing.
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		
	
		
		return id switch {
			(int)MeleeIds.SpeedBurnerCharged => new GenericMeleeProj(
				SpeedBurner.netWeapon, projPos, ProjIds.SpeedBurnerCharged, player,
				1, Global.defFlinch, 2f
			),
			(int)MeleeIds.LigthHeadbutt => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.halfFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.LigthHeadbuttEX => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				4, Global.defFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.Shoryuken => new GenericMeleeProj(
				ShoryukenWeapon.netWeapon, projPos, ProjIds.Shoryuken, player,
				2, Global.defFlinch, 5f, addToLevel: addToLevel
			),
			(int)MeleeIds.MaxZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.XSaber, player,
				4, Global.defFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.IceSlide => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.SiceSlide, player,
				2, Global.defFlinch, 20f, addToLevel: addToLevel
			),

			(int)MeleeIds.LightKick => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.NormalPush, player,
				3, 0, 20f, addToLevel: addToLevel
			),

			(int)MeleeIds.UPDash => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.MechFrogGroundPound, player,
				3, 30, 20f, addToLevel: addToLevel
			),


			(int)MeleeIds.ZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, 10, 10f, addToLevel: addToLevel
			),
			(int)MeleeIds.XBlock => new GenericMeleeProj(
				new RCXPunch(), projPos, ProjIds.SwordBlock, player, 0, 0, 0, isDeflectShield: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.ZSaberAir => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, 10, 10f, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.NovaStrike => new GenericMeleeProj(
				HyperNovaStrike.netWeapon, projPos, ProjIds.NovaStrike, player,
				4, Global.defFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.UPGrab => new GenericMeleeProj(
				new XUPGrab(), projPos, ProjIds.UPGrab, player, 0, 0, 0, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.UPPunch => new GenericMeleeProj(
				new RCXPunch(), projPos, ProjIds.UPPunch, player,
			 2, Global.halfFlinch, 15f, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.UPParryBlock => new GenericMeleeProj(
				new XUPParry(), projPos, ProjIds.UPParryBlock, player, 0, 0, 1, addToLevel: addToLevel
			),
			_ => null
		};
	}



		
	public override void render(float x, float y) {
		if (!shouldRender(x, y)) {
			return;
		} 
		if (sprite.name == "mmx_frozen") {
			Global.sprites["frozen_block"].draw(
				0, pos.x + x - (xDir * 2), pos.y + y + 1, xDir, 1, null, 1, 1, 1, zIndex + 1
			);
		}
		float backupAlpha = alpha;
		if (stingActiveTime > 0) {
			if (stingPaletteTime > 6) {
				stingPaletteTime = 0;
				stingPaletteIndex++;
				if (stingPaletteIndex >= 9) {
					stingPaletteIndex = 0;
				}
			} else {
				stingPaletteTime++;
			}
			if (stingPaletteTime % 4 <= 1) {
				alpha *= 0.25f;
			}
		}
		base.render(x, y);
		alpha = backupAlpha;
	}

	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;
		int index = player.weapon.index;

		if (stingActiveTime > 0 && stingPaletteIndex != 0){
			palette = player.xStingPaletteShader;
			palette.SetUniform("palette", stingPaletteIndex);

			shaders.Add(palette);
			shaders.AddRange(baseShaders);
			return shaders;
		}

		if (index >= (int)WeaponIds.GigaCrush) {
			index = 0;
		}
		if (index == (int)WeaponIds.XSaber) {
			index = 0;
		}
	
		if (index == (int)WeaponIds.HyperCharge && ownedByLocalPlayer) {
			index = player.weapons[player.hyperChargeSlot].index;
		}
		if (hasFullHyperMaxArmor) {
			index = 25;
		}
		if (hasUltimateArmor) {
			index = 0;
		}
		palette = player.xPaletteShader;

		palette?.SetUniform("palette", index);

		if (palette != null) {
			shaders.Add(palette);
		}
		if (shaders.Count == 0) {
			return baseShaders;
		}
		shaders.AddRange(baseShaders);
		return shaders;
	}

	public int getArmorByte() {
		int armorByte = (byte)chestArmor;
		armorByte += (byte)armArmor << 4;
		armorByte += (byte)legArmor << 8;
		armorByte += (byte)helmetArmor << 12;

		return armorByte;
	}

	public void setArmorByte(int armorByte) {
		int[] values = new int[4];
		for (int i = values.Length - 1; i >= 0; i--) {
			int offF = (i + 1) * 4;
			int offB = i * 4;
			values[i] = ((armorByte >> offF << offF) ^ armorByte) >> offB;
		}
		chestArmor = (ArmorId)values[0];
		armArmor = (ArmorId)values[1];
		legArmor = (ArmorId)values[2];
		helmetArmor = (ArmorId)values[3];
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();

		int weaponIndex = currentWeapon?.index ?? 255;
		byte ammo = (byte)MathF.Ceiling(currentWeapon?.ammo ?? 0);
		if (weaponIndex == (int)WeaponIds.HyperCharge) {
			weaponIndex = weapons[player.hyperChargeSlot].index;
			ammo = (byte)MathF.Ceiling(weapons[player.hyperChargeSlot].ammo);
		}
		customData.Add((byte)weaponIndex);
		customData.Add(ammo);
		customData.AddRange(BitConverter.GetBytes(getArmorByte()));

		// Stocked charge flags.
		customData.Add(Helpers.boolArrayToByte([
			stingActiveTime > 0,
			stockedBuster,
			stockedMaxBuster,
			stockedSaber,
			hyperChargeActive,
		]));

		// Hyper Armor Flags.
		customData.Add(Helpers.boolArrayToByte([
			hyperChestActive,
			hyperArmActive,
			hyperLegActive,
			hyperHelmetActive,
			hasUltimateArmor
		]));

		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];

		// Per-player data.
		Weapon? targetWeapon = weapons.Find(w => w.index == data[0]);
		if (targetWeapon != null) {
			weaponSlot = weapons.IndexOf(targetWeapon);
			targetWeapon.ammo = data[1];
		}
		setArmorByte(BitConverter.ToUInt16(data[2..4]));

		// Stocked charge and weapon flags.
		bool[] boolData = Helpers.byteToBoolArray(data[4]);
		stingActiveTime = boolData[0] ? 20 : 0;
		stockedBuster = boolData[1];
		stockedMaxBuster = boolData[2];
		stockedSaber = boolData[3];
		hyperChargeActive = boolData[4];

		// Hyper Armor Flags.
		bool[] armorBoolData = Helpers.byteToBoolArray(data[4]);
		hasXFire = armorBoolData[0];
		hasGaeaArmor = armorBoolData[1];
		hasShadowArmor = armorBoolData[2];
		hasUltimateArmor = armorBoolData[3];
	}
}
