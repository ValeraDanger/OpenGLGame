using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GameOpenGL;

public class Obstacle
{
    public Vector3 Position { get; set; } // Добавляем setter
    public Vector3 Size { get; }
    private readonly bool _isTall;
    private static int _shortTextureId;
    private static int _tallTextureId;
    
    private static bool _texturesLoaded = false;

    public Obstacle(float x, float z, bool tall = false)
    {
        Position = new Vector3(x, tall ? 0.8f : 0.4f, z);
        Size = new Vector3(0.8f, tall ? 1.6f : 0.8f, 0.8f);
        _isTall = tall;
        
        // Загружаем текстуры при создании первого препятствия
        if (!_texturesLoaded)
        {
            try 
            {
                _shortTextureId = TextureManager.LoadTexture("textures/obstacle_short.png");
                _tallTextureId = TextureManager.LoadTexture("textures/obstacle_tall.png");
                _texturesLoaded = true;
            }
            catch (System.IO.FileNotFoundException)
            {
                // Если файл не найден, используем стандартный красный цвет
                _texturesLoaded = false;
            }
        }
    }

    public bool CheckCollision(Vector3 spherePos, float sphereRadius)
    {
        Vector3 halfSize = Size / 2f;
        
        // Вручную рассчитываем абсолютную разницу вместо Vector3.Abs()
        Vector3 delta = new Vector3(
            MathF.Abs(spherePos.X - Position.X),
            MathF.Abs(spherePos.Y - Position.Y),
            MathF.Abs(spherePos.Z - Position.Z)
        );

        if (delta.X > halfSize.X + sphereRadius) return false;
        if (delta.Y > halfSize.Y + sphereRadius) return false;
        if (delta.Z > halfSize.Z + sphereRadius) return false;

        return true;
    }

    public void Draw()
    {
        GL.PushMatrix();
        GL.Translate(Position);

        // Установка материала для блеска
        float[] specReflection = { 0.9f, 0.9f, 0.9f, 1.0f }; // Яркий белый блик
        GL.Material(MaterialFace.Front, MaterialParameter.Specular, specReflection);
        GL.Material(MaterialFace.Front, MaterialParameter.Shininess, 100.0f); // Высокое значение для резкого блика
        
        if (_texturesLoaded)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _isTall ? _tallTextureId : _shortTextureId);
            GL.Color3(1.0f, 1.0f, 1.0f); 
            
            DrawTexturedCube(Size);
            
            GL.Disable(EnableCap.Texture2D);
        }
        else
        {
            // Если текстуры не загружены, используем обычный красный цвет
            GL.Color3(1.0f, 0.0f, 0.0f);
            Primitives.Cube(Size);
        }

        // Сброс свойств материала к значениям по умолчанию (или к другим, если нужно)
        float[] defaultSpec = { 0.0f, 0.0f, 0.0f, 1.0f }; // Без блика
        GL.Material(MaterialFace.Front, MaterialParameter.Specular, defaultSpec);
        GL.Material(MaterialFace.Front, MaterialParameter.Shininess, 0.0f);

        GL.PopMatrix();
    }

    public void DrawShadow()
    {
        GL.PushMatrix();

        GL.Disable(EnableCap.Lighting); // Тени не освещаются
        GL.DepthMask(false);            // Не пишем в буфер глубины
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        GL.Color4(0.0f, 0.0f, 0.0f, 0.35f); // Цвет тени: черный, полупрозрачный

        const float groundLevel = 0.015f; // Тот же уровень, что и у тени игрока

        GL.Translate(Position.X, groundLevel, Position.Z);
        GL.Scale(1.0f, 0.05f, 1.0f); // Сплющиваем тень

        // Рисуем базовую форму препятствия (куб) для тени
        // Используем Size препятствия, чтобы тень соответствовала его размерам
        Primitives.Cube(Size); 

        GL.Disable(EnableCap.Blend);
        GL.DepthMask(true); // Включаем обратно запись в буфер глубины
        // GL.Enable(EnableCap.Lighting); // Освещение будет включено основным рендер-циклом

        GL.PopMatrix();
    }
    
    private static void DrawTexturedCube(Vector3 size)
    {
        Vector3 h = size / 2f;
        
        GL.Begin(PrimitiveType.Quads);
        
        // Передняя грань
        GL.Normal3(0.0f, 0.0f, 1.0f);
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, h.Z);
        
        // Задняя грань
        GL.Normal3(0.0f, 0.0f, -1.0f);
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, -h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, -h.Z);
        
        // Левая грань
        GL.Normal3(-1.0f, 0.0f, 0.0f);
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(-h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, -h.Z);
        
        // Правая грань
        GL.Normal3(1.0f, 0.0f, 0.0f);
        GL.TexCoord2(0, 0); GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(h.X, h.Y, -h.Z);
        
        // Верхняя грань
        GL.Normal3(0.0f, 1.0f, 0.0f);
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, h.Y, -h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, h.Z);
        
        // Нижняя грань
        GL.Normal3(0.0f, -1.0f, 0.0f);
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, -h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, -h.Y, h.Z);
        
        GL.End();
    }
}