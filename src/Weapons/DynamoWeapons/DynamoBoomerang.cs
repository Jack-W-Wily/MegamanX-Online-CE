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
		index = 34;
		weaponSlotIndex = 118;
		killFeedIndex = 168;
		absorbedProj = otherProj;
	}
}


public class DynamoTrick : Weapon
{
	public Projectile absorbedProj;

	public DynamoTrick()
	{
		index = 101;
		weaponSlotIndex = 101;
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
		index = (int)WeaponIds.RocketPunch;
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
	public DynamoBoomerang DynamoBoomerangWeapon;

	public static float getSpeed(int type) {
		return 500;
	}

	public DynamoBoomerangProj(DynamoBoomerang weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, getSpeed(weapon.type), 2, player, "dynamo_spinningblade", 20, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DynamoBoomerang;
		this.player = player;
		shooter = player.character;
		destroyOnHit = false;
		shouldShieldBlock = false;
		if (player.character != null) setzIndex(player.character.zIndex - 100);
		minTime = 0.2f;
		maxReverseTime = 0.4f;
		
		DynamoBoomerangWeapon = weapon;
		
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

			maxReverseTime = 0.4f;
		
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
		if (!reversed) {
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
			if (pos.x > shooter.pos.x) xDir = -1;
			else xDir = 1;
		
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

		if (proj != null && !player.input.isHeld(Control.Special1, player) && proj.time >= proj.minTime) {
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
		character.playSound("dynamosaber", sendRpc: true);
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
		character.playSound("dynamosaber", sendRpc: true);
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
		character.playSound("dynamosaber", sendRpc: true);
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
		projId = (int)ProjIds.PeaceOutRoller;
		maxTime = 0.525f;
		if (type == 1)
		{
			maxTime = 0.4f;
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
			base.vel.y = 50f;
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




