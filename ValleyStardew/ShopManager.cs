using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ValleyStardew {
    public enum ShopState { Closed, MainMenu, BuyPanel, SellPanel }
    public class ShopManager {
        public ShopState CurrentState { get; private set; } = ShopState.Closed;

        private Rectangle _shopIconRect = new Rectangle(20, 70, 48, 48);
        private Rectangle _buyMenuBtnRect = new Rectangle(80, 70, 100, 30);
        private Rectangle _sellMenuBtnRect = new Rectangle(80, 110, 100, 30);
        private Rectangle _closeMenuBtnRect = new Rectangle(80, 150, 100, 30);
        private Rectangle _closePanelBtnRect = new Rectangle(320, 50, 30, 30);
        private Rectangle _panelRect = new Rectangle(50, 50, 300, 400);

        public void Update(MouseState mouseState, MouseState previousMouseState, Inventory inventory) {
            bool clicked = mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            if (!clicked) return;

            Point mousePos = new Point(mouseState.X, mouseState.Y);

            switch (CurrentState) {
                case ShopState.Closed:
                    if (_shopIconRect.Contains(mousePos)) CurrentState = ShopState.MainMenu;
                    break;

                case ShopState.MainMenu:
                    if (_buyMenuBtnRect.Contains(mousePos)) CurrentState = ShopState.BuyPanel;
                    else if (_sellMenuBtnRect.Contains(mousePos)) CurrentState = ShopState.SellPanel;
                    else if (_closeMenuBtnRect.Contains(mousePos)) CurrentState = ShopState.Closed;
                    break;

                case ShopState.BuyPanel:
                    if (_closePanelBtnRect.Contains(mousePos)) CurrentState = ShopState.MainMenu;
                    else {
                        // Логіка кліку по кнопках КУПІВЛІ
                        int yBuyOffset = _panelRect.Y + 60;
                        foreach (var kvp in Crop.Database) {
                            Rectangle btnRect = new Rectangle(_panelRect.X + 220, yBuyOffset, 60, 30);

                            // Якщо клікнули на цю конкретну кнопку і є гроші
                            if (btnRect.Contains(mousePos) && inventory.Money >= kvp.Value.SeedPrice) {
                                inventory.Money -= kvp.Value.SeedPrice;
                                inventory.AddSeed(kvp.Key, 1);
                            }
                            yBuyOffset += 45; // Зсуваємось вниз для наступного товару
                        }
                    }
                    break;

                case ShopState.SellPanel:
                    if (_closePanelBtnRect.Contains(mousePos)) CurrentState = ShopState.MainMenu;
                    else {
                        // Логіка кліку по кнопках ПРОДАЖУ
                        int ySellOffset = _panelRect.Y + 60;
                        foreach (var kvp in inventory.HarvestedCrops) {
                            if (kvp.Value <= 0) continue; // Пропускаємо, якщо цього врожаю немає

                            Rectangle btnRect = new Rectangle(_panelRect.X + 220, ySellOffset, 60, 30);

                            // Якщо клікнули продати
                            if (btnRect.Contains(mousePos)) {
                                int totalEarnings = kvp.Value * Crop.Database[kvp.Key].SellPrice;
                                inventory.Money += totalEarnings;
                                inventory.HarvestedCrops[kvp.Key] = 0; // Обнуляємо цей врожай
                            }
                            ySellOffset += 45;
                        }
                    }
                    break;
            }
        }

        public bool IsPlayerInputBlocked() {
            return CurrentState != ShopState.Closed;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D uiPixel, SpriteFont font, Inventory inventory) {
            // 1. Іконка
            spriteBatch.Draw(uiPixel, _shopIconRect, Color.Purple);
            spriteBatch.DrawString(font, "Shop", new Vector2(_shopIconRect.X + 5, _shopIconRect.Y + 15), Color.White);

            // 2. Головне меню
            if (CurrentState == ShopState.MainMenu) {
                DrawButton(spriteBatch, uiPixel, font, _buyMenuBtnRect, "Buy", Color.Blue);
                DrawButton(spriteBatch, uiPixel, font, _sellMenuBtnRect, "Sell", Color.Green);
                DrawButton(spriteBatch, uiPixel, font, _closeMenuBtnRect, "Close", Color.Red);
            }
            // 3. Панель Купівлі
            else if (CurrentState == ShopState.BuyPanel) {
                spriteBatch.Draw(uiPixel, _panelRect, Color.DarkBlue * 0.9f);
                DrawButton(spriteBatch, uiPixel, font, _closePanelBtnRect, "X", Color.Red);
                spriteBatch.DrawString(font, "--- BUY SEEDS ---", new Vector2(_panelRect.X + 20, _panelRect.Y + 20), Color.White);

                int yOffset = _panelRect.Y + 60;
                foreach (var kvp in Crop.Database) {
                    CropData data = kvp.Value;
                    // Пишемо Назву і Ціну
                    spriteBatch.DrawString(font, $"{data.Name} Seed ({data.SeedPrice}$)", new Vector2(_panelRect.X + 20, yOffset + 5), Color.White);
                    // Малюємо кнопку КУПИТИ
                    DrawButton(spriteBatch, uiPixel, font, new Rectangle(_panelRect.X + 220, yOffset, 60, 30), "Buy", Color.CornflowerBlue);

                    yOffset += 45;
                }
            }
            // 4. Панель Продажу
            else if (CurrentState == ShopState.SellPanel) {
                spriteBatch.Draw(uiPixel, _panelRect, Color.DarkGreen * 0.9f);
                DrawButton(spriteBatch, uiPixel, font, _closePanelBtnRect, "X", Color.Red);
                spriteBatch.DrawString(font, "--- SELL CROPS ---", new Vector2(_panelRect.X + 20, _panelRect.Y + 20), Color.White);

                int yOffset = _panelRect.Y + 60;
                bool hasAnythingToSell = false;

                foreach (var kvp in inventory.HarvestedCrops) {
                    if (kvp.Value <= 0) continue;

                    hasAnythingToSell = true;
                    CropData data = Crop.Database[kvp.Key];
                    int totalValue = kvp.Value * data.SellPrice;

                    // Пишемо Назву, Кількість і Загальну вартість
                    spriteBatch.DrawString(font, $"{data.Name} x{kvp.Value} ({totalValue}$)", new Vector2(_panelRect.X + 20, yOffset + 5), Color.White);
                    // Малюємо кнопку ПРОДАТИ ВСЕ
                    DrawButton(spriteBatch, uiPixel, font, new Rectangle(_panelRect.X + 220, yOffset, 60, 30), "Sell", Color.LimeGreen);

                    yOffset += 45;
                }

                if (!hasAnythingToSell) {
                    spriteBatch.DrawString(font, "Your inventory is empty.", new Vector2(_panelRect.X + 20, yOffset + 10), Color.Gray);
                }
            }
        }

        private void DrawButton(SpriteBatch sb, Texture2D tex, SpriteFont font, Rectangle rect, string text, Color color) {
            sb.Draw(tex, rect, color);
            sb.DrawString(font, text, new Vector2(rect.X + 10, rect.Y + 5), Color.White);
        }
    }
}