using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ObjectBuilders;
using VRage.Game.ModAPI;
using Sandbox.Game.EntityComponents;
using System.Collections.Generic;
using VRage.ModAPI;
using System.Text;
using VRage;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;

namespace Cython.EnergyWeapons
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_InteriorTurret), true, "CX3PlasmaCannon")] 
	public class CX3PlasmaCannon: MyGameLogicComponent
	{
		MyDefinitionId electricityDefinition = new MyDefinitionId (typeof(MyObjectBuilder_GasProperties), "Electricity");
		
		MyObjectBuilder_EntityBase objectBuilder = null;

		IMyCubeBlock cubeBlock = null;
		Sandbox.ModAPI.IMyFunctionalBlock functionalBlock = null;
		Sandbox.ModAPI.IMyTerminalBlock terminalBlock;

		MyResourceSinkComponent resourceSink;
		IMyInventory m_inventory;

		string subtypeName;

		PulseWeaponInfo pulseWeaponInfo;

		List<MyObjectBuilder_AmmoMagazine> chargeObjectBuilders;
		List<SerializableDefinitionId> chargeDefinitionIds;

		float powerConsumption;
		float setPowerConsumption;
		float charge = 0f;

		int damageUpgrades = 0;
		int powerUpgrades = 0;
		float efficiencyUpgrades = 1f;

		public override void Init (MyObjectBuilder_EntityBase objectBuilder)
		{
			base.Init (objectBuilder);
			this.objectBuilder = objectBuilder;

			Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;

			functionalBlock = Entity as Sandbox.ModAPI.IMyFunctionalBlock;
			cubeBlock = Entity as IMyCubeBlock;
			terminalBlock = Entity as Sandbox.ModAPI.IMyTerminalBlock;

			subtypeName = functionalBlock.BlockDefinition.SubtypeName;

			getPulseWeaponInfo (subtypeName);
			initCharges ();

			terminalBlock.AppendingCustomInfo += appendCustomInfo;


			cubeBlock.AddUpgradeValue ("PowerEfficiency", 1.0f);
			cubeBlock.OnUpgradeValuesChanged += onUpgradeValuesChanged;
		}

		private void onUpgradeValuesChanged() {

			if (Entity != null) {

				efficiencyUpgrades = cubeBlock.UpgradeValues ["PowerEfficiency"];
				
			}
		}

		public void appendCustomInfo(Sandbox.ModAPI.IMyTerminalBlock block, StringBuilder info)
		{
			info.Clear ();


			info.AppendLine ("Type: " + cubeBlock.DefinitionDisplayNameText);
			info.AppendLine ("Required Input: " + powerConsumption.ToString("N") + "MW");
			info.AppendLine ("Maximum Input: " + (pulseWeaponInfo.powerUsage/efficiencyUpgrades).ToString("N") + "MW");

			info.AppendLine (" ");

			if (pulseWeaponInfo.classes > 1) {

				info.AppendLine ("Class: " + "Class " + (damageUpgrades + 1) + " Pulse Weapon");

			}

			info.AppendLine ("Energy Per Shot: " + ((pulseWeaponInfo.energyPerProjectile / 60f)/efficiencyUpgrades).ToString("N") + "MJ");
			info.AppendLine ("Current Charge: " + (charge / 60f).ToString("N") + "MJ");
		}

		private void initCharges() {

			chargeObjectBuilders = new List<MyObjectBuilder_AmmoMagazine> ();

			if (pulseWeaponInfo.classes == 1) {

				chargeObjectBuilders.Add (new MyObjectBuilder_AmmoMagazine () { SubtypeName = "" + pulseWeaponInfo.ammoName });

			} else {

				for (int i = 1; i <= pulseWeaponInfo.classes; i++) {

					chargeObjectBuilders.Add (new MyObjectBuilder_AmmoMagazine () { SubtypeName = "" + "Class" + i + pulseWeaponInfo.ammoName });
				}
			}

			chargeDefinitionIds = new List<SerializableDefinitionId> ();

			if (pulseWeaponInfo.classes == 1) {

				chargeDefinitionIds.Add (new SerializableDefinitionId (typeof(MyObjectBuilder_AmmoMagazine), "" + pulseWeaponInfo.ammoName));

			} else {
				
				for (int i = 1; i <= pulseWeaponInfo.classes; i++) {

					chargeDefinitionIds.Add (new SerializableDefinitionId (typeof(MyObjectBuilder_AmmoMagazine), "Class" + i + pulseWeaponInfo.ammoName));
				}
			}
		}

		public override MyObjectBuilder_EntityBase GetObjectBuilder (bool copy = false)
		{
			return objectBuilder;
		}

		private void getPulseWeaponInfo(string name) {

			if (subtypeName == "CX3PlasmaCannon") {
				pulseWeaponInfo = EnergyWeaponManager.WPN_CX3PlasmaCannon;
			}
		}
		
		    public void RemoveSmokeEffects(string subtypename)
            {
            var effect = MyParticlesLibrary.GetParticleEffect("Smoke_Missile");

            var generations = effect.GetGenerations();

            for (int i = 0; i < generations.Count; ++i)
                {
                        effect.RemoveGeneration(i);
                }

            }

		public override void UpdateOnceBeforeFrame ()
		{
			RemoveSmokeEffects(subtypeName);
			
                    MyParticleEffect effect;
                    if (MyParticlesManager.TryCreateParticleEffect("Collision_Sparks", out effect))
                    {
                        effect.Loop = false;
                        effect.UserScale = 0.2f;
                        //MyAPIGateway.Utilities.ShowNotification("Spawned", 17);
                    }

			resourceSink = Entity.Components.Get<MyResourceSinkComponent> ();

			resourceSink.SetRequiredInputByType (electricityDefinition, 0.0001f);
			setPowerConsumption = 0.0001f;

			m_inventory = ((Sandbox.ModAPI.Ingame.IMyTerminalBlock)Entity).GetInventory (0) as IMyInventory;

		}

		public override void UpdateBeforeSimulation ()
		{
			powerConsumption = resourceSink.CurrentInputByType (electricityDefinition);

			if (!m_inventory.IsItemAt (0)) {

				if (!functionalBlock.Enabled) {

					powerConsumption = 0.0001f;
					setPowerConsumption = 0.0001f;

				} else {

					if (setPowerConsumption != (pulseWeaponInfo.powerUsage / efficiencyUpgrades)) {
						
						resourceSink.SetRequiredInputByType (electricityDefinition, (pulseWeaponInfo.powerUsage / efficiencyUpgrades));
						setPowerConsumption = (pulseWeaponInfo.powerUsage / efficiencyUpgrades);

						powerConsumption = resourceSink.CurrentInputByType (electricityDefinition);
					}

					charge += powerConsumption;

					if (charge > (pulseWeaponInfo.energyPerProjectile / efficiencyUpgrades)) {
						m_inventory.AddItems ((MyFixedPoint)pulseWeaponInfo.projectilesPerCharge, chargeObjectBuilders [damageUpgrades]);

						charge -= (pulseWeaponInfo.energyPerProjectile / efficiencyUpgrades);
					}
				}

			} else {
				
				if (setPowerConsumption != 0.0001f) {

					resourceSink.SetRequiredInputByType (electricityDefinition, 0.0001f);
					setPowerConsumption = 0.0001f;

					powerConsumption = resourceSink.CurrentInputByType (electricityDefinition);
				}

			}

			terminalBlock.RefreshCustomInfo ();
		}

		public override void Close ()
		{

			if (m_inventory != null) {

				for (int i = 0; i < pulseWeaponInfo.classes; i++) { 
					m_inventory.RemoveItemsOfType (m_inventory.GetItemAmount (chargeDefinitionIds[i]), chargeObjectBuilders[i]);
				}
			}

			base.Close ();
		}

		public override void MarkForClose ()
		{
			if (m_inventory != null) {

				for (int i = 0; i < pulseWeaponInfo.classes; i++) { 
					m_inventory.RemoveItemsOfType (m_inventory.GetItemAmount (chargeDefinitionIds[i]), chargeObjectBuilders[i]);
				}
			}

			base.MarkForClose ();
		}
	}
}

