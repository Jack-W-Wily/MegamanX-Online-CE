namespace MMXOnline;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using SFML.Graphics;
using SFML.Window;


public class Dynamo : Character {
	public Dynamo(
	Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, heartTanks, isATrans
	) {
		charId = CharIds.Dynamo;
		spriteFrameToSounds["dynamo_run/4"] = "run2";
		spriteFrameToSounds["dynamo_run/9"] = "run2";
	}

	private float ItemThrowCooldown;

	private float CrouchTime;

	private float SlashCooldown;

	public int backFlipCount = 0;

	public int uppercutCount = 0;

	public int airShotCount = 0;

	public int DaggerCount = 0;

	private float AxeCoolDown;

	private float DaggerCooldown;




	public override bool normalCtrl() {

		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT(), true);
			
		}
		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);
		}
		if (player.input.isAPressed(player) &&
		player.input.isL2Held(player)
		) {
			changeState(new DynamoGPChargeState(), true);
		}


		if (player.input.isPressed(Control.Jump, player) && !grounded
		&& charState is not Jump && backFlipCount == 0) {
			changeState(new DynamoBackFlip());
			backFlipCount = 1;
		}

		bool hadokenCheck = false;
		bool shoryukenCheck = false;
		hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);

		if (hadokenCheck && SlashCooldown == 0 && upPressedTimes > 0) {
			changeState(new DynamoBladeDash(), true);
			SlashCooldown = 0.5f;
			upPressedTimes = 0;
		}

		return base.normalCtrl();
	}


	public override bool attackCtrl() {



		if (player.input.isPressed(Control.Shoot, player) &&
		!player.input.isHeld(Control.Down, player)) {
			changeState(new DynamoWhippAttack(), true);
		}
		if (player.input.isPressed(Control.Shoot, player) && grounded &&
		player.input.isHeld(Control.Down, player)) {
			changeState(new DynamoBladeSlash(), true);
		}
		if (player.input.isPressed(Control.Shoot, player) && !grounded &&
		player.input.isHeld(Control.Down, player)) {
			changeState(new DynamoBladeSlashAir(), true);
		}


		if (player.input.isPressed(Control.Shoot, player) && downPressedTimes > 0 &&
		uppercutCount == 0 &&
		player.input.isHeld(Control.Up, player)) {
			changeState(new DynamoUpperCut(), true);
			uppercutCount = 1;
		}


		if ((player.input.isHeld(Control.Left, player)
		|| player.input.isHeld(Control.Right, player)) &&
			player.input.isPressed(Control.Special1, player) &&
		!player.input.isHeld(Control.Down, player) &&
			!player.input.isHeld(Control.Up, player) && ItemThrowCooldown == 0
		) {
			changeState(new DynamoCross(), true);
			ItemThrowCooldown = 1.2f;
		}



		if (
			player.input.isPressed(Control.Special1, player) &&
		!player.input.isHeld(Control.Down, player) &&
			player.input.isHeld(Control.Up, player) && AxeCoolDown == 0
		) {
			changeState(new DynamoAxe(), true);
			AxeCoolDown = 1.2f;
		}



		if ((!player.input.isHeld(Control.Left, player)
		&& !player.input.isHeld(Control.Right, player)) &&
			player.input.isPressed(Control.Special1, player) &&
		!player.input.isHeld(Control.Down, player)
		&& !player.input.isHeld(Control.Up, player)
		) {
			changeState(new DynamoBoomerang(), true);
		}


		if (player.input.isPressed(Control.WeaponLeft, player) &&
		!player.input.isHeld(Control.Down, player) && DaggerCooldown == 0 &&
			!player.input.isHeld(Control.Up, player)
		) {
			changeState(new DynamoDaggerLV1(), true);
			DaggerCount += 1;

			if (DaggerCount > 5) {
				DaggerCooldown = 1.5f;
			}
		}

		if (player.input.isPressed(Control.WeaponLeft, player) &&
		!player.input.isHeld(Control.Down, player) &&
			player.input.isHeld(Control.Up, player) && player.superAmmo > 5
		) {
			changeState(new DynamoDaggerLV2(), true);
			player.superAmmo -= 6;


		}

		if (player.input.isR2Pressed(player)) {
            if (player.input.isHeld(Control.Up, player)) {
               if (player.superAmmo >= 32) {
                    player.superAmmo -= 32;
					changeState(new DynamoHydroStorm(), true);
			
                } 
            }
        }


		if (player.input.isPressed(Control.Special1, player) &&
		player.input.isHeld(Control.Down, player) &&
			!player.input.isHeld(Control.Up, player)
			&& !grounded && airShotCount == 0
		) {
			changeState(new DynamoAirShotState(), true);
			airShotCount = 1;
		}

		bool hadokenCheck = false;
		bool shoryukenCheck = false;
		hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);

		if (hadokenCheck && SlashCooldown == 0) {
			changeState(new DynamoBladeDash(), true);
			SlashCooldown = 1.5f;
		}

		return base.attackCtrl();
	}



	public override int baselineMaxHealth() {

		return 25;
	}


	public override void update() {
		base.update();
		var subtanks = player.subtanks;
		if (charState is Taunt && subtanks.Count > 0 && health < maxHealth) {
						subtanks[1 - 1].use(this);			
		}
		// Hypermode music.
		if (OverDrive) {
			if (musicSource == null) {
				addMusicSource("dynamo", getCenterPos(), true);
			}
		} else if (musicSource != null) {
			musicSource.stop();
			musicSource = null;
		}


		if (charState is Dash &&
		player.input.isHeld(Control.Down, player)
		) {
			changeState(new DynamoSlide());
		}


		if (DaggerCooldown == 0) {
			DaggerCount = 0;
		}

		if (player.superAmmo > 15 && player.input.isHeld(Control.Up, player)
		&& player.input.isPressed(Control.Special2, player) && !isInDamageSprite()) {
			
			initiateDynamoTimeStop();
		}




		//Cooldowns
		Helpers.decrementTime(ref DaggerCooldown);
		Helpers.decrementTime(ref ItemThrowCooldown);
		Helpers.decrementTime(ref SlashCooldown);
		Helpers.decrementTime(ref AxeCoolDown);



		if (grounded || charState is Idle or WallSlide) {
			backFlipCount = 0;
			uppercutCount = 0;
			airShotCount = 0;
		}


		// MicroDash
		if ((charState is Dash || charState is AirDash)
		&& (player.input.isPressed(Control.Shoot, player)
		|| player.input.isPressed(Control.Special1, player))) {
			slideVel = xDir * getDashSpeed();
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


	public void initiateDynamoTimeStop() {
			player.superAmmo -= 16;
			new DarkHoldDProj(new DarkHoldWeapon(), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			playSound("dynamoting", forcePlay: false, sendRpc: true);
			changeState(new Idle());
			playSound("dynamoUltraCross1", forcePlay: false, sendRpc: true);

			if (OverDrive) {
				new DarkHoldProj(
			pos.addxy(0, -20), xDir, this,
			player, player.getNextActorNetId(), rpc: true
			);
				playSound("darkhold");
			}
	}


	// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {

		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new SonicSlicer(), centerPoint, ProjIds.DynamoBlockSlash,
				player, 0.5f, 0, 20, isDeflectShield: true, addToLevel: true, hitSound : "htsnd_slash1"
			);
		}

		if (sprite.name.Contains("blade_attack") && !sprite.name.Contains("air")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber2, player, 3f, 20, 15, clashTier: ClashTier.Weak, addToLevel: true,
			 hitspark: "hitspark_slashx4_2", hitSound : "htsnd_slash_deep1"
			 );
		}

		if (sprite.name.Contains("blade_attack_air")) {
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.DynamoDropSlash, player, 3f, 25, 15f, clashTier: ClashTier.Weak,
			  addToLevel: true, hitspark: "hitspark_slashx4_1", hitSound : "htsnd_slash_deep2"
			 );
		}

		if (sprite.name.Contains("whipattack")) {
			return new GenericMeleeProj(new StrikeChain(), centerPoint,
			 ProjIds.ZSaber1, player, 2f, 15, 15f, clashTier: ClashTier.Weak
			 , isJuggleProjectile: true, addToLevel: true, hitspark: "hitspark_2", hitSound : "mugenhtsnd_hit2"
			 );
		}


		if (sprite.name.Contains("slide") && !sprite.name.Contains("jump")) {
			return new GenericMeleeProj(new SpinningBlade(), centerPoint,
			 ProjIds.ZSaber1, player, 1f, 15, 10f, clashTier: ClashTier.Weak, addToLevel: true, hitSound : "kofhtsnd_punch1"
			 );
		}

		if (sprite.name.Contains("uppercut")) {
			return new GenericMeleeProj(new StrikeChain(), centerPoint,
			 ProjIds.ZSaber3, player, 2f, 15, 10f, clashTier: ClashTier.Weak, hitSound : "htsnd_slash_deep3"
			, hitspark: "hitspark_slashx4_2" , isJuggleProjectile: true, addToLevel: true, isLiftProjectile : true);
		}

		if (sprite.name.Contains("slide_jump")) {
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.GBDKick, player, 2f, 15, 15f, clashTier: ClashTier.Weak
			 , isJuggleProjectile: true, addToLevel: true, hitSound : "kofhtsnd_punch3"
			 );
		}

		if (!sprite.name.Contains("charge") && sprite.name.Contains("groundpunch")) {
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.MechFrogStompShockwave, player, 3f, 0, 15f, clashTier: ClashTier.Weak, hitSound : "kofhtsnd_knock1"
			 , hitspark: "hitspark_big", isJuggleProjectile: true, addToLevel: true);
		}
		if (sprite.name.Contains("charge") && sprite.name.Contains("groundpunch")) {
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.ForceGrabState, player, 0f, 0, 15f, addToLevel: true, hitSound : "kofhtsnd_grab2"
			);
		}

		if (sprite.name.Contains("bladedash")) {
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.HeavyPush, player, 2f, 30, 15f, clashTier: ClashTier.Weak, addToLevel: true,
			  hitspark: "hitspark_slashx4_2",
			  hitSound : "htsnd_slash_deep2"
			);
		}


		return null;
	}


	// Ammo section
	public override void addAmmo(float amount) {
		weaponHealAmount += amount;
	}

	public override void addPercentAmmo(float amount) {
		weaponHealAmount += amount * 0.32f;
	}

	public override bool canAddAmmo() {
		return player.superAmmo < player.superMaxAmmo;
	}


	public override bool canDash() {
		return flag == null;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		//if ((Options.main.enableSkins == true)
		//	&& Global.sprites.ContainsKey("dynamoalt_" + spriteName)){		
		//	return "dynamoalt_" + spriteName;
		//	}
		return "dynamo_" + spriteName;
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


		public override void render(float x, float y) {
		base.render(x, y);

		if (overDriveTimer > 0 && visible) {
			addRenderEffect(RenderEffectType.SpeedDevilTrailNoDash);
			removeRenderEffect(RenderEffectType.SpeedDevilTrail);
		} else {
			removeRenderEffect(RenderEffectType.SpeedDevilTrailNoDash);
			addRenderEffect(RenderEffectType.SpeedDevilTrail);
		} 

		
	}



		

	public override void aiAttack(Actor? target) {
		base.aiAttack(target);
		if (charState is LadderClimb || !charState.attackCtrl || isInvulnerable()) {
			return;
		}
		bool isTargetInAir = pos.y < target?.pos.y - 20;
		bool isTargetClose = pos.x < target?.pos.x - 40;
		aiAttackCooldown = Helpers.randomRange(20,60);
		int dynamoattack = Helpers.randomRange(0, 9);
		
		switch (dynamoattack) {
			case 1 when grounded && isTargetInAir && isTargetClose:
				changeState(new DynamoUpperCut(), true);
										
				break;
			
			case 2 when isTargetClose:
				changeState(new DynamoBladeSlash(), true);
			break;
			case 3 when !isTargetClose:
				changeState(new DynamoBladeDash(), true);
			break;
			case 4 when !isTargetClose:
				player.press(Control.WeaponLeft);
			break;
			case 5 when isTargetClose && !grounded:
				changeState(new DynamoAirShotState(), true);
			break;
			case 6 when !isTargetClose:
				changeState(new DynamoCross(), true);
			break;
			case 7 when isTargetClose:
				changeState(new DynamoAxe(), true);
			break;
			case 8 when !isTargetClose:
				changeState(new DynamoSlide(), true);
			break;
			case 9 when isTargetClose:
				changeState(new DynamoDaggerLV2(), true);
			break;
			default:
				player.press(Control.Shoot);
				if (isTargetClose) { aiAttackCooldown = 0; }
				break;
			
			}
		
	}
	public override void aiDodge(Actor? target) {
		foreach (GameObject gameObject in getCloseActors(32, true, false, false)) {
			if (gameObject is Projectile proj && proj.damager.owner.alliance != player.alliance) {
				if (player.superAmmo >= 16) {
					initiateDynamoTimeStop();
				} else if (grounded) {
					
					changeState(new BlockWCUT(), true);
				}
			}
		}
		base.aiDodge(target);
	}

}





