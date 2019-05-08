using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class ExifManagerDotNet : IDisposable
    {

        private List<EXIF_TAGS> TagList = new List<EXIF_TAGS>();

        private int JpegQuality = 100;
        /// <summary>
        /// 情報のデータ補正設定
        /// </summary>
        public bool AutoAdjust { get; private set; } = true;

        /// <summary>
        /// 指定ファイルにEXIFデータを書き込む
        /// 保持しているデータ以外はすべてクリアする
        /// </summary>
        /// <param name="filePath">元ファイルパス</param>
        /// <param name="dstPath">書き換え後ファイルの保存先 / 指定なしの場合はsrcに上書きする</param>
        /// <returns>0 or エラー値</returns>
        public int SetExifToImage(string filePath, string dstPath = null)
        {
            if (string.IsNullOrEmpty(filePath))
                return (int)ErrorNumber.ArgumentsNull;
            if (!System.IO.File.Exists(filePath))
                return (int)ErrorNumber.FileNotFound;

            if (string.IsNullOrEmpty(dstPath))
                dstPath = filePath;
            else
            {
                try
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(dstPath))))
                        return (int)ErrorNumber.DirectoryNotFound;
                }
                catch
                {
                    return (int)ErrorNumber.FileIOException;
                }
            }


            System.Drawing.Bitmap bmp = null;
            try
            {
                bmp = new System.Drawing.Bitmap(filePath);

                System.Drawing.Imaging.PropertyItem pp;
                if (bmp.PropertyItems.Count() < 0)
                    pp = bmp.PropertyItems[0];
                else
                {
                    // PropertyItem作成
                    System.Drawing.Bitmap tb = new System.Drawing.Bitmap(1, 1);
                    System.IO.MemoryStream mst = new System.IO.MemoryStream();
                    tb.Save(mst, System.Drawing.Imaging.ImageFormat.Jpeg);
                    tb = new System.Drawing.Bitmap(mst);
                    pp = tb.PropertyItems[0];
                }


                foreach (System.Drawing.Imaging.PropertyItem pi in bmp.PropertyItems)
                {
                    bmp.RemovePropertyItem(pi.Id);
                }

                foreach (EXIF_TAGS et in TagList)
                {
                    pp.Id = et.tag;
                    pp.Type = (short)et.type;

                    pp.Value = new byte[et.data.Length];

                    et.data.CopyTo(pp.Value, 0);
                    if (et.type == (int)ExifType.ASCII)
                    {
                        if ((et.data[et.data.Length - 1] != '\0') && AutoAdjust)
                        {
                            byte[] tmpb = new byte[et.data.Length];
                            et.data.CopyTo(tmpb, 0);
                            pp.Value = new byte[et.data.Length + 1];
                            tmpb.CopyTo(pp.Value, 0);
                            pp.Value[et.data.Length] = (byte)'\0';
                        }
                    }

                    switch ((ExifType)Enum.ToObject(typeof(ExifType), et.type))
                    {
                        case ExifType.BYTE:
                            pp.Len = 1;
                            break;
                        case ExifType.SHORT:
                            pp.Len = 2;
                            break;
                        case ExifType.LONG:
                        case ExifType.SLONG:
                            pp.Len = 4;
                            break;
                        case ExifType.RATIONAL:
                        case ExifType.SRATIONAL:
                            pp.Len = 8;
                            break;
                        case ExifType.UNDEFINED:
                        case ExifType.ASCII:
                            pp.Len = pp.Value.Length;
                            break;
                        default:
                            pp.Len = 0;
                            break;
                    }

                    bmp.SetPropertyItem(pp);

                }

                System.Drawing.Imaging.EncoderParameters eps = new System.Drawing.Imaging.EncoderParameters(1);
                System.Drawing.Imaging.EncoderParameter ep = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality);
                eps.Param[0] = ep;

                System.Drawing.Imaging.ImageCodecInfo jici = null;
                foreach (System.Drawing.Imaging.ImageCodecInfo ici in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
                    if (ici.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
                    {
                        jici = ici;
                        break;
                    }

                if (jici is null)
                    return (int)ErrorNumber.ParameterNull;

                if (!filePath.Equals(dstPath))
                {
                    bmp.Save(dstPath, jici, eps);
                    bmp.Dispose();
                }
                else
                {
                    string tempPath = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    bmp.Save(tempPath, jici, eps);
                    bmp.Dispose();

                    try
                    {
                        System.IO.File.Delete(filePath);
                        System.IO.File.Move(tempPath, dstPath);
                    }
                    finally
                    {
                        if (System.IO.File.Exists(tempPath))
                            System.IO.File.Delete(tempPath);
                    }
                }
            }
            catch
            {
                return (int)ErrorNumber.Unknown;
            }
            finally
            {
                if (bmp != null)
                    bmp.Dispose();
            }
            return (int)ErrorNumber.NoError;
        }

        /// <summary>
        /// EXIF情報セットをファイルに書き出す
        /// </summary>
        /// <param name="filePath">保存先</param>
        /// <returns>0 or ErrorCode</returns>
        public int SaveExifToXml(string filePath)
        {
            if (filePath is null)
                return (int)ErrorNumber.ArgumentsNull;

            try
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<EXIF_TAGS>));
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, Encoding.UTF8))
                    xs.Serialize(sw, TagList);

                    return (int)ErrorNumber.NoError;
            }
            catch
            {
                return (int)ErrorNumber.Unknown;
            }
        }

        private bool IsValidFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
            if (!System.IO.File.Exists(fileName))
                return false;

            return true;
        }

        public bool IsValidDir(string pathName)
        {
            if (string.IsNullOrEmpty(pathName))
                return false;
            if (!System.IO.Directory.Exists(pathName))
                return false;
            return true;

        }

        public bool AppendNullByte(ref byte[] bytes)
        {
            if (bytes.Length <= 0)
                return false;

            if(bytes[bytes.Length -1] != '\0')
            {
                byte[] tmpb = new byte[bytes.Length];
                bytes.CopyTo(tmpb, 0);
                bytes = new byte[bytes.Length + 1];
                tmpb.CopyTo(bytes, 0);
                bytes[bytes.Length] = (byte)'\0';
            }

            return true;
        }

        /// <summary>
        /// 保存したEXIF情報セットをファイルに書き出す
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>0 or ErrorCode</returns>
        public int LoadExifFromXml(string filePath)
        {
            if (filePath is null)
                return (int)ErrorNumber.ArgumentsNull;
            if (!System.IO.File.Exists(filePath))
                return (int)ErrorNumber.FileNotFound;

            try
            {
                TagList.Clear();

                System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(List<EXIF_TAGS>));
                using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath, Encoding.UTF8))
                    TagList = (List<EXIF_TAGS>)xs.Deserialize(sr);

                return (int)ErrorNumber.NoError;

            }
            catch
            {
                return (int)ErrorNumber.Unknown;
            }
        }

        /// <summary>
        /// 画像からEXIF情報を取得する
        /// 保持していたEXIFタグリストは破棄する
        /// </summary>
        /// <param name="filePath">JPGファイルパス</param>
        /// <returns>0 or エラー値</returns>
        public int GetExifByImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return (int)ErrorNumber.ArgumentsNull;
            if (!System.IO.File.Exists(filePath))
                return (int)ErrorNumber.FileNotFound;

            System.Drawing.Bitmap bmp = null;
            try
            {
                bmp = new System.Drawing.Bitmap(filePath);
                TagList.Clear();
                foreach (System.Drawing.Imaging.PropertyItem pi in bmp.PropertyItems)
                {
                    EXIF_TAGS te = new EXIF_TAGS();
                    te.tag = pi.Id;
                    te.type = pi.Type;
                    te.data = new byte[pi.Value.Length];
                    pi.Value.CopyTo(te.data, 0);

                    TagList.Add(te);
                }
            }
            catch
            {
                return (int)ErrorNumber.Unknown;
            }
            finally
            {
                bmp.Dispose();
            }
            return (int)ErrorNumber.NoError;
        }

        /// <summary>
        /// タグ情報一覧を別のTagにコピーする
        /// </summary>
        /// <param name="exmm">出力先class</param>
        public void CopyTo(out ExifManagerDotNet exmm)
        {
            exmm = new ExifManagerDotNet();
            foreach (EXIF_TAGS ts in this.TagList)
                exmm.TagList.Add(ts);
        }

        /// <summary>
        /// EXIFリストに値をセットする
        /// タグがある場合は置換、ない場合は追加する
        /// </summary>
        /// <param name="tagId">EXIFタグ番号</param>
        /// <param name="value">セットする値</param>
        /// <param name="rewriteTag">タグのマニュアル指定</param>
        /// <returns>セットの成功可否</returns>
        public bool SetValue(int tagId, dynamic value, int rewriteTag = -1)
        {
            int InsMarker = -1;

            EXIF_TAGS ee = new EXIF_TAGS() { tag = tagId };

            foreach (EXIF_TAGS et in TagList)
            {
                if (tagId != et.tag)
                    continue;
                ee = et;

                InsMarker = TagList.IndexOf(et);
                TagList.Remove(et);

                break;
            }


            Type type = value.GetType();

            if (type == typeof(System.String))
            {
                ee.data = System.Text.Encoding.ASCII.GetBytes(value);
                ee.type = (int)ExifType.ASCII;
            }
            else if (type == typeof(Tuple<Int32, Int32>))
            {
                Tuple<Int32, Int32> tp = value;
                ee.data = new byte[8];
                BitConverter.GetBytes(tp.Item1).CopyTo(ee.data, 0);
                BitConverter.GetBytes(tp.Item2).CopyTo(ee.data, 4);
                ee.type = (int)ExifType.SRATIONAL;
            }
            else if (type == typeof(Tuple<UInt32, UInt32>))
            {
                Tuple<UInt32, UInt32> tp = value;
                ee.data = new byte[8];
                BitConverter.GetBytes(tp.Item1).CopyTo(ee.data, 0);
                BitConverter.GetBytes(tp.Item2).CopyTo(ee.data, 4);
                ee.type = (int)ExifType.RATIONAL;
            }
            else
            {
                ee.data = BitConverter.GetBytes(value);

                if (rewriteTag >= 0) { } //処理不要時のNOP

                else if (type == typeof(byte))
                    ee.type = (int)ExifType.BYTE;
                else if (type == typeof(ushort))
                    ee.type = (int)ExifType.SHORT;
                else if (type == typeof(byte[]))
                    ee.type = (int)ExifType.UNDEFINED;
                else if (type == typeof(UInt32))
                    ee.type = (int)ExifType.LONG;
                else if (type == typeof(int))
                    ee.type = GetExifTypeById(value);
            }


            if (rewriteTag >= 0)
                ee.type = rewriteTag;

            if (InsMarker < 0)
                TagList.Add(ee);
            else
                TagList.Insert(InsMarker, ee);


            return true;
        }

        /// <summary>
        /// Dynamicで指定しなかった場合の自動判定用
        /// MockとしてSLONGを指定 → Listから判別に修正する
        /// </summary>
        /// <param name="data">データ部分</param>
        /// <returns>ExifTypeのInt値</returns>
        public int GetExifTypeById(dynamic data)
        {
            return (int)ExifType.SLONG;
        }

        /// <summary>
        /// タグ番号からタグを検索し、見つかれば値を返却する
        /// </summary>
        /// <param name="tagId">EXIFタグ番号</param>
        /// <param name="parsedValue">取得したデータ</param>
        /// <returns>検索がHitすればTrue/無ければFalse</returns>
        public bool TryGetValue(int tagId, out dynamic parsedValue)
        {
            int i = 0;
            return TryGetValue(tagId, out parsedValue, ref i);
        }

        /// <summary>
        /// タグ番号からタグを検索し、見つかれば値を返却する
        /// </summary>
        /// <param name="tagId">EXIFタグ番号</param>
        /// <param name="parsedValue">取得したデータ</param>
        /// <param name="exifType">取得したEXIFのTYPE</param>
        /// <returns>検索がHitすればTrue/無ければFalse</returns>
        public bool TryGetValue(int tagId, out dynamic parsedValue, ref int exifType)
        {
            parsedValue = null;
            EXIF_TAGS et = null;

            foreach (EXIF_TAGS etag in TagList)
            {
                if (tagId != etag.tag)
                    continue;

                et = etag;
            }

            if (et is null)
                return false;



            switch ((ExifType)Enum.ToObject(typeof(ExifType), et.type))
            {
                case ExifType.BYTE:
                    if (et.data.Length > 0)
                        parsedValue = et.data[0];
                    break;
                case ExifType.ASCII:
                    parsedValue = System.Text.Encoding.ASCII.GetString(et.data).TrimEnd('\0');
                    break;
                case ExifType.SHORT:
                    if (et.data.Length >= 2)
                        parsedValue = BitConverter.ToUInt16(et.data, 0);
                    break;
                case ExifType.LONG:
                    if (et.data.Length >= 4)
                        parsedValue = BitConverter.ToUInt32(et.data, 0);
                    break;
                case ExifType.RATIONAL:
                    if (et.data.Length >= 8)
                        parsedValue = Tuple.Create(BitConverter.ToUInt32(et.data, 0), BitConverter.ToUInt32(et.data, 4));
                    break;
                case ExifType.UNDEFINED:
                    parsedValue = new byte[et.data.Length];
                    et.data.CopyTo(parsedValue, 0);
                    break;
                case ExifType.SLONG:
                    if (et.data.Length >= 4)
                        parsedValue = BitConverter.ToInt32(et.data, 0);
                    break;
                case ExifType.SRATIONAL:
                    if (et.data.Length >= 8)
                        parsedValue = Tuple.Create(BitConverter.ToInt32(et.data, 0), BitConverter.ToInt32(et.data, 4));
                    break;

            }

            return (parsedValue != null);

        }

        /// <summary>
        /// 保持しているEXIFタグのEnumerator取得用
        /// </summary>
        /// <returns>EXIF TAG Number</returns>
        public IEnumerable<int> GetTagEnumerator()
        {
            foreach (EXIF_TAGS te in TagList)
            {
                yield return te.tag;
            }
        }

        /// <summary>
        /// Class内のReturnCodeの内容を返却する
        /// </summary>
        /// <param name="errorCode">エラーコード</param>
        /// <returns>エラーコード定義名</returns>
        public string GetErrorCode(int errorCode)
        {
            try
            {
                ErrorNumber en = (ErrorNumber)Enum.ToObject(typeof(ErrorNumber), errorCode);
                return en.ToString();
            }
            catch
            {
                throw;

            }
            return string.Empty;
        }

        public void Dispose()
        {
            TagList.Clear();
        }

        /// <summary>
        /// 返却のReturnCode管理用
        /// GetErrorCode()で変数名を返却するため名称に注意する
        /// </summary>
        private enum ErrorNumber
        {
            NoError = 0,
            ArgumentsNull = -100,
            FileNotFound = -101,
            DirectoryNotFound = -102,

            FileIOException = -400,
            ParameterNull = -401,
            Unknown = -900,
        }

        /// <summary>
        /// EXIF仕様のTYPE項目を列挙
        /// </summary>
        private enum ExifType
        {
            BYTE = 1,
            ASCII = 2,
            SHORT = 3,
            LONG = 4,
            RATIONAL = 5,
            UNDEFINED = 7,
            SLONG = 9,
            SRATIONAL = 10
        }



        /// <summary>
        /// EXIFタグ格納用class
        /// </summary>
        public class EXIF_TAGS
        {
            public int tag;
            public int type;
            public byte[] data;
        }



    }
}
