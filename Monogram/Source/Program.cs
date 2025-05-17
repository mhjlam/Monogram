using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
	private Scener _sceneManager = default!;
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

		_sceneManager = new Scener(_graphicsDevice, spriteBatch, spriteFont);

		// Models
		var headModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/FemaleHead"));
		var terrainModel = new Model(_graphicsDevice, Content.Load<Texture2D>("Textures/HeightMap"), Vector3.Zero, Vector3.Zero, 0.5f);
		var teapotModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/Teapot"), Vector3.Zero, Vector3.Zero, 10f);
		var squareModel = new Model(Content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/TableTop"), Vector3.Zero, Vector3.Zero, 20f);

		var headgeRow = new List<Model>
		{
			new(headModel, new Vector3(-80f + 0 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 1 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 2 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 3 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 4 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 5 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 6 * 40f, 0f, 0f)),
			new(headModel, new Vector3(-80f + 7 * 40f, 0f, 0f))
		};

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

		var basicEffect = new BasicEffect(_graphicsDevice);
		basicEffect.VertexColorEnabled = true;
		basicEffect.LightingEnabled = true;
		basicEffect.AmbientLightColor = new Vector3(0.3f);
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

		var monocFilter		= new Filter(_graphicsDevice, spriteBatch, Content.Load<Effect>("Effects/Monochrome"));
		var gaussianFilter	= new GaussianBlur(_graphicsDevice, spriteBatch, Content.Load<Effect>("Effects/GaussianBlur"));

		// Lambda that accepts a radians value and outputs a Vector3
		static Vector3 eyeTransform(float radians) => Vector3.Transform(new Vector3(0f, 0f, 100f), Matrix.CreateRotationX(MathHelper.ToRadians(radians)));

		_sceneManager.AddScene(SceneID.Terrain,			basicShader,	[terrainModel],	eyeTransform(-40f));
		_sceneManager.AddScene(SceneID.Wood,			woodTexture,	[squareModel],	eyeTransform(+20f));
		_sceneManager.AddScene(SceneID.Lambert,			lambertian,		[teapotModel],	eyeTransform(-20f));
		_sceneManager.AddScene(SceneID.Phong,			blinnPhong,		[teapotModel],	eyeTransform(-20f));
		_sceneManager.AddScene(SceneID.Normals,			normalColor,	[teapotModel],	eyeTransform(-20f));
		_sceneManager.AddScene(SceneID.Checkered,		checkered,		[teapotModel],	eyeTransform(-20f));
		_sceneManager.AddScene(SceneID.CookTorrance,	cookTorrance,	[headModel]);
		_sceneManager.AddScene(SceneID.Spotlight,		spotlight,		[headModel]);
		_sceneManager.AddScene(SceneID.Multilight,		multilight,		[headModel]);
		_sceneManager.AddScene(SceneID.Projection,		projection,		[headModel]);
		_sceneManager.AddScene(SceneID.Monochrome,		cookTorrance,	[headModel], null, monocFilter);
		_sceneManager.AddScene(SceneID.GaussianBlur,	normalColor,	[headModel], null, gaussianFilter);
		_sceneManager.AddScene(SceneID.Culling,			normalColor,	headgeRow);

		// Initialize camera orbit state
		_sceneManager.Camera.SyncOrbitToCamera();

		_input = new Input(Window, _sceneManager);
	}

	protected override void Update(GameTime gameTime)
	{
		float delta = (float)gameTime.ElapsedGameTime.TotalSeconds * 60f;

		_sceneManager.Update();

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
