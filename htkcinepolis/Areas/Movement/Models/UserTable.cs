﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Movement.Models
{
    public class UserTable : MongoModel
    {
        public UserTable() : base("Users") 
        { 
        }
    }

}