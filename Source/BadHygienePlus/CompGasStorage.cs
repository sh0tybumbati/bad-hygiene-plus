using Verse;

namespace BadHygienePlus
{
    public class CompGasStorage : ThingComp
    {
        private CompProperties_GasStorage Props => (CompProperties_GasStorage)props;

        private CompGasPipe gasPipe;
        public float storedGas = 0f;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // Get gas pipe - storage capacity is declared in the CompGasPipe properties
            gasPipe = parent.GetComp<CompGasPipe>();
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.IsHashIntervalTick(60))
                return;

            if (gasPipe == null || gasPipe.gasNet == null)
                return;

            // Storage is now handled by the GasNet through CompGasPipe's storage capacity
            // No need to manually transfer - the network manages it automatically
        }

        public float DrawGas(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float drawn = amount;
            if (drawn > storedGas)
                drawn = storedGas;

            storedGas -= drawn;
            return drawn;
        }

        public float AddGas(float amount)
        {
            if (amount <= 0f)
                return 0f;

            float space = Props.storageCapacity - storedGas;
            float added = amount;
            if (added > space)
                added = space;

            storedGas += added;
            return added;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref storedGas, "storedGas", 0f);
        }

        public override string CompInspectStringExtra()
        {
            float percentage = (storedGas / Props.storageCapacity) * 100f;
            return $"{Props.gasType} Storage: {storedGas:F1}/{Props.storageCapacity:F0} ({percentage:F0}%)";
        }
    }
}
