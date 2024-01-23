using System;

namespace Sales_Manager.Data.Model
{
	public class ItemAggregatedRecord
	{
		public string ItemName { get; set; }
		public int Qty { get; set; }
		public double Total { get; set; }
		public DateTime StartDate { set; get; }
		public DateTime EndDate { set; get; }
	}
}
