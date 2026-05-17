using System.Collections.Generic;

namespace ValleyStardew {
    public enum CropType {
        Wheat,
        Carrot,
        Tomato 
    }

    public struct CropData {
        public string Name;
        public int SeedPrice;
        public int SellPrice;
        public int MaxPhase; 
        public float SeedDropChance; 
    }

    public class Crop {
        public static readonly Dictionary<CropType, CropData> Database = new Dictionary<CropType, CropData> {
            // Пшениця: росте 3 дні, насіння не повертає
            { CropType.Wheat, new CropData { Name = "Wheat", SeedPrice = 10, SellPrice = 15, MaxPhase = 3, SeedDropChance = 0f } },
            
            // Морква: росте 5 днів, насіння не повертає
            { CropType.Carrot, new CropData { Name = "Carrot", SeedPrice = 12, SellPrice = 20, MaxPhase = 5, SeedDropChance = 0f } },
            
            // Помідор: росте 6 днів, 30% шанс (0.3f) повернути насіння при зборі
            { CropType.Tomato, new CropData { Name = "Tomato", SeedPrice = 15, SellPrice = 25, MaxPhase = 6, SeedDropChance = 0.3f } }
        };

        public CropType Type;
        public int CurrentPhase;
        public int MaxPhase;

        public Crop(CropType type) {
            Type = type;
            CurrentPhase = 0;
            MaxPhase = Database[type].MaxPhase;
        }

        public void Grow() {
            if (CurrentPhase < MaxPhase) CurrentPhase++;
        }

        public bool IsReadyToHarvest() {
            return CurrentPhase == MaxPhase;
        }
    }
}