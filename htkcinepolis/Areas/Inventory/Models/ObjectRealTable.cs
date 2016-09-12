﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Rivka.Db;
using Rivka.Db.MongoDb;

namespace RivkaAreas.Inventory.Models
{
    public class ObjectRealTable : MongoModel
    {
        public ObjectRealTable() : base ("ObjectReal")
        { 
        }
        public String getobjectjoin(string id)
        {
            //Query needed to get the result
            var query = Query.And(Query.EQ("_id", new BsonObjectId(id)));


            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                  .Join("ReferenceObjects", "objectReference", "_id", "name=>objectReferencename,object_id=>id_articulo");


            return Join.Find(query);
        }
        public String getobjectbyepcjoin(string id)
        {
            //Query needed to get the result
            var query = Query.And(Query.EQ("EPC", id));


            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                  .Join("ReferenceObjects", "objectReference", "_id", "name=>objectReferencename,object_id=>id_articulo");


            return Join.Find(query);
        }
    }
   
}