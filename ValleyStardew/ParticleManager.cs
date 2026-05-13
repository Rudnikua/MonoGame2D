using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ValleyStardew {
    public static class ParticleManager {
        private static List<Particle> _particles = new List<Particle>();
        private static Random _rand = new Random();
        private static Texture2D _pixelTexture;

        // Створюємо текстуру (1 білий піксель) кодом
        public static void Init(GraphicsDevice graphicsDevice) {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        // Метод для виклику пилу з-під ніг
       public static void AddWalkingDust(Vector2 feetPosition, Color dustColor) {
            int count = _rand.Next(2, 5); 
            for (int i = 0; i < count; i++) {
                float vx = (float)(_rand.NextDouble() * 30 - 15);
                float vy = (float)(_rand.NextDouble() * -20 - 10);
                Vector2 velocity = new Vector2(vx, vy);

                float lifespan = (float)(_rand.NextDouble() * 0.3 + 0.2); 
                float scale = (float)(_rand.NextDouble() * 3 + 2);        

                // Тепер використовуємо колір, який прийшов із параметра
                _particles.Add(new Particle(feetPosition, velocity, dustColor, lifespan, scale));
            }
        }

        // Метод для частинок при зборі врожаю
        public static void AddHarvestParticles(Vector2 cropPosition, Texture2D cropTexture) {
            if (cropTexture == null) return;

            int count = _rand.Next(6, 12); 
            for (int i = 0; i < count; i++) {
                float vx = (float)(_rand.NextDouble() * 60 - 30);
                float vy = (float)(_rand.NextDouble() * -60 - 10);
                Vector2 velocity = new Vector2(vx, vy);

                float lifespan = (float)(_rand.NextDouble() * 0.3 + 0.3); 
                float scale = (float)(_rand.NextDouble() * 1.5 + 1f); 

                int pieceSize = _rand.Next(3, 7); 
                int srcX = _rand.Next(0, cropTexture.Width - pieceSize);
                int srcY = _rand.Next(0, cropTexture.Height - pieceSize);
                Rectangle sourceRect = new Rectangle(srcX, srcY, pieceSize, pieceSize);

                _particles.Add(new Particle(cropPosition, velocity, Color.White, lifespan, scale, cropTexture, sourceRect));
            }
        }
        public static void Update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Оновлюємо з кінця списку, щоб безпечно видаляти мертві частинки
            for (int i = _particles.Count - 1; i >= 0; i--) {
                if (_particles[i].Update(dt)) {
                    _particles.RemoveAt(i);
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch) {
            foreach (var particle in _particles) {
                particle.Draw(spriteBatch, _pixelTexture);
            }
        }
    }
}