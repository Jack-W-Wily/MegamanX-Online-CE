using System;
using SFML.Graphics;

namespace MMXOnline;

public class CallDownMech : CharState {
	Vile vile = null!;
	RideArmor rideArmor;
	bool isNew;

	public CallDownMech(RideArmor rideArmor, bool isNew, string transitionSprite = "") : base("call_down_mech", "", "", transitionSprite) {
		this.rideArmor = rideArmor;
		this.isNew = isNew;
		superArmor = true;
	}

	public override void update() {
		base.update();
		if (rideArmor == null || rideArmor.destroyed || stateTime > 4) {
			character.changeToIdleOrFall();
			return;
		}

		if (rideArmor.rideArmorState is not RACalldown) {
			/*
			if (character.isVileMK5)
			{
				if (stateTime > 0.75f)
				{
					character.changeToIdleOrFall();
				}
				return;
			}
			*/

			if (//vile.isVileMK5 != true &&
			 MathF.Abs(character.pos.x - rideArmor.pos.x) < 10) {
				rideArmor.putCharInRideArmor(character);
			} else {
				character.changeToIdleOrFall();
			}
		}
	}

		public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		rideArmor.changeState(new RACalldown(character.pos, isNew), true);
		rideArmor.xDir = character.xDir;
		character.stopMoving();
		character.useGravity = false;
		vile = character as Vile ?? throw new NullReferenceException();
	}
}


public class VileDodge : CharState {
	public float dashTime = 0;
	public int initialDashDir;
	Vile vile;
	public BanzaiBeetleProj Banzai;

	public VileDodge() : base("roll", "", "") {
		attackCtrl = false;
		normalCtrl = true;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile;
		character.isDashing = true;
		character.burnTime -= 1;
		if (character.burnTime < 0) {
			character.burnTime = 0;
		}

		initialDashDir = character.xDir;
		if (player.input.isHeld(Control.Left, player)) initialDashDir = -1;
		else if (player.input.isHeld(Control.Right, player)) initialDashDir = 1;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		vile.dodgeRollCooldown = 0.5f;
	}

	public override void update() {
		base.update();


		if (player.input.isPressed(Control.Special1, player)) {
			character.playSound("vileMissile", true);
			character.changeSpriteFromName("banzai_launch", true);	
					character.turnToInput(player.input, player);
		}

	


		if (character.sprite.name.Contains("banzai")
		&& character.frameIndex == 4){	
			if (Banzai == null){
			Banzai=	new BanzaiBeetleProj(new VileMK2Grab(), 
			character.pos.addxy(0,-30), character.xDir, player, 
			player.getNextActorNetId(), true);
			}
		}


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

		if (character.frameIndex >= 4) return;

		dashTime += Global.spf;

		var move = new Point(0, 0);
		move.x = character.getDashSpeed() * initialDashDir;
		character.move(move);
		if (stateTime > 0.1) {
			stateTime = 0;
			//new Anim(this.character.pos.addxy(0, -4), "dust", this.character.xDir, null, true);
		}
	}
}



public class VileDashChargeState : CharState {
	
	public VileDashChargeState() : base("hyperdash_start", "") {
	}

