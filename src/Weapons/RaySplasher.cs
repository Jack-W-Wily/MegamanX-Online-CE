using SFML.Graphics;
using SFML.Graphics.Glsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public class RaySplasher : Weapon {
	public static RaySplasher netWeapon = new RaySplasher();

	public RaySplasher() : base() {
		shootSounds = new string[] { "raySplasher", "raySplasher", "raySplasher", "warpIn" };
		rateOfFire = 1f;
		index = (int)WeaponIds.RaySplasher;
		weaponBarBaseIndex = 21;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 21;
		killFeedIndex = 44;
		weaknessIndex = (int)WeaponIds.SpinningBlade;
	}

	public override void getProjectile(Point pos, int xDir, Player player, float chargeLevel, ushort netProjId) {
		if (chargeLevel < 3) {
			if (player.character is MegamanX mmx) {
				mmx.setShootRaySplasher(true);
			}
		} else {
			if (player.character.ownedByLocalPlayer) {
				player.character.changeState(new RaySplasherChargedState(), true);
			}
		}
	}
}

public class RaySplasherProj : Projectile {
	public RaySplasherProj(
		Weapon weapon, Point pos, int xDir, int spriteType, int dirType,
		bool isTurret, Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 600, 1, player, "raysplasher_proj",
		0, 0.075f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 0.25f;
		projId = (int)ProjIds.RaySplasher;

		if (player.isIris) damager.flinch = 4;
		if (isTurret) {
			projId = (int)ProjIds.RaySplasherChargedProj;
			damager.hitCooldown = 0;
		}
		reflectable = true;
		frameIndex = spriteType;
		frameSpeed = 0;
		fadeSprite = "raysplasher_fade";
		if (dirType == 0) {
			vel = new Point(600 * xDir, -150);
		}
		if (dirType == 1) {
			vel = new Point(600 * xDir, 150);
		}
		if (dirType == 2) {
			vel = new Point(600 * xDir, 0);
		}
		if (player?.character != null && player.isIris){
		changeSprite("iris_crystal_fireball", true);
		}
		if (rpc) {
			rpcCreate(
				pos, player, netProjId, xDir,
				new byte[] { (byte)spriteType, (byte)dirType, isTurret ? (byte)1 : (byte)0 }
			);
		}
	}

	public static Projectile rpcInvoke(ProjParameters args) {
		return new RaySplasherProj(
			RaySplasher.netWeapon, args.pos, args.xDir,
			args.extraData[0], args.extraData[1], (args.extraData[2] == 1),
			args.player, args.netId
		);
	}

	public override void onReflect() {
		base.onReflect();
		if (ownedByLocalPlayer) {
			float randY = Helpers.randomRange(-2f, 1f);
			vel.y *= randY;
			forceNetUpdateNextFrame = true;
		}
	}
}

public class RaySplasherTurret : Actor, IDamagable
{
	private int state = 0;

	private bool isIgnis;

	private bool irisCrystal;

	private Actor target;

	private float health;

	private float maxHealth;

	private const float range = 130f;

	private float drainTime;

	private float LaserCD = 0;

	private float raySplasherShootTime;

	private int raySplasherMod;

	private float velY;

	private ShaderWrapper replaceColorShader;

	private Player playerOwner;

	public Anim anim;

	

