using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;





public class RagingDemonStart : CharState {
	
	public RagingDemonStart() : base("ragingdemon_start", "") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		character.turnToInput(player.input, player);


		if (stateTime > 0.4f) {
			character.changeState(new RagingDemonDash(1));
			character.playSound("vilehyperdashattack", true);
		}

	
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.playSound("ching", true);
}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	}
}

public class RagingDemonDash : CharState {
	float trailTime;
	float chargeTime;

	Character? target;


	public RagingDemonDash(float chargeTime) : base("ragingdemon_dash", "") {
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

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}


public class PopcornHell : CharState {
	public Vile vile = null!;

	float leechTime = 1;


	public PopcornHell(bool grounded) : base(getSprite(grounded)) {
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
		airSprite = "popcorn_hell";
		landSprite = "popcorn_hell";
	}
	public static string getSprite(bool grounded) {
		return grounded ? "popcorn_hell" : "popcorn_hell";
	}

	public override void update() {
		base.update();

		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
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
				vile.pos.addxy(8 * vile.xDir,-21), xDir, 2, MathF.Round(shootVel.byteAngle), "missile_pd_proj",
				vile, vile.player, vile.player.getNextActorNetId(), rpc: true
			);

	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
			exitOnAirborne = true;
		}
	}
}





public class ExplosiveRoundStateBoss : CharState {
	int bombNum;
	bool isNapalm;

	Character vile;

