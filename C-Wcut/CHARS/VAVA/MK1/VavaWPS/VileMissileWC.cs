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

 	public Vile? vava = null;
	public VMissiLeStance() : base("missile_stance") {

	}

	public override void update() {
		base.update();
		if (vava != null){
		Point shootVel = vava.getVileShootVel(true);
	
		//int xDir = character.getShootXDir();
	

		if (character.frameIndex > 0) {
			if (player.input.isPressed(Control.Jump, player)) {
				character.changeToIdleOrFall();
			}


			if (player.input.isAPressed(player)  && player.vileAmmo >= 6) {
				player.vileAmmo -= 6;
				character.frameIndex = 5;
				character.playSound("vileMissile", true);
					new VileMissileProj(
			character.pos.addxy(8 * character.xDir , -21), character.xDir, 1, MathF.Round(shootVel.byteAngle), "missile_hc_proj",
			character, character.player, character.player.getNextActorNetId(), rpc: true
		);
			}

			if (player.input.isBPressed(player) && player.vileAmmo >= 10) {
				player.vileAmmo -= 10;
				character.frameIndex = 5;
				character.playSound("vileMissile", true);
				new VileMissileProj(
			character.pos.addxy(8 * character.xDir , -21), character.xDir, 2, MathF.Round(shootVel.byteAngle), "missile_pd_proj",
			character, character.player, character.player.getNextActorNetId(), rpc: true
		);
			}

			if (player.input.isR2Pressed(player) && player.vileAmmo >= 20) {
				player.vileAmmo -= 20;
				character.frameIndex = 5;
				character.playSound("vileMissile", true);
				new BanzaiBeetleProj(new VileMK2Grab(), 
			character.pos.addxy(0,-30), character.xDir, player, 
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
	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		vava = character as Vile;
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
