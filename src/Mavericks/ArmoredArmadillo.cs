using System;

namespace MMXOnline;

public class ArmoredArmadillo : Maverick {
	public ArmoredAProjWeapon projWeapon = new();
	public ArmoredAChargeReleaseWeapon chargeReleaseWeapon = new();
	public ArmoredARollWeapon rollWeapon = new();
	public const float rollTransJumpPower = 250;
	public bool noArmor;

	public ArmoredArmadillo(Player player, Point pos, Point destPos, int xDir, ushort? netId, bool ownedByLocalPlayer, bool sendRpc = false) :
		base(player, pos, destPos, xDir, netId, ownedByLocalPlayer) {
		stateCooldowns.Add(typeof(MShoot), new MaverickStateCooldown(false, true, 0.4f));
		//stateCooldowns.Add(typeof(ArmoredARollEnterState), new MaverickStateCooldown(false, false, 4));

		spriteToCollider["roll"] = getRollCollider();
		spriteToCollider["na_roll"] = getRollCollider();

		weapon = new Weapon(WeaponIds.ArmoredAGeneric, 95);

		awardWeaponId = WeaponIds.RollingShield;
		weakWeaponId = WeaponIds.ElectricSpark;
		weakMaverickWeaponId = WeaponIds.SparkMandrill;

		netActorCreateId = NetActorCreateId.ArmoredArmadillo;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		// Ammo.
		usesAmmo = true;
		canHealAmmo = true;
		ammo = 32;
		maxAmmo = 32;
		grayAmmoLevel = 8;
	//	barIndexes = (56, 45);
	}

	public override void setHealth(float lastHealth) {
		base.setHealth(lastHealth);
		/*
		if (health < maxHealth * 0.5f) {
			removeArmor(false);
		}
		*/
	}

	public void removeArmor(bool playAnim) {
		if (!ownedByLocalPlayer) return;
		if (!noArmor) {
			noArmor = true;
			changeSpriteFromName(state.sprite, true);
			if (playAnim) {
				Anim.createGibEffect("armoreda_armorpiece", getCenterPos(), player, sendRpc: true);
				playSound("explosion", sendRpc: true);
			}
		}
	}


	public bool isInvincible(Player attacker, int? projId) {
		return state is ArmoredAGuardChargeStateMAX || state is ArmoredAGuardReleaseStateMAX;
	}






	public Damager? targetDamager;

	float AtkCD;
	bool desperation = false;
	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;

			Helpers.decrementTime(ref AtkCD);



			if ((state is MIdle or MRun or MLand or MJump or MFall) && player.isAI){	
		
		
			foreach (var otherPlayer in Global.level.players) {
				if (otherPlayer.character == null) continue;
				if (otherPlayer == player) continue;
				if (otherPlayer == targetDamager?.owner) continue;
				if (otherPlayer.character.isInvulnerable()) continue;
				if (Global.level.gameMode.isTeamMode && otherPlayer.alliance != player.alliance) continue;
				if (otherPlayer.character.getCenterPos().distanceTo(getCenterPos()) > 300) continue;
				Character target = otherPlayer.character;

				if (pos.x > target.pos.x) {
					if (xDir != -1) {
					xDir = -1;
					}
				} else {
					if (xDir != 1) {
					xDir = 1;
					}
				}
				
			
					if (Helpers.randomRange(0,5) == 0){
							changeState(new ArmoredAGuardState());
					}
					if (Helpers.randomRange(0,5) == 1){
							changeState(new ArmoredARollEnterState());
					}
					if (Helpers.randomRange(0,5) == 2){
						changeState(getShootState(true));
					}
					if (Helpers.randomRange(0,5) == 3 && player.bonusHealth <= 0){
							if (!desperation){
						changeState(new ArmoredAGuardChargeStateMAX(6));
						desperation = true;
						}	else {
						changeState(new ArmoredAGuardChargeState(3));
						}
					}
					if (Helpers.randomRange(0,5) == 4){
						changeState(new ArmoredASword());
					}

					if (Helpers.randomRange(0,5) == 5){
						changeState(new BBuffaloDashState());
					}

				


				

				
				break;
			}
	
		AtkCD = 1f;
		}




