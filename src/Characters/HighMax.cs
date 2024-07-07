namespace MMXOnline;
using System;
using System.Collections.Generic;

public class HighMax : Character {
	public HighMax(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
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
		if (!grounded && charState.stateTime > 0.2f &&
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
		return base.attackCtrl();
	}



	public override void update() {
		base.update();

		if (!ownedByLocalPlayer) {
			return;
		}
	
		// Cooldowns.
		Helpers.decrementTime(ref IdlePunchCooldown);
		Helpers.decrementTime(ref CrouchPunchCooldown);
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
				player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}
		 if (  sprite.name.Contains("idle_punch"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 3f, 30);
		}
		 if (  sprite.name.Contains("land"))
		{
			return new GenericMeleeProj(new RakukojinWeapon(player), centerPoint, ProjIds.Rakukojin, player, 2f, 20, 2f);
		}
		 if (  sprite.name.Contains("crouch_punch"))
		{
			return new GenericMeleeProj(new RisingWeapon(player), centerPoint, ProjIds.MechFrogStompShockwave, player, 3f, 0);
		}

		return proj;
	}
}

