using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GameOpenGL;

public class Game() : GameWindow(GameWindowSettings.Default,
    new NativeWindowSettings
    {
        ClientSize = new Vector2i(1280, 720),
        Title = "Akula vs Bikini Bottom 4D",
        API = ContextAPI.OpenGL,
        Profile = ContextProfile.Compatability,
        APIVersion = new Version(3, 3)
    })
{
    private const int MaxTallInGroup = 2;
    private const float MinGap = 8f, MaxGap = 18f;
    private float _gapSinceLast;
    private float _nextGap = 10f;

    private enum State
    {
        Menu,
        Play,
        GameOver
    }

    private State _state = State.Menu;

    private readonly Player _player = new();
    private readonly LaneManager _laneManager = new();
    private readonly Camera _camera = new();

    private readonly List<ObstacleLine> _lines = new();
    private int _skyTextureId; // Added for sky texture

    private int _score = 0, _highScore = 0;
    private const float WorldSpeed = 10f;

    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DepthTest); // Убедимся, что тест глубины включен

        GL.Enable(EnableCap.Fog);
        GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
        GL.Fog(FogParameter.FogStart, 30f);
        GL.Fog(FogParameter.FogEnd, 80f);
        GL.Fog(FogParameter.FogColor, new float[] { 0.1f, 0.3f, 0.5f, 1f });

        GL.ClearColor(0.2f, 0.4f, 0.6f, 1f);
        _highScore = File.Exists("highscore.txt") ? int.Parse(File.ReadAllText("highscore.txt")) : 0;

        // Настройка освещения
        GL.Enable(EnableCap.Lighting);

        // Light0 (основной, направленный сверху-спереди)
        GL.Enable(EnableCap.Light0);
        float[] light0Position = { 0.5f, 1.5f, 1.0f, 0.0f }; // Направленный свет
        GL.Light(LightName.Light0, LightParameter.Position, light0Position);
        GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.2f, 0.2f, 0.2f, 1.0f }); // Меньше общего эмбиента
        GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
        GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 0.8f, 0.8f, 0.8f, 1.0f });

        // Light1 (дополнительный, слева от дорожки, позиционный)
        GL.Enable(EnableCap.Light1);
        float[] light1Position = { -15.0f, 5.0f, 0.0f, 1.0f }; // Позиционный свет слева, немного сверху
        float[] light1Ambient = { 0.1f, 0.1f, 0.1f, 1.0f };
        float[] light1Diffuse = { 0.5f, 0.5f, 0.5f, 1.0f }; // Синий оттенок для бокового света
        float[] light1Specular = { 0.6f, 0.6f, 0.6f, 1.0f };
        GL.Light(LightName.Light1, LightParameter.Position, light1Position);
        GL.Light(LightName.Light1, LightParameter.Ambient, light1Ambient);
        GL.Light(LightName.Light1, LightParameter.Diffuse, light1Diffuse);
        GL.Light(LightName.Light1, LightParameter.Specular, light1Specular);
        // Можно добавить затухание для Light1, если он слишком яркий на расстоянии
        // GL.Light(LightName.Light1, LightParameter.ConstantAttenuation, 0.5f);
        // GL.Light(LightName.Light1, LightParameter.LinearAttenuation, 0.05f);
        // GL.Light(LightName.Light1, LightParameter.QuadraticAttenuation, 0.01f);

        GL.Enable(EnableCap.ColorMaterial);
        GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
        
        GL.Enable(EnableCap.Normalize); // Важно для корректного освещения при масштабировании

        // Load sky texture
        try
        {
            _skyTextureId = TextureManager.LoadTexture("textures/sky.png"); // Ensure sky.png is in textures folder
        }
        catch (System.IO.FileNotFoundException e)
        {
            Console.WriteLine($"Sky texture not found: {e.Message}");
            // Handle error, maybe use a default background color or no sky
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
        Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(60f),
            e.Width / (float)e.Height, 0.1f, 100f);
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadMatrix(ref proj);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        if (!IsFocused) return;
        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        float dt = (float)e.Time;

        switch (_state)
        {
            case State.Menu: UpdateMenu(); break;
            case State.Play: UpdateGameplay(dt); break;
            case State.GameOver:
                if (KeyboardState.IsKeyPressed(Keys.Space)) EnterMenu();
                break;
        }
    }

    private void UpdateMenu()
    {
        if (KeyboardState.IsKeyPressed(Keys.Enter)) StartGame();
        if (MouseState.IsButtonDown(MouseButton.Left))
        {
            var pos = MouseState.Position;
            if (pos.X < Size.X / 2f) StartGame();
            else Close();
        }
    }

    private void UpdateGameplay(float dt)
    {
        _player.Update(dt, KeyboardState);
        _laneManager.Update(dt);

        float dz = WorldSpeed * dt;

        // движение, очки, удаление
        for (int i = _lines.Count - 1; i >= 0; i--)
        {
            var line = _lines[i];
            line.Update(dz);

            if (!line.Passed && line.Z > _player.Position.Z + 1f)
            {
                line.Passed = true;
                _score++;
                Title = $"Akula vs Bikini Bottom 4D   |   Score: {_score}    Best: {_highScore}";
            }

            if (line.Z > _player.Position.Z + 20f)
                _lines.RemoveAt(i);
        }

        // генерация
        _gapSinceLast += dz;
        if (_gapSinceLast >= _nextGap)
        {
            SpawnLine();
            _gapSinceLast = 0f;
            _nextGap = RandomRange(MinGap, MaxGap);
        }

        // коллизии
        foreach (var line in _lines)
        {
            foreach (var ob in line.Obstacles)
            {
                if (ob.CheckCollision(_player.Position, 0.5f))
                {
                    GameOver();
                    return;
                }
            }
        }
    }
    
    // Весь интерфейс
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        RenderSky(); // Draw the sky background first

        _camera.Follow(_player.Position);
        GL.MatrixMode(MatrixMode.Modelview);
        Matrix4 view = _camera.ViewMatrix;
        GL.LoadMatrix(ref view);

        // Включаем освещение для 3D сцены
        GL.Enable(EnableCap.Lighting);
        GL.Enable(EnableCap.DepthTest); // Убедимся, что тест глубины включен для 3D объектов

        _laneManager.Draw();
        foreach (var line in _lines) line.Draw();

        // Отрисовка тени игрока
        DrawPlayerShadow();

        _player.Draw();

        // Отключаем освещение для 2D элементов HUD/Menu
        GL.Disable(EnableCap.Lighting);

        if (_state == State.Play) RenderHud();
        else if (_state == State.Menu) RenderMenu();
        else if (_state == State.GameOver) RenderGameOverOverlay();

        SwapBuffers();
    }

    private void DrawPlayerShadow()
    {
        GL.PushMatrix();
        
        // Тени не должны быть освещены и не должны писать в буфер глубины, чтобы не перекрывать другие объекты некорректно
        GL.Disable(EnableCap.Lighting);
        GL.DepthMask(false); // Отключаем запись в буфер глубины для тени
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        GL.Color4(0.0f, 0.0f, 0.0f, 0.4f); // Цвет тени: черный, полупрозрачный

        float groundLevel = 0.015f; // Чуть выше уровня дороги (yMainPath в LaneManager ~0.01f)
        Vector3 playerPos = _player.Position;
        float playerRoll = _player.GetRollAngle(); // Получаем угол наклона игрока

        GL.Translate(playerPos.X, groundLevel, playerPos.Z);
        GL.Rotate(playerRoll, Vector3.UnitZ); // Применяем наклон игрока к тени
        GL.Scale(1.0f, 0.05f, 1.0f);          // Сплющиваем тень по оси Y

        // Рисуем основную форму тела акулы для тени
        // Используем константу Player.Radius, которую нужно сделать public
        GL.PushMatrix();
        GL.Scale(1f, 0.8f, 2.5f); // Масштабы тела акулы из Player.Draw()
        Primitives.Cube(new Vector3(Player.Radius, Player.Radius, Player.Radius));
        GL.PopMatrix();
        
        GL.Disable(EnableCap.Blend);
        GL.DepthMask(true); // Включаем обратно запись в буфер глубины
        // GL.Enable(EnableCap.Lighting); // Освещение будет включено перед рендерингом _player.Draw()
        
        GL.PopMatrix();
    }

    private void RenderSky()
    {
        if (_skyTextureId == 0) return; // Sky texture not loaded

        GL.MatrixMode(MatrixMode.Projection);
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.Ortho(0, 1, 1, 0, -1, 1); // Ortho projection, Y flipped for typical image coords

        GL.MatrixMode(MatrixMode.Modelview);
        GL.PushMatrix();
        GL.LoadIdentity();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Fog);      // Sky should not be affected by fog
        GL.Disable(EnableCap.Lighting); // Небо не должно быть освещено
        GL.Enable(EnableCap.Texture2D);
        GL.BindTexture(TextureTarget.Texture2D, _skyTextureId);

        GL.Color3(1f, 1f, 1f); // White color to render texture without tint

        GL.Begin(PrimitiveType.Quads);
        GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
        GL.TexCoord2(1, 0); GL.Vertex2(1, 0);
        GL.TexCoord2(1, 1); GL.Vertex2(1, 1);
        GL.TexCoord2(0, 1); GL.Vertex2(0, 1);
        GL.End();

        GL.Disable(EnableCap.Texture2D);
        GL.Enable(EnableCap.DepthTest); // Re-enable depth test for 3D scene
        GL.Enable(EnableCap.Fog);       // Re-enable fog for 3D scene

        GL.MatrixMode(MatrixMode.Projection);
        GL.PopMatrix();
        GL.MatrixMode(MatrixMode.Modelview);
        GL.PopMatrix();
    }

    private void RenderHud()
    {
        GL.MatrixMode(MatrixMode.Projection);
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.Ortho(0, 1, 0, 1, -1, 1);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Lighting); // HUD не освещается
        GL.Color3(1f, 1f, 1f);
        BitmapText.Draw($"SCORE: {_score}", 0.03f, 0.95f, 0.04f);
        GL.Enable(EnableCap.DepthTest);

        GL.MatrixMode(MatrixMode.Projection);
        GL.PopMatrix();
    }

    private void RenderMenu()
    {
        GL.MatrixMode(MatrixMode.Projection);
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.Ortho(0, 1, 0, 1, -1, 1);
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Lighting); // Меню не освещается
        
        GL.Color3(0.1f, 0.1f, 0.1f);
        Primitives.QuadXz(2, 2);
        
        GL.Color3(0.2f, 0.6f, 0.2f);
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex2(0.1, 0.4);
        GL.Vertex2(0.45, 0.4);
        GL.Vertex2(0.45, 0.6);
        GL.Vertex2(0.1, 0.6);
        GL.End();
        GL.Color3(0.6f, 0.2f, 0.2f);
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex2(0.55, 0.4);
        GL.Vertex2(0.9, 0.4);
        GL.Vertex2(0.9, 0.6);
        GL.Vertex2(0.55, 0.6);
        GL.End();

        GL.Color3(1f, 1f, 1f);
        BitmapText.Draw("PLAY", 0.18f, 0.48f, 0.05f);
        BitmapText.Draw("EXIT", 0.67f, 0.48f, 0.05f);
        BitmapText.Draw($"BEST: {_highScore}", 0.35f, 0.75f, 0.04f);

        GL.Enable(EnableCap.DepthTest);
        GL.MatrixMode(MatrixMode.Projection);
        GL.PopMatrix();
    }

    private void RenderGameOverOverlay()
    {
        GL.MatrixMode(MatrixMode.Projection);
        GL.PushMatrix();
        GL.LoadIdentity();
        GL.Ortho(0, 1, 0, 1, -1, 1);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();

        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.Lighting); // Оверлей не освещается
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha);
        
        GL.Color4(0f, 0f, 0f, 0.65f);
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex2(0, 0);
        GL.Vertex2(1, 0);
        GL.Vertex2(1, 1);
        GL.Vertex2(0, 1);
        GL.End();
        
        GL.Color3(1f, 1f, 1f);

        const float titleSize = 0.08f;
        string title = "GAME  OVER";
        float titleX = 0.5f - title.Length * titleSize * 0.55f;
        BitmapText.Draw(title, titleX, 0.60f, titleSize);

        const float subSize = 0.04f;
        string subtitle = "Press  SPACE  to  menu";
        float subX = 0.5f - subtitle.Length * subSize * 0.55f;
        BitmapText.Draw(subtitle, subX, 0.45f, subSize);

        GL.Disable(EnableCap.Blend);
        GL.Enable(EnableCap.DepthTest);

        GL.MatrixMode(MatrixMode.Projection);
        GL.PopMatrix();
    }

    private void StartGame()
    {
        _state = State.Play;
        _player.Reset();
        _lines.Clear();
        _score = 0;
        Title = "Akula vs Bikini Bottom 4D";
    }

    private void EnterMenu() => _state = State.Menu;

    private void GameOver()
    {
        _state = State.GameOver;
        if (_score > _highScore)
        {
            _highScore = _score;
            File.WriteAllText("highscore.txt", _highScore.ToString());
        }
    }

    private void SpawnLine()
    {
        float z = _player.Position.Z - 100f;
        int tallLeft = MaxTallInGroup;

        var line = new ObstacleLine(z);

        for (int lane = -1; lane <= 1; lane++)
        {
            float x = lane * 2f;
            float chance = Random.Shared.NextSingle();
            if (chance < 0.3f) continue;

            bool tall = chance > 0.75f && tallLeft > 0;
            if (tall) tallLeft--;
            
            // Добавляем препятствие в линию
            line.AddObstacle(new Obstacle(x, z, tall));
        }

        if (line.Obstacles.Count > 0)
            _lines.Add(line);
    }

    private static float RandomRange(float a, float b) =>
        a + Random.Shared.NextSingle() * (b - a);
}