	public ExplosiveRoundStateBoss() : base("air_bomb_attack", "", "") {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();



		if (bombNum > 0 && player.input.isBPressed(player)) {
			character.changeState(new Fall(), true);
		}

		var inputDir = player.input.getInputDir(player);
		if (inputDir.x == 0) inputDir.x = character.xDir;
		if (stateTime > 0f && bombNum == 0) {
			bombNum++;
			new MK2NapalmGrenadeProj(
					character.pos, character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				);
			new MK2NapalmGrenadeProj(
					character.pos, -character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class ShoulderCannon : CharState {
	public Vile vile = null!;

	bool first;
	bool second;
	bool third;

	public ShoulderCannon(bool grounded) : base(getSprite(grounded)) {
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
		airSprite = "cannon_air";
		landSprite = "shoulder_cannon";
	}
	public static string getSprite(bool grounded) {
		return grounded ? "shoulder_cannon" : "cannon_air";
	}

	public override void update() {
		base.update();
		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
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
		if (vile.phase2) {
			new RisingSpecterProj(
				shootPos, vile.xDir, vile, vile.player,
				vile.player.getNextActorNetId(), rpc: true
			);
			vile.playSound("risingSpecter", sendRpc: true);
		}
		new VileCannonProj(
				shootPos, vile.xDir, 0, MathF.Round(shootVel.byteAngle), "vile_mk2_proj",
			vile, vile.player, vile.player.getNextActorNetId(), rpc: true
		);


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();

		if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
			exitOnAirborne = true;
		}
	}
}



public class CrimsonPhantomState : CharState {

	public float vileAmmoUsage;

	public CrimsonPhantomState(
		 bool grounded
	) : base(
		grounded ? "crimson_phantom" : "fall"
	) {
		invincible = true;
		vileAmmoUsage = 16f;
		enterSound = "distortion_a";
		specialId = SpecialStateIds.AxlRoll;
	}



	public override void update() {
		base.update();

		if (!character.ownedByLocalPlayer) {
			return;
		}

		
		if (stateTime < 0.15f) {
			character.addRenderEffect(RenderEffectType.StockedChargeLv2, 0.05f, 0.1f);
		}
		if (stateTime > 0f && stateTime < 0.35f) {
			character.move(new Point(character.xDir * -500f, 0));
		}
		if (stateTime >= 0.35) {

		}

		if (stateTime >= 0.15f) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
		if (character.sprite.name.Contains("mk2")) {
			character.changeSpriteFromName("roll", true);
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class InfinityGigAttackBossVer : CharState {
	bool shot = false;
	InfinityGigSecond? proj;
	float specialPressTime;

	public float pushBackSpeed;

	public InfinityGigAttackBossVer(string transitionSprite = "") : base("infinity_gig_boss", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);


		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}

		
		if (proj != null) {
				specialPressTime = 0.25f;
				

				if (character.isAnimOver()) {
					character.changeToIdleOrFall();
					return;
				}
			
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

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 3;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new InfinityGigSecond(character.pos.add(poi), character.xDir, character, player, player.getNextActorNetId(true), 30, true);
					
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


}


public class CrimsonPhantomState2 : CharState {

	public float vileAmmoUsage;

	public CrimsonPhantomState2(
		 bool grounded
	) : base(
		grounded ? "crimson_phantom" : "fall"
	) {
		invincible = true;
		vileAmmoUsage = 16f;
		enterSound = "distortion_a";
		specialId = SpecialStateIds.AxlRoll;
	}



	public override void update() {
		base.update();

		if (!character.ownedByLocalPlayer) {
			return;
		}
		if (stateTime < 0.15f) {
			character.addRenderEffect(RenderEffectType.StockedChargeLv2, 0.05f, 0.1f);
		}
		if (stateTime > 0f && stateTime < 0.35f) {
			character.move(new Point(character.xDir * 500f, 0));
		}
		if (stateTime >= 0.35) {

		}

		if (stateTime >= 0.15f) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class VavaBurensen1 : CharState {

	public RekkohaEffect? effect;
	public VavaBurensen1() : base("burensen_1") {
		canGainMeter = false;
	}

	public override void update() {
		base.update();
		character.isDashing = true;
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (player.isMainPlayer) {
			effect = new RekkohaEffect();
		}

		character.playSound("ching", sendRpc: true);
		new GigaCrushBackwall(character.pos, character);
		new HitStop(character.pos, player, player.getNextActorNetId(), 
		player.ownedByLocalPlayer, overrideTime: 0.3f, sendRpc: true);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}



public class VavaBurensen2 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen2(Character? victim) : base("hyperdash_start", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		if (timein > 0.2f
		) {
		
			character.changeState(new VavaBurensen3(victim), true);
			
		}
			
		character.isDashing = true;
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	
	}
}








public class VavaBurensen3 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen3(Character? victim) : base("hyperdash_attack", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		if (timein > 0.4f) {
		
			character.changeState(new VavaBurensen4(victim), true);
			
		}
		character.isDashing = true;
		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				 
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("hyperdash_attack") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("deadlift") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	
	}
}




public class VavaBurensen4 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen4(Character? victim) : base("golden_right", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		if (timein > 0.5f) {
		
			character.changeState(new VavaBurensen4second(victim), true);
			
		}
		character.isDashing = true;
		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				 
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("hyperdash_attack") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("golden_right") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getCenterPos();
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, 0);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	
	}
}






public class VavaBurensen4second : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen4second(Character? victim) : base("drop_kick", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		if (timein > 0.5f) {
		
			character.changeState(new VavaBurensen5(victim), true);
			
		}
		character.isDashing = true;
		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				 
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("hyperdash_attack") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("golden_right") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getCenterPos();
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, 0);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
	
	}
}






public class VavaBurensen5 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen5(Character? victim) : base("burensen_2", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;

		if (!character.OverDrive){
		if (timein > 0.4f) {
			character.changeState(new VavaBurensen6(victim), true);
			}
		} else {
			if (timein > 2.4f) {
			character.changeState(new VavaBurensen6(victim), true);
			}
		}
		character.isDashing = true;

		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
//				var damager = new Damager(player, 2f, 0, 3);
//				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("hyperdash_attack") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
		}
		if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
		}
		if (character.sprite.name.Contains("deadlift") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
		}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}





public class VavaBurensen6 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen6(Character? victim) : base("burensen_3", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		character.isDashing = true;
		if (timein > 3f
		&& victim != null) {
		
			character.changeState(new VavaBurensen7(victim), true);
			
		}

		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				 
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("burensen_3") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("deadlift") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class VavaBurensen7 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen7(Character? victim) : base("cannon_execution", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		

		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
					new Anim(character.pos, "ef_laser_finisher_vava", character.xDir, null, true);
			
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				/*
				 */
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		character.isDashing = true;
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("cannon_execution") && character.frameIndex < 7) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("deadlift") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		 
		character.useGravity = true;
	}
}




