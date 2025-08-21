using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace MMXOnline;

public class RayGunWC : AxlWeaponWC {
	Character character;
	public static RayGunWC netWeapon = new();
	

	public RayGunWC() {
		shootSounds = ["raygun", ""];
		fireRate = 5;
		altFireRate = 120;
		sprite = "axl_arm_raygun";
		throwIndex = (int)ThrowID.RayGun;
		flashSprite = "empty";
		chargedFlashSprite = "empty";
		index = (int)WeaponIds.RayGun;
		weaponBarBaseIndex = (int)WeaponBarIndex.RayGun;
		weaponSlotIndex = (int)SlotIndex.RGun;
		killFeedIndex = 33;
		canHealAmmo = false;

		maxAmmo = 16;
		ammo = maxAmmo;
		maxSwapCooldown = 60 * 4;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		ushort netId = axl.player.getNextActorNetId();
		ushort netIdEffect = axl.player.getNextActorNetId();
		new RayGunWCProj(axl, pos, byteAngle, netId, sendRpc: true);
		
		new Anim(pos, "x8_axl_rgun_flash", 1, netIdEffect, true, sendRpc: true) {
			byteAngle = byteAngle,
			host = axl
		};
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
	//	Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new VoltTornadoProjWC(this, pos.addxy(-30 * axl.xDir, 25), axl.xDir, axl.player, netId, sendRpc: true);
	}

	public override float getFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 5;
		}
		return fireRate;
	}

	public override float getAltFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 7;
		}
		return altFireRate;
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 1;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return maxAmmo;
	}
}

public class RayGunWCProj : Projectile {
	float len = 0;
	float lenDelay = 0;
	const float maxLen = 50;
	
	public RayGunWCProj(
		Actor owner, Point pos,
		float byteAngle, ushort netProjId,
		bool sendRpc = false, Player? player = null
	) : base(
		pos, 1, owner, "spiralmagnum_proj", netProjId, player
	) {
		fadeSprite = "x8_axl_rgun_flash";
		weapon = RayGunWC.netWeapon;
		projId = (int)ProjIds.RayGunWC;
		damager.damage = 0.5f;

		vel = Point.createFromByteAngle(byteAngle) * 400;
		this.byteAngle = byteAngle;
		maxTime = 0.25f;
		reflectable = true;
		destroyOnHitWall = true;

		if (sendRpc) {
			rpcCreateByteAngle(pos, owner, ownerPlayer, netProjId, byteAngle);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new RayGunWCProj(
			args.owner, args.pos, args.byteAngle, args.netId, player: args.player
		);
	}

	public void updateAngle() {
		angle = vel.angle;
	}

	public override void update() {
		base.update();
		if (lenDelay > 0.01f) {
			len += Global.spf * 300;
			if (len > maxLen) len = maxLen;
		}
		lenDelay += Global.spf;
	}

	public void reflectSide() {
		vel.x *= -1;
		len = 0;
		lenDelay = 0;
		updateAngle();
	}

	public override void onReflect() {
		reflectSide();
		time = 0;
	}

	public override void onDeflect() {
		base.onDeflect();
		len = 0;
		lenDelay = 0;
		updateAngle();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -600));
		}
	}

	public override void render(float x, float y) {
		var normVel = vel.normalize();
		float xOff1 = -(normVel.x * len);
		float yOff1 = -(normVel.y * len);
		float sin = MathF.Sin(Global.time * 42.5f);

		DrawWrappers.DrawLine(
			pos.x + xOff1, pos.y + yOff1, pos.x, pos.y,
			new Color(38, 145, 255), 4 + sin, zIndex - 4, true
		);
		DrawWrappers.DrawLine(
			pos.x + xOff1, pos.y + yOff1, pos.x, pos.y,
			new Color(102, 252, 255), 2 + sin, zIndex - 2, true
		);
		DrawWrappers.DrawLine(
			pos.x + xOff1, pos.y + yOff1, pos.x, pos.y,
			new Color(215, 244, 255), 1 + sin, zIndex, true
		);
	}
}
public class VoltTornadoProjWC : Projectile
{ 
	private Player player;

