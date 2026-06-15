using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;




public class GBDSniperState : CharState {
	public float soundCooldown;

	public Projectile fSplasherProj;


	public bool beam;


	public GBDSniperState()
		: base("sniper", "") {

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("counters_usp_clipin", true);
		character.stopMoving();
		

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

	public override void update() {
		base.update();
		Helpers.decrementTime(ref soundCooldown);
		if (!player.input.isBHeld(player)) {
			character.changeToIdleOrFall();
		}
		

		if (
		player.input.isAPressed(player) && soundCooldown == 0){
			if (character.frameIndex != 2) {
				character.frameIndex = 2;
				soundCooldown = 2;
				character.playSound("spiralMagnum");
			
				}
		}


	}
}




public class GBDUppercut : CharState {


	public GBDUppercut() : base("uppercut") {
	enterSound = "punch2";
	canSpecialCancel = true;
	}

	public override void update() {
		base.update();
		

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}


public class GBDTonfaCharge : CharState {


	public GBDTonfaCharge() : base("tonfa_charge") {
		normalCtrl = true;
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		if (!player.input.isR2Held(player)) {
			if (character.frameIndex != 3) {
				if (player.input.isHeld(Control.Up, player)){
				character.changeState(new GBDTonfaAttackU(), true);
				} else {
				character.changeState(new GBDTonfaAttackF(), true);	
				}
			character.slideVel = character.xDir * character.getDashSpeed() * 0.5f;
			} else {
				if (player.input.isHeld(Control.Up, player)){
				character.changeState(new GBDTonfaAttackUCharged(), true);
				} else {
				character.changeState(new GBDTonfaAttackFCharged(), true);	
				}
			character.slideVel = character.xDir * character.getDashSpeed();	
			}
		}


	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}





public class GBDTonfaAttackF : CharState {

	bool second;
	public GBDTonfaAttackF() : base("tonfa_f") {
	enterSound = "recoilRod1";
	}

	public override void update() {
		base.update();
		if (player.input.isR2Pressed(player) && !second) {
			character.changeSpriteFromName("tonfa_f2", true);
			second = true;
			sprite = "tonfa_f2";
			character.playSound("recoilRod1", true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}



public class GBDTonfaAttackFCharged : CharState {


	public GBDTonfaAttackFCharged() : base("tonfa_charged_f") {
	enterSound = "recoilRod2";
	}

	public override void update() {
		base.update();
		

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}





public class GBDTonfaAttackU : CharState {

	bool second;
	public GBDTonfaAttackU() : base("tonfa_u") {
	enterSound = "recoilRod1";
	}

	public override void update() {
		base.update();
		if (player.input.isR2Pressed(player) && !second) {
			character.changeSpriteFromName("tonfa_u2", true);
			second = true;
			sprite = "tonfa_u2";
			character.slideVel = character.xDir * character.getDashSpeed() * 0.7f;
			character.playSound("recoilRod1", true);
		}

		if (player.input.isR2Pressed(player) && second && character.frameIndex > 2) {
			character.changeSpriteFromName("tonfa_overhead", true);
			second = true;
			sprite = "tonfa_overhead";
			character.slideVel = character.xDir * character.getDashSpeed() ;
			character.playSound("recoilRod1", true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}






public class GBDTonfaAttackUCharged : CharState {


	public GBDTonfaAttackUCharged() : base("tonfa_charged_u") {
	enterSound = "recoilRod2";
	}

	public override void update() {
		base.update();
		

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}







