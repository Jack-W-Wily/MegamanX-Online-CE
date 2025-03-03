
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
	public float initTime;
	public Anim? anim;


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




