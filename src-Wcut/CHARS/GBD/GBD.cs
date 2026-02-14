namespace MMXOnline;
using System;
using System.Collections.Generic;




public class GBD : Character {
	public GBD(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, heartTanks, isATrans
	) {
		charId = CharIds.GBD;
	}



	public GBDSniperAim? GBDAim;

	public float IdlePunchCooldown;
	public float CrouchPunchCooldown;

	public float DodgeCooldown;

	public bool isBike;

	public override bool canDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override bool normalCtrl() {
	
		
		// Blocking
		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded && !player.input.isHeld(Control.Shoot, player) &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT(), true);

		}


		return base.normalCtrl();
	}


	public override bool attackCtrl() {

		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);	
		}
		if (!isBike){
		if (player.input.isAPressed(player)) {
			if (player.input.isHeld(Control.Down, player)) {
			changeState(new VAVAUpperCutPunch(), true);	
			}else {
			changeState(new VAVAJab1(), true);	
			}
		}

		if (player.input.isR2Pressed(player)) {
			changeState(new GBDTonfaCharge(), true);	
		}

		if (player.input.isBPressed(player)) {
			changeState(new GBDSniperState(), true);
			if (GBDAim == null){
				GBDAim = new GBDSniperAim(new IrisCrystal(), pos, getShootXDir(), player, 0,
							player.getNextActorNetId(true), true);	
			}
		}


		} else{
			bikeAttacks();
		}


		return base.attackCtrl();
	}


	public override float getRunSpeed() {
		float runSpeed = 120;
		if (OverDrive) { // this means during OverDrive he gets a speed buff
			runSpeed *= 1.5f;
		}
		return runSpeed * getRunDebuffs();
	}


	public Point getShootVel(bool aimable) {
		Point vel = new Point(1, 0);
		if (!aimable) {
			return vel;
		}

		if (rideArmor != null) {
			if (player.input.isHeld(Control.Up, player)) {
				vel = new Point(1, -0.5f);
			} else {
				vel = new Point(1, 0.5f);
			}
		} else if (charState is VileMK2GrabState) {
			vel = new Point(1, -0.75f);
		} else if (charState is ShoulderCannon or Vava1TridentLine) {
			if (frameIndex == 12) vel = new Point(1, 0.5f);
			if (frameIndex == 15) vel = new Point(1, -0.5f);
		} else if (player.input.isHeld(Control.Up, player)) {
			if (player.input.isHeld(Control.Left, player) || player.input.isHeld(Control.Right, player)) {
				vel = new Point(1, -0.75f);
			} else {
				vel = new Point(1, -3);
			}
		} else if (player.input.isHeld(Control.Down, player) && charState is not Crouch) {
			vel = new Point(1, 0.5f);
		} else if (player.input.isHeld(Control.Down, player) && player.input.isLeftOrRightHeld(player) && charState is Crouch) {
			vel = new Point(1, 0.5f);
		}

		if (charState is RisingSpecterState) {
			vel = new Point(1, -0.75f);
		}

		/*
		if (charState is CutterAttackState)
		{
			vel = new Point(1, -3);
		}
		*/

		return vel;
	}


	public float trailTime;

	public override void update() {
		base.update();
		Helpers.decrementTime(ref trailTime);


		if (player.input.isPressed(Control.Special2, player)) {
			if (!isBike && player.superAmmo > 15) {
			isBike = true;
			} else {
				isBike = false;
			}
		}
		if (isBike){
			
			if (trailTime <= 0) {
			if (OverDrive){
			trailTime = 0.05f;
			new FStagTrailProj(
			 	pos, xDir,
				this ,player, player.getNextActorNetId(), rpc: true
			);
			} else {
				trailTime = 0.5f;
				player.superAmmo -= 1;
					if (player.superAmmo <= 0) {
					isBike = false;
					}
				}
			}
		}
		if ((sprite.name.Contains("pipe") || sprite.name.Contains("gun")
		|| sprite.name.Contains("kick") || sprite.name.Contains("grab")
		) && isAnimOver()) {
			changeSpriteFromName("idle", true);
		}

		if (!ownedByLocalPlayer) {
			return;
		}
	

		// Cooldowns.
		Helpers.decrementTime(ref IdlePunchCooldown);
		Helpers.decrementTime(ref CrouchPunchCooldown);
		Helpers.decrementTime(ref DodgeCooldown);
	}


	public virtual void updateBike() {
			if ((charState is Dash || charState is AirDash)) {
			slideVel = xDir * getDashSpeed() * 0.5f;
		}
	}



	public virtual void bikeAttacks() {
		bool shootPressed = player.input.isPressed(Control.Shoot, player);
		bool specialPressed = player.input.isPressed(Control.Special1, player);
		bool dashPressed = player.input.isPressed(Control.Dash, player);
		if (shootPressed
		&& !player.input.isHeld(Control.Down, player)
		&& !player.input.isHeld(Control.Up, player)
		&& !player.input.isL2Held(player)) {
			if (IdlePunchCooldown == 0) {

				changeSpriteFromName("pipe_slash_3", true);
				IdlePunchCooldown = 0.2f;
			}
		}
		


		if (isBike){
		if (shootPressed && player.input.isHeld(Control.Up, player)) {
			if (IdlePunchCooldown == 0) {

				changeSpriteFromName("pipe_slash", true);
				IdlePunchCooldown = 0.2f;


			}
		}
		if (shootPressed && player.input.isHeld(Control.Down, player)) {
			changeSpriteFromName("pipe_slash_2", true);
		
		}

		if (specialPressed && player.input.isHeld(Control.Up, player)) {
			changeSpriteFromName("kick", true);
			
		}

		if (shootPressed && player.input.isL2Held(player)) {
			changeSpriteFromName("grab_start", true);
			
		}

		if (dashPressed && player.input.isL2Held(player) && DodgeCooldown == 0) {
			invulnTime = -0.3f;
			DodgeCooldown = 0.5f;
		}


		if (specialPressed && CrouchPunchCooldown == 0 && !player.input.isHeld(Control.Down, player)) {
			changeSpriteFromName("gun", true);
			CrouchPunchCooldown = 0.5f;
			playSound("mk2stunshot", sendRpc: true);
			playSound("buster2", sendRpc: true);
			new ZBuster2Proj(
				pos.addxy(20 * xDir, -30), xDir, this, player, player.getNextActorNetId(), rpc: true
			);


		}
		}
	}


	public override void landingCode(bool useSound = true) {
		base.landingCode(useSound);

		if (isBike){
		shakeCamera(sendRpc: true);
		playSound("crash", sendRpc: true);
		}
	}

	public override bool isToughGuyHyperMode() {
		return !isInDamageSprite();
	}


	public virtual bool updateCtrl() {
		if (!ownedByLocalPlayer) {
			return false;
		}
		if (charState.exitOnLanding && grounded) {
			landingCode();
		}
		if (charState.exitOnAirborne && !grounded) {
			changeState(new Fall());
		}


		if (canWallClimb() && !grounded &&
			(charState.airMove && vel.y > 0 || charState is WallSlide) &&
			wallKickTimer <= 0 &&
			player.input.isPressed(Control.Jump, player) &&
			(charState.wallKickLeftWall != null || charState.wallKickRightWall != null)
		) {
			dashedInAir = 0;
			if (player.input.isHeld(Control.Dash, player) &&
				(charState.useDashJumpSpeed || charState is WallSlide)
			) {
				isDashing = true;
				dashedInAir++;
			}
			vel.y = -getJumpPower();
			wallKickDir = 0;
			if (charState.wallKickLeftWall != null) {
				wallKickDir += 1;
			}
			if (charState.wallKickRightWall != null) {
				wallKickDir -= 1;
			}
			if (wallKickDir == 0) {
				if (charState.lastLeftWall != null) {
					wallKickDir += 1;
				}
				if (charState.lastRightWall != null) {
					wallKickDir -= 1;
				}
			}
			if (wallKickDir != 0) {
				xDir = -wallKickDir;
			}
			wallKickTimer = maxWallKickTime;
			changeState(new WallKick(), true);
			var wallSparkPoint = pos.addxy(12 * xDir, 0);
			var rect = new Rect(wallSparkPoint.addxy(-2, -2), wallSparkPoint.addxy(2, 2));
			if (Global.level.checkCollisionShape(rect.getShape(), null) != null) {
				new Anim(wallSparkPoint, "wall_sparks", xDir,
					player.getNextActorNetId(), true, sendRpc: true
				);
			}
			return true;
		}
		if (charState.canStopJump &&
			!grounded && vel.y < 0 &&
			!player.input.isHeld(Control.Jump, player)
		) {
			vel.y = 0;
		}
		if (charState.airMove && !grounded) {
			airMove();
		}
		if (charState.normalCtrl) {
			normalCtrl();
		}
		if (charState.attackCtrl) {
			return attackCtrl();
		}

		return false;
	}


	public override string getSprite(string spriteName) {
		if (isBike){
		return "gbd_b_" + spriteName;
		}
		return "gbd_" + spriteName;
	}

	public override Projectile getProjFromHitbox(Collider hitbox, Point centerPoint) {
		Projectile proj = null;
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new XBuster(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true, isZSaberClang: false, addToLevel: true
			);
		}
		if (sprite.name.Contains("pipe_slash_3")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.UPPunch, player, 3f, 30, isZSaberClang: true, addToLevel: true);
		}

		if (sprite.name.Contains("uppercut")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.TriggerOldFLinch, player, 2f, 30,8, isZSaberClang: true, addToLevel: true, isJuggleProjectile: true);
		}

		if (sprite.name.Contains("jab")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.ZSaber, player, 1f, 14,5, isZSaberClang: true, addToLevel: true);
		}

		if (sprite.name.Contains("tonfa_f")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.ZSaber, player, 2f, 30,8, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("tonfa_charged_f")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.BurensenEND, player, 4f, 0, isZSaberClang: true, addToLevel: true);
		}

		if (sprite.name.Contains("tonfa_u")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.TriggerOldFLinch, player, 2f, 30,8, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("tonfa_charged_u")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.BlockableLaunch, player, 4f, 0, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("tonfa_overhead")) {
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.MechFrogStompShockwave, player, 3f, 0, isZSaberClang: true, addToLevel: true);
		}


		if (sprite.name.Contains("land")) {
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.Rakukojin, player, 2f, 20, 5f, isZSaberClang: true, addToLevel: true);
		}
		if (sprite.name.Contains("kick")) {
			if (isDashing) {
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.MechFrogGroundPound, player, 4f, 20, 5f, isZSaberClang: true, addToLevel: true);
			}
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.MechFrogGroundPound, player, 2f, 20, 5f, isZSaberClang: true, addToLevel: true);
			
		}
		if (sprite.name.Contains("grab")) {
			return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.ForceGrabState, player, 2f, 0, 5f, isZSaberClang: false, addToLevel: true);
		}
		if (sprite.name.Contains("pipe_slash_2")) {
			if (isDashing) {
				return new GenericMeleeProj(new RakukojinWeapon(), centerPoint, ProjIds.MechFrogStompShockwave, player, 3f, 0, isZSaberClang: true, addToLevel: true);
			}
				return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.ForceGrabState, player, 2f, 20, 4f, null, isShield: true, isDeflectShield: true, isZSaberClang: true, addToLevel: true);
		
		}
		if (sprite.name.EndsWith("pipe_slash")) {
			if (isDashing) {
				return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.HeavyPush, player, 3f, 0, 4f, null, isShield: true, isDeflectShield: true, isZSaberClang: true, addToLevel: true);
			}
			return new GenericMeleeProj(new RCXPunch(), centerPoint, ProjIds.ForceGrabState, player, 2f, 20, 4f, null, isShield: true, isDeflectShield: true, isZSaberClang: true, addToLevel: true);
		}
		return proj;
	}


	public override Point getCamCenterPos(bool ignoreZoom = false) {
		if (GBDAim != null) return GBDAim.getCenterPos();
		return base.getCamCenterPos();
	}

	
	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;



		if (player.skinSlot == 1) {
			palette = player.nightmareZeroShader;
		}

		if (palette != null) {
			shaders.Add(palette);
		}
		if (shaders.Count == 0) {
			return baseShaders;
		}
		shaders.AddRange(baseShaders);
		return shaders;
	}

}








