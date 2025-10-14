using Verse;

namespace BadHygienePlus
{
    public class Building_OxygenGenerator : Building
    {
        private CompOxygenGenerator oxygenGenerator;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            oxygenGenerator = GetComp<CompOxygenGenerator>();
        }

        public override void TickRare()
        {
            base.TickRare();
            // Main logic is in CompOxygenGenerator
        }
    }
}
