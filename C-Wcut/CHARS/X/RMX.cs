

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class RockmanX : MegamanX {

	
	public bool canSpecialCancel = false;

	public float DodgeCD;


	public RockmanX(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, XLoadout? xLoadout = null
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		specialButtonMode = 0;	
		charId = CharIds.RockmanX;

		// For special conditions stuff
		if (charState is WarpIn) player.superAmmo = 0;

	}



	// NormalCTRL: is for you to add moves that your new Character that he can do while
	// He isn't Softlocked in a motion be it an attack or a Damage State
	public override bool normalCtrl() {

		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT());
			return true;
		}
	

		return base.normalCtrl();
	}


		public bool canSummonZero => player.loadout.xLoadout.weapon1 < 9 &&
		 player.loadout.xLoadout.weapon2 < 9 &&  player.loadout.xLoadout.weapon3 < 9
		;

		

		
		

	// AttackCtrl: is for you to add moves to your character that he can only perform
	// While the attackCtrl flag is active in a charstate and is conventionally where you add attacks
	public override bool attackCtrl() {


		

		if (player.input.isL2Held(player)
		&& player.input.isAPressed(player)) {
			changeState(new RMXGrabStartState(), true);

		}

		if (player.input.isL2Held(player)
		&& player.input.isPressed(Control.Dash, player) && DodgeCD == 0) {


			changeState(new WarpDodge(pos), true);

			DodgeCD = 1.2f;
		}




		if (player.input.isBPressed(player) &&
		player.input.isHeld(Control.Up, player) &&
		player.input.isLeftOrRightHeld(player) &&
		!player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not RMXDoubleKick) {
			changeState(new RMXDoubleKick(), true);
		}

		if (player.input.isBPressed(player) &&
		!player.input.isHeld(Control.Up, player) &&
		!player.input.isLeftOrRightHeld(player) &&
		!player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not RMXDoubleKick) {
			changeState(new RMXPunch(), true);
		}

		if (player.input.isBPressed(player) &&
		!player.input.isHeld(Control.Up, player) &&
		player.input.isLeftOrRightHeld(player) &&
		!player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not RMXDoubleKick) {
			changeState(new RMXPunch(), true);
			slideVel = xDir * getDashSpeed() * 0.9f;
		}


	

		if (player.input.isR2Pressed(player) && player.input.isHeld(Control.Up, player) && canSummonZero) {
			if (helperZero == null) {
				helperZero = new FakeZero(player, pos, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
				player.superAmmo -= 32;
			}
		}
		bool canUseSupers = player.superAmmo >= 16;

		if (player.input.isR2Pressed(player) && !player.input.isHeld(Control.Up, player) && canUseSupers) {

			if (player.input.isHeld(Control.Down, player)) {
				enterParry();
			} else if (charState is Dash or AirDash) {
				charState.isGrabbing = true;
				changeSpriteFromName("unpo_grab_dash", true);
			} else {
				changeState(new XUPPunchState(grounded), true);
			}
			if (!OverDrive) {
				player.superAmmo -= 16;
			}
		}


		return base.attackCtrl();
	}


	
	
	public void enterParry() {
		if (absorbedProj != null) {
			changeState(new XUPParryProjState(absorbedProj, true, false), true);
			player.weapons.RemoveAll(w => w is AbsorbWeapon);
			absorbedProj = null;
			return;
		}
		changeState(new XUPParryStartState(), true);
		return;
	}

	public FakeZero helperZero;

	public bool helperzeroOnce = false;

		public bool becomeragingcharge = false;
	public override void update() {
		base.update();

		if (!helperzeroOnce && helperZero == null && Global.level.levelData.name == "redandblue_vs_purple_1v1") {
			helperZero = new FakeZero(player, pos, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			helperzeroOnce = true;
		}

		if (charState is not WarpIn and not WarpIdle && bonusHealth == 0 && health < 4 && Global.level.levelData.name == "redandblue_vs_purple_1v1" && !becomeragingcharge) {
			stopMoving();
			becomeragingcharge = true;
			changeState(new XReviveStart(), true);
		}
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);
		Helpers.decrementTime(ref DodgeCD);


		if (Global.level.levelData.name == "zero_vs_x_1v1") {
			hasUltimateArmor = true;
			if (bonusHealth == 0) {
				overDriveTimer = 999;
			}

		}

		if (canSpecialCancel) {


		}
		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
			}
		}


		if (helmetArmor == ArmorId.Light) {
			if (charState is Jump && player.input.isHeld(Control.Down, player)) {
				changeSpriteFromName("headbutt", false);
			}
		}



		if (player.input.isWeaponLeftOrRightPressed(player)) {
			shootCooldown = 0;
		}


		if (player.superAmmo >= player.superMaxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				player.superAmmo = Helpers.clampMax(player.superAmmo + 1, player.superMaxAmmo);
				playSound("healX3", forcePlay: true, true);

			}
		}

	}


	

	public override bool canDash() {
		return flag == null;
	}

	public override bool canWallClimb() {
		return true;
	}

	
	public override string getSprite(string spriteName) {
		if (Global.sprites.ContainsKey("rmx_" + spriteName)) {
			return "rmx_" + spriteName;
		}
		return "rmx_" + spriteName;
	}


	// for the melee hitbox to work
	// This can run on both owners and non-owners. So data used must be in sync.
	public enum MeleeIds {
		None = -1,

		Blocking, // you add more and more and finish with "," always for each move you add

		Grab,

		SpeedBurnerCharged,
		LightHeadbutt,
		LightHeadbuttEX,
		Shoryuken,
		MaxZSaber,
		ZSaber,
		ZSaberAir,
		NovaStrike,

		DoubleKick,
		DoubleKick2,

		Punch1,
		Punch2,

		DashGrab,
		ParryBlock,
		Punch,
	


	}


	// these are where the sprites referenced with each melee
	// IDs are located
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"rmx_block"   => MeleeIds.Blocking, 
			"rmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"rmx_shoryuken" => MeleeIds.Shoryuken,
			"rmx_punch_1" => MeleeIds.Punch1,
			"rmx_grab_start" => MeleeIds.Grab,
			"rmx_punch_2" => MeleeIds.Punch2,
			"rmx_beam_saber" or "rmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"rmx_beam_saber2" => MeleeIds.ZSaber,
			"rmx_double_kick" when frameIndex < 5 => MeleeIds.DoubleKick,
			"rmx_double_kick" when frameIndex > 5 => MeleeIds.DoubleKick2,
			"rmx_beam_saber_air2" => MeleeIds.ZSaberAir,
			"rmx_nova_strike" or "rmx_nova_strike_down" or "rmx_nova_strike_up" => MeleeIds.NovaStrike,
			// Light  Helmet.
			"rmx_jump" or "rmx_jump_shoot" or "rmx_wall_kick" or "rmx_wall_kick_shoot"
			when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LightHeadbutt,
			// Light Helmet when it up-dashes.
			"rmx_headbutt"  => MeleeIds.LightHeadbuttEX,
			// Nothing.
			"rmx_unpo_grab_dash" => MeleeIds.DashGrab,
			"rmx_unpo_punch" or "rmx_unpo_air_punch" => MeleeIds.Punch,
			"rmx_unpo_parry_start" => MeleeIds.ParryBlock,
		
			_ => MeleeIds.None
		});
	}

	// this is where you effectively make the melee hitboxes trigger
	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
					(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockingProjID, player,
				 0, 0, isDeflectShield: true,
				isZSaberClang: true, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.ParryBlock => new GenericMeleeProj(
				RCXParry.netWeapon, projPos, ProjIds.UPParryBlock, player,
				0, 0, 60, addToLevel: addToLevel
			),
			(int)MeleeIds.Punch => new GenericMeleeProj(
				RCXPunch.netWeapon, projPos, ProjIds.MechFrogStompShockwave, player,
				3, 0, 30, addToLevel: addToLevel, hitSound : "dbzclang"
			),
			(int)MeleeIds.DashGrab => new GenericMeleeProj(
				RCXPunch.netWeapon, projPos, ProjIds.newUpGrab, player,
				3, 0, 120, addToLevel: addToLevel
			),

			
				(int)MeleeIds.Grab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 1, 0, isDeflectShield: true,
				isZSaberClang: true, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.SpeedBurnerCharged => new GenericMeleeProj(
				SpeedBurner.netWeapon, projPos, ProjIds.SpeedBurnerCharged, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.LightHeadbutt => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.halfFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.DoubleKick => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.ForceGrabState, player,
				2, 0, 30, addToLevel: addToLevel, hitSound : "htsnd_punch_1"
			),
			(int)MeleeIds.DoubleKick2 => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel, hitSound : "htsnd_punch_2"
			),
			(int)MeleeIds.Punch1 => new GenericMeleeProj(
				RCXPunch.netWeapon, projPos, ProjIds.UPPunch, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel, hitSound : "htsnd_punch_2"
			),
			(int)MeleeIds.Punch2 => new GenericMeleeProj(
				RCXPunch.netWeapon, projPos, ProjIds.VJab1, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel, hitSound : "htsnd_punch_2"
			),
			(int)MeleeIds.LightHeadbuttEX => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.defFlinch, 50, addToLevel: addToLevel, hitSound : "htsnd_punch_3"
			),
			(int)MeleeIds.Shoryuken => new GenericMeleeProj(
				ShoryukenWeapon.netWeapon, projPos, ProjIds.Shoryuken, player,
				2, Global.defFlinch, 10, addToLevel: addToLevel, hitSound : "htsnd_punch_3"
			),
			(int)MeleeIds.MaxZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.XSaber, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel, isZSaberEffect: true
			),
			(int)MeleeIds.ZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				1, Global.halfFlinch, 5, addToLevel: addToLevel, isZSaberEffect: true
			),
			(int)MeleeIds.ZSaberAir => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel, isZSaberEffect: true
			),
			(int)MeleeIds.NovaStrike => new GenericMeleeProj(
				HyperNovaStrike.netWeapon, projPos, ProjIds.NovaStrike, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel
			),

			_ => null
		};
		return proj;
	}



	// Ammo section
	

	public override bool canCharge() {
		return !isInvulnerableAttack();
	}

	

	
	public override void render(float x, float y) {



	

		if (player.isMainPlayer && overDriveTimer > 0) {
			float healthPct = Helpers.clamp01((15 - overDriveTimer) / 15);
			float sy = -27;
			float sx = 20;
			if (xDir == -1) sx = 90 - 20;
			drawFuelMeter(healthPct, sx, sy);
		}
		base.render(x, y);
	}


	public override void chargeGfx() {
		if (ownedByLocalPlayer) {
			chargeEffect.stop();
		}
		if (isCharging()) {
			chargeSound.play();
			int chargeType = 0;
			chargeEffect.update(getChargeLevel(), chargeType);
		}
	}


	public override float getRunSpeed() {
		float runSpeed = 90;
	
		return runSpeed * getRunDebuffs();
	}



	
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;
		int index = currentWeapon?.index ?? 0;

		if (stingActiveTime > 0 && stingPaletteIndex != 0) {
			palette = player.xStingPaletteShader;
			palette.SetUniform("palette", stingPaletteIndex);

			shaders.Add(palette);
			shaders.AddRange(baseShaders);
			return shaders;
		}
		if (index >= (int)WeaponIds.GigaCrush) {
			index = 0;
		}
		if (index == (int)WeaponIds.HyperCharge && ownedByLocalPlayer) {
			index = player.weapons[player.hyperChargeSlot].index;
		}
		if (hasFullHyperMaxArmor) {
			index = 25;
		}
		if (hasUltimateArmor && index == 0) {
			index = 30;
		}
		palette = player.xPaletteShader;

		palette?.SetUniform("palette", index);

		List<ShaderWrapper?> chargePalletes = getChargeShaders() as List<ShaderWrapper?>;
		if (chargePalletes.Count > 0) {
			if (chargePalletes.Count == 1) {
				if (!hyperChargeActive) {
					chargePalletes.Add(null);
				} else if (!chargePalletes.Contains(Player.XYellowC)) {
					chargePalletes.Add(Player.XYellowC);
				}
			}
			ShaderWrapper? targetChargePallete = chargePalletes[MathInt.Floor(
				(chargePalleteTime % (chargePalletes.Count * 2)) / 2f
			)];
			if (targetChargePallete != null) {
				palette = targetChargePallete;
			}
		}

		if (charState is SpeedBurnerCharState) {
			palette = player.speedBurnerOrange;
			if (Global.isOnFrameCycle(8)) {
				palette = player.speedBurnerGrey;
			}
		}
		if (palette != null) {
			shaders.Add(palette);
		}
		if (shaders.Count == 0) {
			return baseShaders;
		}
		shaders.AddRange(baseShaders);
		return shaders;
	}
	public List<ShaderWrapper> getChargeShaders() {
		List<ShaderWrapper> chargePalletes = new();
		ShaderWrapper? defaultChargePallete = null;
		int chargeLevel = getDisplayChargeLevel();
		if (chargeLevel > 0) {
			defaultChargePallete = getDisplayChargeLevel() switch {
				1 => Player.XBlueC,
				2 => Player.XYellowC,
				3 when hasFullHyperMaxArmor => Player.XGreenC,
				3 when armArmor == ArmorId.Max => Player.XOrangeC,
				_ => Player.XPinkC,
			};
			chargePalletes.Add(defaultChargePallete);
		}
		if (stockedMaxBusterLv >= 1) {
			if (defaultChargePallete != Player.XOrangeC) {
				chargePalletes.Add(Player.XOrangeC);
			} else {
				chargePalletes.Add(Player.XPinkC);
			}
		}
		if (stockedBusterLv >= 1) {
			if (!chargePalletes.Contains(Player.XPinkC)) {
				chargePalletes.Add(Player.XPinkC);
			} else if (!chargePalletes.Contains(Player.XOrangeC)) {
				chargePalletes.Add(Player.XOrangeC);
			}
		}
		if (stockedSaber) {
			if (!chargePalletes.Contains(Player.XGreenC)) {
				chargePalletes.Add(Player.XGreenC);
			} else if (!chargePalletes.Contains(Player.XOrangeC)) {
				chargePalletes.Add(Player.XOrangeC);
			}
		}
		if (hyperChargeActive) {
			if (!hasFullHyperMaxArmor && stockedMaxBusterLv == 0 &&
				!chargePalletes.Contains(Player.XOrangeC)
			) {
				chargePalletes.Add(Player.XOrangeC);
			} else if (!stockedSaber && !chargePalletes.Contains(Player.XPinkC)) {
				chargePalletes.Add(Player.XPinkC);
			}
		}
		return chargePalletes;
	}



	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add(Helpers.boolArrayToByte([
			OverDrive,
		]));
		return customData;
	}
	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];
		bool[] flags = Helpers.byteToBoolArray(data[0]);
		OverDrive = flags[0];
	}




}
