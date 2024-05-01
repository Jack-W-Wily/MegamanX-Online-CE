namespace MMXOnline;

public class Iris : Character {
	public Iris(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}

	public NewIrisCrystal irisCrystal;
	

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

		if (irisCrystal == null && player.health > 0 && ownedByLocalPlayer){
		irisCrystal = new NewIrisCrystal(new IrisCrystal(), pos, getShootXDir(), player, player.getNextActorNetId(), rpc: true);
		}

		if (charState.canAttack() && xSaberCooldown == 0f &&
		 player.input.isPressed(Control.WeaponLeft, player))
			{
				xSaberCooldown = 1.5f;
				
                changeState(new RaySplasherChargedState(), true);
			}

		if (charState.canAttack()  && !player.input.isHeld(Control.Up, player) &&
		 player.input.isPressed(Control.Shoot, player))
				{	
        changeState(new IrisCrystalBashState(), true);
				}
		if (charState.canAttack()  && player.input.isHeld(Control.Up, player) &&
		 player.input.isPressed(Control.Shoot, player))
				{	
        changeState(new IrisCrystalRisingBash(), true);
				}
		if (charState.canAttack()  && !player.input.isHeld(Control.Up, player) &&
		 player.input.isPressed(Control.Special1, player))
				{	
        changeState(new IrisCrystalCharge(), true);
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
				player.sigmaSlashWeapon, centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}
		 if (  sprite.name.Contains("attack") && !sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new XUPPunch(player), centerPoint, ProjIds.UPPunch, player, 3f, 30);
		}
		 if (  sprite.name.Contains("rising"))
		{
			return new GenericMeleeProj(new RisingWeapon(player), centerPoint, ProjIds.Rising, player, 3f, 30);
		}

		return proj;
	}
}


