using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BadHygienePlus
{
    /// <summary>
    /// World component that manages all gas networks across all maps
    /// </summary>
    public class GasNetManager : GameComponent
    {
        private Dictionary<Map, List<GasNet>> gasNetsByMap = new Dictionary<Map, List<GasNet>>();

        public GasNetManager(Game game)
        {
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            // Tick all networks every 60 ticks (1 second)
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                foreach (var mapNets in gasNetsByMap.Values)
                {
                    foreach (var net in mapNets)
                    {
                        net.Tick();
                    }
                    // Remove empty networks
                    mapNets.RemoveAll(n => n.connectedComps.Count == 0);
                }
            }
        }

        public GasNet GetOrCreateNetwork(Map map, GasType gasType, CompGasPipe initialComp)
        {
            if (!gasNetsByMap.ContainsKey(map))
            {
                gasNetsByMap[map] = new List<GasNet>();
            }

            var mapNets = gasNetsByMap[map];

            // Find adjacent networks of the same type
            List<GasNet> adjacentNets = new List<GasNet>();
            foreach (IntVec3 adjCell in GenAdj.CellsAdjacentCardinal(initialComp.parent))
            {
                if (!adjCell.InBounds(map)) continue;

                var thingsAtCell = map.thingGrid.ThingsListAt(adjCell);
                foreach (Thing thing in thingsAtCell)
                {
                    var gasComp = thing.TryGetComp<CompGasPipe>();
                    if (gasComp != null && gasComp.GasType == gasType && gasComp.gasNet != null)
                    {
                        if (!adjacentNets.Contains(gasComp.gasNet))
                        {
                            adjacentNets.Add(gasComp.gasNet);
                        }
                    }
                }
            }

            // If no adjacent networks, create new one
            if (adjacentNets.Count == 0)
            {
                var newNet = new GasNet(gasType);
                mapNets.Add(newNet);
                return newNet;
            }

            // If one adjacent network, join it
            if (adjacentNets.Count == 1)
            {
                return adjacentNets[0];
            }

            // Multiple adjacent networks - merge them
            var primaryNet = adjacentNets[0];
            for (int i = 1; i < adjacentNets.Count; i++)
            {
                var netToMerge = adjacentNets[i];
                // Move all comps from netToMerge to primaryNet
                foreach (var comp in netToMerge.connectedComps.ToList())
                {
                    netToMerge.DeregisterComp(comp);
                    primaryNet.RegisterComp(comp);
                    comp.gasNet = primaryNet;
                }
                mapNets.Remove(netToMerge);
            }

            return primaryNet;
        }

        public void NotifyCompDestroyed(CompGasPipe comp)
        {
            if (comp.gasNet != null)
            {
                comp.gasNet.DeregisterComp(comp);

                // Check if network needs to be split
                RebuildNetworksAroundPosition(comp.parent.Map, comp.parent.Position, comp.GasType);
            }
        }

        private void RebuildNetworksAroundPosition(Map map, IntVec3 pos, GasType gasType)
        {
            // Find all gas comps adjacent to this position
            HashSet<CompGasPipe> adjacentComps = new HashSet<CompGasPipe>();

            foreach (IntVec3 adjCell in GenAdj.CellsAdjacentCardinal(pos, Rot4.North, IntVec2.One))
            {
                if (!adjCell.InBounds(map)) continue;

                var thingsAtCell = map.thingGrid.ThingsListAt(adjCell);
                foreach (Thing thing in thingsAtCell)
                {
                    var gasComp = thing.TryGetComp<CompGasPipe>();
                    if (gasComp != null && gasComp.GasType == gasType && !gasComp.parent.Destroyed)
                    {
                        adjacentComps.Add(gasComp);
                    }
                }
            }

            if (adjacentComps.Count == 0) return;

            // Rebuild networks using flood fill
            HashSet<CompGasPipe> visited = new HashSet<CompGasPipe>();

            foreach (var comp in adjacentComps)
            {
                if (visited.Contains(comp)) continue;

                // Create new network for this group
                var newNet = new GasNet(gasType);
                if (!gasNetsByMap.ContainsKey(map))
                {
                    gasNetsByMap[map] = new List<GasNet>();
                }
                gasNetsByMap[map].Add(newNet);

                // Flood fill to find all connected comps
                Queue<CompGasPipe> toProcess = new Queue<CompGasPipe>();
                toProcess.Enqueue(comp);
                visited.Add(comp);

                while (toProcess.Count > 0)
                {
                    var current = toProcess.Dequeue();

                    // Remove from old network
                    if (current.gasNet != null)
                    {
                        current.gasNet.DeregisterComp(current);
                    }

                    // Add to new network
                    current.gasNet = newNet;
                    newNet.RegisterComp(current);

                    // Check neighbors
                    foreach (IntVec3 adjCell in GenAdj.CellsAdjacentCardinal(current.parent))
                    {
                        if (!adjCell.InBounds(map)) continue;

                        var thingsAtCell = map.thingGrid.ThingsListAt(adjCell);
                        foreach (Thing thing in thingsAtCell)
                        {
                            var neighborComp = thing.TryGetComp<CompGasPipe>();
                            if (neighborComp != null &&
                                neighborComp.GasType == gasType &&
                                !neighborComp.parent.Destroyed &&
                                !visited.Contains(neighborComp))
                            {
                                visited.Add(neighborComp);
                                toProcess.Enqueue(neighborComp);
                            }
                        }
                    }
                }
            }
        }
    }
}
