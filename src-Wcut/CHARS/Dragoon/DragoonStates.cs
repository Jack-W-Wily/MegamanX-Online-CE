using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;






public class DragoonSpark : Projectile {
	public DragoonSpark(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player, "ground_spark", 0, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		isShield = true;
		isReflectShield = true;
		maxTime = 0.1f;
		projId = (int)ProjIds.DragoonSpark;
		isMelee = true;
		if (player.character != null) {
			ownerActor = player.character;
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (owner?.character != null) {
			incPos(owner.character.deltaPos);
		}
	}

	
}


public class DragoonPunchState : CharState {


	
	



	public DragoonPunchState(string transitionSprite = "")
		: base("punch", "", "", transitionSprite)
	{
	airMove = true;
	spcCancel = true;
	enterSound = "punch2";
	}



	public float pushBackSpeed;
	public override void update()
	{
	
		base.update();

		if (spcCancel) {
			attackCtrl = true;
		}
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

		if (!character.grounded) {
			sprite = "air_punch1";
			character.changeSpriteFromName("air_punch1", true);
		}	
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		
    }
}




public class DragoonLowPunchState : CharState {


	
	public float pushBackSpeed;



	public DragoonLowPunchState(string transitionSprite = "")
		: base("lowpunch1", "", "", transitionSprite)
	{
	airMove = true;
		spcCancel = true;
		enterSound = "punch1";
	}

