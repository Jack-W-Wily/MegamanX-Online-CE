using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			character.stopMovingWeak();
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
			character.stopMovingWeak();
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
		: base("string_3", "", "", transitionSprite)
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
		character.playSound("dynamoslash", sendRpc: true);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		//character.playSound("rocketPunch", forcePlay: false, sendRpc: true);
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class IrisCrystalCharge : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	

	public IrisCrystalCharge()
		: base("chargegp", "")
	{

	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
		character.playSound("distortion_b", true);
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
		
		if (character.frameIndex < 4 &&!player.input.isHeld("special1", base.player))
			{
			character.changeState(new Idle(), forceChange: true);
			}
		if (character.frameIndex > 3 &&!player.input.isHeld("special1", base.player)
		&& !player.input.isHeld(Control.Up, player))
			{
			character.changeState(new IrisSpawnIce(NapalmAttackType.Napalm), forceChange: true);
			}
		if (character.frameIndex > 3 &&!player.input.isHeld("special1", base.player)
		&& player.input.isHeld(Control.Up, player))
			{
			character.changeState(new IrisSpawnBeam(), forceChange: true);
			}
	}
}




public class IrisSpawnBeam : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	NewIrisCrystal crystal;

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
		if ((character as Iris).irisCrystal != null){
      		character.playSound("irislaser2", forcePlay: false, sendRpc: true);
			new DynamoBeam(weapon, (character as Iris).irisCrystal.pos, character.xDir, player, player.getNextActorNetId(), sendRpc: true);
			}
		}
		if (character.isAnimOver())
		{
		character.changeState(new Idle(), forceChange: true);
		}
	}
}


public class IrisSpawnIce : CharState {
	bool shot;
	bool isGrounded = false;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;

	NewIrisCrystal crystal;
	
	public IrisSpawnIce(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
			specialId = SpecialStateIds.AxlRoll;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
	
		return "shoot_ice";

	}

	public override void update() {
		base.update();
			if (character.grounded){
			isGrounded = true;
			}

  			
		
			shootTime += Global.spf;
			var poi = character.getFirstPOI();
			if (shootTime > 0.15f && poi != null) {
				shootTime = 0;
				character.playSound("flamethrower");
				float xSpeed = 100f;	
				if ((character as Iris).irisCrystal != null){
				new VelGIceProj(new VelGIceWeapon(), (character as Iris).irisCrystal.pos, character.xDir, new Point(xSpeed * (float)character.xDir, -200f), base.player, base.player.getNextActorNetId(), rpc: true);	
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
	    character.stopMovingWeak();
		character.useGravity = false;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.stopMovingWeak();
	}
}



public class IrisSlashProj : Projectile {

	bool sound;
	public IrisSlashProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 3, player, "iris_cannon_slash", 32, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		isJuggleProjectile = true;
		shouldClang = true;
		isShield = true;
		isReflectShield = true;
		maxTime = 1.5f;
		projId = (int)ProjIds.IrisSlashProj;
		isMelee = true;
		if (player.character != null) {
			owningActor = player.character;
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
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
		weapon, pos, xDir, 0, 1, player, "iris_cannon_stab", 30, 0.1f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = false;
		destroyOnHit = false;
		shouldShieldBlock = false;
		setIndestructableProperties();
		shouldClang = true;
		isShield = true;
		isReflectShield = true;
		maxTime = 1.5f;
		projId = (int)ProjIds.IrisStabProj;
		isMelee = true;
		if (player.character != null) {
			owningActor = player.character;
		}

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
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
	private int state = 0;

	private Actor target;

	private float LaserCD = 0;

	private float ShootCD = 0;

		private float raySplasherShootTime;

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

		
				if (LaserCD == 0 && owner.character != null && 
				
				owner.input.isPressed(Control.WeaponRight,owner)
				&& owner.input.isHeld(Control.Up,owner)){
				new RisingSpecterProj(new VileLaser(VileLaserType.RisingSpecter), pos, xDir, owner, owner.getNextActorNetId(), rpc: true);
				LaserCD = 4;
				playSound("irislaser2", sendRpc: true);
			}


			
		
		
				raySplasherMod++;
			if (owner.input.isPressed(Control.WeaponRight,owner) && ShootCD == 0){
				ShootCD = 0.1f;
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
		weapon, pos, xDir, 0, 2, player, "neont_projectile_start",
		0, 0.01f, netProjId, player.ownedByLocalPlayer
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


	




	



