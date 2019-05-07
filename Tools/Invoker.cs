using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Tools
{

    /// <summary>
    /// EXEファイルの実行管理用class
    /// </summary>
    public class Invoker : IDisposable
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
            using (System.IO.StreamReader sr = Procs.StandardOutput)
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
            if (Procs != null)
            {
                if (!Procs.HasExited)
                    Procs.Kill();
                Procs.Dispose();
            }
            resultLines.Clear();
            Procs = null;
        }
    }
}
