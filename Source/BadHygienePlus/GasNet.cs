using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BadHygienePlus
{
    /// <summary>
    /// Manages a network of connected gas pipes and storage
    /// </summary>
    public class GasNet
    {
        public GasType gasType;
        public List<CompGasPipe> connectedComps = new List<CompGasPipe>();

        private float storedGas = 0f;
        private float maxStorage = 0f;

        public GasNet(GasType type)
        {
            gasType = type;
        }

        public void RegisterComp(CompGasPipe comp)
        {
            if (!connectedComps.Contains(comp))
            {
                connectedComps.Add(comp);
                RecalculateStorage();
            }
        }

        public void DeregisterComp(CompGasPipe comp)
        {
            if (connectedComps.Contains(comp))
            {
                connectedComps.Remove(comp);
                RecalculateStorage();
            }
        }

        private void RecalculateStorage()
        {
            maxStorage = 0f;
            foreach (var comp in connectedComps)
            {
                maxStorage += comp.StorageCapacity;
            }
        }

        public float AvailableStorage => maxStorage - storedGas;
        public float StoredGas => storedGas;
        public float MaxStorage => maxStorage;

        /// <summary>
        /// Add gas to the network. Returns amount actually added.
        /// </summary>
        public float AddGas(float amount)
        {
            if (amount <= 0f) return 0f;

            float spaceAvailable = AvailableStorage;
            float toAdd = amount > spaceAvailable ? spaceAvailable : amount;
            storedGas += toAdd;
            return toAdd;
        }

        /// <summary>
        /// Draw gas from the network. Returns amount actually drawn (can be null if no gas available).
        /// </summary>
        public float? DrawGas(float amount)
        {
            if (amount <= 0f || storedGas <= 0f) return null;

            float toDraw = amount > storedGas ? storedGas : amount;
            storedGas -= toDraw;
            return toDraw;
        }

        public void Tick()
        {
            // Clean up dead comps
            connectedComps.RemoveAll(c => c == null || c.parent == null || c.parent.Destroyed);

            // Rebuild network if needed
            if (connectedComps.Count == 0)
            {
                // Network is empty, should be removed by manager
                return;
            }
        }
    }
}
