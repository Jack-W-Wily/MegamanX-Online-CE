using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
namespace MMXOnline;




public class VileCannonWC : Weapon {
	public string projSprite = "";
	public string fadeSprite = "";
	public float vileAmmoUsage;
	public static VileCannonWC netWeaponFR = new VileCannonWC(VileCannonType.FrontRunner);
	public static VileCannonWC netWeaponLG = new VileCannonWC(VileCannonType.LongshotGizmo);
	public static VileCannonWC netWeaponFB = new VileCannonWC(VileCannonType.FatBoy);

	public VileCannonWC(VileCannonType vileCannonType) : base() {
		index = (int)WeaponIds.FrontRunner;
		weaponBarBaseIndex = 56;
		weaponBarIndex = 56;
		killFeedIndex = 56;
		weaponSlotIndex = 43;
		type = (int)vileCannonType;

		if (vileCannonType == VileCannonType.None) {
			displayName = "None";
			description = new string[] { "Do not equip a cannon." };
			killFeedIndex = 126;
			ammousage = 0;
			vileAmmoUsage = 0;
			fireRate = 0;
			vileWeight = 0;
		} else if (vileCannonType == VileCannonType.FrontRunner) {
			fireRate = 45;
			vileAmmoUsage = 8;
			ammousage = vileAmmoUsage;
			displayName = "Front Runner";
			projSprite = "vile_mk2_proj";
			fadeSprite = "vile_mk2_proj_fade";
			description = new string[] { "This cannon not only offers power,", "but can be aimed up and down." };
			vileWeight = 2;
			effect = "None.";
		} else if (vileCannonType == VileCannonType.FatBoy) {
			fireRate = 45;
			damage = "4";
			vileAmmoUsage = 24;
			ammousage = vileAmmoUsage;
			displayName = "Fat Boy";
			projSprite = "vile_mk2_fb_proj";
			fadeSprite = "vile_mk2_fb_proj_fade";
			killFeedIndex = 90;
			weaponSlotIndex = 61;
			description = new string[] { "The most powerful cannon around,", "it consumes a lot of energy." };
			vileWeight = 3;
			effect = "None.";
		}
		if (vileCannonType == VileCannonType.LongshotGizmo) {
			fireRate = 6;
			damage = "1";
			vileAmmoUsage = 4;
			ammousage = vileAmmoUsage;
			displayName = "Longshot Gizmo";
			projSprite = "vile_mk2_lg_proj";
			fadeSprite = "vile_mk2_lg_proj_fade";
			killFeedIndex = 91;
			weaponSlotIndex = 62;
			description = new string[] { "This cannon fires 5 shots at once,", "but leaves you open to attack." };
			vileWeight = 4;
			effect = "Burst of 5 shots.";
		}
	}

	public override void vavaShoot(WeaponIds weaponInput, Vile vile) {
		if (type == (int)VileCannonType.None) return;

		bool isLongshotGizmo = type == (int)VileCannonType.LongshotGizmo;
		Player player = vile.player;
		float overrideAmmoUsage = (isLongshotGizmo && vile.isVileMK2) ? 6 : vileAmmoUsage;

		if (isLongshotGizmo && vile.longshotGizmoCount > 0) {
			vile.usedAmmoLastFrame = true;
			if (vile.weaponHealAmount == 0) {
				player.vileAmmo -= vileAmmoUsage;
				if (player.vileAmmo < 0) player.vileAmmo = 0;
			}
		} else if (!vile.tryUseVileAmmo(overrideAmmoUsage)) return;

		if (isLongshotGizmo) {
			vile.isShootingLongshotGizmo = true;
		}

		bool gizmoStart = (isLongshotGizmo && vile.charState is not Vava1FrontRunner);
		if (gizmoStart || vile.charState is Idle || vile.charState is Run || vile.charState is Dash || vile.charState is VileMK2GrabState) {
			vile.setVileShootTime(this);
			vile.changeState(new Vava1FrontRunner(isLongshotGizmo, vile.grounded), true);
		} else {
			if (vile.charState is LadderClimb) {
				if (player.input.isHeld(Control.Left, player)) vile.xDir = -1;
				if (player.input.isHeld(Control.Right, player)) vile.xDir = 1;
				vile.changeSpriteFromName("ladder_shoot2", true);
			}

			if (vile.charState is Jump || vile.charState is Fall || vile.charState is WallKick || vile.charState is VileHover || vile.charState is AirDash) {
				vile.setVileShootTime(this);
				if (!Options.main.lockInAirCannon) {
					if (vile.charState is AirDash) {
						vile.changeState(vile.getFallState(), true);
					}
					vile.changeSpriteFromName("cannon_air", true);
					Vava1FrontRunner.shootLogic(vile);
				} else {
					vile.changeState(new Vava1FrontRunner(false, false), true);
				}
			} else {
				vile.setVileShootTime(this);
				Vava1FrontRunner.shootLogic(vile);
			}
		}

		if (isLongshotGizmo) {
			vile.longshotGizmoCount++;
			if (vile.longshotGizmoCount >= 5 || player.vileAmmo <= 3) {
				vile.longshotGizmoCount = 0;
				vile.isShootingLongshotGizmo = false;
			}
		}
	}
}


