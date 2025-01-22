using System;
using System.Diagnostics;
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
using System.Windows.Threading;

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
            serialPort = new()
            {
                PortName = "COM8",
                BaudRate = 57600,
            };
            this.Loaded+=MainWindow_Loaded;
            lblStatus.Content = "Hãy kéo thanh trượt để chọn mức nước (10-70)";
        }
        string state = "";
        public double ProgressValue = 0;
        public void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
           
            
            try
            {
                serialPort.DataReceived += SerialDataRecieved;
                serialPort.Open();
                //while (true)
                //{
                //    string data = serialPort.ReadExisting();
                //    Console.WriteLine(data);
                //    Thread.Sleep(200);
                //}
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }
        private SerialPort serialPort;

        public void SerialDataRecieved(object sender, SerialDataReceivedEventArgs e)
        {
            string inData = serialPort.ReadLine().Replace("\r","");
            if (inData.EndsWith("R"))
            {
                inData = "R";
                this.Dispatcher.Invoke(() =>
                {
                    progress.Dispatcher.Invoke(() => progress.Value = slider.Value, DispatcherPriority.Background);
                    //progress.Value = slider.Value;
                });
            }
            else if (inData.Length > 10)
            {
                inData = inData.Substring(0, inData.Length - 2);
            }
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
                    this.Dispatcher.Invoke(() =>
                    {
                        progress.Value = Convert.ToDouble(inData);
                    });
                    // progress.Value=Convert.ToDouble(inData);
                }
                if (state == "Fire")
                {
                    ReadAltitude(inData);
                }
            }
        }

        public void Start()
        {
            try
            {

                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                }
                serialPort.Write("N");
                btnStart.IsEnabled = false;
                lblStatus.Content= "Hệ thống đang bơm nhiên liệu ... ";
                int percent = (int)(slider.Value * 100);
                
                for (int i = 0; i < percent; i+=5)
                {

                    progress.Dispatcher.Invoke(() => progress.Value = i*1.7, DispatcherPriority.Background);
                    Thread.Sleep(500);
                    //this.Dispatcher.Invoke(() =>
                    //{
                    //    progress.Value = i;
                       
                    //});
                   
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
            //ReadAltitude("120");
        }

        private void KetThucBomNuoc()
        {
            this.Dispatcher.Invoke(() =>
            {
                btnStart.Visibility = Visibility.Hidden;
                btnFire.Visibility = Visibility.Visible;
                btnStart.IsEnabled = true;
                lblStatus.Content = "Hệ thống đã sẵn sàng";
                //Thread.Sleep(20 * 1000);
                //Fire();
            });
          
            //btnStart.Visibility = Visibility.Hidden;
            //btnFire.Visibility = Visibility.Visible;
        }

        private void Fire()
        {
            try
            {
                if (btnFire.Visibility== Visibility.Visible)
                {
                    serialPort.Write($"F");
                    state = "Fire";
                }
            }
            catch (Exception ex) {
            MessageBox.Show(ex.Message);
            }
        
        }

        private void FireCompleted() {
            this.Dispatcher.Invoke(() =>
            {
                progress.Value = 0;
                slider.Value = 10;
                lblStatus.Content = "Hệ thống đang chuẩn bị cho chu trình tiếp theo ... ";
            });
        }

        private void ReadAltitude(string value)
        {
            string message = $"Tên lửa đã đạt độ cao {value}";
            SpeakText(message);
            this.Dispatcher.Invoke(() =>
            {
                btnStart.Visibility = Visibility.Hidden;
                btnFire.Visibility = Visibility.Hidden;
                
                progress.Value = 0;
                slider.Value = 0;
                state = "";
            });
            Thread.Sleep(20 * 1000);
            this.Dispatcher.Invoke(() =>
            {
                btnStart.Visibility = Visibility.Visible;
                btnFire.Visibility = Visibility.Hidden;
                progress.Value = 0;
                slider.Value = 0;
                state = "";
                lblStatus.Content = "Hãy kéo thanh trượt để chọn mức nước (10-70)";
            });
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

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
              
                serialPort.Write($"W{(int)(e.NewValue * 100)}");
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}