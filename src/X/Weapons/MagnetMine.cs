﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class MagnetMine : Weapon {
	public static MagnetMine netWeapon = new(); 
	public const int maxMinesPerPlayer = 10;

	public MagnetMine() : base() {
		shootSounds = new string[] { "magnetMine", "magnetMine", "magnetMine", "magnetMineCharged" , ""};
		fireRate = 45;
		index = (int)WeaponIds.MagnetMine;
		weaponBarBaseIndex = 15;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 15;
		killFeedIndex = 20 + (index - 9);
		weaknessIndex = (int)WeaponIds.SilkShot;
		effect = "C: Can absorb projectiles and grow it's size.\nSize growth depends on the damage of the projectile.";
		hitcooldown = "0/0.2";
		damage = "2-4/1-2-4";
		Flinch = "0/26";
		FlinchCD = "0/1";
		type = index;
		displayName = "Magnet Mine ";
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.getShootPos();
		int xDir = character.getShootXDir();
		Player player = character.player;


		if (chargeLevel < 3) {
	
			var magnetMineProj = new MagnetMineProj(this, pos, xDir, player, player.getNextActorNetId(), true);
			character.magnetMines.Add(magnetMineProj);
			if (character.magnetMines.Count > maxMinesPerPlayer) {
				character.magnetMines[0].destroySelf();
				character.magnetMines.RemoveAt(0);
			}
		
		} else {
			new MagnetMineProjCharged(this, pos, xDir, player, player.getNextActorNetId(), true);
		}
	}
}

public class MagnetMineProj : Projectile, IDamagable {
	public bool landed;
	public float health = 2;
	public Player player;
	float maxSpeed = 150;

