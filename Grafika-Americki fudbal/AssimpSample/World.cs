// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using SharpGL.Enumerations;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Threading;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {

        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 5.0f;      

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 500.0f;    

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;


        private uint[] m_textures = null; 
        private enum TextureObjects { PLASTIKA = 0, TRAVA };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private string[] m_textureFiles = { 
            
            ".//textures//plastics.jpg",
            ".//textures//0127-football-green-grass-texture-seamless-hr.jpg"
        };

        private DispatcherTimer jump;
        private bool up = true;
        private float jumpH = 0f;
        private float jumpR = 0f;

        private DispatcherTimer goal;
        private bool animationOngoing = false;
        private float ballDistance = 0f;

        private float size = 1.0f;
        private float speed = 0.5f;
        private float distance = 190.0f;


        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { if (value >= 5 && value <= 85) m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { if (value > 100) m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float Speed {
            get { return speed; }
            set { speed = value; }
        }

        public float Distance {
            get { return distance; }
            set { distance = value; }
        }

        public float Size {
            get { return size; }
            set { size = value; }
        }


        public bool Animation {
            get { return animationOngoing; }
            set { animationOngoing = value; }
        }




        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Color(1f, 0f, 0f);
            
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            m_scene.LoadScene();
            m_scene.Initialize();

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            InitializeTextures(gl);

            InitLights(gl);

            AutomaticAnimation();
        }

        private void InitializeTextures(OpenGL gl) {
            m_textures = new uint[m_textureCount];
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i) {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                Bitmap image = new Bitmap(m_textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA8, imageData.Width, imageData.Height, 0,
                            OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                image.UnlockBits(imageData);
                image.Dispose();
            }

        }

        private void InitLights(OpenGL gl) {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);

            float[] white = new float[] { 1f, 1f, 1f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, new float[] { 0.7f, 0.7f, 0.7f, 0.7f });
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, white);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, white);

            float[] pos = { 1300, 100, 0, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);

            
            float[] pink = new float[] { 1f, 0.4f, 0.7f, 1.0f };
            
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 30F);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, pink);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, pink);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, pink);
        }

        public void AutomaticAnimation() {
            jump = new DispatcherTimer();
            jump.Tick += new EventHandler(AnimateBall);
            jump.Interval = TimeSpan.FromMilliseconds(30);
            jump.Start();
        }

        private void AnimateBall(object sender, EventArgs e) {
            jumpR -= 10 * speed;
            if (up)
                jumpH += speed * 10;
            else
                jumpH -= speed * 10;
            if (jumpH <= 0 || jumpH >= 150) 
                up = !up;
               
        }



        public void GoalAnimation() {
            //SLAJDERE ISKLJUCI
            MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;
            mw.size.IsEnabled = mw.speed.IsEnabled = mw.dist.IsEnabled = false;
            jump.Stop();
            animationOngoing = true;
            goal = new DispatcherTimer();
            goal.Tick += new EventHandler(GoalAnim);
            goal.Interval = TimeSpan.FromMilliseconds(30);
            goal.Start();
            jumpH = 160;
        }

        private void GoalAnim(object sender, EventArgs e) {
            jumpR -= 10 * speed;
            if (ballDistance < 310+distance) {
                ballDistance += speed * 15;
            }
            else {
                goal.Stop();
                ballDistance = 0;
                jumpH = 0;
                up = true;
                animationOngoing = false;
                jump.Start();
                MainWindow mw = (MainWindow)System.Windows.Application.Current.MainWindow;
                mw.size.IsEnabled = mw.speed.IsEnabled = mw.dist.IsEnabled = true;
            }

        }


        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Viewport(0, 0, m_width, m_height);
            gl.PushMatrix();
            gl.LookAt(0,0, m_sceneDistance, 0, 0, 0, 0, 1, 0);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0.0f, -1.0f, 0.0f });
            float[] pos = { 0f, 300F, 105f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pos);


            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.TRAVA]); 
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PushMatrix();
            gl.Scale(3, 3, 3);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0, 1f, 0);
            gl.Color(0.1f, 0.1f, 0.1f);
            gl.TexCoord(0f, 0f);
            gl.Vertex(300f, 0.0f, -300f);
            gl.TexCoord(0f, 1f);

            gl.Vertex(-300f, 0.0f, -300f);
            gl.TexCoord(1f, 1f);

            gl.Vertex(-300f, 0.0f, 300f);
            gl.TexCoord(1f, 0f);
            gl.Vertex(300f, 0.0f, 300f);
            gl.End();
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-100.0f, 0.0f, -100.0f-distance);
            gl.Scale(3f, 250f, 3f);
            gl.Rotate(-90f, 1, 0, 0);
            gl.Color(1.0f, 1.0f, 1.0f);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.PLASTIKA]); 
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            Cylinder target = new Cylinder();
            target.TextureCoords = true;
            target.TopRadius = 1;
            target.CreateInContext(gl);
            target.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(100.0f, 0.0f, -100.0f-distance);
            gl.Scale(3f, 250f, 3f);
            gl.Rotate(-90f, 1, 0, 0);
            Cylinder target1 = new Cylinder();
            //target1.NormalGeneration = Normals.Smooth;
            target1.TextureCoords = true;
            target1.TopRadius = 1;
            target1.CreateInContext(gl);
            target1.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(0f, 100.0f, -100f-distance);
            gl.Scale(100f, 3f, 3f);
            gl.Rotate(90f, 1, 0, 0);
            Cylinder target2 = new Cylinder();
            target2.TextureCoords = true;
            target2.TopRadius = 1;
            target2.CreateInContext(gl);
            target2.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.PushMatrix();
          gl.Translate(0, jumpH + 10* size, 100- ballDistance);

            gl.Rotate(jumpR, 0, 0);
            gl.Translate(0, -10* size, 0);
            gl.Scale(size, size, size);
            gl.Color(0.3f, 0.3f, 0.3f);
            gl.Rotate(-90, 0, 0);
            m_scene.Draw();
            gl.PopMatrix();

            gl.PopMatrix();
          



            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.PushMatrix();
            gl.Viewport(m_width / 2, 0, m_width / 2, m_height / 2);
            gl.LoadIdentity();

            gl.Ortho2D(-20.0f, 20.0f, -20.0f, 20.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            gl.Color(0.5f, 0.1f, 0.1f);
            gl.PushMatrix();
            gl.Translate(5f, -15f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "Predmet:Racunarska grafika");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -15f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "_______________________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -16f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "Sk.god:2020/21");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -16f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "_____________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -17f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "Ime:Nastasja");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -17f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "___________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -18f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "Prezime:Damjanac");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -18f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "_______________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -19f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "Sifra zad:7.2");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(5f, -19f, 0f);
            gl.DrawText3D("Tahoma", 10, 1.0f, 0, "__________");
            gl.PopMatrix();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Flush();
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 0.5f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
