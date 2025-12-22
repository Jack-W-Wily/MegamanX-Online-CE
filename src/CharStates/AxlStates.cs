using System.Linq;
using System;

namespace MMXOnline;
public class AxlState : CharState {
	public Axl axl = null!;

	public AxlState(
		string sprite, string shootSprite = "", string attackSprite = "",
		string transitionSprite = "", string transShootSprite = ""
	) : base(
		sprite, shootSprite, attackSprite,
		transitionSprite, transShootSprite
	) {
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = player.character as Axl ?? throw new NullReferenceException();
	}
}
public class HyperAxlStart : AxlState {
	public float radius = 200;
	public float time;
	public HyperAxlStart(bool isGrounded) : base(isGrounded ? "hyper_start" : "hyper_start_air") {
		invincible = true;
		statusEffectImmune = true;
	}

	public override void update() {
		base.update();

		foreach (var weapon in character.weapons) {
			for (int i = 0; i < 10; i++) weapon.rechargeAmmo(0.1f);
		}

		if (character.loopCount > 8) {
			axl.whiteAxlTime = axl.maxHyperAxlTime;
			RPC.setHyperAxlTime.sendRpc(character.player.id, axl.whiteAxlTime, 1);
			axl.playSound("ching");
			if (player.input.isHeld(Control.Jump, player)) {
				axl.changeState(new Hover(), true);
			} else {
				character.changeToIdleOrFall();
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.clenaseAllDebuffs();
		axl = character as Axl ?? throw new NullReferenceException();
		if (!axl.hyperAxlUsed) {
			axl.hyperAxlUsed = true;
			axl.player.currency -= 10;
		}
		axl.useGravity = false;
		axl.vel = new Point();
		axl.fillHealthToMax();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		axl.useGravity = true;
		if (axl != null) {
			axl.invulnTime = 0.5f;
		}
	}
}

public class Hover : AxlState {
	public SoundWrapper? sound;
	float hoverTime;
	Anim? hoverExhaust;
	public Hover() : base("hover", "hover", "hover", "hover") {
		exitOnLanding = true;
		airMove = true;
		attackCtrl = true;
		normalCtrl = true;
	}

	public override void update() {
		base.update();

		accuracy = 0;
		Point prevPos = character.pos;

		if (character.pos.x != prevPos.x) {
			accuracy = 5;
		}

		if (character.vel.y < 0) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}

		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}

		hoverTime += Global.spf;
		if (hoverExhaust != null) {
			hoverExhaust.changePos(exhaustPos());
			hoverExhaust.xDir = axl.getAxlXDir();
		}
		if ((hoverTime > 2 && !axl.isWhiteAxl()) ||
			!character.player.input.isHeld(Control.Jump, character.player)
		) {
			character.changeState(character.getFallState(), true);
		}
	}

	public Point exhaustPos() {
		if (character.currentFrame.POIs.Length == 0) return character.pos;
		Point exhaustPOI = character.currentFrame.POIs.Last();
		return character.pos.addxy(exhaustPOI.x * axl.getAxlXDir(), exhaustPOI.y);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException() ;
		character.useGravity = false;
		character.vel = new Point();
		hoverExhaust = new Anim(
			exhaustPos(), "hover_exhaust", axl.getAxlXDir(), player.getNextActorNetId(), false, sendRpc: true
		);
		hoverExhaust.setzIndex(ZIndex.Character - 1);
		if (character.ownedByLocalPlayer) {
			sound = character.playSound("axlHover", forcePlay: false, sendRpc: false);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		hoverExhaust?.destroySelf();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("axlHover", character.netId);
	}
}

public class DodgeRoll : AxlState {
	public float dashTime = 0;
	public int initialDashDir;
	public DodgeRoll() : base("roll") {
		normalCtrl = true;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
		character.isDashing = true;
		character.burnTime -= 1;
		initialDashDir = character.xDir;
		if (player.input.isHeld(Control.Left, player)) initialDashDir = -1;
		else if (player.input.isHeld(Control.Right, player)) initialDashDir = 1;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		axl.dodgeRollCooldown = Axl.maxDodgeRollCooldown;
	}

	public override void update() {
		base.update();
		var move = new Point(0, 0);
		move.x = character.getDashSpeed() * initialDashDir;
		character.move(move);
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}

public class SniperAimAxl : AxlState {

	public SniperAimAxl() : base("crouch") {

	}

	public override void update() {
		base.update();
		if (!axl?.isZooming() == true) {
			axl?.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException() ;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		if (axl?.isZooming() == true) {
			axl?.zoomOut();
		}
	}
}
public class AxlTaunt : CharState {
	public AxlTaunt() : base("win") {

	}
	public override void update() {
		base.update();
		if (character.isAnimOver() && !Global.level.gameMode.playerWon(player)) {
			character.changeToIdleOrFall();
		}
		if (!once) {
			once = true;
			character.playSound("ching", sendRpc: true);
		}
	}
}




public class TailShotWA : CharState {
	public Axl axl = null!;
	private bool shot;

