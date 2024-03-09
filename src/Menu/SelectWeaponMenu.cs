﻿using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SFML.Window.Keyboard;

namespace MMXOnline;

public class WeaponCursor {
	public int index;

	public WeaponCursor(int index) {
		this.index = index;
	}
}

public class XWeaponCursor {
	public int index;

	public XWeaponCursor(int index) {
		this.index = index;
	}

	public int startOffset() {
		if (index < 9) return 0;
		else if (index >= 9 && index <= 16) return 9;
		else return 17;
	}

	public int numWeapons() {
		if (index < 9) return 9;
		return 8;
	}

	public void cycleLeft() {
		if (index < 9) index = 17;
		else if (index >= 9 && index <= 16) index = 0;
		else if (index > 16) index = 9;
	}

	public void cycleRight() {
		if (index < 9) index = 9;
		else if (index >= 9 && index <= 16) index = 17;
		else if (index > 16) index = 0;
	}
}

public class SelectWeaponMenu : IMainMenu {
	public bool inGame;
	public List<XWeaponCursor> cursors;
	public int selCursorIndex;
	public List<Point> weaponPositions = new List<Point>();
	public string error = "";
	public int maxRows = 1;
	public int maxCols = 9;
	public static List<string> weaponNames = new List<string>()
	{
			"X-Buster",
			"Homing Torpedo",
			"Chameleon Sting",
			"Rolling Shield",
			"Fire Wave",
			"Storm Tornado",
			"Electric Spark",
			"Boomerang Cutter",
			"Shotgun Ice",
			"Crystal Hunter",
			"Bubble Splash",
			"Silk Shot",
			"Spin Wheel",
			"Sonic Slicer",
			"Strike Chain",
			"Magnet Mine",
			"Speed Burner",
			"Acid Burst",
			"Parasitic Bomb",
			"Triad Thunder",
			"Spinning Blade",
			"Ray Splasher",
			"Gravity Well",
			"Frost Shield",
			"Tornado Fang",
		};

	public List<int> selectedWeaponIndices;
	public IMainMenu prevMenu;

	public SelectWeaponMenu(IMainMenu prevMenu, bool inGame) {
		this.prevMenu = prevMenu;
		for (int i = 0; i < 9; i++) {
			weaponPositions.Add(new Point(80, 42 + (i * 18)));
		}

		selectedWeaponIndices = Options.main.xLoadout.getXWeaponIndices();
		this.inGame = inGame;

		cursors = new List<XWeaponCursor>();
		foreach (var selectedWeaponIndex in selectedWeaponIndices) {
			cursors.Add(new XWeaponCursor(selectedWeaponIndex));
		}
		cursors.Add(new XWeaponCursor(Options.main.xLoadout.melee));
	}

	public bool duplicateWeapons() {
		return (
			selectedWeaponIndices[0] == selectedWeaponIndices[1] ||
			selectedWeaponIndices[1] == selectedWeaponIndices[2] ||
			selectedWeaponIndices[0] == selectedWeaponIndices[2]
		);
	}

	public bool areWeaponArrSame(List<int> wepArr1, List<int> wepArr2) {
		for (int i = 0; i < wepArr1.Count; i++) {
			if (wepArr1[i] != wepArr2[i]) return false;
		}

		return true;
	}

