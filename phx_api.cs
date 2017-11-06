/****************************************************************************
 *
 * Active Silicon
 *
 * File name   : phx_api.cs
 * Function    : C# wrapper for PHX library
 * Updated     : 13-Feb-2015
 *
 * Copyright (c) 2015 Active Silicon
 ****************************************************************************
 * Comments:
 * --------
 * This is the only file a user (or higher level library) needs 
 * to reference in their application in order to use the PHX library.
 *
 ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;  /* DllImport */

/* Disable Compiler CS0169 Warnings ("The private field 'class member' is never used") */
#pragma warning disable 169

namespace ActiveSilicon
{
   public class Phx
   {
      /* API & Library function parameter definitions
       * ============================================
      _FnTypes()
      */
      public const int FNTYPE_EMASK    = unchecked((int)0xF0000000);
      public const int FNTYPE_PHA_API  =           (int)0x00000000;
      public const int FNTYPE_PHA_LIB  =           (int)0x10000000;
      public const int FNTYPE_PHC_API  =           (int)0x20000000;
      public const int FNTYPE_PHC_LIB  =           (int)0x30000000;
      public const int FNTYPE_PHD_API  =           (int)0x40000000;
      public const int FNTYPE_PHD_LIB  =           (int)0x50000000;
      public const int FNTYPE_PDL_API  =           (int)0x60000000;
      public const int FNTYPE_PDL_LIB  =           (int)0x70000000;
      public const int FNTYPE_PCC_API  = unchecked((int)0x80000000);
      public const int FNTYPE_PCC_LIB  = unchecked((int)0x90000000);
      public const int FNTYPE_PHX_API  = unchecked((int)0xC0000000);
      public const int FNTYPE_PHX_LIB  = unchecked((int)0xD0000000);

      /* PHX_Function Definitions
       * ========================
       * These enums are used as magic numbers for all parameters passed to the specific functions,
       * ie any parameter passed to the Acquisition functions will have the top 8 bits set to 3.
       * This is used to confirm that the parameter passed is valid for the function.
       */
      public const int PHX_EMASK_FN          = (int)(FNTYPE_EMASK   | 0x000F0000);
      public const int PHX_CAMERACONFIG_LOAD = (int)(FNTYPE_PHX_API | 0x00010000);
      public const int PHX_SETANDGET         = (int)(FNTYPE_PHX_API | 0x00020000); /* For parameters after PHX_Open has been called */
      public const int PHX_ACQUIRE           = (int)(FNTYPE_PHX_API | 0x00030000);
      public const int PHX_ACTION            = (int)(FNTYPE_PHX_API | 0x00040000);
      public const int PHX_TEST              = (int)(FNTYPE_PHX_API | 0x00050000);
      public const int PHX_COM               = (int)(FNTYPE_PHX_API | 0x00060000);
      public const int PHX_PRE_OPEN          = (int)(FNTYPE_PHX_API | 0x00070000); /* For parameters before PHX_Open has been called */
      public const int PHX_CONTROL           = (int)(FNTYPE_PHX_API | 0x00080000);
      public const int PHX_FBD_SETANDGET     = (int)(FNTYPE_PHX_API | 0x00090000); /* For FireBird specific parameters */

      /* Retained for backwards compatibility with previous software releases.
       * Do not use for new applications.
       */
      public const int PHX_CAMERACONFIG_SAVE = PHX_ACTION;


      /* PHX_CameraConfigLoad Definitions
       * ================================
       */
      public enum etCamConfigLoad
      {
         PHX_EMASK_BOARD      = (int)(PHX_CAMERACONFIG_LOAD | 0x0007),
         PHX_BOARD_AUTO       = (int)(PHX_CAMERACONFIG_LOAD | 0x0000),   /* Auto detect the first available board */
         PHX_BOARD1           = (int)(PHX_CAMERACONFIG_LOAD | 0x0001),   /* 1st board in the scan order */
         PHX_BOARD2           = (int)(PHX_CAMERACONFIG_LOAD | 0x0002),   /* 2nd board in the scan order */
         PHX_BOARD3           = (int)(PHX_CAMERACONFIG_LOAD | 0x0003),   /* 3rd board in the scan order */
         PHX_BOARD4           = (int)(PHX_CAMERACONFIG_LOAD | 0x0004),   /* 4th board in the scan order */
         PHX_BOARD5           = (int)(PHX_CAMERACONFIG_LOAD | 0x0005),   /* 5th board in the scan order */
         PHX_BOARD6           = (int)(PHX_CAMERACONFIG_LOAD | 0x0006),   /* 6th board in the scan order */
         PHX_BOARD7           = (int)(PHX_CAMERACONFIG_LOAD | 0x0007),   /* 7th board in the scan order */
         PHX_BOARD_MAX        = (int)7,                                  /* Maximum board number supported */

         PHX_EMASK_CHANNEL    = (int)(PHX_CAMERACONFIG_LOAD | 0x0070),
         PHX_CHANNEL_AUTO     = (int)(PHX_CAMERACONFIG_LOAD | 0x0000),   /* Auto detect the first available channel */
         PHX_CHANNEL_A        = (int)(PHX_CAMERACONFIG_LOAD | 0x0010),   /* 1st available channel */
         PHX_CHANNEL_B        = (int)(PHX_CAMERACONFIG_LOAD | 0x0020),   /* 2nd available channel */
         PHX_CHANNEL_MAX      = (int)2,                                  /* Maximum channel number supported */
         PHX_CHANNEL_ONLY     = (int)(PHX_CAMERACONFIG_LOAD | 0x0040),   /* i.e. don't use other channel's resources */

         PHX_EMASK_MODE       = (int)(PHX_CAMERACONFIG_LOAD | 0x0280),
         PHX_MODE_NORMAL      = (int)(PHX_CAMERACONFIG_LOAD | 0x0000),   /* i.e. both acquisition engine and com ports */
         PHX_COMMS_ONLY       = (int)(PHX_CAMERACONFIG_LOAD | 0x0080),   /* i.e. no control of acquisition engine */
         PHX_ACQ_ONLY         = (int)(PHX_CAMERACONFIG_LOAD | 0x0200),   /* i.e. no control of com ports */

         PHX_EMASK_UPDATE     = (int)(PHX_CAMERACONFIG_LOAD | 0x0100),
         PHX_UPDATE_FIRMWARE  = (int)(PHX_CAMERACONFIG_LOAD | 0x0100),   /* Reprogram all firmware (to flash) */
         PHX_NO_RECONFIGURE   = (int)(PHX_CAMERACONFIG_LOAD | 0x0400),   /* Don't automatically reconfigure the board
                                                                            if the firmware is already up to date */

         PHX_EMASK_TYPE       = (int)(PHX_CAMERACONFIG_LOAD | 0xC000),
         PHX_EMASK_VARIANT    = (int)(PHX_CAMERACONFIG_LOAD | 0x3800),
         PHX_TYPE_AUTO        = (int)(PHX_CAMERACONFIG_LOAD | 0x0000),
         PHX_DIGITAL          = (int)(PHX_CAMERACONFIG_LOAD | 0x4000),   /* Any digital interface board */

         PHX_D24CL_PCI32      = (int)(PHX_CAMERACONFIG_LOAD | 0x4800),   /* AS-PHX-D24CL-PCI32   variants */
         PHX_D24CL_PE1        = (int)(PHX_CAMERACONFIG_LOAD | 0x4808),   /* AS-PHX-D24CL-PE1     variants */
         PHX_D24AVDS_PE1      = (int)(PHX_CAMERACONFIG_LOAD | 0x5008),   /* AS-PHX-D24AVDS-PE1   variants */

         PHX_D32_PCI32        = (int)(PHX_CAMERACONFIG_LOAD | 0x5000),   /* AS-PHX-D36-PCI32     variants */

         PHX_D36_PCI32        = (int)(PHX_CAMERACONFIG_LOAD | 0x5800),   /* AS-PHX-D36-PCI32     variants */
         PHX_D36_PCI64        = (int)(PHX_CAMERACONFIG_LOAD | 0x6000),   /* AS-PHX-D36-PCI64     variants */
         PHX_D36_PCI64U       = (int)(PHX_CAMERACONFIG_LOAD | 0x6800),   /* AS-PHX-D36-PCI64U    variants */
         PHX_D36_PE1          = (int)(PHX_CAMERACONFIG_LOAD | 0x5808),   /* AS-PHX-D36-PE1       variants */

         PHX_D10HDSDI_PE1     = (int)(PHX_CAMERACONFIG_LOAD | 0x6808),   /* AS-PHX-D10HDSDI-PE1  variants */
         PHX_D20HDSDI_PE1     = (int)(PHX_CAMERACONFIG_LOAD | 0x6008),   /* AS-PHX-D20HDSDI-PE1  variants */

         PHX_D48CL_PCI32      = (int)(PHX_CAMERACONFIG_LOAD | 0x4008),   /* AS-PHX-D48CL-PCI32   variants */
         PHX_D48CL_PCI64      = (int)(PHX_CAMERACONFIG_LOAD | 0x7000),   /* AS-PHX-D48CL-PCI64   variants */
         PHX_D24CL_PE1_MIR    = (int)(PHX_CAMERACONFIG_LOAD | 0x7008),   /* AS-PHX-D24CL-PE1-MIR variants */
         PHX_D48CL_PCI64U     = (int)(PHX_CAMERACONFIG_LOAD | 0x7800),   /* AS-PHX-D48CL-PCI64U  variants */
         PHX_D48CL_PE1        = (int)(PHX_CAMERACONFIG_LOAD | 0x7808),   /* AS-PHX-D48CL-PE1     variants */

         PHX_ANALOGUE         = (int)(PHX_CAMERACONFIG_LOAD | 0x8000),   /* Any analogue interface board */

         PHX_DIGITAL2         = (int)(PHX_CAMERACONFIG_LOAD | 0xC000),   /* Any other digital interface board */
         PHX_D36_PE4          = (int)(PHX_CAMERACONFIG_LOAD | 0xD808),   /* AS-PHX-D36-PE4       variants */
         PHX_D10HDSDI_PE4     = (int)(PHX_CAMERACONFIG_LOAD | 0xE808),   /* AS-PHX-D10HDSDI-PE4  variants */
         PHX_D20HDSDI_PE4     = (int)(PHX_CAMERACONFIG_LOAD | 0xE008),   /* AS-PHX-D20HDSDI-PE4  variants */
         PHX_D48CL_PE4        = (int)(PHX_CAMERACONFIG_LOAD | 0xF008),   /* AS-PHX-D48CL-PE4     variants */
         PHX_D64CL_PE4        = (int)(PHX_CAMERACONFIG_LOAD | 0xD008),   /* AS-PHX-D64CL-PE4     variants */

         /* Retained for backwards compatibility with previous software releases.
          * Do not use for new applications.
          */
         PHX_CHANNEL1 = PHX_CHANNEL_A,
         PHX_CHANNEL2 = PHX_CHANNEL_B
      };

      /* PHX_Action Definitions
       * ======================
       */
      public enum etActionParam
      {
         PHX_EMASK_SAVE = (int)(PHX_ACTION | 0x0007),
         PHX_SAVE_CAM   = (int)(PHX_ACTION | 0x0001),   /* Camera specific parameters */
         PHX_SAVE_SYS   = (int)(PHX_ACTION | 0x0002),   /* system specific parameters */
         PHX_SAVE_APP   = (int)(PHX_ACTION | 0x0004),   /* Application specific parameters */
         PHX_SAVE_ALL   = (int)(PHX_ACTION | 0x0007)    /* Save all parameters */
      };

      /* GetAndSet Parameter Definitions
       * ===============================
       * These define the parameters that can be read or modified via the PHX_ParameterGet & PHX_ParameterSet
       * interface.  Each value is a unique number coded as follows:
       * bits 31 to 24 are the GetAndSet function identifier,
       * bits 23 to 16 are the specific parameter identifier
       * They are implemented as enums to make them visible within the debugger.
       */
      /* Note - Because we have changed the API on a couple of occasions, we have tried to preserve the
       * numbering of the unaffected enums. Therefore there are a number of gaps in the following list where
       * some parameters have been removed.
       */
      public enum etParam

      {
         PHX_PARAM_MASK                = unchecked((int)0xffffff00),
         PHX_INVALID_PARAM             = 0,
         PHX_CACHE_FLUSH               = 1,
         PHX_FORCE_REWRITE             = 2,

         PHX_ACQ_CONTINUOUS            = (int)(PHX_SETANDGET | (1 << 8)),
         PHX_ACQ_NUM_BUFFERS           = (int)(PHX_SETANDGET | (2 << 8)),
         PHX_ACQ_SKIP                  = (int)(PHX_SETANDGET | (3 << 8)),
         PHX_FGTRIG_SRC                = (int)(PHX_SETANDGET | (4 << 8)),
         PHX_FGTRIG_MODE               = (int)(PHX_SETANDGET | (5 << 8)),
         PHX_ACQ_FIELD_MODE            = (int)(PHX_SETANDGET | (6 << 8)),
         PHX_ACQ_XSUB                  = (int)(PHX_SETANDGET | (7 << 8)),
         PHX_ACQ_YSUB                  = (int)(PHX_SETANDGET | (8 << 8)),

         PHX_CAM_ACTIVE_XLENGTH        = (int)(PHX_SETANDGET | (10 << 8)),
         PHX_CAM_ACTIVE_YLENGTH        = (int)(PHX_SETANDGET | (11 << 8)),
         PHX_CAM_ACTIVE_XOFFSET        = (int)(PHX_SETANDGET | (12 << 8)),
         PHX_CAM_ACTIVE_YOFFSET        = (int)(PHX_SETANDGET | (13 << 8)),
         PHX_CAM_CLOCK_POLARITY        = (int)(PHX_SETANDGET | (14 << 8)),
         PHX_CAM_FORMAT                = (int)(PHX_SETANDGET | (15 << 8)),
         PHX_CAM_NUM_TAPS              = (int)(PHX_SETANDGET | (16 << 8)),
         PHX_CAM_SRC_DEPTH             = (int)(PHX_SETANDGET | (17 << 8)),
         PHX_CAM_SRC_COL               = (int)(PHX_SETANDGET | (18 << 8)),
         PHX_CAM_HTAP_DIR              = (int)(PHX_SETANDGET | (19 << 8)),
         PHX_CAM_HTAP_TYPE             = (int)(PHX_SETANDGET | (20 << 8)),
         PHX_CAM_HTAP_NUM              = (int)(PHX_SETANDGET | (21 << 8)),
         PHX_CAM_VTAP_DIR              = (int)(PHX_SETANDGET | (22 << 8)),
         PHX_CAM_VTAP_TYPE             = (int)(PHX_SETANDGET | (23 << 8)),
         PHX_CAM_VTAP_NUM              = (int)(PHX_SETANDGET | (24 << 8)),
         PHX_CAM_TYPE                  = (int)(PHX_SETANDGET | (25 << 8)),
         PHX_CAM_XBINNING              = (int)(PHX_SETANDGET | (26 << 8)),
         PHX_CAM_YBINNING              = (int)(PHX_SETANDGET | (27 << 8)),

         PHX_COMMS_DATA                = (int)(PHX_SETANDGET | (28 << 8)),
         PHX_COMMS_FLOW                = (int)(PHX_SETANDGET | (29 << 8)),
         PHX_COMMS_INCOMING            = (int)(PHX_SETANDGET | (30 << 8)),
         PHX_COMMS_OUTGOING            = (int)(PHX_SETANDGET | (31 << 8)),
         PHX_COMMS_PARITY              = (int)(PHX_SETANDGET | (32 << 8)),
         PHX_COMMS_SPEED               = (int)(PHX_SETANDGET | (33 << 8)),
         PHX_COMMS_STANDARD            = (int)(PHX_SETANDGET | (34 << 8)),
         PHX_COMMS_STOP                = (int)(PHX_SETANDGET | (35 << 8)),

         PHX_DATASRC                   = (int)(PHX_SETANDGET | (36 << 8)),
         PHX_DATASTREAM_VALID          = (int)(PHX_SETANDGET | (37 << 8)),

         PHX_BUS_FORMAT                = (int)(PHX_SETANDGET | (38 << 8)),

         PHX_DST_PTR_TYPE              = (int)(PHX_SETANDGET | (39 << 8)),
         PHX_DST_PTRS_VIRT             = (int)(PHX_SETANDGET | (40 << 8)),

         PHX_DUMMY_PARAM               = (int)(PHX_SETANDGET | (41 << 8)),

         PHX_ERROR_FIRST_ERRNUM        = (int)(PHX_SETANDGET | (42 << 8)),
         PHX_ERROR_FIRST_ERRSTRING     = (int)(PHX_SETANDGET | (43 << 8)),
         PHX_ERROR_HANDLER             = (int)(PHX_SETANDGET | (44 << 8)),
         PHX_ERROR_LAST_ERRNUM         = (int)(PHX_SETANDGET | (45 << 8)),
         PHX_ERROR_LAST_ERRSTRING      = (int)(PHX_SETANDGET | (46 << 8)),

         PHX_COUNTE1_VALUE_NOW         = (int)(PHX_SETANDGET | (47 << 8)),


         PHX_NUM_BOARDS                = (int)(PHX_SETANDGET | (61 << 8)),

         PHX_IO_CCIO_A                 = (int)(PHX_SETANDGET | (62 << 8)),  /* Absolute */
         PHX_IO_CCIO_A_OUT             = (int)(PHX_SETANDGET | (63 << 8)),
         PHX_IO_CCIO_B                 = (int)(PHX_SETANDGET | (64 << 8)),
         PHX_IO_CCIO_B_OUT             = (int)(PHX_SETANDGET | (65 << 8)),

