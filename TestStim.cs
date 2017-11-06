using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO;

namespace Grab
{
    public partial class TestStim : Form
    {

        private Microsoft.DirectX.Direct3D.Device device = null;
       // private Microsoft.DirectX.Direct3D.Device device2 = null;
        //private Microsoft.DirectX.Direct3D.Device device2 = null;
        private bool fullScreen = false;
        private float playingH = 1024f;        // Height of our playing area (meters)
        private float playingW = 1024f;       // Width of our playing area (meters)
        private VertexBuffer vertices = null;
        private VertexBuffer verticesDos = null;
        private VertexBuffer halfMoon = null;
        private VertexBuffer annulizer = null;
        private VertexBuffer approachBar = null;
        private VertexBuffer vertices3 = null;
        private VertexBuffer verticesTex = null;

        private VertexBuffer BackgroundTexture = null;
        private Texture bgTexture = null;
        private Texture soccer = null;
        private Texture black = null;
        private Random gratingdirection = null;

        const int centX = 540;
        const int centY = 512;

        bool OMRstartswitch = false;
        //this watch keeps time independent of the counter variable, as the counter variable is periodically reset to control most of the blob
        //initiation, etc.
        System.Diagnostics.Stopwatch gratingwatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch OMRwatch = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch checkmewatch = new System.Diagnostics.Stopwatch();

        bool gratingSwitch = true;
        int gratingCounter = 0;

        float gratingangle = 0;
        int bgclr = 0;
       // private int lastX = 655, lastY = 512;
        //float lastAngle = 0;
        rectPoints rectPts = null;
        const float numBands = 20;
        VertexBuffer[] circleList = new VertexBuffer[(int)numBands];
        VertexBuffer bgCircle = null;
        GrabAndStimAS_PHX_2016 varSender = null;
        float[] rectCoords;
        int counter = 0;
        float R = 4f;
        float blobRnext, blobRdub;
        float xSquare, ySquare;
        bool BlobSwitch = false;
        float leading, trailing;

        //bot hdelegates are used to change the location of the window after it's been created.
        delegate void SetTitleCallback(string newtitle);
        delegate void SetLocationXCallback(int locSetX);
        delegate void SetLocationYCallback(int locSetY);
        delegate void SetSizeHorizCallback(int horizSet);
        delegate void SetSizeVertCallback(int vertSet);

