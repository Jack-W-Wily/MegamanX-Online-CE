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

	public bool UsedGrabFinisherOnce;
	float timeWaiting;

	public Vava1GrabState(Character? victim) : base("grab") {
		this.victim = victim;
		specialId = SpecialStateIds.AxlRoll;
		grabTime = Vava1Grabbed.maxGrabTime;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		regenTime += Global.spf;
		leechTime += Global.spf;

		if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("_grabbed")
		) {
			//	character.changeToIdleOrFall();
			//	return;
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

		
		if (player.input.isAPressed(player) && !UsedGrabFinisherOnce) {

			character.changeSpriteFromName("grab_attack", true);
		}

		if (player.input.isPressed(Control.Up, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			character.changeSpriteFromName("deadlift", true);
		}

		if (player.input.isPressed(Control.Down, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			character.changeSpriteFromName("violentcrusher_grab", true);
		}
		
			if ((player.input.isPressed(Control.Left, player)
			|| player.input.isPressed(Control.Right, player))&& !UsedGrabFinisherOnce) {
			character.turnToInput(player.input,player);
			UsedGrabFinisherOnce = true;
			character.changeSpriteFromName("throw", true);
		}


		if (character.sprite.name.Contains("attack") && character.frameIndex == 2) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 1, 0, 2);

				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);
			}
		}
	

		if (character.sprite.name.Contains("violentcrusher_grab") && character.frameIndex == 1) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 3, 0, 2);
				damager.applyDamage(victim, false, new FireWave(), character,
				(int)ProjIds.MechFrogStompShockwave);
				new MechFrogStompShockwave(new FireWave(),
				character.pos.addxy(6 * victim.xDir, 0f), victim.xDir, player,
				player.getNextActorNetId(), rpc: true);
				victim.changeState(new KnockedDown(victim.pos.x < character?.pos.x ? -1 : 1), true);
				victim.playSound("crash", true);
			}
		}

		if (character.sprite.name.Contains("throw") && character.frameIndex == 1) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 3, 20, 2);
				damager.applyDamage(victim, false, new FireWave(), character, (int)ProjIds.UPPunch);
			}
		}

		if (regenTime > 0.4f) {
			regenTime = 0;
			character.addHealth(0.5f);
			//damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);
		}



		if ((character.sprite.name.Contains("up"))
		&& character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

		
		if ((character.sprite.name.Contains("up")
		|| character.sprite.name.Contains("violentcrusher_grab")
		|| character.sprite.name.Contains("throw")
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

			if (character.sprite.name.Contains("deadlift")) {
				new VileQuickHomesick(
			character.pos.addxy(0, -30), character.xDir, character, player,
			player.getNextActorNetId(), rpc: true
			);
				character.invulnTime = 0.2f;
			}
		if (newState is not VileMK2GrabState && victim != null &&

		!character.sprite.name.Contains("up") &&
		 !character.sprite.name.Contains("violentcrusher_grab") &&
		 !character.sprite.name.Contains("throw")) {
			victim.grabInvulnTime = 2;
			victim.stunInvulnTime = 1;
			victim?.releaseGrab(character, true);
		}
		specialId = SpecialStateIds.None;
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
	public const float maxGrabTime = 4;
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

		grabTime -= player.mashValue();
		if (grabTime <= 0) {
			character.changeToIdleOrFall();
		}




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



