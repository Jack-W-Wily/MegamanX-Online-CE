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
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		//Dynamo Attacks
		if (player.input.isPressed(Control.Shoot, player)) {
				if (charState.canAttack()) {
				
							changeState(new DynamoBoomerangState(), true);
						
					
				}
		}

		if (player.input.isPressed(Control.Special1, player)) {
				if (charState.canAttack()) {
					if (xSaberCooldown == 0) {
						xSaberCooldown = 1;
						
							changeState(new ZSaberProjSwingState(grounded, false), true);
						
					}
				}
		}
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
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

