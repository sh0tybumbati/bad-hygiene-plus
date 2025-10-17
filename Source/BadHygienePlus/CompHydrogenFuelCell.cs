using RimWorld;
using Verse;

namespace BadHygienePlus
{
    public class CompHydrogenFuelCell : ThingComp
    {
        private CompProperties_HydrogenFuelCell Props => (CompProperties_HydrogenFuelCell)props;

        private CompPowerTrader powerTrader;
        private CompGasPipe hydrogenPipe;
        private CompFlickable flickable;
        private CompBreakdownable breakdownable;

        public float h2Stored = 0f;
        private bool isGeneratingPower = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerTrader = parent.GetComp<CompPowerTrader>();
            flickable = parent.GetComp<CompFlickable>();
            breakdownable = parent.GetComp<CompBreakdownable>();

            // Get hydrogen gas pipe
            hydrogenPipe = parent.GetComp<CompGasPipe>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.IsHashIntervalTick(60))
                return;

            UpdatePowerGeneration();
        }

        private void UpdatePowerGeneration()
        {
            // Check if we can generate power
            bool canGenerate = (flickable == null || flickable.SwitchIsOn) &&
                             (breakdownable == null || !breakdownable.BrokenDown) &&
                             h2Stored > 0f;

            if (canGenerate)
            {
                // Consume hydrogen
                float consumed = Props.h2ConsumptionPerTick;
                if (h2Stored < consumed)
                    consumed = h2Stored;

                h2Stored -= consumed;
                isGeneratingPower = true;

                // Request more hydrogen from gas network
                if (hydrogenPipe != null && hydrogenPipe.gasNet != null && h2Stored < Props.h2StorageCapacity)
                {
                    float needed = Props.h2StorageCapacity - h2Stored;
                    float received = hydrogenPipe.gasNet.DrawGas(needed) ?? 0f;
                    h2Stored += received;
                    if (h2Stored > Props.h2StorageCapacity)
                        h2Stored = Props.h2StorageCapacity;
                }
            }
            else
            {
                isGeneratingPower = false;

                // Still try to fill from gas network when not generating
                if (hydrogenPipe != null && hydrogenPipe.gasNet != null && h2Stored < Props.h2StorageCapacity)
                {
                    float needed = Props.h2StorageCapacity - h2Stored;
                    float received = hydrogenPipe.gasNet.DrawGas(needed) ?? 0f;
                    h2Stored += received;
                    if (h2Stored > Props.h2StorageCapacity)
                        h2Stored = Props.h2StorageCapacity;
                }
            }

            // Update power output by controlling PowerOn state
            if (powerTrader != null)
            {
                // Turn power on/off based on whether we're generating
                // When off, the building won't produce power
                powerTrader.PowerOn = isGeneratingPower;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref h2Stored, "h2Stored", 0f);
            Scribe_Values.Look(ref isGeneratingPower, "isGeneratingPower", false);
        }

        public override string CompInspectStringExtra()
        {
            string text = "";

            // Status
            if (isGeneratingPower)
                text += "Power output: " + ((powerTrader?.Props.PowerConsumption ?? 0f) * -1).ToString("F0") + " W\n";
            else if (h2Stored <= 0f)
                text += "No hydrogen fuel\n";
            else if (flickable != null && !flickable.SwitchIsOn)
                text += "Switched off\n";
            else if (breakdownable != null && breakdownable.BrokenDown)
                text += "Broken down\n";

            // Fuel storage
            float percentage = (h2Stored / Props.h2StorageCapacity) * 100f;
            text += $"H2 Fuel: {h2Stored:F1}/{Props.h2StorageCapacity:F0} ({percentage:F0}%)\n";

            // Consumption rate and fuel remaining
            if (isGeneratingPower && Props.h2ConsumptionPerTick > 0f)
            {
                float consumptionPerDay = Props.h2ConsumptionPerTick * 60000f; // ticks per day
                text += $"Fuel consumption: {consumptionPerDay:F1}/day\n";

                float daysRemaining = h2Stored / consumptionPerDay;
                if (daysRemaining < 1f)
                {
                    float hoursRemaining = daysRemaining * 24f;
                    text += $"Fuel remaining: {hoursRemaining:F1} hours";
                }
                else
                {
                    text += $"Fuel remaining: {daysRemaining:F1} days";
                }
            }
            else if (!isGeneratingPower && h2Stored > 0f)
            {
                // Show potential runtime when not generating
                float consumptionPerDay = Props.h2ConsumptionPerTick * 60000f;
                float daysRemaining = h2Stored / consumptionPerDay;
                text += $"Potential runtime: {daysRemaining:F1} days";
            }

            return text.TrimEnd('\n');
        }
    }
}