         PHX_IO_CCOUT_A                = PHX_IO_CCIO_A,
         PHX_IO_CCOUT_B                = PHX_IO_CCIO_B,

         PHX_IO_OPTO_SET               = (int)(PHX_SETANDGET | (68 << 8)),
         PHX_IO_OPTO_CLR               = (int)(PHX_SETANDGET | (69 << 8)),
         PHX_IO_OPTO                   = (int)(PHX_SETANDGET | (70 << 8)),

         PHX_IO_TTL_A                  = (int)(PHX_SETANDGET | (71 << 8)),
         PHX_IO_TTL_A_OUT              = (int)(PHX_SETANDGET | (72 << 8)),
         PHX_IO_TTL_B                  = (int)(PHX_SETANDGET | (73 << 8)),
         PHX_IO_TTL_B_OUT              = (int)(PHX_SETANDGET | (74 << 8)),

         PHX_TIMEOUT_CAPTURE           = (int)(PHX_SETANDGET | (75 << 8)),
         PHX_TIMEOUT_DMA               = (int)(PHX_SETANDGET | (76 << 8)),
         PHX_TIMEOUT_TRIGGER           = (int)(PHX_SETANDGET | (77 << 8)),

         PHX_INTRPT_SET                = (int)(PHX_SETANDGET | (78 << 8)),
         PHX_INTRPT_CLR                = (int)(PHX_SETANDGET | (79 << 8)),

         PHX_REV_HW                    = (int)(PHX_SETANDGET | (80 << 8)),
         PHX_REV_HW_MAJOR              = (int)(PHX_SETANDGET | (81 << 8)),
         PHX_REV_HW_MINOR              = (int)(PHX_SETANDGET | (82 << 8)),
         PHX_REV_SW                    = (int)(PHX_SETANDGET | (83 << 8)),
         PHX_REV_SW_MAJOR              = (int)(PHX_SETANDGET | (84 << 8)),
         PHX_REV_SW_MINOR              = (int)(PHX_SETANDGET | (85 << 8)),
         PHX_REV_SW_SUBMINOR           = (int)(PHX_SETANDGET | (86 << 8)),

         PHX_ROI_DST_XOFFSET           = (int)(PHX_SETANDGET | (87 << 8)),
         PHX_ROI_DST_YOFFSET           = (int)(PHX_SETANDGET | (88 << 8)),
         PHX_ROI_SRC_XOFFSET           = (int)(PHX_SETANDGET | (89 << 8)),
         PHX_ROI_SRC_YOFFSET           = (int)(PHX_SETANDGET | (90 << 8)),
         PHX_ROI_XLENGTH               = (int)(PHX_SETANDGET | (91 << 8)),
         PHX_ROI_YLENGTH               = (int)(PHX_SETANDGET | (92 << 8)),

         PHX_BUF_DST_XLENGTH           = (int)(PHX_SETANDGET | (93 << 8)),
         PHX_BUF_DST_YLENGTH           = (int)(PHX_SETANDGET | (94 << 8)),

         PHX_STATUS                    = (int)(PHX_SETANDGET | (95 << 8)),

         PHX_BOARD_PROPERTIES          = (int)(PHX_SETANDGET | (102 << 8)),

         PHX_ROI_XLENGTH_SCALED        = (int)(PHX_SETANDGET | (103 << 8)),
         PHX_ROI_YLENGTH_SCALED        = (int)(PHX_SETANDGET | (104 << 8)),

         PHX_BUF_SET                   = (int)(PHX_SETANDGET | (105 << 8)),
         PHX_BUF_SET_COLOUR            = (int)(PHX_SETANDGET | (106 << 8)),

         PHX_LUT_COUNT                 = (int)(PHX_SETANDGET | (107 << 8)),
         PHX_LUT_INFO                  = (int)(PHX_SETANDGET | (108 << 8)),

         PHX_REV_HW_SUBMINOR           = (int)(PHX_SETANDGET | (109 << 8)),

         PHX_LUT_CORRECT               = (int)(PHX_SETANDGET | (110 << 8)),

         PHX_LINETRIG_SRC              = (int)(PHX_SETANDGET | (111 << 8)),
         PHX_LINETRIG_TIMER_CTRL       = (int)(PHX_SETANDGET | (112 << 8)),
         PHX_TIMERA1_PERIOD            = (int)(PHX_SETANDGET | (113 << 8)),
         PHX_CAMTRIG_SRC               = (int)(PHX_SETANDGET | (114 << 8)),
         PHX_EXP_CTRLIO_1              = (int)(PHX_SETANDGET | (115 << 8)),
         PHX_TIMERM1_WIDTH             = (int)(PHX_SETANDGET | (116 << 8)),
         PHX_EXP_CTRLIO_2              = (int)(PHX_SETANDGET | (117 << 8)),
         PHX_TIMERM2_WIDTH             = (int)(PHX_SETANDGET | (118 << 8)),
         PHX_EXP_LINESTART             = (int)(PHX_SETANDGET | (119 << 8)),
         PHX_FGTRIG_DELAY_MODE         = (int)(PHX_SETANDGET | (120 << 8)),
         PHX_TIMERD1_VALUE             = (int)(PHX_SETANDGET | (121 << 8)),
         PHX_EVENTCOUNT_SRC            = (int)(PHX_SETANDGET | (122 << 8)),
         PHX_EVENTGATE_SRC             = (int)(PHX_SETANDGET | (123 << 8)),

         PHX_CAM_HTAP_ORDER            = (int)(PHX_SETANDGET | (124 << 8)),
         PHX_CAM_VTAP_ORDER            = (int)(PHX_SETANDGET | (125 << 8)),

         PHX_EVENT_CONTEXT             = (int)(PHX_SETANDGET | (126 << 8)),

         PHX_CAM_DATA_VALID            = (int)(PHX_SETANDGET | (127 << 8)),

         PHX_COUNT_BUFFER_READY        = (int)(PHX_SETANDGET | (128 << 8)),
         PHX_COUNT_BUFFER_READY_NOW    = (int)(PHX_SETANDGET | (129 << 8)),

         PHX_BIT_SHIFT                 = (int)(PHX_SETANDGET | (130 << 8)),

         PHX_MASK_CCIO                 = (int)0x00000003,
         PHX_MASK_CCOUT                = (int)0x0000000f,
         PHX_MASK_OPTO                 = (int)0x0000000f,
         PHX_IO_CCIO                   = (int)(PHX_SETANDGET | (131 << 8)),  /* Relative */
         PHX_IO_CCOUT                  = PHX_IO_CCIO,
         PHX_IO_CCIO_OUT               = (int)(PHX_SETANDGET | (132 << 8)),

         PHX_IO_TTL                    = (int)(PHX_SETANDGET | (133 << 8)),  /* Relative */
         PHX_IO_TTL_OUT                = (int)(PHX_SETANDGET | (134 << 8)),

         PHX_IO_OPTO_A                 = (int)(PHX_SETANDGET | (135 << 8)),  /* Absolute */
         PHX_IO_OPTO_B                 = (int)(PHX_SETANDGET | (136 << 8)),

         PHX_IO_TIMER_A1_PERIOD        = (int)(PHX_SETANDGET | (137 << 8)),  /* Absolute */
         PHX_IO_TIMER_A2_PERIOD        = (int)(PHX_SETANDGET | (138 << 8)),
         PHX_IO_TIMER_B1_PERIOD        = (int)(PHX_SETANDGET | (139 << 8)),  /* Absolute */
         PHX_IO_TIMER_B2_PERIOD        = (int)(PHX_SETANDGET | (140 << 8)),

         PHX_IO_OPTO_OUT               = (int)(PHX_SETANDGET | (141 << 8)),
         PHX_IO_OPTO_A_OUT             = (int)(PHX_SETANDGET | (142 << 8)),
         PHX_IO_OPTO_B_OUT             = (int)(PHX_SETANDGET | (143 << 8)),

         PHX_FGTRIG_ALIGN              = (int)(PHX_SETANDGET | (144 << 8)),

         PHX_DST_ENDIAN                = (int)(PHX_SETANDGET | (145 << 8)),

         PHX_ACQ_CHAIN                 = (int)(PHX_SETANDGET | (146 << 8)),
         PHX_ACQ_BLOCKING              = (int)(PHX_SETANDGET | (147 << 8)),

         PHX_DST_PTRS_PHYS             = (int)(PHX_SETANDGET | (148 << 8)),

         PHX_BOARD_INFO                = (int)(PHX_SETANDGET | (149 << 8)),
         PHX_DATARATE_TEST             = (int)(PHX_SETANDGET | (150 << 8)),

         PHX_CAM_CLOCK_MAX             = (int)(PHX_SETANDGET | (151 << 8)),

         PHX_COUNTE1_VALUE_DB          = (int)(PHX_SETANDGET | (152 << 8)),

         PHX_CHAN_SYNC_MODE            = (int)(PHX_SETANDGET | (153 << 8)),

         PHX_ACQ_BUFFER_START          = (int)(PHX_SETANDGET | (154 << 8)),

         PHX_LUT_BYPASS                = (int)(PHX_SETANDGET | (155 << 8)),

         PHX_COMMS_PORT_NAME           = (int)(PHX_SETANDGET | (156 << 8)),

         PHX_CVB_PARAM                 = (int)(PHX_SETANDGET | (157 << 8)),
         PHX_USR_FORMAT                = (int)(PHX_SETANDGET | (158 << 8)),

         PHX_ACQ_AUTO_RESTART          = (int)(PHX_SETANDGET | (159 << 8)),

         PHX_ACQ_HSCALE                = (int)(PHX_SETANDGET | (160 << 8)),

         PHX_MERGE_CHAN                = (int)(PHX_SETANDGET | (161 << 8)),
         PHX_MERGE_INTRPT_SET          = (int)(PHX_SETANDGET | (162 << 8)),
         PHX_MERGE_INTRPT_CLR          = (int)(PHX_SETANDGET | (163 << 8)),

         PHX_CLSER_INDEX               = (int)(PHX_SETANDGET | (164 << 8)),

         PHX_FIFO_BUFFER_COUNT         = (int)(PHX_SETANDGET | (165 << 8)),

         PHX_REV_FW_FLASH              = (int)(PHX_SETANDGET | (166 << 8)),
         PHX_REV_FW_LIB                = (int)(PHX_SETANDGET | (167 << 8)),

         PHX_PCIE_INFO                 = (int)(PHX_SETANDGET | (168 << 8)),

         PHX_TAP_MODE                  = (int)(PHX_SETANDGET | (169 << 8)),
         PHX_ACQ_IMAGES_PER_BUFFER     = (int)(PHX_SETANDGET | (170 << 8)),


         PHX_CONFIG_FILE               = (int)(PHX_PRE_OPEN | (1 << 8)),
         PHX_BOARD_VARIANT             = (int)(PHX_PRE_OPEN | (2 << 8)),
         PHX_BOARD_NUMBER              = (int)(PHX_PRE_OPEN | (3 << 8)),
         PHX_CHANNEL_NUMBER            = (int)(PHX_PRE_OPEN | (4 << 8)),
         PHX_CONFIG_MODE               = (int)(PHX_PRE_OPEN | (5 << 8)),

         PHX_CXP_INFO                  = (int)(PHX_FBD_SETANDGET | (1 << 8)),
         PHX_CL_INFO                   = PHX_CXP_INFO,
         PHX_CXP_BITRATE               = (int)(PHX_FBD_SETANDGET | (2 << 8)),
         PHX_CXP_POCXP                 = (int)(PHX_FBD_SETANDGET | (3 << 8)),
         PHX_CXP_DISCOVERY             = (int)(PHX_FBD_SETANDGET | (4 << 8)),
         PHX_CXP_UPLINK_TEST_COUNT_OK  = (int)(PHX_FBD_SETANDGET | (5 << 8)),
         PHX_CXP_UPLINK_TEST_COUNT_ERR = (int)(PHX_FBD_SETANDGET | (6 << 8)),
         PHX_CXP_DOWNTEST_COUNT_OK1    = (int)(PHX_FBD_SETANDGET | (7 << 8)),
         PHX_CXP_DOWNTEST_COUNT_OK2    = (int)(PHX_FBD_SETANDGET | (8 << 8)),
         PHX_CXP_DOWNTEST_COUNT_OK3    = (int)(PHX_FBD_SETANDGET | (9 << 8)),
         PHX_CXP_DOWNTEST_COUNT_OK4    = (int)(PHX_FBD_SETANDGET | (10 << 8)),
         PHX_CXP_DOWNTEST_COUNT_ERR1   = (int)(PHX_FBD_SETANDGET | (11 << 8)),
         PHX_CXP_DOWNTEST_COUNT_ERR2   = (int)(PHX_FBD_SETANDGET | (12 << 8)),
         PHX_CXP_DOWNTEST_COUNT_ERR3   = (int)(PHX_FBD_SETANDGET | (13 << 8)),
         PHX_CXP_DOWNTEST_COUNT_ERR4   = (int)(PHX_FBD_SETANDGET | (14 << 8)),
         PHX_CAMERA_POWER              = (int)(PHX_FBD_SETANDGET | (15 << 8)),
         PHX_CXP_BITRATE_MODE          = (int)(PHX_FBD_SETANDGET | (16 << 8)),
         PHX_CXP_POCXP_MODE            = (int)(PHX_FBD_SETANDGET | (17 << 8)),
         PHX_CXP_DISCOVERY_MODE        = (int)(PHX_FBD_SETANDGET | (18 << 8)),
         PHX_CXP_UPLINK_TEST           = (int)(PHX_FBD_SETANDGET | (19 << 8)),
         PHX_CXP_DOWNLINK_TEST         = (int)(PHX_FBD_SETANDGET | (20 << 8)),
         PHX_IO_422IN_CHX              = (int)(PHX_FBD_SETANDGET | (21 << 8)),
         PHX_IO_422IN_CH1              = (int)(PHX_FBD_SETANDGET | (22 << 8)),
         PHX_IO_422IN_CH2              = (int)(PHX_FBD_SETANDGET | (23 << 8)),
         PHX_IO_422IN_CH3              = (int)(PHX_FBD_SETANDGET | (24 << 8)),
         PHX_IO_422IN_CH4              = (int)(PHX_FBD_SETANDGET | (25 << 8)),
         PHX_IO_OPTOIN_CHX             = (int)(PHX_FBD_SETANDGET | (26 << 8)),
         PHX_IO_OPTOIN_CH1             = (int)(PHX_FBD_SETANDGET | (27 << 8)),
         PHX_IO_OPTOIN_CH2             = (int)(PHX_FBD_SETANDGET | (28 << 8)),
         PHX_IO_OPTOIN_CH3             = (int)(PHX_FBD_SETANDGET | (29 << 8)),
         PHX_IO_OPTOIN_CH4             = (int)(PHX_FBD_SETANDGET | (30 << 8)),
         PHX_IO_TTLIN_CHX              = (int)(PHX_FBD_SETANDGET | (31 << 8)),
         PHX_IO_TTLIN_CH1              = (int)(PHX_FBD_SETANDGET | (32 << 8)),
         PHX_IO_TTLIN_CH2              = (int)(PHX_FBD_SETANDGET | (33 << 8)),
         PHX_IO_TTLIN_CH3              = (int)(PHX_FBD_SETANDGET | (34 << 8)),
         PHX_IO_TTLIN_CH4              = (int)(PHX_FBD_SETANDGET | (35 << 8)),
         PHX_IO_422OUT_CHX             = (int)(PHX_FBD_SETANDGET | (36 << 8)),
         PHX_IO_422OUT_CH1             = (int)(PHX_FBD_SETANDGET | (37 << 8)),
         PHX_IO_422OUT_CH2             = (int)(PHX_FBD_SETANDGET | (38 << 8)),
         PHX_IO_422OUT_CH3             = (int)(PHX_FBD_SETANDGET | (39 << 8)),
         PHX_IO_422OUT_CH4             = (int)(PHX_FBD_SETANDGET | (40 << 8)),
         PHX_IO_OPTOOUT_CHX            = (int)(PHX_FBD_SETANDGET | (41 << 8)),
         PHX_IO_OPTOOUT_CH1            = (int)(PHX_FBD_SETANDGET | (42 << 8)),
         PHX_IO_OPTOOUT_CH2            = (int)(PHX_FBD_SETANDGET | (43 << 8)),
         PHX_IO_OPTOOUT_CH3            = (int)(PHX_FBD_SETANDGET | (44 << 8)),
         PHX_IO_OPTOOUT_CH4            = (int)(PHX_FBD_SETANDGET | (45 << 8)),
         PHX_IO_TTLOUT_CHX             = (int)(PHX_FBD_SETANDGET | (46 << 8)),
         PHX_IO_TTLOUT_CH1             = (int)(PHX_FBD_SETANDGET | (47 << 8)),
         PHX_IO_TTLOUT_CH2             = (int)(PHX_FBD_SETANDGET | (48 << 8)),
         PHX_IO_TTLOUT_CH3             = (int)(PHX_FBD_SETANDGET | (49 << 8)),
         PHX_IO_TTLOUT_CH4             = (int)(PHX_FBD_SETANDGET | (50 << 8)),
         PHX_IO_CCOUT_CHX              = (int)(PHX_FBD_SETANDGET | (51 << 8)),
         PHX_IO_CCOUT_CH1              = (int)(PHX_FBD_SETANDGET | (52 << 8)),
         PHX_IO_CCOUT_CH2              = (int)(PHX_FBD_SETANDGET | (53 << 8)),
         PHX_IO_CCOUT_CH3              = (int)(PHX_FBD_SETANDGET | (54 << 8)),
         PHX_IO_CCOUT_CH4              = (int)(PHX_FBD_SETANDGET | (55 << 8)),
         PHX_CAMTRIG_ENCODER_MODE      = (int)(PHX_FBD_SETANDGET | (56 << 8)),
         PHX_CAMTRIG_ENCODER_SRC       = (int)(PHX_FBD_SETANDGET | (57 << 8)),
         PHX_CAMTRIG_MULTIPLIER        = (int)(PHX_FBD_SETANDGET | (58 << 8)),
         PHX_CAMTRIG_DIVIDER           = (int)(PHX_FBD_SETANDGET | (59 << 8)),
         PHX_CAMTRIG_CXPTRIG_SRC       = (int)(PHX_FBD_SETANDGET | (60 << 8)),
         PHX_CAMTRIG_CXPTRIG_MODE      = (int)(PHX_FBD_SETANDGET | (61 << 8)),
         PHX_CAMTRIG_DELAY_MODE        = (int)(PHX_FBD_SETANDGET | (62 << 8)),
         PHX_TIMERD2_VALUE             = (int)(PHX_FBD_SETANDGET | (63 << 8)),

