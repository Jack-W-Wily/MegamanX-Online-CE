using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class KurumitoStandingKick : CharState {


	public KurumitoStandingKick() : base("kick_1") {

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



public class KurumitoFowardKick : CharState {


	public KurumitoFowardKick() : base("commandkick") {

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






public class KurumitosOrochinagiCharge : CharState {


	public KurumitosOrochinagiCharge() : base("orochinagi_charge") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (!player.input.isBHeld(player)) {
			character.changeState(new KurumitosOrochinagiCharged(), true);
			//this is a Release button type action so you use ! with the "isheld" input call
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

	}
	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.turnToInput(player.input, player);// this allows the char to turn midmove
	}

}



public class KurumitosOrochinagi : CharState {
	bool fired;


	public KurumitosOrochinagi() : base("orochinagi_fire") {
		landSprite = "orochinagi_fire";
		airSprite = "orochinagi_fire_air";
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("flamemOilBurn", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded || character.vel.y < 0) {
			sprite = "orochinagi_fire_air";
			character.changeSpriteFromName(sprite, true);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}





public class KurumitosOrochinagiCharged : CharState {
	bool fired;

	public Kurumitos kr = null!; // sometimes you want your character to have
								 // features tied to conditions in your charstates so you'll need this

	public KurumitosOrochinagiCharged() : base("orochinagi_fire") {
		landSprite = "orochinagi_fire";
		airSprite = "orochinagi_fire_air";
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
		invincible = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("flamemOilBurn", sendRpc: true);
			// this is what you need to make your character spawn a projectile
			new OrochinagiChargedProj(
				character.pos.addxy(30 * character.xDir, -20), character.xDir,
				kr.OverDrive, character, player, player.getNextActorNetId(), rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded || character.vel.y < 0) {
			sprite = "orochinagi_fire_air";
			character.changeSpriteFromName(sprite, true);
		}
		kr = character as Kurumitos ?? throw new NullReferenceException(); // to make sure
		// your character is corresponsive to the reference
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}







public class KurumitosShikiYamiBaraiLv1 : CharState {

	bool fired;

	public Kurumitos kr = null!;

	public KurumitosShikiYamiBaraiLv1() : base("shiki_yami_barai_melee") {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("flamemShoot", sendRpc: true);

			if (kr.OverDrive) {
				character.playSound("flamemShoot", sendRpc: true);// sound will play again
				new YamiBaraiProj(   // projectile also spawns again
				character.pos.addxy(30 * character.xDir, -20), character.xDir,
				kr.OverDrive, character, player, player.getNextActorNetId(), rpc: true
				);
			}

		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		kr = character as Kurumitos ?? throw new NullReferenceException();
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);
		
	}

}


public class KurumitosShikiYamiBaraiLv2 : CharState {

	bool fired;

	public Kurumitos kr = null!;

	public KurumitosShikiYamiBaraiLv2() : base("shiki_yami_barai") {

	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("flamemShoot", sendRpc: true);
			new YamiBaraiProj(
				character.pos.addxy(30 * character.xDir, -20), character.xDir,
				kr.OverDrive, character, player, player.getNextActorNetId(), rpc: true
			);

			if (kr.OverDrive) {
				// this is a delayed action meaning it will happen after a delayed time when triggered
				// in this case i coded for it to happen when used on Kurumito's Hypermode
				Global.level.delayedActions.Add(new DelayedAction(() => {
					character.playSound("flamemShoot", sendRpc: true);// sound will play again
					new YamiBaraiProj(   // projectile also spawns again
					character.pos.addxy(30 * character.xDir, -20), character.xDir,
					kr.OverDrive, character, player, player.getNextActorNetId(), rpc: true
					);
				}, 0.1f));
			}
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		kr = character as Kurumitos ?? throw new NullReferenceException();

	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}





public class KurumitosAirDunk : CharState {


	public KurumitosAirDunk() : base("air_dunk") {
		airMove = true; // This allows you to move Midair as if it were a normal jump
		exitOnLanding = true; //self explanatory
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





public class KurumitosDokuGami : CharState {


	public KurumitosDokuGami() : base("dokugami") {

	}



	public override void update() {
		base.update();

		if (character.sprite.frameIndex < 5) {
			float speed = 100;
			character.move(new Point(character.xDir * speed, 0));
		}

		if (character.sprite.frameIndex >= 3  && player.input.isBPressed(player)) {
			character.changeState(new KuromitosTsuyomi(), true);
		}

		if (character.isAnimOver()) {
			character.changeState(character.getFallState());
		}
	}
}

public class KuromitosTsuyomi : CharState {

	public KuromitosTsuyomi() : base("tsuyomi") {

	}



	public override void update() {
		base.update();

		if (character.sprite.frameIndex >= 3  && player.input.isBPressed(player)) {
			character.changeState(new KuromitosBatsuyomi(), true);
		}
		if (character.isAnimOver()) {
			character.changeState(character.getFallState());
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}



public class KuromitosBatsuyomi : CharState {
	
	
	bool jumpedYet;
	public KuromitosBatsuyomi() : base("batsuyomi") {

	}



	public override void update() {
		base.update();

		if (character.sprite.frameIndex < 5) {
			float speed = 100;
			character.move(new Point(character.xDir * speed, 0));
		}

		if (character.sprite.frameIndex >= 3 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			float ySpeedMod = 0.8f;
			character.vel.y = -character.getJumpPower() * ySpeedMod;
			character.playSound("jump", sendRpc: true);
		}



		if (character.isAnimOver()) {
			character.changeState(character.getFallState());
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}






public class KurumitosShikiOniaki : CharState {

	bool jumpedYet;
	public KurumitosShikiOniaki() : base("shiki_oniaki") {

	}



	public override void update() {
		base.update();

		if (character.sprite.frameIndex >= 3 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			float ySpeedMod = 0.8f;
			character.vel.y = -character.getJumpPower() * ySpeedMod;
			character.playSound("ryuenjin", sendRpc: true);
		}


		if (character.sprite.frameIndex >= 4 && character.sprite.frameIndex < 7) {
			float speed = 100;
			character.move(new Point(character.xDir * speed, 0));
		}



		if (character.isAnimOver()) {
			character.changeState(character.getFallState());
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}