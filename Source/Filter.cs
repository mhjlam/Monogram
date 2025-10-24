using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monogram;

public class Filter
{
	protected GraphicsDevice device;
	protected SpriteBatch spritebatch;
	protected Effect effect;

	public Filter(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect postProcessEffect)
	{
		device = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
		spritebatch = spriteBatch ?? throw new ArgumentNullException(nameof(spriteBatch));
		effect = postProcessEffect ?? throw new ArgumentNullException(nameof(postProcessEffect));
	}

	public virtual void Draw(RenderTarget2D renderTarget)
	{
		if (renderTarget is null) return;

		device.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
		spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, null, null, effect);
		spritebatch.Draw(renderTarget, Vector2.Zero, Color.White);
		spritebatch.End();
	}
}

public class GaussianBlur : Filter
{
	private readonly float sigma;
	private readonly float[] horizontalWeights, verticalWeights;
	private readonly Vector2[] horizontalOffsets, verticalOffsets;
	private readonly RenderTarget2D capture;

	public GaussianBlur(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Effect postProcessEffect, float blurCoefficient = 2.0f)
		: base(graphicsDevice, spriteBatch, postProcessEffect)
	{
		sigma = blurCoefficient;

		float centerX = 1f / device.Viewport.Width;
		float centerY = 1f / device.Viewport.Height;

		CalculateParameters(centerX, 0, out horizontalWeights, out horizontalOffsets);
		CalculateParameters(0, centerY, out verticalWeights, out verticalOffsets);

		capture = new RenderTarget2D(device, device.Viewport.Width, device.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24);
	}

	public override void Draw(RenderTarget2D renderTarget)
	{
		effect.Parameters["Offsets"].SetValue(horizontalOffsets);
		effect.Parameters["Weights"].SetValue(horizontalWeights);

		device.SetRenderTarget(capture);
		base.Draw(renderTarget);
		device.SetRenderTarget(null);

		effect.Parameters["Offsets"].SetValue(verticalOffsets);
		effect.Parameters["Weights"].SetValue(verticalWeights);

		base.Draw(capture);
	}

	private void CalculateParameters(float w, float h, out float[] weights, out Vector2[] offsets)
	{
		int limit = (int)(3 * sigma);
		if (limit % 2 == 0) { limit++; }

		weights = new float[limit];
		offsets = new Vector2[limit];

		weights[0] = GaussianFunction(0);
		offsets[0] = Vector2.Zero;

		float totalWeight = weights[0];

		for (int i = 0; i < limit / 2; ++i)
		{
			float weight = GaussianFunction(i + 1);
			totalWeight += weight * 2;
			Vector2 offset = new Vector2(w, h) * (i * 2 + 1.5f);

			weights[i * 2 + 1] = weight;
			weights[i * 2 + 2] = weight;
			offsets[i * 2 + 1] = offset;
			offsets[i * 2 + 2] = -offset;
		}

		for (int i = 0; i < weights.Length; i++)
		{
			weights[i] /= totalWeight;
		}
	}

	private float GaussianFunction(float x)
	{
		float sigma2 = sigma * sigma;
		return (float)((1.0f / Math.Sqrt(2 * Math.PI * sigma2)) * Math.Exp(-(x * x) / (2 * sigma2)));
	}
}
