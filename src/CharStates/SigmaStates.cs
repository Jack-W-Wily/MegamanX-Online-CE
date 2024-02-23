using System;

namespace MMXOnline;

public class IssueGlobalCommand : CharState {
	public IssueGlobalCommand(string transitionSprite = "") :
		base("summon_maverick", "", "", transitionSprite) {
		superArmor = true;
	}

	public override void update() {
		base.update();

		if (character.isAnimOver()) {
			character.changeState(new Idle(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class CallDownMaverick : CharState {
	Maverick maverick;
	bool isNew;
	bool isRecall;
	int frame;
	public CallDownMaverick(Maverick maverick, bool isNew, bool isRecall, string transitionSprite = "") :
		base("summon_maverick", "", "", transitionSprite) {
		this.maverick = maverick;
		this.isNew = isNew;
		this.isRecall = isRecall;
		superArmor = true;
	}

	public override void update() {
		base.update();

		frame++;

		if (frame > 0 && frame < 10 && (player.isStriker() || player.isSummoner())) {
			if (player.input.isPressed(Control.Shoot, player) && maverick.startMoveControl == Control.Special1) {
				maverick.startMoveControl = Control.Dash;
			} else if (player.input.isPressed(Control.Special1, player) && maverick.startMoveControl == Control.Shoot) {
				maverick.startMoveControl = Control.Dash;
			}
		}

		if (character.isAnimOver()) {
			character.changeState(new Idle(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!isNew) {
			if (maverick.state is not MExit) maverick.changeState(new MExit(character.pos, isRecall));
			else maverick.changeState(new MEnter(character.pos));
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class SigmaSlashWeapon : Weapon {
	public SigmaSlashWeapon() : base() {
		index = (int)WeaponIds.SigmaSlash;
		killFeedIndex = 9;
	}
}

public class SigmaSlashState : CharState {
	CharState prevCharState;
	int attackFrame = 2;
	bool fired;
	public SigmaSlashState(CharState prevCharState) : base(prevCharState.attackSprite, "", "", "") {
		this.prevCharState = prevCharState;
		if (prevCharState is Dash || prevCharState is AirDash) {
			attackFrame = 1;
		}
		useDashJumpSpeed = true;
		airMove = true;
	}

	public override void update() {
		base.update();
		int chargeLevel = character.getChargeLevel();
		if (!character.grounded) {
			landSprite = "attack";
		}
		if (chargeLevel > 1){
		superArmor = true;
		}
		if (prevCharState is Dash) {
			if (character.frameIndex < attackFrame) {
				character.move(new Point(character.getDashSpeed() * (float)character.xDir, 0));
			}
		}

		if (character.frameIndex >= attackFrame && !fired) {
			fired = true;
			character.playSound("saberShot", sendRpc: true);

			Point off = new Point(30, -20);
			if (character.sprite.name == "sigma_attack_air") {
				off = new Point(20, -30);
			}
			if (chargeLevel >2){
			new MechFrogStompShockwave(new MechFrogStompWeapon(player), 
		character.pos.addxy(6 * character.xDir, 0), character.xDir, 
		player, player.getNextActorNetId(), rpc: true);
		character.playSound("crash", sendRpc: true);
		}
			float damage = chargeLevel > 1 ? 4 : 2;
			int flinch = chargeLevel > 1 ? Global.defFlinch : 13;
			new SigmaSlashProj(player.sigmaSlashWeapon, character.pos.addxy(off.x * character.xDir, off.y), character.xDir, player, player.getNextActorNetId(), damage: damage, flinch: flinch, rpc: true);
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		}
	}
}

public class SigmaSlashProj : Projectile {
	public SigmaSlashProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId,
		float damage = 2, int flinch = 26, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, damage, player, "sigma_proj_slash", flinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		maxTime = 0.1f;
		projId = (int)ProjIds.SigmaSlash;
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

public class SigmaBallWeapon : Weapon {
	public SigmaBallWeapon() : base() {
		index = (int)WeaponIds.SigmaBall;
		killFeedIndex = 103;
	}
}

public class SigmaBallProj : Projectile {
	public SigmaBallProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 400, 1, player, "sigma_proj_ball", 2, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.SigmaBall;
		maxTime = 0.5f;
		destroyOnHit = true;
		if (vel != null) {
			this.vel = vel.Value.times(speed);
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}
}

public class SigmaBallShoot : CharState {
	bool shot;
	public Sigma sigma;

	public SigmaBallShoot(string transitionSprite = "") : base("shoot", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		if (character.sprite.loopCount > 0 && !player.input.isHeld(Control.Special1, player)) {
			character.changeState(new Idle(), true);
			return;
		}

		Point vel = new Point(0, 0.2f);
		bool lHeld = player.input.isHeld(Control.Left, player) && !player.isAI;
		bool rHeld = player.input.isHeld(Control.Right, player) && !player.isAI;
		bool uHeld = player.input.isHeld(Control.Up, player) || player.isAI;
		bool dHeld = player.input.isHeld(Control.Down, player) && !player.isAI;

		if (lHeld) {
			character.xDir = -1;
			vel.x = -2;
		} else if (rHeld) {
			character.xDir = 1;
			vel.x = 2;
		}

		if (uHeld) {
			vel.y = -1;
			if (vel.x == 0) vel.x = character.xDir * 0.5f;
		} else if (dHeld) {
			vel.y = 1;
			if (vel.x == 0) vel.x = character.xDir * 0.5f;
		} else vel.x = character.xDir;

		if (!uHeld && !dHeld && (lHeld || rHeld)) {
			vel.y = 0;
			vel.x = character.xDir;
		}

		if (character.sprite.frameIndex == 0) {
			shot = false;
		}
		if (character.sprite.frameIndex == 1 && !shot) {
			shot = true;
			Point poi = character.getFirstPOI() ?? character.getCenterPos();

			player.sigmaAmmo -= 7;
			if (player.sigmaAmmo < 0) player.sigmaAmmo = 0;
			sigma.sigmaAmmoRechargeCooldown = sigma.sigmaHeadBeamTimeBeforeRecharge;
			character.playSound("energyBall", sendRpc: true);
			new SigmaBallProj(player.sigmaBallWeapon, poi, character.xDir, player, player.getNextActorNetId(), vel.normalize(), rpc: true);
			new Anim(poi, "sigma_proj_ball_muzzle", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
		}

		if (character.sprite.loopCount > 5 || player.sigmaAmmo <= 0) {
			character.changeState(new Idle(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		sigma = character as Sigma;
		character.vel = new Point();
	}
}

public class SigmaWallDashState : CharState {
	bool fired;
	int yDir;
	Point vel;
	bool fromGround;
	public Sigma sigma;

	public SigmaWallDashState(int yDir, bool fromGround) : base("wall_dash", "", "", "") {
		this.yDir = yDir;
		this.fromGround = fromGround;
		superArmor = true;
		useDashJumpSpeed = true;
	}

	public override bool canEnter(Character character) {
		if (!base.canEnter(character)) return false;
		return character?.player?.isSigma1() == true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		sigma = character as Sigma;
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

	public override void onExit(CharState newState) {
		character.useGravity = true;
		sigma.leapSlashCooldown = Sigma.maxLeapSlashCooldown;
		base.onExit(newState);
	}

	public override void update() {
		base.update();

		var collideData = Global.level.checkCollisionActor(character, vel.x * Global.spf, vel.y * Global.spf);
		if (collideData?.gameObject is Wall wall) {
			var collideData2 = Global.level.checkCollisionActor(character, vel.x * Global.spf, 0);
			if (collideData2?.gameObject is Wall wall2 && wall2.collider.isClimbable) {
				character.changeState(new WallSlide(character.xDir, wall2.collider), true);
			} else {
				if (vel.y > 0) character.changeState(new Idle(), true);
				else {
					//vel.y *= -1;
					character.isDashing = true;
					character.changeState(new Fall(), true);
				}
			}
		}

		character.move(vel);

		if (stateTime > 0.7f) {
			character.changeState(new Fall(), true);
		}

		if (player.input.isPressed(Control.Shoot, player) &&
			!fired && character.saberCooldown == 0 && character.invulnTime == 0
		) {
			if (yDir == 0) {
				character.changeState(new SigmaSlashState(new Dash(Control.Dash)), true);
				return;
			}

			fired = true;
			character.saberCooldown = sigma.sigmaSaberMaxCooldown;

			character.playSound("saberShot", sendRpc: true);
			character.changeSpriteFromName("wall_dash_attack", true);

			Point off = new Point(30, -20);
			new SigmaSlashProj(player.sigmaSlashWeapon, character.pos.addxy(off.x * character.xDir, off.y), character.xDir, player, player.getNextActorNetId(), damage: 4, rpc: true);
		}
	}
}
