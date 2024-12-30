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
 
	public override void update() {
		base.update();
		
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
	//	maverick.isInGrabState = true;
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
		maverick.useGravity = true;
		maverick.setzIndex(savedZIndex);
		//maverick.grabHitCooldown[player.id + "_" + grabId] = 2f;
		//maverick.isInGrabState = false;
	}

	public bool trySnapToGrabPoint(bool lerp) {
		Point grabberGrabPoint = grabber.getFirstPOIOrDefault("g");
		Point victimGrabOffset = new Point(0, (maverick.height / 2f) + extraYOffset);
		Point destPos = grabberGrabPoint.add(victimGrabOffset);
		if (maverick.pos.distanceTo(destPos) > 25f) {
			lerp = true;
		}
		Point lerpPos = (lerp ? Point.lerp(maverick.pos, destPos, 0.25f) : destPos);
		if (Global.level.checkTerrainCollisionOnce(
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

	public bool customUpdate;

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
			} else if (maverick.stopCeiling()) {
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

	public bool customUpdate;

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
			} else if (maverick.stopCeiling()) {
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

	public bool customUpdate;

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
		//canLandCustom = false;
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);

	}
}


// Applies to freeze, stun, other effects.
public class MGenericStun : MaverickState {
	public Anim? paralyzeAnim;
	public bool changeAnim = true;
	public bool canPlayFrozenSound = true;
	public bool canPlayStaticSound = true;
	public int hurtDir = 1;
	public float hurtSpeed;
	public float flinchYPos;

	public float flinchTime;
	public float flinchMaxTime;

	public MGenericStun() : base("hurt") {

	}

	public override void update() {
		Helpers.decrementFrames(ref flinchTime);

		crystalizeLogic();
		paralizeAnimLogic();
		paralizeAnimLogic2();
		freezeLogic();

		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 1.6f / flinchMaxTime * Global.speedMul, hurtDir);
			maverick.move(new Point(hurtSpeed * 60f, 0));
		}

		if (changeAnim) {
			string stunAnim = getStunAnim();
			maverick.changeSpriteFromName(getStunAnim(), true);
			if (stunAnim == "idle") {
				maverick.sprite.frameSpeed = 0;
			}
		}

		if (maverick.frozenTime == 0 && maverick.crystalizedTime == 0 
		&& maverick.paralyzedTime == 0) {
			if (flinchTime > 0) {
				maverick.changeState(
					new MHurt(hurtDir, MathInt.Ceiling(flinchTime)), true
				);
				return;
			}
			maverick.changeToIdleOrFall();
		}
	}
	
	public void freezeLogic() {
		if (maverick.frozenTime == 0) {
			return;
		}
		if (canPlayFrozenSound) {
			maverick.playSound("igFreeze", true);
			canPlayFrozenSound = false;
		}
		reduceStunFrames(ref maverick.frozenTime);
		maverick.freezeInvulnTime = 2;

		if (maverick.frozenTime == 0) {
			maverick.breakFreeze(player, sendRpc: true);
			canPlayFrozenSound = true;
			changeAnim = true;
		}
	}

	public void crystalizeLogic() {
		if (maverick.crystalizedTime == 0 && !maverick.isCrystalized) {
			return;
		}
		reduceStunFrames(ref maverick.crystalizedTime);
		maverick.crystalizeInvulnTime = 2;

		if (!maverick.isCrystalized && maverick.crystalizedTime > 0) {
			changeAnim = true;
			maverick.crystalizeStart();
			//Global.serverClient?.rpc(RPC.playerToggle, (byte)maverick.player.id, (byte)RPCToggleType.StartCrystalize);
		}
		else if (maverick.isCrystalized && maverick.crystalizedTime == 0) {
			changeAnim = true;
			maverick.crystalizeEnd();
			//Global.serverClient?.rpc(RPC.playerToggle, (byte)maverick.player.id, (byte)RPCToggleType.StopCrystalize);
		}
	}

	public void paralizeAnimLogic() {
		if (maverick.paralyzedTime == 0) {
			return;
		}
		maverick.useGravity = false;
		maverick.vel.y = 0;
		if (canPlayStaticSound) {
			maverick.playSound("voltcStatic");
			canPlayStaticSound = false;
		}
		reduceStunFrames(ref maverick.paralyzedTime);
		maverick.stunInvulnTime = 2;

		if (paralyzeAnim == null && maverick.paralyzedTime > 0) {
			paralyzeAnim = new Anim(
				maverick.getCenterPos(), "vile_stun_static",
				1, maverick.player.getNextActorNetId(), false,
				host: maverick, sendRpc: true
			);
			paralyzeAnim.setzIndex(maverick.zIndex + 100);
		}
		if (maverick.paralyzedTime == 0) {
			changeAnim = true;
			canPlayStaticSound = true;
			if (paralyzeAnim != null) {
				paralyzeAnim.destroySelf();
				paralyzeAnim = null;
			}
		}
	}


	public void paralizeAnimLogic2() {
		if (maverick.paralyzedTime == 0) {
			return;
		}
		maverick.useGravity = true;
		maverick.vel.y = 0;
		if (canPlayStaticSound) {
			maverick.playSound("voltcStatic");
			canPlayStaticSound = false;
		}
		reduceStunFrames(ref maverick.paralyzedTime);
		maverick.stunInvulnTime = 2;

		if (paralyzeAnim == null && maverick.paralyzedTime > 0) {
			paralyzeAnim = new Anim(
				maverick.getCenterPos(), "vile_stun_static",
				1, maverick.player.getNextActorNetId(), false,
				host: maverick, sendRpc: true
			);
			paralyzeAnim.setzIndex(maverick.zIndex + 100);
		}
		if (maverick.paralyzedTime == 0) {
			changeAnim = true;
			canPlayStaticSound = true;
			if (paralyzeAnim != null) {
				paralyzeAnim.destroySelf();
				paralyzeAnim = null;
			}
		}
	}


	public string getStunAnim() {
		if (maverick.frozenTime > 0) {
			return "frozen";
		}
		if (maverick.isCrystalized) {
			return "idle";
		}
		if (maverick.paralyzedTime > 0 && maverick.grounded) {
			return "lose";
		}
		return "hurt";
	}

	public void activateFlinch(int flinchFrames, int xDir) {
		hurtDir = xDir;
		if (player.isX && player.hasBodyArmor(1)) {
			flinchFrames = MathInt.Floor(flinchFrames * 0.75f);
		}
		if (flinchTime > flinchFrames) {
			return;
		}
		if (maverick.paralyzedTime >= 3) {
			maverick.paralyzedTime = 2;
		}
		bool isCombo = (flinchTime != 0);
		hurtSpeed = 1.6f * xDir;
		if (!isCombo) {
			flinchYPos = maverick.pos.y;
		}
		if (flinchFrames >= 2) {
			maverick.vel.y = (-0.125f * (flinchFrames - 1)) * 60f;
			if (isCombo && maverick.pos.y < flinchYPos) {
				maverick.vel.y = (0.002f * flinchTime - 0.076f) * (flinchYPos - maverick.pos.y) + 1;
			}
		}
		flinchTime = flinchFrames;
		flinchMaxTime = flinchFrames;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.stopMovingWeak();

		
		hurtDir = -maverick.xDir;
		// To continue the flinch if was flinched before the stun.
		if (oldState is MHurt hurtState) {
			hurtDir = hurtState.hurtDir;
			hurtSpeed = hurtState.hurtSpeed;
			flinchTime = hurtState.flinchTime;
			flinchYPos = hurtState.flinchYPos;
			if (flinchTime < 0) {
				flinchTime = 0;
			}
		}
	}

	public override void onExit(MaverickState newState) {
		if (paralyzeAnim != null) {
			paralyzeAnim.destroySelf();
			paralyzeAnim = null;
		}

			maverick.useGravity = true;
		if (maverick.crystalizedTime != 0 || maverick.isCrystalized) {
			maverick.crystalizeEnd();
			//Global.serverClient?.rpc(RPC.playerToggle, (byte)character.player.id, (byte)RPCToggleType.StopCrystalize);
		}
		maverick.paralyzedTime = 0;
		maverick.frozenTime = 0;
		maverick.crystalizedTime = 0;

		base.onExit(newState);
	}

	public void reduceStunFrames(ref float arg) {
		arg -= player.mashValue() * 60f;
		if (arg <= 0) {
			arg = 0;
		}
	}

	public float getTimerFalloff() {
		float healthPercent = 1 * (player.health / player.maxHealth);
		return (Global.speedMul * (2 + healthPercent));
	}
}

