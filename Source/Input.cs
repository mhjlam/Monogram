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
    private readonly Point _windowCenter = new(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
    private readonly GameWindow _window = window;
    private readonly Renderer _sceneManager = sceneManager;
    private float _modelRotation = 0f;
    private const float MouseSensitivity = 800.0f;
    private bool _resetMousePosition = false;
    private bool _mousePressed = false;
    private Point _mouseDownPos;
    private const int MouseMoveThreshold = 10;
    private const float ModelScrollSensitivity = 0.5f;
    private const float ModelRotationSmoothing = 0.9f;

    public void Update(
        float delta,
        ref bool isMouseVisible,
        Action syncOrbitToCamera,
        bool dropdownExpanded,
        bool dropdownExpandedLast,
        bool dropdownMouseOver)
    {
        _newKeyState = Keyboard.GetState();
        _newMouseState = Mouse.GetState();

        HandleKeyboard(delta, syncOrbitToCamera);
        HandleMouse(delta, ref isMouseVisible);

        HandleMouseClick(dropdownExpanded, dropdownExpandedLast, dropdownMouseOver);

        // Apply smooth model rotation
        if (Math.Abs(_modelRotation) > 0.0001f)
        {
            _sceneManager.SceneModels.ForEach(m => m.RotationY += _modelRotation * delta);
            _modelRotation *= ModelRotationSmoothing;
        }

        _oldKeyState = _newKeyState;
        _oldMouseState = _newMouseState;
    }

    private void HandleKeyboard(float delta, Action syncOrbitToCamera)
    {
        if (_newKeyState.IsKeyDown(Keys.Escape)) Environment.Exit(0);

        if (_newKeyState.IsKeyDown(Keys.A)) _sceneManager.SceneModels.ForEach(m => m.RotationY += delta);
        if (_newKeyState.IsKeyDown(Keys.D)) _sceneManager.SceneModels.ForEach(m => m.RotationY += -delta);

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
    }

    private void HandleMouse(float delta, ref bool isMouseVisible)
    {
        bool rightDown = _newMouseState.RightButton == ButtonState.Pressed;
        if (rightDown)
        {
            if (!_isRightMouseDown)
            {
                _preLockMousePosition = new(_newMouseState.X, _newMouseState.Y);
                _sceneManager.Camera.SyncOrbitToCamera();
                Mouse.SetPosition(_window.ClientBounds.Left + _windowCenter.X, _window.ClientBounds.Top + _windowCenter.Y);
                _oldMouseState = Mouse.GetState();
                _isRightMouseDown = true;
                _resetMousePosition = false;
            }
            else
            {
                float dx = (_newMouseState.X - (_window.ClientBounds.Left + _windowCenter.X)) * delta * MouseSensitivity;
                float dy = (_newMouseState.Y - (_window.ClientBounds.Top + _windowCenter.Y)) * delta * MouseSensitivity;

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
                _modelRotation += scrollDelta * ModelScrollSensitivity;
            }
        }
    }

    private void HandleMouseClick(bool dropdownExpanded, bool dropdownExpandedLast, bool dropdownMouseOver)
    {
        if (_newMouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released)
        {
            _mousePressed = true;
            _mouseDownPos = new(_newMouseState.X, _newMouseState.Y);
        }
    }
}
