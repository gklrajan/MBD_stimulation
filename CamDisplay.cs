using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.Vision;

using NationalInstruments.Vision.Acquisition.Imaq;
using System.Threading;
using System.IO;

namespace Grab
{
    public partial class CamDisplay : Form
    {
        //private ImaqSession _session = null;
        //private System.ComponentModel.BackgroundWorker acquisitionWorker;
  //      ImaqSession thisSession = null;
  //      VisionImage proxy = new VisionImage(ImageType.U8);
        bool WriteFull = false;
        FileStream dataFullFrame;
        BinaryWriter fullFrameWriter;
        GrabAndStimAS_PHX_2016 varSender;
        const int arrayWidth = 640;
        const int arrayHeight = 480;
        byte[,] fullIM;
        byte[] IMrow = new byte[480*640];
        const int fullSize = 480 * 640;
        uint bufferNumber = 0;
        public CamDisplay(ImaqSession _sessionForDisplay)
        {
            InitializeComponent();
         //   thisSession = _sessionForDisplay;
          //  imageViewer1.Attach(proxy);
            //ProcessThreadCollection threads;
            //Process.
            /*acquisitionWorker = new System.ComponentModel.BackgroundWorker();
            acquisitionWorker.DoWork += new DoWorkEventHandler(acquisitionWorker_DoWork);
            //  Create a session.
            _session = new ImaqSession(interfaceText);
            //  Start a Grab acquisition.
            _session.GrabSetup(true);
            //  Start the background worker thread.
            acquisitionWorker.RunWorkerAsync();*/
        }

   /*     public ImaqSession session
        {
            set
            {
                //thisSession = value;
            }
            get
            {
                return thisSession;
            }
        }
*/
        public void Grabber(object sender)
        {
            //varSender = (GrabAndStim2010)sender; 
            
           // uint frame = 0;
            try
            {
                while (this.Created)
                {
                    //proxy = thisSession.Acquisition.Extract(bufferNumber, out bufferNumber).ToImage();
                   // if (!varSender.newestImage.IsEmpty)
                      //  proxy = varSender.newestImage;
                    //if (WriteFull && varSender.BlobStart)
                    //{
                        //frame++;
                        //fullIM = varSender.newestImage.ImageToArray().U8;
                        //System.Buffer.BlockCopy(fullIM, 0, IMrow, 0, fullSize);
                        //fullFrameWriter.Write(IMrow);
                        //for (int i = 0; i < arrayWidth; i++)
                         //   for (int j = 0; j < arrayHeight; j++)
                          //      fullFrameWriter.Write(fullIM[j, i]);
                            //System.Buffer.BlockCopy(fullIM, 480 * i, IMrow, 0, 480);
                            //fullFrameWriter.Write(IMrow);
                        //framecounter.Text = frame.ToString();
                    //}
                    bufferNumber++;
                }
            }
            catch
            {
                Grabber(sender);
            }
        }

        //private void fullFrameWrite_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (WriteFull == false)
        //    {
        //        WriteFull = true;
        //        string nameOfFullFile = "C:\\Users\\twd\\data\\" + System.DateTime.Today.Month + "_" +
        //            System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumberBox.Text + "_fullFrameMovie.bin";
        //        dataFullFrame = new FileStream(nameOfFullFile, FileMode.Append);
        //        fullFrameWriter = new BinaryWriter(dataFullFrame);
        //    }
        //    else
        //    {
        //        WriteFull = false;
        //        dataFullFrame.Close();
        //    }
        //}

    }
}
