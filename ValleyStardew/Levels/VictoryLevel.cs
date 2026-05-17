using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ValleyStardew {
    public class VictoryLevel : Level {
        private Texture2D _background;
        private Rectangle _screenRect;

        public VictoryLevel(Texture2D background, int screenWidth, int screenHeight) {
            _background = background;
            _screenRect = new Rectangle(0, 0, screenWidth, screenHeight);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            if (_background != null) {
                spriteBatch.Draw(_background, _screenRect, Color.White);
            }
        }
    }
}