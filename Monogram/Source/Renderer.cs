using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Monogram;

public class Renderer
{
	private readonly SpriteFont font;
	private readonly SpriteBatch batch;
	private readonly GraphicsDevice device;

	private Scene scene = null!; // Initialize with null-forgiving operator to satisfy the compiler.
	private readonly List<Scene> scenes;

	private readonly Camera camera;
	private readonly RenderTarget2D capture;
	private readonly BoundingFrustum frustum;

	public SceneID SceneID => scene.Id;
	public Camera Camera => camera;
	public List<Model> SceneModels => scene.Models;

	public Renderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont)
	{
		device = graphicsDevice;
		batch = spriteBatch;
		font = spriteFont;

		scenes = [];
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
		if (scenes.Count == 0) return;
		if (scenes.Count == 1) index = 0;

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

	public void Update(float deltaTime)
	{
		camera.Update();
		frustum.Matrix = camera.ViewMatrix * camera.ProjectionMatrix;
		scene.Shader.Effect.Parameters["CameraPosition"]?.SetValue(camera.Position);

		scene.Update(deltaTime);

		if (scene.Shader.Effect.Parameters["ProjectorViewProjection"] != null)
		{
			Vector3 projectorPosition = scene.Shader.Effect.Parameters["ProjectorPosition"] != null
				? scene.Shader.Effect.Parameters["ProjectorPosition"].GetValueVector3()
				: new Vector3(0f, 20f, 30f);

			Matrix projectorViewProjection =
				Matrix.Identity * SceneModels.First().TransformationMatrix *
				Matrix.CreateLookAt(projectorPosition, new Vector3(0f, 10f, 0f), Vector3.Up) *
				Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(20f), 1f, 1f, 100f);

			scene.Shader.Effect.Parameters["ProjectorViewProjection"].SetValue(projectorViewProjection);
		}
	}

	public void Draw()
	{
		if (scenes.Count == 0 || scene.Models.Count == 0) return;

		device.Clear(Color.Black);

		// Scene-specific 3D rendering (including postprocess if needed)
		scene.Draw(device, frustum, camera, capture);

		// Overlay (scene info, stats, etc)
		batch.Begin();
		scene.DrawOverlay(batch, font);
		batch.End();

		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		device.SamplerStates[0] = SamplerState.LinearWrap;
	}
}
