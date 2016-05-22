using System;

namespace ExampleCommon {

	/// <summary>
	/// Example of a plug-in interface. Defines the functionality but not the 
	/// implementation. For this reason, the assembly containing the interface 
	/// can be referenced by both the host and the plug-in.
	/// </summary>
	public interface IExampleType {

		/// <summary>
		/// Example of an interface method.
		/// </summary>
		void DisplayMessage();
	}
}