         PHX_FW_NUM_DESIGNS            = (int)(PHX_FBD_SETANDGET | (64 << 8)),
         PHX_FW_DESIGN_LIB             = (int)(PHX_FBD_SETANDGET | (65 << 8)),
         PHX_FW_DESIGN_PROG            = (int)(PHX_FBD_SETANDGET | (66 << 8)),
         PHX_FW_DESIGN_FLASH           = (int)(PHX_FBD_SETANDGET | (67 << 8)),
         PHX_STR_DESIGN_LIB            = (int)(PHX_FBD_SETANDGET | (68 << 8)),
         PHX_STR_DESIGN_FLASH          = (int)(PHX_FBD_SETANDGET | (69 << 8)),

         PHX_FGTRIG_FILTER_NS          = (int)(PHX_FBD_SETANDGET | (70 << 8)),
         PHX_CAMTRIG_FILTER_NS         = (int)(PHX_FBD_SETANDGET | (71 << 8)),
         PHX_CAMTRIG_ENCODER_FILTER_NS = (int)(PHX_FBD_SETANDGET | (72 << 8)),
         PHX_TIMERA1_PERIOD_NS         = (int)(PHX_FBD_SETANDGET | (73 << 8)),
         PHX_TIMERD1FG_VALUE_NS        = (int)(PHX_FBD_SETANDGET | (74 << 8)),
         PHX_TIMERM1_WIDTH_NS          = (int)(PHX_FBD_SETANDGET | (75 << 8)),
         PHX_TIMERM2_WIDTH_NS          = (int)(PHX_FBD_SETANDGET | (76 << 8)),
         PHX_TIMERD2_VALUE_NS          = (int)(PHX_FBD_SETANDGET | (77 << 8)),

         PHX_FPGA_CORE_TEMP            = (int)(PHX_FBD_SETANDGET | (78 << 8)),

         PHX_CAMTRIG_DELAY_MODE_D1CAM  = (int)(PHX_FBD_SETANDGET | (79 << 8)),
         PHX_CAMTRIG_DELAY_MODE_D2CAM  = (int)(PHX_FBD_SETANDGET | (80 << 8)),
         PHX_TIMERD1FG_COUNT           = (int)(PHX_FBD_SETANDGET | (81 << 8)),
         PHX_TIMERA1_MODE              = (int)(PHX_FBD_SETANDGET | (82 << 8)),
         PHX_TIMERA1_PULSE_COUNT       = (int)(PHX_FBD_SETANDGET | (83 << 8)),
         PHX_TIMERA1_SWTRIG            = (int)(PHX_FBD_SETANDGET | (84 << 8)),
         PHX_TIMERD1CAM_VALUE_NS       = (int)(PHX_FBD_SETANDGET | (85 << 8)),
         PHX_TIMERD2CAM_VALUE_NS       = (int)(PHX_FBD_SETANDGET | (86 << 8)),

         PHX_IMAGE_TIMESTAMP_MODE      = (int)(PHX_FBD_SETANDGET | (87 << 8)),

         PHX_DST_ALIGNMENT             = (int)(PHX_FBD_SETANDGET | (88 << 8)),
      };

      public enum etAction
      {
         PHX_CONFIG_SAVE   = (int)(PHX_ACTION | (1 << 8)),
         PHX_FIRMWARE_PROG = (int)(PHX_ACTION | (2 << 8))
      };

      /* PHX_ControlRead/Write Definitions
       * =================================
       */
      public enum etControlPort
      {
         PHX_COMMS_PORT       = (int)(PHX_CONTROL | 0x0000),
         PHX_REGISTER_HOST    = (int)(PHX_CONTROL | 0x0001),
         PHX_REGISTER_DEVICE  = (int)(PHX_CONTROL | 0x0002)
      };

      /* PHX_ComParameterGet Definitions
       * ===============================
       */
      public enum etComParam
      {
         PHX_COM_NUM_PORTS             = (int)(PHX_COM | 0x0000),   /* return the number of Phoenix com ports available */
         PHX_COM_CLSER_CONFIG          = (int)(PHX_COM | 0x0001),   /* LEGACY: convert clser index to config magic number */
         PHX_COM_WIN_CONFIG            = (int)(PHX_COM | 0x0002),   /* LEGACY: convert windows comN index to config magic number */
         PHX_COM_GET_HANDLE            = (int)(PHX_COM | 0x0003),   /* Get Phoenix handle for specified Com Port Index */
         PHX_COM_GET_BOARD_CHAN_INDEX  = (int)(PHX_COM | 0x0004)    /* Get Board/Channel Indices */
      };

      /* Retained for backwards compatibility with previous software releases.
       * Do not use for new applications
       */
      public enum etParamCompatibility
      {
         /* Retained for backwards compatibility with previous software releases.
          * Do not use for new applications
          */
         PHX_ACQ_NUM_IMAGES         = etParam.PHX_ACQ_NUM_BUFFERS,        /*   2 << 8 */
         PHX_ACQTRIG_SRC            = etParam.PHX_FGTRIG_SRC,             /*   4 << 8 */
         PHX_ACQTRIG_TYPE           = etParam.PHX_FGTRIG_MODE,            /*   5 << 8 */
         PHX_ACQ_TYPE               = etParam.PHX_ACQ_FIELD_MODE,         /*   6 << 8 */
         PHX_CAM_SRC_BANDS          = etParam.PHX_CAM_SRC_COL,            /*  18 << 8 */
         PHX_CAPTURE_FORMAT         = etParam.PHX_BUS_FORMAT,             /*  38 << 8 */
         PHX_DST_FORMAT             = etParam.PHX_BUS_FORMAT,
         PHX_DST_PTRS               = etParam.PHX_DST_PTRS_VIRT,          /*  40 << 8 */
         PHX_EVENTCOUNT             = etParam.PHX_COUNTE1_VALUE_NOW,      /*  47 << 8 */
         PHX_EVENT_COUNT            = etParam.PHX_COUNTE1_VALUE_NOW,
         PHX_IO_LVDS_A              = etParam.PHX_IO_CCIO_A,              /*  62 << 8 */
         PHX_IO_LVDS_A_OUT          = etParam.PHX_IO_CCIO_A_OUT,          /*  63 << 8 */
         PHX_IO_LVDS_B              = etParam.PHX_IO_CCIO_B,              /*  64 << 8 */
         PHX_IO_LVDS_B_OUT          = etParam.PHX_IO_CCIO_B_OUT,          /*  65 << 8 */
         PHX_LINETRIG_TIMER_PERIOD  = etParam.PHX_TIMERA1_PERIOD,         /* 113 << 8 */
         PHX_EXPTRIG_SRC            = etParam.PHX_CAMTRIG_SRC,            /* 114 << 8 */
         PHX_EXP_SRC                = etParam.PHX_CAMTRIG_SRC,
         PHX_IO_TIMER_1_PERIOD      = etParam.PHX_TIMERM1_WIDTH,          /* 116 << 8 */
         PHX_EXP_CTRLIO_1_DELAY     = etParam.PHX_TIMERM1_WIDTH,
         PHX_IO_TIMER_2_PERIOD      = etParam.PHX_TIMERM2_WIDTH,          /* 118 << 8 */
         PHX_EXP_CTRLIO_2_DELAY     = etParam.PHX_TIMERM2_WIDTH,
         PHX_ACQTRIG_DELAY_TYPE     = etParam.PHX_FGTRIG_DELAY_MODE,      /* 120 << 8 */
         PHX_ACQTRIG_DELAY          = etParam.PHX_TIMERD1_VALUE,          /* 121 << 8 */
         PHX_EVENTCOUNT_TYPE        = etParam.PHX_EVENTCOUNT_SRC,         /* 122 << 8 */
         PHX_EVENTGATE_TYPE         = etParam.PHX_EVENTGATE_SRC,          /* 123 << 8 */
         PHX_BUFFER_READY_COUNT     = etParam.PHX_COUNT_BUFFER_READY,     /* 128 << 8 */
         PHX_BUFFER_READY_COUNTER   = etParam.PHX_COUNT_BUFFER_READY_NOW, /* 129 << 8 */
         PHX_LUT_SHIFT              = etParam.PHX_BIT_SHIFT,              /* 130 << 8 */
         PHX_IO_422_OUT             = etParam.PHX_IO_CCIO_OUT,            /* 132 << 8 */
         PHX_ACQTRIG_ALIGN          = etParam.PHX_FGTRIG_ALIGN,           /* 144 << 8 */
         PHX_EVENTCOUNT_AT_GATE     = etParam.PHX_COUNTE1_VALUE_DB,       /* 152 << 8 */
         PHX_USER_FORMAT            = etParam.PHX_USR_FORMAT,             /* 158 << 8 */
         PHX_REV_FLASH              = etParam.PHX_REV_FW_FLASH,           /* 166 << 8 */
         PHX_TIMERD1_VALUE_NS       = etParam.PHX_TIMERD1FG_VALUE_NS      /*  74 << 8 */
      };

      /* GetAndSet Parameter Values   
       * ===========================
       * These define the values passed for a given parameter within the PHX_ParameterGet & PHX_ParameterSet
       * interface.  Each value consists of a unique incremental number.  However each value contains
       * the parameter value coded within the top 8 bits, which is used as an error check that the correct
       * value is being used for the parameter.
       * They are implemented as enums to make them visible within the debugger.
       */
      public enum etParamValue
      {
         PHX_INVALID_PARAMVALUE = 0,

         /* Boolean flags */
         PHX_ENABLE = 1,
         PHX_DISABLE,

         /* PHX_COMMS_DATA */
         PHX_COMMS_DATA_5 = (int)(etParam.PHX_COMMS_DATA + 1),
         PHX_COMMS_DATA_6,
         PHX_COMMS_DATA_7,
         PHX_COMMS_DATA_8,

         /* PHX_COMMS_STOP */
         PHX_COMMS_STOP_1 = (int)(etParam.PHX_COMMS_STOP + 1),
         PHX_COMMS_STOP_1_5,
         PHX_COMMS_STOP_2,

         /* PHX_COMMS_PARITY */
         PHX_COMMS_PARITY_NONE = (int)(etParam.PHX_COMMS_PARITY + 1),
         PHX_COMMS_PARITY_EVEN,
         PHX_COMMS_PARITY_ODD,

         /* PHX_COMMS_FLOW */
         PHX_COMMS_FLOW_NONE = (int)(etParam.PHX_COMMS_FLOW + 1),
         PHX_COMMS_FLOW_HW,
         PHX_COMMS_FLOW_SW,

         /* PHX_COMMS_STANDARD */
         PHX_COMMS_STANDARD_RS232 = (int)(etParam.PHX_COMMS_STANDARD + 1),
         PHX_COMMS_STANDARD_LVDS,

         /* PHX_IO_OPTO_OUT_SET/CLR */
         PHX_IO_OPTO_OUT1 = 0x00000001,
         PHX_IO_OPTO_OUT2 = 0x00000002,
         PHX_IO_OPTO_OUT3 = 0x00000004,
         PHX_IO_OPTO_OUT4 = 0x00000008,

         /* PHX_IO_OPTO_SET/CLR */
         PHX_IO_OPTO1 = 0x00000001,
         PHX_IO_OPTO2 = 0x00000002,
         PHX_IO_OPTO3 = 0x00000004,
         PHX_IO_OPTO4 = 0x00000008,

         /* PHX_STATUS */
         PHX_STATUS_IDLE = (int)(etParam.PHX_STATUS + 1),
         PHX_STATUS_ACQ_IN_PROGRESS,
         PHX_STATUS_WAITING_FOR_TRIGGER,

         /* PHX_CAM_TYPE */
         PHX_CAM_LINESCAN_ROI = (int)(etParam.PHX_CAM_TYPE + 1),
         PHX_CAM_LINESCAN_NO_ROI,
         PHX_CAM_AREASCAN_ROI,
         PHX_CAM_AREASCAN_NO_ROI,
         PHX_CAM_MULTILINESCAN_ROI,

         /* PHX_CAM_FORMAT */
         PHX_CAM_INTERLACED = (int)(etParam.PHX_CAM_FORMAT + 1),
         PHX_CAM_NON_INTERLACED,

         /* PHX_CAM_SRC_COL */
         PHX_CAM_SRC_MONO     = 0x00000001,
         PHX_CAM_SRC_RGB      = 0x00000003,
         PHX_CAM_SRC_BAYER_RG = (int)(etParam.PHX_CAM_SRC_COL + 1),
         PHX_CAM_SRC_BAYER_GR,
         PHX_CAM_SRC_BAYER_GB,
         PHX_CAM_SRC_BAYER_BG,
         PHX_CAM_SRC_YUV422,
         PHX_CAM_SRC_RGBA,

         /* PHX_CAM_HTAP_DIR */
         PHX_CAM_HTAP_LEFT = (int)(etParam.PHX_CAM_HTAP_DIR + 1),
         PHX_CAM_HTAP_RIGHT,
         PHX_CAM_HTAP_CONVERGE,
         PHX_CAM_HTAP_DIVERGE,

         /* PHX_CAM_HTAP_TYPE */
         PHX_CAM_HTAP_LINEAR = (int)(etParam.PHX_CAM_HTAP_TYPE + 1),
         PHX_CAM_HTAP_OFFSET_1,
         PHX_CAM_HTAP_ALTERNATE,
         PHX_CAM_HTAP_OFFSET_2,
         PHX_CAM_HTAP_SPAN,

         /* PHX_CAM_HTAP_ORDER */
         PHX_CAM_HTAP_ASCENDING = (int)(etParam.PHX_CAM_HTAP_ORDER + 1),
         PHX_CAM_HTAP_DESCENDING,

         /* PHX_CAM_VTAP_DIR */
         PHX_CAM_VTAP_TOP = (int)(etParam.PHX_CAM_VTAP_DIR + 1),
         PHX_CAM_VTAP_BOTTOM,
         PHX_CAM_VTAP_CONVERGE,
         PHX_CAM_VTAP_DIVERGE,

         /* PHX_CAM_VTAP_TYPE */
         PHX_CAM_VTAP_LINEAR = (int)(etParam.PHX_CAM_VTAP_TYPE + 1),
         PHX_CAM_VTAP_OFFSET,
         PHX_CAM_VTAP_ALTERNATE,

         /* PHX_CAM_VTAP_ORDER */
         PHX_CAM_VTAP_ASCENDING = (int)(etParam.PHX_CAM_VTAP_ORDER + 1),
         PHX_CAM_VTAP_DESCENDING,

         /* PHX_CAM_CLOCK_POLARITY */
         PHX_CAM_CLOCK_POS = (int)(etParam.PHX_CAM_CLOCK_POLARITY + 1),
         PHX_CAM_CLOCK_NEG,

         /* PHX_CAM_CLOCK_MAX */
         PHX_CAM_CLOCK_MAX_DEFAULT = (int)(etParam.PHX_CAM_CLOCK_MAX + 1),
         PHX_CAM_CLOCK_MAX_85MHZ,

         /* PHX_ACQ_TYPE */
         PHX_ACQ_FRAME_12 = (int)(etParam.PHX_ACQ_FIELD_MODE + 1),
         PHX_ACQ_FRAME_21,
         PHX_ACQ_FIELD_12,
         PHX_ACQ_FIELD_21,
         PHX_ACQ_FIELD_1,
         PHX_ACQ_FIELD_2,
         PHX_ACQ_FIELD_NEXT,
         PHX_ACQ_LINE_DOUBLE_12,
         PHX_ACQ_LINE_DOUBLE_21,
         PHX_ACQ_LINE_DOUBLE_NEXT,
         PHX_ACQ_LINE_DOUBLE_1,
         PHX_ACQ_LINE_DOUBLE_2,

         /* PHX_ACQ_XSUB */
         PHX_ACQ_X1 = (int)(etParam.PHX_ACQ_XSUB + 1),
         PHX_ACQ_X2,
         PHX_ACQ_X4,
         PHX_ACQ_X8,

         /* PHX_DST_PTR_TYPE */
         PHX_DST_PTR_INTERNAL = (int)(etParam.PHX_DST_PTR_TYPE + 1),
         PHX_DST_PTR_USER_VIRT,
         PHX_DST_PTR_USER_PHYS,

         /* PHX_DATASTREAM_VALID */
         PHX_DATASTREAM_ALWAYS = (int)(etParam.PHX_DATASTREAM_VALID + 1),
         PHX_DATASTREAM_LINE_ONLY,
         PHX_DATASTREAM_FRAME_ONLY,
         PHX_DATASTREAM_FRAME_AND_LINE,

         /* PHX_DATASRC */
         PHX_DATASRC_CAMERA = (int)(etParam.PHX_DATASRC + 1),
         PHX_DATASRC_SIMULATOR_STATIC,
         PHX_DATASRC_SIMULATOR_ROLL,

