using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Monogram;

public class BoundBox(Game game, GraphicsDevice device, Camera camera, BoundingBox box, Color color) : DrawableGameComponent(game)
{
	private BoundingBox _box = box;
	
	private readonly Color _color = color;
	private readonly Camera _camera = camera;
	private readonly GraphicsDevice _device = device;

	// Static resources for drawing
	private static BasicEffect? effect = null;
	private static VertexBuffer? vertexBuffer = null;
	private static IndexBuffer? indexBuffer = null;

	public void UpdateBox(BoundingBox newBox)
	{
		_box = newBox;
	}

	public override void Draw(GameTime gameTime)
	{
		base.Draw(gameTime);
		Draw(_device, _box, _camera, _color);
	}

	public static BoundingBox? GetModelBoundingBox(Model model)
	{
		var xnaModel = model.XnaModel;
		if (xnaModel == null) return null;

		BoundingBox? boundingBox = null;
		foreach (var mesh in xnaModel.Meshes)
		{
			// Try to get mesh bounding box from tag, or compute from vertices
			BoundingBox meshBox;
			if (mesh.Tag is BoundingBox tagBox)
			{
				meshBox = tagBox;
			}
			else
			{
				// Compute from mesh vertices
				var vertices = new List<Vector3>();
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					var vertexBuffer = part.VertexBuffer;
					int vertexStride = part.VertexBuffer.VertexDeclaration.VertexStride;
					int vertexCount = part.NumVertices;
					var vertexData = new byte[vertexStride * vertexCount];
					vertexBuffer.GetData(part.VertexOffset * vertexStride, vertexData, 0, vertexData.Length);

					for (int i = 0; i < vertexCount; i++)
					{
						float x = System.BitConverter.ToSingle(vertexData, i * vertexStride);
						float y = System.BitConverter.ToSingle(vertexData, i * vertexStride + 4);
						float z = System.BitConverter.ToSingle(vertexData, i * vertexStride + 8);
						vertices.Add(new Vector3(x, y, z));
					}
				}
				if (vertices.Count == 0) continue;

				meshBox = BoundingBox.CreateFromPoints(vertices);
				mesh.Tag = meshBox; // Cache for next time
			}

			// Transform the bounding box to world space
			var transformedBox = TransformBoundingBox(meshBox, model.TransformationMatrix);
			boundingBox = boundingBox == null
				? transformedBox
				: BoundingBox.CreateMerged(boundingBox.Value, transformedBox);
		}
		return boundingBox;
	}

	public static BoundingBox TransformBoundingBox(BoundingBox box, Matrix transform)
	{
		var corners = box.GetCorners();
		var transformed = new Vector3[corners.Length];
		for (int i = 0; i < corners.Length; i++)
			transformed[i] = Vector3.Transform(corners[i], transform);
		return BoundingBox.CreateFromPoints(transformed);
	}

	public static void Draw(GraphicsDevice device, BoundingBox box, Camera camera, Color color)
	{
		var corners = box.GetCorners();
		var vertices = corners.Select(c => new VertexPositionColor(c, color)).ToArray();

		short[] indices =
		{
			0, 1, 1, 2, 2, 3, 3, 0,
			4, 5, 5, 6, 6, 7, 7, 4,
			0, 4, 1, 5, 2, 6, 3, 7
		};

		effect ??= new BasicEffect(device)
		{
			VertexColorEnabled = true,
			LightingEnabled = false
		};

		vertexBuffer ??= new VertexBuffer(device, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
		indexBuffer ??= new IndexBuffer(device, IndexElementSize.SixteenBits, 24, BufferUsage.WriteOnly);

		vertexBuffer.SetData(vertices);
		indexBuffer.SetData(indices);

		device.SetVertexBuffer(vertexBuffer);
		device.Indices = indexBuffer;

		effect.World = Matrix.Identity;
		effect.View = camera.ViewMatrix;
		effect.Projection = camera.ProjectionMatrix;

		foreach (var pass in effect.CurrentTechnique.Passes)
		{
			pass.Apply();
			device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, 12);
		}

		device.SetVertexBuffer(null);
		device.Indices = null;
	}
}
