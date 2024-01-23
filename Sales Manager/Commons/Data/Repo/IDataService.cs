using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Data.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sales_Manager.Commons.Data.Repo
{

	public interface IDataService
	{

		Task InitializeDatabase();

		Task<List<SalesItemCounterRecord>> GetSalesItemList();
		Task<AggregatedRecord> ReadLastEntryDate();
		Task<List<AggregatedRecord>> ReadLifetimeYearlyAggregation();

		Task InsertData(SalesRecord newItem);
		Task UpdateData(SalesRecord oldItem, SalesRecord newItem);
		Task DeleteData(SalesRecord item);

		Task<List<SalesRecord>> ReadPaginatedSalesRecords(int pageSize, int currentPage);
		Task<List<SalesRecord>> QueryPaginatedSalesRecords(string name, DateTime start, DateTime end, int pageSize, int currentPage);

		Task<List<AggregatedRecord>> ReadPaginatedWeeklyAggregation(int pageSize, int currentPage);
		Task<List<AggregatedRecord>> ReadPaginatedMonthlyAggregation(int pageSize, int currentPage);
		Task<List<AggregatedRecord>> ReadPaginatedYearlyAggregation(int pageSize, int currentPage);
		Task<AggregatedRecord> ReadLifetimeAggregation();

		Task<SalesItemCounterRecord> GetWeeklyAggregationRecordsCount();
		Task<SalesItemCounterRecord> GetMonthlyAggregationRecordsCount();
		Task<SalesItemCounterRecord> GetYearlyAggregationRecordsCount();

		Task<SalesItemCounterRecord> GetWeeklyItemAggregationRecordsCount(string itemName);
		Task<SalesItemCounterRecord> GetMonthlyItemAggregationRecordsCount(string itemName);
		Task<SalesItemCounterRecord> GetYearlyItemAggregationRecordsCount(string itemName);

		Task<List<List<AggregatedRecord>>> ReadWeeklyForecast(DateTime date);
		Task<List<List<AggregatedRecord>>> ReadMonthlyForecast(DateTime date);
		Task<List<List<AggregatedRecord>>> ReadYearlyForecast(DateTime date);

		Task<ItemAggregatedRecord> ReadWeeklyItemSale(string itemName, DateTime date);
		Task<ItemAggregatedRecord> ReadMonthlyItemSale(string itemName, DateTime date);
		Task<ItemAggregatedRecord> ReadYearlyItemSale(string itemName, DateTime date);

		Task<List<ItemAggregatedRecord>> ReadPaginatedWeeklyItemAggregation(string itemName, int pageSize, int currentPage);
		Task<List<ItemAggregatedRecord>> ReadPaginatedMonthlyItemAggregation(string itemName, int pageSize, int currentPage);
		Task<List<ItemAggregatedRecord>> ReadPaginatedYearlyItemAggregation(string itemName, int pageSize, int currentPage);

		Task InsertNote(NotesRecord item);
		Task UpdateNote(NotesRecord oldItem, NotesRecord newItem);
		Task DeleteNote(NotesRecord oldItem);
		Task<List<NotesRecord>> ReadNotesPaginated(int pageSize, int currentPage);
		Task<List<NotesRecord>> QueryNotesPaginated(DateTime start, DateTime end, int pageSize, int currentPage);

	}

}
