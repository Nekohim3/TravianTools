using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using TravianTools.Data;
using TravianTools.Utils.DataBase;
using TaskStatus = TravianTools.Utils.DataBase.TaskStatus;

namespace TravianTools.TravianUtils
{
    public class TaskListExecutor : NotificationObject
    {
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
        private Thread _thread;

        private bool      _working;

        public bool Working
        {
            get => _working;
            set
            {
                _working = value;
                RaisePropertyChanged(() => Working);
            }
        }

        private ObservableCollection<TempTask> _taskList;

        public ObservableCollection<TempTask> TaskList
        {
            get => _taskList;
            set
            {
                _taskList = value;
                RaisePropertyChanged(() => TaskList);
            }
        }

        public TaskListExecutor(Account acc)
        {
            Account = acc;
        }

        public void Start(ObservableCollection<TempTask> col)
        {
            if (Working) return;
            Working  = true;
            TaskList = col;
            foreach (var x in TaskList)
            {
                if (x.Id < Account.CurrentTaskId)
                    x.Status = TaskStatus.Finished;
                if (x.Id == Account.CurrentTaskId)
                    x.Status = TaskStatus.Exec;
                if (x.Id > Account.CurrentTaskId)
                    x.Status = TaskStatus.Wait;
            }
            _thread  = new Thread(ThFunc);
            _thread.Start();
        }

        public void Stop()
        {
            if (!Working) return;
            Working = false;
        }

        private void ThFunc()
        {
            foreach (var x in TaskList.Select(x => x.Command))
            {
                x.Account = Account;
            }

            while (Working)
            {
                if (Account.CurrentTaskId == 0)
                    Account.CurrentTaskId = TaskList.First().Id;
                var task = TaskList.FirstOrDefault(x => x.Id == Account.CurrentTaskId);
                if (task == null) Working = false;
                else
                {
                    if (task.Command.Execute())
                    {
                        task.Status = TaskStatus.Finished;
                        var ind = TaskList.IndexOf(task);
                        if (ind + 1 == TaskList.Count)
                        {
                            Account.CurrentTaskId = int.MaxValue;
                            Accounts.Save();
                            Working               = false;
                        }
                        else
                        {
                            Account.CurrentTaskId = TaskList[ind + 1].Id;
                            Accounts.Save();
                        }
                    }
                    else
                    {
                        task.Status = TaskStatus.Error;
                        Working     = false;
                    }
                }
            }
        }
    }
}
