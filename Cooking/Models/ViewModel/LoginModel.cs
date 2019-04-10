using System.ComponentModel.DataAnnotations;

namespace Cooking.Models.ViewModel
{
	public class LoginModel
	{
		public int Id { get; set; }
		
		[Required(ErrorMessage = "Не указано имя")]
		public string Name { get; set; }
		 
		[Required(ErrorMessage = "Не указан пароль")]
		public string Password { get; set; }
		
		public string GlobalError { get; set;  }
	}
}