using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace Monogram;

public struct VertexPositionNormalColor : IVertexType
{
    public Vector3 Position;
    public Vector3 Normal;
    public Color Color;

    public VertexPositionNormalColor(Vector3 position, Color color, Vector3 normal)
    {
        Position = position;
        Color = color;
        Normal = normal;
    }

    public static readonly VertexElement[] VertexElements =
    [
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
    ];

    public static readonly VertexDeclaration VertexDeclaration = new(VertexElements);
    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}

public class AmbientMaterial
{
	public Color AmbientColor;
	public float AmbientIntensity;
	// Copy constructor
}

public class LambertianMaterial : AmbientMaterial
{
	public Color DiffuseColor;
}

public class PhongMaterial : LambertianMaterial
{
	public Color SpecularColor;
	public float SpecularIntensity;
	public float SpecularPower;
}

public class CookTorranceMaterial : PhongMaterial
{
	public float Roughness;
	public float ReflectanceCoefficient;
}

public class Model
{
	private readonly float defaultScale;
	private float scale;
	private Vector3 defaultRotation, rotation;
    private Vector3 defaultPosition, position;

    private readonly XnaModel? xnaModel;
    private readonly GraphicsDevice? graphicsDevice;

    private readonly IndexBuffer? indexBuffer;
    private readonly VertexBuffer? vertexBuffer;

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

    public void Scale(float s) => scale = s;
    public void RotateX(float r) => rotation.X += r;
    public void RotateY(float r) => rotation.Y += r;
    public void RotateZ(float r) => rotation.Z += r;
    public void Translate(Vector3 t) => position += t;

    // Create Model from model resource
    public Model(XnaModel model, Vector3? position = null, Vector3? rotation = null, float scale = 1f)
    {
        xnaModel = model;
        indexBuffer = null;
        vertexBuffer = null;

        defaultPosition = this.position = position ?? Vector3.Zero;
        defaultRotation = this.rotation = rotation ?? Vector3.Zero;
        defaultScale = this.scale = scale;
    }

	public Model(Model other, Vector3? position = null, Vector3? rotation = null, float scale = 1f)
	{
		xnaModel = other.xnaModel;
		indexBuffer = null;
		vertexBuffer = null;

		defaultPosition = this.position = position ?? Vector3.Zero;
		defaultRotation = this.rotation = rotation ?? Vector3.Zero;
		defaultScale = this.scale = scale;
	}

	// Create Model from vertex buffer
	public Model(VertexBuffer vbuffer, IndexBuffer ibuffer, Vector3? position = null, Vector3? rotation = null, float scale = 1f)
    {
        xnaModel = null;
        indexBuffer = ibuffer;
        vertexBuffer = vbuffer;

        defaultPosition = this.position = position ?? Vector3.Zero;
        defaultRotation = this.rotation = rotation ?? Vector3.Zero;
        defaultScale = this.scale = scale;
    }

    // Create Model from heightmap
    public Model(GraphicsDevice device, Texture2D heightmap, Vector3? position = null, Vector3? rotation = null, float scale = 1f)
    {
        graphicsDevice = device;

        defaultPosition = this.position = position ?? Vector3.Zero;
        defaultRotation = this.rotation = rotation ?? Vector3.Zero;
        defaultScale = this.scale = scale;

        int width = heightmap.Width;
        int height = heightmap.Height;

        int halfWidth = width / 2;

        var vertices = new VertexPositionNormalColor[width * height];
        var indices = new short[(width - 1) * (height - 1) * 6];

        var bitmap = new Color[width * height];
        heightmap.GetData(bitmap);

        // Compute vertices
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                int v = x + z * width;
                float h = bitmap[x + width * z].R * 0.25f;
                vertices[v].Position = new Vector3(-halfWidth + x, h, -halfWidth + z);
                vertices[v].Color = new Color(0.025f * h, 0.5f, 15f / (h * 2.5f));
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
        for (int i = 0; i < vertices.Length; ++i)
            vertices[i].Normal = Vector3.Zero;

        for (int i = 0; i < indices.Length / 3; ++i)
        {
            int index1 = indices[i * 3];
            int index2 = indices[i * 3 + 1];
            int index3 = indices[i * 3 + 2];

            Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
            Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
            Vector3 normal = Vector3.Cross(side1, side2);

            vertices[index1].Normal += normal;
            vertices[index2].Normal += normal;
            vertices[index3].Normal += normal;
        }

        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i].Normal = Vector3.Normalize(vertices[i].Normal);
        }

        vertexBuffer = new VertexBuffer(device, VertexPositionNormalColor.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
        vertexBuffer.SetData(vertices);

        indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        indexBuffer.SetData(indices);
    }

    public void Draw(SceneDefinition scene, Camera camera)
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
                    part.Effect = scene.Shader.Effect;
                    part.Effect.CurrentTechnique = part.Effect.Techniques[0];

                    part.Effect.Parameters["WVP"].SetValue(worldViewProjection);
                    part.Effect.Parameters["WorldIT"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
                    part.Effect.Parameters["World"]?.SetValue(world);
                }
                mesh.Draw();
            }
        }
        else if (graphicsDevice is not null && indexBuffer is not null && vertexBuffer is not null)
        {
            if (scene.Shader.Effect is BasicEffect basicEffect)
            {
                basicEffect.World = world;
                basicEffect.View = view;
                basicEffect.Projection = projection;

                foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.Indices = indexBuffer;
                    graphicsDevice.SetVertexBuffer(vertexBuffer);
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
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
