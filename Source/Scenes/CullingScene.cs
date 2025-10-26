using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monogram.Source.Scenes;
using System;
using System.Collections.Generic;

namespace Monogram.Scenes;

public class CullingScene(Shader shader, List<Model> models, float totalWidth, Vector3? eye = null) : Scene(SceneID.Culling, shader, models, eye)
{
	private int _culledCount;
	private int _totalCount;

	private readonly float _amplitude = 30f;
	private readonly float _frequency = 0.5f;
	private readonly float _totalWidth = totalWidth;
	private readonly Dictionary<Model, BoundingBox?> _boundingBoxCache = [];

	public override void Update(float deltaTime)
	{
		base.Update(deltaTime);

		_elapsed = MathF.Max(0f, _elapsed >= 1f / _frequency ? 0f : _elapsed);

		float oscillate = MathF.Sin(_elapsed * MathF.Tau * _frequency);
		float width = _totalWidth + _amplitude * (0.5f + 0.5f * oscillate) * (Models.Count - 1);
		float spacing = width / (Models.Count - 1);
		float startX = -width / 2f;

		for (int i = 0; i < Models.Count; i++)
		{
			var model = Models[i];
			float x = startX + i * spacing;
			model.Position = new Vector3(x, 0f, 0f);
		}

		_boundingBoxCache.Clear(); // Invalidate cache after moving models
	}

	public override void Draw(GraphicsDevice device, BoundingFrustum frustum, Camera camera, RenderTarget2D? capture)
	{
		_culledCount = 0;
		_totalCount = 0;

		if (PostProcess != null && capture != null)
		{
			device.SetRenderTarget(capture);
		}

		foreach (var model in Models)
		{
			if (model.XnaModel != null)
			{
				var boundingBox = BoundBox.GetModelBoundingBox(model);
				if (boundingBox.HasValue)
				{
					_totalCount++;
					if (frustum.Intersects(boundingBox.Value))
					{
						model.Draw(Shader.Effect, camera);
					}
					else
					{
						_culledCount++;
					}

					BoundBox.Draw(device, boundingBox.Value, camera, Color.White);
				}
			}
			else
			{
				model.Draw(Shader.Effect, camera);
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
		string text = $"Culled: {_culledCount} / {_totalCount}";

		// Get viewport height for bottom alignment
		int screenHeight = batch.GraphicsDevice.Viewport.Height;
		Vector2 textSize = font.MeasureString(text);
		Vector2 position = new(24, screenHeight - textSize.Y - 24);

		batch.DrawString(font, text, position, Color.Orange);
	}
}
