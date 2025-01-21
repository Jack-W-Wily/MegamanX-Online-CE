using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;

namespace MMXOnline;



public class GlobalParryState : CharState {
	public GlobalParryState() : base("parry_start", "", "", "") {
		superArmor = true;
		airMove = true;
	}

	public override void update() {
		base.update();

		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			character.parryCooldown = 30;
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
			if (player.isVile){
			if (!chr.ownedByLocalPlayer) {
				RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
			} else {
				chr.changeState(new ParriedState(), true);
			}
			character.addHealth(1);
			}
		}
		character.playSound("zeroParry", forcePlay: false, sendRpc: true);	

		if (Helpers.randomRange(0,5) == 5){
			character.addHealth(1);
			character.changeState(new ParriedState(), true);
		}
		character.changeState(new Idle(), true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}

	public bool canParry(Actor damagingActor) {
		if (damagingActor is not Projectile) {
			return false;
		}
		if (player.isVile)return character.frameIndex < 5;
		if (player.isDragoon)return character.frameIndex < 5;
		
		return character.frameIndex == 0;
	}

		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (player.isX || player.isXAnother) {
		character.changeSpriteFromName("unpo_parry_start", true);
		}
		//if (player.isVile){
		//character.changeSpriteFromName("win", true);
		//}
		}
	
}


public class XTeleportState : CharState {
	public bool onceTeleportInSound;
	bool isInvisible;
	Actor? clone;
	Actor? cloneG;
	Rect teleportCollider = new Rect(0f, 0f, 18, 30);
	int width = 18;

	public XTeleportState() : base("land") {
	}

