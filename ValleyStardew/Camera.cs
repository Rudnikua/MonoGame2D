using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ValleyStardew {
    public class Camera {
        public Vector2 Position;

        // Додаємо змінну для зуму (1f означає 100% розмір)
        public float Zoom = 1f;

        public Matrix Transform { get; private set; }

        // Змінна для зберігання попереднього стану коліщатка миші
        private int _previousScrollValue;

        public Camera() {
            // Запам'ятовуємо стан коліщатка при запуску гри
            _previousScrollValue = Mouse.GetState().ScrollWheelValue;
        }

        public void Update(Vector2 targetPosition, int screenWidth, int screenHeight) {
            // 1. Плавне слідування за гравцем
            Position = Vector2.Lerp(Position, targetPosition, 0.1f);

            // --- 2. ЛОГІКА ЗУМУ (КОЛІЩАТКО МИШІ) ---
            MouseState mouseState = Mouse.GetState();

            // Якщо покрутили вгору (нове значення більше за старе)
            if (mouseState.ScrollWheelValue > _previousScrollValue) {
                Zoom += 0.1f; // Приближаємо
            }
            // Якщо покрутили вниз
            else if (mouseState.ScrollWheelValue < _previousScrollValue) {
                Zoom -= 0.1f; // Віддаляємо
            }

            // Оновлюємо старе значення для наступного кадру
            _previousScrollValue = mouseState.ScrollWheelValue;

            // Обмежуємо зум, щоб гравець не міг віддалити в космос або наблизити до пікселів
            // Мінімум 0.5 (віддалення вдвічі), Максимум 3.0 (наближення в 3 рази)
            Zoom = MathHelper.Clamp(Zoom, 0.5f, 3f);
            // ---------------------------------------

            // 3. СТВОРЕННЯ МАТРИЦІ (Зсув -> Масштаб -> Зсув у центр екрана)
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));
        }
    }
}