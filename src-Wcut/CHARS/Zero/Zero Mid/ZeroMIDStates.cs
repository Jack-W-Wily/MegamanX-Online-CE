using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;


public class ZeroGrabStart : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public ZeroGrabStart(string transitionSprite = "")
		: base("grab_start", "", "", transitionSprite) {
	}

	public override void update() {

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
			character.stopMoving();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class ZeroGrabEX : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public float soundCooldown;

	public ZeroGrabEX(string transitionSprite = "")
		: base("grab_ex", "", "", transitionSprite) {
		airMove = true;
	}

	public override void update() {
		Helpers.decrementTime(ref soundCooldown);
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

		if (character.frameIndex == 0 && soundCooldown == 0) {
			soundCooldown = 0.1f;
			character.playSound("buster2", sendRpc: true);
		}

		if (stateTime > 0.5f && !character.sprite.name.Contains("end")) {
			character.changeSpriteFromName("grab_ex_end", true);
		}



		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}








public class ZeroFinalStart : CharState {
	Anim? proj;

	public ZeroFinalStart() : base("final_start", "", "", "") {
		invincible = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && proj != null) {
			proj.destroySelf();
			proj = null;
		}

		character.move(new Point(character.xDir * 350, 0));

		 if (stateTime > 0.6f) {
			character.changeToIdleOrFall();
			return;
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




public class ZeroFinalEnd : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public ZeroFinalEnd(Character? victim) : base("final_end", "", "", "") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		//if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("knocked_down")) {
		//	character.changeToIdleOrFall();
		//	return;
		//}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.33f) {
					leechTime = 0;
				}
				return;
			}
		}

			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();
			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);
			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));

		
	if (!player.input.isHeld(Control.Down, player)
			&& player.input.isPressed(Control.Shoot, player)) {

			if (Helpers.randomRange(0,3) == 0 && leechTime > 0.05f){
			character.changeSpriteFromName("attack", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 1  && leechTime > 0.05f){
			character.changeSpriteFromName("attack_2", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 2 && leechTime > 0.05f){
			character.changeSpriteFromName("attack_air", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 3 && leechTime > 0.05f){
			character.changeSpriteFromName("attack_3", true);
			leechTime = 0;
			}
			
		}

			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {
			character.changeSpriteFromName("hyouretsuzan_fall", true);	
		}


		

		if (player.input.isPressed(Control.Special1, player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
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
		victim.grabInvulnTime = 0.5f;
		 
	}
}








public class ZInferno : CharState {
	public int shootInterval = 12;
	public int distance = 36;
	public int shotMax = 4;
	public int rumbleMax = 7;
	public int rumbleNum;
	public int shotNum;
	public float shootTimer;
	public bool shotModeActive;
	public RekkohaEffect? effect;
	public Weapon weapon;
	public int attackMaxTime = 118;
	public bool exiting;

	public ZInferno(Weapon weapon) : base("rekkoha") {
		this.weapon = weapon;
		invincible = true;
		stunImmune = true;
		pushImmune = true;
	}

	public override void update() {
		weapon.shootCooldown = weapon.fireRate;
		base.update();
		if (exiting) {
			if (character.isAnimOver()) {
				character.changeToIdleOrFall();
			}
			return;
		}
		if (shotModeActive) {
			if (shootTimer >= shootInterval) {
				if (shotNum < shotMax) {
					shotNum++;
					float topScreenY = Global.level.getTopScreenY(character.pos.y);
					float distXL = character.pos.x + distance * shotNum * -1;
					float distXR = character.pos.x + distance * shotNum * 1;


					if (Helpers.randomRange(0,1) == 0){
					new ZInfernoBeam(new FireWave(),
						new Point(distXL, character.pos.y), character.xDir,
						player, player.getNextActorNetId(),
						sendRpc: true
					);
					} else {
					new ZInfernoBeam2(new FireWave(),
						new Point(distXL, character.pos.y), -character.xDir,
						player, player.getNextActorNetId(),
						sendRpc: true
					);
					}
					if (Helpers.randomRange(0,1) == 0){
					new ZInfernoBeam2(new FireWave(),
						new Point(distXR, character.pos.y), character.xDir,
						player, player.getNextActorNetId(),
						sendRpc: true
					);
					} else {
					new ZInfernoBeam2(new FireWave(),
						new Point(distXR, character.pos.y), -character.xDir,
						player, player.getNextActorNetId(),
						sendRpc: true
					);
					}
				}
				if (rumbleNum < rumbleMax) {
					character.shakeCamera(sendRpc: true);
					rumbleNum++;
				}
				shootTimer = 0;
			} else {
				shootTimer += character.speedMul;
			}
		}
		if (character.frameIndex >= 6 && !shotModeActive) {
			shotModeActive = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("rekkoha", sendRpc: true);
			character.playSound("crashX2", sendRpc: true);
			float topScreenY = Global.level.getTopScreenY(character.pos.y);
			new ZInfernoBeam(new FireWave(),
						new Point(character.pos.x, character.pos.y), character.xDir,
						player, player.getNextActorNetId(),
						sendRpc: true
					);
		}
		if (stateFrames >= attackMaxTime) {
			exiting = true;
			character.changeSpriteFromName("giga_end", true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.clenaseAllDebuffs();
		if (player.isMainPlayer) {
			effect = new RekkohaEffect();
		}
	}
}



public class ZInfernoBeam2 : Projectile {
	Player player;
	public ZInfernoBeam2(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 2, player, "zerox1_firebeam_down", Global.superFlinch, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ZinfernoBeam2;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		damager.damage = 6;
		maxTime = 1f;
		hitSound = "kofhtsnd_lightning1";
		this.player = player;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
	}

	public override bool shouldDealDamage(IDamagable damagable) {
	
		return true;
	}
}
	



public class ZInfernoBeam : Projectile {
	Player player;
	public ZInfernoBeam(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 2, player, "zerox1_firebeam_up", Global.superFlinch, 2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ZInfernoBeam;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		damager.damage = 6;
		maxTime = 1f;
		hitSound = "kofhtsnd_lightning1";
		this.player = player;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
	}

	public override bool shouldDealDamage(IDamagable damagable) {
	
		return true;
	}

	
}