	public override void update()
	{
	
		base.update();
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

		if (character.isAnimOver()) {
			character.changeToCrouchOrFall();
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}		

		if (!character.grounded) {
			sprite = "air_punch_low";
			character.changeSpriteFromName("air_punch_low", true);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}





public class DragoonPunchState2 : CharState {


	
	public float pushBackSpeed;



	public DragoonPunchState2(string transitionSprite = "")
		: base("punch2", "", "", transitionSprite)
	{
	airMove = true;
	spcCancel = true;
	enterSound = "punch1";
	}

	public override void update()
	{
	
		base.update();
	
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
		if (!character.grounded) {
			sprite = "air_dunk";
			character.changeSpriteFromName("air_dunk", true);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}





public class DragoonGrab : CharState {


	
	public float pushBackSpeed;



	public DragoonGrab(string transitionSprite = "")
		: base("grab", "", "", transitionSprite)
	{
	airMove = true;
	superArmor = true;
	
	}

	public override void update()
	{
	
		base.update();
	
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
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




public class DragoonKickState : CharState {


	
	public float pushBackSpeed;



	public DragoonKickState(string transitionSprite = "")
		: base("kick1", "", "", transitionSprite)
	{
	airMove = true;
	enterSound = "spinkick";
	}

	public override void update()
	{
	
		base.update();
	
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
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




public class DragoonHadouken : CharState {
	bool fired = false;


	public DragoonHadouken() : base("hadouken_idle", "", "", "") {
	superArmor = true;
	}

	public override void update() {
		base.update();

        Point projpos = character.currentFrame.POIs[0];
		if (character.frameIndex >= 5 && !fired) {
			fired = true;

			Weapon weapon = new HadoukenWeapon(player);
	
			new SpeedBurnerProj(character.pos.addxy(20,-35), character.xDir,character, player, player.getNextActorNetId(), true);
			
			if (invincible || character.iframesTime > 0) {
					Global.level.delayedActions.Add(new DelayedAction(() => {
				new SpeedBurnerProj(character.pos.addxy(20,-35),  character.xDir,character, player, player.getNextActorNetId(), true);
				}, 0.1f));
			}
			character.playSound("speedBurner", sendRpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopCharge();
	}

	public override void onExit(CharState? newState) {
	base.onExit(newState);
	}
}



public class DragoonHadoukenCrouch : CharState {
	bool fired = false;


	public DragoonHadoukenCrouch() : base("hadouken_crouch", "", "", "") {
	
	}

	public override void update() {
		base.update();


         Point projpos = character.currentFrame.POIs[0];
		if (character.frameIndex >= 3 && !fired) {
			fired = true;

			Weapon weapon = new HadoukenWeapon(player);

			new SpeedBurnerProj(character.pos.addxy(20,-15),  character.xDir,character, player, player.getNextActorNetId(), true);
			if (invincible || character.iframesTime > 0) {
					Global.level.delayedActions.Add(new DelayedAction(() => {
				new SpeedBurnerProj(character.pos.addxy(20,-15),  character.xDir,character, player, player.getNextActorNetId(), true);
				}, 0.1f));
			}
			character.playSound("speedBurner", sendRpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopCharge();
	}

	public override void onExit(CharState? newState) {
	base.onExit(newState);
	}
}







public class DragoonRisingFire : CharState {

	bool sound;
	float projCD;
	public RekkohaEffect? effect;

	public DragoonRisingFire( ) : base("risingfire", "", "", "") {
		
		invincible = true;
		enterSound = "dropkick";
	}

	public override void update() {
		base.update();

		float topScreenY = Global.level.getTopScreenY(character.pos.y);

        projCD += Global.spf;

		if (character.frameIndex == 4 && !sound) {
			sound = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crashX2", sendRpc: true);
		}

        if (character.frameIndex >= 6) {
		    if (projCD > 0.1f){
                character.playSound("speedBurner", sendRpc: true);
                projCD = 0;
            	new RisingFireProj(new RisingFire(), character.pos.addxy(-5,-43), character.xDir, player, player.getNextActorNetId(), true);
           }
       
        }

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (player.isMainPlayer) {
			effect = new RekkohaEffect();
		}

		character.playSound("ching", sendRpc: true);
		new GigaCrushBackwall(character.pos, character);
		new HitStop(character.pos, player, player.getNextActorNetId(), 
		player.ownedByLocalPlayer, overrideTime: 0.3f, sendRpc: true);
	}


    
}

public class DragoonSpitFire : CharState {

	bool sound;
	float projCD;
	public RekkohaEffect? effect;

	public DragoonSpitFire( ) : base("spit", "", "", "") {
		
		invincible = true;
		enterSound = "Rooster - FireCharge";
	}

	public override void update() {
		base.update();

		float topScreenY = Global.level.getTopScreenY(character.pos.y);

        projCD += Global.spf;

		if (character.frameIndex == 4 && !sound) {
			sound = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crashX2", sendRpc: true);
		}

        if (character.frameIndex >= 9) {
		    if (projCD > 0.2f){
                character.playSound("speedBurner", sendRpc: true);
              new SpeedBurnerProj(character.pos.addxy(15,-25), character.xDir,character, player, player.getNextActorNetId(), true);
			    projCD = 0;
              }
          
        }

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (player.isMainPlayer) {
			effect = new RekkohaEffect();
		}

		character.playSound("ching", sendRpc: true);
		new GigaCrushBackwall(character.pos, character);
		new HitStop(character.pos, player, player.getNextActorNetId(), 
		player.ownedByLocalPlayer, overrideTime: 0.3f, sendRpc: true);
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}



public class DragoonRising : CharState {
	private bool jumpedYet;
	private bool fired = false;

	private float timeInWall;

	private Projectile? proj;

    public DragoonRising() : base("shoryuken") {
		//superArmor = true;
		useDashJumpSpeed = true;
		enterSound = "Rooster - FireCharge2";
	}

	public override void update() {
		base.update();
		
		int xDir = character.xDir;
		Point pos = character.pos;
		Player player = character.player;

		if (character.sprite.frameIndex >= 1 && !jumpedYet) {
			jumpedYet = true;
			character.vel.y = -character.getJumpPower();
			character.useGravity = true;
		} 
		
		if (character.vel.y < 0) character.move(new Point(character.xDir * 165, 0f));

		if (character.currentFrame.getBusterOffset() != null) {
			Point poi = character.currentFrame.POIs[0];
			Point firePos = character.pos.addxy(poi.x * (float)character.xDir, poi.y);

			if (proj == null) {
				if (!character.isUnderwater()){
					proj = new RisingFireProjChargedStart(
						new RisingFire(), pos, xDir, player, player.getNextActorNetId(), true
					);
				} else {
					proj = new RisingFireProjChargedStart(
						new RisingFire(), pos, xDir, player, player.getNextActorNetId(), true
					);
				}
			//	proj.releasePlasma = player.hasPlasma();
			}
			else proj.changePos(firePos);
		}

		else if (character.sprite.frameIndex == 3 && proj != null) {
			proj.destroySelf();
			proj = null!;
		}
		
		CollideData? wallAbove = Global.level.checkTerrainCollisionOnce(character, 0, -10);
		
		if (wallAbove != null && wallAbove.gameObject is Wall) {
			timeInWall++;
			if (timeInWall > 6) {
				character.vel.y = 1;
				character.changeToIdleOrFall();
				return;
			}
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		if (character.frameIndex > 3 && !fired) {
			fired = true;
			releaseProj();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.dashedInAir = 0;
		character.stopMoving();
		character.useGravity = false;
		if (!character.grounded) {
			character.frameIndex = 2;
			character.frameTime = 2;
		}
	}
	

	public override void onExit(CharState? newState) {
		base.onExit(newState);
			character.useGravity = true;
		if (proj != null) {
			proj.destroySelf();
			if (!fired) releaseProj();
		} 
	}

	void releaseProj() {
		Projectile? rf;
		Point shootPos = character.getShootPos();
		int xDir = character.xDir;

		if (!character.isUnderwater()) {
			rf = new RisingFireProjCharged(
				new RisingFire(), shootPos, xDir, player, 
				player.getNextActorNetId(), rpc: true
			);
		} else {
			rf = new RisingFireWaterProjCharged(
				new RisingFire(), shootPos, xDir, player, 
				player.getNextActorNetId(), rpc: true
			);
		}
	}
}


public class DragoonShoryuken : CharState {
	bool jumpedYet;
	float timeInWall;
	bool isUnderwater;
	Anim? anim;
	float projTime;


	public DragoonShoryuken(bool isUnderwater) : base("shoryuken", "", "") {
		this.isUnderwater = isUnderwater;
		superArmor = true;
		enterSound = "Rooster - FireCharge2";
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && anim != null) {
			anim.visible = false;
		}

		if (character.sprite.frameIndex >= 2 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			character.vel.y = -character.getJumpPower() * 1.55f;
			character.playSound("ryuenjin", sendRpc: true);
		}
		if (character.sprite.frameIndex >= 2 && character.currentFrame.POIs.Length > 0) {
			character.move(new Point(character.xDir * 265, 0));
			Point poi = character.currentFrame.POIs[0];
			Point firePos = character.pos.addxy(poi.x * character.xDir, poi.y);
			if (anim == null) {
				anim = new Anim(firePos, "magmadragoon_shoryuken_flame", character.xDir, player.getNextActorNetId(), false, sendRpc: true);
			} else {
				anim.changePos(firePos);
			}
		} 
		else if (character.sprite.frameIndex > 7 && !character.sprite.name.Contains("finisher")) {
			if (anim != null) {
				anim.destroySelf();
				anim = null;
			}
			character.changeSpriteFromName("shoryuken_finisher", true);
		}

		if (!isUnderwater) {
			projTime += Global.spf;
			if (projTime > 0.06f) {
				projTime = 0;
				var anim = new Anim(character.getCenterPos(), "shoryuken_fade", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
				anim.vel = new Point(-character.xDir * 50, 25);
			}
		}

		var wallAbove = Global.level.checkTerrainCollisionOnce(character, 0, -10);
		if (wallAbove != null && wallAbove.gameObject is Wall) {
			timeInWall += Global.spf;
			if (timeInWall > 0.1f) {
				character.changeSpriteFromName("shoryuken_finisher", true);
				return;
			}
		}

		if (character.isAnimOver()) {
			character.changeState(new Fall());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState? newState) {
		if (anim != null) {
			anim.destroySelf();
			anim = null;
		}
		base.onExit(newState);
	}
}





public class DragoonShoryukenWeak : CharState {
	bool jumpedYet;
	float timeInWall;
	bool isUnderwater;
	Anim? anim;
	float projTime;


	public DragoonShoryukenWeak(bool isUnderwater) : base("shoryuken", "", "") {
		this.isUnderwater = isUnderwater;
		superArmor = true;
		enterSound = "Rooster - FireCharge2";
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && anim != null) {
			anim.visible = false;
		}

		if (character.sprite.frameIndex >= 2 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			character.vel.y = -character.getJumpPower() * 1.2f;
			character.playSound("ryuenjin", sendRpc: true);
		}
		if (character.sprite.frameIndex >= 2 && character.currentFrame.POIs.Length > 0) {
			character.move(new Point(character.xDir * 265, 0));
			Point poi = character.currentFrame.POIs[0];
			Point firePos = character.pos.addxy(poi.x * character.xDir, poi.y);
			if (anim == null) {
				anim = new Anim(firePos, "magmadragoon_shoryuken_flame", character.xDir, player.getNextActorNetId(), false, sendRpc: true);
			} else {
				anim.changePos(firePos);
			}
		} 
		else if (character.sprite.frameIndex > 7 && !character.sprite.name.Contains("finisher")) {
			if (anim != null) {
				anim.destroySelf();
				anim = null;
			}
			
		}

		if (!isUnderwater) {
			projTime += Global.spf;
			if (projTime > 0.06f) {
				projTime = 0;
				var anim = new Anim(character.getCenterPos(), "shoryuken_fade", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
				anim.vel = new Point(-character.xDir * 50, 25);
			}
		}

		var wallAbove = Global.level.checkTerrainCollisionOnce(character, 0, -10);
		if (wallAbove != null && wallAbove.gameObject is Wall) {
			timeInWall += Global.spf;
			if (timeInWall > 0.1f) {
				character.changeState(new Fall());
				return;
			}
		}

		if (character.isAnimOver()) {
			character.changeState(new Fall());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState? newState) {
		if (anim != null) {
			anim.destroySelf();
			anim = null;
		}
		base.onExit(newState);
	}
}





public class DragoonSpinkick : CharState {
	
	
	public float soundTime = 0;

	public DragoonSpinkick() : base("spinkick", "", "", "") {
	
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (stateTime > 0.2f){
		character.move(new Point(character.xDir * 250, 0));
		}
		soundTime -= Global.speedMul;
		if (soundTime <= 0) {
			soundTime = 9;
			character.playSound("spinkick", sendRpc: true);
		}
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




public class DragoonUppercut : CharState {


	public DragoonUppercut() : base("uppercut") {
		wiffCancel = true;
		canSpecialCancel = true;
		enterSound = "punch2";
	}

	bool shotOnce;

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		if (player.input.isBPressed(player) && !shotOnce && character.frameIndex >= 1) {
			   character.playSound("speedBurner", sendRpc: true);
            	new RisingFireProj(new RisingFire(), character.pos.addxy(30 * character.xDir,-43), character.xDir, player, player.getNextActorNetId(), true);
			shotOnce = true;
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}



public class DragoonSenpukiaku : CharState {
	

	public DragoonSenpukiaku() : base("senpukiaku", "", "", "") {
	
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.frameIndex > 2  && character.frameIndex <11){
		character.move(new Point(character.xDir * 350, 0));
		}
	    if (character.isAnimOver()) {
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



public class RagingDemon : CharState {
	

	public RagingDemon() : base("ragingdemon", "", "", "") {
		superArmor = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

	
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





public class DragoonDiveKick : CharState {
	float stuckTime;
	float diveTime;
	

	public DragoonDiveKick() : base("dropkick") {
		superArmor = true;
	}

	public override void update() {
		if (character.frameIndex >= 3 && !once) {
			character.vel.x = character.xDir * 300;
			character.vel.y = 450;
			character.playSound("punch2", sendRpc: true);
			once = true;
		}
		base.update();
		if (!once) {
			return;
		}
		if (character.vel.y < 100) {
			character.changeToLandingOrFall();
			return;
		}
		CollideData? hit = Global.level.checkTerrainCollisionOnce(
			character, character.vel.x * Global.spf, character.vel.y * Global.spf
		);
		if (hit?.isSideWallHit() == true) {
			character.changeState(new Fall(), true);
			return;
		} else if (hit != null) {
			stuckTime += Global.speedMul;
			if (stuckTime >= 6) {
				character.changeToLandingOrFall();
				return;
			}
		}
		if (character.grounded || diveTime >= 6f && character.deltaPos.y == 0) {
			character.changeToLandingOrFall();

			return;
		}
		diveTime += Global.spf;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	    character.stopMoving();
		character.useGravity = false;
	
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		if (character.grounded) {
			 new DragoonSpark(new SpeedBurner(), character.pos, character.xDir, player,  
				 player.getNextActorNetId(), rpc : true);
		}
		character.stopMoving();
	}
}






public class DragoonGrabbed : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;


	 Character? grabber;
	Anim? anim;
	public DragoonGrabbed(Character Grabber, int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 300;
		flinchTime = 0.5f;
		grabber = Grabber;
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -300;
	}

	public override void update() {
		base.update();

		if (anim == null) {
				anim = new Anim(character.getCenterPos(), "magmadragoon_ball_proj", character.xDir, player.getNextActorNetId(), false, sendRpc: true);
			} else {
				anim.changePos(character.getCenterPos());
			}

		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		

		if (stateTime >= 2 || character.grounded) {
			if (grabber != null){
			character.playSound("flamemOilBurn", sendRpc: true);
			new InfernoBeam(new FireWave(),
						character.pos, character.xDir,
						grabber.player, grabber.player.getNextActorNetId(),
						sendRpc: true
					);
			}
			character.changeState(new KnockedDown(character.xDir), true);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		anim?.destroySelf();
	}
}






public class InfernoBeam : Projectile {
	Player player;
	public InfernoBeam(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 2, player, "zerox1_firebeam_up", Global.superFlinch, 2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.InfernoBeam;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		damager.damage = 2;
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

