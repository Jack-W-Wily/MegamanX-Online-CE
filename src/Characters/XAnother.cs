namespace MMXOnline;

public class XAnother : MegamanX {
	public XAnother(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		
		charId = CharIds.XAnother;
	}
	public override bool normalCtrl() {
		
	if (player.superAmmo >= 5 && player.input.isPressed(Control.Dash, player) &&
		player.input.isHeld(Control.Up, player)){
			player.superAmmo -= 5;
			changeState(new UpDash(Control.Dash));
			return true;
		}

	if (charState is Dash or AirDash &&
		player.input.isHeld(Control.Down, player)
		&& player.superAmmo >= 3){
			player.superAmmo -= 3;
			changeState(new XIceSlide());
			slideVel = xDir * getDashSpeed() * 2;
			return true;
		}
		return base.normalCtrl();
	}
	public override void update() {
		base.update();


		if (gigaAttack.ammo >= 28 && !hasUltimateArmor &&
		player.input.isPressed(Control.Special2, player)){
		hasUltimateArmor = true;
		player.addNovaStrike();
		addHealth(5);
		//player.currency -= 5;
		changeSpriteFromName("warp_in", true);

		}
	if (cStingPaletteTime > 5) {
			cStingPaletteTime = 0;
			cStingPaletteIndex++;
		}
		cStingPaletteTime++;



		if (!ownedByLocalPlayer) {
			return;
		}
		// Shotos
		bool hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		bool shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		
		if (hadokenCheck && player.superAmmo >= 14) {
			player.superAmmo -= 14;
			changeState(new Hadouken(), true);
		}
		if (shoryukenCheck && player.superAmmo >= 14) {
			player.superAmmo -= 14;
			changeState(new Shoryuken(isUnderwater()), true);
		}

		if (player.superAmmo > 0 && player.input.isHeld(Control.Down,player) &&
		charState is Jump && !sprite.name.Contains("headbutt")){
			player.superAmmo -= 1;
			changeSpriteFromName("headbutt", false);
		}

		if (player.superAmmo > 0 && player.input.isPressed(Control.Dash,player) &&
		sprite.name.Contains("unpo")){
			player.superAmmo -= 1;
			changeState(new XlightKick(), true);
		}



		//>>>>>>>>>>>>>>>>>

			player.fgMoveAmmo += Global.speedMul;
		if (player.fgMoveAmmo > player.fgMoveMaxAmmo) player.fgMoveAmmo = player.fgMoveMaxAmmo;



		gigaWeapon?.update();
		hyperNovaStrike?.update();
		itemTracer?.update();
		shootingRaySplasher?.burstLogic2(this);

		// Charge and release charge logic.
		if (!isInDamageSprite() && !sprite.name.Contains("block")){
		chargeLogic(shoot);
		}
		player.changeWeaponControls();
		Helpers.decrementFrames(ref shootCooldown);
		Helpers.decrementFrames(ref upPunchCooldown);
		Helpers.decrementFrames(ref parryCooldown);
		Helpers.decrementFrames(ref xSaberCooldown);


		

		if (musicSource == null && hasUltimateArmor) {
			addMusicSource("XvsZeroV2_megasfc", getCenterPos(), true);
		}
	}
	public override bool attackCtrl() {
		if(player.superAmmo >= 5 && parryCooldown == 0 &&
			player.input.isPressed(Control.Special1, player) &&
			player.input.isHeld(Control.Down, player)){
				player.superAmmo -= 5;
				changeState(new XUPParryStartState(), true);
			}
		return base.attackCtrl();
	}
	public override string getSprite(string spriteName) {
		return "rmx_" + spriteName;
	}

	
	
	public enum MeleeIds {
		None = -1,
		SpeedBurnerCharged,
		LigthHeadbutt,
		Shoryuken,
		MaxZSaber,
		ZSaber,
		ZSaberAir,
		NovaStrike,
		XBlock,
		UPGrab,
		UPPunch,

