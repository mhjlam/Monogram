using Microsoft.Xna.Framework;
using System;

namespace Monogram;

public class Camera
{
	private Matrix view;
	private Matrix proj;

	private Vector3 position;
	private Vector3 target;

	private readonly float defaultAspect;
	private Vector3 defaultPosition;
	private Vector3 defaultTarget;

	// Orbit state
	private float _orbitYaw = 0f;
	private float _orbitPitch = 0f;
	private float _orbitDistance = 100f;

	public float OrbitYaw => _orbitYaw;
	public float OrbitPitch => _orbitPitch;
	public float OrbitDistance => _orbitDistance;

	public Vector3 Position => position;
	public Matrix ViewMatrix => view;
	public Matrix ProjectionMatrix => proj;

	public Camera(Vector3 position, Vector3 lookAt, float aspectRatio = 1.0f)
	{
		defaultAspect = aspectRatio;
		defaultPosition = position;
		defaultTarget = lookAt;

		SyncOrbitToCamera();
		Reset();
	}

	public void Transform(Matrix transformationMatrix)
	{
		view = Matrix.Multiply(view, transformationMatrix);
		Vector3.Transform(position, transformationMatrix);
	}

	public void SetEye(Vector3 position, bool isDefault = false)
	{
		this.position = position;
		view = Matrix.CreateLookAt(position, target, Vector3.UnitY);
		if (isDefault) defaultPosition = position;
	}

	public void SetGaze(Vector3 target, bool isDefault = false)
	{
		this.target = target;
		view = Matrix.CreateLookAt(position, target, Vector3.UnitY);
		if (isDefault) defaultTarget = target;
	}

	public void Update()
	{
		view = Matrix.CreateLookAt(position, target, Vector3.UnitY);
	}

	public void Reset()
	{
		SetGaze(defaultTarget);
		SetEye(defaultPosition);
		proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, defaultAspect, 1.0f, 500.0f);
		SyncOrbitToCamera();
	}

	/// <summary>
	/// Syncs the orbit state (yaw, pitch, distance) to the current camera position.
	/// </summary>
	public void SyncOrbitToCamera()
	{
		_orbitDistance = (position - Vector3.Zero).Length();
		if (_orbitDistance < 1e-4f)
		{
			_orbitYaw = 0f;
			_orbitPitch = 0f;
		}
		else
		{
			Vector3 dir = Vector3.Normalize(position - Vector3.Zero);
			_orbitYaw = MathF.Atan2(dir.X, dir.Z);
			_orbitPitch = MathF.Asin(dir.Y);
		}
	}

	/// <summary>
	/// Sets the orbit state and updates the camera position.
	/// </summary>
	public void SetOrbit(float yaw, float pitch, float distance)
	{
		_orbitYaw = yaw;
		_orbitPitch = pitch;
		_orbitDistance = distance;
		SetEye(OrbitToPosition(_orbitYaw, _orbitPitch, _orbitDistance));
		SetGaze(Vector3.Zero);
	}

	/// <summary>
	/// Orbit camera logic: updates yaw/pitch based on mouse delta and updates camera position.
	/// </summary>
	public void Orbit(float dx, float dy)
	{
		const float sensitivity = 0.01f;
		_orbitYaw -= dx * sensitivity;
		_orbitPitch += dy * sensitivity;

		const float maxPitch = MathF.PI / 2f - 0.01f;
		const float minPitch = -maxPitch;
		_orbitPitch = Math.Clamp(_orbitPitch, minPitch, maxPitch);

		SetEye(OrbitToPosition(_orbitYaw, _orbitPitch, _orbitDistance));
		SetGaze(Vector3.Zero);
	}

	public void OrbitZoom(float delta)
	{
		_orbitDistance -= delta * 0.01f;
		_orbitDistance = Math.Clamp(_orbitDistance, 10f, 500f);
		SetEye(OrbitToPosition(_orbitYaw, _orbitPitch, _orbitDistance));
		SetGaze(Vector3.Zero);
	}

	public static Vector3 OrbitToPosition(float yaw, float pitch, float radius)
	{
		float x = radius * MathF.Cos(pitch) * MathF.Sin(yaw);
		float y = radius * MathF.Sin(pitch);
		float z = radius * MathF.Cos(pitch) * MathF.Cos(yaw);
		return new Vector3(x, y, z);
	}
}
