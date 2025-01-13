namespace MMXOnline;

public class EarlyAxl : Character {
	public Anim flashAnim;
	public float magnumCooldown;
	public float hoverTime;
	float shootTime;
	public EarlyAxl(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
		charId = CharIds.EAxl;
	}

	public override void update() {
		base.update();
		Helpers.decrementFrames(ref magnumCooldown);
		if(grounded || charState is WallSlide){hoverTime = 0;}
		// For the shooting animation.
		if (shootAnimTime > 0) {
			shootAnimTime -= Global.speedMul;
			if (shootAnimTime <= 0) {
				shootAnimTime = 0;
				changeSpriteFromName(charState.defaultSprite, false);
				if (charState is WallSlide) {
					frameIndex = sprite.totalFrameNum - 1;
				}
			}
		}
	}
	
	public override bool attackCtrl() {
		if(canShoot() && player.input.isHeld(Control.Shoot, player)){
			shoot();
		}
		if(magnumCooldown == 0 && canShoot() && player.input.isPressed(Control.Special1, player)){
			magnumCooldown = 12;
			changeState(new SpiralMagnumShoot());
		}
		return base.attackCtrl();
	}
	public override bool normalCtrl() {
		if (player.input.isPressed(Control.Jump, player) &&
			canJump() && !grounded && flag == null
		) {
			if(hoverTime <= 1){
			changeState(new AxlEHover(), true);}
			return true;
		}

		/*if (player.canControl && grounded) {
			if (player.input.isPressed(Control.Dash, player)) {
				changeState(new AxlEDodgeRoll(), true);
				return true;
			}
		}*/

		if (player.canControl && grounded) {
			if(charState is Dash && player.input.isPressed(Control.Down, player) ) {
				changeState(new AxlEDodgeRoll(), true);
				return true;
			}
			if (charState is Crouch && player.input.isPressed(Control.Dash, player)) {
				changeState(new AxlEDodgeRoll(), true);
				return true;
			}
		}
		return base.normalCtrl();
	}
	public void shoot() {
		// Cancel non-invincible states.
		if (!charState.attackCtrl && !charState.invincible) {
			changeToIdleOrFall();
		}
		// Shoot anim and vars.
		setShootAnim();
		Point shootPos = getShootPos();
		int xDir = getShootXDir();

		// Shoot stuff.
		shootTime += speedMul;
		if(shootTime > 10f){
			shootTime = 0;
		playSound("buster", sendRpc: true);
		flashAnim = new FlashAnim(shootPos, 0, player.getNextActorNetId(), true);
		flashAnim.xDir = xDir;
		new EarlyAxlProj(
			shootPos, xDir, player, player.getNextActorNetId(), rpc: true
		);}
	}
	public void setShootAnim() {
		string shootSprite = getSprite(charState.shootSprite);
		if (!Global.sprites.ContainsKey(shootSprite)) {
			if (grounded) { shootSprite = "early_axl_shoot"; }
			else { shootSprite = "early_axl_fall_shoot"; }
		}
		if (shootAnimTime == 0) {
			changeSprite(shootSprite, false);
		} else if (charState is Idle) {
			frameIndex = 0;
			frameTime = 0;
		}
		if (charState is LadderClimb) {
			if (player.input.isHeld(Control.Left, player)) {
				this.xDir = -1;
			} else if (player.input.isHeld(Control.Right, player)) {
				this.xDir = 1;
			}
		}
		shootAnimTime = DefaultShootAnimTime;
	}
	public override void render(float x, float y) {
		if (charState is AxlEDodgeRoll) {
			addRenderEffect(RenderEffectType.SpeedDevilTrail);
		} else {
			removeRenderEffect(RenderEffectType.SpeedDevilTrail);
		}
		base.render(x, y);
	}
	public override bool canDash() {
		return true;
	}
	public override bool canShoot() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		return "early_axl_" + spriteName;
	}
}
