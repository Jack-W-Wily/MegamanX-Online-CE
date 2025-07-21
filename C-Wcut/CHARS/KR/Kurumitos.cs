
// How to add a character?
// I highly Reccomend you Copy These two "kuromitos File's That you downloaded in the win rar
// And rename and edit for every single one of them


// I hightly advise you to put all of these in every new .CS file you add
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
// To make sure most things work without you having errors when you want to use Math for example

namespace MMXOnline;

// this is the very basis of the character you need to make sure everything Except for the name
// "kurumitos" which you can name whatever you want is right here



public class Kurumitos : Character {

	// Special For Special conditions and stuff

	/* add it to Character.CS



	public float overDriveTimer;

	public bool OverDrive;


	///////////////////////////////////////////

	add it to Player.CS

	public float player.superAmmo;


	*/
	public float ShikiYamiBaraiCD;
	public float stockedTime;

	public bool canSpecialCancel = false;


	public Kurumitos(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
		) : base( // Make sure it looks exactly like this
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn) {


		charId = CharIds.Kurumitos; // This is the char ID, you can rename it later


		/* You need to make sure to go to Select Character Menu.CS and add him to the 
		"public enum CharIds"  section like this

				

public enum CharIds {			
	X,
	Zero,
	Vile,
	Axl,
	Sigma,
	PunchyZero,                                   NOTE: YOU DON'T NEED to type 
	BusterZero,									X,zero,vile blah blah blah, I placed
	// Non-standard chars start here.           this so you can Locate the area you're supposed
	WolfSigma = 100,							to add your character
	ViralSigma,
	KaiserSigma,
	RagingChargeX,
	// Non-vanilla chars start here.
	Rock = 10,
	// WCUT CHARS 
	// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	Kurumitos,   
	// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
					
					// Always add the new characters bellow the Vanilla characters
				 // Because otherwise the code gets really messy with the IDs
				 // Don't forget the "," at the end

}
// With this done your char ID can be referenced anywhere in the code
Then , you're gonna search for "public class CharSelection {"

then add him like this:


public static CharSelection[] selections => [
		new CharSelection("X", 0, 1, 0, "menu_mmx", 0),						NOte: reminder that 
		new CharSelection("Zero", 1, 1, 0, "menu_szero", 0),				that this whole X,zero,blah blah
		new CharSelection("Kaiser Knuckle", 5, 1, 0, "menu_kzero", 5) {		is just for reference and only
		},																	the //Make sure to .... section
		new CharSelection("Buster Zero", 6, 1, 0, "menu_bzero", 0) {		matters
			offset = new Point(2, 45)
		},
		new CharSelection("Vile", 2, 1, 0, "menu_vvile", 0),
		new CharSelection("Axl", 3, 1, 0, "menu_aaxl", 0){
			offset = new Point(1, 45)
		},
		new CharSelection("Sigma", 4, 1, 0, "menu_ssigma", sigmaIndex),

		
		// Make sure to add your char here
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		new CharSelection("Kurumitos", // Display name in the menu
		(int)CharIds.Kurumitos, // Char ID , you may notice that the ones above have numbers here 
		// 							but adding it as  (int)Char.Ids."your character" is more effective
		1,    // Mapped Char Armor (this is exclusive to make it so X's 1v1 Armors Work)
		0,               // Mapped Char Maverick (This is for sigma's 1v1 mavericks)
		"kr_idle",       // Sprite name to show in the menu, for this example I choose the idle
		 0               // Frame that the sprite will be stuck in
		 ),				 // Make sure to end the whole thing with a "," ion the end
		 // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	];
		// With this done your character is selectable on The Character Select Menu


		With all of that out of the way, to make sure you Locate the Player.cs File and find this section
		and Add your character's ID to them as well

		// Subtanks
	private Dictionary<int, List<SubTank>> charSubTanks = new Dictionary<int, List<SubTank>>() {
		{ (int)CharIds.X, new List<SubTank>() },
		{ (int)CharIds.Zero, new List<SubTank>() },
		{ (int)CharIds.Vile, new List<SubTank>() },
		{ (int)CharIds.Axl, new List<SubTank>() },
		{ (int)CharIds.Sigma, new List<SubTank>() },
		{ (int)CharIds.PunchyZero, new List<SubTank>() },
		{ (int)CharIds.BusterZero, new List<SubTank>() },
		{ (int)CharIds.Rock, new List<SubTank>() },
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		{ (int)CharIds.Kurumitos, new List<SubTank>() },
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


												this is to make it so your character is compatible with
															the SubTank section in the Upgrade Menu
	};

	// Heart tanks
	private Dictionary<int, ProtectedInt> charHeartTanks = new Dictionary<int, ProtectedInt>(){
		{ (int)CharIds.X, new() },
		{ (int)CharIds.Zero, new() },
		{ (int)CharIds.Vile, new() },
		{ (int)CharIds.Axl, new() },
		{ (int)CharIds.Sigma, new() },
		{ (int)CharIds.PunchyZero, new() },
		{ (int)CharIds.BusterZero, new() },
		{ (int)CharIds.Rock, new() },
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		{ (int)CharIds.Kurumitos, new() },  
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
										this is to make it so your character is compatible with
											the Heart Tank section in the Upgrade Menu
	};



	with this Done, you're gonna locate the "public Character? spawnCharAtPoint(" section
	and add him in the else if Chain


	// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	// Kurumitos 
		else if  (charNum == (int)CharIds.Kurumitos) {
			newChar = new Kurumitos(
				this, pos.x, pos.y, xDir,
				false, charNetId, ownedByLocalPlayer, isWarpIn: isWarpIn
			);
		}
	// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	// Error out if invalid id.
		else {
			throw new Exception("Error: Non-valid char ID: " + charNum);
		}   // you' don't need to put this throw exception it's just for reference
			for you to be able to Locate it and Add it above there



		then you're gonna locate "public void transformAxlNet" and add him to the else If 
		chain as well like this
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		else if (data.charNum == (int)CharIds.Kurumitos) {
			retChar = new Kurumitos(
				this, character.pos.x, character.pos.y, character.xDir,
				true, data.dnaNetId, false, isWarpIn: false
			);
		}
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


		then you'll add him to the "public void transformAxl" section's else if chain as well:

		else if (charNum == (int)CharIds.Kurumitos) {
			retChar = new Kurumitos(
				this, character.pos.x, character.pos.y, character.xDir,
				true, dnaNetId, true, isWarpIn: false
			);
		}
		

		with all of that done your Character is fully selectable in the Char menu and also
		compatible with the Axl DNA system , making him effectively finally a real char that won't
		chash the whole game For just existing

		*/


		// For special conditions stuff
		if (charState is WarpIn) player.superAmmo = 0;

	}



