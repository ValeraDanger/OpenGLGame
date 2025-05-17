using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace GameOpenGL;

public class Player
{
    // ――― полосы ―――
    private int _currentLane = 0;
    private const float LaneOffset = 2f;

    // ――― прыжок ―――
    private float _velocityY = 0f;
    private const float Gravity = 20f;
    private const float JumpImpulse = 8f;

    public Vector3 Position { get; private set; } = new(0, 0.5f, 0);


    private float _rollAngle = 0f;

    private const float Radius = 0.5f;

    // скорость вращения
    private const float RollRate = 10f * 360f / (MathF.PI * 2f * Radius);

    public void Update(float dt, KeyboardState kbd)
    {
        // перестроения A / D
        if (kbd.IsKeyPressed(Keys.A) && _currentLane > -1) _currentLane--;
        if (kbd.IsKeyPressed(Keys.D) && _currentLane < 1) _currentLane++;

        float targetX = _currentLane * LaneOffset;
        Position = new Vector3(MathHelper.Lerp(Position.X, targetX, 10f * dt),
            Position.Y,
            Position.Z);

        // прыжок (Space)
        if (kbd.IsKeyPressed(Keys.Space) && Position.Y <= 0.51f)
            _velocityY = JumpImpulse;

        _velocityY -= Gravity * dt;
        Position += new Vector3(0, _velocityY * dt, 0);

        if (Position.Y < 0.5f) // приземление
        {
            Position = new Vector3(Position.X, 0.5f, Position.Z);
            _velocityY = 0f;
        }
    }

    public void Reset()
    {
        _currentLane = 0;
        Position = new Vector3(0, 0.5f, 0);
        _velocityY = 0f;
        _rollAngle = 0f;
    }

    public void Draw()
    {
        GL.PushMatrix();
        GL.Translate(Position);

        // ― Тело акулы ―
        GL.Color3(0.5f, 0.55f, 0.6f); // Серо-синий цвет для акулы

        // Основное тело (вытянутый куб)
        GL.PushMatrix();
        GL.Scale(1f, 0.8f, 2.5f); // Делаем тело более плоским и длинным
        Primitives.Cube(new Vector3(Radius, Radius, Radius)); // Базовый размер куба до масштабирования
        GL.PopMatrix();

        // Нос акулы
        GL.PushMatrix();
        // Позиционируем спереди тела. Тело имеет длину Radius * 2.5f. Передняя точка тела: Z_center + (Radius*2.5f)/2
        // Нос имеет "длину" (по Z) Radius*0.3f. Его центр: Передняя_точка_тела + (Radius*0.3f)/2
        GL.Translate(0, -Radius * 0.1f, (Radius * 2.5f / 2f) + (Radius * 0.3f / 2f) - Radius*0.1f); // Немного ниже и чуть назад от самого края
        GL.Scale(0.6f, 0.6f, 0.8f); // Делаем нос поменьше и чуть вытянутым
        Primitives.Cube(new Vector3(Radius, Radius, Radius));
        GL.PopMatrix();

        // Спинной плавник
        GL.PushMatrix();
        GL.Translate(0, (Radius * 0.8f / 2f) + (Radius * 0.7f / 2f) * 0.8f, Radius * 0.2f); // Смещаем немного вперед по Z
        GL.Scale(1f, 0.8f, 1f); // Масштабируем плавник если нужно
        Primitives.Cube(new Vector3(Radius * 0.15f, Radius * 0.7f, Radius * 0.4f));
        GL.PopMatrix();

        // Хвостовой плавник (вертикальный)
        GL.PushMatrix();
        GL.Translate(0, 0, -(Radius * 2.5f / 2f) - (Radius * 0.2f / 2f));
        Primitives.Cube(new Vector3(Radius * 0.1f, Radius * 0.8f, Radius * 0.2f));
        GL.PopMatrix();
        
        // Боковые плавники (пара)
        // Правый плавник
        GL.PushMatrix();
        GL.Translate(Radius / 1.8f, -Radius * 0.1f, Radius * 0.3f); // Смещаем немного вниз и вперед
        GL.Rotate(20, 0, 0, 1);  // Наклон вокруг оси Z объекта (вперед)
        GL.Rotate(-30, 0, 1, 0); // Наклон наружу вокруг оси Y объекта
        Primitives.Cube(new Vector3(Radius * 0.5f, Radius * 0.15f, Radius * 0.6f)); // Ширина, Толщина, Длина плавника
        GL.PopMatrix();

        // Левый плавник
        GL.PushMatrix();
        GL.Translate(-Radius / 1.8f, -Radius * 0.1f, Radius * 0.3f);
        GL.Rotate(-20, 0, 0, 1); // Наклон вокруг оси Z объекта (вперед)
        GL.Rotate(30, 0, 1, 0);  // Наклон наружу вокруг оси Y объекта
        Primitives.Cube(new Vector3(Radius * 0.5f, Radius * 0.15f, Radius * 0.6f));
        GL.PopMatrix();

        // Глаза акулы
        GL.Color3(0.0f, 0.0f, 0.0f); // Черный цвет для глаз
        // Правый глаз
        // Позиционируем относительно передней части тела. Z тела = Radius * 2.5f.
        // X тела = Radius. Y тела = Radius * 0.8f.
        // Смещаем вперед, немного вбок и вверх от центральной линии.
        // Диск рисуется в плоскости XY, поэтому нам нужно его повернуть, чтобы он смотрел вбок.
        GL.PushMatrix();
        GL.Translate(Radius * 0.4f, Radius * 0.15f, Radius * 0.8f); // X, Y, Z позиция глаза
        GL.Rotate(90, 0, 1, 0); // Поворачиваем диск, чтобы он смотрел вбок по оси X
        DrawDisc(0, 0, 0, Radius * 0.1f);
        GL.PopMatrix();

        // Левый глаз
        GL.PushMatrix();
        GL.Translate(-Radius * 0.4f, Radius * 0.15f, Radius * 0.8f); // X, Y, Z позиция глаза
        GL.Rotate(-90, 0, 1, 0); // Поворачиваем диск, чтобы он смотрел вбок по оси -X
        DrawDisc(0, 0, 0, Radius * 0.1f);
        GL.PopMatrix();


        GL.PopMatrix(); // Завершение трансформаций для игрока
    }

    // ───── маленькие вспомогательные фигуры ─────
    private static void DrawDisc(float x, float y, float z, float r = 0.05f, int seg = 12)
    {
        GL.PushMatrix();
        GL.Translate(x, y, z);
        GL.Begin(PrimitiveType.TriangleFan);
        GL.Vertex3(0, 0, 0);
        for (int i = 0; i <= seg; i++)
        {
            double a = i / (double)seg * Math.PI * 2;
            GL.Vertex3(Math.Cos(a) * r, Math.Sin(a) * r, 0);
        }

        GL.End();
        GL.PopMatrix();
    }

    private static void DrawSmile(float x, float y, float z, float r, int seg = 20)
    {
        GL.PushMatrix();
        GL.Translate(x, y, z);
        GL.Begin(PrimitiveType.TriangleFan);
        GL.Vertex3(0, 0, 0);
        for (int i = 0; i <= seg; i++)
        {
            double a = Math.PI + i / (double)seg * Math.PI; // нижняя полуокружность
            GL.Vertex3(Math.Cos(a) * r, Math.Sin(a) * r, 0);
        }

        GL.End();
        GL.PopMatrix();
    }
}