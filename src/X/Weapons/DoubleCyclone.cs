using System;
using System.Collections.Generic;
using SFML.Audio;
using SFML.Graphics;

namespace MMXOnline;

public class DoubleCyclone : Weapon {
	public static DoubleCyclone netWeapon = new();

	public DoubleCyclone() : base() {
		shootSounds = new string[] { "fakeDoubleCyclone", "fakeDoubleCyclone", "fakeDoubleCyclone", "twinSlasherCharged", "twinSlasherCharged" };
		fireRate = 75;
		index = (int)WeaponIds.DoubleCyclone;
		weaponSlotIndex = 118;
		killFeedIndex = 168;
		weaponBarIndex = weaponBarBaseIndex;
		//weaknessIndex = (int)WeaponIds.TriadThunder;
		damage = "2/2";
		effect = ".";
		hitcooldown = "0/0.5";
		Flinch = "0/26";
		FlinchCD = "0/1";
		maxAmmo = 32;
		ammo = maxAmmo;
		type = index;
		displayName = "Double Cyclone ";
	}

	public override float getAmmoUsage(int chargeLevel) {
		if (chargeLevel >= 3) { return 4; }
		return 1;
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.getShootPos();
		int xDir = character.getShootXDir();
		Player player = character.player;

		if (chargeLevel == 0) {
			player.setNextActorNetId(player.getNextActorNetId());
			new DoubleCycloneProj(this, pos, xDir, 0, player, player.getNextActorNetId(true), true);
		} 
		if (chargeLevel == 1 || chargeLevel == 2) {
			player.setNextActorNetId(player.getNextActorNetId());
			character.changeSpriteFromName("shoot_2hands", true);
			new DoubleCycloneProj(this, pos, 1, 0, player, player.getNextActorNetId(true), true);
			new DoubleCycloneProj(this, pos, -1, 0, player, player.getNextActorNetId(true), true);
		} 
		if (chargeLevel == 3){
		character.changeSpriteFromName("shoot_2hands", true);
		new DoubleCycloneCharged(this, pos, 1, player, player.getNextActorNetId(), sendRpc: true);
		new DoubleCycloneCharged(this, pos, -1, player, player.getNextActorNetId(), sendRpc: true);
		}
		if (chargeLevel == 4){
		character.changeSpriteFromName("shoot_2hands", true);
			new DoubleCycloneProj(this, pos, 1, 0, player, player.getNextActorNetId(true), true);
			new DoubleCycloneProj(this, pos, -1, 0, player, player.getNextActorNetId(true), true);
			new DoubleCycloneProj(this, pos, 1, 1, player, player.getNextActorNetId(true), true);
			new DoubleCycloneProj(this, pos, -1, 1, player, player.getNextActorNetId(true), true);
			new DoubleCycloneCharged(this, pos, 1, player, player.getNextActorNetId(), sendRpc: true);
			new DoubleCycloneCharged(this, pos, -1, player, player.getNextActorNetId(), sendRpc: true);
		}
	}
}

public class DoubleCycloneProj : Projectile {
	Sound? spinSound;
	bool once;

