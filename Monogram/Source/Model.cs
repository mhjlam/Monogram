using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    public bool UseBoundingSphere { get; set; } = false;

    public Vector3 Rotation
    {
        get => rotation;
        set => rotation = value;
    }

    public Vector3 Position
    {
        get => position;
        set => position = value;
    }

    public XnaModel? XnaModel => xnaModel;
    public Matrix ScaleMatrix => Matrix.CreateScale(scale);
    public Matrix RotationMatrix => Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
    public Matrix TranslationMatrix => Matrix.CreateTranslation(Position);
    public Matrix TransformationMatrix => ScaleMatrix * RotationMatrix * TranslationMatrix;

    public Vector3 Scale(Vector3 s) => scale = s;
    public void RotateX(float r) => rotation.X += r;
    public void RotateY(float r) => rotation.Y += r;
    public void RotateZ(float r) => rotation.Z += r;
    public void Translate(Vector3 t) => position += t;

    // Create Model from model resource
    public Model(XnaModel model, Vector3? position = null, Vector3? rotation = null, Vector3 scale = default)
	{
		xnaModel = model;

		defaultPosition = this.position = position ?? Vector3.Zero;
		defaultRotation = this.rotation = rotation ?? Vector3.Zero;
		defaultScale = this.scale = scale == default ? Vector3.One : scale;
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
        scale = defaultScale;
        position = defaultPosition;
        rotation = defaultRotation;
    }
}
