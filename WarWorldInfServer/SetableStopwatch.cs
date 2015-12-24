using System;
using System.Diagnostics;
namespace WarWorldInfinity
{
	public class SetableStopwatch : Stopwatch
	{
		private TimeSpan _startOffset;

		public TimeSpan StartOffset{ get{return _startOffset;} }

		public SetableStopwatch (TimeSpan startOffset)
		{
			_startOffset = startOffset;
		}

		public new long ElapsedMilliseconds
		{
			get 
			{
				return base.ElapsedMilliseconds + (long)_startOffset.TotalMilliseconds;
			}
		}

		public new long ElapsedTicks
		{
			get
			{
				return base.ElapsedTicks + StartOffset.Ticks;
			}
		}

		public long Seconds
		{
			get
			{
				return base.Elapsed.Seconds + StartOffset.Seconds;
			}
		}

		public long Minutes
		{
			get
			{
				return base.Elapsed.Minutes + StartOffset.Minutes;
			}
		}

		public long Hours
		{
			get
			{
				return base.Elapsed.Hours + StartOffset.Hours;
			}
		}

		public long Days
		{
			get
			{
				return base.Elapsed.Days + StartOffset.Days;
			}
		}
	}
}