	public override void update() {
		base.update();

		if (!isInvisible && stateFrames >= 12 && stateFrames < 18) {
			isInvisible = true;
			specialId = SpecialStateIds.XTeleport;
			character.useGravity = false;
		}
		if (isInvisible && stateFrames >= 18) {
			isInvisible = false;
			specialId = SpecialStateIds.None;
			character.useGravity = true;
			if (cloneG != null && canChangePos(cloneG)) {
				Point prevCamPos = player.character.getCamCenterPos();
				player.character.stopCamUpdate = true;
				character.changePos(cloneG.pos);
			}
			clone?.destroySelf();
			clone = null;
		}
		if (clone != null && !clone.destroyed) {
			int xDir = player.input.getXDir(player);
			float moveAmount = xDir * 6 * Global.speedMul;

			CollideData hitWall = Global.level.checkTerrainCollisionOnce(clone, moveAmount, -2);
			if (hitWall != null && hitWall.getNormalSafe().y == 0) {
				float rectW = hitWall.otherCollider.shape.getRect().w();
				if (rectW < 75) {
					float wallClipAmount = moveAmount + xDir * (rectW + width);
					CollideData hitWall2 = Global.level.checkTerrainCollisionOnce(clone, wallClipAmount, -2);
					if (hitWall2 == null && clone.pos.x + wallClipAmount > 0 &&
						clone.pos.x + wallClipAmount < Global.level.width
					) {
						clone.incPos(new Point(wallClipAmount, 0));
						clone.visible = true;
					}
				} else if (xDir != 0) {
					CollideData hitWall2 = Global.level.checkTerrainCollisionOnce(clone, moveAmount, -16);
					float wallY = MathInt.Floor(hitWall.otherCollider.shape.minY);
					if (hitWall2 == null) {
						clone.changePos(new Point(clone.pos.x + moveAmount, clone.pos.y - 64));
						clone.visible = true;
					} else if (clone.pos.y - wallY <= 36 && clone.pos.y - wallY > 0) {
						clone.changePos(new Point(clone.pos.x + moveAmount, wallY - 1));
						clone.visible = true;
					}
				}
			} else {
				if (MathF.Abs(moveAmount) > 0) {
					clone.visible = true;
				}
				clone.move(new Point(moveAmount, 0), useDeltaTime: false);
			}
			if (!canChangePos(clone)) {
				int widthH = MathInt.Ceiling(width / 2.0);
				List<CollideData> hits = Global.level.raycastAllSorted(
					clone.getCenterPos().addxy(-widthH, 0),
					clone.getCenterPos().addxy(-widthH, 200),
					new List<Type> { typeof(Wall) }
				);
				List<CollideData> hits2 = Global.level.raycastAllSorted(
					clone.getCenterPos().addxy(widthH, 0),
					clone.getCenterPos().addxy(widthH, 200),
					new List<Type> { typeof(Wall) }
				);
				CollideData hit = hits.FirstOrDefault();
				CollideData hit2 = hits2.FirstOrDefault();
				if (hit != null && (
					hit2 == null ||
					hit2 != null && hit.otherCollider.shape.minY < hit2.otherCollider.shape.minY
				)) {
					cloneG.visible = true;
					clone.visible = false;
					cloneG.changePos(new Point(clone.pos.x, hit.getHitPointSafe().y));
				} else if (hit2 != null) {
					cloneG.visible = true;
					clone.visible = false;
					cloneG.changePos(new Point(clone.pos.x, hit2.getHitPointSafe().y));
				}
			} else {
				cloneG.visible = false;
				cloneG.changePos(clone.pos);
			}
			if (!canChangePos(clone) && !canChangePos(cloneG)) {
				Point redXPos;
				if (cloneG.visible) {
					redXPos = cloneG.getCenterPos();
				} else {
					redXPos = clone.getCenterPos();
				}
				DrawWrappers.DrawLine(
					redXPos.x - 10, redXPos.y - 10, redXPos.x + 10, redXPos.y + 10, Color.Red, 2, ZIndex.HUD
				);
				DrawWrappers.DrawLine(
					redXPos.x - 10, redXPos.y + 10, redXPos.x + 10, redXPos.y - 10, Color.Red, 2, ZIndex.HUD
				);
			}
		}

		if (stateFrames < 12) {
			character.visible = Global.isOnFrameCycle(5);
		} else if (stateFrames >= 12 && stateFrames < 18) {
			character.visible = false;
		} else if (stateFrames >= 18) {
			if (!onceTeleportInSound) {
				onceTeleportInSound = true;
				character.playSound("boomerkTeleport", sendRpc: true);
			}
			character.visible = Global.isOnFrameCycle(5);
		}
		if (stateFrames >= 30) {
			character.changeToIdleOrFall();
		}
	}
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("boomerkTeleport", sendRpc: true);
		clone = createClone();
		clone.useGravity = true;
		cloneG = createClone();
		character.sprite.frameIndex = 1;
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.visible = true;
		character.useGravity = true;
		if (clone != null) {
			clone.destroySelf();
		}
		if (cloneG != null) {
			cloneG.destroySelf();
		}
	}
	public bool canChangePos(Actor actor) {
		if (Global.level.checkTerrainCollisionOnce(actor, 0, 2) == null) {
			return false;
		}
		List<CollideData> hits = Global.level.getTerrainTriggerList(actor, new Point(0, 2), typeof(KillZone));
		if (hits.Count > 0) {
			return false;
		}
		return true;
	}
	public Actor createClone() {
		Actor tempClone = new Actor(
			"empty",
			new Point(MathInt.Round(character.pos.x), MathInt.Floor(character.pos.y)),
			null, true, false
		);
		Collider col = new Collider(
			teleportCollider.getPoints(), false, tempClone, false, false, 0, new Point(0, 0)
		);
		tempClone.changeSprite("mmx_land", false);
		tempClone.globalCollider = col;
		tempClone.alpha = 0.5f;
		tempClone.xDir = character.xDir;
		tempClone.visible = false;
		tempClone.useGravity = false;
		tempClone.sprite.frameIndex = 1;

		return tempClone;
	}
}





public class XIceSlide : CharState {
	Anim? proj;

	public XIceSlide() : base("sice_slide", "", "", "") {
		enterSound = "chillpSlide";
		immuneToWind = true;
	}

	public override void update() {
		base.update();


		character.move(new Point(character.xDir * 150, 0));

	  if (stateTime > 0.4f) {
			character.changeToIdleOrFall();
			return;
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




public class XlightKick : CharState {
	Anim? proj;

	public XlightKick() : base("kick_lightarmor", "", "", "") {
		enterSound = "dash";
		immuneToWind = true;
	}

	public override void update() {
		base.update();


		character.move(new Point(character.xDir * 50, 0));

	  if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
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





