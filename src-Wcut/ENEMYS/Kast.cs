using System;
using System.Collections.Generic;
using System.Linq;


namespace MMXOnline;

public class Kast : Maverick {
	public VelGMeleeWeapon meleeWeapon = new();

	public Kast(
		Player player, Point pos, int xDir,
		ushort? netId, bool ownedByLocalPlayer, bool sendRpc = false
	) : base(
		player, pos, xDir, netId, ownedByLocalPlayer
	) {
		stateCooldowns = new() {
			{ typeof(MShoot), new(45, true) }
		};
		canClimbWall = true;
		maxHealth = 6;
		awardWeaponId = WeaponIds.Buster;
		weakWeaponId = WeaponIds.ShotgunIce;
		weakMaverickWeaponId = WeaponIds.ChillPenguin;
		dismantleTypeDeath = true;
		weapon = new Weapon(WeaponIds.VelGGeneric, 101);

		netActorCreateId = NetActorCreateId.Kast;
		netOwner = player;
		if (sendRpc) {
			createActorRpc(player.id);
		}

		armorClass = ArmorClass.Light;
		height = 24;
	}

	public bool healthvalueOnce = false;


	public override void creditMaverickKill(Player killer, Player assister, int? weaponIndex) {
		if (killer != null && killer != player) {
			if (Helpers.randomRange(0,5) == 0) {
				new SmallHealthPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			} else if (Helpers.randomRange(0,5) == 1) {
				new LargeHealthPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			} else if (Helpers.randomRange(0,5) == 2) {
				new SmallAmmoPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}  else if (Helpers.randomRange(0,5) == 3) {
				new LargeAmmoPickup(Global.level.mainPlayer, pos, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}    else if (Helpers.randomRange(0,5) == 4) {
				
			}  	else {
			killer.awardCurrency();
			}


			if (Global.level.gameMode is Arena) {
				killer.addKill();
			}
		}
	}

	public override void update() {
		base.update();


		if (!healthvalueOnce) {
			healthvalueOnce = true;
			health = 16;
		}


	
		if (aiBehavior == MaverickAIBehavior.Control) {

		}
	}

	public override string getMaverickPrefix() {
		return "enemy_kast";
	}

	public override float getRunSpeed() {
		return 0f * getRunDebuffs();
	}

	public MaverickState getShootState(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
			new TriadThunderProjCharged(pos, xDir, 3, this, player, player.getNextActorNetId(), rpc: true);
			new TriadThunderProjCharged(pos, -xDir, 3, this, player, player.getNextActorNetId(), rpc: true);
		}, "sparkmSparkX1");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}

	
	public MaverickState getShootState2(bool isAI) {
		var mshoot = new MShoot((Point pos, int xDir) => {
				new GBDMarker(pos, xDir, this, player, player.getNextActorNetId(), rpc: true);
				
		}, "torpedo");
		if (isAI) {
			mshoot.consecutiveData = new MaverickStateConsecutiveData(0, 4, 0.001f);
		}
		return mshoot;
	}


	public MaverickState getBonusState() {
		var mshoot = state;
		   
		if (Options.main.Difficulty > 1) {
			mshoot = new KastStompState();
		} else {
			mshoot = new MIdle();
		}
		return mshoot;
	}


	public override MaverickState[] aiAttackStates() {
		float enemyDist = 199;
		if (target != null && attackgeneralCooldown == 0) {
			enemyDist = MathF.Abs(target.pos.x - pos.x);
		}
		if (attackgeneralCooldown == 0){
			if (Options.main.Difficulty == 0) {
				attackgeneralCooldown = 2;
			}
			if (Options.main.Difficulty == 1) {
				attackgeneralCooldown = 0.6f;
			}
		}
	
		return [
			getBonusState(),
			new KastChainState(1),
			getShootState2(false),
			getShootState2(true),
		];
	}

	// Melee IDs for attacks.
	public enum MeleeIds {
		None = -1,
		Pounce,
	}



	// This can be called from a RPC, so make sure there is no character conditionals here.
	public override Projectile? getMeleeProjById(int id, Point pos, bool addToLevel = true) {
		return (MeleeIds)id switch {
			MeleeIds.Pounce => new GenericMeleeProj(
				meleeWeapon, pos, ProjIds.VelGMelee, player,
				3, Global.defFlinch, addToLevel: addToLevel
			),
			_ => null
		};
	}

}












