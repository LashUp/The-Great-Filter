using System;

namespace Cython.EnergyWeapons
{
	public static class EnergyWeaponManager
	{
		public static BeamWeaponInfo EmergencyBlaster = new BeamWeaponInfo(0.01f, 10f , "Class1PlasmaGunCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static PulseWeaponInfo largeBlockSmallPlasmaTurret = new PulseWeaponInfo(8f, 480f, "Class3PlasmaPulseCharge", 1, 1);
		
		public static PulseWeaponInfo WPN_CX3PlasmaCannon = new PulseWeaponInfo(8f, 480f, "Class3PlasmaPulseCharge", 1, 1);
		
		public static BeamWeaponInfo CX3LanceW = new BeamWeaponInfo(500f, 1260f , "CX3LanceMag", 1, 1000f, 4f, 8f, 20, 3);
		
		public static BeamWeaponInfo CX3LanceW_SM = new BeamWeaponInfo(88f, 1260f , "CX3LanceMag", 1, 1000f, 4f, 8f, 20, 3);
		
		public static BeamWeaponInfo largeBlockSmallLaserTurret = new BeamWeaponInfo(160f, 40f , "Class1LaserBeamCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static BeamWeaponInfo largeBlockSmallLaserTurretC2 = new BeamWeaponInfo(240f, 60f , "Class2LaserBeamCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static BeamWeaponInfo largeBlockSmallLaserTurretC3 = new BeamWeaponInfo(320f, 80f , "Class3LaserBeamCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static BeamWeaponInfo WPN_CX3LG_SM_Laser = new BeamWeaponInfo(0.25f, 80f , "Class3LaserBeamCharge", 1, 500f, 4f, 8f, 480, 4);

		public static BeamWeaponInfo smallBlockSmallLaserTurretC1 = new BeamWeaponInfo(0.8f, 5f , "Class1SMLaserBeamCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static BeamWeaponInfo smallBlockSmallLaserTurret = new BeamWeaponInfo(1.2f, 7.5f , "Class2SMLaserBeamCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static BeamWeaponInfo smallBlockSmallLaserTurretC3 = new BeamWeaponInfo(1.6f, 10f , "Class3SMLaserBeamCharge", 1, 1000f, 4f, 8f, 480, 10);
		
		public static PulseWeaponInfo largeBlockSmallBlasterTurret = new PulseWeaponInfo(50f, 10f, "Class1MGPlasmaGunCharge", 1, 2);
		
		public static PulseWeaponInfo largeBlockSmallBlasterTurretC2 = new PulseWeaponInfo(70f, 15f , "Class2MGPlasmaGunCharge", 1, 2);
		
		public static PulseWeaponInfo largeBlockSmallBlasterTurretC3 = new PulseWeaponInfo(50f, 20f , "Class3MGPlasmaGunCharge", 1, 2);
		
		public static PulseWeaponInfo WPN_CX3LG_SM_Blaster = new PulseWeaponInfo(0.25f, 20f , "Class3MGPlasmaGunCharge", 1, 1);

		public static PulseWeaponInfo smallBlockSmallBlasterTurretC1 = new PulseWeaponInfo(1.08f, 2.4f , "Class1PlasmaGunCharge", 1, 2);
		
		public static PulseWeaponInfo smallBlockSmallBlasterTurret = new PulseWeaponInfo(1.44f, 3.2f , "Class2PlasmaGunCharge", 1, 2);
		
		public static PulseWeaponInfo smallBlockSmallBlasterTurretC3 = new PulseWeaponInfo(0.25f, 3.6f , "Class3PlasmaGunCharge", 1, 2);
		
		public static PulseWeaponInfo FixedBlasterW = new PulseWeaponInfo(0.45f, 2.8f , "Class1MGPlasmaGunCharge", 1, 1);
		
		public static PulseWeaponInfo FixedBlasterCX3W = new PulseWeaponInfo(0.25f, 2.8f , "Class3MGPlasmaGunCharge", 1, 1);
	}
}

