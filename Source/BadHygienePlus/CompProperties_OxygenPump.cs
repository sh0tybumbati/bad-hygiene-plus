using Verse;

namespace BadHygienePlus
{
    public class CompProperties_OxygenPump : CompProperties
    {
        public float o2ConsumptionPerTick = 0.02f;
        public float oxygenationRadius = 5f;

        public CompProperties_OxygenPump()
        {
            compClass = typeof(CompOxygenPump);
        }
    }
}
