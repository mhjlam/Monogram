using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monogram.Source.Scenes;
using System;

namespace Monogram.Scenes;

public class TerrainScene : Scene
{
	private readonly TerrainModel _terrainModel;
	private readonly BasicEffect _basicEffect;

	private float _scanPhase = 0f; // [0,1] progress in current direction
	private float _scanDuration = 2f; // seconds per scan

	private enum ScanDirection
	{
		TopToBottom,
		BottomToTop,
		LeftToRight,
		RightToLeft,
		BackToFront,
		FrontToBack
	}
	private ScanDirection _scanDirection = ScanDirection.TopToBottom;

	private static readonly (float height, Color color)[] ColorStops =
	{
		(1.00f, Color.White),                        // Snow
		(0.80f, new Color(120, 220, 120)),           // Bright green
		(0.60f, new Color(34, 139, 34)),             // Dark green
		(0.40f, new Color(210, 180, 60)),            // Yellowish (sand/grass)
		(0.00f, new Color(30, 60, 180))              // Blue (water)
	};

	public TerrainScene(GraphicsDevice graphicsDevice, Texture2D heightMap, Vector3? eye = null)
		: base(SceneID.Terrain, new Shader(new BasicEffect(graphicsDevice)), [], eye)
	{
		_terrainModel = new TerrainModel(graphicsDevice, heightMap);

		_basicEffect = new BasicEffect(graphicsDevice)
		{
			VertexColorEnabled = true,
			LightingEnabled = true,
			AmbientLightColor = new Vector3(0.3f)
		};
		_basicEffect.DirectionalLight0.Enabled = true;
		_basicEffect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
		_basicEffect.DirectionalLight0.Direction = Vector3.Down;
	}

	public override void Update(float deltaTime)
	{
		base.Update(deltaTime);

		_scanPhase = _elapsed / _scanDuration;
		if (_scanPhase >= 1f)
		{
			_scanPhase = 0f;
			_elapsed = 0f;
			_scanDirection = (ScanDirection)(((int)_scanDirection + 1) % 6);
		}

		 // Replace the for-loop in Update with a parallel loop for large terrains
		var vertices = _terrainModel.Vertices;
		int vertexCount = vertices.Length;

		System.Threading.Tasks.Parallel.For(0, vertexCount, i =>
		{
			var pos = vertices[i].Position;
			var (normX, normY, normZ) = _terrainModel.NormalizedPosition(pos);

			Color terrainColor = InterpolateTerrainColor(normY, ColorStops);

			float scanDist = 1f;
			float scanlineWidth = GetScanlineWidth(_scanDirection);
			switch (_scanDirection)
			{
				case ScanDirection.TopToBottom:
					scanDist = Math.Abs(normY - (1f - _scanPhase));
					break;
				case ScanDirection.BottomToTop:
					scanDist = Math.Abs(normY - _scanPhase);
					break;
				case ScanDirection.LeftToRight:
					scanDist = Math.Abs(normX - _scanPhase);
					break;
				case ScanDirection.RightToLeft:
					scanDist = Math.Abs(normX - (1f - _scanPhase));
					break;
				case ScanDirection.BackToFront:
					scanDist = Math.Abs(normZ - _scanPhase);
					break;
				case ScanDirection.FrontToBack:
					scanDist = Math.Abs(normZ - (1f - _scanPhase));
					break;
			}

			float scanIntensity = MathHelper.Clamp(1f - (scanDist / scanlineWidth), 0f, 1f);
			scanIntensity = (float)Math.Pow(scanIntensity, 0.5);

			Color scanColor = Color.Red;
			float blend = scanIntensity;

			vertices[i].Color = BlendScanlineColor(terrainColor, scanColor, blend);
		});
		_terrainModel.Update(vertices);
	}

	public override void Draw(GraphicsDevice graphicsDevice, BoundingFrustum frustum, Camera camera, RenderTarget2D? capture)
	{
		// Set up the effect for the terrain model and call its Draw
		_basicEffect.World = Matrix.Identity * _terrainModel.TransformationMatrix;
		_basicEffect.View = camera.ViewMatrix;
		_basicEffect.Projection = camera.ProjectionMatrix;

		// Use a temporary Scene to pass the effect to the model
		_terrainModel.Draw(_basicEffect, camera);
	}

	// Helper for multi-stop color interpolation
	private static Color InterpolateTerrainColor(float normH, (float height, Color color)[] stops)
	{
		for (int s = 0; s < stops.Length - 1; ++s)
		{
			var (h1, c1) = stops[s];
			var (h2, c2) = stops[s + 1];
			if (normH <= h1 && normH >= h2)
			{
				float t = (normH - h2) / (h1 - h2);
				return new Color(
					(byte)(c1.R * t + c2.R * (1 - t)),
					(byte)(c1.G * t + c2.G * (1 - t)),
					(byte)(c1.B * t + c2.B * (1 - t))
				);
			}
		}
		// If out of range, clamp to the closest stop
		return normH > stops[0].height ? stops[0].color : stops[^1].color;
	}

	private static Color BlendScanlineColor(Color baseColor, Color scanColor, float blend)
	{
		byte r = (byte)MathHelper.Clamp(baseColor.R * (1 - blend) + scanColor.R * blend, 0, 255);
		byte g = (byte)MathHelper.Clamp(baseColor.G * (1 - blend) + scanColor.G * blend, 0, 255);
		byte b = (byte)MathHelper.Clamp(baseColor.B * (1 - blend) + scanColor.B * blend, 0, 255);
		return new Color(r, g, b);
	}

	private static float GetScanlineWidth(ScanDirection direction) => direction switch
	{
		ScanDirection.TopToBottom or ScanDirection.BottomToTop => 0.04f,
		_ => 0.02f
	};
}
