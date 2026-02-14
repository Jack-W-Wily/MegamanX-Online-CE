using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public class JunkShield : Weapon {
	public static JunkShield netWeapon = new();
	public static float cooldown = 60;

	public JunkShield() : base() {
		displayName = "JUNK SHIELD";
		shootSounds = new string[] { "", "", "", "" };
		fireRate = cooldown;
		switchCooldown = 45;
		killFeedIndex = 0;
		maxAmmo = 10;
		ammo = maxAmmo;
	
	}


	

	public override void vileShootOld(WeaponIds weaponInput, Vile vile) {
		
			Point shootPos = vile.getShootPos();
			int xDir = vile.getShootXDir();
			Player player = vile.player;
			int input = player.input.getYDir(player);

			
		for (int i = 0; i < 3; i++) {
			new JunkShieldMagnet(
				vile.getCenterPos(), vile.xDir, vile,
				vile.player.getNextActorNetId(), i * 85
			);
		}
	}
}

public class JunkShieldMagnet : Anim {

	Vile vile = null!;
	float timer;
	float startAng;
	float ang;
	float radius;
	bool once;

	public JunkShieldMagnet(
		Point pos, int xDir, Vile character, ushort? netId, float ang
	) : base(
		pos, "junk_shield_magnet", xDir, netId, false, true
	) {
		if (ownedByLocalPlayer) {
			vile = character;
			vile.junkShieldProjs.Add(this);
		}
		
		this.ang = ang;
		startAng = ang;
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer || vile == null) return;

		if (radius < 30) radius += 1;
		ang += 5;

		timer += Global.speedMul;
		if (timer >= 15 && !once && startAng == 0) {
			once = true;
			for (int i = 0; i < 8; i++) {
				new JunkShieldPiece(
					vile.getCenterPos(), vile.xDir, vile,
					vile.player.getNextActorNetId(), i * 32, this
				);
			};
		} else if (timer >= 30) destroySelf();
	}

	public override void postUpdate() {
		base.postUpdate();
		if (vile == null) return;
		
		changePos(vile.getCenterPos().add(Point.createFromByteAngle(ang % 256).times(radius)));
	}

	public override void onDestroy() {
		base.onDestroy();
		if (!ownedByLocalPlayer) return;
		vile.junkShieldProjs.Remove(this);
	}
}

public class JunkShieldPiece : Anim {

	Vile vile = null!;
	Anim magnet;
	float startAng;
	float ang;
	float radius = 120;
	public JunkShieldPiece(
		Point pos, int xDir, Character character, ushort? netId, float ang, Anim magnet
	) : base(
		pos, "junk_shield_pieces", xDir, netId, false, true
	) {
		frameSpeed = 0;
		frameIndex = Helpers.randomRange(0, 3);
		this.vile = character as Vile ?? throw new NullReferenceException();
		vile.junkShieldProjs.Add(this);
		this.magnet = magnet;
		startAng = ang;
		this.ang = ang;
		changePos(vile.getCenterPos().add(Point.createFromByteAngle(ang).times(radius)));
	}

	public override void update() {
		base.update();

		if (magnet == null || magnet.destroyed) {
			destroySelf();
			return;
		}

		if (radius > 30) radius -= 4;
		ang += 5;
		changePos(vile.getCenterPos().add(Point.createFromByteAngle(ang).times(radius)));
	}

	public override void onDestroy() {
		base.onDestroy();
		if (!ownedByLocalPlayer) return;
		
		vile.junkShieldProjs.Remove(this);

		if (startAng != 0) return;

		Point pos = vile.getCenterPos();
		int xDir = vile.xDir;
		Player player = vile.player;

		for (int i = 0; i < 3; i++) {
			//Main pices
			float ang = 85 * i;
			var parent = new JunkShieldProj(vile, pos, xDir, ang, player.getNextActorNetId(), true, player)
			{ frameIndex = 5, isParent = true };

			for (int j = 0; j < 2; j++) {
				//Smol pieces
				float angs = ang + (j * 42.5f);
				bool small = j == 1;
				int frame = small ? Helpers.randomRange(0, 1) : Helpers.randomRange(2, 4);
				if (MathF.Ceiling(angs) % 85 == 0 || angs == 0) angs -= 12;

				var son = new JunkShieldProj(vile, pos, xDir, angs, player.getNextActorNetId(), true, player)
				{ frameIndex = frame, smallestSon = small };

				son.parent = parent;
				parent.sons.Add(son);
			}
		}
	}
}


public class JunkShieldProj : Projectile {

	public JunkShieldProj? parent;
	public bool isParent;
	public List<JunkShieldProj?> sons = new();
	public bool smallestSon;
	bool threw;
	Player? player;
	Vile? vile;
	float ang;
	float radius = 40;
	bool sound;
	
	public Pickup? pickup;

