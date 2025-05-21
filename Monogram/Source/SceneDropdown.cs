using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Monogram
{
    public class SceneDropdown
    {
        private readonly List<string> _sceneNames;
        private readonly SpriteFont _font;
        private readonly int _itemHeight;
        private readonly int _width;
        private bool _expanded = false;
        private int _selectedIndex;
        private Rectangle _dropdownRect;
        private Rectangle[] _itemRects;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => _selectedIndex = value;
        }
        public bool Expanded => _expanded;
        public bool IsMouseOver { get; private set; }

        public Rectangle DropdownRect => _dropdownRect;

        public SceneDropdown(List<string> sceneNames, SpriteFont font, int x, int y, int width = 220)
        {
            _sceneNames = sceneNames;
            _font = font;
            _itemHeight = (int)font.MeasureString("A").Y + 8;
            _width = width;
            _selectedIndex = 0;
            _dropdownRect = new Rectangle(x, y, width, _itemHeight);
            _itemRects = new Rectangle[_sceneNames.Count];
            for (int i = 0; i < _sceneNames.Count; i++)
                _itemRects[i] = new Rectangle(x, y + _itemHeight * (i + 1), width, _itemHeight);
        }

        public void Update(MouseState mouse, MouseState prevMouse)
        {
            Point mousePos = new(mouse.X, mouse.Y);
            bool overDropdown = _dropdownRect.Contains(mousePos);
            bool overItem = false;
            if (_expanded)
            {
                for (int i = 0; i < _itemRects.Length; i++)
                {
                    if (_itemRects[i].Contains(mousePos))
                    {
                        overItem = true;
                        break;
                    }
                }
            }
            IsMouseOver = overDropdown || overItem;

            bool mouseClicked = mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed;

            if (!_expanded)
            {
                if (overDropdown && mouseClicked)
                {
                    _expanded = true;
                    return;
                }
            }
            else
            {
                if (overDropdown && mouseClicked)
                {
                    _expanded = false;
                    return;
                }

                for (int i = 0; i < _itemRects.Length; i++)
                {
                    if (_itemRects[i].Contains(mousePos) && mouseClicked)
                    {
                        if (i != _selectedIndex)
                            _selectedIndex = i;
                        _expanded = false;
                        return;
                    }
                }

                if (mouseClicked && !overDropdown && Array.TrueForAll(_itemRects, r => !r.Contains(mousePos)))
                {
                    _expanded = false;
                    return;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw main box
            spriteBatch.DrawBox(_dropdownRect, Color.DarkSlateGray, Color.White);
            spriteBatch.DrawString(_font, _sceneNames[_selectedIndex], new Vector2(_dropdownRect.X + 8, _dropdownRect.Y + 6), Color.White);

            // Draw arrow with fallback if unsupported
            var arrowUp = "▲";
            var arrowDown = "▼";
            var fallbackUp = "^";
            var fallbackDown = "v";

            string arrow;
            if (_expanded)
                arrow = _font.Characters.Contains(arrowUp[0]) ? arrowUp : fallbackUp;
            else
                arrow = _font.Characters.Contains(arrowDown[0]) ? arrowDown : fallbackDown;

            var arrowSize = _font.MeasureString(arrow);
            spriteBatch.DrawString(_font, arrow, new Vector2(_dropdownRect.Right - arrowSize.X - 8, _dropdownRect.Y + 6), Color.White);

            // Draw expanded items
            if (_expanded)
            {
                for (int i = 0; i < _itemRects.Length; i++)
                {
                    var rect = _itemRects[i];
                    spriteBatch.DrawBox(rect, i == _selectedIndex ? Color.DimGray : Color.Gray, Color.White);
                    spriteBatch.DrawString(_font, _sceneNames[i], new Vector2(rect.X + 8, rect.Y + 6), Color.White);
                }
            }
        }
    }

    // Helper extension for drawing rectangles
    public static class SpriteBatchExtensions
    {
        private static Texture2D? _pixel;

        public static void DrawBox(this SpriteBatch spriteBatch, Rectangle rect, Color fill, Color border)
        {
            if (_pixel == null || _pixel.GraphicsDevice.IsDisposed)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            spriteBatch.Draw(_pixel, rect, fill);

            // Border
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), border);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), border);
            spriteBatch.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - 1, rect.Width, 1), border);
            spriteBatch.Draw(_pixel, new Rectangle(rect.Right - 1, rect.Y, 1, rect.Height), border);
        }
    }
}
