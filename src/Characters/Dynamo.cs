namespace MMXOnline;
using System;
using System.Diagnostics.CodeAnalysis;
using SFML.Graphics;

public class Dynamo : Character {
	public Dynamo(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.Dynamo;
	}

	private float ItemThrowCooldown;

	private float CrouchTime;

	private float SlashCooldown;

	public int backFlipCount = 0;

	public int uppercutCount = 0;

	public int airShotCount = 0;

	public int DaggerCount = 0;

	private float DaggerCooldown;




public override bool normalCtrl() {
	
		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}

		if (player.input.isPressed(Control.Jump,player) && !grounded
		&& charState is not Jump && backFlipCount == 0){
			changeState(new DynamoBackFlip());
			backFlipCount = 1;
		}

		bool hadokenCheck = false;
		bool shoryukenCheck = false;
		hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		
		if (hadokenCheck && SlashCooldown == 0) {
			changeState(new DynamoBladeDash(), true);
			SlashCooldown = 1.5f;
		}

		return base.normalCtrl();
	}

	
public override bool attackCtrl() {
	
		

		if (player.input.isPressed(Control.Shoot,player) && 
		!player.input.isHeld(Control.Down,player)){
			changeState(new DynamoWhippAttack(), true);
		}
		if (player.input.isPressed(Control.Shoot,player) && grounded &&
		player.input.isHeld(Control.Down,player)){
			changeState(new DynamoBladeSlash(), true);
		}
		if (player.input.isPressed(Control.Shoot,player) && !grounded &&
		player.input.isHeld(Control.Down,player)){
			changeState(new DynamoBladeSlashAir(), true);
		}


		if (player.input.isPressed(Control.Shoot,player) && downPressedTimes > 0 &&
		uppercutCount == 0 &&
		player.input.isHeld(Control.Up,player)){
			changeState(new DynamoUpperCut(), true);
			uppercutCount = 1;
		}


			if (player.input.isPressed(Control.Special1,player) && 
		!player.input.isHeld(Control.Down,player )&&
			!player.input.isHeld(Control.Up,player)  && ItemThrowCooldown ==0
		){
			changeState(new DynamoCross(), true);
			ItemThrowCooldown = 1.2f;
		}


		if ((player.input.isHeld(Control.Left, player) 
		|| player.input.isHeld(Control.Right, player)) &&
			player.input.isPressed(Control.Special1,player) && 
		!player.input.isHeld(Control.Down,player )&&
			!player.input.isHeld(Control.Up,player)  && ItemThrowCooldown ==0
		){
			changeState(new DynamoCross(), true);
			ItemThrowCooldown = 1.2f;
		}

		if ((!player.input.isHeld(Control.Left, player) 
		&& !player.input.isHeld(Control.Right, player)) &&
			player.input.isPressed(Control.Special1,player) && 
		!player.input.isHeld(Control.Down,player )
		&&	!player.input.isHeld(Control.Up,player) 
		){
			changeState(new DynamoBoomerang(), true);
		}


		if (player.input.isPressed(Control.WeaponLeft,player) && 
		!player.input.isHeld(Control.Down,player )&& DaggerCooldown == 0 &&
			!player.input.isHeld(Control.Up,player)  
		){
			changeState(new DynamoDaggerLV1(), true);
			DaggerCount += 1;

			if (DaggerCount > 5){
			DaggerCooldown = 1.5f;
			}
		}

		if (player.input.isPressed(Control.WeaponLeft,player) && 
		!player.input.isHeld(Control.Down,player )&&
			player.input.isHeld(Control.Up,player)  && player.currency > 0
		){
			changeState(new DynamoDaggerLV2(), true);
			player.currency -= 1;

			
		}

		if (player.input.isPressed(Control.Special1,player) && 
		player.input.isHeld(Control.Down,player )&&
			!player.input.isHeld(Control.Up,player) 
			&& grounded
		){
			changeState(new DynamoGPChargeState(), true);
		}

		if (player.input.isPressed(Control.Special1,player) && 
		player.input.isHeld(Control.Down,player )&&
			!player.input.isHeld(Control.Up,player) 
			&& !grounded && airShotCount == 0
		){
			changeState(new DynamoAirShotState(), true);
			airShotCount = 1;
		}

		bool hadokenCheck = false;
		bool shoryukenCheck = false;
		hadokenCheck = player.input.checkHadoken(player, xDir, Control.Shoot);
		shoryukenCheck = player.input.checkShoryuken(player, xDir, Control.Shoot);
		
		if (hadokenCheck && SlashCooldown == 0) {
			changeState(new DynamoBladeDash(), true);
			SlashCooldown = 1.5f;
		}
		
		return base.attackCtrl();
	}



	public override void update(){
		base.update();

		if (charState is Dash &&
		player.input.isHeld(Control.Down, player)
		){
			changeState(new DynamoSlide());
		}


			if (DaggerCooldown == 0){
			DaggerCount = 0;
			}

		if (player.currency > 1 && player.input.isHeld(Control.Up, player)
		&& player.input.isPressed(Control.Special2, player)){
		player.currency -= 2;
		new DarkHoldDProj(new DarkHoldWeapon(), pos, xDir, player, player.getNextActorNetId(), rpc: true);
		playSound("dynamoting", forcePlay: false, sendRpc: true);
		changeState(new Idle());
		playSound("dynamoUltraCross1", forcePlay: false, sendRpc: true);
		}
		
		


		//Cooldowns
		Helpers.decrementTime(ref DaggerCooldown);
		Helpers.decrementTime(ref ItemThrowCooldown);
		Helpers.decrementTime(ref SlashCooldown);

		if (grounded || charState is Idle or WallSlide){
			backFlipCount = 0;
			uppercutCount = 0;
			airShotCount = 0;
		}


		// MicroDash
			if ((charState is Dash || charState is AirDash) 
			&& (player.input.isPressed(Control.Shoot, player)
			|| player.input.isPressed(Control.Special1, player))){
			slideVel = xDir * getDashSpeed();			
			}
		
		


		

		
	}


// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new SonicSlicer(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}

		if (  sprite.name.Contains("blade_attack") && !sprite.name.Contains("air"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.ZSaber2, player, 3f, 20, 15, ShouldClang : true);
		}

		if (  sprite.name.Contains("blade_attack_air"))
		{
			return new GenericMeleeProj(new SonicSlicer(), centerPoint,
			 ProjIds.DynamoDropSlash, player, 3f, 25,15f, ShouldClang : true);
		}
		
		if (  sprite.name.Contains("whipattack"))
		{
			return new GenericMeleeProj(new StrikeChain(), centerPoint,
			 ProjIds.ZSaber1, player, 2f, 15, 15f, ShouldClang : true
			 , isJuggleProjectile : true);
		}

		if (  sprite.name.Contains("uppercut"))
		{
			return new GenericMeleeProj(new StrikeChain(), centerPoint,
			 ProjIds.ZSaber1, player, 2f, 15, 10f, ShouldClang : true
			 , isJuggleProjectile : true);
		}

		if (  sprite.name.Contains("slide_jump"))
		{
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.GBDKick, player, 2f, 15, 15f, ShouldClang : true
			 , isJuggleProjectile : true);
		}

		if (!sprite.name.Contains("charge") && sprite.name.Contains("groundpunch"))
		{
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.MechFrogStompShockwave, player, 3f, 0, 15f, ShouldClang : true
			 , isJuggleProjectile : true);
		}
			if (sprite.name.Contains("charge") && sprite.name.Contains("groundpunch"))
		{
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.ForceGrabState, player, 0f, 0, 15f, ShouldClang : true
			);
		}

			if (sprite.name.Contains("bladedash"))
		{
			return new GenericMeleeProj(new ShotgunIce(), centerPoint,
			 ProjIds.VileSuperKick, player, 0f, 0, 15f, ShouldClang : true
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
			&& Global.sprites.ContainsKey("dynamoalt_" + spriteName)){		
			return "dynamoalt_" + spriteName;
			}
			return "dynamo_" + spriteName;
	}
}

