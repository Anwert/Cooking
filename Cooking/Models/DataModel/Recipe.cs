using System;
using System.Collections.Generic;
using System.Linq;

namespace Cooking.Models.DataModel
{
	public class Recipe
	{
		public int Id { get; set; }
		
		public string Name { get; set; }
		
		public User Creator { get; set; }
		
		public DateTime AddDate { get; set; }
		
		public DateTime ChangeDate { get; set; }
		
		public string RecipeDescription { get; set; }
		
		public byte[] Picture { get; set; }
		
		public RecipeCategory RecipeCategory { get; set; }
		
		public IEnumerable<RecipeRating> RecipeRatings { get; set; }
		
		public string GetScore()
		{
			var score = 0;
			var count = this.RecipeRatings?.Count();
			
			if (count > 0)
			{
				score += this.RecipeRatings.Sum(recipe_rating => recipe_rating.Score);

				return $"{(float)score / count}, проголосовавших: {count}";
			}
			return null;
		}
		
		public int? UsersScore { get; set; }
	}
}