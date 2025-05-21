using Microsoft.Xna.Framework;

namespace Monogram;

public class AmbientMaterial
{
	public Color AmbientColor;
	public float AmbientIntensity;
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
