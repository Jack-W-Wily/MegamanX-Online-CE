using System;
using System.Collections.Generic;

namespace MMXOnline;

public class VileMK2Grab : Weapon {
	public VileMK2Grab() : base() {
		fireRate = 45;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}

public class VileMK2GrabState : CharState {
	public Character? victim;
	float leechTime = 1;
	public SoundWrapper sound;
	public Point flyVel;
	float flyVelAcc = 500;
	float flyVelMaxSpeed = 200;
	public float fallY;
	Vile vile = null!;

	bool violentcrusherspawn; 
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;

	public VileMK2GrabState(Character? victim) : base("grab") {
		this.victim = victim;
		airMove = true;
		grabTime = VileMK2Grabbed.maxGrabTime;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();


		if (vile.vileForm == 2){
	//	character.useGravity = false;
		if (player.speedDevil) {
			flyVelMaxSpeed *= 1.1f;
			flyVelAcc *= 1.1f;
		}

		float flyVelX = 0;
		if (character.isDashing && character.deltaPos.x != 0) {
			flyVelX = character.xDir * character.getDashSpeed() * 0.5f;
		} else if (character.deltaPos.x != 0) {
			flyVelX = character.xDir * character.getRunSpeed() * 0.5f;
		}

		float flyVelY = 0;
		if (character.vel.y < 0) {
			flyVelY = character.vel.y;
		}

		flyVel = new Point(flyVelX, flyVelY);
		if (flyVel.magnitude > flyVelMaxSpeed) flyVel = flyVel.normalize().times(flyVelMaxSpeed);

		if (character.vel.y > 0) {
			fallY = character.vel.y;
		}

		character.isDashing = false;
		character.stopMoving();
		}
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		


			//if (vile.vileForm == 2){
				if (vile.vileHoverTime > vile.vileMaxHoverTime) {
				vile.vileHoverTime = vile.vileMaxHoverTime;
				character.changeToIdleOrFall();
				return;
				}
				for (int i = 1; i <= 4; i++) {
				CollideData collideData = Global.level.checkTerrainCollisionOnce(character, 0, -10 * i, autoVel: true);
				if (!character.grounded && collideData != null && collideData.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
					if (!violentcrusherspawn){
					character.shakeCamera(sendRpc: true);
					character.playSound("crash", true, true);
					violentcrusherspawn = true;
					new TriadThunderQuake(new VileMK2Grab(), victim.pos, 1, player, player.getNextActorNetId(), rpc: true);
					}
				}

				if (player.input.isPressed(Control.Jump,player)){
					character.vel.y = -character.getJumpPower();
				}
				
				if (player.input.isHeld(Control.Jump, player)) {
				
				Point moveAmount = new Point(character.xDir * 50, -100);
				character.move(moveAmount);
				character.useGravity = false;
				} else { character.useGravity = true; }
				if (base.player.input.isHeld("jump", base.player) && !once) {
				once = true;
				sound = character.playSound("vileHover", forcePlay: false, sendRpc: true);
				}
			}
		//}

		if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("_grabbed")) {
			character.changeToIdleOrFall();
			return;
		}

		if (victim.sprite.name.EndsWith("_grabbed") || victim.sprite.name.EndsWith("_die")) {
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.5f) {
					leechTime = 0;
					character.addHealth(0.5f);
				}
				return;
			}
		}


		leechTime += Global.spf;
		if (leechTime > 0.4f) {
			leechTime = 0;
			character.addHealth(1);
			var damager = new Damager(player, 1, 0, 0.1f);
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);
		}

		if (stateFrames >= 2 && player.input.isPressed(Control.Special1, player)) {
			character.changeToIdleOrFall();
			return;
		}


		if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {
			character.changeSpriteFromName("violentcrusher_grab", true);
			return;	
		}

		if (character.sprite.name.Contains("violentcrusher_grab") && character.frameIndex == 4
		&& !violentcrusherspawn){
		character.shakeCamera(sendRpc: true);
		character.playSound("crash", true, true);
		violentcrusherspawn = true;
		return;
		}


		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}	
	

	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (character is Vile vile) {
			vile.grabCooldown = 1;
		}
		if (newState is not VileMK2GrabState && victim != null) {
			victim.grabInvulnTime = 0.5f;
			//victim.stunInvulnTime = 0.5f;
			victim?.releaseGrab(character, true);
		}

		character.useGravity = true;
		character.sprite.restart();
		character.stopMoving();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("vileHover", character.netId);

	}
}




