namespace MMXOnline;
using System;
using System.Collections.Generic;

public class HighMax : Character {
	public HighMax(
			Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.HighMax;
	}

	
	public float IdlePunchCooldown;
	public float CrouchPunchCooldown;

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override bool normalCtrl() {
		if (!grounded && charState.stateTime > 0.005f &&
		player.input.isPressed(Control.Jump, player) && dashedInAir == 0 
		) {
			dashedInAir++;
			changeState(new HighMaxHover(), true);
			return true;
		}
	
		return base.normalCtrl();
	}


	public override bool attackCtrl() {
		bool shootPressed = player.input.isPressed(Control.Shoot, player);
		bool specialPressed = player.input.isPressed(Control.Special1, player);
		bool dashPressed = player.input.isPressed(Control.Dash, player);
		if (shootPressed && !player.input.isHeld(Control.Down,player)) {
			if (IdlePunchCooldown == 0) {
			
					changeState(new HighMaxIdlePunch1(), true);
					IdlePunchCooldown = 0.8f;
					return true;
				
			
			}
		}
		if (shootPressed && player.input.isHeld(Control.Down,player)) {
			if (CrouchPunchCooldown == 0) {
			
					changeState(new HighMaxCrouchPunch1(), true);
					CrouchPunchCooldown = 0.8f;
					return true;
				
	
			}
		}
		if (dashPressed) {
					changeState(new HighMaxChargePunch(), true);		
					return true;
		}
		return base.attackCtrl();
	}

	public override void update() {
		base.update();


		//KillingSpreeThemes
		//	if (KillingSpree == 3){
		//			if (musicSource == null) {
		//	addMusicSource("HighMax", getCenterPos(), true); 
		//
		//			}
		///	} 

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
		// Cooldowns.
		Helpers.decrementTime(ref IdlePunchCooldown);
		Helpers.decrementTime(ref CrouchPunchCooldown);
	}


	public override void landingCode(bool useSound = true) {
		base.landingCode(useSound);
		shakeCamera(sendRpc: true);
		playSound("crash", sendRpc: true);
		
	}
	
		public override bool isToughGuyHyperMode() {
		return !isInDamageSprite();
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

	public override Projectile getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile proj = null;
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new XBuster(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true, isZSaberClang : false, addToLevel: true
			);
		}
		 if (  sprite.name.Contains("idle_punch"))
		{
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.UPPunch, player, 3f, 30, isZSaberClang : true, addToLevel: true);
		}
		 if (  sprite.name.Contains("land"))
		{
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.Rakukojin, player, 2f, 20, 5f , isZSaberClang : true, addToLevel: true);
		}
		 if (  sprite.name.Contains("crouch_punch"))
		{
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.MechFrogStompShockwave, player, 3f, 0 , isZSaberClang : true, addToLevel: true);
		}
		 if ( sprite.name.Contains("dash_punch"))
		{
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.HeavyPush, player, 2f, 0, 4f, null, isShield: true, isDeflectShield: true, isZSaberClang : true, addToLevel: true);
		}
		return proj;
	}
}

