using System;
using System.Collections.Generic;
using System.Linq;

namespace RC210_DataAssistant_V2
{
	public class MacroLibrary
	{
		private readonly Dictionary<string, Dictionary<string, string>> _macroLibrary = new Dictionary<string, Dictionary<string, string>>();

		public Dictionary<string, Dictionary<string, string>> GetLibraries
		{
			get { return _macroLibrary; }
		}

		public string ParseMacro(string version, string ids)
		{
			string parsedMacro = ids.Split(' ').Aggregate<string, string>(null, (current, id) => current + " " + (_macroLibrary[version].ContainsKey(id) ? _macroLibrary[version][id] : id));

			if (parsedMacro != null)
				parsedMacro = parsedMacro.Substring(1);

			return parsedMacro;
		}

		public void AddMacro(string versions, string id, string description, bool overWrite = false)
		{
			foreach (string version in versions.Split(' '))
			{
				if (!_macroLibrary.ContainsKey(version))
					_macroLibrary.Add(version, new Dictionary<string, string>());

				if (!_macroLibrary[version].ContainsKey(id))
				{
					_macroLibrary[version].Add(id, description);
				}
				else if (overWrite)
				{
					Console.WriteLine("MacroLibrary: {0} Overriding ID: {1} Description: {2}", version, id, description);
					_macroLibrary[version].Remove(id);
					_macroLibrary[version].Add(id, description);	
				}
			}
		}
	}
}
