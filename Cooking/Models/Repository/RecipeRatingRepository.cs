using System.Collections.Generic;
using System.Linq;
using Cooking.Models.DataModel;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Cooking.Models.Repository
{
	public class RecipeRatingRepository : BaseRepository
	{
		public RecipeRatingRepository(IConfiguration config) : base(config) { }
		
		public int Add(RecipeRating rating)
		{
			using (var conn = Connection)
			{
				return conn.ExecuteScalar<int>($@"
insert	recipe_rating (recipe, [user], score)
values	(@recipe, @user, @score)
select	scope_identity()
", new { recipe = rating.RecipeId, user = rating.User.Id, score = rating.Score });
			}
		}
		
		public IEnumerable<RecipeRating> GetByRecipeId(int id)
		{
			using (var conn = Connection)
			{
				return conn.Query<RecipeRating, User, RecipeRating>($@"
select	recipe_rating	{nameof(RecipeRating.Id)},
		score			{nameof(RecipeRating.Score)},
		recipe			{nameof(RecipeRating.RecipeId)},
		[user]			{nameof(RecipeRating.User.Id)}
from	recipe_rating
where	recipe = @{nameof(id)}
", param: new { id }, map: (recipe_rating, user) =>
					{
						recipe_rating.User = user;
						return recipe_rating;
					},
					splitOn: $"{nameof(RecipeRating.User.Id)}, {nameof(RecipeRating.RecipeId)}");
			}
		}
		
		public RecipeRating GetByUserIdAndRecipeId(int user_id, int recipe_id)
		{
			using (var conn = Connection)
			{
				return conn.Query<RecipeRating, User, RecipeRating>($@"
select	recipe_rating	{nameof(RecipeRating.Id)},
		score			{nameof(RecipeRating.Score)},
		recipe			{nameof(RecipeRating.RecipeId)},
		[user]			{nameof(RecipeRating.User.Id)}
from	recipe_rating
where	[user] = @{nameof(user_id)} and recipe = @{nameof(recipe_id)}
", param: new { user_id, recipe_id }, map: (recipe_rating, user) =>
					{
						recipe_rating.User = user;
						return recipe_rating;
					},
					splitOn: $"{nameof(RecipeRating.User.Id)}, {nameof(RecipeRating.RecipeId)}").FirstOrDefault();
			}
		}
		
		public void UpdateByUserAndRecipe(RecipeRating rating)
		{
			using (var conn = Connection)
			{
				conn.Execute($@"
update	recipe_rating
set		score = @score
where	[user] = @user and recipe = @recipe
", new { user = rating.User.Id, score = rating.Score, recipe = rating.RecipeId });
			}
		}
	}
}