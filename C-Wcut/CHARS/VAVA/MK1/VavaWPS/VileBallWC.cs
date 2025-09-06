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






public class SplashHitState : CharState {
	bool shot;
	
	float shootTime;
	int shootCount;

		Vile vile = null!;
	Point counterAttackPos;


	public SplashHitState(string transitionSprite = "") :
		base(getSprite(), "", "", transitionSprite) {

	}

	public static string getSprite() {
		return"kick_3";
	}

	public override void update() {
		base.update();
		Projectile proj;
	
		
		character.slideVel = character.xDir * character.getDashSpeed();	
		
			if (!shot && character.sprite.frameIndex == 4) {
				shot = true;
				//vile.setVileShootTime(vile.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				
					proj = new SplashHitProj(
			character.pos, character.xDir, character, character.player,
			character.player.getNextActorNetId(), rpc: true
		);
				
			}
		
		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}




public class BumptyBoomGranadeLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public BumptyBoomGranadeLaunch(string transitionSprite = "") : base("crouch_nade", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

			var poi = character.getFirstPOI();
			if (!once && poi != null) {
				once = true;
				var proj = new BumptyBoomProj(vile.napalmWeapon, poi.Value, character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
				proj.vel = new Point(character.xDir * 200, -200);
			}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		}

	

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class BumptyBoomProj : Projectile {
		public IDamagable target;
	bool exploded;
	public BumptyBoomProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 150, 2, player, "napalm_grenade", 0, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.NapalmGrenade;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		shouldShieldBlock = false;
	}

	public override void update() {
		base.update();
		if (grounded) {
			explode();
		}
	}

	public override void onHitWall(CollideData other) {
		xDir *= -1;
		explode();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (ownedByLocalPlayer) explode();
	}

	public void explode() {
		if (exploded) return;
		exploded = true;
		if (ownedByLocalPlayer) {
			new GrenadeExplosionProj(
				weapon, pos, xDir, owner, 1, target, Math.Sign(vel.x), owner.getNextActorNetId()
			);
		}
		destroySelf();
	}
}





public class AirSplashHitGranadeLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public AirSplashHitGranadeLaunch(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

			var poi = character.getFirstPOI();
			if (!once && poi != null) {
				once = true;
				var proj = new SplashHitGrenadeProj(
					poi.Value, character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				);
				proj.vel = new Point(character.xDir * 100, 0);
			}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		}

	

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class AirFireNadeLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public AirFireNadeLaunch(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

			var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			var proj = new MK2NapalmGrenadeProj(
					poi.Value, character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				); 
			proj.vel = new Point(character.xDir * 100, 0);
				character.playSound("FireNappalmMK2", forcePlay: false, sendRpc: true);
						
			}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		}

	

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




