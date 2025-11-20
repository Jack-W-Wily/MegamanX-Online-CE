
using System;
using System.Collections.Generic;



namespace MMXOnline;

public class Iris : Character {
	public Iris(
			Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
			charId = CharIds.Iris;
	}

	public NewIrisCrystal irisCrystal;

	public Actor GrabVictim;

	public IrisCannon iriscannon;

	public float IrisGeneralizedCrystalCD;


	public float CannonSlashCD;

	public float CannonStabCD;

	public bool usedcannonONce = false;
	

	public override bool normalCtrl() {
	
		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT());
		}
		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);	
		}
		if (player.input.isL2Held(player) && player.input.isAPressed(player)) {
			changeState(new IrisGrabStart(), forceChange: true);
		}


			
		return base.normalCtrl();
	}


	public override bool attackCtrl() {

		if (player.input.isHeld(Control.Down, player) && !grounded &&
		 player.input.isPressed(Control.Dash, player))
		{	
        changeState(new IrisDiveKick(), true);
		}
		if (!player.input.isHeld(Control.Up, player) &&
		 player.input.isPressed(Control.Shoot, player))
		{	
        changeState(new IrisCrystalBashState(), true);
		}
		if ( player.input.isHeld(Control.Up, player) &&
		 player.input.isPressed(Control.Shoot, player))
		{	
        changeState(new IrisCrystalRisingBash(), true);
		}
		if ( !player.input.isHeld(Control.Up, player) &&
		 player.input.isPressed(Control.Special1, player))
		{	
        changeState(new IrisCrystalCharge(), true);
		}
		return base.attackCtrl();
	}




	public override void update() {
		base.update();

		// Perifericos
		if (!isInDamageSprite()) {

			if (iriscannon == null && player.health > 0
			 && !usedcannonONce &&
			 player.input.isPressed(Control.WeaponLeft, player)
			  && player.input.isHeld(Control.Up, player)
			 && ownedByLocalPlayer && !Global.level.gameMode.isOver) {
				usedcannonONce = true;
				playSound("distortion_a", true);
				new IrisCannon(new IrisCrystal(), pos.addxy(-30, -30), xDir, player, player.getNextActorNetId(), 4, 35, rpc: true);
			}


			if (CannonSlashCD == 0f &&
			 player.input.isPressed(Control.WeaponLeft, player)
			 && !player.input.isHeld(Control.Up, player)
			  && !player.input.isHeld(Control.Left, player)
			   && !player.input.isHeld(Control.Right, player)) {

				playSound("distortion_a", true);
				CannonSlashCD = 1.5f;
				new IrisSlashProj(new IrisCrystal(), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			}

			if (CannonStabCD == 0f &&
		 player.input.isPressed(Control.WeaponLeft, player)
		 && !player.input.isHeld(Control.Up, player)
		 && (player.input.isHeld(Control.Left, player)
		  || player.input.isHeld(Control.Right, player))

		  ) {
				CannonStabCD = 1.25f;
				playSound("distortion_a", true);
				new IrisStabProj(new IrisCrystal(), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			}
		}

		if (player.input.isPressed(Control.Special2, player) && linkedRideArmor == null && player.currency > 2) {
			int raIndex = 7;
			player.currency -= 3;
			linkedRideArmor = new RideArmor(player, pos, raIndex, 0, player.getNextActorNetId(), true, sendRpc: true);
			changeState(new CallDownMech(linkedRideArmor, true), true);

		}

		if (player.input.isPressed(Control.Special2, player) && linkedRideArmor != null) {
				changeState(new CallDownMech(linkedRideArmor, true), true);

		}
		


		// Cooldowns
		Helpers.decrementTime(ref CannonSlashCD);
		Helpers.decrementTime(ref CannonStabCD);
		Helpers.decrementTime(ref IrisGeneralizedCrystalCD);
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

		// No need to delete this code and we'll just reactivate this once I re-add the Killing
		// Spree mechanic ok?
		//		//KillingSpreeThemes
		//	if (KillingSpree == 3){
		//			if (musicSource == null) {
		//	if (Helpers.randomRange(0,1) == 0)	addMusicSource("iris", getCenterPos(), true);  
		//	if (Helpers.randomRange(0,1) == 1)	addMusicSource("MakenaiAiGaKittoAru", getCenterPos(), true); 
		//
		//			}
		//	} 


		if (irisCrystal == null && player.health > 0 && ownedByLocalPlayer && !Global.level.gameMode.isOver) {
			irisCrystal = new NewIrisCrystal(new IrisCrystal(), pos, getShootXDir(), player, 0,
							player.getNextActorNetId(true), true);

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
		return "iris_" + spriteName;
	}


	public override Projectile getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile proj = null;
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new IrisCrystal(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true ,addToLevel: true
			);
		}
		 if (  sprite.name.Contains("attack") && !sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.VirusSlash,
			player, 2f, 20, 20, isZSaberClang : true ,addToLevel: true, hitSound : "kofhtsnd_lightning1"
			);
		}
		
		 if (  sprite.name.Contains("grab") && !sprite.name.Contains("ex") )
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.ForceGrabState,
			player, 0f, 0, 20, isZSaberClang : true ,addToLevel: true, hitSound : "kofhtsnd_grab1"
			);
		}

		 if (  sprite.name.Contains("grab") && sprite.name.Contains("ex") )
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.BlockableLaunch,
			player, 3f, 0, 20, isZSaberClang : false ,addToLevel: true, hitSound : "swordswipeGG"
			);
		}

		 if (sprite.name.Contains("rising")) {
			return new GenericMeleeProj(new IrisCrystal(), centerPoint,
			ProjIds.VirusSlash, player, 3f, 20, 20,
			isZSaberClang: true, addToLevel: true, isJuggleProjectile: true, hitSound : "kofhtsnd_lightning1"
			);
		}

		 if (  sprite.name.Contains("dive_kick"))
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint,
			ProjIds.GBDKick, player, 2f, 20, 20, 
			isZSaberClang : true ,addToLevel: true, hitSound : "kofhtsnd_punch3"
			);
		}


		return proj;
	}




	
	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;



		if (player.skinSlot == 1) {
			palette = player.nightmareZeroShader;
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

}


