using RadarEye;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test3(@"D:\VSWorkSpace\RadarEye\RadarEye\bin\Debug\data\2018-05-03-15-52-54-1116307");
            Test5();
        }
        public static void Test1() {

            CVO cvo = new CVO();
            String path = @"D:\VSWorkSpace\RadarEye\RadarEye\bin\Debug\data2\2018-05-25-15-31-01-2716069";
            cvo.TrackRedVedio(path + @"\1.avi",
                path + @"\3.avi", path + @"\target.txt");
        }
        public static void Test2() {
            List<String> paths = IOTools.ListPaths(@"D:\VSWorkSpace\RadarEye\RadarEye\bin\Debug\data2");

            foreach (String path in paths) {
                CVO cvo = new CVO();
               
                cvo.TrackRedVedio(path + @"\1.avi",
                    path + @"\3.avi", path + @"\target.txt");
            }
            
        }
        public static void Test3(String path) {
            RecordAnalyzer ra = new RecordAnalyzer();
            List<String> label, data;
            ra.DoAna(path, out label,out data);
            IOTools.WriteListToTxt(label, path + @"\label.txt");
            IOTools.WriteListToTxt(data, path + @"\data.txt");

        }
        public static void Test4() {
            String paPath = @"D:\VSWorkSpace\RadarEye\RadarEye\bin\Debug\data2";
            List<String> paths = IOTools.ListPaths(paPath);
            List<String> labelTotal = new List<string>();
            List<String> dataTotal = new List<string>();
            int i = 0;
            int testnum = 1
                ;
            foreach (String path in paths)
            {
                i++;
                if (i == testnum)
                    continue;
                RecordAnalyzer ra = new RecordAnalyzer();
                List<String> label, data;
                ra.DoAna(path, out label, out data);
               // IOTools.WriteListToTxt(label, path + @"\label.txt");
               // IOTools.WriteListToTxt(data, path + @"\data.txt");
                labelTotal.AddRange(label);
                dataTotal.AddRange(data);
            }
            IOTools.WriteListToTxt(labelTotal, paPath + @"\trainlabel.txt");
            IOTools.WriteListToTxt(dataTotal, paPath + @"\traindata2.txt");

            labelTotal = new List<string>();
            dataTotal = new List<string>();
            i = 0;
            foreach (String path in paths)
            {
                i++;
                if (i != testnum)
                    continue;
                RecordAnalyzer ra = new RecordAnalyzer();
                List<String> label, data;
                ra.DoAna(path, out label, out data);
                // IOTools.WriteListToTxt(label, path + @"\label.txt");
                // IOTools.WriteListToTxt(data, path + @"\data.txt");
                labelTotal.AddRange(label);
                dataTotal.AddRange(data);
            }
            IOTools.WriteListToTxt(labelTotal, paPath + @"\testlabel.txt");
            IOTools.WriteListToTxt(dataTotal, paPath + @"\testdata2.txt");
            Test6();
        }
        public static void Test5() {
            CVO cvo = new CVO();
            String path = @"D:\VSWorkSpace\RadarEye\RadarEye\bin\Debug\data2\2018-05-25-14-59-19-7780559";
            cvo.DrawDLInVedio(path + @"\3.avi",
                path + @"\4.avi", @"D:\TorchTest\a.txt", path+@"\index.txt");
        }
        public static void Test6() {
            RecordAnalyzer ra = new RecordAnalyzer();
            ra.UniformLabelDistribution(@"D:\VSWorkSpace\RadarEye\RadarEye\bin\Debug\data2");
        }
    }
}
