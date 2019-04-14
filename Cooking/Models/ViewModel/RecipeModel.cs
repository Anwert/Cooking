using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Cooking.Models.DataModel;

namespace Cooking.Models.ViewModel
{
	public class RecipeModel
	{
		public int Id { get; set; }
		
		[Required(ErrorMessage = "Не указано название")]
		public string Name { get; set; }
		
		[Required(ErrorMessage = "Не указано описание")]
		public string RecipeDescription { get; set; }
		
		public IEnumerable<RecipeCategory> RecipeCategories { get; set; }
		
		public RecipeCategory RecipeCategory { get; set; }
		
		public byte[] Picture { get; set; }
		
		public string OrderBy { get; set; }
		
		public string GlobalError { get; set; }
	}
}