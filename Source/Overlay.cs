using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monogram;

public class Overlay(Game game, Renderer renderer, SpriteFont font, SpriteBatch batch, GameWindow window) : DrawableGameComponent(game)
{
    private readonly Renderer _renderer = renderer;
    private readonly GameWindow _window = window;
    private readonly SpriteFont _font = font;
    private readonly SpriteBatch _batch = batch;
    private readonly SceneDropdown _dropdown = new(renderer.SceneNames, font, 20, 20);

    private int _frameRate;
    private int _frameCounter;
    private int _secondsPassed;
    private int _lastDropdownIndex = 0;

    public int SelectedSceneIndex
    {
        get => _dropdown.SelectedIndex;
        set
        {
            _lastDropdownIndex = value;
            _dropdown.SelectedIndex = value;
        }
    }
    public bool DropdownExpanded => _dropdown.Expanded;
    public bool DropdownMouseOver => _dropdown.IsMouseOver;

    public void UpdateOverlay(MouseState mouse, MouseState prevMouse)
    {
        bool dropdownWasExpanded = _dropdown.Expanded;
        _dropdown.Update(mouse, prevMouse);

        // Only change scene if the dropdown just closed and the index changed
        if (!_dropdown.Expanded && dropdownWasExpanded && _dropdown.SelectedIndex != _lastDropdownIndex)
        {
            _renderer.LoadScene(_dropdown.SelectedIndex);
            _renderer.Camera.SyncOrbitToCamera();
        }
        _lastDropdownIndex = _dropdown.SelectedIndex;
    }

    public override void Update(GameTime gameTime)
    {
        if (_secondsPassed != gameTime.TotalGameTime.Seconds)
        {
            _frameRate = _frameCounter;
            _secondsPassed = gameTime.TotalGameTime.Seconds;
            _frameCounter = 0;
        }
        _frameCounter++;
        base.Update(gameTime);
    }

    public void Draw()
    {
        _batch.Begin();
        _dropdown.Draw(_batch);

        // Draw FPS counter at top-right
        string fpsText = $"FPS: {_frameRate}";
        Vector2 textSize = _font.MeasureString(fpsText);
        Vector2 position = new(_window.ClientBounds.Width - textSize.X - 16, 16);
        _batch.DrawString(_font, fpsText, position, Color.Yellow);

        // Allow scene to draw additional UI
        _renderer.CurrentScene?.DrawOverlay(_batch, _font);

        _batch.End();
    }
}
