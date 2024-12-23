namespace MMXOnline;

public class Zain : Character {
	public Zain(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.Zain;
	}

	private float CounterTimer;

	private float CounterAddcooldown;

	private float SlashCooldown;

	

	public float ZainCounters;


public override bool normalCtrl() {
	
		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}
		return base.normalCtrl();
	}


	public override void update(){
		base.update();


		//Cooldowns
		Helpers.decrementTime(ref CounterTimer);
		Helpers.decrementTime(ref CounterAddcooldown);
		Helpers.decrementTime(ref SlashCooldown);


		

		player.vileAmmo += 1;
		if (ZainCounters >8 ) ZainCounters =8;
		if (ZainCounters < 0) ZainCounters = 0;
		if (CounterTimer == 0 && CounterAddcooldown == 0){
		ZainCounters += 1;
		CounterTimer = 4;
		CounterAddcooldown = 2;
		player.vileAmmo = 0;
		
		} 
		

		


		//KillingSpreeThemes
		//if (KillingSpree == 3){
		//		if (musicSource == null) {
		//			addMusicSource("RequiemNitanchouDiesIrae", getCenterPos(), true); 
		///		}
		//} 


		if (charState.attackCtrl){
			if ((charState is Dash || charState is AirDash) 
			&& (player.input.isPressed(Control.Shoot, player))){
			slideVel = xDir * getDashSpeed();			
			}
		}

		
		if (player.input.isHeld(Control.Down,player)
		&& charState is Dash or AirDash or Fall or Jump &&
		!sprite.name.Contains("spinslash")){
		changeSpriteFromName("spinslash", true);
		}

		if (charState.attackCtrl && SlashCooldown == 0 &&
		 player.input.isPressed(Control.Shoot, player))
		{	
			SlashCooldown = 0.5f;
       		changeState(new ZainProjSwingState(grounded, shootProj: false), forceChange: true);
		}
		if (charState.attackCtrl  && grounded && ZainCounters > 3 &&
		 player.input.isPressed(Control.Special2, player))
		{	
			changeState(new ZainProjSwingState(grounded, shootProj: true), forceChange: true);
			ZainCounters -= 4;
			CounterTimer = 4;
       }
		if (charState.attackCtrl && SlashCooldown == 0f &&
		 player.input.isPressed(Control.WeaponRight, player))
					{
			SlashCooldown = 1f;     
			changeState(new ZainParryStartState(), true);
		}
		
		 if  (player.input.isPressed(Control.WeaponLeft,player) && parryCooldown == 0 &&
				  (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is XUPPunchState || charState is XUPGrabState)
			  ) {
				if (unpoAbsorbedProj != null) {
					changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
					unpoAbsorbedProj = null;		
				} else {
					changeState(new XUPParryStartState(), true);
				}
		}

		 if  (player.input.isPressed(Control.Special1,player) && parryCooldown == 0 &&
				  (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is XUPPunchState || charState is XUPGrabState)
			  ) {
				if (unpoAbsorbedProj != null) {
					changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
					unpoAbsorbedProj = null;	
				}
			  }

		
		
		
	}


// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new SonicSlicer(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}
		
		if (  sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.BlockableLaunch, player, 2f, 0, 1f, null, isShield: true, isDeflectShield: true);
		}
		if (  sprite.name.Contains("spinslash"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.ZSaberRollingSlash, player,
				1, 10, 0.125f, isDeflectShield: true,
				isJuggleProjectile: true,
				ShouldClang : true
			);
		}
		if (  sprite.name.Contains("projswing"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.UPPunch, player, 5f, 30, 0.15f, ShouldClang : true);
		}
		if (  sprite.name.Contains("parry"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.VileSuperKick, player, 1f, 0, 0.15f);
		}
		if (  sprite.name.Contains("thrust"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.Raijingeki, player, 2f, 0, 0.15f);
		}
		return null;
	}


	public override bool canDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "zain_" + spriteName;
	}
}

