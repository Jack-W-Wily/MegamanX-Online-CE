﻿using ProtoBuf;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SFML.Window.Keyboard;

namespace MMXOnline;

[ProtoContract]
public class PlayerCharData {
	[ProtoMember(1)] public int charNum;
	[ProtoMember(2)] public int armorSet = 1;
	[ProtoMember(3)] public int alliance = -1;
	[ProtoMember(4)] public bool isRandom;

	public bool xSelected { get { return charNum == 0; } }

	public int uiSelectedCharIndex;

	public PlayerCharData() {
	}

	public PlayerCharData(int charNum) {
		this.charNum = charNum;
	}
}

public class CharSelection {
	public string name;
	public int mappedCharNum;
	public int mappedCharArmor;
	public int mappedCharMaverick;
	public string sprite;
	public int frameIndex;

	public static int sigmaIndex {
		get {
			return Options.main?.sigmaLoadout?.sigmaForm ?? 0;
		}
	}

	public static List<CharSelection> selections {
		get {
			return new List<CharSelection>()
			{
					new CharSelection("X", 0, 1, 0, "menu_megaman", 0),
					new CharSelection("Zero", 1, 1, 0, "menu_zero", 0),
					new CharSelection("Vile", 2, 1, 0, "menu_vile", 0),
					new CharSelection("Axl", 3, 1, 0, "menu_axl", 0),
					new CharSelection("Sigma", 4, 1, 0, "menu_sigma", sigmaIndex),
					new CharSelection("dynamo", 5, 1, 0, "dynamo_idle", 0),
					new CharSelection("gbd", 6, 1, 0, "tgbd_idle", 0),
				};
		}
	}

	public static List<CharSelection> selections1v1 {
		get {
			return new List<CharSelection>()
			{
					new CharSelection("X(X1)", 0, 1, 0, "menu_megaman", 1),
					new CharSelection("X(X2)", 0, 2, 0, "menu_megaman", 2),
					new CharSelection("X(X3)", 0, 3, 0, "menu_megaman", 3),
					new CharSelection("Zero", 1, 1, 0, "menu_zero", 0),
					new CharSelection("Vile", 2, 1, 0, "menu_vile", 0),
					new CharSelection("Axl", 3, 1, 0, "menu_axl", 0),
					new CharSelection("Sigma", 4, 1, 0, "menu_sigma", sigmaIndex),
					new CharSelection("C.Penguin", 210, 1, 0, "chillp_idle", 0),
					new CharSelection("S.Mandrill", 212, 1, 1, "sparkm_idle", 0),
					new CharSelection("A.Armadillo", 213, 1, 2, "armoreda_idle", 0),
					new CharSelection("L.Octopus", 214, 1, 3, "launcho_idle", 0),
					new CharSelection("B.Kuwanger", 215, 1, 4, "boomerk_idle", 0),
					new CharSelection("S.Chameleon", 216, 1, 5, "stingc_idle", 0),
					new CharSelection("S.Eagle", 217, 1, 6, "storme_idle", 0),
					new CharSelection("F.Mammoth", 218, 1, 7, "flamem_idle", 0),
					new CharSelection("Velguarder", 219, 1, 8, "velg_idle", 0),
					new CharSelection("W.Sponge", 220, 1, 9, "wsponge_idle", 0),
					new CharSelection("W.Gator", 221, 1, 10, "wheelg_idle", 0),
					new CharSelection("B.Crab", 222, 1, 11, "bcrab_idle", 0),
					new CharSelection("F.Stag", 223, 1, 12, "fstag_idle", 0),
					new CharSelection("M.Moth", 224, 1, 13, "morphm_idle", 0),
					new CharSelection("M.Centipede", 225, 1, 14, "magnac_idle", 0),
					new CharSelection("C.Snail", 226, 1, 15, "csnail_idle", 0),
					new CharSelection("O.Ostrich", 227, 1, 16, "overdriveo_idle", 0),
					new CharSelection("Fake Zero", 228, 1, 17, "fakezero_idle", 0),
					new CharSelection("B.Buffalo", 229, 1, 18, "bbuffalo_idle", 0),
					new CharSelection("T.Seahorse", 230, 1, 19, "tseahorse_idle", 0),
					new CharSelection("T.Rhino", 231, 1, 20, "tunnelr_idle", 0),
					new CharSelection("V.Catfish", 232, 1, 21, "voltc_idle", 0),
					new CharSelection("C.Crawfish", 233, 1, 22, "crushc_idle", 0),
					new CharSelection("N.Tiger", 234, 1, 23, "neont_idle", 0),
					new CharSelection("G.Beetle", 235, 1, 24, "gbeetle_idle", 0),
					new CharSelection("B.Hornet", 236, 1, 25, "bhornet_idle", 0),
					new CharSelection("Dr.Doppler", 237, 1, 26, "drdoppler_idle", 0),
				};
		}
	}

