using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TravianTools.TravianUtils
{
    //public static class TRequest
    //{
    //    public static JObject BuildingDestroy(string session, int villageId, int locationId) => 
    //        JObject.Parse(Http.Post(RPG.BuildingDestroy(session, villageId, locationId)));

    //    public static JObject BuildingUpgrade(string session, int villageId, int locationId, int buildingType) =>
    //        JObject.Parse(Http.Post(RPG.BuildingUpgrade(session, villageId, locationId, buildingType)));

    //    public static JObject DialogAction(string session, int questId, int dialogId, string command, string input = "") =>
    //        JObject.Parse(Http.Post(RPG.DialogAction(session, questId, dialogId, command, input)));

    //    public static JObject HeroAttribute(string session, int fightStrengthPoints, int attBonusPoints, int defBonusPoints, int resBonusPoints, int resBonusType) =>
    //        JObject.Parse(Http.Post(RPG.HeroAttribute(session, fightStrengthPoints, attBonusPoints, defBonusPoints, resBonusPoints, resBonusType)));

    //    public static JObject Trade(string session, int villageId, int offeredResource, int offeredAmount, int searchedResource, int searchedAmount, bool kingdomOnly) =>
    //        JObject.Parse(Http.Post(RPG.Trade(session, villageId, offeredResource, offeredAmount, searchedResource, searchedAmount, kingdomOnly)));

    //    public static JObject UseHeroItem(string session, int amount, int id, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.UseHeroItem(session, amount, id, villageId)));

    //    public static JObject NpcTrade(string session, int villageId, int r1, int r2, int r3, int r4) =>
    //        JObject.Parse(Http.Post(RPG.NpcTrade(session, villageId, r1, r2, r3, r4)));

    //    public static JObject SetVillageName(string session, int villageId, string villageName) =>
    //        JObject.Parse(Http.Post(RPG.SetVillageName(session, villageId, villageName)));

    //    public static JObject FinishBuild(string session, int villageId, int price, int queueType) =>
    //        JObject.Parse(Http.Post(RPG.FinishBuild(session, villageId, price, queueType)));

    //    public static JObject RecruitUnits(string session, int villageId, int locationId, int buildingType, string unitId, int count) =>
    //        JObject.Parse(Http.Post(RPG.RecruitUnits(session, villageId, locationId, buildingType, unitId, count)));

    //    public static JObject CollectReward(string session, int villageId, int questId) =>
    //        JObject.Parse(Http.Post(RPG.CollectReward(session, villageId, questId)));

    //    public static JObject SendHeroAdv(string session) =>
    //        JObject.Parse(Http.Post(RPG.SendHeroAdv(session)));

    //    public static JObject ChooseTribe(string session, int tribeId) =>
    //        JObject.Parse(Http.Post(RPG.ChooseTribe(session, tribeId)));

    //    public static JObject GetOasis(string session, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.GetOasis(session, villageId)));

    //    public static JObject SendTroops(string session,    int villageId, int destVillageId, int movementType, bool redeployHero,
    //                                     string spyMission, int t1,        int t2,            int t3,           int  t4, int t5, int t6, int t7, int t8, int t9,
    //                                     int    t10,        int t11) =>
    //        JObject.Parse(Http.Post(RPG.SendTroops(session, villageId, destVillageId, movementType, redeployHero, spyMission, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11)));

    //    public static JObject GetAvailableBuildingList(string session, int villageId, int locationId) =>
    //        JObject.Parse(Http.Post(RPG.GetAvailableBuildingList(session, villageId, locationId)));

    //    public static JObject SolvePuzzle(string session, JArray moves) =>
    //        JObject.Parse(Http.Post(RPG.SolvePuzzle(session, moves)));

    //    public static JObject GetPuzzle(string session) =>
    //        JObject.Parse(Http.Post(RPG.GetPuzzle(session)));

    //    #region Cache
        
    //    public static JObject GetCache(string session, List<string> lst) =>
    //        JObject.Parse(Http.Post(RPG.GetCache(session, lst)));

    //    public static JObject GetCache_All(string session) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_All(session)));

    //    public static JObject GetCache_VillageList(string session) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_VillageList(session)));

    //    public static JObject GetCache_CollectionHeroItemOwn(string session) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_CollectionHeroItemOwn(session)));

    //    public static JObject GetCache_Quest(string session) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_Quest(session)));

    //    public static JObject GetCache_Player(string session, int playerId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_Player(session, playerId)));

    //    public static JObject GetCache_Hero(string session, int playerId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_Hero(session, playerId)));

    //    public static JObject GetCache_BuildingQueue(string session, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_BuildingQueue(session, villageId)));

    //    public static JObject GetCache_BuildingCollection(string session, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_BuildingCollection(session, villageId)));

    //    public static JObject GetCache_Building(string session, int buildingId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_Building(session, buildingId)));

    //    public static JObject GetCache_MovingTroopsCollection(string session, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_MovingTroopsCollection(session, villageId)));

    //    public static JObject GetCache_StationaryTroopsCollection(string session, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_StationaryTroopsCollection(session, villageId)));

    //    public static JObject GetCache_Session(string session) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_Session(session)));

    //    public static JObject GetCache_MapDetails(string session, int villageId) =>
    //        JObject.Parse(Http.Post(RPG.GetCache_MapDetails(session, villageId)));

    //    #endregion

    //    public static dynamic GetDataByName(dynamic data, string name)
    //    {
    //        foreach (var x in data)
    //        {
    //            string[] dataNames = x.name.ToString().Split(':');
    //            var      names     = name.Split(':');
    //            if (dataNames.Length != names.Length) continue;
    //            var eqc = 0;
    //            for (var i = 0; i < names.Length; i++)
    //                if (string.IsNullOrEmpty(names[i])) eqc++;
    //                else if (names[i] == "<>") eqc++;
    //                else if (names[i] == dataNames[i]) eqc++;
    //            if (eqc == names.Length) return x;
    //        }

    //        return null;
    //    }

    //    public static List<dynamic> GetDataArrayByName(dynamic data, string name)
    //    {
    //        var lst = new List<dynamic>();
    //        foreach (var x in data)
    //        {
    //            string[] dataNames = x.name.ToString().Split(':');
    //            var      names     = name.Split(':');
    //            if (dataNames.Length != names.Length) continue;
    //            var eqc = 0;
    //            for (var i = 0; i < names.Length; i++)
    //                if (string.IsNullOrEmpty(names[i])) eqc++;
    //                else if (names[i] == "<>") eqc++;
    //                else if (names[i] == dataNames[i]) eqc++;
    //            if (eqc == names.Length) lst.Add(x);
    //        }

    //        return lst.Count != 0 ? lst : null;
    //    }
    //}
}
