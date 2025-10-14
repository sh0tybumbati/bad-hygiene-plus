using Verse;

namespace BadHygienePlus
{
    public class Building_ElectrolysisChamber : Building
    {
        private CompElectrolyzer electrolyzer;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            electrolyzer = GetComp<CompElectrolyzer>();
        }

        public override void TickRare()
        {
            base.TickRare();
            // Main logic is in CompElectrolyzer
        }
    }
}
