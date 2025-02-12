using System;
using System.Collections.Generic;

namespace MMXOnline;

public class DarkHold : Weapon {
	public static DarkHold netWeapon = new();

	public DarkHold() : base() {
		index = (int)WeaponIds.DarkHold;
		killFeedIndex = 0;
		weaponBarBaseIndex = (int)WeaponBarIndex.DarkHold;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = (int)SlotIndex.DHold;
		weaknessIndex = (int)WeaponIds.ShotgunIce;
		shootSounds = new string[] { "electricSpark", "electricSpark", "electricSpark", "electricSpark" , ""};
		fireRate = 30;
		damage = "0";
		effect =  "Paralizes the time and slows projectiles. \nCharged: Bigger radious.";
		hitcooldown = "0/0.5";
		Flinch = "Paralize";
		FlinchCD = "0";
		type = index;
		displayName = "DarkHold ";
	}

	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.getShootPos();
		int xDir = character.getShootXDir();
		Player player = character.player;

		if (chargeLevel < 2) {
			new ElectricSparkProj(this, pos, xDir, player, 0, player.getNextActorNetId(), rpc: true);
		}
		if (chargeLevel == 2) {
			new ElectricSparkProjSemiCharged(this, pos, xDir, player, 0, player.getNextActorNetId(), rpc: true);
		}
		if (chargeLevel == 3 || chargeLevel >= 3  && player.hasArmArmor(2)) {
			new ElectricSparkProjChargedStart(this, pos, xDir, player, player.getNextActorNetId(), true);
		}
		if (chargeLevel == 4 && !player.hasArmArmor(2)) {
				character.changeState(new ESparkUltraCharged(character.grounded), true);
		}
	}
}