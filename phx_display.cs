/****************************************************************************
 *
 * Active Silicon
 *
 * File name   : phx_display.cs
 * Function    : C# wrapper for PHX Display Library (PDL)
 * Updated     : 13-Feb-2015
 *
 * Copyright (c) 2015 Active Silicon
 ****************************************************************************
 * Comments:
 * --------
 * Reference this file to use the PDL library.
 * 
 ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;  /* DllImport */

namespace ActiveSilicon
{
   public class Pdl
   {

      /* These are the parameters that can be obtained from a
       * PDL display or buffer.
       */
      public enum etPdlParam
      {
         PDL_BUFF_WIDTH       = (int)1,
         PDL_BUFF_HEIGHT      = (int)2,
         PDL_BUFF_ADDRESS     = (int)3,
         PDL_BUFF_STRIDE      = (int)4,
         PDL_PALETTE          = (int)5,
         PDL_WINDOW           = (int)6,
         PDL_DST_FORMAT       = (int)7,
         PDL_BUFF_LAST        = (int)8,
         PDL_PIXEL_FORMAT     = (int)9,
         PDL_PS_DD            = (int)10,
         PDL_F_QUIT           = (int)11,
         PDL_X_POS            = (int)12,
         PDL_Y_POS            = (int)13,
         PDL_SFC_PRIMARY      = (int)14,
         PDL_X_OFFSET         = (int)15,
         PDL_Y_OFFSET         = (int)16,
         PDL_BYTES_PER_PIXEL  = (int)17
      };

      /* Display buffers can be created either in system memory or in
       * the memory on the video card. Additionally, Phoenix can be instructed
       * to capture directly into the display buffer, or to an external
       * buffer. This second case would be used when the user wants to
       * capture to his/her own buffer, perform some image processing
       * and then copy the data to the display buffer.
       */
      public enum etPdlBufferMode
      {
         PDL_BUFF_VIDCARD_MEM_DIRECT,     /* Video card memory, direct capture */
         PDL_BUFF_VIDCARD_MEM_INDIRECT,   /* Video card memory, indirect capture */
         PDL_BUFF_SYSTEM_MEM_DIRECT,      /* System memory, direct capture */
         PDL_BUFF_SYSTEM_MEM_INDIRECT     /* System memory, indirect capture */
      };


      /* PDL_DisplayCreate error handler function */
      [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
      public unsafe delegate void PDL_ErrorHandler(String szFnName, Phx.etStat eErrCode, String szDescString);


#if (WIN64)
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_DisplayCreate       (ref uint hPdlDisplay, System.IntPtr hWindow, uint hCamera, PDL_ErrorHandler ErrHandler);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_DisplayDestroy      (ref uint hPdlDisplay);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_DisplayInit         (uint hPdlDisplay);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_DisplayParameterGet (uint hDisplay, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_DisplayParameterSet (uint hDisplay, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_DisplayPaletteSet   (uint hDisplay);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_BufferCreate        (ref uint hPdlBuffer, uint hDisplay, etPdlBufferMode eMode);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_BufferDestroy       (ref uint hBuffer);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_BufferPaint         (uint hBuffer);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_BufferParameterGet  (uint hBuffer, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("phxdlx64.dll")] public static extern Phx.etStat          PDL_BufferParameterSet  (uint hBuffer, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("phxdlx64.dll")] public unsafe static extern Phx.etStat   PDL_BufferParameterSet  (uint hBuffer, etPdlParam eParam, int* nParamvalue);
#else
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_DisplayCreate       (ref uint hPdlDisplay, System.IntPtr hWindow, uint hCamera, PDL_ErrorHandler ErrHandler);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_DisplayDestroy      (ref uint hPdlDisplay);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_DisplayInit         (uint hPdlDisplay);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_DisplayParameterGet (uint hDisplay, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_DisplayParameterSet (uint hDisplay, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_DisplayPaletteSet   (uint hDisplay);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_BufferCreate        (ref uint hPdlBuffer, uint hDisplay, etPdlBufferMode eMode);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_BufferDestroy       (ref uint hBuffer);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_BufferPaint         (uint hBuffer);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_BufferParameterGet  (uint hBuffer, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("pdlw32.dll")] public static extern Phx.etStat         PDL_BufferParameterSet  (uint hBuffer, etPdlParam eParam, ref uint dwParamValue);
      [DllImport("pdlw32.dll")] public unsafe static extern Phx.etStat  PDL_BufferParameterSet  (uint hBuffer, etPdlParam eParam, int* nParamvalue);
#endif
   }
}