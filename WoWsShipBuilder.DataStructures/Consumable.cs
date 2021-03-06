using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public class Consumable
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public string DescId { get; set; }
        public string Group { get; set; }
        public string IconId { get; set; }
        public int NumConsumables { get; set; }
        public float ReloadTime { get; set; }
        public float WorkTime { get; set; }
        public string ConsumableVariantName { get; set; }
        public string PlaneName { get; set; }
        public Dictionary<string, float> Modifiers { get; set; }
    }
}
