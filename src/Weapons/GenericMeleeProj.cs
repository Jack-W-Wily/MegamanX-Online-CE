﻿namespace MMXOnline;

public class GenericMeleeProj : Projectile {
	public Actor owningActor;

	public GenericMeleeProj(Weapon weapon, Point pos, ProjIds projId, Player player, float? damage = null, int? flinch = null, float? hitCooldown = null, Actor owningActor = null, bool isShield = false, bool isDeflectShield = false, bool isReflectShield = false) :
		base(weapon, pos, 1, 0, 2, player, "empty", 0, 0.25f, null, player.ownedByLocalPlayer) {
		destroyOnHit = false;
		shouldVortexSuck = false;
		shouldShieldBlock = false;
		this.projId = (int)projId;
		damager.damage = damage ?? weapon.damager.damage;
		damager.flinch = flinch ?? weapon.damager.flinch;
		damager.hitCooldown = hitCooldown ?? weapon.damager.hitCooldown;
		if (damager.hitCooldown == 0) damager.hitCooldown = 0.5f;
		this.owningActor = owningActor;
		this.xDir = owningActor?.xDir ?? player.character?.xDir ?? 1;
		this.isShield = isShield;
		this.isDeflectShield = isDeflectShield;
		this.isReflectShield = isReflectShield;
	}

	public override void update() {
		base.update();
	}

	public void charGrabCode(CommandGrabScenario scenario, Character grabber, IDamagable damagable, CharState grabState, CharState grabbedState) {
		if (grabber != null && damagable is Character grabbedChar && grabbedChar.canBeGrabbed()) {
			/*if (!owner.isDefenderFavored) {
				if (ownedByLocalPlayer && !Helpers.isOfClass(grabber.charState, grabState.GetType())) {
					owner.character.changeState(grabState, true);
					if (Global.isOffline) {
						grabbedChar.changeState(grabbedState, true);
					} else {
						grabbedChar.changeState(grabbedState, true);
						//RPC.commandGrabPlayer.sendRpc(grabber.netId, grabbedChar.netId, scenario, false);
					}
				}
			} else {
				if (grabbedChar.ownedByLocalPlayer && !Helpers.isOfClass(grabbedChar.charState, grabbedState.GetType())) {
					grabbedChar.changeState(grabbedState);
					if (Helpers.isOfClass(grabbedChar.charState, grabbedState.GetType())) {
						grabbedChar.changeState(grabbedState, true);
						//RPC.commandGrabPlayer.sendRpc(grabber.netId, grabbedChar.netId, scenario, true);
					}
				}
			}*/
			owner.character.changeState(grabState, true);	
			grabbedChar.changeState(grabbedState, true);
		}
	}

