using Verse;

namespace BadHygienePlus
{
    public class Building_OxygenPump : Building
    {
        private CompOxygenPump oxygenPump;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            oxygenPump = GetComp<CompOxygenPump>();
        }

        public override void TickRare()
        {
            base.TickRare();
            // Main logic is in CompOxygenPump
        }
    }
}
