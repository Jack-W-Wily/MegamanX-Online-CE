namespace MMXOnline;

using System;
using System.Collections.Generic;



public class VileGrab : Weapon {
	public VileGrab() : base() {
		rateOfFire = 0.75f;
		index = (int)WeaponIds.VileGrab;
		killFeedIndex = 63;
	}
}


public class VileGrabState : CharState
{
	public Actor victim;


    public Napalm weapon;

	private float partTime;

	private float chargeTime;


     private int state; 

	private float leechTime = 1f;

	public bool victimWasGrabbedSpriteOnce;

	private float timeWaiting;

	public float dashTime;

	public string initialDashButton;

	public int initialDashDir;


	public int appliedDamage = 0;	

	public float floatSpeed = 0;

	public float smokeTime;

private float yDist;

private float xDist;

public bool isGroundAttack = true;

	 private float topDelay;

	private int upHitCount;

	private int downHitCount;



	public VileGrabState(Actor victim)
		: base("grab", "", "", "")
	{
		this.victim = victim;
		grabTime = 7f;
	}
	public override void update()
	{
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;

	/*	if (character.ownedByLocalPlayer && (character as Vile).isVileMK5) {
			if (base.player.input.isHeld("up", base.player) && !character.sprite.name.EndsWith("air_grab")) {
				character.changeSprite("vilemk5_air_grab", true);
				character.useGravity = false;
				if (getVictim() != null) {
					character.useGravity = false;
					victim.zIndex = character.zIndex + 100;
				}
			}
			float yDir = player.input.checkYDir(player);
			if (yDir != 0) {
				character.useGravity = false;
				floatSpeed += 10 * yDir;
				if (yDir > 0 && floatSpeed > 200) {
					floatSpeed = 100;
					character.useGravity = false;
				} else if (yDir < 0 && floatSpeed < -200) {
					floatSpeed = -100;
					character.useGravity = false;
				}
			} else {
				floatSpeed -= 20 * MathF.Sign(floatSpeed);
			}
			if (yDir == -1 && character.grounded) {
				character.move(new Point(0, -4), false);
			}

			if (floatSpeed != 0) {
				character.move(new Point(0, floatSpeed));
			}
		}
	*/
		if (victimWasGrabbedSpriteOnce && getVictim() == null)
		{
			character.changeToIdleOrFall();
			return;
		}
		if (getVictim() != null || victim != null && victim.sprite.name.EndsWith("_die")) {
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce)
		{
			timeWaiting += Global.spf;
			if (timeWaiting > 1f)
			{
				victimWasGrabbedSpriteOnce = true;
			}
			if (character.isDefenderFavored()) {
				if (leechTime > 0.5f)
				{
					leechTime = 0f;
			character.addHealth(1f);
			
				}
				return;
			}
		}
		if (leechTime > 0.4f)
		{
			leechTime = 0f;
			character.addHealth(1f);
		}
		if (grabTime <= 0f) {
			character.changeToIdleOrFall();
			return;
	//	}
	//	if (appliedDamage == 2) { appliedDamage = 3; }
	//		{
	//		character.changeState(new Idle(), forceChange: true);
		}
		
		//Golden Right
		if ((character as Vile).isVileMK1 && base.player.input.isHeld("up", base.player) 
		&& base.player.input.isPressed("weaponright", base.player))
					{
		character.changeSprite("vile_fist_grab", true);
		character.playSound("hurt", forcePlay: false, sendRpc: true);
		CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 20, -5f);
			if (victim is IDamagable damagable) {
				new Damager(player, 2f, 0, 3f).applyDamage(
					damagable, weakness: false, new VileGrab(), character, (int)ProjIds.VileGrab
				);
			}
					}
					//Scum Vanquisher
		if ((character as Vile).isVileMK1 && player.input.isHeld("right", player) 
		&& base.player.input.isPressed("weaponright", player))
					{
		character.changeSprite("vile_kick_grab", true);
		character.playSound("hurt", forcePlay: false, sendRpc: true);
			if (victim is IDamagable damagable) {
				new Damager(player, 2f, 0, 3f).applyDamage(
					damagable, weakness: false, new VileGrab(), character, (int)ProjIds.VileGrab
				);
			}
					}
					//Die.
		if ((character as Vile).isVileMK2 && base.player.input.isHeld("right", base.player) 
		&& base.player.input.isPressed("weaponright", base.player))
					{
		character.changeSprite("vilemk2ex_stab_grab", true);
		character.playSound("hurt", forcePlay: false, sendRpc: true);
			if (victim is IDamagable damagable) {
				new Damager(player, 2f, 0, 3f).applyDamage(
					damagable, weakness: false, new VileGrab(), character,   (int)ProjIds.VileGrab
				);
			}
					}
				
