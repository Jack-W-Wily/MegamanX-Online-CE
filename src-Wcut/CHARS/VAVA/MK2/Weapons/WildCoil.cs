using System;
using System.Collections.Generic;

namespace MMXOnline;

public class WildCoil : Weapon {
	public static WildCoil netWeapon = new();

	public WildCoil() : base() {
		displayName = "WILD COIL";
	
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 0;
		maxAmmo = 20;
		ammo = maxAmmo;
		fireRate = 60;
		switchCooldown = 45;
		
	}


	public override void vileShootOld(WeaponIds weaponInput, Vile vile) {
		
			Point shootPos = vile.getShootPos();
			int xDir = vile.getShootXDir();
			Player player = vile.player;
			int input = player.input.getYDir(player);
			int chargeLv = vile.getChargeLevel();


	
			new WildCoilChargedProj(vile, shootPos, xDir, 0, player.getNextActorNetId(), true, player);
			new WildCoilChargedProj(vile, shootPos, xDir, 1, player.getNextActorNetId(), true, player);
			vile.playSound("buster3", sendRpc: true);
	
			new WildCoilProj(vile, shootPos, xDir, 0, player.getNextActorNetId(), true, player);
			new WildCoilProj(vile, shootPos, xDir, 1, player.getNextActorNetId(), true, player);
			vile.playSound("buster2", sendRpc: true);
		
	}

	public override float getAmmoUsage(int chargeLevel) {
		if (chargeLevel >= 2) return 2;
		return 1;
	}
}




public class VavaWindCoil : CharState {
	int bombNum;

	public VavaWindCoil() : base("air_bomb_attack", "", "") {
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();

			Point shootPos = character.getShootPos();
		
		
			
			if (stateTime > 0f && bombNum == 0) {
			bombNum++;
				character.playSound("buster2", sendRpc: true);
			if (character.OverDrive) {
				new WildCoilChargedProj(character, shootPos,  character.xDir, 0, player.getNextActorNetId(), true, player);
				new WildCoilChargedProj(character, shootPos, character.xDir, 1, player.getNextActorNetId(), true, player);

			} else {
				new WildCoilProj(character, shootPos,  character.xDir, 0, player.getNextActorNetId(), true, player);
				new WildCoilProj(character, shootPos,  character.xDir, 1, player.getNextActorNetId(), true, player);

			}
		}

			if (stateTime > 0.25f) {
				character.changeToIdleOrFall();
			}
		
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.vel = new Point();
	}

	public override void onExit(CharState? newState) {
		base.onExit(newState);
		character.useGravity = true;
	}
}




public class WildCoilProj : Projectile {

	public int bounceSpeed = 540;
	float soundCooldown;
	float projSpeed = 120;
	public WildCoilProj(
		Actor owner, Point pos, int xDir, int type,
		ushort? netProjId, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner, "wild_coil_start", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.WildCoil;
		useGravity = true;
		maxTime = 1.5f;
		fadeOnAutoDestroy = true;
		fadeSprite = "explosion";
		canBeLocal = false;

		damager.damage = 2;
		damager.flinch = 10;
		damager.hitCooldown = 6;

		vel.y = -200;
		if (type == 0) vel.x = projSpeed * xDir;
		else vel.x = -projSpeed * xDir;

		if (rpc) {
			byte[] extraArgs = new byte[] { (byte)type };

			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new WildCoilProj(
			arg.owner, arg.pos, arg.xDir, arg.extraData[0],
			arg.netId, altPlayer: arg.player
		);
	}

	public override void update() {
		base.update();

		if (!ownedByLocalPlayer) return;
		if (soundCooldown > 0) Helpers.decrementTime(ref soundCooldown);
	}


	public override void onHitWall(CollideData other) {

		var normal = other.hitData.normal ?? new Point(0, -1);


		if (normal.isSideways()) {
			destroySelf();
		} else {
			changeSprite("wild_coil_jump", true);
			if (frameIndex > 0) frameIndex = 0;
			if (soundCooldown <= 0) {
				playSound("wild_coil_bounce", true, true);
				soundCooldown = 6f / 60f;
			}
			vel.y *= -1;
			if (vel.y < 0) {
				if (vel.y != -bounceSpeed) vel.y = -bounceSpeed;
			} else {
				if (vel.y != -bounceSpeed) vel.y = bounceSpeed;
			}

			incPos(new Point(0, 5 * MathF.Sign(vel.y)));
		}
	}
}


public class WildCoilChargedProj : Projectile {

	public int bounceSpeed = 630;
	public float bouncePower = 1;
	float soundCooldown;
	int bounceReq = 1;
	int bounceCounter;
	int bounceBuff;
	bool bouncedOnce;
	int frame;
	float projSpeed = 120;
	Player? player = null;

	public WildCoilChargedProj(
		Actor owner, Point pos, int xDir, int type, 
		ushort? netProjId, bool rpc = false, Player? altPlayer = null
	) : base(
		pos, xDir, owner,"wild_coil_charge_start", netProjId, altPlayer
	) {

		projId = (int)VAVA2ProjIds.WildCoilCharged;
		maxTime = 2f;
		useGravity = true;
		fadeOnAutoDestroy = true;
		fadeSprite = "explosion";
		canBeLocal = false;
		damager.damage = 3;
		damager.flinch = 35;

		player = altPlayer;
		if (player != null) {
			if (player.input.isHeld(Control.Up, player)) bounceSpeed = 480;
			else if (player.input.isHeld(Control.Down, player)) {
				bounceReq = 6;
				bounceSpeed = 60;
			} else bouncePower = 1f;
		}	

		vel.y = -200;
		if (type == 0) vel.x = projSpeed * xDir;
		else vel.x = -projSpeed * xDir;

		if (rpc) {
			byte[] extraArgs = new byte[] { (byte)type };

			rpcCreate(pos, owner, ownerPlayer, netProjId, xDir, extraArgs);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new WildCoilChargedProj(
			arg.owner, arg.pos, arg.xDir, arg.extraData[0], 
			arg.netId, altPlayer: arg.player
		);
	}


	public override void update() {
		base.update();

		if (!ownedByLocalPlayer) return;
		if (soundCooldown > 0) Helpers.decrementTime(ref soundCooldown);

		bounceBuff = (int)bounceCounter / bounceReq;
		frame = bouncedOnce ? frameIndex : 3;
	}


	public override void onHitWall(CollideData other) {
			var normal = other.hitData.normal ?? new Point(0, -1);


		if (normal.isSideways()) {
			destroySelf();
		} else {
			changeSprite("wild_coil_charge_jump", true);
			if (frameIndex > 0) frameIndex = 0;
			if (soundCooldown <= 0) {
				playSound("wild_coil_bounce", true, true);
				soundCooldown = 6f / 60f;
			}
			vel.y *= -1;
			if (vel.y < 0) {
				if (vel.y != -bounceSpeed) vel.y = -bounceSpeed;
			} else {
				if (vel.y != -bounceSpeed) vel.y = bounceSpeed;
			}

			incPos(new Point(0, 5 * MathF.Sign(vel.y)));
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);

		Global.sprites[getOutline()].draw(
			frame, pos.x, pos.y, xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex
		);
	}

	string getOutline() {
		return bounceBuff switch  {
			0 => "wild_coil_outline1",
			1 => "wild_coil_outline2",
			_ => "wild_coil_outline3"
		};
	}

	public override List<byte> getCustomActorNetData() {
		return [
			(byte)bounceBuff,
			(byte)frame
		];
	}

	public override void updateCustomActorNetData(byte[] data) {
		bounceBuff = data[0];
		frame = data[1];
	}
}
