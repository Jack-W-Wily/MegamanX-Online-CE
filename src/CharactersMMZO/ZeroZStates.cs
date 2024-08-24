using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;






public class ZeroZSlash1 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public ZeroZSlash1(string transitionSprite = "")
		: base("slash_1", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		
		base.update();


		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (player.input.isPressed(Control.Shoot, player))
		{
			character.changeState(new ZeroZSlash2());
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}
	


public class ZeroZSlash2 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public ZeroZSlash2(string transitionSprite = "")
		: base("slash_2", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		
		base.update();


		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (player.input.isPressed(Control.Shoot, player))
		{
			character.changeState(new ZeroZSlash3());
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}
	
	


public class ZeroZSlash3 : CharState {
	private float specialPressTime;
	
	public float pushBackSpeed;

	public ZeroZSlash3(string transitionSprite = "")
		: base("slash_3", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}
	



public class ZeroZDashSlash : CharState {
	private float specialPressTime;
	
	public float pushBackSpeed;

	public ZeroZDashSlash(string transitionSprite = "")
		: base("dash_slash", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class ZeroZJumpSlash : CharState {
	private float specialPressTime;
	
	public float pushBackSpeed;

	public ZeroZJumpSlash(string transitionSprite = "")
		: base("jump_slash", "", "", transitionSprite)
	{
	airMove = true;
	exitOnLanding = true;
	}

	public override void update()
	{
		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}