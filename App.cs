using System;
using System.Collections.Generic;
using StereoKit;
using System.Numerics;
using RDR;


namespace StereoKitApp
{
	public class App
	{
		public SKSettings Settings => new SKSettings { 
			appName           = "StereoKit Template",
			assetsFolder      = "Assets",
			displayPreference = DisplayMode.MixedReality
		};

		Pose objPose;
		SmartSphere obj;
		Matrix4x4 floorTransform = Matrix.TS(new Vector3(0, -1.5f, 0), new Vector3(20, 0.1f, 20));
		Material  floorMaterial;
		private Pose windowAdminPose;
		private Sprite powerSprite;
		private Material targetMaterial, seenMaterial, selectedMaterial, earthMaterial;
		private void initSharedResources()
		{
			
			this.powerSprite = Sprite.FromFile("power.png", SpriteType.Single);
			earthMaterial = Default.MaterialPBR.Copy();
			earthMaterial[MatParamName.DiffuseTex] = Tex.FromFile("earth-1.png");

			targetMaterial = Default.Material.Copy(); //matAlphaBlend
			//targetMaterial.Transparency = Transparency.Blend;
			targetMaterial.Transparency = Transparency.None;
			targetMaterial.DepthWrite = true;
			targetMaterial[MatParamName.ColorTint] = new Color(.3f, 1, 0.3f, 1f);
			targetMaterial.Wireframe = true;

			seenMaterial = Default.Material.Copy(); //matAlphaBlend
			// seenMaterial.Transparency = Transparency.Blend;
			seenMaterial.Transparency = Transparency.None;
			seenMaterial.DepthWrite = true;
			seenMaterial[MatParamName.ColorTint] = new Color(.6f, .1f, 0.6f, 1f);

			selectedMaterial = seenMaterial.Copy();

		}
		private void initUI()
		{
			// 
			// set UI scheme
			// Renderer.SkyTex = Tex.FromCubemapEquirectangular("night.hdr", out SphericalHarmonics lighting);
			// Renderer.SkyLight = lighting;
			Color uiColor = Color.HSV(.83f, 0.33f, 1f, 0.8f);
			UI.ColorScheme = uiColor;
			windowAdminPose = new Pose(-.2f, 0, -0.65f, Quat.LookAt(new Vec3(-.2f, 0, -0.5f), Input.Head.position, Vec3.Up));
		}

		private Boolean displayAdminPanel()
		{
			Boolean running = true;
			UI.WindowBegin("Admin", ref this.windowAdminPose, new Vec2(25, 0) * U.cm, UIWin.Normal);
			UI.Text("Admin Panel ");
			
				

			UI.NextLine();
			if (UI.ButtonRound("Exit", this.powerSprite)) running = false;
			UI.SameLine(); UI.Label("Exit Game");
			UI.WindowEnd();
			return running;
		}
		public void Init()
		{
			this.initSharedResources();
			this.initUI();
			Log.Subscribe(OnLog);
			// Create assets used by the app
			// Mesh.GenerateRoundedCube(Vec3.One * 0.1f, 0.02f),
			SmartSphere.Init();
			var pose = new Pose(0, 0, -1.0f, Quat.Identity);
			obj = new SmartSphere(pose, earthMaterial);		


			floorMaterial = new Material(Shader.FromFile("floor.hlsl"));
			floorMaterial.Transparency = Transparency.Blend;
		}

		public void Step()
		{
			if (SK.System.displayType == Display.Opaque)
				Default.MeshCube.Draw(floorMaterial, floorTransform);

			Boolean running = this.displayAdminPanel();
			LogWindow();

			
			obj.Draw();


			if (running == false )
            {
				SK.Quit();
			}
		}
		static Pose logPose = new Pose(0.8f, -0.1f, -0.5f, Quat.LookDir(Vec3.Forward));
		static List<string> logList = new List<string>();
		static string logText = "";
		static void OnLog(LogLevel level, string text)
		{
			if (logList.Count > 15)
				logList.RemoveAt(logList.Count - 1);
			logList.Insert(0, text.Length < 100 ? text : text.Substring(0, 100) + "...\n");

			logText = "";
			for (int i = 0; i < logList.Count; i++)
				logText += logList[i];
		}
		static void LogWindow()
		{
			UI.WindowBegin("Log", ref logPose, new Vec2(40, 0) * U.cm);
			UI.Text(logText);
			UI.WindowEnd();
		}
	}
}