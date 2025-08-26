using System;
using System.Collections.Generic;

namespace MMXOnline;

public class HighMaxHover : CharState {
	float hoverTime;
	
	public HighMaxHover() : base("hover", "hover", "hover", "hover") {
		exitOnLanding = true;
		airMove = true;
		attackCtrl = true;
		normalCtrl = true;
	}


	public Point AimPoint() {
	Point vel = new Point(1, 0);
		

	if (player.input.isHeld(Control.Shoot, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		}  
		if (player.input.isHeld(Control.Special1, player) ) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, 0.75f);
			} else {
				vel = new Point(1, 3);
			}
		} 

		return vel;
	}

	public override void update() {
		base.update();

		if ( character.vel.y < 0 && !player.input.isHeld(Control.Up, player) 
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
		if (!character.sprite.name.Contains("shoot2") && player.input.isHeld(Control.Up, player)){
			if(player.input.isHeld(Control.Dash, player)){
			character.vel.y = -character.getJumpPower() * 1f;
			} else {character.vel.y = -character.getJumpPower() * 0.3f;}
		}
		if (!character.sprite.name.Contains("shoot2") && player.input.isHeld(Control.Down, player)){
			if (player.input.isHeld(Control.Dash, player)){
			character.vel.y = +character.getJumpPower() * 1f;
			} else {character.vel.y = +character.getJumpPower() * 0.3f;}
		}

		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}


		

		hoverTime += Global.spf;
	if ((hoverTime > 10) || hoverTime > 0.2f &&
			character.player.input.isPressed(Control.Jump, character.player)
		) {
			character.changeState(new Fall(), true);
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




public class HighmaxShoot1 : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public HighmaxShoot1(string transitionSprite = "")
		: base("shoot1", "", "", transitionSprite) {

	}

	public Point AimPoint() {
		Point vel = new Point(1, 0);


		if (player.input.isHeld(Control.Shoot, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		}
		if (player.input.isHeld(Control.Special1, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, 0.75f);
			} else {
				vel = new Point(1, 3);
			}
		}

		return vel;
	}

	bool first = false;
	bool seccond = false;
	bool third = false;
	bool fourth = false;

	public override void update() {

		var poi = character.getFirstPOI();
		accuracy = 0;
		Point prevPos = character.pos;
		Point shootVel = AimPoint();

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}

		if (poi != null) {
			if (!first && character.sprite.frameIndex > 2) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				first = true;
				character.playSound("boundBlaster");
			}
			if (!seccond && character.sprite.frameIndex > 4) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				seccond = true;
				character.playSound("boundBlaster");
			}
			if (!third && character.sprite.frameIndex > 6) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				third = true;
				character.playSound("boundBlaster");
			}
			if (!fourth && character.sprite.frameIndex > 8) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				fourth = true;
				character.playSound("boundBlaster");
			}
		}

		if (character.isAnimOver()) {
			if (character.grounded) {
				character.changeState(new Idle());
			} else {
				character.changeState(new HighMaxHover());
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class HighmaxShoot2 : CharState {
	
	public float pushBackSpeed;

	public HighmaxShoot2(string transitionSprite = "")
		: base("shoot2", "", "", transitionSprite)
	{
	
	}

		public Point AimPoint() {
	Point vel = new Point(1, 0);
		

	if (player.input.isHeld(Control.Shoot, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		}  
		if (player.input.isHeld(Control.Special1, player) ) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, 0.75f);
			} else {
				vel = new Point(1, 3);
			}
		} 

		return vel;
	}

	bool first = false;
	bool seccond = false;
	bool third = false;
	bool fourth = false;

	public override void update() {

		var poi = character.getFirstPOI();	
		accuracy = 0;
		Point prevPos = character.pos;
			Point shootVel = AimPoint();

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}
		if (poi != null) {
			if (!first && character.sprite.frameIndex > 2) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				first = true;
				character.playSound("boundBlaster");
			}
			if (!seccond && character.sprite.frameIndex > 4) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				seccond = true;
				character.playSound("boundBlaster");
			}
			if (!third && character.sprite.frameIndex > 6) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				third = true;
				character.playSound("boundBlaster");
			}
			if (!fourth && character.sprite.frameIndex > 8) {
				new HighmaxHomingProj(
					poi.Value, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
				fourth = true;
				character.playSound("boundBlaster");
			}
		}
		if (character.isAnimOver()) {
			if (character.grounded) {
				character.changeState(new Idle());
			} else {
				character.changeState(new HighMaxHover());
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}





public class DesmumeSpam : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public DesmumeSpam(string transitionSprite = "")
		: base("ultimate", "", "", transitionSprite)
	{
	superArmor = true;
	}

		public Point AimPoint() {
	Point vel = new Point(1, 0);
		

	if (player.input.isHeld(Control.Shoot, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		}  
		if (player.input.isHeld(Control.Special1, player) ) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, 0.75f);
			} else {
				vel = new Point(1, 3);
			}
		} 

		return vel;
	}


		float supercooldown = 0;

	public override void update() {

		var poi = character.getFirstPOI();
		accuracy = 0;
		Point prevPos = character.pos;
		Point shootVel = AimPoint();

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}

				Helpers.decrementTime(ref supercooldown);

		if (character.sprite.name.Contains("ultimate") && poi != null) {
			if (Helpers.randomRange(0, 2) == 0 && supercooldown == 0) {
				supercooldown = 0.3f;
				character.playSound("boundBlaster");
				new HighmaxHomingProj(
				poi.Value, 1, character,
				player, player.getNextActorNetId(), 0, rpc: true
				);
			}
			if (Helpers.randomRange(0, 2) == 1 && supercooldown == 0) {
				supercooldown = 0.3f;
				character.playSound("buster4");
				new DesmumeProj1(new XBuster(), poi.Value, character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
			}
			if (Helpers.randomRange(0, 2) == 2 && supercooldown == 0) {
				supercooldown = 0.3f;
				character.playSound("boundBlaster");
				new HighmaxStunShot(
						poi.Value, character.xDir, MathF.Round(shootVel.byteAngle), character,
						character.player, character.player.getNextActorNetId(), rpc: true
					);
			}
		}

		if (character.isAnimOver() || stateTime > 10) {
			if (character.grounded) {
				character.changeState(new Idle());
			} else {
				character.changeState(new HighMaxHover());
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class DesmumeState : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public DesmumeState(string transitionSprite = "")
		: base("desmume", "", "", transitionSprite)
	{
		superArmor = true;
	}

		public Point AimPoint() {
	Point vel = new Point(1, 0);
		

	if (player.input.isHeld(Control.Shoot, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		}  
		if (player.input.isHeld(Control.Special1, player) ) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, 0.75f);
			} else {
				vel = new Point(1, 3);
			}
		} 

		return vel;
	}


	bool first = false;

	public override void update() {

		var poi = character.getFirstPOI();	
		accuracy = 0;
		Point prevPos = character.pos;
			Point shootVel = AimPoint();

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}
		
			if (character.sprite.name.Contains("desmume") && poi != null && !first){
		Projectile proj;
		first = true;
		proj = new DesmumeProj1(new XBuster(), poi.Value, character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
		}	

		if (character.isAnimOver()) {
			if (character.grounded) {
				character.changeState(new Idle());
			} else {
				character.changeState(new HighMaxHover());
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




public class HighMaxIdlePunch1 : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public HighMaxIdlePunch1(string transitionSprite = "")
		: base("idle_punch1", "", "", transitionSprite) {

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
		if (player.input.isHeld(Control.Down, player) && player.input.isPressed(Control.Shoot, player)) {
			character.changeState(new HighMaxCrouchPunch1());
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
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class HighMaxCrouchPunch1 : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public HighMaxCrouchPunch1(string transitionSprite = "")
		: base("crouch_punch", "", "", transitionSprite) {

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
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class HighMaxMegaPunch : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public HighMaxMegaPunch(string transitionSprite = "")
		: base("foward_punch", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
		
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
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class HighMaxChargePunch : CharState {
	float hoverTime;

	bool once;

	public HighMaxChargePunch() : base("dash_punch_charge", "dash_punch_charge", "dash_punch_charge", "dash_punch_charge") {
		exitOnLanding = false;
		airMove = false;
		attackCtrl = false;
		normalCtrl = true;
	}

	public override void update() {
		base.update();

		accuracy = 0;
		Point prevPos = character.pos;

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}

		if (character.vel.y < 0 && !player.input.isHeld(Control.Up, player)
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
		if (player.input.isHeld(Control.Up, player)) {
			character.vel.y = -character.getJumpPower() * 0.2f;
		}
		if (player.input.isHeld(Control.Down, player)) {
			character.vel.y = +character.getJumpPower() * 0.2f;
		}

		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}

		hoverTime += Global.spf;
		if ((hoverTime > 5) || hoverTime > 0.5f &&
				!character.player.input.isHeld(Control.Dash, character.player)
			) {
			if (player.input.isHeld(Control.Up, player)) {
				character.changeState(new HighMaxSlamDownState(), true);
			} else {
				character.changeState(new HighMaxSuperPunchState(), true);
			}
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


public class HighMaxSuperPunchState : CharState {
	Anim? proj;
	bool once;
	public HighMaxSuperPunchState() : base("dash_punch", "", "", "") {
		superArmor = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && proj != null) {
			proj.destroySelf();
			proj = null;
		}
		if (stateTime < 0.7f){
		character.move(new Point(character.xDir * 350, 0));
		}
			CollideData? collideData = Global.level.checkTerrainCollisionOnce(character, character.xDir, 0);
		if (collideData != null && collideData.isSideWallHit() && character.ownedByLocalPlayer) {
			if (!once) {
				character.playSound("crash", forcePlay: false, sendRpc: true);
				once = true;
				var poi = character.getFirstPOI();
				character.playSound("buster4");
				new DesmumeProj1(new XBuster(), poi.Value, character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);

			}
			character.shakeCamera(sendRpc: true);
			if (stateTime > 1f) {
			character.changeState(new Idle(), true);
			return;
			}
		} 
		if (stateTime > 1f) {
			character.changeState(new Idle(), true);
			return;
		}

		if (proj != null) {
			proj.changePos(character.pos.addxy(0, -15));
			proj.xDir = character.xDir;
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
		if (proj != null && !proj.destroyed) proj.destroySelf();
	}
}



public class HighMaxSlamDownState : CharState {
	Anim? proj;
	bool once;
	public HighMaxSlamDownState() : base("slam_grab", "", "", "") {
		superArmor = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && proj != null) {
			proj.destroySelf();
			proj = null;
		}
		if (stateTime < 0.7f){
		character.move(new Point(character.xDir * 0, 350));
		}
		if (character.grounded) {
			if (!once) {
				character.playSound("crash", forcePlay: false, sendRpc: true);
				once = true;
				var poi = character.getFirstPOI();
				character.playSound("buster4");
				new DesmumeProj1(new XBuster(), poi.Value, character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);

			}
			character.shakeCamera(sendRpc: true);
			if (stateTime > 1f) {
			character.changeState(new Idle(), true);
			return;
			}
		} 
		if (stateTime > 1f) {
			character.changeState(new Idle(), true);
			return;
		}

		if (proj != null) {
			proj.changePos(character.pos.addxy(0, -15));
			proj.xDir = character.xDir;
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
		if (proj != null && !proj.destroyed) proj.destroySelf();
	}
}



public class DesmumeProj1 : Projectile {
	public DesmumeProj1(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 150, 1, player, "highmax_punch_proj", 20, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DesmumeProj1;
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (grounded) {
			destroySelf();
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		Point destroyPos = other?.hitData?.hitPoint ?? pos;
		changePos(destroyPos);
		destroySelf();
	}

	public override void onDestroy() {
		if (!ownedByLocalPlayer) return;
		new DesmumeProj2(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
	}
}

public class DesmumeProj2 : Projectile {
	float flameCreateTime = 1;
	public DesmumeProj2(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 100, 1f, player, "highmax_punch_proj", 10, 1f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 2;
		projId = (int)ProjIds.DesmumeProj2;
		useGravity = false;
		collider.wallOnly = true;
		destroyOnHit = false;
		shouldShieldBlock = false;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}



	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -50));
		}
	}


	public override void update() {
		base.update();
	

		var hit = Global.level.checkCollisionActorOnce(this, vel.x * Global.spf, 0, null);
		if (hit?.gameObject is Wall && hit?.hitData?.normal != null && !(hit.hitData.normal.Value.isAngled()) 
		|| owner.input.isPressed(Control.Special1, owner)) {
			if (ownedByLocalPlayer) {
				new DesmumeProj3(weapon, pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
			}
			destroySelf();
		}
	}
}


public class DesmumeProj3 : Projectile {
	public DesmumeProj3(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 0, 2, player, "highmax_punch_proj", 10, 0.5f, netProjId, player.ownedByLocalPlayer) {
		maxTime = 1f;
		projId = (int)ProjIds.DesmumeProj3;
		vel = new Point(0, -200);
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		if (isUnderwater()) {
			destroySelf(disableRpc: true);
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}
