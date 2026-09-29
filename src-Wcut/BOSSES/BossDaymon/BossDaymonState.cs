using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;


public class DaymonDoubleBuster : BusterZeroState {
	public bool fired1;
	public bool fired2;
	public bool isSecond;
	public bool isPinkCharge;
	public bool shootPressedAgain;
	public int startStockLevel;

	public DaymonDoubleBuster(bool isSecond, int startstockLevel) : base("doublebuster") {
		this.isSecond = isSecond;
		this.startStockLevel = startstockLevel;
		useDashJumpSpeed = true;
		airMove = true;
		superArmor = false;
		canStopJump = true;
		canJump = true;
		landSprite = "doublebuster";
		airSprite = "doublebuster_air";
	}

	public override void update() {
		base.update();
		if (player.input.isAPressed(player)|| player.isAI) {
			shootPressedAgain = true;
		}
		if (!fired1 && character.frameIndex == 3) {
			fired1 = true;
			character.playSound("buster3X3", sendRpc: true);
			zero.shootSub(2);
			zero.stockedTime = 0;
		}
		if (!fired2 && character.frameIndex == 7) {
			fired2 = true;
			if (!isPinkCharge) {
				zero.stockedBusterLv = 0;
				character.playSound("buster3X3", sendRpc: true);
				zero.shootSub(2);
			} else {
				zero.stockedBusterLv = 0;
				character.playSound("buster2X3", sendRpc: true);
				zero.shootSub(1);
			}
			zero.stockedTime = 0;
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		} else if (!isSecond && character.frameIndex >= 4 && !shootPressedAgain) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		// For the starting buster;
		if (startStockLevel is 1 or 3) {
			isPinkCharge = true;
		}
		// Non-full charge.
		if (isPinkCharge) {
			zero.stockedBusterLv = 1;
			isPinkCharge = true;
		}
		// Full charge.
		else {
			// We add Z-Saber charge if we fire the full charge and we were at 0 charge before.
			if (startStockLevel == 4 || !isSecond) {
				zero.stockedSaber = true;
			}
			zero.stockedBusterLv = 2;
		}
		if (!character.grounded || character.vel.y < 0) {
			sprite = "doublebuster_air";
			character.changeSpriteFromName(sprite, true);
		}
		// For halfway shot.
		if (startStockLevel <= 2) {
			character.frameIndex = 4;
			fired1 = true;
		}
	}

	public override void onExit(CharState? newState) {
		zero.stockedTime = 0;
		base.onExit(newState);
		// We check if we fired the second shot. If not we add the stocked charge.
		if (!fired2) {
			if (isPinkCharge) {
				zero.stockedBusterLv = 1;
			} else {
				zero.stockedBusterLv = 2;
				zero.stockedSaber = true;
			}
		}
		if (!fired1) {
			if (isPinkCharge) {
				zero.stockedBusterLv = 3;
			} else {
				zero.stockedBusterLv = 4;
				zero.stockedSaber = true;
			}
		}
	}
}






public class DaymonSlide : CharState {
	public const float maxGrabTime = 4;
	
	public long savedZIndex;
	public DaymonSlide() : base("dropkick") {
		
	}

	public override bool canEnter(Character character) {
		if (!base.canEnter(character)) return false;
		return !character.isInvulnerable() && !character.charState.invincible;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMovingS();
		character.stopCharge();
		savedZIndex = character.zIndex;
		
	}

	
	public bool hitonce;


	public override void onExit(CharState? newState) {
		base.onExit(newState);
		
		character.setzIndex(savedZIndex);
	}

	float smokeTime;

	bool firstHit;
	public override void update() {
		base.update();
		Helpers.decrementTime(ref smokeTime);

		if (smokeTime == 0 && character.grounded && stateTime > 0.15f){
			if (!firstHit) {
				firstHit = true;
				character.playSound("ggsweep_5");
				character.shakeCamera(sendRpc: true);
				character.applyDamage(2, player, character, (int)WeaponIds.SpeedBurner, (int)ProjIds.SpeedBurnerRecoil);
			}
		new Anim(
			character.pos.addxy(0, 5),
			"jump_sparks", character.xDir, player.getNextActorNetId(),
			true, sendRpc: true);
		smokeTime = 0.1f;
		}


	//	grabTime -= player.mashValue();
		if (grabTime <= 0) {
			character.changeToIdleOrFall();
		}

		character.move(new Point(character.xDir * 150, 0));
		if (stateTime > 2f && character.grounded) {
			character.changeState(
							new BossBackJump(
								
							), true
						);
		}


		CollideData? collideData = Global.level.checkTerrainCollisionOnce(character, -character.xDir, 0);
		if (!hitonce &&collideData != null && collideData.isSideWallHit() && character.ownedByLocalPlayer) {
			hitonce = true;
				character.changeState(
							new BossBackJump(
								
							), true
						);
			
			new Anim(character.pos, "hitwave_wall", -character.xDir, null, true);
		} 

	}	
}







public class DaymonBladeSpin : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	

	public DaymonBladeSpin(string transitionSprite = "")
		: base("blade_spin", "", "", transitionSprite)
	{
	airMove = true;
	canSpecialCancel = true;
	superArmor = true;
	
	}

	public override void update()
	{
	

		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

	

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (stateTime > 1) {
			character.changeState(new DaymonBladeThrow(),true);
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("throwCross", forcePlay: false, sendRpc: true);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	
		
	
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class DaymonBladeThrow : CharState {
	public float pushBackSpeed;
	DaymonBlade? proj;

	public DaymonBladeThrow() : base("toss_blade") {
		airMove = true;
	}

	public override void update() {
		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		if (proj == null && character.frameIndex >= 1 && character.ownedByLocalPlayer) {
			character.playSound("throwAxe", forcePlay: false, sendRpc: true);
			proj = 	new DaymonBlade(
				character.pos.addxy(16 * character.xDir, -36), character.xDir,
				character, player.getNextActorNetId(), sendRpc: true
			);
		}

		base.update();
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

public class DaymonBlade : Projectile {
	public float angleDist;

	public DaymonBlade(
		Point pos, int xDir, Actor owner, ushort? netId,
		bool sendRpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "daymon_blade_proj", netId, altPlayer
	) {
		weapon = SonicSlicer.netWeapon;
		damager.damage = 1;
		damager.flinch = Global.defFlinch + 5;
		damager.hitCooldown = 8;
		vel = new Point(200 * xDir, -350);

		fadeSprite = "explosion";
		maxTime = 1f;
		fadeOnAutoDestroy = true;
		hitSound = "htsnd_slash1";
		projId = (int)ProjIds.DynamoAxeProj;
		destroyOnHit = false;
		useGravity = false;

		if (sendRpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new DynamoAxeProj(
			args.pos, args.xDir, args.owner, args.netId, altPlayer: args.player
		);
	}

	public override void update() {
		base.update();

		angleDist += 16 * speedMul;
		byteAngle = MathF.Round(xDir * angleDist / 32) * 32;

		vel.x = Helpers.lerp(vel.x, 0, 1 * Global.spf);
		if (vel.y < Physics.MaxFallSpeed) {
			vel.y += Physics.Gravity;
		}
	}
}




public class DaymonSlash : CharState {


	public DaymonSlash() : base("projswing") {
		immuneToWind = true;
		enterSound = "distortion_d";
		
	}

	public override void update() {
		base.update();

		if (character.frameIndex < 3) {
			character.move(new Point(character.xDir * 350, 0));
		}



		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel.y = 0;

	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		specialId = SpecialStateIds.None;
	}
}


