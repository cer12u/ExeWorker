using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExeWorker
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Invoker i = new Invoker();


        }

        /// <summary>
        /// EXEファイルの実行管理用class
        /// </summary>
        private class Invoker :IDisposable
        {
            public string ToolPath { get; private set; } = string.Empty;
            public string WorkDirectory { get; private set; } = string.Empty;
            private List<string> Args = new List<string>();

            private System.Diagnostics.Process Procs = null;

            private List<string> resultLines = new List<string>();
            private int resultIndex = 0;

            private DateTime StartTime = DateTime.MinValue;
            private System.Timers.Timer TimeoutTimer = new System.Timers.Timer();
            private int AllowTime = 30 * 60 * 1000;

            /// <summary>
            /// 実行状態
            /// </summary>
            public bool HasExited
            {
                get
                {
                    if (Procs == null)
                        return true;
                    return Procs.HasExited;
                }
            }

            /// <summary>
            /// 初期化
            /// </summary>
            public Invoker()
            {
                ToolPath = string.Empty;
                Args = new List<string>();
                Procs = null;
                resultLines = new List<string>();
                resultIndex = 0;
            }

            /// <summary>
            /// 実行するEXEファイルの場所を指定する
            /// ファイルパスが有効な場合にセットされる
            /// </summary>
            /// <param name="path">ファイルのパス</param>
            /// <returns>成否</returns>
            public bool SetToolPath(string path)
            {
                if (System.IO.File.Exists(path))
                {
                    ToolPath = path;
                }
                return false;
            }

            /// <summary>
            /// 処理を開始する
            /// </summary>
            /// <returns>実行開始の成否</returns>
            public bool Run()
            {
                if (Procs != null)
                    return false;

                if (string.IsNullOrEmpty(ToolPath))
                    return false;

                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = ToolPath;

                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;

                //if (string.IsNullOrEmpty(WorkDirectory))
                //    psi.WorkingDirectory = System.IO.Path.GetDirectoryName(ToolPath);
                //else
                //    psi.WorkingDirectory = WorkDirectory;
                psi.WorkingDirectory = (string.IsNullOrEmpty(WorkDirectory)) ? System.IO.Path.GetDirectoryName(ToolPath) : WorkDirectory;

                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;

                foreach (string s in Args)
                    psi.Arguments += s + " ";
                psi.Arguments.TrimEnd(' ');


                Procs = new System.Diagnostics.Process();
                Procs.StartInfo = psi;

                TimeoutTimer = new System.Timers.Timer();
                TimeoutTimer.Interval = AllowTime;
                TimeoutTimer.Elapsed += TimeoutHandler;
                TimeoutTimer.AutoReset = false;

                TimeoutTimer.Start();


                Procs.OutputDataReceived += AppendLines;
                Procs.Exited += ExitedHandler;
                //Procs.ExitCode;
                //Procs.ErrorDataReceived;
                Procs.EnableRaisingEvents = true;

                //Timer

                StartTime = Procs.StartTime;



                return false;
            }

            private void TimeoutHandler(object sender, ElapsedEventArgs e)
            {
                if (Procs != null)
                {
                    if (!Procs.HasExited)
                        Procs.Kill();
                }
            }

            private void ExitedHandler(object sender, EventArgs e)
            {
                TimeoutTimer.Stop();
                TimeoutTimer.Dispose();
            }

            private void AppendLines(object sender, DataReceivedEventArgs e)
            {
                using(System.IO.StreamReader sr = Procs.StandardOutput)
                    while (!sr.EndOfStream)
                        resultLines.Add(sr.ReadLine());
            }


            /// <summary>
            /// 出力データの有無を出力する
            /// </summary>
            /// <returns>データがあればtruw</returns>
            public bool HasOutputData()
            {
                if (resultIndex < resultLines.Count)
                    return true;
                return false;

            }

            /// <summary>
            /// 出力を読み込む
            /// 読み出しは１行ごとに行う
            /// </summary>
            /// <returns>取得した１行のデータ</returns>
            public string ReadOutputLine()
            {
                if (resultLines.Count > resultIndex)
                    return resultLines[resultIndex++];
                return string.Empty;
            }

            /// <summary>
            /// 出力を読み込む
            /// 読み出しは１行ごとに行う
            /// </summary>
            /// <param name="idx">マニュアル指定した行番号</param>
            /// <returns>取得した１行のデータ</returns>
            public string ReadOutputLine(int idx)
            {
                if (idx < 0)
                    return string.Empty;

                if (resultLines.Count > idx)
                    return resultLines[idx];
                return string.Empty;

            }

            /// <summary>
            /// 終了処理
            /// </summary>
            public void Dispose()
            {
                if(Procs != null)
                {
                    if (!Procs.HasExited)
                        Procs.Kill();
                    Procs.Dispose();
                }
                resultLines.Clear();
                Procs = null;
            }
        }

        /// <summary>
        /// EXIF処理用class
        /// todo:公開範囲の設定
        /// </summary>
        //public class ExifManager
        //{
        //    public static class StaticFields
        //    {
        //        public static bool Loaded { get; private set; } = false;

        //        public static List<IFD_TYPE> ExifTypes = new List<IFD_TYPE>()
        //        {

        //            new IFD_TYPE() { Id = 1, TypeName = "BYTE", FieldSize = 1 },
        //            new IFD_TYPE() { Id = 2, TypeName = "ASCII", FieldSize = -1 },
        //            new IFD_TYPE() { Id = 3, TypeName = "SHORT", FieldSize = 2 },
        //            new IFD_TYPE() { Id = 4, TypeName = "LONG", FieldSize = 4 },
        //            new IFD_TYPE() { Id = 5, TypeName = "RATIONAL", FieldSize = 8 },
        //            new IFD_TYPE() { Id = 7, TypeName = "UNDEFINED", FieldSize = -1 },
        //            new IFD_TYPE() { Id = 9, TypeName = "SLONG", FieldSize = 4 },
        //            new IFD_TYPE() { Id = 10, TypeName = "SRATIONAL", FieldSize = 8 },
        //            new IFD_TYPE() { Id = -1, TypeName = "", FieldSize = 0 }
        //        };
        //    }


        //    //EXIFのタイプ管理用
        //    public static class ExifType
        //    {
        //        public static readonly IFD_TYPE BYTE = new IFD_TYPE() { Id = 1, TypeName = "BYTE", FieldSize = 1 };
        //        public static readonly IFD_TYPE ASCII = new IFD_TYPE() { Id = 2, TypeName = "ASCII", FieldSize = -1 };
        //        public static readonly IFD_TYPE SHORT = new IFD_TYPE() { Id = 3, TypeName = "SHORT", FieldSize = 2 };
        //        public static readonly IFD_TYPE LONG = new IFD_TYPE() { Id = 4, TypeName = "LONG", FieldSize = 4 };
        //        public static readonly IFD_TYPE RATIONAL = new IFD_TYPE() { Id = 5, TypeName = "RATIONAL", FieldSize = 8 };
        //        public static readonly IFD_TYPE UNDEFINED = new IFD_TYPE() { Id = 7, TypeName = "UNDEFINED", FieldSize = -1 };
        //        public static readonly IFD_TYPE SLONG = new IFD_TYPE() { Id = 9, TypeName = "SLONG", FieldSize = 4 };
        //        public static readonly IFD_TYPE SRATIONAL = new IFD_TYPE() { Id = 10, TypeName = "SRATIONAL", FieldSize = 8 };
        //        public static readonly IFD_TYPE UNKNOWN = new IFD_TYPE() { Id = -1, TypeName = "", FieldSize = 0 };

        //    }

        //    //TYPE構造体
        //    public class IFD_TYPE
        //    {
        //        public int Id;
        //        public string TypeName;
        //        public int FieldSize;
        //    }

        //    public static class ExifField
        //    {


        //        public static class TIFF_Field
        //        {
        //            public static readonly IFD_FIELD H100_ImageWidth = new IFD_FIELD() { Id = 0x100, FieldName = "ImageWidth", Type = ExifType.LONG.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H101_ImageLength = new IFD_FIELD() { Id = 0x101, FieldName = "ImageLength", Type = ExifType.LONG.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H102_BitPerSample = new IFD_FIELD() { Id = 0x102, FieldName = "BitPerSample", Type = ExifType.SHORT.Id, Count = 3, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H103_Compression = new IFD_FIELD() { Id = 0x103, FieldName = "Compression", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H106_PhotometricInterpretation = new IFD_FIELD() { Id = 0x106, FieldName = "PhotometricInterpretation", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H112_Orientation = new IFD_FIELD() { Id = 0x112, FieldName = "Orientation", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H115_SamplesPerPixel = new IFD_FIELD() { Id = 0x115, FieldName = "SamplesPerPixel", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H11C_PlanarConfiguration = new IFD_FIELD() { Id = 0x11C, FieldName = "PlanarConfiguration", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H212_YCbCrSubSampling = new IFD_FIELD() { Id = 0x212, FieldName = "YCbCrSubSampling", Type = ExifType.SHORT.Id, Count = 2, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H213_YCbCrPositioning = new IFD_FIELD() { Id = 0x213, FieldName = "YCbCrPositioning", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H11A_XResolution = new IFD_FIELD() { Id = 0x11A, FieldName = "XResolution", Type = ExifType.RATIONAL.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H11B_YResolution = new IFD_FIELD() { Id = 0x11B, FieldName = "YResolution", Type = ExifType.RATIONAL.Id, Count = 1, Refs = IFD_REFS.TIFF };
        //            public static readonly IFD_FIELD H128_ResolutionUnit = new IFD_FIELD() { Id = 0x128, FieldName = "ResolutionUnit", Type = ExifType.SHORT.Id, Count = 1, Refs = IFD_REFS.TIFF };

        //            //public static readonly IFD_FIELD H115_= new IFD_FIELD() { Id = 0x1, FieldName = "", Type = ExifType.SHORT.Id, Count = 1 Refs = IFD_REFS.TIFF };

        //        }


        //        public static IFD_FIELD GetIFDField(int id)
        //        {
        //            IFD_FIELD ifd = new IFD_FIELD();
        //            Type type = typeof(TIFF_Field);

        //            foreach (System.Reflection.FieldInfo pi in type.GetFields())
        //            {
        //                Console.WriteLine("Prop:{0}", pi.Name);
        //                Console.WriteLine((int)pi.GetValue("Id"));
                        

        //            }
        //            return new IFD_FIELD();
        //        }

        //    }

        //    public class IFD_FIELD
        //    {
        //        public int Id;
        //        public string FieldName;
        //        public int Type;
        //        public int Count;
        //        public int Refs;

        //    }

        //    //REF管理用仮params
        //    public static class IFD_REFS
        //    {
        //        public static readonly int TIFF = 0;
        //        public static readonly int EXIF = 1;
        //        public static readonly int GPS = 2;
        //    }

        //    //EXIFデータ管理エレメント
        //    public class ExifElements
        //    {
        //        public IFD_TYPE IFD = new IFD_TYPE();
        //        public byte[] Contains = new byte[] { };
                

        //    }


        //    public class ExifParams
        //    {
        //        public Int16 TagId;
        //        public Int16 TagType;
        //        public byte[] TagData;

        //    }

        //    public class EXIF
        //    {
        //        public string Version { get; private set; } = string.Empty;

        //        public List<ExifElements> elems = new List<ExifElements>();

        //    }

        //    public EXIF GetExifByImage(string imagePath)
        //    {
        //        return new EXIF();
        //    }

        //    public EXIF GetExifByFile(string confPath)
        //    {
        //        return new EXIF();
        //    }

        //    public bool SetExifToImage(string imagePath)
        //    {
        //        return false;
        //    }

        //    public bool SetExifToFile(string confPath)
        //    {
        //        return false;
        //    }
        //    public bool CopyExif(string srcImg, string dstImg)
        //    {
        //        return false;
        //    }

        //    //public bool GetEnumerator(EXIF exif)
        //    //{
        //    //}
        
        //}


        public class ExifManagerDotNet: IDisposable
        {

            private List<EXIF_TAGS> TagList = new List<EXIF_TAGS>();
            
            private int JpegQuality = 100;
            /// <summary>
            /// 情報のデータ補正設定
            /// </summary>
            public bool AutoAdjust { get;  private set; } = true;

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

                    foreach(EXIF_TAGS et in TagList)
                    {
                        pp.Id = et.tag;
                        pp.Type = (short)et.type;

                        pp.Value = new byte[et.data.Length];

                        et.data.CopyTo(pp.Value, 0);
                        if (et.type == (int)ExifType.ASCII)
                        {
                            if ((et.data[et.data.Length -1] != '\0') && AutoAdjust)
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
                    foreach(System.Drawing.Imaging.ImageCodecInfo ici in System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders())
                        if(ici.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
                        {
                            jici = ici;
                            break;
                        }

                    if (jici is null)
                        return (int)ErrorNumber.ParameterNull;
                    
                    if (!filePath.Equals(dstPath)) {
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


                    using (System.IO.FileStream fs = new System.IO.FileStream(filePath + ".gz", System.IO.FileMode.Create))
                    using (System.IO.Compression.GZipStream gs = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Compress))
                        xs.Serialize(gs, TagList);


                    return (int)ErrorNumber.NoError;
                }
                catch
                {
                    return (int)ErrorNumber.Unknown;
                }
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
                    foreach(System.Drawing.Imaging.PropertyItem pi in bmp.PropertyItems)
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
                foreach(EXIF_TAGS ts in this.TagList)
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
                foreach(EXIF_TAGS te in TagList)
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //ExifManager exm = new ExifManager();
            //exm.GetExifByImage("../../img.JPG");

            string imgPath = "../../img.JPG";
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(imgPath);

            
            foreach(System.Drawing.Imaging.PropertyItem item in bmp.PropertyItems)
            {
                if(item.Id == 0x501B)
                {
                    System.Drawing.ImageConverter ic = new System.Drawing.ImageConverter();
                    System.Drawing.Image thumb = (System.Drawing.Image)ic.ConvertFrom(item.Value);

                    thumb.Save("../../test.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    continue;
                }
                if(item.Id == 0x5090)
                {
                    string val = System.Text.Encoding.ASCII.GetString(item.Value);
                    val = val.TrimEnd(new char[] { '\0' });
                    Console.WriteLine(val);

                }

                switch (item.Type)
                {
                    case 1:
                    case 3:
                    case 4:
                    case 9:
                        Console.Write("{0:X}:{1}:", item.Id, item.Type);
                        foreach (byte b in item.Value)
                            Console.Write("{0:X}", b);
                        Console.WriteLine();
                        break;
                    case 2:
                    //case 5:
                    //case 7:
                    //case 10:
                        string val = System.Text.Encoding.ASCII.GetString(item.Value);
                        val = val.Trim(new char[] { '\0' });
                        Console.WriteLine("{0:X}:{1}:{2}", item.Id, item.Type, val);
                        break;
                    default:
                        Console.WriteLine("{0:X}:{1}:{2}", item.Id, item.Type, item.Len);
                        break;


                }




            }
            bmp.Dispose();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ExifManagerDotNet emdn = new ExifManagerDotNet();
            int i = emdn.GetExifByImage("img.jpg");

            //Console.WriteLine("ErrorReason: " + emdn.GetErrorCode(i));

            //dynamic a;
            //Console.WriteLine(emdn.TryGetTag(0x110, out a));

            emdn.SaveExifToXml("test.xml");

            ExifManagerDotNet e2 = new ExifManagerDotNet();
            e2.LoadExifFromXml("test.xml");

            //emdn.SetValue(0x112, (UInt16)2);
            //emdn.SetValue(0xA20E, Tuple.Create((UInt32)10, (UInt32)20));
            //emdn.SetValue(0xA434, "Lens Pattern");

            emdn.SetValue(0x10F, "TOOL", 2);


            foreach (int j in emdn.GetTagEnumerator())
            {
                dynamic b;
                emdn.TryGetValue(j, out b);
                Console.WriteLine("{0:X} : {1}", j, b);
            }

            //emdn.SetExifToImage("../../test.jpg", "t2.jpg");
            emdn.SetExifToImage("test_br_r.jpg");
            

            //ExifManager.ExifField.GetIFDField(0);
        }
    }
}
