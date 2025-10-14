using Verse;

namespace BadHygienePlus
{
    public class CompProperties_HydrogenFuelCell : CompProperties
    {
        public float h2ConsumptionPerTick = 0.05f;
        public float h2StorageCapacity = 40f;

        public CompProperties_HydrogenFuelCell()
        {
            compClass = typeof(CompHydrogenFuelCell);
        }
    }
}
