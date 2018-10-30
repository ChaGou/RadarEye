using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class RFPattern
    {
        public int ID; //EPC->ID
        public double t;  //s
        public double doppler;
        public int phase;
        public double rss;
        public int GetID(String Epc) {
            try
            {
                return int.Parse(Epc.Substring(Epc.Length - 3, 3));
            }
            catch (Exception e) {
            }
            return -1; 
        }
    }
}
