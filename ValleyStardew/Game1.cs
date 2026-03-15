using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ValleyStardew // Якщо ваш проєкт називається інакше, залиште вашу оригінальну назву тут
{
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // --- ЗМІННІ ПЕРСОНАЖА ---
        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private float _playerSpeed;

        // --- ЗМІННІ КАРТИ ---
        private Texture2D _grassTexture;
        private Texture2D _dirtTexture;
        private Texture2D _waterTexture;
        private const int TILE_SIZE = 64; // Розмір одного квадратика

        // Наша карта: 0 - трава, 1 - земля, 2 - вода
        private int[,] _tileMap = new int[,]
        {
            { 0, 0, 0, 0, 0, 0, 2, 2, 2, 2 },
            { 0, 1, 1, 0, 0, 0, 0, 2, 2, 2 },
            { 0, 1, 1, 0, 0, 0, 0, 0, 2, 2 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            // Ставимо гравця по центру вікна
            _playerPosition = new Vector2(
                _graphics.PreferredBackBufferWidth / 2,
                _graphics.PreferredBackBufferHeight / 2
            );

            _playerSpeed = 5f; // Швидкість руху

            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Завантажуємо всі 4 картинки (назви мають точно збігатися з тими, що в MGCB)
            _playerTexture = Content.Load<Texture2D>("player");
            _grassTexture = Content.Load<Texture2D>("grass64x64");
            _dirtTexture = Content.Load<Texture2D>("dirt64x64");
            _waterTexture = Content.Load<Texture2D>("water64x64");
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // --- ЛОГІКА РУХУ ПЕРСОНАЖА ---
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W))
                _playerPosition.Y -= _playerSpeed;
            if (kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S))
                _playerPosition.Y += _playerSpeed;
            if (kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A))
                _playerPosition.X -= _playerSpeed;
            if (kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D))
                _playerPosition.X += _playerSpeed;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            // 1. СПОЧАТКУ МАЛЮЄМО КАРТУ (щоб вона була на задньому фоні)
            for (int y = 0; y < _tileMap.GetLength(0); y++) {
                for (int x = 0; x < _tileMap.GetLength(1); x++) {
                    int tileType = _tileMap[y, x];
                    Vector2 tilePosition = new Vector2(x * TILE_SIZE, y * TILE_SIZE);

                    Texture2D textureToDraw = _grassTexture;
                    if (tileType == 0) textureToDraw = _grassTexture;
                    else if (tileType == 1) textureToDraw = _dirtTexture;
                    else if (tileType == 2) textureToDraw = _waterTexture;

                    _spriteBatch.Draw(textureToDraw, tilePosition, Color.White);
                }
            }

            // 2. ПОТІМ МАЛЮЄМО ПЕРСОНАЖА (щоб він був поверх карти)
            _spriteBatch.Draw(_playerTexture, _playerPosition, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}