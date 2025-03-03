using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace MMXOnline;

public class AxlWC : Character {
	// Weapon data.
	public Anim chargeAnim;
	public AxlWeaponWC mainWeapon = null!;
	public List<AxlWeaponWC> axlWeapons = new();
	public static List<AxlWeaponWC> getAllSpecialWeapons() {
		return new List<AxlWeaponWC>() {
			new RayGunWC(),
			new BlastLauncherWC(),
			new BlackArrowWC(),
			new SpiralMagnumWC(),
			new PlasmaGunWC(),
			new BoundBlasterWC(),
			new IceGattlingWC(),
			new FlameBurnerWC(),
		};
	}
	public static List<AxlWeaponWC> getAllMainWeapons() {
		return new List<AxlWeaponWC>() {
			new AxlBulletWC(),
		};
	}
	public AxlWeaponWC? axlWeapon {
		get {
			if (weaponSlot < 0 || weaponSlot >= weapons.Count) {
				return null;
			}
			return axlWeapons[weaponSlot];
		}
	}
	// Other variables.
	public const int WhiteAxlCost = 5;
	public bool? shouldDrawArmNet = null;
	public bool isWhite;
	public float whiteTime;
	public float autoChargeCooldown;
	public float dodgeRollCooldown;
	public float aiAttackCooldown;
	public float shootCooldown;
	public float recoilTime;
	public float turnCooldown;
	public bool lockDir;
	public int armDir => charState is WallSlide ? -xDir : xDir;
	public float armAngle = 0;
	public Anim? muzzleFlash;
	public int hoverTimes = 0;




	public bool aiming;
	public IDamagable? axlCursorTarget = null;
	public Character? axlHeadshotTarget = null;


		// Cursor stuff.

		public float stingChargeTime;
	public int lastXDir;


	public Point axlCursorPos;
	public Point? assassinCursorPos;
	public Point axlCursorWorldPos => axlCursorPos.addxy(Global.level.camX, Global.level.camY);
	public Point axlScopeCursorWorldPos;
	public Point axlScopeCursorWorldLerpPos;
	public Point axlZoomOutCursorDestPos;
	public Point axlLockOnCursorPos;
	public Point axlGenericCursorWorldPos {
		get {
			if (!isZooming() || isZoomingIn || isZoomOutPhase1Done) {
				return axlCursorWorldPos;
			}
			return axlScopeCursorWorldPos;
		}
	}


	

	public float axlSwapTime;
	public float axlAltSwapTime;
	public float switchTime;
	public float altSwitchTime;
	public float netArmAngle;
	float targetSoundCooldown;
	public Point nonOwnerAxlBulletPos;
	public float stealthRevealTime;

	public bool aimBackwardsToggle;
	public bool positionLockToggle;
	public bool cursorLockToggle;
	public void resetToggle() {
		aimBackwardsToggle = false;
		positionLockToggle = false;
		cursorLockToggle = false;
	}

	public bool isNonOwnerZoom;
	public Point nonOwnerScopeStartPos;
	public Point nonOwnerScopeEndPos;
	public Point? netNonOwnerScopeEndPos;
	private bool _zoom;
	public bool isZoomingIn;
	public bool isZoomingOut;
	public bool isZoomOutPhase1Done;
	public float zoomCharge;
	public float savedCamX;
	public float savedCamY;
	public bool hyperAxlStillZoomed;

	public float revTime;
	public float revIndex;
	public bool aimingBackwards;



	public AxlWC(
		Player player, float x, float y, int xDir, bool isVisible,
		ushort? netId, bool ownedByLocalPlayer, bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.AxlWC;
		spriteFrameToSounds["axl_run/4"] = "run";
		spriteFrameToSounds["axl_run/8"] = "run";
		configureWeapons();
	}

	public override void preUpdate() {
		base.preUpdate();
		if (!ownedByLocalPlayer) {
			return;
		}
		// Cooldowns.
		Helpers.decrementFrames(ref turnCooldown);
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref recoilTime);
		Helpers.decrementTime(ref autoChargeCooldown);
		// Weapon input logic.
		
