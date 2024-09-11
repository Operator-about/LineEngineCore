using OpenTK;
using Assimp;
using System;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Net.NetworkInformation;





/*
Движок ещё в разработке!
Он не готов
*/




class MainSettingsEngine
{
    public GameWindow _Window;
    public NativeWindowSettings _WindowSettings;
    public GameWindowSettings _GameSettings;

    private ShaderSystem _Shader = new ShaderSystem();
    private Camera _Camera;
    private Import _Import;
    private bool _StatusImport = false;
    private string _CommandStart = "~command add -";
    public string _ErrorCode = "#0000";
    private string _PathModel;
    private bool _ActivImport;
    private Vector3 _PositionLight;
    private Vector3 _ColorLight;
    private bool _CheckShaderImport = true;






    public void Activation()
    {
        _WindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800,800),
            Title = "LineEngine"
        };

        _GameSettings = new GameWindowSettings()
        {

            UpdateFrequency = 60

        };


        _Window = new GameWindow(_GameSettings, _WindowSettings);
       
        _Window.Unload += _Window_Unload;
        _Window.Load += _Window_Load;
        _Window.RenderFrame += _Window_RenderFrame;
        _Window.UpdateFrame += _Window_UpdateFrame;

        _Window.Run();
    }

    
    private void _Window_Load()
    {
        _Shader.UseAndIntilisation("D:\\LineEngine\\Core\\LineEngineCore\\Shader\\VertShader.glsl", "D:\\LineEngine\\Core\\LineEngineCore\\Shader\\FragShader.glsl");
        _Shader.Use();
        _Camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f));
        _CheckShaderImport = false;
        //if (_CheckShaderImport == true)
        //{
        //    try
        //    {
        //        _Shader.UseAndIntilisation("D:\\LineEngine\\Core\\LineEngineCore\\Shader\\VertShader.glsl", "D:\\LineEngine\\Core\\LineEngineCore\\Shader\\FragShader.glsl");
        //        _Shader.Use();
        //        GL.Enable(EnableCap.CullFace);
        //        GL.CullFace(CullFaceMode.Back);
        //        Console.WriteLine($"Shader import done, exit code:{_ErrorCode}");
        //    }
        //    catch
        //    {
        //        _ErrorCode = "#1111";
        //        Console.WriteLine($"System Error Detected! Error Code:{_ErrorCode}");
        //    }
            

        //    _Camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f));
        //    _CheckShaderImport = false;
        //}







        _Import = new Import();
        _Import.ImportModel("D:\\3D\\OPERATOR.fbx");
        //if (_ActivImport == true)
        //{
        //    _Import = new Import();
        //    _Import.ImportModel(_PathModel);
        //    Console.WriteLine(_PathModel);
        //    _ActivImport = false;
        //    Console.WriteLine("Import Done");
        //}
        

        GL.ClearColor(Color4.DimGray);
    }
   


    private void _Window_RenderFrame(FrameEventArgs obj)
    {

        float _ApsRat = _Window.Size.X / (float)_Window.Size.Y;


        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        

        Vector3 _LightPos = new Vector3(1.2f,1.0f,2.0f);
        Vector3 _ViewPos = _Camera._Position;

        
        //Настройки света
        _Shader.Use();


        

        
        _Shader.SetVector3("viewPos", _ViewPos);
        _Shader.Use();
        _Shader.SetVector3("lightPos", _PositionLight);
        _Shader.SetVector3("lightColor", new Vector3(_ColorLight));
        _Shader.SetVector3("objectColor", new Vector3(1.0f, 0.5f, 0.3f));
        //Конец настройки света

        _Shader.SetMatrix4("view", _Camera.GetView());
        _Shader.SetMatrix4("projection", _Camera.GetProjection(_ApsRat));

        Matrix4 _Model = Matrix4.Identity;
        _Shader.SetMatrix4("model", _Model);


        if (_StatusImport == false)
        {
            _StatusImport = true;
            Thread _A = new Thread(AddOrUse);
            _A.Start();
        }


        _Import.Draw(_Shader);
        
        _Window.SwapBuffers();
    }

    private void _Window_UpdateFrame(FrameEventArgs obj)
    {
        //Движение камеры
        var _Input = _Window.KeyboardState;
        const float _CameraSpeed = 1.5f * 0.01f;

        if (_Input.IsKeyDown(Keys.W))
        {
            _Camera._Position += _Camera._Front * _CameraSpeed;
            if (_Camera._Debugging == true)
            {
                Console.WriteLine($"Coordinate(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
            }
        }
        if (_Input.IsKeyDown(Keys.S))
        {
            _Camera._Position -= _Camera._Front * _CameraSpeed;
            if (_Camera._Debugging == true)
            {
                Console.WriteLine($"Coordinate(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
            }
        }
        if (_Input.IsKeyDown(Keys.A))
        {
            _Camera._Position -= _Camera._Front * _CameraSpeed;
            if (_Camera._Debugging == true)
            {
                Console.WriteLine($"Coordinate(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
            }
        }
        if (_Input.IsKeyDown(Keys.D))
        {
            _Camera._Position += _Camera._Front * _CameraSpeed;
            if (_Camera._Debugging == true)
            {
                Console.WriteLine($"Coordinate(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
            }

        }

        var _Mouse = _Window.MouseState;
        const float _Sensitivity = 0.2f;

        _Camera._Yaw += _Mouse.Delta.X * _Sensitivity;
        _Camera._Pitch -= _Mouse.Delta.Y * _Sensitivity;
        _Camera._Pitch = MathHelper.Clamp(_Camera._Pitch, -89.0f,89.0f);

        _Camera.UpdateVector();
    }




    private void _Window_Unload()
    {
        
    }

  
    //Добавление предметов(Доделать)
    public void AddOrUse()
    {
        Console.Write(_CommandStart);
        string _Mode = _CommandStart + Console.ReadLine();
        string[] _AllCommands = new string[6] {$"{_CommandStart} Model",$"{_CommandStart} Light", $"{_CommandStart} Help", $"{_CommandStart} Get Camera Vector",$"{_CommandStart} Camera Debugging", $"{_CommandStart} Show FPS" };



        _PositionLight = new Vector3();


        //Импорт модели
        if (_Mode == _AllCommands[0])
        {
            try
            {
                Console.Write("Pls, write path model her:");
                _PathModel = Console.ReadLine()!;
                _ActivImport = true;
                _Window_Load();
                Console.WriteLine("Success");               
                _StatusImport = false;
                

            }
            catch
            {
                _ErrorCode = "#0002";
                Console.WriteLine($"System Error Detected! Error Code:{_ErrorCode}");
                _StatusImport = false;
                
                
            }

        }

        //Добавление света
        else if (_Mode == _AllCommands[1])
        {
            try
            {
                Console.WriteLine("Pls, write you mode coordinate(if you want get camera coordinate for light pos. pls write command here):");
                _Mode = Console.ReadLine()!;
                if (_Mode == _AllCommands[3])
                {
                    _PositionLight = _Camera._Position;
                    Console.WriteLine("Coordinate get done");
                }
                else
                {
                    Console.WriteLine("X:");
                    int _X = Int32.Parse(Console.ReadLine());
                    _PositionLight.X = _X;
                    Console.WriteLine("Y:");
                    int _Y = Int32.Parse(Console.ReadLine());
                    _PositionLight.Y = _Y;
                    Console.WriteLine("Z:");
                    int _Z = Int32.Parse(Console.ReadLine());
                    _PositionLight.Z = _Z;

                }
                


            
                Console.WriteLine("Pls, add light color:");
                Console.WriteLine("R:");
                int _R = Int32.Parse(Console.ReadLine()!);
                
                Console.WriteLine("G:");
                int _G = Int32.Parse(Console.ReadLine()!);
                
                Console.WriteLine("B:");
                int _B = Int32.Parse(Console.ReadLine()!);

                _ColorLight = new Vector3(_R, _G, _B);


                

                Console.WriteLine("Success");
                _StatusImport = false;
            }
            catch
            {
                _ErrorCode = "#0010";
                Console.WriteLine($"System Error Detected! Error Code:{_ErrorCode}");
                _StatusImport = false;
            }
        }

        //Помощь
        else if(_Mode == _AllCommands[2])
        {
            Console.WriteLine("Reminder! All commands start with: ~command add - " +
                "After this, you can write everything that can be seen below: " +
                " ~command add - Model " +
                " ~command add - Light. " +
                " ~command add - Get Camera Vector" +
                " ~command add - Camera Debugging" +
                " And you can use command: ~command add - Help for more information");

            _StatusImport = false;


        }
        else if (_Mode == _AllCommands[3])
        {
            Console.WriteLine($"Camera coordinate: X:{_Camera._Position.X}. Y:{_Camera._Position.Y}. Z:{_Camera._Position.Z}");
            _StatusImport = false;
        }
        else if (_Mode == _AllCommands[4])
        {
            if (_Camera._Debugging == true)
            {
                _Camera._Debugging = false;
                Console.WriteLine("Debugging offline");
                _StatusImport = false;
            }
            else
            {
                _Camera._Debugging = true;
                Console.WriteLine("Debugging online");
                _StatusImport = false;
            }
            
        }
        else if (_Mode == _AllCommands[5])
        {
            GetFPS(true);
        }
        else
        {
            _StatusImport = false;
        }

       
        
    }

    private void GetFPS(bool _Activ)
    {
        /*
        while (_Activ)
        {
            Task.Delay(1000000000);
            Console.WriteLine(_GameSettings.UpdateFrequency);
        }
        */
    }
}



//Импорт
class Import
{


    private List<float> _Vert = new List<float>();
    private List<float> _Textures = new List<float>();
    private List<float> _Normals = new List<float>();
    private int _VAO;
    private int _VBO;
    private int _VBON;
    private int _VBOTC;

    public void ImportModel(string _Path)
    {
        var _Import = new AssimpContext();
        var _Scene = _Import.ImportFile(_Path, PostProcessPreset.TargetRealTimeMaximumQuality);

        foreach (var _Mesh in _Scene.Materials)
        {

        }
        foreach (var _Material in _Scene.Materials)
        {

        }
    }

    private void LoadModel(Mesh _Mesh)
    {
        _VAO = GL.GenVertexArray();
        GL.BindVertexArray(_VAO);

        _VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);

        foreach (var _Vertex in _Mesh.Vertices)
        {
            _Vert.Add(_Vertex.X);
            _Vert.Add(_Vertex.Y);
            _Vert.Add(_Vertex.Z);

            if (_Mesh.HasTextureCoords(0))
            {
                _Vert.Add(_Mesh.TextureCoordinateChannels[0][_Vertex].X);
                _Vert.Add(_Mesh.TextureCoordinateChannels[0][_Vertex].Y);
            }
        }

        GL.BufferData(BufferTarget.ArrayBuffer, _Vert.Count*sizeof(float), _Vert.ToArray(), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float, false, 5*sizeof(float), 0);
    }

    public void Draw(ShaderSystem _Shader)
    {
        _Shader.Use();

        //Загрузка шейдеров

        GL.BindVertexArray(_VAO);
        GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, _Vert.Count / 3);
        GL.BindVertexArray(0);
    }


}






//Шейдеры
class ShaderSystem()
{
    public int _Hand;
    

    public void UseAndIntilisation(string _VertShaderPath, string _FragShaderPath)
    {
        string _VertSource = File.ReadAllText(_VertShaderPath);
        string _FragSource = File.ReadAllText(_FragShaderPath);

        int _VertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_VertShader, _VertSource);
        GL.CompileShader(_VertShader);
        CheckAllSystem(_VertShader, true, false);

        int _FragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_FragShader, _FragSource);
        GL.CompileShader(_FragShader);
        CheckAllSystem(_FragShader, true, false);

        _Hand = GL.CreateProgram();
        GL.AttachShader(_Hand, _VertShader);
        GL.AttachShader(_Hand, _FragShader);
        GL.LinkProgram(_Hand);
        CheckAllSystem(_Hand, false, true);

        GL.DetachShader(_Hand, _VertShader);
        GL.DetachShader(_Hand, _FragShader);
        GL.DeleteShader(_VertShader);
        GL.DeleteShader(_FragShader);
    }

    public void Use()
    {
        try
        {
            GL.UseProgram(_Hand);
        }
        catch
        {
            Console.WriteLine($"System Error Detected! Error Code:#1111");
        }
        
    }

    public void SetInt(string _Name, int _Value)
    {
        int _Location = GL.GetUniformLocation(_Hand, _Name);
        if (_Location == -1)
        {
            Console.WriteLine($"Предупреждение! {_Name} не найдено в шейдере!");
        }
        GL.Uniform1(_Location, _Value);
    }

    public void SetFloat(string _Name, float _Value)
    {
        int _Location = GL.GetUniformLocation(_Hand, _Name);
        if(_Location == -1)
        {
            Console.WriteLine($"Предупреждение!{_Name}");
        }
        GL.Uniform1(_Location, _Value);
    }

    public void SetVector3(string _Name, Vector3 _Value)
    {
        int _Location = GL.GetUniformLocation(_Hand, _Name);
        if (_Location == -1)
        {
            throw new Exception(_Name);
        }
        GL.Uniform3(_Location, _Value);
    }

    private void CheckAllSystem(int _Shader, bool _CheckShader, bool _CheckLink)
    {
        
        var _Error = GL.GetError();
        if (_Error != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)
        {
            throw new Exception(_Error.ToString());
        }




        if(_CheckShader == true)
        {
            GL.GetShader(_Shader, ShaderParameter.CompileStatus, out int _Success);
            if (_Success == 0)
            {
                string _LogInfo = GL.GetShaderInfoLog(_Shader);
                throw new Exception(_LogInfo);
            }
        }

        if(_CheckLink == true)
        {
            GL.GetProgram(_Shader, GetProgramParameterName.LinkStatus, out int _SuccessLink);
            if (_SuccessLink == 0)
            {
                string _LogInfoLink = GL.GetProgramInfoLog(_Shader);
                throw new Exception(_LogInfoLink);
            }
        }

    }

    public void SetMatrix4(string _Name, Matrix4 _Matrix)
    {
        int _Location = GL.GetUniformLocation(_Hand, _Name);
        GL.UniformMatrix4(_Location, false, ref _Matrix);
    }
}


class Start()
{  
    static void Main()
    {
        MainSettingsEngine _Engine = new MainSettingsEngine();
        _Engine.Activation();        
    }
}