public class Vava1Stunshot : CharState {
	bool isGizmo;
	private Vile vile = null!;
	public Vava1Stunshot(bool isGizmo, bool grounded) : base(getSprite(isGizmo, grounded)) {
		useDashJumpSpeed = true;
		this.isGizmo = false;
	}

	public static string getSprite(bool isGizmo, bool grounded) {
		if (isGizmo) {
			return grounded ? "idle_gizmo" : "cannon_gizmo_air";
		}
		if (!grounded){
		return "cannon_air";
		}
		return "idle_shoot" ;
	}

	public override void update() {
		base.update();
		if (character.grounded) {
            sprite = "idle_shoot";
			character.changeSpriteFromName("idle_shoot", false);
        }
		if (vile.isShootingLongshotGizmo) {
			if (vile.cannonWeapon.shootCooldown == 0) {
				vile.cannonWeapon.vavaShoot(0, vile);
			}
			if (player.vileAmmo <= 0) {
				vile.isShootingLongshotGizmo = false;
			}
			return;
		}
		//groundCodeWithMove();

		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public static void shootLogic(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) {
			return;
		}
		Point shootVel = vile.getVileShootVel(true);

		var player = vile.player;
		vile.playSound("buster", sendRpc: true);

		string muzzleSprite = "cannon_muzzle";
		//if (vile.cannonWeapon.type == (int)VileCannonType.FatBoy) muzzleSprite += "_fb";
		//if (vile.cannonWeapon.type == (int)VileCannonType.LongshotGizmo) muzzleSprite += "_lg";

		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		if (vile.sprite.name.EndsWith("_grab")) {
			shootPos = vile.getFirstPOIOrDefault("s");
		}

		var muzzle = new Anim(
			shootPos, muzzleSprite, vile.getShootXDir(), player.getNextActorNetId(), true, true, host: vile
		);
		muzzle.angle = new Point(shootVel.x, vile.getShootXDir() * shootVel.y).angle;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}
			new StunShotProj(
					shootPos, vile.xDir, MathF.Round(shootVel.byteAngle), vile, 
					vile.player, vile.player.getNextActorNetId(), rpc: true
				);
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		shootLogic(vile);
		if (!isGizmo && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
			exitOnAirborne = true;
		} else {
			exitOnAirborne = false;
			character.useGravity = false;
			character.stopMoving();
		}
		
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		vile.isShootingLongshotGizmo = false;
		character.useGravity = true;
		if (isGizmo) {
			vile.gizmoCooldown = 0.5f;
		}
	}
}



public class Vava1FrontRunner : CharState {
	bool isGizmo;
	private Vile vile = null!;
	public Vava1FrontRunner(bool isGizmo, bool grounded) : base(getSprite(isGizmo, grounded)) {
		useDashJumpSpeed = true;
		this.isGizmo = false;
	}

	public static string getSprite(bool isGizmo, bool grounded) {
		if (isGizmo) {
			return grounded ? "idle_gizmo" : "cannon_gizmo_air";
		}
		return grounded ? "idle_shoot" : "cannon_air";
	}

