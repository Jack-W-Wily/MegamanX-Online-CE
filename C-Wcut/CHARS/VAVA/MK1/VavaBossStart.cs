using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;






public class VB1 : CharState {
	
	public VB1() : base("ragingdemon_start", "") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		character.turnToInput(player.input, player);


		if (stateTime > 0.4f) {
			character.changeState(new VB2(1));
			character.playSound("vilehyperdashattack", true);
		}

	
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.playSound("ching", true);
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class VB2 : CharState {
	float trailTime;
	float chargeTime;

	Character? target;


	public VB2(float chargeTime) : base("ragingdemon_dash", "") {
		this.chargeTime = chargeTime;
		superArmor = true;
		invincible = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;
		character.move(new Point(character.xDir * 400, 0));

		if (stateTime > chargeTime) {
				character.changeState(new VB3(character.grounded));
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.turnToInput(player.input, player);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);

	}
}




public class RagingDemonSuccess : CharState {
	public Character? victim;
	float leechTime = 1;

	float timein = 0;

	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public RagingDemonSuccess(Character? victim) : base("ragingdemon_land", "", "", "") {
		this.victim = victim;
		grabTime = 8;
		invincible = true;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;


		
				victim.changeState(new VileStomped(character));
				var damager = new Damager(player, 1f, 0, 0);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.UPPunch);
		
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		victim?.releaseGrab(character);
	}
}



public class VB3 : CharState {
	public Vile vile = null!;

	bool first;
	bool second;
	bool third;

	public VB3(bool grounded) : base(getSprite(grounded)) {
	
		airSprite = "cannon_air";
		landSprite = "shoulder_cannon";
		invincible = true;
	}
	public static string getSprite(bool grounded) {
		return grounded ? "shoulder_cannon" : "cannon_air";
	}

	public override void update() {
		base.update();
		if (character.sprite.isAnimOver()) {
				character.changeState(new VB4(character.grounded));
		}

		if (character.frameIndex == 9 && !first) {
			shootLogic(vile);
			first = true;
		}
		if (character.frameIndex == 12 && !second) {
			shootLogic(vile);
			second = true;
		}
		if (character.frameIndex == 15 && !third) {
			shootLogic(vile);
			third = true;
		}
	}

	public static void shootLogic(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
		bool isMK2 = vile.isVileMK2;
		Point? headPosNullable = vile.getVileMK2StunShotPos();
		if (headPosNullable == null) return;
		Point shootVel = vile.getVileShootVel(true);
		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		int xDir = vile.xDir;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}


		vile.playSound("frontrunner", sendRpc: true);
		new StunShotProj(
			shootPos, xDir, MathF.Round(shootVel.byteAngle), vile,
			vile.player, vile.player.getNextActorNetId(), rpc: true
		);

		new RisingSpecterProj(
			shootPos, vile.xDir, vile, vile.player,
			vile.player.getNextActorNetId(), rpc: true
		);
		vile.playSound("risingSpecter", sendRpc: true);

		new VileCannonProj(
				shootPos, vile.xDir, 0, MathF.Round(shootVel.byteAngle), "vile_mk2_proj",
			vile, vile.player, vile.player.getNextActorNetId(), rpc: true
		);


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		character.turnToInput(player.input, player);
		if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
			exitOnAirborne = true;
		}
	}
}





public class VB4 : CharState {
	public Vile vile = null!;

	float leechTime = 1;


	public VB4(bool grounded) : base(getSprite(grounded)) {
	
		airSprite = "popcorn_hell";
		landSprite = "popcorn_hell";
		invincible = true;
	}
	public static string getSprite(bool grounded) {
		return grounded ? "popcorn_hell" : "popcorn_hell";
	}

	public override void update() {
		base.update();

		if (character.sprite.isAnimOver()) {
			character.changeState( new Taunt(), true);
		}

		leechTime += Global.spf;
	if (character.frameIndex == 4 && leechTime > 0.05f) {
			if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
			leechTime = 0;

			shootLogic(vile);
		}
	}


	public static void shootLogic(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
		bool isMK2 = vile.isVileMK2;
		Point? headPosNullable = vile.getVileMK2StunShotPos();
		if (headPosNullable == null) return;
		Point shootVel = vile.getVileShootVel(true);
		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		int xDir = vile.xDir;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}
		vile.playSound("mk2stunshot", sendRpc: true);
		new VileMissileProj(
				vile.pos.addxy(-50 * vile.xDir,-30), xDir, 2, MathF.Round(shootVel.byteAngle), "missile_pd_proj",
				vile, vile.player, vile.player.getNextActorNetId(), rpc: true
			);
		new TorpedoProjChargedX(vile.pos.addxy(0,-30), vile.xDir, vile, vile.player, vile.player.getNextActorNetId(true), 0, true);
			
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
			exitOnAirborne = true;
		}
	}
}












