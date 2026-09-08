using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class GBDHomingState : CharState {
	
	bool isDone;
	GBDMarker otherChar;
	float moveAmount;
	float maxMoveAmount;
	public GBDHomingState(GBDMarker otherChar) : 
	base("hyperdash_attack_homing"
	) {
		enterSound = "fstagUppercut";
		this.otherChar = otherChar;
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.grounded = false;
		character.vel.y = 0;
		maxMoveAmount = character.getCenterPos().distanceTo(otherChar.getCenterPos());
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		otherChar.destroySelf(otherChar.fadeSprite, otherChar.fadeSound);
	}

	public override void update() {
		base.update();

		
		

		Point amount = character.getCenterPos().directionToNorm(otherChar.getCenterPos()).times(350);

		//character.move(amount);
		if (otherChar.pos.distanceTo(character.pos) < 300) {
		character.changePos(Point.lerp(character.pos, otherChar.pos, 0.05f));
		} else {
		character.changePos(Point.lerp(character.pos, otherChar.pos, 0.02f));
		}
		moveAmount += amount.magnitude * Global.spf;
		if (moveAmount > maxMoveAmount) {
			character.changeToIdleOrFall();
			return;
		}
	}
}



public class GBDMarker : Projectile, IDamagable {
	public bool landed;
	public float health = 2;
	public Player player;
	float maxSpeed = 150;

	public GBDMarker(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "gbd_marker", netId, player	
	) {
		weapon = MagnetMine.netWeapon;
		damager.damage = 1;
		damager.flinch = Global.miniFlinch;
		vel = new Point(75 * xDir, 0);
		//maxTime = 2f;
		maxDistance = 224;
		fadeSprite = "explosion";
		fadeSound = "explosionX2";
		reflectable = false;
		projId = (int)ProjIds.MagnetMine;
		this.player = player;
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		canBeLocal = false;
		destroyOnHit = true;
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new GBDMarker(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}

	public override void update() {
		base.update();

		if (landed && ownedByLocalPlayer) {
			moveWithMovingPlatform();
		}

		if (ownedByLocalPlayer && owner != null && !landed) {
			vel.x += xDir * 600 * Global.spf;
			if (vel.x > maxSpeed) vel.x = maxSpeed;
			if (vel.x < -maxSpeed) vel.x = -maxSpeed;

			if (!owner.isDead) {
				if (owner.input.isHeld(Control.Up, owner)) {
					vel.y = Helpers.clampMin(vel.y - Global.spf * 2000, -300);
				}
				if (owner.input.isHeld(Control.Down, owner)) {
					vel.y = Helpers.clampMax(vel.y + Global.spf * 2000, 300);
				}
			}
		}
	}

	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		if (!ownedByLocalPlayer) {
			return;
		}
		health -= damage;
		if (health <= 0) {
			destroySelf();
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (!landed && other.gameObject is Wall) {
			landed = true;
			updateDamager(2);

			if (player.isMainPlayer && !Global.level.gameMode.isTeamMode) {
				removeRenderEffect(RenderEffectType.BlueShadow);
				removeRenderEffect(RenderEffectType.RedShadow);
				addRenderEffect(RenderEffectType.GreenShadow);
			}

			vel = new Point();
			changeSprite("gbd_marker", true);
			playSound("minePlant");
			damager.flinch = Global.defFlinch;
			maxTime = 300;

			var triggers = Global.level.getTriggerList(this, 0, 0);
			if (triggers.Any(t => t.gameObject is GBDMarker)) {
				incPos(new Point(Helpers.randomRange(-2, 2), Helpers.randomRange(-2, 2)));
			}
		}
	}

	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return player.alliance != damagerAlliance;
	}

	public bool isInvincible(Player attacker, int? projId) {
		return false;
	}

	public bool canBeHealed(int healerAlliance) {
		return false;
	}

	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) {
	}

	public override void onDestroy() {
		if (!ownedByLocalPlayer) {
			return;
		}
		if (owner.character != null){
		owner.character.GBDMarker.Remove(this);
		}
	}

	public bool canBeSucked(int alliance) {
		if (player.alliance == alliance) return false;
		return true;
	}

	public bool isPlayableDamagable() {
		return false;
	}
}


public class GBDSniperState : CharState {
	public float soundCooldown;

	public Projectile fSplasherProj;


	public bool beam;


	public GBDSniperState()
		: base("sniper", "") {

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("counters_usp_clipin", true);
		character.stopMoving();
		

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

	public override void update() {
		base.update();
		Helpers.decrementTime(ref soundCooldown);
		if (!player.input.isBHeld(player)) {
			character.changeToIdleOrFall();
		}
		

		if (
		player.input.isAPressed(player) && soundCooldown == 0){
			if (character.frameIndex != 2) {
				character.frameIndex = 2;
				soundCooldown = 2;
				character.playSound("spiralMagnum");
			
				}
		}


	}
}




public class GBDUppercut : CharState {


	public GBDUppercut() : base("uppercut") {
	enterSound = "punch2";
	canSpecialCancel = true;
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


public class GBDTonfaCharge : CharState {


	public GBDTonfaCharge() : base("tonfa_charge") {
		normalCtrl = true;
	}

	public override void update() {
		base.update();
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		if (character.frameIndex == 3) {
			if (!once){
			character.addDamageText("!!!", 1);
			once = true;
			}
		}


		if (!player.input.isR2Held(player)) {
			if (character.frameIndex != 3) {
				if (player.input.isHeld(Control.Up, player)){
				character.changeState(new GBDTonfaAttackU(), true);
				} else {
				character.changeState(new GBDTonfaAttackF(), true);	
				}
			character.slideVel = character.xDir * character.getDashSpeed() * 0.5f;
			} else {
				if (player.input.isHeld(Control.Up, player)){
				character.changeState(new GBDTonfaAttackUCharged(), true);
				} else {
				character.changeState(new GBDTonfaAttackFCharged(), true);	
				}
			character.slideVel = character.xDir * character.getDashSpeed();	
			}
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





public class GBDTonfaAttackF : CharState {

	bool second;
	public GBDTonfaAttackF() : base("tonfa_f") {
	enterSound = "recoilRod1";
	}

	public override void update() {
		base.update();
		if (player.input.isR2Pressed(player) && !second) {
			character.changeSpriteFromName("tonfa_f2", true);
			second = true;
			sprite = "tonfa_f2";
			character.playSound("recoilRod1", true);
		}

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



public class GBDTonfaAttackFCharged : CharState {


	public GBDTonfaAttackFCharged() : base("tonfa_charged_f") {
	enterSound = "recoilRod2";
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





public class GBDTonfaAttackU : CharState {

	bool second;
	public GBDTonfaAttackU() : base("tonfa_u") {
	enterSound = "recoilRod1";
	}

	public override void update() {
		base.update();
		if (player.input.isR2Pressed(player) && !second) {
			character.changeSpriteFromName("tonfa_u2", true);
			second = true;
			sprite = "tonfa_u2";
			character.slideVel = character.xDir * character.getDashSpeed() * 0.7f;
			character.playSound("recoilRod1", true);
		}

		if (player.input.isR2Pressed(player) && second && character.frameIndex > 2) {
			character.changeSpriteFromName("tonfa_overhead", true);
			second = true;
			sprite = "tonfa_overhead";
			character.slideVel = character.xDir * character.getDashSpeed() ;
			character.playSound("recoilRod1", true);
		}

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






public class GBDTonfaAttackUCharged : CharState {


	public GBDTonfaAttackUCharged() : base("tonfa_charged_u") {
	enterSound = "recoilRod2";
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







