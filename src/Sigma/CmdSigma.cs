using System;
using System.Collections.Generic;

namespace MMXOnline;

public class CmdSigma : BaseSigma {
	public float saberCooldown;
	public float leapSlashCooldown;

	public float dodgeRollCooldown;
	
	public float sigmaAmmoRechargeCooldown = 0;
	public float sigmaAmmoRechargeTime;
	public float sigmaHeadBeamRechargePeriod = 5;
	public float sigmaHeadBeamTimeBeforeRecharge = 20;

	public CmdSigma(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId,
		bool ownedByLocalPlayer, bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn
	) {
		sigmaSaberMaxCooldown = 1;
		spriteFrameToSounds["sigma_run/2"] = "sigmawalk";
		spriteFrameToSounds["sigma_run/7"] = "sigmawalk";
	}

	public override void update() {
		base.update();


		if (sprite.name.Contains("slash"))sigmaAmmoRechargeCooldown = 0.5f;
		if (charState.attackCtrl){
			if ((charState is Dash || charState is AirDash) 
			&& (player.input.isPressed(Control.Shoot, player)
			|| player.input.isPressed(Control.Special1, player))){
			slideVel = xDir * getDashSpeed();			
			}
		}

		if (!isInDamageSprite() &&
			dodgeRollCooldown == 0 && player.canControl) {
		 if (player.input.isPressed(Control.Dash, player) && player.input.checkDoubleTap(Control.Dash)) {
				changeState(new SigDodge(), true);
				rideArmorPlatform = null;
			}
		}




		if (!ownedByLocalPlayer) {
			return;
		}
		// Cooldowns.
			Helpers.decrementTime(ref dodgeRollCooldown);
		Helpers.decrementTime(ref saberCooldown);
		Helpers.decrementTime(ref leapSlashCooldown);
		Helpers.decrementFrames(ref sigmaAmmoRechargeCooldown);
		// Ammo reload.
		if (sigmaAmmoRechargeCooldown == 0) {
			Helpers.decrementFrames(ref sigmaAmmoRechargeTime);
			if (sigmaAmmoRechargeTime == 0) {
				player.sigmaAmmo = Helpers.clampMax(player.sigmaAmmo + 1, player.sigmaMaxAmmo);
				sigmaAmmoRechargeTime = sigmaHeadBeamRechargePeriod;
			}
		} else {
			sigmaAmmoRechargeTime = 0;
		}
		// For ladder and slide attacks.
		if (isAttacking() && charState is WallSlide or LadderClimb && !isSigmaShooting()) {
			if (isAnimOver() && charState != null && charState is not SigmaSlashState) {
				changeSprite(getSprite(charState.defaultSprite), true);
				if (charState is WallSlide && sprite != null) {
					frameIndex = sprite.totalFrameNum - 1;
				}
			} else if (grounded && sprite.name != "sigma_attack") {
				changeSprite("sigma_attack", false);
			}
		}
	}

	public override bool attackCtrl() {
		if (isInvulnerableAttack() || player.weapon is MaverickWeapon) {
			return false;
		}
		bool attackPressed = false;
		if (player.weapon is not AssassinBullet) {
			if (player.input.isPressed(Control.Shoot, player)) {
				attackPressed = true;
				lastAttackFrame = Global.level.frameCount;
			}
		}
		framesSinceLastAttack = Global.level.frameCount - lastAttackFrame;
		bool lenientAttackPressed = (attackPressed || framesSinceLastAttack < 5);

		if (lenientAttackPressed && saberCooldown == 0) {
			//saberCooldown = sigmaSaberMaxCooldown;

			if (charState is WallSlide or LadderClimb) {
				if (charState is LadderClimb) {
					int inputXDir = player.input.getXDir(player);
					if (inputXDir != 0) {
						xDir = inputXDir;
					}
				}
				changeSprite(getSprite(charState.attackSprite), true);
				playSound("sigmaSaber", sendRpc: true);
				return true;
			}

			if (!player.input.isHeld(Control.Up, player) &&
			!player.input.isHeld(Control.Down, player) )
			changeState(new SigmaSlashState(charState), true);
			return true;
		}
		if (charState is Dash dashState) {
			if (!dashState.stop && player.isSigma &&
				player.input.isPressed(Control.Special1, player) &&
				flag == null// && leapSlashCooldown == 0
			) {
				changeState(new SigmaWallDashState(-1, true), true);
				return true;
			}
		}
		if (charState is AirDash) {
			if (player.isSigma &&
				player.input.isPressed(Control.Special1, player) &&
				flag == null// && leapSlashCooldown == 0
			) {
				changeState(new SigmaWallDashState(1, true), true);
				return true;
			}
		}
		if (grounded && charState is Idle || charState is Run || charState is Crouch) {
			if (player.input.isHeld(Control.Special1, player) && player.sigmaAmmo > 0) {
				sigmaAmmoRechargeCooldown = 0.5f;
				changeState(new SigmaBallShoot(), true);
				return true;
			}
		}
		return base.attackCtrl();
	}

