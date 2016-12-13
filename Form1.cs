using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ini;
using RC210_DataAssistant_V2.Properties;

namespace RC210_DataAssistant_V2
{
	public partial class Form1 : Form
	{
		#region Classes

		private class AutoPatch
		{
			public string Prefix;
			public string AnswerCode;
			public readonly Dictionary<int, string> AutoDial = new Dictionary<int, string>();
			public readonly Dictionary<int, string>  TollRestriction = new Dictionary<int, string>();
			public string RingCount;
			public string TimeOut;
			public string Ports;
			public string PatchMute;
		}

		private class Port
		{
			public string PortName;
			public string PortCode;
			public string HangTimer1;
			public string HangTimer2;
			public string HangTimer3;
			public string TimeoutTimer;
			public string InitialIdTimer;
			public string PendingIdTimer;
			public string InactivityTimer;
			public string InactivityTimerMacro;
			public string DtmfMuteTimer;
			public string EncoderTimer;
			public string KerchunkTimer;
			public string PendingIdSpeechTimer;
			public string TailMessageTimer;
			public string AuxAudioTimer;
			public string VoiceId1;
			public string VoiceId2;
			public string VoiceId3;
			public string Cwid1;
			public string Cwid2;
			public string TailMessage;
			public string TailMessageCounter;
			public string TailMessage1Macro;
			public string TailMessage2Macro;
			public string TailMessage3Macro;

			//Switches
			public string TransmitterEnable;
			public string ReceiverEnable;
			public string DisableInactivityTimer;
			public string RepeaterMode;
			public string SpeechOverride;
			public string SpeechIdOverride;
			//public string AccessMode;
			public string DtmfEnable;
			public string DtmfRequireTone;
			public string DtmfMute;
			public string DtmfCoverTone;
			public string MonitorMix;
			public string KerchunkFilter;
		}

		/// <summary>
		/// Class to hold Macro/ShortMacro items
		/// </summary>
		private class Macro
		{
			public int MacroNumber;
			public string AccessCode;
			public string MacroCodes;
			public string ParsedMacro;
			public string AllowedPorts;
		}

		#endregion Classes

		#region Private Members

		private readonly XmlDocument _localXmlDocument = new XmlDocument();
		private readonly XmlDocument _localXmlDocumentOverRide = new XmlDocument();
		private readonly XmlDocument _remoteXmlDocument = new XmlDocument();

		private readonly string _localXmlDocumentFile = AppDomain.CurrentDomain.BaseDirectory + "macro_def.xml";
		private readonly string _localXmlDocumentOverRideFile = AppDomain.CurrentDomain.BaseDirectory + "macro_def_override.xml";
		
		private double _remoteXmlVersion;
		private double _localXmlVersion;

		private readonly MacroLibrary _macroLibrary = new MacroLibrary();

		private string _datFilename;
		private string _reportFilename;

		private readonly AutoPatch _autoPatch = new AutoPatch();
		private readonly Dictionary<int, Port> _ports = new Dictionary<int, Port>();
		private readonly Dictionary<int, Macro> _macros = new Dictionary<int, Macro>();
		private readonly Dictionary<int, Macro> _shortMacros = new Dictionary<int, Macro>();
		private readonly Dictionary<int, string> _messageMacros = new Dictionary<int, string>();

		#endregion Private Members

		#region Initialization

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			FetchDefXml();

			comboBox_FwVersion.Items.Clear();

			foreach (KeyValuePair<string, Dictionary<string, string>> macroLibrary in _macroLibrary.GetLibraries)
			{
				comboBox_FwVersion.Items.Add(macroLibrary.Key);
			}

			comboBox_FwVersion.SelectedIndex = 0;

			if (Settings.Default.Port1Name != string.Empty)
				groupBox_Port1.Text = @"Port 1 - " + Settings.Default.Port1Name;
			if (Settings.Default.Port2Name != string.Empty)
				groupBox_Port2.Text = @"Port 2 - " + Settings.Default.Port2Name;
			if (Settings.Default.Port3Name != string.Empty)
				groupBox_Port3.Text = @"Port 3 - " + Settings.Default.Port3Name;
		}

		#endregion Initialization

		#region Private Methods

		#region XML Methods

