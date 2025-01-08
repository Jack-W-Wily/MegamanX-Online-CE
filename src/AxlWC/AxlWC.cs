using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace MMXOnline;

public class AxlWC : Character {
	// Weapon data.
	public AxlWeaponWC mainWeapon;
	public List<AxlWeaponWC> axlWeapons = new();
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
	public bool isWhite;
	public float whiteTime;
	public float dodgeRollCooldown;
	public float aiAttackCooldown;
	public float shootCooldown;
	public float recoilTime;
	public int armDir => charState is WallSlide ? -xDir : xDir;
	public float armAngle = 0;
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
	}

	public override void preUpdate() {
		base.preUpdate();
		// Cooldowns.
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		Helpers.decrementFrames(ref recoilTime);
	}

	public override void update() {
		base.update();
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
		// Arm angle.
		updateArmAngle();
		// Charge and release charge logic.
		chargeLogic(chargeShoot);
		weaponSwapLogic();
		// Weapon input logic.
		foreach (AxlWeaponWC weapon in axlWeapons) {
			weapon.axlUpdate(this, weapon == axlWeapon);
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (!muzzleFlash.isAnimOver()) {
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
		} else {
			muzzleFlash.visible = false;
		}
		// Swap on ammo empitiying.
		if (axlWeapon != mainWeapon &&
			axlWeapon.ammo <= 0 && recoilTime <= 0
		) {
			Weapon oldWeapon = axlWeapon;
			weaponSlot = 1;
			onWeaponChange(oldWeapon, axlWeapon);
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
		return (axlWeapon is AxlBulletWC && charState is not OcelotSpin);
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
		// Stop charge if leaving Axl bullets.
		if (oldWeapon == mainWeapon) {
			mainWeapon.ammo = mainWeapon.maxAmmo;
			stopCharge();
		}
		
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
		// Hover.
		if (!grounded && player.input.isPressed(Control.Jump, player) &&
			canJump() && !isDashing && canAirDash() && flag == null
		) {
			dashedInAir++;
			changeState(new HoverAxlWC(), true);
			return true;
		}
		// Block.
		if (grounded && player.input.isHeld(Control.Down, player) && charState is not AxlBlock and not Dash) {
			changeState(new AxlBlock(), true);
			return true;
		}
		return base.normalCtrl();
	}

	// Attack inputs.
	public override bool attackCtrl() {
		if (isCharging() || axlWeapon == null) {
			return base.attackCtrl();
		}
		// Custom inputs.
		bool customImputUsed = axlWeapon.attackCtrl(this);
		if (customImputUsed) {
			return true;
		}
		// For stuff we do not want to do mid guard animation.
		if (charState is AxlBlock) {
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
		weapon.shootMain(this, getAxlBulletPos(axlWeapon), shootAngle, 0);
		weapon.shootCooldown = weapon.getFireRate(this, 0);
		recoilTime = weapon.getRecoil(this, 0);
		weapon.addAmmo(-weapon.getAmmoUse(this, 0), player);
		if (weapon.shootSounds[0] != "") {
			playSound(weapon.shootSounds[0], true, true);
		}
		if (weapon.flashSprite != "") {
			muzzleFlash.changeSprite(weapon.flashSprite, true);
			muzzleFlash.visible = true;
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
		weapon.shootAlt(this, getAxlBulletPos(axlWeapon), shootAngle, 0);
		weapon.shootCooldown = weapon.getAltFireRate(this, 0);
		recoilTime = weapon.getAltRecoil(this, 0);
		weapon.addAmmo(-weapon.getAltAmmoUse(this, 0), player);
		if (weapon.shootSounds[1] != "") {
			playSound(weapon.shootSounds[1], true, true);
		}
		if (weapon.chargedFlashSprite != "") {
			muzzleFlash.changeSprite(weapon.chargedFlashSprite, true);
			muzzleFlash.visible = true;
		}
	}

	public void chargeShoot(int chargeLevel) {
		if (axlWeapon is not AxlBulletWC axlBullet) {
			return;
		}
		float shootAngle = armAngle;
		if (armDir < 0) {
			shootAngle = shootAngle * -1 + 128;
		}
		axlBullet.shootAlt(this, getAxlBulletPos(axlWeapon), shootAngle, chargeLevel);
		axlBullet.shootCooldown = axlBullet.getAltFireRate(this, 0);
		recoilTime = axlBullet.getAltFireRate(this, 0);
		axlBullet.addAmmo(-axlBullet.getAltAmmoUse(this, chargeLevel), player);
		if (recoilTime > 12) {
			recoilTime = 12;
		}
		if (axlBullet.shootSounds[1] != "") {
			playSound(axlBullet.shootSounds[3], true, true);
		}
		muzzleFlash.visible = true;
		muzzleFlash.frameIndex = 0;
		if (axlBullet.chargedFlashSprite != "") {
			muzzleFlash.changeSprite(axlBullet.chargedFlashSprite, true);
			muzzleFlash.visible = true;
		}
	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Special1, player);
	}

	public override bool canAddAmmo() {
		if (mainWeapon.ammo < mainWeapon.maxAmmo && axlWeapon == mainWeapon) {
			return true;
		}
		return false;
	}

	public override void addAmmo(float amount) {
		if (axlWeapon != mainWeapon) {
			mainWeapon.addAmmo(amount, player);
			playSound("subtankFill", true);
		}
		mainWeapon.addAmmoHeal(amount);
	}

	public override void addPercentAmmo(float amount) {
		if (axlWeapon != mainWeapon) {
			mainWeapon.addAmmoPercent(amount);
			playSound("subtankFill", true);
		}
		mainWeapon.addAmmoPercentHeal(amount);
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
		if (axlWeapon.isTwoHanded) {
			retPoint = retPoint.addxy(-7 * armDir, 2);
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
		AxlLoadout axlLoadout = player.loadout.axlLoadout;
		axlWeapons.Add(getWeaponFromIndex(axlLoadout.weapon2));
		axlWeapons.Add(new AxlBulletWC());
		axlWeapons.Add(getWeaponFromIndex(axlLoadout.weapon3));
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
			axlWeapon.spriteFrameIndex, gunOrigin.x + offsets.x * armDir, gunOrigin.y + offsets.y, armDir, 1,
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
		return customData;
	}

	public override void updateCustomActorNetData(byte[] data) {
		base.updateCustomActorNetData(data);
		armAngle = data[0];
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
