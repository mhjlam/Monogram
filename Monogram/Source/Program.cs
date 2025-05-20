using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monogram.Scenes;
using System.Collections.Generic;

namespace Monogram;

public sealed class FrameRateCounter(Game game) : DrawableGameComponent(game)
{
	private int _frameRate;
	private int _frameCounter;
	private int _secondsPassed;

	public int FrameRate => _frameRate;

	public override void Update(GameTime gameTime)
	{
		if (_secondsPassed != gameTime.TotalGameTime.Seconds)
		{
			_frameRate = _frameCounter;
			_secondsPassed = gameTime.TotalGameTime.Seconds;
			_frameCounter = 0;
		}
		_frameCounter++;
	}
}

public sealed class Program : Game
{
	private readonly GraphicsDeviceManager _graphicsDeviceManager;
	private GraphicsDevice _graphicsDevice = default!;
	private Renderer _sceneManager = default!;
	private readonly FrameRateCounter _frameRateCounter;

	private Input _input = default!;
	private SpriteFont _spriteFont = default!;
	private SpriteBatch _spriteBatch = default!;

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

		_frameRateCounter = new(this);
		Components.Add(_frameRateCounter);
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
		_spriteFont = Content.Load<SpriteFont>("Fonts/Segoe12");
		var spriteFont = _spriteFont;
		var spriteBatch = _spriteBatch;

		_sceneManager = new Renderer(_graphicsDevice, spriteBatch, spriteFont);

		// Models
		var headModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/FemaleHead"));
		var terrainModel = new Model(_graphicsDevice, Content.Load<Texture2D>("Textures/HeightMap"), Vector3.Zero, Vector3.Zero, 0.5f);
		var teapotModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/Teapot"), Vector3.Zero, Vector3.Zero, 10f);
		var squareModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/Square"), Vector3.Zero, Vector3.Zero, 20f);

		const int count = 9;
		const float totalWidth = 160f, spacing = totalWidth / (count - 1), startX = -totalWidth / 2f, scale = 0.5f;
		var headgeRow = new List<Model>(count);
		for (int i = 0; i < count; i++)
			headgeRow.Add(new Model(headModel, new Vector3(startX + i * spacing, 0f, 0f), null, scale));

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

		var basicShader		= new Shader(basicEffect);
		var woodTexture		= new WoodShader(Content.Load<Effect>("Effects/Texture"), woodMaterial, _sceneManager.Camera, Content.Load<Texture2D>("Textures/Wood"));
		var blinnPhong		= new PhongShader(Content.Load<Effect>("Effects/BlinnPhong"), phongMaterial, _sceneManager.Camera);
		var normalColor		= new Shader(Content.Load<Effect>("Effects/Normals"));
		var checkered		= new Shader(Content.Load<Effect>("Effects/Checkers"));
		var spotlight		= new SpotLightShader(Content.Load<Effect>("Effects/Spotlight"), phongMaterial);
		var lambertian		= new LambertianShader(Content.Load<Effect>("Effects/Lambertian"), lambertianMaterial);
		var multilight		= new MultiLightShader(Content.Load<Effect>("Effects/MultiLight"), cookTorranceMaterial);
		var projection		= new ProjectionShader(Content.Load<Effect>("Effects/Projective"), lambertianMaterial, Content.Load<Texture2D>("Textures/Smiley"));
		var cookTorrance	= new CookTorranceShader(Content.Load<Effect>("Effects/CookTorrance"), cookTorranceMaterial);

		var monoFilter		= new Filter(_graphicsDevice, spriteBatch, Content.Load<Effect>("Effects/Monochrome"));
		var gaussianFilter	= new GaussianBlur(_graphicsDevice, spriteBatch, Content.Load<Effect>("Effects/GaussianBlur"));

		// Lambda that accepts a radians value and outputs a Vector3
		static Vector3 eyeTransform(float radians) => Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(radians)));

		_sceneManager.AddScene(new Scene(SceneID.Terrain, basicShader, [terrainModel], eyeTransform(-40f)));
		_sceneManager.AddScene(new Scene(SceneID.Wood, woodTexture, [squareModel], eyeTransform(+20f)));
		_sceneManager.AddScene(new Scene(SceneID.Lambert, lambertian, [teapotModel], eyeTransform(-20f)));
		_sceneManager.AddScene(new Scene(SceneID.Phong, blinnPhong, [teapotModel], eyeTransform(-20f)));
		_sceneManager.AddScene(new Scene(SceneID.Normals, normalColor, [teapotModel], eyeTransform(-20f)));
		_sceneManager.AddScene(new Scene(SceneID.Checkered, checkered, [teapotModel], eyeTransform(-20f)));
		_sceneManager.AddScene(new Scene(SceneID.CookTorrance, cookTorrance, [headModel]));
		_sceneManager.AddScene(new Scene(SceneID.Spotlight, spotlight, [headModel]));
		_sceneManager.AddScene(new Scene(SceneID.Multilight, multilight, [headModel]));
		_sceneManager.AddScene(new Scene(SceneID.Projection, projection, [headModel]));
		_sceneManager.AddScene(new Scene(SceneID.Monochrome, cookTorrance, [headModel], null, monoFilter));
		_sceneManager.AddScene(new Scene(SceneID.GaussianBlur, normalColor, [headModel], null, gaussianFilter));
		_sceneManager.AddScene(new CullingScene(normalColor, headgeRow, totalWidth));

		// Initialize camera orbit state
		_sceneManager.Camera.SyncOrbitToCamera();

		_input = new Input(Window, _sceneManager);
	}

	protected override void Update(GameTime gameTime)
	{
		float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_sceneManager.Update(delta);

		bool isMouseVisible = IsMouseVisible;
		_input.Update(delta, ref isMouseVisible, _sceneManager.Camera.SyncOrbitToCamera);
		IsMouseVisible = isMouseVisible;

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		_sceneManager.Draw();

		// Draw FPS counter at top-right
		_spriteBatch.Begin();
		string fpsText = $"FPS: {_frameRateCounter.FrameRate}";
		Vector2 textSize = _spriteFont.MeasureString(fpsText);
		Vector2 position = new(Window.ClientBounds.Width - textSize.X - 16, 16);
		_spriteBatch.DrawString(_spriteFont, fpsText, position, Color.Yellow);
		_spriteBatch.End();

		// Reset device states to defaults for 3D rendering
		GraphicsDevice.BlendState = BlendState.Opaque;
		GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
		GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

		base.Draw(gameTime);
	}

	public static void Main()
	{
		using var program = new Program();
		program.Run();
	}
}
