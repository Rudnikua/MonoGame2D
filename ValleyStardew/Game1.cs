using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ValleyStardew;

namespace ValleyStardew {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Наші нові об'єкти
        private Map _map;
        private Player _player;
        private Camera _camera;

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            // Створюємо їх
            _map = new Map();
            _player = new Player();
            _camera = new Camera();

            // Ставимо гравця і камеру по центру
            _player.Position = new Vector2((_map.Width * _map.TileSize) / 2, (_map.Height * _map.TileSize) / 2);
            _camera.Position = _player.Position;

            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Кажемо кожному завантажити свої картинки
            _map.LoadContent(Content);
            _player.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Оновлюємо гравця (передаємо йому карту для розрахунку колізій)
            _player.Update(_map);

            // Оновлюємо камеру (передаємо їй координати гравця і розміри екрана)
            _camera.Update(_player.Position, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            // Використовуємо матрицю нашої камери
            _spriteBatch.Begin(transformMatrix: _camera.Transform);

            // Малюємо все по черзі
            _map.Draw(_spriteBatch);
            _player.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}