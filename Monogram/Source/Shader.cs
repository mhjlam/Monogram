using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monogram;

public class Shader(Effect effect)
{
	public Effect Effect { get; set; } = effect;
}

public class WoodShader : Shader
{
	public WoodShader(Effect effect, PhongMaterial material, Camera camera, Texture2D texture) : base(effect)
	{
		effect.Parameters["Texture"].SetValue(texture);
		effect.Parameters["LightPosition"].SetValue(new Vector3(100.0f, 100.0f, 100.0f));
		effect.Parameters["CameraPosition"].SetValue(camera.Position);
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["DiffuseColor"].SetValue(material.DiffuseColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
		effect.Parameters["SpecularColor"].SetValue(material.SpecularColor.ToVector3());
		effect.Parameters["SpecularIntensity"].SetValue(material.SpecularIntensity);
		effect.Parameters["SpecularPower"].SetValue(material.SpecularPower);
	}
}

public class LambertianShader : Shader
{
	public LambertianShader(Effect effect, LambertianMaterial material) : base(effect)
	{
		effect.Parameters["LightPosition"].SetValue(new Vector3(100.0f, 100.0f, 100.0f));
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["DiffuseColor"].SetValue(material.DiffuseColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
	}
}

public class PhongShader : Shader
{
	public PhongShader(Effect effect, PhongMaterial material, Camera camera) : base(effect)
	{
		effect.Parameters["LightPosition"].SetValue(new Vector3(100.0f, 100.0f, 100.0f));
		effect.Parameters["CameraPosition"].SetValue(camera.Position);
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["DiffuseColor"].SetValue(material.DiffuseColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
		effect.Parameters["SpecularColor"].SetValue(material.SpecularColor.ToVector3());
		effect.Parameters["SpecularIntensity"].SetValue(material.SpecularIntensity);
		effect.Parameters["SpecularPower"].SetValue(material.SpecularPower);
	}
}

public class CookTorranceShader : Shader
{
	public CookTorranceShader(Effect effect, CookTorranceMaterial material) : base(effect)
	{
		effect.Parameters["LightPosition"].SetValue(new Vector3(100.0f, 100.0f, 100.0f));
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["DiffuseColor"].SetValue(material.DiffuseColor.ToVector3());
		effect.Parameters["SpecularColor"].SetValue(material.SpecularColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
		effect.Parameters["SpecularIntensity"].SetValue(material.SpecularIntensity);
		effect.Parameters["SpecularPower"].SetValue(material.SpecularPower);
		effect.Parameters["Roughness"].SetValue(material.Roughness);
		effect.Parameters["R0"].SetValue(material.ReflectanceCoefficient);
	}
}

public class SpotLightShader : Shader
{
	public SpotLightShader(Effect effect, PhongMaterial material) : base(effect)
	{
		effect.Parameters["LightPosition"].SetValue(new Vector3(5.0f, 5.0f, 5.0f));
		effect.Parameters["LightDirection"].SetValue(new Vector3(-1.0f, -1.0f, 0.0f));
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["DiffuseColor"].SetValue(material.DiffuseColor.ToVector3());
		effect.Parameters["LightColor"].SetValue(material.SpecularColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
		effect.Parameters["OuterAngle"].SetValue(20.0f);
		effect.Parameters["InnerAngle"].SetValue(5.0f);
	}
}

public class MultiLightShader : Shader
{
	public MultiLightShader(Effect effect, CookTorranceMaterial material) : base(effect)
	{
		Vector3[] lights =
		[
			new(10f, 0f, 5f),
			new(-10f, 0f, 5f),
			new(0f, 10f, 5f)
		];
		Vector3[] lightColors =
		[
			Color.Red.ToVector3(),
			Color.Blue.ToVector3(),
			Color.LightPink.ToVector3()
		];

		effect.Parameters["LightDirections"].SetValue(lights);
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["LightColors"].SetValue(lightColors);
		effect.Parameters["SpecularColor"].SetValue(material.SpecularColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
		effect.Parameters["SpecularIntensity"].SetValue(material.SpecularIntensity);
		effect.Parameters["SpecularPower"].SetValue(256f);
		effect.Parameters["LightCount"].SetValue(lights.Length);
	}
}

public class ProjectionShader : Shader
{
	public Vector3 ProjectorPosition = new(0.0f, 0.0f, 40.0f);

	public ProjectionShader(Effect effect, LambertianMaterial material, Texture2D texture) : base(effect)
	{
		effect.Parameters["ProjectorPosition"].SetValue(ProjectorPosition);
		effect.Parameters["AmbientColor"].SetValue(material.AmbientColor.ToVector3());
		effect.Parameters["DiffuseColor"].SetValue(material.DiffuseColor.ToVector3());
		effect.Parameters["AmbientIntensity"].SetValue(material.AmbientIntensity);
		effect.Parameters["ProjectedTexture"].SetValue(texture);
	}
}
