using System.Linq;

namespace MMXOnline;

public class HighMaxHover : CharState {
	float hoverTime;
    bool once;
	bool first = false;
	bool seccond = false;
	bool third = false;
	bool fourth = false;
	public HighMaxHover() : base("hover", "hover", "hover", "hover") {
		exitOnLanding = true;
		airMove = true;
		attackCtrl = false;
		normalCtrl = true;
	}

	public override void update() {
		base.update();

		accuracy = 0;
		Point prevPos = character.pos;

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}

		if (player.input.isPressed(Control.Shoot, player) && !once){
		character.changeSpriteFromName("shoot1", true);
		once = true;
		}
		var poi = character.getFirstPOI();
			
		if (character.sprite.name.Contains("shoot") && poi != null){
			
			if (!first && character.sprite.frameIndex > 2){
			new TorpedoProj(new Torpedo(), poi.Value, character.xDir, player, 3, player.getNextActorNetId(), 0, rpc: true);
			first = true;
			}
			if (!seccond && character.sprite.frameIndex > 4){
			new TorpedoProj(new Torpedo(), poi.Value, character.xDir, player, 3, player.getNextActorNetId(), 0, rpc: true);
			seccond = true;
			}
			if (!third && character.sprite.frameIndex > 6){
			new TorpedoProj(new Torpedo(), poi.Value, character.xDir, player, 3, player.getNextActorNetId(), 0, rpc: true);
			third = true;
			}
			if (!fourth && character.sprite.frameIndex > 8){
			new TorpedoProj(new Torpedo(), poi.Value, character.xDir, player, 3, player.getNextActorNetId(), 0, rpc: true);
			fourth = true;
			}
		}
		if (character.vel.y < 0 && !player.input.isHeld(Control.Up, player) 
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
		if (player.input.isHeld(Control.Up, player)){
			character.vel.y = -character.getJumpPower() * 1f;
		}
		if (player.input.isHeld(Control.Down, player)){
			character.vel.y = +character.getJumpPower() * 1f;
		}

		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}

		hoverTime += Global.spf;
	if ((hoverTime > 10) || hoverTime > 0.2f &&
			character.player.input.isPressed(Control.Jump, character.player)
		) {
			character.changeState(new Fall(), true);
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



public class HighMaxIdlePunch1 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public HighMaxIdlePunch1(string transitionSprite = "")
		: base("idle_punch1", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		
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
		if ( player.input.isHeld(Control.Down, player) && player.input.isPressed(Control.Shoot, player))
		{
			character.changeState(new HighMaxCrouchPunch1());
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
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class HighMaxCrouchPunch1 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public HighMaxCrouchPunch1(string transitionSprite = "")
		: base("crouch_punch", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		
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
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}
