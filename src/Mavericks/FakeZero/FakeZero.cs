using System;
using System.Collections.Generic;
namespace MMXOnline;

public class FakeZero : Maverick {
	public static Weapon getWeapon() { return new Weapon(WeaponIds.FakeZeroGeneric, 150); }
	public float dashDist;
	public float baseSpeed = 50;
	public float accSpeed;
	public int lastDirX;
	public Anim? exhaust;
	public float topSpeed = 200;
	public int shootNum = 0;

	// Ammo uses.
	public static int shootLv2Ammo = 3;
	public static int shootLv3Ammo = 4;


	public bool isClaudio;
	public bool isClaudia;
	public bool isX1Zero;

	// Main creation function.
	public FakeZero(
		Player player, Point pos, Point destPos, int xDir,
		ushort? netId, bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		player, pos, destPos, xDir, netId, ownedByLocalPlayer
	) {
		stateCooldowns = new() {
			{ typeof(FakeZeroMeleeState), new(20) },
			{ typeof(FakeZeroGroundPunchState), new(30) },
			{ typeof(FakeZeroShootState), new(20, true) },
		};

		weapon = getWeapon();
		awardWeaponId = WeaponIds.Buster;
		weakWeaponId = WeaponIds.SpeedBurner;
		weakMaverickWeaponId = WeaponIds.FlameStag;
		canClimbWall = true;
		canClimb = true;

		netActorCreateId = NetActorCreateId.FakeZero;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		exhaust = new Anim(
			pos, "fakezero_exhaust", xDir,
			player.getNextActorNetId(), false, sendRpc: false
		) {
			visible = false
		};

		usesAmmo = true;
		canHealAmmo = true;
		ammo = 28;
		maxAmmo = 28;
		grayAmmoLevel = 3;
		barIndexes = (60, 49);
		gameMavs = GameMavs.X2;
		height = 36;

		spriteFrameToSounds["claudio_jump_start/2"] = "zerosaberx3";
		spriteFrameToSounds["claudio_trippleslash/1"] = "saber1";
		spriteFrameToSounds["claudio_trippleslash/8"] = "saber2";
		spriteFrameToSounds["claudio_trippleslash/17"] = "saber3";
	}

	public override void preUpdate() {
		base.preUpdate();
		if (exhaust != null) {
			if (sprite.name.Contains("dash")) {
				exhaust.zIndex = zIndex - 100;
				exhaust.visible = true;
				exhaust.xDir = xDir;
				exhaust.changePos(getFirstPOIOrDefault());
			} else {
				exhaust.visible = false;
			}
		}
	}

	public override void update() {
		base.update();

		if (!ownedByLocalPlayer) return;

		if (state.normalCtrl || state.attackCtrl || state.aiAttackCtrl ||
			state is FakeZeroMState { canReloadAmmo: true }
		) {
			rechargeAmmo(2);
		}
		if (lastDirX != xDir) {
			accSpeed = 0;
			dashDist = 0;
			if (state is MRun mrun) {
				mrun.once = false;
				changeSpriteFromName("run", true);
				frameIndex = 1;
			}
		}
		lastDirX = xDir;

		if (state is MRun || state is FakeZeroMeleeState) {
			dashDist += accSpeed * Global.spf;
			accSpeed += Global.spf * 150;
			if (accSpeed > topSpeed) {
				accSpeed = topSpeed;
			}
		} else if (grounded && state is not MLand and not MJumpStart || state is MHurt) {
			accSpeed = 0;
		}
	}

	public override bool attackCtrl() {
		isClaudio = sprite.name.StartsWith("claudio");
        isClaudia = sprite.name.StartsWith("fakezero");
        
		if (isClaudia){
		if (input.isHeld(Control.Shoot, player) && state is MRun) {
			changeState(new FakeZeroMeleeState());
			return true;
		}
		if (input.isHeld(Control.Shoot, player)) {
			changeState(new FakeZeroShootState(), false);
			return true;
		}
		if (input.isPressed(Control.Special1, player) && ammo >= shootLv3Ammo) {
			changeState(getBusterComboState());
			return true;
		}
		if (input.isPressed(Control.Dash, player)) {
			changeState(new FakeZeroGroundPunchState());
			return true;
		}
		if (grounded) {
			bool holdGuard;
			if (useChargeJump) {
				holdGuard = input.isHeld(Control.Down, player);
			} else {
				holdGuard = input.isHeld(Control.Up, player);
			}
			if (holdGuard &&state is not FakeZeroGuardState) {
				changeState(new FakeZeroGuardState());
				return true;
			}
		}
		}

		if (isClaudio){
		if (input.isHeld(Control.Shoot, player) && state is MRun) {
			changeState(new FakeZeroMeleeState());
			return true;
		}
		if (input.isHeld(Control.Shoot, player)) {
			changeState(new ClaudioTrppleSlashMaverick(), false);
			return true;
		}
		if (input.isPressed(Control.Special1, player) && ammo >= shootLv3Ammo) {
			changeState(new ClaudioTrippleBusterMaverick(), false);
			ammo -= shootLv3Ammo;
			return true;
		}
		if (input.isR2Pressed(player) && ammo >= shootLv3Ammo) {
			changeState(new ClaudioChargedSlashMaverick(), false);
			ammo -= shootLv3Ammo;
			return true;
		}
		if (input.isPressed(Control.Dash, player)) {
			changeState(new FakeZeroGroundPunchState());
			return true;
		}
		if (grounded) {
			bool holdGuard;
			if (useChargeJump) {
				holdGuard = input.isHeld(Control.Down, player);
			} else {
				holdGuard = input.isHeld(Control.Up, player);
			}
			if (holdGuard &&state is not FakeZeroGuardState) {
				changeState(new FakeZeroGuardState());
				return true;
			}
		}
		}
		return false;
	}

