﻿using System;
using Reign.Core;
using Reign.Video;
using Reign.Video.API;
using Reign.Input;
using Reign.Input.API;
using ShaderMaterials.Shaders;

namespace Demo
{
	#if WINDOWS
	class MainApp : Window
	#elif METRO
	class MainApp : Application
	#endif
	{
		bool loaded;
		RootDisposable root;
		VideoI video;
		QuickDrawI qd;
		ViewPortI viewPort;
		Camera camera;
		QuickDraw3ColorUVMaterial material;
		Texture2DI texture;

		RasterizerStateI rasterizerState;
		SamplerStateI samplerState;
		BlendStateI blendState;
		DepthStencilStateI depthStencilState;

		InputI input;
		MouseI mouse;
		KeyboardI keyboard;
		
		public MainApp()
		#if WINDOWS
		: base("QuickDraw Sample", 512, 512, WindowStartPositions.CenterCurrentScreen, WindowTypes.Frame)
		#elif METRO
		: base(ApplicationOrientations.Landscape)
		#endif
		{
			
		}

		protected override void shown()
		{
			try
			{
				root = new RootDisposable();
				VideoTypes videoType;
				#if METRO
				VideoTypes createVideoTypes = VideoTypes.D3D11;
				#else
				VideoTypes createVideoTypes = VideoTypes.D3D11 | VideoTypes.D3D9 | VideoTypes.OpenGL;
				#endif
				video = Video.Create(createVideoTypes, out videoType, root, this, true);

				QuickDraw3ColorUVMaterial.Init(videoType, video, "Data\\", video.FileTag, ShaderVersions.Max);
				material = new QuickDraw3ColorUVMaterial();
				texture = Texture2D.Create(videoType, video, "Data\\Roxy.dds");
				qd = QuickDraw.Create(videoType, video, QuickDraw3ColorUVMaterial.BufferLayoutDesc);

				var frame = FrameSize;
				viewPort = ViewPort.Create(videoType, video, 0, 0, frame.Width, frame.Height);
				camera = new Camera(viewPort, new Vector3(0, 0, 5), new Vector3(), new Vector3(0, 0+1, 5));

				rasterizerState = RasterizerState.Create(videoType, video, RasterizerStateDesc.Create(videoType, RasterizerStateTypes.Solid_CullNone));
				samplerState = SamplerState.Create(videoType, video, SamplerStateDesc.Create(videoType, SamplerStateTypes.Linear_Wrap));
				blendState = BlendState.Create(videoType, video, BlendStateDesc.Create(videoType, BlendStateTypes.Alpha));
				depthStencilState = DepthStencilState.Create(videoType, video, DepthStencilStateDesc.Create(videoType, DepthStencilStateTypes.None));
				rasterizerState.Enable();
				samplerState.Enable(0);
				blendState.Enable();
				depthStencilState.Enable();

				InputTypes inputType;
				#if METRO
				InputTypes createInputTypes = InputTypes.Metro;
				#else
				InputTypes createInputTypes = InputTypes.WinForms;
				#endif
				input = Input.Create(createInputTypes, out inputType, root, this);
				mouse = Mouse.Create(inputType, input);
				keyboard = Keyboard.Create(inputType, input);

				loaded = true;
			}
			catch (Exception e)
			{
				Message.Show("Error", e.Message);
				dispose();
			}
		}

		private void dispose()
		{
			if (root != null)
			{
				root.Dispose();
				root = null;
			}
		}

		protected override void closing()
		{
			dispose();
		}

		protected override void render()
		{
			if (!loaded) return;

			var e = Streams.TryLoad();
			if (e != null)
			{
				throw e;
			}
			if (Streams.ItemsRemainingToLoad != 0) return;

			input.Update();
			video.Update();
			video.EnableRenderTarget();
			video.Clear(0, .3f, .3f, 1);
			System.Diagnostics.Debug.WriteLine(mouse.Location);
			viewPort.Apply();
			if (!mouse.Left.On) camera.RotateAroundLookLocationWorld(0, .01f, 0);
			else camera.RotateAroundLookLocation(-mouse.Velocity.Y * .05f, -mouse.Velocity.X * .05f, 0);
			if (keyboard.ArrowUp.On) camera.Zoom(.05f, 1);
			if (keyboard.ArrowDown.On) camera.Zoom(-.05f, 1);
			camera.Apply();

			QuickDraw3ColorUVMaterial.Camera = camera.TransformMatrix;
			material.Diffuse = texture;
			material.Enable();
			material.Apply();
			qd.StartTriangles();
			    qd.UV(0, 0); qd.Pos(-1, -1, 0);
			    qd.UV(0, 1); qd.Pos(-1, 1, 0);
			    qd.UV(1, 1); qd.Pos(1, 1, 0);

			    qd.UV(0, 0); qd.Pos(-1, -1, 0);
			    qd.UV(1, 1); qd.Pos(1, 1, 0);
			    qd.UV(1, 0); qd.Pos(1, -1, 0);
			qd.End();

			video.Present();
		}
	}
}
