﻿using Microsoft.EntityFrameworkCore;
using SanTech.Interfaces;
using SanTech.Models;
using SanTech.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SanTech.Services
{
    public class DbProductService : IDbProductService
    {
        private readonly ApplicationContext db;
        private readonly IFileService fileService;
        public DbProductService(ApplicationContext db, IFileService fileService)
        {
            this.db = db;
            this.fileService = fileService;
        }

        public void Add(ProductViewModel createProduct)
        {
            var image = fileService.FromImageToByte(createProduct.UploadedFile);
            Product product = new Product(createProduct.Title, createProduct.Desc, createProduct.SaleProcent, createProduct.BonusNumber, createProduct.Cost, image, createProduct.CategoryId);
            db.Products.Add(product);
            db.SaveChanges();
        }

        public Product Get(int Id)
        {
            return db.Products.Include(x => x.Comments).ThenInclude(x => x.User).Include(x => x.Comments).ThenInclude(x => x.SubComments).ToList().FirstOrDefault(x => x.Id == Id);
        }

        public List<Product> GetAll()
        {
            var products = db.Products.Include(x => x.Comments).ToList();
            return products;
        }

        public IEnumerable<Product> GetProductsInRange(int from, int count, IEnumerable<Product> products)
        {
            if (from < 0 || count <= 0)
                throw new ArgumentOutOfRangeException();
            return products.Skip(from).Take(count);
        }
        public void DeleteProduct(int productId)
        {
            var product = db.Products.ToList().FirstOrDefault(x => x.Id == productId);
            var basket = db.Baskets.ToList().Where(x => x.Product.Id == productId);
            var subComments = db.SubComments.ToList().Where(x => x.Comment.Product.Id == productId);
            var comments = db.Comments.ToList().Where(x => x.Product.Id == productId);
            var favorites = db.Favorites.ToList().Where(x => x.Product.Id == productId);
            db.Baskets.RemoveRange(basket);
            db.SubComments.RemoveRange(subComments);
            db.Comments.RemoveRange(comments);
            db.Favorites.RemoveRange(favorites);
            db.Products.Remove(product);
            db.SaveChanges();
        }
        public void RedactProduct(ProductViewModel newProduct, int productId)
        {
            var product = db.Products.ToList().FirstOrDefault(x => x.Id == productId);
            if (newProduct.UploadedFile is not null)
            product.Image = fileService.FromImageToByte(newProduct.UploadedFile);
            product.Title = newProduct.Title;
            product.Desc = newProduct.Desc ?? product.Desc;
            product.Cost = newProduct.Cost;
            product.SaleProcent = newProduct.SaleProcent;
            product.BonusNumber = newProduct.BonusNumber;
            product.IsNotAvailable = newProduct.IsNotAvailable;
            db.SaveChanges();
        }

        public IEnumerable<Product> Get(SearchViewModel search)
        {
            var products = db.Products.Include(x => x.Comments).AsEnumerable();
            if (search.Category != 0)
                products = products.ToList().Where(x => x.CategoryId == search.Category);
            if (search.CostTo != 0)
            products = products.Where(x => x.Cost * (100 - x.SaleProcent) / 100 >= search.CostFrom && x.Cost * (100 - x.SaleProcent) / 100 <= search.CostTo);
            if (!String.IsNullOrEmpty(search.SearchInput))
                products = products.Where(x => x.Title.Contains(search.SearchInput, StringComparison.OrdinalIgnoreCase));
            switch (search.Sort)
            {
                case "CostDesc":
                    products =  products.OrderByDescending(x => x.Cost * (100 - x.SaleProcent) / 100);
                    break;
                case "CostAsc":
                    products = products.OrderBy(x => x.Cost * (100 - x.SaleProcent) / 100);
                    break;
                case "NameDesc":
                    products = products.OrderByDescending(x => x.Title);
                    break;
                case "NameAsc":
                    products = products.OrderBy(x => x.Title);
                    break;
                case "NewDesc":
                    products = products.OrderByDescending(x => x.Id);
                    break;
                case "NewAsc":
                    products = products.OrderBy(x => x.Id);
                    break;
                default:
                    products = products.OrderByDescending(x => x.Id);
                    break;
            }
            return products;
        }
    }
}
