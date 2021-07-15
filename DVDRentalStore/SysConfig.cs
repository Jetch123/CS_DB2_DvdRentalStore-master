using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVDRentalStore.Config
{
	static class SysConfig
	{
		public static readonly string CONNECTION_STRING = System.Configuration.ConfigurationManager.ConnectionStrings["connection_string"].ToString();
	}
}
