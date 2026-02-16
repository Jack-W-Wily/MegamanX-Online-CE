using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;





public class WarpDodge : CharState {
	const float warpHeight = 150;
	float origYPos;
	int phase = 0;
	Point summonPos;
	bool isNew;
	public WarpDodge(Point summonPos) : base("warp_beam") {
		this.summonPos = summonPos;
		this.isNew = isNew;
		enterSound = "warpIn";
		specialId = SpecialStateIds.AxlRoll;
		airMove = true;
	}

	public override void update() {
		base.update();
		if (phase == 0) {
			character.incPos(new Point(0, -Global.spf * 450));
			if (character.pos.y < origYPos - warpHeight) {
				character.changePos(summonPos.addxy(0, -warpHeight));
				phase = 1;
			}
		} else if (phase == 1) {
			character.incPos(new Point(0, Global.spf * 450));
			if (character.pos.y >= summonPos.y) {		
					character.changeState(new Idle(), true);	
			}
		}
	}

	public override void onEnter(CharState? oldState) {
		base.onEnter(oldState);
		character.vel = Point.zero;
		character.useGravity = false;
		origYPos = character.pos.y;
	
		if (isNew) {
			character.changePos(summonPos.addxy(0, -warpHeight));
			phase = 1;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class RMXDoubleKick : CharState {

	public bool snd1 = false;
	public bool snd2 = false;
	public RMXDoubleKick() : base("double_kick") {
		enterSound = "jump";
	}

	public override void update() {
		base.update();

		if (character.grounded && stateTime > 0.05f) {
			character.changeToCrouchOrFall();
		}

		if (character.frameIndex == 2 && !snd1) {
			snd1 = true;
			character.playSound("punch1");
		}
		if (character.frameIndex == 7 && !snd2) {
			snd2 = true;
			character.playSound("punch2");
		}
		if (player.input.isAPressed(player)) {
			character.changeState(new RMXDoubleKickShoot(), true);
		}

		if (Global.level.checkTerrainCollisionOnce(character, 0, -1) != null && character.vel.y < 0) {
			character.vel.y = 0;
		}

		character.move(new Point(character.xDir * 150, 0));
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -character.getJumpPower() * 0.75f;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}







public class RMXDoubleKickShoot : CharState {
	bool shot = false;
	Buster3GigaProjMelee? proj;
	float specialPressTime;

	public float pushBackSpeed;

	public RMXDoubleKickShoot(string transitionSprite = "") : base("double_kick_shoot", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);


		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		} else {
			character.changeSpriteFromName("double_kick_shot_grounded", true);
			sprite = "double_kick_shot_grounded";
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("buster3", sendRpc: true);
		character.frameIndex = 3;
		character.frameTime = 0;
	//	var poi = character.sprite.getCurrentFrame().POIs[0];
	//	poi.x *= character.xDir;
		proj = new Buster3GigaProjMelee(character.getShootPos(), character.xDir, character, player, player.getNextActorNetId(), true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


}





public class RMXPunch : CharState {
	bool shot = false;


	public float pushBackSpeed;

	public RMXPunch(string transitionSprite = "") : base("punch_1", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();


		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}

		if (player.input.isAPressed(player)) {
			character.changeState(new RMXDoubleKickShoot(), true);
		}


		if (player.input.isBPressed(player) && character.sprite.frameIndex >= 3) {
			character.changeState(new RMXPunch2(), true);
		}


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 50;
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("punch1", sendRpc: true);
	}


	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


}



public class RMXPunch2 : CharState {
	bool shot = false;


	public float pushBackSpeed;

	public RMXPunch2(string transitionSprite = "") : base("punch_2", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();


		if (player.input.isAPressed(player)) {
			character.changeState(new RMXDoubleKickShoot(), true);
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}

		
		 if (character.isAnimOver()) {
					character.changeToIdleOrFall();
					return;
		}
			
		
			if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 50;
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("punch2", sendRpc: true);
	}


	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


}


