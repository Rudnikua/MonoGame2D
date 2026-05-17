using Microsoft.Xna.Framework;
using System;

namespace ValleyStardew.Engine {
    public class TimeManager {
        public int Day { get; private set; } = 1;
        public int Hour { get; private set; } = 6; 
        public int Minute { get; private set; } = 0; 

        public float CurrentDarkness { get; private set; } = 0f;

        public float RealSecondsPerHour { get; set; } = 2.0f;

        private float _timer = 0f;

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