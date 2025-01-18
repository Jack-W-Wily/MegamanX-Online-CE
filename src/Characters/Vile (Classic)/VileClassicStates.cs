using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;


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
		bool isStunShot = true;
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
		Point shootVel = vile.getVileShootVel(isStunShot);
	

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
			 shootPos, vile.xDir, 0, vile.player,
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
		if (vile.vileForm == 0){
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.GoGetterRight), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
		} else {
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.InfinityGig), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
		}
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



public class BecomeMk2 : CharState {
	public float radius = 200;
	Anim drDopplerAnim;
	bool isMK5;
	VileClassic vile;

	public BecomeMk2(bool isMK5) : base(isMK5 ? "revive_to5" : "revive") {
		invincible = true;
		this.isMK5 = isMK5;
	}

	public override void update() {
		base.update();
		if (radius >= 0) {
			radius -= Global.spf * 150;
		}
		if (character.frameIndex < 2) {
			if (Global.frameCount % 4 < 2) {
				character.addRenderEffect(RenderEffectType.Flash);
			} else {
				character.removeRenderEffect(RenderEffectType.Flash);
			}
		} else {
			character.removeRenderEffect(RenderEffectType.Flash);
		}
		if (character.frameIndex == 7 && !once) {
			character.playSound("ching");
			player.health = 1;
			character.addHealth(player.maxHealth);
			once = true;
		}
		if (character.ownedByLocalPlayer) {
			if (character.isAnimOver()) {
				setFlags();
				character.changeState(new Fall(), true);
			}
		} else if (character?.sprite?.name != null) {
			if (!character.sprite.name.EndsWith("_revive") && !character.sprite.name.EndsWith("_revive_to5") && radius <= 0) {
				setFlags();
				character.changeState(new Fall(), true);
			}
		}
	}

	public void setFlags() {
		if (!isMK5) {
			vile.vileForm = 1;
		} else {
			vile.vileForm = 2;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as VileClassic ?? throw new NullReferenceException();
		//character.setzIndex(ZIndex.Foreground);
		character.playSound("revive");
		character.addMusicSource("demo_X3", character.getCenterPos(), false, loop: false);
		if (!isMK5) {
			drDopplerAnim = new Anim(character.pos.addxy(30 * character.xDir, -15), "drdoppler", -character.xDir, null, false);
			drDopplerAnim.blink = true;
		} else {
			if (vile.linkedRideArmor != null) {
				vile.linkedRideArmor.ownedByMK5 = true;
			}
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		setFlags();
		character.removeRenderEffect(RenderEffectType.Flash);
		Global.level.delayedActions.Add(new DelayedAction(() => { character.destroyMusicSource(); }, 0.75f));

		drDopplerAnim?.destroySelf();
		if (character != null) {
			character.invulnTime = 0.5f;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		if (!character.ownedByLocalPlayer) return;

		if (radius <= 0) return;
		Point pos = character.getCenterPos();
		DrawWrappers.DrawCircle(pos.x + x, pos.y + y, radius, false, Color.White, 5, character.zIndex + 1, true, Color.White);
	}
}
