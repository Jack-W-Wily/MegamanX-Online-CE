﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public enum VileCannonType {
	None = -1,
	FrontRunner,
	FatBoy,
	LongshotGizmo
}

public class VileCannon : Weapon {
	public string projSprite;
	public string fadeSprite;
	public float vileAmmoUsage;

	public VileCannon(VileCannonType vileCannonType) : base() {
		index = (int)WeaponIds.FrontRunner;
		weaponBarBaseIndex = 56;
		weaponBarIndex = 56;
		killFeedIndex = 56;
		weaponSlotIndex = 43;
		type = (int)vileCannonType;

		if (vileCannonType == VileCannonType.None) {
			displayName = "None";
			description = new string[] { "Do not equip a cannon." };
			killFeedIndex = 126;
		} else if (vileCannonType == VileCannonType.FrontRunner) {
			rateOfFire = 0.75f;
			vileAmmoUsage = 8;
			displayName = "Front Runner";
			projSprite = "vile_mk2_proj";
			fadeSprite = "vile_mk2_proj_fade";
			description = new string[] { "This cannon not only offers power,", "but can be aimed up and down." };
			vileWeight = 2;
		} else if (vileCannonType == VileCannonType.FatBoy) {
			rateOfFire = 0.75f;
			vileAmmoUsage = 24;
			displayName = "Fat Boy";
			projSprite = "vile_mk2_fb_proj";
			fadeSprite = "vile_mk2_fb_proj_fade";
			killFeedIndex = 90;
			weaponSlotIndex = 61;
			description = new string[] { "The most powerful cannon around,", "it consumes a lot of energy." };
			vileWeight = 3;
		}
		if (vileCannonType == VileCannonType.LongshotGizmo) {
			rateOfFire = 0.1f;
			vileAmmoUsage = 4;
			displayName = "Longshot Gizmo";
			projSprite = "vile_mk2_lg_proj";
			fadeSprite = "vile_mk2_lg_proj_fade";
			killFeedIndex = 91;
			weaponSlotIndex = 62;
			description = new string[] { "This cannon fires 5 shots at once,", "but leaves you open to attack." };
			vileWeight = 4;
		}
	}

	public override void vileShoot(WeaponIds weaponInput, Vile vile) {
		bool isLongshotGizmo = type == (int)VileCannonType.LongshotGizmo;
		if (isLongshotGizmo && vile.gizmoCooldown > 0) return;

		Player player = vile.player;
		if (shootTime > 0 || !player.vileMissileWeapon.isCooldownPercentDone(0.5f)) return;
		if (vile.charState is MissileAttack || vile.charState is RocketPunchAttack) return;
		float overrideAmmoUsage = (isLongshotGizmo && vile.isVileMK2) ? 6 : vileAmmoUsage;

		if (isLongshotGizmo && vile.longshotGizmoCount > 0) {
			vile.usedAmmoLastFrame = true;
			if (vile.weaponHealAmount == 0) {
				player.vileAmmo -= vileAmmoUsage;
				if (player.vileAmmo < 0) player.vileAmmo = 0;
			}
		} else if (!vile.tryUseVileAmmo(overrideAmmoUsage)) return;

		if (isLongshotGizmo) {
			vile.isShootingLongshotGizmo = true;
		}

		bool gizmoStart = (isLongshotGizmo && vile.charState is not CannonAttack);
		if (gizmoStart || vile.charState is Idle || vile.charState is Run || vile.charState is Dash || vile.charState is VileMK2GrabState) {
			vile.setVileShootTime(this);
			vile.changeState(new CannonAttack(isLongshotGizmo, vile.grounded), true);
		} else {
			if (vile.charState is LadderClimb) {
				if (player.input.isHeld(Control.Left, player)) vile.xDir = -1;
				if (player.input.isHeld(Control.Right, player)) vile.xDir = 1;
				vile.changeSpriteFromName("ladder_shoot2", true);
				vile.vileLadderShootCooldown = 0.35f;
			}

			if (vile.charState is Jump || vile.charState is Fall || vile.charState is WallKick || vile.charState is VileHover || vile.charState is AirDash) {
				vile.setVileShootTime(this);
				if (!Options.main.lockInAirCannon) {
					if (vile.charState is AirDash) {
						vile.changeState(new Fall(), true);
					}
					vile.changeSpriteFromName("cannon_air", true);
					CannonAttack.shootLogic(vile);
				} else {
					vile.changeState(new CannonAttack(false, false), true);
				}
			} else {
				vile.setVileShootTime(this);
				CannonAttack.shootLogic(vile);
			}
		}

		if (isLongshotGizmo) {
			vile.longshotGizmoCount++;
			if (vile.longshotGizmoCount >= 5 || player.vileAmmo <= 3) {
				vile.longshotGizmoCount = 0;
				vile.isShootingLongshotGizmo = false;
			}
		}
	}
}

