using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cooking.Models.DataModel;
using Cooking.Models.Repository;
using Cooking.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cooking.Controllers
{
	[Authorize]
	public class ClientController : Controller
	{
		private const int EASY_RECIPRE_CATEGORY_ID = 1;
		private const int MIDDLE_RECIPRE_CATEGORY_ID = 2;
		private const int HARD_RECIPRE_CATEGORY_ID = 3;
		
		private readonly RecipeCategoryRepository _recipeCategoryRepository;
		private readonly RecipeRepository _recipeRepository;
		private readonly UserRepository _userRepository;
		private readonly RecipeRatingRepository _recipeRatingRepository;
		
		public ClientController(RecipeCategoryRepository recipe_category_repository,
			RecipeRepository recipe_repository,
			UserRepository user_repository,
			RecipeRatingRepository recipe_rating_repository)
		{
			_recipeCategoryRepository = recipe_category_repository;
			_recipeRepository = recipe_repository;
			_userRepository = user_repository;
			_recipeRatingRepository = recipe_rating_repository;
		}
		
		public IActionResult Index()
		{
			var recipe_categories = _recipeCategoryRepository.Get();
			
			return View(recipe_categories);
		}
		
		[HttpGet]
		public IActionResult CreateRecipe(string order_by = null)
		{
			var recipe_model = new RecipeModel
			{
				RecipeCategories = _recipeCategoryRepository.Get(),
				OrderBy = order_by
			};
			
			return View(recipe_model);
		}
		
		[HttpPost]
		public IActionResult CreateRecipe([Bind("Id,Name,RecipeDescription,RecipeCategories,RecipeCategory,GlobalError")] RecipeModel recipe_model)
		{
			if (ModelState.IsValid &&  HttpContext.Request.Form.Files.Count > 0)
			{
				var recipe = _recipeRepository.GetByName(recipe_model.Name);
				if (recipe == null)
				{
					recipe = CastModelToRecipe(recipe_model);
					
					var pic = HttpContext.Request.Form.Files[0];
					using (var ms = new MemoryStream())
					{
						pic.CopyTo(ms);
						recipe.Picture = ms.ToArray();
					}
					
					recipe.Creator = GetCurrentUser();
					recipe.AddDate = DateTime.Now;
					recipe.ChangeDate = DateTime.Now;
					
					var recipe_id = _recipeRepository.Create(recipe);
					
					return RedirectToRecipes(recipe_id);
				}
				ModelState.AddModelError("GlobalError", "Такой рецепт уже существует.");
			}
			recipe_model.RecipeCategories = _recipeCategoryRepository.Get();
			return View(recipe_model);
		}
		
		public IActionResult EasyRecipes(string order_by = null)
		{
			var recipes = FillRecipesWithCreatorsAndRatings( _recipeRepository.GetByCategoryId(EASY_RECIPRE_CATEGORY_ID));
			
			ViewBag.OrderBy = order_by;
			
			return View(OrderRecipes(recipes, order_by));
		}
		
		public IActionResult MiddleRecipes(string order_by = null)
		{
			var recipes = FillRecipesWithCreatorsAndRatings( _recipeRepository.GetByCategoryId(MIDDLE_RECIPRE_CATEGORY_ID));
			
			ViewBag.OrderBy = order_by;
			
			return View(OrderRecipes(recipes, order_by));
		}
		
		public IActionResult HardRecipes(string order_by = null)
		{
			var recipes = FillRecipesWithCreatorsAndRatings( _recipeRepository.GetByCategoryId(HARD_RECIPRE_CATEGORY_ID));
			
			ViewBag.OrderBy = order_by;
			
			return View(OrderRecipes(recipes, order_by));
		}
		
		[HttpGet]
		public IActionResult EditRecipe(int id, string order_by = null)
		{
			var recipe_model = CastRecipeToModel(_recipeRepository.GetById(id));
			recipe_model.OrderBy = order_by;
			
			return View(recipe_model);
		}
		
		[HttpPost]
		public IActionResult EditRecipe(RecipeModel recipe_model)
		{
			if (ModelState.IsValid)
			{
				var recipe = _recipeRepository.GetById(recipe_model.Id);
				var picture = recipe.Picture;
				var possible_recipe = _recipeRepository.GetByName(recipe_model.Name);
				if (recipe.Name == recipe_model.Name || possible_recipe == null)
				{
					recipe = CastModelToRecipe(recipe_model);
					
					if (HttpContext.Request.Form.Files.Count > 0)
					{
						var pic = HttpContext.Request.Form.Files[0];
						using (var ms = new MemoryStream())
						{
							pic.CopyTo(ms);
							recipe.Picture = ms.ToArray();
						}
					}
					else
					{
						recipe.Picture = picture;
					}
					
					recipe.ChangeDate = DateTime.Now;
					_recipeRepository.Update(recipe);
					
					return RedirectToRecipes(recipe.Id, recipe_model.OrderBy);
				}
				
				ModelState.AddModelError("GlobalError", "Такой рецепт уже существует.");
			}
			recipe_model.RecipeCategories = _recipeCategoryRepository.Get();
			return View(recipe_model);
		}
		
		[HttpPost]
		public IActionResult DeleteRecipe(int id, string order_by = null)
		{
			var recipe = _recipeRepository.GetById(id);
			var category = recipe.RecipeCategory.Id;
			_recipeRepository.Delete(id);
			
			return RedirectToRecipesByCategory(category, order_by);
		}
		
		public IActionResult ScoreRecipe(int recipe_id, string score, string order_by = null)
		{
			int.TryParse(score, out var score_int);
			var recipe_rating = new RecipeRating
			{
				RecipeId = recipe_id,
				User = GetCurrentUser(),
				Score = score_int
			};
			_recipeRatingRepository.Add(recipe_rating);
			
			return RedirectToRecipes(recipe_id, order_by);
		}
		
		public IActionResult EditScoreRecipe(int recipe_id, string score, string order_by = null)
		{
			int.TryParse(score, out var score_int);
			var recipe_rating = new RecipeRating
			{
				RecipeId = recipe_id,
				User = GetCurrentUser(),
				Score = score_int
			};
			_recipeRatingRepository.UpdateByUserAndRecipe(recipe_rating);
			
			return RedirectToRecipes(recipe_id, order_by);
		}
		
		private IEnumerable<Recipe> OrderRecipes(IEnumerable<Recipe> recipes, string order_by = null)
		{
			if (recipes != null)
			{
				switch (order_by)
				{
					case "rating":
						return recipes.OrderByDescending(x => x.GetScore());
					case "name":
						return recipes.OrderBy(x => x.Name);
					case "add_date":
						return recipes.OrderByDescending(x => x.AddDate);
					case "change_date":
						return recipes.OrderByDescending(x => x.ChangeDate);
					default:
						return recipes.OrderBy(x => x.AddDate);
				}
			}
			return recipes;
		}
		
		private IActionResult RedirectToRecipes(int recipe_id, string order_by = null)
		{
			var recipe = _recipeRepository.GetById(recipe_id);
			
			return RedirectToRecipesByCategory(recipe.RecipeCategory.Id, order_by);
		}
		
		private IActionResult RedirectToRecipesByCategory(int category, string order_by = null)
		{
			switch (category)
			{
				case 1:
					return RedirectToAction("EasyRecipes", new { order_by });
				case 2:
					return RedirectToAction("MiddleRecipes", new { order_by });
				case 3:
					return RedirectToAction("HardRecipes", new { order_by });
				default:
					return RedirectToAction("Index", new { order_by });
			}
		}
		
		private User GetCurrentUser()
		{
			return _userRepository.GetByName(User.Identity.Name);
		}
		
		private Recipe CastModelToRecipe(RecipeModel recipe_model)
		{
			return new Recipe
			{
				Id = recipe_model.Id,
				Name = recipe_model.Name,
				RecipeDescription = recipe_model.RecipeDescription,
				Picture = recipe_model.Picture,
				RecipeCategory = recipe_model.RecipeCategory,
			};
		}
		
		private RecipeModel CastRecipeToModel(Recipe recipe)
		{
			return new RecipeModel
			{
				Id = recipe.Id,
				Name = recipe.Name,
				RecipeDescription = recipe.RecipeDescription,
				Picture = recipe.Picture,
				RecipeCategory = recipe.RecipeCategory,
				RecipeCategories = _recipeCategoryRepository.Get(),
			};
		}
		
		private IEnumerable<Recipe> FillRecipesWithCreatorsAndRatings(IEnumerable<Recipe> recipes)
		{
			foreach (var recipe in recipes)
			{
				recipe.Creator = _userRepository.GetById(recipe.Creator.Id);
				recipe.RecipeRatings = _recipeRatingRepository.GetByRecipeId(recipe.Id);
				recipe.UsersScore = _recipeRatingRepository.GetByUserIdAndRecipeId(GetCurrentUser().Id,
					recipe.Id)?.Score;
			}
			
			return recipes;
		}
	}
}