         /* PHX_DST_FORMAT */
         PHX_BUS_FORMAT_MONO8 = (int)(etParam.PHX_BUS_FORMAT + 1),
         PHX_BUS_FORMAT_MONO16,
         PHX_BUS_FORMAT_MONO32,
         PHX_BUS_FORMAT_MONO36,        /* No longer supported in version 2 release */
         PHX_BUS_FORMAT_BGR5,
         PHX_BUS_FORMAT_BGR565,
         PHX_BUS_FORMAT_XBGR8,         /* PHX_DST_FORMAT_RGBX32, */
         PHX_BUS_FORMAT_BGRX8,         /* PHX_DST_FORMAT_XRGB32, */
         PHX_BUS_FORMAT_BGR16,
         PHX_BUS_FORMAT_RGB5,
         PHX_BUS_FORMAT_RGB565,
         PHX_BUS_FORMAT_XRGB8,         /* PHX_DST_FORMAT_BGRX32, */
         PHX_BUS_FORMAT_RGBX8,         /* PHX_DST_FORMAT_XBGR32, */
         PHX_BUS_FORMAT_RGB16,
         PHX_BUS_FORMAT_BGR101210,     /* Added in version 2 release */
         PHX_BUS_FORMAT_RGB101210,
         PHX_BUS_FORMAT_BGR8,          /* Added in version 2.25 release */
         PHX_BUS_FORMAT_RGB8,
         PHX_BUS_FORMAT_MONO10,
         PHX_BUS_FORMAT_MONO12,
         PHX_BUS_FORMAT_MONO14,
         PHX_DST_FORMAT_BAY8,
         PHX_DST_FORMAT_BAY10,
         PHX_DST_FORMAT_BAY12,
         PHX_DST_FORMAT_BAY14,
         PHX_DST_FORMAT_BAY16,
         PHX_BUS_FORMAT_MONO12P,
         PHX_BUS_FORMAT_BGR12,
         PHX_BUS_FORMAT_RGB12,
         PHX_BUS_FORMAT_YUV422_8,
         PHX_DST_FORMAT_Y12B,
         PHX_BUS_FORMAT_RGB8_PLANAR,
         PHX_BUS_FORMAT_MONO10P,
         PHX_BUS_FORMAT_MONO14P,
         PHX_BUS_FORMAT_RGBA8,
         PHX_BUS_FORMAT_RGBA10,
         PHX_BUS_FORMAT_RGBA12,
         PHX_BUS_FORMAT_RGBA14,
         PHX_BUS_FORMAT_RGBA16,
         PHX_BUS_FORMAT_BAYER_GR8,
         PHX_BUS_FORMAT_BAYER_RG8,
         PHX_BUS_FORMAT_BAYER_GB8,
         PHX_BUS_FORMAT_BAYER_BG8,
         PHX_BUS_FORMAT_BAYER_GR10,
         PHX_BUS_FORMAT_BAYER_RG10,
         PHX_BUS_FORMAT_BAYER_GB10,
         PHX_BUS_FORMAT_BAYER_BG10,
         PHX_BUS_FORMAT_BAYER_GR12,
         PHX_BUS_FORMAT_BAYER_RG12,
         PHX_BUS_FORMAT_BAYER_GB12,
         PHX_BUS_FORMAT_BAYER_BG12,
         PHX_BUS_FORMAT_BAYER_GR14,
         PHX_BUS_FORMAT_BAYER_RG14,
         PHX_BUS_FORMAT_BAYER_GB14,
         PHX_BUS_FORMAT_BAYER_BG14,
         PHX_BUS_FORMAT_BAYER_GR16,
         PHX_BUS_FORMAT_BAYER_RG16,
         PHX_BUS_FORMAT_BAYER_GB16,
         PHX_BUS_FORMAT_BAYER_BG16,
         PHX_BUS_FORMAT_BGR10,
         PHX_BUS_FORMAT_RGB10,
         PHX_BUS_FORMAT_BGR14,
         PHX_BUS_FORMAT_RGB14,

         /* PHX_USR_FORMAT */
         PHX_USR_FORMAT_MONO8 = (int)(etParam.PHX_USR_FORMAT + 1),
         PHX_USR_FORMAT_MONO16,
         PHX_USR_FORMAT_MONO32,
         PHX_USR_FORMAT_MONO36,        /* No longer supported in version 2 release */
         PHX_USR_FORMAT_BGR5,
         PHX_USR_FORMAT_BGR565,
         PHX_USR_FORMAT_XBGR8,         /* PHX_USER_FORMAT_RGBX32, */
         PHX_USR_FORMAT_BGRX8,         /* PHX_USER_FORMAT_XRGB32, */
         PHX_USR_FORMAT_BGR16,
         PHX_USR_FORMAT_RGB5,
         PHX_USR_FORMAT_RGB565,
         PHX_USR_FORMAT_XRGB8,         /* PHX_USER_FORMAT_BGRX32, */
         PHX_USR_FORMAT_RGBX8,         /* PHX_USER_FORMAT_XBGR32, */
         PHX_USR_FORMAT_RGB16,
         PHX_USR_FORMAT_BGR101210,     /* Added in version 2 release */
         PHX_USR_FORMAT_RGB101210,
         PHX_USR_FORMAT_BGR8,          /* Added in version 2.25 release */
         PHX_USR_FORMAT_RGB8,
         PHX_USR_FORMAT_MONO10,
         PHX_USR_FORMAT_MONO12,
         PHX_USR_FORMAT_MONO14,
         PHX_USER_FORMAT_BAY8,
         PHX_USER_FORMAT_BAY10,
         PHX_USER_FORMAT_BAY12,
         PHX_USER_FORMAT_BAY14,
         PHX_USER_FORMAT_BAY16,
         PHX_USR_FORMAT_MONO12P,
         PHX_USR_FORMAT_BGR12,
         PHX_USR_FORMAT_RGB12,
         PHX_USR_FORMAT_YUV422_8,
         PHX_USER_FORMAT_Y12B,
         PHX_USR_FORMAT_RGB8_PLANAR,
         PHX_USR_FORMAT_MONO10P,
         PHX_USR_FORMAT_MONO14P,
         PHX_USR_FORMAT_RGBA8,
         PHX_USR_FORMAT_RGBA10,
         PHX_USR_FORMAT_RGBA12,
         PHX_USR_FORMAT_RGBA14,
         PHX_USR_FORMAT_RGBA16,
         PHX_USR_FORMAT_BAYER_GR8,
         PHX_USR_FORMAT_BAYER_RG8,
         PHX_USR_FORMAT_BAYER_GB8,
         PHX_USR_FORMAT_BAYER_BG8,
         PHX_USR_FORMAT_BAYER_GR10,
         PHX_USR_FORMAT_BAYER_RG10,
         PHX_USR_FORMAT_BAYER_GB10,
         PHX_USR_FORMAT_BAYER_BG10,
         PHX_USR_FORMAT_BAYER_GR12,
         PHX_USR_FORMAT_BAYER_RG12,
         PHX_USR_FORMAT_BAYER_GB12,
         PHX_USR_FORMAT_BAYER_BG12,
         PHX_USR_FORMAT_BAYER_GR14,
         PHX_USR_FORMAT_BAYER_RG14,
         PHX_USR_FORMAT_BAYER_GB14,
         PHX_USR_FORMAT_BAYER_BG14,
         PHX_USR_FORMAT_BAYER_GR16,
         PHX_USR_FORMAT_BAYER_RG16,
         PHX_USR_FORMAT_BAYER_GB16,
         PHX_USR_FORMAT_BAYER_BG16,
         PHX_USR_FORMAT_BGR10,
         PHX_USR_FORMAT_RGB10,
         PHX_USR_FORMAT_BGR14,
         PHX_USR_FORMAT_RGB14,

         /* PHX_LINETRIG_SRC */
         PHX_LINETRIG_NONE = (int)(etParam.PHX_LINETRIG_SRC + 1),
         PHX_LINETRIG_AUXIN_1_RISING,        /* Relative */
         PHX_LINETRIG_AUXIN_1_FALLING,
         PHX_LINETRIG_CTRLIN_2_RISING,       /* No longer supported; retained for enum backward compatibility */
         PHX_LINETRIG_CTRLIN_2_FALLING,      /* No longer supported; retained for enum backward compatibility */
         PHX_LINETRIG_AUXIN_2_RISING,
         PHX_LINETRIG_AUXIN_2_FALLING,
         PHX_LINETRIG_TIMER,
         PHX_LINETRIG_AUXIN_A1_RISING,       /* Absolute */
         PHX_LINETRIG_AUXIN_A1_FALLING,
         PHX_LINETRIG_AUXIN_A2_RISING,
         PHX_LINETRIG_AUXIN_A2_FALLING,
         PHX_LINETRIG_AUXIN_B1_RISING,
         PHX_LINETRIG_AUXIN_B1_FALLING,
         PHX_LINETRIG_AUXIN_B2_RISING,
         PHX_LINETRIG_AUXIN_B2_FALLING,

         /* PHX_LINETRIG_TIMER_CTRL */
         PHX_LINETRIG_TIMER_TIME = (int)(etParam.PHX_LINETRIG_TIMER_CTRL + 1),
         PHX_LINETRIG_TIMER_DISABLE,
         PHX_LINETRIG_TIMER_LINES,

         /* PHX_CAMTRIG_SRC */
         PHX_CAMTRIG_SRC_LINETRIG = (int)(etParam.PHX_CAMTRIG_SRC + 1),
         PHX_CAMTRIG_SRC_FGTRIGA_CHX,
         PHX_CAMTRIG_SRC_NONE,
         PHX_CAMTRIG_SRC_SWTRIG_CHX,
         PHX_CAMTRIG_SRC_AUXIN_1_RISE,        /* Relative */
         PHX_CAMTRIG_SRC_AUXIN_1_FALL,
         PHX_CAMTRIG_SRC_AUXIN_2_RISE,
         PHX_CAMTRIG_SRC_AUXIN_2_FALL,
         PHX_CAMTRIG_SRC_TIMERA1_CHX,
         PHX_CAMTRIG_SRC_AUXIN_A1_RISE,       /* Absolute */
         PHX_CAMTRIG_SRC_AUXIN_A1_FALL,
         PHX_CAMTRIG_SRC_AUXIN_A2_RISE,
         PHX_CAMTRIG_SRC_AUXIN_A2_FALL,
         PHX_CAMTRIG_SRC_AUXIN_B1_RISE,
         PHX_CAMTRIG_SRC_AUXIN_B1_FALL,
         PHX_CAMTRIG_SRC_AUXIN_B2_RISE,
         PHX_CAMTRIG_SRC_AUXIN_B2_FALL,
         PHX_CAMTRIG_SRC_FGTRIGA_CH1,
         PHX_CAMTRIG_SRC_FGTRIGA_CH2,
         PHX_CAMTRIG_SRC_FGTRIGA_CH3,
         PHX_CAMTRIG_SRC_FGTRIGA_CH4,
         PHX_CAMTRIG_SRC_SWTRIG_CH1,
         PHX_CAMTRIG_SRC_SWTRIG_CH2,
         PHX_CAMTRIG_SRC_SWTRIG_CH3,
         PHX_CAMTRIG_SRC_SWTRIG_CH4,
         PHX_CAMTRIG_SRC_TIMERA1_CH1,
         PHX_CAMTRIG_SRC_TIMERA1_CH2,
         PHX_CAMTRIG_SRC_TIMERA1_CH3,
         PHX_CAMTRIG_SRC_TIMERA1_CH4,
         PHX_CAMTRIG_SRC_422IN_CHX_0_RISE,
         PHX_CAMTRIG_SRC_422IN_CH1_0_RISE,
         PHX_CAMTRIG_SRC_422IN_CH2_0_RISE,
         PHX_CAMTRIG_SRC_422IN_CH3_0_RISE,
         PHX_CAMTRIG_SRC_422IN_CH4_0_RISE,
         PHX_CAMTRIG_SRC_422IN_CHX_0_FALL,
         PHX_CAMTRIG_SRC_422IN_CH1_0_FALL,
         PHX_CAMTRIG_SRC_422IN_CH2_0_FALL,
         PHX_CAMTRIG_SRC_422IN_CH3_0_FALL,
         PHX_CAMTRIG_SRC_422IN_CH4_0_FALL,
         PHX_CAMTRIG_SRC_OPTOIN_CHX_0_RISE,
         PHX_CAMTRIG_SRC_OPTOIN_CH1_0_RISE,
         PHX_CAMTRIG_SRC_OPTOIN_CH2_0_RISE,
         PHX_CAMTRIG_SRC_OPTOIN_CH3_0_RISE,
         PHX_CAMTRIG_SRC_OPTOIN_CH4_0_RISE,
         PHX_CAMTRIG_SRC_OPTOIN_CHX_0_FALL,
         PHX_CAMTRIG_SRC_OPTOIN_CH1_0_FALL,
         PHX_CAMTRIG_SRC_OPTOIN_CH2_0_FALL,
         PHX_CAMTRIG_SRC_OPTOIN_CH3_0_FALL,
         PHX_CAMTRIG_SRC_OPTOIN_CH4_0_FALL,
         PHX_CAMTRIG_SRC_TTLIN_CHX_0_RISE,
         PHX_CAMTRIG_SRC_TTLIN_CH1_0_RISE,
         PHX_CAMTRIG_SRC_TTLIN_CH2_0_RISE,
         PHX_CAMTRIG_SRC_TTLIN_CH3_0_RISE,
         PHX_CAMTRIG_SRC_TTLIN_CH4_0_RISE,
         PHX_CAMTRIG_SRC_TTLIN_CHX_0_FALL,
         PHX_CAMTRIG_SRC_TTLIN_CH1_0_FALL,
         PHX_CAMTRIG_SRC_TTLIN_CH2_0_FALL,
         PHX_CAMTRIG_SRC_TTLIN_CH3_0_FALL,
         PHX_CAMTRIG_SRC_TTLIN_CH4_0_FALL,
         PHX_CAMTRIG_SRC_ENCODER_CHX,
         PHX_CAMTRIG_SRC_ENCODER_CH1,
         PHX_CAMTRIG_SRC_ENCODER_CH2,
         PHX_CAMTRIG_SRC_ENCODER_CH3,
         PHX_CAMTRIG_SRC_ENCODER_CH4,

         /* PHX_CAMTRIG_ENCODER_MODE */
         PHX_CAMTRIG_ENCODER_MODE1 = (int)(etParam.PHX_CAMTRIG_ENCODER_MODE + 1),
         PHX_CAMTRIG_ENCODER_MODE2,
         PHX_CAMTRIG_ENCODER_MODE3,
         PHX_CAMTRIG_ENCODER_MODE4,
         PHX_CAMTRIG_ENCODER_MODE5,

         /* PHX_CAMTRIG_ENCODER_SRC */
         PHX_CAMTRIG_SRC_422IN_CHX_0 = (int)(etParam.PHX_CAMTRIG_ENCODER_SRC + 1),
         PHX_CAMTRIG_SRC_422IN_CH1_0,
         PHX_CAMTRIG_SRC_422IN_CH2_0,
         PHX_CAMTRIG_SRC_422IN_CH3_0,
         PHX_CAMTRIG_SRC_422IN_CH4_0,
         PHX_CAMTRIG_SRC_OPTOIN_CHX_0,
         PHX_CAMTRIG_SRC_OPTOIN_CH1_0,
         PHX_CAMTRIG_SRC_OPTOIN_CH2_0,
         PHX_CAMTRIG_SRC_OPTOIN_CH3_0,
         PHX_CAMTRIG_SRC_OPTOIN_CH4_0,
         PHX_CAMTRIG_SRC_TTLIN_CHX_0,
         PHX_CAMTRIG_SRC_TTLIN_CH1_0,
         PHX_CAMTRIG_SRC_TTLIN_CH2_0,
         PHX_CAMTRIG_SRC_TTLIN_CH3_0,
         PHX_CAMTRIG_SRC_TTLIN_CH4_0,

         /* PHX_CAMTRIG_DELAY_MODE */
         PHX_CAMTRIG_DELAY_NONE = (int)(etParam.PHX_CAMTRIG_DELAY_MODE + 1),
         PHX_CAMTRIG_DELAY_TIME,
         PHX_CAMTRIG_DELAY_LINE,

         /* PHX_CAMTRIG_CXPTRIG_SRC */
         PHX_CAMTRIG_CXPTRIG_NONE = (int)(etParam.PHX_CAMTRIG_CXPTRIG_SRC + 1),
         PHX_CAMTRIG_CXPTRIG_TIMERM1_CHX,
         PHX_CAMTRIG_CXPTRIG_TIMERM1_CH1,
         PHX_CAMTRIG_CXPTRIG_TIMERM1_CH2,
         PHX_CAMTRIG_CXPTRIG_TIMERM1_CH3,
         PHX_CAMTRIG_CXPTRIG_TIMERM1_CH4,
         PHX_CAMTRIG_CXPTRIG_TIMERM2_CHX,
         PHX_CAMTRIG_CXPTRIG_TIMERM2_CH1,
         PHX_CAMTRIG_CXPTRIG_TIMERM2_CH2,
         PHX_CAMTRIG_CXPTRIG_TIMERM2_CH3,
         PHX_CAMTRIG_CXPTRIG_TIMERM2_CH4,
         PHX_CAMTRIG_CXPTRIG_SW_RISE,
         PHX_CAMTRIG_CXPTRIG_SW_FALL,
         PHX_CAMTRIG_CXPTRIG_FGTRIGD_CHX,
         PHX_CAMTRIG_CXPTRIG_FGTRIGD_CH1,
         PHX_CAMTRIG_CXPTRIG_FGTRIGD_CH2,
         PHX_CAMTRIG_CXPTRIG_FGTRIGD_CH3,
         PHX_CAMTRIG_CXPTRIG_FGTRIGD_CH4,

