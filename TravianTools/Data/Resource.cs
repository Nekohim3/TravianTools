using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace TravianTools.Data
{
    public class Resource : BaseTravianEntity
    {
        private int _wood;

        public int Wood
        {
            get => _wood;
            set
            {
                _wood = value;
                RaisePropertyChanged(() => Wood);
                RaisePropertyChanged(() => MultiRes);
            }
        }

        private int _clay;

        public int Clay
        {
            get => _clay;
            set
            {
                _clay = value;
                RaisePropertyChanged(() => Clay);
                RaisePropertyChanged(() => MultiRes);
            }
        }

        private int _iron;

        public int Iron
        {
            get => _iron;
            set
            {
                _iron = value;
                RaisePropertyChanged(() => Iron);
                RaisePropertyChanged(() => MultiRes);
            }
        }

        private int _crop;

        public int Crop
        {
            get => _crop;
            set
            {
                _crop = value;
                RaisePropertyChanged(() => Crop);
                RaisePropertyChanged(() => MultiRes);
            }
        }

        public int MultiRes => Wood + Clay + Iron + Crop;

        public Resource()
        {

        }

        public Resource(dynamic wood, dynamic clay, dynamic iron, dynamic crop, long time)
        {
            Update(wood, clay, iron, crop, time);
        }

        public void Update(dynamic wood, dynamic clay, dynamic iron, dynamic crop, long time)
        {
            Wood = wood;
            Clay = clay;
            Iron = iron;
            Crop = crop;

            UpdateTimeStamp = time;
            UpdateTime      = DateTime.Now;
        }
    }
}
