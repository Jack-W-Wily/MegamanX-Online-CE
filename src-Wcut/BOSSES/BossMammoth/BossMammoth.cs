
// How to add a character?
// I highly Reccomend you Copy These two "kuromitos File's That you downloaded in the win rar
// And rename and edit for every single one of them


// I hightly advise you to put all of these in every new .CS file you add
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
// To make sure most things work without you having errors when you want to use Math for example

namespace MMXOnline;


public class BossMammoth : Character {

	

	
	public float ShikiYamiBaraiCD;
	public float stockedTime;

	public bool canSpecialCancel = false;

	
	public Weapon uppercutWeapon;
	public Sprite antler;
	public Sprite antlerDown;
	public Sprite antlerSide;
	public BossMammoth(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
		) : base( // Make sure it looks exactly like this
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn) {


		charId = CharIds.BossClaudio;
		ShouldExplode = true;


		if (charState is WarpIn) player.superAmmo = 0;
		uppercutWeapon = new Weapon(WeaponIds.FStagGeneric, 144, new Damager(player, 0, 0, 0));
	
		spriteFrameToSounds["flamem_warp_in/2"] = "flamemOilBurn";
		spriteFrameToSounds["fstag_run/6"] = "run";
		isWCUTBoss = true;
	}



	// NormalCTRL: is for you to add moves that your new Character that he can do while
	// He isn't Softlocked in a motion be it an attack or a Damage State
	public override bool normalCtrl() {

		if (player.input.isL2Held(player) && grounded){
			changeState(new BlockWCUT(), true);
		
		}
		

		return base.normalCtrl();
	}

	public override CharState getJumpState() => new BossJumpStart();
	
	// AttackCtrl: is for you to add moves to your character that he can only perform
	// While the attackCtrl flag is active in a charstate and is conventionally where you add attacks
	public override bool attackCtrl() {

	
		

		return base.attackCtrl();
	}


	public override Collider getBlockCollider() {
		Rect rect = Rect.createFromWH(0, 0, 18, 40);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}
	

	public bool phase1Theme;

	public bool phase2Theme;
	public bool phase3Theme;

