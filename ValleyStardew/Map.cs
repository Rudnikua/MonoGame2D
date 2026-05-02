using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace ValleyStardew {
    public class Map {
        public int TileSize { get; private set; } = 32;
        public int Width { get; private set; } = 50;
        public int Height { get; private set; } = 50;

        private int[,] _tileMap;
        public Dictionary<Point, Crop> PlantedCrops {get; private set;} = new Dictionary<Point, Crop>();
        private Texture2D _cropPhase0, _cropPhase1, _cropPhase2;
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

            // Corn 3 phases
            _cropPhase0 = content.Load<Texture2D>("1Stage_Seed"); // Зерна на землі
            _cropPhase1 = content.Load<Texture2D>("2Stage_Seed"); // sprout (паросток)
            _cropPhase2 = content.Load<Texture2D>("3Stage_Seed"); // Готовий врожай
        }

        // Метод для взаємодії із землею
        public void InteractWithTile(int x, int y, Inventory inventory) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            int currentTile = _tileMap[y, x];
            Point tilePoint = new Point(x, y); // Створюємо точку для перевірки у словнику
            ToolType activeTool = inventory.ActiveTool;

            // 1. САПКА (Оремо землю)
            if (activeTool == ToolType.Hoe) {
                if (currentTile == 0) _tileMap[y, x] = 1;
            }
            // 2. НАСІННЯ (Садимо)
            else if (activeTool == ToolType.Seed) {
                // Якщо земля зорана (1), насіння є в інвентарі, і на цій клітинці ЩЕ НЕМАЄ рослини
                if (currentTile == 1 && inventory.SeedsCount > 0 && !PlantedCrops.ContainsKey(tilePoint)) {
                    // Садимо базову рослину (Corn)
                    PlantedCrops.Add(tilePoint, new Crop(CropType.Corn));
                    inventory.SeedsCount--;
                }
            }
            // 3. РУКА (Збираємо врожай)
            else if (activeTool == ToolType.Hand) {
                // Якщо на цій клітинці є рослина
                if (PlantedCrops.ContainsKey(tilePoint)) {
                    Crop targetCrop = PlantedCrops[tilePoint];

                    // Збираємо ТІЛЬКИ якщо вона виросла (CurrentPhase == 2)
                    if (targetCrop.IsReadyToHarvest()) {
                        PlantedCrops.Remove(tilePoint); // Видаляємо з карти
                        inventory.HarvestedCrops++;     // Додаємо в інвентар
                    }
                }
            }
        }
        public void Draw(SpriteBatch spriteBatch) {
            // 1. Спочатку малюємо саму землю (фон)
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

            // 2. Тепер малюємо всі рослини ПОВЕРХ землі
            foreach (var item in PlantedCrops) {
                Point pt = item.Key;
                Crop crop = item.Value;
                Vector2 pos = new Vector2(pt.X * TileSize, pt.Y * TileSize);

                Texture2D cropTex = _cropPhase0; // За замовчуванням насіння
                if (crop.CurrentPhase == 1) cropTex = _cropPhase1;
                else if (crop.CurrentPhase == 2) cropTex = _cropPhase2;

                spriteBatch.Draw(cropTex, pos, Color.White);
            }
        }
    }
}