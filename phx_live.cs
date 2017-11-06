/****************************************************************************
 *
 * ACTIVE SILICON LIMITED
 *
 * File name   : phx_live.cs
 * Function    : Example simple acquisition and display application
 * Updated     : 13-Feb-2015
 *
 * Copyright (c) 2015 Active Silicon
 ****************************************************************************
 * Comments:
 * --------
 * This example shows how to initialise the frame grabber and use the Display
 * library to run live double buffered (also known as ping-pong) acquisition,
 * using a callback function.
 * It also shows how to use the image conversion function. It captures into
 * a direct buffer, and then converts the image data into a format suitable
 * for display. This reduces the amount of PCI bandwidth used.
 *
 ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices; /* DllImport */
using ActiveSilicon;

namespace phx_live
{
   class Program
   {
      /* Define an application specific structure to hold user information */
      public struct tPhxLive {
         public volatile uint dwBufferReadyCount;
         public volatile bool fBufferReady;
         public volatile bool fFifoOverflow;
      };

      /*
      phxlive_callback()
       * This is the callback function which handles the interrupt events.
       */
      unsafe static void phxlive_callback(
         uint     hCamera,          /* Camera handle. */
         uint     dwInterruptMask,  /* Interrupt mask. */
         IntPtr   pvParams          /* Pointer to user supplied context */
      )
      {
         tPhxLive *psPhxLive = (tPhxLive*) pvParams;

         /* Handle the Buffer Ready event */
         if ((uint)Phx.etParamValue.PHX_INTRPT_BUFFER_READY == (dwInterruptMask & (uint)Phx.etParamValue.PHX_INTRPT_BUFFER_READY)) {
            /* Increment the Display Buffer Ready Count */
            psPhxLive->dwBufferReadyCount++;
            psPhxLive->fBufferReady = true;
         }

         /* FIFO Overflow */
         if ((uint)Phx.etParamValue.PHX_INTRPT_FIFO_OVERFLOW == (dwInterruptMask & (uint)Phx.etParamValue.PHX_INTRPT_FIFO_OVERFLOW)) {
            psPhxLive->fFifoOverflow = true;
         }

         /* Note:
          * The callback routine may be called with more than 1 event flag set.
          * Therefore all possible events must be handled here.
          */
      }

