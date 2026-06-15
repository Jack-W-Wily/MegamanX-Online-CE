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
	}

	public override void update() {
		base.update();
		if (!character.sprite.name.Contains("end")) {
		character.move(new Point(character.xDir * 300, 0));
		}
		if (stateTime  > 0.3f) {
		if (!character.sprite.name.Contains("end")) {
			sprite = "dash_end";
			character.changeSpriteFromName("dash_end", true);
			if (character.bonusHealth <= 0) {
					character.frameSpeed = 2;
				}
			} 
		}
		if (character.isAnimOver()) {
			character.changeState(new ClaudioGroundPunchState(), true);
		}
		


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class ClaudioBossDashNoPound : CharState {


	public ClaudioBossDashNoPound() : base("dash") {
		immuneToWind = true;
		enterSound = "GDash";
		
	}

	public override void update() {
		base.update();
		if (!character.sprite.name.Contains("end")) {
		character.move(new Point(character.xDir * 300, 0));
		}
		if (stateTime  > 0.3f) {
		if (!character.sprite.name.Contains("end")) {
			sprite = "dash_end";
			character.changeSpriteFromName("dash_end", true);
			} 
		}
		if (character.isAnimOver()) {
			character.changeState(new ClaudioGroundPunchState(), true);
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



public class BossJumpStart : CharState {


	public BossJumpStart() : base("jump_start") {
		immuneToWind = true;
	}

	public override void update() {
		base.update();
		
		if (character.isAnimOver()) {
		
			character.changeState(new BossJump(), true);
			character.vel.y = -character.getJumpPower();
			
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





public class BossJump : CharState {

	bool dropDown;
	public BossJump() : base("jump") {
		enterSound = "land";
		specialId = SpecialStateIds.AxlRoll;
		canSpecialCancel = true;
		normalCtrl = true;
	}

	public override void update() {
		base.update();

		if (character.grounded && stateTime > 0.05f) {
			exitOnLanding = true;
		}
		if (!character.grounded) {
		if (!player.isAI && !character.player.input.isAHeld(player) && !dropDown && character.frameIndex < 8) {
			dropDown = true;
			character.vel.y = 0;
			character.frameIndex = 8;
		}
		if (player.isAI && stateTime == Helpers.randomRange(0,2) && !dropDown) {
			dropDown = true;
			character.vel.y = 0;
			character.frameIndex = 8;
		}
		
			character.move(new Point(character.xDir * 200, 0));
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		specialId = SpecialStateIds.None;
	}
}



public class ClaudioTrppleSlash : CharState {


	public ClaudioTrppleSlash() : base("trippleslash_prepare") {

	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {

			if (character.sprite.name.Contains("prepare")) {
			sprite = "trippleslash";
			character.changeSpriteFromName("trippleslash", true);
			} else {
			character.changeToIdleOrFall();
			}
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (character.bonusHealth <0) {
			sprite = "trippleslash";
			character.changeSpriteFromName("trippleslash", true);
		}
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}




public class ClaudioTrppleSlashMaverick : MaverickState {


	public ClaudioTrppleSlashMaverick() : base("trippleslash") {

	}

	public override void update() {
		base.update();
		var character = maverick;
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(MaverickState newState) {
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
	public override void onExit(CharState? newState) {
		base.onExit(newState);


		character.playSound("flamemOilBurn", sendRpc: true);
		new FireWaveProjChargedStart(character.pos, character.xDir, character, player, player.getNextActorNetId(), true);
		

	}

}



public class ClaudioFWaveMaverick : MaverickState {


	public ClaudioFWaveMaverick() : base("firewave") {

	}

	public override void update() {
		base.update();
		var character = maverick;
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		
	}
	public override void onExit(MaverickState newState) {
		base.onExit(newState);

		var character = maverick;
		character.playSound("flamemOilBurn", sendRpc: true);
		new FireWaveProjChargedStart(character.pos, character.xDir, character, player, player.getNextActorNetId(), true);
		

	}

}


public class ClaudioShingetsurin : CharState {


	public ClaudioShingetsurin() : base("shoot") {

	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			if (character.bonusHealth > 0) {
				character.changeToIdleOrFall();
			} else {
			character.changeState(new ClaudioShingetsurin2(), true);
			}
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
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}




public class ClaudioShingetsurinMaverick : MaverickState {


	public ClaudioShingetsurinMaverick() : base("shoot") {

	}

	public override void update() {
		base.update();
		var character = maverick;
		if (character.isAnimOver()) {
				character.changeToIdleOrFall();	
		}
	}
	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		var character = maverick;
		new ShingetsurinProj(
			character.getShootPos(), character.xDir,
			0f, character, player, player.getNextActorNetId(), rpc: true
		);
		character.playSound("shingetsurinx5", forcePlay: false, sendRpc: true);
	}
	public override void onExit(MaverickState newState) {
		base.onExit(newState);

	}

}




public class ClaudioShingetsurin2 : CharState {


	public ClaudioShingetsurin2() : base("shoot") {

	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeState(new ClaudioChargedSlash(), true);
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
	public override void onExit(CharState? newState) {
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
	public override void onExit(CharState? newState) {
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
				if (character.bonusHealth == 0) {
								new FakeZeroSwordBeamProj(
			character.pos.addxy(0,-30), character.xDir, character,
			player.getNextActorNetId(), sendRpc: true
			);	
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




public class ClaudioChargedSlashMaverick : MaverickState {

	bool once = false;
	public ClaudioChargedSlashMaverick() : base("chargeslash") {

	}

	public override void update() {
		base.update();
		var character = maverick;
		if (character.isAnimOver()) {
			character.changeState(new MIdle());
		}

		if (character.frameIndex >= 4 && !once) {
			once = true;
			character.playSound("crash");
			new Anim(character.pos, "claudio_charge_slash_ef", character.xDir, null, true);
		}
	
	}
	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(MaverickState newState) {
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



public class BossGuard : CharState {
	public BossGuard() : base("guard") {
		invincible = true;

	}

	public override void update() {
		base.update();

		if (stateTime > 1 || stateTime == Helpers.randomRange(0, 2)) {
			character.changeToIdleOrFall();
		}
	}


	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.sprite.name.Contains("guard")) {
			character.changeSpriteFromName("block", true);
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
			if (shootNum == 0) {
				character.playSound("buster3X2", forcePlay: false, sendRpc: true);
				new FakeZeroBusterProj3(
					shootPos.Value, character.xDir, 0, character,
					 player.getNextActorNetId(), sendRpc: true
				);
			} else if (shootNum == 1) {
				character.playSound("buster3X2", forcePlay: false, sendRpc: true);
				new FakeZeroBusterProj3(
					shootPos.Value, character.xDir, 1, character,
					 player.getNextActorNetId(), sendRpc: true
				);
			} else if (shootNum == 2) {
				character.playSound("buster4X2", forcePlay: false, sendRpc: true);
				new FakeZeroSwordBeamProj(
shootPos.Value, character.xDir, character,
player.getNextActorNetId(), sendRpc: true
);

			}
			shootNum++;
			lastShootFrame = character.frameIndex;
		}

		if (shootPos != null && character.frameIndex != lastShootFrame) {

			shootNum++;
			lastShootFrame = character.frameIndex;
		}

		if (character.isAnimOver()) {
			character.changeState(new Idle());
		}
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}






public class ClaudioTrippleBusterMaverick : MaverickState {
	int shootNum;
	int lastShootFrame;
	public ClaudioTrippleBusterMaverick() : base("shoot2") {
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
	}

	public override void update() {
		base.update();
		var character = maverick;

		Point? shootPos = character.getFirstPOI();


		if (shootPos != null && character.frameIndex != lastShootFrame) {
			if (shootNum == 0) {
				character.playSound("buster3X2", forcePlay: false, sendRpc: true);
				new FakeZeroBusterProj3(
					shootPos.Value, character.xDir, 0, character,
					 player.getNextActorNetId(), sendRpc: true
				);
			} else if (shootNum == 1) {
				character.playSound("buster3X2", forcePlay: false, sendRpc: true);
				new FakeZeroBusterProj3(
					shootPos.Value, character.xDir, 1, character,
					 player.getNextActorNetId(), sendRpc: true
				);
			} else if (shootNum == 2) {
				character.playSound("buster4X2", forcePlay: false, sendRpc: true);
				new FakeZeroSwordBeamProj(
		shootPos.Value, character.xDir, character,
		player.getNextActorNetId(), sendRpc: true
			);

			}
			shootNum++;
			lastShootFrame = character.frameIndex;
		}

		if (shootPos != null && character.frameIndex != lastShootFrame) {

			shootNum++;
			lastShootFrame = character.frameIndex;
		}

		if (character.isAnimOver()) {
			character.changeState(new MIdle());
		}
	}
	public override void onExit(MaverickState newState) {
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
			float x = character.pos.x;
			float y = character.pos.y;
			if (character.bonusHealth == 0) {
				if (character is BossClaudio){
				new TriadThunderProjCharged(new Point(x, y), -1, 0, character, player, player.getNextActorNetId(), rpc: true);
				new TriadThunderProjCharged(new Point(x, y), 1, 0, character, player, player.getNextActorNetId(), rpc: true);
				}
				new TriadThunderQuake(new Point(x, y), 1, character, player, player.getNextActorNetId(), rpc: true);
			}
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
