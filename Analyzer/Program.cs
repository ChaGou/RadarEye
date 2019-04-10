using OpenCvSharp.CPlusPlus;
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
        public static List<Scalar> colorList = new List<Scalar>();
       
        static void Main(string[] args)
        {
            //  Test1();
            //
            // Test3(@"E:\Data2\2018-11-09-19-19-33-1266271");
            // Test5();
            // TestVedioTarget();
            //Test6();
           //TestTrainDataLabel(@"E:\Data21");
            colorList.Add(new Scalar(255, 255, 0));
            colorList.Add(new Scalar(255, 0, 255));
            colorList.Add(new Scalar(0, 255, 255));
            colorList.Add(new Scalar(0, 0, 255));
            colorList.Add(new Scalar(0, 255, 0));
            colorList.Add(new Scalar(255, 0, 0));
            List<String> cm = new List<string>();
            //cm.Add(@"E:\Data3");
            //cm.Add(@"E:\Data4");
            //cm.Add(@"E:\Data5");
            //cm.Add(@"E:\Data6");
            //cm.Add(@"E:\Data7");
            cm.Add(@"E:\Data8");
            cm.Add(@"E:\Data12");
            // CombineDataSet(cm, @"E:\DataTest");
            // Test5();
            // DrawResult(@"E:\Data21");
            CVO cvo = new CVO();
            cvo.DrawParticlesInVedio(@"E:\Data21\2019-01-15-20-39-07-3845317\1.avi", @"E:\Data21\2019-01-15-20-39-07-3845317\5.avi", @"C:\torcheye\TorchEye-master\knn_pf_particles.txt", @"C:\torcheye\TorchEye-master\knn_pf_predictions.txt");
            List<int> epcList = new List<int>();
            epcList.Add(4);
            epcList.Add(6);
            epcList.Add(3);
            epcList.Add(1);
           // TestRunningData(@"E:\Data20", epcList);
           // DrawMultipleResult(@"E:\Data20", epcList);
           // Console.ReadKey();
        }
        public static void Test1() {

            CVO cvo = new CVO();
            String path = @"E:\Data3\2018-12-06-14-35-06-5624464";
            cvo.TrackRedVedio(path + @"\1.avi",
                path + @"\3.avi", path + @"\target.txt");
        }
        public static void TestVedioTarget() {
            List<String> paths = IOTools.ListPaths(@"E:\Data21");

            foreach (String path in paths) {
                CVO cvo = new CVO();
               
                cvo.TrackRedVedio(path + @"\1.avi",
                    path + @"\3.avi", path + @"\target.txt");
            }
            
        }
        public static void Test3(String path) {
            RecordAnalyzerPad ra = new RecordAnalyzerPad();
            List<String> label, data,recordTIme;
            ra.DoAna(path, out label,out data, out recordTIme);
            IOTools.WriteListToTxt(label, path + @"\label.txt");
            IOTools.WriteListToTxt(data, path + @"\data.txt");

        }
        public static void TestTrainDataLabel(String paPath) {
             
            List<String> paths = IOTools.ListPaths(paPath);
            List<String> labelTotal = new List<string>();
            List<String> dataTotal = new List<string>();
            List<String> timeTotal = new List<string>();
            int i = 0;
            int testnum = 1
                ;
            foreach (String path in paths)
            {
                i++;
                //if (i == testnum)
                  //  continue;
                RecordAnalyzerPad ra = new RecordAnalyzerPad();
                List<String> label, data, recordTIme;
                ra.DoAna(path, out label, out data, out recordTIme);
                // IOTools.WriteListToTxt(label, path + @"\label.txt");
                // IOTools.WriteListToTxt(data, path + @"\data.txt");
                labelTotal.AddRange(label);
                dataTotal.AddRange(data);
                timeTotal.AddRange(recordTIme);            }
            IOTools.WriteListToTxt(labelTotal, paPath + @"\trainlabel.txt");
            IOTools.WriteListToTxt(dataTotal, paPath + @"\traindata.txt");
            IOTools.WriteListToTxt(timeTotal, paPath + @"\recordTime.txt");



        }
        public static void TestRunningData(String paPath,int epc)
        {
             
            List<String> paths = IOTools.ListPaths(paPath);
            List<String> dataTotal = new List<string>();
            int i = 0;
            int testnum = 1
                ;
            foreach (String path in paths)
            {
                i++;
                //if (i == testnum)
                //  continue;
                RecordAnalyzerPad ra = new RecordAnalyzerPad();
                List<String> label, data, recordTIme;
                ra.DoAnaRunning(path, out data, epc);
                // IOTools.WriteListToTxt(label, path + @"\label.txt");
                // IOTools.WriteListToTxt(data, path + @"\data.txt");
                dataTotal.AddRange(data);
            }
            List<String> labelTotal = new List<string>();
            for (int j = 0; j < dataTotal.Count; j++) {
                labelTotal.Add("1 1");
            }
            if (epc != -1)
            {
                IOTools.WriteListToTxt(dataTotal, paPath + @"\traindata"+epc+".txt");
                IOTools.WriteListToTxt(labelTotal, paPath + @"\trainlabel"+epc+".txt");
            }
            else
            {
                IOTools.WriteListToTxt(dataTotal, paPath + @"\traindata.txt");
                IOTools.WriteListToTxt(labelTotal, paPath + @"\trainlabel.txt");
            }
            



        }
        public static void TestRunningData(String paPath, List<int> epcList) {
            foreach (int epc in epcList)
            {
                TestRunningData(paPath, epc);
            }
            
        }
        public static void CombineDataSet(List<String> inPath, String outputPath)
        {
            List<String> labelTotal = new List<string>();
            List<String> dataTotal = new List<string>();
            List<String> timeTotal = new List<string>();
            foreach (String path in inPath)
            {
                labelTotal.AddRange(IOTools.ReadListFromTxt(path + @"\trainlabel.txt"));
                dataTotal.AddRange(IOTools.ReadListFromTxt(path + @"\traindata.txt"));
                timeTotal.AddRange(IOTools.ReadListFromTxt(path + @"\recordTime.txt"));
            }
            IOTools.WriteListToTxt(labelTotal, outputPath + @"\trainlabel.txt");
            IOTools.WriteListToTxt(dataTotal, outputPath + @"\traindata.txt");
            IOTools.WriteListToTxt(timeTotal, outputPath + @"\recordTime.txt");
        }
        public static void Test5() {
            CVO cvo = new CVO();
            String path = @"E:\Data7\2018-12-07-18-43-40-6917478";
            cvo.DrawDLInVedio(path + @"\3.avi",
                path + @"\4.avi", @"C:\torcheye\TorchEye-master\a.txt", path+@"\index.txt",0,colorList[0]);
        }
        public static void DrawResult(String paPath) {
            List<String> paths = IOTools.ListPaths(paPath);
            List<String> labelTotal = new List<string>();
            List<String> dataTotal = new List<string>();
            List<String> timeTotal = new List<string>();
            int fileoffset = 0;
            CVO cvo = new CVO();
            foreach (String path in paths)
            {
                List<String> indexList = IOTools.ReadListFromTxt(path + @"\index.txt");
                cvo.DrawDLInVedio(path + @"\1.avi",
                path + @"\4.avi", @"C:\torcheye\TorchEye-master\a.txt", path + @"\index.txt", fileoffset,colorList[0]);
                fileoffset += indexList.Count;

                //if (i == testnum)
                //  continue;

            }
        }
        public static void DrawMultipleResult(String paPath,List<int> epcList) {
            for (int i = 0; i < epcList.Count; i++) {
                int epc = epcList[i];
                List<String> paths = IOTools.ListPaths(paPath);
                List<String> labelTotal = new List<string>();
                List<String> dataTotal = new List<string>();
                List<String> timeTotal = new List<string>();
                int fileoffset = 0;
                CVO cvo = new CVO();
                foreach (String path in paths)
                {
                    List<String> indexList = IOTools.ReadListFromTxt(path + @"\index" + epc + ".txt");
                    if(i == 0)
                    cvo.DrawDLInVedio(path + @"\1.avi",
                    path + @"\epc"+ epcList[i]+".avi", paPath + @"\modelResult" + epc + ".txt", path + @"\index" + epc + ".txt", fileoffset, colorList[0]);
                    else
                        cvo.DrawDLInVedio(path + @"\epc" + epcList[i-1] + ".avi",
                    path + @"\epc" + epcList[i] + ".avi", paPath + @"\modelResult" + epc + ".txt", path + @"\index" + epc + ".txt", fileoffset, colorList[i]);

                    fileoffset += indexList.Count;

                    //if (i == testnum)
                    //  continue;

                }
            }
            
        }
        public static void Test6() {
            RecordAnalyzerPad ra = new RecordAnalyzerPad();
            ra.UniformLabelDistribution(@"E:\DataTest");
        }
        
    }
}
