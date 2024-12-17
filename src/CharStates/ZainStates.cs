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
			counterAttackTarget = gmp.owningActor;
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
		if (damagingActor is not GenericMeleeProj) {
			return false;
		}
		return character.frameIndex == 0;
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
		character.parryCooldown = character.maxParryCooldown;
	}
}



public class ZainProjSwingState : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	bool once;
	public ZainProjSwingState(
		bool grounded, bool shootProj
	) : base(
		grounded ? "projswing" : "projswing_air", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "projswing";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		

		 if (base.player.input.isHeld("up", base.player)
		 && character is Zain zain && zain.ZainCounters > 0){
		    character.changeSpriteFromName("rising", true);
			character.dashedInAir++;
			float ySpeedMod = 0.5f;
			if (!once){
				once = true;
				zain.ZainCounters -= 1;
			}
			character.vel.y = (0f - character.getJumpPower()) * ySpeedMod;
		}

		 if (base.player.input.isHeld("down", base.player)
		 && character is Zain zain2 && zain2.ZainCounters > 0){
		    character.changeSpriteFromName("thrust", true);
			if (!once){
				once = true;
				zain2.ZainCounters -= 1;
			}
		}

		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("ZeroSaberX3", forcePlay: false, sendRpc: true);
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



public class ZainSaberProj : Projectile {
	public ZainSaberProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 300, 12, player, "zain_projslash", 60, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		fadeSprite = "zsaber_shot_fade";
		reflectable = true;
		projId = (int)ProjIds.ZainSaberProj;
	
		
	
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		
		if (time > 0.5) {
			destroySelf(fadeSprite);
		}
	}
}