	public DoubleCycloneProj(
		Weapon weapon, Point pos, int xDir, int type, 
		Player player, ushort netProjId, bool rpc = false
	) : base(
		weapon, pos, xDir, 250, 1, player, "double_cyclone_proj", 
		0, 0.3f, netProjId, player.ownedByLocalPlayer
	) {
		maxTime = 1f;
		projId = (int)ProjIds.DoubleCyclone;
		destroyOnHit = false;
		//fadeSprite = "explosion";
		//fadeSound = "explosion";
		/*try {
			spinSound = new Sound(Global.soundBuffers["spinningBlade"].soundBuffer);
			spinSound.Volume = 50f;
		} catch {
			// GM19:
			// Sometimes code above throws for some users with
			// "External component has thrown an exception." error,
			// could investigate more on why
			// Gacel Notes:
			// WTF GM19?
			// You know this is because you use it at object creation.
			// I'm moving this to on onStart().
		}*/
		vel.y = (type == 0 ? -37 : 37);
		if (type == 0) {
			yScale = -1;
		}
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir, (byte)type);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new DoubleCycloneProj(
			DoubleCyclone.netWeapon, arg.pos, arg.xDir,
			arg.extraData[0], arg.player, arg.netId
		);
	}

	public override void update() {
		base.update();
		if (!once && time > 0.1f && spinSound != null) {
			spinSound.Play();
			once = true;
		}
		if (spinSound != null) {
			spinSound.Volume = getSoundVolume() * 0.5f;
			if (spinSound.Volume < 0.1) {
				spinSound.Stop();
				spinSound.Dispose();
				spinSound = null;
			}
		}
		if (MathF.Abs(vel.x) < 400) {
			vel.x -= Global.spf * 450 * xDir;
		}
	}

	public override void onDestroy() {
		base.onDestroy();
		spinSound?.Stop();
		spinSound?.Dispose();
		spinSound = null;
		//int randFlipX = Helpers.randomRange(0, 1) == 0 ? -1 : 1;
		float randFlipX = Helpers.randomRange(0.75f, 1.5f);
		new Anim(pos, "double_cyclone_proj_fade", xDir, null, false) { useGravity = true, vel = new Point(-100 * xDir * randFlipX, Helpers.randomRange(-100, -50)), ttl = 2 };
		new Anim(pos, "double_cyclone_proj_fade", xDir, null, false) { useGravity = true, vel = new Point(100 * xDir * randFlipX, Helpers.randomRange(-100, -50)), ttl = 2 };
	}

	public override void onStart() {
		base.onStart();
	//	spinSound = new Sound(Global.soundBuffers["spinningblade"].soundBuffer);
	//	spinSound.Volume = 50f;
	}

	public override void onHitDamagable(IDamagable damagable) {
		base.onHitDamagable(damagable);
		var actor = damagable.actor();
		//if (actor is Character chr && chr.isStatusImmune()) return;
		//if (actor is not Character && actor is not RideArmor && actor is not Maverick) return;

		float mag = 100;
		if (!actor.grounded) actor.vel.y = 0;
		Point velVector = actor.getCenterPos().directionToNorm(pos).times(mag);
		actor.move(velVector, true);
	}

}


public class DoubleCycloneCharged : Projectile {
	public List<Sprite> spriteMids = new List<Sprite>();
	public float length = 4;
	const int maxLen = 8;
	public float maxSpeed = 400;
	public float tornadoTime;
	public float blowModifier = 0.25f;
	public float soundTime;

	public DoubleCycloneCharged(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool sendRpc = false) :
		base(weapon, pos, xDir, 150, 1, player, "double_cyclone_charged_proj", 1, 0.15f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.DoubleCycloneCharged;
		maxTime = 0.7f;
		sprite.visible = false;
		for (var i = 0; i < maxLen; i++) {
			var midSprite = new Sprite("double_cyclone_charged_proj");
			midSprite.visible = false;
			spriteMids.Add(midSprite);
		}
		destroyOnHit = false;
		shouldShieldBlock = false;

		if (sendRpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void render(float x, float y) {
		int spriteMidLen = 12;
		for (int i = 0; i < length; i++) {
			spriteMids[i].visible = true;
			spriteMids[i].draw(
				frameIndex, pos.x + x + (i * xDir * spriteMidLen), pos.y + y,
				xDir, yDir, getRenderEffectSet(), 1, 1, 1, zIndex
			);
		}

		if (Global.showHitboxes && collider != null) {
			DrawWrappers.DrawPolygon(collider.shape.points, new Color(0, 0, 255, 128), true, ZIndex.HUD, isWorldPos: true);
		}
	}

	public override void update() {
		base.update();

		Helpers.decrementTime(ref soundTime);
		//if (soundTime == 0) {
		//	playSound("straightNightmare");
	//		soundTime = 0.1f;
	//	}

		var topX = 0;
		var topY = 0;

		var spriteMidLen = 12;

		var botX = length * spriteMidLen;
		var botY = 40;

		var rect = new Rect(topX, topY, botX, botY);
		globalCollider = new Collider(rect.getPoints(), true, this, false, false, 0, new Point(0, 0));

		tornadoTime += Global.spf;
		if (tornadoTime > 0.05f) {
			if (length < maxLen) {
				length++;
			} else {
				//vel.x = maxSpeed * xDir;
			}
			tornadoTime = 0;
		}
	}

	public override void onHitDamagable(IDamagable damagable) {
		if (damagable is not Character character) return;
		if (character.charState.invincible) return;
		if (character.isImmuneToKnockback()) return;

		//character.damageHistory.Add(new DamageEvent(damager.owner, weapon.killFeedIndex, true, Global.frameCount));
		if (character.isClimbingLadder()) {
			character.setFall();
		} else if (!character.pushedByTornadoInFrame) {
			float modifier = 2;
			if (character.grounded) modifier = 1f;
			if (character.charState is Crouch) modifier = 0.5f;
			character.move(new Point(maxSpeed * 0.9f * xDir * modifier * blowModifier, 0));
			character.pushedByTornadoInFrame = true;
		}
	}
}

