using System.Linq;

namespace MMXOnline;

public class HyperAxlStart : CharState {
	public float radius = 200;
	public float time;
	public Axl axl;

	public HyperAxlStart(bool isGrounded) : base(isGrounded ? "hyper_start" : "hyper_start_air", "", "", "") {
		invincible = true;
	}

	public override void update() {
		base.update();

		foreach (var weapon in player.weapons) {
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
		axl = character as Axl;
		if (!axl.hyperAxlUsed) {
			axl.hyperAxlUsed = true;
			axl.player.currency -= 10;
		}
		axl.useGravity = false;
		axl.vel = new Point();
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

public class Hover : CharState {
	public SoundWrapper sound;
	float hoverTime;
	Anim hoverExhaust;
	Axl axl;

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
		hoverExhaust.changePos(exhaustPos());
		hoverExhaust.xDir = axl.getAxlXDir();
		if ((hoverTime > 2 && !axl.isWhiteAxl()) ||
			!character.player.input.isHeld(Control.Jump, character.player)
		) {
			character.changeState(new Fall(), true);
		}
	}

	public Point exhaustPos() {
		if (character.currentFrame.POIs.Length == 0) return character.pos;
		Point exhaustPOI = character.currentFrame.POIs.Last();
		return character.pos.addxy(exhaustPOI.x * axl.getAxlXDir(), exhaustPOI.y);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl;
		character.useGravity = false;
		character.vel = new Point();
		hoverExhaust = new Anim(
			exhaustPos(), "hover_exhaust", axl.getAxlXDir(), player.getNextActorNetId(), false, sendRpc: true
		);
		hoverExhaust.setzIndex(ZIndex.Character - 1);
		if (character.ownedByLocalPlayer) {
			sound = character.playSound("axlHover", forcePlay: false, sendRpc: true);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		hoverExhaust.destroySelf();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("axlHover", character.netId);
	}
}

public class DodgeRoll : CharState {
	public float dashTime = 0;
	public int initialDashDir;
	Axl axl;

	public DodgeRoll() : base("roll", "", "") {
		attackCtrl = true;
		normalCtrl = true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl;
		character.isDashing = true;
		character.burnTime -= 1;
		if (character.burnTime < 0) {
			character.burnTime = 0;
		}

		initialDashDir = character.xDir;
		if (player.input.isHeld(Control.Left, player)) initialDashDir = -1;
		else if (player.input.isHeld(Control.Right, player)) initialDashDir = 1;
		character.specialState = (int)SpecialStateIds.AxlRoll;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		axl.dodgeRollCooldown = Axl.maxDodgeRollCooldown;
		character.specialState = (int)SpecialStateIds.None;
	}

	public override void update() {
		base.update();

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

		//if (character.frameIndex >= 4) return;

		dashTime += Global.spf;

		var move = new Point(0, 0);
		move.x = character.getDashSpeed() * initialDashDir;
		character.move(move);
	//	if (stateTime > 0.1) {
		//	stateTime = 0;
			//new Anim(this.character.pos.addxy(0, -4), "dust", this.character.xDir, null, true);
		//}
	}
}

public class SniperAimAxl : CharState {
	public Axl axl;

	public SniperAimAxl() : base("crouch", "", "") {

	}

	public override void update() {
		base.update();
		if (!axl.isZooming()) {
			axl.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (axl.isZooming()) {
			axl.zoomOut();
		}
	}
}



public class OcelotSpin : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public OcelotSpin(string transitionSprite = "")
		: base("ocelotspin", "", "", transitionSprite)
	{
		airMove = true;
	
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



		var gunpos = character.getFirstPOI();
		

		if (player.input.isPressed(Control.Special1, player)){
			

			character.sprite.frameIndex = 3;

			
		
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class TailShot : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public TailShot(string transitionSprite = "")
		: base("tailshot", "", "", transitionSprite)
	{
		airMove = true;
	
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class EvasionBarrage : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;
	public Point shootPOI = new Point(-1, -1);


	public EvasionBarrage(string transitionSprite = "")
		: base("evasionshot", "", "", transitionSprite)
	{
		airMove = true;
	
	}

	float projTime;

	public override void update()
	{
		
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-90 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-80 * character.xDir, 0));
			}
			character.useGravity = true;
		}

			var gunpos = character.getFirstPOI();
		
			if (character.sprite.frameIndex >= 2) {
			character.move(new Point(character.xDir * 150, -120f));
			projTime += Global.spf;
			if (projTime > 0.06f) {
				projTime = 0;
				var anim = new Anim(character.getCenterPos(), "shoryuken_fade", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
				anim.vel = new Point(-character.xDir * 50, 25);	
				new BlueBulletProj(new DoubleBullet(), gunpos.Value, character.xDir, true, player, player.getNextActorNetId(), sendRpc: true);
				character.playSound("axlBullet", sendRpc: true);
			}
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

public class RisingBarrage : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public RisingBarrage(string transitionSprite = "")
		: base("risingbarrage", "", "", transitionSprite)
	{
		airMove = true;
	
	}

	float projTime;

public override void update()
	{
		
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-90 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-80 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		var gunpos = character.getFirstPOI();
		
			if (character.sprite.frameIndex >= 2) {
			character.move(new Point(character.xDir * 150, -120f));
			projTime += Global.spf;
			
			if (projTime > 0.06f) {
				projTime = 0;
				var anim = new Anim(character.pos, "shoryuken_fade", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
				anim.vel = new Point(-character.xDir * 50, 25);	
				new BlueBulletProj(new DoubleBullet(), gunpos.Value, character.xDir, true, player, player.getNextActorNetId(), sendRpc: true);
				character.playSound("axlBullet", sendRpc: true);
			}
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

public class RainStorm : CharState {
	bool jumpedYet;
	float timeInWall;
	bool isUnderwater;
	Anim anim;
	float projTime;
	Axl axl;

	public RainStorm(bool isUnderwater) : base("rainstorm", "", "") {
		this.isUnderwater = isUnderwater;
		superArmor = true;
		airMove = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && anim != null) {
			anim.visible = false;
		}

		if (character.sprite.frameIndex >= 2 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			character.vel.y = -character.getJumpPower() * 1.5f;
		}
			var gunpos = character.getFirstPOI();
		
		if (character.sprite.frameIndex >= 2) {
			character.move(new Point(character.xDir * 150, -120f));
			projTime += Global.spf;
			if (projTime > 0.06f) {
				projTime = 0;
				var anim = new Anim(character.getCenterPos(), "shoryuken_fade", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
				anim.vel = new Point(-character.xDir * 50, 25);	
				new BlueBulletProj(new DoubleBullet(), gunpos.Value, character.xDir, false, player, player.getNextActorNetId(), sendRpc: true);
				character.playSound("axlBullet", sendRpc: true);
			}
		} 

		if (character.isAnimOver()) {
			character.changeState(new Fall());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		axl = character as Axl;
	}

	public override void onExit(CharState newState) {
		if (anim != null) {
			anim.destroySelf();
			anim = null;
		}
		base.onExit(newState);
	//mmx.shoryukenCooldownTime = mmx.maxShoryukenCooldownTime;
	}
}






public class BlueBulletProj : Projectile {
	bool groundedVariant;

	public BlueBulletProj(
		DoubleBullet weapon, Point pos, int xDir,
		bool groundedVariant, Player player, ushort netProjId, bool sendRpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player, "axl_bullet_blue", 4, 0.1f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.BlueBullet;
		fadeSprite = "axl_bullet_fade";
		destroyOnHit = true;
		this.groundedVariant = groundedVariant;

		maxTime = 1f;
	
		
		if (!groundedVariant) {
			vel = new Point(xDir, 2f);
			vel = vel.normalize().times(350);
		
		} else {
			vel = new Point(xDir, -0.5f);
			vel = vel.normalize().times(350);
		
		}
		angle = vel.angle;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir, (byte)(groundedVariant ? 1 : 0));
		}
	}

	public override void update() {
		base.update();

		if (owner.isAxl && owner.character != null && owner.character.charState is RisingBarrage)
			{
			this.vel.y = -250;	
			}

	
		if (!groundedVariant) {
			 
				vel.x *= 0.9f;
			
		} else {
			
				vel.y *= 0.9f;
			
		}
	}

	public override void onHitWall(CollideData other) {
		destroySelf(fadeSprite, disableRpc: true);
	}
}


