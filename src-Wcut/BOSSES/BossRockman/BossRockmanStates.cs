using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class RollEnterState : CharState {
	public RollEnterState() : base("roll_enter") {
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		var maverick = character;
		maverick.vel.y = -ArmoredArmadillo.rollTransJumpPower;
		maverick.frameSpeed = 0;
	}

	public override void update() {
		base.update();
		if (player == null) return;
		var maverick = character;
		maverick.stopCeiling();

		if (maverick.vel.y > 0) {
			maverick.frameSpeed = 1;
		}
		if (maverick.grounded && stateTime >= 0.2f) {
			maverick.changeState(new RollState());
		}
	}
}

public class RollState : CharState {
	public Point rollDir;
	const float rollSpeed = 300;
	public int bounceCount;
	float rollDirTime;
	const float jumpPower = 350;
	float jumpHeldTime;

	public RollState() : base("roll") {
	
	}

	public override void update() {
		base.update();
		if (player == null) return;
		var maverick = character;
		var input = player.input;
		if (input.isPressed(Control.Dash, player)) {
			maverick.changeState(new RollExitState());
			return;
		}

		if (input.isPressed(Control.Jump, player) && maverick.grounded) {
			jumpHeldTime = Global.spf;
			maverick.vel.y = -250;
		}

		if (jumpHeldTime > 0) {
			if (!input.isHeld(Control.Jump, player)) {
				jumpHeldTime = 0;
			} else {
				jumpHeldTime += Global.spf;
				maverick.vel.y = -250;
				if (jumpHeldTime > 0.25f) {
					jumpHeldTime = 0;
				}
			}
		}

		Point moveAmount = rollDir.times(rollSpeed * Global.spf);
		float moveY = moveAmount.y + (maverick.vel.y * Global.spf);
		CollideData? hit = Global.level.checkTerrainCollisionOnce(maverick, moveAmount.x, moveY - 2, autoVel: true);
		Point? newRollDir = null;
		bool stopBouncing = false;
		if (hit != null) {
			Point normal = hit.getNormalSafe();

			var ceilingHit = Global.level.checkTerrainCollisionOnce(maverick, 0, moveY, autoVel: true);
			if (ceilingHit != null) {
				normal = new Point(0, 1);
			}

			if (!normal.isAngled()) {
				// Sideways wall
				if (normal.x != 0) {
					maverick.xDir *= -1;
					newRollDir = new Point(maverick.xDir, 0).normalize();
					// maverick.vel.y = 0;
					if (input.isHeld(Control.Jump, player) && maverick.vel.y <= 0) {
						jumpHeldTime = Global.spf;
						maverick.vel.y = -250;
					}
				} else {
					newRollDir = new Point(maverick.xDir, 0).normalize();
					// Bottom wall
					if (normal.y < 0 && bounceCount > 8) {
						stopBouncing = true;
					}
					// Top wall
					if (normal.y > 0) {
						jumpHeldTime = 0;
						maverick.vel.y = 0;
					}
				}
			}
		}

		if (newRollDir != null) {
			bounceCount++;
			rollDirTime = 0;
			rollDir = newRollDir.Value;
			maverick.playSound("armoredaCrash", sendRpc: true);
			maverick.shakeCamera(sendRpc: true);
		} else {
			maverick.move(moveAmount, false);
			rollDirTime += Global.spf;
		}

		if (stopBouncing || 
			stateTime > 3f
		) {
			maverick.changeState(new RollExitState());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		var maverick = character;
		rollDir = new Point(maverick.xDir, 0);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class RollExitState : CharState {
	public RollExitState() : base("roll_exit") {
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		var maverick = character;
		maverick.vel.y = -ArmoredArmadillo.rollTransJumpPower;
		maverick.frameSpeed = 0;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}

	public override void update() {
		base.update();
		if (player == null) return;
		var maverick = character;
		maverick.stopCeiling();
		if (maverick.vel.y > 0) {
			maverick.frameSpeed = 1;
		}
		if (maverick.grounded) {
			maverick.changeState(new Idle());
		}
	}
}

