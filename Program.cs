using OpenTK;
using Assimp;
using System;
using StbImageSharp;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using OpenTK.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel;
using System.Runtime.CompilerServices;




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

    

    public void Activation()
    {
        _WindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800,800),
            Title = "LineEngine"
        };

        _GameSettings = new GameWindowSettings()
        {

            UpdateFrequency = 1000

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
        _Shader.UseAndIntilisation("Shader\\VertShader.glsl", "Shader\\FragShader.glsl");
        _Shader.Use();

        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        _Camera = new Camera(new Vector3(0.0f, 0.0f, 0.0f));


        _Import = new Import();
        _Import.ImportModel("You model.fbx");
        GL.ClearColor(Color4.CornflowerBlue);
    }
   


    private void _Window_RenderFrame(FrameEventArgs obj)
    {

        float _ApsRat = _Window.Size.X / (float)_Window.Size.Y;


        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        

        Vector3 _LightPos = new Vector3(1.2f,1.0f,2.0f);
        Vector3 _ViewPos = _Camera._Position;

        
        //Настройки света
        _Shader.Use();

        


        _Shader.SetVector3("lightPos", _LightPos);
        _Shader.SetVector3("viewPos", _ViewPos);
        _Shader.SetVector3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
        _Shader.SetVector3("objectColor", new Vector3(1.0f,0.5f,0.3f));
        //Конец настройки света

        _Shader.SetMatrix4("view", _Camera.GetView());
        _Shader.SetMatrix4("projection", _Camera.GetProjection(_ApsRat));

        Matrix4 _Model = Matrix4.Identity;
        _Shader.SetMatrix4("model", _Model);


        _Import.Draw(_Shader);
        GL.DrawArrays(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 3);
        GL.BindVertexArray(0);
        GL.Disable(EnableCap.CullFace);
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
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }
        if (_Input.IsKeyDown(Keys.S))
        {
            _Camera._Position -= _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }
        if (_Input.IsKeyDown(Keys.A))
        {
            _Camera._Position -= _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
        }
        if (_Input.IsKeyDown(Keys.D))
        {
            _Camera._Position += _Camera._Front * _CameraSpeed;
            Console.WriteLine($"Координаты(X){_Camera._Position.X} (Y){_Camera._Position.Y} (Z){_Camera._Position.Z}");
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

    //Импорт модели
    public void ImportModel(string _File)
    {
        var _Import = new AssimpContext();
        var _Scene = _Import.ImportFile(_File, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        foreach (var _Mesh in _Scene.Meshes)
        {
            //Координаты
            foreach (var _Vertex in _Mesh.Vertices)
            {
                _Vert.Add(_Vertex.X);
                _Vert.Add(_Vertex.Y);
                _Vert.Add(_Vertex.Z);
            }

            //Нормали
            foreach (var _Normal in _Mesh.Normals)
            {
                _Normals.Add(_Normal.X);
                _Normals.Add(_Normal.Y);
                _Normals.Add(_Normal.Z);
            }

            //Координаты текстур
            foreach (var _TextureCoord in _Mesh.TextureCoordinateChannels[0])
            {
                _Textures.Add(_TextureCoord.X);
                _Textures.Add(_TextureCoord.Y);
            }



            //Проверка на имение текстуры
            var _Material = _Scene.Materials[_Mesh.MaterialIndex];
            if (_Material.HasTextureDiffuse)
            {
                var _TextureFile = _Material.TextureDiffuse.FilePath;
                var _Texture = LoadTexture(_TextureFile);
                _Textures.Add(_Texture);
            }

        }


        foreach (var _Mesh in _Scene.Meshes)
        {
            foreach (Assimp.Vector3D _Vertex in _Mesh.Vertices)
            {
                Vector3 _ConVert = ConvertVecOpenTK(_Vertex);
                _Vert.Add(_ConVert.X);
                _Vert.Add(_ConVert.Y);
                _Vert.Add(_ConVert.Z);
            }
        }

        _VBON = GL.GenBuffer();
        _VBO = GL.GenBuffer();
        _VBOTC = GL.GenBuffer();
        _VAO = GL.GenVertexArray();

        GL.BindVertexArray(_VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, _Vert.Count * sizeof(float), _Vert.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        if (_Normals.Count == 0)
        {
            Console.WriteLine("Нет нормалей!");
        }


        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBON);
        GL.BufferData(BufferTarget.ArrayBuffer, _Normals.Count * sizeof(float), _Normals.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(1);




        GL.BindBuffer(BufferTarget.ArrayBuffer, _VBOTC);
        GL.BufferData(BufferTarget.ArrayBuffer, _Textures.Count * sizeof(float), _Textures.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(2);


        GL.BindVertexArray(0);
    }

    //Конвертация векторов в вектор Assimp
    private Assimp.Vector3D ConvertVecAssimp(Vector3 _Vec)
    {
        return new Assimp.Vector3D(_Vec.X, _Vec.Y, _Vec.Z);
    }

    //Конвертация в вектор OpenTK
    private OpenTK.Mathematics.Vector3 ConvertVecOpenTK(Assimp.Vector3D _Vert)
    {

        return new OpenTK.Mathematics.Vector3(_Vert.X, _Vert.Y, _Vert.Z);
    }


    //Загрузка текстур
    private int LoadTexture(string _Path)
    {
        int _TextHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _TextHandle);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Linear);

        int _Width = 1024;
        int _Height = 1024;
        byte[] _Pixels = new byte[_Width * _Height];

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _Width, _Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, _Pixels);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        return _TextHandle;
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
        GL.UseProgram(_Hand);
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
