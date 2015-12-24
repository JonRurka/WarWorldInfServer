using System;
using System.Diagnostics;
namespace WarWorldInfinity
{
	public class GameTimer
	{
		private SetableStopwatch _watch;
		private int _totalSeconds;
		private int _seconds;
		private int _minutes;
		private int _hours;
		private int _days;
		private int _months;
		private int _years;
		private int _previousSecond;
		private int _secondsInTick;
		private int _maxSecondsInTick;
		private int _tick;
		private bool _tickIncrease;

		public int TotalSeconds { get { return _totalSeconds; } }
		public int Seconds { get { return _seconds;}  }
		public int Minutes { get {  return _minutes;}  }
		public int Hours { get { return _hours; } }
		public int Days { get {return _days;} }
		public int Months { get { return _months;} }
		public int Years { get { return _years;} }
		public int SecondsInTick { get { return _secondsInTick; } }
		public int MaxSecondsInTick { get { return _maxSecondsInTick; } }
		public int Tick{ get { return _tick; } }
		public bool TickIncrease{ get { return _tickIncrease; } }

		public GameTimer (World.Time offset)
		{
			_totalSeconds = offset.seconds;
			_secondsInTick = 0;
			_maxSecondsInTick = offset.maxSecondsInTicks;
			_tick = offset.tick;
			_watch = new SetableStopwatch (new TimeSpan(0, 0, 0));
			_watch.Start ();
			_previousSecond = (int)_watch.Seconds;
			Logger.Log ("Game Timer Initialized.");
		}

		public bool Update(){
			int newSecond = (int)_watch.Seconds;
			if (_previousSecond != newSecond){
				_previousSecond = newSecond;
				_seconds = newSecond;
				_totalSeconds++;
				_minutes = _totalSeconds / 60;
				_hours = _minutes / 60;
				_days = _hours / 24;
				_months = _days / 30;
				_years = _months / 12;
				if (SecondsInTick >= MaxSecondsInTick){
					_secondsInTick = 0;
					_tick++;
					_tickIncrease = true;
				}
				else{
					_secondsInTick++;
					_tickIncrease = false;
				}
				return true;
			}
			return false;
		}

		public static string GetTime(){
			DateTime time = DateTime.Now;
			return string.Format("{0:00}:{1:00}:{2:00}", time.Hour, time.Minute, time.Second);
		}

		public static string GetDateTime(){
			DateTime time = DateTime.Now;
			return time.ToString ();
		}

		public static int GetEpoch(){
			return (int)(DateTime.UtcNow - new DateTime (1970, 1, 1)).TotalMilliseconds;
		}

		public string GetRuntime ()
		{
			return string.Format ("RunTime: {0:00}:{1:00}:{2:00}:{3:00}", Days, Hours, Minutes, Seconds);
		}

		public override string ToString ()
		{
			return string.Format ("[GameTimer: Seconds={0}, Minutes={1}, Hours={2}, Days={3}, Months={4}, Years={5}]", Seconds, Minutes, Hours, Days, Months, Years);
		}
	}
}