	public MagnetMineProj(
		Weapon weapon, Point pos, int xDir,
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 75, 2, player, "magnetmine_proj",
		0, 0, netProjId, player.ownedByLocalPlayer
	) {
		//maxTime = 2f;
		maxDistance = 224;
		fadeSprite = "explosion";
		fadeSound = "explosion";
		reflectable = false;
		projId = (int)ProjIds.MagnetMine;
		this.player = player;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		canBeLocal = false;
		destroyOnHit = true;


		if (owner.isGBD){
	damager.damage = 1;
		damager.flinch = 4;
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new MagnetMineProj(
			MagnetMine.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void preUpdate() {
		base.preUpdate();
		updateProjectileCooldown();
	}

	public override void update() {
		base.update();

		if (landed && ownedByLocalPlayer) {
			moveWithMovingPlatform();
		}

			if (owner.isGBD){
		changeSprite("tgbd_mine_proj", true);
		}

		if (ownedByLocalPlayer && owner != null && !landed) {
			vel.x += xDir * 600 * Global.spf;
			if (vel.x > maxSpeed) vel.x = maxSpeed;
			if (vel.x < -maxSpeed) vel.x = -maxSpeed;

			if (!owner.isDead && !owner.isGBD) {
				if (owner.input.isHeld(Control.Up, owner)) {
					vel.y = Helpers.clampMin(vel.y - Global.spf * 2000, -300);
				}
				if (owner.input.isHeld(Control.Down, owner)) {
					vel.y = Helpers.clampMax(vel.y + Global.spf * 2000, 300);
				}
			}
			if (owner.isGBD)vel.y = Helpers.clampMax(vel.y + Global.spf * 2000, 300);
		}
	}

	public void applyDamage(float damage, Player? owner, Actor? actor, int? weaponIndex, int? projId) {
		if (!ownedByLocalPlayer) {
			return;
		}
		health -= damage;
		if (health <= 0) {
			destroySelf();
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (!landed && other.gameObject is Wall) {
			landed = true;
			updateDamager(4);

			if (player.isMainPlayer && !Global.level.gameMode.isTeamMode) {
				removeRenderEffect(RenderEffectType.BlueShadow);
				removeRenderEffect(RenderEffectType.RedShadow);
				addRenderEffect(RenderEffectType.GreenShadow);
			}

			vel = new Point();
		if (!owner.isGBD)changeSprite("magnetmine_landed", true);
			playSound("minePlant");
			maxTime = 300;

			var triggers = Global.level.getTriggerList(this, 0, 0);
			if (triggers.Any(t => t.gameObject is MagnetMineProj)) {
				incPos(new Point(Helpers.randomRange(-2, 2), Helpers.randomRange(-2, 2)));
			}
		}
	}

	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId) {
		return player.alliance != damagerAlliance;
	}

	public bool isInvincible(Player attacker, int? projId) {
		return false;
	}

	public bool canBeHealed(int healerAlliance) {
		return false;
	}

	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false) {
	}

	public override void onDestroy() {
		if (!ownedByLocalPlayer) {
			return;
		}
		(player.character as MegamanX)?.magnetMines.Remove(this);
	}

	public bool canBeSucked(int alliance) {
		if (player.alliance == alliance) return false;
		return true;
	}
}

public class MagnetMineProjCharged : Projectile {
	public float size;
	public int phase;

	float soundTime;
	float startY;
	public MagnetMineProjCharged(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 50, 1, player, "magnetmine_charged",
		Global.defFlinch, 0.2f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 4f;
		destroyOnHit = false;
		shouldShieldBlock = false;
		projId = (int)ProjIds.MagnetMineCharged;
		startY = pos.y;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new MagnetMineProjCharged(
			MagnetMine.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		for (int i = Global.level.chargedCrystalHunters.Count - 1; i >= 0; i--) {
			var cch = Global.level.chargedCrystalHunters[i];
			if (cch.pos.distanceTo(pos) < CrystalHunterCharged.radius && cch.owner.alliance != damager.owner.alliance) {
				cch.destroySelf(doRpcEvenIfNotOwned: true);
				size = 11;
				changeSprite("magnetmine_charged3", true);
				damager.damage = 4;
			}
		}

		if (ownedByLocalPlayer && owner != null) {
			int maxY = 150;
			if (!owner.isDead) {
				if (owner.input.isHeld(Control.Up, owner)) {
					vel.y = Helpers.clampMin(vel.y - Global.spf * 2000, -300);
				}
				if (owner.input.isHeld(Control.Down, owner)) {
					vel.y = Helpers.clampMax(vel.y + Global.spf * 2000, 300);
				}
			}
			if (vel.y != 0) {
				forceNetUpdateNextFrame = true;
			}

			if (pos.y > startY + maxY) {
				pos.y = startY + maxY;
				vel.y = 0;
			}
			if (pos.y < startY - maxY) {
				pos.y = startY - maxY;
				vel.y = 0;
			}
			soundTime += Global.spf;
			if (soundTime == 0.1f) {
				playSound("magnetminechargedtravelX2", forcePlay: false, sendRpc: true);
			}
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) {
			return;
		}
		if (other.gameObject is Projectile proj && !proj.destroyed &&
			(damager.owner.alliance != owner.alliance || damager.owner == owner && size < 10)
		) {
			if (!proj.shouldVortexSuck) { return; }
			if (proj is MagnetMineProj magnetMine && !magnetMine.canBeSucked(damager.owner.alliance)) { return; }

			if (proj.ownedByLocalPlayer && proj is MagnetMineProjCharged mineCharged) {
				if (damager.owner.alliance != mineCharged.damager.owner.alliance &&
					mineCharged.size != 0 && size <= mineCharged.size
				) {
					return;
				}
				if (mineCharged.damager.owner == damager.owner && size < mineCharged.size) {
					return;
				}
				size += 12;
			}
			size += proj.damager.damage;
			proj.destroySelfNoEffect(doRpcEvenIfNotOwned: true);

			if (updateSizeAndDamage()) {
				forceNetUpdateNextFrame = true;
			}
		}
	}

//	public override List<byte> getCustomActorNetData() {
//		List<byte> customData = base.getCustomActorNetData();
//		customData.Add((byte)MathF.Floor(size));
//
//		return customData;
//	}

	public override void updateCustomActorNetData(byte[] data) {
		size = data[0];
		updateSizeAndDamage();
	}

	public bool updateSizeAndDamage() {
		if (size >= 10 && phase < 2) {
			changeSprite("magnetmine_charged3", true);
			damager.damage = 4;
		} else if (size >= 5 && phase < 1) {
			changeSprite("magnetmine_charged2", true);
			damager.damage = 2;
		}
		return false;
	}
}
