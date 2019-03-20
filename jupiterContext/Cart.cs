﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace jupiterCore.jupiterContext
{
    public partial class Cart
    {
        public Cart()
        {
            CartProd = new HashSet<CartProd>();
        }

        public int CartId { get; set; }
        public decimal? Price { get; set; }
        public string Location { get; set; }
        public DateTime? PlannedTime { get; set; }
        public byte? IsActivate { get; set; }
        public DateTime? CreateOn { get; set; }
        public DateTime? UpdateOn { get; set; }
        public int? ContactId { get; set; }

        public virtual Contact Contact { get; set; }
        public virtual ICollection<CartProd> CartProd { get; set; }
    }
}
