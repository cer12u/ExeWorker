using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class Images
    {
        public int Thickness { get; set; } = 1;

        public System.Drawing.Color DrawColor { get; private set; } = System.Drawing.Color.Gray; 

        public bool SetColor(System.Drawing.Color c)
        {
            DrawColor = c;
            return true;
        }

        /// <summary>
        /// String形式で色を指定する
        /// </summary>
        /// <param name="Color">使う色を指定する(大小文字が区別される)</param>
        /// <returns>セットの成否</returns>
        public bool SetColor(string Color)
        {
            try
            {
                Type type = typeof(System.Drawing.Color);
                //object ac = Activator.CreateInstance(type);
                System.Reflection.PropertyInfo pi = null;
                pi = type.GetProperty(Color);

                if (pi is null)
                    return false;

                DrawColor = (System.Drawing.Color)pi.GetValue(null, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