	public TailShotWA() : base("tailshot") {
		canStopJump = true;
		canJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 3) {
			character.move(new Point(-80 * character.xDir, 0));
			axl.iframesTime = 8;
		}
		if (character.frameIndex >= 3 && !shot) {
			shot = true;
			character.playSound("axlBulletCharged", sendRpc: true);

			if (character.OverDrive) {
      		 new DynamoBeam(new ElectricSpark(), character.pos.addxy(20 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
			character.shakeCamera(true);
			}
			//axl.mainWeapon.addAmmo(-2, player);
		}

		// Jack: Erhm stop changing the inputs of my stuff without asking. GRRRRRRRRRRRRR.
		// Gacel: Did not knew you were going to add a special-button variant. Will ask next time.
		if (character.frameIndex >= 4) {
			if (player.input.isPressed(Control.Shoot, player)) {
				character.changeState(new AxlString1WA(), true);
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
		axl = character as Axl ?? throw new NullReferenceException();
	}
}

public class AxlString1WA : CharState {
	public Axl axl = null!;
	private bool shot;

	public AxlString1WA() : base("string_1") {
		canJump = true;
		canStopJump = true;
		airMove = true;
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

		if (character.frameIndex >= 4 && player.input.isPressed(Control.Shoot, player)) {
			character.changeState(new AxlString2WA(), true);
		}
		if (character.isAnimOver()) {
			axl.armAngle = 16;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		canStopJump = true;
		axl = character as Axl ?? throw new NullReferenceException();
	}
}


public class AxlString2WA : CharState {
	public Axl axl = null!;
	private bool shot;

	public AxlString2WA() : base("string_2") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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

		if (character.frameIndex >= 4 && player.input.isPressed(Control.Shoot, player)) {
			character.changeState(new AxlString3WA(), true);
		}
		if (character.isAnimOver()) {
			axl.armAngle = 48;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
	}
}

public class AxlString3WA : CharState {
	public Axl axl = null!;
	private bool shot;

	public AxlString3WA() : base("string_3") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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

		if (character.frameIndex >= 5 && player.input.isPressed(Control.Shoot, player)) {
			character.changeState(new AxlString4WA(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
	}
}

public class AxlString4WA : CharState {
	public Axl axl = null!;
	private bool shot;

	public AxlString4WA() : base("string_4") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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

		if (character.frameIndex >= 4 && player.input.isPressed(Control.Shoot, player)) {
			character.changeState(new AxlString5WA(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
	}
}

public class AxlString5WA : CharState {
	public Axl axl = null!;
	private bool shot;

	public AxlString5WA() : base("string_5") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 3 && character.OverDrive) {
			character.iframesTime = 10;
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
		axl = character as Axl ?? throw new NullReferenceException();
	}
}

public class EvasionBarrageWA : CharState {
	public Axl axl = null!;
	public float pushBackSpeed;
	float projTime = 99;
	public int bulletsFired;
	public bool exitCond;
	public int lastFrameFired;

	public EvasionBarrageWA() : base("evasionshot") { }

	public override void update() {
		base.update();
			if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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
				bulletsFired++;
			}
		}
		if ((bulletsFired >= 4) &&
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
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		pushBackSpeed = 180;
		axl = character as Axl ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}

public class RisingBarrageWA : CharState {
	public Axl axl = null!;
	float projTime;

	public RisingBarrageWA() : base("risingbarrage") {
		exitOnAirborne = true;
		// Jack: Keep this, it's funny as hell.
		// Gacel: Will do. But I need to add some exception to it..
		// so Axl cannot shoot the main shot while using it.
		attackCtrl = true; 
	}

	public override void update() {
		base.update();
		Point? gunpos = character.getFirstPOI();
				if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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
			character.playSound("axlBullet", sendRpc: true);
			}
		}
		if (stateFrames >= 30 ) {
			axl.armAngle = -64;
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class AxlRainDropWA : CharState {
	public float pushBackSpeed;
	public bool landedOnce;
	public bool shot;
	public Axl axl = null!;

	public AxlRainDropWA() : base("raindrop") {
		airMove = true;
	}

	public override void update() {
		if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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
			
		}

		if (character.grounded && !landedOnce) {
			character.changeSpriteFromName("raindrop_land", true);
			landedOnce = true;
		}

		if (landedOnce) {
			if (player.input.isPressed(Control.Shoot, player) ) {
				character.changeState(new AxlString1WA(), true);
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
		axl = character as Axl ?? throw new NullReferenceException();
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}
}

public class AxlSpinKickWA : CharState {
	public Axl axl = null!;
	public float pushBackSpeed;

	public AxlSpinKickWA() : base("spinkick") {
		airMove = true;
		enterSound = "punch1";
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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
			if (player.input.isPressed(Control.Shoot, player) ) {
				character.changeState(new AxlString1WA(), true);
			}
			if (player.input.isPressed(Control.Special1, player)) {
				character.changeState(new AxlRollBumpWA(), true);
			}
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}
}

public class AxlAirRaidWA : CharState {
	public Axl axl = null!;
	public Character victim;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	float leechTime = 1;

	public AxlAirRaidWA(Character victim) : base("air_raid") {
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

		if (player.input.isPressed(Control.Shoot, player)) {
			character.changeState(new AxlString1WA(), true);
		}
		if (player.input.isPressed(Control.Special1, player)) {
			character.changeState(new AxlRollBumpWA(), true);
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
		axl = character as Axl ?? throw new NullReferenceException();
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		victim?.releaseGrab(character);
	}
}


public class AxlRollBumpWA : CharState {
	public Axl axl = null!;
	private bool shot;

	public AxlRollBumpWA() : base("rollbump") {
		canJump = true;
		canStopJump = true;
		airMove = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex <= 0 && character.OverDrive) {
			character.iframesTime = 10;
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
		axl = character as Axl ?? throw new NullReferenceException();
	}
}

public class RainStormWA : CharState {
	bool jumpedYet;
	float projTime;
	Axl axl = null!;

	public RainStormWA() : base("rainstorm") {
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
			}
		}

		if (character.isAnimOver() || character.grounded && stateFrames > 10) {
			axl.armAngle = 64;
			character.changeToLandingOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl ?? throw new NullReferenceException();
		axl.isDashing = true;
	}
}

