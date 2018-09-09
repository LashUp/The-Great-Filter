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
using Sandbox.Game.Entities;
using VRageMath;
using VRage.Game.Entity;
using Sandbox.Game.Gui;
using VRage.Utils;
using VRage.Game.ModAPI.Interfaces;

namespace Cython.EnergyWeapons
{
	[MyEntityComponentDescriptor(typeof(MyObjectBuilder_InteriorTurret), true, "CythonLargeBlockSmallLaserTurret")] 
	public class BeamTurret: MyGameLogicComponent
	{
		MyDefinitionId electricityDefinition = new MyDefinitionId (typeof(MyObjectBuilder_GasProperties), "Electricity");

		MyObjectBuilder_EntityBase objectBuilder = null;

		MyEntity3DSoundEmitter e;


		IMyCubeBlock cubeBlock = null;
		Sandbox.ModAPI.IMyFunctionalBlock functionalBlock = null;
		Sandbox.ModAPI.IMyTerminalBlock terminalBlock;

		MyResourceSinkComponent resourceSink;
		IMyInventory m_inventory;

		string subtypeName;

		BeamWeaponInfo beamWeaponInfo;

		float powerConsumption;
		float setPowerConsumption;

		float currentHeat;
		bool overheated = false;

		long lastShootTime;
		int lastShootTimeTicks;


		bool hitBool = false;

		int ticks = 0;

		int damageUpgrades = 0;
		float heatUpgrades = 0;
		float efficiencyUpgrades = 1f;

		Vector3D from = new Vector3D();
		Vector3D to = new Vector3D();
		Vector3D toTarget = new Vector3D();

		List<MyObjectBuilder_AmmoMagazine> chargeObjectBuilders;
		List<SerializableDefinitionId> chargeDefinitionIds;
		
		public override void Init (MyObjectBuilder_EntityBase objectBuilder)
		{
			base.Init (objectBuilder);
			this.objectBuilder = objectBuilder;
			Entity.NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME;

			functionalBlock = Entity as Sandbox.ModAPI.IMyFunctionalBlock;
			cubeBlock = Entity as IMyCubeBlock;
			terminalBlock = Entity as Sandbox.ModAPI.IMyTerminalBlock;

			subtypeName = functionalBlock.BlockDefinition.SubtypeName;

			getBeamWeaponInfo (subtypeName);
			initCharges ();

			cubeBlock.AddUpgradeValue ("PowerEfficiency", 1.0f);
			cubeBlock.OnUpgradeValuesChanged += onUpgradeValuesChanged;

			terminalBlock.AppendingCustomInfo += appendCustomInfo;

			IMyCubeBlock cube = Entity as IMyCubeBlock;
			lastShootTime = ((MyObjectBuilder_InteriorTurret)cube.GetObjectBuilderCubeBlock()).GunBase.LastShootTime;

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
			info.AppendLine ("Maximum Input: " + (beamWeaponInfo.powerUsage/efficiencyUpgrades).ToString("N") + "MW");

			info.AppendLine (" ");

			if (beamWeaponInfo.classes > 1) {
				
				info.AppendLine ("Class: " + "Class " + (damageUpgrades + 1) + " Beam Weapon");

			}

			info.AppendLine ("Heat: " + currentHeat + "/" +(beamWeaponInfo.maxHeat).ToString("N") + "C");
			info.AppendLine ("Overheated: " + overheated);
		}

		private void initCharges() {

			chargeObjectBuilders = new List<MyObjectBuilder_AmmoMagazine> ();

			if (beamWeaponInfo.classes == 1) {

				chargeObjectBuilders.Add (new MyObjectBuilder_AmmoMagazine () { SubtypeName = "" + beamWeaponInfo.ammoName });

			} else {
				
				for (int i = 1; i <= beamWeaponInfo.classes; i++) {

					chargeObjectBuilders.Add (new MyObjectBuilder_AmmoMagazine () { SubtypeName = "" + "Class" + i + beamWeaponInfo.ammoName });
				}
			}

			chargeDefinitionIds = new List<SerializableDefinitionId> ();

			if (beamWeaponInfo.classes == 1) {

				chargeDefinitionIds.Add (new SerializableDefinitionId (typeof(MyObjectBuilder_AmmoMagazine), "" + beamWeaponInfo.ammoName));

			} else {
				
				for (int i = 1; i <= beamWeaponInfo.classes; i++) {

					chargeDefinitionIds.Add (new SerializableDefinitionId (typeof(MyObjectBuilder_AmmoMagazine), "Class" + i + beamWeaponInfo.ammoName));
				}
			}
		}

		public override MyObjectBuilder_EntityBase GetObjectBuilder (bool copy = false)
		{
			return objectBuilder;
		}

