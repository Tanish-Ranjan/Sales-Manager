using Microsoft.Data.Sqlite;
using Sales_Manager.Commons.Data.Model;
using Sales_Manager.Commons.Data.Repo;
using Sales_Manager.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sales_Manager.Data.Repo
{
	public class DataService : IDataService
	{

		private static string DatabasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Database.db");
		private static string ConnectionString => $"Data Source={DatabasePath}";

		private void EnsureDatabaseDirectoryExists()
		{
			string directoryPath = Path.GetDirectoryName(DatabasePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
		}

		public async Task InitializeDatabase()
		{
			EnsureDatabaseDirectoryExists();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();

				string query = @"
					create table if not exists sales_items (
						ItemName text primary key,
						Count integer
					);

					create table if not exists sales (
						id integer primary key autoincrement,
						ItemName text,
						ItemQty integer,
						ItemRate real,
						Total real,
						Date text
					);

					create index if not exists idx_sales_date on sales (Date);

					create table if not exists weekly_sales (
						StartDate text,
						EndDate text,
						TotalAmount real
					);

					create index if not exists idx_weekly_sales_date on weekly_sales (StartDate, EndDate);

					create table if not exists monthly_sales (
						StartDate text,
						EndDate text,
						TotalAmount real
					);

					create index if not exists idx_monthly_sales_date on monthly_sales (StartDate, EndDate);

					create table if not exists yearly_sales (
						StartDate text,
						EndDate text,
						TotalAmount real
					);

					create index if not exists idx_yearly_sales_date on yearly_sales (StartDate, EndDate);

					create table if not exists lifetime_sales (
						TotalAmount real
					);

					create table if not exists weekly_item_sales (
						ItemName text,
						Qty integer,
						Total real,
						StartDate text,
						EndDate text,
						primary key (ItemName, StartDate)
					);

					create index if not exists idx_weekly_item_sales_name_date on weekly_item_sales (ItemName, StartDate, EndDate);

					create table if not exists monthly_item_sales (
						ItemName text,
						Qty integer,
						Total real,
						StartDate text,
						EndDate text,
						primary key (ItemName, StartDate)
					);

					create index if not exists idx_monthly_item_sales_name_date on monthly_item_sales (ItemName, StartDate, EndDate);

					create table if not exists yearly_item_sales (
						ItemName text,
						Qty integer,
						Total real,
						StartDate text,
						EndDate text,
						primary key (ItemName, StartDate)
					);

					create index if not exists idx_yearly_item_sales_name_date on yearly_item_sales (ItemName, StartDate, EndDate);

					create table if not exists notes (
						Id integer primary key autoincrement,
						Note text,
						Date text
					);

					create index if not exists idx_notes_date on notes (Date);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					await command.ExecuteNonQueryAsync();
				}
			}
		}

		public async Task InsertData(SalesRecord newItem)
		{
			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();
				using (SqliteTransaction transaction = connection.BeginTransaction())
				{
					try
					{
						await AddItemName(newItem, connection, transaction);
						await InsertRecordIntoSalesTable(newItem, connection, transaction);
						await IncreaseAggregationForRecord(newItem, connection, transaction);
						await IncreaseItemAggregationForRecord(newItem, connection, transaction);

						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						Console.WriteLine(ex.Message);
					}
				}
			}
		}

		public async Task UpdateData(SalesRecord oldItem, SalesRecord newItem)
		{
			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();
				using (SqliteTransaction transaction = connection.BeginTransaction())
				{
					try
					{
						await UpdateRecordIntoSalesTable(oldItem, newItem, connection, transaction);
						await ReduceAggregationForRecord(oldItem, connection, transaction);
						await ReduceItemAggregationForRecord(oldItem, connection, transaction);
						await RemoveItemName(oldItem, connection, transaction);
						await AddItemName(newItem, connection, transaction);
						await IncreaseAggregationForRecord(newItem, connection, transaction);
						await IncreaseItemAggregationForRecord(newItem, connection, transaction);

						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						Console.WriteLine(ex.Message);
					}
				}
			}
		}

		public async Task DeleteData(SalesRecord item)
		{
			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();
				using (SqliteTransaction transaction = connection.BeginTransaction())
				{
					try
					{
						await DeleteRecordIntoSalesTable(item, connection, transaction);
						await RemoveItemName(item, connection, transaction);
						await ReduceAggregationForRecord(item, connection, transaction);
						await ReduceItemAggregationForRecord(item, connection, transaction);

						transaction.Commit();
					}
					catch (Exception ex)
					{
						transaction.Rollback();
						Console.WriteLine(ex.Message);
					}
				}
			}
		}

		private async Task InsertRecordIntoSalesTable(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			string query = @"
					insert into sales (ItemName, ItemQty, ItemRate, Total, Date)
					values (@ItemName, @ItemQty, @ItemRate, @Total, @Date);
				";

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);
				command.Parameters.AddWithValue("@ItemQty", item.ItemQty);
				command.Parameters.AddWithValue("@ItemRate", item.ItemRate);
				command.Parameters.AddWithValue("@Total", item.Total);
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				await command.ExecuteNonQueryAsync();
			}
		}

		private async Task UpdateRecordIntoSalesTable(SalesRecord oldItem, SalesRecord newItem, SqliteConnection connection, SqliteTransaction transaction)
		{
			string query = @"
				update sales
				set ItemName = @UpdatedItemName, 
					ItemQty = @UpdatedItemQty,
					ItemRate = @UpdatedItemRate,
					Total = @UpdatedTotal,
					Date = @UpdatedDate
				where id = @RecordId;
			";

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@UpdatedItemName", newItem.ItemName);
				command.Parameters.AddWithValue("@UpdatedItemQty", newItem.ItemQty);
				command.Parameters.AddWithValue("@UpdatedItemRate", newItem.ItemRate);
				command.Parameters.AddWithValue("@UpdatedTotal", newItem.Total);
				command.Parameters.AddWithValue("@UpdatedDate", newItem.Date.ToString("yyyy-MM-dd"));
				command.Parameters.AddWithValue("@RecordId", oldItem.Id);

				await command.ExecuteNonQueryAsync();
			}
		}

		private async Task DeleteRecordIntoSalesTable(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			string query = @"delete from sales where id = @RecordId;";

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@RecordId", item.Id);

				await command.ExecuteNonQueryAsync();
			}
		}

		private async Task ReduceAggregationForRecord(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			await ReduceWeeklyAggregation(item, connection, transaction);
			await ReduceMonthlyAggregation(item, connection, transaction);
			await ReduceYearlyAggregation(item, connection, transaction);
			await ReduceLifetimeAggregation(item, connection, transaction);
		}

		private async Task ReduceWeeklyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromWeeklyAggregation(item, connection, transaction);

			if (aggItem.TotalAmount - item.Total != 0)
			{
				// Reduce the record

				string query = @"
					update weekly_sales
					set TotalAmount = TotalAmount - @ReducingTotal
					where StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@ReducingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Remove the record

				string query = @"delete from weekly_sales where StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task ReduceMonthlyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromMonthlyAggregation(item, connection, transaction);
			if (aggItem.TotalAmount - item.Total != 0)
			{
				// Reduce the record
				string query = @"
					update monthly_sales
					set TotalAmount = TotalAmount - @ReducingTotal
					where StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@ReducingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Remove the record
				string query = @"delete from monthly_sales where StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task ReduceYearlyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromYearlyAggregation(item, connection, transaction);
			if (aggItem.TotalAmount - item.Total != 0)
			{
				// Reduce the record
				string query = @"
					update yearly_sales
					set TotalAmount = TotalAmount - @ReducingTotal
					where StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@ReducingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Remove the record
				string query = @"delete from yearly_sales where StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task ReduceLifetimeAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			string query = @"
				update lifetime_sales
				set TotalAmount = TotalAmount - @ReducingTotal;
			";

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ReducingTotal", item.Total);

				await command.ExecuteNonQueryAsync();
			}
		}

		private async Task IncreaseAggregationForRecord(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			await IncreaseWeeklyAggregation(item, connection, transaction);
			await IncreaseMonthlyAggregation(item, connection, transaction);
			await IncreaseYearlyAggregation(item, connection, transaction);
			await IncreaseLifetimeAggregation(item, connection, transaction);
		}

		private async Task IncreaseWeeklyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromWeeklyAggregation(item, connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update weekly_sales
					set TotalAmount = TotalAmount + @IncreasingTotal
					where StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				DateTime startDate = item.Date.Date.AddDays(-(int)item.Date.DayOfWeek);
				DateTime endDate = startDate.AddDays(7).AddSeconds(-1);

				string query = @"
					insert into weekly_sales (TotalAmount, StartDate, EndDate)
					values (@Total, @Start, @End);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Total", item.Total);
					command.Parameters.AddWithValue("@Start", startDate.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", endDate.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task IncreaseMonthlyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromMonthlyAggregation(item, connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update monthly_sales
					set TotalAmount = TotalAmount + @IncreasingTotal
					where StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				DateTime startDate = new DateTime(item.Date.Year, item.Date.Month, 1);
				DateTime endDate = startDate.AddMonths(1).AddSeconds(-1);

				string query = @"
					insert into monthly_sales (TotalAmount, StartDate, EndDate)
					values (@Total, @Start, @End);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Total", item.Total);
					command.Parameters.AddWithValue("@Start", startDate.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", endDate.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task IncreaseYearlyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromYearlyAggregation(item, connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update yearly_sales
					set TotalAmount = TotalAmount + @IncreasingTotal
					where StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				DateTime startDate = new DateTime(item.Date.Year, 1, 1);
				DateTime endDate = startDate.AddYears(1).AddSeconds(-1);

				string query = @"
					insert into yearly_sales (TotalAmount, StartDate, EndDate)
					values (@Total, @Start, @End);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Total", item.Total);
					command.Parameters.AddWithValue("@Start", startDate.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", endDate.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task IncreaseLifetimeAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromLifetimeAggregation(connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update lifetime_sales
					set TotalAmount = TotalAmount + @IncreasingTotal;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				string query = @"
					insert into lifetime_sales (TotalAmount)
					values (@Total);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@Total", item.Total);

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task<AggregatedRecord> ReadRecordFromWeeklyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from weekly_sales where StartDate <= @Date and EndDate >= @Date";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("TotalAmount")),
							StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
							EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
						};
					}
				}
			}

			return readItem;

		}

		private async Task<AggregatedRecord> ReadRecordFromMonthlyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from monthly_sales where StartDate <= @Date and EndDate >= @Date";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("TotalAmount")),
							StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
							EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
						};
					}
				}
			}

			return readItem;

		}

		private async Task<AggregatedRecord> ReadRecordFromYearlyAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from yearly_sales where StartDate <= @Date and EndDate >= @Date";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("TotalAmount")),
							StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
							EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
						};
					}
				}
			}

			return readItem;

		}

		public async Task<SalesItemCounterRecord> GetWeeklyAggregationRecordsCount()
		{

			SalesItemCounterRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select count(*) as Count from weekly_sales;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new SalesItemCounterRecord
							{
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							};
						}
					}
				}
			}

			return readItem;

		}

		public async Task<SalesItemCounterRecord> GetMonthlyAggregationRecordsCount()
		{

			SalesItemCounterRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select count(*) as Count from monthly_sales;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new SalesItemCounterRecord
							{
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							};
						}
					}
				}
			}

			return readItem;

		}

		public async Task<SalesItemCounterRecord> GetYearlyAggregationRecordsCount()
		{

			SalesItemCounterRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select count(*) as Count from yearly_sales;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new SalesItemCounterRecord
							{
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							};
						}
					}
				}
			}

			return readItem;

		}

		private async Task<AggregatedRecord> ReadRecordFromLifetimeAggregation(SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from lifetime_sales";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("TotalAmount"))
						};
					}
				}
			}

			return readItem;

		}

		private async Task ReduceItemAggregationForRecord(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			await ReduceWeeklyItemAggregation(item, connection, transaction);
			await ReduceMonthlyItemAggregation(item, connection, transaction);
			await ReduceYearlyItemAggregation(item, connection, transaction);
		}

		private async Task ReduceWeeklyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromWeeklyItemAggregation(item, connection, transaction);
			if (aggItem.TotalAmount - item.Total != 0)
			{
				// Reduce the record
				string query = @"
					update weekly_item_sales
					set Total = Total - @ReducingTotal, Qty = Qty - @ReducingQty
					where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@ReducingTotal", item.Total);
					command.Parameters.AddWithValue("@ReducingQty", item.ItemQty);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Remove the record
				string query = @"delete from weekly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task ReduceMonthlyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromMonthlyItemAggregation(item, connection, transaction);
			if (aggItem.TotalAmount - item.Total != 0)
			{
				// Reduce the record
				string query = @"
					update monthly_item_sales
					set Total = Total - @ReducingTotal, Qty = Qty - @ReducingQty
					where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@ReducingTotal", item.Total);
					command.Parameters.AddWithValue("@ReducingQty", item.ItemQty);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Remove the record
				string query = @"delete from monthly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date; ";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task ReduceYearlyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromYearlyItemAggregation(item, connection, transaction);
			if (aggItem.TotalAmount - item.Total != 0)
			{
				// Reduce the record
				string query = @"
					update yearly_item_sales
					set Total = Total - @ReducingTotal, Qty = Qty - @ReducingQty
					where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@ReducingTotal", item.Total);
					command.Parameters.AddWithValue("@ReducingQty", item.ItemQty);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Remove the record
				string query = @"delete from yearly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task IncreaseItemAggregationForRecord(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			await IncreaseWeeklyItemAggregation(item, connection, transaction);
			await IncreaseMonthlyItemAggregation(item, connection, transaction);
			await IncreaseYearlyItemAggregation(item, connection, transaction);
		}

		private async Task IncreaseWeeklyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromWeeklyItemAggregation(item, connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update weekly_item_sales
					set Total = Total + @IncreasingTotal, Qty = Qty + @IncreasingQty
					where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);
					command.Parameters.AddWithValue("@IncreasingQty", item.ItemQty);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				DateTime startDate = item.Date.Date.AddDays(-(int)item.Date.DayOfWeek);
				DateTime endDate = startDate.AddDays(7).AddSeconds(-1);

				string query = @"
					insert into weekly_item_sales (ItemName, Qty, Total, StartDate, EndDate)
					values (@ItemName, @Qty, @Total, @Start, @End);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Qty", item.ItemQty);
					command.Parameters.AddWithValue("@Total", item.Total);
					command.Parameters.AddWithValue("@Start", startDate.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", endDate.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task IncreaseMonthlyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromMonthlyItemAggregation(item, connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update monthly_item_sales
					set Total = Total + @IncreasingTotal, Qty = Qty + @IncreasingQty
					where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);
					command.Parameters.AddWithValue("@IncreasingQty", item.ItemQty);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				DateTime startDate = new DateTime(item.Date.Year, item.Date.Month, 1);
				DateTime endDate = startDate.AddMonths(1).AddSeconds(-1);

				string query = @"
					insert into monthly_item_sales (ItemName, Qty, Total, StartDate, EndDate)
					values (@ItemName, @Qty, @Total, @Start, @End);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Qty", item.ItemQty);
					command.Parameters.AddWithValue("@Total", item.Total);
					command.Parameters.AddWithValue("@Start", startDate.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", endDate.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		private async Task IncreaseYearlyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{
			AggregatedRecord aggItem = await ReadRecordFromYearlyItemAggregation(item, connection, transaction);
			if (aggItem != null)
			{
				// Record exists
				string query = @"
					update yearly_item_sales
					set Total = Total + @IncreasingTotal, Qty = Qty + @IncreasingQty
					where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@IncreasingTotal", item.Total);
					command.Parameters.AddWithValue("@IncreasingQty", item.ItemQty);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				// Create new record
				DateTime startDate = new DateTime(item.Date.Year, 1, 1);
				DateTime endDate = startDate.AddYears(1).AddSeconds(-1);

				string query = @"
					insert into yearly_item_sales (ItemName, Qty, Total, StartDate, EndDate)
					values (@ItemName, @Qty, @Total, @Start, @End);
				";

				using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Qty", item.ItemQty);
					command.Parameters.AddWithValue("@Total", item.Total);
					command.Parameters.AddWithValue("@Start", startDate.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", endDate.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();
				}
			}
		}

		public async Task<SalesItemCounterRecord> GetWeeklyItemAggregationRecordsCount(string itemName)
		{

			SalesItemCounterRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select count(*) as Count from weekly_item_sales where ItemName = @Name;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Name", itemName);

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new SalesItemCounterRecord
							{
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							};
						}
					}
				}
			}

			return readItem;

		}

		public async Task<SalesItemCounterRecord> GetMonthlyItemAggregationRecordsCount(string itemName)
		{

			SalesItemCounterRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select count(*) as Count from monthly_item_sales where ItemName = @Name;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Name", itemName);

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new SalesItemCounterRecord
							{
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							};
						}
					}
				}
			}

			return readItem;

		}

		public async Task<SalesItemCounterRecord> GetYearlyItemAggregationRecordsCount(string itemName)
		{

			SalesItemCounterRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select count(*) as Count from yearly_item_sales where ItemName = @Name;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Name", itemName);

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new SalesItemCounterRecord
							{
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							};
						}
					}
				}
			}

			return readItem;

		}

		private async Task AddItemName(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from sales_items where ItemName = @ItemName;";
			SalesItemCounterRecord existingItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						existingItem = new SalesItemCounterRecord
						{
							ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
							Count = reader.GetInt32(reader.GetOrdinal("Count"))
						};
					}
				}

			}

			if (existingItem != null)
			{
				string query2 = @"update sales_items set Count = Count + 1 where ItemName = @ItemName;";
				using (SqliteCommand command = new SqliteCommand(query2, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);

					await command.ExecuteNonQueryAsync();
				}
			}
			else
			{
				string query2 = @"insert into sales_items values (@ItemName, @Count);";
				using (SqliteCommand command = new SqliteCommand(query2, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);
					command.Parameters.AddWithValue("@Count", 1);

					await command.ExecuteNonQueryAsync();
				}
			}

		}

		private async Task RemoveItemName(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"update sales_items set Count = Count - 1 where ItemName = @ItemName;";
			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);

				await command.ExecuteNonQueryAsync();
			}

			string query2 = @"select * from sales_items where ItemName = @ItemName;";
			SalesItemCounterRecord existingItem = null;

			using (SqliteCommand command = new SqliteCommand(query2, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						existingItem = new SalesItemCounterRecord
						{
							ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
							Count = reader.GetInt32(reader.GetOrdinal("Count"))
						};
					}
				}

			}

			if (existingItem != null && existingItem.Count <= 0)
			{
				string query3 = @"delete from sales_items where ItemName = @ItemName;";
				using (SqliteCommand command = new SqliteCommand(query3, connection, transaction))
				{
					command.Parameters.AddWithValue("@ItemName", item.ItemName);

					await command.ExecuteNonQueryAsync();
				}
			}

		}

		private async Task<AggregatedRecord> ReadRecordFromWeeklyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from weekly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("Total")),
							StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
							EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
						};
					}
				}
			}

			return readItem;

		}

		private async Task<AggregatedRecord> ReadRecordFromMonthlyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from monthly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("Total")),
							StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
							EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
						};
					}
				}
			}

			return readItem;

		}

		private async Task<AggregatedRecord> ReadRecordFromYearlyItemAggregation(SalesRecord item, SqliteConnection connection, SqliteTransaction transaction)
		{

			string query = @"select * from yearly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date";
			AggregatedRecord readItem = null;

			using (SqliteCommand command = new SqliteCommand(query, connection, transaction))
			{
				command.Parameters.AddWithValue("@ItemName", item.ItemName);
				command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

				using (SqliteDataReader reader = await command.ExecuteReaderAsync())
				{
					if (await reader.ReadAsync())
					{
						readItem = new AggregatedRecord
						{
							TotalAmount = reader.GetInt32(reader.GetOrdinal("Total")),
							StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
							EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
						};
					}
				}
			}

			return readItem;

		}

		public async Task<List<SalesRecord>> ReadPaginatedSalesRecords(int pageSize, int currentPage)
		{

			List<SalesRecord> dataList = new List<SalesRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from sales order by Date desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							SalesRecord item = new SalesRecord
							{
								Id = reader.GetInt32(reader.GetOrdinal("id")),
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								ItemQty = reader.GetInt32(reader.GetOrdinal("ItemQty")),
								ItemRate = reader.GetDouble(reader.GetOrdinal("ItemRate")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								Date = reader.GetDateTime(reader.GetOrdinal("Date"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<List<SalesRecord>> QueryPaginatedSalesRecords(string name, DateTime start, DateTime end, int pageSize, int currentPage)
		{

			List<SalesRecord> dataList = new List<SalesRecord>();

			try
			{
				using (SqliteConnection connection = new SqliteConnection(ConnectionString))
				{

					await connection.OpenAsync();

					string query = "select * from sales";

					var hasQuery = false;

					if (name != null && name.Length > 0)
					{
						hasQuery = true;
						query += " where ItemName = @ItemName";
					}

					if (hasQuery)
					{
						query += " and";
					}
					else
					{
						query += " where";
					}
					query += " Date >= @StartDate and Date <= @EndDate";

					query += $" order by Date desc limit {pageSize} offset {currentPage * pageSize}";

					using (SqliteCommand command = new SqliteCommand(query, connection))
					{

						if (name != null)
						{
							command.Parameters.AddWithValue("@ItemName", name);
						}

						if (start != null)
						{
							command.Parameters.AddWithValue("@StartDate", start.ToString("yyyy-MM-dd"));
						}

						if (end != null)
						{
							command.Parameters.AddWithValue("@EndDate", end.ToString("yyyy-MM-dd"));
						}

						using (SqliteDataReader reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								SalesRecord item = new SalesRecord
								{
									Id = reader.GetInt32(reader.GetOrdinal("id")),
									ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
									ItemQty = reader.GetInt32(reader.GetOrdinal("ItemQty")),
									ItemRate = reader.GetDouble(reader.GetOrdinal("ItemRate")),
									Total = reader.GetDouble(reader.GetOrdinal("Total")),
									Date = reader.GetDateTime(reader.GetOrdinal("Date"))
								};
								dataList.Add(item);
							}
						}
					}

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: " + ex.Message);
			}

			return dataList;

		}

		public async Task<List<AggregatedRecord>> ReadPaginatedWeeklyAggregation(int pageSize, int currentPage)
		{

			List<AggregatedRecord> dataList = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from weekly_sales order by StartDate desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<List<AggregatedRecord>> ReadPaginatedMonthlyAggregation(int pageSize, int currentPage)
		{

			List<AggregatedRecord> dataList = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from monthly_sales order by StartDate desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<List<AggregatedRecord>> ReadPaginatedYearlyAggregation(int pageSize, int currentPage)
		{

			List<AggregatedRecord> dataList = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from yearly_sales order by StartDate desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<AggregatedRecord> ReadLifetimeAggregation()
		{

			AggregatedRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"select * from lifetime_sales";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new AggregatedRecord
							{
								TotalAmount = reader.GetInt32(reader.GetOrdinal("TotalAmount"))
							};
						}
					}
				}
			}

			return readItem;

		}

		public async Task<List<AggregatedRecord>> ReadLifetimeYearlyAggregation()
		{

			List<AggregatedRecord> dataList = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = "select * from yearly_sales order by StartDate desc";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<List<List<AggregatedRecord>>> ReadWeeklyForecast(DateTime date)
		{

			List<List<AggregatedRecord>> list = new List<List<AggregatedRecord>>();
			List<AggregatedRecord> actualData = new List<AggregatedRecord>();
			List<AggregatedRecord> trainingData = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from weekly_sales where StartDate >= @date order by StartDate asc limit 4";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@date", date.AddDays(-(int)date.DayOfWeek).ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							actualData.Add(item);
						}
					}
				}

				string query2 = $"select * from weekly_sales where StartDate < @date order by StartDate desc limit 4";

				using (SqliteCommand command = new SqliteCommand(query2, connection))
				{

					command.Parameters.AddWithValue("@date", date.AddDays(-(int)date.DayOfWeek).ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							trainingData.Add(item);
						}
					}
				}

				list.Add(actualData);
				list.Add(trainingData);

			}

			return list;

		}

		public async Task<List<List<AggregatedRecord>>> ReadMonthlyForecast(DateTime date)
		{

			List<List<AggregatedRecord>> list = new List<List<AggregatedRecord>>();
			List<AggregatedRecord> actualData = new List<AggregatedRecord>();
			List<AggregatedRecord> trainingData = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from monthly_sales where StartDate >= @date order by StartDate asc limit 4";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@date", new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							actualData.Add(item);
						}
					}
				}

				string query2 = $"select * from monthly_sales where StartDate < @date order by StartDate desc limit 4";

				using (SqliteCommand command = new SqliteCommand(query2, connection))
				{

					command.Parameters.AddWithValue("@date", new DateTime(date.Year, date.Month, 1).ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							trainingData.Add(item);
						}
					}
				}

				list.Add(actualData);
				list.Add(trainingData);

			}

			return list;

		}

		public async Task<List<List<AggregatedRecord>>> ReadYearlyForecast(DateTime date)
		{

			List<List<AggregatedRecord>> list = new List<List<AggregatedRecord>>();
			List<AggregatedRecord> actualData = new List<AggregatedRecord>();
			List<AggregatedRecord> trainingData = new List<AggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from yearly_sales where StartDate >= @date order by StartDate asc limit 4";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@date", new DateTime(date.Year, 1, 1).ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							actualData.Add(item);
						}
					}
				}

				string query2 = $"select * from yearly_sales where StartDate < @date order by StartDate desc limit 4";

				using (SqliteCommand command = new SqliteCommand(query2, connection))
				{

					command.Parameters.AddWithValue("@date", new DateTime(date.Year, 1, 1).ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							AggregatedRecord item = new AggregatedRecord
							{
								TotalAmount = reader.GetDouble(reader.GetOrdinal("TotalAmount")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							trainingData.Add(item);
						}
					}
				}

				list.Add(actualData);
				list.Add(trainingData);

			}

			return list;

		}

		public async Task<AggregatedRecord> ReadLastEntryDate()
		{

			AggregatedRecord readItem = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = "select * from sales order by Date desc limit 1";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							readItem = new AggregatedRecord
							{
								StartDate = reader.GetDateTime(reader.GetOrdinal("Date"))
							};
						}
					}
				}
			}

			return readItem;

		}

		public async Task<List<SalesItemCounterRecord>> GetSalesItemList()
		{
			List<SalesItemCounterRecord> list = new List<SalesItemCounterRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();

				string query = "select * from sales_items";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							list.Add(new SalesItemCounterRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Count = reader.GetInt32(reader.GetOrdinal("Count"))
							});
						}
					}
				}
			}

			return list;
		}

		public async Task<ItemAggregatedRecord> ReadWeeklyItemSale(string itemName, DateTime date)
		{

			ItemAggregatedRecord item = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = "select * from weekly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@ItemName", itemName);
					command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							item = new ItemAggregatedRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
						}
					}

				}

			}

			return item;

		}

		public async Task<ItemAggregatedRecord> ReadMonthlyItemSale(string itemName, DateTime date)
		{

			ItemAggregatedRecord item = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = "select * from monthly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@ItemName", itemName);
					command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							item = new ItemAggregatedRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
						}
					}

				}

			}

			return item;

		}

		public async Task<ItemAggregatedRecord> ReadYearlyItemSale(string itemName, DateTime date)
		{

			ItemAggregatedRecord item = null;

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = "select * from yearly_item_sales where ItemName = @ItemName and StartDate <= @Date and EndDate >= @Date;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@ItemName", itemName);
					command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						if (await reader.ReadAsync())
						{
							item = new ItemAggregatedRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
						}
					}

				}

			}

			return item;

		}

		public async Task<List<ItemAggregatedRecord>> ReadPaginatedWeeklyItemAggregation(string itemName, int pageSize, int currentPage)
		{

			List<ItemAggregatedRecord> dataList = new List<ItemAggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from weekly_item_sales where ItemName = @Name order by StartDate desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Name", itemName);

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							ItemAggregatedRecord item = new ItemAggregatedRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<List<ItemAggregatedRecord>> ReadPaginatedMonthlyItemAggregation(string itemName, int pageSize, int currentPage)
		{

			List<ItemAggregatedRecord> dataList = new List<ItemAggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from monthly_item_sales where ItemName = @Name order by StartDate desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Name", itemName);

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							ItemAggregatedRecord item = new ItemAggregatedRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task<List<ItemAggregatedRecord>> ReadPaginatedYearlyItemAggregation(string itemName, int pageSize, int currentPage)
		{

			List<ItemAggregatedRecord> dataList = new List<ItemAggregatedRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = $"select * from yearly_item_sales where ItemName = @Name order by StartDate desc limit {pageSize} offset {currentPage * pageSize}";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Name", itemName);

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							ItemAggregatedRecord item = new ItemAggregatedRecord
							{
								ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
								Qty = reader.GetInt32(reader.GetOrdinal("Qty")),
								Total = reader.GetDouble(reader.GetOrdinal("Total")),
								StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
								EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate"))
							};
							dataList.Add(item);
						}
					}
				}

			}

			return dataList;

		}

		public async Task InsertNote(NotesRecord item)
		{

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"insert into notes (Note, Date) values (@Note, @Date);";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Note", item.Note);
					command.Parameters.AddWithValue("@Date", item.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();

				}

			}

		}

		public async Task UpdateNote(NotesRecord oldItem, NotesRecord newItem)
		{

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"update notes set Note = @Note, Date = @Date where Id = @Id;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Id", oldItem.Id);
					command.Parameters.AddWithValue("@Note", newItem.Note);
					command.Parameters.AddWithValue("@Date", newItem.Date.ToString("yyyy-MM-dd"));

					await command.ExecuteNonQueryAsync();

				}

			}

		}

		public async Task DeleteNote(NotesRecord oldItem)
		{

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{

				await connection.OpenAsync();

				string query = @"delete from notes where Id = @Id;";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Id", oldItem.Id);

					await command.ExecuteNonQueryAsync();

				}

			}

		}

		public async Task<List<NotesRecord>> ReadNotesPaginated(int pageSize, int currentPage)
		{

			List<NotesRecord> notes = new List<NotesRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();

				string query = $"select * from notes order by Date desc limit {pageSize} offset {currentPage * pageSize};";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{
					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							notes.Add(
								new NotesRecord
								{
									Id = reader.GetInt32(reader.GetOrdinal("Id")),
									Note = reader.GetString(reader.GetOrdinal("Note")),
									Date = reader.GetDateTime(reader.GetOrdinal("Date")),
								}
							);
						}
					}
				}
			}

			return notes;

		}

		public async Task<List<NotesRecord>> QueryNotesPaginated(DateTime start, DateTime end, int pageSize, int currentPage)
		{

			List<NotesRecord> notes = new List<NotesRecord>();

			using (SqliteConnection connection = new SqliteConnection(ConnectionString))
			{
				await connection.OpenAsync();

				string query = $"select * from notes where Date >= @Start and Date <= @End order by Date desc limit {pageSize} offset {currentPage * pageSize};";

				using (SqliteCommand command = new SqliteCommand(query, connection))
				{

					command.Parameters.AddWithValue("@Start", start.ToString("yyyy-MM-dd"));
					command.Parameters.AddWithValue("@End", end.ToString("yyyy-MM-dd"));

					using (SqliteDataReader reader = await command.ExecuteReaderAsync())
					{
						while (await reader.ReadAsync())
						{
							notes.Add(
								new NotesRecord
								{
									Id = reader.GetInt32(reader.GetOrdinal("Id")),
									Note = reader.GetString(reader.GetOrdinal("Note")),
									Date = reader.GetDateTime(reader.GetOrdinal("Date")),
								}
							);
						}
					}
				}
			}

			return notes;

		}

	}
}