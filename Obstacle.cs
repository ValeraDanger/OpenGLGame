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
        
        if (_texturesLoaded)
        {
            // Включаем текстурирование
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _isTall ? _tallTextureId : _shortTextureId);
            GL.Color3(1.0f, 1.0f, 1.0f); // Белый цвет для отображения текстуры без искажения цвета
            
            // Рисуем текстурированный куб
            DrawTexturedCube(Size);
            
            // Отключаем текстурирование
            GL.Disable(EnableCap.Texture2D);
        }
        else
        {
            // Если текстуры не загружены, используем обычный красный цвет
            GL.Color3(1.0f, 0.0f, 0.0f);
            Primitives.Cube(Size);
        }
        
        GL.PopMatrix();
    }
    
    private static void DrawTexturedCube(Vector3 size)
    {
        Vector3 h = size / 2f;
        
        GL.Begin(PrimitiveType.Quads);
        
        // Передняя грань
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, h.Z);
        
        // Задняя грань
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, -h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, -h.Z);
        
        // Левая грань
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(-h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(-h.X, h.Y, -h.Z);
        
        // Правая грань
        GL.TexCoord2(0, 0); GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(h.X, h.Y, -h.Z);
        
        // Верхняя грань
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(-h.X, h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(h.X, h.Y, -h.Z);
        
        // Нижняя грань
        GL.TexCoord2(0, 0); GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.TexCoord2(1, 0); GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.TexCoord2(1, 1); GL.Vertex3(h.X, -h.Y, h.Z);
        GL.TexCoord2(0, 1); GL.Vertex3(h.X, -h.Y, -h.Z);
        
        GL.End();
    }
}