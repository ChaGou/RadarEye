using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarEye
{
    interface TagReportListener
    {
        void tagReport(String epc, int antennaId, int channelIndex, int doppler, int rssiPeak, int phase, long timeStamp, int inventoryParameterSpecID);
    }
}
