using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;






public class DragoonSpark : Projectile {
	public DragoonSpark(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player, "ground_spark", 4, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		isJuggleProjectile = true;
		ShouldClang = true;
		isShield = true;
		isReflectShield = true;
		maxTime = 0.1f;
		projId = (int)ProjIds.DragoonSpark;
		isMelee = true;
		if (player.character != null) {
			owningActor = player.character;
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


	
	public float pushBackSpeed;



	public DragoonPunchState(string transitionSprite = "")
		: base("punch", "", "", transitionSprite)
	{
	airMove = true;
	normalCtrl = true;
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}		
	}

	public override void onExit(CharState newState) {
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
	normalCtrl = true;
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}		
	}

	public override void onExit(CharState newState) {
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}		
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




public class DragoonKickState : CharState {


	
	public float pushBackSpeed;



	public DragoonKickState(string transitionSprite = "")
		: base("kick_air", "", "", transitionSprite)
	{
	airMove = true;
	normalCtrl = true;
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}		
	}

	public override void onExit(CharState newState) {
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
	
			new SpeedBurnerProj(new SpeedBurner(),  character.pos.addxy(20,-35), character.xDir, player, player.getNextActorNetId(), true);
			if (character.frameIndex >= 6)new SpeedBurnerProj(new SpeedBurner(),  character.pos.addxy(20,-35), character.xDir, player, player.getNextActorNetId(), true);
			
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

	public override void onExit(CharState newState) {
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

			new SpeedBurnerProj(new SpeedBurner(), character.pos.addxy(20,-15),  character.xDir, player, player.getNextActorNetId(), true);
			
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

	public override void onExit(CharState newState) {
	base.onExit(newState);
	}
}







public class DragoonRisingFire : CharState {

	bool sound;
	float projCD;
	public RekkohaEffect? effect;

	public DragoonRisingFire( ) : base("risingfire", "", "", "") {
		
		invincible = true;
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
		    if (projCD > 0.2f){
                character.playSound("speedBurner", sendRpc: true);
                projCD = 0;
            	new RisingFireProj(new RisingFire(), character.pos.addxy(-5,-43), character.xDir, player, player.getNextActorNetId(), true);
           }
       
        }

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}


    
}

public class DragoonSpitFire : CharState {

	bool sound;
	float projCD;
	public RekkohaEffect? effect;

	public DragoonSpitFire( ) : base("spit", "", "", "") {
		
		invincible = true;
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
              new SpeedBurnerProj(new SpeedBurner(),  character.pos.addxy(15,-25), character.xDir, player, player.getNextActorNetId(), true);
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
	}

	public override void onExit(CharState newState) {
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
		character.stopMovingWeak();
		character.useGravity = false;
		if (!character.grounded) {
			character.frameIndex = 2;
			character.frameTime = 2;
		}
	}
	

	public override void onExit(CharState newState) {
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
		if (character.sprite.frameIndex == 2 && character.currentFrame.POIs.Length > 0) {
			character.move(new Point(character.xDir * 265, 0));
			Point poi = character.currentFrame.POIs[0];
			Point firePos = character.pos.addxy(poi.x * character.xDir, poi.y);
			if (anim == null) {
				anim = new Anim(firePos, "magmadragoon_shoryuken_flame", character.xDir, player.getNextActorNetId(), false, sendRpc: true);
			} else {
				anim.changePos(firePos);
			}
		} else if (character.sprite.frameIndex > 2) {
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

	public override void onExit(CharState newState) {
		if (anim != null) {
			anim.destroySelf();
			anim = null;
		}
		base.onExit(newState);
	}
}



public class DragoonSenpukiaku : CharState {
	

	public DragoonSenpukiaku() : base("spinkick", "", "", "") {
	
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

	public override void onExit(CharState newState) {
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

	public override void onExit(CharState newState) {
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
		CollideData hit = Global.level.checkTerrainCollisionOnce(
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
		if (character.grounded || diveTime >= 6 && character.deltaPos.y == 0) {
			character.changeToLandingOrFall();
			return;
		}
		diveTime += Global.spf;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	    character.stopMovingWeak();
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.stopMovingWeak();
	}
}