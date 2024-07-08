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

	public HyperZeroStart() : base("hyper_start") {
		invincible = true;
	}

	public override void update() {
		base.update();
		if (virusAnimName != "") {
			int animCount = 0;
			for (int i = 0; i < virusAnim.Length; i++) {
				if (virusAnim[i] != null) {
					if (virusAnim[i].pos == character.getCenterPos()) {
						virusAnim[i].destroySelf();
					}
					if (virusAnim[i].destroyed) {
						character.playSound("shingetsurinx5", true);
						if (stateFrames > 55) {
							virusAnim[i] = null!;
							continue;
						}
						virusAnim[i] = virusAnim[i] = createVirusAnim();
					} else {
						animCount++;
					}
					virusAnim[i].moveToPos(character.getCenterPos(), 300);
					if (virusAnim[i].pos.distanceTo(character.getCenterPos()) < 10) {
						virusAnim[i].destroySelf();
					}
				} else if (delayedVirusTimer[i] > 0) {
					delayedVirusTimer[i] -= Global.speedMul;
					if (delayedVirusTimer[i] <= 0) {
						delayedVirusTimer[i] = 0;
						virusAnim[i] = createVirusAnim();
					}
				}
				if (animCount == 0 && stateFrames > 55 && virusEffectParts != null) {
					virusEffectParts.destroySelf();
				}
			}
		}
		if (time == 0) {
			if (radius >= 0) {
				radius -= Global.spf * 200;
			} else {
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
			if (time >= 1) {
				character.changeToIdleOrFall();
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		zero = character as Zero ?? throw new NullReferenceException();
		character.useGravity = false;
		character.vel = new Point();
		if (zero == null) {
			throw new NullReferenceException();
		}
		character.player.currency -= 10;
		if (zero.hyperMode == 2) {
			zero.changeSpriteFromName("hyper_viral", true);
			virusAnimName = "sigmavirushead";
			virusAnim[0] = createVirusAnim();
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
		if (character != null) {
			character.invulnTime = 0.5f;
		}
		if (zero.isAwakened || zero.isBlack) {
			zero.hyperModeTimer = 20 * 60;
		}
		virusEffectParts?.destroySelf();
		bool playedHitSound = false;
		for (int i = 0; i < virusAnim.Length; i++) {
			if (virusAnim[i]?.destroyed == false) {
				if (!playedHitSound) {
					character?.playSound("shingetsurinx5", true);
					playedHitSound = true;
				}
				virusAnim[i]?.destroySelf();
			}
		}
	}

	public Anim createVirusAnim() {
		float newAngle = Helpers.randomRange(0, 359);
		Point newPos = new(Helpers.cosd(newAngle) * 100, Helpers.sind(newAngle) * 100);
		float diplayAngle = newAngle;
		int xDir = -1;
		if (virusAnimName == "sigmavirushead") {
			xDir = 1;
		}
		if (diplayAngle > 90 && diplayAngle < 270) {
			diplayAngle = (diplayAngle + 180f) % 360f;
			xDir *= -1;
		}
		return new Anim(
			character.getCenterPos() + newPos,
			virusAnimName, xDir,
			player.getNextActorNetId(), false, sendRpc: true
		) {
			angle = diplayAngle
		};
	}

	public void activateHypermode() {
		if (zero.hyperMode == 1) {
			zero.awakenedPhase = 1;
			float storedAmmo = zero.gigaAttack.ammo;
			zero.gigaAttack = new ShinMessenkou();
			zero.gigaAttack.ammo = storedAmmo;
		} else if (zero.hyperMode == 2) {
			zero.isViral = true;
			float storedAmmo = zero.gigaAttack.ammo;
			zero.gigaAttack = new DarkHoldWeapon();
			zero.gigaAttack.ammo = storedAmmo;
			zero.freeBusterShots = 10;
		} else {
			zero.isBlack = true;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		Point pos = character.getCenterPos();
		if (zero.hyperMode == 0) {
			DrawWrappers.DrawCircle(
				pos.x + x, pos.y + y, radius, false, Color.White, 5, character.zIndex + 1, true, Color.White
			);
		}
	}
}

public class SaberParryStartState : CharState {
	public SaberParryStartState() : base("parry_start", "", "", "") {
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
	if (character is Zero zero && zero.isNightmareZero){
		zero.freeBusterShots += 1;
	}
	
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
}
