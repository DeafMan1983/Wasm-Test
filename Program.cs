using System;
using System.Runtime.InteropServices;

using Microsoft.JSInterop.WebAssembly;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WaveEngine.Bindings.OpenGLES;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WasmTest;

internal unsafe class MyRuntime : WebAssemblyJSRuntime
{

	public MyRuntime()
	{
		
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101:Specify marshaling for P/Invoke string arguments", Justification = "Incorrect warning generated")]
internal static class EGL
{
	public const string LibEgl = "libEGL";
	public const int EGL_NONE = 0x3038;
	public const int EGL_RED_SIZE = 0x3024;
	public const int EGL_GREEN_SIZE = 0x3023;
	public const int EGL_BLUE_SIZE = 0x3022;
	public const int EGL_DEPTH_SIZE = 0x3025;
	public const int EGL_STENCIL_SIZE = 0x3026;
	public const int EGL_SURFACE_TYPE = 0x3033;
	public const int EGL_RENDERABLE_TYPE = 0x3040;
	public const int EGL_SAMPLES = 0x3031;
	public const int EGL_WINDOW_BIT = 0x0004;
	public const int EGL_OPENGL_ES2_BIT = 0x0004;
	public const int EGL_OPENGL_ES3_BIT = 0x00000040;
	public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
	public const int EGL_NO_CONTEXT = 0x0;
	public const int EGL_NATIVE_VISUAL_ID = 0x302E;
	public const int EGL_OPENGL_ES_API = 0x30A0;

	[DllImport(LibEgl, EntryPoint = "eglGetProcAddress", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern IntPtr GetProcAddress(string proc);

	[DllImport(LibEgl, EntryPoint = "eglGetDisplay", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern IntPtr GetDisplay(IntPtr displayId);

	[DllImport(LibEgl, EntryPoint = "eglInitialize", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool Initialize(IntPtr display, out int major, out int minor);


	[DllImport(LibEgl, EntryPoint = "eglChooseConfig", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ChooseConfig(IntPtr dpy, int[] attribList, ref IntPtr configs, IntPtr configSize/*fixed to 1*/, ref IntPtr numConfig);

	[DllImport(LibEgl, EntryPoint = "eglBindAPI", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool BindApi(int api);

	[DllImport(LibEgl, EntryPoint = "eglCreateContext", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern IntPtr CreateContext(IntPtr/*EGLDisplay*/ display, IntPtr/*EGLConfig*/ config, IntPtr shareContext, int[] attribList);

	[DllImport(LibEgl, EntryPoint = "eglGetConfigAttrib", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetConfigAttrib(IntPtr/*EGLDisplay*/ display, IntPtr/*EGLConfig*/ config, IntPtr attribute, ref IntPtr value);

	[DllImport(LibEgl, EntryPoint = "eglCreateWindowSurface", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern IntPtr CreateWindowSurface(IntPtr display, IntPtr config, IntPtr win, IntPtr attribList/*fixed to NULL*/);

	[DllImport(LibEgl, EntryPoint = "eglDestroySurface", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern int DestroySurface(IntPtr display, IntPtr surface);

	[DllImport(LibEgl, EntryPoint = "eglDestroyContext", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern int DestroyContext(IntPtr display, IntPtr ctx);

	[DllImport(LibEgl, EntryPoint = "eglMakeCurrent", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool MakeCurrent(IntPtr display, IntPtr draw, IntPtr read, IntPtr ctx);

	[DllImport(LibEgl, EntryPoint = "eglTerminate", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern int Terminate(IntPtr display);

	[DllImport(LibEgl, EntryPoint = "eglSwapBuffers", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern int SwapBuffers(IntPtr display, IntPtr surface);

	[DllImport(LibEgl, EntryPoint = "eglSwapInterval", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
	public static extern int SwapInterval(IntPtr display, int interval);
}

public unsafe static class Test
{
	private static IntPtr display, surface;

	const string vertexShaderSource = @"#version 300 es
		precision highp float;
		layout (location = 0) in vec4 Position0;
		layout (location = 1) in vec4 Color0;
		out vec4 color;
		void main()
		{    
			gl_Position = Position0;	
			color = Color0;
		}";

	const string fragmentShaderSource = @"#version 300 es
		precision highp float;
		in vec4 color;
		out vec4 fragColor;
		void main() 
		{    	 	 	
			fragColor = color;	 	
		}";

	[JSInvokable("frame")]
	public static void Frame(double time)
	{
		GL.glClearColor(1.0f, 1.0f * 0.35f, 0f, 1.0f);
		GL.glClear((uint)AttribMask.ColorBufferBit);
	}

	public static int Main(string[] args)
	{
		Console.WriteLine($"Hello from .Net6!");

		display = EGL.GetDisplay(IntPtr.Zero);
		if (display == IntPtr.Zero)
			throw new Exception("Display was null");

		if (!EGL.Initialize(display, out int major, out int minor))
			throw new Exception("Initialize() returned false.");

		int[] attributeList = new int[]
		{
				EGL.EGL_RED_SIZE  , 8,
				EGL.EGL_GREEN_SIZE, 8,
				EGL.EGL_BLUE_SIZE , 8,
				EGL.EGL_DEPTH_SIZE, 24,
				EGL.EGL_STENCIL_SIZE, 8,
				EGL.EGL_SURFACE_TYPE, EGL.EGL_WINDOW_BIT,
				EGL.EGL_RENDERABLE_TYPE, EGL.EGL_OPENGL_ES3_BIT,
				EGL.EGL_SAMPLES, 16, //MSAA, 16 samples
				EGL.EGL_NONE
		};

		var config = IntPtr.Zero;
		var numConfig = IntPtr.Zero;
		if (!EGL.ChooseConfig(display, attributeList, ref config, (IntPtr)1, ref numConfig))
			throw new Exception("ChoseConfig() failed");
		if (numConfig == IntPtr.Zero)
			throw new Exception("ChoseConfig() returned no configs");

		if (!EGL.BindApi(EGL.EGL_OPENGL_ES_API))
			throw new Exception("BindApi() failed");

		int[] ctxAttribs = new int[] { EGL.EGL_CONTEXT_CLIENT_VERSION, 3, EGL.EGL_NONE };
		var context = EGL.CreateContext(display, config, (IntPtr)EGL.EGL_NO_CONTEXT, ctxAttribs);
		if (context == IntPtr.Zero)
			throw new Exception("CreateContext() failed");

		// now create the surface
		surface = EGL.CreateWindowSurface(display, config, IntPtr.Zero, IntPtr.Zero);
		if (surface == IntPtr.Zero)
			throw new Exception("CreateWindowSurface() failed");

		if (!EGL.MakeCurrent(display, surface, surface, context))
			throw new Exception("MakeCurrent() failed");

		GL.LoadAllFunctions(EGL.GetProcAddress);

		//	int width = EGL.????
		//	int height = EGL.????
		//	GL.glViewport(0, 0, width, height);

		EGL.SwapBuffers(display, surface);

		// https://github.com/emepetres/dotnet-wasm-sample/blob/main/src/jsinteraction/wasm/WebAssemblyRuntime.cs
		using var runtime = new MyRuntime();
		runtime.InvokeVoid("initialize");

		uint vertexShader = GL.glCreateShader(ShaderType.VertexShader);

		IntPtr* textPtr = stackalloc IntPtr[1];
		var lengthArray = stackalloc int[1];

		lengthArray[0] = vertexShaderSource.Length;
		textPtr[0] = Marshal.StringToHGlobalAnsi(vertexShaderSource);

		GL.glShaderSource(vertexShader, 1, (IntPtr)textPtr, lengthArray);
		GL.glCompileShader(vertexShader);
		// checkErrors
		int success = 0;
		var infoLog = stackalloc char[512];
		lengthArray[0] = success;
		GL.glGetShaderiv(vertexShader, ShaderParameterName.CompileStatus, lengthArray);
		if (success > 0)
		{
			GL.glGetShaderInfoLog(vertexShader, 512, (int*)0, infoLog);
			Console.WriteLine($"Error: shader vertex compilation failed: {new string(infoLog)}");
		}

		uint fragmentShader = GL.glCreateShader(ShaderType.FragmentShader);

		lengthArray[0] = fragmentShaderSource.Length;
		textPtr[0] = Marshal.StringToHGlobalAnsi(fragmentShaderSource);
		GL.glShaderSource(fragmentShader, 1, (IntPtr)textPtr, lengthArray);
		GL.glCompileShader(fragmentShader);
		// checkErrors
		lengthArray[0] = success;
		GL.glGetShaderiv(fragmentShader, ShaderParameterName.CompileStatus, lengthArray);
		if (success > 0)
		{
			GL.glGetShaderInfoLog(fragmentShader, 512, (int*)0, infoLog);
			Console.WriteLine($"Error: shader fragment compilation failed: {new string(infoLog)}");
		}

		uint shaderProgram = GL.glCreateProgram();
		GL.glAttachShader(shaderProgram, vertexShader);
		GL.glAttachShader(shaderProgram, fragmentShader);
		GL.glLinkProgram(shaderProgram);
		// checkErrors
		lengthArray[0] = success;
		GL.glGetProgramiv(shaderProgram, ProgramPropertyARB.LinkStatus, lengthArray);
		if (success > 0)
		{
			GL.glGetProgramInfoLog(shaderProgram, 512, (int*)0, infoLog);
			Console.WriteLine($"Error: shader program compilation failed: {new string(infoLog)}");
		}

		GL.glDeleteShader(vertexShader);
		GL.glDeleteShader(fragmentShader);

		float[] vertices = {
			0f, 0.5f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f,
			0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f,
			-0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f
		};

		uint VBO = 0;
		uint VAO = 0;
		GL.glGenVertexArrays(1, &VAO);
		GL.glGenBuffers(1, &VBO);

		GL.glBindVertexArray(VAO);
		GL.glBindBuffer(BufferTargetARB.ArrayBuffer, VBO);

		fixed (float* verticesPtr = &vertices[0])
		{
			GL.glBufferData(BufferTargetARB.ArrayBuffer, vertices.Length * sizeof(float), verticesPtr, BufferUsageARB.StaticDraw);
		}

		int stride = 8 * sizeof(float);
		GL.glVertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, stride, (void*)null);
		GL.glEnableVertexAttribArray(0);

		GL.glVertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, (void*)16);
		GL.glEnableVertexAttribArray(1);

		GL.glBindBuffer(BufferTargetARB.ArrayBuffer, 0);

		GL.glBindVertexArray(0);

		GL.glUseProgram(shaderProgram);
		GL.glBindVertexArray(VAO);
		GL.glDrawArrays(PrimitiveType.Triangles, 0, 3);

		EGL.SwapBuffers(display, surface);

		return args.Length;
	}
}
