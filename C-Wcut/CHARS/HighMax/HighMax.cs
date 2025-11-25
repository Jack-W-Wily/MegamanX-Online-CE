namespace MMXOnline;
using System;
using System.Collections.Generic;

public class HighMax : Character {
	public HighMax(
	Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, heartTanks, isATrans
	) {
		charId = CharIds.HighMax;
	}


	public float IdlePunchCooldown;
	public float shootCooldown;

	public float ZetsubouCooldown;
	public float CrouchPunchCooldown;

	public override bool canDash() {
		return false;
	}

	public override bool canUseLadder() {
		return true;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override bool isTrueStatusImmune() {
		return overDriveTimer > 0;
	}

	public override bool normalCtrl() {
		if (!grounded && charState.stateTime > 0.005f &&
		player.input.isPressed(Control.Jump, player) && dashedInAir == 0
		) {
			changeState(new HighMaxHover(), true);
			return true;
		}

		if (player.input.isL2Held(player) && player.input.isAPressed(player)) {
			changeState(new ZeroGrabStart(), forceChange: true);
		}



		return base.normalCtrl();
	}

	public override bool canPickupFlag() {
		return false;
	}

	public override bool attackCtrl() {
		bool cmdPressed = player.input.isPressed(Control.Special2, player);
		bool WRPressed = player.input.isPressed(Control.WeaponRight, player);
		bool WLPressed = player.input.isPressed(Control.WeaponLeft, player);
		bool shootPressed = player.input.isPressed(Control.Shoot, player);
		bool specialPressed = player.input.isPressed(Control.Special1, player);
		bool dashPressed = player.input.isPressed(Control.Dash, player);
		if (shootPressed && !player.input.isHeld(Control.Down, player)
		 && !player.input.isHeld(Control.Up, player)) {

			changeState(new HighMaxIdlePunch1(), true);
			return true;


		}
		if (shootPressed && player.input.isHeld(Control.Down, player)) {

			changeState(new HighMaxCrouchPunch1(), true);

			return true;



		}

		if (shootPressed && player.input.isHeld(Control.Up, player)) {
			if (IdlePunchCooldown == 0) {

				changeState(new HighMaxMegaPunch(), true);
				IdlePunchCooldown = 1f;
				return true;


			}
		}

		if (specialPressed) {
			if (ZetsubouCooldown == 0) {

				changeState(new DesmumeState(), true);
				ZetsubouCooldown = 2f;
				return true;


			}
		}


		if (cmdPressed && player.superAmmo == player.superMaxAmmo && player.input.isHeld(Control.Up,player)) {
			changeState(new DesmumeSpam(), true);
			player.superAmmo = 0;
			return true;
		}
		if (cmdPressed && player.superAmmo == player.superMaxAmmo && player.input.isHeld(Control.Down,player)) {
			changeState(new DesmumeSpam2(), true);
			player.superAmmo = 0;
			return true;
		}
		if (cmdPressed && player.superAmmo == player.superMaxAmmo && !player.input.isHeld(Control.Up, player) && !player.input.isHeld(Control.Down, player)) {
			changeState(new HighMaxSuperPunchState(), true);
			player.superAmmo = 0;
			return true;
		}

		if (WRPressed) {
			if (shootCooldown == 0) {

				changeState(new HighmaxShoot1(), true);
				shootCooldown = 1f;
			}
			return true;
		}

		if (WLPressed) {
			changeState(new HighmaxShoot2(), true);
			return true;
		}

		if (dashPressed) {
			changeState(new HighMaxChargePunch(), true);
			return true;
		}
		return base.attackCtrl();
	}

	public override void update() {
		base.update();

		if (charState is ZeroGrabStart) {
            charState.superArmor = true;
        }
		if (charState is ZeroGrabEX) {
            charState.invincible = true;
        }
		//KillingSpreeThemes
		//	if (KillingSpree == 3){
		//			if (musicSource == null) {
		//	addMusicSource("HighMax", getCenterPos(), true); 
		//
		//			}
		///	} 
		/// 



		if (!ownedByLocalPlayer) {
			return;
		}
		// Blocking
		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded && !player.input.isHeld(Control.Shoot, player) &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT());

		}
		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);
		}
		// Cooldowns.
		Helpers.decrementTime(ref IdlePunchCooldown);
		Helpers.decrementTime(ref CrouchPunchCooldown);
		Helpers.decrementTime(ref shootCooldown);
		Helpers.decrementTime(ref ZetsubouCooldown);
		Helpers.decrementTime(ref highmaxArmorCooldown);

		if (charState is Hurt && charState.stateFrames == 0) {
			highmaxDmgCount += 1;
		}
		if (highmaxDmgCount > 5) {
			highmaxArmorCooldown = 2;
			highmaxDmgCount = 0;
		}
	}

	public int highmaxDmgCount = 0;
	public float highmaxArmorCooldown;

	public override void landingCode(bool useSound = true) {
		base.landingCode(useSound);
		shakeCamera(sendRpc: true);
		playSound("crash", sendRpc: true);

	}

	public override bool isToughGuyHyperMode() {
		return highmaxArmorCooldown == 0;
	}


	public virtual bool updateCtrl() {
		if (!ownedByLocalPlayer) {
			return false;
		}
		
		if (charState.exitOnLanding && grounded) {
			landingCode();
		}
		if (charState.exitOnAirborne && !grounded) {
			changeState(new Fall());
		}


		if (canWallClimb() && !grounded &&
			(charState.airMove && vel.y > 0 || charState is WallSlide) &&
			wallKickTimer <= 0 &&
			player.input.isPressed(Control.Jump, player) &&
			(charState.wallKickLeftWall != null || charState.wallKickRightWall != null)
		) {
			dashedInAir = 0;
			if (player.input.isHeld(Control.Dash, player) &&
				(charState.useDashJumpSpeed || charState is WallSlide)
			) {
				isDashing = true;
				dashedInAir++;
			}
			vel.y = -getJumpPower();
			wallKickDir = 0;
			if (charState.wallKickLeftWall != null) {
				wallKickDir += 1;
			}
			if (charState.wallKickRightWall != null) {
				wallKickDir -= 1;
			}
			if (wallKickDir == 0) {
				if (charState.lastLeftWall != null) {
					wallKickDir += 1;
				}
				if (charState.lastRightWall != null) {
					wallKickDir -= 1;
				}
			}
			if (wallKickDir != 0) {
				xDir = -wallKickDir;
			}
			wallKickTimer = maxWallKickTime;
			changeState(new WallKick(), true);
			var wallSparkPoint = pos.addxy(12 * xDir, 0);
			var rect = new Rect(wallSparkPoint.addxy(-2, -2), wallSparkPoint.addxy(2, 2));
			if (Global.level.checkCollisionShape(rect.getShape(), null) != null) {
				new Anim(wallSparkPoint, "wall_sparks", xDir,
					player.getNextActorNetId(), true, sendRpc: true
				);
			}
			return true;
		}
		if (charState.canStopJump &&
			!grounded && vel.y < 0 &&
			!player.input.isHeld(Control.Jump, player)
		) {
			vel.y = 0;
		}
		if (charState.airMove && !grounded) {
			airMove();
		}
		if (charState.normalCtrl) {
			normalCtrl();
		}
		if (charState.attackCtrl) {
			return attackCtrl();
		}

		return false;
	}


	public override string getSprite(string spriteName) {
		return "highmax_" + spriteName;
	}


	public override void render(float x, float y) {

		if (overDriveTimer > 0 && visible) {
			addRenderEffect(RenderEffectType.SpeedDevilTrailNoDash);
		} else {
			removeRenderEffect(RenderEffectType.SpeedDevilTrailNoDash);
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


	public override Projectile getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile proj = null;
		if (hitbox.flag == (int)HitboxFlag.Hitbox) {
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new XBuster(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true, isZSaberClang: false, addToLevel: true
			);
		}

		if (  sprite.name.Contains("grab") && !sprite.name.Contains("ex") )
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.ForceGrabState,
			player, 0f, 0, 20, isZSaberClang : true ,addToLevel: true, hitSound : "kofhtsnd_grab1"
			);
		}

		 if (  sprite.name.Contains("grab") && sprite.name.Contains("ex") )
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.BurensenEND,
			player, 3f, 0, 20, isZSaberClang : false ,addToLevel: true, hitSound : "swordswipeGG"
			);
		}


		if (sprite.name.Contains("idle_punch")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, 
			ProjIds.MechFrogGroundPound, player, 3f, 20, isZSaberClang: true, addToLevel: true, hitSound : "kofhtsnd_clamp1"
			);
		}
	//	if (sprite.name.Contains("land")) {
	//		return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, 
	//		ProjIds.Rakukojin, player, 2f, 20, 5f, isZSaberClang: true, addToLevel: true
	//		);
	//	}
		if (sprite.name.Contains("crouch_punch")) {
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint,
			 ProjIds.UPPunch, player, 2f, 25, isZSaberClang: true, addToLevel: true, hitSound : "kofhtsnd_clamp2"
			 );
		}
		if (sprite.name.Contains("slam_grab")) {
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, 
			ProjIds.MechFrogGroundPound, player, 3f, 30, isZSaberClang: true, addToLevel: true, hitSound : "kofhtsnd_knock1"
			);
		}
		if (sprite.name.EndsWith("dash_punch")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint,
			 ProjIds.HeavyPush, player, 2f, 0, 4f, null, isShield: true, 
			 isDeflectShield: true, isZSaberClang: true, addToLevel: true, hitSound : "kofhtsnd_knock1"
			 );
		}
		if (sprite.name.EndsWith("dash_punch_charge")) {
			return new GenericMeleeProj(new RCXPunch(), 
			centerPoint, ProjIds.ForceGrabState, player, 1f, 0, 20f, null, isShield: true,
			 isDeflectShield: true, isZSaberClang: true, addToLevel: true, hitSound : "kofhtsnd_grab2"
			 );
		}
		if (sprite.name.EndsWith("foward_punch") && charState is not HighMaxSuperPunchState) {
			return new GenericMeleeProj(new RCXPunch(), 
			centerPoint, ProjIds.HeavyPush, player, 3f, 0, 20f, 
			null, isShield: true, isDeflectShield: true, isZSaberClang: true, addToLevel: true, hitSound : "kofhtsnd_knock1"
			);
		}
		if (sprite.name.EndsWith("foward_punch") && charState is HighMaxSuperPunchState) {
			return new GenericMeleeProj(new RCXPunch(), 
			centerPoint, ProjIds.BurensenEND, player, 6f, 0, 20f, null, 
			isShield: true, isDeflectShield: true, isZSaberClang: false, addToLevel: true, hitSound : "kofhtsnd_knock1"
			);
		}
		}
		return proj;
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

}

