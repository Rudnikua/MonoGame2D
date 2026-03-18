using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ValleyStardew;

namespace ValleyStardew {
    public class Player {
        public Vector2 Position;
        private Texture2D _texture;
        private float _speed = 5f;

        public void LoadContent(ContentManager content) {
            _texture = content.Load<Texture2D>("player");
        }

        // Передаємо карту в Update, щоб гравець міг дізнатися, де вода
        public void Update(Map map) {
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W)) Position.Y -= _speed;
            if (kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S)) Position.Y += _speed;
            if (kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A)) Position.X -= _speed;
            if (kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D)) Position.X += _speed;

            // --- ОБМЕЖЕННЯ (Колізії для ніг) ---
            if (_texture != null) {
                float feetHeight = map.TileSize;
                float feetY = Position.Y + (_texture.Height - feetHeight);

                float minX = map.TileSize;
                float minYfeet = map.TileSize;
                float maxX = (map.Width - 1) * map.TileSize - _texture.Width;
                float maxYfeet = (map.Height - 1) * map.TileSize - feetHeight;

                float clampedX = MathHelper.Clamp(Position.X, minX, maxX);
                float clampedYfeet = MathHelper.Clamp(feetY, minYfeet, maxYfeet);

                Position.X = clampedX;
                Position.Y = clampedYfeet - (_texture.Height - feetHeight);
            }
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (_texture != null)
                spriteBatch.Draw(_texture, Position, Color.White);
        }
    }
}