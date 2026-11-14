using System;
using System.Collections.Generic;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Input;
using Stride.Rendering;
using Stride.Rendering.Lights;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;

namespace HorrorClown
{
    // ضع هذا السكربت على Entity فارغ في المشهد (Scene) الفارغ
    public class HorrorGame : SyncScript
    {
        const float WalkSpeed = 3.0f, RunSpeed = 5.0f, ClownSpeed = 3.2f, ClownChaseSpeed = 4.2f;
        const float PlayerR = 0.35f, ClownR = 0.5f, Sens = 0.0025f;

        readonly List<BoundingBox> solids = new List<BoundingBox>();
        readonly List<Entity> keys = new List<Entity>();
        readonly List<bool> keyTaken = new List<bool>();
        readonly Random rnd = new Random();

        Entity cam, clown, spot, exitBox;
        Vector3 playerPos;
        float yaw, pitch, flicker;
        float battery = 100f; bool lightOn = true;
        int collected;
        bool dead, won;
        Vector3 wanderTarget;

        readonly Vector3[] keyPos = {
            new Vector3(-8,0.8f,8), new Vector3(8,0.8f,7), new Vector3(-7,0.8f,-2),
            new Vector3(7,0.8f,-3), new Vector3(-6,0.8f,-8) };
        readonly Vector3 clownStart = new Vector3(0, 0, -7);
        readonly Vector3 playerStart = new Vector3(0, 0, 8.5f);

        public override void Start()
        {
            Input.LockMousePosition(true);
            Game.IsMouseVisible = false;

            BuildHouse();
            BuildClown();
            BuildPlayer();
            BuildKeysAndExit();
            AddLight(new Vector3(0, 2.8f, 0), new Color(255, 80, 60), 0.6f, 8);
            Ambient();
            Reset();
        }

        // ---------- بناء العالم ----------
        Material Mat(Color c) => Material.New(GraphicsDevice, new MaterialDescriptor
        {
            Attributes =
            {
                Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(c)),
                DiffuseModel = new MaterialDiffuseLambertModelFeature()
            }
        });

        Entity Make(GeometricPrimitive prim, Vector3 ext, Color c, Vector3 pos)
        {
            var mesh = new Mesh
            {
                Draw = prim.ToMeshDraw(),
                BoundingBox = new BoundingBox(-ext, ext),
                BoundingSphere = new BoundingSphere(Vector3.Zero, ext.Length())
            };
            var model = new Model();
            model.Meshes.Add(mesh);
            model.Materials.Add(new MaterialInstance(Mat(c)));
            var e = new Entity();
            e.Add(new ModelComponent(model));
            e.Transform.Position = pos;
            return e;
        }

        Entity Box(Vector3 center, Vector3 size, Color c, bool solid)
        {
            var e = Make(GeometricPrimitive.Cube.New(GraphicsDevice, size), size / 2, c, center);
            Entity.Scene.Entities.Add(e);
            if (solid) solids.Add(new BoundingBox(center - size / 2, center + size / 2));
            return e;
        }

        void Wall(float x1, float z1, float x2, float z2)
        {
            float t = 0.3f, h = 3f;
            var cx = (x1 + x2) / 2; var cz = (z1 + z2) / 2;
            var sx = Math.Max(Math.Abs(x2 - x1), t); var sz = Math.Max(Math.Abs(z2 - z1), t);
            Box(new Vector3(cx, h / 2, cz), new Vector3(sx, h, sz), new Color(70, 60, 55), true);
        }

        void BuildHouse()
        {
            Box(new Vector3(0, -0.1f, 0), new Vector3(20, 0.2f, 20), new Color(40, 30, 25), false); // أرضية
            Box(new Vector3(0, 3.1f, 0), new Vector3(20, 0.2f, 20), new Color(25, 25, 25), false);  // سقف
            Wall(-10, -10, 10, -10); Wall(-10, 10, 10, 10);
            Wall(-10, -10, -10, 10); Wall(10, -10, 10, 10);
            // جدران داخلية مع أبواب
            Wall(-10, 0, -2, 0); Wall(2, 0, 10, 0);
            Wall(-10, -5, -3, -5); Wall(3, -5, 10, -5);
            Wall(-4, 0, -4, 4); Wall(4, 5, 4, 10);
            // أثاث بسيط
            Box(new Vector3(-8, 0.4f, 6), new Vector3(1.5f, 0.8f, 1), new Color(90, 50, 30), true);
            Box(new Vector3(7, 0.5f, -2), new Vector3(1, 1, 2), new Color(60, 40, 30), true);
            Box(new Vector3(-6, 0.5f, -7), new Vector3(2, 1, 1), new Color(50, 30, 30), true);
        }

        void Part(Entity parent, GeometricPrimitive p, float ext, Color c, Vector3 pos, Vector3 scale)
        {
            var e = Make(p, new Vector3(ext), c, pos);
            e.Transform.Scale = scale;
            parent.AddChild(e);
        }