		UPDash,

		IceSlide,

		LightKick,

		UPParryBlock,
	}



	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"rmx_speedburner" => MeleeIds.SpeedBurnerCharged,
			"rmx_shoryuken" => MeleeIds.Shoryuken,
			"rmx_block" => MeleeIds.XBlock,
			"rmx_beam_saber" or "rmx_beam_saber_air" => MeleeIds.MaxZSaber,
			"rmx_beam_saber2"  => MeleeIds.ZSaber,
			"rmx_beam_saber_air2"  => MeleeIds.ZSaberAir,
			"rmx_nova_strike" or "rmx_nova_strike_down" or "rmx_nova_strike_up" => MeleeIds.NovaStrike,
			"rmx_unpo_grab_dash" => MeleeIds.UPGrab,
			"rmx_unpo_punch" or "rmx_unpo_air_punch" or "rmx_unpo_punch_2" => MeleeIds.UPPunch,
			"rmx_unpo_parry_start" => MeleeIds.UPParryBlock,
			"rmx_up_dash"  => MeleeIds.UPDash,
			"rmx_sice_slide"  => MeleeIds.IceSlide,
			// Light Helmet.
			"rmx_headbutt"  => MeleeIds.LigthHeadbutt,
			"rmx_kick_lightarmor"  => MeleeIds.LightKick,
			// Nothing.
			_ => MeleeIds.None
		});
	}

	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		
	
		
		return id switch {
			(int)MeleeIds.SpeedBurnerCharged => new GenericMeleeProj(
				SpeedBurner.netWeapon, projPos, ProjIds.SpeedBurnerCharged, player,
				1, Global.defFlinch, 2f
			),
			(int)MeleeIds.LigthHeadbutt => new GenericMeleeProj(
				LhHeadbutt.netWeapon, projPos, ProjIds.Headbutt, player,
				4, Global.defFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.Shoryuken => new GenericMeleeProj(
				ShoryukenWeapon.netWeapon, projPos, ProjIds.Shoryuken, player,
				2, Global.defFlinch, 5f, addToLevel: addToLevel
			),
			(int)MeleeIds.MaxZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.XSaber, player,
				4, Global.defFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.IceSlide => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.SiceSlide, player,
				2, Global.defFlinch, 20f, addToLevel: addToLevel
			),

			(int)MeleeIds.LightKick => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.NormalPush, player,
				3, 0, 20f, addToLevel: addToLevel
			),

			(int)MeleeIds.UPDash => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.MechFrogGroundPound, player,
				3, 30, 20f, addToLevel: addToLevel
			),


			(int)MeleeIds.ZSaber => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, 10, 10f, addToLevel: addToLevel
			),
			(int)MeleeIds.XBlock => new GenericMeleeProj(
				new RCXPunch(), projPos, ProjIds.SwordBlock, player, 0, 0, 0, isDeflectShield: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.ZSaberAir => new GenericMeleeProj(
				ZXSaber.netWeapon, projPos, ProjIds.X6Saber, player,
				2, 10, 10f, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.NovaStrike => new GenericMeleeProj(
				HyperNovaStrike.netWeapon, projPos, ProjIds.NovaStrike, player,
				4, Global.defFlinch, 20f, addToLevel: addToLevel
			),
			(int)MeleeIds.UPGrab => new GenericMeleeProj(
				new XUPGrab(), projPos, ProjIds.UPGrab, player, 0, 0, 0, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.UPPunch => new GenericMeleeProj(
				new RCXPunch(), projPos, ProjIds.UPPunch, player,
			 2, Global.halfFlinch, 15f, addToLevel: addToLevel, ShouldClang : true
			),
			(int)MeleeIds.UPParryBlock => new GenericMeleeProj(
				new XUPParry(), projPos, ProjIds.UPParryBlock, player, 0, 0, 1, addToLevel: addToLevel
			),
			_ => null
		};
	}
}
