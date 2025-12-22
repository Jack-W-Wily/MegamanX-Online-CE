using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;
namespace MMXOnline;


public class BanzaiBeetleProj : Projectile {
	public Character? host;
	public BanzaiBeetleProj(
		Weapon weapon, Point pos, int xDir,
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 200, 0, player, "vileproj_banzai_1", 
		0, 0, netProjId, player.ownedByLocalPlayer
	) {
		this.weapon = weapon;
		maxTime = 0.6f;
		projId = (int)ProjIds.BanzaiBeetleProj;
		destroyOnHit = true;
		shouldShieldBlock = true;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new BanzaiBeetleProj(
			new VileMK2Grab(), arg.pos, arg.xDir, arg.player, arg.netId
		);
	}
}


public class BanzaiCarry : CharState {
	bool flinch;
	bool isDone;
	Character otherChar;
	float moveAmount;
	float maxMoveAmount;
	public BanzaiCarry(Character otherChar, bool flinch) : 
	base(flinch ? "hurt" : "fall", flinch ? "" : "fall_shoot", flinch ? "" : "fall_attack"
	) {
		this.flinch = flinch;
		this.otherChar = otherChar;
	}

	public override bool canEnter(Character character) {
		if (!character.ownedByLocalPlayer) return false;
		if (!base.canEnter(character)) return false;
		if (character.isStatusImmune()) return false;
		if (character.isInvulnerable()) return false;
		if (character.charState.superArmor) return false;
		return !character.charState.invincible;
	}

	public override bool canExit(Character character, CharState newState) {
		if (newState is Hurt || newState is Die) return true;
		return isDone;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.grounded = false;
		character.vel.y = 0;
		maxMoveAmount = character.getCenterPos().distanceTo(otherChar.getCenterPos()) * 1.5f;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		player.character.useGravity = true;
	}

	public override void update() {
		base.update();

		if (!character.hasParasite || character.parasiteDamager == null) {
			isDone = true;
			character.changeToIdleOrFall();
			return;
		}

		Point amount = character.getCenterPos().directionToNorm(otherChar.getCenterPos()).times(250);

		character.move(amount);

		moveAmount += amount.magnitude * Global.spf;
		if (character.getCenterPos().distanceTo(otherChar.getCenterPos()) < 5) {
			var pd = character.parasiteDamager;
			pd.applyDamage(character, player.weapon is FrostShield, new VileMK2Grab(), otherChar, (int)ProjIds.ParasiticBombExplode, overrideDamage: 1, overrideFlinch: Global.superFlinch);
			pd.applyDamage(otherChar, player.weapon is FrostShield, new VileMK2Grab(), character, (int)ProjIds.ParasiticBombExplode, overrideDamage: 1, overrideFlinch: Global.superFlinch);
			character.removeBanzai(false, true);
			return;
		} else if (moveAmount > maxMoveAmount) {
			character.removeBanzai(false, false);
			return;
		}
	}
}







public class VMissiLeStance : CharState {

 	public Vile? vile = null;

	public Anim? xh;
	public VMissiLeStance() : base("missile_stance") {

	}

	public override void update() {
		base.update();
		if (vile != null){
		Point shootVel = vile.getVileShootVel(true);
		if (character.frameIndex == 5 && xh == null) {
             xh =  new Anim(
				vile.pos.addxy(8 * vile.xDir,-21),
				"vava_chargebuster_effect", character.xDir, player.getNextActorNetId(), true,
				sendRpc: true
			);
        }
		//int xDir = character.getShootXDir();
	

		if (character.frameIndex > 0) {
			if (player.input.isPressed(Control.Jump, player)) {
				character.changeToIdleOrFall();
			}


			if (player.input.isAPressed(player)  && player.vileAmmo >= 6) {
				player.vileAmmo -= 6;
				character.frameIndex = 5;
				character.playSound("mk2stunshot", true);
				shootPopcorn(vile);
			}

			if (player.input.isBPressed(player) && player.vileAmmo >= 10) {
				player.vileAmmo -= 10;
				character.frameIndex = 5;
				character.playSound("vileMissile", true);
				shootHumerus(vile);
			}

			if (player.input.isR2Pressed(player) && player.vileAmmo >= 20) {
				player.vileAmmo -= 20;
				character.frameIndex = 5;
				character.playSound("vileMissile", true);
				new BanzaiBeetleProj(new VileMK2Grab(), 
			vile.pos.addxy(8 * vile.xDir,-21), character.xDir, player, 
			player.getNextActorNetId(), true);
			}

			if (character.xDir == 1) {
				if (player.input.isPressed(Control.Left, player) && player.input.checkDoubleTap(Control.Left)) {
					character.changeState(new VMissileDash(), true);
				}
			

			}
			if (character.xDir == -1) {
			
				if (player.input.isPressed(Control.Right, player) && player.input.checkDoubleTap(Control.Right)) {
				character.changeState(new VMissileDash(), true);
					}
				
				}
			}
		}
	}



	
	public static void shootPopcorn(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
		bool isMK2 = vile.isVileMK2;
		Point? headPosNullable = vile.getVileMK2StunShotPos();
		if (headPosNullable == null) return;
		Point shootVel = vile.getVileShootVel(true);
		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		int xDir = vile.xDir;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}
		new VileMissileProj(
				vile.pos.addxy(8 * vile.xDir,-21), xDir, 2, MathF.Round(shootVel.byteAngle), "missile_pd_proj",
				vile, vile.player, vile.player.getNextActorNetId(), rpc: true
			);

		}

		public static void shootHumerus(Vile vile) {
		if (vile.sprite.getCurrentFrame().POIs.IsNullOrEmpty()) return;
		bool isMK2 = vile.isVileMK2;
		Point? headPosNullable = vile.getVileMK2StunShotPos();
		if (headPosNullable == null) return;
		Point shootVel = vile.getVileShootVel(true);
		Point shootPos = vile.setCannonAim(new Point(shootVel.x, shootVel.y));
		int xDir = vile.xDir;
		if (vile.getShootXDir() == -1) {
			shootVel = new Point(shootVel.x * vile.getShootXDir(), shootVel.y);
		}
		new VileMissileProj(
				vile.pos.addxy(8 * vile.xDir,-21), xDir, 1, MathF.Round(shootVel.byteAngle), "missile_hc_proj",
				vile, vile.player, vile.player.getNextActorNetId(), rpc: true
			);

		}




	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vile = character as Vile;
	}
	public override void onExit(CharState newState) {
		base.onExit(newState);

	}

}




public class VMissileDash : CharState {


	public VMissileDash() : base("missile_b_dash") {
		immuneToWind = true;
		enterSound = "GDash";
		specialId = SpecialStateIds.AxlRoll;
	}

	public override void update() {
		base.update();
			character.move(new Point(character.xDir * -350, 0));
			if (player.input.isBPressed(player)) {
				character.changeState(new VKamaeHotIcecle(), true);
			}

		

		if (stateTime > 0.2f) {
			character.changeState(new VMissiLeStance(), true);
			return;
		}


	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;


	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
		character.slideVel = character.xDir * -character.getDashSpeed() * 0.9f;
		specialId = SpecialStateIds.None;
	}
}