	public void maverickGrabCode(CommandGrabScenario scenario, Maverick grabber, IDamagable damagable, CharState grabbedState) {
		if (damagable is Character chr && chr.canBeGrabbed()) {
		/*	if (!owner.isDefenderFavored) {
				if (ownedByLocalPlayer && grabber.state.trySetGrabVictim(chr)) {
					if (Global.isOffline) {
						chr.changeState(grabbedState, true);
					} else {
						chr.changeState(grabbedState, true);
						//RPC.commandGrabPlayer.sendRpc(grabber.netId, chr.netId, scenario, false);
					}
				}
			} else {
				if (chr.ownedByLocalPlayer && !Helpers.isOfClass(chr.charState, grabbedState.GetType())) {
					chr.changeState(grabbedState);
					if (Helpers.isOfClass(chr.charState, grabbedState.GetType())) {
						chr.changeState(grabbedState, true);
						//RPC.commandGrabPlayer.sendRpc(grabber.netId, chr.netId, scenario, true);
					}
				}
			}
		}*/
		chr.changeState(grabbedState, true);
		
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);

		if (projId == (int)ProjIds.QuakeBlazer) {
			if (owner.character?.charState is ZeroFallStab hyouretsuzanState) {
				hyouretsuzanState.quakeBlazerExplode(false);
			}
		}
		//>>> Zero Uppercut Bullshit
		/*
		if (projId == (int)ProjIds.Ryuenjin || projId == (int)ProjIds.EBlade || projId == (int)ProjIds.Rising){
			if (damagable is Character chr) {
			float modifier = 1;
			if (chr.isUnderwater()) modifier = 2;
			if (chr.isImmuneToKnockback()) return;
			float xMoveVel = proj.MathF.Sign(pos.x - chr.pos.x);
			chr.move(new Point(xMoveVel * 0 * modifier, -300));
			}
		}*/
		// Command grab section
		Character grabberChar = owner.character;
		Character grabbedChar = damagable as Character;
		if (projId == (int)ProjIds.UPGrab) {
			charGrabCode(CommandGrabScenario.UPGrab, grabberChar, damagable, new XUPGrabState(grabbedChar), new UPGrabbed(grabberChar));
		} else if (projId == (int)ProjIds.VileMK2Grab) {
			charGrabCode(CommandGrabScenario.MK2Grab, grabberChar, damagable, new VileMK2GrabState(grabbedChar), new VileMK2Grabbed(grabberChar));
		}
	
		
		 else if (projId == (int)ProjIds.LaunchODrain && owningActor is LaunchOctopus lo) {
			maverickGrabCode(CommandGrabScenario.WhirlpoolGrab, lo, damagable, new WhirlpoolGrabbed(lo));
		} else if (projId == (int)ProjIds.FStagUppercut && owningActor is FlameStag fs) {
			maverickGrabCode(CommandGrabScenario.FStagGrab, fs, damagable, new FStagGrabbed(fs));
		} else if (projId == (int)ProjIds.WheelGGrab && owningActor is WheelGator wg) {
			maverickGrabCode(CommandGrabScenario.WheelGGrab, wg, damagable, new WheelGGrabbed(wg));
		} else if (projId == (int)ProjIds.MagnaCTail && owningActor is MagnaCentipede ms) {
			maverickGrabCode(CommandGrabScenario.MagnaCGrab, ms, damagable, new MagnaCDrainGrabbed(ms));
		} else if (projId == (int)ProjIds.BoomerangKDeadLift && owningActor is BoomerangKuwanger bk) {
			maverickGrabCode(CommandGrabScenario.DeadLiftGrab, bk, damagable, new DeadLiftGrabbed(bk));
		} else if (projId == (int)ProjIds.GBeetleLift && owningActor is GravityBeetle gb) {
			maverickGrabCode(CommandGrabScenario.BeetleLiftGrab, gb, damagable, new BeetleGrabbedState(gb));
		} else if (projId == (int)ProjIds.CrushCGrab && owningActor is CrushCrawfish cc) {
			maverickGrabCode(CommandGrabScenario.CrushCGrab, cc, damagable, new CrushCGrabbed(cc));
		} else if (projId == (int)ProjIds.BBuffaloDrag && owningActor is BlizzardBuffalo bb) {
			maverickGrabCode(CommandGrabScenario.BBuffaloGrab, bb, damagable, new BBuffaloDragged(bb));
		}
	}

	public override DamagerMessage onDamage(IDamagable damagable, Player attacker) {
		if (isZSaber() || projId == (int)ProjIds.X6Saber || projId == (int)ProjIds.XSaber) {
			Point hitPoint = (damagable as Actor).getCenterPos();
			Collider hitbox = getGlobalCollider();
			Collider collider = (damagable as Actor).collider;

			if (hitbox?.shape != null && collider?.shape != null) {
				var hitboxCenter = hitbox.shape.getRect().center();
				var hitCenter = collider.shape.getRect().center();
				hitPoint = new Point((hitboxCenter.x + hitCenter.x) * 0.5f, (hitboxCenter.y + hitCenter.y) * 0.5f);
			}

			string swordSparkSprite = projId == (int)ProjIds.ZSaber2 ? "sword_sparks_horizontal" : "sword_sparks_angled";

			new Anim(hitPoint, swordSparkSprite, 1, Global.level.mainPlayer.getNextActorNetId(), true, sendRpc: true);
		}

		return null;
	}

	public bool isZSaber() {
		return projId == (int)ProjIds.ZSaber1 || projId == (int)ProjIds.ZSaber3 || projId == (int)ProjIds.ZSaberair || projId == (int)ProjIds.ZSabercrouch || projId == (int)ProjIds.ZSaberdash
			|| projId == (int)ProjIds.ZSaberladder || projId == (int)ProjIds.ZSaberslide || projId == (int)ProjIds.ZSaberProjSwing;
	}

	public override void onDestroy() {
		base.onDestroy();
	}
}
