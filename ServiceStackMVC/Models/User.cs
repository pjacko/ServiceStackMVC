﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ServiceStack.DataAnnotations;

namespace ServiceStackMVC.Models
{
    public class User
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        [References(typeof(StateProvince))]
        public int StateProvinceId { get; set; }
        [Required]
        [Index(Unique = true)]
        public string Email { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string FacebookName { get; set; }
        public string FacebookFirstName { get; set; }
        public string FacebookLastName { get; set; }
        public string FacebookUserId { get; set; }
        public string FacebookUserName { get; set; }
        public string FacebookEmail { get; set; }
    }
}