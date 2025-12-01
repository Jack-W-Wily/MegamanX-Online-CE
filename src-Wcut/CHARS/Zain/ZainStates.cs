using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;



public class ZainParryWeapon : Weapon {
	public ZainParryWeapon() : base() {
		fireRate = 60;
		index = (int)WeaponIds.ZainParry;
		killFeedIndex = 172;
	}
}



public class ZainUPParryMeleeState : CharState {
	Actor counterAttackTarget;
	float damage;
	public ZainUPParryMeleeState(Actor counterAttackTarget, float damage) : base("unpo_parry_attack") {
		// invincible = true;
		superArmor = true;
		this.counterAttackTarget = counterAttackTarget;
		this.damage = damage;
	}

	public override void update() {
		base.update();

		if (counterAttackTarget != null) {
			character.turnToPos(counterAttackTarget.pos);

			float dist = character.pos.distanceTo(counterAttackTarget.pos);
			if (dist < 150) {
				if (character.frameIndex >= 4 && !once) {
					if (character.pos.distanceTo(counterAttackTarget.pos) > 10) {
						character.moveToPos(counterAttackTarget.pos, 350);
					}
				}
			}
		}

		Point? shootPos = character.getFirstPOI("melee");
		if (!once && shootPos != null) {
			once = true;
			new UPParryMeleeProj(shootPos.Value, character.xDir, damage,
			character, player, player.getNextActorNetId(), rpc: true);
			character.playSound("upParryAttack", sendRpc: true);
	
			
			character.shakeCamera(sendRpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		//character.frameIndex = 2;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	//	if (character is RagingChargeX mmx) {
	//		mmx.parryCooldown = mmx.maxParryCooldown;
	//	}
	}
}




public class ZainUPParryStartState : CharState {
	// RagingChargeX mmx;
	public ZainUPParryStartState() : base("unpo_parry_start") {
	}

	public override void update() {
		base.update();

		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public void counterAttack(Player? damagingPlayer, Actor? damagingActor, float damage) {
		Actor? counterAttackTarget = null;
		Projectile? absorbedProj = null;
		
	//	if (player.weapon is XBuster { isUnpoBuster: true }) {
	//		player.weapon.ammo = player.weapon.maxAmmo;
	//	}
		
		if (damagingActor is Projectile proj) {
			if (proj.ownerActor != null) {
				counterAttackTarget = proj.ownerActor;
			}
			if (!proj.isMelee && proj.shouldVortexSuck) {
				absorbedProj = proj;
				absorbedProj.destroySelfNoEffect(doRpcEvenIfNotOwned: true);
			}
		}

		if (absorbedProj != null) {
			if (character.ownedByLocalPlayer) {
				bool shootProj = false;
				bool absorbThenShoot = false;
				character.playSound("upParryAbsorb", sendRpc: true);
				if (!player.input.isWeaponLeftOrRightHeld(player)) {
					character.unpoAbsorbedProj = absorbedProj;
			//		character.player.weapons.Add(new AbsorbWeapon(absorbedProj));
				} else {
					shootProj = true;
					absorbThenShoot = true;
				}
				//mmx.refillUnpoBuster();
				character.changeState(new ZainUPParryProjState(absorbedProj, shootProj, absorbThenShoot), true);
			}

			return;
		}

		if (counterAttackTarget == null) {
			counterAttackTarget = damagingPlayer?.character ?? damagingActor;
		}

		if (counterAttackTarget != null && character.pos.distanceTo(counterAttackTarget.pos) < 75 && counterAttackTarget is Character chr) {
	//		if (!chr.ownedByLocalPlayer) {
	//			RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
	//		} else {
	//			chr.changeState(new ParriedState(), true);
	//		}
		}
	//	mmx.addPercentAmmo(100);
		character.playSound("upParry", sendRpc: true);
		character.changeState(new ZainUPParryMeleeState(counterAttackTarget, damage), true);
	}

	public bool canParry(Actor damagingActor) {
		return character.frameIndex == 0;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

		if (player.isZain){
			character.playSound("distortion_c");
		}
	//	mmx = character as RagingChargeX;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class ZainUPParryMeleeStateBuxa : CharState {
	Actor counterAttackTarget;
	float damage;
	public ZainUPParryMeleeStateBuxa(Actor counterAttackTarget, float damage) : base("unpo_parry_attack", "", "", "") {
		this.counterAttackTarget = counterAttackTarget;
		this.damage = damage;
	}

	public override void update() {
		base.update();

		if (counterAttackTarget != null) {
			character.turnToPos(counterAttackTarget.pos);

			float dist = character.pos.distanceTo(counterAttackTarget.pos);
			if (dist < 150) {
				if (character.frameIndex >= 4 && !once) {
					if (character.pos.distanceTo(counterAttackTarget.pos) > 10) {
						character.moveToPos(counterAttackTarget.pos, 350);
					}
				}
			}
		}

		Point? shootPos = character.getFirstPOI("melee");
		if (!once && shootPos != null) {
			once = true;
			new UPParryMeleeProj(shootPos.Value, character.xDir, damage,
			character, player, player.getNextActorNetId(), rpc: true);
			character.playSound("upParryAttack", sendRpc: true);
			character.shakeCamera(sendRpc: true);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		//character.frameIndex = 2;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	//	if (character is MegamanX mmx) {
	//		mmx.parryCooldown = mmx.maxParryCooldown;
	//	}
	}
}


public class ZainUPParryProjState : CharState {
	Projectile? otherProj;
	Anim? absorbAnim;
	bool shootProj;
	bool absorbThenShoot;
	public ZainUPParryProjState(Projectile otherProj, bool shootProj, bool absorbThenShoot) : base("unpo_parry_attack") {
		this.otherProj = otherProj;
		invincible = true;
		this.shootProj = shootProj;
		this.absorbThenShoot = absorbThenShoot;
	}

	public override void update() {
		base.update();

		if (!shootProj && character.sprite.frameIndex >= 1) {
			character.sprite.frameIndex = 1;
			character.sprite.frameSpeed = 0;
		}

		if (absorbAnim != null) {
			absorbAnim.moveToPos(character.getFirstPOIOrDefault(), 350);
			absorbAnim.xScale -= Global.spf * 5;
			absorbAnim.yScale -= Global.spf * 5;
			if (absorbAnim.xScale <= 0) {
				absorbAnim.destroySelf();
				absorbAnim = null;
				if (!shootProj) {
					character.changeToIdleOrFall();
					return;
				}
			}
		}

		Point? shootPos = character.getFirstPOI("proj");
		if (!once && shootPos != null) {
			once = true;
			float damage = Math.Max(otherProj.damager.damage, 4);
			//int flinch = otherProj.damager.flinch;
			int flinch = Global.defFlinch;
			float hitCooldown = otherProj.damager.hitCooldownSeconds;
			new UPParryRangedProj(
					shootPos.Value, character.xDir,
					otherProj.sprite.name, damage, flinch, hitCooldown,
					character, player, player.getNextActorNetId(), rpc: true
				);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!shootProj || absorbThenShoot) {
			absorbAnim = new Anim(otherProj.pos, otherProj.sprite.name, otherProj.xDir, player.getNextActorNetId(), false, sendRpc: true);
			absorbAnim.syncScale = true;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		absorbAnim?.destroySelf();
			character.unpoAbsorbedProj = null;		

		//character.unpoAbsorbedProj = null;
	//	if (character is RagingChargeX mmx) {
	//		mmx.parryCooldown = mmx.maxParryCooldown;
	//	}
	}
}

public class ZainParryStartState : CharState {
	public ZainParryStartState() : base("parry_start", "", "", "") {
		superArmor = true;
	}

	public override void update() {
		base.update();

		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
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
			if (!chr.ownedByLocalPlayer) {
				RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
			} else {
				chr.changeState(new ParriedState(), true);
			}
		}

		character.playSound("zeroParry", sendRpc: true);
		character.changeState(new ZainParryMeleeState(counterAttackTarget), true);
		character.addHealth(1);
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		
		//character.parryCooldown = character.maxParryCooldown;
	}

	public bool canParry(Actor damagingActor) {
	
		return character.frameIndex == 1;
	}


	
		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

			character.playSound("distortion_d");
		}
}

public class ZainParryMeleeState : CharState {
	Actor? counterAttackTarget;
	Point counterAttackPos;
	public ZainParryMeleeState(Actor? counterAttackTarget) : base("parry", "", "", "") {
		invincible = true;
		this.counterAttackTarget = counterAttackTarget;
	}

	public override void update() {
		base.update();

		if (counterAttackTarget != null) {
			character.turnToPos(counterAttackPos);
			float dist = character.pos.distanceTo(counterAttackPos);
			if (dist < 150) {
				if (character.frameIndex >= 1 && !once) {
					if (dist > 5) {
						var destPos = Point.lerp(character.pos, counterAttackPos, Global.spf * 5);
						character.changePos(destPos);
					}
				}
			}
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (counterAttackTarget != null) {
			counterAttackPos = counterAttackTarget.pos.addxy(character.xDir * 30, 0);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);

	}
}




public class ZainDashParryState : CharState {
	public ZainDashParryState() : base("parry_start", "", "", "") {
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
	//	if (player.isDragoon)return character.frameIndex < 5;
		
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




public class ZainShinGroundStab : CharState {

	public float pushBackSpeed;

	bool fired;
	public ZainShinGroundStab(string transitionSprite = "")
		: base("groundstab", "", "", transitionSprite) {
		airMove = true;
		superArmor = true;
		enterSound = "dbzpunchwave_1";
	}

	public override void update()
	{
	
		base.update();


		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("flashysnd_1", forcePlay: false, sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);
			new ZainPillar(new ElectricSpark(), character.pos.addxy(-100 , 0), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
			new ZainPillar(new ElectricSpark(), character.pos.addxy(100 ,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
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

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}

}


public class ZainParryShinStartState : CharState {
	public ZainParryShinStartState() : base("super_parry", "", "", "") {
		superArmor = true;
	}

	public override void update() {
		base.update();

		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
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
		if (counterAttackTarget != null && character.pos.distanceTo(counterAttackTarget.pos) < 999 &&
			counterAttackTarget is Character chr && stunnableParry
		) {
			if (!chr.ownedByLocalPlayer) {
				RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
			} else {
				chr.changeState(new ParriedState(), true);
			}
		}

		character.playSound("zeroParry", sendRpc: true);
		character.changeState(new ZainParryShinMeleeState(counterAttackTarget), true);
		character.addHealth(5);
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		
		//character.parryCooldown = character.maxParryCooldown;
	}

	public bool canParry(Actor damagingActor) {
	
		return character.frameIndex == 0;
	}


	
		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
			if (!player.isZain){
			character.changeSpriteFromName("parry_start", true);
			}
			character.playSound("distortion_d");
		}
}

public class ZainParryShinMeleeState : CharState {
	Actor? counterAttackTarget;
	Point counterAttackPos;
	public ZainParryShinMeleeState(Actor? counterAttackTarget) : base("uppercut_slash", "", "", "") {
		invincible = true;
		this.counterAttackTarget = counterAttackTarget;
	}

	public override void update() {
		base.update();

		if (counterAttackTarget != null) {
			character.turnToPos(counterAttackPos);
			float dist = character.pos.distanceTo(counterAttackPos);
			if (dist < 950) {
				if (character.frameIndex >= 1 && !once) {
					if (dist > 5) {
						var destPos = Point.lerp(character.pos, counterAttackPos, Global.spf * 5);
						character.changePos(destPos);
					}
				}
			}
		}
		if (!player.isZain){
			character.changeSpriteFromName("parry", true);
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
			character.playSound("flashysnd_1", forcePlay: false, sendRpc: true);
			if (player.isZain){
				new ZainSaberProj(
					new ZSaber(), character.pos.addxy(30 * character.xDir, -20),
					character.xDir, player, player.getNextActorNetId(), rpc: true
				);
			}
		if (counterAttackTarget != null) {
			counterAttackPos = counterAttackTarget.pos.addxy(character.xDir * 30, 0);
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);

	}
}



public class ZainShinProjSwingState : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	bool once;

	bool once1;
	public ZainShinProjSwingState(
		bool grounded, bool shootProj
	) : base(
		grounded ? "super_slash" : "super_slash", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "super_slash";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
		useDashJumpSpeed = true;
		bonusAttackCtrl = true;
		superArmor = true;
	}


	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void update() {
		base.update();
		if (player.input.isHeld(Control.Special2, player) && character.frameIndex == 2) {
				character.frameIndex = 2;
		}

		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_2", forcePlay: false, sendRpc: true);

			if (shootProj) {
				character.playSound("flashysnd_1", forcePlay: false, sendRpc: true);

				new ZainSaberProj(
					new ZSaber(), character.pos.addxy(30 * character.xDir, -20),
					character.xDir, player, player.getNextActorNetId(), rpc: true
				);
			}
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "super_slash";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}




public class ZainProjSwingState : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	bool once;

	bool once1;
	public ZainProjSwingState(
		bool grounded, bool shootProj
	) : base(
		grounded ? "slash" : "projswing_air", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "slash";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
		useDashJumpSpeed = true;
		bonusAttackCtrl = true;
	}


	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (player.input.isHeld(Control.Up, player)) {
			character.changeSpriteFromName("uppercut_slash", true);
		}
	}

	public override void update() {
		base.update();


		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_2", forcePlay: false, sendRpc: true);

			if (shootProj) {
				character.playSound("flashysnd_1", forcePlay: false, sendRpc: true);

				new ZainSaberProj(
					new ZSaber(), character.pos.addxy(30 * character.xDir, -20),
					character.xDir, player, player.getNextActorNetId(), rpc: true
				);
			}
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "projswing_air";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}



public class ZainKokuSlash : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	bool once;

	bool once1;
	public ZainKokuSlash(
		bool grounded, bool shootProj
	) : base(
		grounded ? "projswing" : "projswing_air", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "slash";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
		useDashJumpSpeed = true;
		
	}


		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);


		
			bonusAttackCtrl = true;
		

		
		 if (base.player.input.isHeld("up", base.player)) {
			character.changeSpriteFromName("rising", true);
			character.dashedInAir++;
			float ySpeedMod = 1.5f;
			character.vel.y = (0f - character.getJumpPower()) * ySpeedMod;
		}

		 if (player.input.isHeld(Control.Down, player)
		&& character.grounded){
		    character.changeSpriteFromName("thrust", true);
		}

		if (base.player.input.isHeld(Control.Down, base.player)
		&& !character.grounded){
		    character.changeSpriteFromName("projswing", true);	
			character.vel.y += 300;	
		}
	}

	public override void update() {
		base.update();

		

		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_1", forcePlay: false, sendRpc: true);
			if (shootProj) {
				new ZainSaberProj(
					new ZSaber(), character.pos.addxy(30 * character.xDir, -20),
					character.xDir, player, player.getNextActorNetId(), rpc: true
				);
			}
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "projswing_air";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}






public class ZainGrab : CharState {


	
	public float pushBackSpeed;



	public ZainGrab(string transitionSprite = "")
		: base("grab_2", "", "", transitionSprite)
	{
	airMove = true;
	superArmor = true;
	enterSound = "punch1";
	}

	public override void update()
	{
	
		base.update();
	
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

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}


		if (player.input.isPressed(Control.Shoot, player) &&
		character.downPressedTimes > 0 || player.isAI) {
			character.changeState(new ZainGroundStab(), true);
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




public class ZainGroundStab : CharState {



	public float pushBackSpeed;


	bool fired;
	public ZainGroundStab(string transitionSprite = "")
		: base("groundstab", "", "", transitionSprite) {
		airMove = true;
		superArmor = true;
		enterSound = "dbzpunchwave_1";
	}

	public override void update() {

		base.update();


		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);
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

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}



}




	

public class ZainGrabStab : CharState {

	public float pushBackSpeed;


	bool fired;
	public ZainGrabStab(string transitionSprite = "")
		: base("stabgrab", "", "", transitionSprite) {
		airMove = true;
		superArmor = true;
	}

	public override void update() {

		base.update();


		if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_1", forcePlay: false, sendRpc: true);
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



public class ZainJab : CharState {


	bool fired;
	public ZainJab(string transitionSprite = "")
		: base("jab", "", "", transitionSprite)
	{
	airMove = true;
	superArmor = true;
}

	public override void update()
	{
	
		base.update();


			if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_3", forcePlay: false, sendRpc: true);
		}

	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
		}		
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class ZainAirDunk : CharState {


	
	public float pushBackSpeed;


	bool fired;
	public ZainAirDunk(string transitionSprite = "")
		: base("air_dunk", "", "", transitionSprite)
	{
	airMove = true;
	superArmor = true;
}

	public override void update()
	{
	
		base.update();


			if (character.frameIndex >= 2 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_3", forcePlay: false, sendRpc: true);
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




public class ZainGrabStabEnd : CharState {


	
	public float pushBackSpeed;


	bool fired;

	bool fired2;
	public ZainGrabStabEnd(string transitionSprite = "")
		: base("stabgrab_end", "", "", transitionSprite)
	{
	airMove = true;
	superArmor = true;
	}

	public override void update()
	{
	
		base.update();


			if (character.frameIndex >= 3 && !fired) {
			fired = true;
			character.playSound("dbzpunchwave_1", forcePlay: false, sendRpc: true);
		}



			if (character.frameIndex >= 3 && !fired2) {
			fired2 = true;
			character.shakeCamera(sendRpc: true);
			character.playSound("crash", forcePlay: false, sendRpc: true);
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
		character.xDir *= -1;
    }
}



public class ZainGrabSlash : CharState {


	
	public float pushBackSpeed;



	public ZainGrabSlash(string transitionSprite = "")
		: base("grab", "", "", transitionSprite)
	{
	airMove = true;
	superArmor = true;
	enterSound = "dbzpunchwave_1";
	}

	public override void update()
	{
	
		base.update();


		
	
		
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





