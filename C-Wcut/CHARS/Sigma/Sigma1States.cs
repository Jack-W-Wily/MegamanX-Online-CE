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
		superArmor = true;
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


public class HellGazeEX : CmdSigmaStateWC {
	bool fired;

	float shootCD;

	public HellGazeEX() : base("hellgaze") {
		airMove = true;
		superArmor = true;
	}


	public override void update() {
		Helpers.decrementTime(ref shootCD);
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("amorphusGG", sendRpc: true);
		}
		if (character.frameIndex >= 2 && shootCD == 0) {
			shootCD = 0.05f;
			 new SigmaSkull(
					character.pos.addxy(Helpers.randomRange(-200,200),Helpers.randomRange(-200,200)), character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
					);
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
		if (character.sprite.loopCount > 0 && player.input.isPressed(Control.Special1, player)) {
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




public class SigmaBallShootWCEnhanced : CmdSigmaStateWC {
	public SigmaSkull? SigmaBallsProjHead;
	public Anim? anim;
	public bool shoot;
	public float angle;

	public SigmaBallShootWCEnhanced() : base("shoot") {
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
		SigmaBallsProjHead = new SigmaSkull(
					shootPos, character.xDir, character,
					player, player.getNextActorNetId(), 0, rpc: true
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
			if (player.input.isHeld(Control.Left, player)) initialDashDir = -1;
		else if (player.input.isHeld(Control.Right, player)) initialDashDir = 1;
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





public class SigmaSkull : Projectile, IDamagable {
	public Actor? target;
	public float smokeTime = 0;
	public float maxSpeed = 150;
	public SigmaSkull(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, float? angle = null, bool rpc = false
	) : base(
		pos, xDir, owner, "sigma_skull_proj", netId, player
	) {
		weapon = LaunchOctopus.netWeapon;
		damager.damage = 1;
		damager.flinch = Global.halfFlinch;
		vel = new Point(150 * xDir, 0);
		fadeSprite = "explosion";
		fadeSound = "explosion";
		maxTime = 2f;
		projId = (int)ProjIds.SigmaSkull;
		fadeOnAutoDestroy = true;
		reflectableFBurner = true;
		customAngleRendering = true;
		this.angle = this.xDir == -1 ? 180 : 0;
		if (angle != null) {
			this.angle = angle.Value + (this.xDir == -1 ? 180 : 0);
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		canBeLocal = false;
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new TorpedoProjChargedOcto(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}
	bool homing = true;
	public void reflect(float reflectAngle) {
		angle = reflectAngle;
		target = null;
	}
	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}
	public override void update() {
		base.update();
		if (ownedByLocalPlayer && homing) {
			if (target != null) {
				if (!Global.level.gameObjects.Contains(target)) {
					target = null;
				}
			}
			if (target != null) {
				if (time < 3f) {
					var dTo = pos.directionTo(target.getCenterPos()).normalize();
					var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
					destAngle = Helpers.to360(destAngle);
					if (angle != null) angle = Helpers.lerpAngle((float)angle, destAngle, Global.spf * 3);
				}
			}
			if (time >= 0.15) {
				target = Global.level.getClosestTarget(pos, damager.owner.alliance, true, aMaxDist: Global.screenW * 0.75f);
			} else if (time < 0.15) {
				//this.vel.x += this.xDir * Global.spf * 300;
			}
			if (angle != null) {
				vel.x = Helpers.cosd((float)angle) * maxSpeed;
				vel.y = Helpers.sind((float)angle) * maxSpeed;
			}
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.2) {
			smokeTime = 0;
			if (homing) new Anim(pos, "torpedo_smoke", 1, null, true);
		}
	}
	public override void renderFromAngle(float x, float y) {
		var angle = this.angle;
		var xDir = 1;
		var yDir = 1;
		var frameIndex = 0;
		float normAngle = 0;
		if (angle < 90) {
			xDir = 1;
			yDir = -1;
			normAngle = (float)angle;
		}
		if (angle >= 90 && angle < 180) {
			xDir = -1;
			yDir = -1;
			normAngle = 180 - (float)angle;
		} else if (angle >= 180 && angle < 270) {
			xDir = -1;
			yDir = 1;
			normAngle = (float)angle - 180;
		} else if (angle >= 270 && angle < 360) {
			xDir = 1;
			yDir = 1;
			normAngle = 360 - (float)angle;
		}

		if (normAngle < 18) frameIndex = 0;
		else if (normAngle >= 18 && normAngle < 36) frameIndex = 1;
		else if (normAngle >= 36 && normAngle < 54) frameIndex = 2;
		else if (normAngle >= 54 && normAngle < 72) frameIndex = 3;
		else if (normAngle >= 72 && normAngle < 90) frameIndex = 4;

		sprite.draw(frameIndex, pos.x + x, pos.y + y, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex, actor: this);
	}
	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		if (damage > 0) {
			destroySelf();
		}
	}
	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return damager.owner.alliance != damagerAlliance;
	}
	public bool isInvincible(Player attacker, int? projId) {
		return false;
	}
	public bool canBeHealed(int healerAlliance) {
		return false;
	}
	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) {
	}
	public bool isPlayableDamagable() {
		return false;
	}
}

