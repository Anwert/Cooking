using System.Collections.Generic;
using System.Linq;
using Cooking.Models.DataModel;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Cooking.Models.Repository
{
	public class RecipeRepository : BaseRepository
	{
		public RecipeRepository(IConfiguration config) : base(config) { }
		
		public int Create(Recipe recipe)
		{
			using (var conn = Connection)
			{
				return conn.ExecuteScalar<int>($@"
insert	recipe (creator, name, add_date, change_date, recipe_description, picture, recipe_category)
values	(@creator_id, @name, @add_date, @change_date, @recipe_description, @picture, @recipe_category)
select	scope_identity()
", new
				{
					creator_id = recipe.Creator.Id,
					name = recipe.Name,
					add_date = recipe.AddDate,
					change_date = recipe.ChangeDate,
					recipe_description = recipe.RecipeDescription,
					picture = recipe.Picture,
					recipe_category = recipe.RecipeCategory.Id
				});
			}
		}
		
		public Recipe GetById(int id)
		{
			using (var conn = Connection)
			{
				return conn.Query<Recipe, User, RecipeCategory, Recipe>($@"
select	recipe 				{nameof(Recipe.Id)},
		name				{nameof(Recipe.Name)},
		add_date			{nameof(Recipe.AddDate)},
		change_date			{nameof(Recipe.ChangeDate)},
		recipe_description	{nameof(Recipe.RecipeDescription)},
		picture				{nameof(Recipe.Picture)},
		creator				{nameof(Recipe.Creator.Id)},
		recipe_category		{nameof(Recipe.RecipeCategory.Id)}
from	recipe
where	recipe = @{nameof(id)}
", param: new { id }, map: (recipe, creator, recipe_category) =>
					{
						recipe.Creator = creator;
						recipe.RecipeCategory = recipe_category;
						return recipe;
					},
					splitOn: $"{nameof(Recipe.Creator.Id)}, {nameof(Recipe.RecipeCategory.Id)}").FirstOrDefault();
			}
		}
		
		public Recipe GetByName(string name)
		{
			using (var conn = Connection)
			{
				return conn.Query<Recipe, User, RecipeCategory, Recipe>($@"
select	recipe 				{nameof(Recipe.Id)},
		name				{nameof(Recipe.Name)},
		add_date			{nameof(Recipe.AddDate)},
		change_date			{nameof(Recipe.ChangeDate)},
		recipe_description	{nameof(Recipe.RecipeDescription)},
		picture				{nameof(Recipe.Picture)},
		creator				{nameof(Recipe.Creator.Id)},
		recipe_category		{nameof(Recipe.RecipeCategory.Id)}
from	recipe
where	name = @{nameof(name)}
", param: new { name }, map: (recipe, creator, recipe_category) =>
					{
						recipe.Creator = creator;
						recipe.RecipeCategory = recipe_category;
						return recipe;
					},
					splitOn: $"{nameof(Recipe.Creator.Id)}, {nameof(Recipe.RecipeCategory.Id)}").FirstOrDefault();
			}
		}
		
		public IEnumerable<Recipe> GetAll()
		{
			using (var conn = Connection)
			{
				return conn.Query<Recipe, User, RecipeCategory, Recipe>($@"
select	recipe 				{nameof(Recipe.Id)},
		name				{nameof(Recipe.Name)},
		add_date			{nameof(Recipe.AddDate)},
		change_date			{nameof(Recipe.ChangeDate)},
		recipe_description	{nameof(Recipe.RecipeDescription)},
		picture				{nameof(Recipe.Picture)},
		creator				{nameof(Recipe.Creator.Id)},
		recipe_category		{nameof(Recipe.RecipeCategory.Id)}
from	recipe
", map: (recipe, creator, recipe_category) =>
					{
						recipe.Creator = creator;
						recipe.RecipeCategory = recipe_category;
						return recipe;
					},
					splitOn: $"{nameof(Recipe.Creator.Id)}, {nameof(Recipe.RecipeCategory.Id)}");
			}
		}
		
		public IEnumerable<Recipe> GetByCategoryId(int category_id)
		{
			using (var conn = Connection)
			{
				return conn.Query<Recipe, User, RecipeCategory, Recipe>($@"
select	recipe 				{nameof(Recipe.Id)},
		name				{nameof(Recipe.Name)},
		add_date			{nameof(Recipe.AddDate)},
		change_date			{nameof(Recipe.ChangeDate)},
		recipe_description	{nameof(Recipe.RecipeDescription)},
		picture				{nameof(Recipe.Picture)},
		creator				{nameof(Recipe.Creator.Id)},
		recipe_category		{nameof(Recipe.RecipeCategory.Id)}
from	recipe
where	recipe_category = @{nameof(category_id)}
", param: new { category_id }, map: (recipe, creator, recipe_category) =>
					{
						recipe.Creator = creator;
						recipe.RecipeCategory = recipe_category;
						return recipe;
					},
					splitOn: $"{nameof(Recipe.Creator.Id)}, {nameof(Recipe.RecipeCategory.Id)}");
			}
		}
		
		public void Update(Recipe recipe)
		{
			using (var conn = Connection)
			{
				conn.Execute($@"
update	recipe
set		name				= @name,
		change_date			= @change_date,
		recipe_description	= @recipe_description,
		picture				= @picture,
		recipe_category		= @recipe_category
from	recipe
where	recipe		= @id
", new
				{
					id = recipe.Id,
					name = recipe.Name,
					change_date = recipe.ChangeDate,
					recipe_description = recipe.RecipeDescription,
					picture = recipe.Picture,
					recipe_category = recipe.RecipeCategory.Id
				});
			}
		}
		
		public void Delete(int id)
		{
			using (var conn = Connection)
			{
				conn.Execute($@"
delete	from recipe
where	recipe = @{nameof(id)}
", new { id });
			}
		}
	}
}