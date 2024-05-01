using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;


public class DynamoRoyal : Weapon
{
	public Projectile absorbedProj;

	public DynamoRoyal(Projectile otherProj)
	{
		index = 125;
		weaponSlotIndex = 125;
		killFeedIndex = 168;
		absorbedProj = otherProj;
	}
}


public class DynamoTrick : Weapon
{
	public Projectile absorbedProj;

	public DynamoTrick()
	{
		index = 124;
		weaponSlotIndex = 124;
		killFeedIndex = 168;
	//	absorbedProj = otherProj;
	}
}

public class DynamoSword : Weapon
{
	public Projectile absorbedProj;

	public DynamoSword()
	{
		index = 123;
		weaponSlotIndex = 123;
		killFeedIndex = 168;
	//	absorbedProj = otherProj;
	}
}

public class DynamoBoomerang : Weapon {
	public float vileAmmoUsage;
	public string projSprite;
	public DynamoBoomerang() : base() {
		index = (int)WeaponIds.DynamoBoomerang;
		weaponBarBaseIndex = 0;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 31;
		weaponSlotIndex = 45;
			displayName = "None";
			description = new string[] { "Do not equip a Rocket Punch." };
			killFeedIndex = 126;
		
		}
}

public class DynamoBoomerangProj : Projectile {
	public bool reversed;
	public bool returned;
	Character shooter;
	Player player;
	public float maxReverseTime;
	public float minTime;
	public float smokeTime;
	public Actor target;
	public DynamoBoomerang rocketPunchWeapon;

	public static float getSpeed(int type) {
	return 500;
	}

	public DynamoBoomerangProj(DynamoBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, getSpeed(weapon.type), 2, player, "dynamo_spinningblade", 20, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.RocketPunch;
		this.player = player;
		shooter = player.character;
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.2f;
		maxReverseTime = 0.4f;
		if (weapon.type == (int)RocketPunchType.GoGetterRight) {
			maxReverseTime = 0.3f;
		}
		rocketPunchWeapon = weapon;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		if (projId != (int)ProjIds.DynamoBoomerang) {
			canBeLocal = false;
		}
	}

	public override void update() {
		base.update();
		if (ownedByLocalPlayer && (shooter == null || shooter.destroyed)) {
			destroySelf("explosion", "explosion", true);
			return;
		}
		if (!locallyControlled) return;
		if (!reversed && target != null) {
			vel = new Point(0, 0);
			if (pos.x > target.pos.x) xDir = -1;
			else xDir = 1;
			Point targetPos = target.getCenterPos();
			move(pos.directionToNorm(targetPos).times(speed));
			if (pos.distanceTo(targetPos) < 5) {
				reversed = true;
			}
		}
		if (!reversed && rocketPunchWeapon.type == (int)RocketPunchType.GoGetterRight) {
			if (player.input.isHeld(Control.Up, player)) {
				incPos(new Point(0, -300 * Global.spf));
			} else if (player.input.isHeld(Control.Down, player)) {
				incPos(new Point(0, 300 * Global.spf));
			}
		}

		if (!reversed && time > maxReverseTime) {
			reversed = true;
		}
		if (reversed) {
			vel = new Point(0, 0);
			if (shooter.xDir == 1) xDir = -1;
			if (shooter.xDir == -1) xDir = 1;

			Point returnPos = shooter.getCenterPos();
			if (shooter.sprite.name == "vile_rocket_punch") {
				Point poi = shooter.pos;
				var pois = shooter.sprite.getCurrentFrame()?.POIs;
				if (pois != null && pois.Count > 0) {
					poi = pois[0];
				}
				returnPos = shooter.pos.addxy(poi.x * shooter.xDir, poi.y);
			}

			move(pos.directionToNorm(returnPos).times(speed));
			if (pos.distanceTo(returnPos) < 10) {
				returned = true;
				destroySelf();
			}
		}
	}

	/*
	public override void onHitWall(CollideData other) {
		if (!ownedByLocalPlayer) return;
		reversed = true;
	}
	*/

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (locallyControlled) {
			reversed = true;
		}
		if (isRunByLocalPlayer()) {
			reversed = true;
			RPC.actorToggle.sendRpc(netId, RPCActorToggleType.ReverseRocketPunch);
		}
	}
}

