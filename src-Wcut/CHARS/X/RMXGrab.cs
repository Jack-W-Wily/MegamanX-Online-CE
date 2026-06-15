namespace MMXOnline;

public class RMXGrab : Weapon {
	public RMXGrab() : base() {
		fireRate = 45;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}



public class RMXGrabStartState : CharState {


	public RMXGrabStartState() : base("grab_start") {

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



public class RMXGrabState : CharState {
	public Character? victim;
	public float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;

	public bool UsedGrabFinisherOnce;
	public float timeWaiting;
	public MegamanX? mmx = null;

	public bool unstick;

	public RMXGrabState(Character? victim) : base("grab") {
		this.victim = victim;
		grabTime = 1;
	}

	public override void update() {
		base.update();
		if (victim != null){
			grabTime -= Global.spf;
			leechTime += Global.spf;
			if (!character.sprite.name.Contains("light")){
			if (character.xDir == -1) {
				victim.xDir = 1;
			} else {
				victim.xDir = -1;
			}
			} else {
			if (character.xDir == -1) {
				victim.xDir = -1;
			} else {
				victim.xDir = 1;
			} 
			}
			if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("_grabbed")
			) {
				//	character.changeToIdleOrFall();
				//	return;
			}

			if (victim == null || victim.health <= 0) {
				character.changeToIdleOrFall();
			}

			if (victim != null && victim.sprite.name.EndsWith("_grabbed") || victim != null &&  victim.sprite.name.EndsWith("_die")) {
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
				if (character.xDir == 1) {
					character.xDir = -1;
				} else {
					character.xDir = 1;
				}
				character.changeSpriteFromName("grab_down", true);
			}
			
				if ((player.input.isPressed(Control.Left, player)
				|| player.input.isPressed(Control.Right, player))&& !UsedGrabFinisherOnce) {
				character.turnToInput(player.input,player);
				UsedGrabFinisherOnce = true;
				
				character.changeSpriteFromName("grab_foward", true);
				character.playSound("recoilRod2", true);
				
			}


			if (victim != null && character.sprite.name.Contains("attack") && character.frameIndex == 2) {
				if (leechTime > 0.3f) {
					leechTime = 0;
					var damager = new Damager(player, 1, 0, 60);

					damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.SelfDmg);
				}
			}
			
			if (character.sprite.name.Contains("up") && character.frameIndex == 0 && !unstick) {
				character.vel.y = -character.getJumpPower() * 1.5f;
				normalCtrl = true;
				unstick = true;
			}

			if (victim != null && character.sprite.name.Contains("down") && character.frameIndex == 8) {
				if (leechTime > 0.3f) {
					leechTime = 0;
					new MechFrogStompShockwave(new FireWave(),
					victim.pos.addxy(30 * victim.xDir, 0f), victim.xDir, player,
					player.getNextActorNetId(), rpc: true);
					victim.playSound("crash", true);
				}
			}

			if (character.sprite.name.Contains("foward") && character.frameIndex == 2) {
			
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
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		mmx = character as RockmanX;
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

public class RMXGrabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;
	public RMXGrabbed(Character? grabber) : base(grabber, maxGrabTime, "") {
	}


	public override void update() {
		trySnapToGrabPoint(true);
		if (grabber != null){
			if (grabber.sprite.name.Contains("idle") ||
			grabber.sprite.name.Contains("crouch") ||
			grabber.sprite.name.Contains("run") ||
			grabber.sprite.name.Contains("hurt") ||
			grabber.sprite.name.Contains("grabbed")
		
			) {
				character.changeToIdleOrFall();
			}
		} else {
			character.changeToIdleOrFall();
		}
	}
}


