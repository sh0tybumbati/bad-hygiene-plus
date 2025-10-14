using Verse;

namespace BadHygienePlus
{
    public class CompProperties_GasStorage : CompProperties
    {
        public string gasType = "Hydrogen"; // "Hydrogen" or "Oxygen"
        public float storageCapacity = 100f;

        public CompProperties_GasStorage()
        {
            compClass = typeof(CompGasStorage);
        }
    }
}
