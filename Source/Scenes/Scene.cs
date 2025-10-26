using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Monogram.Source.Scenes;

public enum SceneID
{
	Terrain,
	Lambert,
	Phong,
	Normals,
	Checkered,
	Texture,
	CookTorrance,
	Spotlight,
	Multilight,
	Culling,
	Projection,
	Monochrome,
	GaussianBlur
}

public class Scene(SceneID id, Shader shader, List<Model> models, Vector3? eye = null, Filter? postProcess = null)
{
	public SceneID Id { get; } = id;
	public Vector3 Eye { get; } = eye ?? new Vector3(0f, 0f, 100f);
	public List<Model> Models { get; } = models;
	public Shader Shader { get; } = shader;
	public Filter? PostProcess { get; } = postProcess;

	protected float _elapsed = 0f;

	public virtual string SceneTitle =>
		Id switch
		{
			SceneID.Terrain => "Height Map Terrain",
			SceneID.Lambert => "Lambertian Shader",
			SceneID.Phong => "Blinn-Phong Shader",
			SceneID.Normals => "Normals",
			SceneID.Checkered => "Procedural Checkers",
			SceneID.Texture => "Texture",
			SceneID.CookTorrance => "Cook-Torrance BRDF",
			SceneID.Spotlight => "Spotlight",
			SceneID.Multilight => "Multi-Light",
			SceneID.Culling => "Headgerow",
			SceneID.Projection => "Projective Texture",
			SceneID.Monochrome => "Monochrome Filter",
			SceneID.GaussianBlur => "Gaussian Blur Filter",
			_ => Id.ToString()
		};

	public virtual void Update(float elapsed)
	{
		_elapsed += elapsed;
		if (float.IsPositiveInfinity(_elapsed))
		{
			_elapsed = 0f;
		}
	}

	public virtual void Draw(GraphicsDevice device, BoundingFrustum frustum, Camera camera, RenderTarget2D? capture)
	{
		if (PostProcess != null && capture != null)
		{
			device.SetRenderTarget(capture);
		}

		foreach (var model in Models)
		{
			if (model.XnaModel != null && model.UseBoundingSphere)
			{
				BoundingSphere boundingSphere = new();
				foreach (ModelMesh mesh in model.XnaModel.Meshes)
				{
					boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
				}

				boundingSphere.Center = model.Position;
				if (frustum.Intersects(boundingSphere))
				{
					model.Draw(Shader.Effect, camera);
				}
			}
			else
			{
				model.Draw(Shader.Effect, camera);
			}
		}

		if (PostProcess != null && capture != null)
		{
			device.SetRenderTarget(null);
			PostProcess.Draw(capture);
		}
	}

	public virtual void DrawOverlay(SpriteBatch batch, SpriteFont font)
	{
		// Default: do nothing. Override in derived scenes for custom UI.
	}
}
