using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SFML.Graphics;

namespace MMXOnline;



// Adding a Weapon
public class Vava1Melee : Weapon {
	public static Vava1Melee netWeapon = new();

	public Vava1Melee() : base() {
		fireRate = 45;// frames
		index = (int)WeaponIds.KRMelee;// Make sure to add to "WeaponIds" on Enums.cs for it to work
		killFeedIndex = 167;//what sprite will appear in the kill index
	}
}




