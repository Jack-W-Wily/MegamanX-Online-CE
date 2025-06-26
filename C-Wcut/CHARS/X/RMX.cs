

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



public class RockmanX : MegamanX {

	
	public bool canSpecialCancel = false;


	public RockmanX(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, XLoadout? xLoadout = null
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {

		charId = CharIds.RockmanX; 

		// For special conditions stuff
		if (charState is WarpIn) superBarAmmo = 0;

	}



	// NormalCTRL: is for you to add moves that your new Character that he can do while
	// He isn't Softlocked in a motion be it an attack or a Damage State
	public override bool normalCtrl() {

		if (player.input.isHeld(Control.L2, player) && grounded){
			changeState(new BlockWCUT());
		
		}
		// This is Where hypermode actiavtion happens
			if (player.input.isPressed(Control.Special2, player)
			&& player.currency > 4
			) {
				player.currency -= 5;
				
				/* 
				changeState() you'll be using this for every custom action your character does
				*/
			}

		return base.normalCtrl();
	}


	// AttackCtrl: is for you to add moves to your character that he can only perform
	// While the attackCtrl flag is active in a charstate and is conventionally where you add attacks
	public override bool attackCtrl() {

		
		if (player.input.isHeld(Control.L2, player)
		&& player.input.isPressed(Control.Shoot, player)) {
		
			
		}

		if (player.input.isHeld(Control.L2, player)
		&& player.input.isPressed(Control.Dash, player)) {
		
			
		}



		return base.attackCtrl();
	}


	public override void update() {
		base.update();
		// For the special cancels to work
		if (charState.attackCtrl ||
		charState.normalCtrl ||
		charState is KurumitoStandingKick or
		 KurumitosShikiYamiBaraiLv1 or
		 KurumitoFowardKick or
		 KuromitosBatsuyomi or  KurumitosDokuGami or
		KurumitosAirDunk
		) {
			canSpecialCancel = true;
		} else {
			canSpecialCancel = false;
		}

		if (overDriveTimer > 0) {
		OverDrive = true;
		} else {
			OverDrive = false;
		}
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref overDriveTimer);

	


		if (canSpecialCancel) {


		}
		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
			}
		}







		if (superBarAmmo >= superBarMaxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				superBarAmmo = Helpers.clampMax(superBarAmmo + 1, superBarMaxAmmo);
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


	}


	// these are where the sprites referenced with each melee
	// IDs are located
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"rmx_block"   => MeleeIds.Blocking, 
			"mmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"mmx_shoryuken" => MeleeIds.Shoryuken,
			"mmx_beam_saber" or "mmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"mmx_beam_saber2" => MeleeIds.ZSaber,
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
					(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.BlockingProjID, player,
				 0, 0, isDeflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
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
			(int)MeleeIds.LightHeadbuttEX => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				4, Global.defFlinch, 30, addToLevel: addToLevel
			),
			(int)MeleeIds.Shoryuken => new GenericMeleeProj(
				ShoryukenWeapon.netWeapon, projPos, ProjIds.Shoryuken, player,
				2, Global.defFlinch, 10, addToLevel: addToLevel
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
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return (superBarAmmo < superBarMaxAmmo);
	}

	public override bool canCharge() {
		return !isInvulnerableAttack();
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