		if (state is ArmoredARollState) {
			drainAmmo(4);
		} else if (state is ArmoredAGuardState) {
			drainAmmo(1);
		} else if (state is not ArmoredARollEnterState && state is not ArmoredARollExitState && state is not ArmoredAGuardState) {
			rechargeAmmo(2);
		}

		if (aiBehavior == MaverickAIBehavior.Control && !player.isSummoner()) {
			if (state is MIdle or MRun or MLand or MJump or MFall) {
				if (shootPressed()) {
					changeState(getShootState(false));
				} else if (specialPressed() && !noArmor) {
					if (ammo > 0) {
					
					if (player.input.isHeld(Control.Special2, player)){
						if (player.currency > 8){
							changeState(new ArmoredAGuardChargeStateMAX(6));
						playSound("ching", true);
						player.currency -= 9;
						}
					} else {
						changeState(new ArmoredAGuardState());
					}
					}
				} else if (input.isPressed(Control.Dash, player)) {
					if (ammo >= 8) {
						deductAmmo(8);
						changeState(new ArmoredARollEnterState());
					}
				}
			}
		} else {
			if (state is MIdle or MRun or MLand or MShoot) {
				bool shouldGuard = false;
				Rect rect = collider.shape.getRect();
				if (xDir == -1) {
					rect.x1 -= rect.w() * 3;
				} else {
					rect.x2 += rect.w() * 3;
				}
				var hits = Global.level.checkCollisionsShape(rect.getShape(), null);
				foreach (var hit in hits) {
					if (hit.gameObject is Projectile proj && proj.owner.alliance != player.alliance &&
						MathF.Sign(proj.deltaPos.x) != xDir
					) {
						shouldGuard = true;
						break;
					}
				}

				if (shouldGuard) {
					changeState(new ArmoredAGuardState());
				}
			}
		}

		/*
		if (state is not ArmoredAZappedState && state is not MDie && !noArmor && health < maxHealth * 0.5f)
		{
			removeArmor(true);
		}
		*/
	}

	public override string getMaverickPrefix() {
		return "armoreda";
	}

	public bool hasNoArmor() {
		return sprite.name.Contains("armoreda_na_");
	}

	public Collider getRollCollider() {
		var rect = new Rect(0, 0, 32, 26);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override MaverickState[] aiAttackStates() {
		return [
			getShootState(true),
			new ArmoredARollEnterState(),
		];
	}

	public override MaverickState getRandomAttackState() {
		return aiAttackStates().GetRandomItem();
	}

	private MaverickState getShootState(bool isAI) {
		var shootState = new MShoot((Point pos, int xDir) => {
			playSound("energyBall", sendRpc: true);
			new ArmoredAProj(projWeapon, pos, xDir, player, player.getNextActorNetId(), rpc: true);
		}, null);
		if (isAI) {
			shootState.consecutiveData = new MaverickStateConsecutiveData(0, 2, 0.33f);
		}
		return shootState;
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Roll,
		Sword,
			Grab,
		RollArmorless,
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"armoreda_roll" => MeleeIds.Roll,
			"armoreda_dash_grab" => MeleeIds.Grab,
			"armoreda_sword" => MeleeIds.Sword,
			"armoreda_na_roll" => MeleeIds.RollArmorless,
			_ => MeleeIds.None
		});
	}

	// This can be called from a RPC, so make sure there is no character conditionals here.
	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Roll => new GenericMeleeProj(
				rollWeapon, pos, ProjIds.ArmoredARoll, player,
				3, Global.defFlinch, 45, addToLevel: addToLevel
			),
				MeleeIds.Sword => new GenericMeleeProj(
				rollWeapon, pos, ProjIds.NormalPush, player,
				3, 0, 45, addToLevel: addToLevel
			),
			MeleeIds.RollArmorless => new GenericMeleeProj(
				rollWeapon, pos, ProjIds.ArmoredARoll, player,
				3, Global.defFlinch, 45, addToLevel: addToLevel
			),
			MeleeIds.Grab => new GenericMeleeProj(
				weapon, pos, ProjIds.BBuffaloDrag, player,
				0, 0, addToLevel: addToLevel
			),
			_ => null
		};
	}
}

