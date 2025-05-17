using OpenTK.Mathematics;
using System.Collections.Generic;

namespace GameOpenGL;

public class ObstacleLine
{
    public float Z;
    public bool Passed;
    public List<Obstacle> Obstacles = new();

    public ObstacleLine(float z)
    {
        Z = z;
    }

    public void AddObstacle(Obstacle obstacle)
    {
        Obstacles.Add(obstacle);
    }

    public void Update(float dz)
    {
        Z += dz;
        foreach (var ob in Obstacles)
            ob.Position = new Vector3(ob.Position.X, ob.Position.Y, Z);
    }

    public void Draw()
    {
        foreach (var ob in Obstacles)
            ob.Draw();
    }
}