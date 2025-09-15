using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMXOnline;

namespace MMXOnline;

public class XLoadoutSetup {
	public static List<Weapon> getLoadout(Player player, XLoadout xLoadout) {
		List<Weapon> weapons = new();
		// 1v1/Training loadout.
		bool enableX1Weapons = player.loadout.xLoadout.weapon1 < 9 &&
		 player.loadout.xLoadout.weapon2 < 9 &&  player.loadout.xLoadout.weapon3 < 9
		;
		bool enableX2Weapons = player.loadout.xLoadout.weapon1 >= 9 &&
		 player.loadout.xLoadout.weapon2 >= 9 &&  player.loadout.xLoadout.weapon3 >= 9
		 && player.loadout.xLoadout.weapon1 <= 16 &&
		 player.loadout.xLoadout.weapon2  <= 16&&  player.loadout.xLoadout.weapon3  <= 16
		;
		bool enableX3Weapons = player.loadout.xLoadout.weapon1 >= 17 &&
		 player.loadout.xLoadout.weapon2 >= 17 &&  player.loadout.xLoadout.weapon3 >= 17
		 && player.loadout.xLoadout.weapon1 <= 24 &&
		 player.loadout.xLoadout.weapon2  <= 24 &&  player.loadout.xLoadout.weapon3  <= 24
		;
		bool enableX4Weapons =player.loadout.xLoadout.weapon1 > 24 &&
		 player.loadout.xLoadout.weapon2 > 24  &&  player.loadout.xLoadout.weapon3 > 24 
		;
		
		weapons.Add(new XBuster());

		if (enableX1Weapons) {
			weapons.Add(new HomingTorpedo());
			weapons.Add(new ChameleonSting());
			weapons.Add(new RollingShield());
			weapons.Add(new FireWave());
			weapons.Add(new StormTornado());
			weapons.Add(new ElectricSpark());
			weapons.Add(new BoomerangCutter());
			weapons.Add(new ShotgunIce());
		}
		if (enableX2Weapons) {
			weapons.Add(new CrystalHunter());
			weapons.Add(new BubbleSplash());
			weapons.Add(new SilkShot());
			weapons.Add(new SpinWheel());
			weapons.Add(new SonicSlicer());
			weapons.Add(new StrikeChain());
			weapons.Add(new MagnetMine());
			weapons.Add(new SpeedBurner());
		}
		if (enableX3Weapons) {
			weapons.Add(new AcidBurst());
			weapons.Add(new ParasiticBomb());
			weapons.Add(new TriadThunder());
			weapons.Add(new SpinningBlade());
			weapons.Add(new RaySplasher());
			weapons.Add(new GravityWell());
			weapons.Add(new FrostShield());
			weapons.Add(new TornadoFang());
		}

			if (enableX4Weapons) {
			weapons.Add(new DoubleCyclone());
			weapons.Add(new SoulBody());
			weapons.Add(new FrostTower());
			weapons.Add(new RisingFire());
			weapons.Add(new LightningWeb());
			weapons.Add(new GroundHunter());
			weapons.Add(new AimingLaser());
			weapons.Add(new TwinSlasher());
		}


		
			

		if (player.hasArmArmor(3) || player.xArmor1v1 == 2) weapons.Add(new HyperCharge());
		if (player.hasBodyArmor(2) || player.xArmor1v1 == 3) weapons.Add(new GigaCrush());

		// Regular Loadout.
		if (!enableX1Weapons && !enableX2Weapons && !enableX3Weapons && !enableX4Weapons) {
			
			weapons = xLoadout.getWeaponsFromLoadout(player);
		}
			return weapons;
		}

		
	
}