public class DynamoBoomerangState : CharState {
	bool shot = false;
	DynamoBoomerangProj proj;
	float specialPressTime;
	public DynamoBoomerangState(string transitionSprite = "") : base("rocket_punch", "", "", transitionSprite) {
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref specialPressTime);

		if (proj != null && !player.input.isHeld(Control.Shoot, player) && proj.time >= proj.minTime) {
			proj.reversed = true;
		}

		if (!shot && character.sprite.frameIndex == 1) {
			shoot();
		}
		if (proj != null) {
			
				if (proj.returned || proj.destroyed) {
					character.changeState(new Idle(), true);
					return;
				}
			
		}
	}

	public void shoot() {
		shot = true;
		character.playSound("dynamosaber", sendRpc: true);
		character.frameIndex = 1;
		character.frameTime = 0;
		var poi = character.sprite.getCurrentFrame().POIs[0];
		poi.x *= character.xDir;
		proj = new DynamoBoomerangProj(new DynamoBoomerang(),
		character.pos.add(poi), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
	}

	public void reset() {
		character.frameIndex = 0;
		stateTime = 0;
		shot = false;
	}
}




public class HolyCross : CharState {

	private bool shot;
	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	HolyCrossProj proj;

	public HolyCross(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
	{
	
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


		if (proj == null && character.frameIndex >= 3 && character.ownedByLocalPlayer){
		proj = new HolyCrossProj(new ShieldBoomerang(), character.getShootPos(), character.xDir, player, player.getNextActorNetId(), rpc : true);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}



public class HolyCrossProj : Projectile {
	public float angleDist = 0;
	public float turnDir = 1;
	public Pickup pickup;
	public float angle2;

	public float maxSpeed = 350;
	public float returnTime = 0.55f;
	public float turnSpeed = 100;
	public float maxAngleDist = 280;
	public float soundCooldown;

	public HolyCrossProj(ShieldBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 150, 2, player, "dynamo_holycross_proj", 8, 0.8f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.ShieldBoomerang;
		destroyOnHit = false;
		
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
	
		if (other.gameObject is Pickup && pickup == null) {
			pickup = other.gameObject as Pickup;
			if (!pickup.ownedByLocalPlayer) {
				pickup.takeOwnership();
				RPC.clearOwnership.sendRpc(pickup.netId);
			}
		}

		var character = other.gameObject as Character;
		if (time > returnTime && character != null && character.player == damager.owner) {
			destroySelf();
			character.player.vileAmmo = Helpers.clampMax(character.player.vileAmmo + 8, character.player.vileMaxAmmo);
		}
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

		if (!destroyed && pickup != null) {
			pickup.collider.isTrigger = true;
			pickup.useGravity = false;
			pickup.changePos(pos);
		}

		soundCooldown -= Global.spf;
		if (soundCooldown <= 0) {
			soundCooldown = 0.3f;
			playSound("cutter", sendRpc: true);
		}

		if (time > returnTime) {
			if (angleDist < maxAngleDist) {
				var angInc = (-xDir * turnDir) * Global.spf * turnSpeed;
				angle2 += angInc;
				angleDist += MathF.Abs(angInc);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
			} 
			 if (damager.owner.character != null) {
				var dTo = pos.directionTo(damager.owner.character.getCenterPos()).normalize();
				var destAngle = MathF.Atan2(dTo.y, dTo.x) * 180 / MathF.PI;
				destAngle = Helpers.to360(destAngle);
				angle2 = Helpers.lerpAngle(angle2, destAngle, Global.spf * 10);
				vel.x = Helpers.cosd(angle2) * maxSpeed;
				vel.y = Helpers.sind(angle2) * maxSpeed;
			} else {
				destroySelf();
			}
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 50 * modifier, -300));
		}
	}

}





public class HolyWaterAttack : CharState {
	bool shot;
	NapalmAttackType napalmAttackType;
	float shootTime;
	int shootCount;

	public HolyWaterAttack(NapalmAttackType napalmAttackType, string transitionSprite = "") :
		base(getSprite(napalmAttackType), "", "", transitionSprite) {
		this.napalmAttackType = napalmAttackType;
	}

	public static string getSprite(NapalmAttackType napalmAttackType) {
		return "rocket_punch";
	}