         /* PHX_CAMTRIG_CXPTRIG_MODE */
         PHX_CAMTRIG_CXPTRIG_RISEFALL = (int)(etParam.PHX_CAMTRIG_CXPTRIG_MODE + 1),
         PHX_CAMTRIG_CXPTRIG_RISEFALL_INV,
         PHX_CAMTRIG_CXPTRIG_RISE,
         PHX_CAMTRIG_CXPTRIG_RISE_INV,
         PHX_CAMTRIG_CXPTRIG_FALL,
         PHX_CAMTRIG_CXPTRIG_FALL_INV,

         /* PHX_EXP_CTRLIO_1 */
         PHX_EXP_CTRLIO_1_HW_POS = (int)(etParam.PHX_EXP_CTRLIO_1 + 1),
         PHX_EXP_CTRLIO_1_HW_NEG,
         PHX_EXP_CTRLIO_1_SW_POS,
         PHX_EXP_CTRLIO_1_SW_NEG,

         /* PHX_EXP_CTRLIO_2 */
         PHX_EXP_CTRLIO_2_HW_POS = (int)(etParam.PHX_EXP_CTRLIO_2 + 1),
         PHX_EXP_CTRLIO_2_HW_NEG,
         PHX_EXP_CTRLIO_2_SW_POS,
         PHX_EXP_CTRLIO_2_SW_NEG,

         /* PHX_EXP_LINESTART */
         PHX_EXP_LINESTART_LINE = (int)(etParam.PHX_EXP_LINESTART + 1),
         PHX_EXP_LINESTART_CCIO_2,     /* Relative */
         PHX_EXP_LINESTART_CCIO_A2,    /* Absolute */
         PHX_EXP_LINESTART_CCIO_B2,

         /* PHX_FGTRIG_SRC */
         PHX_FGTRIG_SRC_OPTO_A1 = (int)(etParam.PHX_FGTRIG_SRC + 1), /* Absolute */
         PHX_FGTRIG_SRC_OPTO_A2,
         PHX_FGTRIG_SRC_OPTO_B1,
         PHX_FGTRIG_SRC_OPTO_B2,
         PHX_FGTRIG_SRC_CTRLIN_A1,
         PHX_FGTRIG_SRC_CTRLIN_A2,
         PHX_FGTRIG_SRC_CTRLIN_A3,
         PHX_FGTRIG_SRC_CTRLIN_B1,
         PHX_FGTRIG_SRC_CTRLIN_B2,
         PHX_FGTRIG_SRC_CTRLIN_B3,
         PHX_FGTRIG_SRC_CCIO_A1,
         PHX_FGTRIG_SRC_CCIO_A2,
         PHX_FGTRIG_SRC_CCIO_B1,
         PHX_FGTRIG_SRC_CCIO_B2,
         PHX_FGTRIG_SRC_AUXIN_A1,
         PHX_FGTRIG_SRC_AUXIN_A2,
         PHX_FGTRIG_SRC_AUXIN_B1,
         PHX_FGTRIG_SRC_AUXIN_B2,
         PHX_FGTRIG_SRC_OPTO_1,
         PHX_FGTRIG_SRC_OPTO_2,
         PHX_FGTRIG_SRC_AUXIN_1,
         PHX_FGTRIG_SRC_AUXIN_2,
         PHX_FGTRIG_SRC_CTRLIN_1,
         PHX_FGTRIG_SRC_CTRLIN_2,
         PHX_FGTRIG_SRC_CTRLIN_3,
         PHX_FGTRIG_SRC_CCIO_1,
         PHX_FGTRIG_SRC_CCIO_2,
         PHX_FGTRIG_SRC_TIMERA1_CHX,
         PHX_FGTRIG_SRC_TIMERA1_CH1,
         PHX_FGTRIG_SRC_TIMERA1_CH2,
         PHX_FGTRIG_SRC_TIMERA1_CH3,
         PHX_FGTRIG_SRC_TIMERA1_CH4,
         PHX_FGTRIG_SRC_422IN_CHX_0,
         PHX_FGTRIG_SRC_422IN_CH1_0,
         PHX_FGTRIG_SRC_422IN_CH2_0,
         PHX_FGTRIG_SRC_422IN_CH3_0,
         PHX_FGTRIG_SRC_422IN_CH4_0,
         PHX_FGTRIG_SRC_OPTOIN_CHX_0,
         PHX_FGTRIG_SRC_OPTOIN_CH1_0,
         PHX_FGTRIG_SRC_OPTOIN_CH2_0,
         PHX_FGTRIG_SRC_OPTOIN_CH3_0,
         PHX_FGTRIG_SRC_OPTOIN_CH4_0,
         PHX_FGTRIG_SRC_TTLIN_CHX_0,
         PHX_FGTRIG_SRC_TTLIN_CH1_0,
         PHX_FGTRIG_SRC_TTLIN_CH2_0,
         PHX_FGTRIG_SRC_TTLIN_CH3_0,
         PHX_FGTRIG_SRC_TTLIN_CH4_0,

         /* PHX_FGTRIG_MODE */
         PHX_FGTRIG_FREERUN = (int)(etParam.PHX_FGTRIG_MODE + 1),
         PHX_FGTRIG_FIRST_POS_EDGE,
         PHX_FGTRIG_FIRST_NEG_EDGE,
         PHX_FGTRIG_EACH_POS_EDGE,
         PHX_FGTRIG_EACH_NEG_EDGE,
         PHX_FGTRIG_FIRST_POS_LEVEL,
         PHX_FGTRIG_FIRST_NEG_LEVEL,
         PHX_FGTRIG_EACH_POS_LEVEL,
         PHX_FGTRIG_EACH_NEG_LEVEL,
         PHX_FGTRIG_GATED_POS_LEVEL,
         PHX_FGTRIG_GATED_NEG_LEVEL,
         PHX_FGTRIG_EACH_POS_EDGE_NR,
         PHX_FGTRIG_EACH_NEG_EDGE_NR,

         /* PHX_FGTRIG_ALIGN */
         PHX_FGTRIG_ALIGN_NONE = (int)(etParam.PHX_FGTRIG_ALIGN + 1),
         PHX_FGTRIG_ALIGN_TO_CLK,
         PHX_FGTRIG_ALIGN_TO_LINE,
         PHX_FGTRIG_ALIGN_TO_FRAME,

         /* PHX_FGTRIG_DELAY_MODE */
         PHX_FGTRIG_DELAY_NONE = (int)(etParam.PHX_FGTRIG_DELAY_MODE + 1),
         PHX_FGTRIG_DELAY_LINE,
         PHX_FGTRIG_DELAY_TIME,
         PHX_FGTRIG_DELAY_TIMERM1_CHX,
         PHX_FGTRIG_DELAY_TIMERM1_CH1,
         PHX_FGTRIG_DELAY_TIMERM1_CH2,
         PHX_FGTRIG_DELAY_TIMERM1_CH3,
         PHX_FGTRIG_DELAY_TIMERM1_CH4,

         /* PHX_TIMERA1_MODE */
         PHX_TIMERA1_MODE_RUN = (int)(etParam.PHX_TIMERA1_MODE + 1),
         PHX_TIMERA1_MODE_N_PULSES,
         PHX_TIMERA1_MODE_STOP,
         PHX_TIMERA1_MODE_ABORT,

         /* PHX_TIMESTAMP_MODE */
         PHX_IMAGE_TIMESTAMP_NONE = (int)(etParam.PHX_IMAGE_TIMESTAMP_MODE + 1),
         PHX_IMAGE_TIMESTAMP_MODE1,

         /* PHX_EVENTCOUNT_SRC */
         PHX_EVENTCOUNT_LINE = (int)(etParam.PHX_EVENTCOUNT_SRC + 1),
         PHX_EVENTCOUNT_FRAME,
         PHX_EVENTCOUNT_TIME,

         /* PHX_EVENTGATE_SRC */
         PHX_EVENTGATE_ACQTRIG = (int)(etParam.PHX_EVENTGATE_SRC + 1),
         PHX_EVENTGATE_FRAME,
         PHX_EVENTGATE_ACQ,
         PHX_EVENTGATE_LINE,

         /* PHX_DST_ENDIAN */
         PHX_DST_LITTLE_ENDIAN = (int)(etParam.PHX_DST_ENDIAN + 1),
         PHX_DST_BIG_ENDIAN,

         /* PHX_CHAN_SYNC_MODE */
         PHX_CHAN_SYNC_NONE = (int)(etParam.PHX_CHAN_SYNC_MODE + 1),
         PHX_CHAN_SYNC_ACQEXPTRIG,

         /* PHX_CVB_PARAM */
         PHX_CVB_WIDTH = (int)(etParam.PHX_CVB_PARAM + 1),
         PHX_CVB_HEIGHT,
         PHX_CVB_PLANES,
         PHX_CVB_BIT_DEPTH,   /* per plane */
         PHX_CVB_BYTES_PER_PIXEL,
         PHX_CVB_X_STEP,
         PHX_CVB_Y_STEP,
         PHX_CVB_PLANE_STEP,
         PHX_CVB_MALLOC,

         /* PHX_ACQ_AUTO_RESTART, these parameter values are 'OR'able together */
         PHX_ACQ_AUTO_NONE          = (int)(etParam.PHX_ACQ_AUTO_RESTART + 0x01),
         PHX_ACQ_AUTO_SYNC_LOST     = (int)(etParam.PHX_ACQ_AUTO_RESTART + 0x02),
         PHX_ACQ_AUTO_FIFO_OVERFLOW = (int)(etParam.PHX_ACQ_AUTO_RESTART + 0x04),

         /* PHX_BOARD_VARIANT */
         PHX_BOARD_DIGITAL             = (int)(etCamConfigLoad.PHX_DIGITAL),
         PHX_BOARD_PHX_D24CL_PE1       = (int)(etCamConfigLoad.PHX_D24CL_PE1),
         PHX_BOARD_PHX_D24CL_PE1_MIR   = (int)(etCamConfigLoad.PHX_D24CL_PE1_MIR),
         PHX_BOARD_PHX_D48CL_PE1       = (int)(etCamConfigLoad.PHX_D48CL_PE1),
         PHX_BOARD_PHX_D48CL_PE4       = (int)(etCamConfigLoad.PHX_D48CL_PE4),
         PHX_BOARD_PHX_D64CL_PE4       = (int)(etCamConfigLoad.PHX_D64CL_PE4),
         PHX_BOARD_PHX_D24CL_PCI32     = (int)(etCamConfigLoad.PHX_D24CL_PCI32),
         PHX_BOARD_PHX_D48CL_PCI32     = (int)(etCamConfigLoad.PHX_D48CL_PCI32),
         PHX_BOARD_PHX_D48CL_PCI64     = (int)(etCamConfigLoad.PHX_D48CL_PCI64),
         PHX_BOARD_PHX_D48CL_PCI64U    = (int)(etCamConfigLoad.PHX_D48CL_PCI64U),
         PHX_BOARD_PHX_D10HDSDI_PE1    = (int)(etCamConfigLoad.PHX_D10HDSDI_PE1),
         PHX_BOARD_PHX_D20HDSDI_PE1    = (int)(etCamConfigLoad.PHX_D20HDSDI_PE1),
         PHX_BOARD_PHX_D10HDSDI_PE4    = (int)(etCamConfigLoad.PHX_D10HDSDI_PE4),
         PHX_BOARD_PHX_D20HDSDI_PE4    = (int)(etCamConfigLoad.PHX_D20HDSDI_PE4),
         PHX_BOARD_PHX_D36_PE1         = (int)(etCamConfigLoad.PHX_D36_PE1),
         PHX_BOARD_PHX_D36_PE4         = (int)(etCamConfigLoad.PHX_D36_PE4),
         PHX_BOARD_PHX_D32_PCI32       = (int)(etCamConfigLoad.PHX_D32_PCI32),
         PHX_BOARD_PHX_D36_PCI32       = (int)(etCamConfigLoad.PHX_D36_PCI32),
         PHX_BOARD_PHX_D36_PCI64       = (int)(etCamConfigLoad.PHX_D36_PCI64),
         PHX_BOARD_PHX_D36_PCI64U      = (int)(etCamConfigLoad.PHX_D36_PCI64U),
         PHX_BOARD_PHX_D24AVDS_PE1     = (int)(etCamConfigLoad.PHX_D24AVDS_PE1),
         PHX_BOARD_FBD_1XCLD_2PE8,
         PHX_BOARD_FBD_4XCXP6_2PE8,
         PHX_BOARD_FBD_2XCLD_2PE8,
         PHX_BOARD_FBD_1XCXP6_2PE8,
         PHX_BOARD_FBD_2XCXP6_2PE8,
         PHX_BOARD_FBD_1XCLM_2PE8,
         PHX_BOARD_FBD_2XCLM_2PE8,
         PHX_BOARD_FBD_1XCLD_TTLA_2PE8,
         PHX_BOARD_FBD_4XCXP6_TTLA_2PE8,
         PHX_BOARD_FBD_1XCLM_TTLA_2PE8,
         PHX_BOARD_FBD_1XCLD_2PE8_MIR,
         PHX_BOARD_FBD_4XCXP6_2PE8_MIR,
         PHX_BOARD_FBD_1XCLM_2PE8_MIR,

         /* PHX_BOARD_NUMBER */
         PHX_BOARD_NUMBER_AUTO   = (int)(etCamConfigLoad.PHX_BOARD_AUTO),
         PHX_BOARD_NUMBER_1      = (int)(etCamConfigLoad.PHX_BOARD1),
         PHX_BOARD_NUMBER_2      = (int)(etCamConfigLoad.PHX_BOARD2),
         PHX_BOARD_NUMBER_3      = (int)(etCamConfigLoad.PHX_BOARD3),
         PHX_BOARD_NUMBER_4      = (int)(etCamConfigLoad.PHX_BOARD4),
         PHX_BOARD_NUMBER_5      = (int)(etCamConfigLoad.PHX_BOARD5),
         PHX_BOARD_NUMBER_6      = (int)(etCamConfigLoad.PHX_BOARD6),
         PHX_BOARD_NUMBER_7      = (int)(etCamConfigLoad.PHX_BOARD7),  /* Add or reduce these, if PHX_BOARD_MAX changes */

         /* PHX_CHANNEL_NUMBER */
         PHX_CHANNEL_NUMBER_AUTO = (int)(etCamConfigLoad.PHX_CHANNEL_AUTO),
         PHX_CHANNEL_NUMBER_1    = (int)(etCamConfigLoad.PHX_CHANNEL_A),
         PHX_CHANNEL_NUMBER_2    = (int)(etCamConfigLoad.PHX_CHANNEL_B),
         PHX_CHANNEL_NUMBER_3,
         PHX_CHANNEL_NUMBER_4,
         PHX_CHANNEL_NUMBER_5,
         PHX_CHANNEL_NUMBER_6,
         PHX_CHANNEL_NUMBER_7,
         PHX_CHANNEL_NUMBER_8,

         /* PHX_CONFIG_MODE */
         PHX_CONFIG_NORMAL       = (int)(etCamConfigLoad.PHX_MODE_NORMAL),
         PHX_CONFIG_COMMS_ONLY   = (int)(etCamConfigLoad.PHX_COMMS_ONLY),
         PHX_CONFIG_ACQ_ONLY     = (int)(etCamConfigLoad.PHX_ACQ_ONLY),
         PHX_CONFIG_FW_ONLY      = (int)(etCamConfigLoad.PHX_COMMS_ONLY | etCamConfigLoad.PHX_ACQ_ONLY),

         /* PHX_CXP_BITRATE_MODE */
         PHX_CXP_BITRATE_MODE_AUTO = (int)(etParam.PHX_CXP_BITRATE_MODE + 1),
         PHX_CXP_BITRATE_MODE_CXP1,
         PHX_CXP_BITRATE_MODE_CXP2,
         PHX_CXP_BITRATE_MODE_CXP3,
         PHX_CXP_BITRATE_MODE_CXP5,
         PHX_CXP_BITRATE_MODE_CXP6,

         /* PHX_CXP_BITRATE */
         PHX_CXP_BITRATE_UNKNOWN = (int)(etParam.PHX_CXP_BITRATE + 1),
         PHX_CXP_BITRATE_CXP1    = PHX_CXP_BITRATE_MODE_CXP1,
         PHX_CXP_BITRATE_CXP2    = PHX_CXP_BITRATE_MODE_CXP2,
         PHX_CXP_BITRATE_CXP3    = PHX_CXP_BITRATE_MODE_CXP3,
         PHX_CXP_BITRATE_CXP5    = PHX_CXP_BITRATE_MODE_CXP5,
         PHX_CXP_BITRATE_CXP6    = PHX_CXP_BITRATE_MODE_CXP6,

         /* PHX_CXP_POCXP_MODE */
         PHX_CXP_POCXP_MODE_AUTO = (int)(etParam.PHX_CXP_POCXP_MODE + 1),
         PHX_CXP_POCXP_MODE_OFF,
         PHX_CXP_POCXP_MODE_TRIP_RESET,
         PHX_CXP_POCXP_MODE_FORCEON,

         /* PHX_CXP_DISCOVERY_MODE */
         PHX_CXP_DISCOVERY_MODE_AUTO = (int)(etParam.PHX_CXP_DISCOVERY_MODE + 1),
         PHX_CXP_DISCOVERY_MODE_1X,
         PHX_CXP_DISCOVERY_MODE_2X,
         PHX_CXP_DISCOVERY_MODE_4X,

