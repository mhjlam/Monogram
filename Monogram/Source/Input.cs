using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Monogram;

public class Input
{
    private KeyboardState _oldKeyState;
    private KeyboardState _newKeyState;
    private MouseState _oldMouseState;
    private MouseState _newMouseState;
    private bool _isRightMouseDown = false;
    private Point _preLockMousePosition;
    private readonly Point _windowCenter;
    private readonly GameWindow _window;
    private readonly Scener _sceneManager;
    private float _modelRotation = 0f;

    public Input(GameWindow window, Scener sceneManager)
    {
        _window = window;
        _sceneManager = sceneManager;
        _windowCenter = new Point(window.ClientBounds.Width / 2, window.ClientBounds.Height / 2);
        _oldKeyState = Keyboard.GetState();
        _oldMouseState = Mouse.GetState();
    }

    public void Update(float delta, ref bool isMouseVisible, Action syncOrbitToCamera)
    {
        _newKeyState = Keyboard.GetState();
        _newMouseState = Mouse.GetState();

        // Keyboard controls
        if (_newKeyState.IsKeyDown(Keys.Escape))
            Environment.Exit(0);

        if (_sceneManager.SceneID != SceneID.Culling)
        {
            if (_newKeyState.IsKeyDown(Keys.A)) _sceneManager.SceneModels.ForEach(m => m.RotateY(0.05f * delta));
            if (_newKeyState.IsKeyDown(Keys.D)) _sceneManager.SceneModels.ForEach(m => m.RotateY(-0.05f * delta));
        }
        else
        {
            if (_newKeyState.IsKeyDown(Keys.A)) _sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Left * delta));
            if (_newKeyState.IsKeyDown(Keys.D)) _sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Right * delta));
            if (_newKeyState.IsKeyDown(Keys.W)) _sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Up * delta));
            if (_newKeyState.IsKeyDown(Keys.S)) _sceneManager.SceneModels.ForEach(m => m.Translate(Vector3.Down * delta));
        }

        if (_newKeyState.IsKeyDown(Keys.R) && _oldKeyState.IsKeyUp(Keys.R))
        {
            _sceneManager.Camera.Reset();
            _sceneManager.SceneModels.ForEach(m => m.Reset());
            syncOrbitToCamera();
        }

        if (_newKeyState.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
        {
            _sceneManager.LoadAdjacent(_newKeyState.IsKeyDown(Keys.LeftShift));
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
            }
            else
            {
                int dx = _newMouseState.X - (_window.ClientBounds.Left + _windowCenter.X);
                int dy = _newMouseState.Y - (_window.ClientBounds.Top + _windowCenter.Y);

                _sceneManager.Camera.Orbit(dx, dy);

                Mouse.SetPosition(_window.ClientBounds.Left + _windowCenter.X, _window.ClientBounds.Top + _windowCenter.Y);
            }
            isMouseVisible = false;
        }
        else
        {
            if (_isRightMouseDown)
            {
                Mouse.SetPosition(_preLockMousePosition.X, _preLockMousePosition.Y);
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
                const float modelScrollSensitivity = 0.005f;
                _modelRotation += scrollDelta * modelScrollSensitivity;
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
