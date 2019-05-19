using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    class ExifTool2
    {
        public class Exif
        {
            private List<ExifParams> Exifs = new List<ExifParams>();

            public bool SetParameter(ExifParams ep, bool rewrite = true)
            {
                Exifs.Add(ep);
                return true;
            }

            public bool Insert(ExifParams ep)
            {
                return false;
            }

            public bool SetParameter(int id, byte[] data, ExifType type, bool rewrite = true)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<int> GetIdEnumerator()
            {
                foreach(ExifParams ep in Exifs)
                    yield return ep.Id;
            }

            public byte[] GetValueRaw()
            {
                throw new NotImplementedException();
            }

            public bool TryGetValueRaw()
            {
                throw new NotImplementedException();
            }

            public bool RemoveParameter()
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                Exifs.Clear();
            }

            public bool MakeThumbnail(int width = 100, int height = 100)
            {
                throw new NotImplementedException();
            }


            public bool FixedParams = true;

            private string baseFileName = string.Empty;
        }

        public class ExifParams
        {
            public int Id = -1;
            public ExifType Type = 0;
            public int DataLen = 0;
            public byte[] Data = new byte[] { };


        }

        public static Exif LoadFromImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return null;
            if (!System.IO.File.Exists(fileName))
                return null;

            Exif ex = new Exif();

            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(fileName))
            {
                foreach (System.Drawing.Imaging.PropertyItem pi in bmp.PropertyItems)
                {

                    ex.SetParameter(new ExifParams() { Id = pi.Id, Data = pi.Value, DataLen = pi.Len, Type = (ExifType)pi.Type });


                }
                return ex;
            }
            return null;
        }

        public static bool SaveToImage(Exif exif, string fileName)
        {
            throw new NotImplementedException();
        }

        public static bool SaveToImage(Exif exif, string srcPath, string dstPath)
        {
            throw new NotImplementedException();
        }

        public static Exif LoadFromJson(string fileName, bool Compress=false)
        {
            throw new NotImplementedException();
        }

        public static bool SaveToJson(Exif exif, string srcPath, string dstPath)
        {
            throw new NotImplementedException();
        }

        public static Exif LoadFromSecureFile(string fileName, byte[] Key = null)
        {
            throw new NotImplementedException();
        }

        public static Exif SaveToSecureFile(Exif exif, string srcPath, string dstPath)
        {
            throw new NotImplementedException();
        }

        public static Exif MergeExif(Exif firstKey, Exif secondKey)
        {
            throw new NotImplementedException();
        }

        public static System.Drawing.Imaging.PropertyItem CreatePropertyItem()
        {
            using (System.Drawing.Bitmap tb = new System.Drawing.Bitmap(1, 1))
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                tb.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return tb.PropertyItems[0];
            }
        }


        public static class Config
        {
            public static int JpegQuality { get; set; } = 100;
            public static bool AutoAdjustLocale { get; set; } = true;



        }

        public static class ExifInfo
        {

        }

        public static class DefineDicts
        {

        }

        /// <summary>
        /// EXIFのタイプEnum
        /// </summary>
        public enum ExifType
        {

            Undefined = 0,

        }
    }
}
