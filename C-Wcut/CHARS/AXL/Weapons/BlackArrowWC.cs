namespace MMXOnline;
public class BlackArrowWC : AxlWeaponWC {
	public BlackArrowWC() {
		shootSounds = [ "blackArrow", "blackArrow", "blackArrow", "blackArrow" ];
		isTwoHanded = true;
		throwIndex = (int)ThrowID.BlackArrow;
		flashSprite = "x8_axl_bullet_flash2";
		chargedFlashSprite ="x8_axl_bullet_cflash2";
		fireRate = 20;
		altFireRate = 24;
		index = (int)WeaponIds.BlackArrow;
		weaponBarBaseIndex = (int)WeaponBarIndex.BlackArrow;
		weaponSlotIndex = (int)SlotIndex.BArrow;
		killFeedIndex = 68;
		sprite = "axl_arm_blackarrow";
		maxAmmo = 8;
		ammo = maxAmmo;
	}

	public override void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		ushort netId = axl.player.getNextActorNetId();
		new BlackArrowProj(this, pos, axl.player, bulletDir, netId, rpc: true);
	}

	public override void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
		Point bulletDir = Point.createFromByteAngle(byteAngle);
		Point bulletDir1 = Point.createFromByteAngle(byteAngle - 22);
		Point bulletDir2 = Point.createFromByteAngle(byteAngle + 22);
		if (ammo >= 6 || ammo < 4) {
			new BlackArrowProj2(this, pos, axl.player, bulletDir1, axl.player.getNextActorNetId(), rpc: true);
		}
		if (ammo >= 4) {
			new BlackArrowProj2(this, pos, axl.player, bulletDir, axl.player.getNextActorNetId(), rpc: true);
			new BlackArrowProj2(this, pos, axl.player, bulletDir2, axl.player.getNextActorNetId(), rpc: true);
		}
	}

	public override void axlUpdate(AxlWC axl, bool isSelected) {
		if (axl.isWhite && maxAmmo < 12) {
			maxAmmo = 12;
			if (!isSelected && swapCooldown <= 0) {
				ammo = maxAmmo;
			}
		}
		else if (!axl.isWhite && maxAmmo > 8) {
			maxAmmo = 8;
			if (ammo > maxAmmo) { ammo = maxAmmo; }
		}
	}

	public override float getFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 12;
		}
		return fireRate;
	}

	public override float getAltFireRate(AxlWC axl, int chargeLevel) {
		if (axl.isWhite) {
			return 16;
		}
		return fireRate;
	}

	public override float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 2;
	}

	public override float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 6;
	}
}
public class Particle : Anim {
	//int type;
	Player player;
	float randPartTime;
	public Particle(Point pos, int type, ushort? netId = null, bool sendRpc = false, bool ownedByLocalPlayer = true) :
		base(pos, "empty", 1, netId, false, sendRpc, ownedByLocalPlayer) {
	}

	public override void update() {
		base.update();
		randPartTime += Global.spf;
			if (randPartTime > 0.025f) {
				randPartTime = 0;
				var partSpawnAngle = Helpers.randomRange(0, 360);
			//	float spawnRadius = maxRadius;
				float spawnSpeed = 300;
				var partSpawnPos = pos.addxy(Helpers.cosd(partSpawnAngle), Helpers.sind(partSpawnAngle));
				var partVel = partSpawnPos.directionToNorm(pos).times(spawnSpeed);
				var partSprite = "gbeetle_proj_flare" + (Helpers.randomRange(0, 1) == 0 ? "2" : "");
				new Anim(partSpawnPos, partSprite, 1, player.getNextActorNetId(), false, sendRpc: true) {
					vel = partVel,
				//	ttl = ((spawnRadius - 10) / spawnSpeed),
				};
			}
	}}
