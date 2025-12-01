using System;
using SFML.Graphics;
using System.Collections.Generic;

namespace MMXOnline;




public class VavaVSlashRun : CharState {
	Anim? proj;

	public VavaVSlashRun() : base("slashrun", "", "", "") {
		enterSound = "vileMk5Walk";
		immuneToWind = true;
	}

	public override void update() {
		base.update();

		if (character.ComboTimer > 0){
		normalCtrl = true;
		}
		character.move(new Point(character.xDir * 150, 0));
	   if (character.sprite.name.Contains("slashrun")
	  	&& character.isAnimOver()) {
			sprite = "air_bomb_attack";
			character.changeSpriteFromName("air_bomb_attack", true);	
		}
	  if (character.sprite.name.Contains("air_bomb_attack")
	 	 && character.isAnimOver()) {
			character.changeToIdleOrFall();	
		}


		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = true;
		character.vel.y = 0;
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class VileDodge : CharState {
	public float dashTime = 0;
	public int initialDashDir;
	
	public BanzaiBeetleProj Banzai;

	public VileDodge() : base("roll", "", "") {
		attackCtrl = false;
		normalCtrl = true;
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		
		character.isDashing = true;
		character.burnTime -= 1;
		if (character.burnTime < 0) {
			character.burnTime = 0;
		}

		initialDashDir = character.xDir;
		if (player.input.isHeld(Control.Left, player)) initialDashDir = -1;
		else if (player.input.isHeld(Control.Right, player)) initialDashDir = 1;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		
	}

	public override void update() {
		base.update();


		if (player.input.isPressed(Control.Special1, player)) {
			character.playSound("vileMissile", true);
			character.changeSpriteFromName("banzai_launch", true);	
			sprite = "banzai_launch";
					character.turnToInput(player.input, player);
		}

	


		if (character.sprite.name.Contains("banzai")
		&& character.frameIndex == 4){	
			if (Banzai == null){
			Banzai=	new BanzaiBeetleProj(new VileMK2Grab(), 
			character.pos.addxy(0,-30), character.xDir, player, 
			player.getNextActorNetId(), true);
			}
		}


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
			return;
		}

		if (character.frameIndex >= 4) return;

		dashTime += Global.spf;

		var move = new Point(0, 0);
		move.x = character.getDashSpeed() * initialDashDir;
		character.move(move);
		if (stateTime > 0.1) {
			stateTime = 0;
			//new Anim(this.character.pos.addxy(0, -4), "dust", this.character.xDir, null, true);
		}
	}
}



public class VileStationaryHover : CharState {

	
	Vile vile = null!;
	
	public VileStationaryHover() : base("hover", "") {
		attackCtrl = false;
		normalCtrl = true;
	}

	public override void update() {
		base.update();
		if (player == null) return;

		 if ((!player.input.isHeld(Control.AimAngleUp, player) && stateTime > 0.2f)) {
			character.changeToIdleOrFall();
		}

		character.stopMoving();
	}



	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.stopMoving();
		character.useGravity = true;
	}
}






public class VileChainGrabState : CharState {
	bool fired = false;
	

	public VileChainGrabState() : base("spring_grab", "", "", "") {
		superArmor = true;
	}

