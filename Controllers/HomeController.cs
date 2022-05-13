﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SanTech.Interfaces;
using SanTech.Models;
using SanTech.Models.ViewModels;
using System.Linq;

namespace SanTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDbUserService dbUserService;
        private readonly IDbProductService dbProductService;
        private readonly IDbBasketService dbBasketService;
        private readonly IDbFavoriteService dbFavoriteService;
        public HomeController(IDbUserService dbUserService, IDbProductService dbProductService, IDbBasketService dbBasketService, IDbFavoriteService dbFavoriteService)
        {
            this.dbUserService = dbUserService;
            this.dbProductService = dbProductService;
            this.dbBasketService = dbBasketService;
            this.dbFavoriteService = dbFavoriteService;
        }

        public IActionResult Index(SearchViewModel search)
        {
            var userEmail = HttpContext.Session.GetString("Email");
            ViewBag.LoggedAccount = userEmail;
            if (userEmail is not null)
            {
                ViewBag.User = dbUserService.Get(userEmail);
            }
            var allProducts = dbProductService.GetAll();
            var products = dbProductService.GetProductsInRange(0, 20, dbProductService.Get(search)).ToList();
            if (allProducts.Count() > 0)
                ViewBag.MaxCost = dbProductService.GetAll().Max(x => (x.Cost * (100 - x.SaleProcent) / 100));
            ViewBag.Search = search;
            return View(products);
        }

        public string SignOutAccount()
        {
            ControllerContext.HttpContext.Session.Remove("Email");
            return "<li><a href='../SignInAccount/SignInAccount'><div class='text1 pull__menu__list__text login__button'>Вход/Регистрация</div></a></li>";
        }

        [HttpPost]
        public ViewResult GetAdditionalProducts(int from, int count, SearchViewModel search)
        {
            var allProducts = dbProductService.Get(search);
            var products = dbProductService.GetProductsInRange(from, count, allProducts).ToList();
            return View(products);
        }

        public bool AddToBasket(int Id)
        {
            var userEmail = HttpContext.Session.GetString("Email");
            return dbBasketService.Add(userEmail, Id);
        }

        public bool AddToFavourites(int Id)
        {
            var userEmail = HttpContext.Session.GetString("Email");
            return dbFavoriteService.Add(userEmail, Id);
        }

        public void DeleteFromBasket(int basketId)
        {
            dbBasketService.Delete(basketId);
        }

        public void DeleteFromFavorites(int favoriteId)
        {
            dbFavoriteService.Delete(favoriteId);
        }

        public ViewResult LoadBasket()
        {
            var userEmail = HttpContext.Session.GetString("Email");
            ViewBag.User = userEmail;
            var model = dbBasketService.Get(userEmail);
            ViewBag.TotalCost = model.Sum(x => x.NumberOfProduct * (x.Product.Cost * (100 - x.Product.SaleProcent) / 100));
            return View(model);
        }

        public ViewResult LoadFavorites()
        {
            var userEmail = HttpContext.Session.GetString("Email");
            ViewBag.User = userEmail;
            var model = dbFavoriteService.Get(userEmail);
            return View(model);
        }

        public void ChangeNumberOfBasket(int basketId, int inputValue)
        {
            dbBasketService.ChangeNumberOfBasket(basketId, inputValue);
        }

        public void DeleteAllBasket()
        {
            var userEmail = HttpContext.Session.GetString("Email");
            dbBasketService.DeleteAll(userEmail);
        }

        public void DeleteAllFavorites()
        {
            var userEmail = HttpContext.Session.GetString("Email");
            dbFavoriteService.DeleteAll(userEmail);
        }

        [HttpGet]
        public IActionResult UpdateUser()
        {
            var userEmail = HttpContext.Session.GetString("Email");
            if (userEmail is not null)
            {
                ViewBag.User = dbUserService.Get(userEmail);
                return View();
            }
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateUser(User user, IFormFile UploadedFile)
        {
            var userEmail = HttpContext.Session.GetString("Email");
            dbUserService.UpdateUser(user, userEmail, UploadedFile);
            return RedirectToAction("Index");
        }
    }
}
