using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogram.Source.Scenes;
using System.Collections.Generic;
using System.Linq;

namespace Monogram;

public class Renderer
{
    private readonly GraphicsDevice device;
    private Scene scene = null!;
    private readonly List<Scene> scenes = new();
    private readonly Camera camera;
    private readonly RenderTarget2D capture;
    private readonly BoundingFrustum frustum;

    public SceneID SceneID => scene.Id;
    public Camera Camera => camera;
    public List<Model> SceneModels => scene.Models;
    public List<string> SceneNames => scenes.Select(s => s.SceneTitle).ToList();
    public int CurrentSceneIndex => scenes.FindIndex(s => s == scene);
    public Scene? CurrentScene => scene;

    public Renderer(GraphicsDevice graphicsDevice)
    {
        device = graphicsDevice;
        capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        camera = new Camera(new Vector3(0, 10f, 100f), Vector3.Zero, (float)device.PresentationParameters.BackBufferWidth / device.PresentationParameters.BackBufferHeight);
        frustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
    }

    public void AddScene(Scene scene)
    {
        scenes.Add(scene);
        if (scenes.Count == 1)
            LoadScene(0);
    }

    public void LoadScene(int index)
    {
        if (scenes.Count == 0 || index < 0 || index >= scenes.Count) return;
        scene = scenes[index];
        scene.Models.ForEach(m => m.Reset());
        camera.SetEye(scene.Eye, true);
    }

    public void LoadAdjacent(bool prev = false)
    {
        if (scenes.Count < 2) return;
        int index = scenes.FindIndex(s => s == scene);
        int newIndex = prev
            ? (index - 1 < 0 ? scenes.Count - 1 : index - 1)
            : (index + 1 >= scenes.Count ? 0 : index + 1);
        LoadScene(newIndex);
    }

    public void Update(float elapsed)
    {
        camera.Update();
        frustum.Matrix = camera.ViewMatrix * camera.ProjectionMatrix;
        scene.Shader.Effect.Parameters["CameraPosition"]?.SetValue(camera.Position);
        scene.Update(elapsed);
    }

    public void Draw()
    {
        device.Clear(Color.Black);
        if (scene == null) return;
        scene.Draw(device, frustum, camera, capture);
    }
}
