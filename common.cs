/****************************************************************************
 *
 * Active Silicon
 *
 * File name   : common.cs
 * Function    : Common routines used by the example suite
 * Updated     : 13-Feb-2015
 *
 * Copyright (c) 2015 Active Silicon
 ****************************************************************************
 * Comments:
 * --------
 * This file provides support services for the PHX SDK C# examples.
 *
 ****************************************************************************
 */

using System;
using System.IO;
using ActiveSilicon;

public class PhxCommon
{
   public String        DEFAULT_UP_DIR          = ".\\..\\..\\..\\..\\..\\..\\pcfs\\";
   public String        DEFAULT_UP_DIR_CXP_OLD  = ".\\..\\..\\..\\..\\..\\..\\FireBird CXP\\pcfs\\";
   public String        DEFAULT_UP_DIR_CL_OLD   = ".\\..\\..\\..\\..\\..\\..\\FireBird CL\\pcfs\\";
   public String        DEFAULT_CFG_FILENAME    = "default.pcf";

   /* A structure containing various user defined parameters.
    * These are initialised by the PhxCommonParseCmd function,
    * before being passed to the appropriate subroutine.
    */
   public struct tPhxCmd {
      public uint             dwBoardNumber;
      public Phx.etParamValue eBoardNumber;
      public uint             dwChannelNumber;
      public Phx.etParamValue eChannelNumber;
      public String           strConfigFileName;
   };

   public struct tCxpRegisters {
      public uint dwAcqStartAddress;
      public uint dwAcqStartValue;
      public uint dwAcqStopAddress;
      public uint dwAcqStopValue;
      public uint dwPixelFormatAddress;
   };

   uint htonl(uint A)
   {
      return ((((uint)(A) & 0xff000000) >> 24) |
              (((uint)(A) & 0x00ff0000) >>  8) |
              (((uint)(A) & 0x0000ff00) <<  8) |
              (((uint)(A) & 0x000000ff) << 24));
   }

   /*
   PhxCommonDefaultConfig()
    * Create a default file name (including relative path) for a Config File.
    * This is built up using pre-defined strings for the file name,
    * relative path, and if necessary absolute path to the current directory.
    * If the default file cannot be opened, it is set to NULL.
    */
   public Phx.etStat PhxCommonDefaultConfig(
      ref tPhxCmd ptPhxCmd /* Structure containing parsed information */
   )
   {
      String      strPcfFile;
      Phx.etStat  eStat = Phx.etStat.PHX_OK;

      /* The default config file is in the examples directory not in the directory containing the executable.
       * This is calculated as a relative path from the application directory.
       */
      strPcfFile = DEFAULT_UP_DIR + DEFAULT_CFG_FILENAME;

      if (!File.Exists(strPcfFile)) {
         strPcfFile = DEFAULT_UP_DIR_CXP_OLD + DEFAULT_CFG_FILENAME;
         if (!File.Exists(strPcfFile)) {
            strPcfFile = DEFAULT_UP_DIR_CL_OLD + DEFAULT_CFG_FILENAME;
               if (!File.Exists(strPcfFile)) {
                  strPcfFile = "";
               }
         }
      }

      ptPhxCmd.strConfigFileName = strPcfFile;

      return eStat;
}

