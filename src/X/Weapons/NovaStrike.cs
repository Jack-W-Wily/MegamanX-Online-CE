using System;

namespace MMXOnline;

public class HyperNovaStrike : Weapon {
	public static HyperNovaStrike netWeapon = new();
	public const float ammoUsage = 14;

	public HyperNovaStrike() : base() {
		//damager = new Damager(player, 4, Global.defFlinch, 0.5f);
		shootSounds = new string[] { "", "", "", "" };
		fireRate = 10;
		index = (int)WeaponIds.NovaStrike;
		weaponBarBaseIndex = (int)WeaponBarIndex.NovaStrike;
		weaponBarIndex = weaponBarBaseIndex;
		weaponSlotIndex = 95;
		killFeedIndex = 104;
		ammo = 28;
		drawGrayOnLowAmmo = true;
		drawRoundedDown = true;
		type = index;
		displayName = "Nova Strike ";
	}

	public override void shoot(Character character, int[] args) {
		if (character.ownedByLocalPlayer) {
			Point inputDir = character.player.input.getInputDir(character.player);
			character.changeState(new NovaStrikeState(inputDir), true);
		}
	}

	public override float getAmmoUsage(int chargeLevel) {
		if (Global.level?.isHyper1v1() == true) {
			return 0;
		}
		return ammoUsage;
	}

	public override bool canShoot(int chargeLevel, Player player) {
		return player.character?.flag == null && ammo >= getChipFactoredAmmoUsage(player);
	}

	public float getChipFactoredAmmoUsage(Player player) {
		return player.character is MegamanX mmx && mmx.hyperArmArmor == ArmorId.Max ? ammoUsage / 2 : ammoUsage;
	}
}

public class NovaStrikeState : CharState {
	int upOrDown;
	int leftOrRight;

	bool earthquake;
	public NovaStrikeState(Point inputDir) : base(getNovaDir(inputDir), "", "", "nova_strike_start") {
		immuneToWind = true;
		invincible = true;
		normalCtrl = false;
		attackCtrl = false;
		useGravity = false;

		if (inputDir.y != 0) upOrDown = (int)inputDir.y;
		else leftOrRight = 1;
	}

	public override void update() {
		base.update();

		if (!once && character.isAnimOver()) {
			once = true;

			//if (Helpers.randomRange(0, 10) < 10) {
		//		character.playSound("novaStrikeX4", forcePlay: false, sendRpc: true);
		//	} else {
				character.playSound("novaStrikeX6", forcePlay: false, sendRpc: true);
		//	}
		}

		if (!inTransition()) {
			if (!character.tryMove(new Point(character.xDir * 350 * leftOrRight, 350 * upOrDown), out _) ||
				character.flag != null || stateTime > 0.6f
			) {
				character.changeToIdleOrFall();
				return;
			}
		}

		CollideData? collideData = Global.level.checkTerrainCollisionOnce(character, character.xDir, 0);
		if (collideData != null && collideData.isSideWallHit() && character.ownedByLocalPlayer) {
			
			if (!earthquake){
			character.shakeCamera(sendRpc: true);
			earthquake = true;
			var weapon = new HyperNovaStrike();
			new TriadThunderQuake(weapon, character.pos, 1, player, player.getNextActorNetId(), rpc: true);
			new MechFrogStompShockwave(new MechFrogStompWeapon(player), 
			character.pos.addxy(6 * character.xDir, 0f), character.xDir, player, 
			player.getNextActorNetId(), rpc: true);	
			character.playSound("crash", forcePlay: false, sendRpc: true);
			}
			
		} 

			for (int i = 1; i <= 4; i++) {
				CollideData collideData2 = Global.level.checkTerrainCollisionOnce(character, 0, -10 * i, autoVel: true);
				if (!character.grounded && collideData2 != null && collideData2.gameObject is Wall wall
					&& !wall.isMoving && !wall.topWall && collideData2.isCeilingHit()) {		
			if (!earthquake){
			character.shakeCamera(sendRpc: true);
			earthquake = true;
			var weapon = new HyperNovaStrike();
			new TriadThunderQuake(weapon, character.pos, 1, player, player.getNextActorNetId(), rpc: true);
			new MechFrogStompShockwave(new MechFrogStompWeapon(player), 
			character.pos.addxy(6 * character.xDir, 0f), character.xDir, player, 
			player.getNextActorNetId(), rpc: true);	
			character.playSound("crash", forcePlay: false, sendRpc: true);
			}
			}

			if (character.sprite.name.Contains("down")){
			if (!earthquake){
			character.shakeCamera(sendRpc: true);
			earthquake = true;
			var weapon = new HyperNovaStrike();
			new TriadThunderQuake(weapon, character.pos, 1, player, player.getNextActorNetId(), rpc: true);
			new MechFrogStompShockwave(new MechFrogStompWeapon(player), 
			character.pos.addxy(6 * character.xDir, 0f), character.xDir, player, 
			player.getNextActorNetId(), rpc: true);	
			character.playSound("crash", forcePlay: false, sendRpc: true);
			}
			}
		}
	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		character.stopMoving();
		character.stopCharge();
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.yDir = 1;
	}

	static string getNovaDir(Point input) {
		if (input.y != 0) {
			if (input.y == -1) return "nova_strike_up";
			return "nova_strike_down";
		}

		return "nova_strike";
	}
}
