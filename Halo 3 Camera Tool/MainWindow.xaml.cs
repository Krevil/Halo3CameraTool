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
using System.ComponentModel;
using System.Threading;
using Memory;

namespace Halo_3_Camera_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Mem m = new Mem();
        private readonly BackgroundWorker BGWorker = new BackgroundWorker();
        public bool ThirdPerson = false;
        public bool Freecam = false;
        public bool CameraFrozen = false;
        public bool PlayerFrozen = false;
        public bool BarriersDisabled = false;
        public bool Coordinates = false;
        public bool GamePaused = false;
        public bool ThirtyTick = false;
        public bool ProcOpen = false;
        public static bool AboutShown = false;

        public MainWindow()
        {
            InitializeComponent();
            BGWorker.DoWork += BGWorker_DoWork;
            BGWorker.RunWorkerCompleted += BGWorker_RunWorkerCompleted;
            BGWorker.ProgressChanged += new ProgressChangedEventHandler(BGWorker_ProgressChanged);
            BGWorker.WorkerSupportsCancellation = true;
            BGWorker.WorkerReportsProgress = true;
        }

        private void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!ProcOpen)
            {
                ProcOpen = m.OpenProcess("MCC-Win64-Shipping.exe");
                if (!ProcOpen) ProcOpen = m.OpenProcess("MCCWinStore-Win64-Shipping.exe");
                Thread.Sleep(1000);
                return;
            }
            Thread.Sleep(1000);
            BGWorker.ReportProgress(0);
        }

        private void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (ProcOpen)
            {
                StatusTextBlock.Text = "Status: Connected";
                XCoordTextBlock.Text = m.ReadFloat("halo3.dll+0x1cb15c8,0x2bade8", "", false).ToString();
                YCoordTextBlock.Text = m.ReadFloat("halo3.dll+0x1cb15c8,0x2badec", "", false).ToString();
                ZCoordTextBlock.Text = m.ReadFloat("halo3.dll+0x1cb15c8,0x2badf0", "", false).ToString();
            }
        }

        private void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BGWorker.RunWorkerAsync();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            BGWorker.RunWorkerAsync();
        }

        private void ProcessNotFound()
        {
            MessageBox.Show("Unable to find game process", "Error");
        }

        private void ThirdPerson_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                if (!Freecam)
                {
                    int result = m.ReadByte("halo3.dll+0x13EE4E");
                    if (result == 0x74)
                    {
                        m.WriteMemory("halo3.dll+0x13EE4E", "bytes", "0x90 0x90");
                        ThirdPersonButton.Foreground = Brushes.Red;
                        ThirdPerson = true;
                    }
                    else if (result == 0x90)
                    {
                        m.WriteMemory("halo3.dll+0x13EE4E", "bytes", "0x74 0x0E");
                        ThirdPersonButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                        ThirdPerson = false;
                    }
                }
                else MessageBox.Show("Third Person cannot be used while freecam is active.");
            }
            else ProcessNotFound();
        }

        private void Freecam_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                if (!ThirdPerson)
                {
                    int result = m.ReadByte("halo3.dll+0x13E176");
                    if (result == 0x0F)
                    {
                        m.WriteMemory("halo3.dll+0x13EE4E", "bytes", "0x90 0x90"); //Third Person
                        m.WriteMemory("halo3.dll+0x13E176", "bytes", "0xE9 0x3D 0x03 0x00 0x00 0x90"); //Force Third Person to use Freecam instead
                        //result = m.ReadByte("halo3.dll+0x1cb15c8,0x10540");
                        m.WriteMemory("halo3.dll+0x1cb15c8,0x10540", "byte", "0x02");// + (result + 2).ToString("X")); //Enable Blind
                        FreecamButton.Foreground = Brushes.Red;
                        Freecam = true;
                    }
                    else if (result == 0xE9)
                    {
                        m.WriteMemory("halo3.dll+0x13EE4E", "bytes", "0x74 0x0E");
                        m.WriteMemory("halo3.dll+0x13E176", "bytes", "0x0F 0x84 0xA4 0x03 0x00 0x00");
                        //result = m.ReadByte("halo3.dll+0x1cb15c8,0x10540");
                        //if (result >= 2)
                            m.WriteMemory("halo3.dll+0x1cb15c8,0x10540", "byte", "0x00");// + (result - 2).ToString("X"));
                        FreecamButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                        Freecam = false;
                    }
                }
                else MessageBox.Show("Freecam cannot be used while Third Person is active.", "Error");
            }
            else ProcessNotFound();
        }

        private void FreezeCamera_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                if (Freecam)
                {
                    int result = m.ReadByte("halo3.dll+0x13E177");
                    if (result == 0x84 || result == 0x3D)
                    {
                        m.WriteMemory("halo3.dll+0x13E176", "bytes", "0xE9 0xFD 0x01 0x00 0x00 0x90");
                        FreezeCameraButton.Foreground = Brushes.Red;
                    }
                    else if (result == 0xFD)
                    {

                        m.WriteMemory("halo3.dll+0x13E176", "bytes", "0xE9 0x3D 0x03 0x00 0x00 0x90");
                        FreezeCameraButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                    }
                }
                else MessageBox.Show("Freezing the camera requires Freecam to be active.", "Error");
            }
            else ProcessNotFound();
        }

        private void DisableBarriers_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x1cc139"); //Soft ceilings
                if (result == 0x7E)
                {
                    m.WriteMemory("halo3.dll+0x1cc139", "bytes", "0xEB 0x72"); //Kill triggers
                    DisableBarriersButton.Foreground = Brushes.Red;
                    result = m.ReadByte("halo3.dll+0x1C971D");
                    if (result == 0x7E)
                    {
                        m.WriteMemory("halo3.dll+0x1C971D", "bytes", "0xEB 0x65");
                        result = m.ReadByte("halo3.dll+0x1C9796"); //Safe zones
                        if (result == 0x7E)
                        {
                            m.WriteMemory("halo3.dll+0x1C9796", "bytes", "0xEB 0x6D");
                        }
                    }
                }
                else if (result == 0xEB)
                {
                    m.WriteMemory("halo3.dll+0x1cc139", "bytes", "0x7E 0x72");
                    DisableBarriersButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                    result = m.ReadByte("halo3.dll+0x1C971D");
                    if (result == 0xEB)
                    {
                        m.WriteMemory("halo3.dll+0x1C971D", "bytes", "0x7E 0x65");
                        result = m.ReadByte("halo3.dll+0x1C9796");
                        if (result == 0xEB)
                        {
                            m.WriteMemory("halo3.dll+0x1C9796", "bytes", "0x7E 0x6D");
                        }
                    }
                }
                
            }
            else ProcessNotFound();
        }

        private void FreezePlayer_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x1094DE");
                if (result == 0x0F || result == 0x90)
                {
                    m.WriteMemory("halo3.dll+0x1094DE", "bytes", "0xE9 0xC7 0x01 0x00 0x00 0x90");
                    FreezePlayerButton.Foreground = Brushes.Red;
                }
                else if (result == 0xE9)
                {
                    m.WriteMemory("halo3.dll+0x1094DE", "bytes", "0x90 0x90 0x90 0x90 0x90 0x90");
                    FreezePlayerButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                }
            }
            else ProcessNotFound();
        }

        private void PauseGame_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x1cb15c8,0x10b56");
                if (result == 0x00)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10b56", "byte", "0x08");
                    PauseGameButton.Foreground = Brushes.Red;
                }
                else if (result == 0x08)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10b56", "byte", "0x00");
                    PauseGameButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                }
            }
            else ProcessNotFound();
        }

        private void Coordinates_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x13d6ef");
                if (result == 0x0F)
                {
                    m.WriteMemory("halo3.dll+0x13d6ef", "bytes", "0x90 0x90 0x90 0x90 0x90 0x90");
                    CoordinatesButton.Foreground = Brushes.Red;
                    result = m.ReadByte("halo3.dll+0x13d6fc");
                    if (result == 0x0F)
                    {
                        m.WriteMemory("halo3.dll+0x13d6fc", "bytes", "0x90 0x90 0x90 0x90 0x90 0x90");
                    }
                }
                else if (result == 0x90)
                {
                    m.WriteMemory("halo3.dll+0x13d6ef", "bytes", "0x0F 0x84 0xA4 0x01 0x00 0x00");
                    CoordinatesButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                    result = m.ReadByte("halo3.dll+0x13d6fc");
                    if (result == 0x90)
                    {
                        m.WriteMemory("halo3.dll+0x13d6fc", "bytes", "0x0F 0x84 0x97 0x01 0x00 0x00");
                    }
                }
            }
            else ProcessNotFound();
        }

        private void Acrophobia_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x1cb15c8,0x10539");
                if (result >= 32)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10539", "byte", "0x" + (result - 32).ToString("X"));
                    AcrophobiaButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                }
                else if (result < 32)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10539", "byte", "0x" + (result + 32).ToString("X"));
                    AcrophobiaButton.Foreground = Brushes.Red;
                }
            }
            else ProcessNotFound();
        }

        private void Bandana_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x1cb15c8,0x10536");
                if (result >= 128)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10536", "byte", "0x" + (result - 128).ToString("X"));
                    BandanaButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                }
                else if (result < 128)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10536", "byte", "0x" + (result + 128).ToString("X"));
                    BandanaButton.Foreground = Brushes.Red;
                }
            }
            else ProcessNotFound();
        }

        private void ThirtyTick_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ProcOpen)
            {
                int result = m.ReadByte("halo3.dll+0x1cb15c8,0x10b58");
                if (result == 0x1E)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10b58", "bytes", "0x3C 0x00 0x00 0x00 0x89 0x88 0x88 0x3C");
                    ThirtyTickButton.Foreground = Brushes.Red;
                }
                else if (result == 0x3C)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x10b58", "bytes", "0x1E 0x00 0x00 0x00 0x89 0x88 0x08 0x3D");
                    ThirtyTickButton.Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8));
                }
            }
            else ProcessNotFound();
        }

        private void NotYetImplemented(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not yet implemented", "Error");
        }

        private void XCoordTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(XCoordTextBlock.Text, out float result))
            {
                if (ProcOpen)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x2bade8", "float", XCoordTextBlock.Text);
                }
                else XCoordTextBlock.Text = "0.0";
            }
            else
            {
                XCoordTextBlock.Text = ProcOpen ? m.ReadFloat("halo3.dll+0x1cb15c8,0x2bade8", "", false).ToString() : "0.0";
            }
        }

        private void YCoordTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(YCoordTextBlock.Text, out float result))
            {
                if (ProcOpen)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x2badec", "float", YCoordTextBlock.Text);
                }
                else YCoordTextBlock.Text = "0.0";
            }
            else
            {
                YCoordTextBlock.Text = ProcOpen ? m.ReadFloat("halo3.dll+0x1cb15c8,0x2badec", "", false).ToString() : "0.0";
            }
        }

        private void ZCoordTextBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(ZCoordTextBlock.Text, out float result))
            {
                if (ProcOpen)
                {
                    m.WriteMemory("halo3.dll+0x1cb15c8,0x2badf0", "float", ZCoordTextBlock.Text);
                }
                else ZCoordTextBlock.Text = "0.0";
            }
            else
            {
                ZCoordTextBlock.Text = ProcOpen ? m.ReadFloat("halo3.dll+0x1cb15c8,0x2badf0", "", false).ToString() : "0.0";
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimiseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TopmostButton_Click(object sender, RoutedEventArgs e)
        {
            Topmost = Topmost ? Topmost = false : Topmost = true;
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AboutShown)
            {
                Window window = new AboutWindow();
                window.Show();
                AboutShown = true;
            }
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
