using System;
namespace MMXOnline;



public class CmdSigmaStateWC : CharState {
	public Sigma1 sigma = null!;

	public CmdSigmaStateWC(
		string sprite, string shootSprite = "", string attackSprite = "",
		string transitionSprite = "", string transShootSprite = ""
	) : base(sprite, shootSprite, attackSprite, transitionSprite, transShootSprite
	) {
	}

	public override void onEnter(CharState oldState) {
		sigma = player.character as Sigma1 ?? throw new NullReferenceException();
		base.onEnter(oldState);
	}
}



public class HellGaze : CmdSigmaStateWC {
	bool fired;

	public HellGaze() : base("hellgaze") {
		airMove = true;
	}


	public override void update() {
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("amorphusGG", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		base.update();
	}
}



public class SigmaSlashStateAirWC : CmdSigmaStateWC {
	bool fired;
	public SigmaSlashStateAirWC() : base("attack_air") {
		useDashJumpSpeed = true;
		airMove = true;
		canStopJump = true;
		landSprite = "attack";
		airSprite = "attack_air";
	}

	public override void update() {
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("sigmaSaber", sendRpc: true);
			Point off = new Point(24, -22);
			new SigmaSlashProj(
				character.pos.addxy(off.x * character.xDir, off.y), character.xDir,
				sigma, player, player.getNextActorNetId(), 3, 13, rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		base.update();
	}
}



public class SigmaSlashStateGroundWC : CmdSigmaStateWC {
	bool fired;

	public SigmaSlashStateGroundWC() : base("attack") {
		airMove = true;
	}


	public override void update() {
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("sigmaSaber", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		base.update();
	}
}


public class SigmaSlashStateGround2WC : CmdSigmaStateWC {
	bool fired;

	public SigmaSlashStateGround2WC() : base("attack_crouch") {
		airMove = true;
	}


	public override void update() {
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("sigmaSaber", sendRpc: true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		base.update();
	}
}

public class SigmaSlashStateDashWC : CmdSigmaStateWC {
	bool fired;
	public SigmaSlashStateDashWC() : base("attack_dash") {
		airMove = true;
		canStopJump = true;
	}


	public override void update() {
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("sigmaSaber", sendRpc: true);
			Point off = new Point(26, -22);
			new SigmaSlashProj(
				character.pos.addxy(off.x * character.xDir, off.y), character.xDir,
				sigma, player, player.getNextActorNetId(), 4, 26, rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
		base.update();
	}
}


public class SigmaBallShootWC : CmdSigmaStateWC {
	public SigmaBallProj? SigmaBallsProjHead;
	public Anim? anim;
	public bool shoot;
	public float angle;

	public SigmaBallShootWC() : base("shoot") {
	}

	public override void update() {
		base.update();
		character.turnToInput(player.input, player);
	
		if (character.frameIndex >= 1 && !shoot) {
			shoot = true;
			ammoReduction();
			shootProjectiles();
		} else if (character.sprite.frameIndex == 0) {
			shoot = false;
		}
	
		if (sigma.ballWeapon.ammo <= 0 || character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		// By disabling the code bellow, you can sort of make it MMX1 Accurate
		if (character.sprite.loopCount > 0 && !player.input.isHeld(Control.Special1, player)) {
			character.changeToIdleOrFall();
		}
	}

	public void shootProjectiles() {
		character.playSound("energyBall", sendRpc: true);
		Point shootPos = sigma.getFirstPOI() ?? sigma.getCenterPos();
		angleShoot();
		SigmaBallsProjHead = new SigmaBallProj
		(
			shootPos, 1, angle, sigma,
			player, player.getNextActorNetId(), rpc: true
		);
		anim = new Anim(shootPos, "sigma_proj_ball_muzzle", character.xDir,
			player.getNextActorNetId(), true, sendRpc: true);
	}

	public void ammoReduction() {
		sigma.ballWeapon.addAmmo(-4, player);
		sigma.sigmaAmmoRechargeCooldown = sigma.sigmaHeadBeamTimeBeforeRecharge;
	}

	public void angleShoot() {
		if (character.xDir == 1) {
			if (player.input.isHeld(Control.Down, player)) {
				angle = 42;
			} else if (player.input.isHeld(Control.Up, player)) {
				angle = 216;
			} else {
				angle = 8;
			}
		} else if (character.xDir == -1) {
			if (player.input.isHeld(Control.Down, player)) {
				angle = 94;
			} else if (player.input.isHeld(Control.Up, player)) {
				angle = 164;
			} else {
				angle = 120;
			}
		}
	}
}

public class SigmaWallDashStateWC : CmdSigmaStateWC {
	public bool fired;
	public int yDir;
	public Point vel;
	public bool fromGround;

