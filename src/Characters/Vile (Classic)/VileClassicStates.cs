using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;





public class StunShotAttack : CharState {
	
	private VileClassic vile = null!;

	public StunShotAttack( bool grounded) : base(getSprite( grounded)) {
	airMove = true;
	}

	public static string getSprite( bool grounded) {
	
		return grounded ? "idle_shoot" : "cannon_air";
	}

	public override void update() {
		base.update();

	
		//groundCodeWithMove();

		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public static void shootLogic(VileClassic vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) {
			return;
		}
		Point shootVel = vile.getVileShootVel(true);

		var player = vile.player;
		vile.playSound("frontrunner", sendRpc: true);

		string muzzleSprite = "cannon_muzzle";
	
		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		if (vile.sprite.name.EndsWith("_grab")) {
			shootPos = vile.getFirstPOIOrDefault("s");
		}

		var muzzle = new Anim(
			shootPos, muzzleSprite, vile.getShootXDir(), player.getNextActorNetId(), true, true, host: vile
		);
		muzzle.angle = new Point(shootVel.x, vile.getShootXDir() * shootVel.y).angle;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}

			new StunShotProj(new VileMissile(VileMissileType.ElectricShock),
			 shootPos, vile.getShootXDir(), 0, vile.player,
			  vile.player.getNextActorNetId(), shootVel, rpc: true);
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as VileClassic ?? throw new NullReferenceException();
		shootLogic(vile);
	//	character.useGravity = false;
	//	character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class RocketPunchAttackVC : CharState {
	bool shot = false;
	RocketPunchProj? proj;
	float specialPressTime;
	VileClassic vile = null!;

	public RocketPunchAttackVC() : base("rocket_punch", "", "") {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 1) {
			shoot();
		}
		if (proj != null) {
				if (proj.returned || proj.destroyed) {
					character.changeToIdleOrFall();
					return;
				}
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 1;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.GoGetterRight), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as VileClassic ?? throw new NullReferenceException();
	}
}





public class ExplosiveRoundVC : CharState {
	int bombNum;
	bool isNapalm;
	VileClassic vile = null!;

	public ExplosiveRoundVC() : base("air_bomb_attack", "", "") {
		this.isNapalm = isNapalm;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

	
		if (bombNum > 0 && player.input.isPressed(Control.Special1, player)) {
				character.changeState(new Fall(), true);
				return;
			}

			var inputDir = player.input.getInputDir(player);
			if (inputDir.x == 0) inputDir.x = character.xDir;
			if (stateTime > 0f && bombNum == 0) {
				bombNum++;
				new VileBombProj(new VileBall(VileBallType.ExplosiveRound), character.pos, (int)inputDir.x, player, 0, character.player.getNextActorNetId(), rpc: true);
			}
			if (stateTime > 0.23f && bombNum == 1) {
			
				bombNum++;
				new VileBombProj(new VileBall(VileBallType.ExplosiveRound), character.pos, (int)inputDir.x, player, 0, character.player.getNextActorNetId(), rpc: true);
			}
		

			if (stateTime > 0.68f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as VileClassic ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}
