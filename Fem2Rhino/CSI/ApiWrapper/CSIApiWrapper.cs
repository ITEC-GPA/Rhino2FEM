using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fem2Rhino.CSI.ApiWrapper
{
	public abstract class CSIApiWrapper
	{
		public enum CsiSoftware
		{
			Sap2000,
			Etabs
		}

		protected double _apiVersion;
		protected bool _attachToInstance;

		public double ApiVersion => _apiVersion;
	}
}
