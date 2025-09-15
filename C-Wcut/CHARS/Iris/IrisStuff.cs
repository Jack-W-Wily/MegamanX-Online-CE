using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;


namespace MMXOnline;





public class IrisCrystalBashState : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public IrisCrystalBashState(string transitionSprite = "")
		: base("attack", "", "", transitionSprite)
	{
	airMove = true;
	}

	public override void update()
	{
		
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

	//	new Anim(character.pos,"iris_crystal_bash", character.xDir, player.getNextActorNetId(),true, sendRpc: true	);


		character.playSound("dynamoslash", sendRpc: true);
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
	


public class IrisCrystalRisingBash : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public IrisCrystalRisingBash(string transitionSprite = "")
		: base("attack_rising", "", "", transitionSprite)
	{
	airMove = true;	
	}

	public override void update()
	{

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


		character.playSound("dynamoslash", sendRpc: true);
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



public class IrisBash3 : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public IrisBash3(string transitionSprite = "")
		: base("string_3", "", "", transitionSprite) {
		airMove = true;
	}

	public override void update() {

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
		character.playSound("dynamoslash", sendRpc: true);
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




public class IrisGrabStart : CharState {


	private float specialPressTime;

	public float pushBackSpeed;

	public IrisGrabStart(string transitionSprite = "")
		: base("grab_start", "", "", transitionSprite) {
		airMove = true;
	}

	public override void update() {

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


public class IrisGrabEX : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public IrisGrabEX(string transitionSprite = "")
		: base("grab_ex", "", "", transitionSprite)
	{
	airMove = true;
	}

	public override void update()
	{
	
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
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("distortion_a", sendRpc: true);
		if (!character.grounded) {
			character.stopMoving();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		(character as Iris).GrabVictim = null;
    }
}




public class IrisCrystalCharge : CharState {
	public float dashTime;

	public Projectile fSplasherProj;



	public IrisCrystalCharge()
		: base("chargegp", "") {

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.playSound("distortion_b", true);
		character.stopMoving();
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

	public override void update() {
		base.update();

		if (character.frameIndex < 4 && !player.input.isBHeld(player)) {
			character.changeState(new Idle(), forceChange: true);
		}
		if (character.frameIndex > 3 && !player.input.isBHeld(player)
		&& !player.input.isHeld(Control.Up, player)
		&& !player.input.isHeld(Control.Down, player)
		) {
			character.changeState(new IrisSpawnIce(), forceChange: true);
		}
		if (character.frameIndex > 3 && !player.input.isBHeld(player)
		&& player.input.isHeld(Control.Up, player)) {
			character.changeState(new IrisSpawnBeam(), forceChange: true);
		}
		if (character.frameIndex > 3 && !player.input.isBHeld(player)
		&& player.input.isHeld(Control.Down, player)) {
			character.changeState(new IrisSpawnFire(), forceChange: true);
		}
	}
}




public class IrisSpawnBeam : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	private bool fired;

	public IrisSpawnBeam()
		: base("spawn_lightbeam", "")
	{
			specialId = SpecialStateIds.AxlRoll;
	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;
	}

	public override void onExit(CharState newState)
	{
		base.onExit(newState);
		character.useGravity = true;
	}



	public override void update()
	{
		base.update();
		superArmor = true;
		if (character.frameIndex == 3 && !fired){
		fired = true;
		 TriadThunder weapon = new TriadThunder();
			if ((character as Iris).irisCrystal != null) {
				character.playSound("irislaser2", forcePlay: false, sendRpc: true);
				new IrisLaserProjUp((character as Iris).irisCrystal.pos, character.xDir, player.character, player,
						player.getNextActorNetId(), rpc: true
				);
			}
		}
		
	

		if (character.isAnimOver()) {
			character.changeState(new Idle(), forceChange: true);
		}
	}
}




public class IrisLaserProjDiagonal : Projectile {
	public Point destPos;
	public float sinDampTime = 1;
	public Anim muzzle;
	public IrisLaserProjDiagonal(
		Point poi, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		poi, xDir, owner, "empty", netId, player
	) {
		weapon = VileLaser.netWeaponRS;
		damager.damage = 6;
		damager.flinch = Global.defFlinch;
		damager.hitCooldown = 30;
		maxTime = 0.5f;
		destroyOnHit = false;
		shouldShieldBlock = false;
		vel = new Point();
		projId = (int)ProjIds.IrisLaserProjDiagonal;
		shouldVortexSuck = false;
		float destX = xDir * 150;
		float destY = -100;
		Point toDestPos = new Point(destX, destY);
		pos = poi.addxy(destX * 0.0225f, destY * 0.0225f);
		destPos = pos.add(toDestPos);

		muzzle = new Anim(poi, "risingspecter_muzzle", xDir, null, false, host: player.character) {
			angle = xDir == 1 ? toDestPos.angle : toDestPos.angle + 180
		};

		float ang = poi.directionTo(destPos).angle;
		var points = new List<Point>();
		if (xDir == 1) {
			float sideY = 30 * Helpers.cosd(ang);
			float sideX = -30 * Helpers.sind(ang);
			points.Add(new Point(poi.x - sideX, poi.y - sideY));
			points.Add(new Point(destPos.x - sideX, destPos.y - sideY));
			points.Add(new Point(destPos.x + sideX, destPos.y + sideY));
			points.Add(new Point(poi.x + sideX, poi.y + sideY));
		} else {
			float sideY = 30 * Helpers.cosd(ang);
			float sideX = 30 * Helpers.sind(ang);
			points.Add(new Point(destPos.x - sideX, destPos.y + sideY));
			points.Add(new Point(destPos.x + sideX, destPos.y - sideY));
			points.Add(new Point(poi.x + sideX, poi.y - sideY));
			points.Add(new Point(poi.x - sideX, poi.y + sideY));
		}

		globalCollider = new Collider(points, true, null!, false, false, 0, Point.zero);

		if (rpc) {
			rpcCreate(poi, owner, ownerPlayer, netId, xDir);
		}
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new IrisLaserProjDiagonal(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void onDestroy() {
		base.onDestroy();
		muzzle?.destroySelf();
	}

	public override void update() {
		base.update();
		/*
		if (muzzle != null)
		{
			incPos(muzzle.deltaPos);
			destPos = destPos.add(muzzle.deltaPos);
		}
		*/
	}

	public override void render(float x, float y) {
		base.render(x, y);

		var col1 = new Color(116, 11, 237, 128);
		var col2 = new Color(250, 62, 244, 192);
		var col3 = new Color(240, 240, 240, 255);

		float sin = MathF.Sin(Global.time * 100);
		float sinDamp = Helpers.clamp01(1 - (time / maxTime));

		var dirTo = pos.directionToNorm(destPos);
		float jutX = dirTo.x;
		float jutY = dirTo.y;

		DrawWrappers.DrawLine(pos.x, pos.y, destPos.x, destPos.y, col1, (30 + sin * 15) * sinDamp, 0, true);
		DrawWrappers.DrawLine(
			pos.x - jutX * 2, pos.y - jutY * 2,
			destPos.x + jutX * 2, destPos.y + jutY * 2,
			col2, (20 + sin * 10) * sinDamp, 0, true
		);
		DrawWrappers.DrawLine(
			pos.x - jutX * 4, pos.y - jutY * 4,
			destPos.x + jutX * 4, destPos.y + jutY * 4,
			col3, (10 + sin * 5) * sinDamp, 0, true
		);
	}
}


public class IrisLaserProjUp : Projectile {
	public Point destPos;
	public float sinDampTime = 1;
	public Anim muzzle;
	public IrisLaserProjUp(
		Point poi, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		poi, xDir, owner, "empty", netId, player
	) {
		destroyOnDMG = true;
		weapon = VileLaser.netWeaponRS;
		damager.damage = 6;
		damager.flinch = Global.defFlinch;
		damager.hitCooldown = 30;
		maxTime = 0.5f;
		destroyOnHit = false;
		shouldShieldBlock = false;
		vel = new Point();
		projId = (int)ProjIds.IrisLaserProjUp;
		shouldVortexSuck = false;
		float destX = xDir * 0;
		float destY = -100;
		Point toDestPos = new Point(destX, destY);
		pos = poi.addxy(destX * 0.0225f, destY * 0.0225f);
		destPos = pos.add(toDestPos);

		muzzle = new Anim(poi, "risingspecter_muzzle", xDir, null, false, host: player.character) {
			angle = xDir == 1 ? toDestPos.angle : toDestPos.angle + 180
		};

		float ang = poi.directionTo(destPos).angle;
		var points = new List<Point>();
		if (xDir == 1) {
			float sideY = 90 * Helpers.cosd(ang);
			float sideX = -90 * Helpers.sind(ang);
			points.Add(new Point(poi.x - sideX, poi.y - sideY));
			points.Add(new Point(destPos.x - sideX, destPos.y - sideY));
			points.Add(new Point(destPos.x + sideX, destPos.y + sideY));
			points.Add(new Point(poi.x + sideX, poi.y + sideY));
		} else {
			float sideY = 90 * Helpers.cosd(ang);
			float sideX = 90 * Helpers.sind(ang);
			points.Add(new Point(destPos.x - sideX, destPos.y + sideY));
			points.Add(new Point(destPos.x + sideX, destPos.y - sideY));
			points.Add(new Point(poi.x + sideX, poi.y - sideY));
			points.Add(new Point(poi.x - sideX, poi.y + sideY));
		}

		globalCollider = new Collider(points, true, null!, false, false, 0, Point.zero);

		if (rpc) {
			rpcCreate(poi, owner, ownerPlayer, netId, xDir);
		}
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new IrisLaserProjUp(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void onDestroy() {
		base.onDestroy();
		muzzle?.destroySelf();
	}

	public override void update() {
		base.update();
		/*
		if (muzzle != null)
		{
			incPos(muzzle.deltaPos);
			destPos = destPos.add(muzzle.deltaPos);
		}
		*/
	}

	public override void render(float x, float y) {
		base.render(x, y);

		var col1 = new Color(116, 11, 237, 128);
		var col2 = new Color(250, 62, 244, 192);
		var col3 = new Color(240, 240, 240, 255);

		float sin = MathF.Sin(Global.time * 100);
		float sinDamp = Helpers.clamp01(1 - (time / maxTime));

		var dirTo = pos.directionToNorm(destPos);
		float jutX = dirTo.x;
		float jutY = dirTo.y;

		DrawWrappers.DrawLine(pos.x, pos.y, destPos.x, destPos.y, col1, (30 + sin * 15) * sinDamp, 0, true);
		DrawWrappers.DrawLine(
			pos.x - jutX * 2, pos.y - jutY * 2,
			destPos.x + jutX * 2, destPos.y + jutY * 2,
			col2, (20 + sin * 10) * sinDamp, 0, true
		);
		DrawWrappers.DrawLine(
			pos.x - jutX * 4, pos.y - jutY * 4,
			destPos.x + jutX * 4, destPos.y + jutY * 4,
			col3, (10 + sin * 5) * sinDamp, 0, true
		);
	}
}



public class IrisLaserProjFoward : Projectile {
	public Point destPos;
	public float sinDampTime = 1;
	public Anim muzzle;
	public IrisLaserProjFoward(
		Point poi, int xDir, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		poi, xDir, owner, "empty", netId, player
	) {
		destroyOnDMG = true;
		weapon = VileLaser.netWeaponRS;
		damager.damage = 6;
		damager.flinch = Global.defFlinch;
		damager.hitCooldown = 30;
		maxTime = 0.5f;
		destroyOnHit = false;
		shouldShieldBlock = false;
		vel = new Point();
		projId = (int)ProjIds.IrisLaserProjFoward;
		shouldVortexSuck = false;
		float destX = xDir * 150;
		float destY = 0;
		Point toDestPos = new Point(destX, destY);
		pos = poi.addxy(destX * 0.0225f, destY * 0.0225f);
		destPos = pos.add(toDestPos);

		muzzle = new Anim(poi, "risingspecter_muzzle", xDir, null, false, host: player.character) {
			angle = xDir == 1 ? toDestPos.angle : toDestPos.angle + 180
		};

		float ang = poi.directionTo(destPos).angle;
		var points = new List<Point>();
		if (xDir == 1) {
			float sideY = 0 * Helpers.cosd(ang);
			float sideX = 0 * Helpers.sind(ang);
			points.Add(new Point(poi.x - sideX, poi.y - sideY));
			points.Add(new Point(destPos.x - sideX, destPos.y - sideY));
			points.Add(new Point(destPos.x + sideX, destPos.y + sideY));
			points.Add(new Point(poi.x + sideX, poi.y + sideY));
		} else {
			float sideY = 0 * Helpers.cosd(ang);
			float sideX = 0 * Helpers.sind(ang);
			points.Add(new Point(destPos.x - sideX, destPos.y + sideY));
			points.Add(new Point(destPos.x + sideX, destPos.y - sideY));
			points.Add(new Point(poi.x + sideX, poi.y - sideY));
			points.Add(new Point(poi.x - sideX, poi.y + sideY));
		}

		globalCollider = new Collider(points, true, null!, false, false, 0, Point.zero);

		if (rpc) {
			rpcCreate(poi, owner, ownerPlayer, netId, xDir);
		}
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new IrisLaserProjFoward(
			args.pos, args.xDir, args.owner, args.player, args.netId
		);
	}

	public override void onDestroy() {
		base.onDestroy();
		muzzle?.destroySelf();
	}

	public override void update() {
		base.update();
		/*
		if (muzzle != null)
		{
			incPos(muzzle.deltaPos);
			destPos = destPos.add(muzzle.deltaPos);
		}
		*/
	}

	public override void render(float x, float y) {
		base.render(x, y);

		var col1 = new Color(116, 11, 237, 128);
		var col2 = new Color(250, 62, 244, 192);
		var col3 = new Color(240, 240, 240, 255);

		float sin = MathF.Sin(Global.time * 100);
		float sinDamp = Helpers.clamp01(1 - (time / maxTime));

		var dirTo = pos.directionToNorm(destPos);
		float jutX = dirTo.x;
		float jutY = dirTo.y;

		DrawWrappers.DrawLine(pos.x, pos.y, destPos.x, destPos.y, col1, (30 + sin * 15) * sinDamp, 0, true);
		DrawWrappers.DrawLine(
			pos.x - jutX * 2, pos.y - jutY * 2,
			destPos.x + jutX * 2, destPos.y + jutY * 2,
			col2, (20 + sin * 10) * sinDamp, 0, true
		);
		DrawWrappers.DrawLine(
			pos.x - jutX * 4, pos.y - jutY * 4,
			destPos.x + jutX * 4, destPos.y + jutY * 4,
			col3, (10 + sin * 5) * sinDamp, 0, true
		);
	}
}




public class IrisSpawnIce : CharState {



	float shootTime;


	public IrisSpawnIce(string transitionSprite = "") :
		base(getSprite(), "", "", transitionSprite) {

		specialId = SpecialStateIds.AxlRoll;
	}

	public static string getSprite() {

		return "shoot_ice";

	}

	public override void update() {
		base.update();
		



		shootTime += Global.spf;
		var poi = character.getFirstPOI();
		if (shootTime > 0.15f && poi != null) {
			shootTime = 0;
			character.playSound("flamethrower");
			if ((character as Iris).irisCrystal != null) {
			
				new ShotgunIceProjCharged(
					(character as Iris).irisCrystal.pos, character.xDir, character,
					player, 1, true, player.getNextActorNetId(), rpc: true
				);
			}
		}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
			return;
		}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {

		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}



public class IrisSpawnFire : CharState {



	float shootTime;


	public IrisSpawnFire(string transitionSprite = "") :
		base(getSprite(), "", "", transitionSprite) {

		specialId = SpecialStateIds.AxlRoll;
	}

	public static string getSprite() {

		return "shoot_ice";

	}

	public override void update() {
		base.update();
		



		shootTime += Global.spf;
		var poi = character.getFirstPOI();
		if (shootTime > 0.15f && poi != null) {
			shootTime = 0;
			character.playSound("flamethrower");
			if ((character as Iris).irisCrystal != null) {	
				new FlameMFireballProj(
				(character as Iris).irisCrystal.pos, character.xDir, player.input.isHeld(Control.Down, player),
				character, player, player.getNextActorNetId(), rpc: true
			);
		
			}
		}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
			return;
		}

		if (character.isAnimOver()) {
			character.changeState(new Crouch(""), true);
		}
	}

	public override void onEnter(CharState oldState) {

		base.onEnter(oldState);
		character.stopMoving();
		character.useGravity = false;

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class IrisDiveKick : CharState {
	float stuckTime;
	float diveTime;
	

	public IrisDiveKick() : base("dive_kick") {
	
	}

	public override void update() {
		if (character.frameIndex >= 1 && !once) {
			character.vel.x = character.xDir * 400;
			character.vel.y = 450;
			character.playSound("punch2", sendRpc: true);
			once = true;
		}
		base.update();
		if (!once) {
			return;
		}
		if (character.vel.y < 100) {
			character.changeToLandingOrFall();
			return;
		}
		CollideData hit = Global.level.checkTerrainCollisionOnce(
			character, character.vel.x * Global.spf, character.vel.y * Global.spf
		);
		if (hit?.isSideWallHit() == true) {
			character.changeState(new Fall(), true);
			return;
		} else if (hit != null) {
			stuckTime += Global.speedMul;
			if (stuckTime >= 6) {
				character.changeToLandingOrFall();
				return;
			}
		}
		if (character.grounded || diveTime >= 6 && character.deltaPos.y == 0) {
			character.changeToLandingOrFall();
			return;
		}
		diveTime += Global.spf;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	    character.stopMoving();
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.stopMoving();
	}
}



public class IrisSlashProj : Projectile {

	bool sound;
	public IrisSlashProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 10, 3, player, "iris_cannon_slash", 25, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		isZSaberClang = true;
		isShield = true;
		isReflectShield = true;
		maxTime = 1.5f;

		projId = (int)ProjIds.IrisSlashProj;
		if (player.character != null) {
			owningActor = player.character;
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		destroyOnDMG = true;
	}


		public override void update(){
	base.update();

		if (sprite.frameIndex >= 7 && !sound){
		playSound("rideX4-1", sendRpc: true);
		sound = true;
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (owner?.character != null) {
			incPos(owner.character.deltaPos);
		}
	}

	
}





public class IrisStabProj : Projectile {

	bool sound;
	public IrisStabProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 10, 1, player, "iris_cannon_stab", 20, 0.1f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		isShield = true;
		isReflectShield = true;
		isZSaberClang = true;
		maxTime = 1.5f;
		projId = (int)ProjIds.IrisStabProj;
		if (player.character != null) {
			owningActor = player.character;
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		destroyOnDMG = true;
	}


		public override void update(){
	base.update();

		if (sprite.frameIndex >= 7 && !sound){
		playSound("rideX4-1", sendRpc: true);
		sound = true;
		}
	}

	public override void postUpdate() {
		base.postUpdate();
		if (owner?.character != null) {
			incPos(owner.character.deltaPos);
		}
	}

	
}












public class IrisCannon : Projectile {
	
	public IrisCannon(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId,
		float damage = 6, int flinch = 26, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, damage, player, "iris_cannon_idle", flinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = true;
		destroyOnHit = false;
		shouldShieldBlock = true;
		setIndestructableProperties();
		maxTime = 999f;
	

		projId = (int)ProjIds.IrisCannon;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}



	public override void postUpdate() {
		base.postUpdate();
	
	}

	private int shootNum;


	private float LaserCD = 0;

	private float ShootCD = 0;


	private int raySplasherMod;
	

	public override void update() {
		base.update();
				Helpers.decrementTime(ref LaserCD);
				Helpers.decrementTime(ref ShootCD);

			if (owner.character != null) xDir = owner.character.xDir;
		if (owner.character == null || owner.character.charState is Die) destroySelf();
		if (owner.character == null || !Global.level.gameObjects.Contains(owner.character)){ 
			destroySelf();
			return;
		}


			// Follow player code.
			if (owner?.character != null) {
				Character character = owner.character;
				float targetPosX = (30 * -character.xDir + character.pos.x);
				float targetPosY = (-40 + character.pos.y);
				float moveSpeed = 1.5f * 60;

				// X axis follow.
				if (pos.x < targetPosX) {
					move(new Point(moveSpeed, 0));
					if (pos.x > targetPosX) { pos.x = targetPosX; }
				} else if (pos.x > targetPosX) {
					move(new Point(-moveSpeed, 0));
					if (pos.x < targetPosX) { pos.x = targetPosX; }
				}
				// Y axis follow.
				if (pos.y < targetPosY) {
					move(new Point(0, moveSpeed));
					if (pos.y > targetPosY) { pos.y = targetPosY; }
				} else if (pos.y > targetPosY) {
					move(new Point(0, -moveSpeed));
					if (pos.y < targetPosY) { pos.y = targetPosY; }
				}
			}

			
				if (LaserCD == 0 && owner.character != null &&  owner.superAmmo > 15 &&
				
				owner.input.isPressed(Control.WeaponRight,owner)
				&& owner.input.isHeld(Control.Up,owner)){
				new IrisLaserProjFoward(pos, owner.character.xDir, owner.character, owner,
					owner.getNextActorNetId(), rpc: true
			);
			LaserCD = 2;
			owner.superAmmo -= 16;
				playSound("irislaser2", sendRpc: true);
			}


			
		
		
				raySplasherMod++;
			if (owner.input.isPressed(Control.WeaponRight,owner) && ShootCD == 0 && owner.superAmmo > 0 
			&& owner.character != null && !owner.character.isInDamageSprite()){
				ShootCD = 0.1f;
			if (!owner.character.OverDrive) {
				owner.superAmmo -= 1;
			}
				playSound("shootX3lv", sendRpc: true);
					new IrisFireBallProj(new IrisCrystal(), pos, xDir , shootNum,
					 true, owner, owner.getNextActorNetId(), sendRpc: true);
					shootNum++;
			}
			
		}
	}



	
	
public class IrisFireBallProj : Projectile {
	int shootNum;
	bool isHanging;
	public IrisFireBallProj(
		Weapon weapon, Point pos, int xDir, int shootNum,
		bool isHanging, Player player, ushort netProjId, bool sendRpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player, "neont_projectile_start",
		1, 0.01f, netProjId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.IrisFireBallProj;
		maxTime = 0.875f;
		this.shootNum = shootNum;
		this.isHanging = isHanging;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		// ToDo: Make local.
		canBeLocal = false;
		destroyOnDMG = true;
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;

		if (sprite.name.EndsWith("start")) {
			if (isAnimOver()) {
				if (!isHanging) {
					if (shootNum % 3 == 0) vel = new Point(xDir * 250, 0);
					else if (shootNum % 3 == 1) vel = new Point(xDir * 240, 50);
					else if (shootNum % 3 == 2) vel = new Point(xDir * 240, -50);
				} else {
					if (shootNum % 3 == 0) vel = new Point(xDir * 250, -50);
					else if (shootNum % 3 == 1) vel = new Point(xDir * 229, 100);
					else if (shootNum % 3 == 2) vel = new Point(xDir * 150, 200);
				}
				changeSprite("iris_crystal_fireball", true);
			}
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isPushImmune()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -800));
		}
	}
	

}


	




	



