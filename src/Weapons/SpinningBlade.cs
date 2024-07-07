﻿using System;
using System.Collections.Generic;
using SFML.Audio;
using SFML.Graphics;

namespace MMXOnline;

public class SpinningBlade : Weapon {
	public SpinningBlade() : base() {
		shootSounds = new string[] { "spinningBlade", "spinningBlade", "spinningBlade", "spinningBladeCharged" };
		rateOfFire = 1.25f;
		index = (int)WeaponIds.SpinningBlade;
		weaponBarBaseIndex = 20;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 20;
		killFeedIndex = 43;
		weaknessIndex = (int)WeaponIds.TriadThunder;
	}

private bool GBDdisk;

	public override void getProjectile(Point pos, int xDir, Player player, float chargeLevel, ushort netProjId) {
		
		if (player?.character is MegamanX mmx){
			if (chargeLevel < 3) {
			player.setNextActorNetId(netProjId);
			new SpinningBladeProj(this, pos, xDir, 0, player, player.getNextActorNetId(true));
			new SpinningBladeProj(this, pos, xDir, 1, player, player.getNextActorNetId(true));
			} else  {
				var csb = new SpinningBladeProjCharged(this, pos, xDir, player, netProjId);
				if (mmx.ownedByLocalPlayer) {
				mmx.chargedSpinningBlade = csb;
				}
			}
		}
		if (player?.character != null && player.isGBD){
	
		player.setNextActorNetId(netProjId);
			
		if (!player.input.isHeld(Control.Down, player))	new SpinningBladeProj(this, pos, xDir, 0, player, player.getNextActorNetId(true));
		if (player.input.isHeld(Control.Down, player))new SpinningBladeProj(this, pos, xDir, 1, player, player.getNextActorNetId(true));
		
		}


	}

	public override float getAmmoUsage(int chargeLevel) {
		if (chargeLevel < 3) return 2;
		return base.getAmmoUsage(chargeLevel);
	}
}

public class SpinningBladeProj : Projectile {
	Sound? spinSound;
	bool once;

	public SpinningBladeProj(Weapon weapon, Point pos, int xDir, int type, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 250, 2, player, "spinningblade_proj", 0, 0, netProjId, player.ownedByLocalPlayer) {
		maxTime = 2f;
		projId = (int)ProjIds.SpinningBlade;
		fadeSprite = "explosion";
		fadeSound = "crush";
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
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public override void update()
	{
		base.update();
		if (!once && time > 0.1f && spinSound != null)
		{
			spinSound.Play();
			once = true;
			
		}
		if (spinSound != null)
		{
			spinSound.Volume = getSoundVolume() * 0.5f;
		}
		if (ownedByLocalPlayer && MathF.Abs(vel.x) < 400f)
		{
			vel.x -= Global.spf * 450f * (float)xDir;
		}
		if (time >= 1) damager.damage = 3;
		if (time >= 1) damager.flinch = 4;
		
	}

	public override void onDestroy()
	{
		base.onDestroy();
		spinSound?.Stop();
		spinSound?.Dispose();
		spinSound = null;
		float randFlipX = Helpers.randomRange(0.75f, 1.5f);
		new Anim(pos, "spinningblade_piece1", xDir, null, destroyOnEnd: false)
		{
			useGravity = true,
			vel = new Point((float)(-100 * xDir) * randFlipX, Helpers.randomRange(-100, -50)),
			ttl = 2f
		};
		new Anim(pos, "spinningblade_piece2", xDir, null, destroyOnEnd: false)
		{
			useGravity = true,
			vel = new Point((float)(100 * xDir) * randFlipX, Helpers.randomRange(-100, -50)),
			ttl = 2f
		};
	}
}
  

public class SpinningBladeProjCharged : Projectile {
	public MegamanX? character;
	public float xDist;
	const float maxXDist = 90;
	public float spinAngle;
	bool retracted;
	bool soundPlayed;
	public SpinningBladeProjCharged(Weapon weapon, Point pos, int xDir, Player player, ushort netProjId, bool rpc = false) :
		base(weapon, pos, xDir, 250, 2, player, "spinningblade_charged", Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer) {
		projId = (int)ProjIds.SpinningBladeCharged;
		shouldShieldBlock = false;
		destroyOnHit = false;
		character = (player.character as MegamanX);
		shouldVortexSuck = false;
		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
		canBeLocal = false;
	}

	public override void update() {
		base.update();

		if (time > 0.25f && !soundPlayed) {
			soundPlayed = true;
			playSound("spinningBlade");
		}

		if (!ownedByLocalPlayer) return;

		if (character == null || character.destroyed) {
			destroySelf();
			return;
		}

		if (time > 2) retracted = true;

		if (!retracted) {
			if (xDist < maxXDist) {
				xDist += Global.spf * 240;
			} else {
				xDist = maxXDist;
			}
		} else {
			if (xDist > 0) {
				xDist -= Global.spf * 240;
			} else {
				xDist = 0;
				destroySelf();
				character.removeBusterProjs();
			}
		}

		float xOff = Helpers.cosd(spinAngle) * xDist;
		float yOff = Helpers.sind(spinAngle) * xDist;
		changePos(character.getShootPos().addxy(xDir * xOff, yOff));

		if (character.player.input.isPressed(Control.Shoot, character.player) && xDist >= maxXDist) {
			retracted = true;
		}

		if (character.player.input.isHeld(Control.Up, character.player)) {
			spinAngle -= Global.spf * 360;
		} else if (character.player.input.isHeld(Control.Down, character.player)) {
			spinAngle += Global.spf * 360;
		}
	}

	public override void render(float x, float y) {
		base.render(x, y);
		Point sPos = character.getShootPos();
		DrawWrappers.DrawLine(sPos.x, sPos.y, pos.x, pos.y, new Color(0, 224, 0), 3, zIndex - 100);
		DrawWrappers.DrawLine(sPos.x, sPos.y, pos.x, pos.y, new Color(224, 224, 96), 1, zIndex - 100);
		Global.sprites["spinningblade_base"].draw(MathInt.Round(Global.frameCount * 0.25f) % 3, sPos.x, sPos.y, 1, 1, null, 1, 1, 1, zIndex);
	}

	public override void onDestroy() {
		base.onDestroy();
		if (!ownedByLocalPlayer) return;
		character?.removeBusterProjs();
	}
}
