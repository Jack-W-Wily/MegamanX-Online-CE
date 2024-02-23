using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public class VileMK2Grab : Weapon {
	public VileMK2Grab() : base() {
		rateOfFire = 0.75f;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}

public class VileMK2GrabState : CharState {
	public Character victim;
	float leechTime = 1;

	float hitcd = 1;

	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public VileMK2GrabState(Character victim) : base("grab", "", "", "") {
		this.victim = victim;
		grabTime = VileMK2Grabbed.maxGrabTime;
	}

	bool SpawnShockwave;

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		hitcd += Global.spf;
		if (victimWasGrabbedSpriteOnce && victim == null) {
			character.changeState(new Idle(), true);
			return;
		}

		if (victim.sprite.name.EndsWith("_grabbed") || victim.sprite.name.EndsWith("_die")) {
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
		var damager = new Damager(player, 0.25f, 0, 0);
		if (leechTime > 0.5f) {
			leechTime = 0;
			character.addHealth(1);
			
		}
		if (character.sprite.name.Contains("raging") && character.frameIndex > 5 && hitcd > 0.2f){
			hitcd =0;
			character.playSound("armoredaCrash", sendRpc: true);
			character.shakeCamera(sendRpc: true);
		damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);	
		}
		if (player.input.isPressed(Control.Dash, player)) {
			character.changeSpriteFromName("ragingdemon_grab", true);
			sprite = "ragingdemon_grab";
			sprite = "knocked_down";
			SpawnShockwave = true;
			victim.changeSpriteFromName("knocked_down", true);
		}
		if (player.input.isPressed(Control.Special1, player)) {
			character.changeState(new Idle(), true);
			return;
		}

		if (grabTime <= 0) {
			character.changeState(new Idle(), true);
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		var damager = new Damager(player, 1f, 0, 0);
		character.addHealth(1);		
		damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (SpawnShockwave){
		new MechFrogStompShockwave(new MechFrogStompWeapon(player), 
		character.pos.addxy(6 * character.xDir, 0), character.xDir, 
		player, player.getNextActorNetId(), rpc: true);
		character.playSound("crash", sendRpc: true);
		}
		character.grabCooldown = 1;
		victim.grabInvulnTime = 2;
		victim?.releaseGrab(character);
	}
}

public class VileMK2Grabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;

	

	public VileMK2Grabbed(Character grabber) : base(grabber, maxGrabTime, "_grab") {
	}


	public override void update() {
		base.update();
		trySnapToGrabPoint(true);
	}
}
