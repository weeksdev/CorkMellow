using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorkMellow.ViewModel
{
    class MainWindow: ViewModel.Base
    {
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
        }
        public bool CanRecord()
        {
            return true;
        }
    }
}
