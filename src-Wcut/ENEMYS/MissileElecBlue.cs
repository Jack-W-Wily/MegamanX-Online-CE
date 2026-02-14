using System;
using System.Collections.Generic;

namespace MMXOnline;

public class MissileElecBlue : Maverick {
	public VelGMeleeWeapon meleeWeapon = new();

	public MissileElecBlue(
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

		netActorCreateId = NetActorCreateId.MissileElecBlue;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		armorClass = ArmorClass.Light;
		height = 24;
	}

	public bool healthvalueOnce = false;


	public override void creditMaverickKill(Player killer, Player assister, int? weaponIndex) {
		if (killer != null && killer != player) {
			killer.addKill();
			killer.awardCurrency();
		}

		if (assister != null && assister != player) {
			assister.addAssist();
			assister.addKill();
			assister.awardCurrency();
			awardXWeapon(killer);
		}

	}
	

	public override void update() {
		base.update();


		if (!healthvalueOnce) {
			healthvalueOnce = true;
			health = 15;
		}


		if (state is MIdle) {
			state.invincible = true;
		}
		if (aiBehavior == MaverickAIBehavior.Control) {

		}
	}

	public override string getMaverickPrefix() {
		return "enemy_missilelec";
	}

	public override float getRunSpeed() {
		return 0f * getRunDebuffs();
	}

	public MaverickState getShootState(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
			new TriadThunderProjCharged(pos, xDir, 3, this, player, player.getNextActorNetId(), rpc: true);
			new TriadThunderProjCharged(pos, -xDir, 3, this, player, player.getNextActorNetId(), rpc: true);
		}, "sparkmSparkX1");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}

	
	public MaverickState getShootState2(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
				new TorpedoProjMech(pos, xDir, this, player, player.getNextActorNetId(), rpc: true);
				
		}, "torpedo");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}


	public override MaverickState[] aiAttackStates() {
		float enemyDist = 300;
		if (target != null) {
			enemyDist = MathF.Abs(target.pos.x - pos.x);
		}
	
		return [
			getShootState(false),
			getShootState2(false),
			getShootState2(true),
		];
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Pounce,
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
