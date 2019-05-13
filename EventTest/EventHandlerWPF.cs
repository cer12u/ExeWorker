using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EventTest
{
    class EventHandlerWPF
    {


        public class TimerClass: IDisposable
        {

            public delegate void EventDelegate();
            public event EventDelegate EventHdr;


            System.Timers.Timer t = new System.Timers.Timer();


            public void StartTimer(uint ms)
            {

                t.Interval = ms;
                t.Elapsed += TimerEvent;
                t.Start();

            }

            private void TimerEvent(object sender, ElapsedEventArgs e)
            {
                if (EventHdr != null)
                    EventHdr();
            }

            public void StopTimer()
            {
                if(t.Enabled)
                    t.Stop();
            }

            public void Dispose()
            {
                StopTimer();
            }


        }


    }


}
