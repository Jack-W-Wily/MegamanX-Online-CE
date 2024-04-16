namespace MMXOnline;

public abstract class MGrabbed : MaverickState {
	public Actor grabber;
	public long savedZIndex;
	public string grabSpriteSuffix;
	public bool reverseZIndex;
	public bool freeOnHitWall;
	public bool lerp;
	public bool freeOnGrabberLeave;
	public string additionalGrabSprite;
	public float notGrabbedTime;
	public float grabTime;
	public float maxNotGrabbedTime;
	public float extraYOffset = 0;
	public bool customUpdate;
	public float mashTimeDecay {
		get {
			if (maverick.isHeavy) {
				return Global.spf * heavyDecayMultiplier;
			}
			return Global.spf;
		}
	}
	public float heavyDecayMultiplier = 2;
	public int grabId;

	public MGrabbed(
		Actor grabber, float maxGrabTime, string grabSpriteSuffix, int projId,
		bool reverseZIndex = false, bool freeOnHitWall = true,
		bool lerp = true, string additionalGrabSprite = null,
		float maxNotGrabbedTime = 0.5f
	) : base(
		"hurt"
	) {
		this.grabber = grabber;
		grabTime = maxGrabTime;
		this.grabSpriteSuffix = grabSpriteSuffix;
		this.reverseZIndex = reverseZIndex;
		this.lerp = lerp;
		this.additionalGrabSprite = additionalGrabSprite;
		this.maxNotGrabbedTime = maxNotGrabbedTime;
		this.grabId = projId;
	}
 
	private int MaverickMash = 5;

	public override void update()
	{
		base.update();

		// Maverick mash System
		Global.level.gameMode.setHUDErrorMessage(
					player, Helpers.controlText("PRESS JUMP!!! (" + 
			MaverickMash + " )"), 
					playSound: false, resetCooldown: true
				);
		if (base.player != null)
		{
			if (base.player.input.isPressed("jump", base.player)){
			MaverickMash -= 1;
			}
			if (MaverickMash < 1){
			maverick.changeState(new MIdle());
			}
		}
		if (customUpdate) {
			return;
		}
		if (grabber?.sprite == null ||
			!grabber.sprite.name.EndsWith(grabSpriteSuffix) &&
			(string.IsNullOrEmpty(additionalGrabSprite) || !grabber.sprite.name.EndsWith(additionalGrabSprite))
		) {
			notGrabbedTime += Global.spf;
			if (notGrabbedTime > maxNotGrabbedTime) {
				exitGrab();
				return;
			}
		}
		if (!trySnapToGrabPoint(lerp) && lerp && freeOnHitWall) {
			exitGrab();
			return;
		}
		grabTime -= mashTimeDecay;
		if (grabTime <= 0f) {
			exitGrab();
		}
	}

	public virtual void exitGrab() {
		maverick.changeToIdleFallOrFly();
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		if (!maverick.sprite.name.EndsWith("hurt")) {
			maverick.changeSpriteFromName("die", resetFrame: true);
		}
		maverick.stopMoving();
		maverick.useGravity = false;
		maverick.grounded = false;
		savedZIndex = maverick.zIndex;
		if (!reverseZIndex) {
			maverick.setzIndex(grabber.zIndex - 100);
		} else {
			maverick.setzIndex(grabber.zIndex + 100);
		}
		//canLandCustom = false;
		maverick.isInGrabState = true;
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
		maverick.useGravity = true;
		maverick.setzIndex(savedZIndex);
		maverick.grabHitCooldown = 2f;
		maverick.isInGrabState = false;
	}

	public bool trySnapToGrabPoint(bool lerp) {
		Point grabberGrabPoint = grabber.getFirstPOIOrDefault("g");
		Point victimGrabOffset = new Point(0, (maverick.height / 2f) + extraYOffset);
		Point destPos = grabberGrabPoint.add(victimGrabOffset);
		if (maverick.pos.distanceTo(destPos) > 25f) {
			lerp = true;
		}
		Point lerpPos = (lerp ? Point.lerp(maverick.pos, destPos, 0.25f) : destPos);
		if (Global.level.checkCollisionActor(
				maverick, lerpPos.x - maverick.pos.x,
				lerpPos.y - maverick.pos.y
			)?.gameObject is Wall
		) {
			return false;
		}
		maverick.changePos(lerpPos);
		return true;
	}
}

