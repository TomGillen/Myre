using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using Myre.Graphics.ModelViewer.Model;

namespace Myre.Graphics.ModelViewer.ViewModel
{
    public class StatusViewModel
        : ViewModelBase
    {
        private string currentStatus;
        private float currentProgress;
        private bool isWorking;
        private bool isIndeterminate;
        
        public string Status
        {
            get { return currentStatus; }
            private set
            {
                if (currentStatus != value)
                {
                    currentStatus = value;
                    RaisePropertyChanged("Status");
                }
            }
        }

        public float Progress 
        {
            get { return currentProgress; }
            private set
            {
                if (currentProgress != value)
                {
                    currentProgress = value;
                    RaisePropertyChanged("Progress");

                    IsIndeterminate = currentProgress == -1;
                }
            }
        }

        public bool IsWorking
        {
            get { return isWorking; }
            private set
            {
                if (isWorking != value)
                {
                    isWorking = value;
                    RaisePropertyChanged("IsWorking");
                }
            }
        }

        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                if (isIndeterminate != value)
                {
                    isIndeterminate = value;
                    RaisePropertyChanged("IsIndeterminate");
                }
            }
        }

        public StatusViewModel(IStatusManager status)
        {
            Status = "Ready";
            Progress = 0;
            IsWorking = false;
            
            status.StatusChanged += Update;
        }

        private void Update(IStatusManager status)
        {
            DispatcherHelper.CheckBeginInvokeOnUI((System.Action)delegate()
            {
                Status = status.Status;
                Progress = status.Progress;
                IsWorking = status.IsWorking;
            });
        }
    }
}
