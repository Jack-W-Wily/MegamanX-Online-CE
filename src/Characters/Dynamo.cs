namespace MMXOnline;



public class Dynamo : Character {
	public Dynamo(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}

	public int NightmareBullets = 0;


	public int DynamoCounters = 0;

	public int DynamoStance = 0;

	public float DynamoSlashCD;
	public float DynamoStringCD;
	public float DynamoBoomerangCD;
	public float DynamoPunchCD;

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


	public override void update() {
		base.update();


		//KillingSpreeThemes
		if (KillingSpree == 5){
				if (musicSource == null) {
					addMusicSource("dynamo", getCenterPos(), true);
				}
		} 


		player.changeWeaponControls();
			Helpers.decrementTime(ref DynamoSlashCD);
			Helpers.decrementTime(ref DynamoStringCD);
			Helpers.decrementTime(ref DynamoBoomerangCD);
			Helpers.decrementTime(ref DynamoPunchCD);
		//dynamostuff
		if (player.weapon is DynamoTrick)
		{
		DynamoStance = 0;
		}
		if (player.weapon is DynamoSword)
		{
		DynamoStance = 1;
		}
		if (player.weapon is DynamoRoyal)
		{
		DynamoStance = 2;
		}
		if (player.input.isPressed(Control.WeaponLeft, player))
		{
		if (grounded){
		changeState(new Idle());
		}
		}
		if (player.input.isPressed(Control.WeaponRight, player))
		{
		if (grounded){
		changeState(new Idle());
		}
		}
		// Dynamo Stance 1
		if (DynamoStance == 0){
		if (grounded && charState.canAttack() && player.input.isHeld("shoot", player))
					{
                 changeState(new DynamoBoomerangState());
					}
		if (charState.canAttack() && player.input.isHeld("special1", player))
					{
                 changeState(new DynamoGroundPoundCharge());
					}
		if (xSaberCooldown == 0f && !grounded && charState.canAttack() && player.input.isHeld("shoot", player))
					{
				xSaberCooldown = 1f;
			changeState(new ZSaberProjSwingState(grounded, shootProj: false), forceChange: true);
					}
		}
		// Dynamo STance 2
		if (DynamoStance == 1){
		if (DynamoStringCD == 0f && grounded &&
		charState.canAttack() && player.input.isHeld("shoot", player))
					{
		DynamoStringCD = 0.5f;
		changeState(new DynamoString1());
			}
		if (DynamoSlashCD == 0f && !grounded && charState.canAttack() && player.input.isHeld("shoot", player))
				{
		DynamoSlashCD = 1f;
		changeState(new ZSaberProjSwingState(grounded, shootProj: false), forceChange: true);
				}
		if (DynamoBoomerangCD == 0 && 
		charState.canAttack() && player.input.isPressed("special1", player))
					{
		DynamoBoomerangCD = 1.2f;
        changeState(new EnfrenteMeuDisco2());		
		}
		}
		// Dynamo STance 3
		if (DynamoStance == 2){
		if (grounded && charState.canAttack() && player.input.isPressed("shoot", player))
					{
				if (NightmareBullets > 0)
					{
			if (NightmareBullets != 3)changeState(new DynamoShoot(grounded, shootProj: false), forceChange: true);
			if (NightmareBullets == 3)changeState(new DynamoNightmareBullet(grounded, shootProj: false), forceChange: true);			
				NightmareBullets -= 1;				
					}
                 changeState(new DynamoParryStartState());
					}
		if (DynamoBoomerangCD == 0 && 
		charState.canAttack() && player.input.isPressed("special1", player))
					{
				DynamoBoomerangCD = 1.5f;
               changeState(new NovaStrikeState(), forceChange: true);
					}
		if (dashedInAir == 0 && !grounded && charState.canAttack() && player.input.isHeld("shoot", player))
					{
				dashedInAir += 1; 
			changeState(new DynamoAirShoot(), forceChange: true);
					}
		}
		
	}

	// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("_block")) {
			if (frameIndex == 2){
			return new GenericMeleeProj(
				player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player,
				1, 10, 1, isDeflectShield: true
			);
		}
			return new GenericMeleeProj(
				player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player,
				0, 0, 0, isDeflectShield: true
			);
		}
		else if ( sprite.name.Contains("_projswing") && !collider.isHurtBox())
		{
			return new GenericMeleeProj(player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 3f, 15, 0.9f, null, isShield: false, isDeflectShield: true);
		}
		else if ( sprite.name.Contains("_string") && !collider.isHurtBox())
		{
			return new GenericMeleeProj(player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 2f, 15, 0.15f, null, isShield: true, isDeflectShield: true);
		}
		else if ( sprite.name.Contains("_nova_strike") && !collider.isHurtBox())
		{
			return new GenericMeleeProj(player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 1f, 15, 0.15f, null, isShield: true, isDeflectShield: true);
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
		return "dynamo_" + spriteName;
	}
}

