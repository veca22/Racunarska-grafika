using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Barrel"), "untitled.obj", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Sirina_prozora = (int)Window.GetWindow(this).ActualWidth;
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.Width, (int)openGLControl.Height);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_world.Animation == false)
            {
                switch (e.Key)
                {
                    case Key.C:
                        m_world.AnimationBegin();
                        sliders(false);
                        break;
                    case Key.Q: this.Close(); break;
                    case Key.W: m_world.RotationX -= 5.0f; break;
                    case Key.S: m_world.RotationX += 5.0f; break;
                    case Key.A: m_world.RotationY -= 5.0f; break;
                    case Key.D: m_world.RotationY += 5.0f; break;
                    case Key.Add: m_world.SceneDistance -= 700.0f; break;
                    case Key.Subtract: m_world.SceneDistance += 700.0f; break;

                }
            }
        }

        public void sliders(bool v)
        {
            sliderSize.IsEnabled = v;
            lightPosition.IsEnabled = v;
            r.IsEnabled = v;
            g.IsEnabled = v;
            b.IsEnabled = v;
        }

        private void sliderSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
                m_world.BarrelScale = (float)e.NewValue;
        }

        private void lightPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
                m_world.LightPositionX = (float)e.NewValue;
        }

        private void r_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
                m_world.B = (float)e.NewValue;
        }

        private void g_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
                m_world.R = (float)e.NewValue;
        }

        private void b_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
                m_world.R = (float)e.NewValue;
        }
    }
}
