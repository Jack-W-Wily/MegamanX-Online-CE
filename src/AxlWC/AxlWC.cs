using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace MMXOnline;

public class AxlWC : Character {
	public AxlWeapon mainWeapon;
	public const int WhiteAxlCost = 5;
	public bool isWhite;
	public float whiteTime;
	public float dodgeRollCooldown;
	public float aiAttackCooldown;
	public float shootCooldown;
	public float recoilTime;
	public int axlArmDir => charState is WallSlide ? -xDir : xDir;
	public float armAngle = 0;
	public bool wasMainWeapon;
	public bool wasSpecialPressed;
	public Anim muzzleFlash;

	public AxlWC(
		Player player, float x, float y, int xDir, bool isVisible,
		ushort? netId, bool ownedByLocalPlayer, bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.Axl;
		spriteFrameToSounds["axl_run/4"] = "run";
		spriteFrameToSounds["axl_run/8"] = "run";
		configureWeapons();
		muzzleFlash = new Anim(pos, "axl_pistol_flash", xDir, null, false);
		muzzleFlash.visible = false;
		muzzleFlash.frameIndex = muzzleFlash.sprite.totalFrameNum - 1;
		muzzleFlash.frameTime = currentFrame.duration;
		mainWeapon = (weapons.First((Weapon w) => w is AxlBullet or DoubleBullet) as AxlWeapon)!;
	}

