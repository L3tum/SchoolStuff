#region usings

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

#endregion

namespace Quartett
{
	public class Karte : DynamicObject
	{
		// The inner dictionary.
		private readonly Dictionary<string, object> dictionary
			= new Dictionary<string, object>();

		// If you try to get a value of a property 
		// not defined in the class, this method is called.
		public override bool TryGetMember(
			GetMemberBinder binder, out object result)
		{
			// Converting the property name to lowercase
			// so that property names become case-insensitive.
			string name = binder.Name.ToLower();

			// If the property name is found in a dictionary,
			// set the result parameter to the property value and return true.
			// Otherwise, return false.
			return dictionary.TryGetValue(name, out result);
		}

		// If you try to set a value of a property that is
		// not defined in the class, this method is called.
		public override bool TrySetMember(
			SetMemberBinder binder, object value)
		{
			// Converting the property name to lowercase
			// so that property names become case-insensitive.
			dictionary[binder.Name.ToLower()] = value;

			// You can always add a value to a dictionary,
			// so this method always returns true.
			return true;
		}

		public object GetProperty(string name)
		{
			return dictionary.ContainsKey(name) ? dictionary[name] : null;
		}

		public string[] GetProperties()
		{
			return dictionary.Keys.ToArray();
		}

		public bool HasProperty(string name)
		{
			return dictionary.ContainsKey(name);
		}

		public bool HasProperties(string[] names)
		{
			return names.All(HasProperty);
		}
	}
}