using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace MMXOnline;

public class SoulBody : Weapon {

	public static SoulBody netWeapon = new();
	public SoulBody() : base() {
		index = (int)WeaponIds.SoulBody;
		fireRate = 30;
		weaponSlotIndex = 125;
        weaponBarBaseIndex = 74;
        weaponBarIndex = 63;
			displayName = "Soul Body";
		shootSounds = new string[] {"","buster2X4","buster2X4","SoulBodyAlt",""};
		weaknessIndex = (int)WeaponIds.LightningWeb;
		type = index;
		/* damage = "1/3";
		hitcooldown = "0.5/0.75";
		Flinch = "0/13";
		FlinchCD = hitcooldown;
		effect = "Deals damage on contact. C: Spawns 5 holograms that track enemies."; */
	}

	public override bool canShoot(int chargeLevel, Player player) {

		return base.canShoot(chargeLevel, player) && player.character.sBodyHologram == null
			&& player.character.sBodyClone == null;
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.pos;
		int xDir = character.getShootXDir();
		Player player = character.player;
		if (chargeLevel == 4 && !player.hasArmArmor(2)) {
			character.changeState(new ChargedSoulBodyEX(), true);
		}
		if (chargeLevel == 0) {
			character.changeState(new SoulBodyEX(), true);
		}
		if (chargeLevel == 3 || chargeLevel >= 3  && player.hasArmArmor(2)) {
			character.changeState(new ControlClone(), true);
		} 
		
		if (chargeLevel == 1 || chargeLevel == 2) {
		 new SoulBodyHologram(this, pos, xDir, player, player.getNextActorNetId(), true);
		}	
	}
}


public class SoulBodyHologram : Projectile {

float distance;
	const float maxDist = 96;
	int frameCount;
	public SoulBodyHologram(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId,  bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 1, player,
		"empty", 1, 0.33f, netProjId,
		player.ownedByLocalPlayer
	) {
		
		projId = (int)ProjIds.SoulBodyHologram;
	//	fadeSprite = "soul_body_fade";
		fadeOnAutoDestroy = true;
		frameSpeed = 0;
		changeSprite(owner.character.sprite.name, false);
		frameIndex = owner.character.frameIndex;
		owner.character.sBodyHologram = this;
		maxTime = 2.5f;
		setIndestructableProperties();
		canBeLocal = false;

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new SoulBodyHologram(
			SoulBody.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}
	
	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;
		
		globalCollider = new Collider(new Rect(0,0, 18, 34).getPoints(), 
			true, this, false, false, HitboxFlag.Hitbox, Point.zero);

		if (distance < maxDist) distance += 4;
		else distance = maxDist;

		xDir = owner.character.xDir;

		changePos(owner.character.pos.addxy(owner.character.getShootXDir() * distance, 0));
		changeSprite(owner.character.sprite.name, false);
		frameIndex = owner.character.frameIndex;
		frameCount++;

		if (time >= maxTime * 0.75f) {
			visible = frameCount % 2 == 0;
		}
	}
	
	public override List<ShaderWrapper>? getShaders() {
		var shaders = new List<ShaderWrapper>();
		ShaderWrapper cloneShader = Helpers.cloneShaderSafe("soulBodyPalette");
		int index = (frameCount / 2) % 7;
		if (index == 0) index++;

		cloneShader.SetUniform("palette", index);
		cloneShader.SetUniform("paletteTexture", Global.textures["soul_body_palette"]);
		shaders.Add(cloneShader);
	
		if (shaders.Count > 0) {
			return shaders;
		} else {
			return base.getShaders();
		}
	} 

	public override void onDestroy() {
		base.onDestroy();
		owner.character.sBodyHologram = null!;
	}
}







public class SoulBodyHologram2 : Projectile {

float distance;
	const float maxDist = 1;
	int frameCount;
	public SoulBodyHologram2(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId,  bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player,
		"empty", 0, 0.33f, netProjId,
		player.ownedByLocalPlayer
	) {
		
		projId = (int)ProjIds.SoulBodyHologram2;
	//	fadeSprite = "soul_body_fade";
		fadeOnAutoDestroy = true;
		frameSpeed = 0;
		changeSprite(owner.character.sBodyClone.sprite.name, false);
		frameIndex = owner.character.sBodyClone.frameIndex;
		owner.character.sBodyHologram2 = this;
		maxTime = 6.5f;
		setIndestructableProperties();
		canBeLocal = false;

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new SoulBodyHologram2(
			SoulBody.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}
	
	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;
		
		if (owner.character.sBodyClone == null) destroySelf();
		if (distance < maxDist) distance += 4;
		else distance = maxDist;

		xDir = owner.character.sBodyClone.xDir;

		changePos(owner.character.sBodyClone.pos.addxy(owner.character.sBodyClone.getShootXDir() * distance, 0));
		changeSprite(owner.character.sBodyClone.sprite.name, false);
		frameIndex = owner.character.sBodyClone.frameIndex;
		frameCount++;

		if (time >= maxTime * 0.75f) {
			visible = frameCount % 2 == 0;
		}
	}
	
//	public override List<ShaderWrapper>? getShaders() {
//		var shaders = new List<ShaderWrapper>();
//		ShaderWrapper cloneShader = Helpers.cloneShaderSafe("soulBodyPalette");
//		int index = (frameCount / 2) % 7;
//		if (index == 0) index++;
//
	//	cloneShader.SetUniform("palette", index);
	//	cloneShader.SetUniform("paletteTexture", Global.textures["soul_body_palette"]);
	//	shaders.Add(cloneShader);
	//
	//	if (shaders.Count > 0) {
	//		return shaders;
	//	} else {
	//		return base.getShaders();
	//	}
	//} 

