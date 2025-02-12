using System;
using System.Collections.Generic;

namespace MMXOnline;

public class ElectricSpark : Weapon {
	public static ElectricSpark netWeapon = new();

	public ElectricSpark() : base() {
		index = (int)WeaponIds.ElectricSpark;
		killFeedIndex = 6;
		weaponBarBaseIndex = (int)WeaponBarIndex.ElectricSpark;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = (int)SlotIndex.ESpark;
		weaknessIndex = (int)WeaponIds.ShotgunIce;
		shootSounds = new string[] { "electricSpark", "electricSpark", "electricSpark", "electricSpark" , ""};
		fireRate = 30;
		damage = "2/4";
		effect =  "Can Split. \nCharged: Doesn't destroy on hit.";
		hitcooldown = "0/0.5";
		Flinch = "6/26";
		FlinchCD = "1/0";
		type = index;
		displayName = "Electric Spark ";
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.getShootPos();
		int xDir = character.getShootXDir();
		Player player = character.player;

		if (chargeLevel < 2) {
			new ElectricSparkProj(this, pos, xDir, player, 0, player.getNextActorNetId(), rpc: true);
		}
		if (chargeLevel == 2) {
			new ElectricSparkProjSemiCharged(this, pos, xDir, player, 0, player.getNextActorNetId(), rpc: true);
		}
		if (chargeLevel == 3 || chargeLevel >= 3  && player.hasArmArmor(2)) {
			new ElectricSparkProjChargedStart(this, pos, xDir, player, player.getNextActorNetId(), true);
		}
		if (chargeLevel == 4 && !player.hasArmArmor(2)) {
				character.changeState(new ESparkUltraCharged(character.grounded), true);
		}
	}
}

