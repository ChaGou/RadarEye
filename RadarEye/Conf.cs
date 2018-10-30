using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarEye
{
    public class Conf
    {
        public static String filepath = @"1\";
        public static String datapath = @"C:\ALIBABA\Data\wall\";
        public static String epc = "801";
        public static int antenna = 2;
        public static double r1 = 0.11, r2 = 0.08;
        public static int fps = 10;
        public static double circleRUp =20;
        public static int antennaNum = 60;
        public static double interval = 0.2;//s

        public static int invalidphase = 2048;
        public static int invalidrss = -10000;
    }
}
