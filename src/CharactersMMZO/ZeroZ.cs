namespace MMXOnline;

public class ZeroZ : Character {
	public ZeroZ(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
		charId = CharIds.ZeroZ;
	}


	//Zero's stuff 
	public override void update(){
	base.update();

	// Zero Attacks
	if (charState.canAttack()){
	
		// Tripple slash>>>>>
		if (player.input.isPressed(Control.Shoot, player) && charState is Idle){
		changeState(new ZeroZSlash1(), true);
		}
		//>>>>>>>>>


		// Dash Slash >>>>>>>>>>>>>
		if (player.input.isPressed(Control.Shoot, player) && charState is Dash){
		changeState(new ZeroZDashSlash(), true);
			slideVel = xDir * getDashSpeed();
		}
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

		// Jump Slash >>>>>>>>>>>>>
		if (player.input.isPressed(Control.Shoot, player) && !grounded){
		changeState(new ZeroZJumpSlash(), true);
		}
		// >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>





	}
	
	
	}

	public override bool canDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "zeroz_" + spriteName;
	}

	
	public override Projectile? getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile? proj = null;

		if (sprite.name.Contains("slash_1")) {
		proj = new GenericMeleeProj(
				new ZSaber(), centerPoint, ProjIds.ZSaber1, player, 1, 5, 0.25f, isReflectShield: false
			);
		} 
		if (sprite.name.Contains("slash_2")) {
		proj = new GenericMeleeProj(
				new ZSaber(), centerPoint, ProjIds.ZSaber2, player, 1, 10, 0.25f, isReflectShield: false
			);
		} 
		if (sprite.name.Contains("slash_3")) {
		proj = new GenericMeleeProj(
				new ZSaber(), centerPoint, ProjIds.ZSaber3, player, 3, 20, 0.3f, isReflectShield: false
			);
		} 
		if (sprite.name.Contains("dash_slash")) {
		proj = new GenericMeleeProj(
				new ZSaber(), centerPoint, ProjIds.ZSaber1, player, 2, 20, 0.3f, isReflectShield: false
			);
		} 
		if (sprite.name.Contains("jump_slash")) {
		proj = new GenericMeleeProj(
				new ZSaber(), centerPoint, ProjIds.ZSaber1, player, 2, 15, 0.3f, isReflectShield: false
			);
		} 

		return proj;
	}

}
