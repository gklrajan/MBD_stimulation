using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
//using NationalInstruments;
using NationalInstruments.Vision;
using NationalInstruments.Vision.Acquisition.Imaq;

using System.Threading;
using System.IO;
//using NationalInstruments.DAQmx;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using ActiveSilicon;

namespace Grab
{

     public partial class GrabAndStimAS_PHX_2016 : Form
    {



        private ImaqSession _session = null;
        
        private System.ComponentModel.BackgroundWorker acquisitionWorker;
        // Sets the parameters for the circle used in the image search.
        // H, K are the coordinates of the circle center. H is x width, K is y height. R is the radius in pixels
        const int circH = 512; 
        const int circK = 512;
        const int circR = 225;

        // I don't think these are used
        // -----
        const int ellH = 320; 
        const int ellK = 240;
        const int ellMX = 300;
        const int ellMY = 220;
        const int sqX = 320;
        const int sqY = 240;
        const int sqH = 220;
        const int sqW = 270;
        const int eyeR = 4; //Radius used to search for the second eye.
        // -----

        // Sets the x width start and end of the circle search.
        const int arrayWidth = 1024;
        const int arrayHeight = 1024;

        // I don't think these are used
        // -----
        int blankPixFront;
        int blankPixBack;
        int blankPixTop;
        int blankPixBottom;
        // -------

        //Initializes eye and tail coordinates to an arbitrary position outside of the search radius 
        //but far enough away from array borders that first pixel searches do not throw indexing exceptions.
        int coordEyeAX = 30, coordEyeAY = 30, coordEyeBX = 25, coordEyeBY = 25;
        
        // I don't think these are used
        // -----    
        //   coordTailX = 25, coordTailY = 25, midX = 25, midY = 25, midHolderX = 25, midHolderY = 25;
        // ------

        uint grabNumberDisp; //Counts number of successfully grabbed images within the base acquisition worker. 
                             // This depends on the speed of DirectX scene display, i.e. maxes out at 60Hz.
        public int bgCountStop;

        // This is for the CamDisplay window
        Thread displayCam = null;

        // Poorly named declarations fr each type of display window
        CamDisplay viewerForm = null;
        DebugWin debugger = null;


        // These wre for NI image handling -- you might not need these now
        PixelValue2D extractedPixels = null;
        VisionImage plzWrk = null;

        // These are used for writing to disk
        bool WriteFull = false;
        FileStream dataFullFrame;
        BinaryWriter fullFrameWriter;
        bool writeTrig = false;

        // This is used for "velocity shut off"
        //  Will turn off stimulus when fish velocity rises above set threshold
        float[] velocity = new float[5];
        float veloOUT = 0;

        // I don't what this does
        int compareBlobTime;
        
        // Some sort of binary threshold
        int binthresh;
        
        // Various triggers that tell the program "write to file now", "display stimulus now," etc..
        bool debugON = true;
        public bool TryStim = false;
        public bool WriteFile = false;
        public bool StartShow = false;

        // This is the orientation of the fish
        private float rotAngle;

        //  This was used for a manual dust removal procedure
        List<int> eraser = new List<int>();
        
        // This is the main image variable 
        byte[,] subtractedArray = null;

        // I don't know what this does
        bool ResetInit = true;
        public int BlobLeftRight = 0;

        // I'm pretty sure this is for image buffer transfers
        byte[] IMrow = new byte[arrayWidth * arrayHeight];
        int fullSize = arrayWidth * arrayHeight; 

        // OpenCV stuff for image processing
        Matrix<Byte> IM_mat = new Matrix<Byte>(40,40);
        MCvMoments COM = new MCvMoments();
        Image<Gray, Byte> IMbuff;
        
        //For loading in dimming timecourse file
        string dimFileName = "C:\\Users\\twd\\data\\projector calibration\\loaddimtime_small.bin";
        FileStream dimFile;
        BinaryReader dimFileReader;
        public double[] dimTime; //let varSender have access in the render loop

        //For loading in size over time based on randomly generated velocity walks in matlab -- this should be pre-generated before every
        //new experimental block
        string sizeFileName = "C:\\Users\\twd\\data\\LOOM PAPER CODE\\FIGURE1\\3Apr14_sizetime2.bin";
        FileStream sizeFile;
        BinaryReader sizeFileReader;
        public double[] sizeTime;
        double[] allSize;
        int currSizeInd = 0;

        //these are set pixel value thresholds for detecting when the fish has entered the "live" area of the dish.
        //bgVar1: background pixel noise threshold used during entire FOV background subtraction
        //bgVar2: background pixel noise threshold used during "Fish box" background subtraction. More restrictive than bgVar1
        //bgVar3: upper limit background subtraction threshold
        //bgVar4: value that triggers an exit from "fish box" mode.
        private int bgVar1 = 0, bgVar2 =0, bgVar3 = 0, bgVar4 = 0;

        //  This is also fish orientation -- but technically the orientation fed to the stimulus display window
        public float lockAngle = 0;

        //These coordinates are for the center of the stimulus screen
        public int lockX = circH, lockY = circK; //lockX = 655, lockY = 502; //

        // These are stimulus parameters eventually set by the config. window/file
        public float blobSpeed = 4f;
        public float dubSpeed = 4f;
        public int dubStop = 100;
        public int counterstop = 100;
        public bool StimOff;
        public bool BlobStart = false;
        public bool HalfMoonStart = false;
        public bool blobFlash = false;
        public int bgCount;
        public float adtlAngle = 0;
        public int contrastValue = 0; 
        public float blobDist = 0;
        public float approachDist = 2f;
        public float rectangleAngle = 0;
        public bool downTime = false;

        //Set initial values of Stim window properties here

        public int locationXsender = 3030; //X COORDINATE OF WINDOW 4351;//for power measurements 2680
        public int locationYsender = 500; //Y COORDINATE OF WINDOW 384;//for power measurements 338
        public int sizeHorizsender = 484;//120;// for power measurements
        public int sizeVertsender = 513;//100;// for power measurements
        public float scaleFactor = 0.9570f; //Y BLOB SCALING
        public float scaleFactorX = 0.969f; //X BLOB SCALING
        public float aspRatioSender = 1.060f;
        public float anglechoice = 3.14f;

        // END WINDOW PROPERTIES

        //  These are used to start different types of stimulus trials
        public VisionImage newestImage = new VisionImage(ImageType.U8);
        public bool cLoom = false;
        public bool doubleBlob = false;
        public bool halfBlob = false;
        public float halfAngle = 0f;
        public bool annulus = false;
        public bool shrinking = false;
        public bool loomSquare = false;
        public int SquareCondition = 0;
        public bool obstacle = false;
        public bool StartShowFlash = false;
        public bool OMRtrial = false;
        public int stimLength = 0;
        public float radEndSize = 0;
        public float radStartSize = 0;

        public int millitime;

        public float conversion = 0;
        public float stimSize = 0, stimDist = 0, stimSpeed = 0;

        int BGtime = 0;

        // These are flags that are indeed used
        // foundfish tells program whether to zoom in or not
        bool foundfish = false;

        // Used to trigger background collection
        bool BGsnap = false;


        //For Blob calibration 
        //public int calBlobX = 655;
        //public int calBlobY = 502;

                    // frame # check init
        public uint LastFrame = 0;

        //Other common fps and frame # check init
        public uint Realfps = 0;
        public int Calfps = 0;
        public uint FrameCheck = 0;

        public uint RealFrameCount = 0;
        public uint RealTimeCounter = 0;
        uint LEDStatus = 0;
        double LED_intensity = 0;


        // USed for various timing tasks
        private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch stopwatch2 = new System.Diagnostics.Stopwatch();
        //private System.Diagnostics.Stopwatch stopwatch3 = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch BlobDelay = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch fpswatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch avgAngle = new System.Diagnostics.Stopwatch();

        private System.Diagnostics.Stopwatch retakeBG = new System.Diagnostics.Stopwatch();

        public System.Diagnostics.Stopwatch Gtimer = new System.Diagnostics.Stopwatch();


        // This is used for a BG collection progress bar
        delegate void updateProgBarCallback(int progSet);

        //  This is used for displaying OMR gratings I think
        delegate void GratingsOnCallback(bool OMRstate);

        // Attempt to change fifobuffer from another thread
        delegate void BufferUpdateCallback(bool update);

        // This is more file saving stuff
        FileStream dataFile;
        BinaryWriter dataFileWriter;
        FileStream BGFile;
        BinaryWriter BGFileWriter;

        //    StreamWriter dataFileWriter;
        public StringBuilder csv = new StringBuilder();

        // Don't know what these do
        IntPtr test = new IntPtr();
        IntPtr test2 = new IntPtr();
        double Sxrep, Syrep;
        bool StartShowFlashToggle = false;
        bool zoom = true;

        //  This tells the program whether a BG has been collected or not
        bool BGtaken = false;

        // I don't think these are used
        bool showFore = false;
        bool showBack = false;

        //  This was for NI framegrabber init
        private ImaqBufferCollection bufList;

        // For trial configuration window/files
        TrialConfig ConfigWindow = null;
        Parameters[] sParams = null;
        Random conditionsNum = new Random();
        int conditions = 0;


        bool bgGenLoop = true;
        bool FirstAfterStart = true;
        uint blobsShown = 0;
        int frames = 0;
        ulong bytesOfData = 0;


        // These are used for triggered full frame writing
        byte[] timIMbuff = new byte[0];//1500*1280*1024];
        byte[] timIMbuff2 = new byte[0];//500 * 1280 * 1024];

        byte[] singleIMbuff = new byte[1280 * 1024];
        uint realbuff = 0;
        uint buffcount = 0;
        //byte[] timIMbuff2 = new byte[500 * 1280 * 1024];
        //byte[] timIMbuff3 = new byte[500 * 1280 * 1024];
        //byte[] timIMbuff4 = new byte[500 * 1280 * 1024];
        public int imbuffCount = 0;
        byte[] condArr = null;


        // THis is all used to update UI
        private struct UIUpdateArgs
        {
            public float velo;
            //public string pixelValue;
            public string pixCoord;
            public uint blobNumber;
            public ulong dataNumber;
            public PixelValue2D debug;
            public float rotAngle;
            //public bool stimReporter;
            public int fps;
            //public int stimDirection;

            //public UIUpdateArgs(float _veloOUT, string _pixelValue, string _pixCoord, uint _blobNumber, 
            //    PixelValue2D _debug, float _rotAngle, bool _StimReporter, ulong _dataNumber, int _fps, int _stimDirection)
            public UIUpdateArgs(uint _blobNumber, int _fps, PixelValue2D _debug, string _pixCoord, float _rotAngle, ulong _dataNumber, float _veloOUT)
            {
                velo = _veloOUT;
                //pixelValue = _pixelValue;
                pixCoord = _pixCoord;
                blobNumber = _blobNumber;
                debug = _debug;
                rotAngle = _rotAngle;
                //stimReporter = _StimReporter;
                dataNumber = _dataNumber;
                fps = _fps;
                //stimDirection = _stimDirection;
            }
        }

        static public volatile bool myBufferReady;
        static public volatile uint myBufferReadyCount;
        static public volatile bool myFifoOverflow;

        static public long LockBuffer = 0;
        static public long LastLockBuffer;
        static public long myinterlockbuffer;


        //  This is new and used for PHX framegrabber
        /* Define an application specific structure to hold user information */

        public struct tPhxLive
        {
            public volatile uint dwBufferReadyCount;
            public volatile bool fBufferReady;
            public volatile bool fFifoOverflow;
        };

