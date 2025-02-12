using System;
using System.Collections.Generic;
using ProtoBuf;
using SFML.Graphics;

namespace MMXOnline;

[ProtoContract]
public class AxlWCLoadout {
	[ProtoMember(1)] public int sideArm;
	[ProtoMember(2)] public int weapon1;
	[ProtoMember(3)] public int weapon2;

	public List<int> getBassWeaponIndices() {
		return new List<int>() { sideArm, weapon1, weapon2 };
	}

	public static AxlWCLoadout createDefault() {
		return new AxlWCLoadout() {
			sideArm = 0, weapon1 = 0, weapon2 = 1
		};
	}

	public void validate() {
		if (weapon1 < 0 || weapon1 > 9) weapon1 = 0;
		if (weapon2 < 0 || weapon2 > 9) weapon2 = 0;
		//if (weapon3 < 0 || weapon3 > 9) weapon3 = 0;

		/*if ((weapon1 == weapon2 && weapon1 >= 0) ||
			(weapon1 == weapon3 && weapon2 >= 0) ||
			(weapon2 == weapon3 && weapon3 >= 0)) {
			weapon1 = 0;
			weapon2 = 1;
			weapon3 = 2;
		}*/
	}
}


public class AxlWCWeaponMenu : IMainMenu {
	// Menu controls.
	public IMainMenu prevMenu;
	public int cursorRow;
	bool inGame;
	public AxlWCLoadout targetLoadout;

	// Loadout items.

	public int[] sWeapons = [
		0,1,2
	];

	public int[][] weaponIcons = [
		[0],
		[0, 1, 2, 3, 4, 5, 6, 7, 8],
		[0, 1, 2, 3, 4, 5, 6, 7, 8]
	];
	public string[] categoryNames = [
		"Sidearm", "Main", "Main 2"
	];

	public AxlWeaponWC[] specialWeapons = [
		new RayGunWC(),
		new BlastLauncherWC(),
		new BlackArrowWC(),
		new SpiralMagnumWC(),
		new PlasmaGunWC(),
		new BoundBlasterWC(),
		new IceGattlingWC(),
		new FlameBurnerWC(),
	];
	public AxlWeaponWC[] sideArm = [
		new AxlBulletWC()
	];

	public AxlWCWeaponMenu(IMainMenu prevMenu, bool inGame) {
		this.prevMenu = prevMenu;
		this.inGame = inGame;
		targetLoadout = Options.main.axlWCLoadout;
		sWeapons[0] = targetLoadout.sideArm;
		sWeapons[1] = targetLoadout.weapon1;
		sWeapons[2] = targetLoadout.weapon2;
	}

	public void update() {
		bool okPressed = Global.input.isPressedMenu(Control.MenuConfirm);
		bool backPressed = Global.input.isPressedMenu(Control.MenuBack);
		Helpers.menuUpDown(ref cursorRow, 0, 2);
		if(cursorRow == 0){
		Helpers.menuLeftRightInc(ref sWeapons[cursorRow], 0, sideArm.Length - 1, true, playSound: true);
		}else{
		Helpers.menuLeftRightInc(ref sWeapons[cursorRow], 0, specialWeapons.Length - 1, true, playSound: true);}
		

		// Random loadout
		bool randomPressed = Global.input.isPressedMenu(Control.Special1);

		if (randomPressed) {
			bool duplicatedWeapons = true;

			for (int i = 0; i < 3; i++) {
				if (i != cursorRow) sWeapons[i] = Helpers.randomRange(0, 8);
			}
			Global.playSound("menu");
			
			while (duplicatedWeapons) {
				for (int i = 0; i < 3; i++) {
					if (i != cursorRow) sWeapons[i] = Helpers.randomRange(0, 8);
				}

				if (!duplicateWeapons()) duplicatedWeapons = false;
			}
		}

		if (okPressed || backPressed && !inGame) {
			bool isChanged = false;
			if (!duplicateWeapons()) {
				targetLoadout.sideArm = sWeapons[0];
				targetLoadout.weapon1 = sWeapons[1];
				targetLoadout.weapon2 = sWeapons[2];
				isChanged = true;
			}
			if (isChanged) {
				if (inGame && Global.level != null) {
					if (Options.main.killOnLoadoutChange) {
						Global.level.mainPlayer.forceKill();
					} else {
						Global.level.gameMode.setHUDErrorMessage(
							Global.level.mainPlayer,
							"Loadout change will apply on the next respawn",
							playSound: false
						);
					}
				}
				Options.main.saveToFile();
			}
			
			Menu.change(prevMenu);
			return;
		}
		if (backPressed) {
			Menu.change(prevMenu);
		}
	}

