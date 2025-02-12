using System;

namespace MMXOnline;

public class MaverickWeapon : Weapon {
	public Player? player;
	public bool isMenuOpened;
	public float cooldown;
	public bool summonedOnce;
	public float lastHealth;
	public const float summonerCooldown = 2;
	public const float tagTeamCooldown = 4;
	public const float strikerCooldown = 4;
	public SavedMaverickData? smd;
	protected Maverick? _maverick;
	public Maverick? maverick {
		get {
			if (_maverick != null && _maverick.destroyed) {
				cooldown = _maverick.player.isTagTeam() ? tagTeamCooldown : strikerCooldown;
				lastHealth = _maverick.health;
				if (_maverick.health <= 0) smd = null;
				else smd = new SavedMaverickData(_maverick);
				_maverick = null;
			}
			return _maverick;
		}
		set {
			_maverick = value;
		}
	}
	public int selCommandIndex = 2;
	public int selCommandIndexX = 1;
	public const int maxCommandIndex = 4;
	public float currencyHUDAnimTime;
	public const float currencyHUDMaxAnimTime = 0.75f;
	public float currencyGainCooldown;
	public float currencyGainMaxCooldown {
		get {
			return 10 + player?.currency ?? 0;
		}
	}

	public bool isMoth;

	public MaverickWeapon(Player? player) {
		lastHealth = player?.getMaverickMaxHp() ?? 32;
		this.player = player;
	}

	public override void update() {
		base.update();
		Helpers.decrementTime(ref cooldown);

		if (player != null && !summonedOnce && !player.isStriker() && player.character != null) {
			if (currencyGainCooldown < currencyGainMaxCooldown) {
				currencyGainCooldown += Global.spf;
				if (currencyGainCooldown >= currencyGainMaxCooldown) {
					currencyGainCooldown = 0;
					currencyHUDAnimTime = Global.spf;
					player.currency++;
				}
			}
		}

		if (currencyHUDAnimTime > 0) {
			currencyHUDAnimTime += Global.spf;
			if (currencyHUDAnimTime > currencyHUDMaxAnimTime) {
				currencyHUDAnimTime = 0;
			}
		}
	}

