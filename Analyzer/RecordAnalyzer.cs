using RadarEye;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class RecordAnalyzer
    {

        int indexEPC = 0;
        int indexTime = 6;
        int indexAntenna = 1;
        int indexChannel = 2;
        int indexPhase = 5;
        int indexDoppler = 3;
        int indexRSS = 4;
        char splitChar = ' ';
        public List<RFPattern> GetRFPatternS(String rawdatafilename, int targetAntenna)
        {


            List<RFPattern> pl = new List<RFPattern>();
            List<String> tagReports = IOTools.ReadListFromTxt(rawdatafilename);
            long t0 = long.Parse(tagReports[0].Split(splitChar)[indexTime]);

            for (int i = 0; i < tagReports.Count; i++)
            {
                String[] ss = tagReports[i].Split(splitChar);
                String epc = ss[indexEPC];
                if (int.Parse(ss[indexAntenna]) != targetAntenna)
                    continue;
                if (int.Parse(ss[indexChannel]) != 0)
                    continue;
                

                    RFPattern rfp = new RFPattern();
                    rfp.t = (long.Parse(ss[indexTime]) - t0) / Math.Pow(10, 6);
                rfp.phase = int.Parse(ss[indexPhase]);
                    rfp.doppler = int.Parse(ss[indexDoppler]);
                    rfp.rss = int.Parse(ss[indexRSS]);
                    rfp.ID = rfp.GetID(epc);
                    pl.Add(rfp);

            }
            return pl;
        }
        public Dictionary<double, List<RFPattern>> GetPatternsDictionary(List<RFPattern> patterns,double interval) {
            Dictionary<double,List<RFPattern>> dict = new Dictionary<double,List<RFPattern>>();
            foreach (RFPattern pat in patterns) {
                double index = (int)((pat.t + 0.5 * interval) / interval)*interval;
                if (!dict.ContainsKey(index))
                {
                    List<RFPattern> rfl = new List<RFPattern>();
                    dict.Add(index,rfl);
                }
                dict[index].Add(pat);
            }
            return dict;
        }
        public void UniformLabelDistribution(string path) {
            Dictionary<string, List<string>[]> dict = new Dictionary<string, List<string>[]>();
            int interavl = 40;
            List<String> labelList = IOTools.ReadListFromTxt(path + @"\trainlabel.txt");
            List<String> dataList = IOTools.ReadListFromTxt(path + @"\traindata2.txt");
            for (int i = 0; i < labelList.Count; i++) {
                string[] aixs = labelList[i].Split(' ');
                string key = (int)(double.Parse(aixs[0])/interavl) +","+ (int)(double.Parse(aixs[1]) / interavl);
                if (!dict.ContainsKey(key))
                {
                    List<string>[] ldList = new List<string>[2];
                    ldList[0] = new List<string>();
                    ldList[1] = new List<string>();
                    dict.Add(key, ldList);
                }
                dict[key][0].Add(labelList[i]);
                dict[key][1].Add(dataList[i]);
            }
            int maxcount = 0;
            foreach (KeyValuePair<string, List<string>[]> entry in dict) {
                if (maxcount < entry.Value[0].Count)
                    maxcount = entry.Value[0].Count;
            }
            List<String> outputlabelList = new List<string>();
            List<String> outputdataList = new List<string>();
            Random r = new Random();
            foreach (KeyValuePair<string, List<string>[]> entry in dict)
            {
                outputlabelList.AddRange(entry.Value[0]);
                outputdataList.AddRange(entry.Value[1]);
                for (int i = 0; i < maxcount - entry.Value[0].Count; i++) {
                    int index = r.Next(entry.Value[0].Count);
                    outputlabelList.Add(entry.Value[0][index]);
                    outputdataList.Add(entry.Value[1][index]);
                }
            }
            IOTools.WriteListToTxt(outputdataList,path+@"\traindataUni.txt");
            IOTools.WriteListToTxt(outputlabelList, path + @"\trainlabelUni.txt");


        }
        public RFPattern[] GetRFPatternVector(List<RFPattern> rpl, double time) {
            RFPattern[] patterns = new RFPattern[Conf.antennaNum];
            double[] dis = new double[Conf.antennaNum];
            for (int i = 0; i < Conf.antennaNum; i++)
            {
                dis[i] = 10000;
                patterns[i] = new RFPattern();
                patterns[i].phase = Conf.invalidphase;
                patterns[i].rss = Conf.invalidrss;
            }
            foreach (RFPattern rp in rpl)
            {
                double d = Math.Abs(rp.t - time);
                int index = rp.ID - 101;
                if (dis[index] > d)
                {
                    dis[index] = d;
                    patterns[index] = rp;
                }
            }

            return patterns;
        }
        public int ValidNUm(RFPattern[] vec)
        {
            int num = 0;
            foreach (RFPattern pattern in vec)
            {
                if (pattern.phase != Conf.invalidphase)
                    num++;
            }
            // Console.WriteLine(num);
            return num;
        }
        public double[] GetPhaseVector(List<RFPattern> rpl,double time) {
            double[] phases = new double[Conf.antennaNum];
            double[] dis = new double[Conf.antennaNum];
            for (int i = 0; i < Conf.antennaNum;i++) {
                dis[i] = 10000;
                phases[i] = Conf.invalidphase;
            }
            foreach (RFPattern rp in rpl) {
                double d = Math.Abs(rp.t - time);
                int index = rp.ID - 101;
                if (dis[index] > d) {
                    dis[index] = d;
                    phases[index] = rp.phase;
                }
            }
                
            return phases;
        }
        public int ValidNUm(double[] vec) {
            int num = 0;
            foreach (double phase in vec) {
                if (phase != Conf.invalidphase)
                    num++;
            }
           // Console.WriteLine(num);
            return num;
        }
        public String VecToString(double[] vec)
        {
            String s = "";
            for(int i = 0; i < vec.Length -1;i++)
            {
                s += vec[i];
                s += " ";
            }
            s += vec[vec.Length - 1];
            return s;
        }
        public String VecToString(RFPattern[] vec)
        {
            String s = "";
            for (int i = 0; i < vec.Length; i++)
            {
                s += Math.Cos(vec[i].phase/4096.0*Math.PI*4);
                s += " ";
             //   s += vec[i].rss/-10000.0;
             //   s += " ";
            }

            for (int i = 0; i < vec.Length - 1; i++)
            {
                   s += vec[i].rss/-10000.0;
                   s += " ";
            }
             s += " ";
             s += vec[vec.Length - 1].rss/-10000.0;
            return s;
        }
        public void DoAna(String path,out List<String> label,out List<String> data) {
            label = new List<string>();
            data = new List<string>();
            List<String> indexList = new List<string>();
            List<String> targetList = IOTools.ReadListFromTxt(path+@"\target.txt");
            List<RFPattern> rfl = GetRFPatternS(path+@"\RFData.txt",1);
            Console.WriteLine(rfl[rfl.Count-1].t-rfl[0].t);
            Dictionary<double, List<RFPattern>> RFdict = GetPatternsDictionary(rfl, Conf.interval);
            for (int i = 0; i < targetList.Count; i++) {
                String[] ss = targetList[i].Split(' ');
                int k = int.Parse(ss[0]);
                if (k % (int)(Conf.interval * Conf.fps) == 0) {
                    if (double.Parse(ss[1]) != 10000 && double.Parse(ss[2]) != 10000) {
                        double t = k * 1.0 / Conf.fps;
                        if (!RFdict.ContainsKey(t)) {
                            continue;
                        }
                        RFPattern[] vec = GetRFPatternVector(RFdict[t], t);
                        if (ValidNUm(vec) >= Conf.antennaNum * 1)
                        {
                            label.Add(ss[1] + " " + ss[2]);
                            data.Add(VecToString(vec));
                            indexList.Add("" + k);
                        }
                        /* double[] vec = GetPhaseVector(RFdict[t],t);
                         if (ValidNUm(vec) >= Conf.antennaNum * 1) {
                             label.Add(ss[1]+" "+ss[2]);
                             data.Add(VecToString(vec));
                             indexList.Add(""+k);
                         }*/
                    }
                }
            }
            IOTools.WriteListToTxt(indexList,path+@"\index.txt");

        }

        
    }
}
