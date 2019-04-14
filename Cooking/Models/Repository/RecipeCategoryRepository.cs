using System.Collections.Generic;
using Cooking.Models.DataModel;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Cooking.Models.Repository
{
	public class RecipeCategoryRepository : BaseRepository
	{
		public RecipeCategoryRepository(IConfiguration config) : base(config) { }
		
		public IEnumerable<RecipeCategory> Get()
		{
			using (var conn = Connection)
			{
				return conn.Query<RecipeCategory>($@"
select	recipe_category	{nameof(RecipeCategory.Id)},
		name			{nameof(RecipeCategory.Name)}
from	recipe_category
");
			}
		}
	}
}