public class VavaBurensen8 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen8(Character? victim) : base("hyperdash_start", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		if (timein > 0.2f && !Dashed
		&& !character.sprite.name.Contains("hyperdash_attack")) {
			Dashed = true;
			character.changeSpriteFromName("hyperdash_attack", true);
			
		}
		character.isDashing = true;	
		if (timein > 0.6f && !deadlifted) {
			character.changeSpriteFromName("deadlift", true);
			deadlifted = true;
		}
		if (timein > 1f && !dropkicked) {
			character.changeSpriteFromName("drop_kick", true);
			dropkicked = true;
		}
		if (timein > 1.5f && !stomped) {
			character.changeSpriteFromName("burensen_2", true);
			stomped = true;
		}
	
		if (timein > 2.3f && !character.sprite.name.Contains("burensen_3")
		&& !character.sprite.name.Contains("cannon_execution")) {
			character.changeSpriteFromName("burensen_3", true);
		}
		
		if (timein > 5 && !character.sprite.name.Contains("cannon_execution")) {
			character.changeSpriteFromName("cannon_execution", true);
			}

		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
				new Anim(character.pos, "ef_laser_finisher_vava", -character.xDir, null, true);
			
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				 
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("cannon_execution") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("deadlift") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}
}




/*

public class VavaBurensen7 : CharState {
	public Character? victim;
	public float leechTime = 1;

	public float timein = 0;



	public bool Dashed = false;

	public bool deadlifted = false;

	public bool dropkicked = false;

	public bool stomped = false;


	public bool fired = false;

	public bool victimWasGrabbedSpriteOnce = false;
	public float timeWaiting;
	public VavaBurensen7(Character? victim) : base("hyperdash_start", "", "", "") {
		this.victim = victim;
		grabTime = 30;
		invincible = true;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		timein += Global.spf;
		if (timein > 0.2f && !Dashed
		&& !character.sprite.name.Contains("hyperdash_attack")) {
			Dashed = true;
			character.changeSpriteFromName("hyperdash_attack", true);
			
		}
			
		if (timein > 0.6f && !deadlifted) {
			character.changeSpriteFromName("deadlift", true);
			deadlifted = true;
		}
		if (timein > 1f && !dropkicked) {
			character.changeSpriteFromName("drop_kick", true);
			dropkicked = true;
		}
		if (timein > 1.5f && !stomped) {
			character.changeSpriteFromName("burensen_2", true);
			stomped = true;
		}
	
		if (timein > 2.3f && !character.sprite.name.Contains("burensen_3")
		&& !character.sprite.name.Contains("cannon_execution")) {
			character.changeSpriteFromName("burensen_3", true);
		}
		
		if (timein > 5 && !character.sprite.name.Contains("cannon_execution")) {
			character.changeSpriteFromName("cannon_execution", true);
			}

		if (character.frameIndex == 8 && character.sprite.name.Contains("cannon_execution") && !fired) {
				fired = true;
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				 
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			character.shakeCamera(true);
			}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (character.sprite.name.Contains("burensen_2")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			if (leechTime > 0.10f && character.frameIndex == 7 || character.frameIndex == 3) {
				leechTime = 0;
				character.addHealth(0.13f);
				character.shakeCamera(sendRpc: true);
				var damager = new Damager(player, 2f, 0, 3);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.BurensenStomp);
			}

		}

		if (character.sprite.name.Contains("hyperdash_attack") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("drop_kick") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}
			if (character.sprite.name.Contains("deadlift") && !character.isAnimOver()) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));
			}

		if (character.sprite.name.Contains("cannon_execution") && character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		 
		victim?.changeToIdleOrFall();
	}
}

*/