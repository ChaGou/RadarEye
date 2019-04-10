using RadarEye;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class RecordAnalyzerPad
    {

        int indexTime = 0;
        int indexAntenna = 2;
        int indexEPC = 3;
        int indexPhase = 1;
        char splitChar = ' ';
        public List<RFPatternPad> GetRFPatternS(String rawdatafilename,double t0)
        {


            List<RFPatternPad> pl = new List<RFPatternPad>();
            List<String> tagReports = IOTools.ReadListFromTxt(rawdatafilename);
            if (t0 == -1)
            {
                String[] ss = tagReports[0].Split(splitChar);
                t0 = GetSecondFromString(ss[indexTime]);
            }

            for (int i = 0; i < tagReports.Count; i++)
            {
                String[] ss = tagReports[i].Split(splitChar);
                


                RFPatternPad rfp = new RFPatternPad();
                rfp.t = (GetSecondFromString(ss[indexTime]) - t0);//chacha
                if (rfp.t < 0)
                    continue;
                rfp.phase = double.Parse(ss[indexPhase]);
                rfp.ant = int.Parse(ss[indexAntenna]);
                rfp.ID = -1;
                if(ss.Length > 4)
                    rfp.ID = int.Parse(ss[indexEPC]);
                pl.Add(rfp);

            }
            return pl;
        }
        public Dictionary<int, List<RFPatternPad>> GetPatternsDictionary(List<RFPatternPad> patterns, double interval,int epc)
        {
            Dictionary<int, List<RFPatternPad>> dict = new Dictionary<int, List<RFPatternPad>>();
            foreach (RFPatternPad pat in patterns)
            {
                if (pat.ID != epc)
                    continue;
                int index = (int)((pat.t + 0.5 * interval) / interval);
                if (!dict.ContainsKey(index))
                {
                    List<RFPatternPad> rfl = new List<RFPatternPad>();
                    dict.Add(index, rfl);
                }
                dict[index].Add(pat);
            }
            return dict;
        }
        public void UniformLabelDistribution(string path)
        {
            Dictionary<string, List<string>[]> dict = new Dictionary<string, List<string>[]>();
            int interavl = 10;
            List<String> labelList = IOTools.ReadListFromTxt(path + @"\trainlabel.txt");
            List<String> dataList = IOTools.ReadListFromTxt(path + @"\refine_data.txt");
            for (int i = 0; i < labelList.Count; i++)
            {
                string[] aixs = labelList[i].Split(' ');
                string key = (int)(double.Parse(aixs[0]) / interavl) + "," + (int)(double.Parse(aixs[1]) / interavl);
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
            foreach (KeyValuePair<string, List<string>[]> entry in dict)
            {
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
                for (int i = 0; i < maxcount - entry.Value[0].Count; i++)
                {
                    int index = r.Next(entry.Value[0].Count);
                    outputlabelList.Add(entry.Value[0][index]);
                    outputdataList.Add(entry.Value[1][index]);
                }
            }
            IOTools.WriteListToTxt(outputdataList, path + @"\traindataUni.txt");
            IOTools.WriteListToTxt(outputlabelList, path + @"\trainlabelUni.txt");


        }
        public RFPatternPad[] GetRFPatternVector(List<RFPatternPad> rpl, double time)
        {
            RFPatternPad[] patterns = new RFPatternPad[64];
            double[] dis = new double[64];
            for (int i = 0; i < 64; i++)
            {
                dis[i] = 10000;
                patterns[i] = new RFPatternPad();
                patterns[i].phase = Conf.invalidphase;
            }
            foreach (RFPatternPad rp in rpl)
            {
                double d = Math.Abs(rp.t - time);
                int index = rp.ant;
                if (dis[index] > d)
                {
                    dis[index] = d;
                    patterns[index] = rp;
                }
            }

            return patterns;
        }
        public int ValidNUm(RFPatternPad[] vec)
        {
            int num = 0;
            foreach (RFPatternPad pattern in vec)
            {
                if (pattern.phase != Conf.invalidphase)
                    num++;
            }
            // Console.WriteLine(num);
            return num;
        }
        public int ValidNUm(double[] vec)
        {
            int num = 0;
            foreach (double phase in vec)
            {
                if (phase != Conf.invalidphase)
                    num++;
            }
            // Console.WriteLine(num);
            return num;
        }
        public String VecToString(double[] vec)
        {
            String s = "";
            for (int i = 0; i < vec.Length - 1; i++)
            {
                s += vec[i];
                s += " ";
            }
            s += vec[vec.Length - 1];
            return s;
        }
        public String VecToString(RFPatternPad[] vec)
        {
            String s = "";
            for (int i = 0; i < vec.Length-1; i++)
            {
                s += vec[i].phase;
                s += " ";
                //   s += vec[i].rss/-10000.0;
                //   s += " ";
            }
            s += vec[vec.Length - 1].phase;

            return s;
        }
        public double GetSecondFromString(String s) {
            double t = 0;
            String[] ss = s.Split('.');
           // int year = int.Parse(ss[0].Substring(0, 4));
           // int month = int.Parse(ss[0].Substring(4, 2));
            int day = int.Parse(ss[0].Substring(6, 2));
            int hour = int.Parse(ss[0].Substring(8, 2));
            int minute = int.Parse(ss[0].Substring(10, 2));
            int second = int.Parse(ss[0].Substring(12, 2));
            second = ((day * 24 + hour) * 60 + minute) * 60 + second;
            
            t = double.Parse(second + "." + ss[1]);

            return t;
        }
        public double GetSecondFromPath(String path) {
            String[] ss = path.Split('\\');
            String time = ss[ss.Length - 1];
            ss = time.Split('-');
            String s = "";
            for (int i = 0; i < ss.Length - 1; i++)
                s += ss[i];
            s += '.';
            s += ss[ss.Length - 1];
            return GetSecondFromString(s);
        }
        public void DoAna(String path, out List<String> label, out List<String> data,out List<String> recordTime) 
        {
            label = new List<string>();
            data = new List<string>();
            List<String> indexList = new List<string>();
            recordTime = new List<string>();
            List<String> targetList = IOTools.ReadListFromTxt(path + @"\target.txt");
            double t0 = GetSecondFromPath(path);
            List<RFPatternPad> rfl = GetRFPatternS(path + @"\usrp_data\RFData.txt", t0);
            List<String> timeList = IOTools.ReadListFromTxt(path+@"\t.txt");
            Dictionary<int, List<RFPatternPad>> RFdict = GetPatternsDictionary(rfl, Conf.interval,-1);
            List<String> DictKeys = new List<string>();
            foreach (double key in RFdict.Keys) {
                DictKeys.Add(key + "");
            }
            IOTools.WriteListToTxt(DictKeys, path + @"\dictkeys.txt");
            for (int i = 0; i < targetList.Count; i++)
            {
                String[] ss = targetList[i].Split(' ');
                int targetFrame = int.Parse(ss[0]);
                int k = (int)((GetSecondFromPath(timeList[targetFrame]) - t0)*Conf.fps+0.5) ;
                if (k % (int)(Conf.interval * Conf.fps) == 0)
                {
                    if (double.Parse(ss[1]) != 10000 && double.Parse(ss[2]) != 10000)
                    {
                        int t = k;
                        if (!RFdict.ContainsKey(t))
                        {
                            continue;
                        }
                        RFPatternPad[] vec = GetRFPatternVector(RFdict[t], t*0.1);
                        Console.WriteLine(ValidNUm(vec));
                        if (ValidNUm(vec) >= 64 * 0.95)
                        {
                            label.Add(ss[1] + " " + ss[2]);
                            data.Add(VecToString(vec));
                            indexList.Add("" + targetFrame);
                            recordTime.Add(""+ (GetSecondFromPath(timeList[targetFrame]) - t0));
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
            IOTools.WriteListToTxt(indexList, path + @"\index.txt");
            
        }
        public void DoAnaRunning(String path, out List<String> data,int epc)
        {
            data = new List<string>();
            double t0 = GetSecondFromPath(path);
            List<RFPatternPad> rfl = GetRFPatternS(path + @"\usrp_data\RFData.txt", t0);
            List<String> indexList = new List<string>();
            Dictionary<int, List<RFPatternPad>> RFdict = GetPatternsDictionary(rfl, Conf.interval,epc);
            List<String> DictKeys = new List<string>();
            foreach (double key in RFdict.Keys)
            {
                DictKeys.Add(key + "");
            }
            IOTools.WriteListToTxt(DictKeys, path + @"\dictkeys.txt");
            List<String> timeList = IOTools.ReadListFromTxt(path + @"\t.txt");
            for (int i = 0; i < timeList.Count; i++) {
                int k = (int)((GetSecondFromPath(timeList[i]) - t0) * Conf.fps + 0.5);
                if (k % (int)(Conf.interval * Conf.fps) == 0)
                {
                    
                        int t = k;
                        if (!RFdict.ContainsKey(t))
                        {
                            continue;
                        }
                        RFPatternPad[] vec = GetRFPatternVector(RFdict[t], t*0.1);
                        Console.WriteLine(ValidNUm(vec));
                        if (ValidNUm(vec) >= 64 * 0.4)//chachacha
                        {
                            data.Add(VecToString(vec));
                            indexList.Add("" + i);

                        }
                        /* double[] vec = GetPhaseVector(RFdict[t],t);
                         if (ValidNUm(vec) >= Conf.antennaNum * 1) {
                             label.Add(ss[1]+" "+ss[2]);
                             data.Add(VecToString(vec));
                             indexList.Add(""+k);
                         }*/
                    
                }

            }
            if(epc == -1)
            IOTools.WriteListToTxt(indexList, path + @"\index.txt");
            else
                IOTools.WriteListToTxt(indexList, path + @"\index"+epc+".txt");



        }


    }
}
