using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ValleyStardew {
    public class FarmLevel : Level {
        public Map FarmMap { get; private set; }
        public Entity PlayerEntity { get; private set; }

        public FarmLevel(Texture2D playerTexture, Map map) {
            FarmMap = map;

            // Ініціалізуємо Гравця як порожню Сутність (Entity)
            PlayerEntity = new Entity();

            // Розраховуємо стартову позицію (центр карти)
            Vector2 startPos = new Vector2(map.Width * map.TileSize / 2, map.Height * map.TileSize / 2);

            // Збираємо Гравця з компонентів за принципами архітектури ігрових двигунів
            PlayerEntity.AddComponent(new TransformComponent(startPos));
            PlayerEntity.AddComponent(new PlayerControllerComponent(playerTexture, map));

            // Додаємо сутність гравця в загальний список рівня
            AddEntity(PlayerEntity);
        }

        public override void Update(GameTime gameTime) {
            // Оновлює всі компоненти сутностей, доданих на рівень (наразі це гравець)
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            // Спочатку малюємо тайлову карту (задній фон)
            FarmMap.Draw(spriteBatch);

            // Поверх карти малюємо всі сутності рівня (гравця, тварин, тощо)
            base.Draw(spriteBatch);
        }
    }
}