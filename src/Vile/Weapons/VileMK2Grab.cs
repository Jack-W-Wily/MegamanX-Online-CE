using System;

namespace MMXOnline;








public class VileClassic : VileLaser {
		public static VileClassic netWeapon = new();
	public VileClassic() : base() {
		type = 0;
		fireRate = 45;
		displayName = "Classic";
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}


public class VileMk1 : VileLaser {
		public static VileMk1 netWeapon = new();
	public VileMk1() : base() {
		type = 1;
		fireRate = 45;
		displayName = "VAVA 1";
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}



public class VileMk2 : VileLaser {
		public static VileMk2 netWeapon = new();
	public VileMk2() : base() {
		type = 2;
		fireRate = 45;
		displayName = "VAVA 2";
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}



public class VileMkV : Weapon {
		public static VileMkV netWeapon = new();
	public VileMkV() : base() {
		type = 3;
		fireRate = 45;
		displayName = "VAVA V";
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}




public enum VileGrabType {
	None = -1,
	ViolentCrusher,
	SpringSnatcher,
	SpeedyViper,
}
public class VileGrab : Weapon {
	public float vileAmmoUsage;
	public VileGrab() : base() {
		index = (int)WeaponIds.VileLaser;
	}
}


public class ViolentCrusher : VileGrab {
	public static ViolentCrusher netWeapon = new();
	public ViolentCrusher() : base() {
		index = (int)WeaponIds.VileMK2Grab;
		type = (int)VileGrabType.ViolentCrusher;
		displayName = "Beat Down";
		vileAmmoUsage = 0;
		killFeedIndex = 189;
		vileWeight = 3;
		ammousage = vileAmmoUsage;
		damage = "6";
		hitcooldown = "0.5";
		flinch = "26";
		effect = "Insane Hitbox.";
	}
	public override void vileShoot(Vile vile) {
		if (vile.energy.ammo < vileAmmoUsage) return;
		vile.changeState(new VAVAKamae(), true);
	}
	
	public override void shoot(Character character, int[] args) {
		if (character is not Vile vile) return;
		Point shootPos = vile.setCannonAim(new Point(1.5f, -1));
		
	}
}




public class SpringSnatcher : VileGrab {
	public static SpringSnatcher netWeapon = new();
	public SpringSnatcher() : base() {
		index = (int)WeaponIds.VileMK2Grab;
		type = (int)VileGrabType.SpringSnatcher;
		displayName = "Missile Loader";
		vileAmmoUsage = 0;
		killFeedIndex = 189;
		vileWeight = 3;
		ammousage = vileAmmoUsage;
		damage = "6";
		hitcooldown = "0.5";
		flinch = "26";
		effect = "Insane Hitbox.";
	}
	public override void vileShoot(Vile vile) {
		if (vile.energy.ammo < vileAmmoUsage) return;
		vile.changeState(new VMissiLeStance(), true);
	}
	
	public override void shoot(Character character, int[] args) {
		if (character is not Vile vile) return;
		Point shootPos = vile.setCannonAim(new Point(1.5f, -1));
		
	}
}





public class SpeedyViper : VileGrab {
	public static SpeedyViper netWeapon = new();
	public SpeedyViper() : base() {
		index = (int)WeaponIds.VileMK2Grab;
		type = (int)VileGrabType.SpeedyViper;
		displayName = "Parry";
		vileAmmoUsage = 0;
		killFeedIndex = 190;
		vileWeight = 3;
		ammousage = vileAmmoUsage;
		damage = "6";
		hitcooldown = "0.5";
		flinch = "26";
		effect = "Insane Hitbox.";
	}
	public override void vileShoot(Vile vile) {
		if (vile.energy.ammo < vileAmmoUsage) return;
		vile.changeState(new GlobalParryState(), true);
	}
	
	public override void shoot(Character character, int[] args) {
		if (character is not Vile vile) return;
		Point shootPos = vile.setCannonAim(new Point(1.5f, -1));
		
	}
}



public class VileMK2Grab : Weapon {
	public VileMK2Grab() : base() {
		fireRate = 45;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}




public class VileMK2GrabState : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;

	public VileMK2GrabState(Character? victim) : base("grab") {
		this.victim = victim;
		grabTime = VileMK2Grabbed.maxGrabTime;
		attackCtrl = true;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		if (victimWasGrabbedSpriteOnce && !victim?.sprite.name.EndsWith("_grabbed") == true) {
			character.changeToIdleOrFall();
			return;
		}

		if (victim?.sprite.name.EndsWith("_grabbed") == true|| victim?.sprite.name.EndsWith("_die") == true) {
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.5f) {
					leechTime = 0;
					character.addHealth(1);
				}
				return;
			}
		}

		if (leechTime > 0.5f) {
			leechTime = 0;
			character.addHealth(1);
			var damager = new Damager(player, 1, 0, 30);
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);
		}

		if (stateFrames >= 2 && player.input.isBPressed(player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		if (newState is not VileMK2GrabState && victim != null) {
			victim.grabInvulnTime = 2;
			victim.stunInvulnTime = 1;
			victim?.releaseGrab(character, true);
		}
		if (character != null && character is Vile vile){
			vile.mk2GrabCooldown = 1.5f;
		}
	}
}

public class VileMK2Grabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;
	public VileMK2Grabbed(Character? grabber) : base(grabber, maxGrabTime, "grab") {
	}
}
