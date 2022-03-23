using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace TravianTools.Data
{
    public class BaseTravianEntity : NotificationObject
    {
        private string _desc;

        public string Desc
        {
            get => _desc;
            set
            {
                _desc = value;
                RaisePropertyChanged(() => Desc);
            }
        }

        private DateTime _updateTime = DateTime.MinValue;

        public DateTime UpdateTime
        {
            get => _updateTime;
            set
            {
                _updateTime = value;
                RaisePropertyChanged(() => UpdateTime);
            }
        }

        private long _updateTimeStamp = 0;

        public long UpdateTimeStamp
        {
            get => _updateTimeStamp;
            set
            {
                _updateTimeStamp = value;
                RaisePropertyChanged(() => UpdateTimeStamp);
            }
        }
    }
}
