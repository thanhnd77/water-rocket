using System.IO.Ports;
using System.Speech.Synthesis;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded+=MainWindow_Loaded;
        }
        string state = "Start";
        public void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            serialPort = new()
            {
                PortName = "COM3",
                BaudRate = 9600
            };
            try
            {
                serialPort.DataReceived += SerialDataRecieved;
                serialPort.Open();
                while (true)
                {
                    string data = serialPort.ReadExisting();
                    Console.WriteLine(data);
                    Thread.Sleep(200);
                }
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        private SerialPort serialPort;

        public void SerialDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            string inData = serialPort.ReadLine();
            Console.WriteLine($"Data Received: {inData}");
            if (inData == "R")
            {
                KetThucBomNuoc();
            }
           else if (inData == "D")
            {
                FireCompleted();
            }
            else
            {
                if (state == "Start")
                {
                   progress.Value=Convert.ToDouble(inData);
                }
                if (state == "Fire")
                {
                    ReadAltitude(inData);
                }
            }
        }

        public void Start()
        {
            serialPort.Write($"W{slider.Value}");
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
            //ReadAltitude("120");
        }

        private void KetThucBomNuoc()
        {
            btnStart.Visibility= Visibility.Hidden;
            btnFire.Visibility = Visibility.Visible;
        }

        private void Fire()
        {
            serialPort.Write($"F");
            state = "Fire";
        }

        private void FireCompleted() {
            progress.Value = 0;
            slider.Value = 10;
        }

        private void ReadAltitude(string value)
        {
            string message = $"Tên lửa đã đạt độ cao {value}";
            SpeakText(message);
            Thread.Sleep(20 * 1000);
            btnStart.Visibility = Visibility.Visible;
            btnFire.Visibility = Visibility.Hidden;
            state = "Start";
        }

        private void btnFire_Click(object sender, RoutedEventArgs e)
        {
            Fire();
        }

        private void SpeakText(string TTS)
        {
            SpeechSynthesizer ttssynthesizer = new SpeechSynthesizer();
            ttssynthesizer.Speak(TTS);
        }
    }
}