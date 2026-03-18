using Microsoft.Xna.Framework;

namespace ValleyStardew {
    public class Camera {
        public Vector2 Position;

        // Матриця, яку ми віддамо в SpriteBatch
        public Matrix Transform { get; private set; }

        public void Update(Vector2 targetPosition, int screenWidth, int screenHeight) {
            // Плавне слідування
            Position = Vector2.Lerp(Position, targetPosition, 0.1f);

            // Створення матриці зсуву
            Transform = Matrix.CreateTranslation(
                -Position.X + (screenWidth / 2),
                -Position.Y + (screenHeight / 2),
                0f);
        }
    }
}