	public SigmaWallDashStateWC(int yDir, bool fromGround) : base("wall_dash") {
		this.yDir = yDir;
		this.fromGround = fromGround;
		superArmor = true;
		useDashJumpSpeed = true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

		float xSpeed = 350;
		if (!fromGround) {
			character.xDir *= -1;
		} else {
			character.unstickFromGround();
			character.incPos(new Point(0, -5));
		}
		character.isDashing = true;
		character.dashedInAir++;
		character.stopMoving();
		vel = new Point(character.xDir * xSpeed, yDir * 100);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		character.useGravity = true;
		sigma.leapSlashCooldown = Sigma1.maxLeapSlashCooldown;
		base.onExit(newState);
	}

	public override void update() {
		base.update();

		var collideData = Global.level.checkTerrainCollisionOnce(character, vel.x * Global.spf, vel.y * Global.spf);
		if (collideData?.gameObject is Wall wall) {
			var collideData2 = Global.level.checkTerrainCollisionOnce(character, vel.x * Global.spf, 0);
			if (collideData2?.gameObject is Wall wall2 && wall2.collider.isClimbable) {
				character.changeState(new WallSlide(character.xDir, wall2.collider) { enterSound = "" }, true);
			} else {
				if (vel.y > 0) {
					character.changeToIdleOrFall();
				} else {
					character.isDashing = true;
					character.changeToIdleOrFall();
				}
			}
		}

		character.move(vel);

		if (stateTime > 0.7f) {
			character.changeState(character.getFallState(), true);
		}
		if (player.input.isPressed(Control.Shoot, player) &&
			!fired && sigma.saberCooldown == 0 && character.invulnTime == 0
		) {
			if (yDir == 0) {
				character.changeState(new SigmaSlashStateDash());
				return;
			}
			fired = true;
			sigma.saberCooldown = sigma.sigmaSaberMaxCooldown;
			character.playSound("sigmaSaber", sendRpc: true);
			character.changeSpriteFromName("wall_dash_attack", true);
			Point off = new Point(30, -20);
			new SigmaSlashProj(
				character.pos.addxy(off.x * character.xDir, off.y), character.xDir,
				sigma, player, player.getNextActorNetId(), damage: 4, rpc: true
			);
		}
	}
}




public class SigDodge : CharState {
	public float dashTime = 0;
	public int initialDashDir;
	Sigma1 sigma;


	public SigDodge() : base("roll", "", "") {
		attackCtrl = false;
		normalCtrl = true;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		sigma = character as Sigma1;
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
	}

	public override void update() {
		base.update();

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



public class VirusSlash1 : CharState {
	bool fired = false;
	

	public VirusSlash1() : base("slash_1", "", "", "") {
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
		
		character.playSound("genmureix5", sendRpc: true);
		float slideVel = character.xDir * character.getDashSpeed();
		if (player.input.isHeld(Control.Dash,player)){
			character.move(new Point( slideVel, 0));
			
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}


public class VirusSlash2 : CharState {
	bool fired = false;
	

	public VirusSlash2() : base("slash_2", "", "", "") {
	superArmor = true;
	}

	public override void update() {
		base.update();

		if (character.grounded){
			if (player.input.isHeld(Control.Down,player)
			&& player.input.isLeftOrRightHeld(player)
			&&  character.frameIndex > 2 && player.sigmaAmmo > 4){
				player.sigmaAmmo -= 4;
			character.changeState(new VirusSlash3(), true);
			
			}
		}

		if (character.isAnimOver() || !player.input.isR2Held(player) && character.frameIndex > 4) {
			character.changeState(new VirusSlash3(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
			character.playSound("genmureix5", sendRpc: true);
		float slideVel = character.xDir * character.getDashSpeed();
		if (player.input.isHeld(Control.Dash,player)){
			character.move(new Point( slideVel, 0));
			
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}


public class VirusSlash3 : CharState {
	bool fired = false;
	

	public VirusSlash3() : base("slash_3", "", "", "") {
	superArmor = true;
	}

	public override void update() {
		base.update();

		if (character.grounded){
			if (player.input.isHeld(Control.Up,player)
			&& player.input.isLeftOrRightHeld(player)
			&& character.frameIndex > 4 && player.sigmaAmmo > 4){
				player.sigmaAmmo -= 4;
			character.changeState(new VirusSlash1(), true);
		
			}
		}

		
		if (character.isAnimOver() || !player.input.isR2Held(player) && character.frameIndex > 4 )  {
			character.changeState(new VirusSlash1(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("genmureix5", sendRpc: true);
		float slideVel = character.xDir * character.getDashSpeed();
		if (player.input.isHeld(Control.Dash,player)){
			character.move(new Point( slideVel, 0));
			
		}
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

