namespace MMXOnline;

public class X3SaberProj : Projectile {
	public X3SaberProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false)
		: base(weapon, pos, xDir, 300f, 4f, player, "zsaber_shot", 0, 0.5f, netProjId, player.ownedByLocalPlayer) {
		reflectable = false;
		fadeSprite = "zsaber_shot_fade";
		fadeOnAutoDestroy = true;
		destroyOnHit = false;
		projId = (int)NewProjIds.X3Saber;
		maxTime = 0.5f;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	/*public override void onHitDamagable(IDamagable damagable)
	{
		if (damagable.canBeDamaged(damager.owner.alliance, damager.owner.id, projId)) {
			if (damagable.projectileCooldown.ContainsKey(projId + "_" + owner.id) &&
				damagable.projectileCooldown[projId + "_" + owner.id] >= damager.hitCooldown
			) {
				new Anim(pos, "x3saberhit", xDir, null, destroyOnEnd: true);
			}
			}
		base.onHitDamagable(damagable);
	}*/
}

public class XTriadMK2 : Projectile {
	public bool electrified;
	public Character character;

	public XTriadMK2(
		Weapon weapon, int type, Point pos, int xDir,
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 2, player, "voltc_proj_triadt_deactivated",
		0, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.TriadThunderBall;
		maxTime = 1.8f;
		collider.wallOnly = true;
		destroyOnHit = false;
		shouldShieldBlock = false;

		Point velDir = new(xDir, 0);
		if (type == 1) {
			velDir.y = -0.5f;
		} else if (type == 2) {
			velDir.y = 0.5f;
		}
		vel = velDir.normalize().times(150f);

		if (player?.character != null && player.character.ownedByLocalPlayer) {
			character = player.character;
		}
	}

	public override void update() {
		base.update();
		if (ownedByLocalPlayer && !electrified && time > 0.6f) {
			stopMoving();
			updateDamager(2, 4);
			changeSprite("voltc_proj_triadt_electricity", resetFrame: true);
			electrified = true;
		}
	}

	public override void onHitWall(CollideData other) {
		base.onHitWall(other);
		if (ownedByLocalPlayer) {
			if (other.isGroundHit() || other.isCeilingHit()) {
				vel.y = 0f;
			} else {
				vel = Point.zero;
			}
		}
	}
}
