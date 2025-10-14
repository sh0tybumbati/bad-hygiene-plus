using Verse;

namespace BadHygienePlus
{
    public class CompProperties_OxygenGenerator : CompProperties
    {
        public float o2ConsumptionPerTick = 0.1f;

        public CompProperties_OxygenGenerator()
        {
            compClass = typeof(CompOxygenGenerator);
        }
    }
}
