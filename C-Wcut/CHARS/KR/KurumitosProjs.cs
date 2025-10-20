using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



// Adding a Weapon
public class KRMelee : Weapon {
	public static KRMelee netWeapon = new();

	public KRMelee() : base() {
		fireRate = 45;// frames
		index = (int)WeaponIds.KRMelee;// Make sure to add to "WeaponIds" on Enums.cs for it to work
		killFeedIndex = 167;//what sprite will appear in the kill index
	}
}




// this is a projectile 
public class OrochinagiChargedProj : Projectile {
	public OrochinagiChargedProj(
		Point pos, int xDir, bool isOD, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "kr_orochinagi_proj", netId, player
	) {
		weapon = FireWave.netWeapon; // weapon tied to it will also be compatible with weakness system
		damager.damage = 3;
		damager.flinch = 10;
		//damager.hitcooldown = (inset value); for cooldowns, this one has none since it's autodestroyed on hit
		vel = new Point(150 * xDir, 0);
		fadeOnAutoDestroy = true;
		// you can add pretty much every thing you see on Projectile.CS's bools in this area
		fadeSprite = "kr_orochinagi_proj_fade";
		reflectable = false;
		projId = (int)ProjIds.OrochinagiProj;
		maxTime = 0.5f;
		if (isOD) {
			damager.damage = 4;
			damager.flinch = 30;
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir, isOD ? (byte)1 : (byte)0);
		}
	}



	// To add damage effects
	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (!damagable.isPlayableDamagable()) { return; }
		if (damagable is not Actor actor || !actor.ownedByLocalPlayer) {
			return;
		}
	
		if (damagable is Character chr) {
			chr.burnTime = 2; // this is where the burn DOT effect enters
		}
	
	}


	// for Online display, make sure you add this to RPCCreatePojEX.cs
	/*
	public static Dictionary<int, ProjCreate> functs = new Dictionary<int, ProjCreate> {
	//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	{ (int)ProjIds.OrochinagiProj, OrochinagiChargedProj.rpcInvoke },
	//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
	*/
	public static Projectile rpcInvoke(ProjParameters args) {
		return new OrochinagiChargedProj(
			args.pos, args.xDir, args.extraData[0] == 1, args.owner, args.player, args.netId
		);
	}
}


public class YamiBaraiProj : Projectile {
	public YamiBaraiProj(
		Point pos, int xDir, bool isOD, Actor owner, Player player, ushort? netId, bool rpc = false
	) : base(
		pos, xDir, owner, "kr_shiki_yami_barai_proj", netId, player
	) {
		weapon = FireWave.netWeapon;
		damager.damage = 1;
		damager.flinch = 16;
		vel = new Point(250 * xDir, 0);
		fadeOnAutoDestroy = true;
		fadeSprite = "explosion";
		reflectable = true;
		projId = (int)ProjIds.YamiBaraiProj;
		maxTime = 0.5f;
		if (isOD) {
			damager.damage = 2;
			genericShader = player.zeroPaletteShader;
		}
		if (rpc) {
			rpcCreate(pos, owner, ownerPlayer, netId, xDir, isOD ? (byte)1 : (byte)0);
		}
	}


	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		if (!damagable.isPlayableDamagable()) { return; }
		if (damagable is not Actor actor || !actor.ownedByLocalPlayer) {
			return;
		}
	
		if (damagable is Character chr) {
			chr.burnTime = 1; 
		}
	
	}


	public static Projectile rpcInvoke(ProjParameters args) {
		return new OrochinagiChargedProj(
			args.pos, args.xDir, args.extraData[0] == 1, args.owner, args.player, args.netId
		);
	}
}




public class KRGenericMeleeProj : Projectile {
	public KRGenericMeleeProj(
		Weapon weapon, Point pos, ProjIds projId, Player player,
		float? damage = null, int? flinch = null, float? hitCooldown = null,
		Actor? ownerActor = null, bool isShield = false, bool isDeflectShield = false, bool isReflectShield = false,
		bool addToLevel = false, float? hitCooldownSeconds = null,
		bool isZSaberEffect = false, bool isZSaberEffect2 = false, bool isZSaberEffect2B = false, bool isZSaberClang = false,
		bool isJuggleProjectile = false, bool isPushProjectile = false,
		bool ShouldClang = false
	) : base(
		weapon, pos, 1, 0, 2, player, "empty", 0, 0.5f, null, player.ownedByLocalPlayer, addToLevel: addToLevel
	) {
		destroyOnHit = false;
		shouldVortexSuck = false;
		shouldShieldBlock = false;
		this.projId = (int)projId;
		damager.damage = damage ?? weapon.damager.damage;
		damager.flinch = flinch ?? weapon.damager.flinch;
		if (hitCooldown != null) {
			damager.hitCooldown = hitCooldown.Value;
		}
		else if (hitCooldownSeconds != null) {
			damager.hitCooldownSeconds = hitCooldownSeconds.Value;
		}
		else {
			damager.hitCooldown = weapon?.damager?.hitCooldown ?? 0;
		}
		if (hitCooldownSeconds == null && damager.hitCooldown <= 0) {
			damager.hitCooldown = 30;
		}
		this.ownerActor = ownerActor;
		this.xDir = ownerActor?.xDir ?? player.character?.xDir ?? 1;
		this.isShield = isShield;
		this.isDeflectShield = isDeflectShield;
		this.isReflectShield = isReflectShield;
		this.isZSaberEffect = isZSaberEffect;
		this.isZSaberEffect2 = isZSaberEffect2;
		this.isZSaberEffect2B = isZSaberEffect2B;
		this.isZSaberClang = isZSaberClang;
		this.isJuggleProjectile = isJuggleProjectile;
		this.isPushProjectile = isPushProjectile;
		this.ShouldClang = ShouldClang;
		isMelee = true;
	}


