using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ValleyStardew {
    public abstract class Level {
        protected List<Entity> Entities = new List<Entity>();

        public void AddEntity(Entity entity) {
            Entities.Add(entity);
        }

        public virtual void Update(GameTime gameTime) {
            foreach (var entity in Entities) {
                entity.Update(gameTime);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch) {
            foreach (var entity in Entities) {
                entity.Draw(spriteBatch);
            }
        }
    }
}