public class KastChainProj : Projectile {
	public int state = 0;
	public Player player;
	public float distMoved;
	public float distRetracted;
	public bool reversed;
	public Point toWallVel;
	public Actor? hookedActor;
	public int maxDist = 100;
	public int origXDir;
	public int type;
	public bool isCharged { get { return type == 1; } }
	public float hookWaitTime;
	public Point chainVel;
	public Actor WireHetimarl;
	public Point netOrigin;
	public KastChainProj(
		Point pos, int xDir, Actor WireHetimarl, float spinTime,
		Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "enemy_kast_claw_proj", netId, player	
	) {
		weapon = WSpongeSideChainWeapon.netWeapon;
		damager.damage = 3;
		damager.flinch = Global.defFlinch;
		damager.hitCooldown = 15;

		setIndestructableProperties();
		this.player = player;
		this.WireHetimarl = WireHetimarl;

		origXDir = xDir;
		projId = (int)ProjIds.WSpongeChain;

		maxDist = 50 + (int)(Helpers.clampMax(spinTime, 1.5f) * 100);
		vel = new Point(xDir * (300 + maxDist), 0);
		speed = MathF.Abs(vel.x);

		chainVel = vel;
		netOrigin = pos;

		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir);
		}
		// ToDo: Make local.
		canBeLocal = false;
	}
	public static Projectile rpcInvoke(ProjParameters args) {
		return new KastChainProj(
			args.pos, args.xDir, args.owner, 0, args.owner, args.player, args.netId
		);
	}

	public override void postUpdate() {
		base.postUpdate();
		if (!ownedByLocalPlayer) return;

		if (WireHetimarl != null) {
			var shootPos = WireHetimarl.getFirstPOIOrDefault();
			changePos(new Point(shootPos.x + WireHetimarl.xDir * (distMoved - distRetracted), shootPos.y));
		}
	}

	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) {
			if (!reversed) distMoved += MathF.Abs(speed * Global.spf);
			else distRetracted += MathF.Abs(speed * Global.spf);
			return;
		}

		// Hooked character? Wait for them to become hooked before pulling back. Wait a max of 200 ms
		var hookedChar = hookedActor as Character;
		if (hookedChar != null && !hookedChar.ownedByLocalPlayer && !hookedChar.isStrikeChainState) {
			hookWaitTime += Global.spf;
			if (hookWaitTime < 0.2f) return;
		}

		// Firing
		if (state == 0) {
			distMoved += MathF.Abs(speed * Global.spf);
			if (distMoved >= maxDist) // || (type == 0 && !player.input.isHeld(Control.Shoot)))
			{
				reversed = true;
				chainVel.x *= -1;
				chainVel.y *= -1;
				state = 1;
				time = 0;
			}
		}
		// Retracting (not hooked to wall, possible actor pulled)
		else if (state == 1) {
			distRetracted += MathF.Abs(speed * Global.spf);
			if (hookedActor != null && !(hookedActor is Character)) {
				if (!hookedActor.ownedByLocalPlayer) {
					hookedActor.takeOwnership();
					RPC.clearOwnership.sendRpc(hookedActor.netId);
				}
				hookedActor.useGravity = false;
				hookedActor.grounded = false;
				hookedActor.move(hookedActor.pos.directionTo(WireHetimarl.getCenterPos()).normalize().times(speed));
			}
			if (distRetracted >= distMoved + 10) {
				if (hookedActor != null && !(hookedActor is Character)) {
					hookedActor.changePos(WireHetimarl.getCenterPos());
					hookedActor.useGravity = true;
				}
				destroySelf();
			}
		}
		// Retracting (pulled towards wall)
		else if (state == 2) {
			WireHetimarl.useGravity = false;
			WireHetimarl.stopMoving();
			WireHetimarl.move(toWallVel);
			distRetracted += MathF.Abs(toWallVel.magnitude * Global.spf);
			var collision = Global.level.checkTerrainCollisionOnce(WireHetimarl, toWallVel.x * Global.spf, toWallVel.y * Global.spf, toWallVel);
			if (distRetracted >= distMoved + 20 || collision?.gameObject is Wall) {
				destroySelf();
			}
		}
	}

	public override void onDestroy() {
		var hookedChar = hookedActor as Character;

		if (hookedChar != null && hookedChar.charState is StrikeChainHooked) {
			hookedChar.changeToIdleOrFall();
		}
		if (hookedActor is Anim) {
			hookedActor.useGravity = true;
			hookedActor.vel.x = xDir * 150;
			hookedActor.vel.y = -100;
			(hookedActor as Anim)!.ttl = 0.5f;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		Point origin = ownedByLocalPlayer ? WireHetimarl.getFirstPOIOrDefault() : netOrigin;
		renderGeneric(origin);
		if (!ownedByLocalPlayer) return;
		netOrigin = origin;
	}

	public void renderGeneric(Point origin) {
		float distFromStartX = MathF.Abs(pos.x - origin.x);
		const float len = 8;
		float pieceCount = distFromStartX / len;
		for (int i = 0; i < pieceCount; i++) {
			Global.sprites["enemy_kast_claw_chain"].draw(0, origin.x + (xDir * len * i), pos.y, xDir, 1, null, 1, 1, 1, ZIndex.Background + 100);
		}
	}

	public override void onCollision(CollideData other) {
		base.onCollision(other);
		if (!ownedByLocalPlayer) return;
		if (hookedActor != null) return;
		if (destroyed) return;
		if (state == 2) return;
		if (WireHetimarl.destroyed) return;
		if (reversed) return;
		if (state == 1 && time > 0.2f) return;

		// This code prevents the strike chain landing on the ground when X is falling and has the chain extended and still pulling X
		if (other.gameObject is Wall w && !w.isCracked) {
			var triggerList = Global.level.getTriggerList(this, -deltaPos.x, 0, null, typeof(Wall), typeof(Actor));
			if (triggerList.Any(t => t.gameObject == other.gameObject)) {
				return;
			}
		}

		var wall = other.gameObject as Wall;
		var actor = other.gameObject as Actor;

		if (wall != null && wall.collider.isClimbable && !wall.topWall) {
			reversed = true;
			state = 2;
			toWallVel = chainVel;
			chainVel.x = 0;
			chainVel.y = 0;
			var hitPoint = other.getHitPointSafe();
			changePos(new Point(hitPoint.x - WireHetimarl.xDir * 8, pos.y));
			distMoved = pos.distanceTo(WireHetimarl.getFirstPOIOrDefault());
			// WireHetimarl.changeState(new StrikeChainPullToWall(this, WireHetimarl.charState.shootSprite, toWallVel.y < 0), true);
		} else if (actor != null) {
			var chr = actor as Character;
			var pickup = actor as Pickup;
			if (chr == null && pickup == null) return;
			if (chr?.isGrabImmune() == true || chr?.isPushImmune() == true) return;
			if (chr != null && (!chr.canBeDamaged(player.alliance, player.id, projId) || isDefenderFavored())) return;
			changePos(new Point(chr?.pos.x ?? 0, pos.y));
			distMoved = pos.distanceTo(WireHetimarl.getFirstPOIOrDefault());
			hookActor(actor);
			if (chr != null && chr.canBeDamaged(player.alliance, player.id, projId)) {
				if (Global.serverClient != null) {
					RPC.commandGrabPlayer.sendRpc(netId, chr.netId, CommandGrabScenario.StrikeChain, false);
				}
				chr.hook(this);
			}
		}
	}

	public void hookActor(Actor actor) {
		int reverse = 1;
		if (state == 1) {
			if (time <= 0.2f) reverse = -1;
		}

		state = 1;
		chainVel.x *= -1 * reverse;
		chainVel.y *= -1 * reverse;
		reversed = true;
		hookedActor = actor;
		updateDamager(0);
	}

	public override DamagerMessage? onDamage(IDamagable damagable, Player attacker) {
		if (isDefenderFavored() && damagable is Character chr &&
			Global.serverClient != null &&
			!chr.isGrabImmune() && !chr.isPushImmune()
		) {
			RPC.commandGrabPlayer.sendRpc(netId, chr.netId, CommandGrabScenario.StrikeChain, true);
			chr.hook(this);
		}

		return null;
	}

	public void reverseDir() {
		reversed = true;
		chainVel.x *= -1;
		chainVel.y *= -1;
		state = 1;
		time = 0;
	}

	public bool isLatched() {
		return state == 2;
	}

	public override List<byte> getCustomActorNetData() {
		List<byte> customData = new();

		customData.AddRange(BitConverter.GetBytes(netOrigin.x));
		customData.AddRange(BitConverter.GetBytes(netOrigin.y));

		return customData;
	}
	public override void updateCustomActorNetData(byte[] data) {
		float originX = BitConverter.ToSingle(data[0..4], 0);
		float originY = BitConverter.ToSingle(data[4..8], 0);

		netOrigin = new Point(originX, originY);
	}
}




