using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monogram.Scenes;
using Monogram.Source.Scenes;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;

namespace Monogram;

public sealed class Program : Game
{
	private readonly GraphicsDeviceManager _graphicsDeviceManager;
	private GraphicsDevice _graphicsDevice = default!;
	private Renderer _renderer = default!;

	private Input _input = default!;
	private SpriteFont _spriteFont = default!;
	private SpriteBatch _spriteBatch = default!;

	private MouseState _prevMouseState;

	// Add Overlay field
	private Overlay _overlay = default!;

	// Add these fields to Program
	private bool _dropdownExpandedLast = false;


	public Program()
	{
		Content.RootDirectory = "Content";

		_graphicsDeviceManager = new(this)
		{
			IsFullScreen = false,
			GraphicsProfile = GraphicsProfile.HiDef,
			PreferredBackBufferWidth = 1280,
			PreferredBackBufferHeight = 720,
			SynchronizeWithVerticalRetrace = false
		};
		_graphicsDeviceManager.ApplyChanges();
	}

	protected override void Initialize()
	{
		IsFixedTimeStep = false;
		Window.Position = new(
			GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2 - Window.ClientBounds.Width / 2,
			GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2 - Window.ClientBounds.Height / 2
		);
		IsMouseVisible = false;
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_graphicsDevice = _graphicsDeviceManager.GraphicsDevice;
		_graphicsDevice.RasterizerState = new RasterizerState();

		_spriteBatch = new SpriteBatch(_graphicsDevice);
		_spriteFont = Content.Load<SpriteFont>("Fonts/SegoeUI");

		_renderer = new Renderer(_graphicsDevice);

		// Models
		var head = new Model(Content.Load<XnaModel>("Models/FemaleHead"));
		var cube = new Model(Content.Load<XnaModel>("Models/UnitCube"), Vector3.Zero, Vector3.Zero, new Vector3(40f, 40f, 40f));
		var teapot = new Model(Content.Load<XnaModel>("Models/Teapot"), Vector3.Zero, Vector3.Zero, new Vector3(10f, 10f, 10f));

		const int count = 9;
		const float totalWidth = 160f, spacing = totalWidth / (count - 1), startX = -totalWidth / 2f, scale = 0.5f;
		var headgeRow = Enumerable.Range(0, count)
			.Select(i => new Model(head.XnaModel!, new Vector3(startX + i * spacing, 0f, 0f), Vector3.Zero, new Vector3(scale)))
			.ToList();

		// Materials
		var lambertianMaterial = new LambertianMaterial
		{
			AmbientColor = Color.White,
			AmbientIntensity = 0.2f,
			DiffuseColor = Color.Gray
		};
		var phongMaterial = new PhongMaterial
		{
			AmbientColor = Color.Red,
			AmbientIntensity = 0.2f,
			DiffuseColor = Color.Orange,
			SpecularColor = Color.White,
			SpecularIntensity = 1f,
			SpecularPower = 32f
		};
		var woodMaterial = new PhongMaterial
		{
			AmbientColor = Color.Black,
			AmbientIntensity = 0.2f,
			DiffuseColor = Color.BurlyWood,
			SpecularColor = Color.White,
			SpecularIntensity = 0.2f,
			SpecularPower = 32f
		};
		var cookTorranceMaterial = new CookTorranceMaterial
		{
			AmbientColor = Color.Gold,
			AmbientIntensity = 0.2f,
			DiffuseColor = Color.Goldenrod,
			SpecularColor = Color.White,
			SpecularIntensity = 2f,
			SpecularPower = 25f,
			Roughness = 0.5f,
			ReflectanceCoefficient = 1.42f
		};

		var basicEffect = new BasicEffect(_graphicsDevice)
		{
			VertexColorEnabled = true,
			LightingEnabled = true,
			AmbientLightColor = new Vector3(0.3f)
		};
		basicEffect.DirectionalLight0.Enabled = true;
		basicEffect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
		basicEffect.DirectionalLight0.Direction = Vector3.Down;

		static Vector3 eyeTransform(float dist, float radX, float? radY = null, float? radZ = null)
		{
			float y = radY ?? 0f;
			float z = radZ ?? 0f;
			return Vector3.Transform(
				new Vector3(0f, 0f, dist),
				Matrix.CreateRotationX(MathHelper.ToRadians(radX)) *
				Matrix.CreateRotationY(MathHelper.ToRadians(y)) *
				Matrix.CreateRotationZ(MathHelper.ToRadians(z))
			);
		}

		var woodTexture = new WoodShader(Content.Load<Effect>("Effects/Texture"), woodMaterial, _renderer.Camera, Content.Load<Texture2D>("Textures/Planks"));
		var blinnPhong = new PhongShader(Content.Load<Effect>("Effects/BlinnPhong"), phongMaterial, _renderer.Camera);
		var normalColor = new Shader(Content.Load<Effect>("Effects/Normals"));
		var checkered = new Shader(Content.Load<Effect>("Effects/Checkers"));
		var spotlight = new SpotLightShader(Content.Load<Effect>("Effects/Spotlight"), phongMaterial);
		var lambertian = new LambertianShader(Content.Load<Effect>("Effects/Lambertian"), lambertianMaterial);
		var multilight = new MultiLightShader(Content.Load<Effect>("Effects/MultiLight"), cookTorranceMaterial);
		var projective = new ProjectionShader(Content.Load<Effect>("Effects/Projective"), lambertianMaterial, Content.Load<Texture2D>("Textures/Tattoo"));
		var cookTorrance = new CookTorranceShader(Content.Load<Effect>("Effects/CookTorrance"), cookTorranceMaterial);

		var monochrome = new Filter(_graphicsDevice, _spriteBatch, Content.Load<Effect>("Effects/Monochrome"));
		var gaussianBlur = new GaussianBlur(_graphicsDevice, _spriteBatch, Content.Load<Effect>("Effects/GaussianBlur"));

		_renderer.AddScene(new TerrainScene(_graphicsDevice, Content.Load<Texture2D>("Textures/HeightMap"), eyeTransform(200f, -40f, -20f)));
		_renderer.AddScene(new Scene(SceneID.Lambert, lambertian, [teapot], eyeTransform(100f, -20f)));
		_renderer.AddScene(new Scene(SceneID.Phong, blinnPhong, [teapot], eyeTransform(100f, -20f)));
		_renderer.AddScene(new Scene(SceneID.Normals, normalColor, [teapot], eyeTransform(100f, -20f)));
		_renderer.AddScene(new Scene(SceneID.Texture, woodTexture, [cube], eyeTransform(100f, -20f, -50f)));
		_renderer.AddScene(new Scene(SceneID.Checkered, checkered, [teapot], eyeTransform(100f, -20f)));
		_renderer.AddScene(new Scene(SceneID.CookTorrance, cookTorrance, [head]));
		_renderer.AddScene(new Scene(SceneID.Spotlight, spotlight, [head]));
		_renderer.AddScene(new Scene(SceneID.Multilight, multilight, [head]));
		_renderer.AddScene(new Scene(SceneID.Monochrome, cookTorrance, [head], null, monochrome));
		_renderer.AddScene(new Scene(SceneID.GaussianBlur, normalColor, [head], null, gaussianBlur));
		_renderer.AddScene(new ProjectScene(projective, [teapot], eyeTransform(100f, -20f)));
		_renderer.AddScene(new CullingScene(normalColor, headgeRow, totalWidth));

		_prevMouseState = Mouse.GetState();
		_input = new Input(Window, _renderer);

		_renderer.Camera.SyncOrbitToCamera();

		// Register overlay as a component to call Update() automatically
		_overlay = new Overlay(this, _renderer, _spriteFont, _spriteBatch, Window);
		Components.Add(_overlay);
	}

