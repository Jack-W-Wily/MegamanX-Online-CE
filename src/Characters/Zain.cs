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

	private float CounterCooldown;

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
		//Helpers.decrementTime(ref CounterTimer);
		Helpers.decrementTime(ref CounterCooldown);
		Helpers.decrementTime(ref SlashCooldown);
		if (ZainCounters > 8 ) ZainCounters = 8;
		if (ZainCounters <= 0) {
			ZainCounters = 0;
			counterCooldown = 1;
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
		if (charState.attackCtrl){
			if ((charState is AirDash) 
			&& (player.input.isPressed(Control.Shoot, player)
			|| player.input.isPressed(Control.Special1, player))){
			slideVel = xDir * getDashSpeed();			
			}
		}
		if (player.input.isHeld(Control.Down,player)
		&& charState is Dash or AirDash or Fall or Jump &&
		!sprite.name.Contains("spinslash")){
		changeSpriteFromName("spinslash", true);
		}
		if (player.input.isHeld(Control.Special1,player)
		&& charState.attackCtrl && charState is not Dash or AirDash &&
		!sprite.name.Contains("jab")){
		changeSpriteFromName("jab", true);
		}
		if (sprite.name.Contains("jab") && isAnimOver()){
		changeToIdleOrFall();
		}
		if (charState.attackCtrl  &&
		player.input.isPressed(Control.Shoot, player))
		{		
       		changeState(new ZainProjSwingState(grounded, shootProj: false), forceChange: true);
		}
		if ((charState.attackCtrl || charState.bonusAttackCtrl)  && ZainCounters > 0 &&
		player.input.isPressed(Control.WeaponRight, player))
		{	
			changeState(new ZainKokuSlash(grounded, shootProj: false), forceChange: true);
			ZainCounters -= 1;
        }
		if ((charState.attackCtrl || charState.bonusAttackCtrl)  && ZainCounters > 3 &&
		player.input.isPressed(Control.Special2, player))
		{	
			changeState(new ZainProjSwingState(grounded, shootProj: true), forceChange: true);
			ZainCounters -= 4;
		}

		if ((charState.attackCtrl || charState.bonusAttackCtrl) 
		 &&	 player.input.isPressed(Control.WeaponLeft, player) 
		 && player.input.isHeld(Control.Up,player))
			{ 
			changeState(new ZainParryStartState(), true);
			}
		
		 if  (player.input.isPressed(Control.WeaponLeft,player)
			&&  (charState.attackCtrl || charState.bonusAttackCtrl) 
			&& !player.input.isHeld(Control.Up,player)
			) {
			if (unpoAbsorbedProj != null) {
				changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
				unpoAbsorbedProj = null;		
			} else {
				changeState(new XUPParryStartState(), true);
			}
		}

		 if (player.input.isPressed(Control.Special1,player) && parryCooldown == 0 &&
			  (charState is Idle || charState is Run || charState is Fall || charState is Jump || charState is XUPPunchState || charState is XUPGrabState)
			) {
			if (unpoAbsorbedProj != null) {
				changeState(new XUPParryProjState(unpoAbsorbedProj, true, false), true);
				unpoAbsorbedProj = null;	
			}
			}
		
		if (ZainCounters >= 8) return;

		player.vileAmmo += Global.spf * 15;
		if (player.vileAmmo > player.vileMaxAmmo) {
			player.vileAmmo = 0;
			ZainCounters += 1;
			}
	}


	public override bool isToughGuyHyperMode() {
		return true;
	}

	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}
	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}
	public override bool canAddAmmo() {
		return player.vileAmmo < player.vileMaxAmmo;
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
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.BlockableLaunch, player, 2f, 0, 10f, null, isShield: true, isDeflectShield: true);
		}
		if (  sprite.name.Contains("spinslash"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.ZSaberRollingSlash, player,
				1, 10, 5f, isDeflectShield: true,
				isJuggleProjectile: true,
				ShouldClang : true
			);
		}
		if (  sprite.name.Contains("projswing_air"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber3, player, 4f, 30, 15f, ShouldClang : true);
		}
		if (  sprite.name.Contains("jab"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.UPPunch, player, 2f, 10, 15f, ShouldClang : true);
		}
		if (  sprite.name.Contains("slash") && !sprite.name.Contains("uppercut"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber2, player, 3f, 20, 15f, ShouldClang : true);
		}
		if (  sprite.name.Contains("uppercut"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber1, player, 3f, 20, 15f, ShouldClang : true,
			 isJuggleProjectile : true);
		}
		if (  sprite.name.Contains("projswing") && !sprite.name.Contains("air"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.MechFrogGroundPound , player, 5f, 20, 15f, ShouldClang : true);
		}
		if (  sprite.name.Contains("parry"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.MechFrogStompShockwave, player, 1f, 0, 15f);
		}
		if (  sprite.name.Contains("thrust"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint, ProjIds.SpreadShot, player, 2f, 0, 15f);
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
		if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("zainalt_" + spriteName)){		
			return "zainalt_" + spriteName;
			}
			return "zain_" + spriteName;
	}
}

