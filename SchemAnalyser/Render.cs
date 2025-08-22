using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SchemAnalyser;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Render;

public class GObject
{
    public Vector3 position;
    public Model model;
    public Quaternion rotation;
    public Vector3 color;

    public GObject(Vector3 position, Model model, Vector3 color)
    {
        this.position = position;
        this.model = model;
        this.rotation = Quaternion.Identity;
        this.color = color;
    }
}

public class Model
{
    public uint[] indices;
    public float[] vertices;
    public int _vao, _vbo, _ebo;

    public Model(uint[] indices, float[] vertices)
    {

        this.indices = indices;
        this.vertices = vertices;
        
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * sizeof(float), this.vertices, BufferUsage.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, this.indices.Length * sizeof(uint), this.indices, BufferUsage.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }
}

class JsonModelElement
{
    public float[] from;
    public float[] to;
}

class JsonModel
{
    public List<JsonModelElement> elements;
}

public class ModelLoader
{
    private IResourceMannager resourceMannager;
    public ModelLoader(IResourceMannager resourceMannager)
    {
        this.resourceMannager = resourceMannager;
    }

    public Model Load(string text)
    {
        var obj = JsonSerializer.CreateDefault()
            .Deserialize<JsonModel>(new JsonTextReader(new StringReader(text)));
        

        var vertices = new List<float>();
        var indices = new List<uint>();
        uint vertexOffset = 0;

        foreach (var element in obj.elements)
        {
            var f = element.from;
            var t = element.to;
            
            vertices.AddRange(new float[]
            {
                f[0], f[1], f[2], // 0
                t[0], f[1], f[2], // 1
                t[0], t[1], f[2], // 2
                f[0], t[1], f[2], // 3
                f[0], f[1], t[2], // 4
                t[0], f[1], t[2], // 5
                t[0], t[1], t[2], // 6
                f[0], t[1], t[2], // 7
            }.Select(i => i / 16));
            
            indices.AddRange(new uint[]
            {
                0,1,2,2,3,0,  4,5,6,6,7,4,
                0,1,5,5,4,0,  2,3,7,7,6,2,
                0,3,7,7,4,0,  1,2,6,6,5,1
            }.Select(i => i + vertexOffset));

            vertexOffset += 8;
        }

        return new Model(indices.ToArray(), vertices.ToArray());
    }

}

