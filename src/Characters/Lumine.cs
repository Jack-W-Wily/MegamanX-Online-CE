namespace MMXOnline;

public class Lumine : Character {
	public Lumine(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {

	}


	public float shootcd = 0;


	public override void update() {
		base.update();
		Helpers.decrementTime(ref shootcd);
		if (player.input.isHeld(Control.Shoot, player) && shootcd <= 0){
		shootcd = 0.2f;
		playSound("buster", sendRpc : true);
		new BusterProj(new Buster(), getShootPos(), xDir, 0, player, player.getNextActorNetId());
		}
	}




	public override bool canDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "pallete_" + spriteName;
	}
}