	public override void onDestroy() {
		base.onDestroy();
		owner.character.sBodyHologram2 = null!;
	}
}




public class ControlClone : CharState {

	
	bool fired;
	float cloneCooldown;
	int cloneCount;
	float[] altAngles = new float [] {0, 16, 240, 32, 224}; 

	public ControlClone() : base("summon") {
		normalCtrl = false;
		attackCtrl = false;
		useDashJumpSpeed = true;
		invincible = true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.shootAnimTime = 0;
		character.useGravity = true;
	}

	public override void update() {
		base.update();

		summonUpdate();
		
	}

	void summonUpdate() {
		if (character.isAnimOver() && !fired) {
			new SoulBodyClone(character.player, character.pos.x, character.pos.y, character.xDir, false,
			character.player.getNextATransNetId(), character.ownedByLocalPlayer);
			fired = true;
			return;
		}
	}

}



public class SoulBodyEX : CharState {

	
	bool fired;
	float cloneCooldown;
	int cloneCount;
	float[] altAngles = new float [] {0, 16, 240, 32, 224}; 

	public SoulBodyEX() : base("summon") {
		normalCtrl = false;
		attackCtrl = false;
		useDashJumpSpeed = true;
		superArmor = true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.shootAnimTime = 0;
		character.useGravity = true;
	}

	public override void update() {
		base.update();
		shootUpdate();
	}

	void shootUpdate() {
		Helpers.decrementFrames(ref cloneCooldown);

		if (cloneCount >= 1 && cloneCooldown <= 10) character.changeToIdleOrFall();

		else if (character.isAnimOver() && cloneCooldown <= 0) {
			float ang = altAngles[cloneCount];
			ang = character.xDir == 1 ? ang : -ang + 128;
			Actor? target = Global.level.getClosestTarget(character.getCenterPos(), player.alliance, false, 160);

			if (target != null) ang = character.pos.directionTo(target.getCenterPos()).byteAngle;
			character.playSound("soulBodyX5", true);
			new SoulBodyX5(
				new SoulBody(), character.pos, character.xDir,
				player, player.getNextActorNetId(), cloneCount + 1, ang, true
			); //{ releasePlasma = player.hasPlasma() && cloneCount == 0 };

			cloneCount++;
			cloneCooldown = 20;
		}
	}
}

public class ChargedSoulBodyEX : CharState {

	
	bool fired;
	float cloneCooldown;
	int cloneCount;
	float[] altAngles = new float [] {0, 16, 240, 32, 224}; 

	public ChargedSoulBodyEX() : base("summon") {
		normalCtrl = false;
		attackCtrl = false;
		useDashJumpSpeed = true;
		invincible = true;
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.useGravity = false;
		character.stopMoving();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.shootAnimTime = 0;
		character.useGravity = true;
	}

	public override void update() {
		base.update();
		shootUpdate();
	}

	void shootUpdate() {
		Helpers.decrementFrames(ref cloneCooldown);

		if (cloneCount >= 5 && cloneCooldown <= 10) character.changeToIdleOrFall();

		else if (character.isAnimOver() && cloneCooldown <= 0) {
			float ang = altAngles[cloneCount];
			ang = character.xDir == 1 ? ang : -ang + 128;
			Actor? target = Global.level.getClosestTarget(character.getCenterPos(), player.alliance, false, 160);

			if (target != null) ang = character.pos.directionTo(target.getCenterPos()).byteAngle;
			character.playSound("soulBodyX5", true);
			new SoulBodyX5(
				new SoulBody(), character.pos, character.xDir,
				player, player.getNextActorNetId(), cloneCount + 1, ang, true
			); //{ releasePlasma = player.hasPlasma() && cloneCount == 0 };

			cloneCount++;
			cloneCooldown = 10;
		}
	}
}



public class SoulBodyX5 : Projectile {

	int color;

	public SoulBodyX5(
		Weapon weapon, Point pos, int xDir, Player player,
		ushort? netId, int color, float ang, bool rpc = false
	) : base (
		weapon, pos, 1, 360, 2, 
		player, "soul_body_x5", Global.halfFlinch, 0.5f,
		netId, player.ownedByLocalPlayer
	) {
		projId = (int)ProjIds.SoulBodyX5;
		maxTime = 0.75f;
		destroyOnHit = false;
		vel = Point.createFromByteAngle(ang).times(speed);
		this.color = color;
		byteAngle = ang;

		if (rpc) {
			rpcCreate(pos, player, netId, xDir, new byte[] {(byte)color, (byte)ang});
		} 
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new SoulBodyX5(
			SoulBody.netWeapon, arg.pos, arg.xDir, arg.player, 
			arg.netId, arg.extraData[0], arg.extraData[1]
		);
	}

	public override List<ShaderWrapper>? getShaders() {
		var shaders = new List<ShaderWrapper>();

		ShaderWrapper cloneShader = Helpers.cloneShaderSafe("soulBodyPalette");

		cloneShader.SetUniform("palette", color);
		cloneShader.SetUniform("paletteTexture", Global.textures["soul_body_palette"]);
		shaders.Add(cloneShader);
	
		if (shaders.Count > 0) {
			return shaders;
		} else {
			return base.getShaders();
		}
	}	
}