	public CharSelection(string name, int mappedCharNum, int mappedCharArmor, int mappedCharMaverick, string sprite, int frameIndex) {
		this.name = name;
		this.mappedCharNum = mappedCharNum;
		this.mappedCharArmor = mappedCharArmor;
		this.mappedCharMaverick = mappedCharMaverick;
		this.sprite = sprite;
		this.frameIndex = frameIndex;
	}
}

public class SelectCharacterMenu : IMainMenu {
	public IMainMenu prevMenu;
	public int selectArrowPosY;
	public const int startX = 30;
	public int startY = 46;
	public const int lineH = 10;
	public const uint fontSize = 24;

	public bool is1v1;
	public bool isOffline;
	public bool isInGame;
	public bool isInGameEndSelect;
	public bool isTeamMode;
	public bool isHost;

	public Action completeAction;

	private static PlayerCharData _playerData;
	public static PlayerCharData playerData {
		get {
			if (_playerData == null) {
				_playerData = new PlayerCharData(Options.main.preferredCharacter);
			}
			return _playerData;
		}
		set {
			_playerData = value;
		}
	}

	public SelectCharacterMenu(PlayerCharData playerData) {
		SelectCharacterMenu.playerData = playerData;
	}

	public SelectCharacterMenu(int charNum) {
		SelectCharacterMenu.playerData = new PlayerCharData(charNum);
	}

	public List<CharSelection> charSelections;

	public SelectCharacterMenu(
		IMainMenu prevMenu, bool is1v1, bool isOffline, bool isInGame,
		bool isInGameEndSelect, bool isTeamMode, bool isHost, Action completeAction
	) {
		this.prevMenu = prevMenu;
		this.is1v1 = is1v1;
		this.isOffline = isOffline;
		this.isInGame = isInGame;
		this.isInGameEndSelect = isInGameEndSelect;
		this.completeAction = completeAction;
		this.isTeamMode = isTeamMode;
		this.isHost = isHost;

		charSelections = is1v1 ? CharSelection.selections1v1 : CharSelection.selections;
		playerData.charNum = isInGame ? Global.level.mainPlayer.newCharNum : Options.main.preferredCharacter;

		if (is1v1) {
			playerData.uiSelectedCharIndex = charSelections.FindIndex(c => c.mappedCharNum == playerData.charNum && c.mappedCharArmor == playerData.armorSet);
		} else {
			playerData.uiSelectedCharIndex = charSelections.FindIndex(c => c.mappedCharNum == playerData.charNum);
		}
	}

	public Player mainPlayer { get { return Global.level.mainPlayer; } }

	public void update() {
		if (Global.input.isPressedMenu(Control.MenuConfirm) || (Global.quickStartOnline && !isInGame)) {
			if (!isInGame && Global.quickStartOnline) {
				playerData.charNum = Global.quickStartOnlineClientCharNum;
			}
			if (isInGame && !isInGameEndSelect) {
				if (!Options.main.killOnCharChange && !Global.level.mainPlayer.isDead) {
					Global.level.gameMode.setHUDErrorMessage(mainPlayer, "Change will apply on next death", playSound: false);
					mainPlayer.delayedNewCharNum = playerData.charNum;
				} else if (mainPlayer.newCharNum != playerData.charNum) {
					mainPlayer.newCharNum = playerData.charNum;
					Global.serverClient?.rpc(RPC.switchCharacter, (byte)mainPlayer.id, (byte)playerData.charNum);
					mainPlayer.forceKill();
				}
			}

			completeAction.Invoke();
			return;
		}

		Helpers.menuLeftRightInc(ref playerData.uiSelectedCharIndex, 0, charSelections.Count - 1, true, playSound: true);
		try {
			playerData.charNum = charSelections[playerData.uiSelectedCharIndex].mappedCharNum;
			playerData.armorSet = charSelections[playerData.uiSelectedCharIndex].mappedCharArmor;
		} catch (IndexOutOfRangeException) {
			playerData.uiSelectedCharIndex = 0;
			playerData.charNum = charSelections[0].mappedCharNum;
			playerData.armorSet = charSelections[0].mappedCharArmor;
		}

		if (!isInGameEndSelect) {
			if (!isInGame) {
				if (Global.input.isPressedMenu(Control.MenuBack)) {
					Global.serverClient = null;
					Menu.change(prevMenu);
				}
			} else {
				if (Global.input.isPressedMenu(Control.MenuBack)) {
					Menu.change(prevMenu);
				}
			}
		} else {
			if (Global.input.isPressedMenu(Control.MenuBack)) {
				if (Global.isHost) {
					Menu.change(prevMenu);
				}
			} else if (Global.input.isPressedMenu(Control.MenuPause)) {
				if (!Global.isHost) {
					Menu.change(new ConfirmLeaveMenu(this, "Are you sure you want to leave?", () => {
						Global._quickStart = false;
						Global.leaveMatchSignal = new LeaveMatchSignal(LeaveMatchScenario.LeftManually, null, null);
					}));
				}
			}
		}
	}

