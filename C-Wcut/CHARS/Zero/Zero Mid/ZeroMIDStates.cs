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
		airMove = true;
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class ZeroGrabEX : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;
    
    public float soundCooldown;

	public ZeroGrabEX(string transitionSprite = "")
        : base("grab_ex", "", "", transitionSprite)
    {
        airMove = true;
    }

	public override void update()
	{
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

        if (character.frameIndex == 0 && soundCooldown == 0)
        {
            soundCooldown = 0.1f;
           character.playSound("buster2", sendRpc: true); 
        }

        if (stateTime > 0.5f && !character.sprite.name.Contains("end"))
        {
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}