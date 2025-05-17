using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Monogram;

public enum SceneID
{
	Terrain,
	Lambert,
	Phong,
	Normals,
	Checkered,
	Wood,
	CookTorrance,
	Spotlight,
	Multilight,
	Culling,
	Projection,
	Monochrome,
	GaussianBlur
}

public struct SceneDefinition
{
	public SceneID Id;
	public Vector3 Eye;
	public List<Model> Models;
	public Shader Shader;
	public Filter? PostProcess;

	public readonly string SceneTitle =>
		Id switch
		{
			SceneID.Terrain => "Height Map Terrain",
			SceneID.Lambert => "Lambertian Shader",
			SceneID.Phong => "Blinn-Phong Shader",
			SceneID.Normals => "Normals",
			SceneID.Checkered => "Procedural Checkers",
			SceneID.Wood => "Wood Texture",
			SceneID.CookTorrance => "Cook-Torrance BRDF",
			SceneID.Spotlight => "Spotlight",
			SceneID.Multilight => "Multi-Light",
			SceneID.Culling => "Frustum Culling",
			SceneID.Projection => "Projective Texture",
			SceneID.Monochrome => "Monochrome Filter",
			SceneID.GaussianBlur => "Gaussian Blur Filter",
			_ => Id.ToString()
		};
}

public class Scener
{
	private readonly SpriteFont font;
	private readonly SpriteBatch batch;
	private readonly GraphicsDevice device;

	private SceneDefinition scene;
	private readonly List<SceneDefinition> scenes;

	private readonly Camera camera;
	private readonly RenderTarget2D capture;
	private readonly BoundingFrustum frustum;

	public SceneID SceneID => scene.Id;
	public Camera Camera => camera;
	public List<Model> SceneModels => scene.Models;

	public Scener(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, SpriteFont spriteFont)
	{
		device = graphicsDevice;
		batch = spriteBatch;
		font = spriteFont;

		scenes = [];
		capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, device.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);

		camera = new Camera(new Vector3(0, 10f, 100f), Vector3.Zero, (float)device.PresentationParameters.BackBufferWidth / device.PresentationParameters.BackBufferHeight);
		frustum = new BoundingFrustum(camera.ViewMatrix * camera.ProjectionMatrix);
	}

	public void AddScene(SceneID id, Shader shader, List<Model> models, Vector3? eye = null, Filter? postProcess = null)
	{
		scenes.Add(new SceneDefinition
		{
			Id = id,
			Models = models,
			Shader = shader,
			Eye = eye ?? new Vector3(0f, 0f, 100f),
			PostProcess = postProcess
		});

		if (scenes.Count == 1) LoadScene(0);
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
		int index = scenes.FindIndex(s => s.Equals(scene));
		int newIndex = prev
			? (index - 1 < 0 ? scenes.Count - 1 : index - 1)
			: (index + 1 >= scenes.Count ? 0 : index + 1);
		LoadScene(newIndex);
	}

	public void Update()
	{
		camera.Update();
		frustum.Matrix = camera.ViewMatrix * camera.ProjectionMatrix;
		scene.Shader.Effect.Parameters["CameraPosition"]?.SetValue(camera.Position);

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

		if (scene.PostProcess != null)
			device.SetRenderTarget(capture);

		device.Clear(Color.Black);

		foreach (var model in scene.Models)
		{
			if (model.XnaModel != null)
			{
				BoundingSphere boundingSphere = new();
				foreach (ModelMesh mesh in model.XnaModel.Meshes)
					boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);

				boundingSphere.Center = model.Position;
				if (frustum.Intersects(boundingSphere))
					model.Draw(scene, camera);
			}
			else
			{
				model.Draw(scene, camera);
			}
		}

		if (scene.PostProcess != null)
		{
			device.SetRenderTarget(null);
			scene.PostProcess.Draw(capture);
		}

		batch.Begin();
		batch.DrawString(font, scene.SceneTitle, new Vector2(20f, 20f), Color.White);
		batch.End();

		device.BlendState = BlendState.Opaque;
		device.DepthStencilState = DepthStencilState.Default;
		device.SamplerStates[0] = SamplerState.LinearWrap;
	}
}
