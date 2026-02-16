using System;
using System.Collections.Generic;

namespace MMXOnline;

public class GreenDog : Maverick {
	public VelGMeleeWeapon meleeWeapon = new();

	public GreenDog(
		Player player, Point pos, int xDir,
		ushort? netId, bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		player, pos, xDir, netId, ownedByLocalPlayer
	) {
		stateCooldowns = new() {
			{ typeof(MShoot), new(45, true) }
		};
		canClimbWall = true;
		maxHealth = 6;
		awardWeaponId = WeaponIds.Buster;
		weakWeaponId = WeaponIds.ShotgunIce;
		weakMaverickWeaponId = WeaponIds.ChillPenguin;
		dismantleTypeDeath = true;
		shouldDealColisionDmg = true;
		weapon = new Weapon(WeaponIds.VelGGeneric, 101);

		netActorCreateId = NetActorCreateId.GreenDog;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		armorClass = ArmorClass.Light;
		height = 24;
	}



	
	
	public override void creditMaverickKill(Player killer, Player assister, int? weaponIndex) {
		if (killer != null && killer != player) {
			if (Helpers.randomRange(0,5) == 0) {
				new SmallHealthPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			} else if (Helpers.randomRange(0,5) == 1) {
				new LargeHealthPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			} else if (Helpers.randomRange(0,5) == 2) {
				new SmallAmmoPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}  else if (Helpers.randomRange(0,5) == 3) {
				new LargeAmmoPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}    else if (Helpers.randomRange(0,5) == 4) {
				
			}  	else {
			killer.awardCurrency();
			}
		}
	}

	
	public bool healthvalueOnce = false;


	public override void update() {
		base.update();


		if (!healthvalueOnce) {
			healthvalueOnce = true;
			health = 10;
		}
		if (aiBehavior == MaverickAIBehavior.Control) {
			if (state is MIdle or MRun or MLand) {
				if (shootPressed()) {
					changeState(getShootState());
				} else if (specialPressed()) {
					changeState(getShootState2());
				} else if (input.isPressed(Control.Dash, player)) {
					changeState(new VelGPounceStartState());
				}
			} else if (state is MJump || state is MFall) {
				if (input.isPressed(Control.Dash, player)) {
					changeState(new VelGPounceStartState());
				}
			}
		}
	}

	public override string getMaverickPrefix() {
		return "enemy_greendog";
	}

	public override float getRunSpeed() {
		return 135f * getRunDebuffs();
	}

	public MaverickState getShootState() {
		return new MTaunt();
	}

	public MaverickState getShootState2() {
		return new MTaunt();
	}

	public override MaverickState[] strikerStates() {
		return [
			new VelGShootFireState(),
			new VelGShootIceState(),
			new VelGPounceStartState(),
		];
	}

	public override MaverickState[] aiAttackStates() {
		float enemyDist = 300;
		if (target != null) {
			enemyDist = MathF.Abs(target.pos.x - pos.x);
		}
		//if (enemyDist > 50) {
		//	return [new VelGPounceStartState()];
		//}
		return [
			getShootState2(),
			getShootState(),
			new VelGPounceStartState()
		];
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Pounce,
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"enemy_greendog_pounce" => MeleeIds.Pounce,
			_ => MeleeIds.None
		});
	}

	// This can be called from a RPC, so make sure there is no character conditionals here.
	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Pounce => new GenericMeleeProj(
				meleeWeapon, pos, ProjIds.VelGMelee, player,
				3, Global.defFlinch, addToLevel: addToLevel
			),
			_ => null
		};
	}

}
