using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using ValleyStardew.Engine;

namespace ValleyStardew {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _debugDot;

        private SpriteFont _uiFont;
        private Texture2D _uiPixel;

        private Texture2D _iconHoe, _iconSeed, _iconHand, _slotTexture, _slotSelectedTexture;
        private Texture2D _moneyIcon, _shopIconTexture, _btnFrame;

        private Point _hoveredTile;
        private bool _isTileInRange;

        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        private TimeManager _timeManager;
        private ShopManager _shopManager;
        private Map _map;
        private Camera _camera;

        private Level _currentLevel;

        private Texture2D _victoryScreen;
        private Song _victoryMusic;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 1.5);
            _graphics.PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 1.5);

            _graphics.IsFullScreen = false;
            Window.IsBorderless = true;

            IsMouseVisible = true;
            _graphics.ApplyChanges();
        }

        protected override void Initialize() {
            _map = new Map();
            _camera = new Camera();

            _timeManager = new TimeManager();
            _timeManager.RealSecondsPerHour = 0.2f;

            _timeManager.OnNewDay += () => {
                foreach (var crop in _map.PlantedCrops.Values) {
                    crop.Grow();
                }
            };

            _shopManager = new ShopManager();

            base.Initialize();
            ParticleManager.Init(GraphicsDevice);
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _map.LoadContent(Content);

            _debugDot = new Texture2D(GraphicsDevice, 1, 1);
            _debugDot.SetData(new[] { Color.White });

            _uiFont = Content.Load<SpriteFont>("uiFont");
            _uiPixel = new Texture2D(GraphicsDevice, 1, 1);
            _uiPixel.SetData(new[] { Color.White });

            _moneyIcon = Content.Load<Texture2D>("Money");
            _shopIconTexture = Content.Load<Texture2D>("ShopIcon");
            _btnFrame = Content.Load<Texture2D>("ButtonMenuShop");
            _iconHoe = Content.Load<Texture2D>("Hoe");
            _iconSeed = Content.Load<Texture2D>("Seeds");
            _iconHand = Content.Load<Texture2D>("Hand");
            _slotTexture = Content.Load<Texture2D>("Slot_UnSelected");
            _slotSelectedTexture = Content.Load<Texture2D>("Slot_Selected");

            _victoryScreen = Content.Load<Texture2D>("VictoryScreen");
            _victoryMusic = Content.Load<Song>("Scary Monsters and Nice Sprites"); 

            Texture2D playerTex = Content.Load<Texture2D>("MovementTest3");

            // Стартуємо з рівня ферми
            _currentLevel = new FarmLevel(playerTex, _map);

            // Телепортуємо камеру на гравця одразу при старті
            if (_currentLevel is FarmLevel farm) {
                var playerController = farm.PlayerEntity.GetComponent<PlayerControllerComponent>();
                if (playerController != null) {
                    _camera.Position = playerController.Center;
                    _camera.Update(playerController.Center, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                }
            }

            SoundManager.LoadContent(Content);

            MediaPlayer.Play(SoundManager.BackgroundMusic);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.3f;
        }

        protected override void Update(GameTime gameTime) {
            _timeManager.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var kstate = Keyboard.GetState();

            // Оновлюємо поточну сцену (рівень)
            _currentLevel.Update(gameTime);

            // --- ЯКЩО МИ НА РІВНІ ФЕРМИ (Основна логіка гри) ---
            if (_currentLevel is FarmLevel farm) {
                if (kstate.IsKeyDown(Keys.N) && _previousKeyboardState.IsKeyUp(Keys.N)) {
                    foreach (var crop in _map.PlantedCrops.Values) {
                        crop.Grow();
                    }
                }

                var playerController = farm.PlayerEntity.GetComponent<PlayerControllerComponent>();
                if (playerController != null) {
                    _camera.Update(playerController.Center, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                }

                MouseState mouseState = Mouse.GetState();
                Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);

                Matrix inverseCameraTransform = Matrix.Invert(_camera.Transform);
                Vector2 mouseWorldPos = Vector2.Transform(mouseScreenPos, inverseCameraTransform);

                _hoveredTile.X = (int)Math.Floor(mouseWorldPos.X / _map.TileSize);
                _hoveredTile.Y = (int)Math.Floor(mouseWorldPos.Y / _map.TileSize);

                bool isHoveringMap = _hoveredTile.X >= 0 && _hoveredTile.X < _map.Width &&
                                     _hoveredTile.Y >= 0 && _hoveredTile.Y < _map.Height;

                if (playerController != null) {
                    int playerTileX = (int)(playerController.Center.X / _map.TileSize);
                    int playerTileY = (int)(playerController.Center.Y / _map.TileSize);
                    _isTileInRange = isHoveringMap && (Math.Abs(_hoveredTile.X - playerTileX) <= 3 && Math.Abs(_hoveredTile.Y - playerTileY) <= 3);
                }

                _shopManager.Update(mouseState, _previousMouseState, playerController.PlayerInventory);

                if (_shopManager.TicketBought) {
                    _currentLevel = new VictoryLevel(_victoryScreen, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                    MediaPlayer.Stop();

                    MediaPlayer.Play(_victoryMusic);
                    MediaPlayer.IsRepeating = false; 
                    return;
                }

                Point mousePoint = new Point(mouseState.X, mouseState.Y);
                if (!_shopManager.IsMouseOverUI(mousePoint)) {
                    if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released) {
                        if (_isTileInRange) {
                            _map.InteractWithTile(_hoveredTile.X, _hoveredTile.Y, playerController.PlayerInventory);
                        }
                    }
                }

                _previousMouseState = mouseState;
            }

            _previousKeyboardState = kstate;
            ParticleManager.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // --- МАЛЮВАННЯ ДЛЯ РІВНЯ ФЕРМИ ---
            if (_currentLevel is FarmLevel farm) {
                _spriteBatch.Begin(transformMatrix: _camera.Transform, samplerState: SamplerState.PointClamp);
                _currentLevel.Draw(_spriteBatch);
                ParticleManager.Draw(_spriteBatch);

                if (_hoveredTile.X >= 0 && _hoveredTile.X < _map.Width && _hoveredTile.Y >= 0 && _hoveredTile.Y < _map.Height) {
                    Rectangle tileRect = new Rectangle(_hoveredTile.X * _map.TileSize, _hoveredTile.Y * _map.TileSize, _map.TileSize, _map.TileSize);
                    _spriteBatch.Draw(_uiPixel, tileRect, _isTileInRange ? Color.White * 0.4f : Color.Red * 0.4f);
                }

                var playerController = farm.PlayerEntity.GetComponent<PlayerControllerComponent>();
                //if (playerController != null) {
                //    _spriteBatch.Draw(_debugDot, new Rectangle((int)playerController.Center.X - 5, (int)playerController.Center.Y - 5, 10, 10), Color.Green);
                //}
                //_spriteBatch.Draw(_debugDot, new Rectangle((int)_camera.Position.X - 3, (int)_camera.Position.Y - 3, 6, 6), Color.Red);
                _spriteBatch.End();

                // ІНТЕРФЕЙС ФЕРМИ
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                float darkness = _timeManager.CurrentDarkness;
                if (darkness > 0f) {
                    _spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), new Color(10, 10, 30) * darkness);
                }

                string timeText = $"Day {_timeManager.Day} - {_timeManager.GetTimeString()}";
                Vector2 timePos = new Vector2(GraphicsDevice.Viewport.Width - 125, 20);
                _spriteBatch.DrawString(_uiFont, timeText, timePos + new Vector2(2, 2), Color.Black * 0.7f);
                _spriteBatch.DrawString(_uiFont, timeText, timePos, Color.White);

                if (playerController != null) {
                    _spriteBatch.Draw(_moneyIcon, new Rectangle(20, 20, 32, 32), Color.White);
                    _spriteBatch.DrawString(_uiFont, playerController.PlayerInventory.Money.ToString(), new Vector2(62, 27), Color.Gold);
                    _shopManager.Draw(_spriteBatch, _uiPixel, _btnFrame, _shopIconTexture, _uiFont, playerController.PlayerInventory);

                    // Тулбар
                    int slotSize = 48; int spacing = 8; int totalSlots = playerController.PlayerInventory.Toolbar.Count;
                    int totalWidth = (totalSlots * slotSize) + ((totalSlots - 1) * spacing);
                    int startX = (GraphicsDevice.Viewport.Width - totalWidth) / 2;
                    int startY = GraphicsDevice.Viewport.Height - slotSize - 20;

                    for (int i = 0; i < totalSlots; i++) {
                        Rectangle slotRect = new Rectangle(startX + (slotSize + spacing) * i, startY, slotSize, slotSize);
                        _spriteBatch.Draw((playerController.PlayerInventory.ActiveSlotIndex == i) ? _slotSelectedTexture : _slotTexture, slotRect, Color.White);

                        Texture2D toolIcon = null;
                        if (playerController.PlayerInventory.Toolbar[i] == ToolType.Hoe) toolIcon = _iconHoe;
                        else if (playerController.PlayerInventory.Toolbar[i] == ToolType.Seed) toolIcon = _iconSeed;
                        else if (playerController.PlayerInventory.Toolbar[i] == ToolType.Hand) toolIcon = _iconHand;

                        if (toolIcon != null) _spriteBatch.Draw(toolIcon, new Vector2(slotRect.X + 8, slotRect.Y + 8), Color.White);

                        if (playerController.PlayerInventory.Toolbar[i] == ToolType.Seed) {
                            int count = playerController.PlayerInventory.Seeds.ContainsKey(playerController.PlayerInventory.SelectedSeedType) ? playerController.PlayerInventory.Seeds[playerController.PlayerInventory.SelectedSeedType] : 0;
                            string countText = count.ToString();
                            Vector2 textSize = _uiFont.MeasureString(countText);
                            _spriteBatch.DrawString(_uiFont, countText, new Vector2(slotRect.Right - textSize.X - 5, slotRect.Bottom - textSize.Y - 2) + new Vector2(1, 1), Color.Black);
                            _spriteBatch.DrawString(_uiFont, countText, new Vector2(slotRect.Right - textSize.X - 5, slotRect.Bottom - textSize.Y - 2), Color.White);

                            if (playerController.PlayerInventory.ActiveSlotIndex == i) {
                                string seedName = playerController.PlayerInventory.SelectedSeedType.ToString();
                                Vector2 nameSize = _uiFont.MeasureString(seedName);
                                _spriteBatch.DrawString(_uiFont, seedName, new Vector2(slotRect.Center.X - (nameSize.X / 2), slotRect.Top - nameSize.Y - 8) + new Vector2(1, 1), Color.Black);
                                _spriteBatch.DrawString(_uiFont, seedName, new Vector2(slotRect.Center.X - (nameSize.X / 2), slotRect.Top - nameSize.Y - 8), Color.LimeGreen);
                            }
                        }
                    }
                }
                _spriteBatch.End();
            }
            // --- МАЛЮВАННЯ ДЛЯ РІВНЯ ПЕРЕМОГИ (Екран у звичайних координатах вікна) ---
            else if (_currentLevel is VictoryLevel) {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _currentLevel.Draw(_spriteBatch);
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}