	// NormalCTRL: is for you to add moves that your new Character that he can do while
	// He isn't Softlocked in a motion be it an attack or a Damage State
	public override bool normalCtrl() {

		if (player.input.isL2Held(player) && grounded){
			changeState(new BlockWCUT());
		
		}
		// This is Where hypermode actiavtion happens
			if (player.input.isPressed(Control.Special2, player)
			&& player.currency > 4
			) {
				player.currency -= 5;
				changeState(new OverDriveStart(), true);
				/* 
				changeState() you'll be using this for every custom action your character does
				*/
			}

		return base.normalCtrl();
	}


	// AttackCtrl: is for you to add moves to your character that he can only perform
	// While the attackCtrl flag is active in a charstate and is conventionally where you add attacks
	public override bool attackCtrl() {

		if (player.input.isAPressed(player)) {
			if (grounded) { // For grounded only moves always add a if (grounded) flag
				if (player.input.isLeftOrRightHeld(player)) {
					changeState(new KurumitoFowardKick(), true);
				} else {
					changeState(new KurumitoStandingKick(), true);
				}
			}
			if (!grounded) { // For grounded only moves always add a if (!grounded) flag
				/*the "!" in any bool means it's the opposite but I assume you know the fucking
				basics of code language to be even reading this 
				*/
				changeState(new KurumitosAirDunk(), true);
			}
		}

		

		if (player.input.isBPressed(player)) {		
			changeState(new KurumitosDokuGami(), true); 
		}

		if (player.input.isL2Held(player)
		&& player.input.isAPressed(player)) {
		
			changeState(new KurumitoGrabStartState(), true); 
		}


		if (player.input.isR2Pressed(player)) {
			shoot(0);
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
		// For Overdrive to work
		if (overDriveTimer > 0) {
		OverDrive = true;
		} else {
			OverDrive = false;
		}
		Helpers.decrementTime(ref overDriveTimer);
		// For Cooldowns and other stuff that has deepleeting time
		Helpers.decrementTime(ref ShikiYamiBaraiCD);
		

		// Charge and release charge logic.
		chargeLogic(shoot);



		if (canSpecialCancel) {
			if (player.input.checkShoryuken(player, xDir, Control.Shoot)) {
				changeState(new KurumitosShikiOniaki(), true);

			}


			if (player.input.checkShoryuken(player, xDir, Control.Special1) && player.superAmmo > 26) {
				changeState(new KurumitosOrochinagiCharge(), true);
				player.superAmmo = 0;

			}

			if (getChargeLevel() > 0 && !chargeButtonHeld()) {
				changeState(new KurumitosShikiYamiBaraiLv2(), true);
				stopCharge();
			}


		}
		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
			}
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
		return "kr_" + spriteName;
		// NOTE: your character needs at bare minimum
		// _idle.json, _warp_in.json and _warp_beam.json in order to
		// work or else the game will crash when he Spawns
	}


