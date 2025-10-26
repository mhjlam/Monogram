using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace Monogram
{
    public class SceneDropdown
    {
        private bool _expanded = false;
        private int _selectedIndex;
        private Rectangle _dropdownRect;

		private readonly int _width;
		private readonly int _itemHeight;
		private readonly Rectangle[] _itemRects;
		private readonly SpriteFont _font;
		private readonly List<string> _sceneNames;

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
            _itemRects = [.. Enumerable.Range(0, _sceneNames.Count).Select(i => new Rectangle(x, y + _itemHeight * (i + 1), width, _itemHeight))];
        }

        public void Update(MouseState mouse, MouseState prevMouse)
        {
            Point mousePos = new(mouse.X, mouse.Y);
            bool overDropdown = _dropdownRect.Contains(mousePos);
            bool overItem = _expanded && _itemRects.Any(r => r.Contains(mousePos));
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
                        {
                            _selectedIndex = i;
                        }
                        _expanded = false;
                        return;
                    }
                }

                if (mouseClicked && !overDropdown && !_itemRects.Any(r => r.Contains(mousePos)))
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
            spriteBatch.DrawString(_font, _sceneNames[_selectedIndex], new Vector2(_dropdownRect.X + 8, _dropdownRect.Y + 3), Color.White);

            // Draw arrow with fallback if unsupported
            var arrowUp = "▲";
            var arrowDown = "▼";
            var fallbackUp = "^";
            var fallbackDown = "v";

            string arrow = _expanded
                ? (_font.Characters.Contains(arrowUp[0]) ? arrowUp : fallbackUp)
                : (_font.Characters.Contains(arrowDown[0]) ? arrowDown : fallbackDown);

            var arrowSize = _font.MeasureString(arrow);
            spriteBatch.DrawString(_font, arrow, new Vector2(_dropdownRect.Right - arrowSize.X - 8, _dropdownRect.Y + 3), Color.White);

            // Draw expanded items
            if (_expanded)
            {
                for (int i = 0; i < _itemRects.Length; i++)
                {
                    var rect = _itemRects[i];
                    spriteBatch.DrawBox(rect, i == _selectedIndex ? Color.DimGray : Color.Gray, Color.White);
                    spriteBatch.DrawString(_font, _sceneNames[i], new Vector2(rect.X + 8, rect.Y + 3), Color.White);
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
                _pixel.SetData([Color.White]);
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
