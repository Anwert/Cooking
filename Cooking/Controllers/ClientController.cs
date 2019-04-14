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
		
		public IActionResult SortRecipesByRating(int category)
		{
			var recipes = _recipeRepository.GetByCategoryId(category)?.OrderByDescending(x => x.GetScore());
			
			return ReturnRecipesViewByCategoryAndRecipes(category, FillRecipesWithCreatorsAndRatings(recipes));
		}
		
		public IActionResult SortRecipesByName(int category)
		{
			var recipes = _recipeRepository.GetByCategoryId(category)?.OrderBy(x => x.Name);
			
			return ReturnRecipesViewByCategoryAndRecipes(category, FillRecipesWithCreatorsAndRatings(recipes));
		}
		
		public IActionResult SortRecipesByAddDate(int category)
		{
			var recipes = _recipeRepository.GetByCategoryId(category)?.OrderByDescending(x => x.AddDate);
			
			return ReturnRecipesViewByCategoryAndRecipes(category, FillRecipesWithCreatorsAndRatings(recipes));
		}
		
		public IActionResult SortRecipesByChangeDate(int category)
		{
			var recipes = _recipeRepository.GetByCategoryId(category)?.OrderByDescending(x => x.ChangeDate);
			
			return ReturnRecipesViewByCategoryAndRecipes(category, FillRecipesWithCreatorsAndRatings(recipes));
		}
		
		[HttpGet]
		public IActionResult CreateRecipe()
		{
			var recipe_model = new RecipeModel
			{
				RecipeCategories = _recipeCategoryRepository.Get()
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
		
		public IActionResult EasyRecipes()
		{
			var recipes = FillRecipesWithCreatorsAndRatings( _recipeRepository.GetByCategoryId(EASY_RECIPRE_CATEGORY_ID));

			return View(recipes);
		}
		
		public IActionResult MiddleRecipes()
		{
			var recipes = FillRecipesWithCreatorsAndRatings( _recipeRepository.GetByCategoryId(MIDDLE_RECIPRE_CATEGORY_ID));
			
			return View(recipes);
		}
		
		public IActionResult HardRecipes()
		{
			var recipes = FillRecipesWithCreatorsAndRatings( _recipeRepository.GetByCategoryId(HARD_RECIPRE_CATEGORY_ID));
			
			return View(recipes);
		}
		
		[HttpGet]
		public IActionResult EditRecipe(int id)
		{
			var recipe_model = CastRecipeToModel(_recipeRepository.GetById(id));
			
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
					
					return RedirectToRecipes(recipe.Id);
				}
				
				ModelState.AddModelError("GlobalError", "Такой рецепт уже существует.");
			}
			recipe_model.RecipeCategories = _recipeCategoryRepository.Get();
			return View(recipe_model);
		}
		
		[HttpPost]
		public IActionResult DeleteRecipe(int id)
		{
			var recipe = _recipeRepository.GetById(id);
			var category = recipe.RecipeCategory.Id;
			_recipeRepository.Delete(id);
			
			return RedirectToRecipesByCategory(category);
		}
		
		public IActionResult ScoreRecipe(int recipe_id, string score)
		{
			int.TryParse(score, out var score_int);
			var recipe_rating = new RecipeRating
			{
				RecipeId = recipe_id,
				User = GetCurrentUser(),
				Score = score_int
			};
			_recipeRatingRepository.Add(recipe_rating);
			
			return RedirectToRecipes(recipe_id);
		}
		
		public IActionResult EditScoreRecipe(int recipe_id, string score)
		{
			int.TryParse(score, out var score_int);
			var recipe_rating = new RecipeRating
			{
				RecipeId = recipe_id,
				User = GetCurrentUser(),
				Score = score_int
			};
			_recipeRatingRepository.UpdateByUserAndRecipe(recipe_rating);
			
			return RedirectToRecipes(recipe_id);
		}
		
		private IActionResult RedirectToRecipes(int recipe_id)
		{
			var recipe = _recipeRepository.GetById(recipe_id);
			return RedirectToRecipesByCategory(recipe.RecipeCategory.Id);
		}
		
		private IActionResult RedirectToRecipesByCategory(int category)
		{
			switch (category)
			{
				case 1:
					return RedirectToAction("EasyRecipes");
				case 2:
					return RedirectToAction("MiddleRecipes");
				case 3:
					return RedirectToAction("HardRecipes");
				default:
					return RedirectToAction("Index");
			}
		}
		
		private IActionResult ReturnRecipesViewByCategoryAndRecipes(int category, IEnumerable<Recipe> recipes)
		{
			switch (category)
			{
				case 1:
					return View("EasyRecipes", recipes);
				case 2:
					return View("MiddleRecipes", recipes);
				case 3:
					return View("HardRecipes", recipes);
				default:
					return View("Index");
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