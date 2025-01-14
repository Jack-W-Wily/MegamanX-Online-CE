using System;
using System.Linq;

namespace MMXOnline;
public class SpiralMagnumShoot : CharState {
	public AxlX8 axl = null!;
	public bool shoot;
	public Point shootPOI = new Point(-1, -1);

	public SpiralMagnumShoot(string transitionSprite = "")
		: base("magnum_shoot", "", "", transitionSprite) {

	}

	public override void update() {
		base.update();
		Point poi = character.getFirstPOI() ?? character.pos;
		if (character.sprite.frameIndex == 0 && !shoot) {
			shoot = true;
			new X8AxlSpiralMagnumProj(poi, axl.xDir, player, player.getNextActorNetId(), rpc: true);
			new X8AxlSpiralMagnumShell(poi.addxy(-15 * axl.xDir, 0), -axl.xDir, player.getNextActorNetId(), sendRpc: true);
			axl.flashAnim = new FlashAnim(poi, 0, player.getNextActorNetId(), true);
			axl.flashAnim.xDir = character.xDir;
		character.playSound("axlBullet", sendRpc: true);
		}
		if(!character.grounded){
			character.useGravity = false;
			sprite = "magnum_shoot_air";
			character.changeSpriteFromName(sprite, true);
		}
		if (character.isAnimOver()) {
			if(character.grounded){
			character.changeState(new Idle());}else{
				character.changeState(new X8AxlHover());
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.stopMovingWeak();
		axl = character as AxlX8 ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}
public class X8AxlHover : CharState {
	public SoundWrapper? sound;
	Anim hoverExhaust = null!;
	public AxlX8 axl = null!;

	public X8AxlHover() : base("hover","hover_shoot") {
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
		axl.hoverTime += Global.spf;
		/*if(axl.hoverTime <= 1){
			hoverExhaust.changeSprite("x8_axl_hover_anim_low", true);
		}*/
		hoverExhaust.changePos(exhaustPos());
		hoverExhaust.xDir = axl.xDir;
		if ((axl.hoverTime > 2 ) || !player.input.isHeld(Control.Jump, player)) {
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
		
		axl = character as AxlX8 ?? throw new NullReferenceException();
		character.vel = new Point();
		hoverExhaust = new Anim(
			exhaustPos(), "x8_axl_hover_anim", axl.xDir, player.getNextActorNetId(), false, sendRpc: true
		);
		hoverExhaust.setzIndex(ZIndex.Character + 1);
		if (character.ownedByLocalPlayer) {
			sound = character.playSound("x8AxlHover", forcePlay: false, sendRpc: true);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		hoverExhaust.destroySelf();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("x8AxlHover", character.netId);
	}
}


public class X8AxlDodgeRoll : CharState {
	int initialDashDir;
	public X8AxlDodgeRoll() : base("roll") {
		attackCtrl = true;
		normalCtrl = true;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.burnTime -= 1;
		character.isDashing = true;
		if (character.burnTime < 0) {
			character.burnTime = 0;
		}

		initialDashDir = character.xDir;
		if (player.input.isHeld(Control.Left, player)) initialDashDir = -1;
		else if (player.input.isHeld(Control.Right, player)) initialDashDir = 1;
		new Anim(
			character.getDashDustEffectPos(initialDashDir),"x8_axl_roll_dust", initialDashDir, 
			player.getNextActorNetId(), true, true);
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

		var move = new Point(0, 0);
		move.x = character.getDashSpeed() * initialDashDir;
		character.move(move);
		
	}
}