	public override void update() {
		base.update();

		
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class VileKick1 : CharState {
	bool fired = false;
	

	public VileKick1() : base("kick", "", "", "") {
	
	}

	public override void update() {
		base.update();

		
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}


public class VileSuperKickState : CharState {
	bool fired = false;
	

	public VileSuperKickState() : base("superkick", "", "", "") {
	
	}

	public override void update() {
		base.update();

		
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

		if (player.input.isHeld(Control.Up, player)){
			character.changeSpriteFromName("superkick_up",true);
		}
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}



public class VilePunch1 : CharState {
	bool fired = false;


	public VilePunch1() : base("punch_1", "", "", "") {

	}

	public override void update() {
		base.update();


		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}




public class VilePunch2 : CharState {
	bool fired = false;
	

	public VilePunch2() : base("punch_2", "", "", "") {
	
	}

	public override void update() {
		base.update();

		
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
	}
}





public class VileResistDeath : CharState {
	public float radius = 200;
	XReviveAnim reviveAnim;
	VAVA2 vile;

	public VileResistDeath() : base("die") {
		invincible = true;
		immuneToWind = true;
	}

	public override void update() {
		base.update();
		if (!character.ownedByLocalPlayer) return;

		if (!once && character.frameIndex >= 1 && sprite == "die") {
			character.playSound("ching", sendRpc: true);
			character.addHealth(player.maxHealth);


			once = true;
			var flash = new Anim(character.pos.addxy(0, -33), "up_flash", character.xDir, player.getNextActorNetId(), true, sendRpc: true);
			flash.grow = true;
		}

		if (character.isAnimOver() || stateTime > 1.2f) {
			character.changeToIdleOrFall();
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		reviveAnim = new XReviveAnim(character.getCenterPos(), player.getNextActorNetId(), sendRpc: true);
		character.playSound("xRevive", sendRpc: true);
		character.alive = true;
		VAVA2? vile = Global.level.mainPlayer.character as VAVA2;
		vile.ResitDeathTimes++;
		player.health = 1;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}






public class VAVA2GrabState : CharState {
	public Character? victim;

	float leechTime = 1;
	public SoundWrapper sound;
	public Point flyVel;
	float flyVelAcc = 500;
	float flyVelMaxSpeed = 200;
	public float fallY;
	Vile vile = null!;

	int AIExecution = 0;

	bool violentcrusherspawn; 
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;

	public VAVA2GrabState(Character? victim) : base("grab") {
		this.victim = victim;
		airMove = true;
		grabTime = VileMK2Grabbed.maxGrabTime;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile ?? throw new NullReferenceException();





		if (vile.vileForm == 2){
	//	character.useGravity = false;
		if (player.speedDevil) {
			flyVelMaxSpeed *= 1.1f;
			flyVelAcc *= 1.1f;
		}

		float flyVelX = 0;
		if (character.isDashing && character.deltaPos.x != 0) {
			flyVelX = character.xDir * character.getDashSpeed() * 0.5f;
		} else if (character.deltaPos.x != 0) {
			flyVelX = character.xDir * character.getRunSpeed() * 0.5f;
		}

		float flyVelY = 0;
		if (character.vel.y < 0) {
			flyVelY = character.vel.y;
		}

		flyVel = new Point(flyVelX, flyVelY);
		if (flyVel.magnitude > flyVelMaxSpeed) flyVel = flyVel.normalize().times(flyVelMaxSpeed);

		if (character.vel.y > 0) {
			fallY = character.vel.y;
		}

		character.isDashing = false;
		character.stopMoving();
		}
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		
			

		if (player.isAI){
		AIExecution = Helpers.randomRange(1,3);
		}
			//if (vile.vileForm == 2){
				if (vile.vileHoverTime > vile.vileMaxHoverTime) {
				vile.vileHoverTime = vile.vileMaxHoverTime;
				character.changeToIdleOrFall();
				return;
				}
				for (int i = 1; i <= 4; i++) {
				CollideData collideData = Global.level.checkTerrainCollisionOnce(character, 0, -10 * i, autoVel: true);
				if (!character.grounded && collideData != null && collideData.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData.isCeilingHit()) {
					if (!violentcrusherspawn){
					character.shakeCamera(sendRpc: true);
					character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
					new DynamoBeam(new ElectricSpark(), victim.pos.addxy(20 * victim.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
					character.playSound("crash", true, true);
					violentcrusherspawn = true;
					}
				}

				
				if (player.input.isPressed(Control.Jump,player)
				|| player.isAI && AIExecution == 2 && character.grounded){
					character.vel.y = -character.getJumpPower();
				}
				
				if (player.input.isHeld(Control.Jump, player) || player.isAI && AIExecution == 2) {
				
				Point moveAmount2 = new Point(character.xDir * 50, -100);
				Point moveAmount = new Point(character.xDir * 50, 100);
				if (!player.input.isHeld(Control.Down, player)){
					character.move(moveAmount2);
					character.useGravity = false;
				} else {
					character.move(moveAmount);
					character.useGravity = true;
				}
				
				} else { character.useGravity = true; }
				if ((base.player.input.isHeld("jump", base.player) || player.isAI && AIExecution == 2 )
				
				&& !once) {
				once = true;
				character.playSound("vileHover", forcePlay: false, sendRpc: true);
				}
			}
		//}

		if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("_grabbed")) {
			character.changeToIdleOrFall();
			return;
		}

		if (victim.sprite.name.EndsWith("_grabbed") || victim.sprite.name.EndsWith("_die")) {
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.5f) {
					leechTime = 0;
					character.addHealth(0.5f);
				}
				return;
			}
		}


		leechTime += Global.spf;
		if (leechTime > 0.4f) {
			leechTime = 0;
			character.addHealth(0.5f);
			var damager = new Damager(player, 1, 0, 0.1f);
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.SelfDmg);
		}

		if (stateFrames >= 2 && player.input.isPressed(Control.Special1, player)) {
			character.changeToIdleOrFall();
			return;
		}


		if (player.input.isHeld(Control.Jump, player) && player.input.isHeld(Control.Down, player)){
		if (!violentcrusherspawn && character.grounded){
			
			character.angle = 180;
			character.shakeCamera(sendRpc: true);
			character.playSound("dynamopillar", forcePlay: false, sendRpc: true);
			new DynamoBeam(new ElectricSpark(), victim.pos.addxy(20 * victim.xDir,0), character.xDir,player, player.getNextActorNetId(), sendRpc: true);
			character.playSound("crash", true, true);
			violentcrusherspawn = true;
			}
		}


		if (player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)
			 || player.isAI && AIExecution == 1) {
			character.changeSpriteFromName("violentcrusher_grab", true);
			sprite = "violentcrusher_grab";
			return;	
		}

		if (character.sprite.name.Contains("violentcrusher_grab") && character.frameIndex == 4
		&& !violentcrusherspawn){
		character.shakeCamera(sendRpc: true);
		character.playSound("crash", true, true);
		violentcrusherspawn = true;
		return;
		}


		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}	
	

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.angle = 0;
	
		if (newState is not VAVA2GrabState && victim != null) {
			victim.grabInvulnTime = 0.5f;
			//victim.stunInvulnTime = 0.5f;
			victim?.releaseGrab(character, true);
		}

		character.useGravity = true;
		character.sprite.restart();
		character.stopMoving();
		if (sound != null && !sound.deleted) {
			sound.sound?.Stop();
		}
		RPC.stopSound.sendRpc("vileHover", character.netId);

	}
}




public class VileAirRaid : CharState {
	public Character? victim;
	public BanzaiBeetleProj Banzai;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public VileAirRaid(Character? victim) : base("air_raid", "", "", "") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		//if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("knocked_down")) {
		//	character.changeToIdleOrFall();
		//	return;
		//}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.33f) {
					leechTime = 0;
				}
				return;
			}
		}

			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();
			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);
			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));

		
	if (!player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {

		
			 if (Helpers.randomRange(0,3) == 1 ){
			character.changeSpriteFromName("punch_2", true);
			sprite = "punch_2";
			
			}
			else if (Helpers.randomRange(0,3) == 2){
			character.changeSpriteFromName("kick", true);
			sprite = "kick";
			
			}
			else if (Helpers.randomRange(0,3) == 3 ){
			character.changeSpriteFromName("kick_2", true);
			sprite = "kick_2";
			
 			} 
			else {
				character.changeSpriteFromName("punch_1", true);
				sprite = "punch_1";
			}
			
		}

			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Shoot, player)) {
			character.changeSpriteFromName("air_bomb_attack", true);	
			sprite = "air_bomb_attack";
		}


			if ( player.input.isHeld(Control.Down, player) 
			&& player.input.isPressed(Control.Special1, player)) {
			character.changeSpriteFromName("banzai_launch", true);	
				character.playSound("vileMissile", true);
				sprite = "banzai_launch";
		}

		if (character.sprite.name.Contains("banzai")
		&& character.frameIndex == 4){	
			if (Banzai == null){
			Banzai=	new BanzaiBeetleProj(new VileMK2Grab(), 
			character.pos, character.xDir, player, 
			player.getNextActorNetId(), true);
			}
		}


		if (character.sprite.name.Contains("air_bomb") && character.isAnimOver()){
			character.changeToIdleOrFall();
		}

		if (player.input.isPressed(Control.Jump, player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;

		victim?.releaseGrab(character);
	}
}




