using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;




public class DynamoWhippAttack : CharState {
	bool fired;

	public DynamoWhippAttack() : base("whipattack") {
		landSprite = "whipattack";
		airSprite = "whipattack_air";
		useDashJumpSpeed = true;
		airMove = true;
	}

	public override void update() {
		base.update();
		if (!character.grounded){
			character.changeSpriteFromName(airSprite, false);
		}

		if (character.grounded) {
			character.isDashing = false;
			character.changeSpriteFromName(landSprite, false);
		}
		if (character.frameIndex >= 1 && !fired) {
			character.playSound("DynamoWhip", sendRpc: true);
			fired = true;
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else {
			if ((character.grounded || character.canAirJump() && character.flag == null) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
			character.changeSpriteFromName(sprite, false);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
	}
}



public class DynamoCross : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	DynamoCrossProj proj;

	public DynamoCross(string transitionSprite = "")
		: base("throw_cross", "", "", transitionSprite)
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

		
		if (proj == null && character.frameIndex >= 1 && character.ownedByLocalPlayer){
		character.playSound("throwCross", forcePlay: false, sendRpc: true);
		proj = new DynamoCrossProj(new ShotgunIce(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	
		
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class DynamoCrossProj : Projectile {
	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.55f;
	public float turnSpeed = 300;
	public float maxAngleDist = 200;
	public float soundCooldown;

	public DynamoCrossProj(ShotgunIce weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "dynamo_cross_proj", 8, 0.3f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DynamoCross;
		destroyOnHit = false;
		destroyOnDMG = true;
		maxTime = 1.5f;
		this.vel.y = 0;
		angle2 = 0;
		hitSound = "htsnd_slash1";
		if (xDir == -1) angle2 = -180;

		angle = angle;

		xScale = 0.5f;
		yScale = 0.5f;


		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
	
		if (other.gameObject is Pickup && pickup == null) {
			pickup = other.gameObject as Pickup;
			if (!pickup.ownedByLocalPlayer) {
				pickup.takeOwnership();
				RPC.clearOwnership.sendRpc(pickup.netId);
			}
		}

		var character = other.gameObject as Character;
		if (time > returnTime && character != null && character.player == damager.owner) {
			if (pickup != null) {
				pickup.changePos(character.getCenterPos());
			}
			destroySelf();
			character.player.vileAmmo = Helpers.clampMax(character.player.vileAmmo + 8, character.player.vileMaxAmmo);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		if (pickup != null) {
			pickup.useGravity = true;
			pickup.collider.isTrigger = false;
		}
	}

	

	public override void update() {
		base.update();

			if (time > 0.05f) {
			var angInc = (-xDir * turnDir) * Global.spf * 200;
			angle += angInc;
			}
		if (!owner.isDynamo)destroySelf();
		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
			playSound("throwAxe", sendRpc: true);
		}

		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
			//	vel.y = Helpers.sind(angle2) * maxSpeed;
			} 
			 if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
			//	vel.y = Helpers.sind(angle2) * maxSpeed;
			} else {
				destroySelf();
			}
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}

public class DynamoAxe : CharState {
	public float pushBackSpeed;
	DynamoAxeProj? proj;

	public DynamoAxe() : base("throw_cross") {
		airMove = true;
	}

	public override void update() {
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

		if (proj == null && character.frameIndex >= 1 && character.ownedByLocalPlayer) {
			character.playSound("throwAxe", forcePlay: false, sendRpc: true);
			proj = 	new DynamoAxeProj(
				character.pos.addxy(16 * character.xDir, -36), character.xDir,
				character, player.getNextActorNetId(), sendRpc: true
			);
		}

		base.update();
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

public class DynamoAxeProj : Projectile {
	public float angleDist;

	public DynamoAxeProj(
		Point pos, int xDir, Actor owner, ushort? netId,
		bool sendRpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "dynamo_axe_proj", netId, altPlayer
	) {
		weapon = SonicSlicer.netWeapon;
		damager.damage = 3;
		damager.flinch = Global.defFlinch;
		damager.hitCooldown = 8;
		vel = new Point(200 * xDir, -350);

		fadeSprite = "explosion";
		maxTime = 1f;
		fadeOnAutoDestroy = true;
		hitSound = "htsnd_slash1";
		projId = (int)ProjIds.DynamoAxeProj;
		destroyOnHit = false;
		useGravity = false;

		if (sendRpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new DynamoAxeProj(
			args.pos, args.xDir, args.owner, args.netId, altPlayer: args.player
		);
	}

	public override void update() {
		base.update();

		angleDist += 16 * speedMul;
		byteAngle = MathF.Round(xDir * angleDist / 32) * 32;

		vel.x = Helpers.lerp(vel.x, 0, 1 * Global.spf);
		if (vel.y < Physics.MaxFallSpeed) {
			vel.y += Physics.Gravity;
		}
	}
}

public class DynamoBladeSlashAir : CharState {
	bool fired;

	public DynamoBladeSlashAir() : base("blade_attack_air") {
		landSprite = "blade_attack_air";
		airSprite = "blade_attack_air";
		useDashJumpSpeed = true;
		airMove = true;
		exitOnLanding = true;
	}

	public override void update() {
		base.update();

	
		if (character.frameIndex >= 1 && !fired) {
			character.playSound("dynamosaber", sendRpc: true);
			fired = true;
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else {
			if ((character.grounded || character.canAirJump() && character.flag == null) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
			character.changeSpriteFromName(sprite, false);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		
	}
}



public class DynamoBladeSlash : CharState {
	bool fired;

	public DynamoBladeSlash() : base("blade_attack") {
		landSprite = "blade_attack";
		airSprite = "blade_attack";
		useDashJumpSpeed = true;
		airMove = true;
	}

	public override void update() {
		base.update();

	
		if (character.frameIndex >= 1 && !fired) {
			character.playSound("dynamosaber", sendRpc: true);
			fired = true;
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else {
			if ((character.grounded || character.canAirJump() && character.flag == null) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
			character.changeSpriteFromName(sprite, false);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		
	}

	public override void onExit(CharState oldState) {
		base.onExit(oldState);
		
	}
}



public class DynamoUpperCut : CharState {
	bool jumpedYet;

	public DynamoUpperCut() : base("uppercut_slash") {
		superArmor = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		if (character.sprite.frameIndex >= 2 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			attackCtrl = true;
			if (character is Dynamo dynamo) {
				dynamo.uppercutCount++;
			}
			character.vel.y = -character.getJumpPower() * 1.5f;
			character.playSound("dynamosaber", sendRpc: true);
		}
		if (jumpedYet) {
			character.move(new Point(character.xDir * 50, 0));
		}
		if (character.isAnimOver()) {
			character.changeToLandingOrFall();
		}
	}
}

public class DynamoBoomerang : CharState {
	bool shot = false;
	DynamoBoomerangProj? proj;
	float specialPressTime;
	public float pushBackSpeed;

	public DynamoBoomerang() : base("throw_boomerang") {
		superArmor = true;
		normalLockAlt = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex >= 4) {
			shoot();
			normalCtrl = true;
		}

		if (character.sprite.frameIndex >= 6) {
			attackCtrl = true;
		}

		if (proj != null) {
		
				if (proj.returned || proj.destroyed) {
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
		character.playSound("dynamosaber", sendRpc: true);
		character.frameIndex = 6;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new DynamoBoomerangProj(new IrisCrystal(), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}


}




public class DynamoBoomerangProj : Projectile {
	public bool reversed;
	public bool returned;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor? target;
	int type = 0;

	public DynamoBoomerangProj(
		IrisCrystal weapon, Point pos, int xDir, Player player,
		ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, getSpeed(weapon.type), 3,
		player, "dynamo_boomerang_proj", Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.DynamoBoomerangProj;
		destroyOnHit = false;
		hitSound = "htsnd_slash1";
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.4f;
		maxReverseTime = 0.6f;
		type = 0;
		
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public bool ownerExists => (owner.character?.destroyed == false);

	public override void update() {
		base.update();
		if (ownedByLocalPlayer && !ownerExists) {
			destroySelf("explosion", "explosion");
			return;
		}
	
		if (ownedByLocalPlayer && !reversed && reflectCount == 0 &&
			type == (int)RocketPunchType.InfinityGig
		) {
			if (target == null && owner.character != null) {
				var targets = Global.level.getTargets(owner.character.pos, damager.owner.alliance, true);
				foreach (var t in targets) {
					if (isFacing(t) && MathF.Abs(t.pos.y - owner.character.pos.y) < 120) {
						target = t;
						break;
					}
				}
			} else if (target != null && target.destroyed) {
				vel.x = getSpeed(type) * xDir;
			} else if (target != null) {
				vel = new Point(0, 0);
				Point targetPos = target.getCenterPos();
				move(pos.directionToNorm(targetPos).times(speed));
				if (pos.distanceTo(targetPos) < 5) {
					reversed = true;
				}
				forceNetUpdateNextFrame = true;
			}
		}
		if (!reversed) {
			if (owner.input.isHeld(Control.Up, owner)) {
				incPos(new Point(0, -300 * Global.spf));
			} else if (owner.input.isHeld(Control.Down, owner)) {
				incPos(new Point(0, 300 * Global.spf));
			}
		}
		if (!reversed && time > maxReverseTime) {
			reversed = true;
			vel.x = getSpeed(type) * -xDir;
		}
		if (reversed && owner.character != null) {
			vel = new Point(0, 0);
			if (pos.x > owner.character.pos.x) {
				xDir = -1;
			} else {
				xDir = 1;
			}
			Point returnPos = owner.character.getCenterPos();

			move(pos.directionToNorm(returnPos).times(speed));
			if (pos.distanceTo(returnPos) < 10) {
				returned = true;
				destroySelf();
			}
		}
	}

	/*
	public override void onHitWall(CollideData other) {
		if (!ownedByLocalPlayer) return;
		reversed = true;
	}
	*/

	public static float getSpeed(int type) {
		return 550;
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (locallyControlled) {
			reversed = true;
		}
		if (isRunByLocalPlayer()) {
			reversed = true;
			RPC.actorToggle.sendRpc(netId, RPCActorToggleType.ReverseRocketPunch);
		}
	}
}

public class DynamoBackFlip : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public DynamoBackFlip(string transitionSprite = "")
		: base("backflip", "", "", transitionSprite)
	{
		normalCtrl = true;
		attackCtrl = true;
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 20.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 200;
		}
		character.vel.y = -character.getJumpPower();
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}




public class DynamoSlide : CharState {
	public bool soundPlayed;

	public DynamoSlide() : base("slide") {
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.OverDrive) {
			character.move(new Point(character.xDir * 350, 0));
		} else {
			character.move(new Point(character.xDir * 300, 0));
		}
		if (!soundPlayed && character.frameIndex >= 1) {
			character.playSound("dynamoslide", sendRpc: true);
			soundPlayed = true;
		}

	 	if (stateTime > 0.4f) {
			character.changeToIdleOrFall();
			return;
		}
		if (stateTime > 0.2f && player.input.isPressed(Control.Jump,player)){
			character.changeState(new DynamoSlideKick());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = true;
		character.vel.y = 0;
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}

public class DynamoSlideKick : CharState {
	public DynamoSlideKick() : base("slide_jump") {
		immuneToWind = true;
		normalCtrl = true;
		normalLockAlt = true;
		landSprite = "slide";
		airSprite = "slide_jump";
	}

	public override void update() {
		base.update();
	
		if (character.OverDrive) {
			character.move(new Point(character.xDir * 350, 0));
		} else {
			character.move(new Point(character.xDir * 300, 0));
		}
	  	if (stateFrames >= 40) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.vel.y = -character.getJumpPower();
	}
}




public class DynamoBladeDash : CharState {
	Anim? proj;

	public DynamoBladeDash() : base("bladedash", "", "", "") {
		superArmor = true;
		enterSound = "dynamoslide";
		immuneToWind = true;
	}

	public override void update() {
		base.update();


		character.move(new Point(character.xDir * 450, 0));

		CollideData? collideData = Global.level.checkTerrainCollisionOnce(character, character.xDir, 0);
		if (collideData != null && collideData.isSideWallHit() && character.ownedByLocalPlayer) {
			character.changeToIdleOrFall();
			return;
		} else if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel.y = 0;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class DynamoGPChargeState : CharState {
	
	public float pillarCount;

	public DynamoGPChargeState() : base("groundpunch_charge", "") {
		enterSound = "dynamocharge";
	}

	public override void update() {
		base.update();
		if (player == null) return;

	character.turnToInput(player.input, player);

		pillarCount += Global.spf;
		 if (!player.input.isAHeld(player) && stateTime > 0.2f) {
			if (!character.OverDrive) {
				if (stateTime < 0.5f) { character.changeState(new DynamoGPState()); }
				if (stateTime > 0.5f && stateTime < 1f
				) { character.changeState(new DynamoGPStateLV1()); }
				if (stateTime > 1f && stateTime < 2f) { character.changeState(new DynamoGPStateLV2()); }
				if (stateTime > 2f) { character.changeState(new DynamoGPStateLV3()); }
			} else {
				if (stateTime < 0.5f) { character.changeState(new DynamoGPStateLV1()); }
				if (stateTime > 0.5f && stateTime < 1f
				) { character.changeState(new DynamoGPStateLV2()); }
				if (stateTime > 1f && stateTime < 2f) { character.changeState(new DynamoGPStateLV3()); }
				if (stateTime > 2f) { character.changeState(new DynamoGPStateLV3()); }
		
			}
				
	
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}




public class DynamoGPState : CharState {
	
	bool fired = false;


	public DynamoGPState() : base("groundpunch", "") {
	}

	public override void update() {
		base.update();
		if (player == null) return;

		character.turnToInput(player.input, player);
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);
		}


		if (character.isAnimOver()){
		character.changeToIdleOrFall();
		}
	
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}





public class DynamoGPStateLV1 : CharState {
	
	bool fired = false;
	bool groundedOnce;



	public DynamoGPStateLV1() : base("groundpunch", "") {
	}

	public override void update() {
		base.update();
		if (player == null) return;

		character.turnToInput(player.input, player);
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);	
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(20 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			character.playSound("dynamopillar", forcePlay: false, sendRpc: true);	
		}
		if (character.isAnimOver()){
		character.changeToIdleOrFall();
		}
	
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class DynamoGPStateLV2 : CharState {
	
	bool fired = false;


	public DynamoGPStateLV2() : base("groundpunch", "") {
	}

	public override void update() {
		base.update();
		if (player == null) return;

		character.turnToInput(player.input, player);
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);	
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(20 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(40 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(60 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
		}


		if (character.isAnimOver()){
		character.changeToIdleOrFall();
		}
	
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class DynamoGPStateLV3 : CharState {
	
	bool fired = false;

	public DynamoGPStateLV3() : base("groundpunch", "") {
	}

	public override void update() {
		base.update();
		if (player == null) return;

		character.turnToInput(player.input, player);
		if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);		
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(20 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(40 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(60 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(80 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(100 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(120 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			new DynamoBeam(new ElectricSpark(), character.pos.addxy(140 * character.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
		}


		if (character.isAnimOver()){
		character.changeToIdleOrFall();
		}
	
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}

public class DynamoAirShotState : CharState {
	public bool hasShot;

	public DynamoAirShotState() : base("buster_air") {
		useDashJumpSpeed = true;
		airMove = true;
	}

	public override void update() {
		base.update();
			if (character.frameIndex >= 1 && !hasShot) {
				character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
				new DynamoAirBuster(
					new XBuster(), character.pos, character.xDir, player,
					0, character.player.getNextActorNetId(), rpc: true
				);
				hasShot = true;
				character.vel.y = -200;
			}
			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (character.vel.y < 0) {
			character.vel.y = 0;
		}
	}
}





public class DynamoAirBuster : Projectile {
	int type;
	bool split;
	public DynamoAirBuster(
		Weapon weapon, Point pos, int xDir, Player player,
		int type, ushort netProjId, Point? vel = null, bool rpc = false
	) : base(
		weapon, pos, xDir, 75, 3, player, "dynamo_air_buster_proj",
		Global.miniFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.DynamoAirBuster;
		maxTime = 1f;
		if (type == 1){
			maxTime = 0.6f;
			changeSprite("dynamo_air_buster_ground", true);
			projId = (int)ProjIds.DynamoAirBuster2;
		}
		destroyOnHit = false;
		this.type = type;
		canBeLocal = false;

		if (vel != null) {
			this.vel = vel.Value;
		}
		if (type == 0) {
			this.vel.y = 50;
			useGravity = true;
			gravityModifier = 0.5f;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}

		if (type == 1) {
			byteAngle = this.vel.byteAngle;
		}
	}

	public override void update() {
		base.update();
	}

	public override void onHitWall(CollideData other) {
		if (!ownedByLocalPlayer) return;
		if (!other.gameObject.collider.isClimbable) return;
		if (split) return;
		if (type == 0) {
			var normal = other?.hitData?.normal;
			if (normal != null) {
				normal = normal.Value.leftNormal();
			} else {
				normal = new Point(1, 0);
			}
			Point normal2 = (Point)normal;
			normal2.multiply(250);
			destroySelf(fadeSprite);
			split = true;
			playSound("ballPOR", sendRpc: true);
			new DynamoAirBuster(weapon, pos.clone(), 1, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(), normal2, rpc: true);
			new DynamoAirBuster(weapon, pos.clone(), 1, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(), normal2.times(-1), rpc: true);
			destroySelf();
		}
	}
}

public class DynamoBeam : Projectile {
	Player player;
	public DynamoBeam(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, 1, 0, 2, player, "dynamo_beam_proj", Global.superFlinch, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DynamoBeam;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		maxTime = 1f;
		hitSound = "kofhtsnd_lightning1";
		this.player = player;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
	}

	public override bool shouldDealDamage(IDamagable damagable) {
	
		return true;
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -600));
		}
	}
}




public class DarkHoldDProj : Projectile {
	
	public ShaderWrapper? screenShader;
	float timeInFrames;

	public const int radius = 220;
	//public float drawRadius = 120;
	//public float drawAlpha = 64;

	public float soundTime;

	public Actor? rootProj;

	public DarkHoldDProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "empty", 0, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.1f;
		vel = new Point();
		projId = (int)ProjIds.DarkHoldD;
		setIndestructableProperties();
		Global.level.darkHoldDProjs.Add(this);
		if (Options.main.enablePostProcessing) {
			screenShader = owner.nightmareZeroShader;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	
	public override void update() {
		base.update();
		var screenCoords = new Point(pos.x - Global.level.camX, pos.y - Global.level.camY);
		var normalizedCoords = new Point(screenCoords.x / Global.viewScreenW, 1 - screenCoords.y / Global.viewScreenH);

		//if (isSnails) {
		Helpers.decrementFrames(ref soundTime);
		if (soundTime == 0) {
			playSound("csnailSlowLoop");
			soundTime = 65;
		}
		//} Why only snail gets the cool sound???

		if (screenShader != null) {
			screenShader.SetUniform("x", normalizedCoords.x);
			screenShader.SetUniform("y", normalizedCoords.y);
			screenShader.SetUniform("t", Global.time);
		//	screenShader.SetUniform("r", 0.5f * (drawRadius / (120f / Global.viewSize)));
		}

		if (screenShader == null) {
		//	drawRadius = 120 + 0.5f * MathF.Sin(Global.time * 10);
		//	drawAlpha = 64f + 32f * MathF.Sin(Global.time * 10);
		}
		time += Global.spf;
		if (time > maxTime) {
			destroySelf(disableRpc: true);
		}
		if (ownedByLocalPlayer && rootProj != null) {
			changePos(rootProj.pos);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		Global.level.darkHoldDProjs.Remove(this);
	}

	public override void render(float x, float y) {
		base.render(x, y);

		Color fillColor = new Color(99, 82, 247, 32);
		Color outlineColor = new Color(66, 49, 247, 32);
		Color lineColor = new Color(208, 200, 240, 128);
		if (owner.alliance != Global.level.mainPlayer.alliance) {
			Level level = Global.level;
			if (level != null && level.gameMode?.isTeamMode == true) {
				fillColor = new Color(247, 82, 99, 32);
				outlineColor = new Color(247, 49, 66, 32);
			}
		}
		
		
	}
}





public class DynamoDaggerLV1 : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	DynamoKnifeProj proj;

	public DynamoDaggerLV1(string transitionSprite = "") : base("throw_knife", "", "", transitionSprite) {
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

		
		if (proj == null && character.frameIndex >= 1 && character.ownedByLocalPlayer){
		character.playSound("throwAxe", forcePlay: false, sendRpc: true);
		proj = new DynamoKnifeProj(new  ShotgunIce(), character.getShootPos(), character.xDir, player, 0, player.getNextActorNetId(), rpc: true);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}
}





public class DynamoDaggerLV2 : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	DynamoKnifeProj proj;

	public DynamoDaggerLV2(string transitionSprite = "")
		: base("throw_multiknife", "", "", transitionSprite)
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

		
		if (proj == null && character.frameIndex >= 1 && character.ownedByLocalPlayer){
		character.playSound("throwCross", forcePlay: false, sendRpc: true);
		proj = new DynamoKnifeProj(new ShotgunIce(), character.getShootPos(), character.xDir, player, 2, player.getNextActorNetId(), rpc: true);
		new DynamoKnifeProj(
				new ShotgunIce(),  character.getShootPos(), 
				character.xDir, player, 1,player.getNextActorNetId(),
				((1 * character.xDir), -2), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShotgunIce(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), -1), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShotgunIce(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), 0), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShotgunIce(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), 1), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShotgunIce(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), 2), character, rpc: true
			);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}


		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}
}

public class DynamoKnifeProj : Projectile {
	public int type = 0;
	public float sparkleTime = 0;
	public Character? hitChar;
	public float maxSpeed = 400;

	public DynamoKnifeProj(
		Weapon weapon, Point pos, int xDir, Player player, int type, ushort netProjId,
		(int x, int y)? velOverride = null, Character? hitChar = null, bool rpc = false
	) : base(
		weapon, pos, xDir, 400, 1, player, "dynamo_dagger_proj", 2, 0.001f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.DynamoIceDagger;
		maxTime = 0.4f;
		ShouldClang = true;
		this.hitChar = hitChar;
		hitSound = "htsnd_slash1";
		if (type == 1) {
			damager.damage = 3;
			changeSprite("dynamo_dagger_proj", true);
			damager.flinch = Global.defFlinch;
			maxTime = 0.8f;
			ShouldClang = false;
		}

		fadeSprite = "buster1_fade";
		this.type = type;
		if (velOverride != null) {
			vel = new Point(maxSpeed * velOverride.Value.x, maxSpeed * (velOverride.Value.y * 0.5f));
		}
		reflectable = true;
		//this.fadeSound = "explosion";
		if (rpc) {
			byte[] extraArgs;
			if (velOverride != null) {
				extraArgs = new byte[] {
					(byte)type,
					(byte)(velOverride.Value.x + 128),
					(byte)(velOverride.Value.y + 128)
				};
			} else {
				extraArgs = new byte[] { (byte)type, (byte)(128 + xDir), 128 };
			}
			rpcCreate(pos, player, netProjId, xDir, extraArgs);
		}
	}

	public override void update() {
		base.update();
		if (type == 2)destroySelf();
		sparkleTime += Global.spf;
		if (sparkleTime > 0.05) {
			sparkleTime = 0;
			new Anim(pos, "shotgun_ice_sparkles", 1, null, true);
		}
	}

	public void onHit() {
		if (!ownedByLocalPlayer && type == 0) {
			destroySelf(disableRpc: true);
			return;
		}



		if (type == 2) {
			destroySelf(disableRpc: true);
			Character? chr = null;
			new DynamoKnifeProj(
				weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(),
				((-1 * xDir), -2), chr, rpc: true
			);
			new DynamoKnifeProj(
				weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(),
				((-1 * xDir), -1), chr, rpc: true
			);
			new DynamoKnifeProj(
				weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(),
				((-1 * xDir), 0), chr, rpc: true
			);
			new DynamoKnifeProj(
				weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(),
				((-1 * xDir), 1), chr, rpc: true
			);
			new DynamoKnifeProj(
				weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(),
				((-1 * xDir), 2), chr, rpc: true
			);
		}
	}

	public override void onHitWall(CollideData other) {
		if (!other.gameObject.collider.isClimbable) return;
		onHit();
	}

	public override void onHitDamagable(IDamagable damagable) {
		if (ownedByLocalPlayer) onHit();
		playSound("shotgunicehitX1", forcePlay: false, sendRpc: true);
		base.onHitDamagable(damagable);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new DynamoKnifeProj(
			ShotgunIce.netWeapon, arg.pos, arg.xDir, arg.player,
			arg.extraData[0], arg.netId, (arg.extraData[1] - 128, arg.extraData[2] - 128)
		);
	}
}







public class DynamoHydroStorm : CharState {

	public DynamoHydroStorm() : base("hydrostorm") {
		
	}



	public override void update() {
		base.update();
		if (!once && character.frameIndex == 2) {
					once = true;
					float topY = Global.level.getTopScreenY(character.pos.y);
				
					new HydroStormProj(
						new Point(character.pos.x, topY), character.xDir, character,
						player, player.getNextActorNetId(), rpc: true
					);
					
					character.playSound("chillpBlizzard", sendRpc: true);
				}
				if (character.sprite.isAnimOver()) {
					character.changeState(new Fall());
		}
	}
}






public class HydroStormProj : Projectile {

	public HydroStormProj(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "dynamo_hydrostorm_proj", netId, player
	) {
		weapon = ChillPBlizzardWeapon.netWeapon;
		projId = (int)ProjIds.HydroStormProj;
		damager.damage = 0.01f;
		shouldShieldBlock = false;
		destroyOnHit = false;
		shouldVortexSuck = false;

		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new HydroStormProj(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void update() {
		base.update();
		if (sprite.loopCount > 30) {
			destroySelf();
		}
	}


	
}