		foreach (AxlWeaponWC weapon in axlWeapons) {
			weapon.preAxlUpdate(this, weapon == axlWeapon);
		}
		// Lock dir logic.
		if (lockDir && (
				!(player.input.isHeld(Control.Shoot, player) || player.input.isHeld(Control.Special1, player)) ||
				turnCooldown <= 0 || charState is Dash or AirDash
			)
		) {
			lockDir = false;
			turnCooldown = 0;
		}
	}
	public bool checkLockMoveCoditions(){
		if (!Options.main.moveWhileShooting){
		if (player.input.isHeld(Control.Shoot, player) || player.input.isPressed(Control.Shoot, player)) return true;
		if (axlWeapon is not AxlBulletWC && player.input.isHeld(Control.Special1, player)) return true;
		if (player.input.isPressed(Control.Special1, player)) return true;
		if (isCharging() && player.input.isPressed(Control.Special1, player)) return true;}
		return false;
	}
	public override bool canMove() {
		if (checkLockMoveCoditions()) {
			return false;
		}
		return base.canMove();
	}

	public override int getMaxChargeLevel() {
		return 2;
	}
	public override void chargeGfx() {
		if (ownedByLocalPlayer) {
			chargeEffect.stop();
		}
		if (isCharging()) {
			chargeSound.play();
			if (!sprite.name.Contains("ra_hide")) {
				int level = getChargeLevel();
				
				if(level == 2){
					if(chargeAnim == null){
					//	Point animPos = ;
						chargeAnim = new Anim(getCenterPos(), "x8_axl_charge_part2", 1, null, true);
						//chargeAnim.setzIndex(zIndex - 100);
					}
				addRenderEffect(RenderEffectType.ChargeYellow, 4, 6);
				}
			}
			chargeEffect.update(getChargeLevel(), 5);
		}}
	public override void update() {

		armAngle = netArmAngle;
		bool wasGrounded = grounded;
		base.update();
		if(charState is Die){
			if(chargeAnim != null){
				chargeAnim.destroySelf();
				chargeAnim = null;
			}
		}
		if(isCharging() && getAutoChargeConditions() && player.input.isPressed(Control.Special1, player)){
			autoChargeCooldown = 0.1f;
		}
		if(chargeAnim != null){
			chargeAnim.changePos(getCenterPos());}
		// Hypermode music.
		if (isWhite) {
			if (musicSource == null) {
				addMusicSource("wildFang", getCenterPos(), true);
			}
		} else if (musicSource != null) {
			musicSource.stop();
			musicSource = null;
		}
		if (!ownedByLocalPlayer) {
			return;
		}
		// For Hover
		if (grounded || charState is WallSlide) { hoverTimes = 0; }

		// For String Cancels
		if (sprite.name.Contains("string")) {
			if (player.input.checkDoubleTap(Control.Dash) &&
				player.input.isPressed(Control.Dash, player) && canDash() && flag == null
			) {
				changeState(new DodgeRollAxlWC(), true);
			}
			if (//(wasGrounded || grounded) && 
			// There's no need for this move to be Grounded only 
			 player.input.isHeld(Control.Up, player) &&
				player.input.isPressed(Control.Jump, player)
			) {
				changeState(new AxlFlashKick(), true);
			}

		}
		// For Cancels on Dodgeroll
		if (charState is DodgeRollAxlWC) {
			if (player.input.isHeld(Control.Up, player)
				&& player.input.isPressed(Control.Jump, player)) {
				changeState(new AxlFlashKick(), true);
			}

			if 	(player.input.isPressed(Control.Special1, player)
			&& mainWeapon.ammo > 2){
			changeState(new EvasionBarrage(), true);
			}
		}
		// Weapon update.
		foreach (AxlWeaponWC weapon in axlWeapons) {
			weapon.axlUpdate(this, weapon == axlWeapon);
		}
		// Arm angle.



	

	

		 if (Options.main.axlAimMode == 2) {
				updateAxlCursorPos();
			} else {
				updateAxlDirectionalAim();
			}
		
	//	updateArmAngle();




		// Charge and release charge logic.

		if (!isInDamageSprite()) {
			chargeLogic(chargeShoot);
		}
		weaponSwapLogic();
	}

	public override void postUpdate() {
		base.postUpdate();
		if (!ownedByLocalPlayer) {
			return;
		}
		if (muzzleFlash?.destroyed == false) {
			float oldByteArmAngle = armAngle;
			if (recoilTime > 0) {
				armAngle = MathF.Round(armAngle - recoilTime);
			}
			muzzleFlash.pos = getAxlBulletPos(axlWeapon);
			muzzleFlash.xDir = armDir;
			muzzleFlash.visible = true;
			muzzleFlash.byteAngle = armAngle * armDir;
			muzzleFlash.zIndex = ZIndex.Default;
			if (muzzleFlash.sprite.name == "axl_raygun_flash") {
				muzzleFlash.zIndex = zIndex - 100;
			}
			armAngle = oldByteArmAngle;
		}
		// Swap on ammo empitiying.
		if (axlWeapon != null && axlWeapon != mainWeapon &&
			axlWeapon.ammo <= 0 && recoilTime <= 0
		) {
			Weapon oldWeapon = axlWeapon;
			weaponSlot = 1;
			onWeaponChange(oldWeapon, axlWeapon);
			return;
		}
		// Weapon input logic.
		foreach (AxlWeaponWC weapon in axlWeapons) {
			weapon.postAxlUpdate(this, weapon == axlWeapon);
		}
	}

	public override bool canCrouch() {
		return false;
	}


	public override bool changeState(CharState newState, bool forceChange = false) {
		if (charState is Dash or AirDash) {
			slideVel = xDir * getDashSpeed() * 0.3f;
		}
		return base.changeState(newState, forceChange);
	}





	public bool isAnyZoom() {
		return isZooming() || isZoomingOut || isZoomingIn;
	}

	public void updateAxlCursorPos() {
		float aimThreshold = 5;
		bool axisXMoved = false;
		bool axisYMoved = false;
		// Options.main.aimSensitivity is a float from 0 to 1.
		float distFromNormal = Options.main.aimSensitivity - 0.5f;
		float sensitivity = 1;
		if (distFromNormal > 0) {
			sensitivity += distFromNormal * 7.5f;
		} else {
			sensitivity += distFromNormal * 1.75f;
		}

		// Controller joystick axis move section
		if (Input.aimX > aimThreshold && Input.aimX >= Input.lastAimX) {
			axlCursorPos.x += Global.spf * Global.screenW * (Input.aimX / 100f) * sensitivity;
			axisXMoved = true;
		} else if (Input.aimX < -aimThreshold && Input.aimX <= Input.lastAimX) {
			axlCursorPos.x -= Global.spf * Global.screenW * (MathF.Abs(Input.aimX) / 100f) * sensitivity;
			axisXMoved = true;
		}
		if (Input.aimY > aimThreshold && Input.aimY >= Input.lastAimY) {
			axlCursorPos.y += Global.spf * Global.screenW * (Input.aimY / 100f) * sensitivity;
			axisYMoved = true;
		} else if (Input.aimY < -aimThreshold && Input.aimY <= Input.lastAimY) {
			axlCursorPos.y -= Global.spf * Global.screenW * (MathF.Abs(Input.aimY) / 100f) * sensitivity;
			axisYMoved = true;
		}

		// Controller or keyboard button based aim section
		if (!axisXMoved) {
			if (player.input.isHeld(Control.AimLeft, player)) {
				axlCursorPos.x -= Global.spf * 200 * sensitivity;
			} else if (player.input.isHeld(Control.AimRight, player)) {
				axlCursorPos.x += Global.spf * 200 * sensitivity;
			}
		}
		if (!axisYMoved) {
			if (player.input.isHeld(Control.AimUp, player)) {
				axlCursorPos.y -= Global.spf * 200 * sensitivity;
			} else if (player.input.isHeld(Control.AimDown, player)) {
				axlCursorPos.y += Global.spf * 200 * sensitivity;
			}
		}

		// Mouse based aim
		if (!Menu.inMenu && !player.isAI) {
			if (Options.main.useMouseAim) {
				axlCursorPos.x += Input.mouseDeltaX * 0.125f * sensitivity;
				axlCursorPos.y += Input.mouseDeltaY * 0.125f * sensitivity;
			}
			axlCursorPos.x = Helpers.clamp(axlCursorPos.x, 0, Global.viewScreenW);
			axlCursorPos.y = Helpers.clamp(axlCursorPos.y, 0, Global.viewScreenH);
		}

		if (isWarpIn()) {
			axlCursorPos = getCenterPos().addxy(-Global.level.camX + 50 * xDir, -Global.level.camY);
		}

		// aimbot
		if (player.isAI) {
			var target = Global.level.getClosestTarget(pos, player.alliance, true);
			if (target != null) {
				axlCursorPos = target.pos.addxy(
					-Global.level.camX,
					-Global.level.camY - ((target as Character)?.charState is InRideArmor ? 0 : 16)
				);
			};
		}

		getMouseTargets();
	}




	public void getMouseTargets() {
		axlCursorTarget = null;
		axlHeadshotTarget = null;

		int cursorSize = 1;
		var shape = new Rect(axlGenericCursorWorldPos.x - cursorSize, axlGenericCursorWorldPos.y - cursorSize, axlGenericCursorWorldPos.x + cursorSize, axlGenericCursorWorldPos.y + cursorSize).getShape();
		var hit = Global.level.checkCollisionsShape(shape, new List<GameObject>() { this }).FirstOrDefault(c => c.gameObject is IDamagable);
		if (hit != null) {
			var target = hit.gameObject as IDamagable;
			if (target != null) {
				if (target.canBeDamaged(player.alliance, player.id, null)) {
					axlCursorTarget = target;
				}
			}
		}
		foreach (var enemy in Global.level.players) {
			if (enemy.character != null && enemy.character.canBeDamaged(player.alliance, player.id, null) && enemy.character.getHeadPos() != null) {
				if (axlGenericCursorWorldPos.distanceTo(enemy.character.getHeadPos().Value) < headshotRadius) {
					axlCursorTarget = enemy.character;
					axlHeadshotTarget = enemy.character;
				}
			}
		}
	}


	public void updateAxlDirectionalAim() {
		if (player.input.isCursorLocked(player)) {
			Point worldCursorPos = pos.add(lastDirToCursor);
			axlCursorPos = worldCursorPos.addxy(-Global.level.camX, -Global.level.camY);
			lockOn(out _);
			return;
		}

		if (charState is Assassinate) {
			return;
		}

		Point aimDir = new Point(0, 0);

		if (Options.main.aimAnalog) {
			aimDir.x = Input.aimX;
			aimDir.y = Input.aimY;
		}

		bool aimLeft = player.input.isHeld(Control.AimLeft, player);
		bool aimRight = player.input.isHeld(Control.AimRight, player);
		bool aimUp = player.input.isHeld(Control.AimUp, player);
		bool aimDown = (
			player.input.isHeld(Control.AimDown, player) &&
			(Options.main.axlSeparateAimDownAndCrouch ||
			charState is not Idle && charState is not Crouch)
		);






		if (aimDir.magnitude < 10) {
			if (aimLeft) {
				aimDir.x = -100 * xDir;
			} else if (aimRight) {
				aimDir.x = 100 * xDir;
			}
			if (aimUp) {
				aimDir.y = -100 * xDir;
			} else if (aimDown) {
				aimDir.y = 100 * xDir;
			}
		}

		

		aimingBackwards = player.input.isAimingBackwards(player);

		int aimBackwardsMod = 1;
		if (aimingBackwards && charState is not LadderClimb) {
			if (player.axlWeapon?.isTwoHanded(false) != true) {
				if (Math.Sign(aimDir.x) == Math.Sign(xDir)) {
					aimDir.x *= -1;
				}
				aimBackwardsMod = -1;
			} else {
				// By design, aiming backwards with 2-handed weapons does not actually cause Axl to aim backwards like with 1-handed weapons as this would look really weird.
				// Instead, it locks Axl's aim forward and allows him to backpedal without changing direction.
				xDir = lastXDir;
				if (Math.Sign(aimDir.x) != Math.Sign(xDir)) {
					aimDir.x *= -1;
				}
			}
		}

		if (aimDir.magnitude < 10) {
			aimDir = new Point(xDir * 100 * aimBackwardsMod, 0);
		}

		if (charState is WallSlide) {
			if (xDir == -1) {
				if (aimDir.x < 0) aimDir.x *= -1;
			}
			if (xDir == 1) {
				if (aimDir.x > 0) aimDir.x *= -1;
			}
		}

		float xOff = 0;
		float yOff = -24;
		if (charState is Crouch) yOff = -16;

		//axlCursorPos = pos.addxy(xOff * xDir, yOff).addxy(aimDir.x, aimDir.y).addxy(-Global.level.camX, -Global.level.camY);
		Point destCursorPos = pos.addxy(xOff * xDir, yOff).addxy(aimDir.x, aimDir.y).addxy(-Global.level.camX, -Global.level.camY);

		if (charState is Dash || charState is AirDash) {
			destCursorPos = destCursorPos.addxy(15 * xDir, 0);
		}

		// Try to see if where cursor will go to has auto-aim target. If it does, make that the dest, not the actual dest
		Point oldCursorPos = axlCursorPos;
		axlCursorPos = destCursorPos;
		lockOn(out Point? lockOnPoint);
		if (lockOnPoint != null) {
			destCursorPos = lockOnPoint.Value;
		}
		axlCursorPos = oldCursorPos;

		// Lerp to the new target
		//axlCursorPos = Point.moveTo(axlCursorPos, destCursorPos, Global.spf * 1000);
		if (!Options.main.aimAnalog) {
			axlCursorPos = Point.lerp(axlCursorPos, destCursorPos, Global.spf * 15);
		} else {
			axlCursorPos = destCursorPos;
		}

		lastDirToCursor = pos.directionTo(axlCursorWorldPos);
	}

	Point lastDirToCursor;



	public void lockOn(out Point? lockOnPoint) {
		// Check for lock on targets
		lockOnPoint = null;
		var prevTarget = axlCursorTarget;
		axlCursorTarget = null;
		axlHeadshotTarget = null;
		assassinCursorPos = null;

		if (!Options.main.lockOnSound) return;
		if (player.isDisguisedAxl && !player.isAxlXOD && player.axlWeapon is not AssassinBullet) return;
		if (player.isDisguisedAxl && player.axlWeapon is UndisguiseWeapon) return;
		if (player.input.isCursorLocked(player)) return;

		axlCursorTarget = getLockOnTarget();

		if (axlCursorTarget != null && prevTarget == null && player.isMainPlayer && targetSoundCooldown == 0) {
			Global.playSound("axlTarget", false);
			targetSoundCooldown = Global.spf;
		}

		if (axlCursorTarget != null) {
			axlLockOnCursorPos = (axlCursorTarget as Character).getAimCenterPos();
			lockOnPoint = axlLockOnCursorPos.addxy(-Global.level.camX, -Global.level.camY);
			// axlCursorPos = (axlCursorTarget as Character).getAimCenterPos().addxy(-Global.level.camX, -Global.level.camY);

			if (player.axlWeapon is AssassinBullet) {
				assassinCursorPos = lockOnPoint;
			}
		}
	}

		public bool isZooming() {
		return _zoom && player.isAxlWC;
	}


		public bool hasScopedTarget() {
		if (isZoomingOut || isZoomingIn) return false;
		if (axlCursorTarget == null && axlHeadshotTarget == null) return false;
		var hitData = getFirstHitPos(player.adjustedZoomRange, ignoreDamagables: true);
		if (axlCursorTarget != null && axlHeadshotTarget != null) {
			if (hitData.hitGos.Contains(axlCursorTarget) || hitData.hitGos.Contains(axlHeadshotTarget)) {
				return true;
			}
		}
		return false;
	}

	public int axlXDir {
		get {
			if (sprite.name.Contains("wall_slide")) return -xDir;
			return xDir;
		}
	}

	public int getAxlXDir() {
		if (player.axlWeapon != null && (player.axlWeapon.isTwoHanded(false))) {
			return pos.x < axlGenericCursorWorldPos.x ? 1 : -1;
		}
		return xDir;
	}


		public Point getDoubleBulletArmPos() {
		if (sprite.name == "axl_dash") {
			return new Point(-7, -2);
		}
		if (sprite.name == "axl_run") {
			return new Point(-7, 1);
		}
		if (sprite.name == "axl_jump" || sprite.name == "axl_fall_start" || sprite.name == "axl_fall" || sprite.name == "axl_hover") {
			return new Point(-7, 0);
		}
		return new Point(-5, 2);
	}

	public Point getAxlBulletPos(int poiIndex = 0) {
		if (player.axlWeapon == null) return new Point();

		Point gunArmOrigin = getAxlGunArmOrigin();

		var doubleBullet = player.weapon as DoubleBullet;
		if (doubleBullet != null && doubleBullet.isSecondShot) {
			Point dbArmPos = getDoubleBulletArmPos();
			gunArmOrigin = gunArmOrigin.addxy(dbArmPos.x * getAxlXDir(), dbArmPos.y);
		}

		Sprite sprite = getAxlArmSprite();
		float angle = getShootAngle(ignoreXDir: true) + sprite.animData.frames[0].POIs[poiIndex].angle * axlXDir;
		Point angleDir = Point.createFromAngle(angle).times(sprite.animData.frames[0].POIs[poiIndex].magnitude);

		return gunArmOrigin.addxy(angleDir.x, angleDir.y);
	}



	public Point getAxlBulletDir() {
		Point origin = getAxlBulletPos();
		Point cursorPos = getCorrectedCursorPos();
		return origin.directionTo(cursorPos).normalize();
	}

	public ushort netAxlArmSpriteIndex;
	public string getAxlArmSpriteName() {
	
		return player.axlWeapon?.sprite ?? "axl_arm_pistol";
	}

	public Sprite getAxlArmSprite() {
		if (!ownedByLocalPlayer && Global.spriteNameByIndex.ContainsKey(netAxlArmSpriteIndex)) {
			return new Sprite(Global.spriteNameByIndex[netAxlArmSpriteIndex]);
		}

		return new Sprite(getAxlArmSpriteName());
	}

	public Point getCorrectedCursorPos() {
		if (player.axlWeapon == null) return new Point();
		Point cursorPos = axlGenericCursorWorldPos;
		Point gunArmOrigin = getAxlGunArmOrigin();

		Sprite sprite = getAxlArmSprite();
		float minimumAimRange = sprite.animData.frames[0].POIs[0].magnitude + 5;

		if (gunArmOrigin.distanceTo(cursorPos) < minimumAimRange) {
			Point angleDir = Point.createFromAngle(getShootAngle(true));
			cursorPos = cursorPos.add(angleDir.times(minimumAimRange));
		}
		return cursorPos;
	}

	public Point getAxlHitscanPoint(float maxRange) {
		Point bulletPos = getAxlBulletPos();
		Point bulletDir = getAxlBulletDir();
		return bulletPos.add(bulletDir.times(maxRange));
	}


		public Point getAxlScopePos() {
		if (player.axlWeapon == null) return new Point();
		Point gunArmOrigin = getAxlGunArmOrigin();
		Sprite sprite = getAxlArmSprite();
		if (sprite.animData.frames[0].POIs.Length < 2) return new Point();
		float angle = getShootAngle(ignoreXDir: true) + sprite.animData.frames[0].POIs[1].angle * axlXDir;
		Point angleDir = Point.createFromAngle(angle).times(sprite.animData.frames[0].POIs[1].magnitude);
		return gunArmOrigin.addxy(angleDir.x, angleDir.y);
	}

	public Point getMuzzleOffset(float angle) {
		if (player.axlWeapon == null) return new Point();
		Sprite sprite = getAxlArmSprite();
		Point muzzlePOI = sprite.animData.frames[0].POIs[0];

		float horizontalOffX = 0;// Helpers.cosd(angle) * muzzlePOI.x;
		float horizontalOffY = 0;// Helpers.sind(angle) * muzzlePOI.x;

		float verticalOffX = -axlXDir * Helpers.sind(angle) * muzzlePOI.y;
		float verticalOffY = axlXDir * Helpers.cosd(angle) * muzzlePOI.y;

		return new Point(horizontalOffX + verticalOffX, horizontalOffY + verticalOffY);
	}


	public float getShootAngle(bool ignoreXDir = false) {


		Point gunArmOrigin = getAxlGunArmOrigin();
		Point cursorPos = axlGenericCursorWorldPos;
		float angle = gunArmOrigin.directionTo(cursorPos).angle;

		Point adjustedOrigin = gunArmOrigin.add(getMuzzleOffset(angle));
		float adjustedAngle = adjustedOrigin.directionTo(cursorPos).angle;

		// DEBUG CODE
		//Global.debugString1 = angle.ToString();
		//Global.debugString2 = adjustedAngle.ToString();
		//DrawWrappers.DrawPixel(adjustedOrigin.x, adjustedOrigin.y, Color.Red, ZIndex.Default + 1);
		//DrawWrappers.DrawPixel(gunArmOrigin.x, gunArmOrigin.y, Color.Red, ZIndex.Default + 1);
		//Point angleLine = Point.createFromAngle(angle).times(100);
		//DrawWrappers.DrawLine(gunArmOrigin.x, gunArmOrigin.y, gunArmOrigin.x + angleLine.x, gunArmOrigin.y + angleLine.y, Color.Magenta, 1, ZIndex.Default + 1);
		//Point angleLine2 = Point.createFromAngle(angleWithOffset).times(100);
		//DrawWrappers.DrawLine(gunArmOrigin.x, gunArmOrigin.y, gunArmOrigin.x + angleLine2.x, gunArmOrigin.y + angleLine2.y, Color.Red, 1, ZIndex.Default + 1);
		// END DEBUG CODE

		if (axlXDir == -1 && !ignoreXDir) adjustedAngle += 180;

		return adjustedAngle;
	}





	public RaycastHitData getFirstHitPos(float range, float backOffDist = 0, bool ignoreDamagables = false) {
		var retData = new RaycastHitData();
		Point bulletPos = getAxlBulletPos();
		Point bulletDir = getAxlBulletDir();

		Point maxPos = bulletPos.add(bulletDir.times(range));

		List<CollideData> hits = Global.level.raycastAll(bulletPos, maxPos, new List<Type>() { typeof(Actor), typeof(Wall) });

		CollideData? hit = null;

		foreach (var p in Global.level.players) {
			if (p.character == null || p.character.getHeadPos() == null) continue;
			Rect headRect = p.character.getHeadRect();

			Point startTestPoint = bulletPos.add(bulletDir.times(-range * 2));
			Point endTestPoint = bulletPos.add(bulletDir.times(range * 2));
			Line testLine = new Line(startTestPoint, endTestPoint);
			Shape headShape = headRect.getShape();
			List<CollideData> lineIntersections = headShape.getLineIntersectCollisions(testLine);
			if (lineIntersections.Count > 0) {
				hits.Add(new CollideData(null, p.character.globalCollider, bulletDir, false, p.character, new HitData(null, new List<Point>() { lineIntersections[0].getHitPointSafe() })));
			}
		}

		hits.Sort((cd1, cd2) => {
			float d1 = bulletPos.distanceTo(cd1.getHitPointSafe());
			float d2 = bulletPos.distanceTo(cd2.getHitPointSafe());
			if (d1 < d2) return -1;
			else if (d1 > d2) return 1;
			else return 0;
		});

		foreach (var h in hits) {
			if (h.gameObject is Wall) {
				hit = h;
				break;
			}
			if (h.gameObject is IDamagable damagable && damagable.canBeDamaged(player.alliance, player.id, null)) {
				retData.hitGos.Add(damagable);
				if (h.gameObject is Character c) {
					if (c.isAlwaysHeadshot()) {
						retData.isHeadshot = true;
					}
					// Detect headshots
					else if (h?.hitData?.hitPoint != null && c.getHeadPos() != null) {
						Point headPos = c.getHeadPos().Value;
						Rect headRect = c.getHeadRect();

						Point hitPoint = h.hitData.hitPoint.Value;
						// Bullet position inside head rect
						if (headRect.containsPoint(bulletPos)) {
							hitPoint = bulletPos;
						}

						float xLeeway = c.headshotRadius * 5f;
						float yLeeway = c.headshotRadius;

						float xDist = MathF.Abs(hitPoint.x - headPos.x);
						float yDist = MathF.Abs(hitPoint.y - headPos.y);

						if (xDist < xLeeway && yDist < yLeeway) {
							Point startTestPoint = bulletPos.add(bulletDir.times(-range * 2));
							Point endTestPoint = bulletPos.add(bulletDir.times(range * 2));
							Line testLine = new Line(startTestPoint, endTestPoint);
							Shape headShape = headRect.getShape();
							List<CollideData> lineIntersections = headShape.getLineIntersectCollisions(testLine);
							if (lineIntersections.Count > 0) {
								retData.isHeadshot = true;
							}
						}
					}
				}
				if (ignoreDamagables == false) {
					hit = h;
					break;
				}
			}
		}

		Point targetPos = hit?.hitData?.hitPoint ?? maxPos;
		if (backOffDist > 0) {
			retData.hitPos = bulletPos.add(bulletPos.directionTo(targetPos).unitInc(-backOffDist));
		} else {
			retData.hitPos = targetPos;
		}

		return retData;
	}


		public Character? getLockOnTarget() {
		Character? newTarget = null;
		foreach (var enemy in Global.level.players) {
			if (enemy.character != null && enemy.character.canBeDamaged(player.alliance, player.id, null) && enemy.character.pos.distanceTo(pos) < 150 && !enemy.character.isStealthy(player.alliance)) {
				float distPercent = 1 - (enemy.character.pos.distanceTo(pos) / 150);
				var dirToEnemy = getAxlBulletPos().directionTo(enemy.character.getAimCenterPos());
				var dirToCursor = getAxlBulletPos().directionTo(axlGenericCursorWorldPos);

				float angle = dirToEnemy.angleWith(dirToCursor);

				float leeway = 22.5f;
				if (angle < leeway + (distPercent * (90 - leeway))) {
					newTarget = enemy.character;
					break;
				}
			}
		}

		return newTarget;
	}

	public void updateArmAngle() {
		Point inputDir = new Point();
		inputDir.y = player.input.getYDir(player);
		inputDir.x = MathF.Abs(player.input.getXDir(player));

		if (charState is Crouch && inputDir.y == 1) {
			inputDir.y = 0;
		}
		float targetAngle = MathF.Round(inputDir.byteAngle) % 256;
		float dist = MathF.Abs(targetAngle - armAngle);
		if (dist > 16) {
			armAngle += Math.Sign(targetAngle - armAngle) * 16;
		} else {
			armAngle = targetAngle;
		}
	}



	

	public override bool canTurn() {
		if (ownedByLocalPlayer && !player.isAI && Options.main.axlDirLock &&
			lockDir && turnCooldown > 0 && charState is not Dash and not AirDash
		) {
			return false;
		}
		return base.canTurn();
	}

	public override bool canCharge() {
		return (axlWeapon is AxlBulletWC && charState is not OcelotSpin && charState.attackCtrl);
	}

	public override void increaseCharge() {
		if (isWhite) {
			chargeTime += Global.speedMul * 1.5f;
		}
		chargeTime += Global.speedMul;
	}


	public void weaponSwapLogic() {
		// Weapon swap cooldown reload.
		foreach (AxlWeaponWC weapon in axlWeapons) {
			if (weapon == mainWeapon) {
				continue;
			}
			if (weapon.swapCooldown == 0) {
				continue;
			}
			weapon.swapCooldown -= speedMul;
			weapon.ammo = weapon.maxAmmo * (1 - weapon.swapCooldown / weapon.maxSwapCooldown);
			if (weapon.swapCooldown <= 0) {
				weapon.swapCooldown = 0;
				weapon.ammo = weapon.maxAmmo;
			}
		}
		// Weapon swap.
		if (axlWeapon == null) {
			weaponSlot = 0;
			return;
		}
		if (!charState.attackCtrl) {
			return;
		}
		int swapDir = 0;
		if (player.input.isPressed(Control.WeaponLeft, player)) {
			swapDir--;
		}
		if (player.input.isPressed(Control.WeaponRight, player)) {
			swapDir++;
		}
		if (swapDir == 0) {
			return;
		}
		// Do Weapon switch.
		if (swapDir == -1 && weaponSlot == 0 || swapDir == 1 && weaponSlot == 2) {
			Weapon oldWeapon = axlWeapon;
			weaponSlot = 1;
			onWeaponChange(oldWeapon, axlWeapon);
		} else {
			AxlWeaponWC targetWeapon = axlWeapons[1 + swapDir];
			if (targetWeapon.swapCooldown <= 0) {
				Weapon oldWeapon = axlWeapon;
				weaponSlot = 1 + swapDir;
				onWeaponChange(oldWeapon, axlWeapon);
			}
		}
	}

	public override void onWeaponChange(Weapon oldWeapon, Weapon newWeapon) {
		// Stop charge if leaving Axl bullets
		bool bulletWasFullAmmo = (mainWeapon.ammo >= mainWeapon.maxAmmo);
		if (oldWeapon == mainWeapon) {
			mainWeapon.ammo = mainWeapon.maxAmmo;
			stopCharge();
			if(chargeAnim != null){
			chargeAnim.destroySelf();
			chargeAnim = null;}
		}
		turnCooldown = 0;
		lockDir = false;

		// Set cooldown if leaving special weapon.
		if (oldWeapon is not AxlWeaponWC axlWeapon) {
			return;
		}
		if (oldWeapon != mainWeapon) {
			axlWeapon.swapCooldown = axlWeapon.maxSwapCooldown;
			axlWeapon.shootCooldown = 2;
			axlWeapon.ammo = 0;
		}
		// Throw the weapon away. Except if it's a full ammo main weapon.
		if (oldWeapon != mainWeapon || !bulletWasFullAmmo) {
			// Speed * 4 with a 255 limit. For netcode.
			int throwSpeed = 0;
			if (vel.y < 0) {
				throwSpeed = MathInt.Ceiling(vel.y * -4);
				if (throwSpeed > 255) { throwSpeed = 255; }
			}
			// Create the weapon object.
			armAngle = 0;
			new AxlDiscrardedWeapon(
				axlWeapon.throwIndex,
				getAxlBulletPos(axlWeapon), armDir, throwSpeed,
				player, player.getNextActorNetId(), true, sendRpc: true
			);
		}
		// Set up the arm angle to look like if it's pulling a weapon.
		armAngle = -64;
		shootCooldown = 64 / 7;
	}

	// Non-attack inputs.
	public override bool normalCtrl() {
		// Handles Standard Hypermode Activations.
		if (player.currency >= WhiteAxlCost && !isWhite &&
			player.input.isHeld(Control.Special2, player) &&
			charState is not HyperZeroStart and not WarpIn
		) {
			hyperProgress += Global.spf;
		} else {
			hyperProgress = 0;
		}
		if (hyperProgress >= 1 && player.currency >= WhiteAxlCost) {
			hyperProgress = 0;
			changeState(new HyperAxlWcStart(grounded), true);
			return true;
		}
		// Dodge.
		if (player.input.checkDoubleTap(Control.Dash) &&
			player.input.isPressed(Control.Dash, player) && canDash() && flag == null ||
			charState is Dash && player.input.isPressed(Control.Down, player)
		) {
			changeState(new DodgeRollAxlWC(), true);
			return true;
		}
		// Hover.
		if (!grounded && player.input.isPressed(Control.Jump, player)
			&& getHoverConditions() && hoverTimes == 0 &&
			canJump() && flag == null
		) {
			hoverTimes++;
			changeState(new HoverAxlWC(), true);
			return true;
		}
		// Block.
		if (Options.main.blockInput && grounded && player.input.isHeld(Control.AxlAimBackwards, player) &&
			charState is not AxlBlock2 and not Dash and not OcelotSpin && axlWeapon?.autoFire == false
		) {
			changeState(new AxlBlock2(), true);
			return true;
		}
		if (!Options.main.blockInput && grounded && player.input.isHeld(Control.Down, player) &&
			charState is not AxlBlock and not Dash and not OcelotSpin && axlWeapon?.autoFire == false
		) {
			changeState(new AxlBlock(), true);
			return true;
		}
		return base.normalCtrl();
	}
	public bool getHoverConditions(){
		if (!Options.main.hoverWhileDown){
		if (player.input.isHeld(Control.Down, player)) return false;}
		return true;
	}


	// Attack inputs.
	public override bool attackCtrl() {
		//if (isCharging()) return
		if (axlWeapon == null) {
			return base.attackCtrl();
		}
		// Custom inputs.
		bool customImputUsed = axlWeapon.attackCtrl(this);
		if (customImputUsed) {
			return true;
		}
		// For stuff we do not want to do mid guard animation.
		if (charState is AxlBlock or AxlBlock2 or RisingBarrage) {
			return false;
		}
		if ((player.input.isHeld(Control.Shoot, player) || axlWeapon.autoFire) &&
			shootCooldown <= 0 &&
			axlWeapon.shootCooldown <= 0 &&
			axlWeapon.ammo > 0
		) {
			shootMain(axlWeapon);
			return true;
		}
		if (player.input.isHeld(Control.Special1, player) &&
			shootCooldown <= 0 && axlWeapon.shootCooldown <= 0 &&
			axlWeapon is not AxlBulletWC &&
			axlWeapon.ammo > 0
		) {
			shootAlt(axlWeapon);
			return true;
		}
		return base.attackCtrl();
	}

	// Shoots stuff.
	public void shootMain(AxlWeaponWC? weapon) {
		if (weapon == null) {
			return;
		}
		float shootAngle = armAngle;
		if (armDir < 0) {
			shootAngle = shootAngle * -1 + 128;
		}
		Point shootPos = getAxlBulletPos(axlWeapon);
		weapon.shootMain(this, shootPos, shootAngle, 0);
		weapon.shootCooldown = weapon.getFireRate(this, 0);
		recoilTime = weapon.getRecoil(this, 0);
		turnCooldown = weapon.shootCooldown + 2;
		lockDir = true;
		weapon.addAmmo(-weapon.getAmmoUse(this, 0), player);
		if (weapon.shootSounds[0] != "") {
			playSound(weapon.shootSounds[0], true, true);
		}
		if (weapon.flashSprite != "") {
			if (muzzleFlash?.destroyed == false) {
				muzzleFlash.destroySelf();
			}
			muzzleFlash = new Anim(
				shootPos, weapon.flashSprite, xDir,
				player.getNextActorNetId(), true, sendRpc: true
			);
		}
	}

	public void shootAlt(AxlWeaponWC? weapon) {
		if (weapon == null) {
			return;
		}
		float shootAngle = armAngle;
		if (armDir < 0) {
			shootAngle = shootAngle * -1 + 128;
		}
		Point shootPos = getAxlBulletPos(axlWeapon);
		weapon.shootAlt(this, shootPos, shootAngle, 0);
		weapon.shootCooldown = weapon.getAltFireRate(this, 0);
		recoilTime = weapon.getAltRecoil(this, 0);
		turnCooldown = weapon.shootCooldown + 2;
		lockDir = true;
		weapon.addAmmo(-weapon.getAltAmmoUse(this, 0), player);
		if (weapon.shootSounds[1] != "") {
			playSound(weapon.shootSounds[1], true, true);
		}
		if (weapon.chargedFlashSprite != "") {
			if (muzzleFlash?.destroyed == false)  {
				muzzleFlash.destroySelf();
			}
			muzzleFlash = new Anim(
				shootPos, weapon.chargedFlashSprite, xDir,
				player.getNextActorNetId(), true, sendRpc: true
			);
		}
	}

	public void chargeShoot(int chargeLevel) {
		if (axlWeapon is not AxlBulletWC axlBullet) {
			return;
		}
		if (chargeLevel < getMaxChargeLevel()) {
			return;
		}
		float shootAngle = armAngle;
		if (armDir < 0) {
			shootAngle = shootAngle * -1 + 128;
		}
		if(chargeAnim != null){
			chargeAnim.destroySelf();
			chargeAnim = null;
		}
		Point shootPos = getAxlBulletPos(axlWeapon);
		axlBullet.shootAlt(this, shootPos, shootAngle, chargeLevel);
		axlBullet.shootCooldown = axlBullet.getAltFireRate(this, 0);
		recoilTime = axlBullet.getAltRecoil(this, 0);
		turnCooldown = axlBullet.shootCooldown + 2;
		lockDir = true;
		axlBullet.addAmmo(-axlBullet.getAltAmmoUse(this, chargeLevel), player);
		if (recoilTime > 12) {
			recoilTime = 12;
		}
		if (axlBullet.shootSounds[1] != "") {
			playSound(axlBullet.shootSounds[3], true, true);
		}
		if (axlBullet.chargedFlashSprite != "") {
			if (muzzleFlash?.destroyed == false)  {
				muzzleFlash.destroySelf();
			}
			muzzleFlash = new Anim(
				shootPos, axlBullet.chargedFlashSprite, xDir,
				player.getNextActorNetId(), true, sendRpc: true
			);
		}
		stopCharge();
	}
	public bool getAutoChargeConditions(){
		if(axlWeapon is not AxlBulletWC || !Options.main.autoCharge) return false;
		return true;
		
	}

	public override bool chargeButtonHeld() {
		if(autoChargeCooldown > 0) return false;
		if(getAutoChargeConditions()){
			return true;
		}
		return player.input.isHeld(Control.Special1, player);
	}

	public override bool canAddAmmo() {
		if (axlWeapon != null && axlWeapon.ammo < axlWeapon.maxAmmo && axlWeapon.canHealAmmo) {
			return true;
		}
		return false;
	}

	public override void addAmmo(float amount) {
		axlWeapon?.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		axlWeapon?.addAmmoPercentHeal(amount);
	}

	public Point getAxlBulletPos(AxlWeaponWC? weapon) {
		if (weapon == null) {
			return pos;
		}
		AnimData animData = Global.sprites[weapon.sprite];
		Point shootOrigin = getAxlArmOrigin(weapon);
		if (animData.frames[0].POIs.Length == 0) {
			return shootOrigin;
		}
		float angle = armAngle;
		if (armDir < 0) {
			angle = (angle * -1) % 256;
		}
		Point poi = animData.frames[0].POIs[0];
		poi.x *= armDir;
		Point angleDir = Point.createFromByteAngle(
			angle + poi.byteAngle
		).times(
			poi.magnitude
		);

		return shootOrigin.addxy(angleDir.x, angleDir.y);
	}


	public void addDNACore(Character hitChar) {
		if (!player.ownedByLocalPlayer) return;
		if (!player.isAxlWC) return;
		if (Global.level.is1v1()) return;

		if (player.weapons.Count((Weapon weapon) => weapon is DNACore) < 4) {
			var dnaCoreWeapon = new DNACore(hitChar);
			dnaCoreWeapon.index = (int)WeaponIds.DNACore - player.weapons.Count;
			if (player.isDisguisedAxl) {
				player.oldWeapons.Add(dnaCoreWeapon);
			} else {
				player.weapons.Add(dnaCoreWeapon);
			}
			player.savedDNACoreWeapons.Add(dnaCoreWeapon);
		}
	}



	public Point getAxlArmOrigin(AxlWeaponWC? weapon) {
		if (weapon == null) {
			return pos;
		}
		Point retPoint;
		var pois = sprite.getCurrentFrame().POIs;
		Point roundPos = new Point(MathInt.Round(pos.x), MathInt.Round(pos.y));
		if (pois.Length > 0) {
			retPoint = roundPos.addxy((pois[0].x + 2) * armDir, pois[0].y);
		} else {
			retPoint = roundPos.addxy(6 * armDir, -21);
		}
		if (axlWeapon?.isTwoHanded == true) {
			retPoint = retPoint.addxy(-7 * armDir, 2);
		}
		return retPoint;
	}


	public enum MeleeIds {
		None = -1,
		Block,
		String1,
		String2,
		String3,
		String4,
		String5,
		OcelotSpin,
		TailShot,
		EnemyStep,
		RainStorm,
		RainDrop,
		SpinKick,
		RollBump,
		RisingBarrage
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"axl_block" => MeleeIds.Block,
			"axl_ocelotspin" => MeleeIds.OcelotSpin,
			"axl_string_1" => MeleeIds.String1,
			"axl_string_2" => MeleeIds.String2,
			"axl_string_3" => MeleeIds.String3,
			"axl_string_4" => MeleeIds.String4,
			"axl_string_5" => MeleeIds.String5,
			"axl_tailshot" => MeleeIds.TailShot,
			"axl_risingbarrage" or "axl_flashkick" => MeleeIds.RisingBarrage,
			"axl_rainstorm" => MeleeIds.RainStorm,
			"axl_fall_step" => MeleeIds.EnemyStep,
			"axl_rollbump" => MeleeIds.RollBump,
			"axl_spinkick" => MeleeIds.SpinKick,
			"axl_raindrop" => MeleeIds.RainDrop,
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Block => new GenericMeleeProj(
				ZSaber.netWeapon, pos, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true,
				addToLevel: addToLevel
			),
			MeleeIds.EnemyStep => new GenericMeleeProj(
				new RCXPunch(), pos, ProjIds.GBDKick, player,
			 2, Global.halfFlinch, addToLevel: addToLevel, ShouldClang: true
			),
			MeleeIds.RainStorm => new GenericMeleeProj(
				new RCXPunch(), pos, ProjIds.ForceGrabState, player,
			 2, 0, addToLevel: addToLevel, ShouldClang: true
			),
			MeleeIds.OcelotSpin => new GenericMeleeProj(
				ShotgunIce.netWeapon, pos, ProjIds.ZSaber1, player,
				1, Global.halfFlinch, 4, ShouldClang: true, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			MeleeIds.TailShot => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.FireWave, player,
				3, Global.defFlinch, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			MeleeIds.String1 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.ShotgunIce, player,
				2, Global.defFlinch, 6,
				addToLevel: addToLevel
			),
			MeleeIds.String2 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.VirusSlash, player,
				2, Global.defFlinch, 6,
				addToLevel: addToLevel
			),
			MeleeIds.String3 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.AcidBurst, player,
				2, Global.defFlinch, 5,
				addToLevel: addToLevel
			),
			MeleeIds.String4 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.Raijingeki2, player,
				2, 0, 6,
				addToLevel: addToLevel
			),
			MeleeIds.String5 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.HeavyPush, player,
				2, 0, 6,
				addToLevel: addToLevel
			),
			MeleeIds.RisingBarrage => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.BlockableLaunch, player,
				3, 0, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			MeleeIds.RainDrop => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.SpreadShot, player,
				3, 0,
				addToLevel: addToLevel
			),
			MeleeIds.SpinKick => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.VileAirRaidStart, player,
				2, 30,
				addToLevel: addToLevel
			),
			MeleeIds.RollBump => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.MechFrogStompShockwave, player,
				2, 0,
				addToLevel: addToLevel
			),
			_ => null
		};
	}

	public void configureWeapons() {
		AxlWCLoadout axlLoadout = player.loadout.axlWCLoadout;
		axlWeapons.Add(getAllSpecialWeapons()[axlLoadout.weapon1]);
		axlWeapons.Add(getAllMainWeapons()[axlLoadout.sideArm]);
		axlWeapons.Add(getAllSpecialWeapons()[axlLoadout.weapon2]); 
		mainWeapon = axlWeapons[1];
		weaponSlot = 1;
		weapons = axlWeapons.Cast<Weapon>().ToList();
	}
	

	public AxlWeaponWC getWeaponFromIndex(int index) {
		return index switch {
			1 => new RayGunWC(),
			2 => new BlastLauncherWC(),
			3 => new BlackArrowWC(),
			4 => new SpiralMagnumWC(),
			5 => new BoundBlasterWC(),
			6 => new PlasmaGunWC(),
			7 => new IceGattlingWC(),
			8 => new FlameBurnerWC(),
			_ => new AxlBulletWC()
		};
	}

	public override string getSprite(string spriteName) {
		if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("axlalt_" + spriteName)){		
			return "axlalt_" + spriteName;
			}
		return "axl_" + spriteName;
	}


	public override List<ShaderWrapper> getShaders() {
		var shaders = new List<ShaderWrapper>();
		ShaderWrapper? palette = null;

		int paletteNum = 0;
		if (isWhite) { paletteNum = 1; }
		palette = player.axlPaletteShader;
		palette?.SetUniform("palette", paletteNum);
		palette?.SetUniform("paletteTexture", Global.textures["hyperAxlPalette"]);

		if (palette != null) {
			shaders.Add(palette);
		}
		shaders.AddRange(base.getShaders());

		return shaders;
	}

	public bool shouldDrawArm() {
		if (charState is LadderClimb or LadderEnd) {
			return false;
		}
		return (!isWarpIn() &&
			sprite.name != "axl_win" &&
			sprite.name != "axl_lose" &&
			charState is not Hurt and
			not AxlBlock and
			not AxlBlock2 and
			not Die and
			not GenericStun and
			not InRideArmor and
			not DodgeRollAxlWC and
			not VileMK2Grabbed and
			not KnockedDown and
			not DeadLiftGrabbed and
			not MammothSlammed and
			not UPGrabbed and
			not WhirlpoolGrabbed and
			not RainStorm and
			not EvasionBarrage and
			not RisingBarrage and
			not OcelotSpin and
			not TailShot and
			not AxlString1 and
			not AxlString2 and
			not AxlString3 and
			not AxlString4 and
			not AxlString5 and
			not AxlRainDrop and
			not AxlSpinKick and
			not AxlRollBump and
			not InRideChaser and
			not LadderEnd and
			not InRideChaser and
			not AxlFlashKick
		);
	}


	float axlCursorAngle;


		public void drawBloom() {
		Global.sprites["axl_cursor_top"].draw(0, axlCursorWorldPos.x, axlCursorWorldPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
		Global.sprites["axl_cursor_bottom"].draw(0, axlCursorWorldPos.x, axlCursorWorldPos.y + 1, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
		Global.sprites["axl_cursor_left"].draw(0, axlCursorWorldPos.x, axlCursorWorldPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
		Global.sprites["axl_cursor_right"].draw(0, axlCursorWorldPos.x + 1, axlCursorWorldPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
		Global.sprites["axl_cursor_dot"].draw(0, axlCursorWorldPos.x, axlCursorWorldPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
	}
	
	public void drawAxlCursor() {
		if (!ownedByLocalPlayer) return;
		if (Global.level.gameMode.isOver) return;
		if (isZooming() && !isZoomOutPhase1Done) return;
		// if (isWarpIn()) return;

		if (Options.main.useMouseAim || Global.showHitboxes) {
			drawBloom();
			Global.sprites["axl_cursor"].draw(0, axlCursorWorldPos.x, axlCursorWorldPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
			if (player.assassinHitPos?.isHeadshot == true && player.weapon is AssassinBullet && Global.level.isTraining()) {
				Global.sprites["hud_kill"].draw(0, axlCursorWorldPos.x, axlCursorWorldPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
			}
		}
		if (!Options.main.useMouseAim) {
			if (player.axlWeapon != null && (player.axlWeapon is AssassinBullet || player.input.isCursorLocked(player))) {
				Point bulletPos = getAxlBulletPos();
				float radius = 120;
				float ang = getShootAngle();
				float x = Helpers.cosd(ang) * radius * getShootXDir();
				float y = Helpers.sind(ang) * radius * getShootXDir();
				DrawWrappers.DrawLine(bulletPos.x, bulletPos.y, bulletPos.x + x, bulletPos.y + y, new Color(255, 0, 0, 128), 2, ZIndex.HUD, true);
				if (axlCursorTarget != null && player.assassinHitPos?.isHeadshot == true && player.weapon is AssassinBullet && Global.level.isTraining()) {
					Global.sprites["hud_kill"].draw(0, axlLockOnCursorPos.x, axlLockOnCursorPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1);
				}
			}
			if (axlCursorTarget != null && !isAnyZoom()) {
				axlCursorAngle += Global.spf * 360;
				if (axlCursorAngle > 360) axlCursorAngle -= 360;
				Global.sprites["axl_cursor_x7"].draw(0, axlLockOnCursorPos.x, axlLockOnCursorPos.y, 1, 1, null, 1, 1, 1, ZIndex.Default + 1, angle: axlCursorAngle);
				//drawBloom();
			}
		}

		/*
		if (player.weapon.ammo <= 0)
		{
			if (player.weapon.rechargeCooldown > 0)
			{
				float textPosX = axlCursorPos.x;
				float textPosY = axlCursorPos.y - 20;
				if (!Options.main.useMouseAim)
				{
					textPosX = pos.x - Global.level.camX / Global.viewSize;
					textPosY = (pos.y - 50 - Global.level.camY) / Global.viewSize;
				}
				DrawWrappers.DeferTextDraw(() =>
				{
					Helpers.drawTextStd(
						"Reload:" + player.weapon.rechargeCooldown.ToString("0.0"),
						textPosX, textPosY, Alignment.Center, fontSize: 20,
						outlineColor: Helpers.getAllianceColor()
					);
				});
			}
		}
		*/
	}


	public override void render(float x, float y) {
		base.render(x, y);


		netArmAngle = getShootAngle();
		drawAxlCursor();



		if (!shouldRender(x, y) || !shouldDraw() || !visible) {
			return;
		}
		float angleOffset = 0;
		if (recoilTime > 0) {
			angleOffset = -recoilTime;
		}
		if (ownedByLocalPlayer && shouldDrawArm() || !ownedByLocalPlayer && shouldDrawArmNet == true) {
			drawArm(armAngle + angleOffset);
		}
	}

	public void drawArm(float armAngle) {
		long armZIndex = zIndex - 1;
		if (axlWeapon?.isTwoHanded == true) {
			armZIndex = zIndex + 1;
		}
		if (armDir < 0) {
			armAngle = armAngle * -1;
		}
		Point gunOrigin = getAxlArmOrigin(axlWeapon);

		AnimData axlSprite = Global.sprites[axlWeapon?.sprite ?? "axl_arm_pistol"];
		Point offsets = new Point(axlSprite.frames[0].offset.x, axlSprite.frames[0].offset.y);

		axlSprite.draw(
			axlWeapon?.spriteFrameIndex ?? 0, gunOrigin.x + offsets.x * armDir, gunOrigin.y + offsets.y, armDir, 1,
			getRenderEffectSet(), 1, 1, 1, armZIndex, angle: Helpers.byteToDegree(armAngle), shaders: getShaders()
		);
	}

	public Point getAxlGunArmOrigin() {
		Point retPoint;
		var pois = sprite.getCurrentFrame().POIs;
		Point roundPos = new Point(MathInt.Round(pos.x), MathInt.Round(pos.y));
		if (pois.Length > 0) {
			retPoint = roundPos.addxy(pois[0].x * armDir, pois[0].y);
		} else {
			retPoint = roundPos.addxy(3 * armDir, -21);
		}
		return retPoint;
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add((byte)armAngle);
		customData.Add((byte)weaponSlot);
		customData.Add((byte)recoilTime);
		customData.Add((byte)(axlWeapon?.spriteFrameIndex ?? 0));

		customData.Add(Helpers.boolArrayToByte([
			shouldDrawArm(),
			isWhite,
		]));
		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];
		armAngle = data[0];
		weaponSlot = data[1];
		recoilTime = data[2];
		if (axlWeapon != null) {
			axlWeapon.spriteFrameIndex = data[3];
		}

		bool[] flags = Helpers.byteToBoolArray(data[4]);
		shouldDrawArmNet = flags[0];
		isWhite = flags[1];
	}
}
public enum ThrowID{
	AxlBullet,
	RayGun,
	SpiralMagnum,
	BoundBlaster,
	PlasmaGun,
	BlackArrow,
	BlastLauncher,
	FlameBurner,
	IceGattling
}
public class AxlDiscrardedWeapon : Actor {
	public float time;
	public float currentAngle;
	public float blinkActiveTime = 6;
	public float blinkMaxTime = 11;
	public float blinkTime = 11;

	public AxlDiscrardedWeapon(
		int type, Point pos, int xDir, int throwSpeed,
		Player player, ushort? netId, bool ownedByLocalPlayer,
		bool sendRpc = false
	) : base(
		"axl_discardedweapon", pos, netId, ownedByLocalPlayer, false
	) {
		canBeLocal = true;
		useGravity = true;
		sprite.frameSpeed = 0;
		sprite.frameIndex = type;
		collider.wallOnly = true;
		this.xDir = xDir;

		if (sprite.getCurrentFrame().POIs.Length > 0) {
			Point offset = sprite.getCurrentFrame().POIs[0];
			offset.x *= xDir;
			this.pos -= offset;
		}
		vel.x = -40 * xDir;
		vel.y = -200 - (throwSpeed / 3f);

		if (sendRpc) {
			rpcCreateActor(ProjIds.AxlDiscardedWeapon, pos, player, netId, xDir, [(byte)type, (byte)throwSpeed]);
		}
	}

	public override void preUpdate() {
		time += speedMul;
		Helpers.decrementFrames(ref blinkTime);
	}

	public override void update() {
		base.update();
		if (grounded) {
			vel.x = 0;
			byteAngle = 0;
		} else {
			if (currentAngle < 12) {
				currentAngle += 1;
				if (currentAngle > 12) { currentAngle = 12; }
			}
			byteAngle = -currentAngle * xDir;
		}
		if (blinkTime == 0) {
			if (blinkMaxTime > 3) {
				blinkActiveTime--;
				blinkMaxTime -= 2;
			}
			blinkTime = blinkMaxTime;
		}
		if (time >= 60) {
			destroySelf(disableRpc: true);
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (blinkTime < blinkActiveTime && blinkActiveTime < 6) {
			visible = false;
		} else {
			visible = true;
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		new AxlDiscrardedWeapon(
			args.extraData[0], args.pos, args.xDir, args.extraData[1], args.player, args.netId, false
		);
		return null!;
	}
}