	public override void update() {
		base.update();

		if (napalmAttackType == NapalmAttackType.Napalm) {
			if (!shot && character.sprite.frameIndex == 2) {
				shot = true;
				Projectile proj;
				proj = new HolyWaterGranadeProj(player.vileNapalmWeapon, character.getShootPos(), character.xDir, character.player, character.player.getNextActorNetId(), rpc: true);
				
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



public class HolyWaterGranadeProj : Projectile {
	bool exploded;
	public HolyWaterGranadeProj(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 150, 2, player, "dynamo_holywater_proj", 0, 0.2f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.HolyWaterGranade;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		this.vel = new Point(speed * xDir, -200);
		useGravity = true;
		collider.wallOnly = true;
		fadeSound = "explosion";
		fadeSprite = "explosion";
		shouldShieldBlock = false;
	}

	public override void update() {
		base.update();
		if (grounded) {
			explode();
		}
	}

	public override void onHitWall(CollideData other) {
		xDir *= -1;
		explode();
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (ownedByLocalPlayer) explode();
	}

	public void explode() {
		if (exploded) return;
		exploded = true;
		if (ownedByLocalPlayer) {
			for (int i = -3; i <= 3; i++) {
				new NapalmPartProj(weapon, pos.addxy(0, 0), 1, owner, owner.getNextActorNetId(), false, i * 10, rpc: true);
				new NapalmPartProj(weapon, pos.addxy(0, 0), 1, owner, owner.getNextActorNetId(), true, i * 10, rpc: true);
			}
		}
		destroySelf();
	}
}




public class DaggerThrow : CharState {

	private bool shot;
	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	DynamoDaggerProj proj;

	public DaggerThrow(string transitionSprite = "")
		: base("rocket_punch", "", "", transitionSprite)
	{
	
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


		if (proj == null && character.frameIndex >= 3 && character.ownedByLocalPlayer){
		proj = new DynamoDaggerProj(
					character.getShootPos(), character.getShootXDir(),
					player, player.getNextActorNetId(), rpc: true
				);
		}

		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		
		}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}


public class DynamoDaggerProj : Projectile {
	public DynamoDaggerProj(
		Point pos, int xDir, Player player, ushort? netId, bool rpc = false
	) : base(
		ZeroBuster.netWeapon, pos, xDir,
		350, 1.5f, player, "dynamo_dagger_proj", 2, 0,
		netId, player.ownedByLocalPlayer
	) {
		fadeOnAutoDestroy = true;
		fadeSprite = "dynamo_dagger_proj";
		reflectable = true;
		maxTime = 0.5f;
		projId = (int)ProjIds.DynamoDagger;

		if (rpc) {
			rpcCreate(pos, player, netId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new DynamoDaggerProj(
			args.pos, args.xDir, args.player, args.netId
		);
	}
}




public class DynamoString1 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public DynamoString1(string transitionSprite = "")
		: base("string_1", "", "", transitionSprite)
	{
	
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
		if (player.input.isPressed(Control.Shoot, player))
		{
			character.changeState(new DynamoString2());
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
	


public class DynamoString2 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public DynamoString2(string transitionSprite = "")
		: base("string_2", "", "", transitionSprite)
	{
		
	}

	public override void update()
	{
	
		if (player.input.isPressed(Control.Shoot, player))
		{
			character.changeState(new DynamoString3());
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
	


public class DynamoString3 : CharState {


	private float specialPressTime;
	
	public float pushBackSpeed;

	public DynamoString3(string transitionSprite = "")
		: base("string_3", "", "", transitionSprite)
	{
	
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


public class DynamoGroundPoundCharge : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	

	public DynamoGroundPoundCharge()
		: base("chargegp", "")
	{

	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState)
	{
		base.onExit(newState);
	}

	public override void update()
	{
		base.update();
		if (character.isAnimOver())
		{
		character.changeState(new DynamoGroundPound3(), forceChange: true);
		}
		if (character.frameIndex < 4 &&!player.input.isHeld("special1", base.player))
			{
			character.changeState(new DynamoGroundPound(), forceChange: true);
			}
		if (character.frameIndex > 3 &&!player.input.isHeld("special1", base.player))
			{
			character.changeState(new DynamoGroundPound1(), forceChange: true);
			}
	}
}


public class DynamoGroundPound : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	

	public DynamoGroundPound()
		: base("crouch_nade", "")
	{

	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState)
	{
		base.onExit(newState);
	}

	public override void update()
	{
		base.update();

		if (character.frameIndex == 3){
        TriadThunder weapon = new TriadThunder();
		character.playSound("crashX3", forcePlay: false, sendRpc: true);
		 new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		new TriadThunderQuake(weapon, character.pos.addxy(-10 * character.xDir, 0f), 1, base.player, base.player.getNextActorNetId(), rpc: true);
		
		}
		if (character.isAnimOver())
		{
		character.changeState(new Idle(), forceChange: true);
		}
		if (base.player.input.isHeld("shoot", base.player))
			{
			character.changeState(new Idle(), forceChange: true);
			}
	}
}


public class DynamoGroundPound2 : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	
	private bool fired;

	public DynamoGroundPound2()
		: base("crouch_nade", "")
	{

	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState)
	{
		base.onExit(newState);
	}

	public override void update()
	{
		base.update();
		superArmor = true;
		if (character.frameIndex == 3 && !fired){
		fired = true;
        TriadThunder weapon = new TriadThunder();
		character.playSound("crashX3", forcePlay: false, sendRpc: true);
		new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		new TriadThunderQuake(weapon, character.pos.addxy(-10 * character.xDir, 0f), 1, base.player, base.player.getNextActorNetId(), rpc: true);
		new DynamoBeam(weapon, character.pos.addxy(character.xDir * 20f,0f), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
		}
		if (character.isAnimOver())
		{
		character.changeState(new Idle(), forceChange: true);
		}
		if (base.player.input.isHeld("shoot", base.player))
			{
			character.changeState(new Idle(), forceChange: true);
			}
	}
}


public class DynamoGroundPound1 : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	
	private bool fired;

	public DynamoGroundPound1()
		: base("crouch_nade", "")
	{

	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState)
	{
	base.onExit(newState);
	}

	public override void update()
	{
		base.update();
		superArmor = true;
		if (character.frameIndex == 3 && !fired){
		fired = true;
        TriadThunder weapon = new TriadThunder();
		character.playSound("crashX3", forcePlay: false, sendRpc: true);
		new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		new TriadThunderQuake(weapon, character.pos.addxy(-10 * character.xDir, 0f), 1, base.player, base.player.getNextActorNetId(), rpc: true);
		new DynamoBeam(weapon, character.pos.addxy(character.xDir * 40f,0f), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
		}
		if (character.isAnimOver())
		{
		character.changeState(new Idle(), forceChange: true);
		}
		if (base.player.input.isHeld("shoot", base.player))
			{
			character.changeState(new Idle(), forceChange: true);
			}
	}
}



public class HolyBibleProj : Projectile {
	public float angleDist = 0;
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

	public HolyBibleProj(IrisCrystal weapon, Point pos, int xDir, Player player, ushort netProjId, Point? vel = null, bool rpc = false) :
		base(weapon, pos, xDir, 350, 2, player, "dynamo_holybible_proj", 1, 0.5f, netProjId, player.ownedByLocalPlayer) {
		//fadeSprite = weapon.fadeSprite;
		projId = (int)ProjIds.HolyBible;
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

		xDir = owner.character.xDir;
		if (owner.character.charState is Die || time > 2f) destroySelf();
		if (owner.character == null || !Global.level.gameObjects.Contains(owner.character)){ 
			destroySelf();
			return;
		}
		time += Global.spf;
		if (time > 2) time = 0;
		float x = 20 * MathF.Sin(time * 5);
		yPos = -15 * time;
		Point newPos = owner.character.pos.addxy(x, yPos);
		changePos(newPos);	
	}
}


public class DynamoGroundPound3 : CharState
{
	public float dashTime;

	public Projectile fSplasherProj;

	
	private bool fired;

	public DynamoGroundPound3()
		: base("crouch_nade", "")
	{

	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
	}

	public override void onExit(CharState newState)
	{
		base.onExit(newState);
	}

	public override void update()
	{
		base.update();
		invincible = true;
		if (character.frameIndex == 3 && !fired){
		fired = true;
        TriadThunder weapon = new TriadThunder();
		character.playSound("crashX3", forcePlay: false, sendRpc: true);
		new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		new TriadThunderQuake(weapon, character.pos.addxy(-10 * character.xDir, 0f), 1, base.player, base.player.getNextActorNetId(), rpc: true);
		new DynamoBeam(weapon, character.pos.addxy(character.xDir * 20f,0f), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
		new DynamoBeam(weapon, character.pos.addxy(character.xDir * 40f,0f), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
		new DynamoBeam(weapon, character.pos.addxy(character.xDir * 60f,0f), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
		new DynamoBeam(weapon, character.pos.addxy(character.xDir * 80f,0f), character.xDir, player, player.getNextActorNetId(), sendRpc: true);
		
		}
		if (character.isAnimOver())
		{
		character.changeState(new Idle(), forceChange: true);
		}
		if (base.player.input.isHeld("shoot", base.player))
			{
			character.changeState(new Idle(), forceChange: true);
			}
	}
}


public class  DynamoAirShoot : CharState {

	private bool shot;
	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	public DynamoAirShoot(string transitionSprite = "")
		: base("air_bomb_attack", "", "", transitionSprite)
	{
		
	}

	public override void update()
	{
	

		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed += 7.5f;
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
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
		character.playSound("buster3", forcePlay: false, sendRpc: true);
	 new DynamoDownShoot(
					new VileBall(VileBallType.PeaceOutRoller), character.pos, character.xDir,
					base.player, 0, character.player.getNextActorNetId(), null, rpc: true
	 );
			}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}

public class DynamoDownShoot : Projectile
{
	private int type;

	private Collider floorHitbox;

	private bool split;

	public DynamoDownShoot(Weapon weapon, Point pos, int xDir, Player player, int type, ushort netProjId, Point? vel = null, bool rpc = false)
		: base(weapon, pos, xDir, 75f, 3f, player, "dynamo_buster_proj", 1, 0.5f, netProjId, player.ownedByLocalPlayer)
	{
		projId = (int)ProjIds.DynamoDownShoot;
		maxTime = 0.525f;
		if (type == 1)
		{
			maxTime = 0.8f;
		} else {
			floorHitbox = new Collider(new Rect(0, 0, 4, 4).getPoints(), false, this, false, false, 0, new Point(0, 0));
		}
		destroyOnHit = true;
		this.type = type;
		xScale = 0.75f * (float)xDir;
		yScale = 0.75f;
		if (vel.HasValue)
		{
			base.vel = vel.Value;
		}
		if (type == 0)
		{
			angle = 90;
			base.vel.y = 40f;
			useGravity = true;
			gravityModifier = 0.5f;
		}
		fadeSprite = "dynamo_beam_a";
		fadeOnAutoDestroy = true;
		if (rpc)
		{
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update()
	{
		base.update();
	}

	public override void onHitWall(CollideData other)
	{
		if (ownedByLocalPlayer && type == 0) {
			if (!other.gameObject.collider.isCollidingWith(floorHitbox)) {
				return;
			}
		}
		if (ownedByLocalPlayer && other.gameObject.collider.isClimbable && !split && type == 0)
		{
			Point? normal = other?.hitData?.normal;
			normal = ((!normal.HasValue) ? new Point?(new Point(1f, 0f)) : new Point?(normal.Value.leftNormal()));
			Point normal2 = normal.Value;
			normal2.multiply(250f);
			destroySelf(fadeSprite);
			split = true;
			new DynamoDownShoot(weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(), normal2, rpc: true);
			new DynamoDownShoot(weapon, pos.clone(), xDir, damager.owner, 1, Global.level.mainPlayer.getNextActorNetId(), normal2.times(-1f), rpc: true);
			destroySelf();
		}
	}
}


public class DynamoBeam : Projectile
{
	private Player player;
	
	public float soundTime = 0;

	public DynamoBeam(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false)
		: base(weapon, pos, 1, 0f, 3f, player, "dynamo_beam_b", 10, 1f, netProjId, player.ownedByLocalPlayer)
	{
		projId = (int)ProjIds.DynamoBeam;
		shouldShieldBlock = false;
		shouldVortexSuck = false;
		destroyOnHit = false;
		maxTime = 0.75f;
		this.player = player;
		if (sendRpc)
		{
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update()
	{
		base.update();
		Helpers.decrementTime(ref soundTime);
		
	}

	public override bool shouldDealDamage(IDamagable damagable)
	{
		if (damagable is Actor actor && MathF.Abs(pos.x - actor.pos.x) > 35f)
		{
			return false;
		}
		return true;
	}

	public override void onHitDamagable(IDamagable damagable)
	{
		base.onHitDamagable(damagable);
		if (damagable is Character chr)
		{
			float modifier = 1f;
			if (chr.isUnderwater())
			{
				modifier = 2f;
			}
			if (!chr.isImmuneToKnockback())
			{
				float xMoveVel = MathF.Sign(pos.x - chr.pos.x);
				chr.move(new Point(xMoveVel * 50f * modifier, 0));
				if (MathF.Abs(pos.x - chr.pos.x) <= 35f) {
					chr.move(new Point(xMoveVel * 50f * modifier, -300f));
					chr.vel.y = 0;
				}
			}
		}
	}
}


public class DynamoParryStartState : CharState {





	public DynamoParryStartState() : base("parry_start", "", "", "") {
		superArmor = true;
	}

	public override void update() {
		base.update();

		if (stateTime < 0.1f) {
			character.turnToInput(player.input, player);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public void counterAttack(Player damagingPlayer, Actor damagingActor, float damage) {
		Actor counterAttackTarget = null;
		if (damagingActor is GenericMeleeProj gmp) {
			counterAttackTarget = gmp.owningActor;
		}

		if (counterAttackTarget == null) {
			counterAttackTarget = damagingPlayer?.character ?? damagingActor;
		}

		var proj = damagingActor as Projectile;
		bool stunnableParry = proj != null && proj.canBeParried();
		if (counterAttackTarget != null && character.pos.distanceTo(counterAttackTarget.pos) < 75 && counterAttackTarget is Character chr && stunnableParry) {
			if (!chr.ownedByLocalPlayer) {
				RPC.actorToggle.sendRpc(chr.netId, RPCActorToggleType.ChangeToParriedState);
			} else {
				chr.changeState(new ParriedState(), true);
			}
		}

		character.playSound("zeroParry", sendRpc: true);
		(player.character as Dynamo).NightmareBullets = 6;	
		character.changeState(new Idle(), true);
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.parryCooldown = character.maxParryCooldown;
	}

	public bool canParry(Actor damagingActor) {
		var proj = damagingActor as Projectile;
		if (proj == null) {
			return false;
		}
		return character.frameIndex == 0;
	}
}



public class DynamoShoot : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	public DynamoShoot(
		bool grounded, bool shootProj
	) : base(
		grounded ? "idle_shoot" : "idle_shoot", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "idle_shoot";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
	}

	public override void update() {
		base.update();
		if (!character.grounded) {
			if (player.input.isHeld(Control.Dash, player)) {
				character.isDashing = true;
			}
		}

		if (character.frameIndex >= 0 && !fired) {
			fired = true;
			character.playSound("buster3", sendRpc: true);
			new ZBuster2Proj(
					new ZeroBuster(), character.getShootPos(), character.xDir, 0, player, player.getNextActorNetId(), rpc: true
				);
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "fall_shoot";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}


public class DynamoNightmareBullet : CharState {
	bool fired;
	bool grounded;
	bool shootProj;
	public DynamoNightmareBullet(
		bool grounded, bool shootProj
	) : base(
		grounded ? "unpo_parry_attack" : "unpo_parry_attack", "", "", ""
	) {
		this.grounded = grounded;
		landSprite = "unpo_parry_attack";
		this.shootProj = shootProj;
		if (shootProj) {
			superArmor = true;
		}
		airMove = true;
	}

	public override void update() {
		base.update();
		if (!character.grounded) {
			if (player.input.isHeld(Control.Dash, player)) {
				character.isDashing = true;
			}
		}

		if (character.frameIndex >= 4 && !fired) {
			fired = true;
			character.playSound("buster3", sendRpc: true);
		new TorpedoProj(new Torpedo(), character.getShootPos(), character.xDir, player, 3, player.getNextActorNetId(), 0, rpc: true);
			
		}

		if (character.isAnimOver()) {
			if (character.grounded) character.changeState(new Idle(), true);
			else character.changeState(new Fall(), true);
		} else {
			if ((character.grounded || character.canAirJump()) &&
				player.input.isPressed(Control.Jump, player)
			) {
				if (!character.grounded) {
					character.dashedInAir++;
				}
				character.vel.y = -character.getJumpPower();
				sprite = "unpo_parry_attack";
				defaultSprite = sprite;
				character.changeSpriteFromName(sprite, false);
			}
		}
	}
}
