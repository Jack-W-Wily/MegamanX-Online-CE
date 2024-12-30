using System;
using System.Collections.Generic;

namespace MMXOnline;

public class PunchyZero : Character {
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


	//buster stuff
	public int stockedBusterLv;
	public bool stockedSaber;

	// Weapons.
	public PunchyZeroMeleeWeapon meleeWeapon = new();
	public PZeroParryWeapon parryWeapon = new();
	public Weapon gigaAttack;
	public AwakenedAura awakenedAuraWeapon = new();
	public ZSaber saberSwingWeapon = new();
	public ZeroBuster busterWeapon = new();
	public int gigaAttackSelected;
	
	// Inputs.
	public int shootPressTime;
	public int parryPressTime;
	public int swingPressTime;
	public int specialPressTime;

	// Cooldowns.
	public float dashAttackCooldown;
	public float diveKickCooldown;
	public float uppercutCooldown;
	public float parryCooldown;
	public float hadangekiCooldown;
	public float genmureiCooldown;

	// Hypermode stuff.
	public float donutTimer = 0;
	public int donutsPending = 0;
	public int freeBusterShots = 0;

	// Creation code.
	public PunchyZero(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer, bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.PunchyZero;
		// Loadout stuff.
		PZeroLoadout pzeroLoadout = player.loadout.pzeroLoadout;

		gigaAttackSelected = pzeroLoadout.gigaAttack;
		gigaAttack = pzeroLoadout.gigaAttack switch {
			1 => new CFlasher(),
			2 => new RekkohaWeapon(),
			_ => new RakuhouhaWeapon(),
		};
		hyperMode = pzeroLoadout.hyperMode;
		spriteFrameToSounds["zerox1_run/4"] = "run";
		spriteFrameToSounds["zerox1_run/8"] = "run";
	}

	public override void update() {

		if (stockedBusterLv > 0 && player.input.isPressed(Control.Shoot,player)
		&& charState is not PZeroDoubleBuster){
				changeState(new PZeroDoubleBuster(true, true), true);
		}

			if (stockedBusterLv > 0 || stockedSaber) {
			var renderGfx = stockedBusterLv switch {
				_ when stockedSaber || stockedBusterLv == 2 => RenderEffectType.ChargeGreen,
				1 => RenderEffectType.ChargePink,
				2 => RenderEffectType.ChargeOrange,
				_ => RenderEffectType.ChargeBlue
			};
			addRenderEffect(renderGfx, 0.033333f, 0.1f);
		}


		if (charState is WarpIn){
		freeBusterShots = 3;
		}

		if (isAwakened || isBlack) {
			updateAwakenedAura();
		}
		if (!Global.level.isHyper1v1()) {
			if (isBlack) {
				if (musicSource == null) {
					addMusicSource("zero_X2", getCenterPos(), true);
				}
			} else if (isAwakened) {
				if (musicSource == null) {
					addMusicSource("CannonBall_megasfc", getCenterPos(), true);
				}
			} else if (isViral && ownedByLocalPlayer) {
				if (musicSource == null) {
					addMusicSource("zero_X1", getCenterPos(), true);
				}
			} else {
				destroyMusicSource();
			}
		}
		if (!ownedByLocalPlayer) {
			base.update();
			return;
		}


		if (charState is Crouch){
				if (gigaAttack.ammo >= 28 &&
				player.input.isPressed(Control.WeaponRight, player) ) {
					gigaAttack.addAmmo(-28, player);			
					changeState(new ZeroInferno(), true);	
					
		}
		}
		// Local update starts here.
		inputUpdate();
		Helpers.decrementFrames(ref donutTimer);
		Helpers.decrementFrames(ref hadangekiCooldown);
		Helpers.decrementFrames(ref parryCooldown);
		Helpers.decrementFrames(ref dashAttackCooldown);
		Helpers.decrementFrames(ref diveKickCooldown);
		Helpers.decrementFrames(ref uppercutCooldown);
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
					heal(player, awakenedPhase, true);
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
				scrapDrainCounter = 220;
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
		if (isAwakened) {
			chargeLogic(shootDonuts);
		} else {
			chargeLogic(shoot);
		}
	}

