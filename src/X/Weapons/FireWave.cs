using System;
using System.Collections.Generic;

namespace MMXOnline;

public class FireWave : Weapon {
	public static FireWave netWeapon = new();
	
	public FireWave() : base() {
		index = (int)WeaponIds.FireWave;
		killFeedIndex = 4;
		weaponBarBaseIndex = 4;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 4;
		weaknessIndex = (int)WeaponIds.StormTornado;
		shootSounds = new string[] { "fireWave", "fireWave", "fireWave", "fireWave" , ""};
		fireRate = 4;
		isStream = true;
		//switchCooldown = 0.25f;
		switchCooldownFrames = 0;
		damage = "1/1";
		ammousage = 0.5;
		effect = "Inflicts burn to enemies. DOT: 0.5/2 seconds.\nBurn won't give assists.";
		hitcooldown = "0.2/0.33";
		maxAmmo = 28;
		ammo = maxAmmo;
	}

	public float altfireCooldown;

	
	public override void update() {
		base.update();
		Helpers.decrementTime(ref altfireCooldown);
	}

	

	public override float getAmmoUsage(int chargeLevel) {
		if (chargeLevel >= 3) {
			return 7;
		}
		return 0.15f;
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.getShootPos();
		int xDir = character.getShootXDir();
		Player player = character.player;

		if (character != null) {
			if (character.isUnderwater()) return;

			if (character.player.input.isHeld(Control.Up, player) && altfireCooldown == 0){
			altfireCooldown = 0.5f;
				new VelGFireProj(this, pos, xDir, player, player.getNextActorNetId(), rpc: true);
				Global.level.delayedActions.Add(new DelayedAction(() => {
				new VelGFireProj(this, pos, xDir, player, player.getNextActorNetId(), rpc: true);
				}, 0.025f));
				Global.level.delayedActions.Add(new DelayedAction(() => {
				new VelGFireProj(this, pos, xDir, player, player.getNextActorNetId(), rpc: true);
				}, 0.055f));
				Global.level.delayedActions.Add(new DelayedAction(() => {
				new VelGFireProj(this, pos, xDir, player, player.getNextActorNetId(), rpc: true);
				}, 0.075f));
			}

			if (chargeLevel < 3) {
				var proj = new FireWaveProj(this, pos, xDir, player, player.getNextActorNetId(), true);
				proj.vel.inc(character.vel.times(-0.5f));
			} 
			if (chargeLevel == 3) {
				new FireWaveProjChargedStart(this, pos, xDir, player, player.getNextActorNetId(), true);
			}
			if (chargeLevel == 4) {
				character.changeState(new FireWaveUltraCharge(character.grounded), true);
			}

			
		}
	}
}

public class FireWaveProj : Projectile {
	public FireWaveProj(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 400, 1, player, "fire_wave",
		1, 0.2f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FireWave;
		fadeSprite = "fire_wave_fade";
		maxTime = 0.1f;
		destroyOnHit = false;

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new FireWaveProj(
			FireWave.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.netId
		);
	}
}

public class FireWaveProjChargedStart : Projectile {
	public FireWaveProjChargedStart(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 150, 2, player, "fire_wave_charge", 
		0, 0.222f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FireWaveChargedStart;
		collider.wallOnly = true;
		destroyOnHit = false;
		shouldShieldBlock = false;

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new FireWaveProjChargedStart(
			FireWave.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.netId
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
				new FireWaveProjCharged(
					weapon, pos, xDir, damager.owner, 0,
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

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is FlameMOilSpillProj oilSpill && oilSpill.ownedByLocalPlayer) {
			playSound("flamemOilBurn", sendRpc: true);
			new FlameMBigFireProj(
				new FlameMOilFireWeapon(), oilSpill.pos, oilSpill.xDir,
				oilSpill.angle ?? 0, owner, owner.getNextActorNetId(), rpc: true
			);
			// oilSpill.time = 0;
			oilSpill.destroySelf(doRpcEvenIfNotOwned: true);
			destroySelf();
		}
	}

	public void putOutFire() {
		base.destroySelf("", "", false, true);
	}
}

public class FireWaveProjCharged : Projectile {
	public Sprite spriteMid;
	public Sprite spriteTop;
	public float riseY = 0;
	public float parentTime = 0;
	public FireWaveProjCharged? child;
	public bool reversedOnce;
	public int timesReversed;
	float soundCooldown;
	public FireWaveProjCharged(
		Weapon weapon, Point pos, int xDir, Player player, 
		float parentTime, ushort netProjId, int timesReversed, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 3, player, "fire_wave_charge", 
		30, 1f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.FireWaveCharged;
		spriteMid = new Sprite("fire_wave_charge");
		spriteMid.visible = false;
		spriteTop = new Sprite("fire_wave_charge");
		spriteTop.visible = false;
		useGravity = true;
		collider.wallOnly = true;
		frameSpeed = 0;
		this.parentTime = parentTime;
		destroyOnHit = false;
		shouldShieldBlock = false;
		this.timesReversed = timesReversed;
		new Anim(this.pos.clone(), "fire_wave_charge_flash", 1, null, true);

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		maxTime = 0.48f;
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new FireWaveProjCharged(
			FireWave.netWeapon, arg.pos, arg.xDir, 
			arg.player, 0, arg.netId, 0
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
				child = new FireWaveProjCharged(
					weapon, pos.addxy(16 * xDir, 0), xDir * sign,
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

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is FlameMOilSpillProj oilSpill && oilSpill.ownedByLocalPlayer) {
			playSound("flamemOilBurn", sendRpc: true);
			new FlameMBigFireProj(
				new FlameMOilFireWeapon(), oilSpill.pos, oilSpill.xDir,
				oilSpill.angle ?? 0, owner, owner.getNextActorNetId(), rpc: true
			);
			// oilSpill.time = 0;
			oilSpill.destroySelf(doRpcEvenIfNotOwned: true);
		}
	}
}





public class FireWaveUltraCharge : CharState {
	bool fired = false;
	bool groundedOnce;
	public FireWaveUltraCharge(bool grounded) : base(!grounded ? "fall" : "punch_ground", "", "", "") {
		superArmor = true;
	}

	public override void update() {
		base.update();
		if (!character.ownedByLocalPlayer) return;

		if (!groundedOnce) {
			if (!character.grounded) {
				stateTime = 0;
				return;
			} else {
				groundedOnce = true;
				sprite = "punch_ground";
				character.changeSprite("mmx_punch_ground", true);
			}
		}

		if (character.frameIndex >= 6 && !fired) {
			fired = true;

			float x = character.pos.x;
			float y = character.pos.y;

			character.shakeCamera(sendRpc: true);

			var weapon = new TriadThunder();
			new FlameMOilProj(new FlameMOilWeapon(),  character.pos.addxy(0,-30), -1, player, player.getNextActorNetId(), rpc: true);
			new FlameMOilProj(new FlameMOilWeapon(), character.pos.addxy(0,-30), 1, player, player.getNextActorNetId(), rpc: true);
			new TriadThunderQuake(weapon, new Point(x, y), 1, player, player.getNextActorNetId(), rpc: true);

			character.playSound("crash", forcePlay: false, sendRpc: true);
		}

		if (stateTime > 0.75f) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (character.vel.y < 0) character.vel.y = 0;
	}
}

