using Google.Protobuf;
using System.Reflection;

namespace EpinelPS.Utils
{
    public static class LogUtils
    {
        public static void LogMessageFields(IMessage message)
        {
            Type type = message.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine(type.Name + ":");

            foreach (FieldInfo field in fields)
            {
                if (field.Name.ToLower().Contains("unknown"))
                    continue;
                
                object? value = field.GetValue(message);

                Console.WriteLine($"\t{field.Name}: {value}");
            }
            
            Console.ResetColor();
        }
    }
}
