using System;

namespace Sales_Manager.Data.Model
{
	public class SalesRecord
	{

		public int Id { set; get; }
		public string ItemName { set; get; }
		public int ItemQty { set; get; }
		public double ItemRate { set; get; }
		public double Total { set; get; }
		public DateTime Date { set; get; }

	}
}