public class KastChainState : MaverickState {
	KastChainProj? proj;
	int jumpFramesHeld;
	const int maxJumpFrames = 10;
	bool jumpedOnce;
	float spinTime;

	public KastChainState(float spinTime) : base("chain_throw") {
		this.spinTime = spinTime;
	}

	public override void update() {
		base.update();

		bool jumpHeld = input.isHeld(Control.Jump, player);
		if (jumpHeld) {
			jumpFramesHeld++;
			if (jumpFramesHeld > maxJumpFrames) {
				jumpHeld = false;
			}
		}
		if (!jumpHeld) {
			if (!jumpedOnce && jumpFramesHeld > 0) {
				jumpedOnce = true;
				maverick.vel.y = -maverick.getJumpPower() * getJumpModifier();
				jumpFramesHeld = 0;
				maverick.changeSprite("vinethrow_jump", true);
			}
		}

		if (!maverick.grounded && (proj == null || !proj.isLatched())) {
			var inputDir = input.getInputDir(player);
			if (MathF.Sign(inputDir.x) == maverick.xDir) {
				maverick.move(new Point(inputDir.x * 150, 0));
			}

			if (Global.level.checkTerrainCollisionOnce(maverick, 0, -1) != null && maverick.vel.y < 0) {
				maverick.vel.y = 0;
			}
		}

		if (!maverick.grounded && maverick.sprite.name.EndsWith("wsponge_vinethrow_jump")) {
			maverick.changeSpriteFromName("vine_throw", true);
		}

		if (proj == null && maverick.getFirstPOI() != null) {
			proj = new KastChainProj(
				maverick.getFirstPOIOrDefault(), maverick.xDir, maverick,
				spinTime, maverick, player, player.getNextActorNetId(), rpc: true
			);
			maverick.playSound("wspongeChain", sendRpc: true);
		} else if (proj != null) {
			if (input.isPressed(Control.Shoot, player) && !proj.reversed) {
				proj.reverseDir();
			}

			if (proj.destroyed) {
				maverick.changeToIdleOrFall();
				return;
			}
		}
	}