public class Game : GameWindow
{
    private Shader _shader;
    public Game()
        : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            
            Size = new Vector2i(800, 600), Title = "Schematic Render", Flags = ContextFlags.Default, Vsync = VSyncMode.On
        })
    {
        Resize += OnResize;
    }
    private static void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
    }
    

    private float _angle = 0f;

    private Vector3 _cameraPos = new Vector3(0, 0, 0);
    private Vector3 _cameraFront = -Vector3.UnitZ;
    private Vector3 _cameraUp = Vector3.UnitY;

    private float _yaw = -90f;
    private float _pitch = 0f;
    private float _speed = 12.5f;
    private float _sensitivity = 0.1f;
    private Vector2 _lastMousePos;
    private bool _firstMouse = true;

    public List<GObject> objects = [];
    
    private void UpdateCameraVectors()
    {
        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        _cameraFront = Vector3.Normalize(front);
    }
    
    public static string LoadResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
    
    double time = 0;
    int frames = 0;


    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DepthTest);
        
        _shader = new Shader(LoadResource("SchemAnalyser.Shaders.vertex.glsl"), LoadResource("SchemAnalyser.Shaders.fragment.glsl"));
        
        CursorState = CursorState.Grabbed;
        base.OnLoad();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        if (KeyboardState.IsKeyPressed(Keys.Escape))
        {
            CursorState = CursorState.Normal;
            _firstMouse = true;
        }
        if (MouseState.IsButtonPressed(MouseButton.Left))
        {
            CursorState = CursorState.Grabbed;
            _firstMouse = true;
        }

        if (CursorState != CursorState.Grabbed)
            return;

        var mouse = MousePosition;
        if (_firstMouse)
        {
            _lastMousePos = mouse;
            _firstMouse = false;
        }

        float xOffset = mouse.X - _lastMousePos.X;
        float yOffset = _lastMousePos.Y - mouse.Y;
        _lastMousePos = mouse;

        xOffset *= _sensitivity;
        yOffset *= _sensitivity;

        _yaw += xOffset;
        _pitch += yOffset;
        _pitch = Math.Clamp(_pitch, -89f, 89f);

        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        _cameraFront = Vector3.Normalize(front);

        var velocity = _speed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.W)) _cameraPos += _cameraFront * velocity;
        if (KeyboardState.IsKeyDown(Keys.S)) _cameraPos -= _cameraFront * velocity;
        if (KeyboardState.IsKeyDown(Keys.A)) _cameraPos -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * velocity;
        if (KeyboardState.IsKeyDown(Keys.D)) _cameraPos += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * velocity;
        
        if (KeyboardState.IsKeyDown(Keys.Space)) _cameraPos += new Vector3(0, 1, 0) * velocity / 2;
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) _cameraPos += new Vector3(0, -1, 0) * velocity / 2;
        
        UpdateCameraVectors();
        base.OnUpdateFrame(args);
    }

    private double _time_line = 0;
    
    

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        
        frames++;
        time += args.Time;

        if (time >= 1.0)
        {
            Title = $"OpenTK FPS: {frames}";
            frames = 0;
            time = 0;
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _shader.Use();
        
        var view = Matrix4.LookAt(_cameraPos, _cameraPos + _cameraFront, _cameraUp);
        var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
        
        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", proj);
        _shader.SetVector3("cam", _cameraPos);
        
        _angle += 50f * (float)args.Time;
        _time_line += args.Time;
        
        var loc = GL.GetUniformLocation(_shader.Handle, "model");
        var world_position = GL.GetUniformLocation(_shader.Handle, "world_position");
        var color = GL.GetUniformLocation(_shader.Handle, "color");
        var time_line = GL.GetUniformLocation(_shader.Handle, "time_line");
        
        GL.Uniform1d(time_line, _time_line);

        Model model = null;
        Matrix4 mat = Matrix4.Zero;
        
        foreach (var obj in objects)
        {
            var objectMatrix = Matrix4.CreateTranslation(obj.position);
            GL.UniformMatrix4f(loc, 1, false, ref objectMatrix);
            GL.Uniform3f(world_position, 1, ref obj.position);
            GL.Uniform3f(color, 1, ref obj.color);
            
            if (obj.model != model)
            {
                GL.BindVertexArray(obj.model._vao);
                model =  obj.model;
            }
            //GL.DepthMask(true);
            //GL.LineWidth(2f);
            GL.DrawElements(PrimitiveType.LineStrip, obj.model.indices.Length*2, DrawElementsType.UnsignedInt, 0);
        }

        SwapBuffers();
        base.OnRenderFrame(args);
    }
}

class Shader
{
    public int Handle;
    public Shader(string vertex, string fragment)
    {
        var v = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(v, vertex);
        GL.CompileShader(v);

        var f = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(f, fragment);
        GL.CompileShader(f);
        
        
        GL.GetShaderi(v, ShaderParameterName.CompileStatus, out var vStatus);
        if (vStatus == 0)
        {
            GL.GetShaderInfoLog(v, out var log);
            Console.WriteLine(log);
        }
        

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, v);
        GL.AttachShader(Handle, f);
        GL.LinkProgram(Handle);
        
        GL.DetachShader(Handle, v);
        GL.DetachShader(Handle, f);
        GL.DeleteShader(v);
        GL.DeleteShader(f);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetMatrix4(string name, Matrix4 mat)
    {
        GL.UniformMatrix4f(GL.GetUniformLocation(Handle, name), 1, false, ref mat);
    }
    
    public void SetVector3(string name, Vector3 vec)
    {
        GL.Uniform3f(GL.GetUniformLocation(Handle, name), 1, ref vec);
    }
    
    public void SetQuaternion(string name, Quaternion quaternion)
    {
        GL.Uniform4f(GL.GetUniformLocation(Handle, name), 
            quaternion.X, 
            quaternion.Y, 
            quaternion.Z, 
            quaternion.W
            );
    }
}
