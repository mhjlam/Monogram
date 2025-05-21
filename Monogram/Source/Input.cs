using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Monogram;

public class Input(GameWindow window, Renderer sceneManager)
{
    private KeyboardState _oldKeyState = Keyboard.GetState();
    private KeyboardState _newKeyState;
    private MouseState _oldMouseState = Mouse.GetState();
    private MouseState _newMouseState;
    private bool _isRightMouseDown = false;
    private Point _preLockMousePosition;
    private readonly Point _windowCenter = new Point(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
    private readonly GameWindow _window = window;
    private readonly Renderer _sceneManager = sceneManager;
    private float _modelRotation = 0f;
    private readonly float _mouseSensitivity = 800.0f;
    private bool _resetMousePosition = false;
    private bool _mousePressed = false;
    private Point _mouseDownPos;
    private const int MouseMoveThreshold = 10;

    public void Update(
        float delta,
        ref bool isMouseVisible,
        Action syncOrbitToCamera,
        bool dropdownExpanded,
        bool dropdownExpandedLast,
		bool dropdownMouseOver,
		Action onSceneClick)
    {
        _newKeyState = Keyboard.GetState();
        _newMouseState = Mouse.GetState();

        // Keyboard controls
        if (_newKeyState.IsKeyDown(Keys.Escape)) Environment.Exit(0);

        if (_newKeyState.IsKeyDown(Keys.A)) _sceneManager.SceneModels.ForEach(m => m.RotateY(delta));
        if (_newKeyState.IsKeyDown(Keys.D)) _sceneManager.SceneModels.ForEach(m => m.RotateY(-delta));

        if (_newKeyState.IsKeyDown(Keys.Tab) && _oldKeyState.IsKeyUp(Keys.Tab))
        {
            _sceneManager.LoadAdjacent(_newKeyState.IsKeyDown(Keys.LeftShift));
            syncOrbitToCamera();
        }

        if (_newKeyState.IsKeyDown(Keys.Back) && _oldKeyState.IsKeyUp(Keys.Back))
        {
            _sceneManager.Camera.Reset();
            _sceneManager.SceneModels.ForEach(m => m.Reset());
            syncOrbitToCamera();
        }

        // Mouse controls for orbit camera
        bool rightDown = _newMouseState.RightButton == ButtonState.Pressed;
        if (rightDown)
        {
            if (!_isRightMouseDown)
            {
                _preLockMousePosition = new Point(_newMouseState.X, _newMouseState.Y);
                _sceneManager.Camera.SyncOrbitToCamera();
                Mouse.SetPosition(_window.ClientBounds.Left + _windowCenter.X, _window.ClientBounds.Top + _windowCenter.Y);
                _oldMouseState = Mouse.GetState();
                _isRightMouseDown = true;
                _resetMousePosition = false;
            }
            else
            {
                float dx = (_newMouseState.X - (_window.ClientBounds.Left + _windowCenter.X)) * delta * _mouseSensitivity;
                float dy = (_newMouseState.Y - (_window.ClientBounds.Top + _windowCenter.Y)) * delta * _mouseSensitivity;

                _sceneManager.Camera.Orbit(dx, dy);

                Mouse.SetPosition(_window.ClientBounds.Left + _windowCenter.X, _window.ClientBounds.Top + _windowCenter.Y);
            }
            isMouseVisible = false;
        }
        else
        {
            if (_isRightMouseDown && !_resetMousePosition)
            {
                Mouse.SetPosition(_preLockMousePosition.X, _preLockMousePosition.Y);
                _resetMousePosition = true;
            }
            _isRightMouseDown = false;
            isMouseVisible = true;
        }

        int scrollDelta = _newMouseState.ScrollWheelValue - _oldMouseState.ScrollWheelValue;
        if (scrollDelta != 0)
        {
            if (rightDown)
            {
                _sceneManager.Camera.OrbitZoom(scrollDelta);
            }
            else
            {
                const float modelScrollSensitivity = 0.5f;
                _modelRotation += scrollDelta * modelScrollSensitivity;
            }
        }

        // Mouse click scene change logic
        if (_newMouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
        {
            _mousePressed = true;
            _mouseDownPos = new Point(_newMouseState.X, _newMouseState.Y);
        }

        if (_newMouseState.LeftButton == ButtonState.Released && _oldMouseState.LeftButton == ButtonState.Pressed)
        {
            if (_mousePressed)
            {
                _mousePressed = false;
                int dx = _newMouseState.X - _mouseDownPos.X;
                int dy = _newMouseState.Y - _mouseDownPos.Y;
                if (dx * dx + dy * dy <= MouseMoveThreshold * MouseMoveThreshold)
                {
                    if (!dropdownExpanded && !dropdownExpandedLast && !dropdownMouseOver)
                    {
                        onSceneClick?.Invoke();
                    }
                }
            }
        }

        // Apply smooth model rotation
        if (Math.Abs(_modelRotation) > 0.0001f)
        {
            _sceneManager.SceneModels.ForEach(m => m.RotateY(_modelRotation * delta));
            _modelRotation *= 0.9f;
        }

        _oldKeyState = _newKeyState;
        _oldMouseState = _newMouseState;
    }
}
