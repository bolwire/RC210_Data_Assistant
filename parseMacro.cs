namespace RC210_DataAssistant_V2
{
	partial class Form1
	{
		public string ParseAlarm(string pAlarm)
		{
			pAlarm = pAlarm.Replace("0", "Disabled");
			pAlarm = pAlarm.Replace("1", "Enabled");

			return pAlarm;
		}

		public string ParseMeterAlarmType(string pmType)
		{
			pmType = pmType.Replace("1", "Low Alarm");
			pmType = pmType.Replace("2", "High Alarm");
			return pmType;
		}

		public string ParseMeter(string pmeter)
		{
			pmeter = pmeter.Replace("1", "Volts");
			pmeter = pmeter.Replace("2", "Amps");
			pmeter = pmeter.Replace("3", "Watts");
			pmeter = pmeter.Replace("4", "Degrees");
			pmeter = pmeter.Replace("5", "MPH");
			pmeter = pmeter.Replace("6", "Percent");
			return pmeter;
		}

		public string ParseMonthly(string pmonthly)
		{
			pmonthly = pmonthly.Replace("0", "No");
			pmonthly = pmonthly.Replace("1", "Yes");
			return pmonthly;
		}

		public string ParseDow(string pdow)
		{
			pdow = pdow.Replace("0", "Every Day");
			pdow = pdow.Replace("1", "Monday");
			pdow = pdow.Replace("2", "Tuesday");
			pdow = pdow.Replace("3", "Wednesday");
			pdow = pdow.Replace("4", "Thursday");
			pdow = pdow.Replace("5", "Friday");
			pdow = pdow.Replace("6", "Saturday");
			pdow = pdow.Replace("7", "Sunday");
			pdow = pdow.Replace("8", "Week Days");
			pdow = pdow.Replace("9", "Week Ends");
			return pdow;
		}

		public string ParseHours(string phours)
		{
			phours = phours.Replace("0A", "Every Hour");
			phours = phours.Replace("25", "Disabled");
			return phours;
		}

		public string ParseRemoteOffset(string offset)
		{
			offset = offset.Replace("1", "-");
			offset = offset.Replace("2", "Simplex");
			offset = offset.Replace("3", "+");
			return offset;
		}

		public string ParseCtcssMode(string mode)
		{
			mode = mode.Replace("0", "NO TONE");
			mode = mode.Replace("1", "Encode Only");
			mode = mode.Replace("2", "Encode/Decode");
			return mode;
		}
		
		public string ParseRadioMode(string mode)
		{
			mode = mode.Replace("1", "LSB");
			mode = mode.Replace("2", "USB");
			mode = mode.Replace("3", "CW");
			mode = mode.Replace("4", "FM");
			mode = mode.Replace("5", "AM");
			return mode;
		}
	}
}
