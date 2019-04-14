namespace Cooking.Models.DataModel
{
	public class RecipeRating
	{
		public int Id { get; set; }
		
		public int RecipeId { get; set; }
		
		public User User { get; set; }
		
		public int Score { get; set; }
	}
}