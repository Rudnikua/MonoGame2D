using Microsoft.Xna.Framework;

namespace ValleyStardew {
    public class TransformComponent : Component {
        public Vector2 Position;

        public TransformComponent(Vector2 startPosition) {
            Position = startPosition;
        }
    }
}