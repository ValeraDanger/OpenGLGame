using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GameOpenGL;

public class LaneManager
{
    private readonly List<Vector3> _tiles = new();
    private const float TileLen = 40f;
    private const int TilesAhead = 5;
    private const float Speed = 10f;

    public LaneManager()
    {
        for (int i = 0; i < TilesAhead; i++)
            _tiles.Add(new Vector3(0, 0, -i * TileLen));
    }

    public void Update(float dt)
    {
        float dz = Speed * dt;
        for (int i = 0; i < _tiles.Count; i++)
        {
            var t = _tiles[i];
            t.Z += dz;
            if (t.Z > TileLen) t.Z -= TilesAhead * TileLen;
            _tiles[i] = t;
        }
    }

    public void Draw()
    {
        const float roadWidth = 6f;
        const float centerW = 2f; // Центральная "дорожка"
        const float stripeW = 0.0f; // Ширина разделительных элементов (если нужны)
        const int dashes = 6; // Количество разделительных элементов на плитку

        // Уровни высоты (можно оставить небольшие различия для визуального интереса)
        const float ySeabedSide = 0f;    // Боковые части морского дна
        const float yMainPath = 0.010f;  // Основная "тропа" из песка
        const float yMarkings = 0.012f; // Разделительные элементы, чуть выше

        foreach (var t in _tiles)
        {
            GL.PushMatrix();
            GL.Translate(t);
            
            // Боковые части морского дна (бывшая "трава")
            GL.Color3(0.7f, 0.6f, 0.4f); // Песочно-коричневый
            GL.PushMatrix();
            GL.Translate(0, ySeabedSide, 0);
            Primitives.QuadXz(20f, TileLen, -8f);
            Primitives.QuadXz(20f, TileLen, 8f);
            GL.PopMatrix();
            
            // Основная "тропа" (бывшая "дорога")
            GL.PushMatrix();
            GL.Translate(0, yMainPath, 0);
            GL.Color3(0.85f, 0.75f, 0.55f); // Светло-песочный
            Primitives.QuadXz(roadWidth, TileLen);

            // Центральная часть "тропы" (если нужна для выделения)
            // GL.Color3(0.9f, 0.8f, 0.6f); // Еще светлее песочный
            // Primitives.QuadXz(centerW, TileLen, 0); // Раскомментируйте, если хотите выделить центр
            GL.PopMatrix();
            
            // Разделительные полосы (можно сделать их менее заметными или убрать)
            GL.PushMatrix();
            GL.Translate(0, yMarkings, 0);
            GL.Color3(0.75f, 0.65f, 0.45f); // Темнее песочный для контраста или цвет водорослей
            float dashLen = TileLen / dashes;
            for (int i = 0; i < dashes; i += 2) // Рисуем через одну, чтобы были промежутки
            {
                float z = -TileLen / 2 + i * dashLen + dashLen / 2;
                Primitives.QuadXz(stripeW, dashLen, -1f, z); // левый
                Primitives.QuadXz(stripeW, dashLen, 1f, z); // правый
            }

            GL.PopMatrix();

            GL.PopMatrix();
        }
    }
}