using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.Vision;

using NationalInstruments.Vision.Acquisition.Imaq;

namespace Grab
{
    public partial class DebugWin : Form
    {
        public PixelValue2D debugDisp = null;

        public DebugWin()
        {
            InitializeComponent();
        }

        public void DebugDisplay()
        {
            imageViewer.Image.ArrayToImage(debugDisp);
        }
    }
}
