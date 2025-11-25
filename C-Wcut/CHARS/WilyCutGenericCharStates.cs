using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class HitStop : Actor {
	public float time;
	public Player owner;
//	public ShaderWrapper? timeSlowShader;
	public const int radius = 120;
	public float drawRadius = 120;
	public float drawAlpha = 255;
	public bool isSnails;
	float maxTime = 4;
	float soundTime;
	public HitStop(
		Point pos, Player owner, ushort? netId, bool ownedByLocalPlayer, 
		float? overrideTime = null, bool sendRpc = false
	) : base(
		"empty", pos, netId, ownedByLocalPlayer, false
	) {
		useGravity = false;
		this.owner = owner;
		isSnails = overrideTime != null;

	//	if (Options.main.enablePostProcessing) {
	//		timeSlowShader = owner.timeSlowShader;
	//	}

		Global.level.HitStops.Add(this);

		if (isSnails) {
			maxTime = overrideTime!.Value;
		}

		netOwner = owner;
		netActorCreateId = NetActorCreateId.HitStop;
		if (sendRpc) {
			createActorRpc(owner.id);
		}

		canBeLocal = false;
	}

	public override void update() {
		base.update();
		var screenCoords = new Point(pos.x - Global.level.camX, pos.y - Global.level.camY);
		var normalizedCoords = new Point(screenCoords.x / Global.viewScreenW, 1 - screenCoords.y / Global.viewScreenH);

	


		time += Global.spf;
		if (time > maxTime) {
			destroySelf(disableRpc: true);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		Global.level.HitStops.Remove(this);
		owner.character.hitstops = null;
	}

	
}



/*
in Order for this to work as intended I added this to Damager.cs's applydamage section

			if (character.sprite.name.Contains("block") && damage > 0 && !isArmorPiercing(projId)) {
				if (!hitFromBehind(character, damagingActor, owner, projId)) {
					damage--;
					flinch = 0;
					if (damage < 3) {
						damage = 0;
						character.playSound("m10ding");
					}
				}
			}
*/


public class BlockWCUT : CharState {


	public const float maxBlockTime = 4;

	public float blockTime;


	public BlockWCUT() : base("block") {
		exitOnAirborne = true;
		attackCtrl = true;
		normalCtrl = true;
		immuneToWind = true;
		blockTime = maxBlockTime;
	}

	public override void update() {
		base.update();
		blockTime -= Global.spf;
		bool isHoldingGuard = (
			player.input.isL2Held(player)
		);

		character.turnToInput(player.input, player);

		if (blockTime == 0) {
			character.changeState(new BlockBreak(character.xDir), true);
		}
		if (!isHoldingGuard) {
			character.changeToIdleOrFall();
			return;
		}
		if (Global.level.gameMode.isOver) {
			if (Global.level.gameMode.playerWon(player)) {
				if (!character.sprite.name.Contains("_win")) {
					character.changeSpriteFromName("win", true);
				}
			} else {
				if (!character.sprite.name.Contains("lose")) {
					character.changeSpriteFromName("lose", true);
				}
			}
		}
	}
}




public class BossWait : CharState {




	public BossWait() : base("lose") {
		attackCtrl = true;
	}

	public override void update() {
		base.update();
	
	}
}



public class GlobalParryState : CharState {
	public GlobalParryState() : base("parry_start", "", "", "") {
		superArmor = true;
		airMove = true;
	}

	public override void update() {
		base.update();

		if (player.isZain){
			character.move(new Point(character.xDir * 350, 0));
		}


		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			character.genericParryCooldown = 30;
		}
	}

	public void counterAttack(Player damagingPlayer, Actor damagingActor, float damage) {
		Actor? counterAttackTarget = null;
		if (damagingActor is GenericMeleeProj gmp) {
			counterAttackTarget = gmp.ownerActor;
		}
		if (counterAttackTarget == null) {
			counterAttackTarget = damagingPlayer?.character ?? damagingActor;
		}

		Projectile? proj = damagingActor as Projectile;
		bool stunnableParry = proj != null && proj.canBeParried();
		if (counterAttackTarget != null && character.pos.distanceTo(counterAttackTarget.pos) < 75 &&
			counterAttackTarget is Character chr && stunnableParry
		) {
			if (player.isVile){
			if (!chr.ownedByLocalPlayer) {
				RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
			} else {
				chr.changeState(new ParriedState(), true);
			}
			if (player.isVile){
		Point shootVel = new Point(1, -3);
	
		}
			character.addHealth(1);
			}

			if (player.isZain){
					if (!chr.ownedByLocalPlayer) {
				RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
			} else {
				chr.changeState(new VileMK2Grabbed(character), true);
				character.changeState(new ZainGrab(), true);
			}
			}

		}
		character.playSound("zeroParry", forcePlay: false, sendRpc: true);	

		if (Helpers.randomRange(0,5) == 5){
			character.addHealth(1);
			character.changeState(new ParriedState(), true);
		}
		if (!player.isZain){
		character.changeState(new Idle(), true);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}

	public bool canParry(Actor damagingActor) {
		if (damagingActor is not Projectile) {
			return false;
		}
		if (player.isVile)return character.frameIndex < 5;
		if (player.isDragoon)return character.frameIndex < 5;
		
		return character.frameIndex == 0;
	}

		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (player.isX || player.isX) {
		character.changeSpriteFromName("unpo_parry_start", true);
		}

			if (player.isZain){
			character.changeSpriteFromName("parry_dash", true);
			character.playSound("distortion_d");
			character.playSound("GDash");
		}


		
		}
	
}