   /*
   PhxCommonParseCmd()
    * Parse the command line parameters, and place results in a common structure.
    * The command line parameters take the following form:
    * AppName -b <BoardNumber> -c <ConfigFileName> -o <OutputFileName>
    * -b <BoardNumber>              is an optional parameter specifying which board to use.
    *                               The default value is board 1.
    * -n <ChannelNumber>            is an optional parameter specifying which channel to use.
    *                               The default value is channel 1.
    * -c <ConfigFileName>           is an optional parameter specifying the Phoenix Configuration File.
    *                               The default value is an OS specific path to "default.pcf" which
    *                               is in the root directory of the example suite.
    * -l <CXPDiscovery>             is an optional parameter specifying the number of CXP links.
    *                               The default value is 0 (AUTO).
    * -g <CXPBitRate>               is an optional parameter specifying the CXP bit rate (Gbps) the camera
    *                               has to be set to.
    *                               The default value is 0 (AUTO).
    * Whilst all parameters may be specified, each example application only uses appropriate
    * parameters, for example "OutputFileName" will be ignored by the phx_info example.
    */
   public Phx.etStat PhxCommonParseCmd(
      String[]    args,    /* Command line parameters */
      ref tPhxCmd ptPhxCmd /* Structure containing parsed information */
   )
   {
      Phx.etStat  eStat             = Phx.etStat.PHX_OK;
      int         argc              = args.Length;
      int         nArgvIndex        = 0;
      String      strFunctionName   = "";

      /* Initialise the PhxCmd structure with default values */
      ptPhxCmd.dwBoardNumber     = 1;
      ptPhxCmd.dwChannelNumber   = 1;

      eStat = PhxCommonDefaultConfig(ref ptPhxCmd);
      if (Phx.etStat.PHX_OK != eStat) goto Error;

      /* The first argument is always the function name itself */
      int nPosition = Environment.GetCommandLineArgs()[0].LastIndexOf('\\');
      if (-1 != nPosition && Environment.GetCommandLineArgs()[0].Length > nPosition)
      {
         strFunctionName = Environment.GetCommandLineArgs()[0].Substring(nPosition + 1);
         nPosition = strFunctionName.IndexOf('.');
         if (-1 != nPosition) {
            strFunctionName = strFunctionName.Substring(0, nPosition);
         } else {
            strFunctionName = "";
         }
      }
      Console.WriteLine("*** Active Silicon SDK Example {0} ***\n", strFunctionName);

      /* Parse the command options */
      while (nArgvIndex < argc) {
         /* Board number */
         if ("-b" == args[nArgvIndex] || "-B" == args[nArgvIndex]) {
            if (++nArgvIndex < argc) {
               ptPhxCmd.dwBoardNumber = uint.Parse(args[nArgvIndex]);
            }
         }

         /* chaNnel number */
         else if ("-n" == args[nArgvIndex] || "-N" == args[nArgvIndex]) {
            if (++nArgvIndex < argc) {
               ptPhxCmd.dwChannelNumber = uint.Parse(args[nArgvIndex]);
            }
         }

         /* Config File */
         else if ("-c" == args[nArgvIndex] || "-C" == args[nArgvIndex]) {
            if (++nArgvIndex < argc) {
               ptPhxCmd.strConfigFileName = args[nArgvIndex];
            }
         }

         else {
            Console.WriteLine("Unrecognised parameter {0} - Ignoring", args[nArgvIndex]);
         }

         nArgvIndex++;
      }

      Console.WriteLine ("Using Board Number   = {0}", ptPhxCmd.dwBoardNumber);
      Console.WriteLine ("      Channel Number = {0}", ptPhxCmd.dwChannelNumber);
      Console.Write     ("      Config File    = ");
      if ("" == ptPhxCmd.strConfigFileName) {
         Console.WriteLine("<None>");
      } else {
         Console.WriteLine("{0}", ptPhxCmd.strConfigFileName);
      }
      Console.WriteLine("");

      switch (ptPhxCmd.dwBoardNumber) {
         case 0: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_AUTO; break;
         case 1: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_1;    break;
         case 2: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_2;    break;
         case 3: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_3;    break;
         case 4: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_4;    break;
         case 5: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_5;    break;
         case 6: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_6;    break;
         case 7: ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_7;    break;
         default:
            Console.WriteLine("Unrecognised board number {0} - Ignoring", ptPhxCmd.dwBoardNumber);
            ptPhxCmd.eBoardNumber = Phx.etParamValue.PHX_BOARD_NUMBER_AUTO;
            break;
      }

      switch (ptPhxCmd.dwChannelNumber) {
         case 0: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_AUTO;   break;
         case 1: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_1;      break;
         case 2: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_2;      break;
         case 3: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_3;      break;
         case 4: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_4;      break;
         case 5: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_5;      break;
         case 6: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_6;      break;
         case 7: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_7;      break;
         case 8: ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_8;      break;
         default:
            Console.WriteLine("Unrecognised channel number {0} - Ignoring", ptPhxCmd.dwChannelNumber);
            ptPhxCmd.eChannelNumber = Phx.etParamValue.PHX_CHANNEL_NUMBER_AUTO;
            break;
      }
      Console.WriteLine("");
   
Error:
      return eStat;
   }