	public override void update() {
		base.update();

		if (vile.isShootingLongshotGizmo) {
			if (vile.cannonWeapon.shootCooldown == 0) {
				vile.cannonWeapon.vavaShoot(0, vile);
			}
			if (player.vileAmmo <= 0) {
				vile.isShootingLongshotGizmo = false;
			}
			return;
		}
		//groundCodeWithMove();

		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public static void shootLogic(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) {
			return;
		}
		Point shootVel = vile.getVileShootVel(true);

		var player = vile.player;
		vile.playSound("frontrunner", sendRpc: true);

		string muzzleSprite = "cannon_muzzle";
	//	if (vile.cannonWeapon.type == (int)VileCannonType.FatBoy) muzzleSprite += "_fb";
	//	if (vile.cannonWeapon.type == (int)VileCannonType.LongshotGizmo) muzzleSprite += "_fb";

		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		if (vile.sprite.name.EndsWith("_grab")) {
			shootPos = vile.getFirstPOIOrDefault("s");
		}

		var muzzle = new Anim(
			shootPos, muzzleSprite, vile.getShootXDir(), player.getNextActorNetId(), true, true, host: vile
		);
		muzzle.angle = new Point(shootVel.x, vile.getShootXDir() * shootVel.y).angle;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}
	//	if (vile.cannonWeapon.type == (int)VileCannonType.FrontRunner) {
			new VileCannonProj(
				shootPos, vile.xDir, 0, MathF.Round(shootVel.byteAngle), "vile_mk2_proj",
				vile, player, player.getNextActorNetId(), rpc: true
			);
	/*	}
		else if (vile.cannonWeapon.type == (int)VileCannonType.FatBoy) {
			new VileCannonProj(
				shootPos, vile.xDir, 1, MathF.Round(shootVel.byteAngle), "vile_mk2_fb_proj",
				vile, player, player.getNextActorNetId(), rpc: true
			);
		}
		else if (vile.cannonWeapon.type == (int)VileCannonType.LongshotGizmo) {
			new VileCannonProj(
				shootPos, vile.xDir, 2, MathF.Round(shootVel.byteAngle), "vile_mk2_lg_proj",
				vile, player, player.getNextActorNetId(), rpc: true
			);
		}*/
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();
		shootLogic(vile);
		if (!isGizmo && (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player))) {
			exitOnAirborne = true;
		} else {
			exitOnAirborne = false;
			character.useGravity = false;
			character.stopMoving();
		}
		
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		vile.isShootingLongshotGizmo = false;
		character.useGravity = true;
		if (isGizmo) {
			vile.gizmoCooldown = 0.5f;
		}
	}
}



public class VileCannonProj : Projectile {
	public int type = 0;
	public VileCannonProj(
		Point pos, int xDir, int type, float byteAngle, string sprite,
		Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, sprite , netId, player
	) {
		xScale = xDir;
		maxTime = 0.5f;
		destroyOnHit = true;
		this.type = type;
		if (type == (int)VileCannonType.FrontRunner) {
			weapon = FrontRunner.netWeapon;
			sprite = "vile_mk2_proj";
			fadeSprite = "vile_mk2_proj_fade";
			fadeOnAutoDestroy = true;
			damager.damage = 3;
			damager.flinch = Global.defFlinch;
			projId = (int)ProjIds.FrontRunner;
		} else if (type == (int)VileCannonType.FatBoy) {
			weapon = FatBoy.netWeapon;
			sprite = "vile_mk2_fb_proj";
			fadeSprite = "vile_mk2_fb_proj_fade";
			fadeOnAutoDestroy = true;
			damager.damage = 4;
			damager.flinch = Global.defFlinch;
			projId = (int)ProjIds.FatBoy;
			maxTime = 0.35f;
		} else if (type == (int)VileCannonType.LongshotGizmo) {
			weapon = TridentLine.netWeapon;	
			sprite = "vile_mk2_lg_proj";
			fadeSprite = "vile_mk2_lg_proj_fade";
			fadeOnAutoDestroy = true;	
			damager.damage = 1;
			damager.flinch = Global.defFlinch;
			projId = (int)ProjIds.LongshotGizmo;
		} else if (type == (int)VileCannonType.TridentLine) {
			weapon = TridentLine.netWeapon;	
			sprite = "vava_proj_trident_line";
			fadeSprite = "buster2_fade";
			fadeOnAutoDestroy = true;	
			damager.damage = 2;
			damager.flinch = Global.defFlinch;
			projId = (int)ProjIds.TridentLine;
		}
		byteAngle = byteAngle % 256;
		this.byteAngle = byteAngle;
		vel.x = 300 * Helpers.cosb(byteAngle);
		vel.y = 300 * Helpers.sinb(byteAngle);
		

		if (rpc) {
			List<Byte> extraBytes = new List<Byte> {
			};
			extraBytes.Add((byte)type);
			extraBytes.AddRange(Encoding.ASCII.GetBytes(sprite));
			rpcCreateByteAngle(pos, owner, ownerPlayer, netId, byteAngle, extraBytes.ToArray());

		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		string sprite = Encoding.ASCII.GetString(args.extraData[1..]);
		return new VileCannonProj(
			args.pos, args.xDir, args.extraData[0], args.byteAngle, sprite, args.owner, args.player, args.netId
		);
	}
}

public class Vava1TridentLine : CharState {
	public Vile vile = null!;

	bool first;
	bool second;
	bool third;

	public Vava1TridentLine(bool grounded) : base(getSprite(grounded)) {
		useDashJumpSpeed = true;
		airMove = true;
		canJump = true;
		canStopJump = true;
		airSprite = "cannon_air";
		landSprite = "trident_line";
	}
	public static string getSprite(bool grounded) {
		return grounded ? "trident_line" : "cannon_air";
	}