public class MvrkBBuffaloDragged : MGrabbed {
	public const float maxGrabTime = 1.5f;

	public MvrkBBuffaloDragged(
		BlizzardBuffalo grabber, int projId
	) : base(
		grabber, maxGrabTime, "_dash", projId, reverseZIndex: true,
		freeOnHitWall: false, lerp: true, "_dash_grab"
	) {

	}
}

public class MvrkBeetleGrabbedState : MGrabbed {
	public Character grabbedChar;
	public bool launched;
	private float launchTime;

	public MvrkBeetleGrabbedState(
		GravityBeetle grabber, int projId
	) : base(
		grabber, 1f, "", projId
	) {
		customUpdate = true;
	}

	public override void update() {
		base.update();
		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				maverick.changeToIdleFallOrFly();
			} else if (maverick.stopCeiling() && !maverick.isHeavy) {
				(grabber as GravityBeetle).meleeWeapon.applyDamage(
					maverick, weakness: false,
					grabber, (int)ProjIds.GBeetleLiftCrash
				);
			}
			return;
		}
		Sprite obj = grabber.sprite;
		if (obj != null && obj.name.EndsWith("_dash_lift")) {
			if (grabber.frameIndex < 2) {
				trySnapToGrabPoint(lerp: true);
			} else if (!launched) {
				launched = true;
				maverick.unstickFromGround();
				maverick.vel.y = -600f;
			}
		} else {
			notGrabbedTime += Global.spf;
		}
		if (notGrabbedTime > 0.5f) {
			maverick.changeToIdleFallOrFly();
		}
	}
}

public class MvrkCrushCGrabbed : MGrabbed {
	public const float maxGrabTime = 0.7f;

	public MvrkCrushCGrabbed(
		CrushCrawfish grabber, int projId
	) : base(
		grabber, maxGrabTime, "grab_attack", projId, reverseZIndex: false,
		freeOnHitWall: false, lerp: true, null, 1f
	) {
		heavyDecayMultiplier = 1.4f;
	}
}


public class MvrkDeadLiftGrabbed : MGrabbed {
	public Character grabbedChar;
	public bool launched;
	private float launchTime;

	public MvrkDeadLiftGrabbed(
		BoomerangKuwanger grabber, int projId
	) : base(
		grabber, 1f, "", projId
	) {
		customUpdate = true;
	}

	public override void update() {
		base.update();
		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				maverick.changeToIdleFallOrFly();
			} else if (maverick.stopCeiling() && !maverick.isHeavy) {
				new BoomerangKDeadLiftWeapon(
					(grabber as Maverick).player
				).applyDamage(
					maverick, weakness: false,
					maverick, (int)ProjIds.BoomerangKDeadLift
				);
			}
			return;
		}
		Sprite obj = grabber.sprite;
		if (!once) {
			new BoomerangKDeadLiftWeapon((
				grabber as Maverick)?.player
			).applyDamage(
				maverick, false,
				maverick, (int)ProjIds.BoomerangKDeadLift
			);
			once = true;
		}
		if (obj != null && obj.name.EndsWith("_deadlift")) {
			if (grabber.frameIndex < 4) {
				trySnapToGrabPoint(lerp: true);
			} else if (!launched) {
				launched = true;
				maverick.unstickFromGround();
				maverick.vel.y = -600f;
			}
		} else {
			notGrabbedTime += Global.spf;
		}
		if (notGrabbedTime > 0.5f) {
			maverick.changeToIdleFallOrFly();
		}
	}
}

public class MvrkFStagGrabbed : MGrabbed {
	public Character grabbedChar;

	public float timeNotGrabbed;

	private string lastGrabberSpriteName;

	public const float maxGrabTime = 4;

	public MvrkFStagGrabbed(
		FlameStag grabber, int projId
	) : base(
		grabber, maxGrabTime, "_dash_grab", projId
	) {
		customUpdate = true;
	}