	public RaySplasherTurret(Point pos, Player player, int xDir, ushort netId, bool ownedByLocalPlayer, bool rpc = true)
		: base("raysplasher_turret_start", pos, netId, ownedByLocalPlayer, dontAddToLevel: false)
	{
		if (player != null && ownedByLocalPlayer) {
			playerOwner = player;
		}
		if (player.character != null && player.isVile) {
			isIgnis = true; 
		} else {isIgnis = false;}
		if (player.character != null && player.isIris) {
			irisCrystal = true; 
			
		} else {irisCrystal = false;}
		if (!isIgnis){
		health = 2;
		maxHealth = 2;
		} else { health = 10 ; maxHealth = 10;}
		useGravity = false;
		velY = -10f;
		base.xDir = xDir;
		removeRenderEffect(RenderEffectType.BlueShadow);
		removeRenderEffect(RenderEffectType.RedShadow);
		replaceColorShader = Helpers.cloneShaderSafe("replacecolor");
		if (replaceColorShader != null)
		{
			Vec4 origColor = new Vec4(0.03137255f, 0.03137255f, 0.03137255f, 0f);
			if (player.isMainPlayer)
			{
				replaceColorShader.SetUniform("origColor", origColor);
				replaceColorShader.SetUniform("replaceColor", new Vec4(0f, 0.75f, 0f, 0.5f));
			}
			else if (Global.level.gameMode.isTeamMode && player.alliance == 1)
			{
				replaceColorShader.SetUniform("origColor", origColor);
				replaceColorShader.SetUniform("replaceColor", new Vec4(0.75f, 0f, 0f, 0.5f));
			}
			else if (Global.level.gameMode.isTeamMode && player.alliance == 0)
			{
				replaceColorShader.SetUniform("origColor", origColor);
				replaceColorShader.SetUniform("replaceColor", new Vec4(0f, 0f, 0.75f, 0.5f));
			}
		}
		netOwner = player;
		netActorCreateId = NetActorCreateId.RaySplasherTurret;
		if (rpc) {
			createActorRpc(player.id);
		}
	}

	public override List<ShaderWrapper> getShaders()
	{
		if (replaceColorShader != null)
		{
			return new List<ShaderWrapper> { replaceColorShader };
		}
		return null;
	}

