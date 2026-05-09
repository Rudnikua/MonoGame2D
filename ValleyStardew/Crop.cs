using System.Collections.Generic;

namespace ValleyStardew {
    public enum CropType {
        Wheat,
        Carrot // Додали новий вид для майбутнього
    }

    // Структура, яка містить характеристики рослини
    public struct CropData {
        public string Name;
        public int SeedPrice;
        public int SellPrice;
    }

    public class Crop {
        // СТАТИЧНА БАЗА ДАНИХ. Тут ми налаштовуємо всі ціни гри!
        public static readonly Dictionary<CropType, CropData> Database = new Dictionary<CropType, CropData> {
            { CropType.Wheat, new CropData { Name = "Wheat", SeedPrice = 10, SellPrice = 15 } },
            { CropType.Carrot, new CropData { Name = "Carrot", SeedPrice = 12, SellPrice = 20 } }
        };

        public CropType Type;
        public int CurrentPhase;
        public int MaxPhase = 2;

        public Crop(CropType type) {
            Type = type;
            CurrentPhase = 0;
        }

        public void Grow() {
            if (CurrentPhase < MaxPhase) CurrentPhase++;
        }

        public bool IsReadyToHarvest() {
            return CurrentPhase == MaxPhase;
        }
    }
}