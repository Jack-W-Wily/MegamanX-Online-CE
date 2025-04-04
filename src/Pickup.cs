﻿namespace MMXOnline;

public enum PickupType {
	Health,
	Ammo
}

public enum PickupTypeRpc {
	LargeHealth,
	SmallHealth,
	LargeAmmo,
	SmallAmmo
}

public class Pickup : Actor {
	public float healAmount = 0;
	public PickupType pickupType;
	public Pickup(Player owner, Point pos, string sprite, ushort? netId, bool ownedByLocalPlayer, NetActorCreateId netActorCreateId, bool sendRpc = false) :
		base(sprite, pos, netId, ownedByLocalPlayer, false) {
		netOwner = owner;
		collider.wallOnly = true;
		collider.isTrigger = false;

		this.netActorCreateId = netActorCreateId;
		if (sendRpc) {
			createActorRpc(owner.id);
		}
	}

	public override void update() {
		base.update();
		var leeway = 500;
		if (ownedByLocalPlayer && pos.x > Global.level.width + leeway || pos.x < -leeway || pos.y > Global.level.height + leeway || pos.y < -leeway) {
			destroySelf();
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (other.otherCollider.flag == (int)HitboxFlag.Hitbox) return;

		if (other.gameObject is Character chr) {
			if (!chr.ownedByLocalPlayer) return;

			if (pickupType == PickupType.Health) {
				if (chr.health >= chr.maxHealth && !chr.player.hasSubtankCapacity()) return;
				chr.addHealth(healAmount);
				destroySelf(doRpcEvenIfNotOwned: true);
			} else if (pickupType == PickupType.Ammo) {
					// this is the test code for spawning Enemies but it doesnt
					// Work Online sadly 
		/*		if (Global.level.isRace() && chr is not HogumerMK2){
				chr.SpawnEnemy();
				destroySelf(doRpcEvenIfNotOwned: true);
				} 	else  {*/ 
				if (chr.canAddAmmo()) {
					chr.player.superAmmo += 10;
					chr.addPercentAmmo(healAmount);
					destroySelf(doRpcEvenIfNotOwned: true);
			//	}
				}
			}
		} else if (other.gameObject is RideArmor rideArmor) {
			if (!rideArmor.ownedByLocalPlayer) return;

			if (rideArmor.character != null) {
				if (pickupType == PickupType.Health) {
					if (rideArmor.health >= rideArmor.maxHealth) {
						if (rideArmor.character != null && (
							rideArmor.character.health >= rideArmor.character.maxHealth
						)) {
							return;
						} else {
							rideArmor.character?.addHealth(healAmount);
						}
					} else {
						rideArmor.addHealth(healAmount);
					}
					destroySelf(doRpcEvenIfNotOwned: true);
				} else if (pickupType == PickupType.Ammo) {
					//rideArmor.character.addAmmo(this.healAmount);
					//this.destroySelf();
				}
			}
		} else if (other.gameObject is RideChaser rideChaser) {
			if (!rideChaser.ownedByLocalPlayer) return;

			if (rideChaser.character != null) {
				if (pickupType == PickupType.Health) {
					if (rideChaser.health >= rideChaser.maxHealth && !Global.level.isRace()) {
						if (rideChaser.character != null &&
							rideChaser.character.health >= rideChaser.character.maxHealth
						) {
							return;
						} else {
							rideChaser.character?.addHealth(healAmount);
						}
					} else {
						rideChaser.addHealth(healAmount);
					}
					destroySelf(doRpcEvenIfNotOwned: true);
				}
				if (pickupType == PickupType.Ammo) {
					rideChaser.ItemNum = Helpers.randomRange(1,6);
					destroySelf(doRpcEvenIfNotOwned: true);
				}
			}
			
		} else if (other.gameObject is Maverick maverick && maverick.ownedByLocalPlayer) {
			if (pickupType == PickupType.Health &&
				(maverick.health < maverick.maxHealth || maverick.netOwner.hasSubtankCapacity())
			) {
				maverick.addHealth(healAmount, true);
				destroySelf(doRpcEvenIfNotOwned: true);
			} else if (pickupType == PickupType.Ammo && maverick.ammo < maverick.maxAmmo) {
				maverick.addAmmo(healAmount);
				destroySelf(doRpcEvenIfNotOwned: true);
			}
		}
	}
}

public class LargeHealthPickup : Pickup {

	public float timer;

	public NeutralEnemy enemy;

	public LargeHealthPickup(
		Player owner, Point pos, ushort? netId,
		bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		owner, pos, "pickup_health_large", netId, ownedByLocalPlayer,
		NetActorCreateId.LargeHealth, sendRpc: sendRpc
	) {
		healAmount = 8;
		pickupType = PickupType.Health;



		//		if (enemy == null){
	//	enemy = new NeutralEnemy(
	//			 pos, 0, true, 150, true);
	//	}



	}	


	
}

public class SmallHealthPickup : Pickup {

	public NeutralEnemy enemy;


	public SmallHealthPickup(
		Player owner, Point pos, ushort? netId,
		bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		owner, pos, "pickup_health_small", netId, ownedByLocalPlayer,
		NetActorCreateId.SmallHealth, sendRpc: sendRpc
	) {
		healAmount = 4;
		pickupType = PickupType.Health;

			//		if (enemy == null){
	//	enemy = new NeutralEnemy(
	//			 pos, 0, true, 150, true);
	//	}

	}
}

public class LargeAmmoPickup : Pickup {

		public NeutralEnemy enemy;
	public LargeAmmoPickup(
		Player owner, Point pos, ushort? netId,
		bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		owner, pos, "pickup_ammo_large", netId, ownedByLocalPlayer,
		NetActorCreateId.LargeAmmo, sendRpc: sendRpc
	) {
		healAmount = 50;
		pickupType = PickupType.Ammo;

		//		if (enemy == null){
	//	enemy = new NeutralEnemy(
	//			 pos, 0, true, 150, true);
	//	}

	}
}

public class SmallAmmoPickup : Pickup {

		public NeutralEnemy enemy;
	public SmallAmmoPickup(
		Player owner, Point pos, ushort? netId,
		bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		owner, pos, "pickup_ammo_small", netId, ownedByLocalPlayer,
		NetActorCreateId.SmallAmmo, sendRpc: sendRpc
	) {
		healAmount = 25;
		pickupType = PickupType.Ammo;



	//		if (enemy == null){
	//	enemy = new NeutralEnemy(
	//			 pos, 0, true, 150, true);
	//	}


	}
}
