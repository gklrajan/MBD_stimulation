/****************************************************************************
 *
 * Active Silicon
 *
 * File name   : phx_buffer.cs
 * Function    : C# wrapper for PHX Buffer Library (PBL)
 * Updated     : 13-Feb-2015
 *
 * Copyright (c) 2015 Active Silicon
 ****************************************************************************
 * Comments:
 * --------
 * Reference this file to use the PBL library.
 *
 ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;  /* DllImport */

namespace ActiveSilicon
{
   public class Pbl
   {

      /* These are the parameters that can be obtained from a
       * PBL buffer.
       */
      public enum etPblParam
      {
         PBL_BUFF_WIDTH       = (int)1,
         PBL_BUFF_HEIGHT      = (int)2,
         PBL_BUFF_ADDRESS     = (int)3,
         PBL_BUFF_STRIDE      = (int)4,
         PBL_PALETTE          = (int)5,
         PBL_WINDOW           = (int)6,
         PBL_DST_FORMAT       = (int)7,
         PBL_BUFF_FORMAT      = (int)8,
         PBL_BUFF_BAYDEC      = (int)9,
         PBL_BUFF_BAYCOL      = (int)10,
         PBL_BUFF_POINTER     = (int)11,
         PBL_BUFF_DISPLAY     = (int)12,
         PBL_BUFF_EMODE       = (int)13,
         PBL_BUFF_SURFACE     = (int)14,
         PBL_BUFF_GRCTX       = (int)15,
         PBL_PIXEL_FORMAT     = (int)16,
         PBL_PS_DD            = (int)17
      };   

      /* These are the parameter values that can be obtained from a
       * PBL buffer.
       */
      public enum etPblParamValue
      {
         PBL_BAY_DEC_DUP  = (int)0x100 + etPblParam.PBL_BUFF_BAYDEC,
         PBL_BAY_DEC_AVE  = (int)0x200 + etPblParam.PBL_BUFF_BAYDEC,
         PBL_BAY_DEC_MED  = (int)0x300 + etPblParam.PBL_BUFF_BAYDEC,
         PBL_BAY_COL_RED  = (int)0x100 + etPblParam.PBL_BUFF_BAYCOL,
         PBL_BAY_COL_GRNR = (int)0x200 + etPblParam.PBL_BUFF_BAYCOL,
         PBL_BAY_COL_GRNB = (int)0x300 + etPblParam.PBL_BUFF_BAYCOL,
         PBL_BAY_COL_BLU  = (int)0x400 + etPblParam.PBL_BUFF_BAYCOL
      };   

      /* PBL buffers can be created either in system memory or in
       * the memory on the video card. Additionally, Phoenix can be instructed
       * to capture directly into the display buffer, or to an external
       * buffer. This second case would be used when the user wants to
       * capture to his/her own buffer, perform some image processing
       * and then copy the data to the display buffer.
       */
      public enum etPblBufferMode
      {
         PBL_BUFF_VIDCARD_MEM_DIRECT,     /* Video card memory, direct capture */
         PBL_BUFF_VIDCARD_MEM_INDIRECT,   /* Video card memory, indirect capture */
         PBL_BUFF_SYSTEM_MEM_DIRECT,      /* System memory, direct capture */
         PBL_BUFF_SYSTEM_MEM_INDIRECT     /* System memory, indirect capture */
      };



      /* PBL_BufferCreate error handler function */
      [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
      public unsafe delegate void PBL_ErrorHandler(String szFnName, Phx.etStat eErrCode, String szDescString);


#if (WIN64)
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferCreate        (ref uint hPblBuffer, etPblBufferMode eMode, uint hDisplay, uint hCamera, PBL_ErrorHandler ErrHandler);
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferDestroy       (ref uint hBuffer);
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferInit          (uint hBuffer);
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferParameterGet  (uint hBuffer, etPblParam eParam, ref uint dwParamValue);
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferParameterGet  (uint hBuffer, etPblParam eParam, ref IntPtr Ptr);
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferParameterSet  (uint hBuffer, etPblParam eParam, ref etPblParamValue eParamValue);
      [DllImport("phxblx64.dll")] public static extern Phx.etStat PBL_BufferParameterSet  (uint hBuffer, etPblParam eParam, ref uint dwParamValue);
#else
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferCreate        (ref uint hPblBuffer, etPblBufferMode eMode, uint hDisplay, uint hCamera, PBL_ErrorHandler ErrHandler);
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferDestroy       (ref uint hBuffer);
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferInit          (uint hBuffer);
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferParameterGet  (uint hBuffer, etPblParam eParam, ref uint dwParamValue);
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferParameterGet  (uint hBuffer, etPblParam eParam, ref IntPtr Ptr);
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferParameterSet  (uint hBuffer, etPblParam eParam, ref etPblParamValue eParamValue);
      [DllImport("phxbl.dll")] public static extern Phx.etStat PBL_BufferParameterSet  (uint hBuffer, etPblParam eParam, ref uint dwParamValue);
#endif
   }
}