	protected override void Update(GameTime gameTime)
	{
		float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_renderer.Update(elapsed);

		bool isMouseVisible = IsMouseVisible;

		// Track scene index before input or overlay updates
		int prevSceneIndex = _renderer.CurrentSceneIndex;

		// Pass dropdown expanded state and scene change callback
		_input.Update(
			elapsed,
			ref isMouseVisible,
			_renderer.Camera.SyncOrbitToCamera,
			_overlay.DropdownExpanded,
			_dropdownExpandedLast,
			_overlay.DropdownMouseOver, () =>
			{
				_renderer.LoadAdjacent(false);
				_renderer.Camera.SyncOrbitToCamera();
			});

		IsMouseVisible = isMouseVisible;

		var mouse = Mouse.GetState();
		_overlay.UpdateOverlay(mouse, _prevMouseState);

		int newSceneIndex = _renderer.CurrentSceneIndex;
		if (newSceneIndex != prevSceneIndex)
		{
			_overlay.SelectedSceneIndex = newSceneIndex;
		}

		_prevMouseState = mouse;
		_dropdownExpandedLast = _overlay.DropdownExpanded;

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		_renderer.Draw();
		_overlay.Draw();

		// Reset device state after overlay/UI
		if (_graphicsDevice.BlendState != BlendState.Opaque)
			_graphicsDevice.BlendState = BlendState.Opaque;
		if (_graphicsDevice.DepthStencilState != DepthStencilState.Default)
			_graphicsDevice.DepthStencilState = DepthStencilState.Default;
		if (_graphicsDevice.RasterizerState != RasterizerState.CullCounterClockwise)
			_graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		if (_graphicsDevice.SamplerStates[0] != SamplerState.LinearWrap)
			_graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

		base.Draw(gameTime);
	}

	public static void Main()
	{
		using var program = new Program();
		program.Run();
	}
}