#region weapons
public class ArmoredAProjWeapon : Weapon {
	public ArmoredAProjWeapon() {
		index = (int)WeaponIds.ArmoredAProj;
		killFeedIndex = 95;
	}
}

public class ArmoredAChargeReleaseWeapon : Weapon {
	public ArmoredAChargeReleaseWeapon() {
		index = (int)WeaponIds.ArmoredAChargeRelease;
		killFeedIndex = 95;
	}
}

public class ArmoredARollWeapon : Weapon {
	public ArmoredARollWeapon() {
		index = (int)WeaponIds.ArmoredARoll;
		killFeedIndex = 95;
	}
}

#endregion

#region projectiles
public class ArmoredAProj : Projectile {
	public ArmoredAProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, Character hitChar = null, bool rpc = false) :
		base(weapon, pos, xDir, 200, 3, player, "armoreda_proj", Global.halfFlinch, 0.01f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ArmoredAProj;
		maxTime = 0.7f;
		fadeSprite = "armoreda_proj_fade";

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
	}
}

public class ArmoredAChargeReleaseProj : Projectile {
	public ArmoredAChargeReleaseProj(Weapon weapon, Point pos, int xDir, float byteAngle, float damage, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 400, 4, player, "armoreda_proj_release", Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer) {
		byteAngle = byteAngle % 256;
		vel.x = 400 * Helpers.cosb(byteAngle);
		vel.y = 400 * Helpers.sinb(byteAngle);
		this.byteAngle = byteAngle;
		projId = (int)ProjIds.ArmoredAChargeRelease;
		maxTime = 0.5f;
		damager.damage = damage;

		if (rpc) {
			rpcCreateByteAngle(pos, player, netProjId, byteAngle);
		}
	}


	

	public override void update() {
		base.update();
		if (time > 0.4f && ownedByLocalPlayer){
		destroySelf();
		new TorpedoProj(new ArmoredAChargeReleaseWeapon(), pos, xDir, owner, 6, owner.getNextActorNetId(), 0, rpc: true);
		}
	}
}
#endregion

#region states
public class ArmoredAGuardState : MaverickState {
	public ArmoredAGuardState() : base("block") {
		aiAttackCtrl = true;
		canBeCanceled = false;
	}

	public override void update() {
		base.update();
		if (player == null) return;



		if (input.isHeld(Control.Down, player)){
		maverick.changeState(new ArmoredAGuardChargeState(3));
		}


		if (input.isHeld(Control.Left, player)) maverick.xDir = -1;
		else if (input.isHeld(Control.Right, player)) maverick.xDir = 1;

		if (maverick is ArmoredArmadillo aa && aa.noArmor) {
			maverick.changeState(new MIdle());
			return;
		}

		if (!player.isAI &&
			
			maverick.aiBehavior == MaverickAIBehavior.Control && !player.input.isHeld(Control.Special1, player)) {
			maverick.changeState(new MIdle());
			return;
		}

		if (maverick.aiBehavior != MaverickAIBehavior.Control || player.isAI) {
			if (stateTime > 1) {
				maverick.changeState(new MIdle());
				return;
			}
		}

		if (maverick.ammo <= 0) {
			maverick.changeToIdleOrFall();
			return;
		}
	}

	public override bool canEnter(Maverick maverick) {
		return base.canEnter(maverick) && maverick is ArmoredArmadillo aa && !aa.noArmor;
	}
}

public class ArmoredAGuardChargeState : MaverickState {
	float damage;
	public ArmoredAGuardChargeState(float damage) : base("charge") {
		this.damage = damage;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.playSound("4earmoredaCharge", sendRpc: true);
		if (maverick.ownedByLocalPlayer && player == Global.level.mainPlayer) {
			new GigaCrushBackwallMVK(maverick.pos, maverick);
		}
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (stateTime > 1.7f) {
			maverick.changeState(new ArmoredAGuardReleaseState(damage));
		}
	}
}