	public override void update()
	{
		base.update();
		updateProjectileCooldown();

		if (playerOwner?.character == null) {
				destroySelf();
		}
		if (playerOwner?.character != null &&
		 playerOwner.character.sprite.name.Contains("knocked")) {
				destroySelf();
		}
		if (!ownedByLocalPlayer)
		{
			return;
		}
		if (state == 1 || state == 2) {
			//drainTime += Global.spf;
			if (!isIgnis) {
			if (drainTime >= 6f)
			{
				destroySelf();
				return;
			}
			}
			// Follow player code.
			if (playerOwner?.character != null) {
				Character character = playerOwner.character;
				float targetPosX = (30 * -character.xDir + character.pos.x);
				float targetPosY = (-40 + character.pos.y);
				float moveSpeed = 1.5f * 60;

				// X axis follow.
				if (pos.x < targetPosX) {
					move(new Point(moveSpeed, 0));
					if (pos.x > targetPosX) { pos.x = targetPosX; }
				} else if (pos.x > targetPosX) {
					move(new Point(-moveSpeed, 0));
					if (pos.x < targetPosX) { pos.x = targetPosX; }
				}
				// Y axis follow.
				if (pos.y < targetPosY) {
					move(new Point(0, moveSpeed));
					if (pos.y > targetPosY) { pos.y = targetPosY; }
				} else if (pos.y > targetPosY) {
					move(new Point(0, -moveSpeed));
					if (pos.y < targetPosY) { pos.y = targetPosY; }
				}
			}
		}
		if (isIgnis && !irisCrystal){
		if (state == 0)
		{
			if (Global.level.getTriggerList(this, 0f, velY * Global.spf, null, typeof(Wall)).Count == 0)
			{
				move(new Point(0f, velY));
			}
			velY += Global.spf * 75f;
			if (velY >= 0f)
			{
				velY = 0f;
				if (playerOwner.character != null && playerOwner.input.isPressed("weaponright", playerOwner)){
				new TriadThunderQuake(playerOwner.weapon, pos.addxy(-10 * xDir, 0f), 1, playerOwner, playerOwner.getNextActorNetId(), rpc: true);	
				state = 1;
				}
				changeSprite("ra_ignis_idle", resetFrame: true);
			}
		}else if (state == 1)
		{
			move(new Point(100f, velY));
			if (playerOwner.character != null && !playerOwner.input.isHeld("weaponright", playerOwner)){
				state = 2;
			}
			changeSprite("ra_ignis_punch", resetFrame: true);	
		}else if (state == 2)
		{
		changeSprite("ra_ignis_idle", resetFrame: true);
		if (playerOwner.character != null && playerOwner.input.isPressed("weaponright", playerOwner)){
				new TriadThunderQuake(playerOwner.weapon, pos.addxy(-10 * xDir, 0f), 1, playerOwner, playerOwner.getNextActorNetId(), rpc: true);	
				state = 1;
				}
		}
		
		}

		Helpers.decrementTime(ref LaserCD);
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		if (!isIgnis && irisCrystal){
		//if (anim == null)  anim =
		// new Anim(pos, "iris_cannon_idle", xDir,playerOwner.getNextActorNetId(), true, sendRpc: true);
	
		if (state == 0)
		{
			if (Global.level.getTriggerList(this, 0f, velY * Global.spf, null, typeof(Wall)).Count == 0)
			{
				move(new Point(0f, velY));
			}
			velY += Global.spf * 75f;
			if (velY >= 0f)
			{
				velY = 0f;
				if (playerOwner.character != null && playerOwner.input.isPressed("weaponright", playerOwner)){
				state = 1;
				}
				changeSprite("iris_cannon_idle", resetFrame: true);
			}
		}else if (state == 1)
		{

			if (playerOwner.character != null &&playerOwner.character.xDir == 1)move(new Point(100f, velY));
			if (playerOwner.character != null &&playerOwner.character.xDir == -1)move(new Point(-100f, velY));
			if (playerOwner.character != null &&playerOwner.character != null && !playerOwner.input.isHeld("weaponright", playerOwner)){
				state = 2;
			}
			changeSprite("iris_cannon_fire", resetFrame: true);	
		}else if (state == 2)
		{
		changeSprite("iris_cannon_idle", resetFrame: true);
		if (playerOwner.character != null && playerOwner.input.isPressed("weaponright", playerOwner)){
				state = 1;
				}
		}

		if (playerOwner.character != null && playerOwner.input.isPressed("shoot", playerOwner)){
				state = 3;
		}
		
		
		if (LaserCD == 0 && playerOwner.character != null && playerOwner.input.isPressed("special1", playerOwner)){
				new RisingSpecterProj(new VileLaser(VileLaserType.RisingSpecter), pos, xDir, playerOwner, playerOwner.getNextActorNetId(), rpc: true);
				LaserCD = 4;
				playSound("risingSpecter", sendRpc: true);
			}
		else if (state == 3)
		{
			//isStatic = true;
			Actor closestTarget = Global.level.getClosestTarget(pos, netOwner.alliance, checkWalls: true, 130f);
			if (closestTarget != null)
			{
				target = closestTarget;
				state = 1;
				changeSprite("iris_cannon_fire", resetFrame: true);
			}
		}
		else
		{
			if (state == 3)
			{
				state = 1;
				return;
			}
			_ = target;
			target = Global.level.getClosestTarget(pos, netOwner.alliance, checkWalls: true);
			if (target == null || pos.distanceTo(target.getCenterPos()) >= 130f)
			{
				state = 1;
				target = null;
				changeSprite("iris_cannon_idle", resetFrame: true);
				return;
			}
			raySplasherShootTime += Global.spf;
			if (raySplasherShootTime > 0.4f &&  playerOwner.input.isPressed(Control.WeaponRight, playerOwner))
			{
				playSound("buster2");
				RaySplasherProj raySplasherProj = new RaySplasherProj(new RaySplasher(), pos, (!(pos.x > target.getCenterPos().x)) ? 1 : (-1), raySplasherMod % 3, 0, isTurret: true, netOwner, netOwner.getNextActorNetId(), rpc: true);
				float ang = pos.directionToNorm(target.getCenterPos()).angle;
				ang += Helpers.randomRange(-22.5f/2, 22.5f/2);
				raySplasherProj.vel = Point.createFromAngle(ang).times(600f);
				raySplasherShootTime = 0f;
				raySplasherMod++;
			}
		}
		}
		

		//>>>>>>>>>>>>>>>>>>>>>>>>>>
		if (!isIgnis && !irisCrystal){
	
		if (state == 0)
		{
			if (Global.level.getTriggerList(this, 0f, velY * Global.spf, null, typeof(Wall)).Count == 0)
			{
				move(new Point(0f, velY));
			}
			velY += Global.spf * 75f;
			if (velY >= 0f)
			{
				velY = 0f;
				state = 1;
				changeSprite("raysplasher_turret", resetFrame: true);
			}
		}
		else if (state == 1)
		{
			//isStatic = true;
			Actor closestTarget = Global.level.getClosestTarget(pos, netOwner.alliance, checkWalls: true, 130f);
			if (closestTarget != null)
			{
				target = closestTarget;
				state = 2;
				changeSprite("raysplasher_turret_fire", resetFrame: true);
				playSound("raySplasher");
			}
		}
		else
		{
			if (state != 2)
			{
				return;
			}
			_ = target;
			target = Global.level.getClosestTarget(pos, netOwner.alliance, checkWalls: true);
			if (target == null || pos.distanceTo(target.getCenterPos()) >= 130f)
			{
				state = 1;
				target = null;
				changeSprite("raysplasher_turret", resetFrame: true);
				return;
			}
			raySplasherShootTime += Global.spf;
			if (raySplasherShootTime > 0.4f)
			{
				RaySplasherProj raySplasherProj = new RaySplasherProj(new RaySplasher(), pos, (!(pos.x > target.getCenterPos().x)) ? 1 : (-1), raySplasherMod % 3, 0, isTurret: true, netOwner, netOwner.getNextActorNetId(), rpc: true);
				float ang = pos.directionToNorm(target.getCenterPos()).angle;
				ang += Helpers.randomRange(-22.5f/2, 22.5f/2);
				raySplasherProj.vel = Point.createFromAngle(ang).times(600f);
				raySplasherShootTime = 0f;
				raySplasherMod++;
			}
		}
		}
			
	}