public class BlockBreak : CharState {
	public int hurtDir;
	public float hurtSpeed;

	public BlockBreak(int dir) : base("land") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}
		/*
		if (this.character.isAnimOver()) {
			this.character.changeToIdleOrFall();
		}
		*/
		if (hurtSpeed == 0) {
			character.changeToIdleOrFall();
		}
	}
}

public class Clang : CharState {
	public int hurtDir;
	public float hurtSpeed;

	public Clang(int dir) : base("clang") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}
		/*
		if (this.character.isAnimOver()) {
			this.character.changeToIdleOrFall();
		}
		*/
		if (hurtSpeed == 0) {
			character.changeToIdleOrFall();
		}
	}
}




public class HurtByEnemy : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public HurtByEnemy(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
		flinchTime = 0.5f;
		enterSound = "hurt";
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -100;
		character.invulnTime = 0.3f;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (player.character.canCharge() && player.input.isAHeld(player)) {
			player.character.increaseCharge();
		}

		if (stateTime >= flinchTime) {
			character.changeToIdleOrFall();
			character.invulnTime = 0.2f;
		}
	}
}




public class PushedOver : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public PushedOver(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 300;
		flinchTime = 0.5f;
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -300;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (player.character.canCharge() && player.input.isAHeld(player)) {
			player.character.increaseCharge();
		}

		if (stateTime >= flinchTime) {
			character.changeState(new KnockedDown(-character.xDir), true);
		}
	}
}



public class PushedOver2 : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public PushedOver2(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 300;
		flinchTime = 0.5f;
	//	superArmor = true;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.charState.superArmor || character.charState.invincible) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -300;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (player.character.canCharge() && player.input.isAHeld(player)) {
			player.character.increaseCharge();
		}

		if (stateTime >= flinchTime) {
			character.changeState(new Idle(), true);
		}
	}
}




public class LaunchedState : GenericGrabbedState {
	public Character grabbedChar;
	//private bool once;
	public bool launched;
	float launchTime;
	bool once;
	public LaunchedState(Character grabber) : base(grabber, 1, "") {
		customUpdate = true;
		superArmor = true;
	}


	public override void update() {
		base.update();

		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				character.changeToIdleOrFall();
				return;
			}

			for (int i = 1; i <= 4; i++) {
				CollideData collideData = Global.level.checkTerrainCollisionOnce(character, 0, -10 * i, autoVel: true);
				if (!character.grounded && collideData != null && collideData.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
					if (!once) {
						once = true;
						character.applyDamage(2, player, character, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
						character.playSound("crash", sendRpc: true);
						character.shakeCamera(sendRpc: true);
					}
				}
			}

		}

		if (!launched) {
			launched = true;
			character.unstickFromGround();
			character.vel.y = -600;
		}
	}
}





public class LaunchedStateWeak : GenericGrabbedState {
	public Character grabbedChar;
	//private bool once;
	public bool launched;
	float launchTime;
	bool once;
	public LaunchedStateWeak(Character grabber) : base(grabber, 1, "") {
		customUpdate = true;
		superArmor = true;
	}


	public override void update() {
		base.update();

		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				character.changeToIdleOrFall();
				return;
			}

			for (int i = 1; i <= 4; i++) {
				CollideData collideData = Global.level.checkTerrainCollisionOnce(character, 0, -10 * i, autoVel: true);
				if (!character.grounded && collideData != null && collideData.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
					if (!once) {
						once = true;
						character.applyDamage(2, player, character, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
						character.playSound("crash", sendRpc: true);
						character.shakeCamera(sendRpc: true);
					}
				}
			}

		}

		if (!launched) {
			launched = true;
			character.unstickFromGround();
			character.vel.y = -200;
		}
	}
}




public class LaunchedFowardState : CharState {


	public LaunchedFowardState() : base("hurt") {
		superArmor = false;
		immuneToWind = true;
	}