        //For writing stim size to file
        FileStream dataStimFile = null;
        BinaryWriter stimFileWriter = null;
        int presCount = 0;
        System.Diagnostics.Stopwatch stimWriteWatch = new System.Diagnostics.Stopwatch(); //write the time for each R as well
        public TestStim()
        {
            InitializeComponent();
            
            if (!InitializeDirect3D())
                return;
            vertices = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
              20,      // How many
              device, // What device
              0,      // No special usage
              CustomVertex.PositionColored.Format,
              Pool.Managed);
           verticesDos = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
              10,      // How many
              device, // What device
              0,      // No special usage
              CustomVertex.PositionColored.Format,
              Pool.Managed);
           verticesTex = new VertexBuffer(typeof(CustomVertex.PositionTextured), // Type of vertex
                10,      // How many
                device, // What device
                0,      // No special usage
                CustomVertex.PositionTextured.Format,
                Pool.Managed);
           bgCircle = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
            100,      // How many
            device, // What device
            0,      // No special usage
            CustomVertex.PositionColored.Format,
            Pool.Managed);
           halfMoon = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
            20,      // How many
            device, // What device
            0,      // No special usage
            CustomVertex.PositionColored.Format,
            Pool.Managed);
           BackgroundTexture = new VertexBuffer(typeof(CustomVertex.PositionColoredTextured), // Type of vertex
            4,      // How many
            device, // What device
            0,      // No special usage
            CustomVertex.PositionColoredTextured.Format,
            Pool.Managed);
           annulizer = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
            100,      // How many
            device, // What device
            0,      // No special usage
            CustomVertex.PositionColored.Format,
            Pool.Managed);
           approachBar = new VertexBuffer(typeof(CustomVertex.PositionColored), // Type of vertex
            4,      // How many
            device, // What device
            0,      // No special usage
            CustomVertex.PositionColored.Format,
            Pool.Managed);

           vertices3 = new VertexBuffer(typeof(CustomVertex.PositionTextured), // Type of vertex
            20,      // How many
            device, // What device
            0,      // No special usage
            CustomVertex.PositionTextured.Format,
            Pool.Managed);
            
            bgTexture = TextureLoader.FromFile(device, "C:/Users/Gokul/Desktop/MBDonDell/MBD_Grab&Stim/MIKROTRON - AS - PHX/black.bmp");
            bgclr = Color.Transparent.ToArgb();
            soccer = TextureLoader.FromFile(device, "C:/Users/Gokul/Desktop/MBDonDell/MBD_Grab&Stim/MIKROTRON - AS - PHX/black.bmp");
            black = TextureLoader.FromFile(device, "C:/Users/Gokul/Desktop/MBDonDell/MBD_Grab&Stim/MIKROTRON - AS - PHX/black.bmp");

           for (int i = 0; i < numBands; i++)
           {
               circleList[i] = new VertexBuffer(typeof(CustomVertex.PositionColored),
                         100,
                         device,
                         0,
                         CustomVertex.PositionColored.Format,
                         Pool.Managed);
           }

           gratingdirection = new Random();

            //Try writing the stimulus size to file
            string stimstamp = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
            string nameOfFullFile = "C:/Users/Gokul/Documents/DataBeta/FishParams/" + stimstamp + "_stimFile.bin";
           //string nameOfFullFile = "C:\\Users\\twd\\data\\" + System.DateTime.Today.Month + "_" +
           //    System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + "_fullFrameMovie.bin";
           dataStimFile = new FileStream(nameOfFullFile, FileMode.Append);
           stimFileWriter = new BinaryWriter(dataStimFile);
           
        }

        private bool InitializeDirect3D()
        {
            try
            {
                // Now let's setup our D3D stuff
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
              
                //Lock display loop to 60Hz
                presentParams.BackBufferCount = 1;
                presentParams.PresentationInterval = PresentInterval.One;

                //backbuffercount = 1, presentationinterval = presentinterval.one, swapeffect.discard forces
                //vertical synchronization, i.e. refresh rate locked as close as possible to 60Hz. Originally
                //taken from http://stackoverflow.com/questions/10972533/is-there-anyway-to-know-when-the-screen-has-been-updated-refreshed-opengl-or-di

                //presentParams.FullScreenRefreshRateInHz = 60;

                device = new Microsoft.DirectX.Direct3D.Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
               
                
            }
            catch (DirectXException)
            {
                return false;
            }

            return true;
        }

        Random rand = new Random();
        
        //public bool Render(int[,] renderCoords, bool stimOff, ref int counter, float rotAngle)
        public void Render(object sender)
        {
     
            //MessageBox.Show(checkmewatch.ElapsedMilliseconds.ToString());
           
            varSender = (GrabAndStimAS_PHX_2016)sender;
            //NEED THIS 10/4/10
            //if (varSender.bgCount <= 1)
            //{
            //    R = 0;
            //    blobRnext = 0;
            //    counter = 0;
            //}
            if (device == null)
                return;

            if (varSender.StimOff && gratingSwitch)
            {
                gratingwatch.Start();
                gratingSwitch = false;
            }
            
            //if (!varSender.BlobStart)
            //{
            //    gratingwatch.Start();
            //    OMRstartswitch = false;
            //}



            //repostioning the window as part of my calibration scheme. values are inputted in the main window and sent to this thread, where they are
            //then used to update the location

            SetLocationX(varSender.locationXsender);
            SetLocationY(varSender.locationYsender);
            SetSizeHoriz(varSender.sizeHorizsender);
            SetSizeVert(varSender.sizeVertsender);

            if (varSender.downTime)
            {
                device.Clear(ClearFlags.Target, System.Drawing.Color.Gray, 1.0f, 0);
                device.BeginScene();
                device.EndScene();
                device.Present();
            }

            else
            {
                device.Clear(ClearFlags.Target, Color.FromArgb(100,100,100), 1.0f, 0);
            

                int wid = Width;                            // Width of our display window
                int hit = Height;                           // Height of our display window.
                float aspect = varSender.aspRatioSender * (float)wid / (float)hit;     // What is the aspect ratio? (float)wid / (float)hit;//

                device.RenderState.ZBufferEnable = false;   // We'll not use this feature
                device.RenderState.Lighting = false;        // Or this one...
                device.RenderState.CullMode = Cull.None;    // Or this one...

                float widP = playingH * aspect;         // Total width of window
                //MessageBox.Show(wid.ToString() + " " + widP.ToString());
                device.Transform.Projection = Matrix.OrthoOffCenterLH(0, widP, 0, playingH, 0, 1);
                //device2.Transform.Projection = Matrix.OrthoOffCenterLH(0, widP, 0, playingH, 0, 1);
                //device.Transform.World = Matrix.Translation(varSender.fishCoords[0,0], varSender.fishCoords[0,1], 0)*Matrix.RotationZ(varSender.rotAngle);
                device.Transform.World = Matrix.Translation(-playingW / 2, -playingH / 2, 0)*Matrix.RotationZ((float)Math.PI) * Matrix.Translation(playingW / 2, playingH / 2, 0);
                //device.Transform.World = Matrix.Translation(-5.02f, -6.55f, 0) * Matrix.RotationZ((float)Math.PI / 2) * Matrix.Translation(5.02f, 6.55f, 0);


                GraphicsStream[] gs = new GraphicsStream[(int)numBands];


                //int clr = Color.FromArgb(0, 0, 0).ToArgb();
                int clr = Color.FromArgb(127, 127, 127).ToArgb();
                int q = rand.Next(70);
                int clr2 = Color.FromArgb(255, 255, 255).ToArgb();
                int clr3 = Color.FromArgb(100, 100, 100).ToArgb();
                int testclr = Color.FromArgb(65, 178, 22).ToArgb();
                int darkendingClr = Color.FromArgb(255, 255, 255).ToArgb();
                int blobClr = Color.FromArgb(varSender.contrastValue, varSender.contrastValue, varSender.contrastValue).ToArgb();
                //int blobClr = Color.FromArgb(0, 0, 0).ToArgb();

                float xMidPoint = playingW / 2;
                //MessageBox.Show(xMidPoint.ToString());
                float yMidPoint = playingH / 2;
                
                float Rnext = R / numBands;

                float Radvance = R - R * (500 - counter % 500) / 500;
                float RadvanceNext = Radvance / numBands;
                float xBlob = 0, yBlob = 3f;
                float xHalfMoonBox = 0, yHalfMoonBox = 0;
                //float xMidPoint = xInMeyeA + 0.5f;
                //float yMidPoint = yInMeyeA + 0.5f;
                #region CALIBRATE
                if (varSender.calibrateBox.Checked)
                {
                    //MessageBox.Show("wtf");
                    //MessageBox.Show((System.DateTime.Now.Millisecond - varSender.millitime).ToString());
                    device.Clear(ClearFlags.Target, System.Drawing.Color.White, 1.0f, 0);
                    //device.Clear(ClearFlags.Target, System.Drawing.Color.FromArgb(128,128,128), 1.0f, 0);//For Power measuremnt assay
                    //float newYscaler = 1024 - varSender.lockY;
                    float newYeyeA = 1024f - varSender.lockY;
                    float xInMscaler = varSender.scaleFactorX * ((float)varSender.lockX) - varSender.scaleFactorX * xMidPoint + xMidPoint;
                    float yInMscaler = varSender.scaleFactor * (newYeyeA) - varSender.scaleFactor * yMidPoint + yMidPoint;

                    R = 50f;
                    device.BeginScene();
                    //This is a small square drawn for calibration
                    if (true)//varSender.BlobDelay.ElapsedMilliseconds % 40000 >= 20000)
                    {
                    if (varSender.BlobDelay.ElapsedMilliseconds % 40000 >= 30000)
                        clr = Color.FromArgb(129, 129, 129).ToArgb();
                    clr = Color.FromArgb(0, 0, 0).ToArgb();

                    float xcalibrateBox = xInMscaler;
                    float ycalibrateBox = yInMscaler;
                    float boxWid = 40f;// 200f; //400f;//for power measurement assay

                    if (!varSender.dubCalBox.Checked) //Show the original, standard, small square
                    {
                        GraphicsStream calibrateBox = verticesDos.Lock(0, 0, 0);
 

                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox + boxWid, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox + boxWid, ycalibrateBox + boxWid, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox + boxWid, ycalibrateBox, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox + boxWid, ycalibrateBox - boxWid, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox - boxWid, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox - boxWid, ycalibrateBox - boxWid, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox - boxWid, ycalibrateBox, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox - boxWid, ycalibrateBox + boxWid, 0, clr));
                        calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox + boxWid, 0, clr));
                        verticesDos.Unlock();
                        device.SetStreamSource(0, verticesDos, 0);
                        device.VertexFormat = CustomVertex.PositionColored.Format;
                        device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 8);
                    }
                    else if(varSender.gridBox.Checked){
                        GraphicsStream calibrateBox = null;
                        boxWid = 20f;
                        for (float xint = (float)varSender.lockX - 400; xint < playingW; xint += 200)
                        {
                            for (float yint = (float)varSender.lockY - 400; yint < playingH; yint += 200)
                            {

                                newYeyeA = 1024f - yint;
                                xcalibrateBox = varSender.scaleFactorX * xint - varSender.scaleFactorX * xMidPoint + xMidPoint;
                                ycalibrateBox = varSender.scaleFactor * (newYeyeA) - varSender.scaleFactor * yMidPoint + yMidPoint;
                                calibrateBox = verticesDos.Lock(0, 0, 0);

                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox + boxWid, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox + boxWid, ycalibrateBox + boxWid, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox + boxWid, ycalibrateBox, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox + boxWid, ycalibrateBox - boxWid, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox - boxWid, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox - boxWid, ycalibrateBox - boxWid, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox - boxWid, ycalibrateBox, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox - boxWid, ycalibrateBox + boxWid, 0, clr));
                                calibrateBox.Write(new CustomVertex.PositionColored(xcalibrateBox, ycalibrateBox + boxWid, 0, clr));
                                verticesDos.Unlock();
                                device.SetStreamSource(0, verticesDos, 0);
                                device.VertexFormat = CustomVertex.PositionColored.Format;
                                device.SetRenderState(RenderStates.FillMode, 1);
                                device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 8);
                            }
                        }
                    }
                    else //Show two circles centered on coordimates with radius 5mm. Only use after 40f has been set to 0.4 cm
                    {
                        xBlob = xInMscaler;
                        yBlob = yInMscaler + 50f;
                        R = 50f;
                        blobRnext = 50f;
                        blobClr = Color.FromArgb(0, 0, 0).ToArgb();
                             GraphicsStream blob = bgCircle.Lock(0, 0, 0);
                                        for (int a = 0; a < 20; a++)
                                            blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(2*a * Math.PI / 20)), yBlob + (float)(R * Math.Sin(2*a * Math.PI / 20)), 0, blobClr));


                                bgCircle.Unlock();

                                device.SetStreamSource(0, bgCircle, 0);
                                device.VertexFormat = CustomVertex.PositionColored.Format;
                                device.SetRenderState(RenderStates.FillMode, 1);
                                device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);

                                blobClr = Color.FromArgb(0, 0, 255).ToArgb();
                                yBlob = yInMscaler - 50f;

                                blob = bgCircle.Lock(0, 0, 0);
                                for (int a = 0; a < 20; a++)
                                    blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(R * Math.Sin(2 * a * Math.PI / 20)), 0, blobClr));


                                bgCircle.Unlock();

                                device.SetStreamSource(0, bgCircle, 0);
                                device.VertexFormat = CustomVertex.PositionColored.Format;
                                device.SetRenderState(RenderStates.FillMode, 1);
                                device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);

                    }

                    }
                    if (false)//(varSender.BlobDelay.ElapsedMilliseconds % 40000 >= 10000 && varSender.BlobDelay.ElapsedMilliseconds % 40000 < 20000)
                    {
                        
                        GraphicsStream calibrateBox = verticesTex.Lock(0, 0, 0);

                        float xcalibrateBox = xInMscaler;
                        float ycalibrateBox = yInMscaler;
                        float boxWid = 400f;//20f

                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox, ycalibrateBox, 0f, 0.5f,0.5f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox, ycalibrateBox + boxWid, 0f, 0.5f, 1f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox + boxWid, ycalibrateBox + boxWid, 0f, 1f, 1f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox + boxWid, ycalibrateBox, 0f, 1f, 0.5f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox + boxWid, ycalibrateBox - boxWid, 0f, 1f, 0f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox, ycalibrateBox - boxWid, 0f, 0.5f, 0f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox - boxWid, ycalibrateBox - boxWid, 0f, 0f, 0f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox - boxWid, ycalibrateBox, 0f, 0f, 0.5f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox - boxWid, ycalibrateBox + boxWid, 0f, 0f, 1f));
                        calibrateBox.Write(new CustomVertex.PositionTextured(xcalibrateBox, ycalibrateBox + boxWid, 0f, 0.5f, 1f));
                        verticesTex.Unlock();

                        device.SetStreamSource(0, verticesTex, 0);
                        device.SetTexture(0, soccer);
                        device.VertexFormat = CustomVertex.PositionTextured.Format;
                        device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 8);
                        device.SetTexture(0, null);
                       
                    }
                    device.EndScene();
                    device.Present();
                    //if (varSender.imbuffCount < 10)
                    //{
                    //    Surface frbuff = device.CreateOffscreenPlainSurface(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Format.A8R8G8B8, Pool.Scratch);
                    //    Surface frontbuffer = new Surface(
                    //    Surface backbuffer = device.GetBackBuffer(0, 0, BackBufferType.Mono);
                    //    device.GetFrontBufferData(0, frbuff);
                    //    SurfaceLoader.Save("F:\\data\\wtffff.bmp", ImageFileFormat.Bmp, frbuff);
                    //    frbuff.Dispose();
                       
                    //    device.GetSwapChain(0).
                    //}
                }
                #endregion

                else
                {
                    #region OMR ATTRACTION
                    if (varSender.TryStim)
                    {
                        //gratingCounter = gratingwatch.ElapsedMilliseconds / 1000;
                        float projAdvance = (float)gratingwatch.ElapsedMilliseconds / 40; //was 40 on 11 oct 2017 <-- 100

                        if ((projAdvance + 53 * widP / 72) >= widP)// * Math.Sin(varSender.rotAngle))
                        {
                            gratingwatch.Stop();
                            gratingwatch.Reset();
                            gratingwatch.Start();
                        }
                        //MessageBox.Show(counter.ToString());

                        //float tile = counter / 10;
                        //MessageBox.Show(counter.ToString() + " " + projAdvance.ToString());
                        if (varSender.lockX != 25)
                        {
                            if (varSender.lockX == centX && varSender.lockY > centY)
                                gratingangle = (float)Math.PI;
                            else if (varSender.lockX == centX && varSender.lockY < centY)
                                gratingangle = 0f;
                            else if (varSender.lockY == centY && varSender.lockX < centX)
                                gratingangle = (float)Math.PI / 2;
                            else if (varSender.lockY == centY && varSender.lockX > centX)
                                gratingangle = 3 * (float)Math.PI / 2;
                            else if (varSender.lockY < centY)
                                gratingangle = (float)Math.Atan((float)(varSender.lockX - centX) / (varSender.lockY - centY));
                            else
                                gratingangle = (float)Math.Atan((float)(varSender.lockX - centX) / (varSender.lockY - centY)) + (float)Math.PI;
                            //gratingangle = gratingangle + (float)Math.PI / 2;
                        }
                        else if (gratingwatch.ElapsedMilliseconds % 4000 > 10 && gratingwatch.ElapsedMilliseconds % 4000 < 30 && gratingwatch.ElapsedMilliseconds > 30)
                            gratingangle = (float)gratingdirection.Next(16) / 2;


                        device.Transform.Projection = Matrix.OrthoOffCenterLH(0, playingW, projAdvance + widP / 8, projAdvance + widP / 2, 0, 1) *
                            Matrix.RotationZ(gratingangle) * Matrix.RotationZ((float)Math.PI);
                        device.BeginScene();
                        GraphicsStream stm = BackgroundTexture.Lock(0, 0, 0);
                        stm.Write(new CustomVertex.PositionColoredTextured(0, 0, 0, bgclr, 1, 1));
                        stm.Write(new CustomVertex.PositionColoredTextured(0, 2 * playingH, 0, bgclr, 0, 1));
                        stm.Write(new CustomVertex.PositionColoredTextured(2 * playingW, 2 * playingH, 0, bgclr, 0, 0));
                        stm.Write(new CustomVertex.PositionColoredTextured(2 * playingW, 0, 0, bgclr, 1, 0));

                        BackgroundTexture.Unlock();

                        device.SetTexture(0, bgTexture);
                        device.SetStreamSource(0, BackgroundTexture, 0);
                        device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
                        device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
                        device.SetTexture(0, null);
                        device.EndScene();
                        device.Present();
                        counter++;
                    }
                    #endregion

                    #region CollapsingCircle_commented
                    /*if (true)//varSender.StimOff)
                        {
                            counter = 0;
                            device.BeginScene();
                            if ((R - Rnext * (numBands - 2) - Radvance) <= 0)
                                counter = -1;


                            GraphicsStream bg = bgCircle.Lock(0, 0, 0);
                            if ((R - Rnext * (numBands - 1) - Radvance) <= 0)
                                for (int a = 0; a < 100; a++)
                                    bg.Write(new CustomVertex.PositionColored(xMidPoint + (float)((R + Rnext - Radvance) * Math.Cos(2 * a * Math.PI / 100)), yMidPoint + (float)((R + Rnext - Radvance) * Math.Sin(2.0 * a * Math.PI / 100)), 0, clr));
                            else
                                for (int a = 0; a < 100; a++)
                                    bg.Write(new CustomVertex.PositionColored(xMidPoint + (float)(R * Math.Cos(2 * a * Math.PI / 100)), yMidPoint + (float)(R * Math.Sin(2.0 * a * Math.PI / 100)), 0, clr));
                            bgCircle.Unlock();

                            device.SetStreamSource(0, bgCircle, 0);
                            device.VertexFormat = CustomVertex.PositionColored.Format;

                            // PrimitiveType.TriangleFan draws triangles, always starting at some base vertex (here 0), so the triangles touch vertices 0,1,2, then 0,3,4, etc.
                            // To draw a 20-gon, need 18 triangles, hence the last argument.

                            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 98);

                            for (int i = 0; i < numBands; i++)
                            {
                                if ((R - Rnext * i - Radvance) < 0)
                                {
                                }
                                else
                                {
                                    gs[i] = circleList[i].Lock(0, 0, 0);
                                    //float j = numBands - i - 1;
                                    //int rollOver = 1;
                                    for (int a = 0; a < 100; a++)
                                    {

                                        if (i % 2 != 0)
                                            gs[i].Write(new CustomVertex.PositionColored(xMidPoint + (float)((R - Rnext * i - Radvance) * Math.Cos(2 * a * Math.PI / 100)), yMidPoint + (float)((R - Rnext * i - Radvance) * Math.Sin(2.0 * a * Math.PI / 100)), 0, clr));
                                        else
                                            gs[i].Write(new CustomVertex.PositionColored(xMidPoint + (float)((R - Rnext * i - Radvance) * Math.Cos(2 * a * Math.PI / 100)), yMidPoint + (float)((R - Rnext * i - Radvance) * Math.Sin(2.0 * a * Math.PI / 100)), 0, clr2));

                                    }
                                    circleList[i].Unlock();

                                    device.SetStreamSource(0, circleList[i], 0);
                                    device.VertexFormat = CustomVertex.PositionColored.Format;

                                    // PrimitiveType.TriangleFan draws triangles, always starting at some base vertex (here 0), so the triangles touch vertices 0,1,2, then 0,3,4, etc.
                                    // To draw a 20-gon, need 18 triangles, hence the last argument.

                                    device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 98);
                                }
                            }
                            device.EndScene();
                            device.Present();
                        }*/
                    #endregion
                    else
                    {
                        if (!gratingSwitch)
                        {
                            gratingSwitch = true;
                            gratingwatch.Stop();
                            gratingwatch.Reset();
                        }


                        counter = varSender.bgCount;

                        //device.Clear(ClearFlags.Target, System.Drawing.Color.Gray, 1.0f, 0);
                        device.Clear(ClearFlags.Target, Color.FromArgb(128, 128, 128).ToArgb(), 1.0f, 0);
                        //device.Clear(ClearFlags.Target, Color.FromArgb(255, 255, 255).ToArgb(), 1.0f, 0);
                        device.BeginScene();

                        float newYeyeA = varSender.lockY;
                        float newXeyeA = 1024f - varSender.lockX;

                        float xInMeyeA = varSender.scaleFactorX * (newXeyeA) - varSender.scaleFactorX * xMidPoint + xMidPoint;
                        //(float)varSender.lockX;//0.8175f * ((float)varSender.lockX) + 0.1825f * xMidPoint;
                        float yInMeyeA = varSender.scaleFactor * (newYeyeA) - varSender.scaleFactor * yMidPoint + yMidPoint;//newYeyeA;//0.8175f * (newYeyeA) + 0.1825f * yMidPoint;
                        //MessageBox.Show(varSender.lockX.ToString() + " " + varSender.lockY.ToString());

                        //MessageBox.Show(varSender.lockX.ToString());

                        //MessageBox.Show(counter.ToString());
                        #region RectCode_commented
                        /*if (counter > 120 && refAngle == varSender.rotAngle && refAngle2 == varSender.rotAngle && !lockStim)
                            {
                                lockX = xInMeyeA;
                                lockY = yInMeyeA;
                                lockAngle = varSender.rotAngle;
                                lockStim = true;
                                counter = 120;
                            }*/

                        /*rectPts = new rectPoints(xInMeyeA, yInMeyeA, varSender.rectangleAngle + (float)Math.PI/2);

                        GraphicsStream backRect = verticesDos.Lock(0, 0, 0);
                        backRect.Write(new CustomVertex.PositionColored(xInMeyeA, yInMeyeA, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt1x, rectPts.pt1y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt2x, rectPts.pt2y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt3x, rectPts.pt3y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt4x, rectPts.pt4y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt5x, rectPts.pt5y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt6x, rectPts.pt6y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt7x, rectPts.pt7y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt8x, rectPts.pt8y, 0, clr2));
                        backRect.Write(new CustomVertex.PositionColored(rectPts.pt1x, rectPts.pt1y, 0, clr2));
                        verticesDos.Unlock();
                        device.SetStreamSource(0, verticesDos, 0);
                        device.VertexFormat = CustomVertex.PositionColored.Format;
                        device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 8);*/
                        #endregion
                        if (BlobSwitch && !varSender.BlobStart)
                        {
                            BlobSwitch = false;
                            presCount++;
                            //MessageBox.Show(counter.ToString());
                        }
                        if (varSender.bgCountStop == 0 || varSender.bgCountStop == 5)
                        {
                            if (counter <= varSender.counterstop)
                            {
                                R = varSender.blobSpeed * (float)counter / 60f + varSender.radStartSize; //20f;//0.173f;
                                blobRnext = R;
                                //R = R * 8; //used for power measurements
                            }
                            //if (counter >= varSender.counterstop && varSender.BlobStart)
                            //    MessageBox.Show(R.ToString());

                        }
                        else if (varSender.bgCountStop == 1)
                        {
                            if (counter <= varSender.counterstop)
                            {
                                float distcovered = varSender.stimDist - varSender.stimSpeed * (float)counter / 60f;
                                if (distcovered <= 0)
                                    distcovered = 0.000001f;
                                R = varSender.stimSize / distcovered * 4f / varSender.conversion;
                                if (R > 100f) //This stop was added on 5_14_2014!!!
                                {
                                    R = 100f;
                                    //MessageBox.Show(counter.ToString());
                                }
                                if (varSender.BlobStart && BlobSwitch)
                                {
                                    //MessageBox.Show(counter.ToString());
                                    stimFileWriter.Write(R);
                                    stimFileWriter.Write((float)stimWriteWatch.ElapsedMilliseconds / 1000f);
                                }
                                //R = 100f;
                                blobRnext = R;

                            }
                        }
                        else if (varSender.bgCountStop == 2)
                        {

                            if (counter <= 120)
                            {
                                R = varSender.radStartSize;
                                blobRnext = R;
                            }
                            else
                            {
                                R = varSender.radEndSize;
                                blobRnext = R;
                            }
                        }
                        else if (varSender.bgCountStop == 3) // DIMMING STIMS, READ DIM ARRAY
                        {
                            R = varSender.blobSpeed * (float)varSender.counterstop / 60f + varSender.radStartSize; //set stim size to the final size of looms
                            blobRnext = R;
                            //R = R * 8;
                            int ind = (int)(counter * 17);
                            if (ind >= varSender.dimTime.Length)
                                ind = varSender.dimTime.Length - 1;
                            int cLoomContrast = (int)varSender.dimTime[ind];
                            blobClr = Color.FromArgb(cLoomContrast, cLoomContrast, cLoomContrast).ToArgb();

                        }
                        else if (varSender.bgCountStop == 4) // VELO WALK, READ SIZE ARRAY
                        {
                            int ind = (int)(counter * 17);
                            if (ind >= varSender.sizeTime.Length) //this shouldn't be necessary
                                ind = varSender.sizeTime.Length - 1;
                            R = (float)varSender.sizeTime[ind] * varSender.radStartSize;
                            blobRnext = R;

                        }
                        //else
                        //{
                        //    if (counter <= varSender.counterstop)
                        //    {
                        //        R = 25f / (51f - varSender.blobSpeed * (float)counter);
                        //        //R = 1000f / (100f - varSender.blobSpeed * (float)counter);//(float)counter / varSender.blobSpeed; //20f;//0.173f;
                        //        //MessageBox.Show(R.ToString() + " " + counter.ToString() + " " + varSender.counterstop.ToString());
                        //        blobRnext = R;// (float)counter / varSender.blobSpeed;// 20f;//0.1f;
                        //        if (varSender.shrinking)
                        //        {
                        //            R = varSender.counterstop / varSender.blobSpeed - R;
                        //            blobRnext = varSender.counterstop / varSender.blobSpeed - blobRnext;
                        //        }
                        //    }
                        //}



                        if (counter <= varSender.dubStop)
                            blobRdub = (float)counter / varSender.dubSpeed;

                        if (varSender.blobFlash)
                        {
                            R = 25f;
                            blobRnext = 25f;
                        }


                        float annulusSize = R - 2f;
                        #region DIMMING STIM COMMENTED
                        /*if (counter < 116)
                        {
                            blobRnext = R * ((float)counter / 50) + R;
                            //darkendingClr = Color.FromArgb(255 - counter, 255 - counter, 255 - counter).ToArgb();
                        }
                        else
                        {
                            darkendingClr = Color.FromArgb(140, 140, 140).ToArgb();
                            blobRnext = 0.571f;
                        }
                        if (counter > 60)
                            darkendingClr = Color.FromArgb(140, 140, 140).ToArgb();*/
                        #endregion
                        float Rbg = 40f; //2f;

                        #region BGCIRCLE AND STARTSHOWFLASH
                        //FOR BG CIRCLE
                        //GraphicsStream backCirc = vertices.Lock(0, 0, 0);
                        //for (int a = 0; a < 20; a++)
                        //    backCirc.Write(new CustomVertex.PositionColored(xInMeyeA + (float)(Rbg * Math.Cos(2 * a * Math.PI / 20)), yInMeyeA + (float)(Rbg * Math.Sin(2.0 * a * Math.PI / 20)), 0, (varSender.BlobStart ? darkendingClr : clr2)));
                        //vertices.Unlock();
                        //device.SetStreamSource(0, vertices, 0);
                        //device.VertexFormat = CustomVertex.PositionColored.Format;
                        //device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                        if (varSender.StartShowFlash)
                        {
                            //leading = 100f;
                            //trailing = 100f;
                            //rectPts = new rectPoints(xInMeyeA, yInMeyeA, varSender.lockAngle + varSender.adtlAngle, varSender.adtlAngle, 10f, leading, trailing);
                            //GraphicsStream bar = approachBar.Lock(0, 0, 0);
                            //bar.Write(new CustomVertex.PositionColored(rectPts.Bpt1x, rectPts.Bpt1y, 0, blobClr));
                            //bar.Write(new CustomVertex.PositionColored(rectPts.Bpt2x, rectPts.Bpt2y, 0, blobClr));
                            //bar.Write(new CustomVertex.PositionColored(rectPts.Bpt3x, rectPts.Bpt3y, 0, blobClr));
                            //bar.Write(new CustomVertex.PositionColored(rectPts.Bpt4x, rectPts.Bpt4y, 0, blobClr));

                            //approachBar.Unlock();


                            //device.SetStreamSource(0, approachBar, 0);
                            //device.VertexFormat = CustomVertex.PositionColored.Format;
                            //device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
                            R = 25f;
                            blobRnext = 25f;
                            yBlob = yInMeyeA - 20f /*- ((float)counter) / varSender.blobSpeed)*/ * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle) +
                                varSender.blobDist * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle);
                            xBlob = xInMeyeA + 20f /*- ((float)counter) / varSender.blobSpeed)*/ * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle) +
                                varSender.blobDist * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle);

                            GraphicsStream blob = bgCircle.Lock(0, 0, 0);
                            if (counter % 4 == 0)
                                for (int a = 0; a < 20; a++)
                                    blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(R * Math.Sin(2 * a * Math.PI / 20)), 0, blobClr));
                            else
                                for (int a = 0; a < 20; a++)
                                    blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(blobRnext * Math.Sin(2 * a * Math.PI / 20)), 0, blobClr));
                            bgCircle.Unlock();
                            // device.Transform.World = Matrix.Translation(-lockX, -lockY, 0) * Matrix.RotationZ((float)Math.PI / 6) * Matrix.Translation(lockX, lockY, 0);

                            device.SetStreamSource(0, bgCircle, 0);
                            device.VertexFormat = CustomVertex.PositionColored.Format;
                            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                        }
                        #endregion

                        // NEW OMR STIMULUS PRESENTATION 10/25/12
                        #region OMR STIMULUS
                        if (varSender.BlobStart && varSender.OMRtrial)
                        {



                            //if (!OMRstartswitch)
                            //{
                            //gratingwatch.Stop();
                            //gratingwatch.Reset();
                            //gratingwatch.Start();
                            //OMRstartswitch = true;
                            //}

                            #region shitty gratins
                            //Old, shitty code for OMR gratings locked to fish orientation. Because it seems impossible to find
                            //The center of the device projection ro world, it becomes very difficult to rotate any object in a useful way
                            //Also, this stimulus flickered as it reached the end of the 'advance' loop
                            //float projAdvance = (float)OMRwatch.ElapsedMilliseconds + 50;
                            //if ((projAdvance + 53 * widP / 72) >= widP)// * Math.Sin(varSender.rotAngle))
                            //{
                            //    OMRwatch.Stop();
                            //    OMRwatch.Reset();
                            //    OMRwatch.Start();
                            //}
                            //device.Transform.Projection = Matrix.OrthoOffCenterLH(0, playingW, projAdvance + widP / 8, projAdvance + widP / 2, 0, 10) *
                            //     Matrix.RotationZ(gratingangle);
                            //stm.Write(new CustomVertex.PositionColoredTextured(0, 0, 0, bgclr, projAdvance+0.5f, 1));
                            //stm.Write(new CustomVertex.PositionColoredTextured(0, 2 * playingH, 0, bgclr, projAdvance, 1));
                            //stm.Write(new CustomVertex.PositionColoredTextured(2 * widP, 2 * playingH, 0, bgclr, projAdvance, 0));
                            //stm.Write(new CustomVertex.PositionColoredTextured(2 * widP, 0, 0, bgclr, projAdvance+0.5f, 0));
                            # endregion

                            if (!OMRwatch.IsRunning)
                            {
                                OMRwatch.Stop();
                                OMRwatch.Reset();
                                OMRwatch.Start();
                            }

                            float projAdvance = (float)OMRwatch.ElapsedMilliseconds;
                            projAdvance = projAdvance / 1000/40;

                            gratingangle = varSender.lockAngle + varSender.adtlAngle;

                            //The simplest way to make a monocular OMR stimulus is to draw a rectangle using my old rect class. This rectangle
                            //can be drawn at an arbitrary rotation angle. Then, a sine wave texture is mapped onto the rectangle using u,v
                            //coordinates. u,v coordinates wrap around to 1 (i.e. modulo 1) and effectively move the grating texture as the x
                            //coordimate is changed. To speed up or slow down the gratings, fiddel with the denominator in the projAdvance =
                            //assignment above


                            leading = 500f; //How far away from the initial position one edge of the rectangle is
                            trailing = 500f; //How far away from the initial position the other edge of the rectangle is
                            rectPts = new rectPoints(xInMeyeA, yInMeyeA, varSender.lockAngle + varSender.adtlAngle
                                , varSender.adtlAngle, -10f*(float)Math.Cos(varSender.adtlAngle), leading, trailing);

                            GraphicsStream stm = BackgroundTexture.Lock(0, 0, 0);

                            stm.Write(new CustomVertex.PositionColoredTextured(rectPts.Bpt1x, rectPts.Bpt1y, 0, bgclr, projAdvance - 0.5f, 1));
                            stm.Write(new CustomVertex.PositionColoredTextured(rectPts.Bpt2x, rectPts.Bpt2y, 0, bgclr, projAdvance, 1));
                            stm.Write(new CustomVertex.PositionColoredTextured(rectPts.Bpt3x, rectPts.Bpt3y, 0, bgclr, projAdvance, 0));
                            stm.Write(new CustomVertex.PositionColoredTextured(rectPts.Bpt4x, rectPts.Bpt4y, 0,bgclr,projAdvance - 0.5f, 0));
                       
                            BackgroundTexture.Unlock();


                            device.SetTexture(0, bgTexture);
                            device.SetStreamSource(0, BackgroundTexture, 0);
                            device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
                            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
                            device.SetTexture(0, null);


                        }
                        #endregion

                        if (varSender.BlobStart && !varSender.OMRtrial)
                        {

                            //device.Clear(ClearFlags.Target, System.Drawing.Color.DarkRed, 1.0f, 0);
                            if (!BlobSwitch)
                            {
                                R = 0f;
                                blobRnext = 0f;
                                counter = 0;
                                blobRdub = 0;
                                BlobSwitch = true;
                                stimFileWriter.Write(float.PositiveInfinity);
                                stimFileWriter.Write((float)varSender.stimSpeed);
                                //stimWriteWatch.Stop();
                                //stimWriteWatch.Reset();
                                //stimWriteWatch.Start();
                            }

                            if (counter > varSender.stimLength)
                            {
                                R = 0;
                                blobRnext = 0;
                                blobRdub = 0;

                            }

                            //R = 50f;
                            //blobRnext = 50f;

                            //else
                            //{
                            yBlob = yInMeyeA - (varSender.approachDist) /*- ((float)counter) / varSender.blobSpeed)*/ * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle) +
                                varSender.blobDist * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle);
                            xBlob = xInMeyeA + (varSender.approachDist) /*- ((float)counter) / varSender.blobSpeed)*/ * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle) +
                                    varSender.blobDist * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle);
                            //MessageBox.Show("here");

                            if (varSender.loomSquare)
                            {

                                switch (varSender.SquareCondition)
                                {

                                    case 0:
                                        if (counter > 300)
                                            R = 25f;
                                        leading = 10f;// -10f + 2 * R;
                                        trailing = 10f;// 30f - 2 * R;
                                        break;
                                    case 1:
                                        if (counter > 300)
                                            R = 25f;
                                        leading = 5f;// 30f - 2 * R;
                                        trailing = 5f;// -10f + 2 * R;
                                        break;
                                    case 2:
                                        leading = R / 2;
                                        trailing = R / 2;
                                        break;
                                    case 3:
                                        if (counter > 300)
                                            R = 25f;
                                        leading = 0f - 0.5f * R;// 12.5f - R / 2;
                                        trailing = 20f + 0.5f * R;// 12.5f - R / 2;
                                        break;

                                }
                                rectPts = new rectPoints(xInMeyeA, yInMeyeA, varSender.lockAngle + varSender.adtlAngle, varSender.adtlAngle, varSender.approachDist, leading, trailing);
                                GraphicsStream bar = approachBar.Lock(0, 0, 0);
                                bar.Write(new CustomVertex.PositionColored(rectPts.Bpt1x, rectPts.Bpt1y, 0, blobClr));
                                bar.Write(new CustomVertex.PositionColored(rectPts.Bpt2x, rectPts.Bpt2y, 0, blobClr));
                                bar.Write(new CustomVertex.PositionColored(rectPts.Bpt3x, rectPts.Bpt3y, 0, blobClr));
                                bar.Write(new CustomVertex.PositionColored(rectPts.Bpt4x, rectPts.Bpt4y, 0, blobClr));

                                approachBar.Unlock();


                                device.SetStreamSource(0, approachBar, 0);
                                device.VertexFormat = CustomVertex.PositionColored.Format;
                                device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
                            }
                            //MessageBox.Show(xBlob.ToString() + " " + yBlob.ToString() + " " + varSender.lockAngle.ToString() + " " + xInMeyeA.ToString() + " " + yInMeyeA.ToString());
                            else
                            {

                                if (varSender.halfBlob)
                                {
                                    GraphicsStream blob = bgCircle.Lock(0, 0, 0);
                                    if (counter % 4 == 0)
                                        for (int a = 0; a < 20; a++)
                                            blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(-a * Math.PI / 20 + varSender.halfAngle + varSender.lockAngle)), yBlob + (float)(R * Math.Sin(-a * Math.PI / 20 + varSender.halfAngle + varSender.lockAngle)), 0, blobClr));
                                    else
                                        for (int a = 0; a < 20; a++)
                                            blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(-a * Math.PI / 20 + varSender.halfAngle + varSender.lockAngle)), yBlob + (float)(blobRnext * Math.Sin(-a * Math.PI / 20 + varSender.halfAngle + varSender.lockAngle)), 0, blobClr));
                                }
                                else
                                {

                                    if (varSender.bgCountStop == 3)
                                    { //need to use a different set of graphics objects for dimming
                                        GraphicsStream gs2 = vertices.Lock(0, 0, 0);
                                        for (int a = 0; a < 20; a++)
                                            gs2.Write(new CustomVertex.PositionColored(xBlob + (float)(R * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(R * Math.Sin(2.0 * a * Math.PI / 20)), 0, blobClr));
                                        vertices.Unlock();
                                        device.SetStreamSource(0, vertices, 0);
                                        device.VertexFormat = CustomVertex.PositionColored.Format;
                                        device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);

                                    }
                                    else
                                    {
                                        GraphicsStream gs2 = vertices3.Lock(0, 0, 0);     // Lock the vertex list


                                        for (int a = 0; a < 20; a++)
                                            gs2.Write(new CustomVertex.PositionTextured(xBlob + (float)(R * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(R * Math.Sin(2.0 * a * Math.PI / 20)), 0, (float)(Math.Cos(2 * a * Math.PI / 20)) / 2 + 0.5f, (float)(Math.Sin(2.0 * a * Math.PI / 20)) / 2 + 0.5f));

                                        vertices3.Unlock();

                                        device.SetStreamSource(0, vertices3, 0);
                                        if (varSender.bgCountStop == 0 || varSender.bgCountStop == 4)
                                            device.SetTexture(0, black);
                                        else
                                            device.SetTexture(0, black);
                                        device.VertexFormat = CustomVertex.PositionTextured.Format;
                                        device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                                        device.SetTexture(0, null);
                                    }
                                    //GraphicsStream blob = bgCircle.Lock(0, 0, 0);
                                    //if (counter % 4 == 0)
                                    //    for (int a = 0; a < 20; a++)
                                    //        blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(2*a * Math.PI / 20)), yBlob + (float)(R * Math.Sin(2*a * Math.PI / 20)), 0, blobClr));
                                    //else
                                    //    for (int a = 0; a < 20; a++)
                                    //        blob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRnext * Math.Cos(2*a * Math.PI / 20)), yBlob + (float)(blobRnext * Math.Sin(2*a * Math.PI / 20)), 0, blobClr));
                                }

                                //bgCircle.Unlock();
                                //// device.Transform.World = Matrix.Translation(-lockX, -lockY, 0) * Matrix.RotationZ((float)Math.PI / 6) * Matrix.Translation(lockX, lockY, 0);

                                //device.SetStreamSource(0, bgCircle, 0);
                                //device.VertexFormat = CustomVertex.PositionColored.Format;
                                //device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                                //MessageBox.Show(xBlob.ToString());
                                //device.EndScene();
                                //device.Present();
                                //}
                                #region OBSTACLE
                                if (varSender.obstacle)
                                {
                                    leading = 100f;
                                    trailing = 100f;
                                    rectPts = new rectPoints(xInMeyeA, yInMeyeA, varSender.lockAngle + 3 * (float)Math.PI / 2, 3 * (float)Math.PI / 2, 100f, leading, trailing);
                                    GraphicsStream bar = approachBar.Lock(0, 0, 0);
                                    bar.Write(new CustomVertex.PositionColored(rectPts.Bpt1x, rectPts.Bpt1y, 0, blobClr));
                                    bar.Write(new CustomVertex.PositionColored(rectPts.Bpt2x, rectPts.Bpt2y, 0, blobClr));
                                    bar.Write(new CustomVertex.PositionColored(rectPts.Bpt3x, rectPts.Bpt3y, 0, blobClr));
                                    bar.Write(new CustomVertex.PositionColored(rectPts.Bpt4x, rectPts.Bpt4y, 0, blobClr));

                                    approachBar.Unlock();


                                    device.SetStreamSource(0, approachBar, 0);
                                    device.VertexFormat = CustomVertex.PositionColored.Format;
                                    device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
                                }
                                #endregion
                                #region ANNULUS
                                if (varSender.annulus)
                                {
                                    GraphicsStream annularBlob = annulizer.Lock(0, 0, 0);

                                    for (int a = 0; a < 20; a++)
                                        annularBlob.Write(new CustomVertex.PositionColored(xBlob + (float)(annulusSize * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(annulusSize * Math.Sin(2 * a * Math.PI / 20)), 0, System.Drawing.Color.Gray.ToArgb()));
                                    annulizer.Unlock();
                                    // device.Transform.World = Matrix.Translation(-lockX, -lockY, 0) * Matrix.RotationZ((float)Math.PI / 6) * Matrix.Translation(lockX, lockY, 0);

                                    device.SetStreamSource(0, annulizer, 0);
                                    device.VertexFormat = CustomVertex.PositionColored.Format;
                                    device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);

                                }
                                #endregion
                                #region DOUBLE BLOB
                                if (varSender.doubleBlob)
                                {
                                    //GraphicsStream gsDos = verticesDos.Lock(0, 0, 0);
                                    //gsDos.Write(new CustomVertex.PositionColored(xInMeyeA, yInMeyeA, 0, blobClr));
                                    //gsDos.Write(new CustomVertex.PositionColored(xInMeyeA, yInMeyeA + 20f, 0, blobClr));
                                    //gsDos.Write(new CustomVertex.PositionColored(xInMeyeA + 20f, yInMeyeA + 20f, 0, blobClr));

                                    //verticesDos.Unlock();
                                    //device.SetStreamSource(0, verticesDos, 0);
                                    //device.VertexFormat = CustomVertex.PositionColored.Format;
                                    //device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                                    // device.BeginScene();
                                    //// MessageBox.Show("here");

                                    GraphicsStream Dblob = vertices.Lock(0, 0, 0);
                                    //blob = bgCircle.Lock(0, 0, 0);
                                    yBlob = yInMeyeA - (varSender.approachDist) /*- ((float)counter) / varSender.blobSpeed)*/ * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle + Math.PI) +
                                    varSender.blobDist * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle + Math.PI);
                                    xBlob = xInMeyeA + (varSender.approachDist) /*- ((float)counter) / varSender.blobSpeed)*/ * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle + Math.PI) +
                                        varSender.blobDist * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle + Math.PI);

                                    for (int a = 0; a < 20; a++)
                                        Dblob.Write(new CustomVertex.PositionColored(xBlob + (float)(blobRdub * Math.Cos(2 * a * Math.PI / 20)), yBlob + (float)(blobRdub * Math.Sin(2.0 * a * Math.PI / 20)), 0, blobClr));

                                    vertices.Unlock();
                                    device.SetStreamSource(0, vertices, 0);
                                    device.VertexFormat = CustomVertex.PositionColored.Format;
                                    device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                                    // device.EndScene();
                                    //device.Present();
                                }
                            }

                            //bgCircle.Unlock();
                            //// device.Transform.World = Matrix.Translation(-lockX, -lockY, 0) * Matrix.RotationZ((float)Math.PI / 6) * Matrix.Translation(lockX, lockY, 0);

                            //device.SetStreamSource(0, bgCircle, 0);
                            //device.VertexFormat = CustomVertex.PositionColored.Format;
                            //device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                        }
                                #endregion
                        #region HALFMOON
                        if (varSender.HalfMoonStart)
                        {
                            device.BeginScene();
                            xHalfMoonBox = xInMeyeA + varSender.approachDist * (float)Math.Sin(varSender.lockAngle + varSender.adtlAngle);
                            yHalfMoonBox = yInMeyeA - varSender.approachDist * (float)Math.Cos(varSender.lockAngle + varSender.adtlAngle);

                            GraphicsStream halfMoonBox = halfMoon.Lock(0, 0, 0);
                            for (int a = 0; a < 20; a++)
                                halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox + (float)(0.5f * Math.Cos(2 * a * Math.PI / 20)), yHalfMoonBox + (float)(0.5f * Math.Sin(2.0 * a * Math.PI / 20)), 0, clr2));
                            halfMoon.Unlock();

                            device.SetStreamSource(0, halfMoon, 0);
                            device.VertexFormat = CustomVertex.PositionColored.Format;
                            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                            device.EndScene();
                            device.Present();
                            /*GraphicsStream halfMoonBox = verticesDos.Lock(0, 0, 0);
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox, yHalfMoonBox, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox, yHalfMoonBox + 0.5f, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox + 0.5f, yHalfMoonBox + 0.5f, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox + 0.5f, yHalfMoonBox, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox + 0.5f, yHalfMoonBox - 0.5f, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox, yHalfMoonBox - 0.5f, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox - 0.5f, yHalfMoonBox - 0.5f, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox - 0.5f, yHalfMoonBox, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox - 0.5f, yHalfMoonBox + 0.5f, 0, clr2));
                            halfMoonBox.Write(new CustomVertex.PositionColored(xHalfMoonBox, yHalfMoonBox + 0.5f, 0, clr2));
                            verticesDos.Unlock();
                            device.SetStreamSource(0, verticesDos, 0);
                            device.VertexFormat = CustomVertex.PositionColored.Format;
                            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 8);*/
                        }
                        #endregion

                        device.EndScene();
                        device.Present();
                        long wtf = checkmewatch.ElapsedMilliseconds;
                        SetTitle(wtf.ToString());
                        checkmewatch.Stop();
                        checkmewatch.Reset();
                        checkmewatch.Start();

                    }
                }

                #region OldCode_commented
                //Think this was written to have triangles at every point of the fish displayed
                ///* float newYeyeA = 1024 - varSender.fishCoords[0,1];
                //  float xInMeyeA = (float)varSender.fishCoords[1,0] / 100;// *6 / 1280;
                //  float yInMeyeA = newYeyeA / 100;// *4 / 1024;
                //  float Rbg = 0.5f;

                //  device.BeginScene();
                //  device.Clear(ClearFlags.Target, System.Drawing.Color.White, 1.0f, 0);
                //  GraphicsStream bg = bgCircle.Lock(0, 0, 0);
                //  for (int a = 0; a < 20; a++)
                //      bg.Write(new CustomVertex.PositionColored(xInMeyeA + (float)(Rbg * Math.Cos(2 * a * Math.PI / 20)), yInMeyeA + (float)(Rbg * Math.Sin(2.0 * a * Math.PI / 20)), 0, clr));
                //  bgCircle.Unlock();

                //  device.SetStreamSource(0, bgCircle, 0);
                //  device.VertexFormat = CustomVertex.PositionColored.Format;
                //  device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);
                //  device.EndScene();
                //  device.Present();*/
                ///* GraphicsStream bg = bgCircle.Lock(0, 0, 0);
                //     for (int a = 0; a < 20; a++)
                //         bg.Write(new CustomVertex.PositionColored(xMidPoint + (float)(1 * Math.Cos(2 * a * Math.PI / 20)), yMidPoint + (float)(1 * Math.Sin(2.0 * a * Math.PI / 20)), 0, clr));
                // bgCircle.Unlock();

                // device.SetStreamSource(0, bgCircle, 0);
                // device.VertexFormat = CustomVertex.PositionColored.Format;

                //  PrimitiveType.TriangleFan draws triangles, always starting at some base vertex (here 0), so the triangles touch vertices 0,1,2, then 0,3,4, etc.
                //  To draw a 20-gon, need 18 triangles, hence the last argument.

                // device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 18);*/

                ///*gs.Write(new CustomVertex.PositionColored(2, 2, 0, clr));
                //gs.Write(new CustomVertex.PositionColored(2, 4, 0, clr));
                //gs.Write(new CustomVertex.PositionColored(4, 4, 0, clr));

                //gs.Write(new CustomVertex.PositionColored(xInMeyeA, yInMeyeA, 0, clr));
                //gs.Write(new CustomVertex.PositionColored(xInMeyeA, yInMeyeA +0.1f, 0, clr));
                //gs.Write(new CustomVertex.PositionColored(xInMeyeA +0.1f, yInMeyeA +0.1f, 0, clr));

                //vertices.Unlock();
                //device.SetStreamSource(0, vertices, 0);
                //device.VertexFormat = CustomVertex.PositionColored.Format;
                //device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);/*

                //GraphicsStream gsDos = verticesDos.Lock(0, 0, 0);
                //gsDos.Write(new CustomVertex.PositionColored(xInMeyeB, yInMeyeB, 0, clr2));
                //gsDos.Write(new CustomVertex.PositionColored(xInMeyeB, yInMeyeB + 0.1f, 0, clr2));
                //gsDos.Write(new CustomVertex.PositionColored(xInMeyeB + 0.1f, yInMeyeB + 0.1f, 0, clr2));

                //verticesDos.Unlock();
                //device.SetStreamSource(0, verticesDos, 0);
                //device.VertexFormat = CustomVertex.PositionColored.Format;
                //device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);

                //GraphicsStream gsTres = verticesTres.Lock(0, 0, 0);
                //gsTres.Write(new CustomVertex.PositionColored(xInMtail, yInMtail, 0, clr3));
                //gsTres.Write(new CustomVertex.PositionColored(xInMtail, yInMtail + 0.1f, 0, clr3));
                //gsTres.Write(new CustomVertex.PositionColored(xInMtail + 0.1f, yInMtail + 0.1f, 0, clr3));

                //verticesTres.Unlock();
                //device.SetStreamSource(0, verticesTres, 0);
                //device.VertexFormat = CustomVertex.PositionColored.Format;
                //device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);*/

                //End the scene 
                #endregion
                counter++;
                //if (presCount > 20)
                //        dataStimFile.Close();
                //return blobPassing;
            }




        }
        
        //this is necessary in order to change the window location after it's been created. 
        private void SetTitle(string newtitle)
        {
            if (this.InvokeRequired)
            {
                SetTitleCallback d = new SetTitleCallback(SetTitle);
                this.Invoke(d, new object[] { newtitle });
            }
            else
                this.Text = newtitle;
        }

        private void SetLocationX(int locSetX)
        {
            if (this.InvokeRequired)
            {
                SetLocationXCallback d = new SetLocationXCallback(SetLocationX);
                 this.Invoke(d, new object[] { locSetX });
            }
            else
                this.Left = locSetX;
        }

        private void SetLocationY(int locSetY)
        {
            if (this.InvokeRequired)
            {
                SetLocationYCallback d = new SetLocationYCallback(SetLocationY);
                
                
                this.Invoke(d, new object[] { locSetY });
            }
            else
                this.Top = locSetY;
        }

        private void SetSizeHoriz(int horizSet)
        {
            if (this.InvokeRequired)
            {
                SetSizeHorizCallback d = new SetSizeHorizCallback(SetSizeHoriz);
                this.Invoke(d, new object[] { horizSet });
            }
            else
                this.Width = horizSet;
        }

        private void SetSizeVert(int vertSize)
        {
            if (this.InvokeRequired)
            {
                SetSizeVertCallback d = new SetSizeVertCallback(SetSizeVert);
                this.Invoke(d, new object[] { vertSize });
            }
            else
                this.Height = vertSize;
        }



       

    }

    class rectPoints
    {
        public float pt1x, pt1y, pt2x, pt2y, pt3x, pt3y, pt4x, pt4y, Bpt1x, Bpt1y, Bpt2x, Bpt2y, Bpt3x, Bpt3y, Bpt4x, Bpt4y, startX, startY;//, pt5x, pt5y, pt6x, pt6y, pt7x, pt7y, pt8x, pt8y;
        public rectPoints(float initX, float initY, float angle, float staticAngle, float dist, float leadEdge, float trailEdge)
        {
            startY = initY - dist  * (float)Math.Cos(angle + staticAngle) + (float)Math.Sin(angle + staticAngle);
            startX = initX + dist  * (float)Math.Sin(angle + staticAngle) + (float)Math.Cos(angle + staticAngle);
            pt1y = (startY - trailEdge);
            pt1x = startX;
            pt2y = (startY - trailEdge);// -dist * (float)Math.Cos(angle + staticAngle) + (float)Math.Sin(angle + staticAngle);
            pt2x = (startX + 1000f);// 20f //+dist * (float)Math.Sin(angle + staticAngle) + (float)Math.Cos(angle + staticAngle);
            pt3y = (startY + leadEdge);// -dist * (float)Math.Cos(angle + staticAngle) + (float)Math.Sin(angle + staticAngle);
            pt3x = (startX + 1000f);// 20f //+dist * (float)Math.Sin(angle + staticAngle) + (float)Math.Cos(angle + staticAngle);
            pt4y = (startY + leadEdge);// -dist * (float)Math.Cos(angle + staticAngle) + (float)Math.Sin(angle + staticAngle);
            pt4x = startX;// +dist; *(float)Math.Sin(angle + staticAngle) + (float)Math.Cos(angle + staticAngle);

            angle = angle + (float)Math.PI / 2;
            Bpt1x = (float)((pt1x - startX) * Math.Cos(angle) - (pt1y - startY) * Math.Sin(angle) + startX);
            Bpt1y = (float)((pt1x - startX) * Math.Sin(angle) + (pt1y - startY) * Math.Cos(angle) + startY);
            Bpt2x = (float)((pt2x - startX) * Math.Cos(angle) - (pt2y - startY) * Math.Sin(angle) + startX);
            Bpt2y = (float)((pt2x - startX) * Math.Sin(angle) + (pt2y - startY) * Math.Cos(angle) + startY);
            Bpt3x = (float)((pt3x - startX) * Math.Cos(angle) - (pt3y - startY) * Math.Sin(angle) + startX);
            Bpt3y = (float)((pt3x - startX) * Math.Sin(angle) + (pt3y - startY) * Math.Cos(angle) + startY);
            Bpt4x = (float)((pt4x - startX) * Math.Cos(angle) - (pt4y - startY) * Math.Sin(angle) + startX);
            Bpt4y = (float)((pt4x - startX) * Math.Sin(angle) + (pt4y - startY) * Math.Cos(angle) + startY);
            //Bpt1x = initX;
            //Bpt1y = initY - trailEdge;
            //Bpt2x = initX + 20f;
            //Bpt2y = initY - trailEdge;
            //Bpt3x = initX + 20f;
            //Bpt3y = initY + leadEdge;
            //Bpt4x = initX;
            //Bpt4y = initY + leadEdge;
            //pt1x = (float)(Bpt1x * Math.Cos(angle) - Bpt1y * Math.Sin(angle));
            //pt1y = (float)(Bpt1x * Math.Cos(angle) + Bpt1y * Math.Sin(angle));
            //pt2x = (float)(Bpt2x * Math.Cos(angle) - Bpt2y * Math.Sin(angle));
            //pt2y = (float)(Bpt2x * Math.Cos(angle) + Bpt2y * Math.Sin(angle));
            //pt3x = (float)(Bpt3x * Math.Cos(angle) - Bpt3y * Math.Sin(angle));
            //pt3y = (float)(Bpt3x * Math.Cos(angle) + Bpt3y * Math.Sin(angle));
            //pt4x = (float)(Bpt4x * Math.Cos(angle) - Bpt4y * Math.Sin(angle));
            //pt4y = (float)(Bpt4x * Math.Cos(angle) + Bpt4y * Math.Sin(angle));
            //pt5x = (float)(2f * Math.Sin(angle)) + initX;
            //pt5y = (float)(-2f * Math.Cos(angle)) + initY;
            //pt6x = (float)(-1f * Math.Cos(angle) +2f * Math.Sin(angle)) + initX;
            //pt6y = (float)(-1f * Math.Sin(angle) - 2f* Math.Cos(angle)) + initY;
            //pt7x = (float)(-1f * Math.Cos(angle)) + initX;
            //pt7y = (float)(-1f * Math.Sin(angle)) + initY;
            //pt8x = (float)(-1f * Math.Cos(angle) - 2f* Math.Sin(angle)) + initX;
            //pt8y = (float)(-1f * Math.Sin(angle) + 2f* Math.Cos(angle)) + initY;
            /*MessageBox.Show(pt1x.ToString() + " " + pt1y.ToString() + " " + pt2x.ToString() + " " +
                pt2y.ToString() + " " + pt3x.ToString() + " " + pt3y.ToString() + " " + pt4x.ToString() + " " +
                pt4y.ToString() + " " + pt5x.ToString() + " " + pt5y.ToString() + " " + pt6x.ToString() + " " +
                pt6y.ToString() + " " + pt7x.ToString() + " " + pt7y.ToString() + " " + pt8x.ToString() + " " +
                pt8y.ToString());*/
        }
    }
}
