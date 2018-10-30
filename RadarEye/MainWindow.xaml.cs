using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RadarEye
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public CvCapture cap;
        public CVO cvo;
        public RFO rfo;
        public bool isRecording = false;
        public bool hasReader = false;
        public MainWindow()
        {
            InitializeComponent();
            cap = CvCapture.FromCamera(0);
            cvo = new CVO(cap, toukui);
            rfo = new RFO();
            toukui.Width = cap.FrameWidth;
            toukui.Height = cap.FrameHeight;
            cvo.showCamera();
            if(hasReader)
                rfo.StartRead();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            cvo.stop = true;
            cvo.isRecordingCamera = false;
            if (hasReader)
                rfo.StopRead();






        }

        private void recordbutton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                String pathString = "data2\\"+DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fffffff") + @"\";
                System.IO.Directory.CreateDirectory(pathString);
                Conf.filepath = pathString;
                cvo.recordCamera();
                if (hasReader)
                    rfo.StartRecord();
                recordbutton.Content = "Stop Record";
                isRecording = true;
            }
            else
            {
                isRecording = false;
                cvo.StopRecord();
                if (hasReader)
                    rfo.StopRecord();
                recordbutton.Content = "Start Record";
            }

        }
    }
}
