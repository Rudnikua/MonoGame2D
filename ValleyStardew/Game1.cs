using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ValleyStardew // Залиште вашу назву проєкту, якщо вона інша
{
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // --- ЗМІННІ ПЕРСОНАЖА ---
        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private float _playerSpeed;

        // --- ЗМІННІ КАМЕРИ ---
        private Vector2 _cameraPosition; // Окрема позиція для плавної камери

        // --- ЗМІННІ КАРТИ ---
        private Texture2D _grassTexture;
        private Texture2D _dirtTexture;
        private Texture2D _waterTexture;

        private const int TILE_SIZE = 32; // Новий розмір тайла

        // Розміри нашої великої карти (в клітинках)
        private const int MAP_WIDTH = 50;
        private const int MAP_HEIGHT = 50;

        private int[,] _tileMap; // Масив для карти

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            // 1. ГЕНЕРУЄМО ВЕЛИКУ КАРТУ
            _tileMap = new int[MAP_HEIGHT, MAP_WIDTH];
            for (int y = 0; y < MAP_HEIGHT; y++) {
                for (int x = 0; x < MAP_WIDTH; x++) {
                    // Заповнюємо все травою (0)
                    _tileMap[y, x] = 0;

                    // Робимо рамку з води (2) по краях карти
                    if (x == 0 || y == 0 || x == MAP_WIDTH - 1 || y == MAP_HEIGHT - 1) {
                        _tileMap[y, x] = 2;
                    }
                }
            }

            // 2. СТАВИМО ГРАВЦЯ ПО ЦЕНТРУ КАРТИ
            _playerPosition = new Vector2((MAP_WIDTH * TILE_SIZE) / 2, (MAP_HEIGHT * TILE_SIZE) / 2);
            _playerSpeed = 5f;

            // 3. СТАВИМО КАМЕРУ НА ГРАВЦЯ НА СТАРТІ
            _cameraPosition = _playerPosition;

            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Завантажуємо картинки
            _playerTexture = Content.Load<Texture2D>("player");
            _grassTexture = Content.Load<Texture2D>("grass");
            _dirtTexture = Content.Load<Texture2D>("dirt");
            _waterTexture = Content.Load<Texture2D>("water");
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // --- ЛОГІКА РУХУ ГРАВЦЯ ---
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W))
                _playerPosition.Y -= _playerSpeed;
            if (kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S))
                _playerPosition.Y += _playerSpeed;
            if (kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A))
                _playerPosition.X -= _playerSpeed;
            if (kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D))
                _playerPosition.X += _playerSpeed;
            // ---------------------------

            // --- ОНОВЛЕНІ ОБМЕЖЕННЯ (Колізії тільки для ніг) ---

            // 1. Визначаємо, де знаходяться ноги гравця відносно його загальної позиції
            // Уявимо, що ноги - це нижні 32 пікселі твого високого персонажа (64 пікселі).
            float feetHeight = TILE_SIZE; // Висота зони ніг = 32

            // Координата Y, де починаються ноги (Y гравця + різниця висот)
            float feetY = _playerPosition.Y + (_playerTexture.Height - feetHeight);

            // 2. Рахуємо межі, куди НОГИ не можуть заходити (вода по краях)
            float minX = TILE_SIZE; // 1 тайл зліва
            float minYfeet = TILE_SIZE; // 1 тайл зверху (для ніг)

            // Максимум X: Ширина карти - 1 тайл води - ширина гравця
            float maxX = (MAP_WIDTH - 1) * TILE_SIZE - _playerTexture.Width;

            // Максимум Y для НІГ: Висота карти - 1 тайл води - висота ніг
            float maxYfeet = (MAP_HEIGHT - 1) * TILE_SIZE - feetHeight;

            // 3. Застосовуємо Clamp спочатку до координат ніг
            float clampedX = MathHelper.Clamp(_playerPosition.X, minX, maxX);
            float clampedYfeet = MathHelper.Clamp(feetY, minYfeet, maxYfeet);

            // 4. ПЕРЕРАХОВУЄМО позицію всього гравця назад
            // X залишається тим самим
            _playerPosition.X = clampedX;

            // Y гравця = Clamped Y ніг - (загальна висота - висота ніг)
            _playerPosition.Y = clampedYfeet - (_playerTexture.Height - feetHeight);

            // --- (кінець блоку обмежень) ---

            // --- ЛОГІКА ПЛАВНОЇ КАМЕРИ ---
            // Камера плавно переміщується до позиції гравця (швидкість 0.1f)
            _cameraPosition = Vector2.Lerp(_cameraPosition, _playerPosition, 0.1f);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            // Очищаємо екран
            GraphicsDevice.Clear(Color.Black);

            // --- СТВОРЮЄМО МАТРИЦЮ КАМЕРИ ---
            // Зверніть увагу: тепер ми використовуємо _cameraPosition, а не _playerPosition
            Matrix cameraTransform = Matrix.CreateTranslation(
                -_cameraPosition.X + (_graphics.PreferredBackBufferWidth / 2),
                -_cameraPosition.Y + (_graphics.PreferredBackBufferHeight / 2),
                0f);

            // Передаємо камеру у малювальник
            _spriteBatch.Begin(transformMatrix: cameraTransform);

            // 1. МАЛЮЄМО КАРТУ
            for (int y = 0; y < MAP_HEIGHT; y++) {
                for (int x = 0; x < MAP_WIDTH; x++) {
                    int tileType = _tileMap[y, x];
                    Vector2 tilePosition = new Vector2(x * TILE_SIZE, y * TILE_SIZE);

                    Texture2D textureToDraw = _grassTexture;
                    if (tileType == 1) textureToDraw = _dirtTexture;
                    else if (tileType == 2) textureToDraw = _waterTexture;

                    _spriteBatch.Draw(textureToDraw, tilePosition, Color.White);
                }
            }

            // 2. МАЛЮЄМО ГРАВЦЯ
            _spriteBatch.Draw(_playerTexture, _playerPosition, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}