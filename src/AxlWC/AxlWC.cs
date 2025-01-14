using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using SFML.Graphics;
using SFML.Window;

namespace MMXOnline;

public class AxlWC : Character {
	// Weapon data.
	public AxlWeaponWC mainWeapon = null!;
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
	public bool? shouldDrawArmNet = null;
	public bool isWhite;
	public float whiteTime;
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

	public override void update() {
		bool wasGrounded = grounded;
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
		// Arm angle.
		updateArmAngle();
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
		// Dodge.
		if (player.input.checkDoubleTap(Control.Dash) &&
			player.input.isPressed(Control.Dash, player) && canDash() && flag == null
		) {
			changeState(new DodgeRollAxlWC(), true);
			return true;
		}
		// Hover.
		if (!grounded && player.input.isPressed(Control.Jump, player)
			&& !player.input.isHeld(Control.Down, player) && hoverTimes == 0 &&
			canJump() && flag == null
		) {
			hoverTimes++;
			changeState(new HoverAxlWC(), true);
			return true;
		}
		// Block.
		if (grounded && player.input.isHeld(Control.Down, player) &&
			charState is not AxlBlock and not Dash and not OcelotSpin && axlWeapon?.autoFire == false
		) {
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
		if (charState is AxlBlock or RisingBarrage) {
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
		float shootAngle = armAngle;
		if (armDir < 0) {
			shootAngle = shootAngle * -1 + 128;
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

	public override bool chargeButtonHeld() {
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
				3, Global.halfFlinch, isJuggleProjectile: true,
				addToLevel: addToLevel
			),
			MeleeIds.String1 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.FireWave, player,
				2, Global.defFlinch,
				addToLevel: addToLevel
			),
			MeleeIds.String2 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.FireWave, player,
				2, Global.defFlinch,
				addToLevel: addToLevel
			),
			MeleeIds.String3 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.FireWave, player,
				2, Global.defFlinch, 5,
				addToLevel: addToLevel
			),
			MeleeIds.String4 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.Raijingeki2, player,
				2, 0,
				addToLevel: addToLevel
			),
			MeleeIds.String5 => new GenericMeleeProj(
				FireWave.netWeapon, pos, ProjIds.HeavyPush, player,
				2, 0,
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

	public override void render(float x, float y) {
		base.render(x, y);
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
