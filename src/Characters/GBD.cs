namespace MMXOnline;

public class GBD : Character {
	public GBD(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}

	private float flameCreateTime = 0;

	private float SuperGBDCreateTime = 0;

	public float shiningSparkStacks = 0;

	public bool isOnBike;

	public bool canUseBike;


public override bool normalCtrl() {
	
		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded && noBlockTime == 0 &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}
		return base.normalCtrl();
	}


	public override void update(){
		base.update();

		if (!canUseBike && player.input.isPressed(Control.Special2, player) && player.currency > 2){
		player.currency -= 3;
		canUseBike = true;
		Global.level.gameMode.setHUDErrorMessage(
					player, "Enabled Ride Chaser.", playSound: false, resetCooldown: true
				);
		}
		//KillingSpreeThemes
		if (KillingSpree == 5){
				if (musicSource == null) {
					addMusicSource("boss2", getCenterPos(), true);
				}
		} 


		if (charState.canAttack() && shiningSparkStacks > 10){
			if ((charState is Dash || charState is AirDash) && (player.input.isPressed(Control.Special1, player)
			|| player.input.isPressed(Control.Shoot, player)
			|| player.input.isPressed(Control.WeaponLeft, player)
			|| player.input.isPressed(Control.WeaponRight, player)
			) ){
			slideVel = xDir * getDashSpeed();			
			}
		}
		
			flameCreateTime += Global.spf;
			SuperGBDCreateTime += Global.spf;
			if (flag == null) {
			if (charState is not Hurt &&
				charState is not InRideArmor && (
				deltaPos.x != 0 ||
				charState is Run ||
				charState is Dash ||
				charState is AirDash ||
				charState is WallKick 
			)) {
				if (shiningSparkStacks < 6) {
					shiningSparkStacks += Global.spf * 0.75f;
				} else {
					shiningSparkStacks += Global.spf;
				}
				if (shiningSparkStacks < 125 && shiningSparkStacks > 6) {
					if (shiningSparkStacks < 16) {
						shiningSparkStacks += (Global.spf * (shiningSparkStacks - 6f)) / 8;
					} else {
						shiningSparkStacks += 0.125f;
					}
				}
			} else if (
				shiningSparkStacks > 0 &&
				!charState.inTransition() && (
					charState is Idle ||
					charState is Jump ||
					charState is Crouch ||
					charState is Fall ||
					charState is Hurt
				)
			) {
				if (shiningSparkStacks > 1) {
					shiningSparkStacks -= Global.spf * (5 * shiningSparkStacks);
				} else {
					shiningSparkStacks -= Global.spf * 5;
				}
				if (shiningSparkStacks < 0) {
					shiningSparkStacks = 0;
				}
				if (shiningSparkStacks > 40) {
					shiningSparkStacks = 40;
				}
			}
		} else {
			shiningSparkStacks = 0;
		}
		if (shiningSparkStacks < 0) {
					shiningSparkStacks = 0;
				}

		if (dashedInAir == 0 && charState.canAttack() && player.input.isHeld("up", player) 
		&& player.input.isPressed("jump", player) && charState is not WallKick)
					{
            isDashing = true;
			dashedInAir += 1;
			vel.y = 0f - getJumpPower();
			changeState(new WallKick(), true);
					}
		if (charState.canAttack()  && !isOnBike &&
		 player.input.isPressed("shoot", player))
		{	
			if ((player.input.isHeld("left", player) ||
			player.input.isHeld("right", player)) 
			&& !player.input.isHeld("up", player)){
       		 changeState(new GBDSpear1(), true);
			}
			if ((!player.input.isHeld("left", player) &&
			!player.input.isHeld("right", player)) 
			&& player.input.isHeld("up", player)){
       		 changeState(new GBDSpearUp(), true);
			}
			if ((!player.input.isHeld("left", player) &&
			!player.input.isHeld("right", player)) 
			&& !player.input.isHeld("up", player)){
       		 changeState(new GBDSpearSpin(), true);
			}
			if ((player.input.isHeld("left", player) ||
			player.input.isHeld("right", player)) 
			&& player.input.isHeld("up", player)){
       		 changeState(new GBDSpearRising(), true);
			}
			
		}
		if (charState.canAttack() && xSaberCooldown == 0f &&
		 player.input.isPressed("special1", player))
					{
				xSaberCooldown = 1f;
               if (player.input.isHeld("up", player)){
				 changeState(new EnfrenteMeuDisco2(), true);
			   }
			    if (!player.input.isHeld("up", player)){
				 changeState(new EnfrenteMeuDisco(), true);
			   }
					}
		if (canUseBike && player.input.isPressed(Control.Special2, player))
					{	
        if (!player.input.isHeld(Control.Down, player))  isOnBike = true;
		if (player.input.isHeld(Control.Down, player))  isOnBike = false;
					}




		
			if (charState is KnockedDown)  isOnBike = false;
				
		if (charState.canAttack() && !isOnBike && xSaberCooldown == 0f &&
		 player.input.isPressed(Control.WeaponRight, player))
					{
			changeState(new XTeleportState(), true);
			//xSaberCooldown = 1f;
            //new MagnetMine().getProjectile(getShootPos(), getShootXDir(), player, 0, player.getNextActorNetId());
					}

		
		
	}


// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}
			else if (sprite.name.Contains("dash") && (isOnBike || shiningSparkStacks > 10 ))
		{
			return  new GenericMeleeProj(player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 2f, 8, 0.5f, null, isShield: false, isDeflectShield: true);;
		}
		else if (sprite.name.Contains("jump") && player.input.isPressed("jump", player))
		{
			return new GenericMeleeProj(new GBDKick(), centerPoint, ProjIds.GBDKick, player, 0f, 0, 0f);
		}
		else if (!isOnBike &&  sprite.name.Contains("fall") && player.input.isPressed("jump", player))
		{
			return new GenericMeleeProj(new GBDKick(), centerPoint, ProjIds.GBDKick, player, 0f, 0, 0f);
		}
		else if (  sprite.name.Contains("wall_kick") && shiningSparkStacks < 20)
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 2f, 12);
		}
		else if (  sprite.name.Contains("wall_kick") && shiningSparkStacks >= 20)
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 3f, 30);
		}
		if (  sprite.name.Contains("spear_1"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 2f, 30);
		}
		if (  sprite.name.Contains("spear_up"))
		{
			return new GenericMeleeProj(new RisingWeapon(player), centerPoint, ProjIds.Rising, player, 2f, 30);
		}
		if (  sprite.name.Contains("spear_rising"))
		{
			return new GenericMeleeProj(new RisingWeapon(player), centerPoint, ProjIds.Rising, player, 2f, 30);
		}
		if (  sprite.name.Contains("spear_spin"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 1f, 10, 0.15f);
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
		if (isOnBike) return "tgbdbike_" + spriteName;
		return "tgbd_" + spriteName;
	}
}

