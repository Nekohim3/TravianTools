using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravianTools.Data
{
    public class BuildingQueue : BaseTravianEntity
    {
        public List<(int idq, int type, int loc, int finishTime)> Queue;
        public List<(int id, int freeCount)>      FreeSlots;
 
        public BuildingQueue(dynamic d, long time)
        {
            Queue = new List<(int idq, int type, int loc, int finishTime)>();
            if (d.queues["1"].Count != 0)
            {
                Queue.Add((1, d.queues["1"][0].buildingType, d.queues["1"][0].locationId, d.queues["1"][0].finished));
            }
            if (d.queues["2"].Count != 0)
            {
                Queue.Add((2, d.queues["2"][0].buildingType, d.queues["2"][0].locationId, d.queues["2"][0].finished));
            }
            if (d.queues["4"].Count != 0)
            {
                foreach (var x in d.queues["4"])
                    Queue.Add((4, x.buildingType, x.locationId, x.finished));
            }
            if (d.queues["5"].Count != 0)
            {
                Queue.Add((5, d.queues["5"][0].buildingType, d.queues["5"][0].locationId, d.queues["5"][0].finished));
            }

            FreeSlots       = new List<(int id, int freeCount)>
                              {
                                  (1, d.freeSlots["1"]),
                                  (2, d.freeSlots["2"]),
                                  (4, d.freeSlots["4"])
                              };

            UpdateTime      = DateTime.Now;
            UpdateTimeStamp = time;
        }
    }

}
