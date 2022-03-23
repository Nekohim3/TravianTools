using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TravianTools.Data;

namespace TravianTools.TravianCommands
{
    public abstract class BaseCommand
    {
        [JsonIgnore]
        public virtual Account Account { get; set; }

        public virtual string  Type    { get; set; }

        protected BaseCommand(Account account, Type t)
        {
            Account = account;
            Type    = t.ToString();
        }

        public abstract void Execute();
    }

    public abstract class ResourceCommand : BaseCommand
    {
        public virtual bool NPC { get; set; }

        protected ResourceCommand(Account account, Type t) : base(account, t)
        {

        }

    }

    public class CommandConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(BaseCommand));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            if (jo["Type"].Value<string>().Split('.').Last() == "BuildingUpgradeCmd")
                return jo.ToObject<BuildingUpgradeCmd>(serializer);

            if (jo["Type"].Value<string>().Split('.').Last() == "BuildingDestroyCmd")
                return jo.ToObject<BuildingDestroyCmd>(serializer);

            return null;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
