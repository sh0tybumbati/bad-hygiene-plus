using UnityEngine;
using Verse;

namespace BadHygienePlus
{
    public class Building_HydrogenFuelCell : Building
    {
        private CompHydrogenFuelCell fuelCell;
        private static readonly Material BatteryBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0f, 1f)); // Magenta
        private static readonly Material BatteryBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f)); // Dark gray

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            fuelCell = GetComp<CompHydrogenFuelCell>();
        }

        public override void Draw()
        {
            base.Draw();

            if (fuelCell != null)
            {
                // Draw hydrogen fuel bar (like battery charge bar)
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = DrawPos + Vector3.up * 0.1f;
                r.size = new Vector2(0.55f, 0.08f);
                r.fillPercent = fuelCell.h2Stored / fuelCell.Props.h2StorageCapacity;
                r.filledMat = BatteryBarFilledMat;
                r.unfilledMat = BatteryBarUnfilledMat;
                r.margin = 0.15f;
                Rotation rotation = Rotation;
                rotation.Rotate(RotationDirection.Clockwise);
                r.rotation = rotation;
                GenDraw.DrawFillableBar(r);
            }
        }

        public override void TickRare()
        {
            base.TickRare();
            // Main logic is in CompHydrogenFuelCell
        }
    }
}
