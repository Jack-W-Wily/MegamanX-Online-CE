using System;
using System.Collections.Generic;

namespace MMXOnline;

public class Sigma1 : BaseSigma {
	public Weapon ballWeapon;
	public float saberCooldown;
	public float leapSlashCooldown;
	public float sigmaAmmoRechargeCooldown = 0;
	public float sigmaAmmoRechargeTime;
	public float sigmaHeadBeamRechargePeriod = 5;
	public float sigmaHeadBeamTimeBeforeRecharge = 20;
	public float aiAttackCooldown;
	public float dashSlashCooldown;

	public float sigDodgeCooldown;

	public Sigma1(
	Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId,
		bool ownedByLocalPlayer, bool isWarpIn = true,
		SigmaLoadout? loadout = null,
		int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn,
		loadout, heartTanks, isATrans
	) {
		sigmaSaberMaxCooldown = 1;
		altSoundId = AltSoundIds.X1;
		ballWeapon = new SigmaBallWeapon();
		ShouldExplode = true;
	}

	public override void update() {
		base.update();


		if (player.input.isPressed(Control.Taunt, player) && player.input.isHeld(Control.Up, player)
		&& player.currency > 4
		) {
			player.currency -= 5;
			overDriveTimer = 12;
			playSound("ching");

		}

		if (!ownedByLocalPlayer) {
			return;
		}
		// Cooldowns.
		Helpers.decrementTime(ref sigDodgeCooldown);
		Helpers.decrementTime(ref saberCooldown);
		Helpers.decrementFrames(ref dashSlashCooldown);
		Helpers.decrementTime(ref leapSlashCooldown);
		Helpers.decrementFrames(ref sigmaAmmoRechargeCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		// Ammo reload.
		if (sigmaAmmoRechargeCooldown == 0) {
			Helpers.decrementFrames(ref sigmaAmmoRechargeTime);
			if (sigmaAmmoRechargeTime == 0) {
				ballWeapon.addAmmo(1, player);
				sigmaAmmoRechargeTime = sigmaHeadBeamRechargePeriod;
			}
		} else {
			sigmaAmmoRechargeTime = 0;
		}
		// For ladder and slide attacks.
		if (isAttacking() && charState is WallSlide or LadderClimb && !isSigmaShooting()) {
			if (isAnimOver() && charState != null && charState is not SigmaSlashStateGroundWC
			or SigmaSlashStateDashWC or SigmaSlashStateAirWC
			) {
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
		if (player.weapon is not AssassinBulletChar) {
			if (player.input.isAPressed(player)) {
				attackPressed = true;
				lastAttackFrame = Global.level.frameCount;
			}
		}
		framesSinceLastAttack = Global.level.frameCount - lastAttackFrame;
		bool lenientAttackPressed = (attackPressed || framesSinceLastAttack < 5);

		if (charState is Dash or AirDash or WallSlide or LadderClimb) {
			if (player.input.isBPressed(player) &&
				flag == null
			) {
				changeState(new SigmaWallDashStateWC(-1, true), true);
				return true;
			}
		}
		if (lenientAttackPressed && saberCooldown == 0) {
			saberCooldown = 0;

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
			if (grounded) {
				if (isDashing && sprite.name != getSprite("dash_end") && dashSlashCooldown <= 0) {
					slideVel = getDashSpeed() * 0.7f * xDir;
					dashSlashCooldown = 0;
					changeState(new SigmaSlashStateDashWC(), true);
					return true;
				}
				if (player.input.isHeld(Control.Down, player)) {
					changeState(new SigmaSlashStateGround2WC(), true);
				} else {
					changeState(new SigmaSlashStateGroundWC(), true);
				}
				return true;
			}
			changeState(new SigmaSlashStateAirWC(), true);
			return true;
		}
		if (grounded && charState is Idle || charState is Run || charState is Crouch) {
			if (player.input.isHeld(Control.Special1, player) && ballWeapon.ammo > 0) {
				sigmaAmmoRechargeCooldown = 0.5f;
				changeState(new SigmaBallShootWC(), true);
				return true;
			}
		}
		if (grounded) {
			if (player.input.isR2Pressed(player) && player.superAmmo > 15) {
				player.superAmmo -= 16;
				if (player.input.isHeld(Control.Up, player)) {
					changeState(new VirusSlash2(), true);
				} else {
					changeState(new HellGaze(), true);
				}
				return true;
			}
		}

		if (player.input.isPressed(Control.Dash, player)
		&& player.input.checkDoubleTap(Control.Dash) && sigDodgeCooldown == 0) {
				changeState(new SigDodge(), true);
			sigDodgeCooldown = 0.5f;
				return true;
		}


		return base.attackCtrl();
	}

	public override Collider getBlockCollider() {
		Rect rect = Rect.createFromWH(0, 0, 16, 35);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override string getSprite(string spriteName) {
		return "sigma1alt_" + spriteName;
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Guard,
		AutoGuard,
		GenericSlash,
		ViralSlash,
		HellGaze,
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"sigma1alt_hellgaze" => MeleeIds.HellGaze,
			"sigma1alt_slash_1" or "sigma1alt_slash_2" or "sigma1alt_slash_3" => MeleeIds.ViralSlash,
			"sigma1alt_block" => MeleeIds.Guard,
			"sigma1alt_block_auto" => MeleeIds.AutoGuard,
			"sigma1alt_ladder_attack" or "sigma1alt_wall_slide_attack" or "sigma1alt_attack" or "sigma1alt_attack_crouch" or "sigma1alt_attack_dash" => MeleeIds.GenericSlash,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Guard => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true, addToLevel: addToLevel
			) {
				highPiority = true
			},
			MeleeIds.AutoGuard => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true, addToLevel: addToLevel
			) {
				highPiority = true
			},
			MeleeIds.GenericSlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSlash, player, 2, 20,
				addToLevel: addToLevel
			),
			MeleeIds.HellGaze => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.ElectricShock, player, 2, 0, 5,
				addToLevel: addToLevel
			),
			MeleeIds.ViralSlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaViralSlash, player, 3, 30, 5,
				addToLevel: addToLevel
			),
			_ => null
		};
	}

	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return ballWeapon.ammo < ballWeapon.maxAmmo;
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add((byte)MathF.Ceiling(ballWeapon.ammo));

		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];

		// Per-player data.
		ballWeapon.ammo = data[0];
	}

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
				&& !isInvulnerable() && !(charState is CallDownMaverick or SigmaSlashStateGround)
				&& aiAttackCooldown <= 0) {
				switch (Sattack) {
					case 0 when isTargetClose:
						changeState(new SigmaSlashStateGround(), true);
						break;
					case 1 when isTargetInAir:
						changeState(new SigmaBallShoot(), true);
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
	public override void aiUpdate(Actor? target) {
		if (charState is Die) {
			foreach (Weapon weapon in weapons) {
				if (weapon is MaverickWeapon mw && mw.maverick != null) {
					mw.maverick.changeState(new MExit(mw.maverick.pos, true), true);
				}
			}
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

	public override void render(float x, float y) {



		// For drawing the growing aura that LastStand and Eigengrau Zero uses.
		if (visible && OverDrive) {
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
				player.omegaAuraShaderPurple
			);
			updateOmegaAura();
		}


		if (player.isMainPlayer && overDriveTimer > 0) {
			float healthPct = Helpers.clamp01((12 - overDriveTimer) / 12);
			float sy = -27;
			float sx = 20;
			if (xDir == -1) sx = 90 - 20;
			drawFuelMeter(healthPct, sx, sy);
		}
		base.render(x, y);
	}


}