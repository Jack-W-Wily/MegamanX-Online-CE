using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace MMXOnline;

public class MegaCrush : Weapon {
	public static float stunTime = 0.6f;
	public static float overclockVirusTime = 1.5f;

	public MegaCrush() {
		shootSounds = new List<string> { "", "", "", "" };
		rateOfFire = 1f;
		ammo = 0f;
		index = (int)NewWeaponIds.MegaCrush;
		weaponBarBaseIndex = 25;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 25;
		killFeedIndex = 13;
		maxAmmo = 32f;
	}

	public override void getProjectile(Point pos, int xDir, Player player, float chargeLevel, ushort netProjId) {
		if (player.character.ownedByLocalPlayer) {
			setAttackState(player);
		}
	}

	public void setAttackState(Player player) {
		if (!player.character.ownedByLocalPlayer) {
			return;
		}
		ammo = 0;
		//player.character.changeState(new MegaCrushCharState(), forceChange: true);
		new MegaCrushEffect(player.character, true);
		player.character.playSound("GigaCrushX2", sendRpc: true);
	}

	public override float getAmmoUsage(int chargeLevel) {
		return 32f;
	}

	public override bool canShoot(int chargeLevel, Player player) {
		return ammo >= 32;
	}
}

/*
public class MegaCrushCharState : CharState {
	private MegaCrushProj proj;

	Point moveDir = new(0, -20);

	private bool fired;

	private bool fired1;

	private bool fired3;

	private bool fired2;

	
	private int state;

	private Point poi;


	private float partTime;

	private float chargeTime;

	private bool startGrounded;

	private float ammoTime;
	
	public MegaCrushCharState()
		: base("gigacrush", "", "", "") {
		invincible = true;
	}

	public override void update() {
		character.subTankCooldown = Character.subTankCooldownMax;
		base.update();
			if (player.isVile) {
			character.usedAmmoLastFrame = true;
			Helpers.decrementTime(ref ammoTime);
			if (ammoTime == 0f) {
				ammoTime = 0.1f;
				player.vileAmmo -= 3f;
			}
		}
		if (player.isVile &&(player.vileAmmo <= 0f || (player.input.isPressed("special1", player) && stateTime > 1f))) {
			character.changeToIdleOrFall();
		}
		if (character.player.isX && character.frameIndex == 4 && !fired && character.ownedByLocalPlayer) {
			fired = true;
			proj = new MegaCrushProj(player.character.hasOverclock(ArmorP.Body),
				new MegaCrush(), character.getCenterPos(),
				character.xDir, base.player,
				base.player.getNextActorNetId(), rpc: true
			);
		}
		if (player.input.isHeld("up", player) &&character.isVileMK5EX 
		&& player.scrap > 9 && player.scrap < 40
		&& character.frameIndex == 4 && !fired) {
			 player.scrap -= 8;
				 chargeTime = 8;
			new GBeetleGravityWellProj(character.player.weapon,  character.pos,
			  character.xDir, chargeTime, character.player, character.player.getNextActorNetId()
			 , sendRpc: true);
		}
		if (!Global.level.gameMode.isTeamMode 
		&& player.input.isHeld("up", player)
		&& player.input.isHeld("right", player) 
		&& player.input.isHeld("left", player) 
		 && character.isVileMK5EX 
		&& player.scrap >39
		&& character.frameIndex == 4 && !fired) {
			 player.scrap -= 40;
				 chargeTime = 40;
			new GBeetleGravityWellProj(character.player.weapon,  character.pos,
			  character.xDir, chargeTime, character.player, character.player.getNextActorNetId()
			 , sendRpc: true);
		}
		if (!player.input.isHeld("up", player) &&character.isVileMK5EX  && character.frameIndex == 4 && !fired) {
			 chargeTime = 1;
			 fired = true;
			new GBeetleGravityWellProj(character.player.weapon,  character.pos,
			  character.xDir, chargeTime, character.player, character.player.getNextActorNetId()
			 , sendRpc: true);
		}
		if (character.player.isVile && !character.isVileMK2EX &&  character.frameIndex == 4 && !fired1)
		{
			fired1 = true;
			poi = character.getFirstPOIOrDefault();
			new WolfSigmaBeam(player.weapon, poi.addxy(0f, -130f), character.xDir, 1, 1, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		if (character.player.isVile && !character.isVileMK2EX && character.frameIndex == 5 && !fired2)
		{
			fired2 = true;
			Point spawnPos2 = poi.addxy(50 * character.xDir, -120f);
			Actor closestTarget2 = Global.level.getClosestTarget(poi, base.player.alliance, checkWalls: true, 150f);
			if (closestTarget2 != null)
			{
				spawnPos2.x = closestTarget2.pos.x;
			}
			new WolfSigmaBeam(player.weapon, spawnPos2, character.xDir, 1, 2, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		if (character.player.isVile && character.isVileMK2EX && character.frameIndex == 3)
		{
			Point? shootPos = character.getFirstPOI();
			character.subTankCooldown = Character.subTankCooldownMax;
			chargeTime = 1;
			once = true;
			Point point = new Point(character.pos.y, -90f);
			Point inputDir = player.input.getInputDir(player);
			float speedModifier = 1.5f;
			new ChillPBlizzardProj(player.weapon, character.pos.addxy(0f, -180f), character.xDir, base.player, base.player.getNextActorNetId(), rpc: true);
			character.playSound("chillpBlizzard", forcePlay: false, sendRpc: true);
		}
		if (character.player.isVile && !character.isVileMK2EX && character.frameIndex == 6 && !fired3)
		{
			fired3 = true;
			Point spawnPos = poi.addxy(-50 * character.xDir, -120f);
			Actor closestTarget = Global.level.getClosestTarget(poi, base.player.alliance, checkWalls: true, 150f);
			if (closestTarget != null)
			{
				spawnPos.x = closestTarget.pos.x;
			}
			new WolfSigmaBeam(player.weapon, spawnPos, character.xDir, 1, 2, base.player, base.player.getNextActorNetId(), rpc: true);
		}
		if ( character.sprite.isAnimOver() || character.frameIndex >= 6 && character.frameTime > 0.25f) {
			character.changeState(new Idle(), forceChange: true);
		}
		if (stateTime <= 1.6) {
			character.vel.x = moveDir.x * (1f - stateTime / 1.6f);
			character.vel.y = moveDir.y * (1f - stateTime / 1.6f);
		}
		else {
			character.vel.x = 0;
			character.vel.y = 0;
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		base.player.character.useGravity = false;
		base.player.character.vel.y = 0f;

		if (player.input.isHeld("left", base.player)) {
			moveDir.x = -40f;
			moveDir.y = 0;
		}
		else if (player.input.isHeld("right", base.player)) {
			moveDir.x = 40f;
			moveDir.y = 0;
		}
	}

	

	public override void onExit(CharState newState) {
		base.onExit(newState);
		base.player.character.useGravity = true;
	}
}*/

