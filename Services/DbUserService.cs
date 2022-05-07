﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SanTech.Interfaces;
using SanTech.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SanTech.Services
{
    public class DbUserService : IDbUserService
    {
        private readonly ApplicationContext db;
        private readonly IFileService fileService;
        private readonly IHostingEnvironment hostingEnvironment;
        public DbUserService(ApplicationContext db, IFileService fileService, IHostingEnvironment hostingEnvironment)
        {
            this.db = db;
            this.fileService = fileService;
            this.hostingEnvironment = hostingEnvironment;
        }
        public void Add(User user)
        {
           /* string path = Path.Combine(this.hostingEnvironment.WebRootPath, "img", "default_avatar.png");
            using (Stream fileStream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            using (IFormFile stream = new (path))
            {
                user.Avatar = fileService.FromImageToByte(stream);
            }*/
            db.Users.Add(user);
            db.SaveChanges();
        }

        public void AddBonuses(Application application)
        {
            var user = db.Users.ToList().FirstOrDefault(x => x.Email == application.User.Email);
            user.Bonus += user.Basket.Sum(x => x.Product.BonusNumber);
            db.SaveChanges();
        }

        public void ChangePassword(User user, string password)
        {
            var newUser = db.Users.ToList().FirstOrDefault(x => x.Email == user.Email);
            newUser.Password = password;
            newUser.PasswordConfirm = password;
            db.SaveChanges();
        }

        public int ClearBonuses(Application application)
        {
            var user = db.Users.ToList().FirstOrDefault(x => x.Email == application.User.Email);
            if(application.TotalCost >= application.User.Bonus)
            {
                application.TotalCost -= application.User.Bonus;
                user.Bonus = 0;
            }
            else
            {
                user.Bonus -= application.TotalCost;
                application.TotalCost = 0;
            }
            db.SaveChanges();
            return application.TotalCost;
        }

        public User Get(string email)
        {
            return db.Users.Include(x => x.Basket).ToList().FirstOrDefault(x => x.Email == email);
        }

        public IEnumerable<User> GetAll()
        {
            return db.Users.ToList();
        }

        public User GetUserByEmail(string email)
        {
            return db.Users.ToList().FirstOrDefault(x => x.Email == email);
        }

        public string HashData(string data)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                bytes = md5.ComputeHash(bytes);
                StringBuilder stringBuilder = new StringBuilder();
                foreach(byte u in bytes)
                {
                    stringBuilder.Append(u.ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }

        public string HashTokenFromUser(User user)
        {
            string data = user.Email + user.Phone + user.Password;
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                bytes = md5.ComputeHash(bytes);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (byte u in bytes)
                {
                    stringBuilder.Append(u.ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }
}