      /*
      phxlive()
       * Simple live capture application code, using image conversion in order to reduce
       * the amount of PCI bandwidth used.
       */
      unsafe static int phxlive(
         Phx.etParamValue        eBoardNumber,        /* Board number, i.e. 1, 2, or 0 for next available */
         Phx.etParamValue        eChannelNumber,      /* Channel number */
         String                  strConfigFileName,   /* Name of config file */
         PhxCommon.tCxpRegisters sCameraRegs          /* Camera CXP registers */
      )
      {
         Phx.etStat              eStat                   = Phx.etStat.PHX_OK;    /* Status variable */
         Phx.etParamValue        eAcqType                = 0;                    /* Parameter for use with PHX_ParameterSet/Get calls */
         Pbl.etPblParamValue     eBayCol                 = 0;
         Phx.etParamValue        eParamValue             = 0;
         Pbl.etPblParamValue     ePblCaptureFormat       = 0;
         Phx.etParamValue        eCamSrcCol              = 0;
         Phx.etParamValue        eCaptureFormat          = Phx.etParamValue.PHX_BUS_FORMAT_MONO8;
         Phx.etParamValue        eCamFormat              = 0;
         uint                    dwBufferReadyLast       = 0;                    /* Previous BufferReady count value */
         IntPtr                  pParamValue             = IntPtr.Zero;
         IntPtr                  pConfigFile             = IntPtr.Zero;
         PhxCommon               myPhxCommon             = new PhxCommon();
         Phx.PHX_AcquireCallBack PHX_Callback            = new Phx.PHX_AcquireCallBack(phxlive_callback);
         Phx.stImageBuff[]       asImageBuffers          = null;                 /* Capture buffer array */
         uint[]                  ahCaptureBuffers        = null;                 /* Capture buffer handle array */
         tPhxLive                sPhxLive;                                       /* User defined event context */
         uint                    hCamera                 = 0;                    /* Camera handle */
         uint                    hDisplay                = 0;                    /* Display handle */
         uint                    hDisplayBuffer          = 0;                    /* Display buffer handle */
         uint                    dwAcqNumBuffers         = 0;
         uint                    dwBufferWidth           = 0;
         uint                    dwBufferHeight          = 0;
         uint                    dwBufferStride          = 0;
         uint                    dwCamSrcDepth           = 0;
         bool                    fDebayer                = false;
         bool                    fCameraIsCxp            = false;
         bool                    fIsCxpCameraDiscovered  = false;


         /* Create a Phx handle */
         eStat = Phx.PHX_Create(ref hCamera, Phx.PHX_ErrHandlerDefault);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Set the configuration file name */
         if (!String.IsNullOrEmpty(strConfigFileName)) {
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

         /* Read various parameter values in order to generate the capture buffers. */
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ROI_XLENGTH,      ref dwBufferWidth);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ROI_YLENGTH,      ref dwBufferHeight);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CAM_SRC_DEPTH,    ref dwCamSrcDepth);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CAM_SRC_COL,      ref eCamSrcCol);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_BUS_FORMAT,       ref eCaptureFormat);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CAM_FORMAT,       ref eCamFormat);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ACQ_FIELD_MODE,   ref eAcqType);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_ACQ_NUM_BUFFERS,  ref dwAcqNumBuffers);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Interlaced Camera in Field Mode */
         if (Phx.etParamValue.PHX_CAM_INTERLACED == eCamFormat
            && (  Phx.etParamValue.PHX_ACQ_FIELD_12     == eAcqType
               || Phx.etParamValue.PHX_ACQ_FIELD_21     == eAcqType
               || Phx.etParamValue.PHX_ACQ_FIELD_NEXT   == eAcqType
               || Phx.etParamValue.PHX_ACQ_FIELD_1      == eAcqType
               || Phx.etParamValue.PHX_ACQ_FIELD_2      == eAcqType)) {
            dwBufferHeight /= 2;
         }

         /* Determine PHX_BUS_FORMAT based on the camera format */
         eStat = myPhxCommon.PhxCommonGetBusFormat(eCamSrcCol, dwCamSrcDepth, eCaptureFormat, ref eCaptureFormat);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Determine bayer format */
         fDebayer = true;
                if (Phx.etParamValue.PHX_CAM_SRC_BAYER_BG == eCamSrcCol) {
            eBayCol = Pbl.etPblParamValue.PBL_BAY_COL_BLU;
         } else if (Phx.etParamValue.PHX_CAM_SRC_BAYER_GB == eCamSrcCol) {
            eBayCol = Pbl.etPblParamValue.PBL_BAY_COL_GRNB;
         } else if (Phx.etParamValue.PHX_CAM_SRC_BAYER_GR == eCamSrcCol) {
            eBayCol = Pbl.etPblParamValue.PBL_BAY_COL_GRNR;
         } else if (Phx.etParamValue.PHX_CAM_SRC_BAYER_RG == eCamSrcCol) {
            eBayCol = Pbl.etPblParamValue.PBL_BAY_COL_RED;
         } else {
            fDebayer = false;
         }

         /* Update the PHX_BUS_FORMAT, as it may have changed (above) */
         eStat = Phx.PHX_ParameterSet(hCamera, (Phx.etParam.PHX_BUS_FORMAT | Phx.etParam.PHX_CACHE_FLUSH), ref eCaptureFormat);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Read back the Buffer Stride */
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_BUF_DST_XLENGTH, ref dwBufferStride);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Init the array of capture buffer handles */
         ahCaptureBuffers = new uint[dwAcqNumBuffers];

         /* Init the array of image buffers */
         asImageBuffers = new Phx.stImageBuff[dwAcqNumBuffers + 1];

         /* Create and initialise our capture buffers (not associated with display) */
         for (int i = 0; i < dwAcqNumBuffers; i++) {
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

            /* Set up the type of Bayer conversion required */
            if (fDebayer) {
               Pbl.etPblParamValue ePblParamValue = Pbl.etPblParamValue.PBL_BAY_DEC_DUP;
               eStat = Pbl.PBL_BufferParameterSet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_BUFF_BAYDEC, ref ePblParamValue);
               if (Phx.etStat.PHX_OK != eStat) goto Error;
               eStat = Pbl.PBL_BufferParameterSet(ahCaptureBuffers[i], Pbl.etPblParam.PBL_BUFF_BAYCOL, ref eBayCol);
               if (Phx.etStat.PHX_OK != eStat) goto Error;
            }

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

         /* We create our display with a NULL hWnd, this will automatically create an image window. */
         eStat = Pdl.PDL_DisplayCreate(ref hDisplay, IntPtr.Zero, hCamera, myPhxCommon.PhxCommonDisplayErrorHandler);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* We create a display buffer (indirect) */
         eStat = Pdl.PDL_BufferCreate(ref hDisplayBuffer, hDisplay, Pdl.etPdlBufferMode.PDL_BUFF_SYSTEM_MEM_INDIRECT);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Initialise the display, this associates the display buffer with the display */
         eStat = Pdl.PDL_DisplayInit(hDisplay);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Enable FIFO Overflow events */
         eParamValue = Phx.etParamValue.PHX_INTRPT_FIFO_OVERFLOW;
         eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_INTRPT_SET, ref eParamValue);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Setup our own event context */
         eStat = Phx.PHX_ParameterSet(hCamera, Phx.etParam.PHX_EVENT_CONTEXT, (void*) &sPhxLive);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Check if camera is CXP */
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_BOARD_VARIANT, ref eParamValue);
         if (Phx.etStat.PHX_OK != eStat) goto Error;
         if (  Phx.etParamValue.PHX_BOARD_FBD_4XCXP6_2PE8 == eParamValue
            || Phx.etParamValue.PHX_BOARD_FBD_2XCXP6_2PE8 == eParamValue
            || Phx.etParamValue.PHX_BOARD_FBD_1XCXP6_2PE8 == eParamValue) {
            fCameraIsCxp = true;
         }

         /* Check that camera is discovered (only applies to CXP) */
         if (fCameraIsCxp) {
            myPhxCommon.PhxCommonGetCxpDiscoveryStatus(hCamera, 10, ref fIsCxpCameraDiscovered);
            if (!fIsCxpCameraDiscovered) {
               goto Error;
            }
         }
         
         /* Now start our capture, using the callback method */
         eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_START, PHX_Callback);
         if (Phx.etStat.PHX_OK != eStat) goto Error;

         /* Now start camera */
         if (fCameraIsCxp && 0 != sCameraRegs.dwAcqStartAddress) {
            eStat = myPhxCommon.PhxCommonWriteCxpReg(hCamera, sCameraRegs.dwAcqStartAddress, sCameraRegs.dwAcqStartValue, 800);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
         }

         /* Continue processing data until the user presses a key in the console window
          * or a FIFO overflow is detected.
          */
         Console.WriteLine("Press a key to exit");
         while (0 == myPhxCommon.PhxCommonKbHit()) {
            /* Wait here until either:
             * (a) The user aborts the wait by pressing a key in the console window
             * (b) The BufferReady event occurs indicating that the image is complete
             * (c) The FIFO overflow event occurs indicating that the image is corrupt
             * Keep calling the sleep function to avoid burning CPU cycles
             */
            while (0 == myPhxCommon.PhxCommonKbHit() && !sPhxLive.fBufferReady && !sPhxLive.fFifoOverflow) {
               System.Threading.Thread.Sleep(10);
            }

            if (dwBufferReadyLast != sPhxLive.dwBufferReadyCount) {
               uint dwStaleBufferCount;
               /* If the processing is too slow to keep up with acquisition,
                * then there may be more than 1 buffer ready to process.
                * The application can either be designed to process all buffers
                * knowing that it will catch up, or as here, throw away all but the
                * latest
                */
               dwStaleBufferCount = sPhxLive.dwBufferReadyCount - dwBufferReadyLast;
               dwBufferReadyLast += dwStaleBufferCount;

               /* Throw away all but the last image */
               if (1 < dwStaleBufferCount) {
                  do {
                     eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
                     if (Phx.etStat.PHX_OK != eStat) goto Error;
                     dwStaleBufferCount--;
                  } while (dwStaleBufferCount > 1);
               }
            }
            sPhxLive.fBufferReady = false;

            Phx.stImageBuff stBuffer;
            stBuffer.pvAddress = IntPtr.Zero;
            stBuffer.pvContext = IntPtr.Zero;

            /* Get the info for the last acquired buffer */
            eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_GET, ref stBuffer);
            if (Phx.etStat.PHX_OK != eStat) {
               eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
               if (Phx.etStat.PHX_OK != eStat) goto Error;
               continue;
            }

                /* Process the newly acquired buffer,
                 * which in this simple example is a call to display the data.
                 * For our display function we use the pvContext member variable to
                 * pass a display buffer handle.
                 * Alternatively the actual video data can be accessed at stBuffer.pvAddress
                 */
                byte[] wtf = new byte[4036608];
                Marshal.Copy(stBuffer.pvAddress, wtf, 0, 4036608);
                // Console.WriteLine(wtf[0].ToString());
                for (int i = 50000; i < 500000; i++)
                    wtf[i] = 0;
                Marshal.Copy(wtf, 0, stBuffer.pvAddress, 4036608);     

                        uint hBufferHandle = (uint)stBuffer.pvContext;

            /* This copies/converts data from the direct capture buffer to the indirect display buffer */
            eStat = Pil.PIL_Convert(hBufferHandle, hDisplayBuffer);
            if (Phx.etStat.PHX_OK != eStat) {
               eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
               if (Phx.etStat.PHX_OK != eStat) goto Error;
               continue;
            }

            Pdl.PDL_BufferPaint(hDisplayBuffer);

            /* Having processed the data, release the buffer ready for further image data */
            eStat = Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_BUFFER_RELEASE, IntPtr.Zero);
            if (Phx.etStat.PHX_OK != eStat) goto Error;
         }

         /* In this simple example we abort the processing loop on an error condition (FIFO overflow).
          * However handling of this condition is application specific, and generally would involve
          * aborting the current acquisition, and then restarting.
          */
         if (sPhxLive.fFifoOverflow) {
            Console.WriteLine("FIFO Overflow detected. Aborting.");
         }