public class VileMK2Grabbed : GenericGrabbedState {
	public const float maxGrabTime = 6;
	public VileMK2Grabbed(Character? grabber) : base(grabber, maxGrabTime, "") {
	}

		public override void update() {
		base.update();
			if (grabber.sprite.name.Contains("idle")){
			character.changeToIdleOrFall();
			}
		}
	
}


public class VileStomp : Weapon {
	public VileStomp() : base() {
		fireRate = 0.75f;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}

public class VileStompState : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public VileStompState(Character? victim) : base("stomp", "", "", "") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("knocked_down")) {
			character.changeToIdleOrFall();
			return;
		}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}

		if (character.sprite.name.Contains("stomp")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));

		
		}

		if (leechTime > 0.10f && character.frameIndex == 2) {
			leechTime = 0;
			if (!character.sprite.name.Contains("mk5"))character.addHealth(0.5f);
			if (character.sprite.name.Contains("mk5"))character.addHealth(0.13f);
			character.shakeCamera(sendRpc: true);
		//	var damager = new Damager(player, 1f, 0, 0);
		//	damager.applyDamage(victim, false, new XUPGrab(), character, (int)ProjIds.UPGrab);
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

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.grabCooldown = 0.5f;
		victim.grabInvulnTime = 0.5f;
		victim?.releaseGrab(character);
	}
}



public class VileStomped : CharState {
	public const float maxGrabTime = 4;
	public Character? grabber;
	public long savedZIndex;
	public VileStomped(Character? grabber) : base("knocked_down") {
		this.grabber = grabber;
	}

	public override bool canEnter(Character character) {
		if (!base.canEnter(character)) return false;
		return !character.isInvulnerable() && !character.charState.invincible;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.stopCharge();
		savedZIndex = character.zIndex;
		character.setzIndex(grabber.zIndex - 100);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.grabInvulnTime = 0.5f;
		character.setzIndex(savedZIndex);
	}

	public override void update() {
		base.update();

		grabTime -= player.mashValue();
		if (grabTime <= 0) {
			character.changeToIdleOrFall();
		}
	}
}





public class VileAirRaid : CharState {
	public Character? victim;
	public BanzaiBeetleProj Banzai;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public VileAirRaid(Character? victim) : base("air_raid", "", "", "") {
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

		
			 if (Helpers.randomRange(0,3) == 1 ){
			character.changeSpriteFromName("punch_2", true);
			
			}
			else if (Helpers.randomRange(0,3) == 2){
			character.changeSpriteFromName("kick", true);
			
			}
			else if (Helpers.randomRange(0,3) == 3 ){
			character.changeSpriteFromName("kick_2", true);
			
 			} 
			else {
				character.changeSpriteFromName("punch_1", true);
			}
			
		}

			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {
			character.changeSpriteFromName("air_bomb_attack", true);	
		}


			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Special1, player)) {
			character.changeSpriteFromName("banzai_launch", true);	
				character.playSound("vileMissile", true);
		}

		if (character.sprite.name.Contains("banzai")
		&& character.frameIndex == 4){	
			if (Banzai == null){
			Banzai=	new BanzaiBeetleProj(new VileMK2Grab(), 
			character.pos, character.xDir, player, 
			player.getNextActorNetId(), true);
			}
		}


		if (character.sprite.name.Contains("air_bomb") && character.isAnimOver()){
			character.changeToIdleOrFall();
		}

		if (player.input.isPressed(Control.Jump, player)) {
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

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	//	character.grabCooldown = 0.5f;
	//	victim.grabInvulnTime = 0.5f;
		victim?.releaseGrab(character);
	}
}


