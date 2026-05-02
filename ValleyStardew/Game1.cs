using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using ValleyStardew;

namespace ValleyStardew {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _debugDot;

        private SpriteFont _uiFont;
        private Texture2D _uiPixel;
        private Texture2D _moneyIcon;

        private Point _hoveredTile; // Зберігатиме координати X та Y тайлу, на який дивиться мишка
        private bool _isTileInRange; // Буде true, якщо тайл у зоні 3х3 біля гравця

        private MouseState _previousMouseState;
        private KeyboardState _previousKeyboardState;

        // Наші нові об'єкти
        private Map _map;
        private Player _player;
        private Camera _camera;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // --- ВІКНО НА ВЕСЬ ЕКРАН (Borderless Window) ---

            // 1. Беремо розміри монітора
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2;

            // 2. ВИМИКАЄМО жорсткий повноекранний режим
            _graphics.IsFullScreen = false;

            // 3. Робимо вікно БЕЗ / З РАМКАМИ (прибираємо верхню смужку з хрестиком)
            Window.IsBorderless = false;

            _graphics.ApplyChanges();
        }

        protected override void Initialize() {
            // Створюємо їх
            _map = new Map();
            _player = new Player();
            _camera = new Camera();

            // Ставимо гравця і камеру по центру
            _player.Position = new Vector2((_map.Width * _map.TileSize) / 2, (_map.Height * _map.TileSize) / 2);
            _camera.Position = _player.Center;

            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Кажемо кожному завантажити свої картинки
            _map.LoadContent(Content);
            _player.LoadContent(Content);

            // --- СТВОРЮЄМО ТЕКСТУРУ ДЛЯ ДЕБАГУ ---
            _debugDot = new Texture2D(GraphicsDevice, 1, 1);
            _debugDot.SetData(new[] { Color.White }); // Зафарбовуємо цей 1 піксель білим кольором

            // --- ЗАВАНТАЖЕННЯ ДЛЯ UI ---
            _uiFont = Content.Load<SpriteFont>("uiFont"); // Завантажуємо шрифт
            _uiPixel = new Texture2D(GraphicsDevice, 1, 1);
            _uiPixel.SetData(new[] { Color.White });

            _moneyIcon = Content.Load<Texture2D>("Money");
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Keys.N) && _previousKeyboardState.IsKeyUp(Keys.N)) {
                foreach (var crop in _map.PlantedCrops.Values) {
                    crop.Grow();
                }
            }

            // Оновлюємо гравця (передаємо йому карту для розрахунку колізій)
            _player.Update(_map, gameTime);

            // Оновлюємо камеру (передаємо їй координати гравця і розміри екрана)
            _camera.Update(_player.Center, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // === ЛОГІКА ПРИЦІЛУ ТА МИШКИ ===

            // 1. Беремо позицію миші на екрані (моніторі)
            MouseState mouseState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);

            // 2. ПЕРЕТВОРЕННЯ: Екранні координати -> Світові координати
            // Для цього ми "вивертаємо" матрицю камери навиворіт
            Matrix inverseCameraTransform = Matrix.Invert(_camera.Transform);
            Vector2 mouseWorldPos = Vector2.Transform(mouseScreenPos, inverseCameraTransform);

            // 3. Шукаємо ІНДЕКС тайлу на карті (ділимо на розмір тайлу - 32)
            // Використовуємо Math.Floor, щоб правильно округлювати навіть від'ємні координати
            _hoveredTile.X = (int)Math.Floor(mouseWorldPos.X / _map.TileSize);
            _hoveredTile.Y = (int)Math.Floor(mouseWorldPos.Y / _map.TileSize);

            // 4. Перевіряємо, чи мишка взагалі знаходиться в межах нашої карти 50х50
            bool isHoveringMap = _hoveredTile.X >= 0 && _hoveredTile.X < _map.Width &&
                                 _hoveredTile.Y >= 0 && _hoveredTile.Y < _map.Height;

            // 5. Перевіряємо радіус 7x7 навколо гравця
            // Спочатку дізнаємося, на якому тайлі стоїть сам гравець
            int playerTileX = (int)(_player.Center.X / _map.TileSize);
            int playerTileY = (int)(_player.Center.Y / _map.TileSize);

            // Рахуємо дистанцію по клітинках
            int distanceX = Math.Abs(_hoveredTile.X - playerTileX);
            int distanceY = Math.Abs(_hoveredTile.Y - playerTileY);

            // Тайл доступний, якщо мишка на карті І відстань по X та Y не більша за 1
            _isTileInRange = isHoveringMap && (distanceX <= 3 && distanceY <= 3);

            // === ЛОГІКА КЛІКУ ПО ЗЕМЛІ ===

            // Перевіряємо: Ліва кнопка ЗАРАЗ натиснута, але в МИНУЛОМУ кадрі була відпущена (це і є 1 клік)
            if (mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released) {
                // Якщо клітинка підсвічена білим (у зоні дії)
                if (_isTileInRange) {
                    // Кажемо карті: "Гей, ми клікнули по цій клітинці з таким-то інструментом!"
                    _map.InteractWithTile(_hoveredTile.X, _hoveredTile.Y, _player.PlayerInventory);
                }
            }

            // Зберігаємо поточний стан мишки для наступного кадру!
            _previousMouseState = mouseState;
            _previousKeyboardState = kstate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // Використовуємо матрицю нашої камери
            _spriteBatch.Begin(transformMatrix: _camera.Transform, samplerState: SamplerState.PointClamp);

            // Малюємо все по черзі
            _map.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);

            // --- МАЛЮЄМО ПІДСВІТКУ ТАЙЛУ ---
            // Малюємо тільки якщо мишка знаходиться на самій карті
            if (_hoveredTile.X >= 0 && _hoveredTile.X < _map.Width &&
                _hoveredTile.Y >= 0 && _hoveredTile.Y < _map.Height) {
                // Створюємо прямокутник за координатами тайлу
                Rectangle tileRect = new Rectangle(
                    _hoveredTile.X * _map.TileSize,
                    _hoveredTile.Y * _map.TileSize,
                    _map.TileSize,
                    _map.TileSize
                );

                // Якщо в радіусі 7х7 - колір білий (напівпрозорий), якщо далеко - червоний
                Color highlightColor = _isTileInRange ? Color.White * 0.4f : Color.Red * 0.4f;

                // Використовуємо наш білий UI піксель, щоб замалювати клітинку
                _spriteBatch.Draw(_uiPixel, tileRect, highlightColor);
            }

            // --- МАЛЮЄМО ДЕБАГ-ТОЧКИ ---

            // 1. Зелена точка - Центр гравця (розмір 10x10 пікселів, зміщуємо на -5 щоб відцентрувати)
            Rectangle playerCenterRect = new Rectangle((int)_player.Center.X - 5, (int)_player.Center.Y - 5, 10, 10);
            _spriteBatch.Draw(_debugDot, playerCenterRect, Color.Green);

            // 2. Червона точка - Поточна позиція камери (розмір 6x6, щоб було видно на тлі зеленої)
            Rectangle cameraPosRect = new Rectangle((int)_camera.Position.X - 3, (int)_camera.Position.Y - 3, 6, 6);
            _spriteBatch.Draw(_debugDot, cameraPosRect, Color.Red);

            _spriteBatch.End();

            // === 2. МАЛЮЄМО ІНТЕРФЕЙС (БЕЗ КАМЕРИ) ===
            // Використовуємо звичайний Begin()
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // --- Економіка (Лівий верхній кут) ---
            // Малюємо іконку грошей
            Rectangle coinRect = new Rectangle(20, 20, 32, 32);
            _spriteBatch.Draw(_moneyIcon, coinRect, Color.White);

            // 2. Формуємо текст - тепер ТІЛЬКИ число
            string moneyText = _player.PlayerInventory.Money.ToString();

            // 3. Малюємо текст поруч з іконкою
            // X = 20 (позиція іконки) + 32 (ширина іконки) + 10 (відступ) = 62
            // Y = 27 (трохи нижче, щоб текст був по центру іконки по вертикалі)
            _spriteBatch.DrawString(_uiFont, moneyText, new Vector2(62, 27), Color.Gold);


            // --- Тулбар (Знизу по центру) ---
            int slotSize = 64; // Розмір одного квадратика інвентарю
            int spacing = 10;  // Відстань між квадратиками
            int totalSlots = _player.PlayerInventory.Toolbar.Count;

            // Рахуємо ширину всього тулбара, щоб розмістити його рівно по центру екрана
            int totalWidth = (totalSlots * slotSize) + ((totalSlots - 1) * spacing);
            int startX = (GraphicsDevice.Viewport.Width - totalWidth) / 2;
            int startY = GraphicsDevice.Viewport.Height - slotSize - 20; // 20 пікселів від низу екрана

            // Проходимося циклом по всіх слотах
            for (int i = 0; i < totalSlots; i++) {
                // Рахуємо позицію для кожного квадратика
                Rectangle slotRect = new Rectangle(startX + (slotSize + spacing) * i, startY, slotSize, slotSize);

                // Якщо цей слот зараз активний (вибраний) - малюємо його жовтим, якщо ні - сірим
                Color slotColor = (_player.PlayerInventory.ActiveSlotIndex == i) ? Color.Yellow : Color.DimGray;

                // Малюємо фон слота (робимо його трохи прозорим)
                _spriteBatch.Draw(_uiPixel, slotRect, slotColor * 0.6f);

                // Малюємо назву інструменту всередині слота
                string toolName = _player.PlayerInventory.Toolbar[i].ToString();
                _spriteBatch.DrawString(_uiFont, toolName, new Vector2(slotRect.X + 5, slotRect.Y + 20), Color.White);

                // Якщо це слот з насінням, додатково малюємо його кількість
                if (_player.PlayerInventory.Toolbar[i] == ToolType.Seed) {
                    string seedCount = $"x{_player.PlayerInventory.SeedsCount}";
                    _spriteBatch.DrawString(_uiFont, seedCount, new Vector2(slotRect.X + 5, slotRect.Y + 40), Color.LimeGreen);
                }
            }

            _spriteBatch.End();
            // ==========================================

            base.Draw(gameTime);
        }
    }
}