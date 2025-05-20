using Microsoft.Xna.Framework;
using Monogram.Source.Scenes;
using System.Collections.Generic;
using System.Linq;

namespace Monogram.Scenes;

public class ProjectScene(Shader shader, List<Model> models, Vector3? eye = null) : Scene(SceneID.Projection, shader, models, eye)
{
	public override void Update(float deltaTime)
	{
		base.Update(deltaTime);

		Shader.Effect.Parameters["Time"]?.SetValue(_accumulatedTime);

		// Rotate all models around the Y axis based on elapsed time
		foreach (var model in Models)
		{
			model.Rotation = new Vector3(model.Rotation.X, _accumulatedTime, model.Rotation.Z);
		}

		if (Shader.Effect.Parameters["ProjectorViewProjection"] != null)
		{
			Vector3 projectorPosition = Shader.Effect.Parameters["ProjectorPosition"] != null
				? Shader.Effect.Parameters["ProjectorPosition"].GetValueVector3()
				: new Vector3(0f, 20f, 30f);

			Matrix projectorViewProjection =
				Matrix.Identity * Models.First().TransformationMatrix *
				Matrix.CreateLookAt(projectorPosition, new Vector3(0f, 0f, 0f), Vector3.Up) *
				Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(100f), 1f, 1f, 100f);

			Shader.Effect.Parameters["ProjectorViewProjection"].SetValue(projectorViewProjection);
		}
	}
}
