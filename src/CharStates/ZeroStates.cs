using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;

public class HyperZeroStart : CharState {
	public float radius = 200;
	public float time;
	[AllowNull]
	Anim? ZeroVirus;
	Anim? VirusEffect;
	Anim? VirusEffectParts;
	Anim? VirusHead;
	Anim? DrLight;
	Anim? drWilyAnim;

	[AllowNull]
	Zero zero;

	public HyperZeroStart(int type) : base(
		type == 2 ? "hyper_start3" : 
		type == 1 ? "hyper_start2" : 
		type == 0 ? "hyper_start" : "", "", "") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (time == 0) {
			if (radius >= 0) {
				radius -= Global.spf * 200;
			} else if (character is Zero zero) {
				time = Global.spf;
				radius = 0;
				if (player.isZBusterZero()) {
					zero.blackZeroTime = 9999;
					RPC.actorToggle.sendRpc(character.netId, RPCActorToggleType.ActivateBlackZero2);
				} else if (zero.zeroHyperMode == 0) {
					zero.blackZeroTime = 9999;//zero.maxHyperZeroTime + 1;
					RPC.setHyperZeroTime.sendRpc(character.player.id, zero.blackZeroTime, 0);
				} else if (zero.zeroHyperMode == 1) {
					zero.awakenedZeroTime = 9999;//Global.spf;
					RPC.setHyperZeroTime.sendRpc(character.player.id, zero.awakenedZeroTime, 2);
				} else if (zero.zeroHyperMode == 2) {
					zero.zeroDarkHoldWeapon.ammo = zero.zeroGigaAttackWeapon.ammo;
					zero.isNightmareZero = true;
				}
				character.playSound("ching");
				character.fillHealthToMax();
			}
		} else {
			time += Global.spf;
			if (time > 1) {
				character.changeState(new Idle(), true);
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as Zero;

		character.useGravity = false;
		character.vel = new Point();
		if (zero == null) {
			throw new NullReferenceException();
		}

		if (zero.hyperZeroUsed) {
			return;
		}
		if (base.player.isZBusterZero() || zero.zeroHyperMode == 0) {
			character.player.currency -= 5;
		
			/*drWilyAnim = new Anim(
				
				character.pos.addxy(50 * character.xDir, 0f),
				"LightX3", -character.xDir,
				player.getNextActorNetId(),
				destroyOnEnd: false, sendRpc: true
			);*/
		//	drWilyAnim.fadeIn = true;
			//character.playSound("blackzeroentry", forcePlay: false, sendRpc: true);
		} else if (zero.zeroHyperMode == 1) {
		/*	ZeroVirus = new Anim(
				character.pos.addxy(70 , -10),
				"zerovirus", -character.xDir,
				player.getNextActorNetId(), false, sendRpc: true
			);*/
		//	drWilyAnim.fadeIn = true;
		//	drWilyAnim.blink = true;
			//character.player.awakenedCurrencyEnd = (character.player.currency - 10);
			character.playSound("AwakenedZeroEntry", forcePlay: false, sendRpc: true);
		} else if (zero.zeroHyperMode == 2) {
		/*	VirusEffect = new Anim(
				character.pos.addxy(character.xDir, +10),
				"viruseffect", character.xDir,
				player.getNextActorNetId(), true, sendRpc: true
			);
			VirusEffectParts = new Anim(
				character.pos.addxy(character.xDir, +10),
				"viruseffectparts", character.xDir,
				player.getNextActorNetId(), true, sendRpc: true
			);
			drWilyAnim.fadeIn = true;
			drWilyAnim.blink = true;*/
			character.player.currency -= 5;
			character.playSound("NightmareZeroEntry", forcePlay: false, sendRpc: true);
		}
		zero.hyperZeroUsed = true;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		ZeroVirus?.destroySelf();
		VirusEffect?.destroySelf();
		VirusEffectParts?.destroySelf();
		VirusHead?.destroySelf();
		DrLight?.destroySelf();
		character.invulnTime = 0.5f;
		zero.hyperZeroUsed = false;

		if (zero.zeroHyperMode == 0) {
			zero.blackZeroTime = zero.maxHyperZeroTime + 0.5f;
			RPC.setHyperZeroTime.sendRpc(zero.player.id, zero.blackZeroTime, 0);
		} else if (zero.zeroHyperMode == 1) {
			zero.awakenedCurrencyTime = -30;
			RPC.setHyperZeroTime.sendRpc(zero.player.id, zero.awakenedZeroTime, 2);
		} else if (zero.zeroHyperMode == 2) {
			zero.isNightmareZero = true;
			zero.freeBusterShots = 10;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		Point pos = character.getCenterPos();
		DrawWrappers.DrawCircle(pos.x + x, pos.y + y, radius, false, Color.White, 5, character.zIndex + 1, true, Color.White);
	}
}

public class KKnuckleParry : Weapon {
	public KKnuckleParry() : base() {
		rateOfFire = 0.75f;
		index = (int)WeaponIds.KKnuckleParry;
		killFeedIndex = 172;
	}
}

public class KKnuckleParryStartState : CharState {
	public KKnuckleParryStartState() : base("parry_start", "", "", "") {
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
	if (player.isZero)character.changeState(new KKnuckleParryMeleeState(counterAttackTarget), true);
	if (player.isZain)character.changeState(new KKnuckleParryMeleeState(counterAttackTarget), true);
	
	
	if (player.isX) {
		character.changeState(new Idle(), true);
		character.parryCooldown = 0;
		character.addHealth(1);
	}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		//character.parryCooldown = character.maxParryCooldown;
	}

	public bool canParry(Actor damagingActor) {
		if (damagingActor is not Projectile) {
			return false;
		}
		return character.frameIndex == 0;
	}
}

public class KKnuckleParryMeleeState : CharState {
	Actor? counterAttackTarget;
	Point counterAttackPos;
	public KKnuckleParryMeleeState(Actor? counterAttackTarget) : base("parry", "", "", "") {
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
