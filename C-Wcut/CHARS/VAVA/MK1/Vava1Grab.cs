namespace MMXOnline;


public class Vava1Grab : Weapon {
	public Vava1Grab() : base() {
		fireRate = 45;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}



public class Vava1GrabStartState : CharState {


	public Vava1GrabStartState() : base("spring_grab") {

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
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}



public class Vava1GrabState : CharState {
	public Character? victim;
	float leechTime = 1;

	float regenTime = 1;
	public bool victimWasGrabbedSpriteOnce;

	public bool UsedGrabFinisherOnce = false;
	float timeWaiting;

	public Vava1GrabState(Character? victim) : base("grab") {
		this.victim = victim;
		grabTime = Vava1Grabbed.maxGrabTime;
	}

public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		if (character.xDir == -1) {
			victim.xDir = 1;
		} else {
			victim.xDir = -1;
		}
		if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("_grabbed")
		) {
			//	character.changeToIdleOrFall();
			//	return;
		}

		if (victim == null || victim.health <= 0) {
			character.changeToIdleOrFall();
		}

		if (victim.sprite.name.EndsWith("_grabbed") || victim.sprite.name.EndsWith("_die")) {
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}

		
//		if (player.input.isAPressed(player) && !UsedGrabFinisherOnce) {
//
	//		character.changeSpriteFromName("grab_attack", true);
	//	}

		if (player.input.isPressed(Control.Up, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			sprite = "deadlift";
			character.changeSpriteFromNameIfDifferent("deadlift", true);
		}

		if (player.input.isPressed(Control.Down, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			if (character.xDir == 1) {
				character.xDir = -1;
			} else {
				character.xDir = 1;
			}
			sprite = "violentcrusher_grab";
			character.changeSpriteFromNameIfDifferent("violentcrusher_grab", true);
		}
		
		if ((player.input.isPressed(Control.Left, player)
			|| player.input.isPressed(Control.Right, player))&& !UsedGrabFinisherOnce) {
			character.turnToInput(player.input,player);
			UsedGrabFinisherOnce = true;
			sprite = "superkick";
			character.changeSpriteFromNameIfDifferent("superkick", true);
		}
		
		if (character.sprite.name.Contains("deadlift") && character.frameIndex == 2) {

			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 2, 0, 60);
			character.shakeCamera(sendRpc: true);
				victim?.shakeCamera(sendRpc: true);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.HeavyPush);
			}
		}

		if (character.sprite.name.Contains("violentcrusher_grab") && character.frameIndex == 3) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				new MechFrogStompShockwave(new FireWave(),
				victim.pos.addxy(30 * victim.xDir, 0f), victim.xDir, player,
				player.getNextActorNetId(), rpc: true);
				victim.playSound("crash", true);
			}
		}

		if (character.sprite.name.Contains("superkick") && character.frameIndex == 2) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 2, 0, 0);
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.HeavyPush);
			}
		}



	

		
		if ((character.sprite.name.Contains("deadlift")
		|| character.sprite.name.Contains("violentcrusher_grab")
		|| character.sprite.name.Contains("superkick")
		)
		&& character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


		if (player.input.isBPressed(player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		if (character is Vile vile) {
			vile.grabCooldown = 1;
		}
		if (newState is not VileMK2GrabState && victim != null &&

		!character.sprite.name.Contains("up") &&
		 !character.sprite.name.Contains("down")&&
		 !character.sprite.name.Contains("foward")	) {
			victim.grabInvulnTime = 2;
			victim.stunInvulnTime = 1;
			victim?.releaseGrab(character, true);
		}
	}
}

public class Vava1Grabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;
	public Vava1Grabbed(Character? grabber) : base(grabber, maxGrabTime, "") {
	}


	public override void update() {
		trySnapToGrabPoint(true);
		if (grabber.sprite.name.Contains("idle") ||
		grabber.sprite.name.Contains("crouch") ||
		grabber.sprite.name.Contains("run") ||
		grabber.sprite.name.Contains("hurt") ||
		grabber.sprite.name.Contains("grabbed")
	
		) {
			character.changeToIdleOrFall();
		}
	}
}



public class VileStomped : CharState {
	public const float maxGrabTime = 10;
	public Character? grabber;
	public long savedZIndex;
	public VileStomped(Character? grabber) : base("knocked_down") {
		this.grabber = grabber;
	}

	public override bool canEnter(Character character) {
		if (!base.canEnter(character)) return false;
		return !character.isInvulnerable() && !character.charState.invincible;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.stopCharge();
		savedZIndex = character.zIndex;
		character.setzIndex(grabber.zIndex - 100);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.grabInvulnTime = 0.5f;
		character.setzIndex(savedZIndex);
	}

	public override void update() {
		base.update();

		



			if (grabber.sprite.name.Contains("idle") ||
			grabber.sprite.name.Contains("crouch") ||
			grabber.sprite.name.Contains("run") ||
			grabber.sprite.name.Contains("fall") ||
			grabber.sprite.name.Contains("jump") ||
			grabber.sprite.name.Contains("hurt") ||
			grabber.sprite.name.Contains("grabbed")

			) {
				character.changeToIdleOrFall();
			}
		
	}
}



