using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;


public class ZeroGrabStart : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public ZeroGrabStart(string transitionSprite = "")
		: base("grab_start", "", "", transitionSprite) {
	}

	public override void update() {

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

		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class ZeroGrabEX : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public float soundCooldown;

	public ZeroGrabEX(string transitionSprite = "")
		: base("grab_ex", "", "", transitionSprite) {
		airMove = true;
	}

	public override void update() {
		Helpers.decrementTime(ref soundCooldown);
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

		if (character.frameIndex == 0 && soundCooldown == 0) {
			soundCooldown = 0.1f;
			character.playSound("buster2", sendRpc: true);
		}

		if (stateTime > 0.5f && !character.sprite.name.Contains("end")) {
			character.changeSpriteFromName("grab_ex_end", true);
		}



		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}








public class ZeroFinalStart : CharState {
	Anim? proj;

	public ZeroFinalStart() : base("final_start", "", "", "") {
		invincible = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && proj != null) {
			proj.destroySelf();
			proj = null;
		}

		character.move(new Point(character.xDir * 350, 0));

		 if (stateTime > 0.6f) {
			character.changeToIdleOrFall();
			return;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel.y = 0;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class ZeroFinalEnd : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public ZeroFinalEnd(Character? victim) : base("final_end", "", "", "") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		//if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("knocked_down")) {
		//	character.changeToIdleOrFall();
		//	return;
		//}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.33f) {
					leechTime = 0;
				}
				return;
			}
		}

			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();
			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);
			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));

		
	if (!player.input.isHeld(Control.Down, player)
			&& player.input.isPressed(Control.Shoot, player)) {

			if (Helpers.randomRange(0,3) == 0 && leechTime > 0.05f){
			character.changeSpriteFromName("attack", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 1  && leechTime > 0.05f){
			character.changeSpriteFromName("attack_2", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 2 && leechTime > 0.05f){
			character.changeSpriteFromName("attack_air", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 3 && leechTime > 0.05f){
			character.changeSpriteFromName("attack_3", true);
			leechTime = 0;
			}
			
		}

			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {
			character.changeSpriteFromName("hyouretsuzan_fall", true);	
		}


		

		if (player.input.isPressed(Control.Special1, player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		victim.grabInvulnTime = 0.5f;
		 
	}
}