		private void getBeamWeaponInfo(string name) {

			if (subtypeName == "CythonLargeBlockSmallLaserTurret") {
				beamWeaponInfo = EnergyWeaponManager.largeBlockSmallLaserTurret;
			}
		}

		public override void UpdateOnceBeforeFrame ()
		{

			resourceSink = Entity.Components.Get<MyResourceSinkComponent> ();

			resourceSink.SetRequiredInputByType (electricityDefinition, 0.0001f);
			setPowerConsumption = 0.0001f;

			m_inventory = ((Sandbox.ModAPI.Ingame.IMyTerminalBlock)Entity).GetInventory (0) as IMyInventory;

		}

		public override void UpdateBeforeSimulation ()
		{

			int chargesInInventory = (int) m_inventory.GetItemAmount (chargeDefinitionIds [damageUpgrades]);

			IMyCubeBlock cube = Entity as IMyCubeBlock;

			long currentShootTime = ((MyObjectBuilder_InteriorTurret)cube.GetObjectBuilderCubeBlock()).GunBase.LastShootTime;

			 
			if (currentHeat > 0f) {

				if ((ticks - lastShootTimeTicks) > beamWeaponInfo.heatDissipationDelay) {

					currentHeat -= beamWeaponInfo.heatDissipationPerTick;

					if (currentHeat <= 0f) {

						currentHeat = 0f;

						overheated = false;
					}
						
				}


			}

			//MyAPIGateway.Utilities.ShowNotification("TIME: " + currentShootTime + " :: " + lastShootTime, 17, MyFontEnum.Red);


			if (currentShootTime != lastShootTime) {

				// test

				hitBool = false;

				MyEntitySubpart subpart1 = cubeBlock.GetSubpart("InteriorTurretBase1");
				MyEntitySubpart subpart2 = subpart1.GetSubpart("InteriorTurretBase2");;
				//MyAPIGateway.Utilities.ShowNotification("Dif: " + (currentShootTime - lastShootTime), 17, MyFontEnum.Blue);

				from = subpart2.WorldMatrix.Translation + subpart2.WorldMatrix.Forward * 2.5d;
				to = subpart2.WorldMatrix.Translation + subpart2.WorldMatrix.Forward * 3000d;

				LineD testRay = new LineD(from, to);

				List<MyLineSegmentOverlapResult<MyEntity>> result = new List<MyLineSegmentOverlapResult<MyEntity>>();

				MyGamePruningStructure.GetAllEntitiesInRay(ref testRay, result);


				foreach(var resultItem in result)
				{
					IMyCubeGrid grid = resultItem.Element as IMyCubeGrid;

					IMyDestroyableObject destroyableEntity = resultItem.Element as IMyDestroyableObject;

					if(grid != null)
					{
						IMySlimBlock slimblock;

						double hitd;

						Vector3D? resultVec = grid.GetLineIntersectionExactAll(ref testRay, out hitd, out slimblock);

						if(resultVec != null)
						{

							hitBool = true;

							toTarget = from + subpart2.WorldMatrix.Forward * hitd;

							if(!MyAPIGateway.Session.CreativeMode)
							{
								slimblock.DoDamage(beamWeaponInfo.damage * (currentHeat/beamWeaponInfo.maxHeat + 0.2f), MyStringHash.GetOrCompute("Laser"), false, default(MyHitInfo), cubeBlock.EntityId);
							}
							else
							{
								slimblock.DoDamage(beamWeaponInfo.damage * 1.2f, MyStringHash.GetOrCompute("Laser"), false, default(MyHitInfo), cubeBlock.EntityId);
							}
							//MyAPIGateway.Utilities.ShowNotification("" + s.BlockDefinition.Id.SubtypeId + " ::: " + resultItem.Distance, 17);
						}
					}
					if(destroyableEntity !=  null)
					{
						IMyEntity ent = (IMyEntity) destroyableEntity;
						double hitd = (from - ent.WorldMatrix.Translation).Length();

						toTarget = from + subpart2.WorldMatrix.Forward * hitd;

						hitBool = true;

						if(!MyAPIGateway.Session.CreativeMode)
						{
							destroyableEntity.DoDamage(beamWeaponInfo.damage * (currentHeat/beamWeaponInfo.maxHeat + 0.2f), MyStringHash.GetOrCompute("Laser"), false, default(MyHitInfo), cubeBlock.EntityId);
						}
						else
						{
							destroyableEntity.DoDamage(beamWeaponInfo.damage * 1.2f, MyStringHash.GetOrCompute("Laser"), false, default(MyHitInfo), cubeBlock.EntityId);
						}
					}

				}


				// test

				lastShootTime = currentShootTime;
				lastShootTimeTicks = ticks;

				currentHeat += beamWeaponInfo.heatPerTick;

				if (currentHeat > beamWeaponInfo.maxHeat) {
					currentHeat = beamWeaponInfo.maxHeat;

					overheated = true;
				}
			}

			if(ticks - lastShootTimeTicks < 3)
			{
				var beamcolor = Color.OrangeRed;
				var beamcolor_aux = Color.Khaki;
				var maincolor = new Vector4(beamcolor.X / 30, beamcolor.Y / 30, beamcolor.Z / 30, 1f);
				var auxcolor = new Vector4(beamcolor_aux.X / 30, beamcolor_aux.Y / 30, beamcolor_aux.Z / 30, 1f);
				var material = MyStringId.GetOrCompute("WeaponLaser");
				if(hitBool == false)
				{
					if(!MyAPIGateway.Utilities.IsDedicated)
					{
						if(!MyAPIGateway.Session.CreativeMode)
						{
							VRage.Game.MySimpleObjectDraw.DrawLine(from, to, material, ref auxcolor, 0.30f * (currentHeat/beamWeaponInfo.maxHeat + 0.2f));
							VRage.Game.MySimpleObjectDraw.DrawLine(from, to, material, ref maincolor, 1.0f * (currentHeat/beamWeaponInfo.maxHeat + 0.2f));
						}
						else
						{
							VRage.Game.MySimpleObjectDraw.DrawLine(from, to, material, ref auxcolor, 0.30f * 1.2f);
							VRage.Game.MySimpleObjectDraw.DrawLine(from, to, material, ref maincolor, 1.0f * 1.2f);
						}
					}
				}
				else
				{
					if(!MyAPIGateway.Utilities.IsDedicated)
					{
						if(!MyAPIGateway.Session.CreativeMode)
						{
							VRage.Game.MySimpleObjectDraw.DrawLine(from, toTarget, material, ref auxcolor, 0.30f * (currentHeat/beamWeaponInfo.maxHeat + 0.2f));
							VRage.Game.MySimpleObjectDraw.DrawLine(from, toTarget, material, ref maincolor, 1.0f * (currentHeat/beamWeaponInfo.maxHeat + 0.2f));
						}
						else
						{
							VRage.Game.MySimpleObjectDraw.DrawLine(from, toTarget, material, ref auxcolor, 0.30f * 1.2f);
							VRage.Game.MySimpleObjectDraw.DrawLine(from, toTarget, material, ref maincolor, 1.0f * 1.2f);
						}
					}
				}
			}

			if (chargesInInventory < beamWeaponInfo.keepAtCharge) {

				if (resourceSink.RequiredInputByType(electricityDefinition) != (beamWeaponInfo.powerUsage/efficiencyUpgrades)) {
					
					resourceSink.SetRequiredInputByType (electricityDefinition, (beamWeaponInfo.powerUsage/efficiencyUpgrades));

					setPowerConsumption = (beamWeaponInfo.powerUsage/efficiencyUpgrades);
					powerConsumption = (beamWeaponInfo.powerUsage/efficiencyUpgrades);

				} else {

					if (!functionalBlock.Enabled) {
						
						powerConsumption = 0.0001f;
					}
				}

				if (resourceSink.CurrentInputByType (electricityDefinition) == (beamWeaponInfo.powerUsage/efficiencyUpgrades)) {

					if (!overheated) {
						

						m_inventory.AddItems ((MyFixedPoint)(beamWeaponInfo.keepAtCharge - chargesInInventory), chargeObjectBuilders [damageUpgrades]);
					}
				}

			} else if(chargesInInventory > beamWeaponInfo.keepAtCharge) {
				
				m_inventory.RemoveItemsOfType ((MyFixedPoint)(chargesInInventory - beamWeaponInfo.keepAtCharge), chargeObjectBuilders [damageUpgrades]);

			} else  {
				
				if (setPowerConsumption != 0.0001f) {

					resourceSink.SetRequiredInputByType (electricityDefinition, 0.0001f);

					setPowerConsumption = 0.0001f;
					powerConsumption = 0.0001f;
				}
			}

			terminalBlock.RefreshCustomInfo ();

			ticks++;

		}

		public override void Close ()
		{

			if (m_inventory != null) {

				for (int i = 0; i < beamWeaponInfo.classes; i++) { 
					m_inventory.RemoveItemsOfType (m_inventory.GetItemAmount (chargeDefinitionIds[i]), chargeObjectBuilders[i]);
				}
			}

			base.Close ();
		}

		public override void MarkForClose ()
		{
			if (m_inventory != null) {

				for (int i = 0; i < beamWeaponInfo.classes; i++) { 
					m_inventory.RemoveItemsOfType (m_inventory.GetItemAmount (chargeDefinitionIds[i]), chargeObjectBuilders[i]);
				}
			}

			base.MarkForClose ();
		}

	}
}

