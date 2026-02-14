using System;
using System.Collections.Generic;

namespace MMXOnline;

public class HardKnuckleV : Weapon {
	public static HardKnuckleV netWeapon = new();

	public HardKnuckleV() : base() {
		killFeedIndex = 0;

		drawAmmo = false;
	}


	public override float getAmmoUsage(int chargeLevel) {
		return 0;
	}
	
public override void vileShootOld(WeaponIds weaponInput, Vile vile) {
		
			Point shootPos = vile.getShootPos();
			int xDir = vile.getShootXDir();
			Player player = vile.player;
			int input = player.input.getYDir(player);
			int chargeLevel = vile.getChargeLevel();

			new HardKnuckleVProj(vile, shootPos, xDir, player.getNextActorNetId(), true, player);
			vile.playSound("super_adaptor_punch", sendRpc: true);
	

		
	}
}


public class HardKnuckleVProj : Projectile {

	public bool reversed;
	public bool returned;
	Character shooter = null!;
	Player? player;
	Vile vile = null!;
	public float maxReverseTime;
	public float minTime;
	public Actor? target;
	public HardKnuckleV? HardKnuckleV;
	float projSpeed = 140;

	public HardKnuckleVProj(
		Actor owner, Point pos, int xDir, ushort? netProjId, 
		bool rpc = false, Player? altPlayer = null
	) : base(
			pos, xDir, owner, "vilemk2_hardknuckle", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.HardKnuckleV;
		minTime = 0.2f;

		if (ownedByLocalPlayer) {
			vile = owner as Vile ?? throw new NullReferenceException();
			vile.HardKnuckleVProj = this;

			this.player = ownerPlayer;
			shooter = owner as Character ?? throw new NullReferenceException();
		}
		
		maxReverseTime = 0.5f;
		destroyOnHit = false;
		canBeLocal = false;

		vel.x = projSpeed * xDir;
		damager.damage = 3;
		damager.flinch = Global.defFlinch;
		damager.hitCooldown = 30;

		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new HardKnuckleVProj(
			arg.owner, arg.pos, arg.xDir, arg.netId, altPlayer: arg.player
		);
	}


	public override void update() {
		base.update();
		if (!ownedByLocalPlayer || player == null) return;

		if (ownedByLocalPlayer && (shooter == null || shooter.destroyed)) {
			destroySelf("explosion");
			return;
		}
		var targets = Global.level.getTargets(shooter.pos, player.alliance, true);
		foreach (var t in targets) {
			if (shooter.isFacing(t) && MathF.Abs(t.pos.y - shooter.pos.y) < 80) {
				target = t;
				break;
			}
		}

		if (!reversed && target != null) {
			vel = new Point(0, 0);
			if (pos.x > target.pos.x) xDir = -1;
			else xDir = 1;
			Point targetPos = target.getCenterPos();
			move(pos.directionToNorm(targetPos).times(projSpeed));
			if (pos.distanceTo(targetPos) < 5) {
				reversed = true;
			}
		}

		if (!reversed && time > maxReverseTime) reversed = true;

		if (reversed) {
			vel = new Point(0, 0);
			if (pos.x > shooter.pos.x) xDir = -1;
			else xDir = 1;

			Point returnPos = shooter.getCenterPos();
			if (shooter.sprite.name == "rock_rocket_punch") {
				Point poi = shooter.pos;
				var pois = shooter.sprite.getCurrentFrame()?.POIs;
				if (pois != null && pois.Length > 0) {
					poi = pois[0];
				}
				returnPos = shooter.pos.addxy(poi.x * shooter.xDir, poi.y);
			}

			move(pos.directionToNorm(returnPos).times(projSpeed));
			if (pos.distanceTo(returnPos) < 10) {
				returned = true;
				destroySelf();
				Global.playSound("super_adaptor_punch_recover");
				vile.shootAnimTime = MathF.Min(3, vile.shootAnimTime);
			}
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (locallyControlled) {
			reversed = true;
		}
		if (isRunByLocalPlayer()) {
			reversed = true;
			//RPC.actorToggle.sendRpc(netId, RPCActorToggleType.ReverseRocketPunch);
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		if (vile != null && ownedByLocalPlayer) vile.HardKnuckleVProj = null;
	}
}


public class HardKnuckleVState : CharState {

	bool fired;
	Vile vile = null!;

	public HardKnuckleVState() : base("rocket_punch", "rocket_punch", "", "") {
		normalCtrl = true;
		attackCtrl = true;
		airMove = true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
	}

	public override void update() {
		base.update();

		if (character.frameIndex == 0 && !fired && character.ownedByLocalPlayer) {
			fired = true;

			var poi = character.currentFrame.POIs;
			Point? shootPos = character.getFirstPOI();
			if (shootPos != null) new HardKnuckleVProj(
				vile, shootPos.Value, vile.getShootXDir(), player.getNextActorNetId(true), true, player
			);
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		}
	}
}
