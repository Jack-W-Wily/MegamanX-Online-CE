namespace MMXOnline;

public class BladeDash : CharState {
	int leftOrRight;
	int upOrDown;
	public BladeDash() : base("ex_bladedash_start", "", "", "") {
		superArmor = true;
		immuneToWind = true;
		invincible = true;
	}

	public override void update() {
		base.update();

		if (sprite == "ex_bladedash_start") {
			if (character.isAnimOver()) {
				
					if (player.input.isHeld(Control.Up, player)) {
					upOrDown = -1;
					sprite = "ex_bladedash_u";
					} else if (player.input.isHeld(Control.Down, player)) {
					upOrDown = 1;
					sprite = "ex_bladedash_d";
					} else {
					leftOrRight = 1;
					sprite = "ex_bladedash_f";
					}
				
			}
			
			character.changeSpriteFromName(sprite, true);
			return;
		}

		if (!character.tryMove(new Point(character.xDir * 350 * leftOrRight, 350 * upOrDown), out _)) {
			player.character.changeState(new Fall(), true);
			return;
		}

		if (character.flag != null) {
			player.character.changeState(new Fall(), true);
			return;
		}
		if (stateTime > 0.6f) {
			player.character.changeState(new Fall(), true);
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		player.character.useGravity = false;
		player.character.vel.y = 0;
		player.character.isDashing = true;
		player.character.useGravity = false;
		player.character.vel = new Point(0, 0);
		player.character.dashedInAir++;
		player.character.globalCollider = character.getDashingCollider();
		//player.character.lastAirDashWasSide = true;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		player.character.yDir = 1;
		player.character.useGravity = true;
	}
}

public class BladeArmorWPSword : CharState {
	bool shot;
	bool isGrounded = false;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;
	
	public BladeArmorWPSword(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;

	//	exitOnLanding = true;
		airMove = true;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		//if (isGrounded){
		return "ex_bladesword";
		//}
		//return "flamethrower";
	}

	public override void update() {
		base.update();
			if (character.grounded){
			isGrounded = true;
			}
			shootTime += Global.spf;
			var poi = character.getFirstPOI();

			if (player.weapon is not FireWave &&
			player.weapon is not SpeedBurner &&
			player.weapon is not ElectricSpark &&
			player.weapon is not AcidBurst &&
			player.weapon is not ShotgunIce
			){
			if (character.grounded) {
				sprite = "ex_bladeattack";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}

			if (!character.grounded) {
				sprite = "ex_bladeattack_air";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
			}
			else if (shootTime > 0.06f && poi != null) {
				
				shootTime = 0;
				
				
				if (player.weapon is FireWave){
					new FireWave().getProjectile(poi.Value, character.getShootXDir(), player, 0, player.getNextActorNetId());
				}
				if (player.weapon is SpeedBurner){
					new SpeedBurner(player).getProjectile(poi.Value, character.getShootXDir(), player, 0, player.getNextActorNetId());
				}
				if (player.weapon is ElectricSpark){
					new ElectricSpark().getProjectile(poi.Value, character.getShootXDir(), player, 0, player.getNextActorNetId());
				}
				if (player.weapon is AcidBurst){
					new AcidBurst().getProjectile(poi.Value, character.getShootXDir(), player, 0, player.getNextActorNetId());
				}
				if (player.weapon is ShotgunIce){
					new ShotgunIce().getProjectile(poi.Value, character.getShootXDir(), player, 0, player.getNextActorNetId());
				}
		}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}


public class XHover : CharState {
	float hoverTime;
	int startXDir;
	bool isFalcon;
	public XHover() : base("hover", "hover_shoot", "", "") {
		airMove = true;
	}

	public override void update() {
		base.update();
		if (player.HasFullFalcon()) isFalcon = true;

		character.xDir = startXDir;
		Point inputDir = player.input.getInputDir(player);
		if (!isFalcon){
		if (inputDir.x == character.xDir) {
			if (!sprite.StartsWith("hover_forward")) {
				sprite = "hover_forward";
				shootSprite = sprite + "_shoot";
				character.changeSpriteFromName(sprite, true);
			}
		} else if (inputDir.x == -character.xDir) {
			if (player.input.isHeld(Control.Jump, player)) {
				if (!sprite.StartsWith("hover_backward")) {
					sprite = "hover_backward";
					shootSprite = sprite + "_shoot";
					character.changeSpriteFromName(sprite, true);
				}
			} else {
				character.xDir = -character.xDir;
				startXDir = character.xDir;
				if (!sprite.StartsWith("hover_forward")) {
					sprite = "hover_forward";
					shootSprite = sprite + "_shoot";
					character.changeSpriteFromName(sprite, true);
				}
			}
		} else {
			if (sprite != "hover") {
				sprite = "hover";
				shootSprite = sprite + "_shoot";
				character.changeSpriteFromName(sprite, true);
			}
		}
		if (character.vel.y < 0) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}

		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}
		}
		if (isFalcon){
			if (sprite != "dash") {
				sprite = "dash";
				shootSprite = sprite + "_shoot";
				character.changeSpriteFromName(sprite, true);
			}

			if (player.input.isHeld(Control.Up, player)){
				character.vel.y = -130;
			}
			if (player.input.isHeld(Control.Down, player)){
				character.vel.y = 130;
			}
			if (!player.input.isHeld(Control.Up, player) && !player.input.isHeld(Control.Down, player)){
				character.vel.y = 0;
			}
			
		}

		

		hoverTime += Global.spf;
		if (!isFalcon && hoverTime > 2 || stateTime > 0.2f && character.player.input.isPressed(Control.Jump, character.player)) {
			character.changeState(new Fall(), true);
		}
		if (isFalcon && hoverTime > 10 || stateTime > 0.2f && character.player.input.isPressed(Control.Jump, character.player)) {
			character.changeState(new Fall(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
		startXDir = character.xDir;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}
