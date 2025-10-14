using Verse;

namespace BadHygienePlus
{
    public class Building_HydrogenFuelCell : Building
    {
        private CompHydrogenFuelCell fuelCell;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            fuelCell = GetComp<CompHydrogenFuelCell>();
        }

        public override void TickRare()
        {
            base.TickRare();
            // Main logic is in CompHydrogenFuelCell
        }
    }
}
