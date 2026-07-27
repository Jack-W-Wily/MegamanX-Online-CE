using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;

namespace MMXOnline;

public class HighMaxMovelist : IMainMenu {
	public WeaponCursor[] cursors;
	public int selCursorIndex;
	public bool inGame;
	public string error = "";


	public IMainMenu prevMenu;

	public HighMaxMovelist(IMainMenu prevMenu, bool inGame) {
		this.prevMenu = prevMenu;
		this.inGame = inGame;

		cursors = new WeaponCursor[] {
			new WeaponCursor(0),
			new WeaponCursor(0),
			new WeaponCursor(0),
			new WeaponCursor(0),
			new WeaponCursor(0),
			new WeaponCursor(0),
			new WeaponCursor(0)
		};
	}

	public int maxCatCount = 3;
	public void update() {
		if (!string.IsNullOrEmpty(error)) {
			if (Global.input.isPressedMenu(Control.MenuConfirm)) {
				error = "";
			}
			return;
		}

		

		if (selCursorIndex == 0) {
			maxCatCount = 4;
		} 
		if (selCursorIndex == 1) {
			maxCatCount = 5;
		}
		if (selCursorIndex == 2) {
			maxCatCount = 12;
		}

		

		Helpers.menuLeftRightInc(ref cursors[selCursorIndex].index, 0, maxCatCount - 1, wrap: true, playSound: true);
		Helpers.menuUpDown(ref selCursorIndex, 0, cursors.Length - 1);

		bool backPressed = Global.input.isPressedMenu(Control.MenuBack);
		bool selectPressed = Global.input.isPressedMenu(Control.MenuConfirm) || (backPressed && !inGame);
		if (backPressed) {
			Menu.change(prevMenu);
		}
	}

