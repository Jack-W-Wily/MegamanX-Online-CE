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
		charId = CharIds.Sigma;
		spriteFrameToSounds["sigma1alt_run/2"] = "sigmawalk";
		spriteFrameToSounds["sigma1alt_run/7"] = "sigmawalk";
	}


	public bool isPup;
	public bool isStk;
	public bool isTag;
	public bool isSum;


	public float maverickHomingAttackCD;
	
	public override void update() {
		base.update();

		Helpers.decrementTime(ref maverickHomingAttackCD);

		// Supers

		if (!isInDamageSprite()) {
				
					if (downPressedTimes > 3 && player.input.isL2Pressed(player) && player.superAmmo > 15) {
						player.superAmmo -= 16;	
						changeState(new ZainParryShinStartState(), true);
					}
					if (player.input.checkShoryuken(player, xDir, Control.Special1) && player.superAmmo > 15) {
						// Combo Complexo
						player.superAmmo -= 16;
						changeState(new VirusSlash2(), true);
					}
					if (player.input.checkShoryuken(player, xDir, Control.Shoot) && player.superAmmo > 15) {
						if (OverDrive) {
							changeState(new HellGazeEX(), true);
						} else {
							changeState(new HellGaze(), true);
						}
						player.superAmmo -= 16;
					}
				
		}


		foreach (var maverick in player.mavericks) {
			

	
			if (maverickHomingAttackCD == 0 && player.input.isR2Pressed(player) 
			&& player.weapon is SigmaMenuWeapon


			
			&& player.mavericks.Count > 0 && !maverick.isAttacking() ) {
				MaverickState mState;
				if (player.input.isHeld(Control.Up, player)) {
					mState = maverick.strikerStates()[2];
					maverick.changeState(mState, true);
				} else if (player.input.isHeld(Control.Down, player)) {
					mState = maverick.strikerStates()[1];
					maverick.changeState(mState, true);
				} else {
					mState = maverick.strikerStates()[0];
					maverick.changeState(mState, true);
					
				}
				foreach (var otherPlayer in Global.level.players) {
					if (otherPlayer.character == null) continue;
					if (otherPlayer == player) continue;
					if (otherPlayer == parasiteDamager?.owner) continue;
					if (otherPlayer.character.isInvulnerable()) continue;
					if (Global.level.gameMode.isTeamMode && otherPlayer.alliance != player.alliance) continue;
					if (otherPlayer.character.getCenterPos().distanceTo(getCenterPos()) > ParasiticBomb.carryRange) continue;
					Character target = otherPlayer.character;
					if (target.pos.x < 0) {
						maverick.xDir = -1;
					} else if (target.pos.x > 0) {
						
						maverick.xDir = 1;
					}
					break;
				}

			}
		}
		
		if (player.loadout.sigmaLoadout.commandMode == (int)MaverickModeId.Puppeteer){
			isPup = true;
		} else {
			isPup = false;
		}
		if (player.loadout.sigmaLoadout.commandMode == (int)MaverickModeId.Striker){
			isStk = true;
		} else {
			isStk = false;
		}
		if (player.loadout.sigmaLoadout.commandMode == (int)MaverickModeId.TagTeam){
			isTag = true;
		} else {
			isTag = false;
		}
		if (player.loadout.sigmaLoadout.commandMode == (int)MaverickModeId.Summoner){
			isSum = true;
		} else {
			isSum = false;
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

		chargeLogic(shoot);
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
		bool lenientAttackPressed = player.input.isAPressed(player);

		if (charState is Dash or AirDash or WallSlide or LadderClimb) {
				if (player.input.isBPressed(player) &&
					flag == null
				) {
					if (charState is WallSlide) {
						xDir = -xDir;
					}
					if (player.input.isHeld(Control.Down, player)) {
							changeState(new SigmaWallDashStateWC(1, true), true);
					} else {
						changeState(new SigmaWallDashStateWC(-1, true), true);
					}
		
					return true;
				}
			}


			if (grounded) {

				if (player.input.isL2Held(player)) {
					if (player.input.isHeld(Control.Down, player)) {
					changeState(new GlobalParryState(), true);
					return true;
					}
					else if (player.input.isAPressed(player)) {
						changeState(new SigmaGrabStart(), true);
						
						return true;
					}else {
						if (charState is not BlockWCUT) {
						changeState(new BlockWCUT(), true);
						return true;
						}
					}
				
				}
				
				}

		if (isSum){
			
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
					
					changeState(new SigmaSlashStateAirWC(), true);
					
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
			

			if (player.input.isPressed(Control.Dash, player)
			&& player.input.checkDoubleTap(Control.Dash) && sigDodgeCooldown == 0) {
					changeState(new SigDodge(), true);
				sigDodgeCooldown = 0.5f;
					return true;
			}
		}
		if (isPup){
			
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
					if (player.input.isHeld(Control.Up, player)) {
						changeState(new SigmaSlashStateGround3WC(), true);
					} 
					else if (player.input.isHeld(Control.Down, player)) {
						changeState(new SigmaSlashStateGround2WC(), true);
					} else {
						changeState(new SigmaSlashStateGroundWC(), true);
					}
					return true;
				}
				changeState(new SigmaSlashStateAirWC(), true);
				return true;
			}
			
				if (player.input.isHeld(Control.Special1, player) && ballWeapon.ammo > 0) {
					sigmaAmmoRechargeCooldown = 0.5f;
					changeState(new HeavySlash2(), true);
					return true;
				}
			
			

			if (player.input.isPressed(Control.Dash, player)
			&& player.input.checkDoubleTap(Control.Dash) && sigDodgeCooldown == 0) {
					changeState(new SigDodge(), true);
				sigDodgeCooldown = 0.5f;
					return true;
			}
		}
		if (isStk) {
			if (player.input.isAPressed(player)) {
				shoot(0);
			}
			if (player.input.isBPressed(player)) {
				changeState(new SigmaSlashStateAirWC(), true);
			}
		}
		if (isTag) {



		// Shoot button attacks.
		if (lenientAttackPressed ) {
			if (player.input.isHeld(Control.Up, player) && flag == null && grounded) {
				
					changeState(new SigmaUpDownSlashState(true), true);
				
				return true;
			} else if (player.input.isHeld(Control.Down, player)) {
				
					changeState(new SigmaUpDownSlashState(false), true);
				
				return true;
			}
			

			if (charState is WallSlide || charState is LadderClimb) {
				if (charState is LadderClimb) {
					int inputXDir = player.input.getXDir(player);
					if (inputXDir != 0) {
						xDir = inputXDir;
					}
				}
				changeSprite(getSprite(charState.attackSprite), true);
				playSound("sigma2slash", sendRpc: true);
				return true;
			}
			if (charState is Dash) {
				slideVel = xDir * getRunSpeed() * 1.65f;
				changeState(new Sigma2DashSlashState(), true);
			} else {
				changeState(new SigmaClawState(charState, !grounded), true);
			}
			return true;
		}
		if (grounded && player.input.isPressed(Control.Special1, player) &&
			flag == null && player.superAmmo >= 8
		) {
			if (!player.input.isHeld(Control.Up, player))  {
				player.superAmmo -= 8;
				changeState(new SigmaElectricBallState(), true);
				return true;
			} 
			if (player.input.isHeld(Control.Up, player) && player.superAmmo >= 16) {
				player.superAmmo -= 16;
				changeState(new SigmaElectricBall2StateEX(), true);
				return true;
			}
		}

		}


		return base.attackCtrl();
	}



	public override bool canCharge() {
		return alive && isStk;
	}


	
	public override void chargeLogic(Action<int> shoot) {
		if (chargeButtonHeld() && flag == null ) {
			if (canCharge()) {
				increaseCharge();
			}
		} else if (canShoot()) {
			int chargeLevel = getChargeLevel();
			if (isCharging()) {
				if (chargeLevel >= 1) {
					shoot(chargeLevel);
				}
			}
			stopCharge();
		}
		chargeGfx();
	}

	public List<Projectile> acerolasOnField = new();
	public void shoot(int chargeLevel) {
		if (chargeLevel != 3) {
			for (int i = acerolasOnField.Count - 1; i >= 0; i--) {
				if (acerolasOnField[i].destroyed || acerolasOnField[i].reflectCount > 0) {
					acerolasOnField.RemoveAt(i);
				}
			}
			if (acerolasOnField.Count >= 3) { return; }
			shootProjectiles();
			ammoReduction();
		}
		if (chargeLevel == 3) {
			shootSkull();
			ammoReduction();
		}
		
	}


	public void shootSkull() {
		playSound("Ridearmor - Shot", sendRpc: true);
		Point shootPos = getFirstPOI() ?? getCenterPos();
		angleShoot();
		SigmaBallsProjHead = new SigmaSkull(
					shootPos, xDir, this,
					player, player.getNextActorNetId(), 0, rpc: true
					);
		anim = new Anim(shootPos, "sigma_proj_ball_muzzle", xDir,
			player.getNextActorNetId(), true, sendRpc: true);
		SigmaBallsProjHead.maxSpeed = 350;
		SigmaBallsProjHead.damager.damage = 4;
	}

	public virtual bool chargeButtonHeld() {
		if (isStk && player.input.isAHeld(player)) {
			return true;
		}
		return false;
	}
	public int ShootAngle = 0;
	
	public void angleShoot() {
		
		if (xDir == 1) {
			if (player.input.isHeld(Control.Down, player)) {
				ShootAngle = 42;
			} else if (player.input.isHeld(Control.Up, player)) {
				ShootAngle = 216;
			} else {
				ShootAngle = 8;
			}
		} else if (xDir == -1) {
			if (player.input.isHeld(Control.Down, player)) {
				ShootAngle = 94;
			} else if (player.input.isHeld(Control.Up, player)) {
				ShootAngle = 164;
			} else {
				ShootAngle = 120;
			}
		}
	}


	public SigmaSkull? SigmaBallsProjHead;
	public Anim? anim;
	public void shootProjectiles() {
		playSound("energyBall", sendRpc: true);
		Point shootPos = getFirstPOI() ?? getCenterPos();
		angleShoot();
		var acerola = new SigmaBallProj
		(
			shootPos, 1, ShootAngle, this,
			player, player.getNextActorNetId(), rpc: true
		);
		acerolasOnField.Add(acerola);
		anim = new Anim(shootPos, "sigma_proj_ball_muzzle", xDir,
			player.getNextActorNetId(), true, sendRpc: true);
	}

	
	public void ammoReduction() {
		ballWeapon.addAmmo(-4, player);
		sigmaAmmoRechargeCooldown = sigmaHeadBeamTimeBeforeRecharge;
	}


	public override Collider getBlockCollider() {
		Rect rect = Rect.createFromWH(0, 0, 16, 35);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override string getSprite(string spriteName) {
		if (isStk && Global.sprites.ContainsKey("sigma_" + spriteName)) {
			return "sigma_" + spriteName;
		}
		if (isTag && Global.sprites.ContainsKey("sigma2_" + spriteName)) {
			return "sigma2_" + spriteName;
		}
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

		Parry,

		Grab,

		Sigkick,

		HeavySlash,

		DashSlash,

		KiriOrochi,

		StandingSlash,
		Uppercut,

		Slash1,
		Slash2,
		
		AirSlash,
		UpSlash,
		DownSlash,
		LadderSlash,
		WallSlash,
		Throw,
		GigaAttackSlash

	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"sigma1alt_parry" => MeleeIds.Parry,
			"sigma1alt_hellgaze" => MeleeIds.HellGaze,
			"sigma1alt_slash_1 _virus" or "sigma1alt_slash_2 _virus" or "sigma1alt_slash_3 _virus" => MeleeIds.ViralSlash,
			"sigma1alt_slash_1" or "sigma1alt_slash_2" or "sigma1alt_slash_3" => MeleeIds.HeavySlash,
			"sigma1alt_attack_dash" => MeleeIds.DashSlash,
			"sigma1alt_block" => MeleeIds.Guard,
			"sigma1alt_block_auto" => MeleeIds.AutoGuard,
			"sigma1alt_attack" => MeleeIds.StandingSlash,
			"sigma1alt_ladder_attack" or "sigma1alt_wall_slide_attack"=> MeleeIds.GenericSlash,
			"sigma1alt_attack_crouch" => MeleeIds.KiriOrochi,
			"sigma1alt_grab_start" => MeleeIds.Grab,
			"sigma1alt_grab_kick" => MeleeIds.Sigkick,
			"sigma1alt_uppercut" => MeleeIds.Uppercut,
			"sigma1alt_throw_start" => MeleeIds.Throw,


			"sigma_attack" or "sigma_attack_air"  => MeleeIds.DashSlash,



			"sigma2_attack" => MeleeIds.Slash1,
			"sigma2_attack2" => MeleeIds.Slash2,
			"sigma2_attack_air" => MeleeIds.AirSlash,
			"sigma2_attack_dash" => MeleeIds.DashSlash,
			"sigma2_upslash" => MeleeIds.UpSlash,
			"sigma2_downslash" => MeleeIds.DownSlash,
			"sigma2_ladder_attack" => MeleeIds.LadderSlash,
			"sigma2_wall_slide_attack" => MeleeIds.WallSlash,
			"sigma2_shoot2" => MeleeIds.GigaAttackSlash,


			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Guard => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true, addToLevel: addToLevel, hitspark : "empty"
			) {
				highPiority = true
			},
			MeleeIds.AutoGuard => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true, addToLevel: addToLevel, hitspark : "empty"
			) {
				highPiority = true
			},
			MeleeIds.StandingSlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSlash, player, 2, Global.defFlinch,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			MeleeIds.GenericSlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSlash, player, 2, Global.defFlinch,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			MeleeIds.Uppercut => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.BlockableWeakLaunch, player, 2, 0,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			MeleeIds.DashSlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.HeavyPush, player, 2, 0,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			MeleeIds.KiriOrochi => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.Shippuuga, player, 2, 0,
				addToLevel: addToLevel, hitSound : "htsnd_slash1"
			),
			MeleeIds.HellGaze => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.ElectricShock, player, 2, 0, 5,
				addToLevel: addToLevel, hitSound : "kofhtsnd_lightning1"
			),
			MeleeIds.ViralSlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaViralSlash, player, 4, 30, 5,
				addToLevel: addToLevel, hitSound : "htsnd_slash_deep3"
			),
			MeleeIds.HeavySlash => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.SigmaSlash, player, 2, 30, 5,
				addToLevel: addToLevel, hitSound : "htsnd_slash_deep3"
			),
			MeleeIds.Parry => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.HeavyPush, player, 6, 0, 5,
				addToLevel: addToLevel
			),
			MeleeIds.Grab => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.ForceGrabState, player, 0, 0,
				addToLevel: addToLevel, hitSound : "kofhtsnd_grab2"
			),
			MeleeIds.Sigkick => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.BlockableLaunch, player, 4, 0,
				addToLevel: addToLevel, hitSound : "kofhtsnd_knock1"
			),


			MeleeIds.Throw => new GenericMeleeProj(
				SigmaSlashWeapon.netWeapon, pos, ProjIds.BurensenEND, player, 4, 0,
				addToLevel: addToLevel, hitSound : "kofhtsnd_knock1"
			),


			MeleeIds.Slash1 => new GenericMeleeProj(
				SigmaClawWeapon.netWeapon, pos, ProjIds.Sigma2Claw, player,
				2, Global.halfFlinch, 12, addToLevel: addToLevel, clashTier: ClashTier.Medium
			),
			MeleeIds.Slash2 => new GenericMeleeProj(
				SigmaClawWeapon.netWeapon, pos, ProjIds.Sigma2Claw2, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel, clashTier: ClashTier.Medium
			),
			MeleeIds.AirSlash or MeleeIds.DashSlash => new GenericMeleeProj(
				SigmaClawWeapon.netWeapon, pos, ProjIds.Sigma2Claw, player,
				3, Global.defFlinch, 22, addToLevel: addToLevel, clashTier: ClashTier.Medium
			),
			MeleeIds.UpSlash or MeleeIds.DownSlash => new GenericMeleeProj(
				SigmaClawWeapon.netWeapon, pos, ProjIds.Sigma2UpDownClaw, player,
				3, Global.superFlinch, 30, addToLevel: addToLevel
			),
			MeleeIds.WallSlash or MeleeIds.LadderSlash => new GenericMeleeProj(
				SigmaClawWeapon.netWeapon, pos, ProjIds.Sigma2Claw, player,
				3, Global.defFlinch, 15, addToLevel: addToLevel
			),
			MeleeIds.GigaAttackSlash => new GenericMeleeProj(
				new NeoSigmaGigaAttackWeapon(), pos, ProjIds.Sigma2Ball2, player,
				6, Global.defFlinch, 15, addToLevel: addToLevel
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

	
	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;



		if (player.skinSlot == 1) {
			palette = player.nightmareZeroShader2;
		}
		if (player.skinSlot == 2) {
			palette = player.sigmaPal2;
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

						player.changeWeaponSlot(0);
						break;
					case 2 when isFacingTarget :
						changeState(new SigmaSlashStateGroundWC());
						break;
					case 3 when isFacingTarget :
						changeState(new SigmaSlashStateGround2WC());
						break;
					case 4 when isFacingTarget :
						changeState(new SigmaWallDashStateWC(-1, true), true);
						break;
					case 5 when isFacingTarget:
						changeState(new BlockWCUT(), true);
						break;
					case 6 when isFacingTarget:
						changeState(new VirusSlash2());
						break;
					case 7 when isFacingTarget:
						changeState(new HellGaze());
						break;
				}
			}

			



			if (!isTargetClose && isWishinRangedMoves && grounded) {
				switch (Vattack) {
				case 1 when isFacingTarget:

							player.changeWeaponSlot(0);
						break;
					case 2 when isFacingTarget:
							changeState(new SigmaBallShootWC());
						
						break;
					case 3 when isFacingTarget :
							changeState(new SigmaWallDashStateWC(-1, true), true);
						break;
					case 4 when isFacingTarget :
						changeState(new SigmaBallShootWCEnhanced());
						break;
					case 5 when isFacingTarget :
						changeState(new ZainParryShinStartState(), true);
						addHealth(10);
						break;
					case 6 when isFacingTarget :
						changeState(new ZainParryShinStartState(), true);
		
						break;
					case 7 when isFacingTarget :
						changeState(new HellGazeEX(), true);
		
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
			(charState.attackCtrl || charState is HellGaze or PopcornHell)) {
				//Projectile is not 
				if (!(proj.projId == (int)ProjIds.RollingShieldCharged || proj.projId == (int)ProjIds.RollingShield
					|| proj.projId == (int)ProjIds.MagnetMine || proj.projId == (int)ProjIds.FrostShield || proj.projId == (int)ProjIds.FrostShieldCharged
					|| proj.projId == (int)ProjIds.FrostShieldAir || proj.projId == (int)ProjIds.FrostShieldChargedPlatform || proj.projId == (int)ProjIds.FrostShieldPlatform)
				) {
					if (grounded) {
						if (aiDodgeCD == 0 && !isDashing) {
							
								changeState(new SigDodge(), true);
							
							aiDodgeCD = Helpers.randomRange(0, 30);

						}
					}
				}
			}
		}

		base.aiDodge(target);
	}

}