	public override void update() {
		base.update();
		string grabberSpriteName = grabber.sprite?.name ?? "";
		if (grabberSpriteName.EndsWith("_dash_grab")) {
			trySnapToGrabPoint(lerp: true);
		} else if (grabberSpriteName.EndsWith("_updash") || grabberSpriteName.EndsWith("_downdash")) {
			grabTime -= mashTimeDecay;
			if (grabTime <= 0f) {
				maverick.changeToIdleFallOrFly();
			}
			if (lastGrabberSpriteName != grabberSpriteName) {
				trySnapToGrabPoint(lerp: true);
			} else {
				maverick.incPos(grabber.deltaPos);
			}
		} else {
			timeNotGrabbed += Global.spf;
			if (timeNotGrabbed > 1f) {
				maverick.changeToIdleFallOrFly();
				return;
			}
		}
		lastGrabberSpriteName = grabberSpriteName;
	}
}

public class MvrkMagnaCDrainGrabbed : MGrabbed {
	public const float maxGrabTime = 4;

	public MvrkMagnaCDrainGrabbed(
		MagnaCentipede grabber, int projId
	) : base(
		grabber, maxGrabTime, "_drain", projId
	) {
		base.grabber = grabber;
		grabTime = maxGrabTime;
	}
}


public class MvrkVileMK2Grabbed : MGrabbed {
	public const float maxGrabTime = 2f;

	public MvrkVileMK2Grabbed(
		Character grabber, int projId
	) : base(
		grabber, maxGrabTime, "_grab", projId
	) {
		extraYOffset -= 5;
		heavyDecayMultiplier = 1.5f;
	}
}


public class MvrkUPXGrabbed : MGrabbed {
	public const float maxGrabTime = 1.8f;

	public MvrkUPXGrabbed(
		Character grabber, int projId
	) : base(
		grabber, maxGrabTime, "unpo_grab", projId, additionalGrabSprite: "unpo_grab2"
	) {
		extraYOffset -= 5;
		heavyDecayMultiplier = 1.5f;
	}

	public override void exitGrab() {
		maverick.changeState(new MvrkGrabRelease());
	}
}

public class MvrkWheelGGrabbed : MGrabbed {
	public Character grabbedChar;

	public float timeNotGrabbed;

	private string lastGrabberSpriteName;

	public const float maxGrabTime = 1f;

	public MvrkWheelGGrabbed(
		WheelGator grabber, int projId
	) : base(
		grabber, maxGrabTime, "grabbed", projId
	) {
	}

	public override void update() {
		string grabberSpriteName = grabber.sprite?.name ?? "";
		if (grabberSpriteName.EndsWith("_grab_start")) {
			if (lastGrabberSpriteName != grabberSpriteName) {
				if (!trySnapToGrabPoint(lerp: true)) {
					maverick.changeToIdleFallOrFly();
					return;
				}
			} else {
				maverick.incPos(grabber.deltaPos);
			}
		} else {
			timeNotGrabbed += Global.spf;
			if (timeNotGrabbed > 0.1f) {
				maverick.changeToIdleFallOrFly();
				return;
			}
		}
		lastGrabberSpriteName = grabberSpriteName;
		grabTime -= mashTimeDecay;
		if (grabTime <= 0f) {
			maverick.changeToIdleFallOrFly();
		}
	}
}

public class MvrkWhirlpoolGrabbed : MGrabbed {
	public const float maxGrabTime = 1.4f;

	public MvrkWhirlpoolGrabbed(
		LaunchOctopus grabber, int projId
	) : base(
		grabber, maxGrabTime, "_drain", projId
	) {
		heavyDecayMultiplier = 3f;
	}
}

public class MvrkGrabRelease : MaverickState {
	public MvrkGrabRelease() : base("hurt") {

	}

	public override void update() {
		base.update();
		if (maverick.grounded) {
			maverick.changeState(new MIdle());
		}
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		if (!maverick.sprite.name.EndsWith("hurt")) {
			maverick.changeSpriteFromName("die", resetFrame: true);
		}
		maverick.useGravity = true;
		
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);

	}
}
