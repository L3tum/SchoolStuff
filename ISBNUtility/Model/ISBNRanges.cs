#region usings

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

#endregion

namespace ISBNUtility.Model
{
	public class Rule
	{
		public string Range { get; set; }
		public string Length { get; set; }
	}

	public class Rules
	{
		public object Rule { get; set; }
	}

	public class Group
	{
		public string Prefix { get; set; }
		public string Agency { get; set; }
		public Rules Rules { get; set; }
	}

	public class RegistrationGroups
	{
		public List<Group> Group { get; set; }
	}

	public class ISBNRangeMessage
	{
		public string MessageSource { get; set; }
		public string MessageSerialNumber { get; set; }
		public string MessageDate { get; set; }
		public RegistrationGroups RegistrationGroups { get; set; }
	}

	public class ISBNRanges
	{
		public ISBNRangeMessage ISBNRangeMessage { get; set; }
	}
}