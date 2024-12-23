using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class RagingChargeX : Character {

	public bool hasUltimateArmor;

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


	
	public Projectile? unpoAbsorbedProj;
	public RagingChargeX(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.RagingChargeX;
	}




public override bool normalCtrl() {
		
	if (!player.input.isHeld(Control.Shoot, player) 
		 && charState is not Dash && grounded && 
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);

			if (player.weapon is not FireWave){
			changeState(new SwordBlock());
			}
			return true;
		}

		return base.normalCtrl();
	}



	public override void update() {
		base.update();


		if (player.currency > 4 && !hasUltimateArmor &&
		player.input.isPressed(Control.Special2, player)){
		hasUltimateArmor = true;
		addHealth(50);
		player.currency -= 5;
		changeSpriteFromName("warpin", true);
		}
		// Charge and release charge logic.
		chargeLogic(shoot);
		player.changeWeaponControls();



		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref upPunchCooldown);
		Helpers.decrementFrames(ref parryCooldown);
		Helpers.decrementFrames(ref xSaberCooldown);

		// Shotos
		bool hadokenCheck = false;
		bool shoryukenCheck = false;
	//	if (hasHadoukenEquipped()) {
			hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
	//	}
		//if (hasShoryukenEquipped()) {
			shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		//}
		if (player.isX && hadokenCheck && canUseFgMove()) {
			player.currency -= 3;
			player.fgMoveAmmo = 0;
			changeState(new Hadouken(), true);
	}
		if (player.isX && shoryukenCheck && canUseFgMove()) {
			player.currency -= 3;
			player.fgMoveAmmo = 0;
			changeState(new Shoryuken(isUnderwater()), true);
	}
		//>>>>>>>>>>>>>>>>>



	if (!player.input.isHeld(Control.Shoot, player) 
		 && charState is not Dash && grounded && charState.normalCtrl &&
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);

			if (player.weapon is not FireWave){
			changeState(new SwordBlock());
			}
	
		}

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

		if (!isInvulnerableAttack()) {
			if (charState.attackCtrl && player.input.isPressed(Control.Shoot, player)) {
				if (unpoShotCount <= 0) {
					upPunchCooldown = 0.5f;
					changeState(new XUPPunchState(grounded), true);
					return;
				}
			} else if (player.input.isPressed(Control.Special1, player) &&
				  (charState is Dash || charState is AirDash)) {
				charState.isGrabbing = true;
				changeSpriteFromName("unpo_grab_dash", true);
			} else if
			  (
				  hasUltimateArmor &&
				  player.input.isWeaponLeftOrRightPressed(player) && parryCooldown == 0 &&
				  (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is XUPPunchState || charState is XUPGrabState)
			  ) {
				if (unpoAbsorbedProj != null) {
					changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
					unpoAbsorbedProj = null;
					return;
				} else {
					changeState(new XUPParryStartState(), true);
				}
			}
			else if
			  (
				 !hasUltimateArmor &&
				  player.input.isWeaponLeftOrRightPressed(player) && parryCooldown == 0 &&
				  (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is XUPPunchState || charState is XUPGrabState)
			  ) {
					changeState(new GlobalParryState(), true);	
			}
		}

		if (charState.attackCtrl && canShoot() && canChangeWeapons() && 
			player.input.isPressed(Control.Special1, player) &&
			charState.normalCtrl && !charState.isGrabbing
		) {
			if (xSaberCooldown == 0) {
				xSaberCooldown = 60;
				changeState(new X6SaberState(grounded), true);
				return;
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
				applyDamage(1f, player, this, null, null);
			}
		}
		unpoShotCount = 0;
		if (player.weapon != null) {
			unpoShotCount = MathInt.Floor(player.weapon.ammo / player.weapon.getAmmoUsage(0));
		}
	}


	// Shoots stuff.
	public void shoot(int chargeLevel) {
	if (ownedByLocalPlayer){

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
			playSound("buster2X3", sendRpc: true);
			new BusterUnpoProj(new RagingChargeBuster(),  getShootPos(), xDir, player, player.getNextActorNetId(), true);
		
		} else if (chargeLevel == 2) {
			playSound("buster3X3", sendRpc: true);
				new BusterUnpoProj(new RagingChargeBuster(),  getShootPos(), xDir, player, player.getNextActorNetId(), true);
		
		} else if (chargeLevel == 3 || chargeLevel >= 4) {
			playSound("plasmaShot", sendRpc: true);
				new BusterPlasmaProj(
					getShootPos(), getShootXDir(),
					player, player.getNextActorNetId(), rpc: true
				);
				}			
		}
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

	public override Point getShootPos() {
		Point? busterOffsetPos = currentFrame.getBusterOffset();
		if (busterOffsetPos == null) {
			return getCenterPos();
		}
		Point busterOffset = busterOffsetPos.Value;
//		if (armArmor == ArmorId.Max && sprite.needsX3BusterCorrection()) {
//			if (busterOffset.x > 0) { busterOffset.x += 4; }
//			else if (busterOffset.x < 0) { busterOffset.x -= 4; }
//		}
		busterOffset.x *= xDir;
		if (player.weapon is RollingShield && charState is Dash) {
			busterOffset.y -= 2;
		}
		return pos.add(busterOffset);
	}
	
	public override bool canShoot() {
		if (isInvulnerableAttack() ||
			
			shootCooldown > 0 ||
			invulnTime > 0) {
			return false;
		}
		return true;
	}

	public override bool canCharge() {
		return !isInvulnerableAttack();
	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Shoot, player);
	}

	public override bool canChangeWeapons() {
		
		return base.canChangeWeapons();
	}





	public override void addAmmo(float amount) {
		getRefillTargetWeapon()?.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		getRefillTargetWeapon()?.addAmmoPercentHeal(amount);
	}

	public Weapon? getRefillTargetWeapon() {
		if (player.weapon.canHealAmmo && player.weapon.ammo < player.weapon.maxAmmo) {
			return player.weapon;
		}
		foreach (Weapon weapon in player.weapons) {
			if (weapon is GigaCrush or HyperNovaStrike or HyperCharge &&
			player.weapon.ammo < player.weapon.maxAmmo
			) {
				return weapon;
			}
		}
		Weapon? targetWeapon = null;
		float targetAmmo = Int32.MaxValue;
		foreach (Weapon weapon in player.weapons) {
			if (!weapon.canHealAmmo) {
				continue;
			}
			if (weapon != player.weapon &&
				weapon.ammo < weapon.maxAmmo &&
				weapon.ammo < targetAmmo
			) {
				targetWeapon = weapon;
				targetAmmo = targetWeapon.ammo;
			}
		}
		return targetWeapon;
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




	public override bool isCCImmuneHyperMode() {
		return false;
	}

	public override string getSprite(string spriteName) {
		return "rmx_" + spriteName;
	}

	public override void render(float x, float y) {
		if (!shouldRender(x, y)) {
			return;
		}
		if (sprite.name == "rmx_frozen") {
			Global.sprites["frozen_block"].draw(
				0, pos.x + x - (xDir * 2), pos.y + y + 1, xDir, 1, null, 1, 1, 1, zIndex + 1
			);
		}
		if (!hideHealthAndName()) {
			
		}
		base.render(x, y);
	}

	public override int getMaxChargeLevel() {
		if (!hasUltimateArmor){
		return 2;	
		}
		return 4;
	}


	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"rmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"rmx_shoryuken" => MeleeIds.Shoryuken,
			"rmx_block" or "rmx_block" => MeleeIds.XBlock,
			"rmx_beam_saber" or "rmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"rmx_beam_saber2"  => MeleeIds.ZSaber,
			"rmx_beam_saber_air2"  => MeleeIds.ZSaberAir,
			"rmx_nova_strike" or "rmx_nova_strike_down" or "rmx_nova_strike_up" => MeleeIds.NovaStrike,
			"rmx_unpo_grab_dash" => MeleeIds.UPGrab,
			"rmx_unpo_punch" or "rmx_unpo_air_punch" or "rmx_unpo_punch_2" => MeleeIds.UPPunch,
			"rmx_unpo_parry_start" => MeleeIds.UPParryBlock,

			// Light Helmet.
		//	"rmx_jump" or "rmx_jump_shoot" or "rmx_wall_kick" or "rmx_wall_kick_shoot"
		//	when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LigthHeadbutt,
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
				2, 10, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.XBlock => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.FrostShield, player,
				0, 0, 0f, addToLevel: addToLevel, isDeflectShield : true
			),
			(int)MeleeIds.ZSaberAir => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, 10, 0.5f, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.NovaStrike => new GenericMeleeProj(
				HyperNovaStrike.netWeapon, projPos, ProjIds.NovaStrike, player,
				4, Global.defFlinch, 0.5f, addToLevel: addToLevel
			),
			(int)MeleeIds.UPGrab => new GenericMeleeProj(
				new XUPGrab(), projPos, ProjIds.UPGrab, player, 0, 0, 0, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.UPPunch => new GenericMeleeProj(
				new XUPPunch(player), projPos, ProjIds.UPPunch, player,
			 2, Global.halfFlinch, 0.15f, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.UPParryBlock => new GenericMeleeProj(
				new XUPParry(), projPos, ProjIds.UPParryBlock, player, 0, 0, 1, addToLevel: addToLevel
			),
			_ => null
		};
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
		UPParryBlock,
	}


}
