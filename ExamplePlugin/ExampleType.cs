using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExampleCommon;

namespace ExamplePlugin {

	/// <summary>
	/// Example implementation of a plug-in interface. Note that the type 
	/// inherits from MarshalByRefObject - this is necessary to ensure that 
	/// method calls occur within the plug-in AppDomain. If you omit this (and 
	/// the type is not serializable) then you will not be able to call any 
	/// methods on the object.
	/// </summary>
	public class ExampleType : MarshalByRefObject, IExampleType {

		/// <summary>
		/// Implements the method from the interface.
		/// </summary>
		public void DisplayMessage() {
			Console.WriteLine("This is a message from the example plug-in.");
		}
	}
}
