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


	

	public override void vileShoot(WeaponIds weaponInput, Vile vile) {
		
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
	float radius = 80;
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
	float radius = 30;
	bool sound;

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

		if ((time >= (Global.speedMul * 15) / 60 && player.input.isPressed(Control.Shoot, player))	) {
			shootProjs();
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

		new JunkShieldProj2(
			vile, vile.getCenterPos(), xDir, damager.owner.getNextActorNetId(), frameIndex, a, true
		);

		destroySelf();
	}
}


public class JunkShieldProj2 : Projectile {
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

		vel = Point.createFromByteAngle(ang).times(180);

		if (rpc) rpcCreate(pos, owner, ownerPlayer, netId, xDir, new byte[] { (byte)fi, (byte)ang });
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new JunkShieldProj2(
			arg.owner, arg.pos, arg.xDir, arg.netId,
			arg.extraData[0], arg.extraData[1], altPlayer: arg.player
		);
	}
}