	public float speedCounter = 10;
	float dustTime;
	Anim dustReverse;

	public VoltTornadoProjWC(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false)
		: base(weapon, pos, xDir, 0f, 0, player, "x8_axl_voltnado", 0, 0.5f, netProjId, player.ownedByLocalPlayer)
	{
		fadeSprite = "x8_axl_voltnado_fade";
		fadeOnAutoDestroy = true;
		//projId = (int)ProjIds.VoltTornado;
	//	Character character = player.character;
		/*
		if (character != null && character.isWhiteAxl())
		{
			projId = (int)ProjIds.VoltTornadoHyper);
		}
		*/
		projId = (int)ProjIds.VoltTornado;

		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		maxTime = 1.7f;
		maxTime = 4f;
		this.player = player;
		isPlatform = true;
	//	platformAllianceOnly = true;
	//	isNonClimbablePlatform = true;
		if (sendRpc)
		{
			rpcCreate(pos, player, netProjId, xDir);
		}
		netcodeOverride = NetcodeModel.FavorDefender;
	}

	public override void onStart()
	{
		base.onStart();
		playSound("voltTornado");
	}

	public override void update()
	{
		base.update();
		if (!ownedByLocalPlayer) return;
		
		if (Global.level.checkCollisionPoint(pos, new List<GameObject>()) != null) {
			useGravity = false;
			vel.y = 0;

			if (Global.level.checkCollisionPoint(pos.addxy(xDir, -3), new List<GameObject>()) != null)  {
				move(new Point(0, -3), false);
			} else if (Global.level.checkCollisionPoint(pos.addxy(xDir, -1), new List<GameObject>()) != null) {
				move(new Point(0, -1), false);
			}
		} else {
			useGravity = true;
		}
		if (time >= 0.3f) {
			vel.x = xDir * speedCounter;
			if (speedCounter < 200) {
				speedCounter += Global.spf * 200;
				if (speedCounter > 200) {
					speedCounter = 200;
			if (speedCounter < 50) {
				speedCounter += Global.spf * 50;
				if (speedCounter > 50) {
					speedCounter = 50;
				}
			}
		} else {
			vel.x = xDir * 10;
		}}}
		dustTime += speedMul;
		if(dustTime > 10){
			dustTime = 0;
			dustReverse = new Anim(pos.addxy(-32 * xDir, 0), "x8_axl_roll_dust", xDir, owner.getNextActorNetId(), true);
			new Anim(pos.addxy(32 * xDir, 0), "x8_axl_roll_dust", -xDir, owner.getNextActorNetId(), true);
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Actor actor) {
			if (!actor.ownedByLocalPlayer) {
				return;
			}
		} else {
			return;
		}
		if (damagable is Character chr) {
			if (!chr.isPushImmune()) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -600));
				float dist = (chr.pos.x - pos.x) * xDir;
				float frontPosDist = 15 * xDir + pos.x - chr.pos.x;
				if (dist <= 15 && dist >= -15) {
					if (frontPosDist * xDir < 8) {
						chr.move(new Point(frontPosDist, 0), false);
					} else {
						chr.move(new Point(xDir * 480f, 0));
					}
				}
			}
		}
	}
}

public class VoltnadoDust : Anim {
	//int type;
	public VoltnadoDust(Point pos, int type, ushort? netId = null, bool sendRpc = false, bool ownedByLocalPlayer = true) :
		base(pos, "dust", 1, netId, false, sendRpc, ownedByLocalPlayer) {
			vel.x = 30;
			vel.y = -100;
		if(type == 1){
			vel.x *= -1;
		}}

	public override void update() {
		base.update();
		if (isAnimOver()) {
			destroySelf();
		}
	}}
	