public class VileTeleport : CharState {
	const float warpHeight = 150;
	float origYPos;
	int phase = 0;
	Point summonPos;
	bool isNew;
	public VileTeleport(Point summonPos) : base("warp_beam") {
		this.summonPos = summonPos;
		this.isNew = isNew;
		enterSound = "warpIn";
	}

	public override void update() {
		base.update();
		if (phase == 0) {
			character.incPos(new Point(0, -Global.spf * 450));
			if (character.pos.y < origYPos - warpHeight) {
				character.changePos(summonPos.addxy(0, -warpHeight));
				phase = 1;
			}
		} else if (phase == 1) {
			character.incPos(new Point(0, Global.spf * 450));
			if (character.pos.y >= summonPos.y) {
				character.changeState(new Idle(), true);
			}
		}
	}

	public override void onEnter(CharState? oldState) {
		base.onEnter(oldState);
		character.vel = Point.zero;
		character.useGravity = false;
		origYPos = character.linkedRideArmor.pos.y;

		if (isNew) {
			character.changePos(summonPos.addxy(0, -warpHeight));
			phase = 1;
		}
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}


public class VileStomp : Weapon {
	public VileStomp() : base() {
		fireRate = 0.75f;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}


public class VileStompState : CharState {
	public Character? victim;
	float leechTime = 1;
	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;
	public VileStompState(Character? victim) : base("stomp", "", "", "") {
		this.victim = victim;
		grabTime = 3;
	}

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