Error:
         /* Now cease all captures */
         if (0 != hCamera) {
            /* Stop camera */
            if (fIsCxpCameraDiscovered && 0 != sCameraRegs.dwAcqStopAddress) {
               myPhxCommon.PhxCommonWriteCxpReg(hCamera, sCameraRegs.dwAcqStopAddress, sCameraRegs.dwAcqStopValue, 800);
            }
            /* Stop frame grabber */
            Phx.PHX_StreamRead(hCamera, Phx.etAcq.PHX_ABORT, IntPtr.Zero);
         }

         Console.WriteLine("Exiting");
         return 0;
      }

      unsafe static int Main(string[] args) {
         PhxCommon               myPhxCommon = new PhxCommon();
         PhxCommon.tCxpRegisters sCameraRegs = new PhxCommon.tCxpRegisters();
         PhxCommon.tPhxCmd       sPhxCmd     = new PhxCommon.tPhxCmd();
         int                     nStatus     = 0;

         myPhxCommon.PhxCommonParseCmd(args, ref sPhxCmd);
         myPhxCommon.PhxCommonParseCxpRegs(sPhxCmd.strConfigFileName, ref sCameraRegs);
         nStatus = phxlive(sPhxCmd.eBoardNumber, sPhxCmd.eChannelNumber, sPhxCmd.strConfigFileName, sCameraRegs);
         return nStatus;
      }
   }
}
