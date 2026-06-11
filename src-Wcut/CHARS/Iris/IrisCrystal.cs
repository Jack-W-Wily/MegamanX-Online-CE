
namespace MMXOnline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;







public class IrisCrystal : Weapon {
	public float vileAmmoUsage;
	public string projSprite;

		public static IrisCrystal netWeapon = new IrisCrystal();

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

	public bool reversed;
	public float initTime;
	public Anim? anim;

	public float posTimer;


	public NewIrisCrystal(
	Weapon weapon, Point pos, int xDir, Player player, 
		int type, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player, "iris_crystal_bb_behavior", 
		0, 0, netProjId, player.ownedByLocalPlayer
	) { 
		projId = (int)ProjIds.IrisCrystal;
		destroyOnHit = false;
		maxAngleDist = 45;
		returnTime = 0;
		damager.damage = 2;
		damager.flinch = 30;
		damager.hitCooldown = 20;
		this.vel.y = 50;
		angle2 = 0;
		if (xDir == -1) angle2 = -180;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir, new byte[] { (byte)type });
		}

		canBeLocal = false;
	}
	


	public static Projectile rpcInvoke(ProjParameters arg) {
		return new NewIrisCrystal(
			IrisCrystal.netWeapon, arg.pos, arg.xDir, 
			arg.player, arg.extraData[0], arg.netId
		);
	}



	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
	}

	public override void onDestroy() {
		base.onDestroy();
		if (owner.character is Iris irs && irs != null) {
		irs.irisCrystal = null;
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

		if (owner.character.charState is IrisCrystalRisingBash || 
		owner.character.charState is IrisCrystalCharge &&
		owner.input.isHeld(Control.Up, owner) && owner.input.isAPressed(owner)){
			state = 1;
		}
		if (state == 1){
			if (sprite.name != "iris_crystal_bash_up") changeSprite("iris_crystal_bash_up", true);
			if (	owner.character.charState is not IrisCrystalCharge ){
			changePos(owner.character.pos);
			}
		}

		if (owner.character.charState is IrisCrystalBashState  || 
		owner.character.charState is IrisCrystalCharge &&
		!owner.input.isHeld(Control.Up, owner) && owner.input.isAPressed(owner)){
			state = 2;		
		}
		if (state == 2){
			if (sprite.name != "iris_crystal_bash") changeSprite("iris_crystal_bash", true);
			if (	owner.character.charState is not IrisCrystalCharge ){
			changePos(owner.character.pos);
			}
		}

		
		if (owner.character.charState is IrisCrystalCharge ) state = 4;
		if (state == 4) {

		if (owner.input.isHeld(Control.Up, owner)) {
				vel.y = -150;
			} 
			 if (owner.input.isHeld(Control.Down, owner)) {
			    vel.y = 150;
			}
			 if (owner.input.isHeld(Control.Right, owner)) {
				vel.x = 150;
			}
			 if (owner.input.isHeld(Control.Left, owner)) {
				vel.x = -150;
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


		if(  owner.character.charState is  IrisSpawnBeam
		or IrisSpawnIce or IrisSpawnFire) state = 5;
		
		if (state == 5) {
			vel.x = 0;
			vel.y = 0;
		}

		if ((owner.character as Iris).GrabVictim != null) state = 6;
		
		if (state == 6 && (owner.character as Iris).GrabVictim != null) {
			Point GrabPos = (owner.character as Iris).GrabVictim.pos;
			changePos(GrabPos);
		}

		if (owner.character.charState is not IrisCrystalBashState
		&& owner.character.charState is not IrisCrystalRisingBash
		&& owner.character.charState is not IrisSpawnBeam
		&& owner.character.charState is not IrisSpawnIce
		&& owner.character.charState is not IrisGrabEX
		&& owner.character.charState is not IrisSpawnFire
		&& owner.character.charState is not IrisCrystalCharge) {
			state = 0;
		}
		if (state == 0) {
			xPosTimer += Global.spf;

			if (xPosTimer > 0.5f) {
				if (!zposTop) {
				zIndex = owner.character.zIndex - 3;
				zposTop = true;
				} else {
				zIndex = owner.character.zIndex + 3;
				zposTop = false;
				}
				xPosTimer = 0;
			}
			if (!reversed){
			posTimer += Global.spf;
			if (sprite.name != "iris_crystal_bb_behavior")changeSprite("iris_crystal_bb_behavior", false);
			if (posTimer > 2){
			
			reversed = true;
			}
			float x = 20 * MathF.Sin(posTimer * 5);
			yPos = -15 * posTimer;
			Point newPos = owner.character.pos.addxy(x, yPos);
			changePos(newPos);
			} else {
			posTimer -= Global.spf;
			if (sprite.name != "iris_crystal_bb_behavior")changeSprite("iris_crystal_bb_behavior", false);
			if (posTimer <= 0){
			reversed = false;
			}
			float x = 20 * MathF.Sin(posTimer * 5);
			yPos = -15 * posTimer;
			Point newPos = owner.character.pos.addxy(x, yPos);
			changePos(newPos);	
			}
		}
	}

	public float xPosTimer;
	public bool zposTop = false;
}










public class IrisHoverState : CharState {
	float hoverTime;
	
	public IrisHoverState() : base("idle", "idle", "idle", "idle") {
		exitOnLanding = true;
		airMove = true;
		attackCtrl = true;
		normalCtrl = true;
		enterSound = "irisridefly";
	}



	public override void update() {
		base.update();

		if ( character.vel.y < 0 && !player.input.isHeld(Control.Up, player) 
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
		if (!character.sprite.name.Contains("shoot2") && player.input.isHeld(Control.Up, player)){
			if(player.input.isHeld(Control.Dash, player)){
			character.vel.y = -character.getJumpPower() * 1f;
			} else {character.vel.y = -character.getJumpPower() * 0.3f;}
		}
		if (!character.sprite.name.Contains("shoot2") && player.input.isHeld(Control.Down, player)){
			if (player.input.isHeld(Control.Dash, player)){
			character.vel.y = +character.getJumpPower() * 1f;
			} else {character.vel.y = +character.getJumpPower() * 0.3f;}
		}

		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}


		

		hoverTime += Global.spf;
	if ((hoverTime > 10) || hoverTime > 0.2f &&
			character.player.input.isPressed(Control.Jump, character.player)
		) {
			character.changeState(new Fall(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	character.useGravity = false;
	character.vel = new Point();

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

}






public class RAIrisSlashState : CharState {
	float hoverTime;
	
	public RAIrisSlashState() : base("slash", "", "", "") {
		exitOnLanding = false;
		airMove = true;
		enterSound = "rideX4-1";
	}



	public override void update() {
		base.update();

		if ( character.vel.y < 0 && !player.input.isHeld(Control.Up, player) 
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
	
		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}


		

		
	if (character.isAnimOver()
		) {
			character.changeState(new IrisHoverState(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	character.useGravity = false;
	character.vel = new Point();

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

}







public class RAIrisSlashStateRising : CharState {
	float hoverTime;
	
	public RAIrisSlashStateRising() : base("rising", "", "", "") {
		exitOnLanding = false;
		airMove = true;

		enterSound = "rideX4-1";
	}



	public override void update() {
		base.update();

		if ( character.vel.y < 0 && !player.input.isHeld(Control.Up, player) 
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
	
		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}


		

		
	if (character.isAnimOver()
		) {
			character.changeState(new IrisHoverState(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	character.useGravity = false;
	character.vel = new Point();

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

}






public class RAIrisSlashStateReverse: CharState {
	float hoverTime;
	
	public RAIrisSlashStateReverse() : base("slash_reverse", "", "", "") {
		exitOnLanding = false;
		airMove = true;
		invincible = true;
		enterSound = "fakeDoubleCyclone";
	}



	public override void update() {
		base.update();

		if ( character.vel.y < 0 && !player.input.isHeld(Control.Up, player) 
		&& !player.input.isHeld(Control.Down, player)) {
			character.vel.y += Global.speedMul * character.getGravity();
			if (character.vel.y > 0) character.vel.y = 0;
		}
	
		if (character.gravityWellModifier > 1) {
			character.vel.y = 53;
		}


		

		
	if (character.isAnimOver()
		) {
			character.changeState(new IrisHoverState(), true);
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
	character.useGravity = false;
	character.vel = new Point();

	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
	}

}








public class RAIrisKuenzan : CharState {
	
	
	public float soundTime = 0;

	public RAIrisKuenzan() : base("kuenzan", "", "", "") {
	
		immuneToWind = true;
		airMove = true;
	}

	public override void update() {
		base.update();

		if (stateTime > 0.2f){
		character.move(new Point(character.xDir * 450, 0));
		}
		soundTime -= Global.speedMul;
		if (soundTime <= 0) {
			soundTime = 9;
			character.playSound("rideX4-1", sendRpc: true);
		}
	    if (stateTime > 2f) {
			character.changeToIdleOrFall();
			return;
		}

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel.y = 0;
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
}
}




