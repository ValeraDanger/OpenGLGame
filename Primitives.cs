using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GameOpenGL;

public static class Primitives
{
    public static void QuadXz(float w, float h, float x = 0, float z = 0)
    {
        float width = w / 2f;
        float length = h / 2f;
        GL.Begin(PrimitiveType.Quads);
        GL.Vertex3(x - width, 0, z - length);
        GL.Vertex3(x + width, 0, z - length);
        GL.Vertex3(x + width, 0, z + length);
        GL.Vertex3(x - width, 0, z + length);
        GL.End();
    }

    public static void Sphere(float r, int slices, int stacks)
    {
        for (int i = 0; i < stacks; i++)
        {
            double lat0 = Math.PI * (-0.5 + (double)i / stacks);
            double lat1 = Math.PI * (-0.5 + (double)(i + 1) / stacks);
            double z0 = r * Math.Sin(lat0), zr0 = r * Math.Cos(lat0);
            double z1 = r * Math.Sin(lat1), zr1 = r * Math.Cos(lat1);

            GL.Begin(PrimitiveType.QuadStrip);
            for (int j = 0; j <= slices; j++)
            {
                double lng = 2 * Math.PI * (j % slices) / slices;
                double x = Math.Cos(lng), y = Math.Sin(lng);
                GL.Normal3(x * zr0, y * zr0, z0);
                GL.Vertex3(x * zr0, y * zr0, z0);
                GL.Normal3(x * zr1, y * zr1, z1);
                GL.Vertex3(x * zr1, y * zr1, z1);
            }

            GL.End();
        }
    }

    public static void Cube(Vector3 s)
    {
        Vector3 h = s / 2f;
        GL.Begin(PrimitiveType.Quads);
        // фронт
        GL.Normal3(0.0f, 0.0f, 1.0f);
        GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.Vertex3(h.X, -h.Y, h.Z);
        GL.Vertex3(h.X, h.Y, h.Z);
        GL.Vertex3(-h.X, h.Y, h.Z);
        // зад
        GL.Normal3(0.0f, 0.0f, -1.0f);
        GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.Vertex3(-h.X, h.Y, -h.Z);
        GL.Vertex3(h.X, h.Y, -h.Z);
        GL.Vertex3(h.X, -h.Y, -h.Z);
        // лево
        GL.Normal3(-1.0f, 0.0f, 0.0f);
        GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.Vertex3(-h.X, h.Y, h.Z);
        GL.Vertex3(-h.X, h.Y, -h.Z);
        // право
        GL.Normal3(1.0f, 0.0f, 0.0f);
        GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.Vertex3(h.X, h.Y, -h.Z);
        GL.Vertex3(h.X, h.Y, h.Z);
        GL.Vertex3(h.X, -h.Y, h.Z);
        // верх
        GL.Normal3(0.0f, 1.0f, 0.0f);
        GL.Vertex3(-h.X, h.Y, -h.Z);
        GL.Vertex3(-h.X, h.Y, h.Z);
        GL.Vertex3(h.X, h.Y, h.Z);
        GL.Vertex3(h.X, h.Y, -h.Z);
        // низ
        GL.Normal3(0.0f, -1.0f, 0.0f);
        GL.Vertex3(-h.X, -h.Y, -h.Z);
        GL.Vertex3(h.X, -h.Y, -h.Z);
        GL.Vertex3(h.X, -h.Y, h.Z);
        GL.Vertex3(-h.X, -h.Y, h.Z);
        GL.End();
    }
}