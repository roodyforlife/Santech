﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanTech.Models
{
    public class ProductCreate
    {
        public string Title { get; set; }
        public string Desc { get; set; }
        public int SaleProcent { get; set; }
        public int BonusNumber { get; set; }
        public int Cost { get; set; }
    }
}
