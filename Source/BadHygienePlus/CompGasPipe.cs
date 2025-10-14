using Verse;

namespace BadHygienePlus
{
    public class CompProperties_GasPipe : CompProperties
    {
        public GasType gasType;
        public float storageCapacity = 0f;  // 0 for pipes, >0 for storage

        public CompProperties_GasPipe()
        {
            compClass = typeof(CompGasPipe);
        }
    }

    public class CompGasPipe : ThingComp
    {
        public GasNet gasNet;

        private CompProperties_GasPipe Props => (CompProperties_GasPipe)props;

        public GasType GasType => Props.gasType;
        public float StorageCapacity => Props.storageCapacity;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                // Join or create network
                var manager = Current.Game.GetComponent<GasNetManager>();
                if (manager != null)
                {
                    gasNet = manager.GetOrCreateNetwork(parent.Map, GasType, this);
                    gasNet.RegisterComp(this);
                }
            }
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);

            var manager = Current.Game?.GetComponent<GasNetManager>();
            if (manager != null && gasNet != null)
            {
                manager.NotifyCompDestroyed(this);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            // Note: gasNet is rebuilt on load, don't need to save it
        }

        public override string CompInspectStringExtra()
        {
            if (gasNet == null) return "Not connected to gas network";

            string gasName = GasType == GasType.Hydrogen ? "H2" : "O2";
            return $"{gasName} network: {gasNet.StoredGas:F1}/{gasNet.MaxStorage:F0}";
        }
    }
}