public class MKnockedDown : MaverickState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public MKnockedDown(int dir) : base("knocked_down") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
		flinchTime = 0.5f;
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.vel.y = -100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			maverick.move(new Point(hurtSpeed, 0));
		}


		if (stateTime >= flinchTime) {
			maverick.changeToIdleOrFall();
		}
	}
}



public class MPushedOver : MaverickState {
	public int hurtDir;
	public float hurtSpeed;
	public float flinchTime;
	public MPushedOver(int dir) : base("hurt") {
		hurtDir = dir;
		hurtSpeed = dir * 300;
		flinchTime = 0.5f;
	}


	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.vel.y = -300;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			maverick.move(new Point(hurtSpeed, 0));
		}

		if (stateTime >= flinchTime) {
			maverick.changeState(new MKnockedDown(-maverick.xDir), true);
		}
	}
}




public class MLaunchedState : MaverickState {

	//private bool once;
	public bool launched;
	float launchTime;
	bool once;

	public bool customUpdate;
	public MLaunchedState() :  base("hurt")  {
		customUpdate = true;
		superArmor = true;
	}

	
	public override void update() {
		base.update();

		if (launched) {
			launchTime += Global.spf;
			if (launchTime > 0.33f) {
				maverick.changeToIdleOrFall();
				return;
			}

			for (int i = 1; i <= 4; i++) {
				CollideData collideData = Global.level.checkTerrainCollisionOnce(maverick, 0, -10 * i, autoVel: true);
				if (!maverick.grounded && collideData != null && collideData.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
						if (!once){
							once = true;
							maverick.applyDamage(3, player, maverick, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
						//	character.changeState(new Hurt(-character.xDir, Global.defFlinch, 0), true);
		
						}
							maverick.playSound("crash", sendRpc: true);
							maverick.shakeCamera(sendRpc: true);
							//return;
						}
			}
	
		}

			if (!launched) {
				launched = true;
				maverick.unstickFromGround();
				maverick.vel.y = -600;
			}	 
	}
}
