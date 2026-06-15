namespace MMXOnline;

public class KurumaGrab : Weapon {
	public KurumaGrab() : base() {
		fireRate = 45;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}



public class KurumitoGrabStartState : CharState {


	public KurumitoGrabStartState() : base("grab_start") {

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
	public override void onExit(CharState? newState) {
		base.onExit(newState);

	}

}



public class KurumaGrabState : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;

	public bool UsedGrabFinisherOnce;
	float timeWaiting;

	public KurumaGrabState(Character? victim) : base("grab") {
		this.victim = victim;
		grabTime = 1;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
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
			character.changeSpriteFromName("grab_up", true);
		}

		if (player.input.isPressed(Control.Down, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			character.changeSpriteFromName("grab_down", true);
		}
		
			if ((player.input.isPressed(Control.Left, player)
			|| player.input.isPressed(Control.Right, player))&& !UsedGrabFinisherOnce) {
			character.turnToInput(player.input,player);
			UsedGrabFinisherOnce = true;
			character.changeSpriteFromName("grab_foward", true);
		}


		if (character.sprite.name.Contains("attack") && character.frameIndex == 2) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 1, 0, 2);

				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.SelfDmg);
			}
		}
		
		if (character.sprite.name.Contains("up") && character.frameIndex == 1) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 3, 25, 2);
					new Anim(victim.pos, "explosion", 1, player.getNextActorNetId(), true, sendRpc: true, character.ownedByLocalPlayer);
				character.playSound("explosionX3", sendRpc: true);
				character.shakeCamera(sendRpc: true);
				victim.shakeCamera(sendRpc: true);
				damager.applyDamage(victim, false, new FireWave(), character, (int)ProjIds.Ryuenjin);
			}
		}

		if (character.sprite.name.Contains("down") && character.frameIndex == 1) {
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

		if (character.sprite.name.Contains("foward") && character.frameIndex == 1) {
			if (leechTime > 0.3f) {
				leechTime = 0;
				var damager = new Damager(player, 3, 20, 2);
				damager.applyDamage(victim, false, new FireWave(), character, (int)ProjIds.UPPunch);
			}
		}



		if ((character.sprite.name.Contains("up"))
		&& character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

		
		if ((character.sprite.name.Contains("up")
		|| character.sprite.name.Contains("down")
		|| character.sprite.name.Contains("foward")
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
	
		if (victim != null &&

		!character.sprite.name.Contains("up") &&
		 !character.sprite.name.Contains("down") &&
		 !character.sprite.name.Contains("foward")) {
			victim.grabInvulnTime = 2;
			victim.stunInvulnTime = 1;
			victim?.releaseGrab(character, true);
		}
		specialId = SpecialStateIds.None;
	}
}

public class KurumaGrabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;
	public KurumaGrabbed(Character? grabber) : base(grabber, maxGrabTime, "") {
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