					//Die. MKv version
		if (  (character as Vile).isVileMK5 && base.player.input.isHeld("right", base.player) 
		&& base.player.input.isPressed("weaponright", base.player))
					{
		if ((character as Vile).isVileMK5)		{
		character.changeSprite("vilemk5_gravity_grab", true);
		}
		if (!(character as Vile).isVileMK5)		{
		character.changeSprite("vilemkv_gravity_grab", true);
		}
		character.playSound("hurt", forcePlay: false, sendRpc: true);
			if (victim is IDamagable damagable) {
				new Damager(player, 2f, 0, 3f).applyDamage(
					damagable, weakness: false, new VileGrab(), character,   (int)ProjIds.VileGrab
				);
			}
					}
					//Raging Demon
		if (  (character as Vile).isVileMK1 && player.currency > 7 && base.player.input.isHeld("special2", base.player) && base.player.input.isPressed("weaponright", base.player))
					{
		character.changeSprite("vile_ragingdemon_grab", true);
		character.playSound("hurt", forcePlay: false, sendRpc: true);
			if (victim is IDamagable damagable) {
				new Damager(player, 1f, 0, 0.25f).applyDamage(
					damagable, weakness: false, new VileGrab(), character,   (int)ProjIds.VileGrab
				);
			}
			}//MKV Special UNlocked
			if (  (character as Vile).isVileMK5
			&& base.player.input.isPressed("special2", base.player)
			&& vile.vilegrabextraCooldown == 0f)
		{
			Point? shootPos = character.getFirstPOI();
          character.changeSprite("vilemk5_super_shoot", true);
	      vile.vilegrabextraCooldown = 1.5f;
			new GBeetleGravityWellProj(player.weapon, character.getShootPos(),
			 character.xDir, chargeTime, base.player, base.player.getNextActorNetId()
			 , sendRpc: true);
					}
		//Violent Crusher Test
		if (base.player.input.isHeld("down", base.player) && base.player.input.isPressed("weaponright", base.player))
					{
		character.changeState(new Idle(), forceChange: true);
		character.playSound("triadThunderCharged", forcePlay: false, sendRpc: true);
		 new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(6 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
					}
					//Rising Death Test
    if (((character as Vile).isVileMK2 ||
	 (character as Vile).isVileMK2 || 
	 (character as Vile).isVileMK5EX) 
	 && base.player.input.isPressed("up", base.player))
	     {
		if ( !(character as Vile).isVileMK5EX){
		character.changeSprite("vilemk2ex_air_grab", true);
		}
		if ( (character as Vile).isVileMK5EX){
		character.changeSprite("vilemkv_rising_grab", true);
		}
		character.vel.y = 0f - character.getJumpPower();
		 }
	
		if (  ((character as Vile).isVileMK2 
		|| (character as Vile).isVileMK2 || 
		(character as Vile).isVileMK5EX) 
		&& base.player.input.isHeld("up", base.player))
		{
		character.useGravity = true;	
		float speed = 300f;
		float yFactor = 1f;;
		CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 20, -5f);
		if (collideData != null && collideData.isCeilingHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.shakeCamera(sendRpc: true);
			character.changeState(new Idle());
		}
		if (collideData != null && collideData.isGroundHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.shakeCamera(sendRpc: true);
			character.changeState(new Idle());
		}
		if (state == 2)
		{
			yFactor = -1f;
		}
		Point moveAmount = new Point(character.xDir * 50, (0f - speed) * yFactor);
		if (state != 1)
		{
			character.move(moveAmount);
			yDist += Global.spf * speed;
		}
		if (state == 0)
		{
			if (base.player.input.isHeld("jump", base.player))
			{
				reverse();
			}
			return;
		}
		if (state == 1)
		{
			topDelay += Global.spf;
			if (topDelay > 0.1f)
			{
				state = 2;
			}
			return;
		}
		}

