using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Monogram.Scenes;

// Culling scene with custom draw and overlay
public class CullingScene(Shader shader, List<Model> models, float totalWidth, Vector3? eye = null) : Scene(SceneID.Culling, shader, models, eye)
{
	private int _culledCount;
	private int _totalCount;

	private readonly float _amplitude = 30f;	// How much spacing increases/decreases
	private readonly float _frequency = 0.5f;	// Oscillations per second
	private readonly float _totalWidth = totalWidth;

	public int CulledCount => _culledCount;
	public int TotalCount => _totalCount;

	public override void Update(float deltaTime)
	{
		base.Update(deltaTime);

		// Reset accumulated time after each full oscillation
		_accumulatedTime = MathF.Max(0f, _accumulatedTime >= 1f / _frequency ? 0f : _accumulatedTime);

		float oscillate = MathF.Sin(_accumulatedTime * MathF.Tau * _frequency);
		float width = _totalWidth + _amplitude * (0.5f + 0.5f * oscillate) * (Models.Count - 1);
		float spacing = width / (Models.Count - 1);
		float startX = -width / 2f;

		for (int i = 0; i < Models.Count; i++)
		{
			var model = Models[i];
			float x = startX + i * spacing;
			model.Position = new Vector3(x, 0f, 0f);
		}
	}

	public override void Draw(GraphicsDevice device, BoundingFrustum frustum, Camera camera, RenderTarget2D? capture)
	{
		_culledCount = 0;
		_totalCount = 0;

		if (PostProcess != null && capture != null)
			device.SetRenderTarget(capture);

		foreach (var model in Models)
		{
			if (model.XnaModel != null)
			{
				// Compute the model's bounding box in world space
				BoundingBox? boundingBox = BoundBox.GetModelBoundingBox(model);
				if (boundingBox.HasValue)
				{
					_totalCount++;
					bool visible = frustum.Intersects(boundingBox.Value);
					if (visible)
						model.Draw(this, camera);
					else
						_culledCount++;

					// Visualize bounding box
					BoundBox.Draw(device, boundingBox.Value, camera, Color.White);
				}
			}
			else
			{
				model.Draw(this, camera);
				_totalCount++;
			}
		}

		if (PostProcess != null && capture != null)
		{
			device.SetRenderTarget(null);
			PostProcess.Draw(capture);
		}
	}

	public override void DrawOverlay(SpriteBatch batch, SpriteFont font)
	{
		base.DrawOverlay(batch, font);
		string cullText = $"Culled: {_culledCount} / {_totalCount}";
		batch.DrawString(font, cullText, new Vector2(20f, 48f), Color.Orange);
	}
}
