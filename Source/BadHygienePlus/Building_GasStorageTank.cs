using Verse;

namespace BadHygienePlus
{
    public class Building_GasStorageTank : Building
    {
        private CompGasStorage gasStorage;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            gasStorage = GetComp<CompGasStorage>();
        }

        public override void TickRare()
        {
            base.TickRare();
            // Main logic is in CompGasStorage
        }
    }
}
