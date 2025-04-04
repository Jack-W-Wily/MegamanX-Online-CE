using System;

namespace MMXOnline;

public enum RocketPunchType {
	None = -1,
	GoGetterRight,
	SpoiledBrat,
	InfinityGig,
}

public class RocketPunch : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public RocketPunch(RocketPunchType rocketPunchType) : base() {
		index = (int)WeaponIds.RocketPunch;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
		type = (int)rocketPunchType;
		projSprite = "rocket_punch_proj";

		if (rocketPunchType == RocketPunchType.None) {
			displayName = "None";
			description = new string[] { "Do not equip a Rocket Punch." };
			killFeedIndex = 126;
		} else if (rocketPunchType == RocketPunchType.GoGetterRight) {
			fireRate = 60;
			displayName = "Go-Getter Right";
			vileAmmoUsage = 8;
			projSprite = "rocket_punch_proj";
			description = new string[] { "A rocket punch sends your fist", "flying to teach enemies a lesson." };
			vileWeight = 3;
			ammousage = vileAmmoUsage;
			damage = "3";
			hitcooldown = "0.5";
			Flinch = "13";
			FlinchCD = "1";
			effect = "Won't destroy on hit.";
		} else if (rocketPunchType == RocketPunchType.SpoiledBrat) {
			fireRate = 12;
			displayName = "Spoiled Brat";
			vileAmmoUsage = 8;
			projSprite = "rocket_punch_sb_proj";
			description = new string[] { "Though lacking in power, this", "rocket punch offers intense speed." };
			killFeedIndex = 77;
			vileWeight = 3;
			ammousage = vileAmmoUsage;
			damage = "2";
			hitcooldown = "0.1";
			Flinch = "13";
			FlinchCD = "1";
			effect = "Destroys on hit.";
		}
		if (rocketPunchType == RocketPunchType.InfinityGig) {
			fireRate = 60;
			displayName = "Infinity Gig";
			vileAmmoUsage = 16;
			projSprite = "rocket_punch_ig_proj";
			description = new string[] { "Advanced homing technology can be", "difficult to get a handle on." };
			killFeedIndex = 78;
			vileWeight = 3;
			ammousage = vileAmmoUsage;
			damage = "3";
			hitcooldown = "0.5";
			Flinch = "13";
			FlinchCD = "1";
			effect = "Homing,Travels further.";
		}
	}

	public override void vileShoot(WeaponIds weaponInput, Vile vile) {
		if (vile.charState is RocketPunchAttack && type != (int)RocketPunchType.SpoiledBrat) return;

		if (shootCooldown == 0 && vile.charState is not Dash && vile.charState is not AirDash) {
			if (vile.tryUseVileAmmo(vileAmmoUsage)) {
				vile.setVileShootTime(this);
				if (vile.charState is RocketPunchAttack rpa) {
					rpa.shoot();
				} else {
					vile.changeState(new RocketPunchAttack(), true);
				}
			}
		}
	}
}

public class RocketPunchProj : Projectile {
	public bool reversed;
	public bool returned;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor? target;
	int type = 0;