   /*
   PhxCommonParseCxpRegs()
    * Parse the PCF file for CXP camera registers.
    */
   public Phx.etStat PhxCommonParseCxpRegs(
      String            strConfigFileName,   /* Name of the Phoenix Configuration File */
      ref tCxpRegisters ptCameraRegisters    /* Structure containing a description of the CXP registers */
   )
   {
      Phx.etStat  eStat    = Phx.etStat.PHX_OK;
      String      strLine  = "";

      if (!File.Exists(strConfigFileName)) {
         eStat = Phx.etStat.PHX_ERROR_BAD_PARAM;
         goto Exit;
      }

      ptCameraRegisters.dwAcqStartAddress      = 0;
      ptCameraRegisters.dwAcqStartValue        = 0;
      ptCameraRegisters.dwAcqStopAddress       = 0;
      ptCameraRegisters.dwAcqStopValue         = 0;
      ptCameraRegisters.dwPixelFormatAddress   = 0;

      /* Parse the configuration file */
      /* Only check for lines starting with '#!' */
      StreamReader srConfigFile = File.OpenText(strConfigFileName);

      /* Read and parse each line. */
      while (!srConfigFile.EndOfStream) {
         strLine = srConfigFile.ReadLine();
         String[] strSplit = strLine.Split(new Char[] {' ', ',', '\t', '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
         if (strSplit.Length >= 3) {
            if (strSplit[0].Length >= 2) {
               /* If the first token of a line is '#!' then it could be a line describing a CXP register. */
               if (0 == strSplit[0].CompareTo("#!")) {
                  if (0 == strSplit[1].CompareTo("CXP_REG_ACQ_START_ADDRESS")) {
                     ptCameraRegisters.dwAcqStartAddress = Convert.ToUInt32(strSplit[2], 16);
                  }
                  else if (0 == strSplit[1].CompareTo("CXP_REG_ACQ_START_VALUE")) {
                     ptCameraRegisters.dwAcqStartValue = Convert.ToUInt32(strSplit[2], 10);
                  }
                  else if (0 == strSplit[1].CompareTo("CXP_REG_ACQ_STOP_ADDRESS")) {
                     ptCameraRegisters.dwAcqStopAddress = Convert.ToUInt32(strSplit[2], 16);
                  }
                  else if (0 == strSplit[1].CompareTo("CXP_REG_ACQ_STOP_VALUE")) {
                     ptCameraRegisters.dwAcqStopValue = Convert.ToUInt32(strSplit[2], 10);
                  }
                  else if (0 == strSplit[1].CompareTo("CXP_REG_PIXEL_FORMAT_ADDRESS")) {
                     ptCameraRegisters.dwPixelFormatAddress = Convert.ToUInt32(strSplit[2], 16);
                  }
               }
            }
         }
      }

      srConfigFile.Close();

   Exit:
      return eStat;
   }

   /*
   PhxCommonWriteCxpReg()
    * Write to a 32 bit CXP register in camera.
    */
   public unsafe Phx.etStat PhxCommonWriteCxpReg(
      uint hCamera,     /* PHX handle */
      uint dwAddress,   /* Address of the camera register */
      uint dwValue,     /* Value to write */
      uint dwTimeout    /* Timeout for operation */
   )
   {
      Phx.etStat  eStat    = Phx.etStat.PHX_OK;
      uint        dwSize   = sizeof(uint);

      dwValue = htonl(dwValue);

      eStat = Phx.PHX_ControlWrite(hCamera, Phx.etControlPort.PHX_REGISTER_DEVICE, ref dwAddress, ref dwValue, ref dwSize, dwTimeout);
      return eStat;
   }

   /*
   PhxCommonReadCxpReg()
    * Read from a 32 bit CXP register in camera.
    */
   public unsafe Phx.etStat PhxCommonReadCxpReg(
      uint     hCamera,    /* PHX handle */
      uint     dwAddress,  /* Address of the camera register */
      ref uint dwValue,    /* Value to write */
      uint     dwTimeout   /* Timeout for operation */
   )
   {
      Phx.etStat  eStat    = Phx.etStat.PHX_OK;
      uint        dwSize   = sizeof(uint);

      eStat = Phx.PHX_ControlRead(hCamera, Phx.etControlPort.PHX_REGISTER_DEVICE, ref dwAddress, ref dwValue, ref dwSize, dwTimeout);
      if (Phx.etStat.PHX_OK == eStat) {
         dwValue = htonl(dwValue);
      }
      return eStat;
   }

   /*
   PhxCommonGetCxpDiscoveryStatus()
    * Checks if CXP camera is discovered.
    */
   public Phx.etStat PhxCommonGetCxpDiscoveryStatus(
      uint     hCamera,
      uint     dwTimeoutSec,
      ref bool fIsDiscovered
   )
   {
      Phx.etStat  eStat    = Phx.etStat.PHX_OK;
      uint        dwIndex  = 0;

      Console.WriteLine("Checking for CXP camera discovery...");

      fIsDiscovered = false;
      while (dwIndex++ < dwTimeoutSec * 2) { /* Sleep between two attempts is only 500 ms */
         Phx.etCxpInfo eDiscoveryStatus = 0;
         eStat = Phx.PHX_ParameterGet(hCamera, Phx.etParam.PHX_CXP_INFO, ref eDiscoveryStatus);
         if (Phx.etStat.PHX_OK != eStat) goto Exit;
         if ((int)Phx.etCxpInfo.PHX_CXP_CAMERA_DISCOVERED == ((int)eDiscoveryStatus & (int)(Phx.etCxpInfo.PHX_CXP_CAMERA_DISCOVERED))) {
            fIsDiscovered = true;
            break;
         }

         Console.Write(".");

         System.Threading.Thread.Sleep(500);
      }
      Console.WriteLine("");

      if (fIsDiscovered) {
         Console.WriteLine("CXP camera is discovered.");
      } else {
         Console.WriteLine("CXP camera was not discovered.");
      }

   Exit:
      return eStat;
   }

   /*
   PhxCommonGetBusFormat()
    * Determine the PHX_BUS_FORMAT based on PHX_CAM_SRC_COL and PHX_CAM_SRC_DEPTH.
    */
   public Phx.etStat PhxCommonGetBusFormat(
      Phx.etParamValue     ePhxCamSrcCol,    /* PHX_CAM_SRC_COL value */
      uint                 dwPhxCamSrcDepth, /* PHX_CAM_SRC_DEPTH value */
      Phx.etParamValue     ePhxBusFormatIn,  /* Current PHX_BUS_FORMAT value */
      ref Phx.etParamValue ePhxBusFormatOut  /* PHX_BUS_FORMAT return value */
   )
   {
      Phx.etStat  eStat       = Phx.etStat.PHX_OK;
      int         nBayerDepth = 0;

      /* Check if legacy PHX_DST_FORMAT_BAY value is being used, and replace it with new PHX_BUS_FORMAT_BAYER value if needed */
      if (Phx.etParamValue.PHX_DST_FORMAT_BAY8 == ePhxBusFormatIn) {
         switch (ePhxCamSrcCol) {
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_RG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG8; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GR: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR8; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GB: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB8; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_BG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG8; break;
            default: break;
         }
      } else if (Phx.etParamValue.PHX_DST_FORMAT_BAY10 == ePhxBusFormatIn) {
         switch (ePhxCamSrcCol) {
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_RG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG10; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GR: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR10; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GB: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB10; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_BG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG10; break;
            default: break;
         }
      } else if (Phx.etParamValue.PHX_DST_FORMAT_BAY12 == ePhxBusFormatIn) {
         switch (ePhxCamSrcCol) {
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_RG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG12; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GR: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR12; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GB: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB12; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_BG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG12; break;
            default: break;
         }
      } else if (Phx.etParamValue.PHX_DST_FORMAT_BAY14 == ePhxBusFormatIn) {
         switch (ePhxCamSrcCol) {
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_RG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG14; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GR: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR14; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GB: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB14; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_BG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG14; break;
            default: break;
         }
      } else if (Phx.etParamValue.PHX_DST_FORMAT_BAY16 == ePhxBusFormatIn) {
         switch (ePhxCamSrcCol) {
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_RG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG16; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GR: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR16; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_GB: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB16; break;
            case Phx.etParamValue.PHX_CAM_SRC_BAYER_BG: ePhxBusFormatIn = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG16; break;
            default: break;
         }
      }

      /* Check if Bayer format (used when determining the PHX_BUS_FORMAT for a Mono source) */
             if ( Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR8  <= ePhxBusFormatIn
               && Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG8  >= ePhxBusFormatIn) {
         nBayerDepth = 8;
      } else if ( Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR10 <= ePhxBusFormatIn
               && Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG10 >= ePhxBusFormatIn) {
         nBayerDepth = 10;
      } else if ( Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR12 <= ePhxBusFormatIn
               && Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG12 >= ePhxBusFormatIn) {
         nBayerDepth = 12;
      } else if ( Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR14 <= ePhxBusFormatIn
               && Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG14 >= ePhxBusFormatIn) {
         nBayerDepth = 14;
      } else if ( Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR16 <= ePhxBusFormatIn
               && Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG16 >= ePhxBusFormatIn) {
         nBayerDepth = 16;
      }

      /* Set the return value to the input PHX_BUS_FORMAT value */
      ePhxBusFormatOut = ePhxBusFormatIn;

      /* Determine PHX_BUS_FORMAT output value */
      switch (ePhxCamSrcCol) {
         case Phx.etParamValue.PHX_CAM_SRC_MONO:
            switch (dwPhxCamSrcDepth) {
               case 8:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_MONO8 != ePhxBusFormatIn && 8 != nBayerDepth) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_MONO8;
                  } break;

               case 10:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_MONO10P != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_MONO10  != ePhxBusFormatIn && 10 != nBayerDepth) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_MONO10;
                  } break;

               case 12:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_MONO12P != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_MONO12  != ePhxBusFormatIn && 12 != nBayerDepth) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_MONO12;
                  } break;

               case 14:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_MONO14P != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_MONO14  != ePhxBusFormatIn && 14 != nBayerDepth) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_MONO14;
                  } break;

               case 16:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_MONO16 != ePhxBusFormatIn && 16 != nBayerDepth) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_MONO16;
                  } break;

               default:
                  eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE;
                  goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_RGB:
            switch (dwPhxCamSrcDepth) {
               case 8:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_RGB8 != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_BGR8 != ePhxBusFormatIn) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGB8;
                  } break;

               case 10:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_RGB10 != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_BGR10 != ePhxBusFormatIn) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGB10;
                  } break;

               case 12:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_RGB12 != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_BGR12 != ePhxBusFormatIn) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGB12;
                  } break;

               case 14:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_RGB14 != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_BGR14 != ePhxBusFormatIn) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGB14;
                  } break;

               case 16:
                  if (Phx.etParamValue.PHX_BUS_FORMAT_RGB16 != ePhxBusFormatIn &&
                      Phx.etParamValue.PHX_BUS_FORMAT_BGR16 != ePhxBusFormatIn) {
                     ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGB16;
                  } break;

               default:
                  eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE;
                  goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_BAYER_RG:
            switch (dwPhxCamSrcDepth) {
               case  8: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG8;  break;
               case 10: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG10; break;
               case 12: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG12; break;
               case 14: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG14; break;
               case 16: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_RG16; break;
               default: eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE; goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_BAYER_GR:
            switch (dwPhxCamSrcDepth) {
               case  8: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR8;  break;
               case 10: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR10; break;
               case 12: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR12; break;
               case 14: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR14; break;
               case 16: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GR16; break;
               default: eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE; goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_BAYER_GB:
            switch (dwPhxCamSrcDepth) {
               case  8: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB8;  break;
               case 10: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB10; break;
               case 12: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB12; break;
               case 14: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB14; break;
               case 16: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_GB16; break;
               default: eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE; goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_BAYER_BG:
            switch (dwPhxCamSrcDepth) {
               case  8: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG8;  break;
               case 10: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG10; break;
               case 12: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG12; break;
               case 14: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG14; break;
               case 16: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_BAYER_BG16; break;
               default: eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE; goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_YUV422:
            switch (dwPhxCamSrcDepth) {
               case 8:
               case 16: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_YUV422_8; break;
               default: eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE; goto Exit;
            } break;

         case Phx.etParamValue.PHX_CAM_SRC_RGBA:
            switch (dwPhxCamSrcDepth) {
               case  8: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGBA8;   break;
               case 10: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGBA10;  break;
               case 12: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGBA12;  break;
               case 14: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGBA14;  break;
               case 16: ePhxBusFormatOut = Phx.etParamValue.PHX_BUS_FORMAT_RGBA16;  break;
               default: eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE; goto Exit;
            } break;

         default:
            eStat = Phx.etStat.PHX_ERROR_BAD_PARAM_VALUE;
            goto Exit;
      }

   Exit:
      return eStat;
   }

   /*
   PhxCommonDisplayErrorHandler()
    * Handles errors from display library.
    */
   public void PhxCommonDisplayErrorHandler(
      String      strFnName,     /* Function name */
      Phx.etStat  eErrCode,      /* Error code */
      String      strDescString  /* Error description */
   )
   {
      String strErrorMessage = String.Format("{0} failed with error code {1:x}.\n", strFnName, eErrCode);
      if (strDescString.Length > 0) {
         String.Concat(strErrorMessage, strDescString);
      }
      Console.WriteLine("{0}", strErrorMessage);
   }

   /* 
   PhxCommonKbHit()
   * Implementation of a keyboard input routine to terminate application.
    */
   public int PhxCommonKbHit()
   {
      if (true == Console.KeyAvailable) {
         return 1;
      } else {
         return 0;
      }
   }

   /*
   PhxCommonKbRead()
    * Implementation of a keyboard character read routine.
    */
   public int PhxCommonKbRead()
   {
      ConsoleKeyInfo cki;
      
      cki = Console.ReadKey();
      return (int)cki.KeyChar;
   }

   public static Byte[] GetBytesFromString(String str)
   {
      Byte [] bytes = new Byte[str.Length];

      for (Int32 i = 0; i < bytes.Length; i++) {
         bytes[i] = (Byte)str[i];
      }

      return bytes;
   }

}
