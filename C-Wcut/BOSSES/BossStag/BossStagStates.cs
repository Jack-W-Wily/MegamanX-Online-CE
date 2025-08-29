using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class BossStagMState : CharState {
	public BossStag BFlameStagger = null!;
	public BossStagMState(
		string sprite, string transitionSprite = ""
	) : base(
		sprite, transitionSprite
	) {
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		BFlameStagger = character as BossStag ?? throw new NullReferenceException();
	}
}

public class BFStagShoot : BossStagMState {
	bool shotOnce;
	FStagFireballProj? fireball;
	bool isSecond;
	public BFStagShoot(bool isSecond) : base(isSecond ? "punch2" : "punch") {
		this.isSecond = isSecond;
	}

	public override void update() {
		base.update();

		Point shootPos = BFlameStagger.getCenterPos();
			if (character.frameIndex >= 7 && !shotOnce && !isSecond) {
				shotOnce = true;
				BFlameStagger.playSound("fstagShoot", sendRpc: true);
				fireball = new FStagFireballProj(
					shootPos, character.xDir, 0, BFlameStagger, player, 
					player.getNextActorNetId(), rpc: true);
			}
			if (isSecond && character.frameIndex >= 4 && !once) {
				BFlameStagger.playSound("fstagShoot", sendRpc: true);
				once = true;
				fireball = new FStagFireballProj(
					shootPos, character.xDir, 1, BFlameStagger, player, 
					player.getNextActorNetId(), rpc: true);
			}


		if (!isSecond && character.frameIndex >= 8) {
			if (player.isAI || player.input.isPressed(Control.Shoot, player)) {
				character.changeState(new BFStagShoot(true));
				return;
			}
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (fireball != null && fireball.vel.isZero()) {
			fireball.destroySelf();
		}
	}
}



public class BFStagDashChargeState : BossStagMState {
	FStagDashChargeProj? proj;
	public BFStagDashChargeState() : base("angry") {
	}

	public override void update() {
		base.update();
		if (BFlameStagger == null) return;

		proj?.incPos(character.deltaPos);
		character.turnToInput(player.input, player);

		if (player.isAI) {
			if (stateTime > 0.4f) {
				character.changeState(new BFStagDashState(stateTime));
			}
		} else if ((!player.input.isHeld(Control.Dash, player) && stateTime > 0.2f) || stateTime > 0.6f) {
			character.changeState(new BFStagDashState(stateTime));
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		proj = new FStagDashChargeProj(
			character.getFirstPOIOrDefault("fire_body"), character.xDir,
			BFlameStagger, player, player.getNextActorNetId(), rpc: true
		);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		proj?.destroySelf();
	}
}

public class BFStagDashState : BossStagMState {
	float trailTime;
	FStagDashProj? proj;
	float chargeTime;
	public Anim? ProjVisible;
	public BFStagDashState(float chargeTime) : base("dash") {
		this.chargeTime = chargeTime;
		enterSound = "fstagDash";
	}

	public override void update() {
		base.update();
		if (BFlameStagger == null) return;

		if (player.input.isPressed(Control.Special1, player)) {
			character.changeState(new BFStagGrabState(true));
			return;
		}
		ProjVisible?.changePos(character.getFirstPOIOrDefault("fire_dash"));
		proj?.changePos(character.getFirstPOIOrDefault("fire_dash"));
		character.move(new Point(character.xDir * 400, 0));
		Helpers.decrementTime(ref trailTime);
		if (trailTime <= 0) {
			trailTime = 0.04f;
			new FStagTrailProj(
			 	character.getFirstPOIOrDefault("fire_trail"), character.xDir,
				BFlameStagger ,player, player.getNextActorNetId(), rpc: true
			);
		}
		if (player.input.isPressed(Control.Dash, player) || stateTime > chargeTime) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();

		ProjVisible = new Anim(
			character.getFirstPOIOrDefault("fire_dash"), "fstag_fire_dash", character.xDir,
			player.getNextActorNetId(), false, sendRpc: true
		);
		proj = new FStagDashProj(
			character.getFirstPOIOrDefault("fire_dash"), character.xDir, 0,
			BFlameStagger, player, player.getNextActorNetId(), rpc: true
		);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		proj?.destroySelf();
		ProjVisible?.destroySelf();
	}
}

public class BFStagGrabState : BossStagMState {
	float xVel = 400;
	public Character? victim;
	float endLagTime;
	public BFStagGrabState(bool fromDash) : base("dash_grab") {
		if (!fromDash) xVel = 0;
		
	}

	public override void update() {
		base.update();
		if (player == null) return;

		xVel = Helpers.lerp(xVel, 0, Global.spf * 2);
		character.move(new Point(character.xDir * xVel, 0));

		character.turnToInput(player.input, player);

		if (victim == null && character.frameIndex >= 6) {
			character.changeToIdleOrFall();
			return;
		}

		if (character.isAnimOver()) {
			if (victim != null) {
				endLagTime += Global.spf;
				if (endLagTime > 0.25f) {
					character.changeState(new BFStagUppercutState(victim));
				}
			} else {
				character.changeToIdleOrFall();
			}
		}
	}

	public virtual bool trySetGrabVictim(Character grabbed) {
		if (victim == null) {
			victim = grabbed;
			return true;
		}
		return false;
	}
}

public class BFStagUppercutState : BossStagMState {
	FStagDashProj? proj;
	float yDist;
	int state;
	public Anim? ProjVisible;
	public Character victim;
	float topDelay;
	int upHitCount;
	int downHitCount;
	public BFStagUppercutState(Character victim) : base("updash") {
		this.victim = victim;
		enterSound = "fstagUppercut";
	}