	public RocketPunchProj(
		RocketPunch weapon, Point pos, int xDir, Player player,
		ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, getSpeed(weapon.type), 1,
		player, weapon.projSprite, Global.defFlinch, 0.3f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.RocketPunch;
		destroyOnHit = false;
		isJuggleProjectile = true;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.15f;
		maxReverseTime = 0.3f;
		damager.flinch = Global.halfFlinch;

		if (weapon.type == (int)RocketPunchType.SpoiledBrat) {
			damager.damage = 0.5f;
			//damager.hitCooldown = 6;
			maxTime = 0.15f;
			destroyOnHit = true;
			projId = (int)ProjIds.SpoiledBrat;
			type = 1;
		} else if (weapon.type == (int)RocketPunchType.InfinityGig) {
			projId = (int)ProjIds.InfinityGig;
			type = 2;
		} else {
			maxReverseTime = 0.2f;
			type = 0;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		canBeLocal = false;
	}

	public bool ownerExists => (owner.character?.destroyed == false);

	public override void update() {
		base.update();

		if (time > 2) 	destroySelf("explosion", "explosion");
			
		if (ownedByLocalPlayer && !ownerExists) {
			destroySelf("explosion", "explosion");
			return;
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.08f) {
			smokeTime = 0;
			var smoke = new Anim(pos, "torpedo_smoke", xDir, null, true);
			smoke.setzIndex(zIndex - 100);
		}

		if (ownedByLocalPlayer && !reversed && reflectCount == 0 &&
			type == (int)RocketPunchType.InfinityGig
		) {
			if (target == null && owner.character != null) {
				var targets = Global.level.getTargets(owner.character.pos, damager.owner.alliance, true);
				foreach (var t in targets) {
					if (isFacing(t) && MathF.Abs(t.pos.y - owner.character.pos.y) < 120) {
						target = t;
						break;
					}
				}
			} else if (target != null && target.destroyed) {
				vel.x = getSpeed(type) * xDir;
			} else if (target != null) {
				vel = new Point(0, 0);
				Point targetPos = target.getCenterPos();
				move(pos.directionToNorm(targetPos).times(speed));
				if (pos.distanceTo(targetPos) < 5) {
					reversed = true;
				}
				forceNetUpdateNextFrame = true;
			}
		}
		if (!reversed && type == (int)RocketPunchType.GoGetterRight && damager.owner?.character is Vile vile) {
			if (vile.player.input.isHeld(Control.Up, vile.player)) {
				incPos(new Point(0, -300 * Global.spf));
			} else if (vile.player.input.isHeld(Control.Down, vile.player)) {
				incPos(new Point(0, 300 * Global.spf));
			}
		}
		if (!reversed && time > maxReverseTime) {
			reversed = true;
			vel.x = getSpeed(type) * -xDir;
		}
		if (reversed && owner.character != null) {
			vel = new Point(0, 0);
			if (pos.x > owner.character.pos.x) {
				xDir = -1;
			} else {
				xDir = 1;
			}
			Point returnPos = owner.character.getCenterPos();

			move(pos.directionToNorm(returnPos).times(speed));
			if (pos.distanceTo(returnPos) < 10) {
				returned = true;
				destroySelf();
			}
		}
	}

	/*
	public override void onHitWall(CollideData other) {
		if (!ownedByLocalPlayer) return;
		reversed = true;
	}
	*/

	public static float getSpeed(int type) {
		return type switch {
			(int)RocketPunchType.SpoiledBrat => 600,
			(int)RocketPunchType.InfinityGig => 500,
			_ => 500
		};
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (locallyControlled) {
			reversed = true;
		}
		if (isRunByLocalPlayer()) {
			reversed = true;
			RPC.actorToggle.sendRpc(netId, RPCActorToggleType.ReverseRocketPunch);
		}
	}
}

public class RocketPunchAttack : CharState {
	bool shot = false;
	RocketPunchProj? proj;
	float specialPressTime;
	Vile vile = null!;

		public float pushBackSpeed;


	public RocketPunchAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}

		if (proj == null){
		if (player.input.isPressed(Control.Special1, player)) {
				character.changeToIdleOrFall();
		}
		}
		if (proj != null) {
			if (vile.rocketPunchWeapon.type == (int)RocketPunchType.SpoiledBrat) {
				if (player.input.isPressed(Control.Special1, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 4;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeToIdleOrFall();
					return;
				}
			} else {
				if (proj.returned || proj.destroyed) {
					character.changeToIdleOrFall();
					return;
				}
			}
		}
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

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 3;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(vile.rocketPunchWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}




public class GoGetterRightAttack : CharState {
	bool shot = false;
	RocketPunchProj? proj;
	float specialPressTime;
	Vile vile = null!;


	public float pushBackSpeed;

	public GoGetterRightAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}

	
		
		if (proj != null) {
			if (player.input.isPressed(Control.Special1, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 4;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeToIdleOrFall();
					return;
				}	
				if (proj.returned || proj.destroyed) {
					character.changeToIdleOrFall();
					return;
				}
			
		}
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

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 3;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.GoGetterRight), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


}



public class InfinityGigAttack : CharState {
	bool shot = false;
	RocketPunchProj? proj;
	float specialPressTime;
	Vile vile = null!;


	public float pushBackSpeed;

	public InfinityGigAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 3) {
			shoot();
		}

		
		if (proj != null) {
			if (player.input.isPressed(Control.Special1, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 4;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeToIdleOrFall();
					return;
				}
			
		}
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

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 3;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.InfinityGig), character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}


}




public class SpoiledBratPunch : CharState {
	bool shot = false;
	RocketPunchProj proj;
	float specialPressTime;
	float shootcd;
	bool grounded;

		public float pushBackSpeed;

	public SpoiledBratPunch(string transitionSprite = "") : base("spoiled_brat", "", "", transitionSprite) {
	this.grounded = grounded;
	airMove = true;

	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);
			Helpers.decrementTime(ref shootcd);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (shootcd == 0 && (character.sprite.frameIndex == 1 || character.sprite.frameIndex == 3) ) {
			shoot();
			shootcd = 0.1f;
			player.vileAmmo -= 4;
		}
			if (player.input.isPressed(Control.Shoot, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime == 0 || player.vileAmmo == 0) {
					character.changeState(new Idle(), true);
					return;
				}
			
		
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

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		specialPressTime = 0.25f;
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

		public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
	public void shoot() {
		character.playSound("rocketPunch", sendRpc: true);
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.SpoiledBrat), 
		character.pos.add(poi), character.xDir, character.player, 
		character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}