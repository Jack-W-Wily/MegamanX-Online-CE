using System;
using System.Collections.Generic;

namespace MMXOnline;




public class GreenEyedLampState : CharState {
	Character vile;

	public GreenEyedLampState() : base("green_eyed_lamp", "", "") {
        enterSound = "ryuenjin";
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		
			

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}
