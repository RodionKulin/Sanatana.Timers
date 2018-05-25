using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Timers
{
    public class NonReentrantResumableTimer : NonReentrantTimer
    {
        //fields
        protected PersistFile _lastCallbackFile;



        //init
        public NonReentrantResumableTimer(FileInfo lastCallbackFile
            , Func<bool> timerCallback, TimeSpan callbackInterval, bool intervalFromCallbackStarted)
            : base(timerCallback, callbackInterval, intervalFromCallbackStarted)
        {
            _lastCallbackFile = new PersistFile(lastCallbackFile);
        }

        

        //exxecution methods
        public override void Start()
        {
            TimeSpan dueTime = GetDueTime();
            Start(dueTime);
        }

        protected override void CallBack(object state)
        {
            WriteCallbackTimeUtc(DateTime.UtcNow);

            base.CallBack(state);
        }



        //last timer execution time
        public virtual TimeSpan GetDueTime()
        {
            TimeSpan dueTime;
            DateTime? lastCallbackTime = GetLastCallbackTimeUtc();

            //if first time than launch immediatly
            if (lastCallbackTime == null)
            {
                dueTime = TimeSpan.Zero;
            }
            else
            {
                TimeSpan timeFromLastCallback = DateTime.UtcNow - lastCallbackTime.Value;
                bool moreThenInterval = timeFromLastCallback >= CallbackInterval;

                //time from last execution is greater than interval
                if (moreThenInterval)
                {
                    dueTime = TimeSpan.Zero;
                }
                else
                {
                    dueTime = CallbackInterval - timeFromLastCallback;
                }
            }

            return dueTime;
        }

        public virtual DateTime? GetLastCallbackTimeUtc()
        {
            if(_lastCallbackStartedUtc != null)
            {
                return _lastCallbackStartedUtc;
            }
            
            string line = _lastCallbackFile.ReadLine();
            return ParseCallbackTime(line);
        }

        protected virtual DateTime? ParseCallbackTime(string line)
        {
            DateTime lastTimeUtc;
            bool parsed = DateTime.TryParse(line
                , CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out lastTimeUtc);

            if (parsed == false)
            {
                return null;
            }

            return DateTime.SpecifyKind(lastTimeUtc, DateTimeKind.Utc);
        }

        public virtual void WriteCallbackTimeUtc(DateTime callbackTime)
        {
            string date = callbackTime.ToString(CultureInfo.InvariantCulture);
            _lastCallbackFile.WriteLine(date);
        }
    }
}
