using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TravianTools.Data;

namespace TravianTools.TravianCommands
{
    public abstract class BaseCommand : NotificationObject
    {
        [JsonIgnore]
        public virtual Account Account { get; set; }

        public virtual string  Type    { get; set; }

        protected BaseCommand(Account account)
        {
            Account = account;
            Type    = GetType().ToString();
        }

        public abstract void Execute();
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

            return jo.ToObject(Type.GetType(jo["Type"].Value<string>()), serializer);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