        void BuildClown()
        {
            clown = new Entity("Clown");
            if (Content.Exists("Monster"))   // الوحش المستورد (Monster.obj) بدل المهرج
            {
                var mm = new Entity("MonsterModel");
                mm.Add(new ModelComponent(Content.Load<Model>("Monster")));
                clown.AddChild(mm);
                clown.Add(new LightComponent { Type = new LightPoint { Radius = 4, Color = new ColorRgbProvider(new Color3(1f, 0.1f, 0.05f)) }, Intensity = 0.4f });
                Entity.Scene.Entities.Add(clown);
                return;
            }
            var g = GraphicsDevice;
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.5f, 16), 0.5f, new Color(200, 20, 30), new Vector3(0, 1.0f, 0), new Vector3(1, 1.4f, 1));
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.3f, 16), 0.3f, new Color(240, 240, 235), new Vector3(0, 1.95f, 0), Vector3.One);
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.08f, 8), 0.08f, new Color(255, 0, 0), new Vector3(0, 1.93f, -0.3f), Vector3.One);
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.04f, 8), 0.04f, Color.Black, new Vector3(-0.12f, 2.02f, -0.27f), Vector3.One);
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.04f, 8), 0.04f, Color.Black, new Vector3(0.12f, 2.02f, -0.27f), Vector3.One);
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.16f, 8), 0.16f, new Color(30, 180, 60), new Vector3(-0.3f, 2.1f, 0), Vector3.One);
            Part(clown, GeometricPrimitive.Sphere.New(g, 0.16f, 8), 0.16f, new Color(30, 180, 60), new Vector3(0.3f, 2.1f, 0), Vector3.One);
            Part(clown, GeometricPrimitive.Cylinder.New(g, 0.35f, 0.3f, 12), 0.35f, new Color(110, 30, 150), new Vector3(0, 2.38f, 0), Vector3.One);
            Part(clown, GeometricPrimitive.Cylinder.New(g, 0.8f, 0.2f, 10), 0.5f, new Color(230, 200, 30), new Vector3(-0.2f, 0.4f, 0), Vector3.One);
            Part(clown, GeometricPrimitive.Cylinder.New(g, 0.8f, 0.2f, 10), 0.5f, new Color(230, 200, 30), new Vector3(0.2f, 0.4f, 0), Vector3.One);
            clown.Add(new LightComponent { Type = new LightPoint { Radius = 4, Color = new ColorRgbProvider(new Color3(1f, 0.1f, 0.1f)) }, Intensity = 0.4f });
            Entity.Scene.Entities.Add(clown);
        }

        void BuildPlayer()
        {
            cam = new Entity("Camera");
            cam.Add(new CameraComponent { NearClipPlane = 0.05f, FarClipPlane = 80f });
            spot = new Entity("Flashlight");
            spot.Add(new LightComponent
            {
                Type = new LightSpot { Range = 14, AngleInner = 15, AngleOuter = 32, Color = new ColorRgbProvider(new Color3(1f, 0.95f, 0.8f)) },
                Intensity = 6f
            });
            cam.AddChild(spot);
            Entity.Scene.Entities.Add(cam);
            SceneSystem.GraphicsCompositor.Cameras[0].Camera = cam.Get<CameraComponent>();
        }

        void BuildKeysAndExit()
        {
            foreach (var p in keyPos)
            {
                var k = Make(GeometricPrimitive.Cube.New(GraphicsDevice, new Vector3(0.25f)), new Vector3(0.2f), new Color(255, 220, 0), p);
                k.Add(new LightComponent { Type = new LightPoint { Radius = 3, Color = new ColorRgbProvider(new Color3(1f, 0.85f, 0f)) }, Intensity = 0.8f });
                Entity.Scene.Entities.Add(k);
                keys.Add(k); keyTaken.Add(false);
            }
            exitBox = Box(new Vector3(0, 1.2f, -9.6f), new Vector3(1.6f, 2.4f, 0.2f), new Color(20, 200, 60), false);
            AddLight(new Vector3(0, 1.5f, -8.8f), new Color(30, 255, 80), 1.0f, 5);
        }

        void AddLight(Vector3 pos, Color c, float intensity, float radius)
        {
            var e = new Entity();
            e.Add(new LightComponent { Type = new LightPoint { Radius = radius, Color = new ColorRgbProvider(c.ToColor3()) }, Intensity = intensity });
            e.Transform.Position = pos;
            Entity.Scene.Entities.Add(e);
        }

        void Ambient()
        {
            var e = new Entity();
            e.Add(new LightComponent { Type = new LightAmbient { Color = new ColorRgbProvider(new Color3(0.2f, 0.2f, 0.3f)) }, Intensity = 0.12f });
            Entity.Scene.Entities.Add(e);
        }

        // ---------- منطق اللعبة ----------
        void Reset()
        {
            playerPos = playerStart; battery = 100f; lightOn = true; yaw = 0; pitch = 0; collected = 0; dead = false; won = false;
            clown.Transform.Position = clownStart;
            wanderTarget = clownStart;
            for (int i = 0; i < keys.Count; i++) { keyTaken[i] = false; keys[i].Transform.Position = keyPos[i]; }
        }

        bool Hit(Vector3 p, float r)
        {
            foreach (var b in solids)
                if (p.X + r > b.Minimum.X && p.X - r < b.Maximum.X && p.Z + r > b.Minimum.Z && p.Z - r < b.Maximum.Z)
                    return true;
            return false;
        }

        Vector3 Move(Vector3 pos, Vector3 d, float r)
        {
            var p = pos; p.X += d.X; if (Hit(p, r)) p.X = pos.X;
            var q = p; q.Z += d.Z; if (Hit(q, r)) q.Z = p.Z;
            return q;
        }

        public override void Update()
        {
            float dt = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            if (dead || won)
            {
                DebugText.Print(dead ? "المهرج امسكك! اضغط R لإعادة اللعب" : "هربت! اضغط R للعب مرة ثانية", new Int2(20, 20));
                if (Input.IsKeyPressed(Keys.R)) Reset();
                return;
            }

            // الكاميرا
            yaw -= Input.Mouse.Delta.X * Sens * 100f / 100f * 100f / 100f;
            pitch = MathUtil.Clamp(pitch - Input.Mouse.Delta.Y * Sens, -1.3f, 1.3f);
            var fwd = new Vector3(-(float)Math.Sin(yaw), 0, -(float)Math.Cos(yaw));
            var right = new Vector3((float)Math.Cos(yaw), 0, -(float)Math.Sin(yaw));

            var dir = Vector3.Zero;
            if (Input.IsKeyDown(Keys.W)) dir += fwd;
            if (Input.IsKeyDown(Keys.S)) dir -= fwd;
            if (Input.IsKeyDown(Keys.D)) dir += right;
            if (Input.IsKeyDown(Keys.A)) dir -= right;
            if (dir.LengthSquared() > 0) dir.Normalize();
            float speed = Input.IsKeyDown(Keys.LeftShift) ? RunSpeed : WalkSpeed;
            playerPos = Move(playerPos, dir * speed * dt, PlayerR);

            cam.Transform.Position = playerPos + new Vector3(0, 1.6f, 0);
            cam.Transform.Rotation = Quaternion.RotationYawPitchRoll(yaw, pitch, 0);

            // مفاتيح
            for (int i = 0; i < keys.Count; i++)
            {
                if (keyTaken[i]) continue;
                keys[i].Transform.Rotation = Quaternion.RotationY((float)Game.UpdateTime.Total.TotalSeconds * 2f);
                if (Vector3.Distance(new Vector3(playerPos.X, 0.8f, playerPos.Z), keyPos[i]) < 1.0f)
                {
                    keyTaken[i] = true; collected++;
                    keys[i].Transform.Position = new Vector3(0, -100, 0);
                }
            }

            // المهرج
            var cp = clown.Transform.Position;
            var toP = playerPos - cp; toP.Y = 0;
            float dist = toP.Length();
            Vector3 target; float cs;
            float hear = Input.IsKeyDown(Keys.LeftShift) && dir.LengthSquared() > 0 ? 14f : 8f;   // الجري يصدر ضجيج
            if (dist < hear) { target = playerPos; cs = ClownChaseSpeed; }
            else
            {
                if (Vector3.Distance(cp, wanderTarget) < 1f)
                    wanderTarget = new Vector3((float)(rnd.NextDouble() * 16 - 8), 0, (float)(rnd.NextDouble() * 16 - 8));
                target = wanderTarget; cs = ClownSpeed;
            }
            var cd = target - cp; cd.Y = 0;
            if (cd.LengthSquared() > 0.01f)
            {
                cd.Normalize();
                cp = Move(cp, cd * cs * dt, ClownR);
                clown.Transform.Position = cp;
                clown.Transform.Rotation = Quaternion.RotationY((float)Math.Atan2(-cd.X, -cd.Z));
            }
            if (dist < 0.9f) dead = true;

            // وميض الكشاف كلما اقترب المهرج
            flicker += dt;
            var light = spot.Get<LightComponent>();
            if (Input.IsKeyPressed(Keys.F)) lightOn = !lightOn;
            if (lightOn) battery = Math.Max(0f, battery - 0.9f * dt);
            if (battery <= 0f) lightOn = false;
            light.Intensity = !lightOn ? 0f : (dist < 6f && (int)(flicker * 12) % 3 == 0 ? 0.5f : 6f);

            // الفوز
            if (collected == keys.Count && Math.Abs(playerPos.X) < 1f && playerPos.Z < -8.6f) won = true;

            DebugText.Print($"المفاتيح: {collected}/{keys.Count}  |  البطارية: {(int)battery}%  |  F كشاف - Shift جري (يجذب الوحش)", new Int2(20, 20));
            if (collected == keys.Count) DebugText.Print("اذهب للباب الأخضر!", new Int2(20, 45));
        }
    }
}
