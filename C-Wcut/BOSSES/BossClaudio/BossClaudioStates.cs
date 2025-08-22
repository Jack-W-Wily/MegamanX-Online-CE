using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;




public class ClaudioBossDash : CharState {


	public ClaudioBossDash() : base("dash") {
		immuneToWind = true;
		enterSound = "GDash";
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();

		character.move(new Point(character.xDir * 350, 0));


		if (stateTime > 0.2f) {
			character.changeState(new ClaudioGroundPunchState(), true);
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
		character.slideVel = character.xDir * character.getDashSpeed() * 0.9f;
	}
}

public class ClaudioTrppleSlash : CharState {


	public ClaudioTrppleSlash() : base("trippleslash") {

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
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}


public class ClaudioFWave : CharState {


	public ClaudioFWave() : base("firewave") {

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
	public override void onExit(CharState newState) {
		base.onExit(newState);


		character.playSound("flamemOilBurn", sendRpc: true);
		

	}

}

public class ClaudioShingetsurin : CharState {


	public ClaudioShingetsurin() : base("shoot") {

	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		new ShingetsurinProj(
			character.getShootPos(), character.xDir,
			0f, character, player, player.getNextActorNetId(), rpc: true
		);
		character.playSound("shingetsurinx5", forcePlay: false, sendRpc: true);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}



public class ClaudioDashPrepare : CharState {


	public ClaudioDashPrepare() : base("dash_prepare") {

	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeState(new ClaudioBossDash(), true);
		}
		
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}


public class ClaudioChargedSlash : CharState {

	bool once = false;
	public ClaudioChargedSlash() : base("chargeslash") {

	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeState(new Idle());
		}
		
		if (character.frameIndex >= 4 && !once) {
			once = true;
			character.playSound("crash");
			new Anim(character.pos, "claudio_charge_slash_ef", character.xDir, null, true);
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}



public class ClaudioGuardState : CharState {
	public ClaudioGuardState() : base("guard") {
		invincible = true;

	}

	public override void update() {
		base.update();

		if (stateTime > 2 || stateTime == Helpers.randomRange(0, 2)) {
			character.changeState(new ClaudioBossDash(), true);
		}
	}
}




public class ClaudioTrippleBuster : CharState {
	int shootNum;
	int lastShootFrame;
	public ClaudioTrippleBuster() : base("shoot2") {
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void update() {
		base.update();

		Point? shootPos = character.getFirstPOI();

	
		if (shootPos != null && character.frameIndex != lastShootFrame) {
			
			shootNum++;
			lastShootFrame = character.frameIndex;
		}

		if (character.isAnimOver()) {
			character.changeState(new Idle());
		}
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class ClaudioGroundPunchState : CharState {
	
	public ClaudioGroundPunchState() : base("groundpunch") {
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void update() {
		base.update();

		if (character.frameIndex == 3 && !once) {
			character.playSound("crashX2", forcePlay: false, sendRpc: true);
			character.shakeCamera(sendRpc: true);
			once = true;
			RockProjectile(15);
			RockProjectile(-15);

			Global.level.delayedActions.Add(new DelayedAction(() => {
				RockProjectile(35);
				RockProjectile(-35);
			}, 0.075f));

			Global.level.delayedActions.Add(new DelayedAction(() => {
				RockProjectile(-55);
				RockProjectile(55);
			}, 0.15f));
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public void RockProjectile(int dist) {
		new FakeZeroRockProj(
			character.pos.addxy(dist, 0), character.xDir, character,
			player.getNextActorNetId(), sendRpc: true
		);

	}
}
