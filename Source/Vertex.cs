using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monogram;

public struct Vertex(Vector3 position, Color color, Vector3 normal) : IVertexType
{
	public Vector3 Position = position;
	public Vector3 Normal = normal;
	public Color Color = color;
	
	public static readonly VertexElement[] VertexElements =
	[
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
		new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
	];

	public static readonly VertexDeclaration VertexDeclaration = new(VertexElements);
	readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}
