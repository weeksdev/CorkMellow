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
                    OnPropertyChanged("Devices");
                }
                return _devices;
            }
        }
        public void GetDevices()
        {
            _devices = new List<WaveInCapabilitiesDisplay>();
            for (var i = 0; i < DeviceCount; i++)
            {
                var capabilities = WaveIn.GetCapabilities(i);
                _devices.Add(new WaveInCapabilitiesDisplay() { Number = i, Capabilities = capabilities });
            }
        }
        public class RecordingPlot
        {
            public float Minimum { get; set; }
            public float Maximum { get; set; }
        }

        private List<RecordingPlot>  _currentRecordingPlots = new List<RecordingPlot>();
        public List<RecordingPlot>  CurrentRecordingPlots
        {
            get { 
                return _currentRecordingPlots; 
            }
            set { 
                _currentRecordingPlots = value;
                OnPropertyChanged("CurrentRecordingPlots");
            }
        }
        
        public void AddRecordingPlot(float minimum, float maximum)
        {
            _currentRecordingPlots.Add(new RecordingPlot() { Minimum = minimum, Maximum = maximum });
        }

        
        public NAudio.Wave.WaveIn waveSource = null;
        public NAudio.Wave.WaveFileWriter waveFile = null;
        public bool isRecording = false;
        private string _tempFilePath;
        public void StartRecording()
        {
            _tempFilePath = System.IO.Path.GetTempFileName();
            this.CurrentRecordingPlots = new List<RecordingPlot>();
            waveFile = new WaveFileWriter(_tempFilePath, waveSource.WaveFormat);
        }
        public void StopRecording()
        {
            waveFile.Close();
            waveFile = null;
            //AddWaveFormLines();   
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Wav File (*.wav)|*.wav";
            if (dialog.ShowDialog() == true)
            {
                System.IO.File.Copy(_tempFilePath, dialog.FileName, true);
            }
        }
        
        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveFile != null)
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                waveFile.Flush();
            }
            _lastMaximum = 0;
            _lastMinimum = 0;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((e.Buffer[index + 1] << 8) |
                            e.Buffer[index + 0]);

                float sample32 = sample / 32768f;
                float value = sample32 * 100;
                _lastMaximum = Math.Max(value, _lastMaximum);
                _lastMinimum = Math.Min(value, _lastMinimum);
            }
            AddRecordingPlot(_lastMinimum, _lastMaximum);
            OnPropertyChanged("CurrentRecordingPlots");
            OnPropertyChanged("CurrentInputLevel");
        }
        
        public float CurrentInputLevel {
            get{
                return _lastMaximum;
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
        private RelayCommand _playCommand;

        public ICommand PlayCommand
        {
            get
            {
                if (_playCommand == null)
                    _playCommand = new RelayCommand(param => this.Play(), param => true);
                return _playCommand;
            }
        }

        public void Play()
        {
            WaveOut waveOut = new WaveOut();
            var audioFileReader = new AudioFileReader(this._tempFilePath);
            waveOut.Init(audioFileReader);
            waveOut.Play();
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

        private float _lastMaximum = 0;
        private float _lastMinimum = 0;

    }
}
