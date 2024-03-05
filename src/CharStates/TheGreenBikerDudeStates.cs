namespace MMXOnline;


public class GBDKick : Weapon {
	public GBDKick() : base() {
		rateOfFire = 0.75f;
		index = (int)WeaponIds.GBDKick;
		killFeedIndex = 92;
	}
}


public class EnfrenteMeuDisco : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	public EnfrenteMeuDisco(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
	{

	}

	public override void update()
	{
	

		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	
		
		new SpinningBlade().getProjectile(character.getShootPos(), character.getShootXDir(), player, 0, player.getNextActorNetId());
			
		
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class EnfrenteMeuDisco2 : CharState {

	private bool shot;
	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	public EnfrenteMeuDisco2(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
	{
	
	}

	public override void update()
	{
	
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		character.playSound("spinningBlade", forcePlay: false, sendRpc: true);
		new WheelGSpinWheelProj(player.weapon, character.pos.addxy(character.xDir * -4, -20), character.xDir, player, player.getNextActorNetId(allowNonMainPlayer: true),rpc: true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



