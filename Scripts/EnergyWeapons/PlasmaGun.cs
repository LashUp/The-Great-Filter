using System;
using Sandbox.Common.ObjectBuilders;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using Sandbox.ModAPI;
using VRage.ModAPI;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using VRage.Game;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.Game.EntityComponents;
using VRage.Utils;

namespace ChemicalThrusters
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_AutomaticRifle), true, "EmergencyBlaster")] 
	public class PlasmaGun: MyGameLogicComponent
	{
		readonly float MaxElectricityConsumption = 0.002f;

		MyObjectBuilder_EntityBase m_objectBuilder = null;

		MyResourceSinkComponent m_resourceSink;
		//MyResourceSourceComponent m_resourceSource;

		MyDefinitionId ElectricityDefinitionsId = new MyDefinitionId (typeof(MyObjectBuilder_GasProperties), "Electricity");

		int m_ticks = 0;
		
		public override void Init (MyObjectBuilder_EntityBase objectBuilder)
		{
			m_objectBuilder = objectBuilder;

			Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;

			MyResourceSinkInfo ElectricityResourceSinkInfo = new MyResourceSinkInfo { 
				ResourceTypeId = ElectricityDefinitionsId, 
				MaxRequiredInput = MaxElectricityConsumption, 
				RequiredInputFunc = OnUpdateElectricityUsage};

			MyResourceSinkComponent ResourceSink = new MyResourceSinkComponent();

			ResourceSink.Init(MyStringHash.GetOrCompute("Factory"), ElectricityResourceSinkInfo);

			Entity.Components.Add<MyResourceSinkComponent>(ResourceSink);

			ResourceSink.Update();

			m_resourceSink = ResourceSink;
			/*
			// Because a Cargo Container does not produce anything, we need to create a new Resource Source for it and attach it to the block
			MyResourceSourceComponent ResourceSource = new MyResourceSourceComponent();
			Entity.Components.Add<MyResourceSourceComponent>(ResourceSource);

			// We let it produce OxyHydrogen gas with a defined (changed by us later) maximum of 1200MW
			MyResourceSourceInfo oxyHydrogenResourceSourceInfo = new MyResourceSourceInfo() {ResourceTypeId = ElectricityDefinitionsId, DefinedOutput = 0.0001f, IsInfiniteCapacity = true, ProductionToCapacityMultiplier = 60*60};
			ResourceSource.Init(MyStringHash.GetOrCompute("Battery"), oxyHydrogenResourceSourceInfo);
			ResourceSource.SetMaxOutputByType(ElectricityDefinitionsId, 0.00005f);
			ResourceSource.SetProductionEnabledByType(ElectricityDefinitionsId, true);

			m_resourceSource = ResourceSource;
			*/
		}
			

		public override MyObjectBuilder_EntityBase GetObjectBuilder (bool copy = false)
		{
			((MyObjectBuilder_AutomaticRifle)m_objectBuilder).CurrentAmmo = ((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition;
			return m_objectBuilder;
		}

		float OnUpdateElectricityUsage()
		{
			if(((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition < 10) {
				return MaxElectricityConsumption;
			} else {
				return 0;
			}
		}

		public override void UpdateBeforeSimulation ()
		{
			//((IMyHandheldGunObject<MyGunBase>)Entity).CurrentAmmunition = 100;
			//((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition = 100;

			if((m_ticks % 20) == 0) {
				if(((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition < 10) {
					if(m_resourceSink.IsPoweredByType(ElectricityDefinitionsId)) {
						((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition += 1;
						((MyObjectBuilder_AutomaticRifle)Entity.GetObjectBuilder()).CurrentAmmo = ((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition;
					}
					MyAPIGateway.Utilities.ShowNotification("Tick", 40, VRage.Game.MyFontEnum.Red);
					((IMyHandheldGunObject<MyGunBase>)Entity).CurrentAmmunition = ((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition;
					//m_resourceSource.SetProductionEnabledByType(ElectricityDefinitionsId, false);
				}
			} else {
				//m_resourceSource.SetProductionEnabledByType(ElectricityDefinitionsId, true);
			}

			m_resourceSink.Update();

			MyAPIGateway.Utilities.ShowNotification("Läuft: " + ((IMyHandheldGunObject<MyGunBase>)Entity).DefinitionId, 17);
			MyAPIGateway.Utilities.ShowNotification("" + ((IMyHandheldGunObject<MyGunBase>)Entity).CurrentAmmunition + ":" + ((IMyHandheldGunObject<MyGunBase>)Entity).CurrentMagazineAmmunition, 17);
			m_ticks++;
		}
	}
}

