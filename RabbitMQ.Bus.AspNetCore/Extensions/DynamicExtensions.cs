using Newtonsoft.Json;
using System.Text;

namespace System
{
    public static class DynamicExtensions
    {
        public static string Serialize<TMessage>(this TMessage message)
        {
            return JsonConvert.SerializeObject(message);
        }
        public static byte[] GetBytes(this string body)
        {
            return Encoding.UTF8.GetBytes(body);
        }
    }
}
