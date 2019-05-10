using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            NestedClass ns = new NestedClass();

            ns.SetValue(ns, "Param[1][1][2]", 1);

            ns.SetValue(ns, "Param", 1);

        }

        public class NestedClass
        {
            public int Param { get; set; } = 0;
            public List<int> ParamL1 { get; set; } = new List<int>();
            public List<List<int>> ParamL2 { get; set; } = new List<List<int>>();
            public List<List<List<int>>> ParamL3 { get; set; } = new List<List<List<int>>>();
            public List<List<List<List<int>>>> ParamL4 { get; set; } = new List<List<List<List<int>>>>();

            public bool SetValue(object obj, string name, int value)
            {
                //System.Reflection.PropertyInfo ip = obj.GetType().GetProperty("ParamL2");
                //if (ip is null)
                //    return false;
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(obj.GetType())){
                    Console.WriteLine("Assignable");
                }

                Type tp = obj.GetType();
                foreach(System.Reflection.PropertyInfo pi in tp.GetType().GetProperties())
                {
                    Type ttp = pi.PropertyType;
                    if(ttp == typeof(System.String))
                    {
                        Console.WriteLine("String");
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(ttp))
                    {
                        Console.WriteLine(pi.Name);
                        Console.WriteLine("Match");
                    }
                }

                //Console.WriteLine("FieldInfo");
                //foreach (System.Reflection.FieldInfo fi in tp.GetType().GetFields())
                //    Console.WriteLine(fi.Name);
                //Console.WriteLine("MemberInfo");
                //foreach (System.Reflection.MemberInfo mi in tp.GetType().GetMethods())
                //    Console.WriteLine(mi.Name);
                
                return false;
            }

        }
    }
}
