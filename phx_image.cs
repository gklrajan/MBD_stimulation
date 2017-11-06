/****************************************************************************
 *
 * Active Silicon
 *
 * File name   : phx_image.cs
 * Function    : C# wrapper for PHX Image Library (\)
 * Updated     : 13-Feb-2015
 *
 * Copyright (c) 2015 Active Silicon
 ****************************************************************************
 * Comments:
 * --------
 * Reference this file to use the PIL library.
 * 
 ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;  /* DllImport */

namespace ActiveSilicon
{
   public class Pil
   {

#if (WIN64)
      [DllImport("phxilx64.dll")] public static extern Phx.etStat PIL_Convert(uint hSrcBuffer, uint hDstBuffer);
#else
      [DllImport("phxil.dll")] public static extern Phx.etStat PIL_Convert(uint hSrcBuffer, uint hDstBuffer);
#endif
   }
}
