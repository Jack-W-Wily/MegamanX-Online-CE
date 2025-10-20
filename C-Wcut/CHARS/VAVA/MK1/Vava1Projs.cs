using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



// Adding a Weapon
public class Vava1Melee : Weapon {
	public static Vava1Melee netWeapon = new();

	public Vava1Melee() : base() {
		fireRate = 45;// frames
		index = (int)WeaponIds.KRMelee;// Make sure to add to "WeaponIds" on Enums.cs for it to work
		killFeedIndex = 167;//what sprite will appear in the kill index
	}
}






public class Vava1GenericMeleeProj : Projectile {
	public Vava1GenericMeleeProj(
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