	public void render() {
		if (!inGame) {
			DrawWrappers.DrawTextureHUD(Global.textures["loadoutbackground"], 0, 0);
		} else {
			DrawWrappers.DrawTextureHUD(Global.textures["pausemenuload"], 0, 0);
		}

		Fonts.drawText(FontType.Yellow, "Move List", Global.screenW * 0.5f, 20, Alignment.Center);
		var outlineColor = inGame ? Color.White : Helpers.LoadoutBorderColor;
		float botOffY = inGame ? 0 : -2;

		int startY = 40;
		int startX = 30;
		int wepH = 15;

		float wepPosX = 195;
		float wepTextX = 187;

		Global.sprites["cursor"].drawToHUD(0, startX, startY + (selCursorIndex * wepH) - 2);
		Color color;
		float alpha;
		
		color = Color.White;
		alpha = 1f;

		float hyperModeYPos = startY - 6 + (wepH * 0);
		float hyperModeYPos2 = startY - 6 + (wepH * 1);
		float hyperModeYPos3 = startY - 6 + (wepH * 2);
		float hyperModeYPos4 = startY - 6 + (wepH * 3);
		float hyperModeYPos5 = startY - 6 + (wepH * 4);
		float hyperModeYPos6 = startY - 6 + (wepH * 5);


		int wsy = 167;
		DrawWrappers.DrawRect(
			22, wsy-12, Global.screenW - 22, wsy + 28, true, new Color(0, 0, 0, 100), 1,
			ZIndex.HUD, false, outlineColor: outlineColor
		);


		#region Normals
		Fonts.drawText(
			FontType.Blue, "Normals:", 40, hyperModeYPos,
			selected: selCursorIndex == 0
		);

		if (cursors[0].index == 0) {
			Fonts.drawText(
				FontType.Grey, "X Buster", wepTextX, hyperModeYPos,
				selected: selCursorIndex == 0
			);
			if (selCursorIndex == 0) {
				Fonts.drawText(FontType.Green, "A (neutral).",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Chargeable, Press R1/L1.",
				 Global.halfScreenW, wsy + 10, Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "To Swap Selected Main Weapon.",
				 Global.halfScreenW, wsy + 18, Alignment.Center);
			} 
		} else if (cursors[0].index == 1) {
			Fonts.drawText(
				FontType.Red, "Punch", wepTextX, hyperModeYPos,
				selected: selCursorIndex == 0
			);
			if (selCursorIndex == 0) {
				
				Fonts.drawText(FontType.Green, "B (neutral).",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Can be Followed Up with B or A.",
				 Global.halfScreenW, wsy+12,Alignment.Center);
			
			} 
		} else if (cursors[0].index == 2) {
			Fonts.drawText(
				FontType.DarkPurple, "Double Kick", wepTextX, hyperModeYPos,
				selected: selCursorIndex == 0
			);
			if (selCursorIndex == 0) {
				
				Fonts.drawText(FontType.Green, "Up + B.",
				  Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Can be followed Up with A.",
				  Global.halfScreenW, wsy+12,Alignment.Center);
			} 
		} else if (cursors[0].index == 3) {
			Fonts.drawText(
				FontType.DarkPurple, "Grab", wepTextX, hyperModeYPos,
				selected: selCursorIndex == 0
			);
			if (selCursorIndex == 0) {

				Fonts.drawText(FontType.Green, "Hold L1 Press A.",
				  Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Can be followed Up with any Direction for Finishers.",
				  Global.halfScreenW, wsy+12,Alignment.Center);
			}
		} 
		
		#endregion

		
		#region Special Moves
		Fonts.drawText(
			FontType.Blue, "Special Moves:", 40, hyperModeYPos2,
			selected: selCursorIndex == 1
		);

		if (cursors[1].index == 0) {
			Fonts.drawText(
				FontType.Grey, "Follow-Up Shot", wepTextX, hyperModeYPos2,
				selected: selCursorIndex == 1
			);
			if (selCursorIndex == 1) {
				Fonts.drawText(FontType.Green, "A (Follow Up Only).",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Can be used after Punch or Double Kick.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			}
		} else if (cursors[1].index == 1) {
			Fonts.drawText(
				FontType.Red, "Warp Dodge", wepTextX, hyperModeYPos2,
				selected: selCursorIndex == 1
			);
			if (selCursorIndex == 1) {
				Fonts.drawText(FontType.Green, "Hold L2 Press Dash.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Grants Iframes Start to finish.",
				 Global.halfScreenW, wsy+12,Alignment.Center);
			} 
		}
		else if (cursors[1].index == 2) {
			Fonts.drawText(
				FontType.DarkPurple, "U.P Grab", wepTextX, hyperModeYPos2,
				selected: selCursorIndex == 1
			);
			if (selCursorIndex == 1) {				
				Fonts.drawText(FontType.Green, "Press R2 During Dashes",
				  Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Lifesteals, Costs 50% SuperBar.",
				  Global.halfScreenW, wsy+12,Alignment.Center);
			}
		}
		else if (cursors[1].index == 3) {
			Fonts.drawText(
				FontType.DarkPurple, "U.P Parry", wepTextX, hyperModeYPos2,
				selected: selCursorIndex == 1
			);
			if (selCursorIndex == 1) {
				Fonts.drawText(FontType.Green, "Down + R2.",
				  Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Costs 50% Super Bar, Refills Ammo on activation.",
				  Global.halfScreenW, wsy+10,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Can be followed Up with R2 To Throw Absorbed Projectile.",
				  Global.halfScreenW, wsy+18,Alignment.Center);
			} 
		} else if (cursors[1].index == 4) {
			Fonts.drawText(
				FontType.DarkPurple, "U.P Fist", wepTextX, hyperModeYPos2,
				selected: selCursorIndex == 1
			);
			if (selCursorIndex == 1) {
				Fonts.drawText(FontType.Green, "R2 (Neutral).",
				  Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Has Iframes start to finish,",
				  Global.halfScreenW, wsy+10,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Quick Reversal tool Costs 50% Bar.",
				  Global.halfScreenW, wsy+18,Alignment.Center);
			}
		}
		#endregion
		
		#region Armor Moves
		Fonts.drawText(
			FontType.Blue, "Armor Moves:", 40, hyperModeYPos3,
			selected: selCursorIndex == 2
		);
		
		if (cursors[2].index == 0) {
			Fonts.drawText(
				FontType.Grey, "HeadButt EX (Light Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "Press Jump While Crouched.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Can destroy Cracked Walls.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 1) {
			Fonts.drawText(
				FontType.DarkPurple, "Toe Attacker (Light Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "Press Jump On Walls or Enemies.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Additional Jump, Can Destroy Cracked Walls.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 2) {
			Fonts.drawText(
				FontType.DarkPurple, "Hadouken (Light Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "D, F , A. ",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Powerful Projectile.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else  if (cursors[2].index == 3) {
			Fonts.drawText(
				FontType.DarkPurple, "Scanner (Giga Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "CMD (Neutral). (Giga Armor)",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Scans Enemy Name / HP Values.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else  if (cursors[2].index == 4) {
			Fonts.drawText(
				FontType.DarkPurple, "Shoryuken (Giga Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "F, D, F, A.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Powerful Uppercut Attack.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 5) {
			Fonts.drawText(
				FontType.DarkPurple, "Giga Crush (Giga Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "D, D, CMD.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Devastating Explosive Wave.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else  if (cursors[2].index == 6) {
			Fonts.drawText(
				FontType.DarkPurple, "Up Dash EX (Max Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "U + Dash .",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Dashes Upwards Damaging Bellow Foes.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			}
		} else  if (cursors[2].index == 7) {
			Fonts.drawText(
				FontType.DarkPurple, "HyperCharge EX (Max Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "D,F, A .",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Instantly Fires Maximum Charge.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 8) {
			Fonts.drawText(
				FontType.DarkPurple, "NovaStrike (Force Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "D,D, CMD.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Requires 50% Super Bar.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 9) {
			Fonts.drawText(
				FontType.DarkPurple, "Shin NovaStrike (Ultimate Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "Direction + CMD Button .",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Uses Nova Energy.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 10) {
			Fonts.drawText(
				FontType.DarkPurple, "Plasma Shot (Force Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "D, F, A.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Fires a PlasmaShot that leaves a Plasma Ball.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		} else if (cursors[2].index == 11) {
			Fonts.drawText(
				FontType.DarkPurple, "Hover (Force Armor)", wepTextX, hyperModeYPos3,
				selected: selCursorIndex == 2
			);
			if (selCursorIndex == 2) {
				Fonts.drawText(FontType.Green, "Jump (midair).",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Allows for brief midair control.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		}

			
		#endregion



		#region Overdrive
		Fonts.drawText(
			FontType.Blue, "Overdrive:", 40, hyperModeYPos4,
			selected: selCursorIndex == 3
		);
		
		if (cursors[3].index == 0) {
			Fonts.drawText(
				FontType.Grey, "Unlimited Potential", wepTextX, hyperModeYPos4,
				selected: selCursorIndex == 3
			);
			if (selCursorIndex == 3) {
				Fonts.drawText(FontType.Green, "Unlocks X's Hidden Potential.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Makes all X special moves have no Super Cost.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		}
		if (cursors[3].index == 1) {
			Fonts.drawText(
				FontType.Grey, "Golden X (Max Armor)", wepTextX, hyperModeYPos4,
				selected: selCursorIndex == 3
			);
			if (selCursorIndex == 3) {
				Fonts.drawText(FontType.Green, "X Ressurects as Golden X.",
				 Global.halfScreenW, wsy-6,Alignment.Center);
				Fonts.drawText(FontType.DarkPurple, "Requires 5 Metals, must die with Overdrive Active.",
				 Global.halfScreenW, wsy + 12, Alignment.Center);
			} 
		}
		#endregion




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
}



