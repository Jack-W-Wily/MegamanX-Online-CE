using System;
using System.Collections.Generic;

namespace MMXOnline;

public class EnemySpawnerChar : BaseSigma {
	public float fireballCooldown;
	public Weapon fireballWeapon = new Sigma3FireWeapon();
	public float maxFireballCooldown = 0.39f;
	public float shieldCooldown;
	public float maxShieldCooldown = 1.125f;
	public bool shootOnce;

	public EnemySpawnerChar(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId,
		bool ownedByLocalPlayer, bool isWarpIn = true,
		SigmaLoadout? loadout = null,
		int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn,
		loadout, heartTanks, isATrans
	) {
		sigmaSaberMaxCooldown = 0.5f;
		altSoundId = AltSoundIds.X3;
		charId = CharIds.EnemySpawnerChar;


	}
	

	public override bool isSoftLocked() {
		if (	Global.level.levelData.name == "st_cybermaze_test" ) return true;
		
		return false;
	}

	public override bool canAirJump() {
		return true;
	}

	public override bool canDash() {
		return false;
	}

	public override void update() {
		base.update();
		if (	Global.level.levelData.name == "st_cybermaze_test") {
			vel.x = 0;
			if (charState is Idle){
			changeState(new BossWait());
			}
			vel.y = 0;
		}
		player.weapons.RemoveAll(w => w is not AbsorbWeapon);
		if (!ownedByLocalPlayer) {
			return;
		}
		fireballWeapon.update();
		Helpers.decrementTime(ref fireballCooldown);
		Helpers.decrementTime(ref shieldCooldown);
		Helpers.decrementFrames(ref aiAttackCooldown);
		if (sprite.name.EndsWith(charState.shootSpriteEx) == false) {
			shootOnce = false;
		}
		// For ladder and slide shoot.
		if (charState is WallSlide or LadderClimb &&
			charState.shootSpriteEx != "" &&
			sprite.name.EndsWith(charState.shootSpriteEx) == true
		) {
			if (isAnimOver() && charState is not Sigma3Shoot) {
				changeSpriteFromName(charState.sprite, true);
			} else {
				var shootPOI = getFirstPOI();
				if (shootPOI != null && fireballWeapon.shootCooldown == 0) {
					fireballWeapon.shootCooldown = 0.15f;
					int upDownDir = MathF.Sign(player.input.getInputDir(player).y);
					float ang = getShootXDir() == 1 ? 0 : 180;
					if (charState.shootSprite.EndsWith("jump_shoot_downdiag")) {
						ang = getShootXDir() == 1 ? 45 : 135;
					}
					if (charState.shootSprite.EndsWith("jump_shoot_down")) {
						ang = 90;
					}
					if (ang != 0 && ang != 180) {
						upDownDir = 0;
					}
					if (!shootOnce) {
						playSound("sigma3shoot", sendRpc: true);
						new Sigma3FireProj(
							shootPOI.Value, ang, upDownDir,
							player, player.getNextActorNetId(), sendRpc: true
						);
						shootOnce = true;
					}
				}
			}
		}
	}

