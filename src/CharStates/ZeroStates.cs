using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

namespace MMXOnline;
public class HyperZeroStart : CharState {
	public float radius = 200;
	public float time;
	Zero zero = null!;
	Anim? virusEffectParts;
	Anim[] virusAnim = new Anim[3];
	float[] delayedVirusTimer = {0, 7, 14};
	string virusAnimName = "";

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
				activateHypermode();
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
		character.player.currency -= 5;
	
		if (player.input.isHeld(Control.Up, player)) {
			zero.changeSpriteFromName("hyper_awakened", true);
			virusAnimName = "zerovirus";
			virusAnim[0] = createVirusAnim();
			virusEffectParts = new Anim(character.pos.addxy(0, 4), "viruseffect", -character.xDir, null, false);
			virusEffectParts.blink = true;
		} else {
		character.playSound("blackzeroentry", forcePlay: false, sendRpc: true);
			
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		if (character != null) {
			character.invulnTime = 0.5f;
		}
		if (zero.isAwakened || zero.isBlack) {
			zero.hyperModeTimer = Zero.maxBlackZeroTime + 30;
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
		if (player.input.isHeld(Control.Up, player)) {
			zero.awakenedPhase = 1;
			float storedAmmo = zero.gigaAttack.ammo;
			zero.gigaAttack = new ShinMessenkou();
			zero.gigaAttack.ammo = 32;
		} 
		if (player.input.isHeld(Control.Down, player)) {
			zero.isViral = true;
			float storedAmmo = zero.gigaAttack.ammo;
			zero.gigaAttack = new DarkHoldWeapon();
			zero.gigaAttack.ammo = 32;
		} if (!player.input.isHeld(Control.Down, player)
		    && !player.input.isHeld(Control.Up, player)) {
			zero.isBlack = true;
			zero.freeBusterShots = 10;
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
		if (Helpers.randomRange(0, 10) < 10) {
			character.playSound("zeroParry", forcePlay: false, sendRpc: true);
		} else {
			character.playSound("zeroParry2", forcePlay: false, sendRpc: true);
		}
		character.changeState(new KKnuckleParryMeleeState(counterAttackTarget), true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}

	public bool canParry(Actor damagingActor) {
		if (damagingActor is not Projectile) {
			return false;
		}
		return character.frameIndex == 0;
	}
}




public class ZeroFinalStart : CharState {
	Anim? proj;

	public ZeroFinalStart() : base("final_start", "", "", "") {
		invincible = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.isUnderwater() && proj != null) {
			proj.destroySelf();
			proj = null;
		}

		character.move(new Point(character.xDir * 350, 0));

		 if (stateTime > 0.6f) {
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




public class ZeroFinalEnd : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public ZeroFinalEnd(Character? victim) : base("final_end", "", "", "") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		//if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("knocked_down")) {
		//	character.changeToIdleOrFall();
		//	return;
		//}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.33f) {
					leechTime = 0;
				}
				return;
			}
		}

			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();
			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);
			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));

		
	if (!player.input.isHeld(Control.Down, player)
			&& player.input.isPressed(Control.Shoot, player)) {

			if (Helpers.randomRange(0,3) == 0 && leechTime > 0.05f){
			character.changeSpriteFromName("attack", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 1  && leechTime > 0.05f){
			character.changeSpriteFromName("attack_2", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 2 && leechTime > 0.05f){
			character.changeSpriteFromName("attack_air", true);
			leechTime = 0;
			}
			if (Helpers.randomRange(0,3) == 3 && leechTime > 0.05f){
			character.changeSpriteFromName("attack_3", true);
			leechTime = 0;
			}
			
		}

			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {
			character.changeSpriteFromName("hyouretsuzan_fall", true);	
		}


		

		if (player.input.isPressed(Control.Special1, player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.grabCooldown = 1;
		victim.grabInvulnTime = 0.5f;
		victim?.releaseGrab(character);
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