	public MaverickState getBusterComboState() {
		return shootNum switch {
			1 => new FakeZeroC2State(),
			2 => new FakeZeroC3State(),
			_ => new FakeZeroC1State()
		};
	}

	public override float getRunSpeed() {
		float retSpeed = baseSpeed + accSpeed;
		if (retSpeed > Physics.WalkSpeedSec) {
			return retSpeed * getRunDebuffs();
		}
		return Physics.WalkSpeedSec * getRunDebuffs();
	}

	public override string getMaverickPrefix() {
		if (player.isX) return "zerox1";
		if (Options.main.iLikeMagicalGirls)return "fakezero";
		return "claudio";
	}

	public override MaverickState[] strikerStates() {
		return [
			new FakeZeroShootState(2),
			new FakeZeroC1State(),
			new FakeZeroGroundPunchState(),
		];
	}

	public override MaverickState[] aiAttackStates() {
		List<MaverickState> aiStates = [
			new FakeZeroShootState(2)
		];
		float enemyDist = 300;

		if (target != null) {
			enemyDist = MathF.Abs(target.pos.x - pos.x);
		}
		
			aiStates.Add(new FakeZeroC2State());
			aiStates.Add(new FakeZeroC1State());

	
		if (enemyDist <= 70) {
			aiStates.Add(new FakeZeroGroundPunchState());
		} else {
			aiStates.Add(new FakeZeroMeleeState(true));
		}
		return aiStates.ToArray();
	}

	public override void aiUpdate() {
		base.aiUpdate();
		if ((controlMode == MaverickModeId.Summoner || player.isX)&&
			Helpers.randomRange(0, 2) == 1 && ammo >= 8 && state.aiAttackCtrl
		) {
			foreach (GameObject gameObject in getCloseActors(64, true, false, false)) {
				if (gameObject is Projectile proj &&
					proj.damager.owner.alliance != player.alliance &&
					!proj.isMelee
				) {
					changeState(new FakeZeroGuardState());
				}
			}
		}
	}

	public MaverickState getShootState(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
			new FakeZeroBusterProj(
				pos, xDir, this, player.getNextActorNetId(), sendRpc: true
			);
		}, "busterX2");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}

	public override void onDestroy() {
		base.onDestroy();
		exhaust?.destroySelf();
	}



		// for the melee hitbox to work
	// This can run on both owners and non-owners. So data used must be in sync.
	public enum MeleeIds {
		None = -1,

		Blocking, // you add more and more and finish with "," always for each move you add

		Grab,

		TrippleSlash,
		Rising,
		FireWave,

		DashSlash, 
		TrippleBusterSlash,

		
	}


	// these are where the sprites referenced with each melee
	// IDs are located
	public override int getHitboxMeleeId(Collider hitbox) {
		return (int)(sprite.name switch {
			"kr_block"  /*referenced sprite*/ => MeleeIds.Blocking, /*melee ID related to said sprite*/
			"claudio_chargeslash"  => MeleeIds.DashSlash,
			"claudio_trippleslash" => MeleeIds.TrippleSlash,
			"claudio_shoot2" => MeleeIds.TrippleBusterSlash,
			"claudio_dash" => MeleeIds.Rising,
			"claudio_jump"  or  "claudio_rising" => MeleeIds.Rising,
			

			_ => MeleeIds.None
		});
	}

	// this is where you effectively make the melee hitboxes trigger
	public override Projectile? getMeleeProjById(int id, Point projPos, bool addToLevel = true) {
		Projectile? proj = id switch {
			(int)MeleeIds.Blocking => new GenericMeleeProj(
				new KRMelee(), // referenced weapon to make it compatible with the
							   // Weakness system and also the killfeed
				projPos, // to make sure it's where the hitbox is placed
				ProjIds.BlockingProjID, // this is the projectile ID referenced to it
				/*
				NOTE: make sure you add every projectile ID to the "Enums.cs"'s "ProjIDs" section
				or else it won't work
				*/
				player, // means the player owns it
				damage: 0.0f, // how much dmg
				flinch: 0, // how many frames will the person be flinched or not at all
				hitCooldown: 0, // how many frames until that hitbox can be effective again
								// Ideally you shorten this if you want it to multihit
				isShield: false,// non piercing projectiles are destroyed on contact and can clang things 
								// with the "isZSaberClang" propety On
				isReflectShield: false, // Projectiles are sent the opposite way when in contact and can clang
				isDeflectShield: true,// projectiles are sent up in the air when in contact and can clang stuff
				isZSaberClang: false,// this propety makes it so your move clangs in contact shield type hitboxes
				isZSaberEffect: false,// adds the Zsaber slashing effect
				addToLevel: addToLevel // make sure this is always active like this or your projectile won't work
			),


			(int)MeleeIds.TrippleSlash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.X6Saber, player,
				 3,30,10, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Rising => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.X6Saber, player,
				 2,30,10, isReflectShield: false,
				isZSaberClang: true, isZSaberEffect: false,
				isJuggleProjectile:  true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.TrippleBusterSlash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.X6Saber, player,
				 5,20,10, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.DashSlash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.HeavyPush, player,
				 5,0,10, isReflectShield: false,
				isZSaberClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			_ => null
		};
		return proj;
	}



}