	public Maverick summon(Player player, Point pos, Point destPos, int xDir, bool isMothHatch = false) {
		// X1
		if (this is ChillPenguinWeapon) {
			maverick = new ChillPenguin(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is SparkMandrillWeapon) {
			maverick = new SparkMandrill(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is ArmoredArmadilloWeapon) {
			maverick = new ArmoredArmadillo(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is LaunchOctopusWeapon) {
			maverick = new LaunchOctopus(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is BoomerangKuwangerWeapon) {
			maverick = new BoomerangKuwanger(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is StingChameleonWeapon) {
			maverick = new StingChameleon(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is StormEagleWeapon) {
			maverick = new StormEagle(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is FlameMammothWeapon) {
			maverick = new FlameMammoth(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is VelguarderWeapon) {
			maverick = new Velguarder(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		}
		  // X2
		  else if (this is WireSpongeWeapon) {
			maverick = new WireSponge(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is WheelGatorWeapon) {
			maverick = new WheelGator(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is BubbleCrabWeapon) {
			maverick = new BubbleCrab(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is FlameStagWeapon) {
			maverick = new FlameStag(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is MorphMothWeapon mmw) {
			if (mmw.isMoth) {
				maverick = new MorphMoth(player, pos, destPos, xDir, player.getNextActorNetId(), true, isMothHatch, sendRpc: true);
			} else {
				maverick = new MorphMothCocoon(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
			}
		} else if (this is MagnaCentipedeWeapon) {
			maverick = new MagnaCentipede(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is CrystalSnailWeapon) {
			maverick = new CrystalSnail(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is OverdriveOstrichWeapon) {
			maverick = new OverdriveOstrich(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is FakeZeroWeapon) {
			maverick = new FakeZero(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		}
		  // X3
		  else if (this is BlizzardBuffaloWeapon) {
			maverick = new BlizzardBuffalo(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is ToxicSeahorseWeapon) {
			maverick = new ToxicSeahorse(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is TunnelRhinoWeapon) {
			maverick = new TunnelRhino(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is VoltCatfishWeapon) {
			maverick = new VoltCatfish(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is CrushCrawfishWeapon) {
			maverick = new CrushCrawfish(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is NeonTigerWeapon) {
			maverick = new NeonTiger(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is GravityBeetleWeapon) {
			maverick = new GravityBeetle(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is BlastHornetWeapon) {
			maverick = new BlastHornet(player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true);
		} else if (this is DrDopplerWeapon ddw) {
			var drDoppler = new DrDoppler(
				player, pos, destPos, xDir, player.getNextActorNetId(), true, sendRpc: true
			);
			drDoppler.ballType = ddw.ballType;
			maverick = drDoppler;
		}

		if (maverick == null) {
			throw new Exception("Error summoning maverick on maverick weapon " + this.GetType().ToString());
		}
		if (summonedOnce) {
			maverick.setHealth(lastHealth);
		} else {
			lastHealth = maverick.maxHealth;
		}
		smd?.applySavedMaverickData(maverick, player.isPuppeteer());
		if (player.isStriker()) {
			if (maverick is not MorphMothCocoon) {
				maverick.ammo = maverick.maxAmmo;
			}
		}
		summonedOnce = true;
		return maverick;
	}

	public bool canUseSubtank(SubTank subtank) {
		return maverick != null && maverick.health < maverick.maxHealth;
	}
}

public class SigmaMenuWeapon : Weapon {
	public SigmaMenuWeapon() {
		index = (int)WeaponIds.Sigma;
		weaponSlotIndex = (int)SlotIndex.Sigma;
		displayName = "Sigma";
		fireRate = 60 * 4;
		drawAmmo = false;
		drawCooldown = false;
	}
}

public class ChillPenguinWeapon : MaverickWeapon {
	public ChillPenguinWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.ChillPenguin;
		weaponSlotIndex = (int)SlotIndex.CP;
		displayName = "Icy Penguigo";
	}
}

public class SparkMandrillWeapon : MaverickWeapon {
	public SparkMandrillWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.SparkMandrill;
		weaponSlotIndex = (int)SlotIndex.SMandrill;
		displayName = "Spark Mandriller";
	}
}

public class ArmoredArmadilloWeapon : MaverickWeapon {
	public ArmoredArmadilloWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.ArmoredArmadillo;
		weaponSlotIndex = (int)SlotIndex.AArmardillo;
		displayName = "Armored Armage";
	}
}

public class LaunchOctopusWeapon : MaverickWeapon {
	public LaunchOctopusWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.LaunchOctopus;
		weaponSlotIndex = (int)SlotIndex.LOctopus;
		displayName = "Launch Octopus";
	}
}

public class BoomerangKuwangerWeapon : MaverickWeapon {
	public BoomerangKuwangerWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.BoomerangKuwanger;
		weaponSlotIndex = (int)SlotIndex.BKwagata;
		displayName = "Boomerang Kuwanger";
	}
}

public class StingChameleonWeapon : MaverickWeapon {
	public StingChameleonWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.StingChameleon;
		weaponSlotIndex = (int)SlotIndex.SChameleon;
		displayName = "Sting Chameleon";
	}
}

public class StormEagleWeapon : MaverickWeapon {
	public StormEagleWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.StormEagle;
		weaponSlotIndex = (int)SlotIndex.SEagle;
		displayName = "Storm Eagle";
	}
}

public class FlameMammothWeapon : MaverickWeapon {
	public FlameMammothWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.FlameMammoth;
		weaponSlotIndex = (int)SlotIndex.FMammoth;
		displayName = "Flame Mammoth";
	}
}

public class VelguarderWeapon : MaverickWeapon {
	public VelguarderWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.Velguarder;
		weaponSlotIndex = (int)SlotIndex.Doggo;
		displayName = "Velguarder";
	}
}

public class WireSpongeWeapon : MaverickWeapon {
	public WireSpongeWeapon(Player player) : base(player) {
		index = (int)WeaponIds.WireSponge;
		weaponSlotIndex = (int)SlotIndex.WSponge;
		displayName = "Wire Sponge";
	}
}

public class WheelGatorWeapon : MaverickWeapon {
	public WheelGatorWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.WheelGator;
		weaponSlotIndex = (int)SlotIndex.WGator;
		displayName = "Wheel Gator";
	}
}

public class BubbleCrabWeapon : MaverickWeapon {
	public BubbleCrabWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.BubbleCrab;
		weaponSlotIndex = (int)SlotIndex.Carlos;
		displayName = "Bubbly Crablos";
	}
}

public class FlameStagWeapon : MaverickWeapon {
	public FlameStagWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.FlameStag;
		weaponSlotIndex = (int)SlotIndex.FStag;
		displayName = "Flame Stag";
	}
}

public class MorphMothWeapon : MaverickWeapon {
	public MorphMothWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.MorphMoth;
		weaponSlotIndex = (int)SlotIndex.MMMoth;
		displayName = "Morph Moth";
	}

	public override void update() {
		base.update();
		if (!isMoth) weaponSlotIndex = (int)SlotIndex.MMConcoon;
		else weaponSlotIndex = (int)SlotIndex.MMMoth;
	}
}

public class MagnaCentipedeWeapon : MaverickWeapon {
	public MagnaCentipedeWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.MagnaCentipede;
		weaponSlotIndex = (int)SlotIndex.MCentiped;
		displayName = "Magna Centipede";
	}
}

public class CrystalSnailWeapon : MaverickWeapon {
	public CrystalSnailWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.CrystalSnail;
		weaponSlotIndex = (int)SlotIndex.Dav;
		displayName = "Crystal Snail";
	}
}

public class OverdriveOstrichWeapon : MaverickWeapon {
	public OverdriveOstrichWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.OverdriveOstrich;
		weaponSlotIndex = (int)SlotIndex.OOOStrich;
		displayName = "Overdrive Ostrich";
	}
}

public class FakeZeroWeapon : MaverickWeapon {
	public FakeZeroWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.FakeZero;
		weaponSlotIndex = (int)SlotIndex.Claudio;
		displayName = "Claudio (Clone Zero)";
	}
}

public class BlizzardBuffaloWeapon : MaverickWeapon {
	public BlizzardBuffaloWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.BlizzardBuffalo;
		weaponSlotIndex = (int)SlotIndex.BBuffalo;
		displayName = "Blizzard Buffalo";
	}
}

public class ToxicSeahorseWeapon : MaverickWeapon {
	public ToxicSeahorseWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.ToxicSeahorse;
		weaponSlotIndex = (int)SlotIndex.TSeahorse;
		displayName = "Toxic Seahorse";
	}
}

public class TunnelRhinoWeapon : MaverickWeapon {
	public TunnelRhinoWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.TunnelRhino;
		weaponSlotIndex = (int)SlotIndex.TRino;
		displayName = "Tunnel Rhino";
	}
}

public class VoltCatfishWeapon : MaverickWeapon {
	public VoltCatfishWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.VoltCatfish;
		weaponSlotIndex = (int)SlotIndex.VCatfish;
		displayName = "Volt Catfish";
	}
}

public class CrushCrawfishWeapon : MaverickWeapon {
	public CrushCrawfishWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.CrushCrawfish;
		weaponSlotIndex = (int)SlotIndex.CCrawfish;
		displayName = "Crush Crawfish";
	}
}

public class NeonTigerWeapon : MaverickWeapon {
	public NeonTigerWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.NeonTiger;
		weaponSlotIndex = (int)SlotIndex.NTiger;
		displayName = "Neon Tiger";
	}
}

public class GravityBeetleWeapon : MaverickWeapon {
	public GravityBeetleWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.GravityBeetle;
		weaponSlotIndex = (int)SlotIndex.GBeetle;
		displayName = "Gravity Beetle";
	}
}

public class BlastHornetWeapon : MaverickWeapon {
	public BlastHornetWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.BlastHornet;
		weaponSlotIndex = (int)SlotIndex.Bhornet;
		displayName = "Blast Hornet";
	}
}

public class DrDopplerWeapon : MaverickWeapon {
	public int ballType; // 0 = shock gun, 1 = vaccine
	public DrDopplerWeapon(Player? player) : base(player) {
		index = (int)WeaponIds.DrDoppler;
		weaponSlotIndex = (int)SlotIndex.DrCatus;
		displayName = "Dr. Doppler";
	}
}
