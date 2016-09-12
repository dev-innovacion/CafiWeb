﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using Rivka.Db.MongoDb;
using Newtonsoft.Json;
using Rivka.Db;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
namespace RivkaAreas.Tags.Models
{
    public class ObjectReal : MongoModel
    {
        //staticFieds contains the required fields, each document must have this values
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        static public List<String> staticFields = new List<String>() {"_id", "name" };

        public ObjectReal(string table = "ObjectReal")
            : base(table)
        {
        }
        public bool isValidDocument(BsonDocument document)
        {
            return true;
        }
        public String SaveRowFalse(String jsonString, String id = null)
        {
            BsonDocument doc;

            try //trying to parse the jsonString into a bsondocument
            {
                doc = BsonDocument.Parse(jsonString);
            }
            catch (Exception e)
            { //if it's not a valid jsonString, the user can't be saved
                return null;
            }

            /*if (!isValidDocument(doc)) //is the document valid?
                return null;*/

            if (id != null && id != "" && id != "null") //is this an update?
            {
                doc.Set("_id", new ObjectId(id)); //setting the user's id to the document
                try
                {
                    doc.Set("CreatedDate", BsonDocument.Parse(GetRow(id)).GetElement("CreatedDate").Value); //the createdDate must not be modified
                }
                catch (Exception e)
                {
                    doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //setting the createdDate
                }

                try
                {
                    doc.Set("Creator", BsonDocument.Parse(GetRow(id)).GetElement("Creator").Value);
                }
                catch (Exception e)
                {
                    try
                    {
                        doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
                    }
                    catch (Exception e2)
                    {
                        //do not set the creator
                    }
                }
            }
            else //is a new user?
            {
                doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //setting the createdDate
                doc.Set("CreatedTimeStamp", Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));
                try
                {
                    doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
                }
                catch (Exception ex)
                {

                }
                doc.Set("system_status", false);
            }
            doc.Set("LastmodDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            return doc["_id"].ToString();
        }
        public String getRow(string objectId)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(objectId));
                BsonDocument result = resultObject.ToBsonDocument();
                result.Set("_id", result.GetElement("_id").Value.ToString());

                if (!isValidDocument(result)) //if the document has not a correct structure, delete it and return no document
                {
                    deleteRow(objectId);
                    return null;
                }

                return result.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public void deleteRow(String rowID)
        {
            try
            {
                var query = Query.And(Query.EQ("_id", new ObjectId(rowID)));
                collection.Remove(query);
            }
            catch (Exception e) { /*Ignored*/ }
        }

        public string GetObjects(string varLocation)
        {
            //Query needed to get the result
            var query = Query.And(Query.EQ("location", varLocation));
            
            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext")
                .Join("Users", "Creator", "_id", "name=>Creator");

            return Join.Find(query);
        }
        public string Valididcafi(List<string> ids)
        {
            //Query needed to get the result
            BsonArray idsarray = new BsonArray();
            foreach (string id in ids)
            {
                try
                {
                    idsarray.Add(id);
                }
                catch { }
            }
            var query = Query.And(Query.In("id_registro", idsarray));

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext")
                .Join("Locations", "location", "_id", "name=>location_name,parent=>conjunto")
                .Join("Users", "Creator", "_id", "name=>Creator");

            return Join.Find(query);
        }
        public string ValidEpcs(List<string> Epcs)
        {
            //Query needed to get the result
            BsonArray epcsarray = new BsonArray();
            foreach(string epc in Epcs){
                try
                {
                    epcsarray.Add(epc);
                }
                catch { }
            }
            var query = Query.And(Query.In("EPC",epcsarray));

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext")
                .Join("Locations", "location", "_id", "name=>location_name,parent=>conjunto")
                .Join("Users", "Creator", "_id", "name=>Creator");

            return Join.Find(query);
        }
        public string Getby(string key,List<string> datas)
        {
            //Query needed to get the result
            BsonArray datasarray = new BsonArray();
            foreach (string data in datas)
            {
                try
                {
                    datasarray.Add(data);
                }
                catch { }
            }
            var query = Query.And(Query.In(key, datasarray));

            JoinCollections Join = new JoinCollections();
            Join.Select("Locations")
                .Join("Locations", "parent", "_id", "name=>conjunto_name");
                

            return Join.Find(query);
        }
        public string GetbyCustom(string key, List<string> datas,string Collection)
        {
            //Query needed to get the result
            BsonArray datasarray = new BsonArray();
            foreach (string data in datas)
            {
                try
                {
                    datasarray.Add(data);
                }
                catch { }
            }
            var query = Query.And(Query.In(key, datasarray));

            JoinCollections Join = new JoinCollections();
            Join.Select(Collection)
                .Join("Users", "Creator", "_id", "name=>user_name");


            return Join.Find(query);
        }
        public string ValidIds(List<string> Ids)
        {
            //Query needed to get the result
            BsonArray idsarray = new BsonArray();
            foreach (string epc in Ids)
            {
                try
                {
                    idsarray.Add(new BsonObjectId(epc));
                }
                catch { }
            }
            var query = Query.And(Query.In("_id", idsarray));

            JoinCollections Join = new JoinCollections();
            Join.Select("ObjectReal")
                .Join("ReferenceObjects", "objectReference", "_id", "name=>object, ext=>ext")
                .Join("Locations", "location", "_id", "name=>location_name,parent=>conjunto")
                .Join("Users", "Creator", "_id", "name=>Creator");

            return Join.Find(query);
        }

        public string GetIdUnico()
        {
            string queryFunction = "getIDOne()";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            MongoConection conection = (MongoConection)Conection.getConection();
            double result = conection.getDataBase().Eval(SubFunction).AsDouble;

            return result.ToString();
        }
    }
}