	public override Collider getBlockCollider() {
		Rect rect = Rect.createFromWH(0, 0, 18, 40);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override bool attackCtrl() {
		if (isInvulnerableAttack() || player.weapon is MaverickWeapon) {
			return false;
		}
		bool attackPressed = false;
		if (player.weapon is not AssassinBulletChar) {
			if (player.input.isAPressed(player)) {
				attackPressed = true;
				lastAttackFrame = Global.floorFrameCount;
			}
		}
		framesSinceLastAttack = Global.floorFrameCount - lastAttackFrame;
		bool lenientAttackPressed = (attackPressed || framesSinceLastAttack < 5);

		if (player.input.isAPressed(player) && fireballCooldown == 0) {
			if (Helpers.randomRange(0, 3) == 0) {
				new ViralSigmaShootProj(new MechaniloidWeapon(player, MechaniloidType.Bird), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			} else if (Helpers.randomRange(0, 3) == 1) {
					new ViralSigmaShootProj(new MechaniloidWeapon(player, MechaniloidType.Tank), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			
			} else if (Helpers.randomRange(0, 3) == 2) {
					new ViralSigmaShootProj(new MechaniloidWeapon(player, MechaniloidType.BallWalker), pos, xDir, player, player.getNextActorNetId(), rpc: true);
			
			} else {
			 new MetClassic(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			}
			 fireballCooldown = 10;
		}
		if (player.input.isBPressed(player) && fireballCooldown == 0) {
			 new GreenDog(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			 fireballCooldown = 10;
		}
		if (player.input.isPressed(Control.WeaponLeft, player) && fireballCooldown == 0) {
			 new HogumerEasy(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			 fireballCooldown = 10;
		}
		if (player.input.isPressed(Control.WeaponRight, player) && fireballCooldown == 0) {
			 new MissileElecBlue(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			 fireballCooldown = 10;
		}
		if (player.input.isR2Pressed(player) && StageMaverick == null) {
			if (Global.level.levelData.name == "airport") {
			StageMaverick  = new StormEagle(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "mountain") {
			StageMaverick  = new ChillPenguin(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "powerplant") {
			StageMaverick  = new SparkMandrill(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "factory") {
			StageMaverick  = new FlameMammoth(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "forest") {
			StageMaverick  = new StingChameleon(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "tower") {
			StageMaverick  = new BoomerangKuwanger(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "gallery") {
			StageMaverick  = new ArmoredArmadillo(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}
			else if (Global.level.levelData.name == "ocean") {
			StageMaverick  = new LaunchOctopus(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);	
			}

			else {
			StageMaverick  = new FakeZero(player, pos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			}
			StageMaverick?.health = 200;
			StageMaverick?.shouldDealColisionDmg = true;
		}
		return base.attackCtrl();
	}

	public Maverick StageMaverick;

	public override string getSprite(string spriteName) {
		return "empty_" + spriteName;
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Guard,
		Shield,
		ShieldGuard,
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override int getHitboxMeleeId(Collider hitbox) {
		if (sprite.name == "sigma3_block") {
			return (int)MeleeIds.ShieldGuard;
		}
		if (hitbox.name == "shield") {
			return (int)MeleeIds.Shield;
		}
		return (int)MeleeIds.None;
	}

	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return id switch {
			(int)MeleeIds.Shield or (int)MeleeIds.ShieldGuard => new GenericMeleeProj(
				new Weapon(), pos, ProjIds.Sigma3ShieldBlock, player,
				damage: 0, flinch: 0, hitCooldown: 60,
				isDeflectShield: true, isShield: true,
				isReflectShield: id == (int)MeleeIds.ShieldGuard,
				addToLevel: addToLevel
			) {
				highPiority = true
			},
			_ => null
		};
	}


	public override bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
	return  false;
	}

	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;

		if (Global.isOnFrameCycle(8)) {
			palette = player.sigmaShieldShader;
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
	public float aiAttackCooldown;
	public override void aiAttack(Actor? target) {
		bool isTargetInAir = pos.y < target?.pos.y - 20;
		bool isTargetClose = pos.x < target?.pos.x - 10;
		bool isFacingTarget = (pos.x < target?.pos.x && xDir == 1) || (pos.x >= target?.pos.x && xDir == -1);
		if (currentWeapon is MaverickWeapon mw &&
			mw.maverick == null && canAffordMaverick(mw)
		) {
			buyMaverick(mw);
			if (mw.maverick != null) {
				changeState(new CallDownMaverick(mw.maverick, true, false), true);
			}
			mw.summon(player, pos, xDir);
			player.changeToSigmaSlot();
		}
		if (charState is not LadderClimb) {
			int DoppmaSigmaAttack = Helpers.randomRange(0, 4);
			if (isTargetInAir) DoppmaSigmaAttack = 1;
			if (charState?.isGrabbedState == false && !player.isDead && aiAttackCooldown <= 0 &&
				!isInvulnerable() && !(charState is CallDownMaverick or SigmaThrowShieldState or Sigma3Shoot)) {
				switch (DoppmaSigmaAttack) {
					case 0 when isFacingTarget:
						player.press(Control.Shoot);
						break;
					case 1 when isFacingTarget:
						player.press(Control.Special1);
						break;
					case 2:
						player.press(Control.R2);			
						break;
					case 3:
						player.press(Control.WeaponLeft);						
						break;
					case 4:
						player.press(Control.WeaponRight);
						break;
				}
				aiAttackCooldown = 20;
			}
		}
		base.aiAttack(target);
	}
	public override void aiUpdate(Actor? target) {
		if (charState is Die) {
			foreach (Weapon weapon in weapons) {
				if (weapon is MaverickWeapon mw && mw.maverick != null) {
					mw.maverick.changeState(new MExit(mw.maverick.pos, true), true);
				}
			}
		}
		base.aiUpdate(target);
	}
}
