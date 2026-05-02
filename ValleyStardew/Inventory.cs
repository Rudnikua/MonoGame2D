using System.Collections.Generic;

namespace ValleyStardew {
    // 1. Створюємо перелік (Enum) усіх можливих інструментів
    public enum ToolType {
        Hand, // Рука (для збору врожаю)
        Hoe,  // Сапка (для оранки землі)
        Seed  // Насіння (для посадки)
    }

    public class Inventory {
        // --- ЕКОНОМІКА ТА РЕСУРСИ ---
        public int Money { get; set; } = 0;           // Гроші гравця
        public int SeedsCount { get; set; } = 1;      // Початкове насіння (можеш змінити на 10 для тесту)
        public int HarvestedCrops { get; set; } = 0;  // Кількість зібраного врожаю для продажу

        // --- ЛОГІКА ТУЛБАРУ ---
        // Список інструментів, які лежать у тулбарі
        public List<ToolType> Toolbar { get; private set; }

        // Індекс поточного вибраного слота (починається з 0)
        public int ActiveSlotIndex { get; private set; } = 0;

        public Inventory() {
            // При створенні інвентарю заповнюємо наш тулбар
            Toolbar = new List<ToolType> {
                ToolType.Hoe,   // Слот 0: Сапка (стандартно в руках на початку)
                ToolType.Seed,  // Слот 1: Насіння
                ToolType.Hand   // Слот 2: Рука
            };
        }

        // Зручна властивість, щоб завжди знати, що саме зараз тримає гравець
        public ToolType ActiveTool {
            get { return Toolbar[ActiveSlotIndex]; }
        }

        // Метод для перемикання слотів (наприклад, клавішами 1, 2, 3)
        public void SelectSlot(int index) {
            // Перевіряємо, чи існує такий слот, щоб гра не вилетіла
            if (index >= 0 && index < Toolbar.Count) {
                ActiveSlotIndex = index;
            }
        }
    }
}