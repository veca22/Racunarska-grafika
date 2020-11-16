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
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using System.Windows;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        private int sirina_prozora;
        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 600;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private uint[] textures = null;
        
        private enum TextureObjects
        {
            Metal = 0,
            Wood,
            Concrete,
            Burrel
        };

        private readonly int textureCount = 3;

        private string[] textureFiles =
        {
            ".//Textures//metal.jpg",
            ".//Textures//wood.jpg",
            ".//Textures//concrete.jpg",
        };
        private float barrelX = 0, barrelY = 0, barrelRotation = 0;//RELATIVNI POMERAJ BURETA KOD ANIMACIJE, I RODTACIJA KAD SE KOTRLJA
        private float barrelY_Free = 0; //POMERAJ PO Y, DOK NE PADNE NA TLO (DOK JE BURE U SLOBODNOM PADU)
        private float barrelScale = 1f; //FAKTOR SKALIRANJA BURETA, PODESAVA SE SLAJDERIMA IZ MAINWINDOWA
        private DispatcherTimer timer; //Tajmer animacije
        private float holderRotation = 0; //Rotacija drzaca u animaciji
        private bool animation = false;
        private float lightPositionX = 0; // Pomeraj svetla po x osi
        private float r = 1f, g = 1f, b = 0f; //Boja reflektora, po zadatku je prvo zuta;
        private bool rotated = true;

        #endregion Atributi

        #region Properties
        public float R
        {
            get { return r; }
            set { r = value; }
        }

        public float B
        {
            get { return b; }
            set { b = value; }
        }


        public float G
        {
            get { return g; }
            set { g = value; }
        }

        public bool Animation
        {
            get { return animation; }
            set { animation = value; }
        }

        public float LightPositionX
        {
            get { return lightPositionX; }
            set { lightPositionX = value; }
        }

        public float BarrelScale
        {
            get { return barrelScale; }
            set { barrelScale = value; }
        }


        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        /// 
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
            set { m_xRotation = value; if (m_xRotation < 5) m_xRotation = 5; if (m_xRotation > 70) m_xRotation = 70; } // Zabrana gledanja scene odozdo
        }

        public int Sirina_prozora
        {
            get { return sirina_prozora; }
            set { sirina_prozora = value;}
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
            set { m_sceneDistance = value; }
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
            gl.Color(1f, 1f, 1f);
           

            gl.Enable(OpenGL.GL_DEPTH_TEST); // culling(sakrivanje nevidljivih povrsina)
            gl.Enable(OpenGL.GL_CULL_FACE); // test dubine
            gl.Enable(OpenGL.GL_TEXTURE);

            m_scene.LoadScene();
            m_scene.Initialize();

            gl.Enable(OpenGL.GL_NORMALIZE); //normalizuj normale (2)
            gl.Enable(OpenGL.GL_COLOR_MATERIAL); // Color Tracking II (1)
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE); // Ambijentalna i difuzna komponenta. II (1)

            SetupLighing(gl); // Svetlost
            SetupTextures(gl); // Teksture          
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            gl.PushMatrix();

            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.PushMatrix();

            gl.LoadIdentity();
            gl.LookAt(-300, 150, m_sceneDistance, 0, 0, 0, 0, 1, 0); //Pozicioniranje kamere (gore levo ne mnogo od podloge) (6)
            float[] pos = { 0, 120, 0, 1.0f };//STACIONARNO SVETLO
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pos);//POZICIJA STACIONARNOG SVETLA


            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            #region Bure
            gl.PushMatrix();
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0.0f, -1.0f, 0.0f });//REFLETOR BACA SVETLO NA DOLE, ZASTO OVDE A NE U METODI Z ASVETLO? DA BI SE ROTACIJE PROMENILE I NA OVO
     
            float[] pos2 = { 200 + lightPositionX, 150, 0f, 1.0f };//IZNAD BURETA I POSOTLJA, ALI SE DODAJE I VREDNOST SLAJDERA KOJI POMERA SVETLO (7)
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pos2);//REFLEKTOR, POZICIJA
            float[] COLOR = { r, g, b };//VREDNOST SLAJDERA

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, COLOR);//SVE KOMPONENTE REFLEKTORSKOG IZVORA PODESI NA ONO STO SLADJERI POKAZUJU (7)
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, COLOR);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, COLOR);    
            
            gl.Translate(200f + barrelX, 90 + barrelY + barrelY_Free, 0);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);//EZIM STAPANJA ZA MODEL BURETA : ADD (10) 
            gl.Scale(barrelScale, barrelScale, barrelScale);//UNIFORMNO SKLAIRANJE ZA PARAMETAR  SA SLAJDERA (7)
            gl.Translate(0, 13.575, 0);//DONJI DEO U KOORINATNI POCETAK           
            gl.Rotate(0f, 0, barrelRotation);//OKRENI ZBOG KOTRLJANJA, barrelRotation JE 0 KAD NEMA ANIMACIJE
            gl.Translate(0, -13.575, 0);//CENTAR U KOORDIANTNI POCETAK
            m_scene.Draw();          
            gl.PopMatrix();
            #endregion

            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);//DECAL ZA SVE OSTALE TEKSTURE             

            #region Podloga
            gl.PushMatrix();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Concrete]); //Podloga od betona (5)
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity(); // prazna matrica (da se poniste prethodne)
        
            gl.Scale(6, 6, 6);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Normal(0, 1, 0); // normala na podlogu (2)

            // gl.Translate(-400.0f, -60.0f, 0.0f);
            gl.Rotate(0, 0, 10);
            gl.Scale(55, 1, 55);

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(0.5f, 0.5f, 0.5f);

            gl.TexCoord(0f, 0f);
            gl.Vertex(-5f, 0.0f, -5f);
            gl.TexCoord(0f, 1f);
            gl.Vertex(-5.0f, 0.0f, 5.0f);
            gl.TexCoord(1f, 1f);
            gl.Vertex(5.0f, 0.0f, 5.0f);
            gl.TexCoord(1f, 0f);
            gl.Vertex(5.0f, 0.0f, -5.0f);
            gl.End();
            gl.PopMatrix();
            #endregion
        

            #region Rupa
            gl.PushMatrix();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Metal]); //IZABERI METAL
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();//UCITAJ PRAZNU TEXTURE MATRICU, DA SE PONISTE TRANSFORMACIJE OD RANIEJ AKO IH IMA
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            //gl.Translate(300f, -50f, 250f);
            gl.Translate(-200f, -35f , 0f);
            //gl.Scale(20f, 12f, -20f);
            gl.Rotate(-90, 1, 0, 0);
            gl.Rotate(-10, 0, 1, 0);
         
            Disk disk = new Disk();
            disk.TextureCoords = true;
            disk.NormalGeneration = Normals.Smooth;
            disk.Loops = 120;
            disk.Slices = 30;
            disk.InnerRadius = 0f;
            disk.OuterRadius = 70;
            disk.CreateInContext(gl);
            disk.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
  
            gl.PopMatrix();
            #endregion

            #region Postolje
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[(int)TextureObjects.Wood]); //IZABERI DRVO
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();//UCITAJ PRAZNU TEXTURE MATRICU, DA SE PONISTE TRANSFORMACIJE OD RANIEJ AKO IH IMA
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.Translate(200, 20, 0);
            gl.Rotate(holderRotation, 0, 0, 1);//ROTIRANJE PRILIKOM ANIMACIJE, U SUPROTNOM JE holderRotation 0 
            gl.Rotate(-90f, 0f, 0f);
         
            Cylinder cil = new Cylinder();
            cil.TextureCoords = true;
            cil.NormalGeneration = Normals.Smooth;
            cil.BaseRadius = 70;
            cil.Slices = 4;
            cil.Height = 80;          
            cil.CreateInContext(gl);
            cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
           
            gl.PopMatrix();
            #endregion

            #region Tekst
            gl.PushMatrix();
            gl.Viewport(0, 0, m_width/2, m_height/2);     //donji desni ugao
            gl.DrawText3D("Helvetica", 14, 0, 0, "");
            gl.DrawText(Sirina_prozora - 250, 100, 1.0f, 1.0f, 0.0f, "Helvetica ", 14, "Predmet: Racunarska grafika");
            gl.DrawText(Sirina_prozora - 250, 80, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "Sk.god:  2019/20.");
            gl.DrawText(Sirina_prozora - 250, 60, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "Ime: Veljko");
            gl.DrawText(Sirina_prozora - 250, 40, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "Prezime: Vukovic");
            gl.DrawText(Sirina_prozora - 250, 20, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "Sifra zad: 13.1");
            gl.DrawText(Sirina_prozora - 250, 100, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "_______________________");
            gl.DrawText(Sirina_prozora - 250, 80, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "______________");
            gl.DrawText(Sirina_prozora - 250, 60, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "________");
            gl.DrawText(Sirina_prozora - 250, 40, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "______________");
            gl.DrawText(Sirina_prozora - 250, 20, 1.0f, 1.0f, 0.0f, "Helvetica", 14, "___________");
            gl.PopMatrix();
            #endregion


            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        private void SetupLighing(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING); //ukljuci svetlo
            enableLight0(true, gl); // ukljuci sv. izvor 0 (tackasto i stacionarno)
            enableLight1(true, gl);// ukljuci sv. izvor 1 (reflektor)

            float[] white = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] yellow = new float[] { 1.0f, 1.0f, 0.0f };

            //tackasti izvor, stacionaran (2)
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, white);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, white);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, white);

            //iznad bureta, cut off 30 (9)
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 30f); //da bude reflektor
          
        }

        private void SetupTextures(OpenGL gl)
        {
            textures = new uint[textureCount];
            gl.Enable(OpenGL.GL_TEXTURE_2D); // ukljuciti teksture
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL); // stapanje sa materijalom (GL_DECAL) (3)
            gl.GenTextures(textureCount, textures);
            for (int i = 0; i < textureCount; ++i)
            {//REDOM UCITAJ SLIKE I KREIRAJ TEKSURE
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[i]);
                Bitmap image = new Bitmap(textureFiles[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                System.Drawing.Imaging.BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA8, imageData.Width, imageData.Height, 0,
                            OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR); //linearno filtiranje po obe ose (3)
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT); //wrapping (GL_REPEAT) za obe ose (3)
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        public void enableLight0(bool e, OpenGL gl)
        {
            if (e) gl.Enable(OpenGL.GL_LIGHT0); else gl.Disable(OpenGL.GL_LIGHT0);
        }

        public void enableLight1(bool e, OpenGL gl)
        {
            if (e) gl.Enable(OpenGL.GL_LIGHT1); else gl.Disable(OpenGL.GL_LIGHT1);
        }

        public void AnimationBegin()
        {
            animation = true; //OZNACI DA JE POCELA ANIMACIJA, BITNO DA BI U MAINWINDOWS HANDLER IGNORISAO KOMANDE KORISNIKA
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(StartMovingBarrel);
            timer.Interval = TimeSpan.FromMilliseconds(28);
            timer.Start();

        }

        private void StartMovingBarrel(object sender, EventArgs e)
        {
            //ZASTO SE KORISTI BAS 0.176?? ZATO STO JE TO VREDNOST KOJA SE DOBIJE NA OSNOVU TRIGONOMETRIJE I ROTIRANE PODLOGE (ODNOS DUZINE PODLOGE I RAZLIKE NJENIH Y MAX I Y MIN JE 1:0.176)
            

            if (holderRotation < 25)//PRVI DEO ANIMACIJE POMERA DRZAC
            {              
                holderRotation++;
                barrelX -= 2f;//I MALO POMERI BURE DA BI SE POMERAL ZAJEDNO SA DRZACEM
                barrelY -= 0.176f * 3f;//I MALO POMERI BURE DA BI SE POMERAL ZAJEDNO SA DRZACEM          
            }
            else if (holderRotation == 25 && barrelY_Free != -59)//AKO SE DRZAC DOVLJNO ROTIRA, I AKO BURE NIJE DOTAKLO TLO
            {
                barrelX -= 4f;//RANDOM VREDNOST, DA IZLGEDA DA PADA
                barrelY_Free -= 8f;
                if (barrelY_Free < -59) barrelY_Free = -59;//KAD PROBIJE GRANICU, VRATI NA 53, JER JE TADA TACNO NA PODLOZI
            }
            else if (barrelX > -425 + 50 + 25)
            {//NA -230 POCINJE RUPA, PA SE DO RUPE KOTRLJA OVAKO: 
                //KOTRLJANJE                
                barrelRotation += 5;//ROTACIJA ZBOG KOTRLJANJA
                barrelX -= 3f;//I MALO POMERI BURE DA BI SE POMERAL ZAJEDNO SA DRZACEM
                barrelY -= 0.176f * 3f;//I MALO POMERI BURE DA BI SE POMERAL ZAJEDNO SA DRZACEM  
            }
            else
            {
                //A KAD NAIDJE NA RUPU POCINJE NAGLO DA PROPADA
                barrelY -= 9;
                barrelX -= 4f;           
                if (barrelY < -200)
                {//KADA DOVLJNO PROPADNE ZAVRSI ANIAMCIJU
                    animation = false;
                    timer.Stop();//ZAUSTAVI TAJMER DA SE NE POZIVA VISE OVA METODA
                    ((MainWindow)Application.Current.MainWindow).sliders(true);//UKLJUCI SLAJDERE
                    barrelY_Free = barrelX = barrelY = barrelRotation = holderRotation = 0;//VRATI SVE NA POCETNE VREDNOSTI
                }
            }
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
            gl.Perspective(60f, (double)width / height, 1f, 20000f);
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