	public override void update() {
		base.update();
		if (character.downPressedTimes > 10 || character.upPressedTimes > 10 || character.leftPressedTimes > 10
		|| character.rightPressedTimes > 10) {
            character.changeToIdleOrFall();
        }
		character.angle += 10;
		character.move(new Point(character.xDir * 350, 0));
		if (stateTime > 2f) {
			character.changeToIdleOrFall();
		}


		CollideData? collideData = Global.level.checkTerrainCollisionOnce(character, character.xDir, 0);
		if (collideData != null && collideData.isSideWallHit() && character.ownedByLocalPlayer) {
		character.applyDamage(2, player, character, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
					character.changeState(
							new KnockedDown(
								-character.xDir
							), true
						);
			character.playSound("mugenhtsnd_hit3", sendRpc: true);
			character.shakeCamera(sendRpc: true);
			new Anim(character.pos, "hitwave_wall", -character.xDir, null, true);
			
		} 
		

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = true;
		character.vel.y = -character.getJumpPower() * 0.75f;
	
            character.xDir = -character.xDir;
        
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.angle = 0;
	}
}





public class WcutGenericDodgeF : CharState {


	public WcutGenericDodgeF() : base("dodge_f") {
		immuneToWind = true;
		enterSound = "dash";
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();

		character.move(new Point(character.xDir * 250, 0));



		if (stateTime > 0.3f || character.flag != null) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.sprite.name.Contains("dodge_f")) {
			character.changeSpriteFromName("dash", false);
		}
		character.useGravity = true;
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.slideVel = character.xDir * character.getDashSpeed() * 0.9f;
		specialId = SpecialStateIds.None;
	}
}




//this is where our Hypermode gets activated

public class OverDriveStart : CharState {

	bool fired;

	public OverDriveStart() : base("activate_od") {
		invincible = true; //grants tangible invincibility
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("ching", sendRpc: true);
			character.overDriveTimer = 12; // this will grant 12 seconds of hypermode
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);
	}

}



public class WcutGenericDodgeU : CharState {


	public WcutGenericDodgeU() : base("dodge_up") {
		immuneToWind = true;
		enterSound = "dash";
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();

	



		if (stateTime > 0.4f) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -character.getJumpPower();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		specialId = SpecialStateIds.None;
	}
}



public class WcutGenericDodgeB : CharState {


	public WcutGenericDodgeB() : base("dash_end") {
		immuneToWind = true;
		enterSound = "dash";
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();

		character.move(new Point(character.xDir * -250, 0));

		if (stateTime > 0.2f) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = true;

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.slideVel = character.xDir * -character.getDashSpeed() * 0.9f;
		specialId = SpecialStateIds.None;
	}
}




public class DropDown : CharState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public DropDown(int dir) : base("knocked_down") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
		flinchTime = 0.5f;
	}

	public override bool canEnter(Character character) {
		if (character.isStatusImmune()) return false;
		if (character.isFlinchImmune()) return false;
		if (character.isInvulnerable()) return false;
		if (character.vaccineTime > 0) return false;
		return base.canEnter(character);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = 300;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}

		if (character.canCharge() && player.input.isAHeld(player)) {
			character.increaseCharge();
		}

		if (character is Axl axl) {
			axl.stealthRevealTime = Axl.maxStealthRevealTime;
		}

		if (stateTime >= flinchTime) {
			character.changeToIdleOrFall();
		}
	}
}




public class ForceGrabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;
	public bool Teched;
	public float techTimer;
	public ForceGrabbed(Character? grabber) : base(grabber, maxGrabTime, "") {
	}


	public override void update() {
		techTimer += Global.spf;
		if (!Teched && techTimer > 0.2f && techTimer < 0.4f && player.input.isPressed(Control.Jump, player)) {
            character.changeToIdleOrFall();
			Teched = true;
			character.playSound("htsnd_block", true);
			character.addHealth(3);
			if (grabber is Character grabberChar) {
                grabberChar?.changeState(new ZeroClang(grabberChar.xDir), true);
            }
					Global.level.gameMode.setHUDErrorMessage(
					player, "Tech Bonus!!!!",
					playSound: false, resetCooldown: true
				);
        }

		if (player.input.isPressed(Control.Jump, player)) {
            Teched = true;
			if (techTimer < 0.2f && techTimer >0.4f ){
			character.playSound("error", true);
				Global.level.gameMode.setHUDErrorMessage(
					player, "Tech Fail.",
					playSound: false, resetCooldown: true
				);
			}
        }

		if (techTimer > 0.2f && techTimer < 0.4f ){
			Global.level.gameMode.setHUDErrorMessage(
					player, "PRESS JUMP TO TECH.",
					playSound: false, resetCooldown: true
				);
			}

		trySnapToGrabPoint(true);
		if (!grabber.sprite.name.Contains("gbd_b")) {
			if (grabber.sprite.name.Contains("idle") ||
			grabber.sprite.name.Contains("crouch") ||
			grabber.sprite.name.Contains("run") ||
			grabber.sprite.name.Contains("fall") ||
			grabber.sprite.name.Contains("jump") ||
			grabber.sprite.name.Contains("hurt") ||
			grabber.sprite.name.Contains("grabbed")

			) {
				character.changeToIdleOrFall();
			}
		} else {
			if (
		grabber.sprite.name.Contains("hurt") ||
		grabber.sprite.name.Contains("grabbed")
		
		) {
			character.changeToIdleOrFall();
		}
		}
	}
}