	public bool duplicateWeapons() {
		return sWeapons[1] == sWeapons[2];
	}

	public void render() {
		if (!inGame) {
			DrawWrappers.DrawTextureHUD(Global.textures["loadoutbackground"], 0, 0);
		} else {
			DrawWrappers.DrawTextureHUD(Global.textures["pausemenuload"], 0, 0);
		}
		Fonts.drawText(FontType.Blue, "Bass Loadout", Global.screenW * 0.5f, 24, Alignment.Center);

		int startY = 55;
		int startX = 30;
		int startX2 = 120;
		int wepW = 18;
		int wepH = 20;
		float rightArrowPos = Global.screenW - 106;
		float leftArrowPos = startX2 - 15;
		Global.sprites["cursor"].drawToHUD(0, startX, startY - 1 + cursorRow * wepH);

		for (int i = 0; i < 3; i++) {
			// Position.
			float yPos = startY - 6 + (i * wepH);
			// Current variable.
			int selectVar = i switch {
				_ => sWeapons[i]
			};
			// Category name.
			Fonts.drawText(FontType.Blue, categoryNames[i], 40, yPos - 1, selected: i == cursorRow);
			if (Global.frameCount % 60 < 30) {
				Fonts.drawText(
					FontType.Blue, ">", i < 3 ? rightArrowPos : rightArrowPos - 108, yPos - 1,
					Alignment.Center, selected: cursorRow == i
				);
				Fonts.drawText(
					FontType.Blue, "<", leftArrowPos, yPos - 1 , Alignment.Center, selected: cursorRow == i
				);
			}
			// Icons.
			for (int j = 0; j < weaponIcons[i].Length; j++) {
				// Draw icon sprite.
				Global.sprites["hud_bars_slot"].drawToHUD(
					weaponIcons[i][j], startX2 + (j * wepW), startY + (i * wepH)
				);
				// Darken non-selected icons.
				if (selectVar != j) {
					DrawWrappers.DrawRectWH(
						startX2 + (j * wepW) - 7, startY + (i * wepH) - 7,
						14, 14, true, Helpers.FadedIconColor, 1, ZIndex.HUD, false
					);
				}
			}
		}
		// Weapon and data.
		string menuTitle = "";
		string weaponTitle = "";
		string weaponDescription = "";
		Weapon currentWeapon = new();
		
			currentWeapon = specialWeapons[sWeapons[cursorRow]];
			menuTitle = "Special Weapon";
			weaponTitle = currentWeapon.displayName;;
		//	weaponDescription = currentWeapon.descriptionV2;;
		
		// Draw rectangle.
		int wsy = 127;
		DrawWrappers.DrawRect(
			25, wsy, Global.screenW - 25, wsy + 18, true, new Color(0, 0, 0, 100), 1,
			ZIndex.HUD, false, outlineColor: Helpers.LoadoutBorderColor
		);
		DrawWrappers.DrawRect(
			25, wsy, Global.screenW - 25, wsy + 72, true, new Color(0, 0, 0, 100), 1,
			ZIndex.HUD, false, outlineColor: Helpers.LoadoutBorderColor
		);
		// Draw descriptions.
		float titleY1 = wsy + 3;
		float titleY2 = titleY1 + 19;
		float row1Y = titleY2 + 13;
		float row2Y = row1Y + 25;
		Fonts.drawText(
			FontType.Blue, menuTitle,
			Global.halfScreenW, titleY1, Alignment.Center
		);
		Fonts.drawText(
			FontType.Purple, weaponTitle,
			Global.halfScreenW, titleY2, Alignment.Center
		);
		/* Fonts.drawText(
			FontType.WhiteSmall, weaponDescription,
			Global.halfScreenW, row1Y, Alignment.Center
		); */
	}
}
