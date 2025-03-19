using System.Windows.Forms;
using System.Threading;
using System.Drawing;
namespace Graphics
{
    public partial class GraphicsForm : Form
    {
        Renderer renderer = new Renderer();

        public GraphicsForm()
        {
            InitializeComponent();
            simpleOpenGlControl1.InitializeContexts();
            initialize();
            programTimer.Enabled = true;
        }
        void initialize()
        {
            renderer.Initialize();
        }

        private void GraphicsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            renderer.CleanUp();
        }

        private void simpleOpenGlControl1_Paint(object sender, PaintEventArgs e)
        {
            renderer.Update();
            renderer.Draw();
        }

        private void programTimer_Tick(object sender, System.EventArgs e)
        {
            simpleOpenGlControl1.Refresh();
        }

        private void simpleOpenGlControl1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // If space pressed
            if (e.KeyChar == ' ')
            {
                renderer.vinylSpinning = !(renderer.vinylSpinning);
            }

            simpleOpenGlControl1.Refresh();
        }
    }
}
