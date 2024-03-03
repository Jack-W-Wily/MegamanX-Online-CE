using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public class DynamoBoomerang : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public DynamoBoomerang() : base() {
		index = (int)WeaponIds.RocketPunch;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
			displayName = "None";
			description = new string[] { "Do not equip a Rocket Punch." };
			killFeedIndex = 126;
		
		}
}

public class DynamoBoomerangProj : Projectile {
	public bool reversed;
	public bool returned;
	Character shooter;
	Player player;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor target;
	public DynamoBoomerang DynamoBoomerangWeapon;

	public static float getSpeed(int type) {
		return 500;
	}

	public DynamoBoomerangProj(DynamoBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, getSpeed(weapon.type), 2, player, "dynamo_spinningblade", 20, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DynamoBoomerang;
		this.player = player;
		shooter = player.character;
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.2f;
		maxReverseTime = 0.4f;
		
		DynamoBoomerangWeapon = weapon;
		
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		if (projId != (int)ProjIds.DynamoBoomerang) {
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

			maxReverseTime = 0.4f;
		
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
		if (!reversed) {
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

public class DynamoBoomerangState : CharState {
	bool shot = false;
	DynamoBoomerangProj proj;
	float specialPressTime;
	public DynamoBoomerangState(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
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
			
				if (proj.returned || proj.destroyed) {
					character.changeState(new Idle(), true);
					return;
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
		proj = new DynamoBoomerangProj(new DynamoBoomerang(),
		character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}


