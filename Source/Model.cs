using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace Monogram;

public class Model
{
	private Vector3 defaultScale, scale;
	private Vector3 defaultRotation, rotation;
	private Vector3 defaultPosition, position;
	private readonly XnaModel? xnaModel;

	// For custom geometry
	protected VertexBuffer? vertexBuffer;
	protected IndexBuffer? indexBuffer;
	protected int primitiveCount;

	// Optional bounding check
	public bool UseBoundingSphere { get; set; }

	public Vector3 Rotation
	{
		get => rotation;
		set
		{
			rotation = value;
			_transformationDirty = true;
		}
	}

	public float RotationY
	{
		get => rotation.Y;
		set
		{
			rotation.Y = value;
			_transformationDirty = true;
		}
	}

	public Vector3 Position
	{
		get => position;
		set
		{
			position = value;
			_transformationDirty = true;
		}
	}

	public XnaModel? XnaModel => xnaModel;

	private Matrix _scaleMatrix, _rotationMatrix, _translationMatrix, _transformationMatrix;
	private bool _transformationDirty = true;

	public Matrix ScaleMatrix => GetTransformation(ref _scaleMatrix);
	public Matrix RotationMatrix => GetTransformation(ref _rotationMatrix);
	public Matrix TranslationMatrix => GetTransformation(ref _translationMatrix);
	public Matrix TransformationMatrix => GetTransformation(ref _transformationMatrix);

	public Model(XnaModel model, Vector3? position = null, Vector3? rotation = null, Vector3 scale = default)
	{
		xnaModel = model;
		defaultPosition = this.position = position ?? Vector3.Zero;
		defaultRotation = this.rotation = rotation ?? Vector3.Zero;
		defaultScale = this.scale = scale == default ? Vector3.One : scale;
		_transformationDirty = true;
	}

	public virtual void Draw(Effect effect, Camera camera)
	{
		Matrix world = Matrix.Identity * TransformationMatrix;
		Matrix view = camera.ViewMatrix;
		Matrix projection = camera.ProjectionMatrix;
		Matrix worldViewProjection = world * view * projection;

		if (xnaModel is not null)
		{
			foreach (ModelMesh mesh in xnaModel.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					part.Effect = effect;
					effect.CurrentTechnique = effect.Techniques[0];

					effect.Parameters["WVP"]?.SetValue(worldViewProjection);
					effect.Parameters["WorldIT"]?.SetValue(Matrix.Transpose(Matrix.Invert(world)));
					effect.Parameters["World"]?.SetValue(world);

					var device = part.VertexBuffer.GraphicsDevice;
					device.SetVertexBuffer(part.VertexBuffer);
					device.Indices = part.IndexBuffer;

					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
					}
				}
			}
		}
	}

	public void Reset()
	{
		position = defaultPosition;
		rotation = defaultRotation;
		scale = defaultScale;
		_transformationDirty = true;
	}

	private Matrix GetTransformation(ref Matrix cache)
	{
		if (_transformationDirty)
		{
			_scaleMatrix = Matrix.CreateScale(scale);
			_rotationMatrix = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
			_translationMatrix = Matrix.CreateTranslation(position);
			_transformationMatrix = _scaleMatrix * _rotationMatrix * _translationMatrix;
			_transformationDirty = false;
		}
		return cache;
	}
}
