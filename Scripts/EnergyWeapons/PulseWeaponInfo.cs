using System;

namespace Cython.EnergyWeapons
{
	public class PulseWeaponInfo
	{
		public float powerUsage;
		public float energyPerProjectile;

		public string ammoName;
		public int classes;

		public int projectilesPerCharge;

		public PulseWeaponInfo(float powerUsage, float energyPerProjectile, string ammoName, int classes,  int projectilesPerCharge) {

			this.powerUsage = powerUsage;
			this.energyPerProjectile = energyPerProjectile;
			this.ammoName = ammoName;
			this.classes = classes;
			this.projectilesPerCharge = projectilesPerCharge;
		}
	}
}

