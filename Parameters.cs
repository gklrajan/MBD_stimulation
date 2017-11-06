using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grab
{
    public enum TrialType { Approach, Radial, VeloWalk };

    public class Parameters
    {
        public TrialType trialType;
        public float radSpeed, begRadSize, endRadSize, appSpeed, appSize, appDist, angle, contrast, distoff, cFactor;
        public int tLength, sLength, tInterval;
        public bool veloshut;

        public Parameters(TrialType _trialType, float _angle, float _param1, float _param3, float _param2, float _distoff, float _contrast, int _tLength, int _sLength, int _tInterval, bool _veloshut, float _cFactor)
        {
            trialType = _trialType;
            if (trialType == TrialType.Radial)
            {
                radSpeed = _param1;
                begRadSize = _param2;
                endRadSize = _param3;
            }
            else
            {
                appSpeed = _param2;
                appSize = _param1;
                appDist = _param3;
            }
            angle = _angle;
            contrast = _contrast;
            distoff = _distoff;
            tLength = _tLength;
            sLength = _sLength;
            tInterval = _tInterval;
            veloshut = _veloshut;
            cFactor = _cFactor;
          
        }

        
    }
}
