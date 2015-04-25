using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using NAudio.Wave;
using NAudio.Mixer;
using Microsoft.Win32;

namespace CorkMellow.ViewModel
{
    class MainWindow: ViewModel.Base
    {
        public MainWindow()
        {
            waveSource = new NAudio.Wave.WaveIn();
            waveSource.WaveFormat = new NAudio.Wave.WaveFormat(44100, 1);
            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
            waveSource.StartRecording();
        }
        
        private string _status = "Welcome to cork.mellow, please record something.";
        public string Status
        {
            get 
            { 
                return _status; 
            }
            set 
            { 
                _status = value;
                this.OnPropertyChanged("Status");
            }
        }

        public int DeviceCount
        {
            get { return WaveIn.DeviceCount; }
        }

        public class WaveInCapabilitiesDisplay
        {
            public int Number { get; set; }
            public WaveInCapabilities Capabilities { get; set; }
        }

        public List<WaveInCapabilitiesDisplay> _devices = null;
        public List<WaveInCapabilitiesDisplay> Devices
        {
            get
            {
                if (_devices == null)
                {
                    GetDevices();
                }
                return _devices;
            }
        }

        public void GetDevices()
        {
            for (var i = 0; i < DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                Devices.Add(new WaveInCapabilitiesDisplay() { Number = i, Capabilities = capabilities });
            }
        }
        public NAudio.Wave.WaveIn waveSource = null;
        public NAudio.Wave.WaveFileWriter waveFile = null;
        public bool isRecording = false;
        private string _tempFilePath;
        public void StartRecording()
        {
            //waveSource = new NAudio.Wave.WaveIn();
            //waveSource.WaveFormat = new NAudio.Wave.WaveFormat(44100, 1);
            //waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            //waveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(waveSource_RecordingStopped);
            _tempFilePath = System.IO.Path.GetTempFileName();
            waveFile = new WaveFileWriter(_tempFilePath, waveSource.WaveFormat);
            //waveSource.StartRecording();
        }
        public void StopRecording()
        {
            waveFile = null;
            //waveSource.StopRecording();
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Wav File (*.wav)|*.wav";
            if (dialog.ShowDialog() == true)
            {
                System.IO.File.Copy(_tempFilePath, dialog.FileName, true);
            }
            else
            {
                System.IO.File.Delete(_tempFilePath);
            }
            
        }
        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
            _lastPeak = 0;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) |
                            e.Buffer[index + 0]);

                float sample32 = sample / 32768f;
                if (Math.Abs(sample32) > _lastPeak)
                {
                    _lastPeak = Math.Abs(sample32);
                }
            }
            OnPropertyChanged("CurrentInputLevel");
        }
        
        public float CurrentInputLevel {
            get{
                return _lastPeak * 100;
            }
        }
        void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveSource != null)
            {
                waveSource.Dispose();
                waveSource = null;
            }

            if (waveFile != null)
            {
                waveFile.Dispose();
                waveFile = null;
            }

        }
        private RelayCommand _recordCommand;
        public ICommand RecordCommand
        {
            get
            {
                if (_recordCommand == null)
                    _recordCommand = new RelayCommand(param => this.Record(), param => this.CanRecord());
                return _recordCommand;
            }
        }
        
        public void Record()
        {
            this.Status = "Recording...";
            if (!isRecording)
            {
                StartRecording();
                isRecording = true;
            }
            else
            {
                StopRecording();
                isRecording = false;
            }
        }
        public bool CanRecord()
        {
            return true;
        }
        private double _microphoneVolume = 100;
        public double MicrophoneVolume
        {
            get
            {
                return _microphoneVolume;
            }
            set
            {
                SetMicrophoneVolume(value);
                _microphoneVolume = value;
                OnPropertyChanged("MicrophoneVolume");
            }
        }
        public void SetMicrophoneVolume(double volume)
        {
            int waveInDeviceNumber = 0;
            var mixerLine = new MixerLine((IntPtr)waveInDeviceNumber,
                                           0, MixerFlags.WaveIn);
            foreach (var control in mixerLine.Controls)
            {
                if (control.ControlType == MixerControlType.Volume)
                {
                    var volumeControl = control as UnsignedMixerControl;
                    volumeControl.Percent = volume;
                    break;
                }
            }
        }

        private float _lastPeak = 1;
    }
}
