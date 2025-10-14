using Verse;

namespace BadHygienePlus
{
    public class CompProperties_Electrolyzer : CompProperties
    {
        public float waterConsumptionPerTick = 0.1f;
        public float h2ProductionPerTick = 0.2f;
        public float o2ProductionPerTick = 0.1f;
        public float h2StorageCapacity = 200f;
        public float o2StorageCapacity = 100f;

        public CompProperties_Electrolyzer()
        {
            compClass = typeof(CompElectrolyzer);
        }
    }
}
