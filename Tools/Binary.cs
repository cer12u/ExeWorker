using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class Binary
    {
        private List<EditParams> ChangeLog = new List<EditParams>() { };

        /// <summary>
        /// 変更箇所を記録する
        /// </summary>
        private class EditParams
        {
            public ulong Pos { get; private set; } = 0; //追記位置
            public byte[] Bytes { get; private set; } = new byte[] { };

            public int Size
            {
                get
                {
                    return Bytes.Length;
                }
            }

            void Set(ulong position, byte[] b)
            {
                Pos = position;
                Bytes = new byte[b.Length];
                b.CopyTo(Bytes, 0);
            }

            public EditParams() { }

            public EditParams(ulong pos, byte[] b)
            {
                Set(pos, b);
            }


        }

        public Binary()
        {
            ChangeLog.Clear();

        }

        public bool LoadFile(string file)
        {
            throw new Exception();
        }
        public bool LoadStream(System.IO.Stream stream)
        {
            throw new Exception();
        }


        public bool Append(ulong pos, byte[] b)
        {
            EditParams e = new EditParams(pos, b);

            ChangeLog.Add(e);
            return true;
        }

    }
}