	public void update() {
		if (!string.IsNullOrEmpty(error)) {
			if (Global.input.isPressedMenu(Control.MenuConfirm)) {
				error = null;
			}
			return;
		}

		if (selCursorIndex < 3) {
			if (Global.input.isPressedMenu(Control.MenuLeft)) {
				cursors[selCursorIndex].index--;
				if (cursors[selCursorIndex].index == -1) cursors[selCursorIndex].index = 24; //8;
				else if (cursors[selCursorIndex].index == 8) cursors[selCursorIndex].index = 8; //16;
				else if (cursors[selCursorIndex].index == 16) cursors[selCursorIndex].index = 16; //24;
				Global.playSound("menu");
			} else if (Global.input.isPressedMenu(Control.MenuRight)) {
				cursors[selCursorIndex].index++;
				if (cursors[selCursorIndex].index == 9) cursors[selCursorIndex].index = 9; //0;
				else if (cursors[selCursorIndex].index == 17) cursors[selCursorIndex].index = 17; //9;
				else if (cursors[selCursorIndex].index == 25) cursors[selCursorIndex].index = 0; //17;
				Global.playSound("menu");
			}
			if (Global.input.isPressedMenu(Control.WeaponLeft)) {
				cursors[selCursorIndex].cycleLeft();
			} else if (Global.input.isPressedMenu(Control.WeaponRight)) {
				cursors[selCursorIndex].cycleRight();
			}
		} else {
			Helpers.menuLeftRightInc(ref cursors[selCursorIndex].index, 0, 2, playSound: true);
		}

		Helpers.menuUpDown(ref selCursorIndex, 0, 3);

		for (int i = 0; i < 3; i++) {
			selectedWeaponIndices[i] = cursors[i].index;
		}

		bool backPressed = Global.input.isPressedMenu(Control.MenuBack);
		bool selectPressed = Global.input.isPressedMenu(Control.MenuConfirm) || (backPressed && !inGame);
		if (selectPressed) {
			if (duplicateWeapons() && Options.main.xLoadout.melee != 3) {
				error = "Cannot select same weapon more than once!";
				return;
			}

			bool shouldSave = false;
			if (cursors[3].index != Options.main.xLoadout.melee) {
				Options.main.xLoadout.melee = cursors[3].index;
				if (Global.level?.mainPlayer != null) {
					Global.level.mainPlayer.loadout.xLoadout.melee = cursors[3].index;
					//Global.level.mainPlayer.syncLoadout();
					Global.level.mainPlayer.forceKill();
				}
				shouldSave = true;
			}

			if (!areWeaponArrSame(selectedWeaponIndices, Options.main.xLoadout.getXWeaponIndices())) {
				Options.main.xLoadout.weapon1 = selectedWeaponIndices[0];
				Options.main.xLoadout.weapon2 = selectedWeaponIndices[1];
				Options.main.xLoadout.weapon3 = selectedWeaponIndices[2];
				shouldSave = true;
				if (inGame) {
					if (Options.main.killOnLoadoutChange) {
						Global.level.mainPlayer.forceKill();
					} else if (!Global.level.mainPlayer.isDead) {
						Global.level.gameMode.setHUDErrorMessage(Global.level.mainPlayer, "Change will apply on next death", playSound: false);
					}
				}
			}

			if (shouldSave) {
				Options.main.saveToFile();
			}

			if (inGame) Menu.exit();
			else Menu.change(prevMenu);
		} else if (backPressed) {
			Menu.change(prevMenu);
		}
	}

