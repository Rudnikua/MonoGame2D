using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ValleyStardew.Engine {
    public class Camera {
        public Vector2 Position;

        public float Zoom = 1f;

        public Matrix Transform { get; private set; }

        private int _previousScrollValue;

        public Camera() {
            _previousScrollValue = Mouse.GetState().ScrollWheelValue;
        }

        public void Update(Vector2 targetPosition, int screenWidth, int screenHeight) {
            // 1. Плавне слідування за гравцем
            Position = Vector2.Lerp(Position, targetPosition, 0.1f);

            MouseState mouseState = Mouse.GetState();

            if (mouseState.ScrollWheelValue > _previousScrollValue) {
                Zoom += 0.1f; // Приближаємо
            }
            else if (mouseState.ScrollWheelValue < _previousScrollValue) {
                Zoom -= 0.1f; // Віддаляємо
            }

            _previousScrollValue = mouseState.ScrollWheelValue;

            Zoom = MathHelper.Clamp(Zoom, 0.5f, 3f);
            // ---------------------------------------

            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                        Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
                        Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));
        }
    }
}