using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarEye
{
    public class RFO : TagReportListener
    {
        RFIDReader rfidReader;
        bool isRecording = false;
        bool isReading = false;
        List<String> dataList = new List<string>();
        public void tagReport(string epc, int antennaId, int channelIndex, int doppler, int rssiPeak, int phase, long timeStamp, int inventoryParameterSpecID)
        {
          //  Console.WriteLine(epc + " " + phase);
            if (isRecording)
                dataList.Add(epc + " " + antennaId + " " + channelIndex + " " + doppler + " " + rssiPeak + " " + phase + " " + timeStamp);

        }
        public void StartRead()
        {
            rfidReader = new RFIDReader();
            rfidReader.listener = this;
            rfidReader.Initializing();
            rfidReader.Connecting();
            rfidReader.ConfigReader();
            rfidReader.ADDRoSpec();
            rfidReader.EnableRoSpec(1111);
            rfidReader.StartRoSpec(1111);
        }
        public void StopRead()
        {

            rfidReader.CleanUpReaderConfiguration();
            rfidReader.Close();
        }
        public void StartRecord()
        {
            List<String> dataList = new List<string>();
            isRecording = true;
        }
        public void StopRecord()
        {
            isRecording = false;
            IOTools.WriteListToTxt(dataList, Conf.filepath + "RFData.txt");
        }
    }
}
