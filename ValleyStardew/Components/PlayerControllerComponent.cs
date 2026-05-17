using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using ValleyStardew.Engine;

namespace ValleyStardew {
    public class PlayerControllerComponent : Component {
        public Inventory PlayerInventory = new Inventory();

        private Texture2D _texture;
        private Map _map;
        private KeyboardState _previousKeyboardState;

        private float _speed = 4f;
        private float _stepTimer = 0f;
        private const float StepInterval = 0.4f;

        // Розміри вашого спрайтшиту персонажа
        private int _characterTextureWidth = 32;
        private int _characterTextureHeight = 35;
        private int _rowSpacing = 42;

        private int _currentFrame = 0;
        private int _totalFramesInRow = 4;
        private int _currentAnimationRow = 0;

        private float _animationSpeed = 0.15f;
        private float _animationTimer = 0f;

        private SpriteEffects _spriteEffects = SpriteEffects.None;
        private bool _isMoving = false;

        public Vector2 Center {
            get {
                var transform = Owner.GetComponent<TransformComponent>();
                if (transform == null) return Vector2.Zero;
                return new Vector2(
                    transform.Position.X + _characterTextureWidth / 2f,
                    transform.Position.Y + _characterTextureHeight / 2f
                );
            }
        }

        public PlayerControllerComponent(Texture2D texture, Map map) {
            _texture = texture;
            _map = map;
        }

        public override void Update(GameTime gameTime) {
            var kstate = Keyboard.GetState();
            _isMoving = false;

            // Отримуємо доступ до позиції через сусідній компонент сутності
            var transform = Owner.GetComponent<TransformComponent>();
            if (transform == null) return;

            // --- ВИБІР ІНСТРУМЕНТУ (Клавіші 1, 2, 3) ---
            if (kstate.IsKeyDown(Keys.D1)) PlayerInventory.SelectSlot(0);
            else if (kstate.IsKeyDown(Keys.D2)) PlayerInventory.SelectSlot(1);
            else if (kstate.IsKeyDown(Keys.D3)) PlayerInventory.SelectSlot(2);

            // --- ПЕРЕМИКАННЯ НАСІННЯ (Клавіша Q) ---
            if (kstate.IsKeyDown(Keys.Q) && _previousKeyboardState.IsKeyUp(Keys.Q)) {
                PlayerInventory.CycleSeedType();
            }

            // --- ЛОГІКА РУХУ ---
            if (kstate.IsKeyDown(Keys.Up) || kstate.IsKeyDown(Keys.W)) {
                transform.Position.Y -= _speed;
                _currentAnimationRow = 2;
                _spriteEffects = SpriteEffects.None;
                _isMoving = true;
            }
            if (kstate.IsKeyDown(Keys.Down) || kstate.IsKeyDown(Keys.S)) {
                transform.Position.Y += _speed;
                _currentAnimationRow = 0;
                _isMoving = true;
            }
            if (kstate.IsKeyDown(Keys.Right) || kstate.IsKeyDown(Keys.D)) {
                transform.Position.X += _speed;
                _currentAnimationRow = 1;
                _spriteEffects = SpriteEffects.FlipHorizontally;
                _isMoving = true;
            }
            if (kstate.IsKeyDown(Keys.Left) || kstate.IsKeyDown(Keys.A)) {
                transform.Position.X -= _speed;
                _currentAnimationRow = 1;
                _spriteEffects = SpriteEffects.None;
                _isMoving = true;
            }

            // --- ТАЙМЕР АНІМАЦІЇ ---
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

                    Vector2 feetPos = new Vector2(transform.Position.X + 16, transform.Position.Y + 32);
                    Color currentTileColor = _map.GetTileColorAt(feetPos);

                    ParticleManager.AddWalkingDust(feetPos, currentTileColor);
                    _stepTimer = 0f;
                }
            } else {
                _currentFrame = 0;
                _stepTimer = StepInterval;
            }

            // --- КОЛІЗІЇ З МЕЖАМИ КАРТИ ---
            if (_texture != null) {
                float feetHeight = _map.TileSize;
                float feetY = transform.Position.Y + (_characterTextureHeight - feetHeight);

                float minX = _map.TileSize;
                float minYfeet = _map.TileSize;
                float maxX = (_map.Width - 1) * _map.TileSize - _characterTextureWidth;
                float maxYfeet = (_map.Height - 1) * _map.TileSize - feetHeight;

                float clampedX = MathHelper.Clamp(transform.Position.X, minX, maxX);
                float clampedYfeet = MathHelper.Clamp(feetY, minYfeet, maxYfeet);

                transform.Position.X = clampedX;
                transform.Position.Y = clampedYfeet - (_characterTextureHeight - feetHeight);
            }

            _previousKeyboardState = kstate;
        }

        public override void Draw(SpriteBatch spriteBatch) {
            if (_texture == null) return;

            var transform = Owner.GetComponent<TransformComponent>();
            if (transform == null) return;

            Rectangle sourceRectangle = new Rectangle(
                _currentFrame * _characterTextureWidth,
                _currentAnimationRow * _rowSpacing,
                _characterTextureWidth,
                _characterTextureHeight
            );

            spriteBatch.Draw(
                texture: _texture,
                position: transform.Position,
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