	public new float getJumpModifier() {
		if (jumpFramesHeld == 1) return 1f;
		if (jumpFramesHeld == 2) return 1f;
		if (jumpFramesHeld == 3) return 1.01f;
		if (jumpFramesHeld == 4) return 1.015f;
		if (jumpFramesHeld == 5) return 1.02f;
		if (jumpFramesHeld == 6) return 1.025f;
		if (jumpFramesHeld == 7) return 1.05f;
		if (jumpFramesHeld == 8) return 1.1f;
		if (jumpFramesHeld == 9) return 1.25f;
		if (jumpFramesHeld >= 10) return 1.5f;
		return 0;
	}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
		proj?.destroySelf();
		maverick.useGravity = true;
	}
}





public class KastStompState : MaverickState {
	public KastStompState() : base("jump") {
	}

	public override void update() {
		base.update();

		if (maverick.grounded && stateTime > 0.05f) {
			new MechFrogStompShockwave(new FireWave(),
				maverick.pos, maverick.xDir, player,
				player.getNextActorNetId(), rpc: true);
				maverick.playSound("crash", true);
			new MechFrogStompShockwave(new FireWave(),
				maverick.pos, -maverick.xDir, player,
				player.getNextActorNetId(), rpc: true);
				
			landingCode();
			
			return;
		}

		wallClimbCode();

		if (Global.level.checkTerrainCollisionOnce(maverick, 0, -1) != null && maverick.vel.y < 0) {
			maverick.vel.y = 0;
		}

		maverick.move(new Point(maverick.xDir * 300, 0));
	}

	public override void onEnter(MaverickState oldState) {
		base.onEnter(oldState);
		maverick.vel.y = -maverick.getJumpPower()* 1.3f;
		}

	public override void onExit(MaverickState newState) {
		base.onExit(newState);
	}
}