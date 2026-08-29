using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;

public class RocketPunchWC : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public static RocketPunchWC netWeaponGGR = new RocketPunchWC(RocketPunchType.GoGetterRight);
	public static RocketPunchWC netWeaponSB = new RocketPunchWC(RocketPunchType.SpoiledBrat);
	public static RocketPunchWC netWeaponIG = new RocketPunchWC(RocketPunchType.InfinityGig);
	public RocketPunchWC(RocketPunchType rocketPunchType) : base() {
		index = (int)WeaponIds.RocketPunch;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
		type = (int)rocketPunchType;
		projSprite = "rocket_punch_proj";

		if (rocketPunchType == RocketPunchType.None) {
			displayName = "None";
			description = new string[] { "Do not equip a Rocket Punch." };
			killFeedIndex = 126;
			ammousage = 0;
			fireRate = 0;
			vileWeight = 0;
		} else if (rocketPunchType == RocketPunchType.GoGetterRight) {
			fireRate = 60;
			displayName = "Go-Getter Right";
			projSprite = "rocket_punch_proj";
			description = new string[] { "A rocket punch sends your fist", "flying to teach enemies a lesson." };
			vileWeight = 3;
			damage = "3";
			hitcooldown = "0.5";
			flinch = "13";
			flinchCD = "1";
			effect = "Won't destroy on hit.";
		} else if (rocketPunchType == RocketPunchType.SpoiledBrat) {
			fireRate = 12;
			displayName = "Spoiled Brat";
			projSprite = "rocket_punch_sb_proj";
			description = new string[] { "Though lacking in power, this", "rocket punch offers intense speed." };
			killFeedIndex = 77;
			vileWeight = 3;
			damage = "2";
			hitcooldown = "0.1";
			flinch = "13";
			flinchCD = "1";
			effect = "Destroys on hit.";
		}
		if (rocketPunchType == RocketPunchType.InfinityGig) {
			fireRate = 60;
			displayName = "Infinity Gig";
			projSprite = "rocket_punch_ig_proj";
			description = new string[] { "Advanced homing technology can be", "difficult to get a handle on." };
			killFeedIndex = 78;
			vileWeight = 3;
			damage = "3";
			hitcooldown = "0.5";
			flinch = "13";
			flinchCD = "1";
			effect = "Homing,Travels further.";
		}
		if (rocketPunchType == RocketPunchType.EgotisticalPill) {
			fireRate = 60;
			displayName = "Infinity Gig";
		projSprite = "rocket_punch_ep_proj";
			description = new string[] { "Advanced homing technology can be", "difficult to get a handle on." };
			killFeedIndex = 78;
			vileWeight = 3;
			damage = "3";
			hitcooldown = "0.5";
			flinch = "13";
			flinchCD = "1";
			effect = "Homing,Travels further.";
		}
	}


}

public class RocketPunchProjWC : Projectile {
	public bool reversed;
	public bool returned;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor? target;
	int type = 0;

	int gigHits = 0;