	public override void preUpdate() {
		base.preUpdate();
		// Cooldowns.
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref recoilTime);
		if (currentWeapon != mainWeapon) {
			wasMainWeapon = false;
		}
		else if (!wasMainWeapon) {
			wasMainWeapon = player.input.isPressed(Control.Special1, player);
		}
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) {
			return;
		}
		// Arm angle.
		updateArmAngle();
		// Charge and release charge logic.
		chargeLogic(chargeShoot);
		// Weapon swap cooldown reload.
		foreach (Weapon weapon in weapons) {
			if (weapon == mainWeapon || weapon is not AxlWeapon axlWeapon) {
				continue;
			}
			if (axlWeapon.swapCooldown == 0) {
				continue;
			}
			axlWeapon.swapCooldown -= speedMul;
			axlWeapon.ammo = axlWeapon.maxAmmo * (1 - axlWeapon.swapCooldown / axlWeapon.maxSwapCooldown);
			if (axlWeapon.swapCooldown <= 0) {
				axlWeapon.swapCooldown = 0;
				axlWeapon.ammo = axlWeapon.maxAmmo;
			}
		}
		// Weapon swap.
		if (currentWeapon == null) {
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
			Weapon oldWeapon = currentWeapon;
			weaponSlot = 1;
			onWeaponChange(oldWeapon, currentWeapon);
		} else {
			Weapon targetWeapon = weapons[1 + swapDir];
			if (targetWeapon is AxlWeapon axlWeapon && axlWeapon.swapCooldown <= 0) {
				Weapon oldWeapon = currentWeapon;
				weaponSlot = 1 + swapDir;
				onWeaponChange(oldWeapon, currentWeapon);
			}
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (!muzzleFlash.isAnimOver()) {
			float oldByteArmAngle = armAngle;
			if (recoilTime > 0) {
				armAngle = MathF.Round(armAngle - recoilTime);
			}
			muzzleFlash.pos = getAxlBulletPos();
			muzzleFlash.xDir = axlArmDir;
			muzzleFlash.visible = true;
			muzzleFlash.byteAngle = armAngle * axlArmDir;
			muzzleFlash.zIndex = ZIndex.Default;
			if (muzzleFlash.sprite.name == "axl_raygun_flash") {
				muzzleFlash.zIndex = zIndex - 100;
			}
			armAngle = oldByteArmAngle;
		} else {
			muzzleFlash.visible = false;
		}
		// For negative edge.
		if (currentWeapon == mainWeapon && wasMainWeapon) {
			wasSpecialPressed = player.input.isHeld(Control.Special1, player);
		} else {
			wasSpecialPressed = false;
		}
		// Swap on ammo empitiying.
		if (currentWeapon != mainWeapon && currentWeapon is AxlWeapon axlWP &&
			axlWP.ammo <= 0 && recoilTime <= 0
		) {
			Weapon oldWeapon = currentWeapon;
			weaponSlot = 1;
			onWeaponChange(oldWeapon, currentWeapon);
			return;
		}
	}

	public override bool canCrouch() {
		return false;
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

	public override bool canCharge() {
		return (currentWeapon is AxlBullet or DoubleBullet && charState is not OcelotSpin);
	}

	public override void onWeaponChange(Weapon oldWeapon, Weapon newWeapon) {
		// Stop charge if leaving Axl bullets.
		if (oldWeapon == mainWeapon) {
			mainWeapon.ammo = mainWeapon.maxAmmo;
			stopCharge();
		}
		// Set cooldown if leaving special weapon.
		else if (oldWeapon is AxlWeapon axlWeapon) {
			axlWeapon.swapCooldown = axlWeapon.maxSwapCooldown;
			axlWeapon.shootCooldown = 2;
			axlWeapon.ammo = 0;
		}
		// Throw the weapon away. Except if it's a full ammo main weapon.
		if (oldWeapon != mainWeapon || mainWeapon.ammo < mainWeapon.maxAmmo) {
			// Speed * 4 with a 255 limit. For netcode.
			int throwSpeed = 0;
			if (vel.y < 0) {
				throwSpeed = MathInt.Ceiling(vel.y * -4);
				if (throwSpeed > 255) { throwSpeed = 255; }
			}
			// Create the weapon object.
			new AxlDiscrardedWeapon(
				oldWeapon.index - (int)WeaponIds.AxlBullet,
				getAxlBulletPos(), axlArmDir, throwSpeed,
				player, player.getNextActorNetId(), true, sendRpc: true
			);
		}
		// Set up the arm angle to look like if it's pulling a weapon.
		armAngle = -64;
		shootCooldown = 64/7;
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
		// Hover.
		if (!grounded && player.input.isPressed(Control.Jump, player) &&
			canJump() && !isDashing && canAirDash() && flag == null
		) {
			dashedInAir++;
			changeState(new HoverAxlWC(), true);
			return true;
		}
		// Block.
		if (grounded && player.input.isHeld(Control.Down, player) && charState is not AxlBlock and not Dash)  {
			changeState(new AxlBlock(), true);
			return true;
		}
		return base.normalCtrl();
	}

	// Attack inputs.
	public override bool attackCtrl() {
		if (isCharging() || currentWeapon == null) {
			return base.attackCtrl();
		}
		if (currentWeapon == mainWeapon) {
			Point inputDir = player.input.getInputDir(player);
			bool specialPressed = wasSpecialPressed && !player.input.isHeld(Control.Special1, player);
			// Shoryken does not use negative edge at all.
			if (player.input.checkShoryuken(player, xDir, Control.Special1) && currentWeapon.ammo > 0) {
				changeState(new RainStorm(), true);
				return true;
			}
			// Negative edge inputs.
			if (inputDir.y == 1 && charState is Dash && currentWeapon.ammo > 0) {
				changeState(new RisingBarrage(), true);
				return true;
			}
			if (specialPressed && inputDir.y == -1 && currentWeapon.ammo > 0) {
				changeState(new TailShot(), true);
				return true;
			}
			if (specialPressed && (inputDir.y == 1 || currentWeapon.ammo == 0)) {
				changeState(new OcelotSpin(), true);
				return true;
			}
			if (specialPressed && currentWeapon.ammo > 0) {
				vel.y = -getJumpPower() * 2f;
				changeState(new EvasionBarrage(), true);
				return true;
			}
		}
		// For stuff we do not want to do mid guard animation.
		if (charState is AxlBlock) {
			return false;
		}
		if ((player.input.isHeld(Control.Shoot, player) || currentWeapon is PlasmaGun) &&
			shootCooldown <= 0 &&
			currentWeapon.shootCooldown <= 0 &&
			currentWeapon.ammo > 0
		) {
			shootMain();
			return true;
		}
		if (player.input.isHeld(Control.Special1, player) &&
			shootCooldown <= 0 && currentWeapon.shootCooldown <= 0 &&
			currentWeapon is not AxlBullet &&
			currentWeapon.ammo > 0
		) {
			shootAlt();
			return true;
		}
		return base.attackCtrl();
	}

	// Shoots stuff.
	public void shootMain() {
		if (currentWeapon is AxlWeapon axlWeapon) {
			float shootAngle = armAngle;
			if (axlArmDir < 0) {
				shootAngle = shootAngle * -1 + 128;
			}
			axlWeapon.axlGetProjectile(
				axlWeapon, getAxlBulletPos(), axlArmDir, player, Helpers.byteToDegree(shootAngle),
				null, null, pos, 0, player.getNextActorNetId()
			);
			axlWeapon.shootCooldown = axlWeapon.fireRate;
			recoilTime = axlWeapon.fireRate - 4;
			axlWeapon.addAmmo(-axlWeapon.getAmmoUsage(0), player);
			if (recoilTime > 12) {
				recoilTime = 12;
			}
			if (axlWeapon.shootSounds[0] != "") {
				playSound(axlWeapon.shootSounds[0], true, true);
			}
			if (axlWeapon.flashSprite != "") {
				muzzleFlash.changeSprite(axlWeapon.flashSprite, true);
				muzzleFlash.visible = true;
			} 
		}
	}

	public void shootAlt() {
		if (currentWeapon is AxlWeapon axlWeapon) {
			float shootAngle = armAngle;
			if (axlArmDir < 0) {
				shootAngle = shootAngle * -1 + 128;
			}
			axlWeapon.axlGetProjectile(
				axlWeapon, getAxlBulletPos(), axlArmDir,
				player, Helpers.byteToDegree(shootAngle),
				null, null, pos, axlWeapon is AxlBullet ? 1 : 3, player.getNextActorNetId()
			);
			axlWeapon.shootCooldown = axlWeapon.altFireCooldown;
			recoilTime = axlWeapon.altFireCooldown - 4;
			axlWeapon.addAmmo(-axlWeapon.getAmmoUsage(3), player);
			if (recoilTime > 12) {
				recoilTime = 12;
			}
			if (axlWeapon.shootSounds[3] != "") {
				playSound(axlWeapon.shootSounds[3], true, true);
			}
			if (axlWeapon.chargedFlashSprite != "") {
				muzzleFlash.changeSprite(axlWeapon.flashSprite, true);
				muzzleFlash.visible = true;
			}
			else if (axlWeapon.chargedFlashSprite != "") {
				muzzleFlash.changeSprite(axlWeapon.flashSprite, true);
				muzzleFlash.visible = true;
			}
		}
	}

	public void chargeShoot(int chargeLevel) {
		if (currentWeapon is AxlBullet axlBullet) {
			float shootAngle = armAngle;
			if (axlArmDir < 0) {
				shootAngle = shootAngle * -1 + 128;
			}
			axlBullet.axlGetProjectile(
				axlBullet, getAxlBulletPos(), axlArmDir, player, Helpers.byteToDegree(shootAngle),
				null, null, pos, chargeLevel + 1, player.getNextActorNetId()
			);
			axlBullet.shootCooldown = axlBullet.altFireCooldown;
			recoilTime = axlBullet.altFireCooldown - 4;
			axlBullet.addAmmo(-axlBullet.getAmmoUsage(chargeLevel + 1), player);
			if (recoilTime > 12) {
				recoilTime = 12;
			}
			if (axlBullet.shootSounds[3] != "") {
				playSound(axlBullet.shootSounds[3], true, true);
			}
			muzzleFlash.visible = true;
			muzzleFlash.frameIndex = 0;
			if (axlBullet.chargedFlashSprite != "") {
				muzzleFlash.changeSprite(axlBullet.flashSprite, true);
				muzzleFlash.visible = true;
			}
		}
	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Special1, player);
	}

	public override bool canAddAmmo() {
		if (mainWeapon.ammo < mainWeapon.maxAmmo && currentWeapon == mainWeapon) {
			return true;
		}
		return false;
	}

	public override void addAmmo(float amount) {
		if (currentWeapon != mainWeapon) {
			mainWeapon.addAmmo(amount, player);
			playSound("subtankFill", true);
		}
		mainWeapon.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		if (currentWeapon != mainWeapon) {
			mainWeapon.addAmmoPercent(amount);
			playSound("subtankFill", true);
		}
		mainWeapon.addAmmoPercentHeal(amount);
	}
	
	public Point getAxlBulletPos() {
		AnimData animData = Global.sprites[player.axlWeapon?.sprite ?? "axl_arm_pistol"];;
		Point shootOrigin = getAxlArmOrigin();
		if (animData.frames[0].POIs.Length == 0) {
			return shootOrigin;
		}
		float angle = armAngle;
		if (axlArmDir < 0) {
			angle = (angle * -1) % 256;
		}
		Point poi = animData.frames[0].POIs[0];
		poi.x *= axlArmDir;
		Point angleDir = Point.createFromByteAngle(
			angle + poi.byteAngle
		).times(
			poi.magnitude
		);

		return shootOrigin.addxy(angleDir.x, angleDir.y);
	}

	public Point getAxlArmOrigin() {
		Point retPoint;
		var pois = sprite.getCurrentFrame().POIs;
		Point roundPos = new Point(MathInt.Round(pos.x), MathInt.Round(pos.y));
		if (pois.Length > 0) {
			retPoint = roundPos.addxy((pois[0].x + 2) * axlArmDir, pois[0].y);
		} else {
			retPoint = roundPos.addxy(6 * axlArmDir, -21);
		}
		if ((currentWeapon as AxlWeapon)?.isTwoHanded(true) == true) {
			retPoint = retPoint.addxy(-7 * axlArmDir, 2);
		}
		return retPoint;
	}


	public enum MeleeIds {
		None = -1,
		Block,
		OcelotSpin,
		TailShot,
		RisingBarrage
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"axl_block" => MeleeIds.Block,
			"axl_ocelotspin" => MeleeIds.OcelotSpin,
			"axl_tailshot" => MeleeIds.TailShot,
			"axl_risingbarrage" => MeleeIds.RisingBarrage,
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
			MeleeIds.OcelotSpin => new GenericMeleeProj(
				ShotgunIce.netWeapon, pos, ProjIds.ZSaber1, player,
				1, 0, 4, ShouldClang: true, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			MeleeIds.TailShot => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.FireWave, player,
				3, Global.halfFlinch, 5, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			MeleeIds.RisingBarrage => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.FireWave, player,
				3, 0, 8, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			_ => null
		};
	}

	public void configureWeapons() {
		if (Global.level.isTraining() && !Global.level.server.useLoadout) {
			weapons = Weapon.getAllAxlWeapons(player.axlLoadout).Select(w => w.clone()).ToList();
			weapons[0] = new AxlBullet(0);
		} else if (Global.level.is1v1()) {
			weapons.Add(new AxlBullet());
			weapons.Add(new RayGun(player.axlLoadout.rayGunAlt));
			weapons.Add(new BlastLauncher(player.axlLoadout.blastLauncherAlt));
			weapons.Add(new BlackArrow(player.axlLoadout.blackArrowAlt));
			weapons.Add(new SpiralMagnum(player.axlLoadout.spiralMagnumAlt));
			weapons.Add(new BoundBlaster(player.axlLoadout.boundBlasterAlt));
			weapons.Add(new PlasmaGun(player.axlLoadout.plasmaGunAlt));
			weapons.Add(new IceGattling(player.axlLoadout.iceGattlingAlt));
			weapons.Add(new FlameBurner(player.axlLoadout.flameBurnerAlt));
		} else {
			weapons = player.loadout.axlLoadout.getWeaponsFromLoadout();
			weapons.Insert(1, new AxlBullet(0));
			weaponSlot = 1;
		}
		if (ownedByLocalPlayer) {
			foreach (var dnaCore in player.savedDNACoreWeapons) {
				weapons.Add(dnaCore);
			}
		}
		if (weapons[0].type > 0) {
			weapons[0].ammo = player.axlBulletTypeLastAmmo[weapons[0].type];
		}
	}

	public override string getSprite(string spriteName) {
		return "axl_" + spriteName;
	}

	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
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
			not Die and
			not GenericStun and
			not InRideArmor and
			not DodgeRoll and
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
			not InRideChaser and 
			not LadderEnd
		);
	}

	public override void render(float x, float y) {
		base.render(x, y);
		if (!shouldRender(x, y) || !shouldDraw() || !visible) {
			return;
		}
		float angleOffset = 0;
		if (recoilTime > 0) {
			angleOffset = -recoilTime;
		}

		if (shouldDrawArm()) {
			drawArm(armAngle + angleOffset);
		}
	}

	public void drawArm(float armAngle) {
		long armZIndex = zIndex - 1;
		if ((currentWeapon as AxlWeapon)?.isTwoHanded(true) == true) {
			armZIndex = zIndex + 1;
		}
		if (axlArmDir < 0) {
			armAngle = armAngle * -1;
		}
		Point gunOrigin = getAxlArmOrigin();

		AnimData axlSprite = Global.sprites[player.axlWeapon?.sprite ?? "axl_arm_pistol"];
		Point offsets = new Point(axlSprite.frames[0].offset.x, axlSprite.frames[0].offset.y);

		axlSprite.draw(
			0, gunOrigin.x + offsets.x * axlArmDir, gunOrigin.y + offsets.y, axlArmDir, 1,
			getRenderEffectSet(), 1, 1, 1, armZIndex, angle: Helpers.byteToDegree(armAngle), shaders: getShaders()
		);
	}

	public Point getAxlGunArmOrigin() {
		Point retPoint;
		var pois = sprite.getCurrentFrame().POIs;
		Point roundPos = new Point(MathInt.Round(pos.x), MathInt.Round(pos.y));
		if (pois.Length > 0) {
			retPoint = roundPos.addxy(pois[0].x * axlArmDir, pois[0].y);
		} else {
			retPoint = roundPos.addxy(3 * axlArmDir, -21);
		}
		return retPoint;
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add((byte)armAngle);
		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		base.updateCustomActorNetData(data);
		armAngle =data[0];
	}
}


public class AxlDiscrardedWeapon : Actor {
	public float time;
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
			Point offset =  sprite.getCurrentFrame().POIs[0];
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