         /* PHX_CXP_DISCOVERY */
         PHX_CXP_DISCOVERY_UNKNOWN  = (int)(etParam.PHX_CXP_DISCOVERY + 1),
         PHX_CXP_DISCOVERY_1X       = PHX_CXP_DISCOVERY_MODE_1X,
         PHX_CXP_DISCOVERY_2X       = PHX_CXP_DISCOVERY_MODE_2X,
         PHX_CXP_DISCOVERY_4X       = PHX_CXP_DISCOVERY_MODE_4X,

         /* PHX_DST_ALIGMNENT */
         PHX_DST_LSB_ALIGNED = (int)(etParam.PHX_DST_ALIGNMENT + 1),
         PHX_DST_MSB_ALIGNED,

         /* PHX_INTRPT_CONDITIONS */
         PHX_INTRPT_TEST               = 0x00000001,
         PHX_INTRPT_BUFFER_READY       = 0x00000002,
         PHX_INTRPT_FIFO_OVERFLOW      = 0x00000004,
         PHX_INTRPT_FRAME_LOST         = 0x00000008,
         PHX_INTRPT_CAPTURE_COMPLETE   = 0x00000010,
         PHX_INTRPT_FRAME_START        = 0x00000020,
         PHX_INTRPT_FRAME_END          = 0x00000040,
         PHX_INTRPT_LINE_START         = 0x00000080,
         PHX_INTRPT_LINE_END           = 0x00000100,
         PHX_INTRPT_FGTRIG_START       = 0x00000200,
         PHX_INTRPT_FGTRIG_END         = 0x00000400,
         PHX_INTRPT_TIMEOUT            = 0x00000800,
         PHX_INTRPT_SYNC_LOST          = 0x00001000,
         PHX_INTRPT_TIMERA1            = 0x00002000,
         PHX_INTRPT_GLOBAL_ENABLE      = unchecked((int)0x80000000),

         /* Retained for backwards compatibility with previous software releases.
          * Do not use for new applications.
          */
         /* PHX_CAM_SRC_COL */
         PHX_CAM_SRC_BAY_RGGB = PHX_CAM_SRC_BAYER_RG,
         PHX_CAM_SRC_BAY_GRBG = PHX_CAM_SRC_BAYER_GR,
         PHX_CAM_SRC_BAY_GBRG = PHX_CAM_SRC_BAYER_GB,
         PHX_CAM_SRC_BAY_BGGR = PHX_CAM_SRC_BAYER_BG,
         PHX_CAM_SRC_YUV      = PHX_CAM_SRC_YUV422,      /* For backwards compatibility from 7.3.15 */

         /* PHX_CAM_HTAP_DIR */
         PHX_CAM_HTAP_BOTH    = PHX_CAM_HTAP_CONVERGE,   /* For Backwards compatibility from 2.10.1 */

         /* PHX_CAM_HTAP_TYPE */
         PHX_CAM_HTAP_OFFSET  = PHX_CAM_HTAP_OFFSET_1,   /* Backward Compatibility prior to 4.14 */

         /* PHX_CAM_VTAP_DIR */
         PHX_CAM_VTAP_BOTH    = PHX_CAM_VTAP_CONVERGE,   /* Backward compatibility prior to v6.34 */

         /* PHX_ACQ_TYPE */
         PHX_ACQ_FRAME        = PHX_ACQ_FRAME_12,        /* For Backwards compatibility from 2.8.2 */

         /* PHX_DST_FORMAT */
         PHX_DST_FORMAT_Y8          = PHX_BUS_FORMAT_MONO8,
         PHX_DST_FORMAT_Y16         = PHX_BUS_FORMAT_MONO16,
         PHX_DST_FORMAT_Y32         = PHX_BUS_FORMAT_MONO32,
         PHX_DST_FORMAT_Y36         = PHX_BUS_FORMAT_MONO36,      /* No longer supported in version 2 release */
         PHX_DST_FORMAT_RGB15       = PHX_BUS_FORMAT_BGR5,
         PHX_DST_FORMAT_RGB16       = PHX_BUS_FORMAT_BGR565,
         PHX_DST_XBGR32             = PHX_BUS_FORMAT_XBGR8,       /* PHX_DST_FORMAT_RGBX32, */
         PHX_DST_BGRX32             = PHX_BUS_FORMAT_BGRX8,       /* PHX_DST_FORMAT_XRGB32, */
         PHX_DST_FORMAT_RGB48       = PHX_BUS_FORMAT_BGR16,
         PHX_DST_FORMAT_BGR15       = PHX_BUS_FORMAT_RGB5,
         PHX_DST_FORMAT_BGR16       = PHX_BUS_FORMAT_RGB565,
         PHX_DST_XRGB32             = PHX_BUS_FORMAT_XRGB8,       /* PHX_DST_FORMAT_BGRX32, */
         PHX_DST_RGBX32             = PHX_BUS_FORMAT_RGBX8,       /* PHX_DST_FORMAT_XBGR32, */
         PHX_DST_FORMAT_BGR48       = PHX_BUS_FORMAT_RGB16,
         PHX_DST_FORMAT_RGB32       = PHX_BUS_FORMAT_BGR101210,   /* Added in version 2 release */
         PHX_DST_FORMAT_BGR32       = PHX_BUS_FORMAT_RGB101210,
         PHX_DST_FORMAT_RGB24       = PHX_BUS_FORMAT_BGR8,        /* Added in version 2.25 release */
         PHX_DST_FORMAT_BGR24       = PHX_BUS_FORMAT_RGB8,
         PHX_DST_FORMAT_Y10         = PHX_BUS_FORMAT_MONO10,
         PHX_DST_FORMAT_Y12         = PHX_BUS_FORMAT_MONO12,
         PHX_DST_FORMAT_Y14         = PHX_BUS_FORMAT_MONO14,
         PHX_DST_FORMAT_Y12_PACKED  = PHX_BUS_FORMAT_MONO12P,
         PHX_DST_FORMAT_RGB36       = PHX_BUS_FORMAT_BGR12,
         PHX_DST_FORMAT_BGR36       = PHX_BUS_FORMAT_RGB12,
         PHX_DST_FORMAT_YUV422      = PHX_BUS_FORMAT_YUV422_8,
         PHX_DST_FORMAT_RRGGBB8     = PHX_BUS_FORMAT_RGB8_PLANAR,
         PHX_DST_FORMAT_Y10_PACKED  = PHX_BUS_FORMAT_MONO10P,
         PHX_DST_FORMAT_Y14_PACKED  = PHX_BUS_FORMAT_MONO14P,

         PHX_DST_FORMAT_RGBX32   = PHX_BUS_FORMAT_XBGR8,   /* Backward Compatibility prior to 5.54, New names are consistent with TMG */
         PHX_DST_FORMAT_XRGB32   = PHX_BUS_FORMAT_BGRX8,
         PHX_DST_FORMAT_BGRX32   = PHX_BUS_FORMAT_XRGB8,
         PHX_DST_FORMAT_XBGR32   = PHX_BUS_FORMAT_RGBX8,
         PHX_DST_FORMAT_2Y12     = PHX_BUS_FORMAT_MONO12P,

         /* PHX_USER_FORMAT */
         PHX_USER_FORMAT_Y8         = PHX_USR_FORMAT_MONO8,
         PHX_USER_FORMAT_Y16        = PHX_USR_FORMAT_MONO16,
         PHX_USER_FORMAT_Y32        = PHX_USR_FORMAT_MONO32,
         PHX_USER_FORMAT_Y36        = PHX_USR_FORMAT_MONO36,      /* No longer supported in version 2 release */
         PHX_USER_FORMAT_RGB15      = PHX_USR_FORMAT_BGR5,
         PHX_USER_FORMAT_RGB16      = PHX_USR_FORMAT_BGR565,
         PHX_USER_XBGR32            = PHX_USR_FORMAT_XBGR8,       /* PHX_DST_FORMAT_RGBX32, */
         PHX_USER_BGRX32            = PHX_USR_FORMAT_BGRX8,       /* PHX_DST_FORMAT_XRGB32, */
         PHX_USER_FORMAT_RGB48      = PHX_USR_FORMAT_BGR16,
         PHX_USER_FORMAT_BGR15      = PHX_USR_FORMAT_RGB5,
         PHX_USER_FORMAT_BGR16      = PHX_USR_FORMAT_RGB565,
         PHX_USER_XRGB32            = PHX_USR_FORMAT_XRGB8,       /* PHX_DST_FORMAT_BGRX32, */
         PHX_USER_RGBX32            = PHX_USR_FORMAT_RGBX8,       /* PHX_DST_FORMAT_XBGR32, */
         PHX_USER_FORMAT_BGR48      = PHX_USR_FORMAT_RGB16,
         PHX_USER_FORMAT_RGB32      = PHX_USR_FORMAT_BGR101210,   /* Added in version 2 release */
         PHX_USER_FORMAT_BGR32      = PHX_USR_FORMAT_RGB101210,
         PHX_USER_FORMAT_RGB24      = PHX_USR_FORMAT_BGR8,        /* Added in version 2.25 release */
         PHX_USER_FORMAT_BGR24      = PHX_USR_FORMAT_RGB8,
         PHX_USER_FORMAT_Y10        = PHX_USR_FORMAT_MONO10,
         PHX_USER_FORMAT_Y12        = PHX_USR_FORMAT_MONO12,
         PHX_USER_FORMAT_Y14        = PHX_USR_FORMAT_MONO14,
         PHX_USER_FORMAT_Y12_PACKED = PHX_USR_FORMAT_MONO12P,
         PHX_USER_FORMAT_RGB36      = PHX_USR_FORMAT_BGR12,
         PHX_USER_FORMAT_BGR36      = PHX_USR_FORMAT_RGB12,
         PHX_USER_FORMAT_YUV422     = PHX_USR_FORMAT_YUV422_8,
         PHX_USER_FORMAT_RRGGBB8    = PHX_USR_FORMAT_RGB8_PLANAR,
         PHX_USER_FORMAT_Y10_PACKED = PHX_USR_FORMAT_MONO10P,
         PHX_USER_FORMAT_Y14_PACKED = PHX_USR_FORMAT_MONO14P,

         PHX_USER_FORMAT_RGBX32     = PHX_USR_FORMAT_XBGR8,       /* Backward Compatibility prior to 5.54, New names are consistent with TMG */
         PHX_USER_FORMAT_XRGB32     = PHX_USR_FORMAT_BGRX8,
         PHX_USER_FORMAT_BGRX32     = PHX_USR_FORMAT_XRGB8,
         PHX_USER_FORMAT_XBGR32     = PHX_USR_FORMAT_RGBX8,
         PHX_USER_FORMAT_2Y12       = PHX_USR_FORMAT_MONO12P,

         /* PHX_LINETRIG_SRC */
         PHX_LINETRIG_CTRLIN_1_RISING  = PHX_LINETRIG_AUXIN_1_RISING,      /* Backward Compatibility prior to 2.4.0 */
         PHX_LINETRIG_CTRLIN_1_FALLING = PHX_LINETRIG_AUXIN_1_FALLING,
         PHX_LINETRIG_CTRLIN_3_RISING  = PHX_LINETRIG_AUXIN_2_RISING,
         PHX_LINETRIG_CTRLIN_3_FALLING = PHX_LINETRIG_AUXIN_2_FALLING,

         /* PHX_LINETRIG_TIMER_CTRL */
         PHX_LINETRIG_TIMER_START   = PHX_LINETRIG_TIMER_TIME,       /* Backward Compatibility prior to 3.53.0 */
         PHX_LINETRIG_TIMER_STOP    = PHX_LINETRIG_TIMER_DISABLE,

         /* PHX_CAMTRIG_SRC */
         PHX_EXPTRIG_LINETRIG          = PHX_CAMTRIG_SRC_LINETRIG,         /* Backward compatibility */
         PHX_EXPTRIG_ACQTRIG           = PHX_CAMTRIG_SRC_FGTRIGA_CHX,
         PHX_EXPTRIG_NONE              = PHX_CAMTRIG_SRC_NONE,
         PHX_EXPTRIG_SWTRIG            = PHX_CAMTRIG_SRC_SWTRIG_CHX,
         PHX_EXPTRIG_AUXIN_1_RISING    = PHX_CAMTRIG_SRC_AUXIN_1_RISE,     /* Relative */
         PHX_EXPTRIG_AUXIN_1_FALLING   = PHX_CAMTRIG_SRC_AUXIN_1_FALL,
         PHX_EXPTRIG_AUXIN_2_RISING    = PHX_CAMTRIG_SRC_AUXIN_2_RISE,
         PHX_EXPTRIG_AUXIN_2_FALLING   = PHX_CAMTRIG_SRC_AUXIN_2_FALL,
         PHX_EXPTRIG_TIMER             = PHX_CAMTRIG_SRC_TIMERA1_CHX,
         PHX_EXPTRIG_AUXIN_A1_RISING   = PHX_CAMTRIG_SRC_AUXIN_A1_RISE,    /* Absolute */
         PHX_EXPTRIG_AUXIN_A1_FALLING  = PHX_CAMTRIG_SRC_AUXIN_A1_FALL,
         PHX_EXPTRIG_AUXIN_A2_RISING   = PHX_CAMTRIG_SRC_AUXIN_A2_RISE,
         PHX_EXPTRIG_AUXIN_A2_FALLING  = PHX_CAMTRIG_SRC_AUXIN_A2_FALL,
         PHX_EXPTRIG_AUXIN_B1_RISING   = PHX_CAMTRIG_SRC_AUXIN_B1_RISE,
         PHX_EXPTRIG_AUXIN_B1_FALLING  = PHX_CAMTRIG_SRC_AUXIN_B1_FALL,
         PHX_EXPTRIG_AUXIN_B2_RISING   = PHX_CAMTRIG_SRC_AUXIN_B2_RISE,
         PHX_EXPTRIG_AUXIN_B2_FALLING  = PHX_CAMTRIG_SRC_AUXIN_B2_FALL,
         PHX_EXP_LINETRIG              = PHX_EXPTRIG_LINETRIG,             /* Backward compatibility prior to 3.5.0 */
         PHX_EXP_ACQTRIG               = PHX_EXPTRIG_ACQTRIG,

         /* PHX_EXP_LINESTART */
         PHX_EXP_LINESTART_CTRLIO_2 = PHX_EXP_LINESTART_CCIO_2,   /* Backward Compatibility prior to 2.4.0 */

         /* PHX_FGTRIG_SRC */
         /* Backward compatibility */
         PHX_ACQTRIG_OPTO_A1     = PHX_FGTRIG_SRC_OPTO_A1,        /* Absolute */
         PHX_ACQTRIG_OPTO_A2     = PHX_FGTRIG_SRC_OPTO_A2,
         PHX_ACQTRIG_OPTO_B1     = PHX_FGTRIG_SRC_OPTO_B1,
         PHX_ACQTRIG_OPTO_B2     = PHX_FGTRIG_SRC_OPTO_B2,
         PHX_ACQTRIG_CTRLIN_A1   = PHX_FGTRIG_SRC_CTRLIN_A1,
         PHX_ACQTRIG_CTRLIN_A2   = PHX_FGTRIG_SRC_CTRLIN_A2,
         PHX_ACQTRIG_CTRLIN_A3   = PHX_FGTRIG_SRC_CTRLIN_A3,
         PHX_ACQTRIG_CTRLIN_B1   = PHX_FGTRIG_SRC_CTRLIN_B1,
         PHX_ACQTRIG_CTRLIN_B2   = PHX_FGTRIG_SRC_CTRLIN_B2,
         PHX_ACQTRIG_CTRLIN_B3   = PHX_FGTRIG_SRC_CTRLIN_B3,
         PHX_ACQTRIG_CCIO_A1     = PHX_FGTRIG_SRC_CCIO_A1,
         PHX_ACQTRIG_CCIO_A2     = PHX_FGTRIG_SRC_CCIO_A2,
         PHX_ACQTRIG_CCIO_B1     = PHX_FGTRIG_SRC_CCIO_B1,
         PHX_ACQTRIG_CCIO_B2     = PHX_FGTRIG_SRC_CCIO_B2,
         PHX_ACQTRIG_AUXIN_A1    = PHX_FGTRIG_SRC_AUXIN_A1,
         PHX_ACQTRIG_AUXIN_A2    = PHX_FGTRIG_SRC_AUXIN_A2,
         PHX_ACQTRIG_AUXIN_B1    = PHX_FGTRIG_SRC_AUXIN_B1,
         PHX_ACQTRIG_AUXIN_B2    = PHX_FGTRIG_SRC_AUXIN_B2,
         PHX_ACQTRIG_OPTO_1      = PHX_FGTRIG_SRC_OPTO_1,         /* Relative */
         PHX_ACQTRIG_OPTO_2      = PHX_FGTRIG_SRC_OPTO_2,
         PHX_ACQTRIG_AUXIN_1     = PHX_FGTRIG_SRC_AUXIN_1,
         PHX_ACQTRIG_AUXIN_2     = PHX_FGTRIG_SRC_AUXIN_2,
         PHX_ACQTRIG_CTRLIN_1    = PHX_FGTRIG_SRC_CTRLIN_1,
         PHX_ACQTRIG_CTRLIN_2    = PHX_FGTRIG_SRC_CTRLIN_2,
         PHX_ACQTRIG_CTRLIN_3    = PHX_FGTRIG_SRC_CTRLIN_3,
         PHX_ACQTRIG_CCIO_1      = PHX_FGTRIG_SRC_CCIO_1,
         PHX_ACQTRIG_CCIO_2      = PHX_FGTRIG_SRC_CCIO_2,
         PHX_ACQTRIG_TIMER       = PHX_FGTRIG_SRC_TIMERA1_CHX,
         PHX_ACQTRIG_OPTO1       = PHX_ACQTRIG_OPTO_A1,           /* Backward Compatibility prior to 2.4.0 */
         PHX_ACQTRIG_OPTO2       = PHX_ACQTRIG_OPTO_A2,
         PHX_ACQTRIG_OPTO3       = PHX_ACQTRIG_OPTO_B1,
         PHX_ACQTRIG_OPTO4       = PHX_ACQTRIG_OPTO_B2,
         PHX_ACQTRIG_CTRL1IN_1   = PHX_ACQTRIG_CTRLIN_A1,
         PHX_ACQTRIG_CTRL1IN_2   = PHX_ACQTRIG_CTRLIN_A2,
         PHX_ACQTRIG_CTRL1IN_3   = PHX_ACQTRIG_CTRLIN_A3,
         PHX_ACQTRIG_CTRL2IN_1   = PHX_ACQTRIG_CTRLIN_B1,
         PHX_ACQTRIG_CTRL2IN_2   = PHX_ACQTRIG_CTRLIN_B2,
         PHX_ACQTRIG_CTRL2IN_3   = PHX_ACQTRIG_CTRLIN_B3,
         PHX_ACQTRIG_CTRLIO_1    = PHX_ACQTRIG_CCIO_A1,
         PHX_ACQTRIG_CTRLIO_2    = PHX_ACQTRIG_CCIO_A2,
         PHX_ACQTRIG_CTRLIO_3    = PHX_ACQTRIG_CCIO_B1,
         PHX_ACQTRIG_CTRLIO_4    = PHX_ACQTRIG_CCIO_B2,

