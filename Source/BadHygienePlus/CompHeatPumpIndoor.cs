using RimWorld;
using Verse;
using System.Collections.Generic;
using UnityEngine;

namespace BadHygienePlus
{
    /// <summary>
    /// Heat pump indoor unit that automatically switches between heating and cooling modes
    /// Works alongside DBH's CompAirconUnit without inheritance
    /// </summary>
    [StaticConstructorOnStartup]
    public class CompHeatPumpIndoor : ThingComp
    {
        private static readonly Texture2D HeatingIcon = ContentFinder<Texture2D>.Get("UI/Commands/TempLower", false) ?? BaseContent.BadTex;
        private static readonly Texture2D CoolingIcon = ContentFinder<Texture2D>.Get("UI/Commands/TempRaise", false) ?? BaseContent.BadTex;

        private const float MODE_THRESHOLD = 2f; // Switch mode when 2°C away from target
        private const float MIN_HEATING_OUTDOOR_TEMP = -25f; // -25°C = -13°F

        private bool isHeating = false;
        private CompTempControl tempControl;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            tempControl = parent.GetComp<CompTempControl>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (parent.IsHashIntervalTick(60)) // Check every 60 ticks (1 second)
            {
                UpdateHeatPumpMode();
            }
        }

        private void UpdateHeatPumpMode()
        {
            if (tempControl == null)
                return;

            Room room = parent.GetRoom(RegionType.Set_Passable);
            if (room == null)
                return;

            float roomTemp = room.Temperature;
            float targetTemp = tempControl.targetTemperature;
            float outdoorTemp = parent.Map.mapTemperature.OutdoorTemp;

            // Determine if we should be heating or cooling
            bool shouldHeat = roomTemp < (targetTemp - MODE_THRESHOLD);
            bool shouldCool = roomTemp > (targetTemp + MODE_THRESHOLD);

            // Check if outdoor temperature allows heating
            bool canHeat = outdoorTemp >= MIN_HEATING_OUTDOOR_TEMP;

            // Switch modes as needed
            if (shouldCool && isHeating)
            {
                isHeating = false;
            }
            else if (shouldHeat && !isHeating && canHeat)
            {
                isHeating = true;
            }
            else if (isHeating && !canHeat)
            {
                // Disable heating if outdoor temp too low
                isHeating = false;
            }

            // When heating, push heat into the room
            if (isHeating && canHeat)
            {
                float heatPushRate = 21f; // Same as cooling rate
                GenTemperature.PushHeat(parent, heatPushRate);
            }
            // Cooling is handled by DBH's CompAirconUnit
        }

        public bool IsHeating => isHeating;

        public bool CanHeat
        {
            get
            {
                if (parent?.Map == null)
                    return false;
                float outdoorTemp = parent.Map.mapTemperature.OutdoorTemp;
                return outdoorTemp >= MIN_HEATING_OUTDOOR_TEMP;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isHeating, "isHeating", false);
        }

        public override string CompInspectStringExtra()
        {
            string result = "";

            // Current mode
            string mode = isHeating ? "Heating" : "Cooling";
            result += $"Mode: {mode}\n";

            // Room and target temperatures
            Room room = parent.GetRoom(RegionType.Set_Passable);
            if (room != null && tempControl != null)
            {
                float roomTemp = room.Temperature;
                float targetTemp = tempControl.targetTemperature;
                result += $"Room: {roomTemp.ToStringTemperature()} / Target: {targetTemp.ToStringTemperature()}\n";
            }

            // Outdoor temperature
            if (parent?.Map != null)
            {
                float outdoorTemp = parent.Map.mapTemperature.OutdoorTemp;
                result += $"Outdoor: {outdoorTemp.ToStringTemperature()}";

                // Show warning if cannot heat
                if (room != null && tempControl != null &&
                    room.Temperature < (tempControl.targetTemperature - MODE_THRESHOLD) && !CanHeat)
                {
                    result += $"\nHeating unavailable below {MIN_HEATING_OUTDOOR_TEMP.ToStringTemperature()} outdoor";
                }
            }

            return result;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            // Add mode indicator gizmo
            Command_Action modeIndicator = new Command_Action
            {
                defaultLabel = isHeating ? "Heating" : "Cooling",
                defaultDesc = isHeating
                    ? "Heat pump is in heating mode. Outdoor unit absorbing heat from outside air."
                    : "Heat pump is in cooling mode. Outdoor unit exhausting heat outside.",
                icon = isHeating ? HeatingIcon : CoolingIcon,
                action = delegate { } // Read-only indicator
            };

            // Color code the icon
            if (isHeating)
            {
                modeIndicator.defaultIconColor = new Color(1f, 0.5f, 0.2f); // Orange for heating
            }
            else
            {
                modeIndicator.defaultIconColor = new Color(0.4f, 0.7f, 1f); // Blue for cooling
            }

            // Show disabled if heating but can't heat
            if (isHeating && !CanHeat)
            {
                modeIndicator.Disable("Outdoor temperature too low for heating");
            }

            yield return modeIndicator;
        }
    }
}
