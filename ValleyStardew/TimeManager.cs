using Microsoft.Xna.Framework;
using System;

namespace ValleyStardew {
    public class TimeManager {
        public int Day { get; private set; } = 1;
        public int Hour { get; private set; } = 6; // Стартуємо о 6:00
        public int Minute { get; private set; } = 0; // Хвилини завжди 0, бо ми додаємо по годині

        // Додай цю змінну до інших (наприклад, біля _timer)
        public float CurrentDarkness { get; private set; } = 0f;

        // --- НАЛАШТУВАННЯ ШВИДКОСТІ ---
        // Скільки реальних секунд триває 1 ігрова година.
        // Зміни це значення на більше (наприклад 5.0f), щоб час ішов ще повільніше.
        public float RealSecondsPerHour { get; set; } = 2.0f;

        private float _timer = 0f;

        // Подія, яка викликається о 6:00 ранку
        public Action OnNewDay;

        public void Update(GameTime gameTime) {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Рахуємо прогрес поточної години (від 0.0 до 1.0)
            float hourProgress = _timer / RealSecondsPerHour;

            // Оновлюємо плавне освітлення КОЖНОГО КАДРУ
            UpdateDarkness(hourProgress);

            // Коли година закінчується
            if (_timer >= RealSecondsPerHour) {
                _timer -= RealSecondsPerHour;

                Hour++;

                if (Hour >= 24) {
                    Hour = 0;
                    Day++;
                }

                if (Hour == 6) {
                    OnNewDay?.Invoke();
                }
            }
        }

        public string GetTimeString() {
            // Виводимо час у форматі "06:00", "23:00", "00:00"
            return $"{Hour:D2}:00";
        }

        // Метод для визначення прозорості нічного неба
        private void UpdateDarkness(float hourProgress) {
            float startDarkness = 0f;
            float endDarkness = 0f;

            // --- НАЛАШТУВАННЯ ОСВІТЛЕННЯ ПО ГОДИНАХ ---

            // З 6:00 до 17:00 (День - повністю світло)
            if (Hour >= 6 && Hour < 17) {
                startDarkness = 0f;
                endDarkness = 0f;
            }
            // О 17:00 починає злегка темніти
            else if (Hour == 17) {
                startDarkness = 0f;
                endDarkness = 0.2f;
            }
            // О 18:00 темніє сильніше
            else if (Hour == 18) {
                startDarkness = 0.2f;
                endDarkness = 0.4f;
            }
            // О 19:00 настає ніч
            else if (Hour == 19) {
                startDarkness = 0.4f;
                endDarkness = 0.65f;
            }
            // З 20:00 до 03:00 (Глибока ніч)
            else if (Hour >= 20 || Hour <= 3) {
                startDarkness = 0.65f;
                endDarkness = 0.65f;
            }
            // О 04:00 починає світлішати
            else if (Hour == 4) {
                startDarkness = 0.65f;
                endDarkness = 0.3f;
            }
            // О 05:00 світанок (до 6:00 стане повністю світло)
            else if (Hour == 5) {
                startDarkness = 0.3f;
                endDarkness = 0f;
            }

            // Плавний перехід між початковою і кінцевою темрявою поточної години
            CurrentDarkness = MathHelper.Lerp(startDarkness, endDarkness, hourProgress);
        }
    }
}