         /* PHX_FGTRIG_MODE */
         PHX_ACQTRIG_NONE              = PHX_FGTRIG_FREERUN,            /* Backward compatibility */
         PHX_ACQTRIG_FIRST_POS_EDGE    = PHX_FGTRIG_FIRST_POS_EDGE,
         PHX_ACQTRIG_FIRST_NEG_EDGE    = PHX_FGTRIG_FIRST_NEG_EDGE,
         PHX_ACQTRIG_EACH_POS_EDGE     = PHX_FGTRIG_EACH_POS_EDGE,
         PHX_ACQTRIG_EACH_NEG_EDGE     = PHX_FGTRIG_EACH_NEG_EDGE,
         PHX_ACQTRIG_FIRST_POS_LEVEL   = PHX_FGTRIG_FIRST_POS_LEVEL,
         PHX_ACQTRIG_FIRST_NEG_LEVEL   = PHX_FGTRIG_FIRST_NEG_LEVEL,
         PHX_ACQTRIG_EACH_POS_LEVEL    = PHX_FGTRIG_EACH_POS_LEVEL,
         PHX_ACQTRIG_EACH_NEG_LEVEL    = PHX_FGTRIG_EACH_NEG_LEVEL,
         PHX_ACQTRIG_GATED_POS_LEVEL   = PHX_FGTRIG_GATED_POS_LEVEL,
         PHX_ACQTRIG_GATED_NEG_LEVEL   = PHX_FGTRIG_GATED_NEG_LEVEL,

         /* PHX_FGTRIG_ALIGN */
         PHX_ACQTRIG_ALIGN_NONE     = PHX_FGTRIG_ALIGN_NONE,      /* Backward compatibility */
         PHX_ACQTRIG_ALIGN_TO_CLK   = PHX_FGTRIG_ALIGN_TO_CLK,
         PHX_ACQTRIG_ALIGN_TO_LINE  = PHX_FGTRIG_ALIGN_TO_LINE,
         PHX_ACQTRIG_ALIGN_TO_FRAME = PHX_FGTRIG_ALIGN_TO_FRAME,

         /* PHX_FGTRIG_DELAY_MODE */
         PHX_ACQTRIG_DELAY_NONE  = PHX_FGTRIG_DELAY_NONE,         /* Backward compatibility */
         PHX_ACQTRIG_DELAY_LINE  = PHX_FGTRIG_DELAY_LINE,
         PHX_ACQTRIG_DELAY_TIMER = PHX_FGTRIG_DELAY_TIME,

         /* PHX_EVENTGATE_SRC */
         PHX_EVENTGATE_START = PHX_EVENTGATE_ACQ,     /* Retained for backwards compatibility */

         /* PHX_BOARD_VARIANT */
         PHX_BOARD_FBD = PHX_BOARD_FBD_1XCLD_2PE8,   /* Backwards compatibility, prior to v6.36 */

         /* PHX_INTRPT_CONDITIONS */
         PHX_INTRPT_DMA             = PHX_INTRPT_BUFFER_READY,    /* Retained for backwards compatibility */
         PHX_INTRPT_FIFO_A_OVERFLOW = PHX_INTRPT_FIFO_OVERFLOW,   /* Retained for backwards compatibility */
         PHX_INTRPT_ACQ_TRIG_START  = PHX_INTRPT_FGTRIG_START,    /* Retained for backwards compatibility */
         PHX_INTRPT_ACQ_TRIG_END    = PHX_INTRPT_FGTRIG_END,      /* Retained for backwards compatibility */
         PHX_INTRPT_TIMER           = PHX_INTRPT_TIMERA1,         /* Retained for backwards compatibility */
      };

      /*
      PHX_IO_METHOD(ForBrief)
      */
      public enum etPhxIoMethod
      {
         PHX_EMASK_IO_METHOD                 = 0x3F000000,
         PHX_IO_METHOD_WRITE                 = 0x00000000,
         PHX_IO_METHOD_READ                  = 0x00000000,
         PHX_IO_METHOD_BIT_SET               = 0x01000000,
         PHX_IO_METHOD_BIT_CLR               = 0x02000000,
         PHX_IO_METHOD_BIT_TIMERMX_POS       = 0x04000000,
         PHX_IO_METHOD_BIT_FGTRIGD_POS       = 0x05000000,
         PHX_IO_METHOD_BIT_FIFO_WARN_POS     = 0x06000000,
         PHX_IO_METHOD_BIT_TIMERMX_NEG       = 0x08000000,
         PHX_IO_METHOD_BIT_FGTRIGD_NEG       = 0x09000000,
         PHX_IO_METHOD_BIT_FIFO_WARN_NEG     = 0x0A000000,
         PHX_IO_METHOD_BIT_TIMERM1_POS_CHX   = 0x10000000,
         PHX_IO_METHOD_BIT_TIMERM1_NEG_CHX   = 0x11000000,
         PHX_IO_METHOD_BIT_TIMERM1_POS_CH1   = 0x12000000,
         PHX_IO_METHOD_BIT_TIMERM1_NEG_CH1   = 0x13000000,
         PHX_IO_METHOD_BIT_TIMERM1_POS_CH2   = 0x14000000,
         PHX_IO_METHOD_BIT_TIMERM1_NEG_CH2   = 0x15000000,
         PHX_IO_METHOD_BIT_TIMERM1_POS_CH3   = 0x16000000,
         PHX_IO_METHOD_BIT_TIMERM1_NEG_CH3   = 0x17000000,
         PHX_IO_METHOD_BIT_TIMERM1_POS_CH4   = 0x18000000,
         PHX_IO_METHOD_BIT_TIMERM1_NEG_CH4   = 0x19000000,
         PHX_IO_METHOD_BIT_TIMERM2_POS_CHX   = 0x1A000000,
         PHX_IO_METHOD_BIT_TIMERM2_NEG_CHX   = 0x1B000000,
         PHX_IO_METHOD_BIT_TIMERM2_POS_CH1   = 0x1C000000,
         PHX_IO_METHOD_BIT_TIMERM2_NEG_CH1   = 0x1D000000,
         PHX_IO_METHOD_BIT_TIMERM2_POS_CH2   = 0x1E000000,
         PHX_IO_METHOD_BIT_TIMERM2_NEG_CH2   = 0x1F000000,
         PHX_IO_METHOD_BIT_TIMERM2_POS_CH3   = 0x20000000,
         PHX_IO_METHOD_BIT_TIMERM2_NEG_CH3   = 0x21000000,
         PHX_IO_METHOD_BIT_TIMERM2_POS_CH4   = 0x22000000,
         PHX_IO_METHOD_BIT_TIMERM2_NEG_CH4   = 0x23000000,
         PHX_IO_METHOD_BIT_FGTRIGD_POS_CHX   = 0x24000000,
         PHX_IO_METHOD_BIT_FGTRIGD_NEG_CHX   = 0x25000000,
         PHX_IO_METHOD_BIT_FGTRIGD_POS_CH1   = 0x26000000,
         PHX_IO_METHOD_BIT_FGTRIGD_NEG_CH1   = 0x27000000,
         PHX_IO_METHOD_BIT_FGTRIGD_POS_CH2   = 0x28000000,
         PHX_IO_METHOD_BIT_FGTRIGD_NEG_CH2   = 0x29000000,
         PHX_IO_METHOD_BIT_FGTRIGD_POS_CH3   = 0x2A000000,
         PHX_IO_METHOD_BIT_FGTRIGD_NEG_CH3   = 0x2B000000,
         PHX_IO_METHOD_BIT_FGTRIGD_POS_CH4   = 0x2C000000,
         PHX_IO_METHOD_BIT_FGTRIGD_NEG_CH4   = 0x2D000000,

         /* Retained for backwards compatibility with previous software releases.
          * Do not use for new applications.
          */
         PHX_IO_METHOD_BIT_TIMER_POS   = 0x04000000,     /* Retained for backwards compatibility */
         PHX_IO_METHOD_BIT_HW_POS      = 0x04000000,     /* Retained for backwards compatibility */
         PHX_IO_METHOD_BIT_ACQTRIG_POS = 0x05000000,     /* Retained for backwards compatibility */
         PHX_IO_METHOD_BIT_TIMER_NEG   = 0x08000000,     /* Retained for backwards compatibility */
         PHX_IO_METHOD_BIT_HW_NEG      = 0x08000000,     /* Retained for backwards compatibility */
         PHX_IO_METHOD_BIT_ACQTRIG_NEG = 0x09000000      /* Retained for backwards compatibility */
      };

      /*
      PHX_BOARD_INFO(ForBrief)
      */
      public enum etBoardInfo
      {
         PHX_BOARD_INFO_PCI_3V         = 0x00000001,     /* 3V PCI Interface      */
         PHX_BOARD_INFO_PCI_5V         = 0x00000002,     /* 5V PCI Interface      */
         PHX_BOARD_INFO_PCI_33M        = 0x00000004,     /* 33MHz PCI Interface   */
         PHX_BOARD_INFO_PCI_66M        = 0x00000008,     /* 66MHz PCI Interface   */
         PHX_BOARD_INFO_PCI_32B        = 0x00000010,     /* 32bit PCI Interface   */
         PHX_BOARD_INFO_PCI_64B        = 0x00000020,     /* 64bit PCI Interface   */
         PHX_BOARD_INFO_LVDS           = 0x00000040,     /* LVDS Camera Interface */
         PHX_BOARD_INFO_CL             = 0x00000080,     /* Camera Link Interface */
         PHX_BOARD_INFO_CHAIN_MASTER   = 0x00000100,     /* Master Chaining */
         PHX_BOARD_INFO_CHAIN_SLAVE    = 0x00000200,     /* Slave Chaining  */
         PHX_BOARD_INFO_PCI_EXPRESS    = 0x00000400,     /* PCI Express interface  */
         PHX_BOARD_INFO_CL_BASE        = 0x00000800,     /* Camera Link Base only  */
         PHX_BOARD_INFO_CL_MEDIUM      = 0x00001000,     /* Camera Link Medium     */
         PHX_BOARD_INFO_CL_FULL        = 0x00002000,     /* Camera Link Full       */
         PHX_BOARD_INFO_BOARD_3V       = 0x00010000,     /* Board is 3V compatible */
         PHX_BOARD_INFO_BOARD_5V       = 0x00020000,     /* Board is 5V compatible */
         PHX_BOARD_INFO_BOARD_33M      = 0x00040000,     /* Board is 33MHz compatible */
         PHX_BOARD_INFO_BOARD_66M      = 0x00080000,     /* Board is 66MHz compatible */
         PHX_BOARD_INFO_BOARD_32B      = 0x00100000,     /* Board is 32bit compatible */
         PHX_BOARD_INFO_BOARD_64B      = 0x00200000      /* Board is 64bit compatible */
      };

      /*
       * PHX_PCIE_INFO()
       */
      public enum etPcieInfo
      {
         PHX_CXP_LINKS_MAX             = 4,
         PHX_EMASK_PCIE_INFO_LINK_GEN  = 0x00000003,
         PHX_PCIE_INFO_UNKNOWN         = 0x00000000,     /* Unknown */
         PHX_PCIE_INFO_LINK_GEN1       = 0x00000001,     /* Link status Gen1 */
         PHX_PCIE_INFO_LINK_GEN2       = 0x00000002,     /* Link status Gen2 */
         PHX_PCIE_INFO_LINK_GEN3       = 0x00000003,     /* Link status Gen3 */
         PHX_EMASK_PCIE_INFO_LINK_X    = 0x0000001C,
         PHX_PCIE_INFO_LINK_X1         = 0x00000004,     /* Link status x1 */
         PHX_PCIE_INFO_LINK_X2         = 0x00000008,     /* Link status x2 */
         PHX_PCIE_INFO_LINK_X4         = 0x0000000C,     /* Link status x4 */
         PHX_PCIE_INFO_LINK_X8         = 0x00000010,     /* Link status x8 */
         PHX_PCIE_INFO_LINK_X12        = 0x00000014,     /* Link status x12 */
         PHX_PCIE_INFO_LINK_X16        = 0x00000018,     /* Link status x16 */
         PHX_PCIE_INFO_LINK_X32        = 0x0000001C,     /* Link status x32 */
         PHX_EMASK_PCIE_INFO_FG_GEN    = 0x00000300,
         PHX_PCIE_INFO_FG_GEN1         = 0x00000100,     /* Frame grabber Gen1 */
         PHX_PCIE_INFO_FG_GEN2         = 0x00000200,     /* Frame grabber Gen2 */
         PHX_PCIE_INFO_FG_GEN3         = 0x00000300,     /* Frame grabber Gen3 */
         PHX_EMASK_PCIE_INFO_FG_X      = 0x00001C00,
         PHX_PCIE_INFO_FG_X1           = 0x00000400,     /* Frame grabber x1 */
         PHX_PCIE_INFO_FG_X2           = 0x00000800,     /* Frame grabber x2 */
         PHX_PCIE_INFO_FG_X4           = 0x00000C00,     /* Frame grabber x4 */
         PHX_PCIE_INFO_FG_X8           = 0x00001000,     /* Frame grabber x8 */
         PHX_PCIE_INFO_FG_X12          = 0x00001400,     /* Frame grabber x12 */
         PHX_PCIE_INFO_FG_X16          = 0x00001800,     /* Frame grabber x16 */
         PHX_PCIE_INFO_FG_X32          = 0x00001C00,     /* Frame grabber x32 */
         PHX_EMASK_PCIE_INFO_SLOT_GEN  = 0x00030000,
         PHX_PCIE_INFO_SLOT_GEN1       = 0x00010000,     /* Slot Gen1 */
         PHX_PCIE_INFO_SLOT_GEN2       = 0x00020000,     /* Slot Gen2 */
         PHX_PCIE_INFO_SLOT_GEN3       = 0x00030000,     /* Slot Gen3 */
         PHX_EMASK_PCIE_INFO_SLOT_X    = 0x001C0000,
         PHX_PCIE_INFO_SLOT_X1         = 0x00040000,     /* Slot x1 */
         PHX_PCIE_INFO_SLOT_X2         = 0x00080000,     /* Slot x2 */
         PHX_PCIE_INFO_SLOT_X4         = 0x000C0000,     /* Slot x4 */
         PHX_PCIE_INFO_SLOT_X8         = 0x00100000,     /* Slot x8 */
         PHX_PCIE_INFO_SLOT_X12        = 0x00140000,     /* Slot x12 */
         PHX_PCIE_INFO_SLOT_X16        = 0x00180000,     /* Slot x16 */
         PHX_PCIE_INFO_SLOT_X32        = 0x001C0000,     /* Slot x32 */
      };

      /*
       * PHX_CXP_INFO()
       */
      public enum etCxpInfo
      {
         PHX_CXP_CAMERA_DISCOVERED  = 0x00000001,
         PHX_CL_CAMERA_CONNECTED    = PHX_CXP_CAMERA_DISCOVERED,
         PHX_CXP_CAMERA_IS_POCXP    = 0x00000002,
         PHX_CXP_POCXP_UNAVAILABLE  = 0x00000004,
         PHX_CXP_POCXP_TRIPPED      = 0x00000008,
         PHX_CXP_LINK1_USED         = 0x00000100,
         PHX_CXP_LINK2_USED         = 0x00000200,
         PHX_CXP_LINK3_USED         = 0x00000400,
         PHX_CXP_LINK4_USED         = 0x00000800,
         PHX_CXP_LINK1_MASTER       = 0x00010000,
         PHX_CXP_LINK2_MASTER       = 0x00020000,
         PHX_CXP_LINK3_MASTER       = 0x00040000,
         PHX_CXP_LINK4_MASTER       = 0x00080000,
      };

