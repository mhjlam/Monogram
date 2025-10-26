using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Monogram;

public class TerrainModel : Model
{
	private Vertex[] _vertices;
	
	private readonly int _primitiveCount;
	private readonly float minY, maxY, rangeY;
	private readonly float minX, maxX, rangeX;
	private readonly float minZ, maxZ, rangeZ;
	private readonly IndexBuffer _indexBuffer;
	private readonly VertexBuffer _vertexBuffer;
	
	public TerrainModel(GraphicsDevice device, Texture2D heightmap, Vector3? position = null, Vector3? rotation = null)
		: base(null!, position ?? Vector3.Zero, rotation, new Vector3(1f, 0.25f, 1f))
	{
		int width = heightmap.Width;
		int height = heightmap.Height;
		int halfWidth = width / 2;

		_vertices = new Vertex[width * height];
		var indices = new short[(width - 1) * (height - 1) * 6];

		var bitmap = new Color[width * height];
		heightmap.GetData(bitmap);

		// Compute vertices
		for (int x = 0; x < width; ++x)
		{
			for (int z = 0; z < height; ++z)
			{
				int v = x + z * width;
				float h = bitmap[x + width * z].R;
				_vertices[v].Position = new Vector3(-halfWidth + x, h, -halfWidth + z);
				_vertices[v].Color = new Color(0.025f * h, 0.5f, 15f / (h * 2.5f));
			}
		}

		// Compute indices
		for (int x = 0, c = 0; x < width - 1; ++x)
		{
			for (int y = 0; y < height - 1; ++y)
			{
				short tl = (short)(x + y * width);
				short tr = (short)((x + 1) + y * width);
				short bl = (short)(x + (y + 1) * width);
				short br = (short)((x + 1) + (y + 1) * width);

				indices[c++] = tl;
				indices[c++] = tr;
				indices[c++] = br;

				indices[c++] = br;
				indices[c++] = bl;
				indices[c++] = tl;
			}
		}

		// Compute normals
		for (int i = 0; i < _vertices.Length; ++i)
		{
			_vertices[i].Normal = Vector3.Zero;
		}

		for (int i = 0; i < indices.Length / 3; ++i)
		{
			int index1 = indices[i * 3];
			int index2 = indices[i * 3 + 1];
			int index3 = indices[i * 3 + 2];

			Vector3 side1 = _vertices[index1].Position - _vertices[index3].Position;
			Vector3 side2 = _vertices[index1].Position - _vertices[index2].Position;
			Vector3 normal = Vector3.Cross(side1, side2);

			_vertices[index1].Normal += normal;
			_vertices[index2].Normal += normal;
			_vertices[index3].Normal += normal;
		}

		for (int i = 0; i < _vertices.Length; ++i)
		{
			_vertices[i].Normal = Vector3.Normalize(_vertices[i].Normal);
		}

		_vertexBuffer = new VertexBuffer(device, Vertex.VertexDeclaration, _vertices.Length, BufferUsage.WriteOnly);
		_vertexBuffer.SetData(_vertices);

		_indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
		_indexBuffer.SetData(indices);

		_primitiveCount = indices.Length / 3;

		// Compute min/max for normalization ONCE
		minY = float.MaxValue; maxY = float.MinValue;
		minX = float.MaxValue; maxX = float.MinValue;
		minZ = float.MaxValue; maxZ = float.MinValue;

		for (int i = 0; i < _vertices.Length; ++i)
		{
			var pos = _vertices[i].Position;
			if (pos.Y < minY) { minY = pos.Y; }
			if (pos.Y > maxY) { maxY = pos.Y; }
			if (pos.X < minX) { minX = pos.X; }
			if (pos.X > maxX) { maxX = pos.X; }
			if (pos.Z < minZ) { minZ = pos.Z; }
			if (pos.Z > maxZ) { maxZ = pos.Z; }
		}

		rangeY = Math.Max(1e-5f, maxY - minY);
		rangeX = Math.Max(1e-5f, maxX - minX);
		rangeZ = Math.Max(1e-5f, maxZ - minZ);
	}

	public void Update(Vertex[] vertices)
	{
		if (_vertexBuffer == null)
		{
			return;
		}
		_vertexBuffer.SetData(vertices);
	}

	public override void Draw(Effect effect, Camera camera)
	{
		// Assume TransformationMatrix already includes scale/rotation/translation
		var parameters = effect.Parameters;
		parameters["World"]?.SetValue(TransformationMatrix);
		parameters["View"]?.SetValue(camera.ViewMatrix);
		parameters["Projection"]?.SetValue(camera.ProjectionMatrix);

		var device = _vertexBuffer.GraphicsDevice;

		// Only change rasterizer state if needed
		var previousRasterizerState = device.RasterizerState;
		if (device.RasterizerState != RasterizerState.CullNone)
		{
			device.RasterizerState = RasterizerState.CullNone;
		}

		device.SetVertexBuffer(_vertexBuffer);
		device.Indices = _indexBuffer;

		foreach (var pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _primitiveCount);
		}

		// Restore rasterizer state only if it was changed
		if (device.RasterizerState != previousRasterizerState)
		{
			device.RasterizerState = previousRasterizerState;
		}
	}

	public Vertex[] Vertices
	{
		get => _vertices;
		set => _vertices = value;
	}

	public (float normX, float normY, float normZ) NormalizedPosition(Vector3 position)
	{
		float normX = (position.X - minX) / rangeX;
		float normY = (position.Y - minY) / rangeY;
		float normZ = (position.Z - minZ) / rangeZ;
		return (normX, normY, normZ);
	}
}
