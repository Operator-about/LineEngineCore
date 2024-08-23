using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Камера
public class Camera
{
    //Переменные
    public Vector3 _Position { get; set; }
    public Vector3 _Front { get; private set; } = -Vector3.UnitZ;
    public Vector3 _Up { get; private set; } = Vector3.UnitY;
    public Vector3 _Right => Vector3.Normalize(Vector3.Cross(_Front, _Up));



    public float _Yaw = -90.0f;
    public float _Pitch = 0.0f;
    public float _Fov = 45.0f;
    //Инцилизация
    public Camera(Vector3 _StartPos)
    {
        _Position = _StartPos;
        UpdateVector();
    }

    public Matrix4 GetView()
    {
        return Matrix4.LookAt(_Position, _Position + _Front, _Up);
    }

    public Matrix4 GetProjection(float _AspRat)
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_Fov), _AspRat, 0.1f, 100.0f);
    }

    public void UpdateVector()
    {
        _Front = new Vector3(
            MathF.Cos(MathHelper.DegreesToRadians(_Yaw) * MathF.Cos(MathHelper.DegreesToRadians(_Pitch))),
            MathF.Sin(MathHelper.DegreesToRadians(_Pitch)),
            MathF.Sin(MathHelper.DegreesToRadians(_Yaw) * MathF.Cos(MathHelper.DegreesToRadians(_Pitch)))
        );

        _Front.Normalize();
        
    }

    
}