	public void render() {
		if (!inGame) {
			DrawWrappers.DrawTextureHUD(Global.textures["loadoutbackground"], 0, 0);
		} else {
			DrawWrappers.DrawTextureHUD(Global.textures["pausemenuload"], 0, 0);
		}

		Fonts.drawText(FontType.Yellow, "X Loadout", Global.screenW * 0.5f, 24, Alignment.Center);
		var outlineColor = inGame ? Color.White : Helpers.LoadoutBorderColor;
		float botOffY = inGame ? 0 : -1;

		int startY = 45;
		int startX = 30;
		int startX2 = 120;
		int wepW = 18;
		int wepH = 20;

		float rightArrowPos = 278;
		float leftArrowPos = startX2 - 15;

		Global.sprites["cursor"].drawToHUD(0, startX, startY + (selCursorIndex * wepH));
		for (int i = 0; i < 4; i++) {
			float yPos = startY - 6 + (i * wepH);

			if (i == 3) {
				Fonts.drawText(FontType.Blue, "Special ", 40, yPos + 2, selected: selCursorIndex == i);

				for (int j = 0; j < 3; j++) {
					if (j == 0) {
						Global.sprites["hud_weapon_icon"].drawToHUD(30, startX2 + (j * wepW), startY + (i * wepH));
					} 
					if (j == 1) {
						Global.sprites["hud_weapon_icon"].drawToHUD(65, startX2 + (j * wepW), startY + (i * wepH));
					}
					if (j == 2) {
						Global.sprites["hud_weapon_icon"].drawToHUD(26, startX2 + (j * wepW), startY + (i * wepH));
					}

					if (cursors[3].index != j) {
						DrawWrappers.DrawRectWH(
							startX2 + (j * wepW) - 7, startY + (i * wepH) - 7, 14, 14,
							true, Helpers.FadedIconColor, 1, ZIndex.HUD, false
						);
					}
				}
				Helpers.drawWeaponSlotSymbol(
					startX2 - 8, startY + (i * wepH) - 8, "²"
				);
				if (cursors[3].index != 0) {
					DrawWrappers.DrawRectWH(
						startX2 + 4, startY + (i * wepH) + 3, 4, 5,
						true, Helpers.FadedIconColor, 1, ZIndex.HUD, false
					);
				}
				break;
			}

			Fonts.drawText(FontType.Blue, "Slot " + (i + 1).ToString(), 40, yPos + 2, selected: selCursorIndex == i);

			if (Global.frameCount % 60 < 30) {
				Fonts.drawText(
					FontType.Blue, ">", cursors[i].index < 9 ? rightArrowPos : rightArrowPos - 18, yPos + 2,
					Alignment.Center, selected: selCursorIndex == i
				);
				Fonts.drawText(
					FontType.Blue, "<", leftArrowPos, yPos + 2, Alignment.Center, selected: selCursorIndex == i
				);
			}

			for (int j = 0; j < cursors[i].numWeapons(); j++) {
				int jIndex = j + cursors[i].startOffset();
				Global.sprites["hud_weapon_icon"].drawToHUD(jIndex, startX2 + (j * wepW), startY + (i * wepH));
				/*Helpers.drawTextStd(
					(j + 1).ToString(), startX2 + (j * wepW), startY + (i * wepH) + 10, Alignment.Center
				);*/
				if (selectedWeaponIndices[i] == jIndex) {
					/*DrawWrappers.DrawRectWH(
						startX2 + (j * wepW) - 7, startY + (i * wepH) - 7, 14, 14, false,
						Helpers.DarkGreen, 1, ZIndex.HUD, false
					);*/
				} else {
					DrawWrappers.DrawRectWH(
						startX2 + (j * wepW) - 7, startY + (i * wepH) - 7, 14, 14, true,
						Helpers.FadedIconColor, 1, ZIndex.HUD, false
					);
				}
			}
		}

		int wsy = 162;

		DrawWrappers.DrawRect(
			25, wsy - 42, Global.screenW - 25, wsy + 30, true, new Color(0, 0, 0, 100),
			1, ZIndex.HUD, false, outlineColor: outlineColor
		);
		DrawWrappers.DrawRect(
			25, wsy - 42, Global.screenW - 25, wsy - 24, true, new Color(0, 0, 0, 100), 1,
			ZIndex.HUD, false, outlineColor: outlineColor
		);

		if (selCursorIndex >= 3) {
			Fonts.drawText(
				FontType.Purple, "Special Key",
				Global.halfScreenW, 126, Alignment.Center
			);
			if (cursors[3].index == 0) {
				Fonts.drawText(FontType.Blue, "Classic X", Global.halfScreenW, 146, Alignment.Center);
				Fonts.drawText(
					FontType.Green, "Gets Stronger by Buying armorrs and can switch them midfight.",
					Global.halfScreenW, wsy, Alignment.Center
				);
			}
			if (cursors[3].index == 1) {
				Fonts.drawText(FontType.Green, "X Kai", Global.halfScreenW, 146, Alignment.Center);
				Fonts.drawText(
					FontType.Blue,"Gets Stronger by Stealinng powers from others.",
					Global.halfScreenW, wsy, Alignment.Center
				);
			}
				if (cursors[3].index == 2) {
				Fonts.drawText(FontType.Green, "IX", Global.halfScreenW, 146, Alignment.Center);
				Fonts.drawText(
					FontType.Blue,"Gets Stronger overtime by stacking buffs.",
					Global.halfScreenW, wsy, Alignment.Center
				);
			}
		} else {
			int wi = selectedWeaponIndices[selCursorIndex];
			int strongAgainstIndex = Weapon.getAllXWeapons().FindIndex(w => w.weaknessIndex == wi);
			var weapon = Weapon.getAllXWeapons()[wi];
			int weakAgainstIndex = weapon.weaknessIndex;
			int[] strongAgainstMaverickIndices = getStrongAgainstMaverickFrameIndex(wi);
			int weakAgainstMaverickIndex = getWeakAgainstMaverickFrameIndex(wi);

			Fonts.drawText(
				FontType.Purple,  "Slot " + (selCursorIndex + 1).ToString() + " weapon",
				Global.halfScreenW, 126, Alignment.Center
			);
			Fonts.drawText(
				FontType.Orange, weaponNames[selectedWeaponIndices[selCursorIndex]],
				Global.halfScreenW, 146, Alignment.Center
			);
			//Global.sprites["hud_weapon_icon"].drawToHUD(weapon.weaponSlotIndex, Global.halfScreenW + 75, 148);
			Fonts.drawText(FontType.Green, "Strong against: ", 200, wsy + 2, Alignment.Right);
			if (strongAgainstIndex > 0) {
				Global.sprites["hud_weapon_icon"].drawToHUD(strongAgainstIndex, 210, wsy + 5);
			} else {
				Fonts.drawText(FontType.Grey, "Nothing", 202, wsy + 2);
			}
			if (strongAgainstMaverickIndices.Length > 0 && strongAgainstMaverickIndices[0] > 0) {
				Global.sprites["hud_weapon_icon"].drawToHUD(strongAgainstMaverickIndices[0], 228, wsy + 5);
			}
			if (strongAgainstMaverickIndices.Length > 1 && strongAgainstMaverickIndices[1] > 0) {
				Global.sprites["hud_weapon_icon"].drawToHUD(strongAgainstMaverickIndices[1], 246, wsy + 5);
			}
			Fonts.drawText(FontType.Green, "Weak against: ", 200, wsy + 17, Alignment.Right);
			if (weakAgainstIndex > 0) {
				Global.sprites["hud_weapon_icon"].drawToHUD(weakAgainstIndex, 210, wsy + 20);
			} else {
				Fonts.drawText(FontType.Grey, "Nothing", 202, wsy + 17);
			}
			if (weakAgainstMaverickIndex > 0) {
				Global.sprites["hud_weapon_icon"].drawToHUD(weakAgainstMaverickIndex, 228, wsy + 20);
			}
		}

		/*
		Helpers.drawTextStd(Helpers.menuControlText("Left/Right: Change Weapon"), Global.screenW * 0.5f, 195 + botOffY, Alignment.Center, fontSize: 16);
		Helpers.drawTextStd(Helpers.menuControlText("Up/Down: Change Slot"), Global.screenW * 0.5f, 200 + botOffY, Alignment.Center, fontSize: 16);
		Helpers.drawTextStd(Helpers.menuControlText("WeaponL/WeaponR: Quick cycle X1/X2/X3 weapons"), Global.screenW * 0.5f, 205 + botOffY, Alignment.Center, fontSize: 16);
		string helpText = Helpers.menuControlText("[BACK]: Back, [OK]: Confirm");
		if (!inGame) helpText = Helpers.menuControlText("[BACK]: Save and back");
		Helpers.drawTextStd(helpText, Global.screenW * 0.5f, 210 + botOffY, Alignment.Center, fontSize: 16);
		*/
		if (!string.IsNullOrEmpty(error)) {
			float top = Global.screenH * 0.4f;
			DrawWrappers.DrawRect(
				17, 17, Global.screenW - 17, Global.screenH - 17, true,
				new Color(0, 0, 0, 224), 0, ZIndex.HUD, false
			);
			Fonts.drawText(FontType.Red, "ERROR", Global.screenW / 2, top - 20, alignment: Alignment.Center);
			Fonts.drawText(FontType.RedishOrange, error, Global.screenW / 2, top, alignment: Alignment.Center);
			Fonts.drawTextEX(
				FontType.Grey, Helpers.controlText("Press [OK] to continue"),
				Global.screenW / 2, 20 + top, alignment: Alignment.Center
			);
		}
	}

