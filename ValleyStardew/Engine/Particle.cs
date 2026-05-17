using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ValleyStardew.Engine {
    public class Particle {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Lifespan;
        public float MaxLifespan;
        public float Scale;
        
        public Texture2D Texture; 
        public Rectangle? SourceRect; 

        public Particle(Vector2 position, Vector2 velocity, Color color, float lifespan, float scale, Texture2D texture = null, Rectangle? sourceRect = null) {
            Position = position;
            Velocity = velocity;
            Color = color;
            Lifespan = lifespan;
            MaxLifespan = lifespan;
            Scale = scale;
            Texture = texture;
            SourceRect = sourceRect;
        }

        public bool Update(float dt) {
            Position += Velocity * dt; 
            Lifespan -= dt;            
            return Lifespan <= 0;      
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D defaultTexture) {
            float opacity = Lifespan / MaxLifespan; 
            Texture2D texToDraw = Texture ?? defaultTexture;
            
            spriteBatch.Draw(texToDraw, Position, SourceRect, Color * opacity, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }
    }
}