        /*
        phxlive_callback()
         * This is the callback function which handles the interrupt events.
         */
        unsafe static void phxlive_callback(
           uint hCamera,          /* Camera handle. */
           uint dwInterruptMask,  /* Interrupt mask. */
           IntPtr pvParams         /* Pointer to user supplied context */
        )

     

        {
           //   pvParams = Marshal.AllocHGlobal(1000);

             tPhxLive* psPhxLive = (tPhxLive*)pvParams;

            /* Handle the Buffer Ready event */
            if ((uint)Phx.etParamValue.PHX_INTRPT_BUFFER_READY == (dwInterruptMask & (uint)Phx.etParamValue.PHX_INTRPT_BUFFER_READY))

            {
                /* Increment the Display Buffer Ready Count */
                //                   psPhxLive->dwBufferReadyCount++;
                //                   psPhxLive->fBufferReady = true;
                //          myBufferReadyCount++;
                //          myBufferReady = true;

                Interlocked.Add(ref LockBuffer, 1);



            }

            /* FIFO Overflow */
            if ((uint)Phx.etParamValue.PHX_INTRPT_FIFO_OVERFLOW == (dwInterruptMask & (uint)Phx.etParamValue.PHX_INTRPT_FIFO_OVERFLOW))
            {
                // psPhxLive->fFifoOverflow = true;
       //         myFifoOverflow = true;


            }

            /* Note:
             * The callback routine may be called with more than 1 event flag set.
             * Therefore all possible events must be handled here.
             */

            //  Marshal.FreeHGlobal(pvParams);
        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // SET ACQUISITION PARAMETERS HERE//

        int AcqTime = 600000; //run 660000 for 11 mins, 960000 for 16 mins, 1260000 for 21 mins
        uint ROIx = 1024; // Max resolution is 2336x1728px for the MC4082
        uint ROIy = 1024; // lower ROI --> less data --> faster disk writing 
                          // This is not exactly ROI; basically just selecting buffer size, rest of the sent image is not captured in the buffer
                          // Real camera ROI and ROIOffset lines exist in the code; currently non-functional and not very useful anyhow.
                          // uint OffsetX = 128;
                          // uint OffsetY = 128;
        uint CameraFPS = 750; // when writing to disk, this is not very imp as you will be limited by writing speed
        int VideoFPS = 165; // this can change; look at console window calFPS readout to adjust i f required
        uint CameraExp = 1052;
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        //  uint CaptureMode = 1;
        //  uint ImgPerBuff = 1; 

        Phx.etStat eStat = Phx.etStat.PHX_OK;    /* Status variable */
        Phx.etParamValue eAcqType = 0;                    /* Parameter for use with PHX_ParameterSet/Get calls */
        Phx.etParamValue eParamValue = 0;
        Pbl.etPblParamValue ePblCaptureFormat = 0;
        Phx.etParamValue eCamSrcCol = 0;
        Phx.etParamValue eCaptureFormat = Phx.etParamValue.PHX_BUS_FORMAT_MONO8;
        Phx.etParamValue eCamFormat = 0;
        uint dwBufferReadyLast = 0;                    /* Previous BufferReady count value */
        IntPtr pParamValue = IntPtr.Zero;
        IntPtr pConfigFile = IntPtr.Zero;
        PhxCommon myPhxCommon = new PhxCommon();
        Phx.PHX_AcquireCallBack PHX_Callback = new Phx.PHX_AcquireCallBack(phxlive_callback);
        Phx.stImageBuff[] asImageBuffers = null;                 /* Capture buffer array */
        uint[] ahCaptureBuffers = null;                 /* Capture buffer handle array */
        uint hCamera = 0;                    /* Camera handle */
        uint dwAcqNumBuffers = 0;
        uint dwBufferWidth = 0;
        uint dwBufferHeight = 0;
        uint dwBufferStride = 0;
        uint dwCamSrcDepth = 0;
        bool fCameraIsCxp = false;
        bool fIsCxpCameraDiscovered = false;
        tPhxLive sPhxLive;                                       /* User defined event context */



        // Should have figured this out earlier; Init these arrays here outside the loop really 
        //reduces the runtime for the processing loop
        Byte[] MBD_image = new byte[arrayWidth * arrayHeight]; // Max resolution is 2336x1728px for the MC4082

        // This, when working properly, should be our image
        byte[,] CXP_image = new byte[arrayHeight, arrayWidth];

        //    int ROIxx = Convert.ToInt32(ROIx); // some padantic functions down the line would only take int arguments for ROI!
        //    int ROIyy = Convert.ToInt32(ROIy);

        /* Some timers for fps calculation and troubleshooting */
        System.Diagnostics.Stopwatch fpswatch4R = new System.Diagnostics.Stopwatch();
   //     System.Diagnostics.Stopwatch fpswatch4C = new System.Diagnostics.Stopwatch();
        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();


        unsafe public GrabAndStimAS_PHX_2016(Phx.etParamValue eBoardNumber,        /* Board number, i.e. 1, 2, or 0 for next available */
         Phx.etParamValue eChannelNumber,      /* Channel number */
         String strConfigFileName,   /* Name of config file */
         PhxCommon.tCxpRegisters sCameraRegs          /* Camera CXP registers */
            )
        {
  //                                tPhxLive sPhxLive;                                       /* User defined event context */
  //          tPhxLive* psPhxLive = (tPhxLive*)pvParams;

            //program and camera initialization below
            InitializeComponent();
            //  Initialize the UI.
            startButton.Enabled = true;
            stopButton.Enabled = false;
            quitButton.Enabled = true;
            interfaceTextBox.Text = "img0";

            //  Set up the acquisition background worker thread.  This thread
            //  will actually acquire the images and issue a callback
            //  to update the UI.
            acquisitionWorker = new System.ComponentModel.BackgroundWorker();
            acquisitionWorker.DoWork += new DoWorkEventHandler(acquisitionWorker_DoWork);
 //           acquisitionWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(acquisitionWorker_RunWorkerCompleted);
            acquisitionWorker.ProgressChanged += new ProgressChangedEventHandler(acquisitionWorker_ProgressChanged);
            acquisitionWorker.WorkerReportsProgress = true;
            acquisitionWorker.WorkerSupportsCancellation = true;

            // This inits the config window
            ConfigWindow = new TrialConfig();
            
            //Used to bring up the B/W thresholded view of the fish. Not actually used for debugging -- is useful for normal program operation.
            debugger = new DebugWin();
            if (debugON == true)
            {
                debugger.Show();
            }


            //Initialiaze numeric scroll boxes for controlling stim window properties
            this.xPos.Value = locationXsender;
            this.yPos.Value = locationYsender;
            this.aspRatio.Value = (decimal)aspRatioSender;
            this.horizSize.Value = sizeHorizsender;
            this.vertSize.Value = sizeVertsender;
            this.blobX.Value = lockX;
            this.blobY.Value = lockY;
            this.blobScaling.Value = (decimal)scaleFactor;
            this.blobScalingX.Value = (decimal)scaleFactorX;

            //Initialize work around for image display in Imaqdx
            // THiswas for displaying NI fraqme grabber images
            plzWrk = new VisionImage(ImageType.U8);
            debugger.imageViewer.Attach(plzWrk);

            // These refer to boxes on the UI
            BGtime = Int32.Parse(bgInterval.Text);
            binthresh = Int32.Parse(textBox1.Text);
            //for (int i = 0; i < 1000; i++)
            //{
            //    timIMbuff[i] = new byte[1024,1280];
            //}

            //Do some additional initializations
            // I don't thhink this is used
            // ----
            blankPixFront = 0;
            blankPixBack = arrayWidth;
            blankPixTop = 0;
            blankPixBottom = arrayHeight;
            // -----

            IMbuff = new Image<Gray, Byte>(arrayHeight, arrayWidth);

            // You might want to look at this
            #region Phoenix init stuff

            /* Create a Phx handle */
            eStat = Phx.PHX_Create(ref hCamera, Phx.PHX_ErrHandlerDefault); //PhX errorhandler Phx.PHX_Create(ref hCamera, Phx.PHX_ErrHandlerDefault) removed temporarly; 
                                                                            //to manually consider frame losses during analysis
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Set the configuration file name */
            if (!String.IsNullOrEmpty(strConfigFileName))
            {
                pConfigFile = Marshal.UnsafeAddrOfPinnedArrayElement(PhxCommon.GetBytesFromString(strConfigFileName), 0);
                eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_CONFIG_FILE, ref pConfigFile);
                if (Phx.etStat.PHX_OK != eStat) goto Error;
            }

            /* Set the board number */
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_BOARD_NUMBER, ref eBoardNumber);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Set the channel number */
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_CHANNEL_NUMBER, ref eChannelNumber);
            if (Phx.etStat.PHX_OK != eStat) goto Error;