	public void render() {
		if (!charSelections.InRange(playerData.uiSelectedCharIndex)) {
			playerData.uiSelectedCharIndex = 0;
		}
		CharSelection charSelection = charSelections[playerData.uiSelectedCharIndex];

		if (!isInGame) {
			DrawWrappers.DrawTextureHUD(Global.textures["severbrowser"], 0, 0);
		} else {
			DrawWrappers.DrawTextureHUD(Global.textures["pausemenu"], 0, 0);
		}

		// DrawWrappers.DrawTextureHUD(
		//	Global.textures["cursor"], startX - 10, menuOptions[(int)selectArrowPosY].pos.y - 1
		//);
		if (!isInGame) {
			Fonts.drawText(
				FontType.Yellow, "Select Character".ToUpper(),
				Global.halfScreenW, 22, alignment: Alignment.Center
			);
		} else {
			if (Global.level.gameMode.isOver) {
				DrawWrappers.DrawRect(
					Global.halfScreenW - 90, 18, Global.halfScreenW + 90, 33,
					true, new Color(0, 0, 0, 100), 1, ZIndex.HUD, false, outlineColor: Color.White
				);
				Fonts.drawText(
					FontType.Yellow, "Select Character For Next Match".ToUpper(),
					Global.halfScreenW, 22, alignment: Alignment.Center
				);
			} else {
				DrawWrappers.DrawRect(
					Global.halfScreenW - 67, 18, Global.halfScreenW + 67, 33,
					true, new Color(0, 0, 0, 100), 1, ZIndex.HUD, false, outlineColor: Color.White
				);
				Fonts.drawText(
					FontType.Yellow, "Select Character".ToUpper(),
					Global.halfScreenW, 22, alignment: Alignment.Center
				);
			}
		}

		// Draw character + box

		var charPosX1 = Global.halfScreenW;
		var charPosY1 = 85;
		Global.sprites["playerbox"].drawToHUD(0, charPosX1, charPosY1);
		string sprite = charSelection.sprite;
		int frameIndex = charSelection.frameIndex;
		float yOff = sprite.EndsWith("_idle") ? (Global.sprites[sprite].frames[0].rect.h() * 0.5f) : 0;
		Global.sprites[sprite].drawToHUD(frameIndex, charPosX1, charPosY1 + yOff);

		// Draw text

		if (Global.frameCount % 60 < 30) {
			Fonts.drawText(
				FontType.Orange, "<", Global.halfScreenW - 60, Global.halfScreenH + 22,
				Alignment.Center
			);
			Fonts.drawText(
				FontType.Orange, ">", Global.halfScreenW + 60, Global.halfScreenH + 22,
				Alignment.Center
			);
		}
		Fonts.drawText(
			FontType.Orange, charSelection.name, Global.halfScreenW, Global.halfScreenH + 22,
			alignment: Alignment.Center
		);

		string[] description = { };
		//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
		
		if (playerData.charNum == 0) {
			description = new string[] {"All-around ranged shooter.", "Can equip a variety of weapons and armor." };
		} else if (playerData.charNum == 1) {
			description = new string[] { "Powerful melee warrior", "with high damage combos." };
		} else if (playerData.charNum == 2) {
			description = new string[] { "Unpredictable threat that can self-revive", "and call down Ride Armors." };
		} else if (playerData.charNum == 3) {
			description = new string[] { 
				"Precise and deadly ranged assassin", "with aiming and rapid fire capabilities."
			};
		} else if (playerData.charNum == 4) {
			description = new string[] {
				"A fearsome military commander that can", "summon Mavericks on the battlefield."
			};
		} else if (playerData.charNum == 5) {
			description = new string[] {
				"Skilled Bounty hunter", "Gets rewards for hunting down high scorers."
			};
		}
		
		if (description.Length > 0) {
			DrawWrappers.DrawRect(
				25, startY + 98, Global.screenW - 25, startY + 127,
				true, new Color(0, 0, 0, 100), 1, ZIndex.HUD, false, outlineColor: Color.White
			);
			for (int i = 0; i < description.Length; i++) {
				Fonts.drawText(
					FontType.Green, description[i],
					Global.halfScreenW, startY + 94 + (10 * (i + 1)), alignment: Alignment.Center
				);
			}
		}
		if (!isInGame) {
			Fonts.drawTextEX(
				FontType.Grey, "[OK]: Continue, [BACK]: Back\n[MLEFT]/[MRIGHT]: Change character",
				Global.screenW * 0.5f, 178, Alignment.Center
			);
		} else {
			if (!Global.isHost) {
				Fonts.drawTextEX(
					FontType.Grey, "[ESC]: Quit\n[MLEFT]/[MRIGHT]: Change character",
					Global.screenW * 0.5f, 190, Alignment.Center
				);
			} else {
				Fonts.drawTextEX(
					FontType.Grey, "[OK]: Continue, [BACK]: Back\n[MLEFT]/[MRIGHT]: Change character",
					Global.screenW * 0.5f, 190, Alignment.Center
				);
			}
		}
	}
}