public class MegaCrushProj : Projectile {
	public float radius = 10f;

	public float maxActiveTime;

	public Player ownerPlayer;

	bool isOverclock;

	bool shootOnce = false;

	public MegaCrushProj(
		bool isOverclock, Weapon weapon,
		Point pos, int xDir, Player player,
		ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 0f, 1f, player,
		"empty", 13, 0.3f, netProjId, player.ownedByLocalPlayer
	) {
		maxActiveTime = 0.4f;
		destroyOnHit = false;
		shouldShieldBlock = false;
		vel = default(Point);
		projId = (int)NewProjIds.MegaCrush;
		shouldVortexSuck = false;
		ownerPlayer = player;
		isShield = true;
		isDeflectShield = true;
		if (isOverclock) {
			projId = (int)NewProjIds.MegaCrushOverclock;
			damager.flinch = 26;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update() {
		base.update();
		foreach (GameObject item in Global.level.getGameObjectArray()) {
			Actor actor = item as Actor;
			IDamagable damagable = item as IDamagable;
			if (actor != null && damagable != null && actor.ownedByLocalPlayer &&
				damagable.canBeDamaged(damager.owner.alliance, damager.owner.id, null) &&
				!(actor.pos.distanceTo(pos) > radius + 15f)
			) {
				damager.applyDamage(damagable, weakness: false, weapon, this, projId);
				if (pos.x < actor.pos.x) {
					actor.xFlinchPushVel = 400;
				} else if (pos.x > actor.pos.x) {
					actor.xFlinchPushVel = -400;
				} else {
					actor.xFlinchPushVel = 400 * -actor.xDir;
				}
			}
		}
		radius += Global.spf * 400f;

		if (time > maxActiveTime) {
			destroySelf(disableRpc: true);
		}
		if (time >= 0.25f && !shootOnce) {
			new MegaCrushProj2(isOverclock, weapon, pos, 1, ownerPlayer, null);
			shootOnce = true;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		double transparency = (time - 0.1) / (maxActiveTime - 0.1);
		if (transparency < 0) { transparency = 0; }
		//Color col1 = new(238, 226, 142, (byte)(225.0 - 225.0 * transparency));
		//Color col2 = new(255, 250, 230, (byte)(225.0 - 225.0 * transparency));
		Color col1 = new(54, 42, 100, (byte)(225.0 - 225.0 * transparency));
		Color col2 = new(178, 142, 238, (byte)(225.0 - 225.0 * transparency));
		DrawWrappers.DrawCircle(pos.x + x, pos.y + y, radius, filled: true, col1, 5f, zIndex + 1, isWorldPos: true, col2);
	}
}

public class MegaCrushProj2 : Projectile {
	public float radius = 10f;

	public Dictionary<int, bool> immuneTargets = new();

	public float maxActiveTime;

	public MegaCrushProj2(
		bool isOverclock, Weapon weapon,
		Point pos, int xDir,
		Player player, ushort? netProjId
	) : base(
		weapon, pos, xDir, 0, 0, player,
		"empty", 0, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		maxActiveTime = 0.5f;
		destroyOnHit = false;
		shouldShieldBlock = false;
		vel = default(Point);
		projId = (int)NewProjIds.MegaCrushPull;
		shouldVortexSuck = false;
	}

	public override void update() {
		base.update();
		radius = 150f - time * 150f / 0.5f;
		foreach (GameObject item in Global.level.getGameObjectArray()) {
			Actor actor = item as Actor;
			IDamagable damagable = item as IDamagable;
			if (actor != null && damagable != null && actor.ownedByLocalPlayer &&
				damagable.canBeDamaged(damager.owner.alliance, damager.owner.id, null) &&
				!(actor.pos.distanceTo(pos) > radius + 15f)
			) {
				applyPush(actor);
			}
		}
		if (time > maxActiveTime) {
			destroySelf(disableRpc: true);
		}
	}

	public override void render(float offsetX, float offsetY) {
		base.render(offsetX, offsetY);
		if (radius > 0f) {
			Color col1 = new(15, 12, 30, (byte)(164));
			Color col2 = new(54, 42, 100, (byte)(200));	
			DrawWrappers.DrawCircle(
				pos.x, pos.y, radius,
				filled: true, col1,
				5f, -2000001L,
				outlineColor: col2
			);
		}
	}

	public void applyPush(Actor actor) {
		if (actor == null || !actor.ownedByLocalPlayer ||
			actor.netId == null || immuneTargets.GetValueOrDefault(actor.netId.Value)
		) {
			return;
		}
		if (actor is Character chr && !chr.isCCImmune() ||
			actor is RideArmor ||
			actor is Maverick 
			
		) {
			float mag = 300;

			actor.vel.y = 0;
			actor.xFlinchPushVel = 0;
	
			Point normalDirection = actor.getCenterPos().directionToNorm(pos);
			Point velVector = new(
				normalDirection.x * mag,
				normalDirection.y * mag * 2
			);
			actor.move(velVector);
			actor.xFlinchPushVel = normalDirection.x * mag;

			if (actor.pos.distanceTo(pos) < 20) {
				immuneTargets[actor.netId.Value] = true;
				actor.yPushVel = normalDirection.y * 300;
			}
		}
	}
}

public class MegaCrushEffect : Effect {
	public Character character;

	private float frame1Time;

	private float frame2Time;

	private float time;

	Color colour = new(244, 238, 195, 164);
	Color colourLines = new(136, 184, 248, 200);

	public MegaCrushEffect(Character character, bool sendRPC = false) : base(character.pos) {
		this.character = character;
		//if (sendRPC) {
		//	RPC.createEffect.sendRpc(EffectId.MegaCrushStart, character.player);
		//}
	}

	public override void update() {
		base.update();
		pos = character.pos;
		time += Global.spf;
		if (time > 3f) {
			destroySelf();
		}
		else if (!(character.sprite.name != "mmx_gigacrush") && character.frameIndex <= 2) {
			if (character.frameIndex < 2) {
				frame1Time += Global.spf;
			}
			if (character.frameIndex == 2) {
				frame2Time += Global.spf;
			}
		}
	}

	public override void render(float offsetX, float offsetY) {
		base.render(offsetX, offsetY);
		if (character.sprite.name != "mmx_gigacrush" || character.frameIndex > 2) {
			return;
		}
		Point pos = character.getCenterPos();
		if (character.frameIndex < 2) {
			for (int i = 0; i < 8; i++) {
				float angle = i * 45;
				float ox = (float)Helpers.randomRange(10, 30) * Helpers.cosd(angle) * (float)(MathF.Round(25f * frame1Time / 1.5f) % 5);
				float oy = (float)Helpers.randomRange(10, 30) * Helpers.sind(angle) * (float)(MathF.Round(25f * frame1Time / 1.5f) % 5);
				DrawWrappers.DrawLine(pos.x + ox, pos.y + oy, pos.x + ox * 2f, pos.y + oy * 2f, colourLines, 1f, character.zIndex);
			}
		}
		else if (character.frameIndex == 2) {
			float radius = 150f - frame2Time * 150f / 0.5f;
			if (radius > 0f) {
				byte colour2 = (byte)(255.0 - 255.0 * ((radius - 75) / 75.0));
				byte colour1 = 255;
				if (radius <= 75) {
					colour2 = 0;
					colour1 = (byte)(255.0 - 255.0 * (radius / 75.0));
				}
				Color colour = new(128, colour1, colour2, 164);
				DrawWrappers.DrawCircle(
					pos.x, pos.y, radius,
					filled: true, new(0, 0, 0 ,164),
					5f, -2000001L,
					outlineColor: colour
				);
			}
		}
	}
}

/*
public class XtremeChargeShot : CharState {
	private bool fired;

	private int type;

	static Buster tempBuster = new Buster();

	private int dirMul = 1;

	public XtremeChargeShot(int type) : base("x2_shot", "", "", "") {
		this.type = type;
	}

	public override void update() {
		base.update();
		if (!character.grounded) {
			airCode();
			if (player.input.isHeld("dash", base.player)) {
				character.isDashing = true;
			}
		}
		if (!fired && character.currentFrame.getBusterOffset().HasValue) {
			fired = true;
			if (type == 0) {
				new Buster3Proj(
					player.weapon, character.getShootPos(), character.getShootXDir() * dirMul,
					2, player, player.getNextActorNetId(), rpc: true
				);
			}
			else if (type >= 1) {
				Weapon wp = player.weapon;
				if (wp is not Buster) {
					wp = tempBuster;
					player.weapon.shootTime = wp.rateOfFire;
				}
				new BusterXtr2Proj(
					wp, character.getShootPos(), character.getShootXDir() * dirMul,
					player, player.getNextActorNetId(), rpc: true
				);
			}
		}
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		bool air = !character.grounded;
		if (oldState is WallSlide) {
			sprite = "wall_slide_shoot";
			character.changeSpriteFromName(sprite, resetFrame: true);
			dirMul = -1;
			return;
		}
		if (type % 2 == 0 && !air) {
			sprite = "x2_shot";
			character.changeSpriteFromName("x2_shot", resetFrame: true);
		}
		if (type % 2 == 1 && !air) {
			sprite = "x2_shot2";
			character.changeSpriteFromName("x2_shot2", resetFrame: true);
		}
		if (type % 2 == 0 && air) {
			sprite = "x2_air_shot";
			character.changeSpriteFromName("x2_air_shot", resetFrame: true);
			landSprite = "x2_shot";
		}
		if (type % 2 == 1 && air) {
			sprite = "x2_air_shot2";
			character.changeSpriteFromName("x2_air_shot2", resetFrame: true);
			landSprite = "x2_shot2";
		}
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.shootAnimTime = 0;
	}
}

public class XtremeDash : Dash {
	public XtremeDash(string initialDashButton) : base(initialDashButton) {

	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.xtremeDashCooldown = character.xtremeDashMaxCooldown;
		character.specialState = (int)SpecialStateIds.XtremeDash;
		invincible = true;
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.xtremeDashCooldown = character.xtremeDashMaxCooldown;
		character.specialState = (int)SpecialStateIds.None;
		invincible = false;
	}

	public override void update() {
		base.update();
		character.xtremeDashCooldown = character.xtremeDashMaxCooldown;
	}

	public override void render(float x, float y) {
		base.render(x, y);
		if (character.ownedByLocalPlayer) {
			renderOverlay(character);
		}
	}

	public static void renderOverlay(Character character) {
		character.sprite.draw(
			character.frameIndex,
			character.pos.x + (float)character.xDir * character.currentFrame.offset.x,
			character.pos.y + (float)character.yDir * character.currentFrame.offset.y,
			character.xDir, character.yDir,
			null, 0.25f,
			character.xScale, character.yScale,
			character.zIndex+1,
			null,//character.getShaders(),
			character.angle.GetValueOrDefault(),
			character,
			false,
			null//character.getShadersArmor()
		);
	}

}*/
