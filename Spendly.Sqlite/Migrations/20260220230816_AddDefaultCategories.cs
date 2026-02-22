using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spendly.Sqlite.Migrations
{
	/// <inheritdoc />
	public partial class AddDefaultCategories : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.InsertData(
				table: "categories",
				columns: new[] { "id", "name" },
				values: new object[,]
				{
					{ Guid.Parse("8f26f478-0c40-4b3f-89f0-b5a20c590754"), "Groceries" },
					{ Guid.Parse("706e6a8d-3ec8-4db5-ab18-d9439cca7969"), "Sports" },
					{ Guid.Parse("f80a4cfb-efcd-4098-92d0-55e84f9d61f5"), "Entertainment" },
					{ Guid.Parse("cdaddc65-bac1-4c1b-a41f-a9b3c802f844"), "Cafes" },
					{ Guid.Parse("e0488ce8-43b9-4c35-a4ae-c2423ba2ba2b"), "Health" },
					{ Guid.Parse("7aa84b6d-4567-489b-985e-049fc5c2adb6"), "Transport" },
					{ Guid.Parse("ec6dc327-41f5-47b2-ab47-28f94219c87b"), "Rent" },
					{ Guid.Parse("7b1cbb60-be58-44a2-b3c8-84d9f2aedeea"), "Subscriptions" },
					{ Guid.Parse("4c870269-df19-4585-b721-016263df6098"), "Education" },
					{ Guid.Parse("23bc895d-68ec-4b83-bf3c-861bd66951bb"), "Clothing" },
					{ Guid.Parse("757bd3d2-bb54-4834-bad5-5ccfe88c1f01"), "Gifts" },
					{ Guid.Parse("1758c56f-507c-4712-98db-a02cdf655776"), "Travel" },
					{ Guid.Parse("e48da97d-3a58-4d21-9312-23ad6c5c168a"), "Insurance" },
					{ Guid.Parse("60e00e2c-fbe0-4537-a336-0aac8bf60c4f"), "Taxes" },
					{ Guid.Parse("b76ce3d2-1d2b-49df-9c01-8d298a3a759e"), "Investments" },
					{ Guid.Parse("ef7f9de1-78bf-46cb-ba9c-04be4301367f"), "Other" }
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("""
			                         DELETE FROM categories
			                         WHERE id IN (
			                           '8f26f478-0c40-4b3f-89f0-b5a20c590754',
			                           '706e6a8d-3ec8-4db5-ab18-d9439cca7969',
			                           'f80a4cfb-efcd-4098-92d0-55e84f9d61f5',
			                           'cdaddc65-bac1-4c1b-a41f-a9b3c802f844',
			                           'e0488ce8-43b9-4c35-a4ae-c2423ba2ba2b',
			                           '7aa84b6d-4567-489b-985e-049fc5c2adb6',
			                           'ec6dc327-41f5-47b2-ab47-28f94219c87b',
			                           '7b1cbb60-be58-44a2-b3c8-84d9f2aedeea',
			                           '4c870269-df19-4585-b721-016263df6098',
			                           '23bc895d-68ec-4b83-bf3c-861bd66951bb',
			                           '757bd3d2-bb54-4834-bad5-5ccfe88c1f01',
			                           '1758c56f-507c-4712-98db-a02cdf655776',
			                           'e48da97d-3a58-4d21-9312-23ad6c5c168a',
			                           '60e00e2c-fbe0-4537-a336-0aac8bf60c4f',
			                           'b76ce3d2-1d2b-49df-9c01-8d298a3a759e',
			                           'ef7f9de1-78bf-46cb-ba9c-04be4301367f'
			                         );
			                     """);
		}
	}
}