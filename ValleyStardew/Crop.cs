namespace ValleyStardew {
    // Список усіх видів насіння у грі
    public enum CropType {
        Corn, // Наприклад, Ріпа (базова рослина)
        // Сюди потім додаси Tomato, Potato тощо
    }

    public class Crop {
        public CropType Type;
        public int CurrentPhase; // 0 = щойно посадили, 1 = паросток, 2 = готово
        public int MaxPhase = 2; // Максимальна фаза росту

        public Crop(CropType type) {
            Type = type;
            CurrentPhase = 0; // Завжди починаємо з насіння
        }

        // Метод, який буде викликатися, коли настає новий день
        public void Grow() {
            if (CurrentPhase < MaxPhase) {
                CurrentPhase++;
            }
        }

        public bool IsReadyToHarvest() {
            return CurrentPhase == MaxPhase;
        }
    }
}