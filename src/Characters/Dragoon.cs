namespace MMXOnline;
using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

public class Dragoon : Character {
	public Dragoon(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.Dragoon;
		spriteFrameToSounds["magmadragoon_run/3"] = "dragoonfall_2";
		spriteFrameToSounds["magmadragoon_run/8"] = "dragoonfall_2";
		player.superAmmo = 0;
	}

	private float ItemThrowCooldown;

	private float CrouchTime;

	private float AttCooldown;


	public int AirShoryuken = 0;

	private float Hadouken;




public override bool normalCtrl() {
	
		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}

		
		
		if (player.input.isHeld(Control.Up, player)&&
		player.input.isHeld(Control.Special2, player)&&
		player.superAmmo > 13) {
			changeState(new DragoonRisingFire());
			player.superAmmo -= 14;
			
		}

		if (!player.input.isHeld(Control.Up, player)&&
		player.input.isHeld(Control.Special2, player)&&
	
			player.superAmmo > 13) {
			changeState(new DragoonSpitFire());
			player.superAmmo -= 14;
			
		}
		
		bool hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		bool hadokenCheck2 = player.input.checkHadoken(player, xDir, Control.Special1);
		bool shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		bool shoryukenCheck2 = player.input.checkShoryuken(player, xDir, Control.Special1);
		bool senpukiakuCheck = player.input.checkHadoken(player, xDir, Control.WeaponRight);
		

		
		
		if (hadokenCheck) {
			changeState(new DragoonHadoukenCrouch(), true);	
		}
		if (hadokenCheck2) {
			changeState(new DragoonHadouken(), true);	
		}
		if (shoryukenCheck) {
			changeState(new DragoonRising(), true);	
		}
		if (shoryukenCheck2) {
			changeState(new DragoonShoryuken(isUnderwater()), true);	
		}
		if (senpukiakuCheck) {
			changeState(new DragoonSenpukiaku(), true);	
		}


		if (player.input.isPressed(Control.WeaponLeft,player) ){
		changeState(new GlobalParryState(), true);
		}
	
	

		return base.normalCtrl();
	}

	
public override bool attackCtrl() {
	
		

		if (player.input.isPressed(Control.Shoot,player) && 
		!player.input.isHeld(Control.Down,player)){
			changeState(new DragoonPunchState(), true);
		}
		if (player.input.isPressed(Control.Special1,player) && 
		!player.input.isHeld(Control.Down,player)){
			changeState(new DragoonPunchState2(), true);
		}
		if (player.input.isPressed(Control.WeaponRight,player) && 
		!player.input.isHeld(Control.Down,player)){
			changeState(new DragoonKickState(), true);
		}

		if (player.input.isPressed(Control.WeaponRight,player) && !grounded &&
		player.input.isHeld(Control.Down,player)){
			changeState(new DragoonDiveKick(), true);
		}



		return base.attackCtrl();
	}



	public override void update(){
		base.update();



		//avoid issues like over gaining ammo and over losing ammo
		if(player.superAmmo > player.superMaxAmmo){
			player.superAmmo = player.superMaxAmmo;
		}
		if(player.superAmmo < 0){
			player.superAmmo = 0;
		}


		if (sprite.name.Contains("air_raid"))invulnTime = 2;



		if ((player.input.isHeld(Control.Left, player)&&
		player.input.isHeld(Control.Right, player)) &&
		shootPressedTimes > 1 && wRightPressedTimes > 0 &&
		specialPressedTimes > 0 &&
			player.currency > 9
		) {
			changeState(new RagingDemon());
			player.currency -= 10;
			
		}




		//Cooldowns
		Helpers.decrementTime(ref AttCooldown);
		Helpers.decrementTime(ref ItemThrowCooldown);
		Helpers.decrementTime(ref CrouchTime);

	

		// MicroDash
			if ((charState is Dash || charState is AirDash)){
			slideVel = xDir * getDashSpeed() *  0.5f;			
			}
		
		


		

		
	}


// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new SonicSlicer(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}

			if (sprite.name.Contains("parry_start"))
			{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.ForceGrabState, player, 1, 0, 5
			);
		}

		if (sprite.name.Contains("grab"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.HeavyPush, player, 1, 0, 30
			);
		}


			if (sprite.name.Contains("punch") && !sprite.name.Contains("2"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 20, 15, ShouldClang : true
			);
		}
			if (sprite.name.Contains("punch") && sprite.name.Contains("2"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 3, 20, 15, ShouldClang : true
			);
		}
			if (sprite.name.Contains("kick") && !sprite.name.Contains("spin")  && !sprite.name.Contains("drop"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 15, 8, ShouldClang : true
			);
		}
			if (sprite.name.Contains("kick") && sprite.name.Contains("spin"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 20, 8, ShouldClang : true
			);
		}
			if (sprite.name.Contains("kick") && sprite.name.Contains("drop"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.MechFrogStompShockwave, player, 3, 20, 8, ShouldClang : true
			);
		}
			if (sprite.name.Contains("shoryuken") && charState is DragoonRising)
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 1f, 20, 12, ShouldClang : true, isJuggleProjectile : true
			);
		}

			if (sprite.name.Contains("shoryuken") && charState is not DragoonRising )
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 20, 6, ShouldClang : true, isJuggleProjectile : true
			);
		}
		

		if (  sprite.name.Contains("risingfire"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint, ProjIds.BlockableLaunch, player, 5f, 0, 10f, null, isShield: true, isDeflectShield: true);
		}

			if (  sprite.name.Contains("spit"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint, ProjIds.HeavyPush, player, 5f, 0, 10f, null, isShield: true, isDeflectShield: true);
		}


			if (sprite.name.Contains("ragingdemon"))
		{
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.VileAirRaidStart, player, 0f, 0, 15f
			);
		}

			if (sprite.name.Contains("air_raid"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.DistanceNeedler, player, 15, 30, 1
			);
		}
		
		
	
		return null;
	}


	public override bool canDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		if ((Options.main.enableSkins == true)
			&& Global.sprites.ContainsKey("magmadragoonalt_" + spriteName)){		
			return "magmadragoonalt_" + spriteName;
			}
			return "magmadragoon_" + spriteName;
	}
}