public class VileCannonProj : Projectile {
	public VileCannonProj(VileCannon weapon, Point pos, int xDir, int gizmoNum, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 300, 3, player, weapon.projSprite, 4, 0f, netProjId, player.ownedByLocalPlayer) {
		fadeSprite = weapon.fadeSprite;
		projId = (int)ProjIds.MK2Cannon;
		maxTime = 0.5f;
		destroyOnHit = true;

		if (weapon.type == (int)VileCannonType.FrontRunner) {
			// Nothing.
		} else if (weapon.type == (int)VileCannonType.FatBoy) {
			xScale = xDir;
			damager.damage = 4;
			damager.flinch = Global.defFlinch;
			projId = (int)ProjIds.FatBoy;
			maxTime = 0.35f;
		} else if (weapon.type == (int)VileCannonType.LongshotGizmo) {
			damager.damage = 1;
			/*
			if (player.vileAmmo >= 32 - weapon.vileAmmoUsage) damager.damage = 3;
			else if (player.vileAmmo >= 32 - weapon.vileAmmoUsage * 2) damager.damage = 2;
			else damager.damage = 1;
			*/
			projId = (int)ProjIds.LongshotGizmo;
		}

		if (vel != null) {
			var norm = vel.Value.normalize();
			this.vel.x = norm.x * speed * xDir;
			this.vel.y = norm.y * speed;
		}

		angle = this.vel.angle;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		angle = this.vel.angle;
	}
}

public class CannonAttack : CharState {
	bool isGizmo;
	
	public CannonAttack(bool isGizmo, bool grounded) : base(getSprite(isGizmo, grounded), "", "", "") {
		this.isGizmo = isGizmo;
	}

	public static string getSprite(bool isGizmo, bool grounded) {
		if (isGizmo) {
			return grounded ? "idle_gizmo" : "cannon_gizmo_air";
		}
		return grounded ? "idle_shoot" : "cannon_air";
	}

	public override void update() {
		base.update();

		if (vile.isShootingLongshotGizmo) {
			return;
		}

		//groundCodeWithMove();

		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public static void shootLogic(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) {
			return;
		}
        Point shootVel = vile.getVileShootVel(true);

		var player = vile.player;
		vile.playSound("frontrunner", sendRpc: true);

		string muzzleSprite = "cannon_muzzle";
		if (player.cannonWeapon.type == (int)VileCannonType.FatBoy) muzzleSprite += "_fb";
		if (player.cannonWeapon.type == (int)VileCannonType.LongshotGizmo) muzzleSprite += "_lg";

		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		if (vile.sprite.name.EndsWith("_grab")) {
			shootPos = vile.getFirstPOIOrDefault("s");
		}

		var muzzle = new Anim(shootPos, muzzleSprite, vile.getShootXDir(), player.getNextActorNetId(), true, true, host: vile);
		muzzle.angle = new Point(shootVel.x, vile.getShootXDir() * shootVel.y).angle;

		new VileCannonProj(player.weapons.FirstOrDefault(w => w is VileCannon) as VileCannon, shootPos, vile.getShootXDir(), vile.longshotGizmoCount, player, player.getNextActorNetId(), shootVel, rpc: true);
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		shootLogic(vile);
		character.useGravity = false;
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		vile.isShootingLongshotGizmo = false;
		character.useGravity = true;
		if (isGizmo) {
			vile.gizmoCooldown = 0.5f;
		}
	}
}
