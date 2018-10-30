using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarEye
{
    public class IOTools
    {
        public static void WriteListToTxt(List<String> ls, String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                foreach (String s in ls)
                {
                    sw.WriteLine(s);

                }
                sw.Close();
            }
        }
        public static void WriteListToTxt(List<double> ls, String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                foreach (double s in ls)
                {
                    sw.WriteLine(s);

                }
                sw.Close();
            }
        }
        public static void WriteDoubleArrayToTxt(double[,] array, String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    String s = "";
                    for (int j = 0; j < array.GetLength(1); j++)
                        s += array[i, j] + " ";
                    sw.WriteLine(s);
                }



                sw.Close();
            }
        }
        public static void WriteByteArrayToTxt(byte[,] array, String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    String s = "";
                    for (int j = 0; j < array.GetLength(1); j++)
                        s += array[i, j] + " ";
                    sw.WriteLine(s);
                }



                sw.Close();
            }
        }
        public static void WriteStringToTxt(String s, String filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {

                sw.WriteLine(s);


                sw.Close();
            }
        }
        public static List<String> ReadListFromTxt(String filename)
        {
            List<String> ls = new List<string>();
            using (StreamReader sr = new StreamReader(filename))
            {
                String s;
                while ((s = sr.ReadLine()) != null)
                {
                    ls.Add(s);
                }
                sr.Close();
            }
            return ls;
        }
        public static void ListFiles(String path)
        {

            FileSystemInfo info = new DirectoryInfo(path);
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录 
            if (dir == null) return;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件 
                if (file != null)
                    Console.WriteLine(file.FullName + " " + file.Name);
                DirectoryInfo dd = files[i] as DirectoryInfo;
                if (dd != null)
                {

                    Console.WriteLine(dd.FullName + " " + dd.Name);
                    // ListFiles(dd.FullName);
                }

            }
        }
        public static List<String> ListPaths(String path)
        {
            List<String> folderList = new List<string>();

            FileSystemInfo info = new DirectoryInfo(path);
            if (!info.Exists) return null;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录 
            if (dir == null) return null;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件 
                if (file != null)
                    Console.WriteLine(file.FullName + " " + file.Name);
                DirectoryInfo dd = files[i] as DirectoryInfo;
                if (dd != null)
                {

                    Console.WriteLine(dd.FullName + " " + dd.Name);
                    folderList.Add(dd.FullName);
                    // ListFiles(dd.FullName);
                }

            }
            return folderList;
        }
        public static void ListFiles(String path, List<String> fileList)
        {

            FileSystemInfo info = new DirectoryInfo(path);
            if (!info.Exists) return;

            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录 
            if (dir == null) return;

            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件 
                if (file != null)
                {
                    fileList.Add(file.FullName);
                    Console.WriteLine(file.FullName + " " + file.Name);
                }

                DirectoryInfo dd = files[i] as DirectoryInfo;
                if (dd != null) ListFiles(dd.FullName, fileList);

            }
        }
    }
}
