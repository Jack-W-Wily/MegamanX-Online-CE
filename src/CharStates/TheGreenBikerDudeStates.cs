namespace MMXOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;


public class GBDKick : Weapon {
	public GBDKick() : base() {
		fireRate = 60;
		index = (int)WeaponIds.GBDKick;
		killFeedIndex = 92;
	}
}


public class EnfrenteMeuDisco : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	ShieldBoomerangProj proj;

	public EnfrenteMeuDisco(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
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

		
		if (proj == null && character.frameIndex >= 3 && character.ownedByLocalPlayer){
		character.playSound("spinningBlade", forcePlay: false, sendRpc: true);
		proj = new ShieldBoomerangProj(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
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


public class EnfrenteMeuDisco2 : CharState {

	private bool shot;
	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	ShieldBoomerangProj2 proj;

	public EnfrenteMeuDisco2(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
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


		if (proj == null && character.frameIndex >= 3 && character.ownedByLocalPlayer){
		character.playSound("spinningBlade", forcePlay: false, sendRpc: true);
		proj = new ShieldBoomerangProj2(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
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











public class ShieldBoomerang : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public ShieldBoomerang() : base() {
		index = (int)WeaponIds.ShieldBoomerang;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
			displayName = "Shield Boomerang";
			description = new string[] { "Shield Boomerang" };
			killFeedIndex = 126;
		
		}
}


public class ShieldBoomerangProj : Projectile {
	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.55f;
	public float turnSpeed = 300;
	public float maxAngleDist = 280;
	public float soundCooldown;

	public ShieldBoomerangProj(ShieldBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "meudisco_proj", 8, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ShieldBoomerang;
		destroyOnHit = false;
		
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

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
		if (!owner.isGBD)destroySelf();
		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
			playSound("cutter", sendRpc: true);
		}

		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} 
			 if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
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




public class ShieldBoomerangProj2 : Projectile {
	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.15f;
	public float turnSpeed = 300;
	public float maxAngleDist = 180;
	public float soundCooldown;

	public ShieldBoomerangProj2(ShieldBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "meudisco2_proj", 8, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ShieldBoomerang2;
		destroyOnHit = false;
		
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

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
			if (time > 1.5f) destroySelf();
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
		if (!owner.isGBD)destroySelf();
		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
			playSound("cutter", sendRpc: true);
		}

		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} 
			 if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
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




public class GBDSpear1 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpear1(string transitionSprite = "")
		: base("spear_1", "", "", transitionSprite)
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




public class GBDSpearUp : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpearUp(string transitionSprite = "")
		: base("spear_up", "", "", transitionSprite)
	{
	
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

public class GBDSpearRising : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpearRising(string transitionSprite = "")
		: base("spear_rising", "", "", transitionSprite)
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



public class GBDSpearSpin : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public GBDSpearSpin(string transitionSprite = "")
		: base("spear_spin", "", "", transitionSprite)
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

		if (player.input.isPressed(Control.Shoot, player)&& character.sprite.frameIndex > 5){
		character.sprite.frameIndex = 4;
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







public class ChainrodProj : Projectile {

	int upOrDown;
	int startDir;
	Character mmx;
	Player player;
	float dist;
	float distRetracted;
	float maxDist = 180;
	Point reverseVel;
	bool reversed; //Used for all cases (Hooking actors, pulling to walls or just returning to X)
	bool toWall;
	Point toWallVel;
	Actor? hookedActor;
	bool toActor;
	float hookWaitTime;

	public ChainrodProj(
		Weapon weapon, Point pos, int xDir, Player player,
		ushort? netId, int upOrDown = 0, bool rpc = false
	) : base (
		weapon, pos, 1, 600, 2,
		player, "chainrod_proj", 0, 0.5f,
		netId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.ChainrodProj;
		destroyOnHit = false;
		maxTime = 4;
		startDir = xDir;
		//xScale = 1;

		//Set character and player
		mmx = player.character;
		mmx.ChainrodProj = this;
		this.player = player;

		//Reduce range if carrying a flag.
		if (mmx.flag != null) maxDist /= 2;

		//Set angle and speed.
		this.upOrDown = upOrDown;
		byteAngle = upOrDown * 32;
		if (xDir < 0) byteAngle = -byteAngle + 128;
		//if (byteAngle < 0) byteAngle += 256;
		vel = Point.createFromByteAngle(byteAngle.Value).times(speed);

		canBeLocal = false;

		if (rpc) {
			rpcCreateByteAngle(pos, player, netId, byteAngle.Value);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new ChainrodProj(
			StrikeChain.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;

		//We destroy the chain if X faces to the opposite xDir
		if (mmx.xDir != startDir && mmx.charState is not WallSlide) {
			destroySelf();
			return;
		}

		// Hooked character? Wait for them to become hooked before pulling back. Wait a max of 200 ms
		var hookedChar = hookedActor as Character;
		if (hookedChar != null && !hookedChar.ownedByLocalPlayer && !hookedChar.isStrikeChainState) {
			hookWaitTime += Global.speedMul;
			if (hookWaitTime < 12) return;
		}
		
		//If pulled towards a wall
		if (toWall) {
			mmx.move(toWallVel);
			var collision = Global.level.checkTerrainCollisionOnce(mmx, (toWallVel.x * Global.spf), (toWallVel.y * Global.spf));
			if (collision?.gameObject is Wall) {
				destroySelf();
				float momentum = 0.25f * (distRetracted / maxDist);
				mmx.xSwingVel = toWallVel.x * (0.25f + momentum) * 0.5f;
				if (mmx.isDashing && mmx.player.hasBootsArmor(ArmorId.Giga) && mmx.flag == null) mmx.xSwingVel *= 1.1f;
				mmx.vel.y = toWallVel.y;
				//Yes, X2 Boots increase it.
			}	
		} 


		//Actor hooked
		//This only runs if hookedActor is a Pickup.
		//Characters uses StrikeChainHooked state.
		else if (toActor) {
			if (hookedActor != null && !(hookedActor is Character)) {
				if (!hookedActor.ownedByLocalPlayer) {
					hookedActor.takeOwnership();
					RPC.clearOwnership.sendRpc(hookedActor.netId);
				}
				hookedActor.useGravity = false;
				hookedActor.grounded = false;
				hookedActor.move(hookedActor.pos.directionTo(player.character.getCenterPos()).normalize().times(speed));
			}
			if (distRetracted >= dist + 10) {
				if (hookedActor != null && !(hookedActor is Character)) {
					hookedActor.changePos(player.character.getCenterPos());
					hookedActor.useGravity = true;
				}
				destroySelf();
			}
		}
		else {
			//If the chain reaches max range and didn't catch anything, it turns back.
			if (!reversed) {
				dist += Math.Abs(Global.speedMul * vel.magnitude) / 60;
				if (dist >= maxDist) reverse(vel);
			}
		}

		// We check if the chain came back to X or viceversa
		if (reversed) {
			distRetracted += Math.Abs(Global.speedMul * reverseVel.magnitude) / 60;
			if (distRetracted >= dist) {
				destroySelf();
				return;
			} 
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		//Syncs with current buster position.
		Point shootPos = mmx.getShootPos();
		if (byteAngle != null) {
			changePos(
				new Point(
					shootPos.x + (dist - distRetracted) * Helpers.cosb(byteAngle.Value), 
					shootPos.y + (dist - distRetracted) * Helpers.sinb(byteAngle.Value)
				)
			);
		}
		
	}

	public void reverse(Point newVel) {
		vel.x *= -1;
		vel.y *= -1;
		reversed = true;
		reverseVel = newVel;
	}

	public void hookActor(Actor? actor) {
		toActor = true;
		hookedActor = actor;
		reverse(vel);
	}

	public override void onDestroy() {
		base.onDestroy();
		if (hookedActor != null) hookedActor.useGravity = true;
		mmx.ChainrodProj = null;
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		if (reversed) return;

		var wall = other.gameObject as Wall;

		if (wall != null && wall.collider.isClimbable && !toWall) {
			toWall = true;
			toWallVel = vel;
			if (mmx.flag != null) toWallVel.multiply(0.5f);
			stopMoving();
			reverse(toWallVel);
			if (mmx.grounded) mmx.incPos(new Point(0, -4));
			mmx.changeState(new StrikeChainPullToWall(this, mmx.charState.shootSprite, toWallVel.y < 0), true);
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer || hookedActor != null || reversed) return;
	
		var actor = other.gameObject as Actor;

		if (actor != null) {
			var chr = actor as Character;
			var pickup = actor as Pickup;
			if (chr == null && pickup == null) {
				//Lo quitamo' o miedo- I mean
				//It doesn't hook neither Mavericks nor Ride Armors.
				if (actor is Maverick || actor is RideArmor) {
					hookActor(null);
				}
				return;
			}
			//Character specific code.
		//	if (chr != null) {
		//		if (!chr.canBeDamaged(player.alliance, player.id, projId)) return;
		//		hookActor(actor);

				//if (Global.serverClient != null) {
				//	RPC.commandGrabPlayer.sendRpc(netId, chr.netId, CommandGrabScenario.StrikeChain, false);
				//}
			//	chr.hook(this);
		//	}
			//If chr is null, then it hooks a pickup
			else if (pickup != null) hookActor(actor);
		}
	}

	public override void render(float x, float y) {
		base.render(x,y);
		if (byteAngle == null) return;

		float length = dist - distRetracted;
		int pieceSize = 8;
		int maxI = MathInt.Floor(length / pieceSize);
		int chainFrame = Helpers.clamp((int)(5 * length / maxDist), 0, 5);
		float xOff = (length) * Helpers.cosb(byteAngle.Value);
		float yOff = (length) * Helpers.cosb(byteAngle.Value);

	///	DrawWrappers.DrawLine(
		//	pos.x, pos.y, mmx.getShootPos().x, mmx.getShootPos().y,
		//	new Color(206, 123, 239, 255), 4, ZIndex.Character - 2
		//);

		for (int i = 0; i < maxI; i++) {
			xOff = (length - (pieceSize * i)) * Helpers.cosb(byteAngle.Value);
			yOff = (length - (pieceSize * i)) * Helpers.sinb(byteAngle.Value);
			
			Global.sprites["chainrod_chain"].draw(
				chainFrame, pos.x - xOff, pos.y - yOff, 
				xDir, yDir, getRenderEffectSet(), 1, 1, 1, 
				ZIndex.Character - 1, angle: byteAngle.Value * 1.40625f
			);
		}
	}
}
