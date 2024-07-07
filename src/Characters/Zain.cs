namespace MMXOnline;

public class Zain : Character {
	public Zain(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}

	private float CounterTimer;

	private float CounterAddcooldown;

	public float shiningSparkStacks = 0;

	public bool isOnBike;

	public bool canUseBike;

	public float ZainCounters;


public override bool normalCtrl() {
	
		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded &&noBlockTime == 0 &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}
		return base.normalCtrl();
	}


	public override void update(){
		base.update();
		Helpers.decrementTime(ref CounterTimer);
		Helpers.decrementTime(ref CounterAddcooldown);
		player.vileAmmo += 1;
		if (ZainCounters >8 ) ZainCounters =8;
		if (ZainCounters < 0) ZainCounters = 0;
		if (CounterTimer == 0 && CounterAddcooldown == 0){
		ZainCounters += 1;
		CounterTimer = 4;
		CounterAddcooldown = 2;
		player.vileAmmo = 0;
		
		} 
		if (ZainCounters == 0){
		Global.level.gameMode.setHUDErrorMessage(player, "0", playSound: false);		
		}
		if (ZainCounters == 1){
		Global.level.gameMode.setHUDErrorMessage(player, "1", playSound: false);		
		}
		if (ZainCounters == 2){
		Global.level.gameMode.setHUDErrorMessage(player, "2", playSound: false);		
		}
		if (ZainCounters == 3){
		Global.level.gameMode.setHUDErrorMessage(player, "3", playSound: false);		
		}
		if (ZainCounters == 4){
		Global.level.gameMode.setHUDErrorMessage(player, "4", playSound: false);		
		}

		


		//KillingSpreeThemes
		if (KillingSpree == 5){
				if (musicSource == null) {
					addMusicSource("boss2", getCenterPos(), true);
				}
		} 


		if (charState.canAttack()){
			if ((charState is Dash || charState is AirDash) 
			&& (player.input.isPressed(Control.Special1, player)
			|| player.input.isPressed(Control.Shoot, player))){
			slideVel = xDir * getDashSpeed();			
			}
		}
		if (charState.canAttack()  &&
		 player.input.isPressed("shoot", player))
		{	
       		changeState(new ZSaberProjSwingState(grounded, shootProj: false), forceChange: true);
		}
		if (charState.canAttack()  && grounded && ZainCounters > 3 &&
		 player.input.isPressed(Control.Special2, player))
		{	
			changeState(new ZSaberProjSwingState(grounded, shootProj: true), forceChange: true);
			ZainCounters -= 4;
			CounterTimer = 4;
       }
		if (charState.canAttack() && xSaberCooldown == 0f &&
		 player.input.isPressed("special1", player))
					{
			xSaberCooldown = 1f;     
			changeState(new KKnuckleParryStartState(), true);
		}
		
		
		
		
	}


// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}
		
		if (  sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new HoutenjinWeapon(player), centerPoint, ProjIds.BlockableLaunch, player, 2f, 0, 1f, null, isShield: true, isDeflectShield: true);
		}
		if (  sprite.name.Contains("spinslash"))
		{
			return new GenericMeleeProj(new RisingWeapon(player), centerPoint, ProjIds.Rising, player, 2f, 10, 0.5f);
		}
		if (  sprite.name.Contains("projswing"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 5f, 30, 0.15f);
		}
		if (  sprite.name.Contains("parry"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.MechFrogStompShockwave, player, 5f, 30, 0.15f);
		}
		if (  sprite.name.Contains("thrust"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.Raijingeki, player, 2f, 0, 0.15f);
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

