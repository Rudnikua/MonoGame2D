using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace ValleyStardew {
    public class Map {
        public int TileSize { get; private set; } = 32;
        public int Width { get; private set; } = 50;
        public int Height { get; private set; } = 50;

        private int[,] _tileMap;
        private Texture2D _grassTexture, _dirtTexture, _waterTexture;

        public Map() {
            // Генерація карти при створенні об'єкта
            _tileMap = new int[Height, Width];
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    _tileMap[y, x] = 0; // Трава
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        _tileMap[y, x] = 2; // Вода по краях
                }
            }
        }

        public void LoadContent(ContentManager content) {
            _grassTexture = content.Load<Texture2D>("grass");
            _dirtTexture = content.Load<Texture2D>("dirt");
            _waterTexture = content.Load<Texture2D>("water");
        }

        public void Draw(SpriteBatch spriteBatch) {
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    int tileType = _tileMap[y, x];
                    Vector2 pos = new Vector2(x * TileSize, y * TileSize);

                    Texture2D tex = _grassTexture;
                    if (tileType == 1) tex = _dirtTexture;
                    else if (tileType == 2) tex = _waterTexture;

                    spriteBatch.Draw(tex, pos, Color.White);
                }
            }
        }
    }
}