	public JunkShieldProj(
		Actor owner, Point pos, int xDir, float ang,
		ushort? netProjId, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "junk_shield_pieces", netProjId, altPlayer
	) {
		projId = (int)VAVA2ProjIds.JunkShield;

		if (ownedByLocalPlayer) {
			vile = owner as Vile;
			if (vile != null) {
				vile.junkShieldProjs.Add(this);
				changePos(vile.getCenterPos().add(Point.createFromByteAngle(ang).times(radius)));
			}
		}

		damager.damage = 1;
		damager.flinch = 1;
		damager.hitCooldown = 15;

		destroyOnHit = true;
		frameSpeed = 0;

		player = altPlayer;
		this.ang = ang;

		canBeLocal = false;

		if (rpc) {
			byte[] extraArgs = new byte[] { (byte)ang };

			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new JunkShieldProj(
			arg.owner, arg.pos, arg.xDir, arg.extraData[0], 
			arg.netId, altPlayer: arg.player
		);
	}

	public override void update() {
		base.update();

		if (parent != null && parent.destroyed && smallestSon) destroySelf();
		if (threw) return;

		//Non-Local players end here.
		if (!ownedByLocalPlayer || vile == null) return;

		ang += 5;
		changePos(vile.getCenterPos().add(Point.createFromByteAngle(ang % 256).times(radius)));

		if (vile.charState is Die) {
			destroySelfNoEffect();
			return;
		}

		if (player == null) return;

		if ((time >= (Global.speedMul * 50) / 120 && player.input.isPressed(Control.Shoot, player))	) {
			shootProjs();
		}
	}

		public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is Pickup && pickup == null) {
			pickup = other.gameObject as Pickup;
			if (!pickup?.ownedByLocalPlayer == true) {
				pickup?.takeOwnership();
				RPC.clearOwnership.sendRpc(pickup?.netId);
			}
		}
	
	}
	
	
	public override void onDestroy() {
		base.onDestroy();
		if (!ownedByLocalPlayer || vile == null) return;

		vile.junkShieldProjs.Remove(this);
	}

	public void shootProjs() {
		if (vile == null) return;

		if (parent != null && parent.destroyed && !smallestSon) {
			threw = true;
			changePos(vile.getCenterPos());
			shoot(ang + 12);
			playSound("thunder_bolt");
		}
		else if (isParent) {
			threw = true;
			changePos(vile.getCenterPos());
			float a = ang;
			shoot(a);
			playSound("thunder_bolt");

			foreach(var son in sons) {
				if (son == null) continue;
				int i = 1;
				son.threw = true;
				son.changePos(vile.getCenterPos());
				Global.level.delayedActions.Add(new DelayedAction(() => {
					son?.shoot(a);
				},0.12f * i	));
				i++;
			}
		}
	}

	public void shoot(float a) {
		if (vile == null) return;
		if (Helpers.randomRange(0, 1) == 0) {
			new JunkShieldProj2(
			vile, vile.getCenterPos(), xDir, damager.owner.getNextActorNetId(), frameIndex, a, true
		);
		} else {
				new JunkShieldProj2(
			vile, vile.getCenterPos(), -xDir, damager.owner.getNextActorNetId(), frameIndex, a, true
		);
		}

		destroySelf();
	}
}




public class JunkShieldProj2 : Projectile {

	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup? pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.15f;
	public float turnSpeed = 300;
	public float maxAngleDist = 180;
	public float soundCooldown;

	public JunkShieldProj2(
		Actor owner, Point pos, int xDir, ushort? netId,
		int fi, float ang, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "junk_shield_pieces", netId, altPlayer
	) {
		projId = (int)VAVA2ProjIds.JunkShield2;
		maxTime = 0.75f;

		frameIndex = fi;
		frameSpeed = 0;
		damager.damage = 2;
		damager.hitCooldown = 60;

			vel = new Point(350 * xDir, 50);
		

		if (rpc) rpcCreate(pos, owner, ownerPlayer, netId, xDir, new byte[] { (byte)fi, (byte)ang });
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (other.gameObject is Pickup && pickup == null) {
			pickup = other.gameObject as Pickup;
			if (!pickup?.ownedByLocalPlayer == true) {
				pickup?.takeOwnership();
				RPC.clearOwnership.sendRpc(pickup?.netId);
			}
		}

		if (time > returnTime && other.gameObject is Character character && character.player == damager.owner) {
			if (pickup != null) {
				pickup.changePos(character.getCenterPos());
			}
			destroySelf();
			character.addAmmo(8);
		}
	}
	public override void onDestroy() {
		base.onDestroy();
		if (pickup != null) {
			pickup.useGravity = true;
			pickup.collider.isTrigger = false;
		}
	}

	public override void update() {
		base.update();

		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
		
		}


		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			}  else if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} else {
				destroySelf();
			}
		}
	}


	public static Projectile rpcInvoke(ProjParameters arg) {
		return new JunkShieldProj2(
			arg.owner, arg.pos, arg.xDir, arg.netId,
			arg.extraData[0], arg.extraData[1], altPlayer: arg.player
		);
	}
}





