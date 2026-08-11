using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;

public class BFMammothMState : CharState {
	public BossMammoth burningNoumander = null!;
	public BFMammothMState(
		string sprite, string transitionSprite = ""
	) : base(
		sprite, transitionSprite
	) {
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		burningNoumander = character as BossMammoth ?? throw new NullReferenceException();
	}
}


public class BFlameMInfernoCharge : BFMammothMState {
	public BFlameMInfernoCharge() : base("inferno_charge") {
		invincible = true;
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (stateTime > 1 && !once) {
			once = true;
			character.changeSpriteFromName("inferno_maxed", true);
		}

		if (stateTime > 2) {
			character.changeState(new BFlameMInfernoRelease(), true);
		}
	}
}



public class BFlameMInfernoRelease : BFMammothMState {
	public BFlameMInfernoRelease() : base("inferno_release") {
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();

		character.playSound("ching", sendRpc: true);
		new GigaCrushBackwall(character.pos, character);
		new HitStop(character.pos, player, player.getNextActorNetId(), 
		player.ownedByLocalPlayer, overrideTime: 0.3f, sendRpc: true);
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (character.frameIndex == 6 && !once) {
			once = true;
			new FlameMStompShockwave(
			burningNoumander.getFirstPOI() ?? burningNoumander.getCenterPos(), character.xDir,
			burningNoumander, player, player.getNextActorNetId(), rpc: true);
			character.shakeCamera(sendRpc: true);
			character.playSound("flamemTaunt", sendRpc: true);
			character.playSound("flamemOilBurn", sendRpc: true);


			new FlameMBigFireProj(character.pos.addxy(30, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(60, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(90, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(120, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(150, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(180, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(210, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-30, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-60, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-90, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-120, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-150, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-180, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);
			new FlameMBigFireProj(character.pos.addxy(-210, 0), character.xDir, 0, character, player, player.getNextActorNetId(), rpc: true);



		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}

public class BFlameMGrabStart : BFMammothMState {
	public BFlameMGrabStart() : base("grab") {
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (character.frameIndex == 6 && !once && character.bonusHealth == 0) {
			once = true;
			new FlameMOilProj(
			burningNoumander.getFirstPOI() ?? burningNoumander.getCenterPos(), character.xDir,
			burningNoumander, player, player.getNextActorNetId(), rpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}




public class BFlameMGrabFinisher : BFMammothMState {
	public BFlameMGrabFinisher() : base("grab_finisher") {
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (character.frameIndex == 6 && !once) {
			once = true;
			new FlameMStompShockwave(
			burningNoumander.getFirstPOI() ?? burningNoumander.getCenterPos(), character.xDir,
			burningNoumander, player, player.getNextActorNetId(), rpc: true);
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", sendRpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}




public class BFlameMShootState : BFMammothMState {
	public BFlameMShootState() : base("shoot") {
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (character.frameIndex == 1 && !once) {
			once = true;
			character.playSound("flamemShoot", sendRpc: true);
			new FlameMFireballProj(
			burningNoumander.getFirstPOI() ?? burningNoumander.getCenterPos(), character.xDir, player.input.isHeld(Control.Down, player),
			burningNoumander, player, player.getNextActorNetId(), rpc: true);
		}


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}


public class BFlameMAntiAir : BFMammothMState {
	public BFlameMAntiAir() : base("antiair") {
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (character.frameIndex == 1 && !once) {
			once = true;
			new FlameMFireballProj(
			burningNoumander.getFirstPOI() ?? burningNoumander.getCenterPos(), character.xDir, player.input.isHeld(Control.Down, player),
			burningNoumander, player, player.getNextActorNetId(), rpc: true);
		}


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}
public class BFlameMOilState : BFMammothMState {
	public BFlameMOilState() : base("shoot2") {
	}

	public override bool canEnter(Character character) {
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void update() {
		base.update();
		if (burningNoumander == null) return;

		if (character.frameIndex == 6 && !once) {
			once = true;
			new FlameMOilProj(
			burningNoumander.getFirstPOI() ?? burningNoumander.getCenterPos(), character.xDir,
			burningNoumander, player, player.getNextActorNetId(), rpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}

public class BFlameMJumpPressState : BFMammothMState {
	public BFlameMJumpPressState() : base("fall") {
		exitOnLanding = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

	
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel = new Point(0, 300);
	}
}
public class BFlameMJumpStateAI : BFMammothMState {
	public BFlameMJumpStateAI() : base("jump", "jump_start") {
	}

	public override void update() {
		base.update();
		if (player == null) return;
		if (stateTime >= 24f/60f) {
			character.changeState(new BFlameMJumpPressState());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -character.getJumpPower() * 1.25f;
	}
}