	public override void update() {
		base.update();
		if (character.sprite.isAnimOver()) {
			character.changeToIdleOrFall();
		}

		if (character.frameIndex == 9 && !first) {
			shootLogic(vile);
			first = true;
		}
		if (character.frameIndex == 12 && !second) {
			shootLogic(vile);
			second = true;
		}
		if (character.frameIndex == 15 && !third) {
			shootLogic(vile);
			third = true;
		}
	}

	public static void shootLogic(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
		bool isMK2 = vile.isVileMK2;
		Point? headPosNullable = vile.getVileMK2StunShotPos();
		if (headPosNullable == null) return;
		Point shootVel = vile.getVileShootVel(true);
		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		int xDir = vile.xDir;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}


		vile.playSound("frontrunner", sendRpc: true);
	
	
		new VileCannonProj(
				shootPos, vile.xDir, 3, MathF.Round(shootVel.byteAngle), "vava_proj_trident_line",
			vile, vile.player, vile.player.getNextActorNetId(), rpc: true
		);


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();

		if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
			exitOnAirborne = true;
		}
	}
}






public class Vava1GizmoDash : CharState {


	public Vava1GizmoDash() : base("gizmo_dash_grab") {
		immuneToWind = true;
		enterSound = "GDash";
	}

	public override void update() {
		base.update();
		character.move(new Point(character.xDir * 250, 0));

	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


	}

    public override void onEnter(CharState oldState) {
        base.onEnter(oldState);
        character.useGravity = false;
	}

    public override void onExit(CharState? newState) {
        base.onExit(newState);
        character.useGravity = true;
        character.slideVel = character.xDir * character.getDashSpeed() * 0.9f;
	}
}



public class GizmoDashHoming : CharState {
	
	bool isDone;
	Character otherChar;
	float moveAmount;
	float maxMoveAmount;
	public GizmoDashHoming(Character otherChar) : 
	base("gizmo_dash_grab"
	) {
		
		this.otherChar = otherChar;
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.grounded = false;
		character.vel.y = 0;
		maxMoveAmount = character.getCenterPos().distanceTo(otherChar.getCenterPos()) * 1.5f;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		player.character.useGravity = true;
	}

	public override void update() {
		base.update();

		

		Point amount = character.getCenterPos().directionToNorm(otherChar.getCenterPos()).times(250);

		character.move(amount);

		moveAmount += amount.magnitude * Global.spf;
		if (moveAmount > maxMoveAmount) {
			character.changeToIdleOrFall();
			return;
		}
	}
}




public class VavaGizmoGrabState : CharState {

    public Vile vile = null!;
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;

	public bool UsedGrabFinisherOnce = false;

	public bool hitONCE = false;

	public VavaGizmoGrabState(Character? victim) : base("gizmo_grab_success") {
		this.victim = victim;
		grabTime = 1;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;


		if (player.input.isPressed(Control.Down, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			sprite = "violentcrusher_grab";
			character.changeSpriteFromNameIfDifferent("violentcrusher_grab", true);
		}

		if (player.input.isPressed(Control.Down, player) && !UsedGrabFinisherOnce) {
			UsedGrabFinisherOnce = true;
			if (character.xDir == 1) {
				character.xDir = -1;
			} else {
				character.xDir = 1;
			}
			sprite = "violentcrusher_grab";
			character.changeSpriteFromNameIfDifferent("violentcrusher_grab", true);
		}

		
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.5f) {
					leechTime = 0;
					character.addHealth(1);
				}
				return;
			}
		}

		if (leechTime > 0.5f) {
			leechTime = 0;
			character.addHealth(0.5f);
			var damager = new Damager(player, 0.5f, 0, 0);
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.SelfDmg);
		}

		if (stateFrames >= 2 && player.input.isR2Pressed(player)) {
			vile.changeState(new CannonAttack(new LongShotGizmo()), true);
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}

		if (character.sprite.name.Contains("violentcrusher_grab") && character.frameIndex == 3) {
			if (!hitONCE) {
				hitONCE = true;
				new MechFrogStompShockwave(new FireWave(),
				victim.pos.addxy(30 * victim.xDir, 0f), victim.xDir, player,
				player.getNextActorNetId(), rpc: true);
				victim.playSound("crash", true);
			}
		}

			if ((character.sprite.name.Contains("deadlift")
		|| character.sprite.name.Contains("violentcrusher_grab")
		|| character.sprite.name.Contains("superkick")
		)
		&& character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}


	}
    
    public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();

		if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
			exitOnAirborne = true;
		}
	}

	public override void onExit(CharState? newState) {
        base.onExit(newState);
        if (newState is not VileMK2GrabState && victim != null) {
            victim.grabInvulnTime = 2;
            victim.stunInvulnTime = 1;
            victim?.releaseGrab(character, true);
        }
    }
}