	// for the melee hitbox to work
	// This can run on both owners and non-owners. So data used must be in sync.
	public enum MeleeIds {
		None = -1,

		Blocking, // you add more and more and finish with "," always for each move you add
		StandingKick,
		AirDunk,
		ShikiYamiBarai,
		ShikiOniaki,
		Orochinagi,
		Grab,

		Ombrada,
		Dokugami,
		Tsuyomi,
		Batsuyomi,
	}


	// these are where the sprites referenced with each melee
	// IDs are located
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"kr_block"  /*referenced sprite*/ => MeleeIds.Blocking, /*melee ID related to said sprite*/
			"kr_kick_1" or "kr_commandkick" => MeleeIds.StandingKick,
			"kr_batsuyomi" => MeleeIds.Batsuyomi,
			"kr_dokugami" => MeleeIds.Dokugami,
			"kr_tsuyomi" => MeleeIds.Tsuyomi,
			"kr_air_dunk" or "kr_ombrada" => MeleeIds.AirDunk,
			"kr_shiki_yami_barai_melee" => MeleeIds.ShikiYamiBarai,
			"kr_shiki_oniaki" => MeleeIds.ShikiOniaki,
			"kr_grab_start" => MeleeIds.Grab,
			"kr_orochinagi_fire" or "kr_orochinagi_fire_air" => MeleeIds.Orochinagi,