public class ArmoredAGuardReleaseState : MaverickState {
	float damage;
	public ArmoredAGuardReleaseState(float damage) : base("release") {
		this.damage = damage;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.shakeCamera(sendRpc: true);
		maverick.playSound("armoredaRelease", sendRpc: true);
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (!once && maverick.frameIndex >= 2) {
			once = true;
			for (int i = 256; i >= 0; i -= 32) {
				new ArmoredAChargeReleaseProj((maverick as ArmoredArmadillo).chargeReleaseWeapon,
				 maverick.getCenterPos(), 1, i, damage, player, player.getNextActorNetId(), rpc: true);
			}
		}	

		if (maverick.isAnimOver()) {
			maverick.changeState(new MIdle());
		}
	}
	
}





public class ArmoredAGuardChargeStateMAX : MaverickState {
	float damage;
	public ArmoredAGuardChargeStateMAX(float damage) : base("charge") {
		this.damage = damage;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.playSound("4earmoredaCharge", sendRpc: true);
		if (maverick.ownedByLocalPlayer && player == Global.level.mainPlayer) {
			new GigaCrushBackwallMVK(maverick.pos, maverick);
		}
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (stateTime > 1.7f) {
			maverick.changeState(new ArmoredAGuardReleaseStateMAX(6));
		}
	}
}

public class ArmoredAGuardReleaseStateMAX : MaverickState {
	float damage;

	float resetCD;

	int resetTimes = 0;

	
	public RekkohaEffect? effect;



	public ArmoredAGuardReleaseStateMAX(float damage) : base("release") {
		this.damage = damage;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
			effect = new RekkohaEffect();
		
	}

	public override void update() {
		base.update();
		Helpers.decrementTime(ref resetCD);
		if (player == null) return;

		if (resetCD == 0 && maverick.frameIndex >= 2) {
			once = true;
		maverick.shakeCamera(sendRpc: true);
		maverick.playSound("armoredaRelease", sendRpc: true);
		 resetCD = 0.11f;
			for (int i = 256; i >= 0; i -= 32) {
				new ArmoredAChargeReleaseProj((maverick as ArmoredArmadillo).chargeReleaseWeapon,
				 maverick.getCenterPos(), 1, i, damage, player, player.getNextActorNetId(), rpc: true);
			}
		}	

	

		if (maverick.isAnimOver()) {
			maverick.changeState(new MIdle());
		}
	}
	
}





public class ArmoredASword : MaverickState {
	public ArmoredASword() : base("sword") {
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.playSound("sigma2slash", sendRpc: true);
	}

	public override void update() {
		base.update();
		if (player == null) return;
		if (maverick.isAnimOver()) {
			maverick.changeState(new MIdle());
		}
	}
	
}



public class ArmoredAZappedState : MaverickState {
	Point pushDir;
	public ArmoredAZappedState() : base("zapped") {
		canEnterSelf = false;
		aiAttackCtrl = true;
		canBeCanceled = false;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		if (stateTime < 0.33f) {
			maverick.move(pushDir);
		}

		if (stateTime > 1f) {
			//(maverick as ArmoredArmadillo).removeArmor(true);
			maverick.changeToIdleOrFall();
		}
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.playSound("armoredaZap", sendRpc: true);
		pushDir = new Point(-maverick.xDir * 75, 0);
		maverick.vel.y = -100;
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
		/*if (maverick is ArmoredArmadillo aa && !aa.noArmor) {
			aa.removeArmor(true);
		}*/
	}
}

public class ArmoredARollEnterState : MaverickState {
	public ArmoredARollEnterState() : base("roll_enter") {
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.vel.y = -ArmoredArmadillo.rollTransJumpPower;
		maverick.frameSpeed = 0;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		maverick.stopCeiling();

		if (maverick.vel.y > 0) {
			maverick.frameSpeed = 1;
		}
		if (maverick.grounded && stateFrame >= 2) {
			maverick.changeState(new ArmoredARollState());
		}
	}
}

public class ArmoredARollState : MaverickState {
	public Point rollDir;
	const float rollSpeed = 300;
	public int bounceCount;
	float rollDirTime;
	const float jumpPower = 350;
	float jumpHeldTime;

	bool isScrapBuffed;

	bool AIJump;
	public ArmoredARollState() : base("roll") {
	}

