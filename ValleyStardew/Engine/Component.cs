using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ValleyStardew {
    public abstract class Component {
        public Entity Owner { get; set; } 

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch) { }
    }
}