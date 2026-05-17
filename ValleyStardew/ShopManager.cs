using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ValleyStardew {
    // Оголошення станів магазину
    public enum ShopState { Closed, MainMenu, BuyPanel, SellPanel }

    public class ShopManager {
        public ShopState CurrentState { get; private set; } = ShopState.Closed;

        private Rectangle _shopIconRect = new Rectangle(20, 70, 48, 48);

        // Кнопки головного меню (Кнопку Close видалено)
        private Rectangle _buyMenuBtnRect = new Rectangle(80, 70, 96, 48);
        private Rectangle _sellMenuBtnRect = new Rectangle(80, 125, 96, 48);

        // Основна панель (X = 80, щоб співпадати з кнопками)
        private Rectangle _panelRect = new Rectangle(80, 50, 320, 420);

        // Хрестик закриття панелей (ЗАЛИШИВСЯ БЕЗ ЗМІН)
        private Rectangle _closePanelBtnRect => new Rectangle(
            _panelRect.Right - 32 - 10,
            _panelRect.Top + 10,
            32,
            32
        );

        public void Update(MouseState mouseState, MouseState previousMouseState, Inventory inventory) {
            bool clicked = mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
            if (!clicked) return;

            Point mousePos = new Point(mouseState.X, mouseState.Y);

            if (_shopIconRect.Contains(mousePos)) {
                SoundManager.ClickSound.Play();

                if (CurrentState == ShopState.Closed) {
                    CurrentState = ShopState.MainMenu; 
                } else {
                    CurrentState = ShopState.Closed;   
                }
                return; 
            }

            switch (CurrentState) {
                case ShopState.MainMenu:
                    if (_buyMenuBtnRect.Contains(mousePos)) {
                        SoundManager.ClickSound.Play();
                        CurrentState = ShopState.BuyPanel;
                    } else if (_sellMenuBtnRect.Contains(mousePos)) {
                        SoundManager.ClickSound.Play();
                        CurrentState = ShopState.SellPanel;
                    }
                    break;

                case ShopState.BuyPanel:
                case ShopState.SellPanel:
                    // Хрестик всередині панелі повертає нас до головного меню магазину
                    if (_closePanelBtnRect.Contains(mousePos)) {
                        SoundManager.ClickSound.Play();
                        CurrentState = ShopState.MainMenu;
                    } else {
                        UpdatePanelInteractions(mousePos, inventory);
                    }
                    break;
            }
        }

        private void UpdatePanelInteractions(Point mousePos, Inventory inventory) {
            int yOffset = _panelRect.Y + 75;
            if (CurrentState == ShopState.BuyPanel) {
                foreach (var kvp in Crop.Database) {
                    Rectangle btnRect = new Rectangle(_panelRect.X + 200, yOffset, 96, 48);
                    if (btnRect.Contains(mousePos) && inventory.Money >= kvp.Value.SeedPrice) {
                        SoundManager.ClickSound.Play();
                        inventory.Money -= kvp.Value.SeedPrice;
                        inventory.AddSeed(kvp.Key, 1);
                    }
                    yOffset += 55;
                }
            } else if (CurrentState == ShopState.SellPanel) {
                foreach (var kvp in inventory.HarvestedCrops) {
                    if (kvp.Value <= 0) continue;
                    Rectangle btnRect = new Rectangle(_panelRect.X + 200, yOffset, 96, 48);
                    if (btnRect.Contains(mousePos)) {
                        SoundManager.ClickSound.Play();
                        inventory.Money += kvp.Value * Crop.Database[kvp.Key].SellPrice;
                        inventory.HarvestedCrops[kvp.Key] = 0;
                    }
                    yOffset += 55;
                }
            }
        }

        public bool IsMouseOverUI(Point mousePos) {
            if (_shopIconRect.Contains(mousePos)) return true;

            if (CurrentState == ShopState.MainMenu) {
                if (_buyMenuBtnRect.Contains(mousePos)) return true;
                if (_sellMenuBtnRect.Contains(mousePos)) return true;
            } else if (CurrentState == ShopState.BuyPanel || CurrentState == ShopState.SellPanel) {
                if (_panelRect.Contains(mousePos)) return true;
                if (_closePanelBtnRect.Contains(mousePos)) return true;
            }

            return false;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D uiPixel, Texture2D btnTexture, Texture2D shopIcon, SpriteFont font, Inventory inventory) {
            spriteBatch.Draw(shopIcon, _shopIconRect, Color.White);

            if (CurrentState == ShopState.MainMenu) {
                DrawButton(spriteBatch, btnTexture, font, _buyMenuBtnRect, "Buy");
                DrawButton(spriteBatch, btnTexture, font, _sellMenuBtnRect, "Sell");
            } else if (CurrentState == ShopState.BuyPanel || CurrentState == ShopState.SellPanel) {
                DrawNineSlice(spriteBatch, btnTexture, _panelRect, 12);

                spriteBatch.Draw(uiPixel, _closePanelBtnRect, Color.Red);
                Vector2 xSize = font.MeasureString("X");
                Vector2 xPos = new Vector2(
                    _closePanelBtnRect.X + (_closePanelBtnRect.Width - xSize.X) / 2,
                    _closePanelBtnRect.Y + (_closePanelBtnRect.Height - xSize.Y) / 2
                );
                spriteBatch.DrawString(font, "X", xPos, Color.White);

                string title = (CurrentState == ShopState.BuyPanel) ? "--- BUY SEEDS ---" : "--- SELL CROPS ---";
                spriteBatch.DrawString(font, title, new Vector2(_panelRect.X + 30, _panelRect.Y + 30), Color.Gold);

                int yOffset = _panelRect.Y + 75;
                if (CurrentState == ShopState.BuyPanel) {
                    foreach (var kvp in Crop.Database) {
                        spriteBatch.DrawString(font, $"{kvp.Value.Name}\n{kvp.Value.SeedPrice}$", new Vector2(_panelRect.X + 30, yOffset), Color.White);
                        DrawButton(spriteBatch, btnTexture, font, new Rectangle(_panelRect.X + 200, yOffset, 96, 48), "Get");
                        yOffset += 55;
                    }
                } else {
                    foreach (var kvp in inventory.HarvestedCrops) {
                        if (kvp.Value <= 0) continue;
                        int totalValue = kvp.Value * Crop.Database[kvp.Key].SellPrice;
                        spriteBatch.DrawString(font, $"{kvp.Key} x{kvp.Value}\nTotal: {totalValue}$", new Vector2(_panelRect.X + 30, yOffset), Color.White);
                        DrawButton(spriteBatch, btnTexture, font, new Rectangle(_panelRect.X + 200, yOffset, 96, 48), "Sell All");
                        yOffset += 55;
                    }
                }
            }
        }

        private void DrawButton(SpriteBatch sb, Texture2D tex, SpriteFont font, Rectangle rect, string text) {
            DrawNineSlice(sb, tex, rect, 12);
            Vector2 textSize = font.MeasureString(text);
            Vector2 textPos = new Vector2(rect.X + (rect.Width - textSize.X) / 2, rect.Y + (rect.Height - textSize.Y) / 2);
            sb.DrawString(font, text, textPos, Color.White);
        }

        private void DrawNineSlice(SpriteBatch sb, Texture2D tex, Rectangle targetRect, int thickness) {
            int w = tex.Width;
            int h = tex.Height;
            // Кути
            sb.Draw(tex, new Rectangle(targetRect.Left, targetRect.Top, thickness, thickness), new Rectangle(0, 0, thickness, thickness), Color.White);
            sb.Draw(tex, new Rectangle(targetRect.Right - thickness, targetRect.Top, thickness, thickness), new Rectangle(w - thickness, 0, thickness, thickness), Color.White);
            sb.Draw(tex, new Rectangle(targetRect.Left, targetRect.Bottom - thickness, thickness, thickness), new Rectangle(0, h - thickness, thickness, thickness), Color.White);
            sb.Draw(tex, new Rectangle(targetRect.Right - thickness, targetRect.Bottom - thickness, thickness, thickness), new Rectangle(w - thickness, h - thickness, thickness, thickness), Color.White);
            // Краї
            sb.Draw(tex, new Rectangle(targetRect.Left + thickness, targetRect.Top, targetRect.Width - 2 * thickness, thickness), new Rectangle(thickness, 0, w - 2 * thickness, thickness), Color.White);
            sb.Draw(tex, new Rectangle(targetRect.Left + thickness, targetRect.Bottom - thickness, targetRect.Width - 2 * thickness, thickness), new Rectangle(thickness, h - thickness, w - 2 * thickness, thickness), Color.White);
            sb.Draw(tex, new Rectangle(targetRect.Left, targetRect.Top + thickness, thickness, targetRect.Height - 2 * thickness), new Rectangle(0, thickness, thickness, h - 2 * thickness), Color.White);
            sb.Draw(tex, new Rectangle(targetRect.Right - thickness, targetRect.Top + thickness, thickness, targetRect.Height - 2 * thickness), new Rectangle(w - thickness, thickness, thickness, h - 2 * thickness), Color.White);
            // Центр
            sb.Draw(tex, new Rectangle(targetRect.Left + thickness, targetRect.Top + thickness, targetRect.Width - 2 * thickness, targetRect.Height - 2 * thickness), new Rectangle(thickness, thickness, w - 2 * thickness, h - 2 * thickness), Color.White);
        }
    }
}