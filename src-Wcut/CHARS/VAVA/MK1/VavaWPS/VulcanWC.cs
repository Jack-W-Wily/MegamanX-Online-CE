namespace MMXOnline;


public class VavaZipZapper : VileState {
	bool isGrounded;
	public float shootTime;
	public Point shootPOI = new Point(-1, -1);
	public Point groundShotPOI = new Point(12, -11);

	public VavaZipZapper() : base("zipzapper") {
		useGravity = false;
		useDashJumpSpeed = true;
		canSpecialCancel = true;
	}

	public override void update() {
		base.update();
		character.turnToInput(player.input, player);
        bool WeaponRightHeld = player.input.isHeld(Control.WeaponRight, player);
		shootTime += Global.speedMul;
		if (shootTime >= 5) {
			player.vileAmmo -= 1;
			shootTime = 0;
			character.playSound("vulcan");
			Point poiPos;
			if (!isGrounded) {
				poiPos = character.getPOIPos(shootPOI);
			} else {
				poiPos = (character.getFirstPOI() ?? character.getPOIPos(groundShotPOI));
			}
		
			new ZipZapperProj(
					character.pos.addxy(21 * character.xDir , -24), character.xDir, isGrounded, vile, player,
					player.getNextActorNetId(), rpc: true
				);
		}

		if (player.vileAmmo <= 0 || !WeaponRightHeld) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
		character.vel = new Point();
	}
}



public class VavaDistantNeedler : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	bool isGrounded;
	public float shootTime;
	public Point shootPOI = new Point(-1, -1);
	public Point groundShotPOI = new Point(12, -11);

    bool once;

	public VavaDistantNeedler(string transitionSprite = "")
		: base("distant_needler", "", "", transitionSprite)
	{
	airMove = true;	
	}

	public override void update()
    {

            
            	shootTime += Global.speedMul;
		if (character.frameIndex == 2 && !once) {
			player.vileAmmo -= 1;
            shootTime = 0;
            once = true;
			character.playSound("bbuffaloBeam");
			Point poiPos;
			if (!isGrounded) {
				poiPos = character.getPOIPos(shootPOI);
			} else {
				poiPos = (character.getFirstPOI() ?? character.getPOIPos(groundShotPOI));
			}
		
			new VulcanDistanceNeedler(
					character.pos.addxy(18 * character.xDir , -33), character.xDir, character,
						player, player.getNextActorNetId(), rpc: true
				);
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

	//	new Anim(character.pos,"iris_crystal_bash_up", character.xDir, player.getNextActorNetId(), true, sendRpc: true	);


	
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}





public class ZipZapperProj : Projectile {
    public bool groundedVariant;
    public ZipZapperProj(
        Point pos, int xDir, bool groundedVariant,
        Actor owner, Player player, ushort? netId, bool rpc = false
    ) : base(
        pos, xDir, owner, "vulcan_zz_proj", netId, player
    ) {
        weapon = WildHorseKick.netWeapon;
        damager.damage = 1;
		damager.flinch = 1;
        damager.hitCooldown = 6;
        maxTime = 0.19f;
        destroyOnHit = true;
        destroyOnHitWall = true;
        this.groundedVariant = groundedVariant;
        angle = vel.angle;
        fadeOnAutoDestroy = true;
        fadeSprite = "flamethrower_dw_fade";
        projId = (int)ProjIds.ZipZapperProj;
        if (!groundedVariant) {
            vel = new Point(100 * xDir, 0);
        } else {
            vel = new Point(350 * xDir, 0);
            maxTime = 0.35f;
        }
        if (rpc) {
            rpcCreate(pos, owner, ownerPlayer, netId, xDir, (byte)(groundedVariant ? 1 : 0));
        }
    }
    public static Projectile rpcInvoke(ProjParameters args) {
        return new ZipZapperProj(
            args.pos, args.xDir, args.extraData[0] == 1,
            args.owner, args.player, args.netId
        );
    }
    public override void update() {
        base.update();

    }
}

