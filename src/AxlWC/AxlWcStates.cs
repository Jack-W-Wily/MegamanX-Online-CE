using System;
using System.Linq;

namespace MMXOnline;

public class HyperAxlWcStart : CharState {
	public AxlWC axl = null!;

	public HyperAxlWcStart(bool isGrounded) : base(isGrounded ? "hyper_start" : "hyper_start_air") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (stateTime > 2) {
			axl.whiteTime = 1200;
			axl.isWhite = true;
			axl.playSound("ching");
			if (player.input.isHeld(Control.Jump, player) && !character.grounded) {
				axl.changeState(new HoverAxlWC(), true);
			} else {
				character.changeToIdleOrFall();
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException(); ;
		axl.player.currency -= AxlWC.WhiteAxlCost;
		axl.useGravity = false;
		axl.stopMoving();
		axl.fillHealthToMax();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		axl.useGravity = true;
		if (axl != null) {
			axl.invulnTime = 0.5f;
		}
	}
}

public class HoverAxlWC : CharState {
	public SoundWrapper? sound;
	float hoverTime;
	Anim hoverExhaust = null!;
	AxlWC axl = null!;

	public HoverAxlWC() : base("hover") {
		useGravity = false;
		exitOnLanding = true;
		airMove = true;
		attackCtrl = true;
		normalCtrl = true;
	}

	public override void update() {
		base.update();
		if (character.vel.y < 0) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
		if (character.gravityWellModifier > 1) {
			character.vel.y += Global.level.gravity / 2f;
		}
		hoverTime += Global.spf;
		hoverExhaust.changePos(exhaustPos());
		hoverExhaust.xDir = axl.xDir;
		if ((hoverTime > 2 && !axl.isWhite) || !player.input.isHeld(Control.Jump, player)) {
			character.changeState(new Fall(), true);
		}
	}

	public Point exhaustPos() {
		if (character.currentFrame.POIs.Length == 0) { return character.pos; };
		Point exhaustPOI = character.currentFrame.POIs.Last();
		return character.pos.addxy(exhaustPOI.x * axl.xDir, exhaustPOI.y);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
		character.vel = new Point();
		hoverExhaust = new Anim(
			exhaustPos(), "hover_exhaust", axl.xDir, player.getNextActorNetId(), false, sendRpc: true
		);
		hoverExhaust.setzIndex(ZIndex.Character - 1);
		if (character.ownedByLocalPlayer) {
			sound = character.playSound("axlHover", forcePlay: false, sendRpc: true);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		hoverExhaust.destroySelf();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("axlHover", character.netId);
	}
}

public class DodgeRollAxlWC : CharState {
	public int initialDashDir;
	AxlWC axl = null!;

	public DodgeRollAxlWC() : base("roll") {
		attackCtrl = true;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}
		character.move(new Point(character.getDashSpeed() * initialDashDir, 0));
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
		character.isDashing = true;

		initialDashDir = player.input.getXDir(player);
		if (initialDashDir == 0) {
			initialDashDir = character.xDir;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		axl.dodgeRollCooldown = Axl.maxDodgeRollCooldown * 60;
	}
}

public class OcelotSpin : CharState {
	public bool specialPressed;
	public float soundTimer;
	private AxlWC axl = null!;

	public OcelotSpin() : base("ocelotspin") {
		exitOnAirborne = true;
		normalCtrl = true;
	}