	public override void update() {
		base.update();

		if (!isWarpIn() && charState is not WarpIdle  ){
		if (bonusHealth > 0) {
				
			} else {
				if (health > 10 && !phase1Theme){
					phase1Theme = true;
				addMusicSource("Xvs8Generals_BossX1", getCenterPos(), true);
				} 
				if (health < 10 && !phase2Theme){
					phase2Theme = true;
					iframesTime = 600;
				addMusicSource("Xvs8Generals_RAGEMODE", getCenterPos(), true);
				}
		}
		}
	
		Helpers.decrementTime(ref overDriveTimer);
		Helpers.decrementTime(ref ShikiYamiBaraiCD);
		

		// Charge and release charge logic.
		chargeLogic(shoot);



		if (OverDrive) {
			stockedTime += Global.spf;
			if (stockedTime >= 61f / 60f) {
				stockedTime = 0;
				playSound("stockedSaber");
			}
		}







		if (player.superAmmo >= player.superMaxAmmo) {
			weaponHealAmount = 0;
		}
		if (weaponHealAmount > 0 && player.health > 0) {
			weaponHealTime += Global.spf;
			if (weaponHealTime > 0.05) {
				weaponHealTime = 0;
				weaponHealAmount--;
				player.superAmmo = Helpers.clampMax(player.superAmmo + 1, player.superMaxAmmo);
				playSound("healX3", forcePlay: true, true);
				
			}
		}
		
	}


	

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "flamem_" + spriteName;

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
			"flamem_dash" or "flamem_antiair" => MeleeIds.DashSlash,
			"flamem_fall" => MeleeIds.TrippleSlash,
			"flamem_inferno_release" => MeleeIds.TrippleBusterSlash,
			"flamem_jump" => MeleeIds.Rising,
			"flamem_grab" => MeleeIds.Grab,
			"fstag_orochinagi_start"  or  "fstag_wall_dash" => MeleeIds.Rising,
			

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
								// with the "ShouldClang" propety On
				isReflectShield: false, // Projectiles are sent the opposite way when in contact and can clang
				isDeflectShield: true,// projectiles are sent up in the air when in contact and can clang stuff
				ShouldClang: false,// this propety makes it so your move clangs in contact shield type hitboxes
				isZSaberEffect: false,// adds the Zsaber slashing effect
				addToLevel: addToLevel // make sure this is always active like this or your projectile won't work
			),

			(int)MeleeIds.Grab => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.ForceGrabState, player,
				 3,0,10, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.TrippleSlash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.X6Saber, player,
				 3,30,10, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			(int)MeleeIds.Rising => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.X6Saber, player,
				 2,30,10, isReflectShield: false,
				clashTier: ClashTier.Weak, isZSaberEffect: false,
				isJuggleProjectile:  true,
				addToLevel: addToLevel
			),
			(int)MeleeIds.TrippleBusterSlash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.X6Saber, player,
				 5,80,10, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),

			(int)MeleeIds.DashSlash => new GenericMeleeProj(
				new KRMelee(), projPos, ProjIds.HeavyPush, player,
				 5,0,10, isReflectShield: false,
				ShouldClang: false, isZSaberEffect: false,
				addToLevel: addToLevel
			),
			_ => null
		};
		return proj;
	}



	// Ammo section
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return (player.superAmmo < player.superMaxAmmo);
	}



	// Name is self explanatory but this is for your character to be able to use 
	// The charge mechanic from the Megaman Games Just like megaman Games
	public override bool canCharge() {
		return !isInvulnerableAttack();
	}

	public override void chargeGfx() {
		if (ownedByLocalPlayer) {
			chargeEffect.stop();
		}
		if (isCharging()) {
			chargeSound.play();
			int chargeType = 0;
			chargeEffect.update(getChargeLevel(), chargeType);
		}
	}

	public override int getMaxChargeLevel() {
		return 4;
	}




	public override bool chargeButtonHeld() {
		return player.input.isR2Held(player);
	}

	public override void increaseCharge() {
		float factor = 1;
		if (OverDrive) factor = 1.5f; // this means during OverDrive he gets a chargespeed buff
		chargeTime += Global.speedMul * factor;
	}

	public override float getRunSpeed() {
		float runSpeed = 130;
		
		return runSpeed * getRunDebuffs();
	}




	// Shoots stuff.
	public void shoot(int chargeLevel) {


		if (chargeLevel >= 1) {
			stopCharge();
		}
	}




	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;


		
		if (player.skinSlot == 1) {
			palette = player.nightmareZeroShader;
		}
		
		if (palette != null) {
			shaders.Add(palette);
		}
		if (shaders.Count == 0) {
			return baseShaders;
		}
		shaders.AddRange(baseShaders);
		return shaders;
	}



	public override List<byte> getCustomActorNetData() {
		List<byte> customData = base.getCustomActorNetData();
		customData.Add(Helpers.boolArrayToByte([
			OverDrive,
		]));
		return customData;
	}
	public override void updateCustomActorNetData(byte[] data) {
		// Update base arguments.
		base.updateCustomActorNetData(data);
		data = data[data[0]..];
		bool[] flags = Helpers.byteToBoolArray(data[0]);
		OverDrive = flags[0];
	}


		public override float getJumpPower() {
		float jumpModifier = 3;
	
		return jumpModifier + base.getJumpPower();
	}



		
	(float twitch, float grow, int time) omegaAura = new(0.015f, 0, 0);

	void updateOmegaAura() {
		omegaAura.twitch -= 0.05f;
		if (omegaAura.twitch < 0.05)
			omegaAura.twitch = 0.15f;

		if (omegaAura.time >= 0 && omegaAura.time < 50)
			omegaAura.grow += 0.0025f;
		else if (omegaAura.time >= 55 && omegaAura.time < 105)
			omegaAura.grow -= 0.0025f;

		omegaAura.time++;
		if (omegaAura.time > 110) {
			omegaAura.time = 0;
		}
	}

	public override void render(float x, float y) {
		addRenderEffect(RenderEffectType.SpeedDevilTrail);
		// For drawing the growing aura that LastStand and Eigengrau Zero uses.
		


	
		base.render(x, y);
	}

	public float AIHellBarrageCD;

	public bool AIStart;

	public bool isBoss;


	public override void landingCode(bool useSound = true) {
		base.landingCode(useSound);
		shakeCamera(sendRpc: true);
		playSound("crash", sendRpc: true);
		new FlameMStompShockwave(
			pos, xDir,
			this, player, player.getNextActorNetId(), rpc: true);
			shakeCamera(sendRpc: true);

	}

	public override void aiAttack(Actor? target) {
		int Vattack = Helpers.randomRange(1, 7);
		Helpers.decrementFrames(ref AIHellBarrageCD);
		bool isTargetInAir = pos.y > target?.pos.y - 20;
		bool isTargetClose = target?.getCenterPos().distanceTo(getCenterPos()) < 50;
		bool isWishinRangedMoves = target?.getCenterPos().distanceTo(getCenterPos()) < 120;
		bool isFacingTarget = (pos.x < target?.pos.x && xDir == 1) || (pos.x >= target?.pos.x && xDir == -1);
		if (Global.level.is1v1()) {
			isBoss = true;
		}
		if (isBoss) {
			player.superAmmo = player.superMaxAmmo;
		}


		if (!charState.isGrabbedState && !player.isDead && !isInvulnerableAttack()
				&& aiAttackCooldown <= 0 && charState.attackCtrl) {

			if (grounded) {
				switch (Vattack) {
					case 1 when isFacingTarget:
						changeState(new BFlameMAntiAir());
						break;
					case 2 when isFacingTarget:
						changeState(new BFlameMGrabStart());
						break;
					case 3 when isFacingTarget:
						changeState(new BFlameMShootState());
						break;
					case 4 when isFacingTarget:
						changeState(new BFlameMOilState());
						break;
					case 5 when isFacingTarget:
						changeState(new BFlameMGrabStart());
						slideVel = getDashSpeed() * 2;
						break;
					case 6 when isFacingTarget:
						changeState(new BFlameMGrabStart());
						slideVel = getDashSpeed();
						break;
					case 7 when isFacingTarget && bonusHealth == 0:
						changeState(new BFlameMInfernoCharge());
						
						break;
				}
			}



			if (!grounded && isTargetClose) {
				switch (Vattack) {
					case 1 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
					case 2 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
					case 3 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
					case 4 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
					case 5 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
					case 6 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
					case 7 when isFacingTarget:
						changeState(new BFlameMJumpPressState());
						break;
				}
			}
			if (bonusHealth > 0) {
				aiAttackCooldown = Helpers.randomRange(60, 120);
			} else {
				if (health > 10){
				aiAttackCooldown = Helpers.randomRange(20, 60);
				} 
				if (health < 10){
				aiAttackCooldown = Helpers.randomRange(0, 30);
				}
			}
		}



		base.aiAttack(target);
	}

	public float aiBlocktime;

	public float aiDodgeCD;
	public override void aiDodge(Actor? target) {
		Helpers.decrementFrames(ref aiBlocktime);
		Helpers.decrementFrames(ref aiDodgeCD);
		foreach (GameObject gameObject in getCloseActors(64, true, false, false)) {
			if (gameObject is Projectile proj && proj.damager.owner.alliance != player.alliance &&
			(charState.attackCtrl || charState is ShoulderCannon or PopcornHell)) {
				//Projectile is not 
				if (!(proj.projId == (int)ProjIds.RollingShieldCharged || proj.projId == (int)ProjIds.RollingShield
					|| proj.projId == (int)ProjIds.MagnetMine || proj.projId == (int)ProjIds.FrostShield || proj.projId == (int)ProjIds.FrostShieldCharged
					|| proj.projId == (int)ProjIds.FrostShieldAir || proj.projId == (int)ProjIds.FrostShieldChargedPlatform || proj.projId == (int)ProjIds.FrostShieldPlatform)
				) {
					if (grounded) {
						if (aiDodgeCD == 0 && !isDashing) {
							changeState(new BossGuard());	
								aiDodgeCD = Helpers.randomRange(100, 220);
							
						}
					} 
				}
			}
		}
	
		base.aiDodge(target);
	}
	
}
