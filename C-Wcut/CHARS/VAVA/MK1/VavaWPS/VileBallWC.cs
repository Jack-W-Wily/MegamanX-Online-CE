using System;
using System.Collections.Generic;

namespace MMXOnline;


public class ExplosiveRoundState : CharState {
	int bombNum;
	bool isNapalm;

	Character vile;

	public ExplosiveRoundState() : base("air_bomb_attack", "", "") {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		

			if (bombNum > 0 && player.input.isBPressed(player)) {
				character.changeState(new Fall(), true);
			}

			var inputDir = player.input.getInputDir(player);
			if (inputDir.x == 0) inputDir.x = character.xDir;
			if (stateTime > 0f && bombNum == 0) {
				bombNum++;
				new VileBombProj(
				character.pos, (int)inputDir.x, 0, vile, player,
				character.player.getNextActorNetId(), rpc: true);
			}
		if (stateTime > 0.23f && bombNum == 1) {
			character.changeState(new Fall(), true);


			bombNum++;
			new VileBombProj(
			character.pos, (int)inputDir.x, 0, vile, player,
				character.player.getNextActorNetId(), rpc: true);
		}
		if (stateTime > 0.45f && bombNum == 2) {
			character.changeState(new Fall(), true);

			bombNum++;
			new VileBombProj(
			character.pos, (int)inputDir.x, 0, vile, player,
			character.player.getNextActorNetId(), rpc: true);
		}

			if (stateTime > 0.68f) {
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



public class PeaceOutRollerAttack : CharState {
	int bombNum;
	bool isNapalm;
	Character vile;

	public PeaceOutRollerAttack() : base("air_bomb_attack", "", "") {
	
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		
			if (stateTime > 0f && bombNum == 0) {
				bombNum++;
					new PeaceOutRollerProj(
						character.getCenterPos().addxy(20*character.xDir,0), character.xDir, 1, vile, player, 
						character.player.getNextActorNetId(), rpc: true
					);
				}

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


public class SpreadShotKnee : CharState {
	int bombNum;
	Character vile;

	public SpreadShotKnee( ) : base("air_bomb_attack", "", "") {
	
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var ebw = new VileElectricBomb();
		if (bombNum > 0 && player.input.isBPressed(player)) {
			character.changeToIdleOrFall();
			return;
		}

		for (int i = 0; i < 7; i++) {
			if (stateTime > i * 0.1f && bombNum == i) {
				bombNum++;
				new StunShotProj2(
					character.pos, character.xDir, i + 1, 0, vile,
					character.player, character.player.getNextActorNetId(), rpc: true
				);
			}
		}
			
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
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


