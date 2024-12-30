using System;
using System.Collections.Generic;

namespace MMXOnline;

public class Doppma : BaseSigma {
	public float fireballCooldown;
	public Weapon fireballWeapon = new Sigma3FireWeapon();
	public float maxFireballCooldown = 0.39f;
	public float shieldCooldown;
	public float maxShieldCooldown = 1.125f;

	public Doppma(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId,
		bool ownedByLocalPlayer, bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible,
		netId, ownedByLocalPlayer, isWarpIn
	) {
		sigmaSaberMaxCooldown = 0.5f;
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) {
			return;
		}

		if (charState.attackCtrl){
			if ((charState is Dash || charState is AirDash) 
			&& (player.input.isPressed(Control.Shoot, player)
			|| player.input.isPressed(Control.Special1, player))){
			slideVel = xDir * getDashSpeed();			
			}
		}


		fireballWeapon.update();
		Helpers.decrementTime(ref fireballCooldown);
		Helpers.decrementTime(ref shieldCooldown);
		// For ladder and slide shoot.
		if (charState is WallSlide or LadderClimb &&
			!string.IsNullOrEmpty(charState?.shootSprite) &&
			sprite?.name?.EndsWith(charState.shootSprite) == true
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
					playSound("sigma3shoot", sendRpc: true);
					new Sigma3FireProj(
						shootPOI.Value, ang, upDownDir,
						player, player.getNextActorNetId(), sendRpc: true
					);
				}
			}
		}
	}

	public override Collider getBlockCollider() {
		Rect rect = Rect.createFromWH(0, 0, 23, 55);
		return new Collider(rect.getPoints(), false, this, false, false, HitboxFlag.Hurtbox, new Point(0, 0));
	}

	public override bool attackCtrl() {
		if (isInvulnerableAttack() || player.weapon is MaverickWeapon) {
			return false;
		}
		bool attackPressed = false;
		if (player.weapon is not AssassinBullet) {
			if (player.input.isPressed(Control.Shoot, player)) {
				attackPressed = true;
				lastAttackFrame = Global.level.frameCount;
			}
		}
		framesSinceLastAttack = Global.level.frameCount - lastAttackFrame;
		bool lenientAttackPressed = (attackPressed || framesSinceLastAttack < 5);

		// Shoot button attacks.
		if (lenientAttackPressed) {
			if (charState is LadderClimb) {
				if (player.input.isHeld(Control.Left, player)) {
					xDir = -1;
				} else if (player.input.isHeld(Control.Right, player)) {
					xDir = 1;
				}
			}

			if (fireballWeapon.shootCooldown == 0 && fireballCooldown == 0) {
				if (charState is WallSlide or LadderClimb) {
					changeSpriteFromName(charState.shootSprite, true);
				} else {
					changeState(new Sigma3Shoot(player.input.getInputDir(player)), true);
				}
				fireballCooldown = maxFireballCooldown;
				return true;
			}
		}
		if (player.input.isPressed(Control.Special1, player) &&
			charState is not SigmaThrowShieldState && shieldCooldown == 0
		) {
			shieldCooldown = maxShieldCooldown;
			changeState(new SigmaThrowShieldState(), true);
			return true;
		}
		if (player.input.isHeld(Control.Down, player) &&
		player.input.isHeld(Control.Dash, player) &&
			charState is not BurnerPunch && shieldCooldown == 0
		) {
			shieldCooldown = maxShieldCooldown;
			changeState(new BurnerPunch(), true);
			return true;
		}

		return base.attackCtrl();
	}

	public override string getSprite(string spriteName) {
		return "sigma3_" + spriteName;
	}

	// This can run on both owners and non-owners. So data used must be in sync.
	public override Projectile? getProjFromHitbox(Collider collider, Point centerPoint) {
		if (collider.name == "shield") {
			bool isBlock = sprite.name == getSprite("block");
			return new GenericMeleeProj(
				new Weapon(), centerPoint, ProjIds.Sigma3ShieldBlock, player,
				damage: 0, flinch: 0, hitCooldown: 1, isDeflectShield: true, isShield: true, isReflectShield: isBlock
			);
		}

		if (sprite.name.Contains("punch")) {
			return new GenericMeleeProj(
				new Weapon(), centerPoint, ProjIds.FireWave, player,
				damage: 2, flinch: 20, hitCooldown: 1, isDeflectShield: true, isShield: true
			);
		}

		if (sprite.name.Contains("dash")) {
			return new GenericMeleeProj(
				new Weapon(), centerPoint, ProjIds.UPPunch, player,
				damage: 2, flinch: 20, hitCooldown: 1, isDeflectShield: true, isShield: true
			);
		}

		if (sprite.name.Contains("throw")) {
			return new GenericMeleeProj(
				new Weapon(), centerPoint, ProjIds.NormalPush, player,
				damage: 2, flinch: 0, hitCooldown: 1, isDeflectShield: true, isShield: true
			);
		}

		return base.getProjFromHitbox(collider, centerPoint);
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
}
