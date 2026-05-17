using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ValleyStardew {
    public class Entity {
        private List<Component> _components = new List<Component>();

        public void AddComponent(Component component) {
            component.Owner = this;
            _components.Add(component);
        }

        public T GetComponent<T>() where T : Component {
            return _components.OfType<T>().FirstOrDefault();
        }

        public void Update(GameTime gameTime) {
            foreach (var comp in _components) comp.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch) {
            foreach (var comp in _components) comp.Draw(spriteBatch);
        }
    }
}