		//Godpress
if (  ((character as Vile).isVileMK2 || 
(character as Vile).isVileMK2 || 
(character as Vile).isVileMK5EX) 
&& base.player.input.isHeld("special2", base.player) 
&& base.player.input.isHeld("dash", base.player)){
	sprite = "godpress_grab";
	character.changeSpriteFromName("godpress_grab", resetFrame: true);
		if (  ((character as Vile).isVileMK2 || 
		(character as Vile).isVileMK2 || 
		(character as Vile).isVileMK5EX) && base.player.input.isHeld("dash", base.player)
		 && character.sprite.name.EndsWith("godpress_grab"))
		{
			character.isDashing = true;
			character.move(new Point(character.xDir * 200, 0f));
			character.useGravity =  false;	
		CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 20, -5f);
		if (collideData != null && collideData.isSideWallHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.shakeCamera(sendRpc: true);
			character.changeState(new Idle());
		}
		}
		}
		//MKV Ride the Lightning
if (  (character as Vile).isVileMK5 
&& !(character as Vile).isVileMK5EX 
&& base.player.input.isPressed("jump", base.player)
 && base.player.input.isPressed("weaponright", base.player))
 {
	 character.vel.y = 0f - character.getJumpPower();
	character.changeSprite("vilemk5_rising_grab", true);
 }
		if ( ((character as Vile).isVileMK5EX 
		|| (character as Vile).isVileMK5) 
		&& base.player.input.isHeld("jump", base.player) && base.player.input.isHeld("weaponright", base.player))
		{
			character.useGravity = true;	
		float speed = 400f;
		float yFactor = 1f;
	CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 20, -5f);
		if (collideData != null && collideData.isCeilingHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.shakeCamera(sendRpc: true);
			character.changeState(new Idle());
		}
		if (  ((character as Vile).isVileMK5EX 
		|| (character as Vile).isVileMK5) && state == 2)
		{
			yFactor = -5f;
		}
		Point moveAmount = new Point(character.xDir * 50, (0f - speed) * yFactor);
		if ( (character as Vile).isVileMK5 && state != 1)
		{
			character.move(moveAmount);
			yDist += Global.spf * speed;
		}
		if ((character as Vile).isVileMK5 && state == 0)
		{
			character.changeSprite("vilemk5_spin_grab", true);
		}
		if ( (character as Vile).isVileMK5 && state == 1)
		{
			topDelay += Global.spf;
			if (topDelay > 0.1f)
			{
				state = 2;
			}
			return;
		}
		}
		if (vile.isVileMK5 && character.grounded && character.sprite.name.EndsWith("air_grab")) {
   CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 1, 1f);
		if (  collideData != null 
		&& collideData.isCeilingHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.playSound("hexaInvolute", forcePlay: false, sendRpc: true);}
			character.shakeCamera(sendRpc: true);
			crashAndDamage();
			character.changeState(new Idle());
		
		}
		if ( character.grounded && character.sprite.name.Contains("rising")) {
   CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 1, 1f);
		if (  collideData != null 
		&& collideData.isCeilingHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.playSound("hexaInvolute", forcePlay: false, sendRpc: true);}
			character.shakeCamera(sendRpc: true);
			crashAndDamage();
			character.changeState(new Idle());
		
		}
		if ( character.grounded && character.sprite.name.EndsWith("spin_grab")) {
   CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 1, 1f);
		if (  collideData != null 
		&& collideData.isGroundHit())
		{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.playSound("hexaInvolute", forcePlay: false, sendRpc: true);}
			character.shakeCamera(sendRpc: true);
			crashAndDamage();
			character.changeState(new Idle());
		}
	}


	public void reverse()
	{
		if (((character as Vile).isVileMK2 
		|| (character as Vile).isVileMK2 
		|| (character as Vile).isVileMK5EX) && state == 0)
		{
			state = 1;
			if ((character as Vile).isVileMK2 
			|| (character as Vile).isVileMK2) {
			character.changeSprite("vilemk2ex_risingdeath_grab", true);
			}
			if ((character as Vile).isVileMK5EX) {
			character.changeSprite("vilemkv_spin_grab", true);
			}
		}
	}


	public void crashAndDamage()
	{
		if (victim is IDamagable damagable) {
				new Damager(player, 2f, 0, 1f).applyDamage(
					damagable, weakness: false, new VileGrab(), character,   (int)ProjIds.VileGrab
				);
		}
		new BBuffaloCrashProj(player.weapon, character.pos, character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		character.playSound("crash", forcePlay: false, sendRpc: true);
		character.shakeCamera(sendRpc: true);

		if (  (character as Vile).isVileMK5EX && base.player.input.isHeld("dash", base.player))
	 {
		Point spawnPos2 = character.pos.addxy(0 * character.xDir, -150f);
		 new DynamoBeam(new Napalm(NapalmType.SplashHit), victim.pos, character.xDir, player, player.getNextActorNetId(), sendRpc: true);
	
		}
	}


	public Actor getVictim() {
		if (victim == null) {
			return null;
		}
		if (victim is Character && victim.sprite.name.EndsWith("_grabbed")) {
			return victim;
		}
		if (victim is Maverick mvrk && mvrk.isInGrabState) {
			return victim;
		}
		return null;
	}

	public override void onEnter(CharState oldState)
	{
		base.onEnter(oldState);
	
			character.useGravity = false;
		
	}
	

	public override void onExit(CharState newState)
	{
		base.onExit(newState);
		character.grabCooldown = 1f;
		character.useGravity = true;
		(victim as Character).releaseGrab(character, true);
	
    
/*
if (  vile.isVileMK5EX && player.currency > 11 && base.player.input.isHeld("dash", base.player))
	 {
		Point spawnPos2 = character.pos.addxy(0 * character.xDir, -150f);
		 new DynamoBeam(new Napalm(NapalmType.SplashHit), victim.pos, character.xDir, player, player.getNextActorNetId(), sendRpc: true);
	
		}
		
		//extras
	 if (  (vile.isVileMK5EX || vile.isVileMK5) && base.player.input.isHeld("special2", base.player))
	 {
		Point spawnPos2 = character.pos.addxy(0 * character.xDir, -150f);
		new WolfSigmaBeam(player.weapon, spawnPos2, character.xDir, 1, 2, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		
		 if (  vile.isVileMK1 && base.player.input.isHeld("special2", base.player))
	 {
		character.playSound("crash", forcePlay: false, sendRpc: true);
		character.useGravity = true;
		 new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		 if (  vile.isVileMK2 && base.player.input.isHeld("up", base.player))
	 {
		character.playSound("crash", forcePlay: false, sendRpc: true);
		character.useGravity = true;
		 new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		 if (  vile.isVileMK2 && base.player.input.isHeld("dash", base.player))
	 {
		float xSpeed = 100f;
		character.playSound("crashX3", forcePlay: false, sendRpc: true);
		new VelGIceProj(new VelGIceWeapon(), character.pos.addxy(0 * character.xDir, 0f), character.xDir, new Point(xSpeed * (float)character.xDir, -200f), base.player, base.player.getNextActorNetId(), rpc: true);
		new VelGIceProj(new VelGIceWeapon(), character.pos.addxy(0 * character.xDir, 0f), character.xDir, new Point(xSpeed * (float)character.xDir, -200f), base.player, base.player.getNextActorNetId(), rpc: true);
		new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		 if (  vile.isVileMK5 && base.player.input.isHeld("up", base.player))
	 {
		character.playSound("crashX2", forcePlay: false, sendRpc: true);
		 new MechFrogStompShockwave(new MechFrogStompWeapon(base.player), character.pos.addxy(-10 * character.xDir, 0f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
					}
	}*/
	}
}