	public override void update() {
		base.update();
		if (player == null) return;

	character.turnToInput(player.input, player);

	
		 if ((!player.input.isHeld(Control.Dash, player) && stateTime > 0.2f)) {
			character.changeState(new VileDashState(stateTime));
				character.playSound("vilehyperdashattack", true);
	
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

public class VileDashState : CharState {
	float trailTime;
	float chargeTime;
	public VileDashState(float chargeTime) : base("hyperdash_attack", "") {
		this.chargeTime = chargeTime;
		superArmor = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

	//	if (player.input.isPressed(Control.Special1, player)) {
	//		character.changeState(new FStagGrabState(true));
	//		return;
	//	}

		character.move(new Point(character.xDir * 400, 0));

	if (player.input.isPressed(Control.Dash, player) || stateTime > chargeTime) {
			character.changeToIdleOrFall();
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


public class VileRevive : CharState {
	public float radius = 200;
	Anim drDopplerAnim;
	bool isMK5;
	Vile vile;

	public VileRevive(bool isMK5) : base(isMK5 ? "revive_to5" : "revive") {
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
		vile = character as Vile ?? throw new NullReferenceException();
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
		new GigaCrushEffect(character);
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



public class VileStationaryHover : CharState {

	
	Vile vile = null!;
	
	public VileStationaryHover() : base("hover", "") {
		attackCtrl = false;
		normalCtrl = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		 if ((!player.input.isHeld(Control.AimAngleUp, player) && stateTime > 0.2f)) {
			character.changeToIdleOrFall();
		}

		character.stopMoving();
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.stopMoving();
		character.useGravity = true;
	}
}



public class VileHover : CharState {
	public SoundWrapper sound;
	public Point flyVel;
	float flyVelAcc = 500;
	float flyVelMaxSpeed = 200;
	public float shootcd;
	public float fallY;
	Vile vile = null!;

	public VileHover(string transitionSprite = "") : base("hover", "hover_shoot", "", transitionSprite) {
		exitOnLanding = true;
		attackCtrl = false;
		normalCtrl = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		shootcd += Global.spf;
		if (player == null) return;

		if (character.flag != null) {
			character.changeToIdleOrFall();
			return;
		}

		if (vile.vileHoverTime > vile.vileMaxHoverTime) {
			vile.vileHoverTime = vile.vileMaxHoverTime;
			character.changeToIdleOrFall();
			return;
		}

		if (character.charState is not VileHover) return;

		if (Global.level.checkTerrainCollisionOnce(character, 0, -character.getYMod()) != null && character.vel.y * character.getYMod() < 0) {
			character.vel.y = 0;
		}

		
		Point move = getHoverMove();
		

		if (player.input.isHeld(Control.AimAngleUp, player)){
		
		character.vel.y = 0;
		character.stopMoving();
		
		}

		if (!character.sprite.name.EndsWith("cannon_air") || character.sprite.time > 0.1f) {
			if (MathF.Abs(move.x) < -75 && !character.isUnderwater()) {
				sprite = "hover_backward";
				character.changeSpriteFromNameIfDifferent("hover_backward", false);
			}
			if (MathF.Abs(move.x) > 75 && !character.isUnderwater()) {
				sprite = "hover_forward";
				character.changeSpriteFromNameIfDifferent("hover_forward", false);
			} else {
				sprite = "hover";
				character.changeSpriteFromNameIfDifferent("hover", false);
			}
		}

	if (player.input.isHeld(Control.Special1, player) && vile.gizmoCooldown == 0) {
		vile.changeSpriteFromNameIfDifferent("cannon_gizmo_air", true);
		shootLogic(vile);
	
			if (vile.longshotGizmoCount >= 5 || player.vileAmmo <= 3) {
				vile.longshotGizmoCount = 0;
				vile.isShootingLongshotGizmo = false;
				vile.gizmoCooldown = 1f;
			}
		}
		if (move.magnitude > 0) {
			character.move(move);
		}

		

		if (character.isUnderwater()) {
			character.frameIndex = 0;
			character.frameSpeed = 0;
		}
		if (base.player.input.isHeld("jump", base.player) && !once) {
			once = true;
			sound = character.playSound("vileHover", forcePlay: false, sendRpc: true);
		}
	}



	public static void shootLogic(Vile vile) {
		
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) {
			return;
		}
		Point shootVel = vile.getVileShootVel(true);

		var player = vile.player;
		

		string muzzleSprite = "cannon_muzzle";
		 muzzleSprite += "_lg";

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
		if (vile.GizmoSpreadCD == 0){
		new VileCannonProj(
			new VileCannon(VileCannonType.LongshotGizmo),
			shootPos, MathF.Round(shootVel.byteAngle), //vile.longshotGizmoCount,
			player, player.getNextActorNetId(), rpc: true
		);
		vile.playSound("frontrunner", sendRpc: true);
		vile.GizmoSpreadCD = 0.12f;
			vile.longshotGizmoCount++;
		}
	}



	public Point getHoverMove() {
		bool isSoftLocked = character.isSoftLocked();
		bool isJumpHeld = !isSoftLocked && player.input.isHeld(Control.Jump, player) && character.pos.y > -5;

		var inputDir = isSoftLocked ? Point.zero : player.input.getInputDir(player);
		inputDir.y = isJumpHeld ? -1 : 0;

		//if (inputDir.x > 0) character.xDir = 1;
		//if (inputDir.x < 0) character.xDir = -1;

		if (inputDir.y == 0 || character.gravityWellModifier > 1) {
			if (character.frameIndex >= character.sprite.loopStartFrame) {
				character.frameIndex = character.sprite.loopStartFrame;
				character.frameSpeed = 0;
			}
if(!player.input.isHeld(Control.AimAngleUp,player))character.addGravity(ref fallY);
		} else {
			character.frameSpeed = 1;
			fallY = Helpers.lerp(fallY, 0, Global.spf * 10);
			vile.vileHoverTime += Global.spf;
		}

		if (inputDir.isZero()) {
			flyVel = Point.lerp(flyVel, Point.zero, Global.spf * 5f);
		} else {
			float ang = flyVel.angleWith(inputDir);
			float modifier = Math.Clamp(ang / 90f, 1, 2);

			flyVel.inc(inputDir.times(Global.spf * flyVelAcc * modifier));
			if (flyVel.magnitude > flyVelMaxSpeed) {
				flyVel = flyVel.normalize().times(flyVelMaxSpeed);
			}
		}

		var hit = character.checkCollision(flyVel.x * Global.spf, flyVel.y * Global.spf);
		if (hit != null && !hit.isGroundHit()) {
			flyVel = flyVel.subtract(flyVel.project(hit.getNormalSafe()));
		}

		return flyVel.addxy(0, fallY);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		character.useGravity = false;
		if (vile.hasSpeedDevil) {
			flyVelMaxSpeed *= 1.1f;
			flyVelAcc *= 1.1f;
		}

		float flyVelX = 0;
		if (character.deltaPos.x != 0) {
			flyVelX = character.xDir * character.getDashOrRunSpeed() * 0.5f;
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

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.sprite.restart();
		character.stopMoving();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("vileHover", character.netId);

	}
}




public class VileChainGrabState : CharState {
	bool fired = false;
	

	public VileChainGrabState() : base("spring_grab", "", "", "") {
		superArmor = true;
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



public class VileKick1 : CharState {
	bool fired = false;
	

	public VileKick1() : base("kick", "", "", "") {
	
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


public class VileSuperKickState : CharState {
	bool fired = false;
	

	public VileSuperKickState() : base("superkick", "", "", "") {
	
	}

	public override void update() {
		base.update();

		
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

		if (player.input.isHeld(Control.Up, player)){
			character.changeSpriteFromName("superkick_up",true);
		}
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class VilePunch1 : CharState {
	bool fired = false;
	

	public VilePunch1() : base("punch_1", "", "", "") {
	
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

