using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MMXOnline;

public partial class RPCCreateProj : RPC {
	public static Dictionary<int, ProjCreate> functs = new Dictionary<int, ProjCreate> {
		// X Stuff.
		//BUSTERS
		{ (int)ProjIds.Buster, BusterProj.rpcInvoke },
		{ (int)ProjIds.Buster2, Buster2Proj.rpcInvoke },
		{ (int)ProjIds.BusterUnpo, RagingBusterProj.rpcInvoke },
		{ (int)ProjIds.Buster3, Buster3Proj.rpcInvoke },
		{ (int)ProjIds.Buster4, Buster4Proj.rpcInvoke },
		{ (int)ProjIds.BusterX3Proj2, BusterX3Proj2.rpcInvoke },
		{ (int)ProjIds.BusterX3Proj3, BusterX3Proj3.rpcInvoke },
		
		{ (int)ProjIds.BusterX3Plasma, BusterPlasmaProj.rpcInvoke },
		{ (int)ProjIds.BusterX3PlasmaHit, BusterPlasmaHitProj.rpcInvoke },

		//X1 PROJS
		{ (int)ProjIds.Torpedo, TorpedoProj.rpcInvoke },
		{ (int)ProjIds.TorpedoCharged, TorpedoProj.rpcInvoke },
		{ (int)ProjIds.Sting, StingProj.rpcInvoke },
		{ (int)ProjIds.StingDiag, StingProj.rpcInvoke },
		{ (int)ProjIds.RollingShield, RollingShieldProj.rpcInvoke },
		{ (int)ProjIds.RollingShieldCharged, RollingShieldProjCharged.rpcInvoke },
		{ (int)ProjIds.FireWave, FireWaveProj.rpcInvoke },
		{ (int)ProjIds.FireWaveChargedStart, FireWaveProjChargedStart.rpcInvoke },
		{ (int)ProjIds.FireWaveCharged, FireWaveProjCharged.rpcInvoke },
		{ (int)ProjIds.Tornado, TornadoProj.rpcInvoke },
		{ (int)ProjIds.TornadoCharged, TornadoProjCharged.rpcInvoke },
		{ (int)ProjIds.ElectricSpark, ElectricSparkProj.rpcInvoke },
		
		{ (int)ProjIds.ElectricSparkChargedStart, ElectricSparkProjChargedStart.rpcInvoke },
		{ (int)ProjIds.ElectricSparkCharged, ElectricSparkProjCharged.rpcInvoke },
		{ (int)ProjIds.Boomerang, BoomerangProj.rpcInvoke },
		{ (int)ProjIds.BoomerangCharged, BoomerangProjCharged.rpcInvoke },
		{ (int)ProjIds.ShotgunIce, ShotgunIceProj.rpcInvoke },
		{ (int)ProjIds.ShotgunIceCharged, ShotgunIceProjCharged.rpcInvoke },
		{ (int)ProjIds.ShotgunIceSled, ShotgunIceProjSled.rpcInvoke },
		{ (int)ProjIds.Hadouken, HadoukenProj.rpcInvoke} ,

		//X2 PROJS
		{ (int)ProjIds.CrystalHunter, CrystalHunterProj.rpcInvoke },
		{ (int)ProjIds.BubbleSplash, BubbleSplashProj.rpcInvoke },
		{ (int)ProjIds.BubbleSplashCharged, BubbleSplashProjCharged.rpcInvoke },
		{ (int)ProjIds.SilkShot, SilkShotProj.rpcInvoke },
		{ (int)ProjIds.SilkShotShrapnel, SilkShotProjShrapnel.rpcInvoke },
		{ (int)ProjIds.SilkShotChargedLv2, SilkShotProjLv2.rpcInvoke },
		{ (int)ProjIds.SilkShotCharged, SilkShotProjCharged.rpcInvoke },
		{ (int)ProjIds.SpinWheel, SpinWheelProj.rpcInvoke },
		{ (int)ProjIds.SpinWheelChargedStart, SpinWheelProjChargedStart.rpcInvoke },
		{ (int)ProjIds.SpinWheelCharged, SpinWheelProjCharged.rpcInvoke },
		{ (int)ProjIds.SonicSlicerStart, SonicSlicerStart.rpcInvoke },
		{ (int)ProjIds.SonicSlicer, SonicSlicerProj.rpcInvoke },
		{ (int)ProjIds.SonicSlicerCharged, SonicSlicerProjCharged.rpcInvoke },
		{ (int)ProjIds.StrikeChain, StrikeChainProj.rpcInvoke },
		{ (int)ProjIds.StrikeChainCharged, StrikeChainProjCharged.rpcInvoke },
		
		{ (int)ProjIds.MagnetMine, MagnetMineProj.rpcInvoke },
		{ (int)ProjIds.MagnetMineCharged, MagnetMineProjCharged.rpcInvoke },
		{ (int)ProjIds.SpeedBurner, SpeedBurnerProj.rpcInvoke },
		{ (int)ProjIds.SpeedBurnerWater, SpeedBurnerProjWater.rpcInvoke },
		{ (int)ProjIds.ItemTracer, ItemTracerProj.rpcInvoke },

		//X3 PROJS
		{ (int)ProjIds.AcidBurst, AcidBurstProj.rpcInvoke },
		{ (int)ProjIds.AcidBurstSmall, AcidBurstProjSmall.rpcInvoke },
		{ (int)ProjIds.AcidBurstCharged, AcidBurstProjCharged.rpcInvoke },
		{ (int)ProjIds.ParasiticBomb, ParasiticBombProj.rpcInvoke },
		{ (int)ProjIds.ParasiticBombCharged, ParasiticBombProjCharged.rpcInvoke },
		{ (int)ProjIds.TriadThunder, TriadThunderProj.rpcInvoke },
		{ (int)ProjIds.TriadThunderQuake, TriadThunderQuake.rpcInvoke },
		{ (int)ProjIds.TriadThunderCharged, TriadThunderProjCharged.rpcInvoke },
		{ (int)ProjIds.SpinningBlade, SpinningBladeProj.rpcInvoke },
		{ (int)ProjIds.SpinningBladeCharged, SpinningBladeProjCharged.rpcInvoke },
		{ (int)ProjIds.RaySplasher, RaySplasherProj.rpcInvoke },
		{ (int)ProjIds.RaySplasherChargedProj, RaySplasherTurretProj.rpcInvoke },
		{ (int)ProjIds.GravityWell, GravityWellProj.rpcInvoke },
		{ (int)ProjIds.GravityWellCharged, GravityWellProjCharged.rpcInvoke },
		{ (int)ProjIds.FrostShield, FrostShieldProj.rpcInvoke },
		{ (int)ProjIds.FrostShieldAir, FrostShieldProjAir.rpcInvoke },
		{ (int)ProjIds.FrostShieldGround, FrostShieldProjGround.rpcInvoke },
		{ (int)ProjIds.FrostShieldCharged, FrostShieldProjCharged.rpcInvoke },
		{ (int)ProjIds.FrostShieldChargedGrounded, FrostShieldProjChargedGround.rpcInvoke },
		{ (int)ProjIds.FrostShieldPlatform, FrostShieldProjPlatform.rpcInvoke },
		{ (int)ProjIds.TornadoFang, TornadoFangProj.rpcInvoke },
		{ (int)ProjIds.TornadoFang2, TornadoFangProj.rpcInvoke },
		{ (int)ProjIds.TornadoFangCharged, TornadoFangProjCharged.rpcInvoke },
		{ (int)ProjIds.XSaberProj, XSaberProj.rpcInvoke },

		// X4 Stuff Projs
				
		{ (int)ProjIds.LightningWebProj, LightningWebProj.rpcInvoke },
		{ (int)ProjIds.LightningWeb, LightningWebProjWeb.rpcInvoke },
		{ (int)ProjIds.LightningWebChargedProj, LightningWebProjCharged.rpcInvoke },
		{ (int)ProjIds.LightningWebCharged, LightningWebProjWebCharged.rpcInvoke },
		{ (int)ProjIds.FrostTower, FrostTowerProj.rpcInvoke },
		{ (int)ProjIds.FrostTowerCharged, FrostTowerProjCharged.rpcInvoke },
		{ (int)ProjIds.SoulBodyHologram, SoulBodyHologram.rpcInvoke },
		{ (int)ProjIds.SoulBodyHologram2, SoulBodyHologram.rpcInvoke },
		{ (int)ProjIds.SoulBodyX5, SoulBodyX5.rpcInvoke },
		{ (int)ProjIds.RisingFire, RisingFireProj.rpcInvoke },
		{ (int)ProjIds.RisingFireChargedStart, RisingFireProjChargedStart.rpcInvoke },
		{ (int)ProjIds.RisingFireCharged, RisingFireProjCharged.rpcInvoke },
		{ (int)ProjIds.RisingFireUnderwater, RisingFireWaterProj.rpcInvoke },
		{ (int)ProjIds.RisingFireUnderwaterCharged, RisingFireWaterProjCharged.rpcInvoke },
		{ (int)ProjIds.GroundHunter, GroundHunterProj.rpcInvoke },
		{ (int)ProjIds.GroundHunterCharged, GroundHunterChargedProj.rpcInvoke },
		{ (int)ProjIds.GroundHunterSmall, GroundHunterSmallProj.rpcInvoke },
		{ (int)ProjIds.AimingLaser, AimingLaserProj.rpcInvoke },
		{ (int)ProjIds.AimingLaserCharged, AimingLaserChargedProj.rpcInvoke },
		{ (int)ProjIds.AimingLaserMissle, PeacockMissle.rpcInvoke },
		
		{ (int)ProjIds.DoubleCyclone, DoubleCycloneProj.rpcInvoke },
		{ (int)ProjIds.TwinSlasher, TwinSlasherProj.rpcInvoke },
		{ (int)ProjIds.TwinSlasher2, TwinSlasherProj.rpcInvoke },
		{ (int)ProjIds.TwinSlasherCharged, TwinSlasherProjCharged.rpcInvoke },
		{ (int)ProjIds.TwinSlasherCharged2, TwinSlasherProjCharged.rpcInvoke },
		{ (int)ProjIds.TwinSlasherCharged3, TwinSlasherProjCharged.rpcInvoke },
		{ (int)ProjIds.TwinSlasherCharged4, TwinSlasherProjCharged.rpcInvoke },


	
	

		//EXTRA
		{ (int)ProjIds.UPParryMelee, UPParryMeleeProj.rpcInvoke },
		{ (int)ProjIds.UPParryProj, UPParryRangedProj.rpcInvoke },
		{ (int)ProjIds.ChainrodProj, ChainrodProj.rpcInvoke },

		// Wily Cut Axl.
		{ (int)ProjIds.AxlDiscardedWeapon, AxlDiscrardedWeapon.rpcInvoke},
		{ (int)ProjIds.BlastLauncherWC, BlastLauncherWCProj.rpcInvoke},
		{ (int)ProjIds.GreenSpinnerWC, GreenSpinnerWCProj.rpcInvoke},
		{ (int)ProjIds.RayGunWC, RayGunWCProj.rpcInvoke},
		{ (int)ProjIds.IceGattlingWC, IceGattlingWCProj.rpcInvoke},
		{ (int)ProjIds.IceGattlingAltWC, IceGattlingAltWCProj.rpcInvoke},
		{ (int)ProjIds.SpiralMagnumWC, SpiralMagnumWCProj.rpcInvoke},
		{ (int)ProjIds.FormicAcidWC, FormidAcidProj.rpcInvoke},
		{ (int)ProjIds.AxlBulletWC, AxlBulletWCProj.rpcInvoke},
		{ (int)ProjIds.CopyShotWC, CopyShotWCProj.rpcInvoke},
		{ (int)ProjIds.BlueBullet, BlueBulletProj.rpcInvoke},
		{ (int)ProjIds.AxlMeleeBullet, AxlMeleeBullet.rpcInvoke},
		
		// Vile stuff.
		{ (int)ProjIds.FrontRunner, VileCannonProj.rpcInvoke },
		{ (int)ProjIds.FatBoy, VileCannonProj.rpcInvoke },
		{ (int)ProjIds.LongshotGizmo, VileCannonProj.rpcInvoke },
		// Buster Zero
		{ (int)ProjIds.DZBuster, DZBusterProj.rpcInvoke },
		{ (int)ProjIds.DZBuster2, DZBuster2Proj.rpcInvoke },
		{ (int)ProjIds.DZBuster3, DZBuster3Proj.rpcInvoke },

		// Dynamo
			{ (int)ProjIds.DynamoIceDagger, DynamoKnifeProj.rpcInvoke },
			{ (int)ProjIds.DynamoAxeProj, DynamoAxeProj.rpcInvoke },
	
		// Mavericks
		{ (int)ProjIds.VoltCSuck, VoltCSuckProj.rpcInvoke },
		{ (int)ProjIds.TSeahorseAcid2, TSeahorseAcid2Proj.rpcInvoke },
		{ (int)ProjIds.WSpongeSpike, WSpongeSpike.rpcInvoke },
		{ (int)ProjIds.BBuffaloIceProj, BBuffaloIceProj.rpcInvoke },



		//Axl
		{ (int)ProjIds.BlackArrowGround, BlackArrowGrounded.rpcInvoke },



		// Iris
		{ (int)ProjIds.IrisCrystal, NewIrisCrystal.rpcInvoke },
		


		// For Enemy RPC
		{ (int)ProjIds.HGM2RPC, HGM2RPC.rpcInvoke },
	
	};

}

public struct ProjParameters {
	public int projId;
	public Point pos;
	public int xDir;
	public Player player;
	public ushort netId;
	public byte[] extraData;
	public float angle;
	public float byteAngle;
	public Actor owner;
}

public delegate Projectile ProjCreate(ProjParameters arg);