	public RocketPunchProjWC(
		RocketPunchWC weapon, Point pos, int xDir, Player player,
		ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, getSpeed(weapon.type), 1,
		player, weapon.projSprite, Global.defFlinch, 0.3f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.RocketPunchWC;
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.15f;
		maxReverseTime = 0.3f;
		damager.flinch = Global.defFlinch;

		if (weapon.type == (int)RocketPunchType.SpoiledBrat) {
			damager.damage = 0.5f;
			damager.hitCooldown = 0;
			maxTime = 0.15f;
			destroyOnHit = true;
			projId = (int)ProjIds.SpoiledBratWC;
			type = 1;
		} else if (weapon.type == (int)RocketPunchType.InfinityGig) {
			projId = (int)ProjIds.InfinityGigWC;
			maxReverseTime = 0.5f;
			type = 2;
		} else if (weapon.type == (int)RocketPunchType.EgotisticalPill) {
			projId = (int)ProjIds.EgotisticalPillProj;
			damager.damage = 3f;
			maxReverseTime = 3f;
			maxTime = 5f;
			type = 3;
		} else {
			maxReverseTime = 0.2f;
			type = 0;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		canBeLocal = false;
	}

	public bool ownerExists => (owner.character?.destroyed == false);

	public override void update() {
		base.update();
		if (type == 1) {
			damager.damage = 0.5f;
		}
		if (time > 2) 	destroySelf("explosion", "explosion");
			
		if (ownedByLocalPlayer && !ownerExists) {
			destroySelf("explosion", "explosion");
			return;
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.08f) {
			smokeTime = 0;
			var smoke = new Anim(pos, "torpedo_smoke", xDir, null, true);
			smoke.setzIndex(zIndex - 100);
		}

		if (ownedByLocalPlayer && !reversed && reflectCount == 0 &&
			(type == (int)RocketPunchType.InfinityGig || damager.owner?.character is Vile vile2
			&& (vile2.phase2 || vile2.OverDrive || vile2.sprite.name.Contains("mk5"))) && type != (int)RocketPunchType.EgotisticalPill
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
		if (!reversed && type == (int)RocketPunchType.GoGetterRight && damager.owner?.character is Vile vile) {
			if (vile.player.input.isHeld(Control.Up, vile.player)) {
				incPos(new Point(0, -300 * Global.spf));
			} else if (vile.player.input.isHeld(Control.Down, vile.player)) {
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
		return type switch {
			(int)RocketPunchType.SpoiledBrat => 600,
			(int)RocketPunchType.InfinityGig => 500,
			_ => 500
		};
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (locallyControlled && type != (int)RocketPunchType.EgotisticalPill) {
			reversed = true;
			
		}
		
		if (isRunByLocalPlayer() && type != (int)RocketPunchType.EgotisticalPill) {
			reversed = true;
			RPC.actorToggle.sendRpc(netId, RPCActorToggleType.ReverseRocketPunch);
		}
	}
}

public class GoGetterRightAttack : CharState {
	bool shot = false;
	RocketPunchProjWC? proj;
	float specialPressTime;

	public float pushBackSpeed;

	public GoGetterRightAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	canSpecialCancel = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isBHeld(player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
			player.vileAmmo -= 8;
		}

	
		
		if (proj != null) {
			if (player.input.isBPressed(player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 4;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeToIdleOrFall();
					return;
				}	
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
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 3;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProjWC(new RocketPunchWC(RocketPunchType.GoGetterRight), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
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



public class InfinityGigAttack : CharState {
	bool shot = false;
	RocketPunchProjWC? proj;
	float specialPressTime;

	public float pushBackSpeed;

	public InfinityGigAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	canSpecialCancel = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isBHeld(player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
			player.vileAmmo -= 14;
		}

		
		if (proj != null) {
			if (player.input.isBPressed(player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 4;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
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
		proj = new RocketPunchProjWC(new RocketPunchWC(RocketPunchType.InfinityGig), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
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




public class EgotisticalPillAttack : CharState {
	bool shot = false;
	RocketPunchProjWC? proj;
	float specialPressTime;

	public float pushBackSpeed;

	public EgotisticalPillAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	canSpecialCancel = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isBHeld(player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
			player.vileAmmo -= 14;
		}

		
		if (proj != null) {
			if (player.input.isBPressed(player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 4;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
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
		proj = new RocketPunchProjWC(new RocketPunchWC(RocketPunchType.EgotisticalPill), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
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





public class VAVAGoldenRight : CharState {


	public VAVAGoldenRight() : base("golden_right") {
		canSpecialCancel = true;
		enterSound = "rocketPunch";
		superArmor = true;
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}





public class Vilemk2Mijo : CharState {


	public Vilemk2Mijo() : base("mijo") {
		canSpecialCancel = true;
		enterSound = "rocketPunch";
		superArmor = true;
	}

	public override void update() {
		base.update();
		character.playSound("rocketPunch", true);
		character.playSound("crash", true);
		if (stateTime > 2) {
			character.changeToIdleOrFall();
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
			new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}


public class ZeroNuclear : CharState {


	public ZeroNuclear() : base("nuclear") {
		canSpecialCancel = true;
		superArmor = true;
			canGainMeter = false;
	}

	public override void update() {
		base.update();
		if (character.frameIndex > 6) {
			character.playSound("rekkoha", true);
			character.playSound("crash", true);
			character.shakeCamera(true);
		}

		if (character.frameIndex > 8) {
			
		}
		if (stateTime > 8) {
			character.changeToIdleOrFall();
				new GigaCrushPilar(character.pos, ZIndex.Character + 10);
			
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}

public class SpoiledBratPunch : CharState {
	bool shot = false;
	RocketPunchProjWC proj;
	float specialPressTime;
	float shootcd;
	bool grounded;

	bool AIloopit;

	public float pushBackSpeed;

	public SpoiledBratPunch(string transitionSprite = "") : base("spoiled_brat", "", "", transitionSprite) {
		this.grounded = grounded;
		airMove = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();
		if (player.vileAmmo <= 0) {
			character.changeState(new Idle(), true);
			return;
		}
		Helpers.decrementTime(ref specialPressTime);
		Helpers.decrementTime(ref shootcd);

		if (proj != null && !player.input.isBHeld(player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (shootcd == 0 && (character.sprite.frameIndex == 1 || character.sprite.frameIndex == 3)) {
			shoot();
			shootcd = 0.1f;
			player.vileAmmo -= 4;
		}
		if (player.input.isAPressed(player) || AIloopit && stateTime < 1.2f) {
			specialPressTime = 0.25f;
		}

		if (specialPressTime == 0 || player.vileAmmo == 0) {
			character.changeState(new Idle(), true);
			return;
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
		if (player.isAI && Helpers.randomRange(0, 1) == 1) {
			AIloopit = true;
		}
		specialPressTime = 0.25f;
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
	public void shoot() {
		character.playSound("rocketPunch", sendRpc: true);
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProjWC(new RocketPunchWC(RocketPunchType.SpoiledBrat),
		character.pos.add(poi), character.xDir, character.player,
		character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}





public class InfinityGigSecond : Projectile, IDamagable {
	public Actor? target;
	public float smokeTime = 0;
	public float maxSpeed = 150;
	public InfinityGigSecond(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, float? angle = null, bool rpc = false
	) : base(
		pos, xDir, owner, "rocket_punch_ig_proj", netId, player
	) {

		netcodeOverride = NetcodeModel.FavorDefender;
		damager.damage = 1;
		damager.flinch = Global.halfFlinch;
		vel = new Point(150 * xDir, 0);
		fadeSprite = "explosion";
		fadeSound = "explosion";
		maxTime = 2f;
		projId = (int)ProjIds.InfinityGigSecond;
		fadeOnAutoDestroy = true;
		reflectableFBurner = true;
		customAngleRendering = true;
		this.angle = this.xDir == -1 ? 180 : 0;
		if (angle != null) {
			this.angle = angle.Value + (this.xDir == -1 ? 180 : 0);
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		canBeLocal = false;
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new InfinityGigSecond(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}
	bool homing = true;
	public void reflect(float reflectAngle) {
		angle = reflectAngle;
		target = null;
	}
	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}
	public override void update() {
		base.update();
		if (ownedByLocalPlayer && homing) {
			if (target != null) {
				if (!Global.level.gameObjects.Contains(target)) {
					target = null;
				}
			}
			if (target != null) {
				if (time < 3f) {
					var dTo = pos.directionTo(target.getCenterPos()).normalize();
					var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
					destAngle = Helpers.to360(destAngle);
					angle = Helpers.lerpAngle(angle, destAngle, Global.spf * 3);
				}
			}
			if (time >= 0.15) {
				target = Global.level.getClosestTarget(pos, damager.owner.alliance, true, aMaxDist: Global.screenW * 0.75f);
			} else if (time < 0.15) {
				//this.vel.x += this.xDir * Global.spf * 300;
			}
			vel.x = Helpers.cosd(angle) * maxSpeed;
			vel.y = Helpers.sind(angle) * maxSpeed;
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.2) {
			smokeTime = 0;
			if (homing) new Anim(pos, "torpedo_smoke", 1, null, true);
		}
	}
	public override void renderFromAngle(float x, float y) {
		var angle = this.angle;
		var xDir = 1;
		var yDir = 1;
		var frameIndex = 0;
		float normAngle = 0;
		if (angle < 90) {
			xDir = 1;
			//yDir = -1;
			normAngle = angle;
		}
		if (angle >= 90 && angle < 180) {
			xDir = -1;
			//yDir = -1;
			normAngle = 180 - angle;
		} else if (angle >= 180 && angle < 270) {
			xDir = -1;
			yDir = 1;
			normAngle = angle - 180;
		} else if (angle >= 270 && angle < 360) {
			xDir = 1;
			yDir = 1;
			normAngle = 360 - angle;
		}

		if (normAngle < 18) frameIndex = 0;
		else if (normAngle >= 18 && normAngle < 36) frameIndex = 1;
		else if (normAngle >= 36 && normAngle < 54) frameIndex = 2;
		else if (normAngle >= 54 && normAngle < 72) frameIndex = 3;
		else if (normAngle >= 72 && normAngle < 90) frameIndex = 4;

		sprite.draw(frameIndex, pos.x + x, pos.y + y, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex, actor: this);
	}

	public override void onDestroy() {
		base.onDestroy();
		new InfinityGigThird(pos.addxy(70, 2), -xDir, owner.character, owner, owner.getNextActorNetId(true), 30, true);
					
	}

	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		if (damage > 0) {
			destroySelf();
		}
	}
	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return damager.owner.alliance != damagerAlliance;
	}
	public bool isInvincible(Player attacker, int? projId) {
		return false;
	}
	public bool canBeHealed(int healerAlliance) {
		return false;
	}
	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) {
	}
	public bool isPlayableDamagable() {
		return false;
	}
}




public class InfinityGigThird: Projectile, IDamagable {
	public Actor? target;
	public float smokeTime = 0;
	public float maxSpeed = 150;
	public InfinityGigThird(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, float? angle = null, bool rpc = false
	) : base(
		pos, xDir, owner, "rocket_punch_ig_proj", netId, player
	) {
	
		netcodeOverride = NetcodeModel.FavorDefender;
		damager.damage = 1;
		damager.flinch = Global.halfFlinch;
		vel = new Point(150 * xDir, 0);
		fadeSprite = "explosion";
		fadeSound = "explosion";
		maxTime = 2f;
		projId = (int)ProjIds.InfinityGigSecond;
		fadeOnAutoDestroy = true;
		reflectableFBurner = true;
		customAngleRendering = true;
		this.angle = this.xDir == -1 ? 180 : 0;
		if (angle != null) {
			this.angle = angle.Value + (this.xDir == -1 ? 180 : 0);
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		canBeLocal = false;
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new InfinityGigThird(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}
	bool homing = true;
	public void reflect(float reflectAngle) {
		angle = reflectAngle;
		target = null;
	}
	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}
	public override void update() {
		base.update();
		if (ownedByLocalPlayer && homing) {
			if (target != null) {
				if (!Global.level.gameObjects.Contains(target)) {
					target = null;
				}
			}
			if (target != null) {
				if (time < 3f) {
					var dTo = pos.directionTo(target.getCenterPos()).normalize();
					var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
					destAngle = Helpers.to360(destAngle);
					angle = Helpers.lerpAngle(angle, destAngle, Global.spf * 3);
				}
			}
			if (time >= 0.15) {
				target = Global.level.getClosestTarget(pos, damager.owner.alliance, true, aMaxDist: Global.screenW * 0.75f);
			} else if (time < 0.15) {
				//this.vel.x += this.xDir * Global.spf * 300;
			}
			vel.x = Helpers.cosd(angle) * maxSpeed;
			vel.y = Helpers.sind(angle) * maxSpeed;
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.2) {
			smokeTime = 0;
			if (homing) new Anim(pos, "torpedo_smoke", 1, null, true);
		}
	}
	public override void renderFromAngle(float x, float y) {
		var angle = this.angle;
		var xDir = 1;
		var yDir = 1;
		var frameIndex = 0;
		float normAngle = 0;
		if (angle < 90) {
			xDir = 1;
			//yDir = -1;
			normAngle = angle;
		}
		if (angle >= 90 && angle < 180) {
			xDir = -1;
			//yDir = -1;
			normAngle = 180 - angle;
		} else if (angle >= 180 && angle < 270) {
			xDir = -1;
			yDir = 1;
			normAngle = angle - 180;
		} else if (angle >= 270 && angle < 360) {
			xDir = 1;
			yDir = 1;
			normAngle = 360 - angle;
		}

		if (normAngle < 18) frameIndex = 0;
		else if (normAngle >= 18 && normAngle < 36) frameIndex = 1;
		else if (normAngle >= 36 && normAngle < 54) frameIndex = 2;
		else if (normAngle >= 54 && normAngle < 72) frameIndex = 3;
		else if (normAngle >= 72 && normAngle < 90) frameIndex = 4;

		sprite.draw(frameIndex, pos.x + x, pos.y + y, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex, actor: this);
	}
	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		if (damage > 0) {
			destroySelf();
		}
	}


	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return damager.owner.alliance != damagerAlliance;
	}
	public bool isInvincible(Player attacker, int? projId) {
		return false;
	}
	public bool canBeHealed(int healerAlliance) {
		return false;
	}
	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) {
	}
	public bool isPlayableDamagable() {
		return false;
	}
}





public class RocketPunchProj : Projectile {
	public bool reversed;
	public bool returned;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor? target;
	public int type = 0;
	public int num = 0;
	public RocketPunchProj(
		Point pos, int xDir, int num, string sprite,
		Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, sprite , netId, player
	) {
		damager.damage = 3;
		damager.flinch = Global.halfFlinch;
		damager.hitCooldown = 30;
		vel = new Point (getSpeed(type) * xDir, 0);
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (ownerPlayer.character != null) setzIndex(ownerPlayer.character.zIndex - 100);
		minTime = 0.2f;
		maxReverseTime = 0.4f;
		damager.flinch = Global.halfFlinch;
		this.num = num;
		if (num == (int)RocketPunchType.SpoiledBrat) {
			weapon = SpoiledBrat.netWeapon;
			damager.damage = 2;
			damager.hitCooldown = 6;
			maxTime = 0.25f;
			destroyOnHit = true;
			projId = (int)ProjIds.SpoiledBrat;
			sprite = "rocket_punch_sb_proj";
			type = 1;
		} else if (num == (int)RocketPunchType.InfinityGig) {
			weapon = InfinityGig.netWeapon;
			projId = (int)ProjIds.InfinityGig;
			sprite = "rocket_punch_ig_proj";
			type = 2;
		} else if (num == (int)RocketPunchType.GoGetterRight) {
			weapon = GoGetterRight.netWeapon;
			maxReverseTime = 0.3f;
			projId = (int)ProjIds.RocketPunch;
			type = 0;
			sprite = "rocket_punch_proj";
		}
		if (rpc) {
			List<Byte> extraBytes = new List<Byte> {
			};
			extraBytes.Add((byte)num);
			extraBytes.AddRange(Encoding.ASCII.GetBytes(sprite));
			rpcCreate(pos, owner, ownerPlayer, netId, xDir, extraBytes.ToArray());

		}
		canBeLocal = false;
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		string sprite = Encoding.ASCII.GetString(args.extraData[1..]);
		return new RocketPunchProj(
			args.pos, args.xDir, args.extraData[0], sprite, args.owner, args.player, args.netId
		);
	}

	public bool ownerExists => (owner.character?.destroyed == false);

	public override void update() {
		base.update();
		if (ownedByLocalPlayer && !ownerExists) {
			destroySelf("explosion", "explosion");
			return;
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.08f) {
			smokeTime = 0;
			var smoke = new Anim(pos, "torpedo_smoke", xDir, null, true);
			smoke.setzIndex(zIndex - 100);
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
				vel.x = 500 * xDir;
			} else if (target != null) {
				vel = new Point(0, 0);
				Point targetPos = target.getCenterPos();
				move(pos.directionToNorm(targetPos).times(500));
				if (pos.distanceTo(targetPos) < 5) {
					reversed = true;
				}
				forceNetUpdateNextFrame = true;
			}
		}
		if (!reversed && type == (int)RocketPunchType.GoGetterRight && damager.owner?.character is Vile vile) {
			if (vile.player.input.isHeld(Control.Up, vile.player)) {
				incPos(new Point(0, -300 * Global.spf));
			} else if (vile.player.input.isHeld(Control.Down, vile.player)) {
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

			move(pos.directionToNorm(returnPos).times(getSpeed(type)));
			if (pos.distanceTo(returnPos) < 10) {
				returned = true;
				destroySelf();
			}
		}
	}

	
	public override void onHitWall(CollideData other) {
		if (!ownedByLocalPlayer) return;
		reversed = true;
	}
	

	public static float getSpeed(int type) {
		return type switch {
			(int)RocketPunchType.SpoiledBrat => 600,
			(int)RocketPunchType.InfinityGig => 500,
			_ => 500
		};
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