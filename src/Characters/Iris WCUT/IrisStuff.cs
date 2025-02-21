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
      		character.playSound("crashX3", forcePlay: false, sendRpc: true);
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
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
	
		return "shoot_ice";

	}

	public override void update() {
		base.update();
			if (character.grounded){
			isGrounded = true;
			}

  			
			if (!character.grounded){
			if (player.input.isHeld(Control.Jump, player)){
			character.useGravity = false;} else {character.useGravity = true;}
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

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}





public class IrisCrystal : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public IrisCrystal() : base() {
		index = (int)WeaponIds.IrisCrystal;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
			displayName = "Iris Crystal";
			description = new string[] { "Iris's Mighty Crystal." };
			killFeedIndex = 126;
		
		}
}

public class NewIrisCrystal : Projectile {

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


	public NewIrisCrystal(
		IrisCrystal weapon, Point pos, int xDir, Player player, ushort netProjId,
		float damage = 6, int flinch = 26, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, damage, player, "iris_crystal_bb_behavior", flinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		canBeLocal = false;
		//fadeSprite = weapon.fadeSprite;
		projId = (int)ProjIds.IrisCrystal;
		destroyOnHit = false;
		maxAngleDist = 45;
		returnTime = 0;
		damager.damage = 1;
		damager.hitCooldown = 0.33f;
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}
	



	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
	}

	public override void onDestroy() {
		base.onDestroy();
		if (pickup != null) {
			pickup.useGravity = true;
			pickup.collider.isTrigger = false;
		}
	}


	public override void update() {
		base.update();

		if (owner.character != null) xDir = owner.character.xDir;
		if (owner.character == null || owner.character.charState is Die) destroySelf();
		if (owner.character == null || !Global.level.gameObjects.Contains(owner.character)){ 
			destroySelf();
			return;
		}

		if (owner.character.charState is IrisCrystalRisingBash )state = 1;
		if (state == 1){
			if (sprite.name != "iris_crystal_bash_up") changeSprite("iris_crystal_bash_up", true);
			changePos(owner.character.pos);
		}

		if (owner.character.charState is IrisCrystalBashState )state = 2;		
		if (state == 2){
			if (sprite.name != "iris_crystal_bash") changeSprite("iris_crystal_bash", true);
			changePos(owner.character.pos);
		}

		
		if (owner.character.charState is IrisCrystalCharge) state = 4;
		if (state == 4) {
		if (owner.input.isHeld(Control.Up, owner)) {
				vel.y = -100;
			} 
			 if (owner.input.isHeld(Control.Down, owner)) {
			    vel.y = 100;
			}
			 if (owner.input.isHeld(Control.Right, owner)) {
				vel.x = 100;
			}
			 if (owner.input.isHeld(Control.Left, owner)) {
				vel.x = -100;
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


		if( owner.character.charState is  IrisSpawnBeam
		|| owner.character.charState is  IrisSpawnIce) state = 5;
		
		if (state == 5) {
			vel.x = 0;
			vel.y = 0;
		}

		if (owner.character.charState is not IrisCrystalBashState 
		&& owner.character.charState is not IrisCrystalRisingBash
		&& owner.character.charState is not IrisSpawnBeam
		&& owner.character.charState is not IrisSpawnIce
		&& owner.character.charState is not IrisCrystalCharge){
		state = 0;
		}
		if (state == 0) {
		time += Global.spf;
		if (sprite.name != "iris_crystal_bb_behavior")changeSprite("iris_crystal_bb_behavior", false);
		if (time > 2) time = 0;
		float x = 20 * MathF.Sin(time * 5);
		yPos = -15 * time;
		Point newPos = owner.character.pos.addxy(x, yPos);
		changePos(newPos);
		}
	}
}




public class IrisSlashProj : Projectile {

	bool sound;
	public IrisSlashProj(
		Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 3, player, "iris_cannon_slash", Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = true;
		destroyOnHit = false;
		shouldShieldBlock = true;
		setIndestructableProperties();
		maxTime = 1.3f;
		projId = (int)ProjIds.IrisSlashProj;
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

		
				if (LaserCD == 0 && owner.character != null && owner.input.isPressed("special1", owner)){
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
}


	




	



