using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;

/*
in Order for this to work as intended I added this to Damager.cs's applydamage section

			if (character.sprite.name.Contains("block") && damage > 0 && !isArmorPiercing(projId)) {
				if (!hitFromBehind(character, damagingActor, owner, projId)) {
					damage--;
					flinch = 0;
					if (damage < 3) {
						damage = 0;
						character.playSound("m10ding");
					}
				}
			}
*/


public class BlockWCUT : CharState {


	public const float maxBlockTime = 4;

	public float blockTime;


	public BlockWCUT() : base("block") {
		exitOnAirborne = true;
		attackCtrl = true;
		normalCtrl = true;
		stunResistant = true;
		immuneToWind = true;
		blockTime = maxBlockTime;
	}

	public override void update() {
		base.update();
		blockTime -= Global.spf;
		bool isHoldingGuard = (
			player.input.isHeld(Control.L2, player)
		);
		if (blockTime == 0) {
			character.changeState(new BlockBreak(character.xDir), true);
		}
		if (!isHoldingGuard) {
			character.changeToIdleOrFall();
			return;
		}
		if (Global.level.gameMode.isOver) {
			if (Global.level.gameMode.playerWon(player)) {
				if (!character.sprite.name.Contains("_win")) {
					character.changeSpriteFromName("win", true);
				}
			} else {
				if (!character.sprite.name.Contains("lose")) {
					character.changeSpriteFromName("lose", true);
				}
			}
		}
	}
}



public class BlockBreak : CharState {
	public int hurtDir;
	public float hurtSpeed;

	public BlockBreak(int dir) : base("land") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}
		/*
		if (this.character.isAnimOver()) {
			this.character.changeToIdleOrFall();
		}
		*/
		if (hurtSpeed == 0) {
			character.changeToIdleOrFall();
		}
	}
}

public class Clang : CharState {
	public int hurtDir;
	public float hurtSpeed;

	public Clang(int dir) : base("clang") {
		hurtDir = dir;
		hurtSpeed = dir * 100;
	}

	public override void update() {
		base.update();
		if (hurtSpeed != 0) {
			hurtSpeed = Helpers.toZero(hurtSpeed, 400 * Global.spf, hurtDir);
			character.move(new Point(hurtSpeed, 0));
		}
		/*
		if (this.character.isAnimOver()) {
			this.character.changeToIdleOrFall();
		}
		*/
		if (hurtSpeed == 0) {
			character.changeToIdleOrFall();
		}
	}
}