	public override void update() {
		base.update();
		if (player == null) return;




		
		if (input.isPressed(Control.Dash, player)) {
			maverick.changeState(new ArmoredARollExitState());
			return;
		}
		

		if (input.isPressed(Control.Jump, player) && maverick.grounded 
		|| stateTime > 1 && AIJump) {
			jumpHeldTime = Global.spf;
			maverick.vel.y = -250;
		}

		if (jumpHeldTime > 0) {
			if (!input.isHeld(Control.Jump, player) || stateTime > 3 && AIJump) {
				jumpHeldTime = 0;
			} else {
				jumpHeldTime += Global.spf;
				maverick.vel.y = -250;
				if (jumpHeldTime > 0.25f) {
					jumpHeldTime = 0;
				}
			}
		}

		Point moveAmount = rollDir.times(rollSpeed * Global.spf);
		float moveY = moveAmount.y + (maverick.vel.y * Global.spf);
		CollideData hit = Global.level.checkTerrainCollisionOnce(maverick, moveAmount.x, moveY - 2, autoVel: true);
		Point? newRollDir = null;
		bool stopBouncing = false;
		if (hit != null) {
			Point normal = hit.getNormalSafe();

			var ceilingHit = Global.level.checkTerrainCollisionOnce(maverick, 0, moveY, autoVel: true);
			if (ceilingHit != null) {
				normal = new Point(0, 1);
				if (isScrapBuffed){
					new RollingShieldProj(new RollingShield(), maverick.pos, maverick.xDir, player, player.getNextActorNetId(), true);	
				}
			}

			if (!normal.isAngled()) {
				// Sideways wall
				if (normal.x != 0) {
					maverick.xDir *= -1;
					newRollDir = new Point(maverick.xDir, 0).normalize();
					// maverick.vel.y = 0;
					if ((input.isHeld(Control.Jump, player) || stateTime > 1 && AIJump) && maverick.vel.y <= 0) {
						jumpHeldTime = Global.spf;
						maverick.vel.y = -250;
					}
				} else {
					newRollDir = new Point(maverick.xDir, 0).normalize();
					// Bottom wall
					if (normal.y < 0 && bounceCount > 8) {
						stopBouncing = true;
					}
					// Top wall
					if (normal.y > 0) {
						jumpHeldTime = 0;
						maverick.vel.y = 0;
					}
				}
			}
		}

		if (newRollDir != null) {
			bounceCount++;
			rollDirTime = 0;
			rollDir = newRollDir.Value;
			maverick.playSound("armoredaCrash", sendRpc: true);
			if (isScrapBuffed){
					new RollingShieldProj(new RollingShield(), maverick.pos, maverick.xDir, player, player.getNextActorNetId(), true);	
				}
			maverick.shakeCamera(sendRpc: true);
		} else {
			maverick.move(moveAmount, false);
			rollDirTime += Global.spf;
		}

		if (stopBouncing || maverick.ammo <= 0) {
			maverick.changeState(new ArmoredARollExitState());
		}
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		rollDir = new Point(maverick.xDir, 0);

		if (Helpers.randomRange(0,5) == 5 && player.isAI){
			AIJump = true;
		}
		if (player.input.isHeld(Control.Special2, player) && player.currency > 0
		|| player.isAI && player.bonusHealth <= 0){
		player.currency -= 1;
		isScrapBuffed = true;
		}
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
	}
}

public class ArmoredARollExitState : MaverickState {
	public ArmoredARollExitState() : base("roll_exit") {
		aiAttackCtrl = true;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.vel.y = -ArmoredArmadillo.rollTransJumpPower;
		maverick.frameSpeed = 0;
		new RollingShieldProj(new RollingShield(), maverick.pos, maverick.xDir, player, player.getNextActorNetId(), true);	
		new RollingShieldProj(new RollingShield(), maverick.pos, -maverick.xDir, player, player.getNextActorNetId(), true);	
		maverick.playSound("crash", sendRpc: true);
					
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
	}

	public override void update() {
		base.update();
		if (player == null) return;

		maverick.stopCeiling();
		if (maverick.vel.y > 0) {
			maverick.frameSpeed = 1;
		}
		if (maverick.grounded) {
			maverick.changeState(new MIdle());
		}
	}
}
#endregion
