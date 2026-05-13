using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ValleyStardew;

namespace ValleyStardew {
    public class Player {
        public Vector2 Position;
        private KeyboardState _previousKeyboardState;

        private float _stepTimer = 0f;
        private const float StepInterval = 0.4f; // Час між кроками (в секундах). Зміни, якщо кроки зашвидкі/заповільні

        // Додаємо інвентар гравцеві
        public Inventory PlayerInventory = new Inventory();
        public Vector2 Center {
            get {
                return new Vector2(
                    Position.X + (_characterTextureWidth / 2f),
                    Position.Y + (_characterTextureHeight / 2f)
                    );
            }
        }
        private Texture2D _texture;
        private float _speed = 4f;

        // --- ГНУЧКІ ЗМІННІ ДЛЯ АНІМАЦІЇ ---

        // ВАЖЛИВО! Відкрий спрайтшит і виміряй реальний розмір ОДНОГО персонажа!
        // Наприклад, він може бути 32x48, навіть якщо тайл 32x32.
        private int _characterTextureWidth = 32; // <--- ВПИШИ РЕАЛЬНУ ШИРИНУ ПЕРСОНАЖА
        private int _characterTextureHeight = 35; // <--- ВПИШИ РЕАЛЬНУ ВИСОТУ ПЕРСОНАЖА (від голови до ніг)

        // Якщо між рядками є пустий відступ, ця цифра буде більшою за _characterTextureHeight!
        // (Наприклад, якщо персонаж 48 пікселів і є 2 пікселі відступу, то _rowSpacing = 50)
        private int _rowSpacing = 42; // <--- ВПИШИ РЕАЛЬНУ ВИСОТУ РЯДКА (від голови до наступної голови)

        private int _currentFrame = 0;
        private int _totalFramesInRow = 4;

        private int _currentAnimationRow = 0;
        private float _animationSpeed = 0.15f;
        private float _animationTimer = 0f;

        private SpriteEffects _spriteEffects = SpriteEffects.None;
        private bool _isMoving = false;


        public void LoadContent(ContentManager content) {
            _texture = content.Load<Texture2D>("MovementTest3");
        }

        public void Update(Map map, GameTime gameTime) {
            var kstate = Keyboard.GetState();
            _isMoving = false;

            // --- ВИБІР ІНСТРУМЕНТУ (Клавіші 1, 2, 3) ---
            if (kstate.IsKeyDown(Keys.D1)) {
                PlayerInventory.SelectSlot(0); // Вибираємо слот 0 (Сапка)
            } else if (kstate.IsKeyDown(Keys.D2)) {
                PlayerInventory.SelectSlot(1); // Вибираємо слот 1 (Насіння)
            } else if (kstate.IsKeyDown(Keys.D3)) {
                PlayerInventory.SelectSlot(2); // Вибираємо слот 2 (Рука)
            }

            // --- ПЕРЕМИКАННЯ НАСІННЯ (Клавіша Q) ---
            if (kstate.IsKeyDown(Keys.Q) && _previousKeyboardState.IsKeyUp(Keys.Q)) {
                PlayerInventory.CycleSeedType();
            }

            // --- 1. ЛОГІКА РУХУ ТА ВИБІР АНІМАЦІЇ ---
            // У моєму коді: 0 - Вниз, 1 - Вправо (і Вліво), 2 - Вгору. 
            // Якщо у тебе _currentAnimationRow = 2 йде боком, значить у тебе 
            // на спрайтшиті інший порядок! Просто поміняй цифри місцями!

            if (kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W)) {
                Position.Y -= _speed;
                _currentAnimationRow = 2; // Переконайся, що на спрайтшиті це ВГОРУ
                _spriteEffects = SpriteEffects.None;
                _isMoving = true;
            }
            if (kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S)) {
                Position.Y += _speed;
                _currentAnimationRow = 0; // Переконайся, що це ВНИЗ
                _isMoving = true;
            }
            if (kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D)) {
                Position.X += _speed;
                _currentAnimationRow = 1; // Переконайся, що це ВПРАВО
                _spriteEffects = SpriteEffects.FlipHorizontally;
                _isMoving = true;
            }
            if (kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A)) {
                Position.X -= _speed;
                _currentAnimationRow = 1; // Беремо анімацію ВПРАВО
                _spriteEffects = SpriteEffects.None;
                _isMoving = true;
            }

            // --- 2. ТАЙМЕР АНІМАЦІЇ ---
            if (_isMoving) {
                _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_animationTimer > _animationSpeed) {
                    _currentFrame++;
                    if (_currentFrame >= _totalFramesInRow) {
                        _currentFrame = 0;
                    }
                    _animationTimer = 0f;
                }

                // --- ЛОГІКА ЗВУКУ ТА ПИЛУ ПРИ ХОДЬБІ ---
                _stepTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_stepTimer >= StepInterval) {
                    SoundManager.PlayWalk(); 
                    
                    // Розраховуємо позицію ніг гравця (центр по X, низ по Y)
                    Vector2 feetPos = new Vector2(Position.X + 16, Position.Y + 32); // Твої координати ніг
                    Color currentTileColor = map.GetTileColorAt(feetPos); 

                    // 2. Викликаємо пил з правильним кольором
                    ParticleManager.AddWalkingDust(feetPos, currentTileColor);
                    
                    _stepTimer = 0f;         
                }
            } else {
                _currentFrame = 0;
                _stepTimer = StepInterval;
            }

            // --- 3. КОЛІЗІЇ З МЕЖАМИ КАРТИ ---
            // Колізії ідеально підлаштовуються під будь-який розмір персонажа!
            if (_texture != null) {
                float feetHeight = map.TileSize;
                float feetY = Position.Y + (_characterTextureHeight - feetHeight);

                float minX = map.TileSize;
                float minYfeet = map.TileSize;
                float maxX = (map.Width - 1) * map.TileSize - _characterTextureWidth;
                float maxYfeet = (map.Height - 1) * map.TileSize - feetHeight;

                float clampedX = MathHelper.Clamp(Position.X, minX, maxX);
                float clampedYfeet = MathHelper.Clamp(feetY, minYfeet, maxYfeet);

                Position.X = clampedX;
                Position.Y = clampedYfeet - (_characterTextureHeight - feetHeight);
            }

            _previousKeyboardState = kstate;
        }

        public void Draw(SpriteBatch spriteBatch) {
            if (_texture != null) {
                // **ОНОВЛЕНА МАГІЯ ВИРІЗАННЯ!**
                // Тепер ми використовуємо _characterTextureWidth/Height для розміру,
                // але _rowSpacing для переходу між рядками (ігноруючи пусті відступи)!
                Rectangle sourceRectangle = new Rectangle(
                    _currentFrame * _characterTextureWidth,
                    _currentAnimationRow * _rowSpacing,
                    _characterTextureWidth,
                    _characterTextureHeight
                );

                spriteBatch.Draw(
                    texture: _texture,
                    position: Position,
                    sourceRectangle: sourceRectangle,
                    color: Color.White,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: 1f,
                    effects: _spriteEffects,
                    layerDepth: 0f
                );
            }
        }
    }
}