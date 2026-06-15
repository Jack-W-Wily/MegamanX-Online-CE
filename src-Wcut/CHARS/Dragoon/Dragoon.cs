namespace MMXOnline;
using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;
using System.Collections.Generic;

public class Dragoon : Character {
	public Dragoon(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true, int? heartTanks = null, bool isATrans = false
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, heartTanks, isATrans
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

	public int airSpinkick = 0;

	private float Hadouken;




	

	// For Shaders stuff
	public override List<ShaderWrapper> getShaders() {
		List<ShaderWrapper> baseShaders = base.getShaders();
		List<ShaderWrapper> shaders = new();
		ShaderWrapper? palette = null;



		if (player.skinSlot == 1) {
			palette = player.nightmareZeroShader;
		}
		if (player.skinSlot == 2) {
			palette = player.nightmareZeroShader2;
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

public override bool normalCtrl() {
	
		if (player.input.isL2Held(player) &&
			!isAttacking() && grounded &&
			charState is not BlockWCUT
		) {
			changeState(new BlockWCUT(), true);
			
		}
		if (player.input.isL2Held(player) && player.input.isPressed(Control.Dash, player)) {
			changeState(new WcutGenericDodgeF(), true);	
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
			if (OverDrive){
			charState.invincible = true;
			} else {
				iframesTime = 8;
			}
		}
		if (hadokenCheck2) {
			changeState(new DragoonHadouken(), true);
			if (OverDrive){
			charState.invincible = true;
			} else {
				iframesTime = 8;
			}
		}
		if (shoryukenCheck) {
			changeState(new DragoonRising(), true);	
			if (OverDrive){
			charState.invincible = true;
			} else {
				iframesTime = 8;
			}
		}
		if (shoryukenCheck2) {
			changeState(new DragoonShoryuken(isUnderwater()), true);	
			if (OverDrive){
			charState.invincible = true;
			} else {
				iframesTime = 8;
			}
		}
		if (senpukiakuCheck) {
			changeState(new DragoonSenpukiaku(), true);	
			if (OverDrive){
			charState.invincible = true;
			} else {
				iframesTime = 8;
			}
		}


		if (player.input.isL2Held(player) && player.input.isAPressed(player) ){
		changeState(new GlobalParryState(), true);
		
		}
	
	

		return base.normalCtrl();
	}

	
public override bool attackCtrl() {
	
		

		if (player.input.isPressed(Control.Shoot,player)) {
			
			if (!player.input.isHeld(Control.Down,player)){
		
			changeState(new DragoonPunchState(), true);
			} else {
			changeState(new DragoonLowPunchState(), true);
			}
		}
		if (player.input.isPressed(Control.Special1,player) && 
		!player.input.isHeld(Control.Down,player)){
			changeState(new DragoonPunchState2(), true);
		}
		if (player.input.isPressed(Control.Special1,player) && 
		player.input.isHeld(Control.Down,player)){
			changeState(new DragoonUppercut(), true);
		}
		if (player.input.isPressed(Control.WeaponRight,player) && 
		!player.input.isHeld(Control.Down,player)){
			changeState(new DragoonKickState(), true);
		}
		if (player.input.isPressed(Control.WeaponRight,player) && !grounded &&
		player.input.isHeld(Control.Down,player)){
			changeState(new DragoonDiveKick(), true);
		}




		if (player.input.isR2Pressed(player)) {
            if (player.input.isHeld(Control.Up, player) && AirShoryuken == 0) {
            AirShoryuken = 1;
			 if (player.input.isLeftOrRightHeld(player)) {
                changeState(new DragoonRising(), true);	
          	 	} else {
                changeState(new DragoonShoryuken(isUnderwater()), true);
                }
				
            } else if (player.input.isHeld(Control.Down, player)) {
            changeState(new DragoonHadoukenCrouch(), true);	    
            } else {
                if (player.input.isLeftOrRightHeld(player)) {
					if (airSpinkick ==0){
                changeState(new DragoonSpinkick(), true);	
				airSpinkick = 1;
					}
          	 	} else {
                changeState(new DragoonHadouken(), true);	
                }
            }
        }



		return base.attackCtrl();
	}


	

	

	public override bool spcCancel() {

		// JumpCancel
		if (player.input.isPressed(Control.Jump, player) && canJump()) {
				vel.y = -getJumpPower();
				isDashing = true;
				changeState(getJumpState());
				return true;
		} 
		if (player.input.isPressed(Control.WeaponRight,player) && !grounded &&
		player.input.isHeld(Control.Down,player)){
			changeState(new DragoonDiveKick(), true);
		}


		

		return base.spcCancel();
	}



	public override void update(){
		base.update();


		if (grounded || charState is WallSlide or WallKick or InRideArmor) {
			airSpinkick = 0;
			AirShoryuken = 0;
		}
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
				new SonicSlicer(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true, addToLevel: true
			);
		}

			if (sprite.name.Contains("parry_start"))
			{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.GenericWCUTGrabProjID, player, 1, 0, 5, addToLevel: true, hitSound : "kofhtsnd_grab1"
			);
		}

		if (sprite.name.Contains("grab"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.HeavyPush, player, 1, 0, 30, addToLevel: true, hitSound : "GDash"
			);
		}


			if (sprite.name.Contains("punch") && !sprite.name.Contains("2") && !sprite.name.Contains("air"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.UPPunch, player, 1, 9, 10, ShouldClang : true, addToLevel: true, hitSound : "kofhtsnd_punch1"
			);
		}
			if (sprite.name.Contains("punch") && !sprite.name.Contains("2") && sprite.name.Contains("air"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.UPPunch, player, 1.5f, 18, 10, ShouldClang : true, addToLevel: true, hitSound : "kofhtsnd_punch1"
			);
		}
			if (sprite.name.Contains("punch") && sprite.name.Contains("2"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 3, 20, 15, ShouldClang : true, addToLevel: true, hitSound : "kofhtsnd_punch2"
			);
		}
			if (sprite.name.Contains("kick") && !sprite.name.Contains("spin")  && !sprite.name.Contains("drop"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 15, 8, ShouldClang : true, addToLevel: true
			);
		}
			if (sprite.name.Contains("kick") && sprite.name.Contains("spin"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 20, 8, ShouldClang : true, addToLevel: true, hitSound : "kofhtsnd_punch3"
			);
		}
			if (sprite.name.Contains("kick") && sprite.name.Contains("drop"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.ForceGrabState, player, 1, 0, 8, ShouldClang : true, addToLevel: true, hitSound : "kofhtsnd_punch4"
			);
		}
			if (sprite.name.Contains("shoryuken") && charState is DragoonRising)
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 1f, 30, 12, ShouldClang : true, isJuggleProjectile : true, addToLevel: true, hitSound : "kofhtsnd_punch4", isLiftProjectile : true
			);
		}


