using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Data;

namespace TravianTools.TravianUtils
{
    public class FreeInstantBuild : NotificationObject
    {
        private bool _working;

        public bool Working
        {
            get => _working;
            set
            {
                _working = value;
                if (_working)
                    Run();
                else
                    Stop();
                RaisePropertyChanged(() => Working);
            }
        }

        private Account _account;

        public Account Account
        {
            get => _account;
            set
            {
                _account = value;
                RaisePropertyChanged(() => Account);
            }
        }

        private Thread _workThread;

        public FreeInstantBuild(Account acc)
        {
            Account = acc;
        }

        private void Run()
        {
            if(_workThread != null) return;
            _workThread = new Thread(ThreadFunc);
            _workThread.Start();
        }

        private void Stop()
        {
            if(_workThread == null) return;
            _workThread.Abort();
            _workThread = null;
        }

        private void ThreadFunc()
        {
            while (Working)
            {
                if (Account.Running)
                {
                    Account.Driver.GetCache(Account.Player.VillageList.Select(x => $"BuildingQueue:{x.Id}").ToList());
                    foreach (var x in Account.Player.VillageList)
                    {
                        var q1 = x.Queue.Queue.FirstOrDefault(c => c.idq == 1);
                        var q2 = x.Queue.Queue.FirstOrDefault(c => c.idq == 2);
                        var q5 = x.Queue.Queue.FirstOrDefault(c => c.idq == 5);
                        if (q1 != default((int, int, int, int)))
                            if (q1.finishTime - x.Queue.UpdateTimeStamp < 295)
                                Account.Driver.FinishNow(x.Id, 1, 0);
                        if (q2 != default((int, int, int, int)))
                            if (q2.finishTime - x.Queue.UpdateTimeStamp < 295)
                                Account.Driver.FinishNow(x.Id, 2, 0);
                        if (q5 != default((int, int, int, int)))
                            if (q5.finishTime - x.Queue.UpdateTimeStamp < 295)
                                Account.Driver.FinishNow(x.Id, 5, 0);
                    }
                }
                Thread.Sleep(5000);
            }
        }
    }
}
