namespace MMXOnline;

public class UpgradeZeroMenu : IMainMenu {
	public int selectArrowPosY;
	public IMainMenu prevMenu;

	public int optionPosX = 20;
	public int[] optionPosY;

	public UpgradeZeroMenu(IMainMenu prevMenu) {
		this.prevMenu = prevMenu;
		optionPosY = new int[] {
			40,
			50,
			80,
			90
		};
	}

	public void update() {
		var mainPlayer = Global.level.mainPlayer;

		if (!Global.level.server.disableHtSt && Global.input.isPressedMenu(Control.MenuLeft)) {
			UpgradeMenu.onUpgradeMenu = true;
			Menu.change(new UpgradeMenu(prevMenu));
			return;
		}

		Helpers.menuUpDown(ref selectArrowPosY, 0, 0);

		if (Global.input.isPressedMenu(Control.MenuConfirm)) {
			if (selectArrowPosY == 0) {
				if (!mainPlayer.blackZarzo && mainPlayer.currency >= ZeroMID.BlackZeroCost) {
					mainPlayer.blackZarzo = true;
					if (mainPlayer.character is ZeroMID zarzo) {
						zarzo.isBlack = true;
					}
					Global.playSound("ching");
					mainPlayer.currency -= ZeroMID.BlackZeroCost;
				}
			} 
		} else if (Global.input.isPressedMenu(Control.MenuBack)) {
			Menu.change(prevMenu);
		}
	}

	public void render() {
		var mainPlayer = Global.level.mainPlayer;
		ZeroMID? CZarzo = mainPlayer.character as ZeroMID;
		var gameMode = Global.level.gameMode;
		DrawWrappers.DrawTextureHUD(Global.textures["pausemenu"], 0, 0);
		if (!mainPlayer.blackZarzo) {
			DrawWrappers.DrawTextureHUD(Global.textures["NewZeroMenu"], Global.halfScreenW + 60, Global.halfScreenH - 103);
		} else {
			DrawWrappers.DrawTextureHUD(Global.textures["NewZeroMenuBlack"], Global.halfScreenW + 60, Global.halfScreenH - 103);
		}
		if (!Global.level.server.disableHtSt && Global.flFrameCount % 60 < 30) {
			Fonts.drawText(FontType.DarkPurple, "<", 18, Global.halfScreenH + 10, Alignment.Center);
			Fonts.drawText(FontType.DarkPurple, "Items", 18, Global.halfScreenH + 20, Alignment.Center);
		}

		//if (mainPlayer.speedDevil) Global.sprites["menu_vilespeeddevil"].drawToHUD(0, 310, 110);
		//if (mainPlayer.frozenCastle) Global.sprites["menu_vilefrozencastle"].drawToHUD(0, 310, 110);

		Global.sprites["cursor"].drawToHUD(0, optionPosX - 6, optionPosY[0] + selectArrowPosY * 40 + 3);

		Fonts.drawText(FontType.Yellow, "Zero Armor", Global.screenW * 0.5f, 10, Alignment.Center);

		Fonts.drawText(
			FontType.Golden,
			Global.nameCoins + ": " + mainPlayer.currency,
			Global.screenW * 0.5f, 20, Alignment.Center
		);

		Fonts.drawText(
			mainPlayer.currency < ZeroMID.BlackZeroCost && CZarzo?.isBlack == false ? FontType.Grey : FontType.Blue,
			"BlackZero", optionPosX, optionPosY[0],
			selected: selectArrowPosY == 0
		);
		Fonts.drawText(
			mainPlayer.currency < ZeroMID.BlackZeroCost && CZarzo?.isBlack == false ? FontType.Grey :
			CZarzo?.isBlack == true ? FontType.Orange : FontType.Purple ,
			CZarzo?.isBlack == false ? $"({ZeroMID.BlackZeroCost} {Global.nameCoins})" : "(Bought)",
			optionPosX + 86, optionPosY[0]
		);
		Fonts.drawText(
			mainPlayer.currency < ZeroMID.BlackZeroCost && CZarzo?.isBlack == false ? FontType.Grey : FontType.DarkPurple,
			"Rushed Upgrade by Dr Light," +
			"\nthis armor Grants an upgraded Z buster and increased flinch power",
			optionPosX, optionPosY[1]
		);

		

		Fonts.drawTextEX(FontType.Grey, "[MLEFT]/[MRIGHT]: Change Armor", 40, 188);
		Fonts.drawTextEX(FontType.Grey,
			"[OK]: Upgrade, [ALT]: Unupgrade, [BACK]: Back", 40, 198
		);
	}

}