	public void applyDamage(Player owner, int? weaponIndex, float damage, int? projId)
	{
		if (projId == 60)
		{
			damage *= 2f;
		}
		addDamageTextHelper(owner, damage, 4f, sendRpc: false);
		health -= damage;
		if (health <= 0f)
		{
			health = 0f;
			if (ownedByLocalPlayer)
			{
				destroySelf();
			}
		}
	}

	public bool canBeDamaged(int damagerAlliance, int? damagerPlayerId, int? projId)
	{
		if (sprite.name == "raysplasher_turret_start")
		{
			return false;
		}
		if (projId != 65)
		{
			return netOwner.alliance != damagerAlliance;
		}
		return true;
	}

	public bool isInvincible(Player attacker, int? projId)
	{
		return false;
	}

	public bool canBeHealed(int healerAlliance)
	{
		if (netOwner.alliance == healerAlliance)
		{
			return health < maxHealth;
		}
		return false;
	}

	public void heal(Player healer, float healAmount, bool allowStacking = true, bool drawHealText = false)
	{
		health += healAmount;
		if (drawHealText && healer != netOwner && ownedByLocalPlayer)
		{
			addDamageTextHelper(netOwner, 0f - healAmount, 16f, sendRpc: true);
		}
		if (health > maxHealth)
		{
			health = maxHealth;
		}
	}