	public override void update() {
		if (character.frameIndex >= 1 && character.frameIndex <= 10 && soundTimer <= 0) {
			character.playSound("cutter");
			soundTimer = 20;
		} else {
			soundTimer--;
		}
		if (player.input.isPressed(Control.Special1, player)) {
			specialPressed = true;
		}

		if (specialPressed && character.frameIndex == 11) {

			if (!player.input.isHeld(Control.Down, player)) {
				character.changeState(new AxlSpinKick(), true);
			} else {
				character.frameIndex = 7;
			}
			specialPressed = false;
		}

		if (character.isAnimOver()) {
			axl.armAngle = -64;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class AxlFlashKick : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlFlashKick() : base("flashkick") { }

	public override void update() {
		base.update();
		if (character.frameIndex <= 3) {
			useGravity = false;
			character.move(new Point(-80 * character.xDir, 0));
		}
		if (character.frameIndex >= 4 && !shot) {
			useGravity = true;
			shot = true;
			character.vel.y = -character.getJumpPower();
			new AxlFlashKickProj(
			new StormTornado(), character.pos.addxy(15 * character.xDir, -26),
			character.xDir, player, player.getNextActorNetId(), true);

			character.playSound("genocideCutter2", sendRpc: true);
		}

		if (character.isAnimOver()) {
			axl.armAngle = 0;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = 0;
		character.iframesTime = 6;
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class AxlFlashKickProj : Projectile {
	public AxlFlashKickProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 3, player, "axl_flashkick_proj", Global.defFlinch, 0.2f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		shouldClang = true;
		isJuggleProjectile = true;
		isDeflectShield = true;
		maxTime = 0.1f;
		projId = (int)ProjIds.AxlFlashKickProj;
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

public class TailShot : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public TailShot() : base("tailshot") {
		canStopJump = true;
		canJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 3) {
			character.move(new Point(-80 * character.xDir, 0));
			axl.iframesTime = 2;
		}
		if (character.frameIndex >= 3 && !shot) {
			shot = true;
			character.playSound("axlBulletCharged", sendRpc: true);
			//axl.mainWeapon.addAmmo(-2, player);
		}

		// Jack: Erhm stop changing the inputs of my stuff without asking. GRRRRRRRRRRRRR.
		// Gacel: Did not knew you were going to add a special-button variant. Will ask next time.
		if (character.frameIndex >= 4) {
			if (player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
				character.changeState(new AxlString1(), true);
			}
			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new AxlSpinKick(), true);
			}
		}
		if (character.isAnimOver()) {
			axl.armAngle = 32;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class AxlString1 : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlString1() : base("string_1") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		airMoveTurn = false;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlMeleeBullet(
				axl, gunpos, character.xDir,
				player.getNextActorNetId(), sendRpc: true
			);
			character.playSound("axlBulletCharged", sendRpc: true);
			//axl.mainWeapon.addAmmo(-2, player);
		}

		if (character.frameIndex >= 4 && player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
			character.changeState(new AxlString2(), true);
		}
		if (character.isAnimOver()) {
			axl.armAngle = 16;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		canStopJump = true;
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}


public class AxlString2 : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlString2() : base("string_2") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		airMoveTurn = false;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0) {
			character.iframesTime = 2;
		}
		if (character.frameIndex < 3 && axl.grounded) {
			float xInput = player.input.getXDir(player);
			float moveSpeed = 80 * axl.getRunDebuffs();
			if (xInput == 0) {
				xInput = -axl.xDir * 0.5f;
			}
			character.move(new Point(moveSpeed * xInput, 0));
		}
		if (character.frameIndex >= 3 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlMeleeBullet(
				axl, character.pos.addxy(30 * character.xDir, -26),
				character.xDir, player.getNextActorNetId(), sendRpc: true
			);
			character.playSound("axlBulletCharged", sendRpc: true);
			//axl.mainWeapon.addAmmo(-2, player);
		}

		if (character.frameIndex >= 4 && player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
			character.changeState(new AxlString3(), true);
		}
		if (character.isAnimOver()) {
			axl.armAngle = 48;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class AxlString3 : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlString3() : base("string_3") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		airMoveTurn = false;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0) {
			character.iframesTime = 2;
		}
		if (character.frameIndex < 3 && axl.grounded) {
			float xInput = player.input.getXDir(player);
			float moveSpeed = 80 * axl.getRunDebuffs();
			if (xInput == 0) {
				xInput = -axl.xDir * 0.5f;
			}
			character.move(new Point(moveSpeed * xInput, 0));
		}
		if (character.frameIndex >= 3 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlMeleeBullet(
				axl, gunpos, character.xDir,
				player.getNextActorNetId(), sendRpc: true
			);
			character.playSound("axlBulletCharged", sendRpc: true);
			//axl.mainWeapon.addAmmo(-2, player);
		}
		if (character.isAnimOver()) {
			axl.armAngle = -32;
			character.changeToIdleOrFall();
		}

		if (character.frameIndex >= 5 && player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
			character.changeState(new AxlString4(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class AxlString4 : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlString4() : base("string_4") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		airMoveTurn = false;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && axl.grounded) {
			character.iframesTime = 2;
		}
		if (character.frameIndex < 3) {
			float xInput = player.input.getXDir(player);
			float moveSpeed = 80 * axl.getRunDebuffs();
			if (xInput == 0) {
				xInput = -axl.xDir * 0.5f;
			}
			character.move(new Point(moveSpeed * xInput, 0));
		}
		if (character.frameIndex >= 3 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlMeleeBullet(
				axl, gunpos, character.xDir,
				player.getNextActorNetId(), sendRpc: true
			);

			character.playSound("axlBulletCharged", sendRpc: true);
			//axl.mainWeapon.addAmmo(-2, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		if (character.frameIndex >= 4 && player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
			character.changeState(new AxlString5(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class AxlString5 : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlString5() : base("string_5") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		airMoveTurn = false;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 3 && axl.grounded) {
			character.iframesTime = 7;
		}

		if (character.frameIndex <= 4) {
			float xInput = player.input.getXDir(player);
			float moveSpeed = 80 * axl.getRunDebuffs();
			if (xInput == 0) {
				xInput = -axl.xDir * 0.5f;
			}
			character.move(new Point(moveSpeed * xInput, 0));
		}

		if (character.frameIndex >= 10 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlMeleeBullet(
				axl, gunpos.addxy(1 * character.xDir, -1),
				character.xDir, player.getNextActorNetId(), sendRpc: true
			);
			new AxlMeleeBullet(
				axl, gunpos.addxy(-1 * character.xDir, 1),
				character.xDir, player.getNextActorNetId(), sendRpc: true
			);
			character.playSound("axlBulletCharged", sendRpc: true);
		//	axl.mainWeapon.addAmmo(-2, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class EvasionBarrage : CharState {
	public AxlWC axl = null!;
	public float pushBackSpeed;
	float projTime = 99;
	public int bulletsFired;
	public bool exitCond;
	public int lastFrameFired;

	public EvasionBarrage() : base("evasionshot") { }

	public override void update() {
		base.update();
			if (character.frameIndex <= 0) {
			character.iframesTime = 2;
		}
		if (pushBackSpeed > 0) {
			character.vel.y = 0;
			character.useGravity = false;
			if (!character.grounded || character.frameIndex >= 2) {
				character.move(new Point(-80 * character.xDir, -pushBackSpeed));
			}
			pushBackSpeed -= Physics.Gravity / 2f;
		} else {
			character.useGravity = true;
		}
		Point? gunpos = character.getFirstPOI();

		if (character.sprite.frameIndex >= 2) {
			character.move(new Point(character.xDir * -150, 0));
			if (gunpos != null && lastFrameFired != character.frameIndex) {
				lastFrameFired = character.frameIndex;
				BlueBulletProj.newWithDir(
					axl, gunpos.Value, character.xDir,
					player.getNextActorNetId(), sendRpc: true
				);
				new Anim(
					gunpos.Value.addxy(-2 * character.xDir, 0),
					"x8_axl_bullet_flash", character.xDir,
					player.getNextActorNetId(), true, sendRpc: true,
					host: character
				);
				character.playSound("axlBullet", sendRpc: true);
				axl.mainWeapon.addAmmo(-0.75f, player);
				bulletsFired++;
			}
		}
		if ((bulletsFired >= 4 || axl.mainWeapon.ammo <= 0) &&
			(character.frameIndex == 5 || character.frameIndex == 3)
		) {
			exitCond = true;
		}
		if (exitCond && character.frameIndex != 5 && character.frameIndex != 3) {
			axl.armAngle = 0;
			character.xPushVel = -100 * character.xDir;
			if (pushBackSpeed > 0) {
				character.vel.y = -pushBackSpeed;
			}
			else if (character.vel.y > 0) {
				character.vel.y *= 0.5f;
			}
			character.changeToLandingOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMovingWeak();
		pushBackSpeed = 180;
		axl = character as AxlWC ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}

public class RisingBarrage : CharState {
	public AxlWC axl = null!;
	float projTime;

	public RisingBarrage() : base("risingbarrage") {
		exitOnAirborne = true;
		// Jack: Keep this, it's funny as hell.
		// Gacel: Will do. But I need to add some exception to it..
		// so Axl cannot shoot the main shot while using it.
		attackCtrl = true; 
	}

	public override void update() {
		base.update();
		Point? gunpos = character.getFirstPOI();
				if (character.frameIndex <= 0) {
			character.iframesTime = 7;
		}
		if (character.sprite.frameIndex >= 2 && gunpos != null) {
			character.move(new Point(character.xDir * 150, 0));
			projTime += character.speedMul;

			if (projTime >= 4) {
				projTime = 0;
				var anim = new Anim(
					character.pos, "shoryuken_fade", character.xDir,
					player.getNextActorNetId(), true, sendRpc: true
				);
				anim.vel = new Point(-character.xDir * 50, 25);
				new BlueBulletProj(
					axl, gunpos.Value, axl.armDir == 1 ? -45 : 160,
					player.getNextActorNetId(), sendRpc: true
				);
				if (axl.frameIndex >= 4) {
					new AxlMeleeBullet(
						axl, gunpos.Value, character.xDir,
						player.getNextActorNetId(),
						byteAngle: -64 * axl.xDir, sendRpc: true
					);
				}
				axl.mainWeapon.addAmmo(-0.5f, player);
				character.playSound("axlBullet", sendRpc: true);
			}
		}
		if (stateFrames >= 30 || axl.mainWeapon.ammo <= 0) {
			axl.armAngle = -64;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class AxlRainDrop : CharState {
	public float pushBackSpeed;
	public bool landedOnce;
	public bool shot;
	public AxlWC axl = null!;

	public AxlRainDrop() : base("raindrop") {
		airMove = true;
	}

	public override void update() {
		if (character.frameIndex <= 0) {
			character.iframesTime = 8;
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


		if (character.frameIndex >= 4 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlMeleeBullet(
				axl, gunpos.addxy(0, -20), character.xDir,
				 player.getNextActorNetId(), sendRpc: true, byteAngle: 64 * axl.xDir
			);
			new AxlMeleeBullet(
				axl, gunpos.addxy(0, -20), character.xDir,
				 player.getNextActorNetId(), sendRpc: true, byteAngle: 64 * axl.xDir
			);
			character.playSound("axlBulletCharged", sendRpc: true);
			axl.mainWeapon.addAmmo(-2, player);
		}

		if (character.grounded && !landedOnce) {
			character.changeSpriteFromName("raindrop_land", true);
			landedOnce = true;
		}

		if (landedOnce) {
			if (player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
				character.changeState(new AxlString1(), true);
			}
			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new AxlSpinKick(), true);
			}
		}


		base.update();

		if (character.isAnimOver() && character.sprite.name.Contains("raindrop_land")) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}
}

public class AxlSpinKick : CharState {
	public AxlWC axl = null!;
	public float pushBackSpeed;

	public AxlSpinKick() : base("spinkick") {
		airMove = true;
		enterSound = "punch1";
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0) {
			character.iframesTime = 5;
		}
		if (character.frameIndex < 3 && character.grounded) {
			float xInput = player.input.getXDir(player);
			float moveSpeed = 100 * character.getRunDebuffs();
			if (xInput == 0) {
				xInput = -character.xDir * 0.5f;
			}
			character.move(new Point(moveSpeed * xInput, 0));
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

		if (character.frameIndex > 4) {
			if (player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
				character.changeState(new AxlString1(), true);
			}
			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new AxlRollBump(), true);
			}
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}
}

public class AxlAirRaid : CharState {
	public AxlWC axl = null!;
	public Character victim;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	float leechTime = 1;

	public AxlAirRaid(Character victim) : base("air_raid") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
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

		if (player.input.isPressed(Control.Shoot, player) && axl.mainWeapon.ammo > 0) {
			character.changeState(new AxlString1(), true);
		}
		if (player.input.isPressed(Control.Special1, player)) {
			character.changeState(new AxlRollBump(), true);
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
		axl = character as AxlWC ?? throw new NullReferenceException();
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		victim?.releaseGrab(character);
	}
}


public class AxlRollBump : CharState {
	public AxlWC axl = null!;
	private bool shot;

	public AxlRollBump() : base("rollbump") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		airMoveTurn = false;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && axl.grounded) {
			character.iframesTime = 2;
		}
		if (character.frameIndex < 3) {
			float xInput = player.input.getXDir(player);
			float moveSpeed = 90 * axl.getRunDebuffs();
			if (xInput == 0) {
				xInput = -axl.xDir * 0.5f;
			}
			character.move(new Point(moveSpeed * xInput, 0));
		}
		if (character.frameIndex >= 3 && !shot) {
			Point gunpos = character.getFirstPOI() ?? axl.pos;
			shot = true;
			new AxlFlashKickProj(
				new StormTornado(), character.pos.addxy(15 * character.xDir, 0),
				character.xDir, player, player.getNextActorNetId(), true
			);
			character.playSound("punch1", sendRpc: true);
		
		}
		if (player.input.isPressed(Control.Special1, player) && character.frameIndex >= 3) {
			character.changeState(new AxlFlashKick(), true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}

public class RainStorm : CharState {
	bool jumpedYet;
	float projTime;
	AxlWC axl = null!;

	public RainStorm() : base("rainstorm") {
		superArmor = true;
		airMove = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		if (character.sprite.frameIndex >= 2 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			character.vel.y = -character.getJumpPower() * 1.5f;
		}
		Point gunpos = character.getFirstPOI() ?? character.pos;

		if (character.sprite.frameIndex >= 2) {
			float xSpeed = 0;
			if (player.input.getXDir(player) == 0) {
				xSpeed = 60;
			}
			character.move(new Point(xSpeed * axl.xDir, -120f));
			projTime += character.speedMul;
			if (projTime >= 4 && character.frameIndex >= 6) {
				projTime = 0;
				new BlueBulletProj(
					axl, gunpos.addxy(0, -20), 64,
					player.getNextActorNetId(), sendRpc: true
				);
				if (character.frameIndex >= 7) {
					new AxlMeleeBullet(
						axl, gunpos.addxy(0, -20), character.xDir,
						player.getNextActorNetId(), sendRpc: true,
						byteAngle: 64 * axl.xDir
					);
				}
				character.playSound("axlBullet", sendRpc: true);
				axl.mainWeapon.addAmmo(-0.5f, player);
			}
		}

		if (character.isAnimOver() || character.grounded && stateFrames > 10) {
			axl.armAngle = 64;
			character.changeToLandingOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
		axl.isDashing = true;
	}
}

public class AxlBlock : CharState {
	private AxlWC axl = null!;

	public AxlBlock() : base("block") {
		exitOnAirborne = true;
		normalCtrl = true;
		attackCtrl = true;
		stunResistant = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();
		axl.armAngle = 0;
		if (!player.input.isHeld(Control.Down, player) ||  player.input.isHeld(Control.Jump, player)) {
			character.changeToIdleOrFall();
			return;
		}
		if(!Options.main.blockInput) return;
		if(!player.input.isHeld(Control.AxlAimBackwards,player)){
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}
public class AxlBlock2 : CharState {
	private AxlWC axl = null!;

	public AxlBlock2() : base("block") {
		exitOnAirborne = true;
		normalCtrl = true;
		attackCtrl = true;
		stunResistant = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();
		axl.armAngle = 0;
		if(!Options.main.blockInput) return;
		if(!player.input.isHeld(Control.AxlAimBackwards,player)){
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as AxlWC ?? throw new NullReferenceException();
	}
}