		private void UpdateXml()
		{
			try
			{
				_remoteXmlDocument.Load("http://www.kc5tdg.com/macro_def.xml?" + DateTime.Now);
				var remoteXmlElement = _remoteXmlDocument["macros"];
				if (remoteXmlElement != null)
				{
					_remoteXmlVersion = Convert.ToDouble(remoteXmlElement.Attributes["version"].Value);
				}

				try
				{
					_localXmlDocument.Load(_localXmlDocumentFile);

					var localXmlElement = _localXmlDocument["macros"];
					if (localXmlElement != null)
					{
						_localXmlVersion = Convert.ToDouble(localXmlElement.Attributes["version"].Value);

					}
					else
					{
						try
						{
							_remoteXmlDocument.Save(_localXmlDocumentFile);
							MessageBox.Show(
								string.Format("There was a problem with your Macro definition file, it has been restored to version: {0}",
									_remoteXmlVersion), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						catch (Exception e)
						{
							MessageBox.Show(
								string.Format(
									"There was a problem with your Macro definition file, there was also an error restoring the file. Please consult the support group for futher help.\n\nError:\n\n{0}",
									e.Message), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}

					}

					if (_remoteXmlVersion > _localXmlVersion)
					{

						try
						{
							_remoteXmlDocument.Save(_localXmlDocumentFile);

							MessageBox.Show(
								string.Format("Your Macro definition file has been upgraded to version: {0}", _remoteXmlVersion),
								@"New Definitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						catch (Exception e)
						{
							MessageBox.Show(
								string.Format(
									"There was an error upgrading your Macro definition file to version: {0}\n\nError: {1}\n\nPlease consult the support group for further help.",
									_remoteXmlVersion, e.Message), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
							return;
						}
					}

					if (_remoteXmlVersion == _localXmlVersion)
					{
						MessageBox.Show(
							string.Format(
								"Your Macro definition file is already up to date.\n\n" + "Remote version: {0}\nLocal version: {1}",
								_remoteXmlVersion, _localXmlVersion), @"Macro Definitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}

					FetchDefXml();
				}

				catch (Exception)
				{
					_remoteXmlDocument.Save(_localXmlDocumentFile);
					MessageBox.Show(
						string.Format(
							"There was an error locating your local Macro definition file, it has been restored to version: {0}",
							_remoteXmlVersion),
						@"New Definitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				MessageBox.Show(
					string.Format(
						"There was a problem fetching the remote XML file, please consult the support group for further help.\n\nError Message:\n{0}",
						e.Message), @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

		}

		
		private void FetchDefXml()
		{
			string filepath = AppDomain.CurrentDomain.BaseDirectory + "xml\\";
			DirectoryInfo d = new DirectoryInfo(filepath);

			foreach (var file in d.GetFiles("*.xml"))
			{
				LoadXMLFile(filepath + file.Name);
			}
		}

		private void LoadXMLFile(string file)
		{
			if (File.Exists(file))
			{
				_localXmlDocument.Load(file);

				var localXmlElement = _localXmlDocument["macros"];
				if (localXmlElement != null)
				{
					_localXmlVersion = Convert.ToDouble(localXmlElement.Attributes["version"].Value);

				}
			}
			else
			{
				UpdateXml();
			}

			if (!File.Exists(file))
			{
				MessageBox.Show(
					@"Something has gone terribly wrong, unable to locate an xml file for Macro definitions, please consult the support group for help.",
					@"Unrecoverable Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

				return;
			}

			//Load the regulat macro definitions
			_localXmlDocument.Load(file);

			foreach (XmlNode node in _localXmlDocument.ChildNodes)
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					if (childNode.Attributes != null)
						_macroLibrary.AddMacro(childNode.Attributes["versions"].Value, childNode.Attributes["id"].Value, childNode.Attributes["description"].Value);
				}
			}

			//Load the custom over ride definitions, if any exist
			if (File.Exists(_localXmlDocumentOverRideFile))
			{
				_localXmlDocumentOverRide.Load(_localXmlDocumentOverRideFile);

				foreach (XmlNode node in _localXmlDocumentOverRide.ChildNodes)
				{
					foreach (XmlNode childNode in node.ChildNodes)
					{
						if (childNode.Attributes != null)
							_macroLibrary.AddMacro(childNode.Attributes["versions"].Value, childNode.Attributes["id"].Value, childNode.Attributes["description"].Value, true);
					}
				}
			}
		}

		#endregion XML Methods

		#region Load .dat file

		private void Load_datFile()
		{
			if (_datFilename == string.Empty || !File.Exists(_datFilename))
				return;
			Load_MessageMacros();
			Load_Macros();
			Load_ShortMacros();
			Load_AutoPatch();
		}
		
		#endregion Load .dat file

		#region Generate Report

		private void Generate_Report()
		{
			double fwVersion = Convert.ToDouble(comboBox_FwVersion.SelectedItem);
			Load_Ports(fwVersion);

			if (!File.Exists(_datFilename))
			{
				MessageBox.Show(@"Unable to locate .dat file: " + _datFilename);
				return;
			}

			SaveFileDialog result = new SaveFileDialog
			{
				Filter = @"HTML File (*.html)|*.html",
				InitialDirectory = Path.GetDirectoryName(_reportFilename)
			};

			if (result.ShowDialog() != DialogResult.OK)
				return;

			_reportFilename = result.FileName;

			//Load the dat file
			Load_datFile();

			IniFile datFile = new IniFile(_datFilename);

			//Create the file. 
			using (StreamWriter rW = new StreamWriter(_reportFilename))
			{
				//Date Header
				if (checkBox_DateHeader.Checked)
					rW.WriteLine("<CENTER><H2>" + DateTime.Now + "</H2></CENTER>");

				//Custom Header
				if (checkBox_CustomHeader.Checked)
					rW.WriteLine("<CENTER><H2>" + textBox_CustomHeader.Text + "</H2></CENTER><HR>");

				//PreAccess Prefix
				if (checkBox_PreAccessPrefix.Checked)
					rW.WriteLine("<CENTER><B>PreAccess Prefix:</B> " + datFile.IniReadValue("PreAccess", "PreAccessPrefix") + "</CENTER>");

				//Global Lock Code
				if (checkBox_GlobalLockCode.Checked)
					rW.WriteLine("<CENTER><B>Global Lock Code:</B> " + datFile.IniReadValue("Unlock", "Lock") + "</CENTER>");

				//Terminator Digit
				if (checkBox_TerminatorDigit.Checked)
					rW.WriteLine("<CENTER><B>Terminator Digit:</B> " + datFile.IniReadValue("Unlock", "Terminator") + "</CENTER>");

				//DTMF Test pad code
				if (checkBox_DTMFTestPadCode.Checked)
					rW.WriteLine("<CENTER><B>DTMF Pad Test:</B> " + datFile.IniReadValue("PadTest", "DTMFTestPrefix") + "</CENTER>");

				//Add the <HR> tag if needed
				if (checkBox_PreAccessPrefix.Checked || checkBox_GlobalLockCode.Checked || checkBox_TerminatorDigit.Checked || checkBox_DTMFTestPadCode.Checked)
					rW.WriteLine("<CENTER><HR></CENTER>");

				//Port1
				if (checkBox_P1UnlockCode.Checked || checkBox_P1IDs.Checked || checkBox_P1TailMessages.Checked ||
				    checkBox_P1Timers.Checked)
				{
					string port1Header = _ports[1].PortName != string.Empty ? "Port 1 - " + _ports[1].PortName : "Port 1";
					rW.WriteLine("<CENTER><TABLE border=1 cellpadding=5>");
					rW.WriteLine("<TR><TD align=center colspan=6><H2>" + port1Header + "</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Unlock Code</B></TD><TD><B>Voice IDs</B></TD><TD><B>CW IDs</B></TD><TD><B>Tail Messages</B></TD><TD><B>Timers</B></TD><TD><B>Switches</B></TD></TR>");
					rW.WriteLine("<TR>");

					//Unlock Code
					string port1UnlockCode = checkBox_P1UnlockCode.Checked ? _ports[1].PortCode : "Not Displayed";
					rW.WriteLine("<TD valign=top>" + port1UnlockCode + "</TD>");

					//Voice IDs
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P1IDs.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Voice ID 1:</B></TD><TD>" + _ports[1].VoiceId1 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Voice ID 2:</B></TD><TD>" + _ports[1].VoiceId2 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Voice ID 3:</B></TD><TD>" + _ports[1].VoiceId3 + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//CW IDs
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P1IDs.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>CW ID 1:</B></TD><TD>" + _ports[1].Cwid1 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>CW ID 2:</B></TD><TD>" + _ports[1].Cwid2 + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//Tail Messages
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P1TailMessages.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Message:</B></TD><TD>" + ((_ports[1].TailMessage == "0") ? "Disabled" : _ports[1].TailMessage) + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Counter:</B></TD><TD>" + _ports[1].TailMessageCounter + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Timer:</B></TD><TD>" + _ports[1].TailMessageTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 1:</B></TD><TD>" + _ports[1].TailMessage1Macro + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 2:</B></TD><TD>" + _ports[1].TailMessage2Macro + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 3:</B></TD><TD>" + _ports[1].TailMessage3Macro + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//Timers
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P1Timers.Checked)
					{
						rW.WriteLine("<TABLE border=0>");

						if (fwVersion < 7.02)
						{
							rW.WriteLine("<TR><TD><B>Hang:</B></TD><TD>" + _ports[1].HangTimer1 + "</TD></TR>");
						}
						else
						{
							rW.WriteLine("<TR><TD><B>Hang 1:</B></TD><TD>" + _ports[1].HangTimer1 + "</TD></TR>");
							rW.WriteLine("<TR><TD><B>Hang 2:</B></TD><TD>" + _ports[1].HangTimer2 + "</TD></TR>");
							rW.WriteLine("<TR><TD><B>Hang 3:</B></TD><TD>" + _ports[1].HangTimer3 + "</TD></TR>");
						}

						rW.WriteLine("<TR><TD><B>Timeout:</B></TD><TD>" + _ports[1].TimeoutTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Initial ID:</B></TD><TD>" + _ports[1].InitialIdTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Pending ID:</B></TD><TD>" + _ports[1].PendingIdTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Pending Speech ID:</B></TD><TD>" + _ports[1].PendingIdSpeechTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Inactivity (Macro):</B></TD><TD>" + _ports[1].InactivityTimer + " (" + _ports[1].InactivityTimerMacro + ")</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Mute:</B></TD><TD>" + _ports[1].DtmfMuteTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Encoder:</B></TD><TD>" + _ports[1].EncoderTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Kerchunck:</B></TD><TD>" + _ports[1].KerchunkTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>AUX Audio:</B></TD><TD>" + _ports[1].AuxAudioTimer + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}
					
					rW.WriteLine("</TD><TD>");

					//Switches
					if (checkBox_P1Switches.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Transmitter:</B></TD><TD>" + _ports[1].TransmitterEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Receiver:</B></TD><TD>" + _ports[1].ReceiverEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Disable Inactivity Timer:</B></TD><TD>" + _ports[1].DisableInactivityTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Repeater Mode:</B></TD><TD>" + _ports[1].RepeaterMode + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Speech Override:</B></TD><TD>" + _ports[1].SpeechOverride + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Speech ID Override:</B></TD><TD>" + _ports[1].SpeechIdOverride + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Enable:</B></TD><TD>" + _ports[1].DtmfEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Mute:</B></TD><TD>" + _ports[1].DtmfMute + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Require Tone:</B></TD><TD>" + _ports[1].DtmfRequireTone + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Covertone:</B></TD><TD>" + _ports[1].DtmfCoverTone + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Monitor Mix:</B></TD><TD>" + _ports[1].MonitorMix + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Kerchunk Filter:</B></TD><TD>" + _ports[1].KerchunkFilter + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					rW.WriteLine("</TR></TABLE><HR>");
				}

				//Port2
				if (checkBox_P2UnlockCode.Checked || checkBox_P2IDs.Checked || checkBox_P2TailMessages.Checked ||
					checkBox_P2Timers.Checked)
				{
					string port2Header = _ports[2].PortName != string.Empty ? "Port 2 - " + _ports[2].PortName : "Port 2";
					rW.WriteLine("<CENTER><TABLE border=1 cellpadding=5>");
					rW.WriteLine("<TR><TD align=center colspan=6><H2>" + port2Header + "</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Unlock Code</B></TD><TD><B>Voice IDs</B></TD><TD><B>CW IDs</B></TD><TD><B>Tail Messages</B></TD><TD><B>Timers</B></TD><TD><B>Switches</B></TD></TR>");
					rW.WriteLine("<TR>");

					//Unlock Code
					string port2UnlockCode = checkBox_P2UnlockCode.Checked ? _ports[2].PortCode : "Not Displayed";
					rW.WriteLine("<TD valign=top>" + port2UnlockCode + "</TD>");

					//Voice IDs
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P2IDs.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Voice ID 1:</B></TD><TD>" + _ports[2].VoiceId1 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Voice ID 2:</B></TD><TD>" + _ports[2].VoiceId2 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Voice ID 3:</B></TD><TD>" + _ports[2].VoiceId3 + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//CW IDs
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P2IDs.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>CW ID 1:</B></TD><TD>" + _ports[2].Cwid1 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>CW ID 2:</B></TD><TD>" + _ports[2].Cwid2 + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//Tail Messages
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P2TailMessages.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Message:</B></TD><TD>" + ((_ports[2].TailMessage == "0") ? "Disabled" : _ports[2].TailMessage) + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Counter:</B></TD><TD>" + _ports[2].TailMessageCounter + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Timer:</B></TD><TD>" + _ports[2].TailMessageTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 1:</B></TD><TD>" + _ports[2].TailMessage1Macro + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 2:</B></TD><TD>" + _ports[2].TailMessage2Macro + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 3:</B></TD><TD>" + _ports[2].TailMessage3Macro + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//Timers
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P2Timers.Checked)
					{
						rW.WriteLine("<TABLE border=0>");

						if (fwVersion < 7.02)
						{
							rW.WriteLine("<TR><TD><B>Hang:</B></TD><TD>" + _ports[2].HangTimer1 + "</TD></TR>");
						}
						else
						{
							rW.WriteLine("<TR><TD><B>Hang 1:</B></TD><TD>" + _ports[2].HangTimer1 + "</TD></TR>");
							rW.WriteLine("<TR><TD><B>Hang 2:</B></TD><TD>" + _ports[2].HangTimer2 + "</TD></TR>");
							rW.WriteLine("<TR><TD><B>Hang 3:</B></TD><TD>" + _ports[2].HangTimer3 + "</TD></TR>");
						}

						rW.WriteLine("<TR><TD><B>Timeout:</B></TD><TD>" + _ports[2].TimeoutTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Initial ID:</B></TD><TD>" + _ports[2].InitialIdTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Pending ID:</B></TD><TD>" + _ports[2].PendingIdTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Pending Speech ID:</B></TD><TD>" + _ports[2].PendingIdSpeechTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Inactivity (Macro):</B></TD><TD>" + _ports[2].InactivityTimer + " (" + _ports[2].InactivityTimerMacro + ")</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Mute:</B></TD><TD>" + _ports[2].DtmfMuteTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Encoder:</B></TD><TD>" + _ports[2].EncoderTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Kerchunck:</B></TD><TD>" + _ports[2].KerchunkTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>AUX Audio:</B></TD><TD>" + _ports[2].AuxAudioTimer + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD><TD>");

					//Switches
					if (checkBox_P1Switches.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Transmitter:</B></TD><TD>" + _ports[2].TransmitterEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Receiver:</B></TD><TD>" + _ports[2].ReceiverEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Disable Inactivity Timer:</B></TD><TD>" + _ports[2].DisableInactivityTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Repeater Mode:</B></TD><TD>" + _ports[2].RepeaterMode + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Speech Override:</B></TD><TD>" + _ports[2].SpeechOverride + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Speech ID Override:</B></TD><TD>" + _ports[2].SpeechIdOverride + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Enable:</B></TD><TD>" + _ports[2].DtmfEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Mute:</B></TD><TD>" + _ports[2].DtmfMute + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Require Tone:</B></TD><TD>" + _ports[2].DtmfRequireTone + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Covertone:</B></TD><TD>" + _ports[2].DtmfCoverTone + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Monitor Mix:</B></TD><TD>" + _ports[2].MonitorMix + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Kerchunk Filter:</B></TD><TD>" + _ports[2].KerchunkFilter + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					rW.WriteLine("</TR></TABLE><HR>");
				}

				//Port3
				if (checkBox_P3UnlockCode.Checked || checkBox_P3IDs.Checked || checkBox_P3TailMessages.Checked ||
				    checkBox_P3Timers.Checked)
				{
					string port3Header = _ports[3].PortName != string.Empty ? "Port 3 - " + _ports[3].PortName : "Port 3";
					rW.WriteLine("<CENTER><TABLE border=1 cellpadding=5>");
					rW.WriteLine("<TR><TD align=center colspan=6><H2>" + port3Header + "</H2></TD></TR>");
					rW.WriteLine(
						"<TR><TD><B>Unlock Code</B></TD><TD><B>Voice IDs</B></TD><TD><B>CW IDs</B></TD><TD><B>Tail Messages</B></TD><TD><B>Timers</B></TD><TD><B>Switches</B></TD></TR>");
					rW.WriteLine("<TR>");

					//Unlock Code
					string port3UnlockCode = checkBox_P3UnlockCode.Checked ? _ports[3].PortCode : "Not Displayed";
					rW.WriteLine("<TD valign=top>" + port3UnlockCode + "</TD>");

					//Voice IDs
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P3IDs.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Voice ID 1:</B></TD><TD>" + _ports[3].VoiceId1 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Voice ID 2:</B></TD><TD>" + _ports[3].VoiceId2 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Voice ID 3:</B></TD><TD>" + _ports[3].VoiceId3 + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//CW IDs
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P3IDs.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>CW ID 1:</B></TD><TD>" + _ports[3].Cwid1 + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>CW ID 2:</B></TD><TD>" + _ports[3].Cwid2 + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//Tail Messages
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P3TailMessages.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Message:</B></TD><TD>" +
						             ((_ports[3].TailMessage == "0") ? "Disabled" : _ports[3].TailMessage) + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Counter:</B></TD><TD>" + _ports[3].TailMessageCounter + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Timer:</B></TD><TD>" + _ports[3].TailMessageTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 1:</B></TD><TD>" + _ports[3].TailMessage1Macro + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 2:</B></TD><TD>" + _ports[3].TailMessage2Macro + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Message 3:</B></TD><TD>" + _ports[3].TailMessage3Macro + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					//Timers
					rW.WriteLine("<TD valign=top>");

					if (checkBox_P3Timers.Checked)
					{
						rW.WriteLine("<TABLE border=0>");

						if (fwVersion < 7.02)
						{
							rW.WriteLine("<TR><TD><B>Hang:</B></TD><TD>" + _ports[3].HangTimer1 + "</TD></TR>");
						}
						else
						{
							rW.WriteLine("<TR><TD><B>Hang 1:</B></TD><TD>" + _ports[3].HangTimer1 + "</TD></TR>");
							rW.WriteLine("<TR><TD><B>Hang 2:</B></TD><TD>" + _ports[3].HangTimer2 + "</TD></TR>");
							rW.WriteLine("<TR><TD><B>Hang 3:</B></TD><TD>" + _ports[3].HangTimer3 + "</TD></TR>");
						}

						rW.WriteLine("<TR><TD><B>Timeout:</B></TD><TD>" + _ports[3].TimeoutTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Initial ID:</B></TD><TD>" + _ports[3].InitialIdTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Pending ID:</B></TD><TD>" + _ports[3].PendingIdTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Pending Speech ID:</B></TD><TD>" + _ports[3].PendingIdSpeechTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Inactivity (Macro):</B></TD><TD>" + _ports[3].InactivityTimer + " (" +
						             _ports[3].InactivityTimerMacro + ")</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Mute:</B></TD><TD>" + _ports[3].DtmfMuteTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Encoder:</B></TD><TD>" + _ports[3].EncoderTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Kerchunck:</B></TD><TD>" + _ports[3].KerchunkTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>AUX Audio:</B></TD><TD>" + _ports[3].AuxAudioTimer + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD><TD>");

					//Switches
					if (checkBox_P1Switches.Checked)
					{
						rW.WriteLine("<TABLE border=0>");
						rW.WriteLine("<TR><TD><B>Transmitter:</B></TD><TD>" + _ports[3].TransmitterEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Receiver:</B></TD><TD>" + _ports[3].ReceiverEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Disable Inactivity Timer:</B></TD><TD>" + _ports[3].DisableInactivityTimer + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Repeater Mode:</B></TD><TD>" + _ports[3].RepeaterMode + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Speech Override:</B></TD><TD>" + _ports[3].SpeechOverride + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Speech ID Override:</B></TD><TD>" + _ports[3].SpeechIdOverride + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Enable:</B></TD><TD>" + _ports[3].DtmfEnable + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Mute:</B></TD><TD>" + _ports[3].DtmfMute + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Require Tone:</B></TD><TD>" + _ports[3].DtmfRequireTone + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>DTMF Covertone:</B></TD><TD>" + _ports[3].DtmfCoverTone + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Monitor Mix:</B></TD><TD>" + _ports[3].MonitorMix + "</TD></TR>");
						rW.WriteLine("<TR><TD><B>Kerchunk Filter:</B></TD><TD>" + _ports[3].KerchunkFilter + "</TD></TR>");
						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD>");

					rW.WriteLine("</TR></TABLE><HR>");
				}

				//Macros
				if (checkBox_Macros.Checked || checkBox_MacroCodes.Checked)
				{
					rW.WriteLine("<TABLE align=center border=1 width=75%>");
					rW.WriteLine("<TR><TD align=center colspan=4><H2>Macros</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Macro #</B></TD><TD><B>Code</B></TD><TD><B>Ports allowed</B></TD><TD><B>Actions</B></TD></TR>");

					foreach (KeyValuePair<int, Macro> macro in _macros)
					{
						rW.WriteLine("<TR><TD valign=top>" + macro.Value.MacroNumber + "</TD>");
						rW.WriteLine("<TD valign=top>" + macro.Value.AccessCode + "</TD>");
						rW.WriteLine("<TD valign=top>" + macro.Value.AllowedPorts + "</TD>");
						//rW.WriteLine("<TD>" + ((checkBox_MacroCodes.Checked) ? macro.Value.MacroCodes : macro.Value.ParsedMacro)+ "</TD></TR>");
						rW.WriteLine("<TD>");
						rW.WriteLine(((checkBox_Macros.Checked) ? macro.Value.ParsedMacro : ""));
						rW.WriteLine((checkBox_Macros.Checked && checkBox_MacroCodes.Checked) ? "<HR>" : "");
						rW.WriteLine(((checkBox_MacroCodes.Checked) ? macro.Value.MacroCodes : ""));
						rW.WriteLine("</TD></TR>");
					}


					//ShortMacros

					foreach (KeyValuePair<int, Macro> macro in _shortMacros)
					{
						rW.WriteLine("<TR><TD valign=top>" + (macro.Key + 40).ToString(CultureInfo.InvariantCulture) + "</TD>");
						rW.WriteLine("<TD valign=top>" + macro.Value.AccessCode + "</TD>");
						rW.WriteLine("<TD valign=top>" + macro.Value.AllowedPorts + "</TD>");
						//rW.WriteLine("<TD>" + ((checkBox_MacroCodes.Checked) ? macro.Value.MacroCodes : macro.Value.ParsedMacro) + "</TD></TR>");
						rW.WriteLine("<TD>");
						rW.WriteLine(((checkBox_Macros.Checked) ? macro.Value.ParsedMacro : ""));
						rW.WriteLine((checkBox_Macros.Checked && checkBox_MacroCodes.Checked) ? "<HR>" : "");
						rW.WriteLine(((checkBox_MacroCodes.Checked) ? macro.Value.MacroCodes : ""));
						rW.WriteLine("</TD></TR>");
					}

					rW.WriteLine("</TABLE><HR>");
				}

				//Setpoints
				if (checkBox_SetpointsScheduler.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center width=75%>");
					rW.WriteLine("<TR><TD align=center colspan=7><H2>Scheduler Setpoints</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Setpoint #</B></TD><TD><B>Day of week</B></TD><TD><B>Monthly</B></TD><TD><B>Week of month</B>(if monthly)</TD><TD><B>Start Hour</B></TD><TD><B>Start Minutes</B></TD><TD><B>Macro to run</B></TD></TR>");

					for (int i = 1; i <= 20; i++)
					{
						rW.WriteLine("<TR><TD>" + i + "</TD>");
						rW.WriteLine("<TD>" + ParseDow(datFile.IniReadValue("Scheduler", "DOW(" + i + ")")) + "</TD>");
						rW.WriteLine("<TD>" + ParseMonthly(datFile.IniReadValue("Scheduler", "Monthly(" + i + ")")) + "</TD>");
						rW.WriteLine("<TD>" + datFile.IniReadValue("Scheduler", "Week(" + i + ")") + "</TD>");
						rW.WriteLine("<TD>" + ParseHours(datFile.IniReadValue("Scheduler", "Hours(" + i + ")")) + "</TD>");
						rW.WriteLine("<TD>" + datFile.IniReadValue("Scheduler", "Minutes(" + i + ")") + "</TD>");
						rW.WriteLine("<TD>" + datFile.IniReadValue("Scheduler", "MacroToRun(" + i + ")") + "</TD></TR>");

						//if they want to
						var macroNum = Convert.ToInt32(datFile.IniReadValue("Scheduler", "MacroToRun(" + i + ")"));
						rW.WriteLine("<TR><TD colspan=7>");

						rW.WriteLine(macroNum < 41 ? _macros[macroNum].ParsedMacro + "<HR>" + _macros[macroNum].MacroCodes : _shortMacros[macroNum - 40].ParsedMacro + "<HR>" + _shortMacros[macroNum - 40].MacroCodes);

						rW.WriteLine("</TD></TR>");
					}

					rW.WriteLine("</TABLE><HR>");
				}

				//CT MSG Macros
				if (checkBox_CTMessageMacros.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=4><H2>Courtesty Tone Message Macros</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Port</B></TD><TD><B>Tone</B></TD><TD><B>Message Macro</B></TD><TD><B>Macro Contents</B></TD></TR>");

					for (int i = 1; i <= 3; i++)
					{
						for (int j = 1; j <= 10; j++)
						{
							var tone1 = datFile.IniReadValue("Courtesy", "P" + i + "Tone1(" + j + ")");
								
							if ((Convert.ToInt32(tone1) > 40) || (Convert.ToInt32(tone1) == 0))
								continue;

							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" + j + "</TD>");
							rW.WriteLine("<TD>" + tone1 + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("MessageMacros", "MessageMacro(" + tone1 + ")") + "</TD></TR>");
						}

						if (fwVersion >= 7.02) //From version 7.02 there is a single pool of Courtesy Tones @ P1Tone1 - P1Tone10
							break;
					}

					rW.WriteLine("</TABLE><HR>");
				}

				//Messages
				if (checkBox_MessageMacros.Checked)
				{

					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=2><H2>Message Macros</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Message Macro</B></TD><TD><B>Words spoken</B></TD></TR>");

					int lastMessage = checkBox_RTCOption.Checked ? 70 : 40;
					for (int i = 1; i <= lastMessage; i++)
					{
						var messageMacro = datFile.IniReadValue("MessageMacros", "MessageMacro(" + i + ")");
						if (messageMacro != "" && messageMacro != "NONE STORED")
						{
							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" + messageMacro +"</TD></TR>");
						}
					}
					
					rW.WriteLine("</TABLE><HR>");
				}

				//General Timers
				if (checkBox_GeneralTimers.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=3><H2>General Timers</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Timer</B></TD><TD><B>Time (seconds)</B></TD><TD><B>Macro to run</B></TD></TR>");

					for (int i = 1; i <= 6; i++)
					{
						var generalTimer = datFile.IniReadValue("Alarms", "GeneralTimer(" + i + ")"); // != 0
						if (generalTimer != "0")
						{
							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" + generalTimer + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Alarms", "GeneralTimerMacro(" + i + ")") + "</TD></TR>");
						}	
					}

					rW.WriteLine("</TABLE>");
					rW.WriteLine("<HR>");
				}

				//Analog Meters
				if (checkBox_AnalogMeters.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=6><H2>Analog Meters</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Meter #</B></TD><TD><B>Meter Type</B></TD><TD><B>Actual Low</B></TD><TD><B>Meter Low</B></TD><TD><B>Actual High</B></TD><TD><B>Meter High</B></TD></TR>");

					for (int i = 1; i <= 8; i++)
					{
						var meter = datFile.IniReadValue("Analog", "Meter(" + i + ")");

						if (meter != "0")
						{
							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" + ParseMeter(meter) + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "RealLow(" + i + ")") + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "MeterLow(" + i + ")") + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "RealHigh(" + i + ")") + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "MeterHigh(" + i + ")") + "</TD></TR>");
						}

					}
					rW.WriteLine("</TABLE>");

					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=5><H2>Analog Meter Alarms</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Alarm #</B></TD><TD><B>Alarm Type</B></TD><TD><B>Meter #</B></TD><TD><B>Meter Set Point</B></TD><TD><B>Macro to run</B></TD></TR>");

					for (int i = 1; i <= 8; i++)
					{
						var meterAlarm = datFile.IniReadValue("Analog", "MeterAlarmType(" + i + ")");
						if (meterAlarm != "0")
						{
							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" +ParseMeterAlarmType(datFile.IniReadValue("Analog", "MeterAlarmType(" + i + ")")) + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "MeterNum(" + i + ")") + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "MeterSetPoint(" + i + ")") + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Analog", "MeterMacro(" + i + ")") + "</TD></TR>");
						}
					}
					rW.WriteLine("</TABLE><HR>");
				}

				//Alarms
				if (checkBox_Alarms.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=4><H2>Alarms</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Alarm #</B></TD><TD><B>Status</B></TD><TD><B>High to Low macro</B></TD><TD><B>Low to High macro</B></TD></TR>");

					for (int i = 1; i <= 5; i++)
					{
						rW.WriteLine("<TR><TD>" + i + "</TD>");
						rW.WriteLine("<TD>" + ParseAlarm(datFile.IniReadValue("PortSwitches", "Alarm(" + i + ")")) +"</TD>");
						rW.WriteLine("<TD>" + datFile.IniReadValue("Alarms", "AlarmLowMacroNum(" + i + ")") + "</TD>");
						rW.WriteLine("<TD>" + datFile.IniReadValue("Alarms", "AlarmHighMacroNum(" + i + ")") + "</TD></TR>");
					}

					rW.WriteLine("</TABLE><HR>");
				}

				//DTMF Memories
				if (checkBox_DTMFMemories.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=2><H2>DTMF Memories</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Memory</B></TD><TD><B>DTMF String</B></TD></TR>");

					int dtmfMemoryCount = (checkBox_RTCOption.Checked) ? 50 : 20;
					for (int i = 1; i <= dtmfMemoryCount; i++)
					{
						var dtmfMemory = datFile.IniReadValue("DTMF", "DTMF(" + i + ")");
						if (dtmfMemory != "" && dtmfMemory != "NONE STORED")
						{
							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("DTMF", "DTMF(" + i + ")") + "</TD></TR>");
						}
					}

					rW.WriteLine("</TABLE><HR>");
				}

				//Remote Base Memories
				if (checkBox_RemoteBaseMemories.Checked)
				{
					rW.WriteLine("<TABLE border=1 align=center>");
					rW.WriteLine("<TR><TD align=center colspan=6><H2>Remote Base Memories</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Memory</B></TD><TD><B>Frequency</B></TD><TD><B>Offset</B></TD><TD><B>CTCSS</B></TD><TD><B>CTCSS Mode</B></TD><TD><B>Radio Mode</B></TD></TR>");
					int remoteBaseMemories = (checkBox_RTCOption.Checked) ? 40 : 10;
					for (int i = 1; i <= remoteBaseMemories; i++)
					{
						var freqString = datFile.IniReadValue("Remote", "FreqString(" + i + ")");
						if (freqString != "" && freqString != "0" && freqString != "NONE")
						{
							rW.WriteLine("<TR><TD>" + i + "</TD>");
							rW.WriteLine("<TD>" + freqString.Substring(0, freqString.Length - 1) + "</TD>");
							rW.WriteLine("<TD>" + ParseRemoteOffset(freqString.Substring(freqString.Length - 1, 1)) + "</TD>");
							rW.WriteLine("<TD>" + datFile.IniReadValue("Remote", "CTCSS(" + i + ")") + "</TD>");
							rW.WriteLine("<TD>" + ParseCtcssMode(datFile.IniReadValue("Remote", "CTCSSMode(" + i + ")")) + "</TD>");
							rW.WriteLine("<TD>" + ParseRadioMode(datFile.IniReadValue("Remote", "RadioMode(" + i + ")")) + "</TD></TR>");
						}
					}
					rW.WriteLine("</TABLE><HR>");
				}

				//AutoPatch Settings
				if (checkBox_APSettings.Checked)
				{
					rW.WriteLine("<TABLE border=1 aling=center");
					rW.WriteLine("<TR><TD align=center colspan=2><H2>AutoPatch Settings</H2></TD></TR>");
					rW.WriteLine("<TR><TD><B>Prefix:</B> " + ((checkBox_APPrefix.Checked) ? _autoPatch.Prefix : "Not Displayed") +
						            "</TD>");
					rW.WriteLine("<TD><B>Answer Code:</B> " +
						            ((checkBox_APAnswerCode.Checked) ? _autoPatch.AnswerCode : "Not Displayed") + "</TD></TR>");
					rW.WriteLine("<TR><TD><B>Ring Count:</B> " + _autoPatch.RingCount + "</TD>");
					rW.WriteLine("<TD><B>Timeout:</B> " + _autoPatch.TimeOut + "</TD></TR>");
					rW.WriteLine("<TR><TD><B>Allowed Ports:</B> " + _autoPatch.Ports + "</TD>");
					rW.WriteLine("<TD><B>Patch Mute:</B> " + _autoPatch.PatchMute + "</TD></TR>");
						
					rW.WriteLine("<TR><TD valign=top>");

					//AutoPatch Memories
					if (checkBox_APMemories.Checked)
					{
						rW.WriteLine("<TABLE border=1 align=center>");
						rW.WriteLine("<TR><TD align=center colspan=2><H2>AutoPatch Memories</H2></TD></TR>");
						rW.WriteLine("<TR><TD><B>Memory</B></TD><TD><B>DTMF String</B></TD></TR>");

						for (int i = 1; i <= 200; i++)
						{
							var autoDial = datFile.IniReadValue("AutoPatch", "AutoDial(" + i + ")");
							if (autoDial.Length > 0)
							{
								rW.WriteLine("<TR><TD>" + i + "</TD>");
								rW.WriteLine("<TD>" + datFile.IniReadValue("AutoPatch", "AutoDial(" + i + ")") + "</TD></TR>");
							}
						}

						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD><TD valign=top>");

					//AutoPatch Toll Restrictions
					if (checkBox_APTollRestrictions.Checked)
					{
						rW.WriteLine("<TABLE border=1 align=center>");
						rW.WriteLine("<TR><TD align=center colspan=2><H2>AutoPatch Toll Restrictions</H2></TD></TR>");
						rW.WriteLine("<TR><TD><B>Entry</B></TD><TD><B>Toll</B></TD></TR>");

						for (int i = 1; i <= 100; i++)
						{
							var apTollRestriction = datFile.IniReadValue("AutoPatch", "TollRestrict(" + i + ")");
							if (apTollRestriction.Length > 0)
							{
								rW.WriteLine("<TR><TD>" + i + "</TD>");
								rW.WriteLine("<TD>" + datFile.IniReadValue("AutoPatch", "TollRestrict(" + i + ")") + "</TD></TR>");
							}
						}

						rW.WriteLine("</TABLE>");
					}

					rW.WriteLine("</TD></TR>");
					rW.WriteLine("</TABLE><HR>");
				}

				rW.WriteLine("F/W Target Version: " + comboBox_FwVersion.SelectedItem + "<BR>");
				rW.WriteLine("This file was generated by Data Assistant V2 written by James M. Bolding - KC5TDG: bolwire@cableone.net<br>");
				rW.WriteLine("Please join the Data Assistant Group at: <a href=https://groups.google.com/forum/#!forum/rc210_data_assistant>https://groups.google.com/forum/#!forum/rc210_data_assistant</a>");

				var dr = MessageBox.Show(
					string.Format("Your report has been created at:\n\n{0}\n\nWould you like to open it now?", _reportFilename),
					@"Report created",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Information
					);
				
				if (dr == DialogResult.Yes) Process.Start(_reportFilename);
			}
		}


		#endregion Generate Report

		#region Load AutoPatch

		private void Load_AutoPatch()
		{
			IniFile datFile = new IniFile(_datFilename);

			_autoPatch.Prefix = datFile.IniReadValue("Autopatch", "APPrefix");
			_autoPatch.AnswerCode = datFile.IniReadValue("Autopatch", "AnswerCode");
			_autoPatch.RingCount =datFile.IniReadValue("Autopatch", "RingNumber");
			_autoPatch.TimeOut = datFile.IniReadValue("Autopatch", "APTimeOut");
			_autoPatch.Ports = datFile.IniReadValue("Autopatch", "EnablePatch");
			_autoPatch.PatchMute = datFile.IniReadValue("Autopatch", "PatchMute");

			_autoPatch.AutoDial.Clear();

			for (int i = 1; i <= 200; i++)
			{
				_autoPatch.AutoDial.Add(i,datFile.IniReadValue("Autopatch", string.Format("AutoDial({0})", i)));
			}

			_autoPatch.TollRestriction.Clear();

			for (int i = 1; i <= 200; i++)
			{
				_autoPatch.TollRestriction.Add(i, datFile.IniReadValue("Autopatch", string.Format("TollRestrict({0})", i)));
			}
		}

		#endregion Load AutoPatch

		#region Load Ports

		private void Load_Ports(double fwVersion)
		{
			IniFile datFile = new IniFile(_datFilename);

			_ports.Clear();

			for (int i = 1; i <= 3; i++)
			{
				string portName = string.Empty;
				
				if (i == 1)
					portName = Settings.Default.Port1Name;
				if (i == 2)
					portName = Settings.Default.Port2Name;
				if (i == 3)
					portName = Settings.Default.Port3Name;

				Port portItem = new Port
				{
					PortCode = datFile.IniReadValue("Unlock", string.Format("CurrentPortUnlock({0})", i)),
					TimeoutTimer = datFile.IniReadValue("Timers", string.Format("TimeOut({0})", i)),
					InitialIdTimer = datFile.IniReadValue("Timers", string.Format("IIDTime({0})", i)),
					PendingIdTimer = datFile.IniReadValue("Timers", string.Format("PIDTime({0})", i)),
					InactivityTimer = datFile.IniReadValue("Timers", string.Format("InActiveTime({0})", i)),
					InactivityTimerMacro = datFile.IniReadValue("Timers", string.Format("InActivityMacro({0})", i)),
					DtmfMuteTimer = datFile.IniReadValue("Timers", string.Format("DTMFTime({0})", i)),
					EncoderTimer = datFile.IniReadValue("Timers", string.Format("CTCSSTime({0})", i)),
					KerchunkTimer = datFile.IniReadValue("Timers", string.Format("KerchunkTime({0})", i)),
					PendingIdSpeechTimer = datFile.IniReadValue("Timers", string.Format("IDSpeakTime({0})", i)),
					TailMessageTimer = datFile.IniReadValue("Timers", string.Format("AnnounceTime({0})", i)),
					AuxAudioTimer = datFile.IniReadValue("Timers", string.Format("AuxTime({0})", i)),
					VoiceId1 = datFile.IniReadValue("ID", string.Format("VoiceID1({0})", i)),
					VoiceId2 = datFile.IniReadValue("ID", string.Format("VoiceID2({0})", i)),
					VoiceId3 = datFile.IniReadValue("ID", string.Format("VoiceID3({0})", i)),
					Cwid1 = datFile.IniReadValue("ID", string.Format("CWID1({0})", i)),
					Cwid2 = datFile.IniReadValue("ID", string.Format("CWID2({0})", i)),
					TailMessage = datFile.IniReadValue("PortSwitches", string.Format("TailMessageNum({0})", i)),
					TailMessageCounter = datFile.IniReadValue("PortSwitches", string.Format("TailCounter({0})", i)),
					TailMessage1Macro = datFile.IniReadValue("PortSwitches", string.Format("P{0}MessageNum(1)", i)),
					TailMessage2Macro = datFile.IniReadValue("PortSwitches", string.Format("P{0}MessageNum(2)", i)),
					TailMessage3Macro = datFile.IniReadValue("PortSwitches", string.Format("P{0}MessageNum(3)", i)),

					//Switches
					TransmitterEnable = (datFile.IniReadValue("PortSwitches", string.Format("TxEnable({0})", i)) == "0") ? "Disabled" : "Enabled",
					ReceiverEnable = (datFile.IniReadValue("PortSwitches", string.Format("RxEnable({0})", i)) == "0") ? "Disabled" : "Enabled",
					DisableInactivityTimer = (datFile.IniReadValue("PortSwitches", string.Format("DisableTimeout({0})", i)) == "0") ? "Disabled" : "Enabled",
					RepeaterMode = (datFile.IniReadValue("PortSwitches", string.Format("FDup({0})", i)) == "0") ? "Disabled" : "Enabled",
					SpeechOverride = (datFile.IniReadValue("PortSwitches", string.Format("SpeechOverride({0})", i)) == "0") ? "Disabled" : "Enabled",
					SpeechIdOverride = (datFile.IniReadValue("PortSwitches", string.Format("SpeechIDOverride({0})", i)) == "0") ? "Disabled" : "Enabled",
					//AccessMode = datFile.IniReadValue("PortSwitches", string.Format("P{0}MessageNum(3)", i)),
					DtmfEnable = (datFile.IniReadValue("PortSwitches", string.Format("DTMFEnable({0})", i)) == "0") ? "Disabled" : "Enabled",
					DtmfRequireTone = (datFile.IniReadValue("PortSwitches", string.Format("DTMFNeedPL({0})", i)) == "0") ? "Disabled" : "Enabled",
					DtmfMute = (datFile.IniReadValue("PortSwitches", string.Format("DTMFMute({0})", i)) == "0") ? "Disabled" : "Enabled",
					DtmfCoverTone = (datFile.IniReadValue("PortSwitches", string.Format("DTMFCovertone({0})", i)) == "0") ? "Disabled" : "Enabled",
					MonitorMix = (datFile.IniReadValue("PortSwitches", string.Format("MonMix({0})", i)) == "0") ? "Disabled" : "Enabled",
					KerchunkFilter = (datFile.IniReadValue("PortSwitches", string.Format("Kerchunk({0})", i)) == "0") ? "Disabled" : "Enabled",

				};

				if (fwVersion < 7.02)
				{
					portItem.HangTimer1 = datFile.IniReadValue("Timers", string.Format("HangTime({0})", i));
				}
				else
				{
					portItem.HangTimer1 = datFile.IniReadValue("Timers", string.Format("HangTime1({0})", i));
					portItem.HangTimer2 = datFile.IniReadValue("Timers", string.Format("HangTime2({0})", i));
					portItem.HangTimer3 = datFile.IniReadValue("Timers", string.Format("HangTime3({0})", i));
				}


				if (portName != string.Empty)
					portItem.PortName = portName;

				_ports.Add(i, portItem);
			}
		}

		#endregion Load Ports

		#region Load Macros/ShortMacros

		private void Load_Macros()
		{
			IniFile datFile = new IniFile(_datFilename);
			
			_macros.Clear();

			for (int i = 1; i <= 40; i++)
			{
				string macro = datFile.IniReadValue("Macros", string.Format("Macro({0})", i));
				string macroCode = datFile.IniReadValue("Macros", string.Format("MacroCode({0})", i));
				string allowedPorts = datFile.IniReadValue("Macros", string.Format("PortToAllow({0})", i));

				Macro macroItem = new Macro
				{
					MacroNumber = i,
					//MacroCodes = macro,
					AccessCode = macroCode,
					ParsedMacro = _macroLibrary.ParseMacro(comboBox_FwVersion.SelectedItem.ToString(), macro),
					AllowedPorts = allowedPorts
				};

				char[] splitChar = {' '};
				string[] macroCodes = macro.Split(splitChar);

				foreach (string code in macroCodes)
				{
					macroItem.MacroCodes = macroItem.MacroCodes + " " + string.Format("<a title=\"{0}\">{1}</a>", _macroLibrary.ParseMacro(comboBox_FwVersion.SelectedItem.ToString(), code), code);
				}

				macroItem.MacroCodes = macroItem.MacroCodes.Substring(1);

				_macros.Add(i,macroItem);
			}
		}

		private void Load_ShortMacros()
		{
			IniFile datFile = new IniFile(_datFilename);

			_shortMacros.Clear();

			for (int i = 1; i <= 50; i++)
			{
				string shortMacro = datFile.IniReadValue("Macros", string.Format("ShortMacro({0})", i));
				string shortMacroCode = datFile.IniReadValue("Macros", string.Format("ShortMacroCode({0})", i));
				string allowedPorts = datFile.IniReadValue("Macros", string.Format("PortToAllow({0})", i + 40));

				Macro shortMacroItem = new Macro
				{
					MacroNumber = i + 40,
					//MacroCodes = shortMacro,
					AccessCode = shortMacroCode,
					ParsedMacro = _macroLibrary.ParseMacro(comboBox_FwVersion.SelectedItem.ToString(), shortMacro),
					AllowedPorts = allowedPorts
				};

				char[] splitChar = { ' ' };
				string[] macroCodes = shortMacro.Split(splitChar);

				foreach (string code in macroCodes)
				{
					shortMacroItem.MacroCodes = shortMacroItem.MacroCodes + " " + string.Format("<a title=\"{0}\">{1}</a>", _macroLibrary.ParseMacro(comboBox_FwVersion.SelectedItem.ToString(), code), code);
				}

				shortMacroItem.MacroCodes = shortMacroItem.MacroCodes.Substring(1);

				_shortMacros.Add(i, shortMacroItem);
			}
		}

		#endregion Load Macros/ShortMacros

		#region Load Message Macros

		private void Load_MessageMacros()
		{
			IniFile datFile = new IniFile(_datFilename);

			_messageMacros.Clear();

			int messageMacroCount = checkBox_RTCOption.Checked ? 70 : 40;

			for (int i = 1; i <= messageMacroCount; i++)
			{
				_messageMacros.Add(i,datFile.IniReadValue("MessageMacros", string.Format("MessageMacro({0})",i)));
			}
		}

		#endregion Load Message Macros

		#endregion Private Methods

		#region ToolStripMenu Even Handlers

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(@"This will overwrite any existing options, are you sure ?",
				@"Warning - Overwrite Options", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result != DialogResult.Yes)
				return;

			Settings.Default.Save();
		}
		
		private void resetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(@"This is a global reset of options that cannot be undone, are you sure ?",
				@"Warning - Options Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

			if (result != DialogResult.Yes)
				return;

			Settings.Default.Reset();
		}

		private void recallToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(@"This will repopulate the options to match the saved options, are you sure ?",
				@"Warning - Overwrite Options", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result != DialogResult.Yes)
				return;

			Settings.Default.Reload();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			viewReportToolStripMenuItem.Enabled = openReportFolderToolStripMenuItem.Enabled = false;
			OpenFileDialog resultOpenFileDialog = new OpenFileDialog
			{
				Filter = @"RC210 dat file (*.dat)|*.dat"
			};

			if (_datFilename != string.Empty)
				resultOpenFileDialog.InitialDirectory = Path.GetDirectoryName(_datFilename);

			if (resultOpenFileDialog.ShowDialog() != DialogResult.OK)
				return;

			_datFilename = resultOpenFileDialog.FileName;
			Text = @"RC210 Data Assistant V2 - " + Path.GetFileName(_datFilename);
			generateReportToolStripMenuItem1.Enabled = true;
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			generateReportToolStripMenuItem1.Enabled = false;
			viewReportToolStripMenuItem.Enabled = openReportFolderToolStripMenuItem.Enabled = false;

			_datFilename = string.Empty;
			Text = @"RC210 Data Assistant V2";
		}

		private void port1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string value = Settings.Default.Port1Name;
			var result = InputBox(@"Port 1 name", "Port 1 name:", ref value);

			if (result != DialogResult.OK)
				return;

			Settings.Default.Port1Name = value;
			groupBox_Port1.Text = @"Port 1 - " + value;
		}

		private void port2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string value = Settings.Default.Port2Name;
			var result = InputBox(@"Port 2 name", "Port 2 name:", ref value);

			if (result != DialogResult.OK)
				return;
			
			Settings.Default.Port2Name = value;
			groupBox_Port2.Text = @"Port 2 - " + value;
		}

		private void port3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string value = Settings.Default.Port3Name;
			var result = InputBox(@"Port 3 name", "Port 3 name:", ref value);

			if (result != DialogResult.OK)
				return;
			
			Settings.Default.Port3Name = value;
			groupBox_Port3.Text = @"Port 3 - " + value;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FormAbout aboutDialog = new FormAbout();
			aboutDialog.ShowDialog();
		}

		private void generateReportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Generate_Report();
			viewReportToolStripMenuItem.Enabled = openReportFolderToolStripMenuItem.Enabled = true;
		}

		private void viewReportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!File.Exists(_reportFilename))
			{
				MessageBox.Show(
					string.Format("Report not found at path:\n\n{0}\n\nPlease verify that you have already generated the report.", _reportFilename), @"Report not found", MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation);
				return;
			}

			Process.Start(_reportFilename);
		}

		private void openReportFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (_reportFilename != null)
				return;

			if (Directory.Exists(Path.GetDirectoryName(_reportFilename)))
				Process.Start(Path.GetDirectoryName(_reportFilename));
		}

		private void versionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show(
				string.Format("Macro Definitions\n\nLocal file: {0}\nRemote file: {1}", _localXmlVersion, _remoteXmlVersion),
				@"Macro Definitions", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void updateToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//FetchDefXml(true);
			UpdateXml();
		}

		#endregion ToolStripMenu Even Handlers

		#region InputBox

		public static DialogResult InputBox(string title, string promptText, ref string value)
		{
			Form form = new Form();
			Label label = new Label();
			TextBox textBox = new TextBox();
			Button buttonOk = new Button();
			Button buttonCancel = new Button();

			form.Text = title;
			label.Text = promptText;
			textBox.Text = value;

			buttonOk.Text = @"OK";
			buttonCancel.Text = @"Cancel";
			buttonOk.DialogResult = DialogResult.OK;
			buttonCancel.DialogResult = DialogResult.Cancel;

			label.SetBounds(9, 20, 372, 13);
			textBox.SetBounds(12, 36, 372, 20);
			buttonOk.SetBounds(228, 72, 75, 23);
			buttonCancel.SetBounds(309, 72, 75, 23);

			label.AutoSize = true;
			textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
			buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			form.ClientSize = new Size(396, 107);
			form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
			form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.StartPosition = FormStartPosition.CenterScreen;
			form.MinimizeBox = false;
			form.MaximizeBox = false;
			form.AcceptButton = buttonOk;
			form.CancelButton = buttonCancel;

			DialogResult dialogResult = form.ShowDialog();
			value = textBox.Text;
			return dialogResult;
		}

		#endregion InputBox

		private void xMLFilePathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string filepath = AppDomain.CurrentDomain.BaseDirectory + "xml\\";

			MessageBox.Show("I look for .xml files at the following location:\n\n" + filepath);
		}

		
	}
}
