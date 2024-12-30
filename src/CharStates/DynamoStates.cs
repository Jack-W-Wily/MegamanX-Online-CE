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
		normalCtrl = true;
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
		proj = new DynamoCrossProj(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
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
			character.stopMovingWeak();
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

	public DynamoCrossProj(ShieldBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "dynamo_cross_proj", 8, 0.3f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DynamoCross;
		destroyOnHit = false;
		maxTime = 1.5f;
		this.vel.y = 0;
		angle2 = 0;
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
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
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
		//normalCtrl = true;
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
		//normalCtrl = true;
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
	float timeInWall;
	
	Anim anim;
	float projTime;
	

	public DynamoUpperCut() : base("uppercut_slash", "", "") {
		superArmor = true;
		//airMove = true;
		normalCtrl = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		

		if (character.sprite.frameIndex >= 2 && !jumpedYet) {
			jumpedYet = true;
			character.dashedInAir++;
			attackCtrl = true;
			character.vel.y = -character.getJumpPower() * 1.5f;
		}
		
		if (character.sprite.frameIndex >= 2) {
			character.move(new Point(character.xDir * 50, -120f));
			
		} 

		if (character.isAnimOver()) {
			character.changeState(new Fall());
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	
	}

	public override void onExit(CharState newState) {
		if (anim != null) {
			anim.destroySelf();
			anim = null;
		}
		base.onExit(newState);
}
}



public class DynamoBoomerang : CharState {
	bool shot = false;
	DynamoBoomerangProj? proj;
	float specialPressTime;

		public float pushBackSpeed;


	public DynamoBoomerang() : base("throw_boomerang") {
		normalCtrl = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 4) {
			shoot();
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
			character.stopMovingWeak();
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
		proj = new DynamoBoomerangProj(new ShieldBoomerang(), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
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
		ShieldBoomerang weapon, Point pos, int xDir, Player player,
		ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, getSpeed(weapon.type), 3,
		player, "dynamo_boomerang_proj", Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.DynamoBoomerangProj;
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.2f;
		maxReverseTime = 0.4f;
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
			character.stopMovingWeak();
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
	Anim? proj;

	public DynamoSlide() : base("slide", "", "", "") {
		enterSound = "dynamoslide";
		immuneToWind = true;
	}

	public override void update() {
		base.update();


		character.move(new Point(character.xDir * 350, 0));

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
	Anim? proj;

	public DynamoSlideKick() : base("slide_jump", "", "", "") {
		immuneToWind = true;
		normalCtrl = true;
	}

	public override void update() {
		base.update();
	
		character.move(new Point(character.xDir * 350, 0));

	  if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = true;
	
		character.vel.y = -character.getJumpPower();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		
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
		 if ((!player.input.isHeld(Control.Special1, player) && stateTime > 0.2f)) {
			
			if (stateTime < 0.5f )
			{character.changeState(new DynamoGPState());}
			if (stateTime > 0.5f && stateTime < 1f 
			){character.changeState(new DynamoGPStateLV1());}
			if (stateTime > 1f && stateTime < 2f )
			{character.changeState(new DynamoGPStateLV2());}	
			if (stateTime > 2f )
			{character.changeState(new DynamoGPStateLV3());}
		
				
	
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
	int bombNum;
	bool isNapalm;


	public DynamoAirShotState() : base("buster_air", "", "") {
	
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		
			if (stateTime > 0f && bombNum == 0) {
				bombNum++;
				character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
				new DynamoAirBuster(new XBuster(), character.pos, character.xDir, player, 0, character.player.getNextActorNetId(), rpc: true);
			}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class DynamoAirBuster : Projectile {
	int type;
	bool split;
	public DynamoAirBuster(
		Weapon weapon, Point pos, int xDir, Player player, int type, ushort netProjId, Point? vel = null, bool rpc = false
	) : base(
		weapon, pos, xDir, 75, 3, player, "dynamo_air_buster_proj", Global.miniFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
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

		if (vel != null) this.vel = (Point)vel;
		if (type == 0) {
			this.vel.y = 50;
			useGravity = true;
			gravityModifier = 0.5f;
		} else {
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
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
			new DynamoAirBuster(weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(), normal2, rpc: true);
			new DynamoAirBuster(weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(), normal2.times(-1), rpc: true);
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
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -600));
		}
	}
}




public class DarkHoldDProj : Projectile {
	public float radius = 10;
	public float attackRadius => (radius + 15);
	public ShaderWrapper? screenShader;
	float timeInFrames;

	public DarkHoldDProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "empty", 0, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 3f;
		vel = new Point();
		projId = (int)ProjIds.DarkHoldD;
		setIndestructableProperties();
		Global.level.darkHoldDProjs.Add(this);
		if (Options.main.enablePostProcessing) {
			screenShader = player.darkHoldDScreenShader;
			updateShader();
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		updateShader();
		timeInFrames++;

		if (timeInFrames < 150) {
			foreach (var gameObject in Global.level.getGameObjectArray()) {
				if (gameObject != this && gameObject is Actor actor && actor.locallyControlled && inRange(actor)) {
					// For characters.
					if (actor is Character chara && chara.darkHoldInvulnTime <= 0) {
						if (timeInFrames > 30) {
							continue;
						}
						if (chara.canBeDamaged(damager.owner.alliance, damager.owner.id, null)) {
							chara.addDarkHoldTime(150 - timeInFrames, damager.owner);
							chara.darkHoldInvulnTime = (150 - timeInFrames) * 60f;
						}
						continue;
					}
					// For maverick and rides
					if (actor is RideArmor or Maverick or Mechaniloid) {
						if (actor.timeStopTime <= 0) {
							continue;
						}
						IDamagable? damagable = actor as IDamagable;
						if (damagable?.canBeDamaged(damager.owner.alliance, damager.owner.id, null) == true) {
							continue;
						}
						actor.timeStopTime = 160 - timeInFrames;
					}
					// For projectiles
					if (actor is Projectile && actor.timeStopTime <= 0) {
						if (actor is BCrabSummonBubbleProj or BCrabSummonCrabProj &&
							(actor as IDamagable)?.canBeDamaged(damager.owner.alliance, damager.owner.id, null) != true
						) {
							continue;
						}
						actor.timeStopTime = 160 - timeInFrames;
					}
				}
			}
		}
		if (timeInFrames <= 30) {
			radius += (1f/60f) * 400;
		}
		if (timeInFrames >= 150 && radius > 0) {
			radius -= (1f/60f) * 800;
			if (radius <= 0) {
				radius = 0;
			}
		}
	}

	public bool inRange(Actor actor) {
		return (actor.getCenterPos().distanceTo(pos) <= attackRadius);
	}

	public void updateShader() {
		if (screenShader != null) {
			var screenCoords = new Point(
				pos.x - Global.level.camX,
				pos.y - Global.level.camY
			);
			var normalizedCoords = new Point(
				screenCoords.x / Global.viewScreenW,
				1 - screenCoords.y / Global.viewScreenH
			);
			float ratio = Global.screenW / (float)Global.screenH;
			float normalizedRadius = (radius / Global.screenH);

			screenShader.SetUniform("ratio", ratio);
			screenShader.SetUniform("x", normalizedCoords.x);
			screenShader.SetUniform("y", normalizedCoords.y);
			if (Global.viewSize == 2) {
				screenShader.SetUniform("r", normalizedRadius * 0.5f);
			} else {
				screenShader.SetUniform("r", normalizedRadius);
			}
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		if (screenShader == null) {
			var col = new Color(255, 251, 239, (byte)(164 - 164 * (time / maxTime)));
			var col2 = new Color(255, 219, 74, (byte)(224 - 224 * (time / maxTime)));
			DrawWrappers.DrawCircle(pos.x + x, pos.y + y, radius, true, col, 1, zIndex + 1, true);
			DrawWrappers.DrawCircle(pos.x + x, pos.y + y, radius, false, col2, 3, zIndex + 1, true, col2);
		}

	}

	public override void onDestroy() {
		base.onDestroy();
		Global.level.darkHoldDProjs.Remove(this);
	}
}







public class DynamoDaggerLV1 : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	DynamoKnifeProj proj;

	public DynamoDaggerLV1(string transitionSprite = "")
		: base("throw_knife", "", "", transitionSprite)
	{
	airMove = true;
	normalCtrl = true;
	
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
		proj = new DynamoKnifeProj(new  ShieldBoomerang(), character.getShootPos(), character.xDir, player, 0, player.getNextActorNetId(), rpc: true);
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
			character.stopMovingWeak();
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
		proj = new DynamoKnifeProj(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, 2, player.getNextActorNetId(), rpc: true);
		new DynamoKnifeProj(
				new ShieldBoomerang(),  character.getShootPos(), 
				character.xDir, player, 1,player.getNextActorNetId(),
				((1 * character.xDir), -2), character, rpc: true
			);
			new ShotgunIceProj(
				new ShieldBoomerang(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), -1), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShieldBoomerang(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), 0), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShieldBoomerang(),  character.getShootPos(), 
				character.xDir, player, 1, player.getNextActorNetId(),
				((1 * character.xDir), 1), character, rpc: true
			);
			new DynamoKnifeProj(
				new ShieldBoomerang(),  character.getShootPos(), 
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
			character.stopMovingWeak();
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
			new ShotgunIceProj(
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