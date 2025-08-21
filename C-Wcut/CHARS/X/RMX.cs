

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
		// This is Where hypermode actiavtion happens
			if (player.input.isPressed(Control.Special2, player)
			&& player.currency > 4
			) {
				player.currency -= 5;
			overDriveTimer = 12;
			
			}

		return base.normalCtrl();
	}


	// AttackCtrl: is for you to add moves to your character that he can only perform
	// While the attackCtrl flag is active in a charstate and is conventionally where you add attacks
	public override bool attackCtrl() {

			if (player.superAmmo == player.superMaxAmmo && 
		downPressedTimes >= 2 && player.input.isR2Pressed(player)) {
					changeState(new GigaCrushCharState(), true);
				downPressedTimes = 0;
				player.superAmmo = 0;
					return true;
		}

		if (player.input.checkShoryuken(player, xDir, Control.Shoot) &&
		player.superAmmo == player.superMaxAmmo 
		) {
			changeState(new Shoryuken(isUnderwater()), true);
				player.superAmmo = 0;
		}
		


		if (player.input.isL2Held(player)
		&& player.input.isAPressed(player)) {
			changeState(new RMXGrabStartState(), true);
			
		}

		if (player.input.isL2Held(player)
		&& player.input.isPressed(Control.Dash, player) && DodgeCD == 0) {

		
				changeState(new WarpDodge(pos), true);
			
			DodgeCD = 0.43f;
		}


		
		
		if (player.input.isR2Pressed(player) &&
		player.input.isHeld(Control.Up, player) &&
		player.input.isLeftOrRightHeld(player) &&
		!player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not RMXDoubleKick) {
				changeState(new RMXDoubleKick(), true);
		}

		if (player.input.isR2Pressed(player) &&
		!player.input.isHeld(Control.Up, player) &&
		!player.input.isLeftOrRightHeld(player) &&
		!player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not RMXDoubleKick) {
			changeState(new RMXPunch(), true);
		}

		if (player.input.isR2Pressed(player) &&
		!player.input.isHeld(Control.Up, player) &&
		player.input.isLeftOrRightHeld(player) &&
		!player.input.checkShoryuken(player, xDir, Control.R2)
		&& charState is not RMXDoubleKick) {
			changeState(new RMXPunch(), true);
			slideVel = xDir * getDashSpeed() * 0.9f;
		}



		return base.attackCtrl();
	}


	public override void update() {
		base.update();
	

		if (overDriveTimer > 0) {
		OverDrive = true;
		} else {
			OverDrive = false;
		}
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);
		Helpers.decrementTime(ref DodgeCD);

	


		if (canSpecialCancel) {


		}
		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
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
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	
	public override string getSprite(string spriteName) {
		if (Global.sprites.ContainsKey("rmx_" + spriteName)) {
			return "rmx_" + spriteName;
		}
		return "mmx_" + spriteName;
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


	}


	// these are where the sprites referenced with each melee
	// IDs are located
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"rmx_block"   => MeleeIds.Blocking, 
			"mmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"mmx_shoryuken" => MeleeIds.Shoryuken,
			"rmx_punch_1" => MeleeIds.Punch1,
			"rmx_grab_start" => MeleeIds.Grab,
			"rmx_punch_2" => MeleeIds.Punch2,
			"mmx_beam_saber" or "mmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"mmx_beam_saber2" => MeleeIds.ZSaber,
			"rmx_double_kick" when frameIndex < 5 => MeleeIds.DoubleKick,
			"rmx_double_kick" when frameIndex > 5 => MeleeIds.DoubleKick2,
			"mmx_beam_saber_air2" => MeleeIds.ZSaberAir,
			"mmx_nova_strike" or "mmx_nova_strike_down" or "mmx_nova_strike_up" => MeleeIds.NovaStrike,
			// Light  Helmet.
			"mmx_jump" or "mmx_jump_shoot" or "mmx_wall_kick" or "mmx_wall_kick_shoot"
			when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LightHeadbutt,
			// Light Helmet when it up-dashes.
			"mmx_up_dash" or "mmx_up_dash_shoot"
			when helmetArmor == ArmorId.Light && stingActiveTime == 0 => MeleeIds.LightHeadbuttEX,
			// Nothing.

			_ => MeleeIds.None
		});
	}

	// this is where you effectively make the melee hitboxes trigger
	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
					(int)MeleeIds.Blocking => new RMXGenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockingProjID, player,
				 0, 0, isDeflectShield: true,
				isZSaberClang: true, isZSaberEffect: false,
				addToLevel: addToLevel
			),
				(int)MeleeIds.Grab => new RMXGenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 1, 0, isDeflectShield: true,
				isZSaberClang: true, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.SpeedBurnerCharged => new RMXGenericMeleeProj(
				SpeedBurner.netWeapon, projPos, ProjIds.SpeedBurnerCharged, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.LightHeadbutt => new RMXGenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.halfFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.DoubleKick => new RMXGenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.ForceGrabState, player,
				2, 0, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.DoubleKick2 => new RMXGenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.Punch1 => new RMXGenericMeleeProj(
				RCXPunch.netWeapon, projPos, ProjIds.UPPunch, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.Punch2 => new RMXGenericMeleeProj(
				RCXPunch.netWeapon, projPos, ProjIds.VJab1, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.LightHeadbuttEX => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.Shoryuken => new RMXGenericMeleeProj(
				ShoryukenWeapon.netWeapon, projPos, ProjIds.Shoryuken, player,
				2, Global.defFlinch, 10, addToLevel: addToLevel
			),
			(int)MeleeIds.MaxZSaber => new RMXGenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.XSaber, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel, isZSaberEffect: true
			),
			(int)MeleeIds.ZSaber => new RMXGenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				1, Global.halfFlinch, 5, addToLevel: addToLevel, isZSaberEffect: true
			),
			(int)MeleeIds.ZSaberAir => new RMXGenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, Global.defFlinch, 30, addToLevel: addToLevel, isZSaberEffect: true
			),
			(int)MeleeIds.NovaStrike => new RMXGenericMeleeProj(
				HyperNovaStrike.netWeapon, projPos, ProjIds.NovaStrike, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel
			),

			_ => null
		};
		return proj;
	}



	// Ammo section
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return (player.superAmmo < player.superMaxAmmo);
	}

	public override bool canCharge() {
		return !isInvulnerableAttack();
	}

	public override bool chargeButtonHeld() {
		return player.input.isR2Held(player) || player.input.isAHeld(player)  || player.input.isBHeld(player);
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



	public override void increaseCharge() {
		float factor = 1;
		if (OverDrive) factor = 1.5f; // this means during OverDrive he gets a chargespeed buff
		chargeTime += Global.speedMul * factor;
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
		int chargeLevel = getChargeLevel();
		if (chargeLevel > 0) {
			defaultChargePallete = getChargeLevel() switch {
				1 => Player.XBlueC,
				2 => Player.XYellowC,
				3 when hasFullHyperMaxArmor => Player.XGreenC,
				3 when armArmor == ArmorId.Max => Player.XOrangeC,
				_ => Player.XPinkC,
			};
			chargePalletes.Add(defaultChargePallete);
		}
		if (stockedMaxBuster) {
			if (defaultChargePallete != Player.XOrangeC) {
				chargePalletes.Add(Player.XOrangeC);
			} else {
				chargePalletes.Add(Player.XPinkC);
			}
		}
		if (stockedBuster) {
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
			if (!hasFullHyperMaxArmor && !stockedMaxBuster && !chargePalletes.Contains(Player.XOrangeC)) {
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
