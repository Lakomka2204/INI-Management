using System;
namespace ININ.Test
{
    class Test
    {
        static void Main(string[] args)
        {
            D4();
            return;
            D3();
            D1();
            D2();
        }
        static void D1()
        {
            string s = "Section";
            INI ini = new INI("5.ini", INIMode.UpdateOnDispose);
            ini.SetValue(s, "Key", "Value");
            ini.SetValue(s, "Key2", "Value2");
            ini.SetValue(s, "Key3", 0.3);
            Console.WriteLine(ini.GetNumberValue(s, "Key3")); // 0.3
            Console.WriteLine(ini.GetStringValue(s, "Key2")); // Value2
            Console.WriteLine(ini.IsNumber(s, "Key2")); // False
            Console.WriteLine(ini.IsNumber(s, "Key3")); // True
            Console.WriteLine(ini.KeyExists(s, "Key_")); // False
            Console.WriteLine(ini.KeyExists(s, "Key3")); // True
            Console.WriteLine(ini.SectionExists(s)); // True
            Console.WriteLine(ini.SectionExists("section")); // False
            Console.WriteLine(ini.DeleteKey(s, "Key5")); // False
            Console.WriteLine(ini.DeleteKey(s, "Key")); // True
            Console.WriteLine(ini.DeleteSection("section")); // 0 
            //Console.WriteLine(ini.DeleteSection(s)); // 2

            ini.Dispose();
            Console.ReadLine();
            try { ini.GetStringValue("a", "a"); }
            catch (Exception e) { Console.WriteLine(e); }
        }
        static void D2()
        {
            Console.WriteLine("Opening...");
            using (INI x = new INI("ch.ini"))
                foreach (string s in x.GetSections())
                    foreach (string ss in x.GetKeys(s))
                        Console.WriteLine(ss);
        }
        static void D3()
        {
            using(INI v = new INI("6.ini"))
            {
                Console.WriteLine(v.GetNumberValue("1","2"));
                Console.WriteLine(v.GetStringValue("1","3"));
                v.SetValue("2", "1", Math.PI).SetValue("2", "2", "string");
                Console.WriteLine(v.GetStringValue("2","1"));
                Console.WriteLine(v.GetStringValue("2","2"));
                v.DeleteKey("2", "2");

            }
            Environment.Exit(0);
        }
        static void D4()
        {
            using(INI v = new INI("7.ini"))
            {
                v.SetValue(
                    new INIEntry("a", "a", "a"),
                    new INIEntry("b", "b", "b"));
                
            }
        }
    }
}
