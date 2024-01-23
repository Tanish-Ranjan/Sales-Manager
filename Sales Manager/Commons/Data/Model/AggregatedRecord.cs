using System;

namespace Sales_Manager.Data.Model
{
	public class AggregatedRecord
	{
		public double TotalAmount { set; get; }
		public DateTime StartDate { set; get; }
		public DateTime EndDate { set; get; }
	}
}