		if ( sprite.name.Contains("senpukiaku"))
		{	
			if (frameIndex < 10){
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.ForceGrabState, player, 2, 0, 8, ShouldClang : true, addToLevel: true, hitSound : "kofhtsnd_punch3"
			);}
			else {
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 3, 20, 30, addToLevel: true, hitSound : "kofhtsnd_megapunch1"
			);
			}
			
			
		}


			if (sprite.name.Contains("shoryuken") && charState is not DragoonRising )
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.FireWave, player, 2, 35, 6, ShouldClang : true, isJuggleProjectile : true , addToLevel: true, hitSound : "kofhtsnd_punch4", isLiftProjectile : true
			);
		}
		

		if (  sprite.name.Contains("risingfire"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint, ProjIds.BlockableLaunch, player, 
			5f, 0, 10f, null, isShield: true, isDeflectShield: true , addToLevel: true, hitSound : "kofhtsnd_megapunch1"
			);
		}

		if (  sprite.name.Contains("uppercut"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint, ProjIds.BlockableWeakLaunch, player, 
			2f, 0, 10f, null, isShield: true, isDeflectShield: true , addToLevel: true, hitSound : "kofhtsnd_megapunch1"
			);
		}

			if (  sprite.name.Contains("spit"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint, ProjIds.HeavyPush, 
			player, 5f, 0, 10f, null, isShield: true, isDeflectShield: true , addToLevel: true, hitSound : "kofhtsnd_megapunch1"
			);
		}


			if (sprite.name.Contains("ragingdemon"))
		{
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.RagingDemon, player, 0f, 0, 15f, addToLevel: true, hitSound : "kofhtsnd_megapunch1"
			);
		}

			if (sprite.name.Contains("air_raid"))
		{
			return new GenericMeleeProj(new FireWave(), centerPoint,
			 ProjIds.DistanceNeedler, player, 15, 30, 1, addToLevel: true
			);
		}
		
		
	
		return null;
	}


	public override bool canDash() {
		return flag == null;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		//if ((Options.main.enableSkins == true)
		//	&& Global.sprites.ContainsKey("magmadragoonalt_" + spriteName)){		
		//	return "magmadragoonalt_" + spriteName;
		//	}
			return "magmadragoon_" + spriteName;
	}


	public override void aiAttack(Actor? target) {
		
		if (charState is LadderClimb || !charState.attackCtrl || isInvulnerable()) {
			return;
		}
		bool isTargetInAir = pos.y < target?.pos.y - 20;
		bool isTargetClose = pos.x < target?.pos.x - 40;
		
		
		int AIAttack = Helpers.randomRange(0, 9);
		if (aiAttackCooldown == 0 ){
		switch (AIAttack) {
			case 1 :
				player.press(Control.Shoot);
										
				break;
			
			case 2 :
				player.press(Control.Special1);
			break;
			case 3:
				player.press(Control.R2);
			break;
			case 4 :
				player.press(Control.L2);
			break;
			case 5:
				player.press(Control.Shoot);
			break;
			case 6:
				player.press(Control.Special2);
			break;
			case 7:
				player.press(Control.WeaponLeft);
			break;
			case 8:
				player.press(Control.WeaponRight);
			break;
			
			default:
				player.press(Control.Shoot);
				
				break;
			
				
			}
			aiAttackCooldown = Helpers.randomRange(20,60);
		}


	}


}



