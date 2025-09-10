using System;
using System.Collections.Generic;

namespace MMXOnline;


public class ExplosiveRoundState : CharState {
	int bombNum;
	bool isNapalm;

	Character vile;

	public ExplosiveRoundState() : base("air_bomb_attack", "", "") {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		

			if (bombNum > 0 && player.input.isBPressed(player)) {
				character.changeState(new Fall(), true);
			}

			var inputDir = player.input.getInputDir(player);
			if (inputDir.x == 0) inputDir.x = character.xDir;
			if (stateTime > 0f && bombNum == 0) {
				bombNum++;
				new VileBombProj(
				character.pos, (int)inputDir.x, 0, vile, player,
				character.player.getNextActorNetId(), rpc: true);
			}
		if (stateTime > 0.23f && bombNum == 1) {
			character.changeState(new Fall(), true);


			bombNum++;
			new VileBombProj(
			character.pos, (int)inputDir.x, 0, vile, player,
				character.player.getNextActorNetId(), rpc: true);
		}
		if (stateTime > 0.45f && bombNum == 2) {
			character.changeState(new Fall(), true);

			bombNum++;
			new VileBombProj(
			character.pos, (int)inputDir.x, 0, vile, player,
			character.player.getNextActorNetId(), rpc: true);
		}

			if (stateTime > 0.68f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class PeaceOutRollerAttack : CharState {
	int bombNum;
	bool isNapalm;
	Character vile;

	public PeaceOutRollerAttack() : base("air_bomb_attack", "", "") {
	
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		
			if (stateTime > 0f && bombNum == 0) {
				bombNum++;
					new PeaceOutRollerProj(
						character.getCenterPos().addxy(20*character.xDir,0), character.xDir, 1, vile, player, 
						character.player.getNextActorNetId(), rpc: true
					);
				}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class SpreadShotKnee : CharState {
	int bombNum;
	Character vile;

	public SpreadShotKnee( ) : base("air_bomb_attack", "", "") {
	
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var ebw = new VileElectricBomb();
		if (bombNum > 0 && player.input.isBPressed(player)) {
			character.changeToIdleOrFall();
			return;
		}

		for (int i = 0; i < 7; i++) {
			if (stateTime > i * 0.1f && bombNum == i) {
				bombNum++;
				new StunShotProj2(
					character.pos, character.xDir, i + 1, 0, vile,
					character.player, character.player.getNextActorNetId(), rpc: true
				);
			}
		}
			
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}






public class SplashHitState : CharState {
	bool shot;
	
	float shootTime;
	int shootCount;

		Vile vile = null!;
	Point counterAttackPos;


	public SplashHitState(string transitionSprite = "") :
		base(getSprite(), "", "", transitionSprite) {

	}

	public static string getSprite() {
		return"kick_3";
	}

	public override void update() {
		base.update();
		Projectile proj;
	
		
		character.slideVel = character.xDir * character.getDashSpeed();	
		
			if (!shot && character.sprite.frameIndex == 4) {
				shot = true;
				//vile.setVileShootTime(vile.vileNapalmWeapon);
				var poi = character.sprite.getCurrentFrame().POIs[0];
				poi.x *= character.xDir;

				
					proj = new SplashHitProj(
			character.pos, character.xDir, character, character.player,
			character.player.getNextActorNetId(), rpc: true
		);
				
			}
		
		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}




public class BumptyBoomGranadeLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public BumptyBoomGranadeLaunch(string transitionSprite = "") : base("crouch_nade", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

			var poi = character.getFirstPOI();
			if (!once && poi != null) {
				once = true;
				var proj = new BumptyBoomProj(vile.napalmWeapon, poi.Value, character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
				proj.vel = new Point(character.xDir * 200, -200);
			}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		}

	

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class BumptyBoomProj : Projectile {
		public IDamagable target;
	bool exploded;
	public BumptyBoomProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 150, 2, player, "napalm_grenade", 0, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.NapalmGrenade;
	
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		shouldShieldBlock = false;
	}

	public override void update() {
		base.update();
		if (grounded) {
			explode();
		}
	}

	public override void onHitWall(CollideData other) {
		xDir *= -1;
		explode();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (ownedByLocalPlayer) explode();
	}

	public void explode() {
		if (exploded) return;
		exploded = true;
		if (ownedByLocalPlayer) {
			new GrenadeExplosionProj(
				weapon, pos, xDir, owner, 1, target, Math.Sign(vel.x), owner.getNextActorNetId()
			);
		}
		destroySelf();
	}
}





public class RumblingBangLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public RumblingBangLaunch(string transitionSprite = "") : base("crouch_nade", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			var proj = new NapalmGrenadeProj(
				poi.Value, character.xDir, vile, character.player,
				character.player.getNextActorNetId(), rpc: true
			);
			proj.vel = new Point(character.xDir * 100, 0);
		}

		if (stateTime > 0.25f) {
			character.changeToIdleOrFall();
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class AirSplashHitGranadeLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public AirSplashHitGranadeLaunch(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			var proj = new SplashHitGrenadeProj(
				poi.Value, character.xDir, vile, character.player,
				character.player.getNextActorNetId(), rpc: true
			);
			proj.vel = new Point(character.xDir * 100, 0);
		}

		if (stateTime > 0.25f) {
			character.changeToIdleOrFall();
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class TerriotiralPowState : CharState {
	int bombNum;

	TerritorialPowProj proj;
	TerritorialPowProj proj2;
	TerritorialPowProj proj3;
	Vile vile = null!;

	public TerriotiralPowState(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

			var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			character.playSound("ballPOR", sendRpc: true);
			proj = new TerritorialPowProj(
				poi.Value, character.xDir, vile, character.player,
				character.player.getNextActorNetId(), rpc: true
			);
			proj2 = new TerritorialPowProj(
				poi.Value, character.xDir, vile, character.player,
				character.player.getNextActorNetId(), rpc: true
			);
			proj3 = new TerritorialPowProj(
				poi.Value, character.xDir, vile, character.player,
				character.player.getNextActorNetId(), rpc: true
			);
			proj.vel = new Point(character.xDir * 300, 0);
			proj2.vel = new Point(character.xDir * 300, 200);
			proj3.vel = new Point(character.xDir * 300, 400);
			}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		}

	

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (proj != null) {
			proj.vel = new Point(character.xDir * 0, 0);
		}
		if (proj2 != null) {
			proj2.vel = new Point(character.xDir * 0, 0);
		}
		if (proj3 != null) {
			proj3.vel = new Point(character.xDir * 0, 0);
		}
		character.useGravity = true;
		
	}
}





public class AirFireNadeLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public AirFireNadeLaunch(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			var proj = new MK2NapalmGrenadeProj(
					poi.Value, character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				);
			proj.vel = new Point(character.xDir * 100, 0);
			character.playSound("FireNappalmMK2", forcePlay: false, sendRpc: true);

		}

		if (stateTime > 0.25f) {
			character.changeToIdleOrFall();
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class SwordBouqueteLaunch : CharState {
	int bombNum;

	Vile vile = null!;

	public SwordBouqueteLaunch(string transitionSprite = "") : base("air_bomb_attack", "", "", transitionSprite) {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

		var poi = character.getFirstPOI();
		if (!once && poi != null) {
			once = true;
			var proj = new SwordBouquetStart(
					poi.Value, character.xDir, vile, character.player,
					character.player.getNextActorNetId(), rpc: true
				);
			proj.vel = new Point(character.xDir * 100, 0);
			character.playSound("FireNappalmMK2", forcePlay: false, sendRpc: true);

		}

		if (stateTime > 0.25f) {
			character.changeToIdleOrFall();
		}
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class SwordBouquetStart : Projectile {
	public SwordBouquetStart(
		Point pos, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "sword_bouquet_proj_start", netId, player
	) {
		weapon = FireWave.netWeapon;
		damager.damage = 2;
		damager.hitCooldown = 13;
		vel = new Point(150 * xDir, 0);
		projId = (int)ProjIds.SwordBouquetStart;
		if (collider != null) { collider.wallOnly = true; }
		destroyOnHit = false;
		shouldShieldBlock = false;
		maxTime = 8; // WDYM IT WAS INFINITE BEFORE
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new FireWaveProjChargedStart(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void update() {
		base.update();
		if (isUnderwater()) {
			destroySelf(disableRpc: true);
			return;
		}
		incPos(new Point(0, Global.spf * 100));
		if (grounded) {
			destroySelf();
			if (ownedByLocalPlayer) {
				new SwordBouquetProj(
					pos, xDir, this, damager.owner, 0,
					Global.level.mainPlayer.getNextActorNetId(), 0, rpc: true
				);
				playSound("fireWave");
			}
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		var character = damagable as Character;
	}

	public void putOutFire() {
		base.destroySelf("", "", false, true);
	}
}

public class SwordBouquetProj : Projectile {
	public Sprite spriteMid;
	public Sprite spriteTop;
	public float riseY = 0;
	public float parentTime = 0;
	public SwordBouquetProj? child;
	public bool reversedOnce;
	public int timesReversed;
	float soundCooldown;
	public SwordBouquetProj(
		Point pos, int xDir, Actor owner, Player player, float parentTime,
		ushort? netId, int timesReversed, bool rpc = false
	) : base(
		pos, xDir, owner, "sword_bouquet_proj", netId, player	
	) {
		weapon = FireWave.netWeapon;
		damager.damage = 1;
		damager.hitCooldown = 19;
		vel = new Point(0 * xDir, 0);
		projId = (int)ProjIds.FireWaveCharged;
		spriteMid = new Sprite("sword_bouquet_proj");
		spriteMid.visible = false;
		spriteTop = new Sprite("sword_bouquet_proj");
		spriteTop.visible = false;
		useGravity = true;
		if (collider != null) { collider.wallOnly = true; }
		frameSpeed = 0;
		this.parentTime = parentTime;
		destroyOnHit = false;
		destroyOnDMG = true;
		shouldShieldBlock = false;
		this.timesReversed = timesReversed;
		new Anim(this.pos, "fire_wave_charge_flash", 1, null, true);

		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		maxTime = 0.48f;
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new SwordBouquetProj(
		args.pos, args.xDir, args.owner, args.player, 0, args.netId, 0
		);
	}

	public override void render(float x, float y) {
		sprite.draw(frameIndex, pos.x + x, pos.y + y - riseY, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex);
		spriteMid.draw((int)MathF.Round(frameIndex + (sprite.totalFrameNum / 3)) % sprite.totalFrameNum, pos.x + x, pos.y + y - 6 - riseY, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex);
		spriteTop.draw((int)MathF.Round(frameIndex + (sprite.totalFrameNum / 2)) % sprite.totalFrameNum, pos.x + x, pos.y + y - 12 - riseY, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex);
	}

	public override void update() {
		base.update();
		if (isUnderwater()) {
			destroySelf(disableRpc: true);
			return;
		}
		if (soundCooldown > 0) {
			soundCooldown = Helpers.clampMin0(soundCooldown - Global.spf);
		}
		frameSpeed = 1;
		if (time >= 0.16f) {
			spriteTop.visible = true;
			spriteMid.visible = true;
			riseY += (Global.spf * 75);
		}
		if (time > 0.2f && child == null && parentTime < 3) {
			if (soundCooldown == 0) {
				playSound("fireWave");
				soundCooldown = 0.25f;
			}

			if (ownedByLocalPlayer) {
				var wall = Global.level.checkTerrainCollisionOnce(this, 16 * xDir, -4);
				var sign = 1;
				if (wall != null && wall.gameObject is Wall && wall.hitData.normal != null && !wall.hitData.normal.Value.isAngled()) {
					sign = -1;
					timesReversed++;
				} else {
				}

				if (timesReversed > 0) {
					destroySelf();
					return;
				}
				child = new SwordBouquetProj(
					pos.addxy(16 * xDir, 0), xDir * sign, this,
					damager.owner, time + parentTime, Global.level.mainPlayer.getNextActorNetId(),
					timesReversed, rpc: true
				);
			}
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		var character = damagable as Character;
	}

	public override void onDestroy() {
		var newPos = pos.addxy(0, -24 - riseY);
		new Anim(newPos, "fire_wave_charge_fade", 1, null, true);
	}

	public void putOutFire() {
		base.destroySelf("", "", false, true);
	}
}

