using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public class VileMK2Grab : Weapon {
	public VileMK2Grab() : base() {
		rateOfFire = 0.75f;
		index = (int)WeaponIds.VileMK2Grab;
		killFeedIndex = 63;
	}
}

public class VileMK2GrabState : CharState {
	public Character victim;
	float leechTime = 1;

	public Vile vile;

	float hitcd = 1;

	bool firstHeal;

	private bool usechaingrab;

	public bool victimWasGrabbedSpriteOnce;
	float timeWaiting;


	
    public Napalm weapon;

	private float partTime;

	private float chargeTime;


     private int state; 

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


	public VileMK2GrabState(Character victim) : base("grab", "", "", "") {
		this.victim = victim;
		grabTime = VileMK2Grabbed.maxGrabTime;
	}

	bool SpawnShockwave;

	bool TriggerGroundColision;

	int combonum = 0;

	public override void update() {
		base.update();
		grabTime -= Global.spf;
		leechTime += Global.spf;
		hitcd += Global.spf;


		if (victimWasGrabbedSpriteOnce && victim == null) {
			character.changeState(new Idle(), true);
			return;
		}

		if (player.isVile && player.input.isPressed(Control.Shoot,player)){
			combonum += 1;
		
		if (combonum == 1)character.changeSpriteFromName("string_1", true);
		if (combonum == 2)character.changeSpriteFromName("string_2", true);
		if (combonum == 3)character.changeSpriteFromName("string_3", true);
		}

		if (victim.sprite.name.EndsWith("_grabbed") || victim.sprite.name.EndsWith("_die")) {
			victimWasGrabbedSpriteOnce = true;
		}
		if (!victimWasGrabbedSpriteOnce) {
			timeWaiting += Global.spf;
			if (timeWaiting > 1) {
				victimWasGrabbedSpriteOnce = true;
			}
		}
		
		if (player.weapon is StrikeChain && player.isX){
			var damager = new Damager(player, 3f, 0, 0);

			sprite = "ex_chain_grab";
		if (stateTime < 1)character.changeSpriteFromName("ex_chain_grab", true);
			
		if (leechTime > 0.5f) {
			leechTime = 0;
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);			
		}

		if (character.isAnimOver()){
			SpawnShockwave = true;
			character.changeState(new Idle(), true);
			victim?.releaseGrab(character, true);
		}

		}