	private int getWeakAgainstMaverickFrameIndex(int wi) {
		if (wi == (int)WeaponIds.Torpedo) return new ArmoredArmadilloWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.Sting) return new BoomerangKuwangerWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.RollingShield) return new SparkMandrillWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.FireWave) return new StormEagleWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.Tornado) return new StingChameleonWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.ElectricSpark) return new ChillPenguinWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.Boomerang) return new LaunchOctopusWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.ShotgunIce) return new FlameMammothWeapon(null).weaponSlotIndex;

		if (wi == (int)WeaponIds.StrikeChain) return new OverdriveOstrichWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.SpinWheel) return new WireSpongeWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.BubbleSplash) return new WheelGatorWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.SpeedBurner) return new BubbleCrabWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.SilkShot) return new FlameStagWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.MagnetMine) return new MorphMothWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.CrystalHunter) return new MagnaCentipedeWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.SonicSlicer) return new CrystalSnailWeapon(null).weaponSlotIndex;

		if (wi == (int)WeaponIds.AcidBurst) return new BlizzardBuffaloWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.ParasiticBomb) return new GravityBeetleWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.TriadThunder) return new TunnelRhinoWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.SpinningBlade) return new VoltCatfishWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.RaySplasher) return new CrushCrawfishWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.GravityWell) return new NeonTigerWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.FrostShield) return new BlastHornetWeapon(null).weaponSlotIndex;
		if (wi == (int)WeaponIds.TunnelFang) return new ToxicSeahorseWeapon(null).weaponSlotIndex;

		return 0;
	}

	private int[] getStrongAgainstMaverickFrameIndex(int weaponIndex) {
		return weaponIndex switch {
			(int)WeaponIds.Torpedo => new int[] { new BoomerangKuwangerWeapon(null).weaponSlotIndex },
			(int)WeaponIds.Sting => new int[] { new StormEagleWeapon(null).weaponSlotIndex },
			(int)WeaponIds.RollingShield => new int[] { new LaunchOctopusWeapon(null).weaponSlotIndex },
			(int)WeaponIds.FireWave => new int[] { new ChillPenguinWeapon(null).weaponSlotIndex },
			(int)WeaponIds.Tornado => new int[] { new FlameMammothWeapon(null).weaponSlotIndex },
			(int)WeaponIds.ElectricSpark => new int[] { new ArmoredArmadilloWeapon(null).weaponSlotIndex },
			(int)WeaponIds.Boomerang => new int[] { new StingChameleonWeapon(null).weaponSlotIndex },
			(int)WeaponIds.ShotgunIce => new int[] {
				new SparkMandrillWeapon(null).weaponSlotIndex,
				new VelguarderWeapon(null).weaponSlotIndex
			},
			(int)WeaponIds.StrikeChain => new int[] { new WheelGatorWeapon(null).weaponSlotIndex },
			(int)WeaponIds.SpinWheel => new int[] { new BubbleCrabWeapon(null).weaponSlotIndex },
			(int)WeaponIds.BubbleSplash => new int[] { new FlameStagWeapon(null).weaponSlotIndex },
			(int)WeaponIds.SpeedBurner => new int[] {
				new MorphMothWeapon(null).weaponSlotIndex,
				new FakeZeroWeapon(null).weaponSlotIndex
			},
			(int)WeaponIds.SilkShot => new int[] { new MagnaCentipedeWeapon(null).weaponSlotIndex },
			(int)WeaponIds.MagnetMine => new int[] { new CrystalSnailWeapon(null).weaponSlotIndex },
			(int)WeaponIds.CrystalHunter => new int[] { new OverdriveOstrichWeapon(null).weaponSlotIndex },
			(int)WeaponIds.SonicSlicer => new int[] { new WireSpongeWeapon(null).weaponSlotIndex },
			(int)WeaponIds.AcidBurst => new int[] {
				new TunnelRhinoWeapon(null).weaponSlotIndex,
				new DrDopplerWeapon(null).weaponSlotIndex
			},
			(int)WeaponIds.ParasiticBomb => new int[] { new BlizzardBuffaloWeapon(null).weaponSlotIndex },
			(int)WeaponIds.TriadThunder => new int[] { new CrushCrawfishWeapon(null).weaponSlotIndex },
			(int)WeaponIds.SpinningBlade => new int[] { new NeonTigerWeapon(null).weaponSlotIndex },
			(int)WeaponIds.RaySplasher => new int[] { new GravityBeetleWeapon(null).weaponSlotIndex },
			(int)WeaponIds.GravityWell => new int[] { new BlastHornetWeapon(null).weaponSlotIndex },
			(int)WeaponIds.FrostShield => new int[] { new ToxicSeahorseWeapon(null).weaponSlotIndex },
			(int)WeaponIds.TunnelFang => new int[] { new VoltCatfishWeapon(null).weaponSlotIndex },
			_ => new int[] {}
		};
	}
}