            /* Open the board using the above configuration file */
            eStat = Phx.PHX_Open(hCamera);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Set the Image ROI */
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ROI_XLENGTH, ref ROIx); // ROIx and ROIy values init above
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ROI_YLENGTH, ref ROIy);

            /* Read various parameter values in order to generate the capture buffers. */
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ROI_XLENGTH, ref dwBufferWidth);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ROI_YLENGTH, ref dwBufferHeight);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CAM_SRC_DEPTH, ref dwCamSrcDepth);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CAM_SRC_COL, ref eCamSrcCol);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_BUS_FORMAT, ref eCaptureFormat);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CAM_FORMAT, ref eCamFormat);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ACQ_FIELD_MODE, ref eAcqType);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ACQ_NUM_BUFFERS, ref dwAcqNumBuffers);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* If you wnt to change the number of acquisition buffers, change here. 
             * Currently, I don't see a need. Can easily acquire at the frame rate we want (~560Hz) with just 2 buffers*/
               dwAcqNumBuffers = 100;

            /* Interlaced Camera in Field Mode */
            if (Phx.etParamValue.PHX_CAM_INTERLACED == eCamFormat
               && (Phx.etParamValue.PHX_ACQ_FIELD_12 == eAcqType
                  || Phx.etParamValue.PHX_ACQ_FIELD_21 == eAcqType
                  || Phx.etParamValue.PHX_ACQ_FIELD_NEXT == eAcqType
                  || Phx.etParamValue.PHX_ACQ_FIELD_1 == eAcqType
                  || Phx.etParamValue.PHX_ACQ_FIELD_2 == eAcqType))
            {
                dwBufferHeight /= 100;
            }

            /* Determine PHX_BUS_FORMAT based on the camera format */
            eStat = myPhxCommon.PhxCommonGetBusFormat(eCamSrcCol, dwCamSrcDepth, eCaptureFormat, ref eCaptureFormat);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Update the PHX_BUS_FORMAT, as it may have changed (above) */
            eStat = Phx.PHX_ParameterSet(hCamera, (Phx.etParam.PHX_BUS_FORMAT | Phx.etParam.PHX_CACHE_FLUSH), ref eCaptureFormat);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Read back the Buffer Stride */
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_BUF_DST_XLENGTH, ref dwBufferStride);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            //      dwBufferStride = 2336;


            /* Init the array of capture buffer handles */
            ahCaptureBuffers = new uint[dwAcqNumBuffers];

            /* Init the array of image buffers */
            asImageBuffers = new Phx.stImageBuff[dwAcqNumBuffers + 1];

            /* Create and initialise our capture buffers (not associated with display) */
            for (int i = 0; i < dwAcqNumBuffers; i++)
            {
                /* We create a capture buffer for our double buffering */
                eStat = Pbl.PBL_BufferCreate(ref ahCaptureBuffers[i], Pbl.etPblBufferMode.PBL_BUFF_SYSTEM_MEM_DIRECT, 0, hCamera, myPhxCommon.PhxCommonDisplayErrorHandler);
                if (Phx.etStat.PHX_OK != eStat) goto Error;

                /* Initialise our capture buffer */
                eStat = Pbl.PBL_BufferParameterSet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_BUFF_WIDTH, ref dwBufferWidth);
                if (Phx.etStat.PHX_OK != eStat) goto Error;
                eStat = Pbl.PBL_BufferParameterSet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_BUFF_HEIGHT, ref dwBufferHeight);
                if (Phx.etStat.PHX_OK != eStat) goto Error;
                eStat = Pbl.PBL_BufferParameterSet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_BUFF_STRIDE, ref dwBufferStride);
                if (Phx.etStat.PHX_OK != eStat) goto Error;
                ePblCaptureFormat = (Pbl.etPblParamValue)eCaptureFormat;
                eStat = Pbl.PBL_BufferParameterSet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_DST_FORMAT, ref ePblCaptureFormat);
                if (Phx.etStat.PHX_OK != eStat) goto Error;
                eStat = Pbl.PBL_BufferInit(ahCaptureBuffers[i]);
                if (Phx.etStat.PHX_OK != eStat) goto Error;

                /* Build up our array of capture buffers */
                Pbl.PBL_BufferParameterGet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_BUFF_ADDRESS, ref asImageBuffers[i].pvAddress);
                asImageBuffers[i].pvContext = (IntPtr)ahCaptureBuffers[i];
            }
            /* Terminate the array */
            asImageBuffers[dwAcqNumBuffers].pvAddress = System.IntPtr.Zero;
            asImageBuffers[dwAcqNumBuffers].pvContext = System.IntPtr.Zero;

            /* The above code has created dwAcqNumBuffers acquisition buffers.
             * Therefore ensure that the Phoenix is configured to use this number, by overwriting
             * the value already loaded from the config file.
             */
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ACQ_NUM_BUFFERS, ref dwAcqNumBuffers);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* These are 'direct' buffers, so we must tell Phoenix about them
             * so that it can capture data directly into them.
             */

            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_BUF_DST_XLENGTH, ref dwBufferStride);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_BUF_DST_YLENGTH, ref dwBufferHeight);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_DST_PTRS_VIRT, asImageBuffers);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eParamValue = Phx.etParamValue.PHX_DST_PTR_USER_VIRT;
            eStat = Phx.PHX_ParameterSet(hCamera, (Phx.etParam.PHX_DST_PTR_TYPE | Phx.etParam.PHX_CACHE_FLUSH | Phx.etParam.PHX_FORCE_REWRITE), ref eParamValue);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Enable FIFO Overflow events */
            eParamValue = Phx.etParamValue.PHX_INTRPT_FIFO_OVERFLOW;
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_INTRPT_SET, ref eParamValue);
            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Setup our own event context */
            fixed (tPhxLive* psPhxLive = &sPhxLive)
            {

                /* Setup our own event context */
                eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_EVENT_CONTEXT, psPhxLive); //ptrsPhxLive //(void*)&sPhxLive
                if (Phx.etStat.PHX_OK != eStat) goto Error;
            }

            /* Check if camera is CXP */
            eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_BOARD_VARIANT, ref eParamValue);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            if (Phx.etParamValue.PHX_BOARD_FBD_4XCXP6_2PE8 == eParamValue
               || Phx.etParamValue.PHX_BOARD_FBD_2XCXP6_2PE8 == eParamValue
               || Phx.etParamValue.PHX_BOARD_FBD_1XCXP6_2PE8 == eParamValue)
            {
                fCameraIsCxp = true;
            }

            /* Set the number of images per buffer */
            //     eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ACQ_IMAGES_PER_BUFFER, ref ImgPerBuff); // variable to set in the initialization block ~ line # 120
            //     Console.WriteLine("Number of images per buffer = {0}\r\n", ImgPerBuff);

            /* Check that camera is discovered (only applies to CXP) */
            if (fCameraIsCxp)
            {
                myPhxCommon.PhxCommonGetCxpDiscoveryStatus(hCamera, 10, ref fIsCxpCameraDiscovered);
                if (!fIsCxpCameraDiscovered)
                {
                    goto Error;
                }
            }

            /* To set the capture mode - Continuous or Snapshot
            eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ACQ_CONTINUOUS, ref CaptureMode); // variable to set in the initialization block ~ line # 120
            if (CaptureMode == 1)
                Console.WriteLine("\r\nCapture Mode is CONTINUOUS\r\n");
            if (CaptureMode == 0)
                Console.WriteLine("\r\nCapture Mode is SNAPSHOT\r\n");
                */

            /* init for reg writing tests */
            //    uint dwValue = 0;
            //    uint ExpTim = 0;
            //    uint CXPspeed = 0;

            //  eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ROI_XLENGTH, ref ROIx);
            //  eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ROI_SRC_XOFFSET, ref OffsetX);
            //  eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ROI_YLENGTH, ref ROIy);
            //  eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_ROI_SRC_YOFFSET, ref OffsetY);

            //   uint a = 0;
            //   uint b = 0;
            //   eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ROI_XLENGTH, ref a);
            //   eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ROI_YLENGTH, ref b);


            /* Writing to the camera regs. Figuring this out took ages, and able to implement this, a lifetime */

            // eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 16404, 262216, 500); // connection 4 speed 6250; 
            //device not ready error; hex value to be written 0x40048; likely written as mid big endian;
            eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x8814, CameraFPS, 500); // fps dec value 34836
            eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x8840, CameraExp, 500); // exposure time 34880
            eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 37648, 1, 500); // frame counter stamp
            eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 37652, 1, 500); // camera time stamps

            //   eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x8180, 1, 500); //0x8180 ROI selector
            //   eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x8184, 1, 500); // RegionMode: bin; 0x8184
            //   eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x3000, ROIx, 500); // Width: 128 to max; to be incremented in steps of 64; 0x3000
            //   eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x3004, ROIy, 500); // Height: 1 to max; to be incremented in steps of 1; 0x3004 
            //   eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x8800, OffsetX, 500); // OffsetXReg: horizontal offset - again incremented by 64 uptosensorWidth; 0x8800
            //   eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, 0x8804, OffsetY, 500); // OffsetYReg: vertical offset - 1 to sensorHeight max, stepsize of 1; 0x8804

            /* for reg writing tests */
            // eStat = myPhxCommon.PhxCommonReadCxpReg(hCamera, 34836, ref dwValue, 500);
            // eStat = myPhxCommon.PhxCommonReadCxpReg(hCamera, 34880, ref ExpTim, 500);
            // eStat = myPhxCommon.PhxCommonReadCxpReg(hCamera, 0x4014, ref CXPspeed, 500); // connection 4 speed 6250

            /* Now start our capture, using the callback method */
            if (Phx.etStat.PHX_OK != eStat) goto Error;
            eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_START, PHX_Callback);

            if (Phx.etStat.PHX_OK != eStat) goto Error;

            /* Now start camera */
            if (fCameraIsCxp && 0 != sCameraRegs.dwAcqStartAddress)
            {
                eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, sCameraRegs.dwAcqStartAddress, sCameraRegs.dwAcqStartValue, 1);
                if (Phx.etStat.PHX_OK != eStat) goto Error;
            }

            //videowriter init and open a new instance
            //     int width = ROIxx;
            //     int height = ROIyy;
            //     int fps = VideoFPS;
            //    VideoFileWriter writer = new VideoFileWriter(); // new instance of ffmpeg videowriter
            //    string videotimestamp = DateTime.Now.ToString("MM.dd.yyyy_HH.mm.ss.fff"); //timestamp for filename
            //    writer.Open("C:/Users/Gokul/Documents/DataBeta/Video/" + videotimestamp + ".avi", width, height, fps, VideoCodec.MPEG4);

            //Cal fps init
            //     uint buffcount = 0;
            //     int frames = 0;
            //     int prevframes = 0;

            //start the timers now! poorly named, ofcourse
            timer.Start(); // for runtime
            fpswatch4R.Start(); // for real-time camera fps
            Gtimer.Start();

            //    fpswatch4C.Start(); // for calculated system fps

            Error:;
            //do nothing  
                      
            /* Now cease all captures
            if (0 != hCamera)
            {
                /* Stop camera
                if (fIsCxpCameraDiscovered && 0 != sCameraRegs.dwAcqStopAddress)
                {
                    myPhxCommon.PhxCommonWriteCxpReg(hCamera, sCameraRegs.dwAcqStopAddress, sCameraRegs.dwAcqStopValue, 800);
                }
                /* Stop frame grabber 
                Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_ABORT, IntPtr.Zero);
            }

            */


            #endregion

        }



        private void startButton_Click(object sender, EventArgs e)
        {

            try
            {
                //  Update the UI.
                startButton.Enabled = false;
                stopButton.Enabled = true;
                //bufNumTextBox.Text = "";
                //pixelValTextBox.Text = "";
                //  Create a session.
               // _session = new ImaqSession("img0");
                //  Configure the image viewer.
                //displayImage = new VisionImage((ImageType)_session.Attributes[ImaqStandardAttribute.ImageType].GetValue());
                //imageViewer.Attach(displayImage);
                //  Create a buffer collection for the acquisition with the requested
                //  number of images, and configure the buffers to loop continuously.
                //int numberOfImages = 2; //WAS 20!!!
                //bufList = _session.CreateBufferCollection(numberOfImages, ImaqBufferCollectionType.PixelValue2D);
                //for (int i = 0; i < bufList.Count; ++i)
                //{
                //    bufList[i].Command = (i == bufList.Count - 1) ? ImaqBufferCommand.Loop : ImaqBufferCommand.Next;
                //}
                ////  Configure and start the acquisition.
                //_session.Acquisition.Configure(bufList);
                //_session.Acquisition.AcquireAsync();
                //_session.Acquisition.AcquireCompleted += new EventHandler<AsyncCompletedEventArgs>(Acquisition_AcquireCompleted);
                //  Start the background worker thread.
                acquisitionWorker.RunWorkerAsync();
                fullSize = (int)(arrayHeight*arrayWidth);
                singleIMbuff = new byte[fullSize];
                //arrayHeight = 1728;// (int)_session.RegionOfInterest.Height;
                //arrayWidth = 2336;// (int)_session.RegionOfInterest.Width;
                IMrow = new byte[fullSize];
            }
            catch (ImaqException ex)
            {
                MessageBox.Show(ex.Message, "NI-IMAQ Error");
                Cleanup();
            }

        }

        //void Acquisition_AcquireCompleted(object sender, AsyncCompletedEventArgs e)
        //{
        //    realbuff++;
        //}
        int bgGenerateCount = 0;

        void acquisitionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //  This is the main function of the acquisition background worker thread.
            //  Perform image processing here instead of the UI thread to avoid a
            //  sluggish or unresponsive UI.
            BackgroundWorker worker = (BackgroundWorker)sender;

          //  tPhxLive sPhxLive;                                       /* User defined event context */


            //          tPhxLive sPhxLive;                                       /* User defined event context */


            //Used to work with/initialize camera viewing window (to see raw image)
            if (displayCam != null)
            {
             //   viewerForm.session = _session;
                displayCam.Resume();
            }

            try
            {
                bgCount = 0; //initializes bgCount, which should iterate at the same rate as grabNumDisp, but is periodically reset.
                             // bgCount is used for time-dependent processing, e.g. animating OMR, blob, etc.

                //various variable initializations
                string pixCoord = "";
                string pixelValue = "";


                //These bools are used to toggle between stimulus states
                //TryStim = true;
                StimOff = true;



                int fps = 0, velocount = 0;



                rotAngle = 0;

                // image arrays used to store background (used for background subtraction)
                byte[,] backgroundPixels = new byte[arrayHeight, arrayWidth];

                // Uses the mode ofpixels over collected frames for BG
                byte[,,] backgroundPixelsMode = new byte[arrayHeight, arrayWidth, 20];



                //initializes image objects

                // NI board
             //   VisionImage backgroundImage = new VisionImage(ImageType.U8);

                //NEED THIS -- this is the background image loop that acquires the maximum background 
                //image used for subtraction
                uint bufferNumber = 0;






                //initializing the conditions random generator



                //poorly named stopwatches used for counting/timing things like image acquistion and stimulus toggling
                stopwatch.Start();
                stopwatch2.Start();

                //this a better named stopwatch used for calculating the FPS the program is running at.
                fpswatch.Start();





                //  Loop until we tell the thread to cancel or we get an error.  When this
                //  function completes the acquisitionWorker_RunWorkerCompleted method will
                //  be called.

                Matrix<Byte> backh = new Matrix<byte>(arrayHeight, arrayWidth);
                while (!worker.CancellationPending)
                {

                    //this is where fps is calculated
                    if (FirstAfterStart)
                    {
                        // Start the stimulus thread
                        Thread renderer = new Thread(new ParameterizedThreadStart(loopRender));
                        renderer.Start(this);

                        FirstAfterStart = false;
                        //Generate pseudorandom stimulus order
                        if (!calibrateBox.Checked && !spontBox.Checked)
                        {
                            condArr = new byte[sParams.Length * 30];

                            for (int j = 0; j < 30; j++)
                            {
                                List<byte> newConds = new List<byte>(sParams.Length);
                                for (int k = 0; k < sParams.Length; k++)
                                    newConds.Add((byte)k);
                                for (int i = sParams.Length - 1; i >= 0; i--)
                                {
                                    int nextInd = conditionsNum.Next(i + 1);
                                    condArr[j * sParams.Length + sParams.Length - 1 - i] = newConds[nextInd];
                                    newConds.RemoveAt(nextInd);
                                }
                            }

                            // This is used for the random velocity thing
                            if (sParams[0].trialType == TrialType.VeloWalk) //load in pre-generated size(t) based on random velo walk
                                allSize = readSizeFile();
                        }
                        else // What is this>?
                            BlobDelay.Start();


                    }


                    // Calcvulate current FPS

                    if ((int)fpswatch.ElapsedMilliseconds >= 1000)
                    {
                        fps = frames;
                        frames = 0;
                        fpswatch.Reset();
                        fpswatch.Start();
                    }

                    if (buffcount != realbuff)
                    {
                        frames++;
                        buffcount = realbuff;
                    }

                    // fps = frames;
                    //gets the newest camera image and saves it in newestImage -- a VisionImage object
                    //_session.Grab(newestImage, true);

                    //two counters that do important things and iterate at 60Hz
                    bgCount = (int)stopwatch.ElapsedMilliseconds / 17;
                    grabNumberDisp = (uint)stopwatch2.ElapsedMilliseconds / 17;
                    //  Update the UI by calling ReportProgress on the background worker.
                    //  This will call the acquisition_ProgressChanged method in the UI
                    //  thread, where it is safe to update UI elements.  Do not update UI
                    //  elements directly in this thread as doing so could result in a
                    //  deadlock.


                    //     while (!sPhxLive.fBufferReady && !sPhxLive.fFifoOverflow)
                    //     {
                    // do nothing!
                    //     }



                    /*                  while (0 == myPhxCommon.PhxCommonKbHit() && !sPhxLive.fBufferReady && !sPhxLive.fFifoOverflow) // Wait for buffer to be filled
                                      {
                                          // do nothing!
                                      }
                    */


                    //            while (!myBufferReady)
                    //            {
                    //do nothing!
                    //            }

                    while (Interlocked.Read(ref LockBuffer) > myinterlockbuffer)
                    {

                        myinterlockbuffer = Interlocked.Read(ref LockBuffer);


                        GetLast:
                        {

                            //                      Thread.Sleep(1);


                            /*                if (dwBufferReadyLast != sPhxLive.dwBufferReadyCount)
                                            {
                                                uint dwStaleBufferCount;

                                                /* If the processing is too slow to keep up with acquisition,
                                                 * then there may be more than 1 buffer ready to process.
                                                 * The application can either be designed to process all buffers
                                                 * knowing that it will catch up, or as here, throw away all but the
                                                 * latest

                                                dwStaleBufferCount = sPhxLive.dwBufferReadyCount - dwBufferReadyLast;
                                                dwBufferReadyLast += dwStaleBufferCount;


                                                /* Throw away all but the last image
                                                if (1 < dwStaleBufferCount)
                                                {
                                                    do
                                                    {
                                                        eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
                                                        dwStaleBufferCount--;
                                                    } while (dwStaleBufferCount > 1);
                                                }
                                            }
                        */


                            //                  sPhxLive.fBufferReady = false;
                            //                   BufferUpdate(false);
                            //                   myBufferReady = false;



                            // Init a working buffer to pull information from the ping-pong buffer

                            Phx.stImageBuff stBuffer;
                            stBuffer.pvAddress = IntPtr.Zero;
                            stBuffer.pvContext = IntPtr.Zero;

                            /* Get the info for the last acquired buffer */
                            eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_GET, ref stBuffer);
                            if (Phx.etStat.PHX_OK != eStat)
                            {
                                eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
                                continue;

                            }

                            /*  copy data from unmanaged memory buffer to a managed array */
                            Marshal.Copy(stBuffer.pvAddress, MBD_image, 0, arrayWidth * arrayHeight);
                            /* Copying the data into a 2D array*/
                            Buffer.BlockCopy(MBD_image, 0, CXP_image, 0, arrayWidth * arrayHeight);

                            RealFrameCount = getCurrentFrame(CXP_image);
                            //   Console.WriteLine("{0}", RealFrameCount);

                        }
                        /*CheckBox frame number of the next image with the previous and decide whether to process it 
                         * or throw it (Release -- goes to buffer release) */

                        //                   while (RealFrameCount <= FrameCheck)
                        //                   {
                        //                       eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
                        //                       goto GetLast;                  
                        //                   }


                        /* Frame rate from camera information */

                        /*   RealFrameStamp = new byte[4];
                             System.Buffer.BlockCopy(CXP_image, 0, RealFrameStamp, 0, 2);
                             RealFrameCount = System.BitConverter.ToUInt32(RealFrameStamp, 0); */
                        if ((int)fpswatch4R.ElapsedMilliseconds >= 1000)
                        {
                            Realfps = (RealFrameCount - LastFrame);
                            //  Console.WriteLine("fps = {0}", Realfps);
                            long fpswatchValue = fpswatch4R.ElapsedMilliseconds;
                            LastFrame = RealFrameCount;
                            fpswatch4R.Reset();
                            fpswatch4R.Start();
                        }

                        FrameCheck = RealFrameCount; // assign the current number to FrameCheck before leaving

                        /* Camera TimseStamp */
                        byte[] RealTimeStamp = new byte[4];
                        System.Buffer.BlockCopy(CXP_image, 4, RealTimeStamp, 0, 4);
                        // Array.Reverse(RealTimeStamp);
                        RealTimeCounter = ((System.BitConverter.ToUInt32(RealTimeStamp, 0)) * 40) / 1000; // based on time counter; 25MHz/40ns; in usecs
                                                                                                          // Console.WriteLine("Time = {0}", RealTimeCounter);

                        /* Calculated frame rate */
                        realbuff = getFrameCount(CXP_image);

                        /* Print acquisition parametrs to console window and write to disk in csv format */
                        string SystemStamp = DateTime.Now.ToString("mm.ss.fff");
                        //      Console.WriteLine("frame # = {0}, Realfps = {1}, Calfps = {2}, CamTim = {3}, SysTim = {4}", RealFrameCount, Realfps, Calfps, RealTimeCounter, SystemStamp);
                        //                    var csvTime = RealTimeCounter.ToString();
                        //                    var csvFrame = RealFrameCount.ToString();
                        //                    var csvFps = Realfps.ToString();
                        //                    var csvCalfps = fps.ToString();
                        //               //     var csvROIx = ROIx.ToString();
                        //     var csvROIy = ROIy.ToString();
                        //                    var newLine = string.Format("{0},{1},{2},{3}, {4}", csvFrame, csvFps, csvCalfps, csvTime, SystemStamp);
                        //                    csv.AppendLine(newLine);


                        /* Parsing bytes to create image and then save */
                        //                       Image <Gray, byte> depthImage = new Image<Gray, byte>(arrayWidth, arrayHeight); // again only int arguments allowed
                        //                       depthImage.Bytes = MBD_image;
                        /*
                                            depthImage.ROI = new Rectangle(55, 60, 25, 25); // select 625 pixels around the LED
                                            var LED_Region = depthImage.GetAverage(depthImage); // calculate average intensity of these LED pixels
                                            LED_intensity = LED_Region.Intensity;

                                            if (LED_intensity > 200) // report when LED is ON or OFF
                                            {

                                                LEDStatus = 1;
                                            }
                                            else
                                                LEDStatus = 0;

                                            depthImage.Bytes = MBD_image; // deselecting the ROI
                        */

                        // string imagetimestamp = DateTime.Now.ToString("MM.dd.yyyy_HH.mm.ss.fff");
                        // depthImage.Save("C:/Users/Gokul/Documents/DataBeta/Images/" + imagetimestamp + ".jpg");

                        /* Write images into a video. This is using AForge ffmpeg assembly  */
                        //   Bitmap myImage = depthImage.ToBitmap(); //convert to a bitmap
                        //   writer.WriteVideoFrame(myImage);
                        //   myImage.Dispose();

                        /* Having processed the data, release the buffer ready for further image data */
                        eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);

                        //-------------------------
                        //            extractedPixels = _session.Acquisition.Extract(bufferNumber, out bufferNumber).ToPixelArray();
                        realbuff = getFrameCount(CXP_image);

                        if (BGtaken)
                        {
                            if (!zoom)
                                foundfish = false;
                            if (!triggerBox.Checked)
                            {
                                try
                                {
                                    subtractedArray = BGSubtract(CXP_image, backh, binthresh, ref foundfish, ref coordEyeAX, ref coordEyeAY);
                                }
                                catch { foundfish = false; }
                                //subtractedArray = extractedPixels.U8;
                                IM_mat.Data = subtractedArray;




                                test = new IntPtr();

                                test = CvInvoke.cvGetImage(IM_mat, IMbuff);

                                CvInvoke.cvMoments(test, ref COM, 0);
                                if (foundfish)
                                {
                                    coordEyeAX = (int)COM.GravityCenter.x + coordEyeAX - 30;
                                    coordEyeAY = (int)COM.GravityCenter.y + coordEyeAY - 30;
                                }
                                else
                                {
                                    coordEyeAX = (int)COM.GravityCenter.x;
                                    coordEyeAY = (int)COM.GravityCenter.y;
                                }
                                Sxrep = 0;
                                Syrep = 0;
                                rotAngle = momentOrientation(COM, ref Sxrep, ref Syrep);

                                //coordEyeAX = 640;
                                //coordEyeAY = 512;
                                //rotAngle = (float)Math.PI / 4;
                                //foundfish = true;

                                lockX = coordEyeAX;
                                lockY = coordEyeAY;


                                //if (Math.Abs(rotAngle - lockAngle) < 2 * (float)Math.PI / 3 || (Math.Abs(rotAngle - lockAngle) > 3 * (float)Math.PI / 2))
                                lockAngle = rotAngle;


                                //sliding window average velocity 5 images
                                velocity[velocount] = (float)Math.Sqrt((lockX - coordEyeBX) * (lockX - coordEyeBX) + (lockY - coordEyeBY) * (lockY - coordEyeBY));
                                velocount++;
                                veloOUT = 0;
                                if (velocount == 5)
                                    velocount = 0;
                                for (int z = 0; z < 5; z++)
                                    veloOUT += velocity[z];
                                veloOUT = veloOUT / 5;
                                coordEyeBX = lockX;
                                coordEyeBY = lockY;

                                //Re-take BG if 10 minutes have elapsed.

                                // NI board
                                // VisionImage backgroundImage = new VisionImage(ImageType.U8);

                                if (false)//(retakeBG.ElapsedMilliseconds >= 300000 && spontBox.Checked)
                                {
                                    bgGenLoop = true;
                                    bgGenerateCount = 0;
                                    retakeBG.Stop();
                                    retakeBG.Reset();
                                    retakeBG.Start();
                                    stopwatch.Reset();
                                    updateProgBar(-100);
                                    backgroundPixels = new byte[arrayHeight, arrayWidth];
                                }

                                if (bgGenerateCount < 10)
                                {
                                    if (bgGenLoop) //We have triggered a new background image loop
                                    {
                                        stopwatch.Start();
                                        bgGenLoop = false;
                                    }
                                    if (stopwatch.ElapsedMilliseconds > BGtime)
                                    {
                                        bgGenerateCount++;
                                        bgGenLoop = true;
                                        stopwatch.Reset();
                                        //backgroundImage = CXP_image;// _session.Acquisition.Extract(bufferNumber, out bufferNumber).ToImage();
                                        backgroundPixels = MaxArray(backgroundPixels, CXP_image);
                                        updateProgBar(100 / 10);
                                    }
                                }
                                else
                                {

                                    System.Buffer.BlockCopy(backgroundPixels, 0, IMrow, 0, fullSize);
                                    backh.Bytes = IMrow;
                                }
                            }
                            else
                            {
                                subtractedArray = CXP_image;
                            }
                            UpdateToggleStims();

                            bufferNumber++;
                        }
                        else if (BGsnap)
                        {
                            if (bgGenerateCount < 20)
                            {
                                if (bgGenLoop)
                                {
                                    stopwatch.Start();
                                    bgGenLoop = false;
                                }
                                if (stopwatch.ElapsedMilliseconds > BGtime)
                                {
                                    bgGenerateCount++;
                                    stopwatch.Reset();
                                    bgGenLoop = true;
                                    // backgroundImage = _session.Acquisition.Extract(bufferNumber, out bufferNumber).ToImage();
                                    backgroundPixels = MaxArray(backgroundPixels, CXP_image);
                                    backgroundPixelsMode = ExtendArray3D(backgroundPixelsMode, CXP_image, bgGenerateCount);
                                    //updateProgBar(100/10);
                                    updateProgBar(100 / 20);
                                    bufferNumber++;
                                }
                            }
                            else
                            {
                                backgroundPixels = CalculateMode(backgroundPixelsMode, bgGenerateCount);

                                System.Buffer.BlockCopy(backgroundPixels, 0, IMrow, 0, fullSize);
                                backh.Bytes = IMrow;
                                BGsnap = false;
                                BGtaken = true;
                                if (WriteFile)
                                    BGFileWriter.Write(IMrow);
                            }
                            subtractedArray = CXP_image;
                            bufferNumber++;
                            retakeBG.Start(); //Make sure to collect another BG (passively) after 10 minutes
                        }
                        else
                        {
                            //for (int i = 0; i < CXP_image.GetLength(0); i++)
                            //    for (int j = 0; j < CXP_image.GetLength(1); j++)
                            //        CXP_image[i, j] = 240;
                            subtractedArray = CXP_image;

                            //if (!zoom)
                            //{
                            //    Image<Gray, Byte> holder = new Image<Gray, byte>(1280, 1024);
                            //    Matrix<Byte> IMmatbuffer = new Matrix<byte>(1024, 1280);
                            //    //linearize 2D array
                            //    byte[] linIMbuffer = new byte[1280 * 1024];
                            //    System.Buffer.BlockCopy(extractedPixels.U8, 0, linIMbuffer, 0, 1280 * 1024);
                            //    //assign pixel values
                            //    holder.Bytes = linIMbuffer;
                            //    IMmatbuffer.Bytes = holder.Bytes;
                            //    subtractedArray = IMmatbuffer.Data;

                            //}
                            //if (imbuffCount < 1000)
                            //{
                            //    bytesOfData++;

                            //    timIMbuff[imbuffCount] = (byte[,]) extractedPixels.U8.Clone(); 

                            //    //System.Buffer.BlockCopy(extractedPixels.U8, 0, timIMbuff, imbuffCount * fullSize, fullSize);
                            //    imbuffCount++;
                            //}
                            //if (imbuffCount >= 1000 && imbuffCount < 2000)
                            //{
                            //    //subtractedArray = new byte[1024, 1280];
                            //     subtractedArray = timIMbuff[imbuffCount - 1000];

                            //    //imbuffCount++;calibr
                            //}
                            if (calibrateBox.Checked)
                                UpdateToggleStims();
                            bufferNumber++;
                        }

                        //default running statement -- normal program use when calibrate box in window is not checked
                        //if (!calibrateBox.Checked)


                        //    

                        //    //veloOUT = 10;



                        //}




                        // frames++;


                        pixCoord = "[" + coordEyeAX.ToString() + " , " + coordEyeAY.ToString() + "]";
                        pixelValue = (!Double.IsNaN(Sxrep) && !Double.IsInfinity(Sxrep) ? Decimal.Round((decimal)Sxrep, 2).ToString() : "NaN") +
                            " " + (!Double.IsNaN(Syrep) && !Double.IsInfinity(Syrep) ? Decimal.Round((decimal)Syrep, 2).ToString() : "NaN");

                        //worker.ReportProgress(0, new UIUpdateArgs(veloOUT, pixelValue, pixCoord, blobsShown, 
                        //    calibrateBox.Checked ? new PixelValue2D(gridCalibrate(extractedPixels.U8)) : new PixelValue2D(extractedPixels.U8), 
                        //    rotAngle, !BlobStart, bytesOfData, fps,BlobLeftRight));
                        worker.ReportProgress(0, new UIUpdateArgs(blobsShown, fps, calibrateBox.Checked ? new PixelValue2D(gridCalibrate(subtractedArray)) : new PixelValue2D(subtractedArray), pixCoord, rotAngle, bytesOfData, veloOUT));//(ulong)imbuffCount*1000uL));

                        // RemovePix(subtractedArray, coordEyeAX, coordEyeAY, coordEyeBX, coordEyeBY, coordTailX, coordTailY)
                        // WriteImage(coordEyeAX, coordEyeAY, CXP_image, subtractedArray, true);
                    }

                }
            }
            catch (ImaqException ex)
            {
                //  If an error occurs and the background worker thread is not being
                //  cancelled, then pass the exception along in the result so that
                //  it can be handled in the acquisition_RunWorkerCompleted method.
                if (!worker.CancellationPending)
                    e.Result = ex;
            }

        }

        //static bool CalibrateCheck(int checkX, int checkY,

        public float momentOrientation(MCvMoments binIM, ref double Sx, ref double Sy)
        {
            double u_20 = binIM.GetCentralMoment(2, 0) / binIM.GetCentralMoment(0, 0);
            double u_02 = binIM.GetCentralMoment(0, 2) / binIM.GetCentralMoment(0, 0);
            double u_11 = binIM.GetCentralMoment(1, 1) / binIM.GetCentralMoment(0, 0);
            double u_30 = binIM.GetCentralMoment(3, 0) / binIM.GetCentralMoment(0, 0);
            double u_03 = binIM.GetCentralMoment(0, 3) / binIM.GetCentralMoment(0, 0);

            Sx = u_30 / Math.Sqrt(u_20 * u_20 * u_20);
            Sy = u_03 / Math.Sqrt(u_20 * u_20 * u_02);
 
            float orAngle = 0.5f * (float)Math.Atan2(-2 * u_11, u_20 - u_02);
            if ((Sx > 0 && orAngle < (float)Math.PI/6 && orAngle > -(float)Math.PI/6) || (Sy > 0 && orAngle <= -(float)Math.PI/6) || (Sy < 0 && orAngle >= (float)Math.PI/6))
                orAngle+= (float)Math.PI;
            if (orAngle < 0)
                orAngle += 2 * (float)Math.PI;
            return orAngle;
                //return (orAngle + 3 * (float)Math.PI / 2);
            
            
        }

        public byte[,] MeanArrayRun(byte[,] meanSoFar, byte[,] newArray, byte numSoFar)
        {
            for (int i = 0; i < arrayWidth; i++)
                for (int j = 0; j < arrayHeight; j++)
                    meanSoFar[j, i] = (byte)((meanSoFar[j, i] * numSoFar + newArray[j, i]) / (numSoFar + 1));

            return meanSoFar;
        }

        public byte[,] MaxArray(byte[,] maxSoFar, byte[,] newArray)
        {
            for (int i = 0; i < arrayWidth; i++)
                for (int j = 0; j < arrayHeight; j++)
                    if (newArray[j, i] > maxSoFar[j, i])
                        maxSoFar[j, i] = newArray[j, i];

            return maxSoFar;
        }

        public byte[,,] ExtendArray3D(byte[,,] extendedSoFar, byte[,] newArray, int BGloopCount)
        {
            for (int i = 0; i < arrayWidth; i++)
                for (int j = 0; j < arrayHeight; j++)
                    extendedSoFar[j, i, BGloopCount-1] = newArray[j, i];

            return extendedSoFar;
        }

        public byte[,] CalculateMode(byte[, ,] fullArray, int BGloopCount)
        {
            byte[] intensityFrequency = new byte[256];
            byte[,] modeImage = new byte[arrayHeight,arrayWidth];

            for (int i = 0; i < arrayWidth; i++)
                for (int j = 0; j < arrayHeight; j++)
                {
                    intensityFrequency = new byte[256];
                    for (int z = 0; z < BGloopCount; z++)
                    {
                        intensityFrequency[fullArray[j, i, z]]++;
                    }
                    int maxCount = 0;
                    int maxInd = 0;
                    for (int z = 0; z < intensityFrequency.Length; z++)
                        if (intensityFrequency[z] > maxCount)
                        {
                            maxCount = intensityFrequency[z];
                            maxInd = z;
                        }
                    if (maxCount <= 1){
                        int avgInt = 0;
                        for (int z = 0; z < BGloopCount; z++)
                        {
                            avgInt = avgInt + fullArray[j, i, z];
                        }
                        modeImage[j, i] = (byte)(avgInt/BGloopCount);
                    }
                    else
                        modeImage[j,i] = (byte)maxInd;
                }

            return modeImage;
        }

        public void WriteImage(int midX, int midY, byte[,] pixels, byte[,] threshPixels, bool rawIM)
        {
            if (rawIM && (midX - 30) >= 0 && (midY - 30) >= 0 && (midX + 30) < arrayWidth && (midY + 30) < arrayHeight)
            {
                for (int i = (midX - 30); i < (midX + 30); i++)
                {
                    for (int j = (midY - 30); j < (midY + 30); j++)
                    {
                        dataFileWriter.Write((float)pixels[j, i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 60; i++)
                {
                    for (int j = 0; j < 60; j++)
                    {
                            dataFileWriter.Write((float)threshPixels[j, i]);

                    }
                }
            }
        }

        public void loopRender(object sender)
        {
            while (secondForm.Created)
                secondForm.Render(sender);
        }

        public byte[,] gridCalibrate(byte[,] aImage)
        {
            if (gridBox.Checked) {
                for (int i = circH-400; i < arrayWidth; i += 200)
                {
                    for (int j = 0; j < arrayHeight; j++)
                    {
                        aImage[j, i] = 0;
                        aImage[j, i + 1] = 0;
                    }
                }
                for (int j = circK-400; j < arrayHeight; j += 200)
                {
                    for (int i = 0; i < arrayWidth; i++)
                    {
                        aImage[j, i] = 0;
                        aImage[j + 1, i] = 0;
                    }
                }
            }
            else
            {
                for (int i = circH; i < arrayWidth; i += 50)
                {
                    for (int j = 0; j < arrayHeight; j++)
                    {
                        aImage[j, i] = 0;
                        aImage[j, i + 1] = 0;
                    }
                }
                for (int j = circK; j < arrayHeight; j += 50)
                {
                    for (int i = 0; i < arrayWidth; i++)
                    {
                        aImage[j, i] = 0;
                        aImage[j + 1, i] = 0;
                    }
                }
            }
            return aImage;
        }


       

        static bool IsTimeForStim(int x, int y)
        {
            return !((int)Math.Sqrt((x - circH) * (x - circH) + (y - circK) * (y - circK)) < 400); //Unmtil; 25Mar14, has always been 200 R
        }

       


        #region Misc Circle_Ellipse
        static int circPointGenPosR(int H, int K, int x, int R)
        {
            return ((int)(Math.Sqrt(R * R - (x - H) * (x - H)) + K));
        }

        static int circPointGenNegR(int H, int K, int x, int R)
        {
            return ((int)(-Math.Sqrt(R * R - (x - H) * (x - H)) + K));
        }

        static int circPointGenPos(int H, int K, int x)
        {
            return ((int)(Math.Sqrt(eyeR * eyeR - (x - H) * (x - H)) + K));
        }

        static int circPointGenNeg(int H, int K, int x)
        {
            return ((int)(-Math.Sqrt(eyeR * eyeR - (x - H) * (x - H)) + K));
        }

        static int findEllStart(int pixX)
        {
            return ((int)(-Math.Sqrt(ellMY * ellMY * (1 - ((pixX - ellH) * (pixX - ellH)) / (ellMX * ellMX))) + ellK));
        }

        static int findEllEnd(int pixY)
        {
            return ((int)(Math.Sqrt(ellMY * ellMY * (1 - ((pixY - ellH) * (pixY - ellH)) / (ellMX * ellMX))) + ellK));
        }

        static int findCircStart(int pixX)
        {
            return ((int)(-Math.Sqrt(circR * circR - (pixX - circH) * (pixX - circH)) + circK));
        }

        static int findCircEnd(int pixY)
        {
            return ((int)(Math.Sqrt(circR * circR - (pixY - circH) * (pixY - circH)) + circK));
        }


        static bool IsInEllipse(int x, int y)
        {
            return (((float)((x - ellH) * (x - ellH)) / (ellMX * ellMX) + (float)((y - ellK) * (y - ellK)) / (ellMY * ellMY)) <= 1);
        }

        static bool IsInCircle(int x, int y)
        {
            return (((x - circH) * (x - circH) + (y - circK) * (y - circK)) <= (circR * circR));
        }
        #endregion


        //This is where al lof the displayed variables in the 
       // window are updated
        void acquisitionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //  Update the UI with the information passed from the background worker thread.
            //uint bufferNumber = (uint)e.UserState;
           // bufNumTextBox.Text = bufferNumber.ToString();
            UIUpdateArgs updateArgs = (UIUpdateArgs)e.UserState;
            veloBox.Text = updateArgs.velo.ToString();
            ////pixelValTextBox.Text = updateArgs.pixelValue;
            darkestPixValue.Text = updateArgs.pixCoord;
            //darkestPixCoord.Text = updateArgs.pixelValue;
            fishAngleBox.Text = updateArgs.rotAngle.ToString();
            //blobNum.Text = updateArgs.blobNumber.ToString();
            dataNum.Text = (updateArgs.dataNumber / 1000).ToString();
            plzWrk.ArrayToImage(updateArgs.debug);
            ////Image.ArrayToImage(updateArgs.debug);
            fpsBox.Text = updateArgs.fps.ToString();
            blobNum.Text = updateArgs.blobNumber.ToString();
            
            //if (updateArgs.stimReporter)
            //{
            //    OMRdot.BackColor = Color.LawnGreen;
            //    OMRdot.ForeColor = Color.LawnGreen;
            //    BLOBDot.BackColor = Color.Black;
            //    BLOBDot.ForeColor = Color.Black;
            //}
            //else
            //{
            //    OMRdot.BackColor = Color.Black;
            //    OMRdot.ForeColor = Color.Black;
            //    if (updateArgs.stimDirection == 0)
            //    {
            //        //BLOBDot.BackColor = Color.LawnGreen;
            //        BLOBDot.ForeColor = Color.LawnGreen;
            //        BLOBDot.Text = "0";// "SB";// "TR";
                       
            //    }
            //    else if (updateArgs.stimDirection == 1)
            //    {
            //        //BLOBDot.BackColor = Color.Red;
            //        BLOBDot.ForeColor = Color.Red;
            //        BLOBDot.Text = "32";// "TL"; //"MS";//
            //    }
            //    else if (updateArgs.stimDirection == 2)
            //    {
            //        //BLOBDot.BackColor = Color.Blue;
            //        BLOBDot.ForeColor = Color.Blue;
            //        BLOBDot.Text = "64";// "MB"; //"BR";
            //    }
            //    else if (updateArgs.stimDirection == 3)
            //    {
            //        //BLOBDot.BackColor = Color.Orange;
            //        BLOBDot.ForeColor = Color.Orange;
            //        BLOBDot.Text = "96";// "BL";//"SS";//
            //    }
            //    else if (updateArgs.stimDirection == 4)
            //    {
            //        //BLOBDot.BackColor = Color.Orange;
            //        BLOBDot.ForeColor = Color.Purple;
            //        BLOBDot.Text = "128";// "FB"; // "MR";
            //    }
            //    else if (updateArgs.stimDirection == 5)
            //    {
            //        //BLOBDot.BackColor = Color.Orange;
            //        BLOBDot.ForeColor = Color.Yellow;
            //        BLOBDot.Text = "160"; //"FS";//
            //    }
            //    else if (updateArgs.stimDirection == 6)
            //    {
            //        //BLOBDot.BackColor = Color.Orange;
            //        BLOBDot.ForeColor = Color.White;
            //        BLOBDot.Text = "SSB";// "NS";
            //    }
            //    else if (updateArgs.stimDirection == 7)
            //    {
            //        BLOBDot.ForeColor = Color.Gray;
            //        BLOBDot.Text = "SSS";// "NS";
            //    }
            //    else if (updateArgs.stimDirection == 8)
            //    {
            //        BLOBDot.ForeColor = Color.Gray;
            //        BLOBDot.Text = "IN";// "NS";
            //    }
            //}
        }

        void acquisitionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //  The background worker thread has completed its execution.  Perform any cleanup here.
            if (e.Result is ImaqException)
            {
                //  If we get here it means that we had an error in the background worker thread
                //  that we need to handle.
                MessageBox.Show(((ImaqException)e.Result).ToString(), "NI-IMAQ Error");
            }
            Cleanup();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_session != null)
                {
                    //  Signal the background worker thread to stop and then stop the acquisition.
                    acquisitionWorker.CancelAsync();
                    
                    _session.Close();
                }
                if(displayCam!=null)
                    displayCam.Suspend();
            }
            catch (ImaqException ex)
            {
                MessageBox.Show(ex.Message, "NI-IMAQ Error");
            }
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            //  Clean up and exit.
            stopButton.PerformClick();
            this.Close();
            Application.Exit();
        }

        private void Cleanup()
        {
            if (_session != null)
            {
                // Close the session.
                _session.Close();
                _session = null;
            }
            //  Update the UI.
            startButton.Enabled = true;
            stopButton.Enabled = false;
        }

        TestStim secondForm = new TestStim();

        private void stimWin_Click(object sender, EventArgs e)
        {
            secondForm.Show();
        }

        

        private void camWin_Click(object sender, EventArgs e)
        {
            if (startButton.Enabled == true)
                MessageBox.Show("Start the acquistion first!");
            else
            {
                viewerForm = new CamDisplay(_session);
                viewerForm.Show();
                displayCam = new Thread(new ParameterizedThreadStart(viewerForm.Grabber));
                displayCam.Start(this);
            }
        }

        private void fileWrite_Click(object sender, EventArgs e)
        {
            if (WriteFile == false)
            {
                WriteFile = true;
                fileWrite.Text = "Stop Writing";
                //string nameOfFile = "C:\\Users\\twd\\data\\" + System.DateTime.Today.Month + "_" + 
                //System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + ".bin";
                string filetimestamp = DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");
                string nameOfFile = "C:/Users/Gokul/Documents/DataBeta/FishParams/" + filetimestamp + "_loom.bin";
                dataFile = new FileStream(nameOfFile, FileMode.Append);
                dataFileWriter = new BinaryWriter(dataFile);
               // dataFileWriter = new StreamWriter(dataFile);
                nameOfFile = "C:/Users/Gokul/Documents/DataBeta/FishParams/" + filetimestamp + "_BG.bin";
                BGFile = new FileStream(nameOfFile, FileMode.Append);
                BGFileWriter = new BinaryWriter(BGFile);
                //string nameOfFileBG = "C:\\Users\\twd\\data\\" + System.DateTime.Today.Month + "_" +
                //    System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + "_bgsub.bin";
                //dataFileBG = new FileStream(nameOfFileBG, FileMode.Append);
                //dataFileWriterBG = new BinaryWriter(dataFileBG);

            }
            else
            {
                WriteFile = false;
                fileWrite.Text = "Write to File";
                dataFile.Close();
                BGFile.Close();
                //dataFileBG.Close();
            }
        }

        private void calibrateBox_CheckedChanged(object sender, EventArgs e)
        {
            if (calibrateBox.Checked)
            {
                bgVar1 = 15;
                bgVar2 = 15;
                bgVar3 = 255;
                bgVar4 = 150;
                millitime = System.DateTime.Now.Millisecond;
            }
            else
            {
                bgVar1 = 30;
                bgVar2 = 20;
                bgVar3 = 40;
                bgVar4 = 100;
            }

        }

        //Relays control value changes to Stim Window via public sender variables.
        private void xPos_ValueChanged(object sender, EventArgs e)
        {
            locationXsender = (int)xPos.Value;
        }

        private void yPos_ValueChanged(object sender, EventArgs e)
        {
            locationYsender = (int)yPos.Value;
        }

        private void aspRatio_ValueChanged(object sender, EventArgs e)
        {
            aspRatioSender = (float)aspRatio.Value;
        }

        private void horizSize_ValueChanged(object sender, EventArgs e)
        {
            sizeHorizsender = (int)horizSize.Value;
        }

        private void vertSize_ValueChanged(object sender, EventArgs e)
        {
            sizeVertsender = (int)vertSize.Value;
        }

        private void blobX_ValueChanged(object sender, EventArgs e)
        {
            lockX = (int)blobX.Value;
        }

        private void blobY_ValueChanged(object sender, EventArgs e)
        {
            lockY = (int)blobY.Value;
        }

        private void blobScaling_ValueChanged(object sender, EventArgs e)
        {
            scaleFactor = (float)blobScaling.Value;
        }

        private void blobScalingX_ValueChanged(object sender, EventArgs e)
        {
            scaleFactorX = (float)blobScalingX.Value;
        }

        private void lockXBox_TextChanged(object sender, EventArgs e)
        {
            //lockX = Int32.Parse(lockXBox.Text);
        }

        private void lockYBox_TextChanged(object sender, EventArgs e)
        {
            //lockY = Int32.Parse(lockYBox.Text);
        }

        private void updateangle_Click(object sender, EventArgs e)
        {
            //anglechoice = float.Parse(chooseAngle.Text);
        }


        private void updateProgBar(int progSet)
        {
            if (this.InvokeRequired)
            {
                updateProgBarCallback d = new updateProgBarCallback(updateProgBar);
                this.Invoke(d, new object[] { progSet });
            }
            else
                if (this.bgProgBar.Value + progSet <= 100)
                    this.bgProgBar.Value += progSet;
        }

        private void GratingsOn(bool OMRstate)
        {
            if (this.InvokeRequired)
            {
                GratingsOnCallback d = new GratingsOnCallback(GratingsOn);
                this.Invoke(d, new object[] { OMRstate });
            }
            else
                gratingsON.Checked = OMRstate;
        }

  /*      private void BufferUpdate(bool update)
        {
            if (this.InvokeRequired)
            {
                BufferUpdateCallback d = new BufferUpdateCallback(BufferUpdate);
                this.Invoke(d, new object[] { update });
            }
            else
                myBufferReady = false;

        }

  */

        private void fullFrameWrite_CheckedChanged(object sender, EventArgs e)
        {
            if (WriteFull == false)
            {
                WriteFull = true;
                string nameOfFullFile = "C:/Users/Gokul/Documents/DataBeta/FishParams/" + System.DateTime.Today.Month + "_" +
                    System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + "_fullFrameMovie.bin";
                //string nameOfFullFile = "C:\\Users\\twd\\data\\" + System.DateTime.Today.Month + "_" +
                //    System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + "_fullFrameMovie.bin";
                dataFullFrame = new FileStream(nameOfFullFile, FileMode.Append);
                fullFrameWriter = new BinaryWriter(dataFullFrame);
            }
            else
            {
                WriteFull = false;
                dataFullFrame.Close();
            }
        }

        private void takeBG_Click(object sender, EventArgs e)
        {
            if (!BGsnap)
                BGsnap = true;
            if (BGtaken)
            {
                bgGenLoop = true;
                BGtaken = false;
                bgGenerateCount = 0;
            }
        }

        private byte[,] BGSubtract(byte[,] im, Matrix<Byte> back, int thresh, ref bool flag, ref int coordX, ref int coordY)
        {
            //If we haven't found the fish, collect and subtract the entire image
            //To make this fast, use EmguCV subtraction and thresholding by first copying the 2D image array im into a 1D array, 
            //IMrow, which can then be assigned to an OpenCV image instance that is used for subtraction. In the end, the linearized
            //bytes from the final, subtracted, thresholded image are fed back to a matrix, and the matrix data is returned as a 2D array.
            if (!flag)
            {
                //initialize new image
                Image<Gray, Byte> holder = new Image<Gray, byte>(arrayWidth, arrayHeight);
                Image<Gray, Byte> holder2 = new Image<Gray, byte>(arrayWidth, arrayHeight);
                Matrix<Byte> IMmatbuffer = new Matrix<byte>(arrayHeight, arrayWidth);
                //linearize 2D array
                byte[] linIMbuffer = new byte[arrayWidth* arrayHeight];
                System.Buffer.BlockCopy(im, 0, linIMbuffer, 0, arrayWidth* arrayHeight);
                //assign pixel values
                holder.Bytes = linIMbuffer;
                holder2.Bytes = back.Bytes;
                //subtract and threshold
                IMmatbuffer.Bytes = (holder.AbsDiff(holder2)).ThresholdBinary(new Gray(thresh), new Gray(255)).Bytes;
                if (IMmatbuffer.Sum > 5 && zoom)
                    flag = true;
                coordX = 30;
                coordY = 30;
                return IMmatbuffer.Data;
            }
            //If we have found the fish, we first want to subsample the image in an ROI around the previous center of mass to speed
            //up operations. This means all of our images and 2D arrays can be smaller. Because it is faster to work with OpenCv matrices
            //than images, as much of the processing as possible is first done in matrix form. 
            else
            {
                int cutsize = 60;
                int cX = coordX - cutsize/2;
                int cY = coordY - cutsize/2;
                //initialize a small image and a small matrix
                Matrix<Byte> test = new Matrix<byte>(cutsize, cutsize);
                //Matrix<Byte> test2 = new Matrix<byte>(1024, 1280);
                Matrix<Byte> matholder2 = new Matrix<byte>(cutsize, cutsize);
                //Matrix<Byte> test2 = new Matrix<byte>(52, 50);
                Image<Gray, Byte> holder = new Image<Gray, byte>(cutsize, cutsize);
                Image<Gray, Byte> holder2 = new Image<Gray, byte>(cutsize, cutsize);

                //initialize matrix for subsampling
                Matrix<Byte> matholder = new Matrix<byte>(im);

                //subsample, then COPY SUBSAMPLED DATA TO NEW, SMALLER MATRIX. THIS IS NECESSARY FOR SOME REASON, AND TOOK ME AGES TO FIGURE OUT
                matholder = matholder.GetSubRect(new Rectangle(cX, cY, cutsize, cutsize));
                //matholder.CopyTo(test2);
                matholder.CopyTo(test);
                //Copy SubRect to image
                holder.Bytes = test.Bytes;

                
                back = back.GetSubRect(new Rectangle(cX, cY, cutsize, cutsize));
                back.CopyTo(matholder2);
                holder2.Bytes = matholder2.Bytes;
                //subtract and threshold
                //test.Bytes = (holder.AbsDiff(holder2)).ThresholdBinary(new Gray(thresh), new Gray(255)).Bytes;
                test.Bytes = (holder.AbsDiff(holder2)).ThresholdToZero(new Gray(thresh)).Bytes;
                //holder3.Bytes = (holder.AbsDiff((back.GetSubRect(new Rectangle(cX, cY, 52, 50))))).ThresholdBinary(new Gray(thresh), new Gray(255)).Bytes;
                //test.Bytes = holder3.Bytes;
                if (test.Sum < 5)
                    flag = false;

                return test.Data;
            }
        }

        private void bgInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                BGtime = Int32.Parse(bgInterval.Text);
            }

        }

        private void zoomoutbutt_Click_1(object sender, EventArgs e)
        {
            zoom = false;
        }

        private void zoominbutt_Click_1(object sender, EventArgs e)
        {
            zoom = true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                binthresh = Int32.Parse(textBox1.Text);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            showFore = !showFore;
                
        }

        private void button2_Click(object sender, EventArgs e)
        {
            showBack = !showBack;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult ConfigOK = ConfigWindow.ShowDialog();
            
            if (ConfigOK == DialogResult.OK)
            {
                sParams = ConfigWindow.stimParams;
            }
        }

        private void UpdateToggleStims()
        {



            if (calibrateBox.Checked)
            {
                //MessageBox.Show((System.DateTime.Now.Millisecond - millitime).ToString());
                if ((BlobStart && WriteFull) || (WriteFull && spontBox.Checked && !spontBoxLowFPS.Checked))
                {
                    if (imbuffCount == 100)
                        locationXsender = 2000;
                    if (imbuffCount < 1500)
                    {
                        bytesOfData++;

                        System.Buffer.BlockCopy(extractedPixels.U8, 0, timIMbuff, imbuffCount * fullSize, fullSize);

                        imbuffCount++;
                    }
                    else if (imbuffCount < 2000)
                    {
                        bytesOfData++;
                        System.Buffer.BlockCopy(extractedPixels.U8, 0, timIMbuff2, (imbuffCount - 1500) * fullSize, fullSize);

                        imbuffCount++;
                    }

                    if (WriteFull && imbuffCount == 2000)
                    {
                        fullFrameWriter.Write(timIMbuff);
                        fullFrameWriter.Write(timIMbuff2);

                    }
                    if (imbuffCount == 2000)
                    {
                        imbuffCount = 0;
                        WriteFull = false;
                    }

                }

            }
            else
            {

                if (!spontBox.Checked)
                {
                    //this is somehow used to toggle OMR gratings on/off once the fish reaches the center of dish
                    if (gratingsON.Checked)
                        TryStim = true;
                    else
                        TryStim = false;


                    //StimOff = true is when the fish is not in center of dish.
                    //StimOff = false is when the fish enters 100 pixel radius from center, 
                    //calculated in IsTimeForStim
                    StimOff = IsTimeForStim(coordEyeAX, coordEyeAY);



                    if (!StimOff && !StartShow)
                    {
                        TryStim = false;
                        GratingsOn(false);
                        StartShow = true;

                        bgCount = 0;
                        stopwatch.Reset();
                        stopwatch.Start();
                        BlobDelay.Reset();
                        BlobDelay.Start();
                        // blobFlash = true;
                         doubleBlob = true;


                    }
                    if (StartShow && !BlobStart && BlobDelay.ElapsedMilliseconds > sParams[0].tInterval)
                    {
                       

                        StartShowFlash = false;

                        blobFlash = false;
                        doubleBlob = true;
                        halfBlob = false;
                        shrinking = false;
                        annulus = false;
                        loomSquare = false;
                        OMRtrial = false;
                        
                        conditions = condArr[blobsShown];
                         blobsShown++;
                        if (!OMRtrialBox.Checked)
                        {

                            if (sParams[conditions].trialType == TrialType.Radial)
                            {
                                //conversion is a scale factor that converts floating points values in program to real-life centimeters
                                conversion = sParams[conditions].cFactor;
                                // parameter setting the expansion speed (I believe in cm/s)
                                blobSpeed = sParams[conditions].radSpeed * 40f / sParams[conditions].cFactor;
                                // this is used to set the relative position of the stimulus (relative to the fish body axis)
                                adtlAngle = sParams[conditions].angle / 180 * (float)Math.PI;
                                // this is the distance betwee nthe center of spot and the fish
                                approachDist = sParams[conditions].distoff * 40f / sParams[conditions].cFactor;
                                // this sets the contrast of the stim
                                contrastValue = (int)sParams[conditions].contrast;
                                // total time of before stimulus disappears
                                stimLength = sParams[conditions].sLength / 1000 * 60;
                                // this sets the starting size of the spot
                                radStartSize = sParams[conditions].begRadSize * 40f / sParams[conditions].cFactor;
                                // this sets the final size of the spot
                                radEndSize = sParams[conditions].endRadSize * 40f / sParams[conditions].cFactor;
                                // calculated internally to know when to stop expansion
                                counterstop = (int)Math.Round(((sParams[conditions].endRadSize * 40f / sParams[conditions].cFactor - radStartSize) /
                                    blobSpeed * 60));
                                //MessageBox.Show(((sParams[conditions].endRadSize * 40f / sParams[conditions].cFactor - radStartSize) /
                                //    blobSpeed * 60).ToString() + " " + counterstop.ToString());
                                //OK, remember to remove this!! This is a shortcut for implementing "jumps" on Jan31_2014 only!!!
                                //ALSO USE TO IMPLEMENT DIMMING STIMS WHEN RAD SPEED == 0
                                if (sParams[conditions].radSpeed > 50)
                                    bgCountStop = 2;
                                else if (sParams[conditions].radSpeed == 0)
                                {
                                    //load dimming timecourse array
                                    dimTime = readDimFile();
                                    bgCountStop = 3; // set flag
                                }
                                else
                                {
                                    Random textType = new Random();
                                    int stimType = textType.Next(2);
                                    if (stimType == 0)
                                        bgCountStop = 0;
                                    else
                                        bgCountStop = 5;
                                }
                                
                            }
                            else if (sParams[conditions].trialType == TrialType.Approach)
                            {
                                bgCountStop = 1;
                                conversion = sParams[conditions].cFactor;
                                stimSpeed = sParams[conditions].appSpeed;
                                adtlAngle = sParams[conditions].angle / 180 * (float)Math.PI;
                                stimDist = sParams[conditions].appDist;
                                approachDist = sParams[conditions].distoff * 40f / sParams[conditions].cFactor;
                                contrastValue = (int)sParams[conditions].contrast;
                                stimLength = sParams[conditions].sLength / 1000 * 60;
                                stimSize = sParams[conditions].appSize;
                                counterstop = (int)(sParams[conditions].appDist / sParams[conditions].appSpeed * 60);
                            }
                            else //Random Velo Walk
                            {
                                //sizeTime = getNextSizeTime(allSize, currSizeInd, sParams[conditions].sLength, sParams[conditions].cFactor);
                                sizeTime = getNextSizeTime(allSize, currSizeInd, sParams[conditions].sLength, sParams[conditions].cFactor);

                                currSizeInd = currSizeInd + sParams[conditions].sLength; //used to have a -1 here (erroneously)
                                bgCountStop = 4;
                                stimLength = sParams[conditions].sLength / 1000 * 60;
                                adtlAngle = sParams[conditions].angle / 180 * (float)Math.PI;
                                approachDist = sParams[conditions].distoff * 40f / sParams[conditions].cFactor;
                                radStartSize = 1;
                            }

                        }
                        else
                        {

                            switch (conditions)
                            {
                                case 0:
                                    adtlAngle = 0;
                                    OMRtrial = true;

                                    break;
                                case 1:
                                    adtlAngle = (float)Math.PI;
                                    OMRtrial = true;
                                    break;
                            }
                        }

                        bgCount = 0;
                        BlobStart = true;
                        //blobFlash = true;
                        //MessageBox.Show(lockX.ToString() + " " + lockY.ToString());

                        stopwatch.Reset();
                        stopwatch.Start();
                        BlobDelay.Reset();
                        lockAngle = rotAngle;

                        //used to stop the program after a specified time or number of stimulus presentations
                        if (false)//blobsShown % 60 == 0)
                        {
                            downTime = true;
                            Thread.Sleep(300000000);
                            downTime = false;
                            stopwatch.Reset();
                            stopwatch2.Reset();
                            //stopwatch3.Reset();
                            fpswatch.Reset();
                            stopwatch.Start();
                            stopwatch2.Start();
                            //stopwatch3.Start();
                            fpswatch.Start();
                            frames = 0;
                            grabNumberDisp = 0;
                            bgCount = 0;
                            BlobStart = false;
                            HalfMoonStart = false;
                            TryStim = true;
                            StimOff = true;
                            rotAngle = 0;
                            lockAngle = 0;
                            //conditions = 0;
                            //blobsShown = 0;
                        }
                        if (WriteFile)
                        {
                            dataFileWriter.Write(float.PositiveInfinity);
                            //dataFileWriter.Write(lockAngle);
                            dataFileWriter.Write(adtlAngle);
                            if (sParams[conditions].trialType == TrialType.Radial)
                                dataFileWriter.Write(blobSpeed);
                            else if (sParams[conditions].trialType == TrialType.Approach)
                                dataFileWriter.Write(stimSpeed);
                            else
                            {
                                int ind = (int)(bgCount * 17);
                                if (ind >= sizeTime.Length) //this shouldn't be necessary
                                    ind = sizeTime.Length - 1;
                                dataFileWriter.Write((float)sizeTime[ind]);
                            }
                            //dataFileWriter.Write(sParams[conditions].trialType == TrialType.Radial ? blobSpeed : stimSpeed);

                            //dataFileWriter.Write((float)counterstop);

                            //dataFileWriter.Write((float)SquareCondition);
                            //dataFileWriter.Write(halfAngle);
                            dataFileWriter.Write((float)bgCountStop);
                            //dataFileWriter.Write((float)contrastValue);
                            dataFileWriter.Write(approachDist);
                            //dataFileWriter.Write((float)tapTimer);
                            //dataFileWriter.Write(doubleBlob ? 1f : 0f);
                            //dataFileWriter.Write(StartShowFlashToggle ? 1f : 0f);

                        }

                    }



                    if (bgCount > sParams[0].tLength/1000*60 && BlobStart)
                    {
                        BlobStart = false;
                        HalfMoonStart = false;
                        blobFlash = false;
                        //tapper.Suspend();
                        bgCount = 0;
                        //TryStim = true;
                        //gratingsON.Visible = true;
                        StartShow = false;
                        OMRtrial = false;
                        //rotAngle = 0;
                        //refAngle = 0.25f;
                        //refAngle2 = 0.5f;
                        //lockAngle = 0;
                        stopwatch.Reset();
                        stopwatch.Start();
                        stopwatch2.Reset();
                        stopwatch2.Start();
                        imbuffCount = 0;
                        //OMRswitchCounter = 0;
                    }
                    if (veloOUT > 1.7 && BlobStart && !OMRtrial && sParams[0].veloshut)
                    {
                        //approachDist = 500f;
                        //compareBlobTime = bgCount;
                        stimSize = 0;
                        blobSpeed = 0;
                        radStartSize = 0;
                       
                    }


                }

                if (fileWrite.Text == "Stop Writing")
                {
                    if (coordEyeAY < 30 || coordEyeAY > 993)
                        WriteFile = false;
                    else
                        WriteFile = true;
                }

                if ((WriteFile && BlobStart) || (WriteFile && spontBox.Checked && grabNumberDisp > 10))
                {
                    bytesOfData += 6416;
                    //write time stamp here
                    //    dataFileWriter.Write((float)stopwatch.ElapsedMilliseconds);
                    dataFileWriter.Write((float)RealFrameCount);
                    dataFileWriter.Write((float)RealTimeCounter);
                    long sysTimer = Gtimer.ElapsedMilliseconds;
                    dataFileWriter.Write((float)sysTimer);

                    dataFileWriter.Write((float)coordEyeAX);
                    //dataFileWriter.Write((float)realbuff);
                    dataFileWriter.Write((float)coordEyeAY);
                    //int ind = (int)(bgCount * 17);
                    //if (ind >= sizeTime.Length) //this shouldn't be necessary
                    //    ind = sizeTime.Length - 1;
                    //dataFileWriter.Write((float)sizeTime[ind]);
                    dataFileWriter.Write(rotAngle);

                    //      WriteImage(coordEyeAX, coordEyeAY, extractedPixels.U8, subtractedArray, true);
                    //       WriteImage(coordEyeAX, coordEyeAY, extractedPixels.U8, subtractedArray, false);

     //               WriteImage(coordEyeAX, coordEyeAY, CXP_image, subtractedArray, true);


    //                dataFileWriter.Write((float)LEDStatus);
    //                dataFileWriter.Write((float)LED_intensity);

                }

                //for(int i =0;i < 30000000;i++)
              
                if (triggerBox.Checked)
                {
                    int numbuffs = 1499 - imbuffCount % 1500;
 //                   System.Buffer.BlockCopy(extractedPixels.U8, 0, timIMbuff, numbuffs * fullSize, fullSize);
                    imbuffCount++;
                    if (writeTrig)
                    {
                        writeTrig = false;
                        fullFrameWriter.Write(timIMbuff);
                    }
                }

                if ((BlobStart && WriteFull) || (WriteFull && spontBox.Checked && grabNumberDisp > 10 && !spontBoxLowFPS.Checked))
                {
                
                    if (imbuffCount < 1500)
                    {
                        bytesOfData++;

                        System.Buffer.BlockCopy(extractedPixels.U8, 0, timIMbuff, imbuffCount * fullSize, fullSize);
                       
                        imbuffCount++;
                    }
                    else if (imbuffCount < 2000)
                    {
                        bytesOfData++;
                        System.Buffer.BlockCopy(extractedPixels.U8, 0, timIMbuff2, (imbuffCount-1500) * fullSize, fullSize);

                        imbuffCount++;
                    }
                    
                    if (WriteFull && imbuffCount == 2000)
                    {
                        fullFrameWriter.Write(timIMbuff);
                        fullFrameWriter.Write(timIMbuff2);

                    }
                    if (imbuffCount == 2000)
                        imbuffCount = 0;

                }
                else if ((WriteFull && spontBox.Checked && grabNumberDisp > 10 && spontBoxLowFPS.Checked))
                {
                    System.Buffer.BlockCopy(extractedPixels.U8, 0, singleIMbuff, 0, fullSize);
                    singleIMbuff[0] = (byte)(arrayWidth / 255);
                    singleIMbuff[1] = (byte)(arrayWidth % 255);
                    singleIMbuff[2] = (byte)(arrayHeight / 255);
                    singleIMbuff[3] = (byte)(arrayHeight % 255);
                    fullFrameWriter.Write(singleIMbuff);
                }

            }
        }

        private void triggerBox_CheckedChanged(object sender, EventArgs e)
        {
            //Note: this shares a filestream with WriteFull --> cannot both be checked!!
            if (triggerBox.Checked)
            {
             
                string nameOfFullFile = "C:/Users/Gokul/Documents/DataBeta/FishParams/" + System.DateTime.Today.Month + "_" +
                    System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + "_fullFrameMovie.bin";
                //string nameOfFullFile = "C:\\Users\\twd\\data\\" + System.DateTime.Today.Month + "_" +
                //    System.DateTime.Today.Day + "_" + System.DateTime.Today.Year + "_" + fileNumber.Text + "_fullFrameMovie.bin";
             //   dataFullFrame = new FileStream(nameOfFullFile, FileMode.Append);
             //   fullFrameWriter = new BinaryWriter(dataFullFrame);
            }
            else
            {
 
                dataFullFrame.Close();
            }
        }

        private void writeBufferButton_Click(object sender, EventArgs e)
        {
            writeTrig = true;
        }

        private double[] readDimFile()
        {
            
            dimFile = new FileStream(dimFileName, FileMode.Open);
            dimFileReader = new BinaryReader(dimFile);
            double[] dimArr = new double[dimFileReader.BaseStream.Length/8]; //calculate the number of values in the binary file
            for (int i = 0; i < dimArr.Length; i++)
                dimArr[i] = dimFileReader.ReadDouble();
            dimFileReader.Close();
            dimFile.Close();
            return dimArr;
        }

        private double[] readSizeFile() //reads in an entire, multiple trial binary file over random velo/sizre generated by matlab
        {
            sizeFile = new FileStream(sizeFileName, FileMode.Open);
            sizeFileReader = new BinaryReader(sizeFile);
            double[] sizeArr = new double[sizeFileReader.BaseStream.Length / 8]; //calculate the number of values in the binary file
            for (int i = 0; i < sizeArr.Length; i++)
                sizeArr[i] = sizeFileReader.ReadDouble();
            sizeFileReader.Close();
            sizeFile.Close();
           
            return sizeArr;
        }

        private double[] getNextSizeTime(double[] sizes, int startInd, int getLength, float converter)
        {
            //Takes an array containing all velo walk trials concatenated and the starting index of the desired trial. Returns and array beginning
            //at the desired index and getLnegth long.
            double[] nextSizes = new double[getLength];
            for (int i = 0; i < getLength; i++)
                nextSizes[i] = sizes[startInd + i] * 40f/converter; 
           
            return nextSizes;
        }

        uint getFrameCount(byte[,] im)
        {
            byte[] inFrameCount = new byte[4];
            Array.Reverse(inFrameCount);
            System.Buffer.BlockCopy(im, 100, inFrameCount, 0, 4);
            //MessageBox.Show(System.BitConverter.ToUInt32(inFrameCount, 0).ToString());
            return System.BitConverter.ToUInt32(inFrameCount, 0);
        }

        uint getCurrentFrame(byte[,] im)
        {
            /* ID frame number */
            byte[] RealFrameStamp = new byte[4];
            System.Buffer.BlockCopy(im, 0, RealFrameStamp, 0, 3);
            return System.BitConverter.ToUInt32(RealFrameStamp, 0);
            //   Console.WriteLine("{0}", RealFrameCount);
        }

    }



    

}