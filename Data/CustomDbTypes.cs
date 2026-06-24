using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Data
{
	public enum AppointmentStatus
	{
		// upcomming
		pending,
		// completed
		done,
		// waiting for Labtests or xrays
		waiting,
		current,
		rejected
	}
}
