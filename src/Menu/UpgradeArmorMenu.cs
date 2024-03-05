﻿using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public class UpgradeArmorMenu : IMainMenu {
	public int selectArrowPosY;
	public IMainMenu prevMenu;

	public Point optionPos1 = new Point(25, 40);
	public Point optionPos2 = new Point(25, 80);
	public Point optionPos3 = new Point(25, 110);
	public Point optionPos4 = new Point(25, 170);

	public Level level { get { return Global.level; } }
	public Player mainPlayer { get { return Global.level.mainPlayer; } }

	public static int xGame = 1;

	public UpgradeArmorMenu(IMainMenu prevMenu) {
		this.prevMenu = prevMenu;
	}

	public void update() {
		if (updateHyperArmorUpgrades(mainPlayer)) return;

		// Should not be able to reach here but preventing upgrades just in case
		if (!mainPlayer.canUpgradeXArmor()) return;

		Helpers.menuUpDown(ref selectArrowPosY, 0, 3);

		if (Global.input.isPressedMenu(Control.MenuLeft)) {
			xGame--;
			if (xGame < 1) {
				xGame = 1;
				if (!Global.level.server.disableHtSt) {
					UpgradeMenu.onUpgradeMenu = true;
					Menu.change(new UpgradeMenu(prevMenu));
					return;
				}
			}
		} else if (Global.input.isPressedMenu(Control.MenuRight)) {
			xGame++;
			if (xGame > 3) {
				xGame = 3;
				if (!Global.level.server.disableHtSt) {
					UpgradeMenu.onUpgradeMenu = true;
					Menu.change(new UpgradeMenu(prevMenu));
					return;
				}
			}
		}

		if (Global.input.isPressedMenu(Control.MenuBack)) {
			Menu.change(prevMenu);
			return;
		}

		if (mainPlayer.hasGoldenArmor() || mainPlayer.hasUltimateArmor()) {
			return;
		}

		if (Global.input.isPressedMenu(Control.MenuConfirm)) {
			if (selectArrowPosY == 0) {
				if (mainPlayer.helmetArmorNum != xGame) {
					if (!mainPlayer.isHeadArmorPurchased(xGame)) {
						if (mainPlayer.currency >= MegamanX.headArmorCost) {
							purchaseHelmetArmor(mainPlayer, xGame);
							Global.playSound("ching");
							if (mainPlayer.helmetArmorNum == 0) {
								upgradeHelmetArmor(mainPlayer, xGame);
							}
						}
					} else {
						upgradeHelmetArmor(mainPlayer, 0);
						upgradeHelmetArmor(mainPlayer, xGame);
						Global.playSound("ching");
					}
				} else if (mainPlayer.hasAllX3Armor() && !mainPlayer.hasChip(2)) {
					mainPlayer.setChipNum(2, false);
					Global.playSound("ching");
				}
			}
			if (selectArrowPosY == 1) {
				if (mainPlayer.bodyArmorNum != xGame) {
					if (!mainPlayer.isBodyArmorPurchased(xGame)) {
						if (mainPlayer.currency >= MegamanX.bodyArmorCost) {
							purchaseBodyArmor(mainPlayer, xGame);
							Global.playSound("ching");
							if (mainPlayer.bodyArmorNum == 0) {
								upgradeBodyArmor(mainPlayer, xGame);
							}
						}
					} else {
						upgradeBodyArmor(mainPlayer, 0);
						upgradeBodyArmor(mainPlayer, xGame);
						Global.playSound("ching");
					}
				} else if (mainPlayer.hasAllX3Armor() && !mainPlayer.hasChip(1)) {
					mainPlayer.setChipNum(1, false);
					Global.playSound("ching");
				}
			}
			if (selectArrowPosY == 2) {
				if (mainPlayer.armArmorNum != xGame) {
					if (!mainPlayer.isArmArmorPurchased(xGame)) {
						if (mainPlayer.currency >= MegamanX.armArmorCost) {
							purchaseArmArmor(mainPlayer, xGame);
							Global.playSound("ching");
							if (mainPlayer.armArmorNum == 0) {
								upgradeArmArmor(mainPlayer, xGame);
							}
						}
					} else {
						upgradeArmArmor(mainPlayer, 0);
						upgradeArmArmor(mainPlayer, xGame);
						Global.playSound("ching");
					}
				} else if (mainPlayer.hasAllX3Armor() && !mainPlayer.hasChip(3)) {
					mainPlayer.setChipNum(3, false);
					Global.playSound("ching");
				}
			}
			if (selectArrowPosY == 3) {
				if (mainPlayer.bootsArmorNum != xGame) {
					if (!mainPlayer.isBootsArmorPurchased(xGame)) {
						if (mainPlayer.currency >= MegamanX.bootsArmorCost) {
							purchaseBootsArmor(mainPlayer, xGame);
							Global.playSound("ching");
							if (mainPlayer.bootsArmorNum == 0) {
								upgradeBootsArmor(mainPlayer, xGame);
							}
						}
					} else {
						upgradeBootsArmor(mainPlayer, 0);
						upgradeBootsArmor(mainPlayer, xGame);
						Global.playSound("ching");
					}
				} else if (mainPlayer.hasAllX3Armor() && !mainPlayer.hasChip(0)) {
					mainPlayer.setChipNum(0, false);
					Global.playSound("ching");
				}
			}
		} else if (Global.input.isPressedMenu(Control.MenuAlt)) {
			if (selectArrowPosY == 0) {
				if (mainPlayer.helmetArmorNum == xGame) {
					if (mainPlayer.hasAllX3Armor() && mainPlayer.hasChip(2)) {
						mainPlayer.setChipNum(2, true);
					} else {
						upgradeHelmetArmor(mainPlayer, 0);
					}
				}
			}
			if (selectArrowPosY == 1) {
				if (mainPlayer.bodyArmorNum == xGame) {
					if (mainPlayer.hasAllX3Armor() && mainPlayer.hasChip(1)) {
						mainPlayer.setChipNum(1, true);
					} else {
						upgradeBodyArmor(mainPlayer, 0);
					}
				}
			}
			if (selectArrowPosY == 2) {
				if (mainPlayer.armArmorNum == xGame) {
					if (mainPlayer.hasAllX3Armor() && mainPlayer.hasChip(3)) {
						mainPlayer.setChipNum(2, true);
					} else {
						upgradeArmArmor(mainPlayer, 0);
					}
				}
			}
			if (selectArrowPosY == 3) {
				if (mainPlayer.bootsArmorNum == xGame) {
					if (mainPlayer.hasAllX3Armor() && mainPlayer.hasChip(0)) {
						mainPlayer.setChipNum(0, true);
					} else {
						upgradeBootsArmor(mainPlayer, 0);
					}
				}
			}
		}
	}

	public static void upgradeHelmetArmor(Player player, int type) {
		player.helmetArmorNum = type;
	}

	public static void purchaseHelmetArmor(Player player, int type) {
		if (!player.isHeadArmorPurchased(type)) {
			player.currency -= MegamanX.headArmorCost;
			player.setHeadArmorPurchased(type);
		}
	}

	public static void upgradeBodyArmor(Player player, int type) {
		player.bodyArmorNum = type;
		if (type == 2) {
			player.addGigaCrush();
		}

		if (type == 0) {
			player.removeGigaCrush();
		}
	}

	public static void purchaseBodyArmor(Player player, int type) {
		if (!player.isBodyArmorPurchased(type)) {
			player.currency -= MegamanX.bodyArmorCost;
			player.setBodyArmorPurchased(type);
		}
	}

	public static void upgradeArmArmor(Player player, int type) {
		player.armArmorNum = type;
		if (type == 3) {
			//player.addHyperCharge();
		}
		if (type == 0) {
			//player.removeHyperCharge();
		}
	}

	public static void purchaseArmArmor(Player player, int type) {
		if (type != 0 && !player.isArmArmorPurchased(type)) {
			player.currency -= MegamanX.armArmorCost;
			player.setArmArmorPurchased(type);
		}
	}

	public static void upgradeBootsArmor(Player player, int type) {
		player.bootsArmorNum = type;
	}

	public static void purchaseBootsArmor(Player player, int type) {
		if (type != 0 && !player.isBootsArmorPurchased(type)) {
			player.currency -= MegamanX.bootsArmorCost;
			player.setBootsArmorPurchased(type);
		}
	}

	public void render() {
		var gameMode = level.gameMode;
		DrawWrappers.DrawTextureHUD(Global.textures["pausemenu"], 0, 0);

		Helpers.drawTextStd(TCat.Title, string.Format("Upgrade Armor(MMX{0})", xGame), Global.screenW * 0.5f, 8, Alignment.Center, fontSize: 48);
		Helpers.drawTextStd(
			Global.nameCoins + ": " + mainPlayer.currency,
			Global.screenW * 0.5f, 25, Alignment.Center
		);

		if (Global.frameCount % 60 < 30) {
			if (xGame == 1) {
				Helpers.drawTextStd(TCat.Option, ">", Global.screenW - 19, Global.halfScreenH, Alignment.Center, fontSize: 32);
				Helpers.drawTextStd(TCat.Option, "X2", Global.screenW - 19, Global.halfScreenH + 15, Alignment.Center, fontSize: 20);

				if (!Global.level.server.disableHtSt) {
					Helpers.drawTextStd(TCat.Option, "<", 12, Global.halfScreenH, Alignment.Center, fontSize: 32);
					Helpers.drawTextStd(TCat.Option, "Items", 12, Global.halfScreenH + 15, Alignment.Center, fontSize: 20);
				}
			} else if (xGame == 2) {
				Helpers.drawTextStd(TCat.Option, ">", Global.screenW - 19, Global.halfScreenH, Alignment.Center, fontSize: 32);
				Helpers.drawTextStd(TCat.Option, "X3", Global.screenW - 19, Global.halfScreenH + 15, Alignment.Center, fontSize: 20);

				Helpers.drawTextStd(TCat.Option, "<", 12, Global.halfScreenH, Alignment.Center, fontSize: 32);
				Helpers.drawTextStd(TCat.Option, "X1", 12, Global.halfScreenH + 15, Alignment.Center, fontSize: 20);
			} else if (xGame == 3) {
				Helpers.drawTextStd(TCat.Option, "<", 12, Global.halfScreenH, Alignment.Center, fontSize: 32);
				Helpers.drawTextStd(TCat.Option, "X2", 12, Global.halfScreenH + 15, Alignment.Center, fontSize: 20);

				if (!Global.level.server.disableHtSt) {
					Helpers.drawTextStd(TCat.Option, ">", Global.screenW - 19, Global.halfScreenH, Alignment.Center, fontSize: 32);
					Helpers.drawTextStd(TCat.Option, "Items", Global.screenW - 19, Global.halfScreenH + 15, Alignment.Center, fontSize: 20);
				}
			}
		}

		Global.sprites["menu_xdefault"].drawToHUD(0, 220, 120);

		if (mainPlayer.hasUltimateArmor()) Global.sprites["menu_xultimate"].drawToHUD(0, 220, 120);
		else if (mainPlayer.hasGoldenArmor()) Global.sprites["menu_xgolden"].drawToHUD(0, 220, 120);
		else {
			if (mainPlayer.helmetArmorNum == 1) Global.sprites["menu_xhelmet"].drawToHUD(0, 220, 120);
			if (mainPlayer.bodyArmorNum == 1) Global.sprites["menu_xbody"].drawToHUD(0, 220, 120);
			if (mainPlayer.armArmorNum == 1) Global.sprites["menu_xarm"].drawToHUD(0, 220, 120);
			if (mainPlayer.bootsArmorNum == 1) Global.sprites["menu_xboots"].drawToHUD(0, 220, 120);

			if (mainPlayer.helmetArmorNum == 2) Global.sprites["menu_xhelmet2"].drawToHUD(0, 220, 120);
			if (mainPlayer.bodyArmorNum == 2) Global.sprites["menu_xbody2"].drawToHUD(0, 220, 120);
			if (mainPlayer.armArmorNum == 2) Global.sprites["menu_xarm2"].drawToHUD(0, 220, 120);
			if (mainPlayer.bootsArmorNum == 2) Global.sprites["menu_xboots2"].drawToHUD(0, 220, 120);

			if (mainPlayer.helmetArmorNum >= 3) Global.sprites["menu_xhelmet3"].drawToHUD(0, 220, 120);
			if (mainPlayer.bodyArmorNum >= 3) Global.sprites["menu_xbody3"].drawToHUD(0, 220, 120);
			if (mainPlayer.armArmorNum >= 3) Global.sprites["menu_xarm3"].drawToHUD(0, 220, 120);
			if (mainPlayer.bootsArmorNum >= 3) Global.sprites["menu_xboots3"].drawToHUD(0, 220, 120);
		}

		Point optionPos = new Point();
		if (selectArrowPosY == 0) optionPos = optionPos1;
		if (selectArrowPosY == 1) optionPos = optionPos2;
		if (selectArrowPosY == 2) optionPos = optionPos3;
		if (selectArrowPosY == 3) optionPos = optionPos4;

		float yOff = xGame == 3 && mainPlayer.hasAllX3Armor() ? 9 : -1;
		Global.sprites["cursor"].drawToHUD(0, optionPos1.x - 8, optionPos.y + 4 + yOff);

		var grayColor = new Color(128, 128, 128);

		bool showChips = mainPlayer.hasAllX3Armor() && xGame == 3;

		// Head section
		Color headPartColor = mainPlayer.helmetArmorNum == xGame ? grayColor : Color.White;
		Color headMessageColor = mainPlayer.helmetArmorNum == xGame ? grayColor :
			(!mainPlayer.isHeadArmorPurchased(xGame) && mainPlayer.currency < MegamanX.headArmorCost ? Color.Red :
			(!mainPlayer.isHeadArmorPurchased(xGame) ? Color.Green : Color.White));
		Helpers.drawTextStd(getHeadArmorMessage(), optionPos1.x + 57, optionPos1.y, fontSize: 24, color: headMessageColor);
		Helpers.drawTextStd(" -----------", optionPos1.x + 114, optionPos1.y, fontSize: 24, color: headPartColor);
		if (xGame == 1) {
		Helpers.drawTextStd(TCat.Option, "LightArmor", optionPos1.x, optionPos1.y, fontSize: 24, color: headPartColor, selected: selectArrowPosY == 0 && !showChips);
		
			Helpers.drawTextStd("Grants Light Armor", optionPos1.x + 5, optionPos1.y + 11, fontSize: 18, color: headPartColor);
		}
		if (xGame == 2) {
			Helpers.drawTextStd(TCat.Option, "Falcon Armor (Coming Soon)", optionPos1.x, optionPos1.y, fontSize: 24, color: headPartColor, selected: selectArrowPosY == 0 && !showChips);
		
			Helpers.drawTextStd("Grants Falcon Armor.", optionPos1.x + 5, optionPos1.y + 11, fontSize: 18, color: headPartColor);
		}
		if (xGame == 3) {
			Helpers.drawTextStd(TCat.Option, "?????", optionPos1.x, optionPos1.y, fontSize: 24, color: headPartColor, selected: selectArrowPosY == 0 && !showChips);
		
			//if (mainPlayer.hasAllX3Armor()) {
			//	Helpers.drawTextStd(TCat.Option, "ENHANCEMENT CHIP", optionPos1.x + 5, optionPos1.y + 11, fontSize: 18, color: mainPlayer.hasChip(2) ? grayColor : Color.White, selected: selectArrowPosY == 0);
			//	Helpers.drawTextStd("Slowly regenerate health.", optionPos1.x + 5, optionPos1.y + 19, fontSize: 18, color: mainPlayer.hasChip(2) ? grayColor : Color.White);
			//} else {
			//	Helpers.drawTextStd("Gain a radar to detect enemies.", optionPos1.x + 5, optionPos1.y + 11, fontSize: 18, color: mainPlayer.helmetArmorNum == xGame ? grayColor : Color.White);
			//}
		}

		// Body section
		Color bodyPartColor = mainPlayer.bodyArmorNum == xGame ? grayColor : Color.White;
		Color bodyMessageColor = mainPlayer.bodyArmorNum == xGame ? grayColor :
			(!mainPlayer.isBodyArmorPurchased(xGame) && mainPlayer.currency < MegamanX.bodyArmorCost ? Color.Red :
			(!mainPlayer.isBodyArmorPurchased(xGame) ? Color.Green : Color.White));
		Helpers.drawTextStd(getBodyArmorMessage(), optionPos2.x + 57, optionPos2.y, fontSize: 24, color: bodyMessageColor);
		Helpers.drawTextStd(" -----------", optionPos2.x + 114, optionPos2.y, fontSize: 24, color: bodyPartColor);
		if (xGame == 1) {
		Helpers.drawTextStd(string.Format("Grants Giga Armor."), optionPos2.x + 5, optionPos2.y + 11, fontSize: 18, color: bodyPartColor);
		Helpers.drawTextStd(TCat.Option, "GIGA ARMOR", optionPos2.x, optionPos2.y, fontSize: 24, color: bodyPartColor, selected: selectArrowPosY == 1 && !showChips);
		}
		if (xGame == 2) {
		Helpers.drawTextStd("Grants GAEA ARMOR", optionPos2.x + 5, optionPos2.y + 11, fontSize: 18, color: bodyPartColor);
		Helpers.drawTextStd(TCat.Option, "GAEA ARMOR (coming Soon)", optionPos2.x, optionPos2.y, fontSize: 24, color: bodyPartColor, selected: selectArrowPosY == 1 && !showChips);
		}
		if (xGame == 3) {
			Helpers.drawTextStd(TCat.Option, "??????? (Out of order)", optionPos2.x, optionPos2.y, fontSize: 24, color: bodyPartColor, selected: selectArrowPosY == 1 && !showChips);
		
			//if (mainPlayer.hasAllX3Armor()) {
			//	Helpers.drawTextStd(TCat.Option, "ENHANCEMENT CHIP", optionPos2.x + 5, optionPos2.y + 11, fontSize: 18, color: mainPlayer.hasChip(1) ? grayColor : Color.White, selected: selectArrowPosY == 1);
			//	Helpers.drawTextStd("Improves barrier defense.", optionPos2.x + 5, optionPos2.y + 19, fontSize: 18, color: mainPlayer.hasChip(1) ? grayColor : Color.White);
			//} else {
			//	Helpers.drawTextStd("Gain a barrier on taking damage.", optionPos2.x + 5, optionPos2.y + 11, fontSize: 18, color: mainPlayer.bodyArmorNum == xGame ? grayColor : Color.White);
			//}
		}

		// Arm section
		Color armPartColor = mainPlayer.armArmorNum == xGame ? grayColor : Color.White;
		Color armMessageColor = mainPlayer.armArmorNum == xGame ? grayColor :
			(!mainPlayer.isArmArmorPurchased(xGame) && mainPlayer.currency < MegamanX.armArmorCost ? Color.Red :
			(!mainPlayer.isArmArmorPurchased(xGame) ? Color.Green : Color.White));
		Helpers.drawTextStd(getArmArmorMessage(), optionPos3.x + 57, optionPos3.y, fontSize: 24, color: armMessageColor);
		Helpers.drawTextStd(" -------", optionPos3.x + 114, optionPos3.y, fontSize: 24, color: armPartColor);
		if (xGame == 1) {
		Helpers.drawTextStd(TCat.Option, "MAX ARMOR", optionPos3.x, optionPos3.y, fontSize: 24, color: armPartColor, selected: selectArrowPosY == 2 && !showChips);
		Helpers.drawTextStd("Grants MAX armor.", optionPos3.x + 5, optionPos3.y + 11, fontSize: 18, color: armPartColor);
		
		}
		if (xGame == 2){
		Helpers.drawTextStd(TCat.Option, "BLADE ARMOR (Coming Soon)", optionPos3.x, optionPos3.y, fontSize: 24, color: armPartColor, selected: selectArrowPosY == 2 && !showChips);	
		Helpers.drawTextStd("Store an extra charge shot.", optionPos3.x + 5, optionPos3.y + 11, fontSize: 18, color: armPartColor);
		}
		if (xGame == 3) {
		Helpers.drawTextStd(TCat.Option, "??????? (Out of order)", optionPos3.x, optionPos3.y, fontSize: 24, color: armPartColor, selected: selectArrowPosY == 2 && !showChips);			
			//if (mainPlayer.hasAllX3Armor()) {
			//	Helpers.drawTextStd(TCat.Option, "ENHANCEMENT CHIP", optionPos3.x + 5, optionPos3.y + 11, fontSize: 18, color: mainPlayer.hasChip(3) ? grayColor : Color.White, selected: selectArrowPosY == 2);
			//	Helpers.drawTextStd("Reduce ammo usage by half.", optionPos3.x + 5, optionPos3.y + 19, fontSize: 18, color: mainPlayer.hasChip(3) ? grayColor : Color.White);
			//} else {
			//	Helpers.drawTextStd("Grants the Hyper Charge", optionPos3.x + 5, optionPos3.y + 11, fontSize: 18, color: mainPlayer.armArmorNum == xGame ? grayColor : Color.White);
			//	Helpers.drawTextStd("and Cross Shot abilities.", optionPos3.x + 5, optionPos3.y + 19, fontSize: 18, color: mainPlayer.armArmorNum == xGame ? grayColor : Color.White);
			//}
		}

		// Foot section
		Color bootsPartColor = mainPlayer.bootsArmorNum == xGame ? grayColor : Color.White;
		Color bootsMessageColor = mainPlayer.bootsArmorNum == xGame ? grayColor :
			(!mainPlayer.isBootsArmorPurchased(xGame) && mainPlayer.currency < MegamanX.bootsArmorCost ? Color.Red :
			(!mainPlayer.isBootsArmorPurchased(xGame) ? Color.Green : Color.White));
		Helpers.drawTextStd(TCat.Option, "Foot Parts", optionPos4.x, optionPos4.y, fontSize: 24, color: bootsPartColor, selected: selectArrowPosY == 3 && !showChips);
		Helpers.drawTextStd(getBootsArmorMessage(), optionPos4.x + 57, optionPos4.y, fontSize: 24, color: bootsMessageColor);
		Helpers.drawTextStd(" --------", optionPos4.x + 114, optionPos4.y, fontSize: 24, color: bootsPartColor);
		if (xGame == 1) {
			Helpers.drawTextStd(TCat.Option, "FORCE ARMOR", optionPos4.x, optionPos4.y, fontSize: 24, color: bootsPartColor, selected: selectArrowPosY == 3 && !showChips);	
			Helpers.drawTextStd("Grants Force Armor.", optionPos4.x + 5, optionPos4.y + 11, fontSize: 18, color: bootsPartColor);
		}
		if (xGame == 2) {
			Helpers.drawTextStd(TCat.Option, "SHADOW ARMOR (Coming Soon)", optionPos4.x, optionPos4.y, fontSize: 24, color: bootsPartColor, selected: selectArrowPosY == 3 && !showChips);	
			Helpers.drawTextStd("Grants Shadow Armor.", optionPos4.x + 5, optionPos4.y + 11, fontSize: 18, color: bootsPartColor);
		}
		if (xGame == 3) {
			Helpers.drawTextStd(TCat.Option, "??????? (Out of order)", optionPos4.x, optionPos4.y, fontSize: 24, color: bootsPartColor, selected: selectArrowPosY == 3 && !showChips);	
			
			//if (mainPlayer.hasAllX3Armor()) {
			//	Helpers.drawTextStd(TCat.Option, "ENHANCEMENT CHIP", optionPos4.x + 5, optionPos4.y + 11, fontSize: 18, color: mainPlayer.hasChip(0) ? grayColor : Color.White, selected: selectArrowPosY == 3);
			//	Helpers.drawTextStd("Dash twice in the air.", optionPos4.x + 5, optionPos4.y + 19, fontSize: 18, color: mainPlayer.hasChip(0) ? grayColor : Color.White);
			//} else {
			//	Helpers.drawTextStd("Gain a midair upward dash.", optionPos4.x + 5, optionPos4.y + 11, fontSize: 18, color: mainPlayer.bootsArmorNum == xGame ? grayColor : Color.White);
			//}
		}

		if (mainPlayer.hasChip(2)) Global.sprites["menu_chip"].drawToHUD(0, 220 - 4, optionPos1.y);
		if (mainPlayer.hasChip(1)) Global.sprites["menu_chip"].drawToHUD(0, 220 - 4, optionPos2.y);
		if (mainPlayer.hasChip(3)) Global.sprites["menu_chip"].drawToHUD(0, 220 - 38, optionPos3.y);
		if (mainPlayer.hasChip(0)) Global.sprites["menu_chip"].drawToHUD(0, 220 - 28, optionPos4.y);

		drawHyperArmorUpgrades(mainPlayer, 0);

		Helpers.drawTextStd(Helpers.menuControlText("Left/Right: Change Armor Set"), Global.halfScreenW, 208, Alignment.Center, fontSize: 16);
		Helpers.drawTextStd(Helpers.menuControlText("[OK]: Upgrade, [ALT]: Unupgrade, [BACK]: Back"), Global.halfScreenW, 214, Alignment.Center, fontSize: 16);
	}

	public static bool updateHyperArmorUpgrades(Player mainPlayer) {
		if (mainPlayer.character == null) return false;
		if (mainPlayer.character.charState is NovaStrikeState) return false;

		if (Global.input.isPressedMenu(Control.Special1)) {
			if (mainPlayer.canUpgradeUltimateX()) {
				if (!mainPlayer.character.boughtUltimateArmorOnce) {
					mainPlayer.currency -= Player.ultimateArmorCost;
					mainPlayer.character.boughtUltimateArmorOnce = true;
				}
				mainPlayer.setUltimateArmor(true);
				Global.playSound("ching");
				return true;
			} else if (mainPlayer.canUpgradeGoldenX()) {
				if (!mainPlayer.character.boughtGoldenArmorOnce) {
					mainPlayer.currency -= Player.goldenArmorCost;
					mainPlayer.character.boughtGoldenArmorOnce = true;
				}
				mainPlayer.setGoldenArmor(true);
				Global.playSound("ching");
				return true;
			}
		} else if (Global.input.isPressedMenu(Control.MenuAlt)) {
			if (mainPlayer.hasUltimateArmor()) {
				mainPlayer.setUltimateArmor(false);
				return true;
			} else if (mainPlayer.hasGoldenArmor()) {
				mainPlayer.setGoldenArmor(false);
				return true;
			}
		}
		return false;
	}

	public static void drawHyperArmorUpgrades(Player mainPlayer, int offY) {
		if (mainPlayer.character == null) return;
		if (mainPlayer.character.charState is NovaStrikeState) return;

		string specialText = "";
		if (mainPlayer.canUpgradeUltimateX() && mainPlayer.isX && !mainPlayer.isDisguisedAxl) {
			specialText = ("[SPC]: Ultimate Armor" +
				(mainPlayer.character.boughtUltimateArmorOnce ? "" : $" (10 {Global.nameCoins})")
			);
		} else if (mainPlayer.canUpgradeGoldenX() && mainPlayer.isX && !mainPlayer.isDisguisedAxl) {
			specialText = (
				"[SPC]: Hyper Chip" +
				(mainPlayer.character.boughtGoldenArmorOnce ? "" : $" (5 {Global.nameCoins})")
			);
		}

		if (mainPlayer.hasUltimateArmor()) {
			specialText += "\n[ALT]: Take Off Ultimate Armor";
		} else if (mainPlayer.hasGoldenArmor()) {
			specialText += "\n[ALT]: Disable Hyper Chip";
		}

		if (!string.IsNullOrEmpty(specialText)) {
			specialText = specialText.TrimStart('\n');
			float yOff = specialText.Contains('\n') ? -3 : 0;
			float yPos = Global.halfScreenH + 25;
			DrawWrappers.DrawRect(5, yPos + offY, Global.screenW - 5, yPos + 30 + offY, true, Helpers.MenuBgColor, 0, ZIndex.HUD + 200, false);
			Helpers.drawTextStd(TCat.OptionNoSplit, Helpers.controlText(specialText).ToUpperInvariant(), Global.halfScreenW, yPos + 11 + yOff + offY, Alignment.Center, fontSize: 24, selected: true);
		}

	}

	public string getHeadArmorMessage() {
		if (mainPlayer.isHeadArmorPurchased(xGame)) {
			return mainPlayer.helmetArmorNum == xGame ? " (Active)" : " (Bought)";
		}
		return $" ({MegamanX.headArmorCost} {Global.nameCoins})";
	}

	public string getBodyArmorMessage() {
		if (mainPlayer.isBodyArmorPurchased(xGame)) {
			return mainPlayer.bodyArmorNum == xGame ? " (Active)" : " (Bought)";
		}
		return $" ({MegamanX.bodyArmorCost} {Global.nameCoins})";
	}

	public string getArmArmorMessage() {
		if (mainPlayer.isArmArmorPurchased(xGame)) {
			return mainPlayer.armArmorNum == xGame ? " (Active)" : " (Bought)";
		}
		return $" ({MegamanX.armArmorCost} {Global.nameCoins}))";
	}

	public string getBootsArmorMessage() {
		if (mainPlayer.isBootsArmorPurchased(xGame)) {
			return mainPlayer.bootsArmorNum == xGame ? " (Active)" : " (Bought)";
		}
		return $" ({MegamanX.bootsArmorCost} {Global.nameCoins}))";
	}
}