	public override Collider getBlockCollider() {
		Rect rect = Rect.createFromWH(0, 0, 16, 35);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

public override string getSprite(string spriteName) {
			if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("sigma1alt_" + spriteName)){		
			return "sigma1alt_" + spriteName;
			}
			return "sigma_" + spriteName;
	}
	

	// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile? getProjFromHitbox(Collider collider, Point centerPoint) {
		
		
		Projectile? proj = sprite.name switch {
			"sigma_ladder_attack" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				3, 30, 25f
			),
			"sigma_wall_slide_attack" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				3, 30, 25f
			),
				"sigma_wall_dash" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				1, 30, 25f, isDeflectShield : true, isJuggleProjectile : true
			),
				"sigma_dash" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				1, 30, 25f
			),
				"sigma_slash_1" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VirusSlash, player,
				2, Global.defFlinch, 15f
			),
				"sigma_slash_2" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VirusSlash, player,
				3, Global.superFlinch, 15f
			),
				"sigma_slash_3" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VirusSlash, player,
				3, Global.defFlinch, 15f
			),
				"sigma_grab" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VileSuperKick, player,
				2, Global.defFlinch, 15f
			),
				"sigma_block" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				2, Global.defFlinch, 60f
			),








			"sigma1alt_ladder_attack" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				3, 30, 25f
			),
			"sigma1alt_wall_slide_attack" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				3, 30, 25f
			),
				"sigma1alt_wall_dash" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				1, 30, 25f, isDeflectShield : true, isJuggleProjectile : true
			),
				"sigma1alt_dash" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				1, 30, 25f
			),
				"sigma1alt_slash_1" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VirusSlash, player,
				2, Global.defFlinch, 15f
			),
				"sigma1alt_slash_2" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VirusSlash, player,
				3, Global.superFlinch, 15f
			),
				"sigma1alt_slash_3" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VirusSlash, player,
				3, Global.defFlinch, 15f
			),
				"sigma1alt_grab" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.VileSuperKick, player,
				2, Global.defFlinch, 15f
			),
				"sigma1alt_block" => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, centerPoint, ProjIds.SigmaSlash, player,
				2, Global.defFlinch, 60f
			),

			_ => null
		};
		if (proj != null) {
			return proj;
		}

		
		return base.getProjFromHitbox(collider, centerPoint);
	}

	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return (player.sigmaAmmo < player.sigmaMaxAmmo);
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add((byte)MathF.Ceiling(player.sigmaAmmo));

		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];

		// Per-player data.
		player.sigmaAmmo = data[0];
	}

	public float aiAttackCooldown;
	public override void aiAttack(Actor? target) {
		bool isTargetInAir = pos.y < target?.pos.y - 20;
		bool isTargetClose = pos.x < target?.pos.x - 10;
		if (currentWeapon is MaverickWeapon mw &&
			mw.maverick == null && canAffordMaverick(mw)
		) {
			buyMaverick(mw);
			if (mw.maverick != null) {
				changeState(new CallDownMaverick(mw.maverick, true, false), true);
			}
			mw.summon(player, pos.addxy(0, -112), pos, xDir);
			player.changeToSigmaSlot();
		}
		if (charState is not LadderClimb) {
			int Sattack = Helpers.randomRange(0, 5);
			if (charState?.isGrabbedState == false && !player.isDead
				&& !isInvulnerable() && !(charState is CallDownMaverick or SigmaSlashState)
				&& aiAttackCooldown <= 0) {
				switch (Sattack) {
					case 0 when isTargetClose:
						changeState(new SigmaSlashState(charState), true);
						break;
					case 1 when isTargetInAir:
						changeState(new SigmaBallShootEX(), true);
						break;
					case 2 when charState is Dash && grounded:
						changeState(new SigmaWallDashState(xDir, true), true);
						break;
					case 3:
						player.changeWeaponSlot(1);						
						break;
					case 4:
						player.changeWeaponSlot(2);
						break;
					case 5:
						player.changeWeaponSlot(0);
						break;
				}
				aiAttackCooldown = 18;
			}
		}
		base.aiAttack(target);
	}

	public override void aiDodge(Actor? target) {
		foreach (GameObject gameObject in getCloseActors(32, true, false, false)) {
			if (gameObject is Projectile proj && proj.damager.owner.alliance != player.alliance) {
				if (!(proj.projId == (int)ProjIds.SwordBlock)) {
						changeState(new SigmaBlock(), true);
				}
			}
		}
		base.aiDodge(target);
	}
}
