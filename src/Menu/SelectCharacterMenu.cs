using System;
using ProtoBuf;
using SFML.Graphics;

namespace MMXOnline;

[ProtoContract]
public class PlayerCharData {
	[ProtoMember(1)] public int charNum;
	[ProtoMember(2)] public int armorSet = 1;
	[ProtoMember(3)] public int alliance = -1;
	[ProtoMember(4)] public bool isRandom;

	public bool xSelected { get { return charNum == (int)CharIds.X; } }

	public int uiSelectedCharIndex;

	public PlayerCharData() {
	}

	public PlayerCharData(int charNum) {
		this.charNum = charNum;
	}
}

public enum CharIds {
	X,
	Zero,
	Vile,
	AxlWC,
	Sigma,
	PunchyZero,
	BusterZero,

	// Ruben:
	// It must follow this order because gm hard coded prefered char option
	// Jack:
	// Rubens if you change the Character IDs order one more time
	// I am gonna Kill your entire family and rape your sister
	// Gacel:
	// Really. Do not change this order unless really needed it breaks stuff.
	// Ruben:
	// Ok then you fix this mess gacel.

	// Wily Cut chars.
	XAnother,
	Zain,
	GBD,
	Dynamo,
	Dragoon,
	AxlAnother,
	Iris,

	// Old stuff.
	AxlOld,

	// Non-standard chars start here.
	WolfSigma = 100,
	ViralSigma,
	KaiserSigma,

	RagingChargeX,

	SoulBodyClone,
	
	// Non-vanilla chars start here.
	Rock = 10000,
}

public class CharSelection {
	public string name;
	public int mappedCharNum;
	public int mappedCharArmor;
	public int mappedCharMaverick;
	public string sprite;
	public int frameIndex;
	public Point offset = new Point(0, 23);

	public static int sigmaIndex => Options.main?.sigmaLoadout?.sigmaForm ?? 0;

	public static CharSelection[] selections => [
		new CharSelection("X", (int)CharIds.X, sprite: "smenu_x"),
		new CharSelection("X (Another)", (int)CharIds.XAnother, sprite: "smenu_xanother"),  
		new CharSelection("Zero (Mid)", (int)CharIds.Zero, sprite: "smenu_zero"),
		new CharSelection("Zero (Early)", (int)CharIds.PunchyZero, sprite: "smenu_zero_2"),
	//	new CharSelection("Axl", (int)CharIds.AxlX8, sprite: "smenu_axl2"),
		new CharSelection("Iris", (int)CharIds.Iris, sprite: "smenu_iris"),
		new CharSelection("Axl", (int)CharIds.AxlWC, sprite: "smenu_axl"),
		new CharSelection("GBD", (int)CharIds.GBD, sprite: "smenu_gbd"),
		new CharSelection("Sigma", (int)CharIds.Sigma, sprite: "smenu_sigma"),
		new CharSelection("Vava", (int)CharIds.Vile, sprite: "smenu_vile"),
		new CharSelection("Zain", (int)CharIds.Zain, sprite: "smenu_zain"),
		new CharSelection("Magma Dragoon", (int)CharIds.Dragoon, sprite: "smenu_dragoon"),
		new CharSelection("Dynamo", (int)CharIds.Dynamo, sprite: "smenu_dynamo"),
		//new CharSelection("High Max", (int)CharIds.Highmax, sprite: "smenu_highmax"),
	];

	public static CharSelection[] selections1v1 => [
		new CharSelection("X", (int)CharIds.X, sprite: "smenu_x"),
		new CharSelection("X (Another)", (int)CharIds.XAnother, sprite: "smenu_xanother"),  
		new CharSelection("Zero (Mid)", (int)CharIds.BusterZero, sprite: "smenu_zero_2"),
		new CharSelection("Zero (Early)", (int)CharIds.PunchyZero, sprite: "smenu_zero"),
		new CharSelection("Axl", (int)CharIds.AxlWC, sprite: "smenu_axl"),
		new CharSelection("Axl (Another)", (int)CharIds.AxlAnother, sprite: "smenu_axl"),
		new CharSelection("GBD", (int)CharIds.GBD, sprite: "smenu_gbd"),
		new CharSelection("Sigma", (int)CharIds.Sigma, sprite: "smenu_sigma"),
		new CharSelection("Vava", (int)CharIds.Vile, sprite: "smenu_vile"),
		new CharSelection("Zain", (int)CharIds.Zain, sprite: "smenu_zain"),
		new CharSelection("Magma Dragoon", (int)CharIds.Dragoon, sprite: "smenu_dragoon"),
		new CharSelection("Dynamo", (int)CharIds.Dynamo, sprite: "smenu_dynamo"),
			
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
	];