			_ => MeleeIds.None
		});
	}

	// this is where you effectively make the melee hitboxes trigger
	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), // referenced weapon to make it compatible with the
							   // Weakness system and also the killfeed
				projPos, // to make sure it's where the hitbox is placed
				ProjIds.BlockingProjID, // this is the projectile ID referenced to it
				/*
				NOTE: make sure you add every projectile ID to the "Enums.cs"'s "ProjIDs" section
				or else it won't work
				*/
				player, // means the player owns it
				damage: 0.0f, // how much dmg
				flinch: 0, // how many frames will the person be flinched or not at all
				hitCooldown: 0, // how many frames until that hitbox can be effective again
								// Ideally you shorten this if you want it to multihit
				isShield: false,// non piercing projectiles are destroyed on contact and can clang things 
								// with the "isZSaberClang" propety On
				isReflectShield: false, // Projectiles are sent the opposite way when in contact and can clang
				isDeflectShield: true,// projectiles are sent up in the air when in contact and can clang stuff
				isZSaberClang: false,// this propety makes it so your move clangs in contact shield type hitboxes
				isZSaberEffect: false,// adds the Zsaber slashing effect
				addToLevel: addToLevel // make sure this is always active like this or your projectile won't work
			),



			(int)MeleeIds.AirDunk => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MechFrogGroundPound, player,
				 2, Global.defFlinch, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),


			(int)MeleeIds.StandingKick => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				 2, Global.halfFlinch,20, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			

			(int)MeleeIds.Dokugami => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.MagnetMine, player,
				 2, Global.halfFlinch,10, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.Tsuyomi => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GravityWell, player,
				 2, Global.halfFlinch,10, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.Batsuyomi => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.KRStandingKick, player,
				 2, Global.defFlinch,10, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),

			(int)MeleeIds.ShikiOniaki => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Ryuenjin, player,
				 2, Global.halfFlinch, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.ShikiYamiBarai => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Ryuenjin, player,
				 1, Global.halfFlinch,10, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Orochinagi => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.Ryuenjin, player,
				 2, Global.halfFlinch,10, isReflectShield: true,
				isZSaberClang: true, isZSaberEffect: true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Grab => new KRGenericMeleeProj(
				new KRMelee(), projPos, ProjIds.GenericWCUTGrabProjID, player,
				 1,0,10, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
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



	/* make sure to add this in GameMode.cs in the "public void renderAmmo(" section
	in order for your ammo baar to even display 


	if (player.character is Kurumitos) {
			baseY += 25;
			Global.sprites["hud_weapon_base"].drawToHUD(39, baseX, baseY);
			baseY -= 16;
			for (var i = 0; i < MathF.Ceiling(player.vileMaxAmmo * ammoDisplayMultiplier); i++) {
				if (i < Math.Ceiling(player.vileAmmo * ammoDisplayMultiplier)) {
					Global.sprites["hud_weapon_full"].drawToHUD(32, baseX, baseY);
				} else {
					Global.sprites["hud_health_empty"].drawToHUD(0, baseX, baseY);
				}
				baseY -= 2;
			}
			Global.sprites["hud_health_top"].drawToHUD(0, baseX, baseY);
			return;
		}
	*/





	// Name is self explanatory but this is for your character to be able to use 
	// The charge mechanic from the Megaman Games Just like megaman Games
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

	public override int getMaxChargeLevel() {
		return 4;
	}




	public override bool chargeButtonHeld() {
		return player.input.isR2Held(player);
	}

	public override void increaseCharge() {
		float factor = 1;
		if (OverDrive) factor = 1.5f; // this means during OverDrive he gets a chargespeed buff
		chargeTime += Global.speedMul * factor;
	}

	public override float getRunSpeed() {
		float runSpeed = 90;
		if (OverDrive) { // this means during OverDrive he gets a speed buff
			runSpeed *= 1.15f;
		}
		return runSpeed * getRunDebuffs();
	}




	// Shoots stuff.
	public void shoot(int chargeLevel) {


		if (chargeLevel == 0) {
			changeState(new KurumitosShikiYamiBaraiLv1(), true);
			stopCharge();
		} else if (chargeLevel == 1) {
			changeState(new KurumitosShikiYamiBaraiLv2(), true);
			stopCharge();
		} else if (chargeLevel == 2) {
			changeState(new KurumitosShikiYamiBaraiLv2(), true);
			stopCharge();
		} else if (chargeLevel == 3) {
			changeState(new KurumitosShikiYamiBaraiLv2(), true);
			stopCharge();
		} else if (chargeLevel >= 4) {
			changeState(new KurumitosOrochinagi(), true);
			stopCharge();
		}
		if (chargeLevel >= 1) {
			stopCharge();
		}
	}




	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;

		if (OverDrive) {
			palette = player.zeroPaletteShader;
			palette?.SetUniform("palette", 1);
			palette?.SetUniform("paletteTexture", Global.textures["hyperBusterZeroPalette"]);
		}

		if (Global.isOnFrameCycle(4)) {
			switch (getChargeLevel()) {
				case 1:
					palette = Player.ZeroBlueC;
					break;
				case 2:
					palette = Player.ZeroBlueC;
					break;
				case 3:
					palette = Player.ZeroPinkC;
					break;
				case 4:
					palette = Player.ZeroGreenC;
					break;
			}
			if (OverDrive) {
				palette = Player.XOrangeC;
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
