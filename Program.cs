using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ActiveSilicon;

namespace Grab
{
       static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
          unsafe static void Main(string[] args)
        {
            PhxCommon myPhxCommon = new PhxCommon();
            PhxCommon.tCxpRegisters sCameraRegs = new PhxCommon.tCxpRegisters();
            PhxCommon.tPhxCmd sPhxCmd = new PhxCommon.tPhxCmd();

            myPhxCommon.PhxCommonParseCmd(args, ref sPhxCmd);
            myPhxCommon.PhxCommonParseCxpRegs(sPhxCmd.strConfigFileName, ref sCameraRegs);


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GrabAndStimAS_PHX_2016(sPhxCmd.eBoardNumber, sPhxCmd.eChannelNumber, sPhxCmd.strConfigFileName, sCameraRegs));

        }
    }
}