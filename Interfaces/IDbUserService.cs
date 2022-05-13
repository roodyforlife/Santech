﻿using Microsoft.AspNetCore.Http;
using SanTech.Models;
using System.Collections.Generic;

namespace SanTech.Interfaces
{
    public interface IDbUserService
    {
        public void Add(User user);
        public User Get(string email);
        public IEnumerable<User> GetAll();
        public int ClearBonuses(Order order);
        public void AddBonuses(Application application);
        public void ChangePassword(User user, string password);
        public void UpdateUser(User newUser, string userEmail, IFormFile UploadedFile);
    }
}