		if (player.isVile){

			if ((character as Vile).isVileMK4 && player.input.isHeld(Control.Up, player)){
				var damager = new Damager(player, 3f, 0, 0);
				sprite = "ex_chain_grab";
				if (stateTime < 1)character.changeSpriteFromName("ex_chain_grab", true);		
				if (leechTime > 0.5f) {
				leechTime = 0;
				damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);			
				}
				if (character.isAnimOver()){
				SpawnShockwave = true;
				character.changeState(new Idle(), true);
				victim?.releaseGrab(character, true);
				}
			}			
			if (leechTime > 0.8f) {
			leechTime = 0;
			character.addHealth(1);		
			}
			// STab
			if (character.sprite.name.Contains("gravity") && character.frameIndex > 0 && hitcd > 2f){
			hitcd =0;
			var damager = new Damager(player, 2f, 0, 0);
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);	
			}
			if (player.input.isPressed(Control.Left, player) && !character.sprite.name.Contains("raging")) {
			character.changeSpriteFromName("gravity_grab", true);
			sprite = "gravity_grab";
			}
			// Stomp
			if (character.sprite.name.Contains("raging") && character.frameIndex > 5 && hitcd > 0.2f){
			hitcd =0;
			character.playSound("vilestomp", sendRpc: true);
			character.shakeCamera(sendRpc: true);
			var damager = new Damager(player, 0.25f, 0, 0);
			damager.applyDamage(victim, false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab);	
			}
			if (player.input.isPressed(Control.Dash, player) && !character.sprite.name.Contains("raging")) {
			character.changeSpriteFromName("ragingdemon_grab", true);
			sprite = "ragingdemon_grab";
			sprite = "knocked_down";
			SpawnShockwave = true;
			victim.changeSpriteFromName("knocked_down", true);
			}
			if (frameTime >= 2 && player.input.isPressed(Control.Special1, player)) {
			character.changeState(new Idle(), true);
			return;
			}
			if (grabTime <= 0) {
			character.changeState(new Idle(), true);
			return;
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
					damagable, weakness: false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab
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
					damagable, weakness: false, new VileMK2Grab(), character, (int)ProjIds.VileMK2Grab
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
					damagable, weakness: false, new VileMK2Grab(), character,   (int)ProjIds.VileMK2Grab
				);
			}
					}
				
					//Die. MKv version
		if (  (character as Vile).isVileMK5 && base.player.input.isHeld("right", base.player) 
		&& base.player.input.isPressed("weaponright", base.player))
					{
		if ((character as Vile).isVileMK5){
		character.changeSprite("vilemk5_gravity_grab", true);
		}
		if (!(character as Vile).isVileMK5){
		character.changeSprite("vilemkv_gravity_grab", true);
		}
		character.playSound("hurt", forcePlay: false, sendRpc: true);
			if (victim is IDamagable damagable){
				new Damager(player, 2f, 0, 3f).applyDamage(
					damagable, weakness: false, new VileMK2Grab(), character,   (int)ProjIds.VileMK2Grab
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
					damagable, weakness: false, new VileMK2Grab(), character,   (int)ProjIds.VileMK2Grab
				);
			}
		}//MKV Special UNlocked
		if (  (character as Vile).isVileMK5EX
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
		 (character as Vile).isVileMK2EX || 
		 (character as Vile).isVileMK5 ||
		  (character as Vile).isVileMK5EX
		 ) ) {
			if (base.player.input.isPressed("up", base.player)){
				if ((character as Vile).isVileMK2){
				character.changeSprite("vilemk2ex_air_grab", true);
				}
				if ((character as Vile).isVileMK2EX){
				character.changeSprite("vilemk2ex_air_grab", true);
				}
				if ((character as Vile).isVileMK5){
				character.changeSprite("vilemk5_air_grab", true);
				}
				if ((character as Vile).isVileMK5EX){
				character.changeSprite("vilemkv_rising_grab", true);
				}
			character.vel.y = 0f - character.getJumpPower();
			}
			if (base.player.input.isHeld("up", base.player)){
			character.useGravity = true;	
			float speed = 300f;
			float yFactor = 1f;;
				CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 20, -5f);
				if (collideData != null && collideData.isCeilingHit()){
				crashAndDamage();
				character.playSound("crash", forcePlay: false, sendRpc: true);
				character.shakeCamera(sendRpc: true);
				character.changeState(new Idle());
				}
				if (state == 2)	{
				yFactor = -1f;
				}
			Point moveAmount = new Point(character.xDir * 50, (0f - speed) * yFactor);
				if (state != 1)	{
				character.move(moveAmount);
				yDist += Global.spf * speed;
				}
				if (state == 0){
					if (base.player.input.isHeld("jump", base.player))
					{
					reverse();
					}return;}
				if (state == 1){
				topDelay += Global.spf;
					if (topDelay > 0.1f){
					state = 2;
					}return;}
			}
		}
		if (TriggerGroundColision){
			CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 20, -5f);	
		if (collideData != null && collideData.isGroundHit() || character.grounded)
			{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.shakeCamera(sendRpc: true);
			character.changeState(new Idle());
			}
		}
		//Godpress
		if ((
		(character as Vile).isVileMK2 ||
		(character as Vile).isVileMK2EX || 
		(character as Vile).isVileMK5EX || 
		(character as Vile).isVileMK5
		)  
		&& base.player.input.isHeld("special2", base.player)){
		character.changeSpriteFromName("godpress_grab", resetFrame: true);
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
		//MKV Ride the Lightning
		/*if ((character as Vile).isVileMK5 
		&& !(character as Vile).isVileMK5EX 
		&& base.player.input.isPressed("jump", base.player)
 		&& base.player.input.isPressed("weaponright", base.player))
 		{
		 character.vel.y = 0f - character.getJumpPower();
		character.changeSprite("vilemk5_rising_grab", true);
 		}
		if ( ((character as Vile).isVileMK5EX 
		|| (character as Vile).isVileMK5) 
		&& base.player.input.isHeld("jump", base.player))
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
		if ((character as Vile).isVileMK5 && character.grounded && character.sprite.name.EndsWith("air_grab")) {
  			 CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 1, 1f);
			if (collideData != null && collideData.isCeilingHit())
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
			if (collideData != null && collideData.isCeilingHit()){
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.playSound("hexaInvolute", forcePlay: false, sendRpc: true);}
			character.shakeCamera(sendRpc: true);
			crashAndDamage();
			character.changeState(new Idle());
		
		}
		if ( character.grounded && character.sprite.name.EndsWith("spin_grab")) {
  			 CollideData collideData = Global.level.checkCollisionActor(character, character.xDir * 1, 1f);
			if (collideData != null && collideData.isGroundHit())
			{
			crashAndDamage();
			character.playSound("crash", forcePlay: false, sendRpc: true);
			character.playSound("hexaInvolute", forcePlay: false, sendRpc: true);}
			character.shakeCamera(sendRpc: true);
			crashAndDamage();
			character.changeState(new Idle());
		}*/
	}
}

	public void reverse()
	{
		TriggerGroundColision = true;
		if (((character as Vile).isVileMK2 
		
		|| (character as Vile).isVileMK5EX) && state == 0)
		{
			state = 1;
			if ((character as Vile).isVileMK2) {
			character.changeSprite("vilemk2ex_risingdeath_grab", true);
			}
			if ((character as Vile).isVileMK5EX) {
			character.changeSprite("vilemkv_spin_grab", true);
			}
		}
	}

	
	public void crashAndDamage()
	{
		character.playSound("crash", forcePlay: false, sendRpc: true);
		character.shakeCamera(sendRpc: true);
		Point spawnPos2 = character.pos.addxy(0 * character.xDir, -150f);
		new DynamoBeam(new Napalm(NapalmType.SplashHit), victim.pos, character.xDir, player, player.getNextActorNetId(), sendRpc: true);
	}


	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		}


	public override void onExit(CharState newState) {
		base.onExit(newState);
		if (SpawnShockwave){
		new MechFrogStompShockwave(new MechFrogStompWeapon(player), 
		victim.pos.addxy(6 * character.xDir, 0), character.xDir, 
		player, player.getNextActorNetId(), rpc: true);
		character.playSound("crash", sendRpc: true);
		}
		character.grabCooldown = 1;
		character.grabtimeout = 0.15f;
		if (newState is not VileMK2GrabState && victim != null) {
			victim.grabInvulnTime = 0;
			victim?.releaseGrab(character, true);
			
		}
	}
}

public class VileMK2Grabbed : GenericGrabbedState {
	public const float maxGrabTime = 4;

	

	public VileMK2Grabbed(Character grabber) : base(grabber, maxGrabTime, "_grab") {
	}


	public override void update() {
		base.update();
		trySnapToGrabPoint(true);
	}
}