	public override void update() {
		base.update();
	}
	public bool isZSaberEffectBool(bool isEffect2, bool isEffect2B) {
		if (isEffect2) return isZSaberEffect2;
		if (isEffect2B) return isZSaberEffect2B;
		return isZSaberEffect;
	}
	public void charGrabCode(
		CommandGrabScenario scenario, Character? grabber,
		IDamagable? damagable, CharState grabState, CharState grabbedState
	) {
		if (grabber != null && damagable is Character grabbedChar && grabbedChar.canBeGrabbed()) {
			if (!owner.isDefenderFavored) {
				if (ownedByLocalPlayer && !Helpers.isOfClass(grabber.charState, grabState.GetType())) {
					owner.character.changeState(grabState, true);
					if (Global.isOffline) {
						grabbedChar.changeState(grabbedState, true);
					} else {
						RPC.commandGrabPlayer.sendRpc(grabber.netId, grabbedChar.netId, scenario, false);
					}
				}
			} else {
				if (grabbedChar.ownedByLocalPlayer &&
					!Helpers.isOfClass(grabbedChar.charState, grabbedState.GetType())
				) {
					grabbedChar.changeState(grabbedState);
					if (Helpers.isOfClass(grabbedChar.charState, grabbedState.GetType())) {
						RPC.commandGrabPlayer.sendRpc(grabber.netId, grabbedChar.netId, scenario, true);
					}
				}
			}
		}
	}

	public void maverickGrabCode(CommandGrabScenario scenario, Maverick grabber, IDamagable damagable, CharState grabbedState) {
		if (damagable is Character chr && chr.canBeGrabbed()) {
			if (!owner.isDefenderFavored) {
				if (ownedByLocalPlayer && grabber.state.trySetGrabVictim(chr)) {
					if (Global.isOffline) {
						chr.changeState(grabbedState, true);
					} else {
						RPC.commandGrabPlayer.sendRpc(grabber.netId, chr.netId, scenario, false);
					}
				}
			} else {
				if (chr.ownedByLocalPlayer && !Helpers.isOfClass(chr.charState, grabbedState.GetType())) {
					chr.changeState(grabbedState);
					if (Helpers.isOfClass(chr.charState, grabbedState.GetType())) {
						RPC.commandGrabPlayer.sendRpc(grabber.netId, chr.netId, scenario, true);
					}
				}
			}
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);

		if (projId == (int)ProjIds.QuakeBlazer) {
			if (owner.character?.charState is ZeroDownthrust hyouretsuzanState) {
				hyouretsuzanState.quakeBlazerExplode(false);
			}
		}

		// Command grab section
		Character? grabberChar = owner.character;
		Character? grabbedChar = damagable as Character;
	}

	public override DamagerMessage? onDamage(IDamagable? damagable, Player? attacker) {	
		Point? hitPoint = (damagable as Actor)?.getCenterPos() ?? new Point(0,0);
		Collider? hitbox = getGlobalCollider();
		Collider? collider = (damagable as Actor)?.collider;
		if (hitbox?.shape != null && collider?.shape != null) {
			var hitboxCenter = hitbox.shape.getRect().center();
			var hitCenter = collider.shape.getRect().center();
			hitPoint = new Point((hitboxCenter.x + hitCenter.x) * 0.5f, (hitboxCenter.y + hitCenter.y) * 0.5f);
		}
		string SaberShotFade = "zsaber_shot_fade";
		string SaberSlashFade = "zsaber_slash_fade";
		if (ownedByLocalPlayer) {
			if (isZSaberEffectBool(false, false)) {
				new Anim(hitPoint.Value, SaberShotFade, xDir,
					Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}
			if (isZSaberEffectBool(true, false)) {
				new Anim(hitPoint.Value, SaberSlashFade, xDir,
					Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}
			if (isZSaberEffectBool(false, true)) {
				new Anim(hitPoint.Value, SaberSlashFade, xDir*-1,
					Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
			}
		}
		return null;
	}
	public override void onDestroy() {
		base.onDestroy();
	}
}

