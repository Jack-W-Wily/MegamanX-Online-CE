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
		character.parryCooldown = character.maxParryCooldown;
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
		if (player.input.isHeld(Control.Up, player)){
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
		bonusAttackCtrl = true;
	}


		public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		 if (base.player.input.isHeld("up", base.player)){
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
			character.stopMovingWeak();
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
		: base("groundstab", "", "", transitionSprite)
	{
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
		: base("stabgrab", "", "", transitionSprite)
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
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
			character.stopMovingWeak();
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}		
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}		

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
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