public class GBDSniperAim : Projectile {

public float angleDist = 0;

	public float state = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;
	public float maxSpeed = 350;
	public float returnTime = 0.15f;
	public float turnSpeed = 300;
	public float maxAngleDist = 180;
	public float soundCooldown;
	public float yPos;
	public float initTime;
	public Anim? anim;


	public GBDSniperAim(
	Weapon weapon, Point pos, int xDir, Player player, 
		int type, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "gbd_sniper_aim", 
		0, 0, netProjId, player.ownedByLocalPlayer
	) { 
		projId = (int)ProjIds.GBDAim;
		destroyOnHit = false;
		maxAngleDist = 45;
		returnTime = 0;
		damager.damage = 6;
		damager.flinch = 30;
		damager.hitCooldown = 20;
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir, new byte[] { (byte)type });
		}

		canBeLocal = false;
	}
	


	public static Projectile rpcInvoke(ProjParameters arg) {
		return new NewIrisCrystal(
			IrisCrystal.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.extraData[0], arg.netId
		);
	}



	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
	}

	public override void onDestroy() {
		base.onDestroy();
		if (owner.character is GBD gbd && gbd != null) {
		gbd.GBDAim = null;
		}
	}



	

	public override void update() {
		base.update();
		Helpers.decrementTime(ref soundCooldown);
		if (owner.character != null) xDir = owner.character.xDir;
		if (owner.character == null || owner.character.charState is Die) destroySelf();
		if (owner.character == null || !Global.level.gameObjects.Contains(owner.character)){ 
			destroySelf();
			return;
		}

		if (sprite.name.Contains("shot") && isAnimOver()){ 
			changeSprite("gbd_sniper_aim", true);
		}


		if (owner.character.charState is GBDSniperState &&
		owner.input.isHeld(Control.Up, owner) && owner.input.isAPressed(owner) && soundCooldown == 0){
			state = 1;
		}
		if (state == 1){
			if (sprite.name != "gbd_sniper_shot") {
				changeSprite("gbd_sniper_shot", true);
				soundCooldown = 2;
			
				playSound("zing1");
				}

		}

		if (
		owner.character.charState is GBDSniperState &&
		!owner.input.isHeld(Control.Up, owner) && owner.input.isAPressed(owner) && soundCooldown == 0){
			state = 2;		
		}
		if (state == 2){
			if (sprite.name != "gbd_sniper_shot2") {
				changeSprite("gbd_sniper_shot2", true);
				soundCooldown = 2;
				playSound("zing1");
				}
		}

		
		if (owner.character.charState is GBDSniperState ) state = 4;
		if (state == 4) {

		if (owner.input.isHeld(Control.Up, owner)) {
				vel.y = -150;
			} 
			 if (owner.input.isHeld(Control.Down, owner)) {
			    vel.y = 150;
			}
			 if (owner.input.isHeld(Control.Right, owner)) {
				vel.x = 150;
			}
			 if (owner.input.isHeld(Control.Left, owner)) {
				vel.x = -150;
			}
			 if (!owner.input.isHeld(Control.Left, owner)
			    && !owner.input.isHeld(Control.Right, owner)
				&& !owner.input.isHeld(Control.Up, owner)
				&& !owner.input.isHeld(Control.Down, owner)
				) {
				vel.x = 0;
				vel.y = 0;
			}
		}


	
		

		if (owner.character.charState is not GBDSniperState) {
			destroySelf();
		}
		if (state == 0) {
		time += Global.spf;
		if (time > 2) time = 0;
		float x = 20 * MathF.Sin(time * 5);
		yPos = -15 * time;
		Point newPos = owner.character.pos.addxy(x, yPos);
		changePos(newPos);
		}
	}
}