public class ElectricSparkProj : Projectile {
	public int type = 0;
	public bool split = false;
	public ElectricSparkProj(
		Weapon weapon, Point pos, int xDir, Player player, 
		int type, ushort netProjId, (int x, int y)? vel = null, bool rpc = false
	) : base(
		weapon, pos, xDir, 150, 2, player, "electric_spark",
		Global.miniFlinch, 0, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.ElectricSpark;
		maxTime = 1.2f;

		if (type >= 1) {
			maxTime = 0.4f;

			if (vel != null) base.vel = new Point(vel.Value.x, vel.Value.y);
		}

		fadeSprite = "electric_spark_fade";
		this.type = type;
		reflectable = true;
		shouldShieldBlock = false;
		canBeLocal = false;

		if (rpc) {
			byte[] extraArgs;

			if (vel != null) {
				extraArgs = new byte[] { 
					(byte)type, 
					(byte)(vel.Value.x + 128),
					(byte)(vel.Value.y + 128) };
			} else {
				extraArgs = new byte[] { (byte)type, (byte)(128 + xDir), 128 };
			}

			rpcCreate(pos, player, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new ElectricSparkProj(
			ElectricSpark.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.extraData[0], arg.netId,
			(arg.extraData[1] - 128, arg.extraData[2] - 128)
		);
	}

	public override void onHitWall(CollideData other) {
		if (split) return;
		if (type == 0) {
			destroySelf(fadeSprite);
			split = true;
			if (!ownedByLocalPlayer) {
				return;
			}

			var normal = other?.hitData?.normal;
            if (normal != null) {
                if (normal.Value.x == 0) normal = new Point(-1, 0);
                normal = ((Point)normal).leftNormal();
            }

            Point normal2 =  new Point(0, 1);
			if (normal != null) normal2 = (Point)normal;
            normal2.multiply(speed * 3);

			new ElectricSparkProj(
				weapon, pos.clone(), xDir, damager.owner, 1,
				Global.level.mainPlayer.getNextActorNetId(), ((int)normal2.x, (int)normal2.y), true
			);
			new ElectricSparkProj(
				weapon, pos.clone(), xDir, damager.owner, 2,
				Global.level.mainPlayer.getNextActorNetId(), ((int)normal2.x * -1, (int)normal2.y * -1), rpc: true
			);
		}
	}

	public override void onReflect() {
		vel.y *= -1;
		base.onReflect();
	}
}


public class ElectricSparkProjSemiCharged : Projectile {
	public int type = 0;
	public bool split = false;
	public ElectricSparkProjSemiCharged(
		Weapon weapon, Point pos, int xDir, Player player, 
		int type, ushort netProjId, (int x, int y)? vel = null, bool rpc = false
	) : base(
		weapon, pos, xDir, 150, 3, player, "sparkm_proj_spark",
		Global.defFlinch, 0, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.ElectricSpark;
		maxTime = 2f;

		if (type >= 1) {
			maxTime = 0.4f;

			if (vel != null) base.vel = new Point(vel.Value.x, vel.Value.y);
		}

		fadeSprite = "electric_spark_fade";
		this.type = type;
		reflectable = true;
		shouldShieldBlock = false;
		canBeLocal = false;

		if (rpc) {
			byte[] extraArgs;

			if (vel != null) {
				extraArgs = new byte[] { 
					(byte)type, 
					(byte)(vel.Value.x + 128),
					(byte)(vel.Value.y + 128) };
			} else {
				extraArgs = new byte[] { (byte)type, (byte)(128 + xDir), 128 };
			}

			rpcCreate(pos, player, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new ElectricSparkProjSemiCharged(
			ElectricSpark.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.extraData[0], arg.netId,
			(arg.extraData[1] - 128, arg.extraData[2] - 128)
		);
	}

	public override void onHitWall(CollideData other) {
		if (split) return;
		if (type == 0) {
			destroySelf(fadeSprite);
			split = true;
			if (!ownedByLocalPlayer) {
				return;
			}

			var normal = other?.hitData?.normal;
            if (normal != null) {
                if (normal.Value.x == 0) normal = new Point(-1, 0);
                normal = ((Point)normal).leftNormal();
            }

            Point normal2 =  new Point(0, 1);
			if (normal != null) normal2 = (Point)normal;
            normal2.multiply(speed * 3);

			new ElectricSparkProj(
				weapon, pos.clone(), xDir, damager.owner, 1,
				Global.level.mainPlayer.getNextActorNetId(), ((int)normal2.x, (int)normal2.y), true
			);
			new ElectricSparkProj(
				weapon, pos.clone(), xDir, damager.owner, 2,
				Global.level.mainPlayer.getNextActorNetId(), ((int)normal2.x * -1, (int)normal2.y * -1), rpc: true
			);
		}
	}

	public override void onReflect() {
		vel.y *= -1;
		base.onReflect();
	}
}


public class ElectricSparkProjChargedStart : Projectile {
	public ElectricSparkProjChargedStart(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 4, player, "electric_spark_charge_start",
		Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.ElectricSparkChargedStart;
		destroyOnHit = false;
		shouldShieldBlock = false;

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new ElectricSparkProjChargedStart(
			ElectricSpark.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (sprite.isAnimOver()) {
			destroySelf();
			if (ownedByLocalPlayer) {
				new ElectricSparkProjCharged(
					weapon, pos.addxy(-1, 0), -1, damager.owner,
					damager.owner.getNextActorNetId(true), rpc: true
				);
				new ElectricSparkProjCharged(
					weapon, pos.addxy(1, 0), 1, damager.owner,
					damager.owner.getNextActorNetId(true), rpc: true
				);
			}
		}
	}
}

public class ElectricSparkProjCharged : Projectile {
	public ElectricSparkProjCharged(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 450, 4, player, "electric_spark_charge",
		Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.ElectricSparkCharged;
		maxTime = 0.3f;
		destroyOnHit = false;
		shouldShieldBlock = false;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new ElectricSparkProjCharged(
			ElectricSpark.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.netId
		);
	}
}


public class ESparkUltraCharged : CharState {
	bool fired = false;
	bool groundedOnce;
	public ESparkUltraCharged(bool grounded) : base(!grounded ? "fall" : "punch_ground", "", "", "") {
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

			
			new TriadThunderProjCharged(player.weapon, new Point(x, y), -1, 1, player, player.getNextActorNetId(), rpc: true);
			new TriadThunderProjCharged(player.weapon, new Point(x, y), 1, 1, player, player.getNextActorNetId(), rpc: true);
			new TriadThunderQuake(player.weapon, new Point(x, y), 1, player, player.getNextActorNetId(), rpc: true);

			character.playSound("sparkmSparkX1", forcePlay: false, sendRpc: true);
		
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
