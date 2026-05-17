using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System; 

namespace ValleyStardew {
    public class Map {
        public int TileSize { get; private set; } = 32;
        public int Width { get; private set; } = 50;
        public int Height { get; private set; } = 50;

        private int[,] _tileMap;
        public Dictionary<Point, Crop> PlantedCrops { get; private set; } = new Dictionary<Point, Crop>();

        // Спрайти для всіх рослин
        private Texture2D _wheatPhase0, _wheatPhase1, _wheatPhase2;
        private Texture2D _carrotPhase0, _carrotPhase1, _carrotPhase2;
        private Texture2D _tomatoPhase0, _tomatoPhase1, _tomatoPhase2;

        private Texture2D _grassTexture, _dirtTexture, _waterTexture;
        private Random _random = new Random(); // Генератор для шансу насіння

        public Map() {
            _tileMap = new int[Height, Width];
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    _tileMap[y, x] = 0;
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        _tileMap[y, x] = 2;
                }
            }
        }

        public void LoadContent(ContentManager content) {
            _grassTexture = content.Load<Texture2D>("grass");
            _dirtTexture = content.Load<Texture2D>("dirt");
            _waterTexture = content.Load<Texture2D>("water");

            // Пшениця
            _wheatPhase0 = content.Load<Texture2D>("Wheat_Stage1");
            _wheatPhase1 = content.Load<Texture2D>("Wheat_Stage2");
            _wheatPhase2 = content.Load<Texture2D>("Wheat_Stage3");

            // Морква 
            _carrotPhase0 = content.Load<Texture2D>("Carrot_Stage1");
            _carrotPhase1 = content.Load<Texture2D>("Carrot_Stage2");
            _carrotPhase2 = content.Load<Texture2D>("Carrot_Stage3");

            // Помідори
            _tomatoPhase0 = content.Load<Texture2D>("Tomato_Stage1");
            _tomatoPhase1 = content.Load<Texture2D>("Tomato_Stage2");
            _tomatoPhase2 = content.Load<Texture2D>("Tomato_Stage3");
        }

        public void InteractWithTile(int x, int y, Inventory inventory) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            int currentTile = _tileMap[y, x];
            Point tilePoint = new Point(x, y);
            ToolType activeTool = inventory.ActiveTool;

            // 1. САПКА
            if (activeTool == ToolType.Hoe) {
                if (currentTile == 0) _tileMap[y, x] = 1;
                SoundManager.DirtWork.Play();
            }
            // 2. НАСІННЯ
            else if (activeTool == ToolType.Seed) {
                if (currentTile == 1 && inventory.HasSeed(inventory.SelectedSeedType) && !PlantedCrops.ContainsKey(tilePoint)) {
                    PlantedCrops.Add(tilePoint, new Crop(inventory.SelectedSeedType));
                    SoundManager.PlantSeed.Play();
                    inventory.RemoveSeed(inventory.SelectedSeedType);
                }
            }
            // 3. РУКА (ЗБІР ВРОЖАЮ + ШАНС НАСІННЯ)
            else if (activeTool == ToolType.Hand) {
                if (PlantedCrops.ContainsKey(tilePoint)) {
                    Crop targetCrop = PlantedCrops[tilePoint];
                    if (targetCrop.IsReadyToHarvest()) {
                        PlantedCrops.Remove(tilePoint);

                        _tileMap[y, x] = 0; 

                        SoundManager.HandCollect.Play();
                        inventory.AddHarvest(targetCrop.Type, 1);

                        Texture2D particleTexture = null;
                        
                        switch (targetCrop.Type) {
                            case CropType.Wheat:
                                particleTexture = _wheatPhase2; 
                                break;
                            case CropType.Carrot:
                                particleTexture = _carrotPhase2; 
                                break;
                            case CropType.Tomato:
                                particleTexture = _tomatoPhase2; 
                                break;
                        }

                        Vector2 centerOfTile = new Vector2(
                            x * TileSize + (TileSize / 2), 
                            y * TileSize + (TileSize / 2)
                        );

                        if (particleTexture != null) {
                            ParticleManager.AddHarvestParticles(centerOfTile, particleTexture); 
                        }

                        float dropChance = Crop.Database[targetCrop.Type].SeedDropChance;
                        if (dropChance > 0f) {
                            if (_random.NextDouble() <= dropChance) {
                                inventory.AddSeed(targetCrop.Type, 1);
                            }
                        }
                    }
                }
            }
        }

        public Color GetTileColorAt(Vector2 worldPosition) {
            // Перераховуємо піксельні координати у координати сітки (тайли)
            int x = (int)(worldPosition.X / TileSize);
            int y = (int)(worldPosition.Y / TileSize);

            // Перевірка меж мапи
            if (x < 0 || x >= Width || y < 0 || y >= Height) return Color.Transparent;

            int tileType = _tileMap[y, x];

            // Повертаємо колір залежно від типу тайлу
            return tileType switch {
                0 => new Color(100, 150, 50),  // Трава (зеленуватий пил)
                1 => new Color(139, 69, 19),   // Зорана земля (коричневий)
                2 => new Color(100, 200, 255), // Вода (блакитні бризки)
                _ => Color.White
            };
        }

        public void Draw(SpriteBatch spriteBatch) {
            // 1. Фон (земля, трава, вода)
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

            // 2. Рослини
            foreach (var item in PlantedCrops) {
                Point pt = item.Key;
                Crop crop = item.Value;
                Vector2 pos = new Vector2(pt.X * TileSize, pt.Y * TileSize);

                Texture2D cropTex = null;

                // Розподіляємо 3 картинки на різну кількість днів
                if (crop.Type == CropType.Wheat) {
                    // Пшениця (3 дні)
                    if (crop.CurrentPhase <= 1) cropTex = _wheatPhase0;
                    else if (crop.CurrentPhase == 2) cropTex = _wheatPhase1;
                    else cropTex = _wheatPhase2;
                } else if (crop.Type == CropType.Carrot) {
                    // Морква (5 днів)
                    if (crop.CurrentPhase <= 1) cropTex = _carrotPhase0;       // День 0, 1
                    else if (crop.CurrentPhase <= 4) cropTex = _carrotPhase1;  // День 2, 3, 4
                    else cropTex = _carrotPhase2;                              // День 5
                } else if (crop.Type == CropType.Tomato) {
                    // Помідори (6 днів)
                    if (crop.CurrentPhase <= 2) cropTex = _tomatoPhase0;       // День 0, 1, 2
                    else if (crop.CurrentPhase <= 5) cropTex = _tomatoPhase1;  // День 3, 4, 5
                    else cropTex = _tomatoPhase2;                              // День 6
                }

                if (cropTex != null) {
                    spriteBatch.Draw(cropTex, pos, Color.White);
                }
            }
        }
    }
}