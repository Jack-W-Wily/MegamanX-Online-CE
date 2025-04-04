namespace MMXOnline;

public class Rock : Character {
	public Rock(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
		charId = CharIds.Rock;
		isVisible = false;
	}

	




	private float AtkCD;


	public override void update(){
		base.update();
		invulnTime = 1;
		Helpers.decrementTime(ref AtkCD);
		bonusHealth = 12;


		if (AtkCD == 0 && grounded
		){


			foreach (var otherPlayer in Global.level.players) {
				if (otherPlayer.character == null) continue;
				if (otherPlayer == player) continue;
				if (otherPlayer == parasiteDamager?.owner) continue;
				if (otherPlayer.character.isInvulnerable()) continue;
				if (Global.level.gameMode.isTeamMode && otherPlayer.alliance != player.alliance) continue;
				if (otherPlayer.character.getCenterPos().distanceTo(getCenterPos()) > ParasiticBomb.carryRange) continue;
				Character target = otherPlayer.character;

				if (pos.x > target.pos.x) {
					if (xDir != -1) {
					xDir = -1;
					}
				} else {
					if (xDir != 1) {
					xDir = 1;
					}
				}
				
				if (hgm == null){
				
		hgm = new HogumerMK2(player, pos.x, pos.y, xDir, false,
		player.getNextATransNetId(), ownedByLocalPlayer);

				}
				break;
			}
	
		AtkCD = 2;
		}


//		if (charState.normalCtrl){
//			changeState(new InfiniteState(), true);
//		}



	}


	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}

	public override string getSprite(string spriteName) {
		return "null_" + spriteName;
	}
}







public class InfiniteState : CharState {

	public InfiniteState(string transitionSprite = "")
		: base("idle", "", "", transitionSprite)
	{
	airMove = true;
	
	}

	public override void update()
	{
	
		character.useGravity = false;
		character.invulnTime = 1;
	


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	 }
}

