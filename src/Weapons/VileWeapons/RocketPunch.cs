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

		if (rocketPunchType == RocketPunchType.None) {
			displayName = "None";
			description = new string[] { "Do not equip a Rocket Punch." };
			killFeedIndex = 126;
		} else if (rocketPunchType == RocketPunchType.GoGetterRight) {
			rateOfFire = 1f;
			displayName = "Go-Getter Right";
			vileAmmoUsage = 8;
			projSprite = "rocket_punch_proj";
			description = new string[] { "A rocket punch sends your fist", "flying to teach enemies a lesson." };
			vileWeight = 3;
		} else if (rocketPunchType == RocketPunchType.SpoiledBrat) {
			rateOfFire = 0.2f;
			displayName = "Spoiled Brat";
			vileAmmoUsage = 8;
			projSprite = "rocket_punch_sb_proj";
			description = new string[] { "Though lacking in power, this", "rocket punch offers intense speed." };
			killFeedIndex = 77;
			vileWeight = 3;
		}
		if (rocketPunchType == RocketPunchType.InfinityGig) {
			rateOfFire = 1f;
			displayName = "Infinity Gig";
			vileAmmoUsage = 16;
			projSprite = "rocket_punch_ig_proj";
			description = new string[] { "Advanced homing technology can be", "difficult to get a handle on." };
			killFeedIndex = 78;
			vileWeight = 3;
		}
	}

	public override void vileShoot(WeaponIds weaponInput, Vile vile) {
		if (vile.charState is RocketPunchAttack && type != (int)RocketPunchType.SpoiledBrat) return;

		if (shootTime == 0 && vile.charState is not Dash && vile.charState is not AirDash) {
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
	Character shooter;
	Player player;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor target;
	public RocketPunch rocketPunchWeapon;

	public static float getSpeed(int type) {
		if ((int)RocketPunchType.SpoiledBrat == type) return 600;
		else if ((int)RocketPunchType.InfinityGig == type) return 500;
		else return 500;
	}

	public RocketPunchProj(RocketPunch weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, getSpeed(weapon.type), 2, player, weapon.projSprite, 20, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.RocketPunch;
		this.player = player;
		shooter = player.character;
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.2f;
		maxReverseTime = 0.4f;
		if (weapon.type == (int)RocketPunchType.GoGetterRight) {
			maxReverseTime = 0.3f;
		}
		rocketPunchWeapon = weapon;
		if (weapon.type == (int)RocketPunchType.SpoiledBrat) {
			damager.damage = 1;
			damager.hitCooldown = 0.1f;
			maxTime = 0.15f;
			destroyOnHit = true;
			projId = (int)ProjIds.SpoiledBrat;
		} else if (weapon.type == (int)RocketPunchType.InfinityGig) {
			projId = (int)ProjIds.InfinityGig;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		if (projId != (int)ProjIds.RocketPunch) {
			canBeLocal = false;
		}
	}

	public override void update() {
		base.update();
		if (ownedByLocalPlayer && (shooter == null || shooter.destroyed)) {
			destroySelf("explosion", "explosion", true);
			return;
		}
		smokeTime += Global.spf;
		if (smokeTime > 0.08f) {
			smokeTime = 0;
			var smoke = new Anim(pos, "torpedo_smoke", xDir, null, true);
			smoke.setzIndex(zIndex - 100);
		}
		if (!locallyControlled) return;

		if (rocketPunchWeapon.type == (int)RocketPunchType.SpoiledBrat) {
			if (time > maxTime) {
				destroySelf();
			}
			return;
		}
		if (rocketPunchWeapon.type == (int)RocketPunchType.InfinityGig && target == null) {
			if (player.vileRocketPunchWeapon.type == (int)RocketPunchType.InfinityGig) {
				var targets = Global.level.getTargets(shooter.pos, player.alliance, true);
				foreach (var t in targets) {
					if (shooter.isFacing(t) && MathF.Abs(t.pos.y - shooter.pos.y) < 120) {
						target = t;
						break;
					}
				}
			}
			maxReverseTime = 0.4f;
		}
		if (!reversed && target != null) {
			vel = new Point(0, 0);
			if (pos.x > target.pos.x) xDir = -1;
			else xDir = 1;
			Point targetPos = target.getCenterPos();
			move(pos.directionToNorm(targetPos).times(speed));
			if (pos.distanceTo(targetPos) < 5) {
				reversed = true;
			}
		}
		if (!reversed && rocketPunchWeapon.type == (int)RocketPunchType.GoGetterRight) {
			if (player.input.isHeld(Control.Up, player)) {
				incPos(new Point(0, -300 * Global.spf));
			} else if (player.input.isHeld(Control.Down, player)) {
				incPos(new Point(0, 300 * Global.spf));
			}
		}

		if (!reversed && time > maxReverseTime) {
			reversed = true;
		}
		if (reversed) {
			vel = new Point(0, 0);
			if (pos.x > shooter.pos.x) xDir = -1;
			else xDir = 1;

			Point returnPos = shooter.getCenterPos();
			if (shooter.sprite.name == "vile_rocket_punch") {
				Point poi = shooter.pos;
				var pois = shooter.sprite.getCurrentFrame()?.POIs;
				if (pois != null && pois.Count > 0) {
					poi = pois[0];
				}
				returnPos = shooter.pos.addxy(poi.x * shooter.xDir, poi.y);
			}

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
	RocketPunchProj proj;
	float specialPressTime;
	public RocketPunchAttack(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 1) {
			shoot();
		}
		if (proj != null) {
			if (player.vileRocketPunchWeapon.type == (int)RocketPunchType.SpoiledBrat) {
				if (player.input.isPressed(Control.Special1, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 1;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeState(new Idle(), true);
					return;
				}
			} else {
				if (proj.returned || proj.destroyed) {
					character.changeState(new Idle(), true);
					return;
				}
			}
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 1;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(player.vileRocketPunchWeapon, character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}



public class GoGetterRightPunch : CharState {
	bool shot = false;
	RocketPunchProj proj;
	float specialPressTime;

	bool grounded;
	public GoGetterRightPunch(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	this.grounded = grounded;
	airMove = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 1) {
			shoot();
		}
		if (proj != null) {
			if (player.vileRocketPunchWeapon.type == (int)RocketPunchType.SpoiledBrat) {
				if (player.input.isPressed(Control.Special1, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 1;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeState(new Idle(), true);
					return;
				}
			} else {
				if (proj.returned || proj.destroyed) {
					character.changeState(new Idle(), true);
					return;
				}
			}
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 1;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new RocketPunchProj(new RocketPunch(RocketPunchType.GoGetterRight), 
		character.pos.add(poi), character.xDir, character.player, 
		character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}



public class SpoiledBratPunch : CharState {
	bool shot = false;
	RocketPunchProj proj;
	float specialPressTime;
	bool grounded;
	public SpoiledBratPunch(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	this.grounded = grounded;
	airMove = true;
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 1) {
			shoot();
		}
		if (proj != null) {
			if (player.vileRocketPunchWeapon.type == (int)RocketPunchType.SpoiledBrat) {
				if (player.input.isPressed(Control.Special1, player)) {
					specialPressTime = 0.25f;
				}

				if (specialPressTime > 0 && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
					character.frameIndex = 1;
					character.frameTime = 0;
				} else if (character.isAnimOver()) {
					character.changeState(new Idle(), true);
					return;
				}
			} else {
				if (proj.returned || proj.destroyed) {
					character.changeState(new Idle(), true);
					return;
				}
			}
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("rocketPunch", sendRpc: true);
		character.frameIndex = 1;
		character.frameTime = 0;
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


public class Ouroboros : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	private bool fired;

	public Ouroboros(string transitionSprite = "")
		: base("super_shoot", "", "", transitionSprite)
	{

	}

	public override void update()
	{
		
		if (!fired && character.frameIndex > 2){
			new StrikeChain().getProjectile(character.getShootPos(), character.getShootXDir(), player, 0, player.getNextActorNetId());
		fired = true;
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

		base.update();
		Helpers.decrementTime(ref specialPressTime);
		if (stateTime > 0.5f) {
			character.changeToIdleOrFall();
		}
		if (character.isAnimOver()) {
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