	public CharSelection(
		string name, int mappedCharNum, int mappedCharArmor = 0,
		int mappedCharMaverick = 0, string sprite = "", int frameIndex = 0
	) {
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

	public CharSelection[] charSelections;
	public static PlayerCharData playerData = new (Options.main.preferredCharacter);

	public SelectCharacterMenu(PlayerCharData playerData) {
		SelectCharacterMenu.playerData = playerData;
	}

	public SelectCharacterMenu(int charNum) {
		SelectCharacterMenu.playerData = new PlayerCharData(charNum);
	}

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
			playerData.uiSelectedCharIndex = charSelections.FindIndex(
				c => c.mappedCharNum == playerData.charNum && c.mappedCharArmor == playerData.armorSet
			);
		} else {
			playerData.uiSelectedCharIndex = charSelections.FindIndex(
				c => c.mappedCharNum == playerData.charNum
			);
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

		Helpers.menuLeftRightInc(
			ref playerData.uiSelectedCharIndex, 0, charSelections.Length - 1, true, playSound: true
		);
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
		//Global.sprites["playerbox"].drawToHUD(0, charPosX1, charPosY1+2);
		string sprite = charSelection.sprite;
		int frameIndex = charSelection.frameIndex;
		float yOff = sprite.EndsWith("_idle") ? (Global.sprites[sprite].frames[0].rect.h() * 0.5f) : 0;
		Global.sprites[sprite].drawToHUD(
			frameIndex,
			charPosX1 + charSelection.offset.x,
			charPosY1 + yOff + charSelection.offset.y
		);

		// Draw text

		if (Global.frameCount % 60 < 30) {
			Fonts.drawText(
				FontType.Orange, "<", Global.halfScreenW - 60, Global.halfScreenH + 28,
				Alignment.Center
			);
			Fonts.drawText(
				FontType.Orange, ">", Global.halfScreenW + 60, Global.halfScreenH + 28,
				Alignment.Center
			);
		}
		Fonts.drawText(
			FontType.Orange, charSelection.name, Global.halfScreenW, Global.halfScreenH + 28,
			alignment: Alignment.Center
		);

		string[] description = playerData.charNum switch {
			(int)CharIds.X => new string[]{
				"X, Standard Zoner that can use Armors and mix them."
			},
			(int)CharIds.XAnother => new string[]{
				"Lacks ability to mix armors but has a bigger moveset."
			},
			(int)CharIds.PunchyZero => new string[] {
				"High Damage Melee fighter but weak midair."
			},
			/*(int)CharIds.ZeroX6 => new string[] {
				"Long range figther with a powerfull buster."
			},*/
			(int)CharIds.Zero => new string[] {
				"Combo & Rushdown fighter\nwith a variety of combo routes and hypermodes."
			},
			(int)CharIds.Vile => new string[] {
				"Multiarchetype Fighter that can adapt\nto any strategy but lacks HP."
			},
			(int)CharIds.AxlWC => new string[] {
				"Gunslinger Rushdown Zoner hybrid\nwith access to i-frames."
			},
			(int)CharIds.AxlAnother => new string[] {
				"???"
			},
			(int)CharIds.Sigma => new string[] {
				"Commander type unit that can\nuse mavericks to assist his kit."
			},
			(int)CharIds.Zain => new string[] {
				"High Damage Fighter with Defensive skills\nbut can't combo without resoruces."
			},
			(int)CharIds.GBD => new string[] {
				"Hit and run type Trickster\nthat can summon ride chasers."
			},
			(int)CharIds.Dynamo => new string[] {
				"Multi strategy fighter with great mobility options."
			},
			(int)CharIds.Dragoon => new string[] {
				"Shoto styled fighter with burning damage passive."
			},
			(int)CharIds.Iris => new string[] {
				"VAGABUNDA ESTRUPADA ARROMBADA"
			},
			_ => new string[] { "ERROR" }
		};
		if (description.Length > 0) {
			DrawWrappers.DrawRect(
				25, startY + 102, Global.screenW - 25, startY + 125,
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
				Global.screenW * 0.5f, 175, Alignment.Center
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