      /* Acquire Function Definitions
       * ============================
       * These define the operations that can be performed by the 
       * PHX_Acquire() function.
       * They are implemented as enums to make them visible within the debugger.
       */
      public enum etAcq
      {
         PHX_START               = (int)(PHX_ACQUIRE | (1 << 8)),
         PHX_CHECK_AND_WAIT      = (int)(PHX_ACQUIRE | (2 << 8)),
         PHX_CHECK_AND_RETURN    = (int)(PHX_ACQUIRE | (3 << 8)),
         PHX_STOP                = (int)(PHX_ACQUIRE | (4 << 8)),
         PHX_ABORT               = (int)(PHX_ACQUIRE | (5 << 8)),
         PHX_BUFFER_GET          = (int)(PHX_ACQUIRE | (6 << 8)),
         PHX_BUFFER_RELEASE      = (int)(PHX_ACQUIRE | (7 << 8)),
         PHX_BUFFER_ABORT        = (int)(PHX_ACQUIRE | (8 << 8)),
         PHX_EVENT_HANDLER       = (int)(PHX_ACQUIRE | (9 << 8)),
         PHX_START_IMMEDIATELY   = (int)(PHX_ACQUIRE | (10 << 8)),
         PHX_SWTRIG              = (int)(PHX_ACQUIRE | (11 << 8)),
         PHX_UNLOCK              = (int)(PHX_ACQUIRE | (12 << 8)),
         PHX_AUTO_WHITE_BALANCE  = (int)(PHX_ACQUIRE | (13 << 8)),
         PHX_AUTO_RESTART        = (int)(PHX_ACQUIRE | (14 << 8)),
         PHX_BUFFER_GET_MERGED   = (int)(PHX_ACQUIRE | (15 << 8)),
         PHX_USER_LOCK           = (int)(PHX_ACQUIRE | (16 << 8)),
         PHX_USER_UNLOCK         = (int)(PHX_ACQUIRE | (17 << 8)),
         PHX_BUFFER_OBJECT_GET   = (int)(PHX_ACQUIRE | (18 << 8)),

         /* Retained for backwards compatibility with previous software releases.
          * Do not use for new applications.
          */
         PHX_EXPOSE = PHX_SWTRIG
      };

      /* This structure is used to specify the address and context of a block of
       * memory (used to specify user allocated image buffers). It can be used
       * either for virtual or physical addresses.
       */
      public struct stImageBuff
      {
         public IntPtr pvAddress;
         public IntPtr pvContext;
      };

      /* This structure is used to specify the address, size and context of a
       * block of memory (used to specify user locked image buffers).
      */
      public struct stUserBuff
      {
         public IntPtr pvAddress;
         public UInt64 qwSizeBytes;
         public IntPtr pvContext;
      };

      public struct stPhxTimeStamp
      {
         public UInt64 qwTime;
         public UInt64 wqEvent;
      };

      public enum etBufferParam
      {
         PHX_BUFFER_VIRTUAL_ADDR,
         PHX_BUFFER_CONTEXT,
         PHX_BUFFER_SEQUENCE_TAG,
         PHX_BUFFER_TIMESTAMP,
         PHX_BUFFER_NUM_TIMESTAMPS
      };

      /*
       * NOTE
       * If you change the following values you ***MUST*** change the
       * appropriate number of initializers for the global array.
       */
      /* The maximum allowable number of image buffers. */
      public const int PHX_MAX_BUFFS   = 0x00007FFF;
      public const int PHX_BUFFS_MASK  = 0x00007FFF;

      /* Timeout Definitions
       * ===================
       */
      public enum eTimeouts
      {
         PHX_TIMEOUT_IMMEDIATE   = 0,
         PHX_TIMEOUT_INFINITE    = -1,
         PHX_TIMEOUT_PROTOCOL    = -2 /* 999 to be confirmed */
      };

      /* Status Definitions
       * ==================
       */
      public enum etStat
      {
         PHX_OK = 0,
         PHX_ERROR_BAD_HANDLE,
         PHX_ERROR_BAD_PARAM,
         PHX_ERROR_BAD_PARAM_VALUE,
         PHX_ERROR_READ_ONLY_PARAM,
         PHX_ERROR_OPEN_FAILED,
         PHX_ERROR_INCOMPATIBLE,
         PHX_ERROR_HANDSHAKE,
         PHX_ERROR_INTERNAL_ERROR,
         PHX_ERROR_OVERFLOW,
         PHX_ERROR_NOT_IMPLEMENTED,
         PHX_ERROR_HW_PROBLEM,
         PHX_ERROR_NOT_SUPPORTED,
         PHX_ERROR_OUT_OF_RANGE,
         PHX_ERROR_MALLOC_FAILED,
         PHX_ERROR_SYSTEM_CALL_FAILED,
         PHX_ERROR_FILE_OPEN_FAILED,
         PHX_ERROR_FILE_CLOSE_FAILED,
         PHX_ERROR_FILE_INVALID,
         PHX_ERROR_BAD_MEMBER,
         PHX_ERROR_HW_NOT_CONFIGURED,
         PHX_ERROR_INVALID_FLASH_PROPERTIES,
         PHX_ERROR_ACQUISITION_STARTED,
         PHX_ERROR_INVALID_POINTER,
         PHX_ERROR_LIB_INCOMPATIBLE,
         PHX_ERROR_SLAVE_MODE,

         /* Phoenix display library errors */
         PHX_ERROR_DISPLAY_CREATE_FAILED,
         PHX_ERROR_DISPLAY_DESTROY_FAILED,
         PHX_ERROR_DDRAW_INIT_FAILED,
         PHX_ERROR_DISPLAY_BUFF_CREATE_FAILED,
         PHX_ERROR_DISPLAY_BUFF_DESTROY_FAILED,
         PHX_ERROR_DDRAW_OPERATION_FAILED,

         /* Registry errors */
         PHX_ERROR_WIN32_REGISTRY_ERROR,

         PHX_ERROR_PROTOCOL_FAILURE,
         PHX_ERROR_CXP_INVALID_ADDRESS,      /* Protocol errors reported by the camera */
         PHX_ERROR_CXP_INVALID_DATA,
         PHX_ERROR_CXP_INVALID_CONTROL,
         PHX_ERROR_CXP_WRITE_TO_READ_ONLY,
         PHX_ERROR_CXP_READ_FROM_WRITE_ONLY,
         PHX_ERROR_CXP_SIZE_TOO_LARGE,
         PHX_ERROR_CXP_INCORRECT_SIZE,
         PHX_ERROR_CXP_MALFORMED_PACKET,
         PHX_ERROR_CXP_FAILED_CRC,
         PHX_ERROR_CXP_COMMAND_MISMATCH,     /* Protocol errors detected by the driver */
         PHX_ERROR_CXP_WAIT_ACK_DATA,
         PHX_ERROR_CXP_SIZE_MISMATCH,
         PHX_ERROR_CXP_STATUS_MISMATCH,
         PHX_ERROR_CXP_ACK_TIMEOUT,          /* Timeout errors */
         PHX_ERROR_CXP_WAIT_ACK_TIMEOUT,
         PHX_ERROR_CXP_USER_ACK_TIMEOUT,
         PHX_ERROR_CXP_INITIAL_ACK_TIMEOUT,
         PHX_ERROR_CXP_TENTATIVE_ACK_TIMEOUT,
         PHX_ERROR_CXP_RX_PACKET_INVALID,    /* Rx Protocol errors detected by the driver */
         PHX_ERROR_CXP_RX_PACKET_CRC_ERROR,
         PHX_ERROR_CXP_INVALID_READ_ACK,
         PHX_ERROR_CXP_INVALID_WRITE_ACK,
         PHX_ERROR_CXP_INVALID_TENTATIVE_ACK,
         PHX_ERROR_CXP_INVALID_RESET_ACK,
         PHX_ERROR_CXP_INVALID_WAIT_ACK,

         PHX_WARNING_TIMEOUT = 0x8000,
         PHX_WARNING_FLASH_RECONFIG,
         PHX_WARNING_ZBT_RECONFIG,
         PHX_WARNING_NOT_PHX_COM,
         PHX_WARNING_NO_PHX_BOARD_REGISTERED,
         PHX_WARNING_TIMEOUT_EXTENDED,
         PHX_WARNING_FW_PARTIALLY_UPDATED,

         /* Retained for backwards compatibility with previous software releases.
          * Do not use for new applications.
          */
         PHX_ERROR_INVALID_FLASH_PROPERITES  = PHX_ERROR_INVALID_FLASH_PROPERTIES, /* Backwards compatibility */
         PHX_WARNING                         = PHX_WARNING_TIMEOUT                 /* Backwards compatibility */
      };

      /* This structure is used to specify a colour by its individual components.
       */
      public struct tColour
      {
         uint bRed;
         uint bGreen;
         uint bBlue;
      };

      /* This structure holds the details of a logical LUT
       */
      public struct tLutInfo
      {
         uint     dwLut;
         uint     dwColour;
         uint     dwTap;
         uint     dwBrightness;
         uint     dwContrast;
         uint     dwGamma;
         uint     dwFloor;
         uint     dwCeiling;
         IntPtr   pwLutData;
         uint     dwSize;
      };

      /* Default settings for the Luts */
      public enum eLutCtrl
      {
         PHX_LUT_DEFAULT_BRIGHTNESS = 100,
         PHX_LUT_DEFAULT_CONTRAST   = 100,
         PHX_LUT_DEFAULT_GAMMA      = 100,
         PHX_LUT_MAX_LUTS           = 256,  /* Maximum number of LUTs across a line */
         PHX_LUT_MAX_COLS           = 3,    /* Maximum number of LUT components */
         PHX_LUT_MAX_TAPS           = 2     /* Maximum number of camera taps */
      };


      public void _PHX_SleepMs(int x)
      {
         Thread.Sleep(x);
      }


      /* PHX_CameraConfigLoad error handler function */
      [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
      public unsafe delegate void PHX_ErrorHandler(String szFnName, etStat eErrCode, String szDescString);

      /* PHX_Acquire callback function */
      [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
      public unsafe delegate void PHX_AcquireCallBack(uint hCamera, uint dwInterruptMask, IntPtr pvParams);

#if (WIN64)
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_Action                   (uint hCamera, etAction eAction, etActionParam eActionParam, ref String eString);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_Create                   (ref uint hCamera, PHX_ErrorHandler ErrHandler);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_Open                     (uint hCamera);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_Close                    (ref uint hCamera);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_Destroy                  (ref uint hCamera);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_ParameterGet             (uint hCamera, etParam eParam, ref etCxpInfo eParamValue);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_ParameterGet             (uint hCamera, etParam eParam, ref etParamValue eParamValue);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_ParameterGet             (uint hCamera, etParam eParam, ref etBoardInfo eBoardInfo);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_ParameterGet             (uint hCamera, etParam eParam, ref String eString);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_ParameterGet             (uint hCamera, etParam eParam, ref System.IntPtr propertiesPtr);
      [DllImport("phxlx64.dll")] public static extern etStat          PHX_ParameterGet             (uint hCamera, etParam eParam, ref uint dwParamValue);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref etParamValue eParamValue);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref uint dwValue);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref String eString);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref System.IntPtr configFileName);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, Phx.stImageBuff[] asImageBuffs);
      [DllImport("phxlx64.dll")] public unsafe static extern etStat  PHX_ParameterSet              (uint hCamera, etParam eParam, int* nParamValue);
      [DllImport("phxlx64.dll")] public unsafe static extern etStat  PHX_ParameterSet              (uint hCamera, etParam eParam, void* nParamValue);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_StreamRead                (uint hCamera, etAcq eAcq, PHX_AcquireCallBack pvData);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_StreamRead                (uint hCamera, etAcq eAcq, IntPtr pvData);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_StreamRead                (uint hCamera, etAcq eAcq, ref stImageBuff stBuffer);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ControlRead               (uint hCamera, etControlPort eControlPort, ref uint dwAddress, ref uint dwValue, ref uint pdwNum, uint dwMsTimeout);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ControlReset              (uint hCamera, etControlPort eControlPort, ref uint dwAddress, uint dwMsTimeout);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ControlWrite              (uint hCamera, etControlPort eControlPort, ref uint dwAddress, ref uint dwValue, ref uint pdwNum, uint dwMsTimeout);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_BufferParameterGet        (uint hCamera, uint hBuffer, etBufferParam eBufferParam, ref UInt64 qw);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_BufferParameterSet        (uint hCamera, uint hBuffer, etBufferParam eBufferParam, ref UInt64 qw);

      [DllImport("phxlx64.dll")] public static extern void           PHX_ErrHandlerDefault         (String szFnName, etStat eErrCode, String szDescString);
      [DllImport("phxlx64.dll")] public static extern void           PHX_ErrCodeDecode             (String szDescString, etStat eErrCode);
      [DllImport("phxlx64.dll")] public static extern void           PHX_DebugDefaultTraceHandler  (String szDescString);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_MutexCreate               (uint hCamera, ref IntPtr phPhxMutex, String szMutexName);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_MutexDestroy              (uint hCamera, ref IntPtr phPhxMutex);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_MutexAcquire              (uint hCamera, IntPtr hPhxMutex, uint dwMsTimeout);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_MutexRelease              (uint hCamera, IntPtr hPhxMutex);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_SemaphoreCreate           (uint hCamera, ref IntPtr phPhxSem, uint dwInitialCount, uint dwMaximumCount);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_SemaphoreDestroy          (uint hCamera, ref IntPtr phPhxSem);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_SemaphoreSignal           (uint hCamera, IntPtr hPhxMutex, uint dwCount);
      [DllImport("phxlx64.dll")] public static extern etStat         PHX_SemaphoreWaitWithTimeout  (uint hCamera, IntPtr hPhxMutex, uint dwWait);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_ComParameterGet           (uint hCamera, etComParam eComParam, ref uint eComParamValue);

      [DllImport("phxlx64.dll")] public static extern etStat         PHX_MemoryAlloc               (uint hCamera, ref IntPtr Buffer, uint dwSizeInBytes, uint dwAlignment, uint dwFlags);
      [DllImport("phxlx64.dll")] public static extern void           PHX_MemoryFreeAndNull         (uint hCamera, ref IntPtr Buffer);
#else
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_Action                    (uint hCamera, etAction eAction, etActionParam eActionParam, ref String eString);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_Create                    (ref uint hCamera, PHX_ErrorHandler ErrHandler);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_Open                      (uint hCamera);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_Close                     (ref uint hCamera);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_Destroy                   (ref uint hCamera);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterGet              (uint hCamera, etParam eParam, ref etCxpInfo eParamValue);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterGet              (uint hCamera, etParam eParam, ref etParamValue eParamValue);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterGet              (uint hCamera, etParam eParam, ref etBoardInfo eBoardInfo);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterGet              (uint hCamera, etParam eParam, ref String eString);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterGet              (uint hCamera, etParam eParam, ref System.IntPtr propertiesPtr);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterGet              (uint hCamera, etParam eParam, ref uint dwParamValue);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref etParamValue eParamValue);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref uint dwValue);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref String eString);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, ref System.IntPtr configFileName);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ParameterSet              (uint hCamera, etParam eParam, Phx.stImageBuff[] asImageBuffs);
      [DllImport("phxlw32.dll")] public unsafe static extern etStat  PHX_ParameterSet              (uint hCamera, etParam eParam, int* nParamValue);
      [DllImport("phxlw32.dll")] public unsafe static extern etStat  PHX_ParameterSet              (uint hCamera, etParam eParam, void* nParamValue);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_StreamRead                (uint hCamera, etAcq eAcq, PHX_AcquireCallBack pvData);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_StreamRead                (uint hCamera, etAcq eAcq, IntPtr pvData);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_StreamRead                (uint hCamera, etAcq eAcq, ref stImageBuff stBuffer);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ControlRead               (uint hCamera, etControlPort eControlPort, ref uint dwAddress, ref uint dwValue, ref uint pdwNum, uint dwMsTimeout);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ControlReset              (uint hCamera, etControlPort eControlPort, ref uint dwAddress, uint dwMsTimeout);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ControlWrite              (uint hCamera, etControlPort eControlPort, ref uint dwAddress, ref uint dwValue, ref uint pdwNum, uint dwMsTimeout);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_BufferParameterGet        (uint hCamera, uint hBuffer, etBufferParam eBufferParam, ref UInt64 qw);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_BufferParameterSet        (uint hCamera, uint hBuffer, etBufferParam eBufferParam, ref UInt64 qw);

      [DllImport("phxlw32.dll")] public static extern void           PHX_ErrHandlerDefault         (String szFnName, etStat eErrCode, String szDescString);
      [DllImport("phxlw32.dll")] public static extern void           PHX_ErrCodeDecode             (String szDescString, etStat eErrCode);
      [DllImport("phxlw32.dll")] public static extern void           PHX_DebugDefaultTraceHandler  (String szDescString);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_MutexCreate               (uint hCamera, ref IntPtr phPhxMutex, String szMutexName);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_MutexDestroy              (uint hCamera, ref IntPtr phPhxMutex);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_MutexAcquire              (uint hCamera, IntPtr hPhxMutex, uint dwMsTimeout);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_MutexRelease              (uint hCamera, IntPtr hPhxMutex);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_SemaphoreCreate           (uint hCamera, ref IntPtr phPhxSem, uint dwInitialCount, uint dwMaximumCount);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_SemaphoreDestroy          (uint hCamera, ref IntPtr phPhxSem);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_SemaphoreSignal           (uint hCamera, IntPtr hPhxMutex, uint dwCount);
      [DllImport("phxlw32.dll")] public static extern etStat         PHX_SemaphoreWaitWithTimeout  (uint hCamera, IntPtr hPhxMutex, uint dwWait);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_ComParameterGet           (uint hCamera, etComParam eComParam, ref uint eComParamValue);

      [DllImport("phxlw32.dll")] public static extern etStat         PHX_MemoryAlloc               (uint hCamera, ref IntPtr Buffer, uint dwSizeInBytes, uint dwAlignment, uint dwFlags);
      [DllImport("phxlw32.dll")] public static extern void           PHX_MemoryFreeAndNull         (uint hCamera, ref IntPtr Buffer);
#endif

      public int PHX_MAX_IMAGE_BUFFERS = PHX_MAX_BUFFS;    /* Prior to v3.04 */
   }
}