	public override void onDestroy()
	{
		base.onDestroy();
		playSound("freezebreak2");

		if (!irisCrystal && !isIgnis){
		new Anim(pos, "raysplasher_turret_pieces", 1, null, destroyOnEnd: false)
		{
			ttl = 2f,
			useGravity = true,
			vel = Point.random(-150f, -50f, -100f, -50f),
			frameIndex = 0,
			frameSpeed = 0f
		};
		new Anim(pos, "raysplasher_turret_pieces", -1, null, destroyOnEnd: false)
		{
			ttl = 2f,
			useGravity = true,
			vel = Point.random(50f, 150f, -100f, -50f),
			frameIndex = 0,
			frameSpeed = 0f
		};
		new Anim(pos, "raysplasher_turret_pieces", 1, null, destroyOnEnd: false)
		{
			ttl = 2f,
			useGravity = true,
			vel = Point.random(-150f, -50f, -100f, -50f),
			frameIndex = 1,
			frameSpeed = 0f
		};
		new Anim(pos, "raysplasher_turret_pieces", -1, null, destroyOnEnd: false)
		{
			ttl = 2f,
			useGravity = true,
			vel = Point.random(50f, 150f, -100f, -50f),
			frameIndex = 1,
			frameSpeed = 0f
		};
		}
		if (anim != null) anim.destroySelf();
		netOwner.turrets.Remove(this);
	}


	public override Projectile getProjFromHitbox(Collider hitbox, Point centerPoint)
	{
		if (hitbox == null || playerOwner == null || sprite?.name == null)
		{
			return null;
		}
		Projectile proj = null;
		if (playerOwner == null) {
			return proj;
		}

		if (sprite.name.Contains("punch"))
		{
			proj = new GenericMeleeProj(
					new MechPunchWeapon(playerOwner), centerPoint,
					ProjIds.MechPunch, playerOwner, owningActor: this
				);
		}
		return proj;
	}

	public override void render(float x, float y)
	{
		base.render(x, y);
		if (netOwner.alliance == Global.level.mainPlayer.alliance)
		{
			float healthBarInnerWidth = 14f;
			Color color = default(Color);
			float healthPct = health / maxHealth;
			float width = Helpers.clampMax(MathF.Ceiling(healthBarInnerWidth * healthPct), healthBarInnerWidth);
			if ((double)healthPct > 0.66)
			{
				color = Color.Green;
			}
			else if ((double)healthPct <= 0.66 && (double)healthPct >= 0.33)
			{
				color = Color.Yellow;
			}
			else if ((double)healthPct < 0.33)
			{
				color = Color.Red;
			}
			float offY = 12f;
			DrawWrappers.DrawRect(pos.x - 8f, pos.y - 3f - offY, pos.x + 8f, pos.y - offY, filled: true, Color.Black, 0f, 999999L, isWorldPos: true, Color.White);
			DrawWrappers.DrawRect(pos.x - 7f, pos.y - 2f - offY, pos.x - 7f + width, pos.y - 1f - offY, filled: true, color, 0f, 999999L);
		}
		if (netOwner.isMainPlayer && replaceColorShader == null)
		{
			Global.sprites["cursorchar"].draw(0, pos.x + x, pos.y + y - 17f, 1, 1, null, 1f, 1f, 1f, zIndex + 1);
		}
		renderDamageText(10f);
	}
}


public class RaySplasherChargedState : CharState {
	bool fired = false;
	public RaySplasherChargedState() : base("point_up", "", "", "") {
		superArmor = true;
	}

	public override void update() {
		base.update();

		if (character.frameIndex >= 3 && !fired) {
			fired = true;

			var turret = new RaySplasherTurret(character.getShootPos(), player, character.xDir, player.getNextActorNetId(), character.ownedByLocalPlayer, rpc: true);
			player.turrets.Add(turret);
			if (player.turrets.Count > 1) {
				player.turrets[0].destroySelf();
			}
		}

		if (stateTime > 0.5f) {
			character.changeState(new Idle());
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
