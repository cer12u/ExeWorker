using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusManager
{
    class Status
    {
    }


    public enum StatusMode
    {
        Bitmap,
        AutomationId,

    }


    public class Manager
    {
        public List<SeqItem> SeqItems = new List<SeqItem>();

        public class SeqItem
        {

        }

    }

    public class BitmapManager: Manager
    {

    }

    public class AutomationIdManager: Manager
    {

    }

}