		if (victimWasGrabbedSpriteOnce && !victim.sprite.name.EndsWith("knocked_down")) {
			character.changeToIdleOrFall();
			return;
		}

		if (victim.sprite.name.EndsWith("knocked_down") || victim.sprite.name.EndsWith("_die")) {
			// Consider a max timer of 0.5-1 second here before the move just shorts out. Same with other command grabs
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}

		if (character.sprite.name.Contains("stomp")) {
			Point enemyHeadPos = victim.getHeadPos() ?? victim.getCenterPos().addxy(0, -10);
			Point poi = character.getFirstPOIOffsetOnly() ?? new Point();

			Point snapPos = enemyHeadPos.addxy(-poi.x * character.xDir, -poi.y);

			character.changePos(Point.lerp(character.pos, snapPos, 0.25f));


		}

		if (leechTime > 0.10f && character.frameIndex == 2) {
			leechTime = 0;
			if (!character.sprite.name.Contains("mk5")) character.addHealth(0.5f);
			if (character.sprite.name.Contains("mk5")) character.addHealth(0.13f);
			character.shakeCamera(sendRpc: true);
			//	var damager = new Damager(player, 1f, 0, 0);
			//	damager.applyDamage(victim, false, new XUPGrab(), character, (int)ProjIds.UPGrab);
		}


		if (player.input.isPressed(Control.Special1, player)) {
			character.changeToIdleOrFall();
			return;
		}

		if (grabTime <= 0) {
			character.changeToIdleOrFall();
			return;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		victim.grabInvulnTime = 0.5f;
		victim?.releaseGrab(character);
	}
}




public class LockDownMissileStart : Projectile {
	public Character character;

	public LockDownMissileStart(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 300, 1, player, "missile_lockdown_proj_start", 
		Global.defFlinch, 0.75f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.25f;
		projId = (int)ProjIds.LockDownMissileStart;
		destroyOnHit = false;
		character = player.character;
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new LockDownMissileStart(
			LightningWeb.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) {return;}
		if (character.player.input.isR2Pressed(character.player)) {
			destroySelf();
		}
	}
	public override void onDestroy() {
		base.onDestroy();
		if (ownedByLocalPlayer) {
			new LockDownMissileWall(weapon, pos, xDir, base.owner, base.owner.getNextActorNetId(), rpc: true);
		}
	}
}


public class LockDownMissileWall : Projectile {

	Wall wall;
	public LockDownMissileWall(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "missile_lockdown_proj_wall", 
		Global.halfFlinch, 1f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 1f;
		projId = (int)ProjIds.LockDownMissileWall;
		setIndestructableProperties();
		fadeSprite = "explosion";
		fadeOnAutoDestroy = true;
	
		collider.isClimbable = true;
		collider.wallOnly = false;
		isStatic = true;
		
		var rect = collider.shape.getRect().getPoints();
		wall = new Wall("Collision Shape", new List<Point>()
		{
				rect[0].add(new Point(0, 0)),
				rect[1].add(new Point(0, 0)),
				rect[2].add(new Point(0, 0)),
				rect[3].add(new Point(0, 0)),
			});

		Global.level.addGameObject(wall);
		
		if (player.character != null) zIndex = player.character.zIndex - 10;
		
		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new LockDownMissileWall(
			LightningWeb.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}

	public override void onDestroy() {
		base.onDestroy();
		if (wall != null) Global.level.removeGameObject(wall);
	}
}
