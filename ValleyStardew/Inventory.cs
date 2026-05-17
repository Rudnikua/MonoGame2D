using System.Collections.Generic;

namespace ValleyStardew {
    public enum ToolType { Hand, Hoe, Seed }

    public class Inventory {
        public int Money { get; set; } = 100;

        public Dictionary<CropType, int> Seeds { get; private set; }
        public Dictionary<CropType, int> HarvestedCrops { get; private set; }

        public CropType SelectedSeedType { get; set; } = CropType.Wheat;

        public List<ToolType> Toolbar { get; private set; }
        public int ActiveSlotIndex { get; private set; } = 0;
        public ToolType ActiveTool => Toolbar[ActiveSlotIndex];

        public Inventory() {
            Toolbar = new List<ToolType> { ToolType.Hoe, ToolType.Seed, ToolType.Hand };
            Seeds = new Dictionary<CropType, int>();
            HarvestedCrops = new Dictionary<CropType, int>();

            // На старті даємо 1 насінину Corn
            Seeds[CropType.Wheat] = 1;
        }

        public void SelectSlot(int index) {
            if (index >= 0 && index < Toolbar.Count) ActiveSlotIndex = index;
        }

        public void AddSeed(CropType type, int amount) {
            if (!Seeds.ContainsKey(type)) Seeds[type] = 0;
            Seeds[type] += amount;
        }

        public bool HasSeed(CropType type) {
            return Seeds.ContainsKey(type) && Seeds[type] > 0;
        }

        public void RemoveSeed(CropType type) {
            if (HasSeed(type)) Seeds[type]--;
        }

        public void AddHarvest(CropType type, int amount) {
            if (!HarvestedCrops.ContainsKey(type)) HarvestedCrops[type] = 0;
            HarvestedCrops[type] += amount;
        }

        public void CycleSeedType() {
            // Отримуємо масив усіх існуючих видів насіння
            CropType[] allTypes = (CropType[])System.Enum.GetValues(typeof(CropType));

            // Знаходимо, який індекс у поточного вибраного насіння
            int currentIndex = System.Array.IndexOf(allTypes, SelectedSeedType);

            // Беремо наступний індекс. Якщо дійшли до кінця - повертаємось на 0
            int nextIndex = (currentIndex + 1) % allTypes.Length;
            SelectedSeedType = allTypes[nextIndex];
        }
    }
}