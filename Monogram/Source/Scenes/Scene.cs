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

// Base Scene class
public class Scene(SceneID id, Shader shader, List<Model> models, Vector3? eye = null, Filter? postProcess = null)
{
	public SceneID Id { get; } = id;
	public Vector3 Eye { get; } = eye ?? new Vector3(0f, 0f, 100f);
	public List<Model> Models { get; } = models;
	public Shader Shader { get; } = shader;
	public Filter? PostProcess { get; } = postProcess;

	protected float _accumulatedTime = 0f;

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

	public virtual void Update(float deltaTime)
	{
		_accumulatedTime += deltaTime;
		if (float.IsPositiveInfinity(_accumulatedTime))
			_accumulatedTime = 0f;
	}

	public virtual void Draw(GraphicsDevice device, BoundingFrustum frustum, Camera camera, RenderTarget2D? capture)
	{
		// If postprocess, render to capture target
		if (PostProcess != null && capture != null)
			device.SetRenderTarget(capture);

		foreach (var model in Models)
		{
			if (model.XnaModel != null)
			{
				BoundingSphere boundingSphere = new();
				foreach (ModelMesh mesh in model.XnaModel.Meshes)
					boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
				
				boundingSphere.Center = model.Position;
				if (frustum.Intersects(boundingSphere))
					model.Draw(this, camera);
			}
			else
			{
				model.Draw(this, camera);
			}
		}

		// If postprocess, draw to backbuffer
		if (PostProcess != null && capture != null)
		{
			device.SetRenderTarget(null);
			PostProcess.Draw(capture);
		}
	}

	public virtual void DrawOverlay(SpriteBatch batch, SpriteFont font)
	{
		batch.DrawString(font, SceneTitle, new Vector2(20f, 20f), Color.White);
	}
}
