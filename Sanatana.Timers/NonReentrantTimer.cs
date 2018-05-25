using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.Timers
{
    public class NonReentrantTimer : IDisposable
    {
        //constants
        protected static TimeSpan DISABLED_TIMESPAN = TimeSpan.FromMilliseconds(-1);


        //fields
        protected Timer _timer;
        protected DateTime? _lastCallbackStartedUtc;
        protected TimeSpan _callbackInterval;
        protected Func<bool> _timerCallback;
        private long _isStarted;
        private volatile bool _isProcessingCallback;


        //properties
        public virtual TimeSpan CallbackInterval
        {
            get
            {
                return _callbackInterval;
            }
            set 
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException("Timer interval can not be less then zero");
                }

                _callbackInterval = value; 
            }
        }
        /// <summary>
        /// Start counting interval time from the moment of previous Callback start. Otherwise will start interval count from callback finish time.
        /// </summary>
        public virtual bool IntervalFromCallbackStarted { get; set; }
        public virtual bool IsStarted
        {
            get
            {
                return Interlocked.Read(ref _isStarted) == 1;
            }
            protected set
            {
                long longValue = value ? 1 : 0;
                Interlocked.Exchange(ref _isStarted, longValue);
            }
        }
        public virtual bool IsProcessingCallback
        {
            get
            {
                return _isProcessingCallback;
            }
            protected set
            {
                _isProcessingCallback = value;
            }
        }


        //init
        protected NonReentrantTimer()
        {
            _timer = new Timer(CallBack);
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        public NonReentrantTimer(Func<bool> timerCallback, TimeSpan callbackInterval
            , bool intervalFromCallbackStarted)
            : this()
        {
            _timerCallback = timerCallback;
            CallbackInterval = callbackInterval;
            IntervalFromCallbackStarted = intervalFromCallbackStarted;
        }
        public NonReentrantTimer(Action timerCallback, TimeSpan callbackInterval
            , bool intervalFromCallbackStarted)
            : this()
        {
            _timerCallback = () => {
                timerCallback();
                return IsStarted;
            };
            CallbackInterval = callbackInterval;
            IntervalFromCallbackStarted = intervalFromCallbackStarted;
        }



        //methods
        protected virtual void CallBack(object state)
        {
            _lastCallbackStartedUtc = DateTime.UtcNow;

            IsProcessingCallback = true;
            IsStarted = _timerCallback();
            IsProcessingCallback = false;

            if (IsStarted)
            {
                TimeSpan nextCallbackInterval = GetInterval();

                try
                {
                    _timer.Change(nextCallbackInterval, DISABLED_TIMESPAN);
                }
                catch
                {
                }
            }
        }
       
        /// <summary>
        /// Start timer.
        /// </summary>     
        public virtual void Start()
        {
            if (IsStarted)
            {
                return;
            }

            TimeSpan nextCallbackInterval = GetInterval();
            Start(nextCallbackInterval);
        }
        
        /// <summary>
        /// Start timer with initial due time.
        /// </summary>
        /// <param name="dueTime">Duetime before first start</param>
        public virtual void Start(TimeSpan dueTime)
        {
            if (IsStarted)
            {
                return;
            }

            IsStarted = true;

            try
            {
                _timer.Change(dueTime, DISABLED_TIMESPAN);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Stop
        /// </summary>
        public virtual void Stop()
        {
            IsStarted = false;

            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch
            {
            }
        }
        
        protected virtual TimeSpan GetInterval()
        {
            if (IntervalFromCallbackStarted)
            {
                TimeSpan lastSendInterval = DateTime.UtcNow - (_lastCallbackStartedUtc ?? DateTime.MinValue);
                TimeSpan nextCallbackInterval;

                if (lastSendInterval > _callbackInterval)
                    nextCallbackInterval = TimeSpan.Zero;
                else
                    nextCallbackInterval = _callbackInterval - lastSendInterval;

                return nextCallbackInterval;
            }
            else
            {
                return _callbackInterval;
            }
        }


        //IDisposable
        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