	public override void update() {
		base.update();
		if (BFlameStagger == null) return;

		proj?.changePos(character.pos);
		ProjVisible?.changePos(character.pos);

		float speed = 450;
		float yFactor = 1;
		if (state == 2) {
			yFactor = -1;
		}

		Point moveAmount = new Point(character.xDir * 50, -speed * yFactor);
		if (state != 1) {
			character.move(moveAmount);
			yDist += Global.spf * speed;
		}

		if (state == 0) {
			var hit = checkCollisionNormal(moveAmount.x * Global.spf, moveAmount.y * Global.spf);
			if (hit != null) {
				if (hit.isCeilingHit()) {
					crashAndDamage(true);
					reverse();
				} else {
					upHitCount++;
					if (upHitCount > 5) {
						crashAndDamage(true);
						reverse();
					} else {
						character.xDir *= -1;
					}
				}
			} else if (yDist > 224) {
				reverse();
			}
		} else if (state == 1) {
			topDelay += Global.spf;
			if (topDelay > 0.1f) {
				state = 2;
			}
		} else {
			var hit = checkCollisionNormal(moveAmount.x * Global.spf, moveAmount.y * Global.spf);
			if (hit != null) {
				if (hit.isGroundHit()) {
					crashAndDamage(false);
					character.changeToIdleOrFall();
				} else {
					downHitCount++;
					if (downHitCount > 5) {
						crashAndDamage(false);
						character.changeToIdleOrFall();
					} else {
						character.xDir *= -1;
					}
				}
			}
		}
	}

	public Character? getVictim() {
		if (victim == null) return null;
		if (!victim.sprite.name.EndsWith("_grabbed")) {
			return null;
		}
		return victim;
	}

	public void crashAndDamage(bool isCeiling) {
		if (getVictim() != null) {
			BFlameStagger.uppercutWeapon.applyDamage(
				victim, false, BFlameStagger, (int)ProjIds.FStagUppercut,
				overrideDamage: isCeiling ? 3 : 5, overrideFlinch: isCeiling ? 0 : Global.defFlinch,
				sendRpc: true
			);
		}
		character.playSound("crash", sendRpc: true);
		character.shakeCamera(sendRpc: true);
	}

	public void reverse() {
		if (state == 0) {
			state = 1;
			proj?.changeSprite("fstag_fire_downdash", true);
			BFlameStagger.changeSpriteFromName("downdash", true);
			ProjVisible?.changeSprite("fstag_fire_downdash", true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.unstickFromGround();
		ProjVisible = new Anim(
			BFlameStagger.pos, "fstag_fire_updash", character.xDir,
			player.getNextActorNetId(), false, sendRpc: true
		);
		proj = new FStagDashProj(
			BFlameStagger.pos, BFlameStagger.xDir, 1, BFlameStagger,
			player, player.getNextActorNetId(), rpc: true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		BFlameStagger.useGravity = true;
		proj?.destroySelf();
		ProjVisible?.destroySelf();
		if (getVictim() != null) {
			victim.releaseGrab(character);
		}
	}
}


public class BFStagWallDashState : BossStagMState {
	public BFStagWallDashState() : base("wall_dash") {
	enterSound = "jumpx2";
	}

	public override void update() {
		base.update();
		if (character.grounded) {
			character.landingCode();
			return;
		}

		if (Global.level.checkTerrainCollisionOnce(character, 0, -1) != null && character.vel.y < 0) {
			character.vel.y = 0;
		}
		character.move(new Point(character.xDir * 350, 0));
	}
}





public class BFStagOrochinagiCharge : CharState {


	public BFStagOrochinagiCharge() : base("orochinagi_start") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (!player.input.isBHeld(player)) {
			character.changeState(new BFStagOrochinagiCharged(), true);
			//this is a Release button type action so you use ! with the "isheld" input call
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

	}
	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.turnToInput(player.input, player);// this allows the char to turn midmove
	}

}



public class BFStagOrochinagi : CharState {
	bool fired;


	public BFStagOrochinagi() : base("orochinagi_end") {
		landSprite = "orochinagi_end";
		airSprite = "orochinagi_end";
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("flamemOilBurn", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded || character.vel.y < 0) {
			sprite = "orochinagi_end";
			character.changeSpriteFromName(sprite, true);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}





public class BFStagOrochinagiCharged : CharState {
	bool fired;

	public BossStag kr = null!; // sometimes you want your character to have
								 // features tied to conditions in your charstates so you'll need this

	public BFStagOrochinagiCharged() : base("orochinagi_end") {
		landSprite = "orochinagi_end";
		airSprite = "orochinagi_end";
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
		invincible = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("flamemOilBurn", sendRpc: true);
			// this is what you need to make your character spawn a projectile
			new OrochinagiChargedProj(
				character.pos.addxy(30 * character.xDir, -20), character.xDir,
				kr.OverDrive, character, player, player.getNextActorNetId(), rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded || character.vel.y < 0) {
			sprite = "orochinagi_end";
			character.changeSpriteFromName(sprite, true);
		}
		kr = character as BossStag ?? throw new NullReferenceException(); // to make sure
		// your character is corresponsive to the reference
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}