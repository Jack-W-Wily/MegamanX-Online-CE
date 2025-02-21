namespace MMXOnline;

public class Iris : Character {
	public Iris(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
			charId = CharIds.Iris;
	}

	public NewIrisCrystal irisCrystal;
	public IrisCannon iriscannon;

	public float IrisGeneralizedCrystalCD;

	public bool usedcannonONce = false;
	

	public override bool normalCtrl() {
	
		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded  &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}


			
		return base.normalCtrl();
	}


	public override bool attackCtrl() {
	
		

			if (iriscannon == null && player.health > 0 
		 && !usedcannonONce && 
		 player.input.isPressed(Control.WeaponLeft, player) 
		  && player.input.isHeld(Control.Up, player)
		 && ownedByLocalPlayer && !Global.level.gameMode.isOver )	{
			usedcannonONce = true;
            new IrisCannon(new SigmaSlashWeapon(), pos.addxy(-30,-30), xDir, player, player.getNextActorNetId(), 4, 35, rpc: true);
		}

				if (IrisGeneralizedCrystalCD == 0f &&
		 player.input.isPressed(Control.WeaponLeft, player)  && !player.input.isHeld(Control.Up, player) )
			{
				IrisGeneralizedCrystalCD = 1.5f;
				
                  	new IrisSlashProj(new SigmaSlashWeapon(), pos, xDir, player, player.getNextActorNetId(), rpc: true);
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




		public override void update(){
		base.update();

		// Cooldowns
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


		if (irisCrystal == null && player.health > 0 && ownedByLocalPlayer && !Global.level.gameMode.isOver){
		irisCrystal = new NewIrisCrystal(new IrisCrystal(), pos, getShootXDir(), player, player.getNextActorNetId(), rpc: true);
		}

	
		}


		

	public override bool canDash() {
		return true;
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
				new IrisCrystal(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}
		 if (  sprite.name.Contains("attack") && !sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.VirusSlash, player, 3f, 30, 20, ShouldClang : true);
		}
		 if (  sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new IrisCrystal(), centerPoint, ProjIds.VirusSlash, player, 3f, 30, 20, ShouldClang : true);
		}

		return proj;
	}
}


