using System;

namespace MMXOnline;

public abstract class ZeroGenericMeleeState : CharState {
	public Zero zero = null!;

	public int comboFrame = Int32.MaxValue;

	public string sound = "";
	public bool soundPlayed;
	public int soundFrame = Int32.MaxValue;

	public ZeroGenericMeleeState(string spr) : base(spr) {
	}

	public override void update() {
		base.update();
		if (character.sprite.frameIndex >= soundFrame && !soundPlayed) {
			character.playSound(sound, forcePlay: false, sendRpc: true);
			soundPlayed = true;
		}
		if (character.sprite.frameIndex >= comboFrame) {
			altCtrls[0] = true;
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.turnToInput(player.input, player);
		zero = character as Zero ?? throw new NullReferenceException();
	}

	public virtual bool altCtrlUpdate(bool[] ctrls) {
		return false;
	}
}

public class ZeroSlash1State : ZeroGenericMeleeState {
	public ZeroSlash1State() : base("attack") {
		sound = "saber1";
		soundFrame = 4;
		comboFrame = 6;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero.zeroTripleStartTime = Global.time;
	}


	public override bool altCtrlUpdate(bool[] ctrls) {
		if (zero.shootPressed || player.isAI) {
			zero.shootPressTime = 0;
			zero.changeState(new ZeroSlash2State(), true);
			return true;
		}
		return false;
	}
}

public class ZeroSlash2State : ZeroGenericMeleeState {
	public ZeroSlash2State() : base("attack2") {
		sound = "saber2";
		soundFrame = 1;
		comboFrame = 3;
	}

	public override bool altCtrlUpdate(bool[] ctrls) {
		if (zero.shootPressed || player.isAI) {
			zero.shootPressTime = 0;
			zero.changeState(new ZeroSlash3State(), true);
			return true;
		}
		return false;
	}
}

public class ZeroSlash3State : ZeroGenericMeleeState {
	public ZeroSlash3State() : base("attack3") {
		sound = "saber3";
		soundFrame = 1;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero.zeroTripleSlashEndTime = Global.time;
	}

	public override bool altCtrlUpdate(bool[] ctrls) {
		return false;
	}
}

public class ZeroAirSlashState : ZeroGenericMeleeState {
	public ZeroAirSlashState() : base("attack_air") {
		sound = "saber1";
		soundFrame = 3;
		comboFrame = 7;

		airMove = true;
		exitOnLanding = true;
		useDashJumpSpeed = true;
		canStopJump = true;
	}

	public override void update() {
		base.update();
		if (character.sprite.frameIndex >= comboFrame) {
			attackCtrl = true;
		}
	}
}

public class ZeroRollingSlashtate : ZeroGenericMeleeState {
	public ZeroRollingSlashtate() : base("attack_air2") {
		sound = "saber1";
		soundFrame = 1;

		airMove = true;
		exitOnLanding = true;
		useDashJumpSpeed = true;
		canStopJump = true;
	}
}

public class ZeroDoubleBuster : CharState {
	bool fired1;
	bool fired2;
	bool isSecond;
	bool shootPressedAgain;
	bool isPinkCharge;
	Zero zero = null!;

	public ZeroDoubleBuster(bool isSecond, bool isPinkCharge) : base("doublebuster", "", "", "") {
		this.isSecond = isSecond;
		superArmor = true;
		this.isPinkCharge = isPinkCharge;
		airMove = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		if (!character.ownedByLocalPlayer) return;

		if (player.input.isPressed(Control.Shoot, player)) {
			shootPressedAgain = true;
		}

		if (!fired1 && character.frameIndex == 3) {
			fired1 = true;
			if (!isPinkCharge) {
				character.playSound("buster3X3", sendRpc: true);
				new ZBuster4Proj(
					character.getShootPos(),
					character.getShootXDir(), 1, player, player.getNextActorNetId(), rpc: true
				);
			} else {
				character.playSound("buster2X3", sendRpc: true);
				new ZBuster2Proj(
					character.getShootPos(), character.getShootXDir(),
					0, player, player.getNextActorNetId(), rpc: true
				);
			}
		}
		if (!fired2 && character.frameIndex == 7) {
			fired2 = true;
			if (!isPinkCharge) {
				//zero.doubleBusterDone = true;
			} else {
				//character.stockCharge(false);
			}
			character.playSound("buster3X3", sendRpc: true);
			new ZBuster4Proj(
				character.getShootPos(), character.getShootXDir(),
				0, player, player.getNextActorNetId(), rpc: true
			);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else if (!isSecond && character.frameIndex >= 4 && !shootPressedAgain) {
			character.changeToIdleOrFall();
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "doublebuster_air";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as Zero ?? throw new NullReferenceException();

		if (!isPinkCharge) {
			//character.stockSaber(true);
		} else {
			//character.stockCharge(!isSecond);
		}
		sprite = "doublebuster";
		defaultSprite = sprite;
		landSprite = "doublebuster";
		if (!character.grounded || character.vel.y < 0) {
			defaultSprite = sprite;
			sprite = "doublebuster_air";
		}
		character.changeSpriteFromName(sprite, true);
		if (isSecond) {
			character.frameIndex = 4;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (isSecond && character is Zero zero) {
			//zero.doubleBusterDone = true;
		}
	}
}

public class AwakenedZeroHadangeki : CharState {
	bool fired;

	public AwakenedZeroHadangeki() : base("projswing") {
		landSprite = "projswing";
		airSprite = "projswing_air";
		useDashJumpSpeed = true;
		airMove = true;
		superArmor = true;
	}

	public override void update() {
		base.update();
		if (character.grounded) {
			character.isDashing = false;
		}
		if (character.frameIndex >= 7 && !fired) {
			character.playSound("zerosaberx3", sendRpc: true);
			fired = true;
			new ZSaberProj(
				character.pos.addxy(30 * character.xDir, -20), character.xDir,
				player, player.getNextActorNetId(), rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "projswing_air";
				character.changeSpriteFromName(sprite, false);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded || character.vel.y < 0) {
			sprite = "projswing_air";
			defaultSprite = sprite;
			character.changeSpriteFromName(sprite, true);
		}
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
	}
}

public class AwakenedZeroHadangekiWall : CharState {
	bool fired;
	public int wallDir;
	public Collider wallCollider;

	public AwakenedZeroHadangekiWall(int wallDir, Collider wallCollider) : base("wall_slide_attack") {
		this.wallDir = wallDir;
		this.wallCollider = wallCollider;
		superArmor = true;
		useGravity = false;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 4 && !fired) {
			character.playSound("zerosaberx3", sendRpc: true);
			fired = true;
			new ZSaberProj(
				character.pos.addxy(30 * -wallDir, -20), -wallDir,
				player, player.getNextActorNetId(), rpc: true
			);
		}
		if (character.isAnimOver()) {
			character.changeState(new WallSlide(wallDir, wallCollider));
			character.sprite.frameIndex = character.sprite.frames.Count - 1;
		}
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		useGravity = true;
	}
}