	public override bool canCharge() {
		return (player.currency > 0 || freeBusterShots > 0) && donutsPending == 0 && !isInvulnerableAttack();
	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Shoot, player);
	}

	public void setShootAnim() {
		string shootSprite = getSprite(charState.shootSprite);
		if (!Global.sprites.ContainsKey(shootSprite)) {
			if (grounded) { shootSprite = "zerox1_shoot"; }
			else { shootSprite = "zerox1_fall_shoot"; }
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
			currencyUse = 1;
			if (!isViral){
			playSound("buster4", sendRpc: true);
			new ZBuster4Proj(
				shootPos, xDir, 0, player, player.getNextActorNetId(), rpc: true
			);
			}
			if (isViral)changeState(new PZeroDoubleBuster(false, true), true);
			
			
				
		}
		if (currencyUse > 0) {
			if (freeBusterShots > 0) {
				freeBusterShots--;
			} else if (player.currency > 0) {
				player.currency--;
			}
		}
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
			if (player.currency > 0) {
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

	public void inputUpdate() {
		if (shootPressTime > 0) {
			shootPressTime--;
		}
		if (specialPressTime > 0) {
			specialPressTime--;
		}
		if (parryPressTime > 0) {
			parryPressTime--;
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
		if (player.input.isPressed(Control.WeaponLeft, player) ||
			player.input.isPressed(Control.WeaponRight, player) && !isAwakened
		) {
			parryPressTime = 6;
		}
		if (player.input.isPressed(Control.WeaponRight, player) && isAwakened) {
			swingPressTime = 6;
		}
	}

	public override bool normalCtrl() {
		// Hypermode activation.

		if (charState.attackCtrl && charState is not Dash && grounded && 
				player.input.isHeld(Control.Up, player) )
			 {
			turnToInput(player.input, player);
			changeState(new SwordBlock());
			return true;
		}

		int cost = Player.zeroHyperCost;
		if (isAwakened) {
			cost = 4;
		}
		if (player.currency >= cost &&
			player.input.isHeld(Control.Special2, player) &&
			charState is not HyperZeroStart and not WarpIn && (
				!isViral && !isAwakened && !isBlack ||
				isAwakened && !hyperOvertimeActive
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
			changeState(new HyperPunchyZeroStart(), true);
			return true;
		}
		// Regular states.
		return base.normalCtrl();
	}

	public override bool attackCtrl() {
		if (donutsPending != 0) {
			return false;
		}
		if (isAwakened && swingPressTime > 0 && hadangekiCooldown == 0) {
			hadangekiCooldown = 60;
			if (charState is WallSlide wallSlide) {
				changeState(new PunchyZeroHadangekiWall(wallSlide.wallDir, wallSlide.wallCollider), true);
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
			return true;
		}
		if (grounded && vel.y >= 0) {
			return groundAttacks();
		}
		return airAttacks();
	}


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

	public override bool altCtrl(bool[] ctrls) {
		if (charState is PZeroGenericMeleeState zgms) {
			return zgms.altCtrlUpdate(ctrls);
		}
		return false;
	}

	public bool groundAttacks() {
		if (parryPressTime > 0 && parryCooldown == 0) {
			changeState(new PZeroParry(), true);
			return true;
		}
		int yDir = player.input.getYDir(player);
		if (isDashing && dashAttackCooldown == 0 &&
			player.input.getYDir(player) == 0 && shootPressTime > 0
		) {
			changeState(new PZeroSpinKick(), true);
			return true;
		}
		if (shootPressTime > 0) {
			if (yDir == -1) {
				changeState(new PZeroShoryuken(), true);
				return true;
			}
			if (yDir == 1) {
				
					changeState(new PZeroYoudantotsu(), true);
				
				return true;
			}
			if (grounded && isDashing) {
				slideVel = xDir * getDashSpeed() * 0.8f;
			}
			changeState(new PZeroPunch1(), true);
			return true;
		}
		if (specialPressTime > 0) {
			return groundSpcAttacks();
		}
		return false;
	}

	public bool airAttacks() {
		int yDir = player.input.getYDir(player);
		if (//diveKickCooldown == 0 && 
		(shootPressTime > 0 || specialPressTime > 0) && yDir == 1) {
			changeState(new PZeroDiveKickState(), true);
			return true;
		}
	
		if (shootPressTime > 0) {
			changeState(new PZeroKick(), true);
		}
		return false;
	}

	public bool groundSpcAttacks() {
		int yDir = player.input.getYDir(player);
		if (yDir == -1) {
			changeState(new ZeroBurnKnuckle(), true);
			return true;
		}
		if (yDir == 1) {
			if (gigaAttack.shootCooldown > 0 || gigaAttack.ammo < gigaAttack.getAmmoUsage(0)) {
				return false;
			}
			if (!isAwakened){

					if (isViral){
					changeState(new Rakuhouha(new ShinMessenkou()), true);	
					}else{
					changeState(new ZeroRocks(new FakeZeroWeapon(player)), true);
					}
			} else {
			changeState(new Rakuhouha(new ShinMessenkou()), true);		
			}
				gigaAttack.addAmmo(-10, player);
			
			return true;
		}
		if (isDashing) {
			slideVel = xDir * getDashSpeed() * 0.8f;
		}
		changeState(new PunchyZeroHadangeki(), true);
		return true;
	}

	public override bool canAirJump() {
		return dashedInAir == 0 || isBlack;
	}

	public override string getSprite(string spriteName) {
		return "zerox1_" + spriteName;
	}

	public override bool isToughGuyHyperMode() {
		return isBlack || isGenmuZero;
	}
	public override bool canShoot() {
	//	if (isInvulnerableAttack()) return false;
		return true;//base.canShoot();
	}

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

	public override Projectile? getProjFromHitbox(Collider hitbox, Point centerPoint) {
		int meleeId = getHitboxMeleeId(hitbox);
		if (meleeId == -1) {
			return null;
		}
		Projectile? proj = getMeleeProjById(meleeId, centerPoint);
		if (proj != null) {
			proj.meleeId = meleeId;
			proj.owningActor = this;
			return proj;
		}
		return null;
	}

	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"zerox1_punch" => MeleeIds.Punch,
			"zerox1_punch2" => MeleeIds.Punch2,
			"zerox1_spinkick" => MeleeIds.Spin,
			"zerox1_kick_air" => MeleeIds.AirKick,
			"zerox1_parry_start" => MeleeIds.Parry,
			"zerox1_parry" => MeleeIds.ParryAttack,
			"zerox1_shoryuken" or "zerox1_pipe_rising" => MeleeIds.Uppercut,
			"zerox1_megapunch" => MeleeIds.StrongPunch,
			"zerox1_dropkick" => MeleeIds.DropKick,
			"zerox1_burnknuckle" => MeleeIds.BurnKnUckle,
			"zerox1_block" => MeleeIds.Gokumonken,

			"zerox1_pipe_attack" or "zerox1_projswing" or "zerox1_projswing_air" or "zerox1_wall_slide_attack" => MeleeIds.SaberSwing,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Punch => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.PZeroPunch, player,
				2, Global.halfFlinch, 0.25f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Punch2 => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.PZeroPunch2, player, 2, Global.halfFlinch, 0.25f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Spin => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.PZeroSenpuukyaku, player, 2, Global.halfFlinch, 0.5f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.AirKick => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.PZeroAirKick, player, 2, Global.halfFlinch, 0.25f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Uppercut => new GenericMeleeProj(
				ZeroShoryukenWeapon.staticWeapon, projPos, ProjIds.PZeroShoryuken, player, 3, Global.defFlinch, 0.5f,
				ShouldClang : true,
				isJuggleProjectile : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.StrongPunch => new GenericMeleeProj(
				MegaPunchWeapon.staticWeapon, projPos, ProjIds.PZeroYoudantotsu, player, 4, 0, 0.5f,
				addToLevel: addToLevel
			),
			(int)MeleeIds.BurnKnUckle => new GenericMeleeProj(
				MegaPunchWeapon.staticWeapon, projPos, ProjIds.PZeroEnkoukyaku, player, 4, 20, 0.5f,
				addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.DropKick => new GenericMeleeProj(
				DropKickWeapon.staticWeapon, projPos, ProjIds.PZeroEnkoukyaku, player, 2, 30, 0.5f,
				ShouldClang : true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Parry => new GenericMeleeProj(
				parryWeapon, projPos, ProjIds.PZeroParryStart, player, 0, 0, 0,
				addToLevel: addToLevel
			),
			(int)MeleeIds.ParryAttack => (new GenericMeleeProj(
				parryWeapon, projPos, ProjIds.PZeroParryAttack, player, 4, Global.defFlinch, 0.5f,
				ShouldClang : true,
				addToLevel: addToLevel
			) {
				netcodeOverride = NetcodeModel.FavorDefender
			}),
			(int)MeleeIds.AwakenedAura => (new GenericMeleeProj(
				awakenedAuraWeapon, projPos, ProjIds.AwakenedAura, player, 0, 0, 0.5f,
				addToLevel: addToLevel
			) {
				netcodeOverride = NetcodeModel.FavorDefender
			}),
			(int)MeleeIds.SaberSwing => new GenericMeleeProj(
				saberSwingWeapon, projPos, ProjIds.ZSaberProjSwing, player,
				3, Global.defFlinch, 0.5f, isReflectShield: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Gokumonken => new GenericMeleeProj(
				meleeWeapon, projPos, ProjIds.SwordBlock, player, 0, 0, 0, isDeflectShield: true,
				addToLevel: addToLevel
			),
			_ => null
		};


		

		return proj;
	}

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
						ProjIds.AwakenedAura, player, damage, flinch, 0.5f
					) {
						globalCollider = globalCollider.clone(),
						meleeId = (int)MeleeIds.AwakenedAura
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

	public enum MeleeIds {
		None = -1,
		Punch,
		Punch2,
		Spin,
		StrongPunch,
		AirKick,
		Uppercut,
		DropKick,
		BurnKnUckle,

		Parry,
		ParryAttack,
		SaberSwing,
		Gokumonken,
		AwakenedAura
	}

	// For parry purposes.
	public override void onCollision(CollideData other) {
		if (charState.specialId == SpecialStateIds.PZeroParry &&
			other.gameObject is Projectile proj &&
			proj.damager?.owner?.teamAlliance != player.teamAlliance &&
			charState is PZeroParry zeroParry &&
			proj.damager?.damage > 0 &&
			zeroParry.canParry(proj, proj.projId)
		) {
			zeroParry.counterAttack(proj.owner, proj);
			return;
		}
		base.onCollision(other);
	}

	public override void addAmmo(float amount) {
		gigaAttack.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		gigaAttack.addAmmoPercentHeal(amount);
	}

	public override bool canAddAmmo() {
		return (gigaAttack.ammo < gigaAttack.maxAmmo);
	}

	public override float getRunSpeed() {
		float runSpeed = Physics.WalkSpeed;
		if (isBlack) {
			runSpeed *= 1.15f;
		}
		return runSpeed * getRunDebuffs();
	}

	public override float getLabelOffY() {
		if (sprite.name.Contains("_ra_")) {
			return 25;
		}
		return 45;
	}

	public override void render(float x, float y) {
		if (isViral && visible) {
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
			string auraSprite = "zero_awakened_aura";
			if (sprite.name.Contains("dash")) {
				auraSprite = "zero_awakened_aura2";
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
			Global.sprites[auraSprite].draw(
				awakenedAuraFrame,
				pos.x + x + (xOff * auraXDir),
				pos.y + y + yOff, auraXDir,
				1, null, auraAlpha, 1, 1,
				zIndex - 1, shaders: shaders
			);
		}
		if (isBlack && visible) {
			float xOff = 0;
			int auraXDir = 1;
			float yOff = 5;
			string auraSprite = "zero_absolutewing3";
			if (sprite.name.Contains("dash")) {
				auraSprite = "zero_absolutewing2";
				auraXDir = xDir;
				yOff = 8;
			}
			if (!grounded) {
				auraSprite = "zero_absolute_wing";
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
			Global.sprites[auraSprite].draw(
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
}
