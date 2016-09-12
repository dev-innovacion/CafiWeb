﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Globalization;
//using Rivka.Error;

namespace Rivka.Db.MongoDb
{
    /// <summary>
    ///     <author>
    ///         Abigail Rodríguez Álvarez
    ///     </author>
    /// </summary>
    public class MongoModel : ModelInterface
    {
        //.........
        //... General Actions for the Data Base
        //... All functions get and return a string
        //...
        //... Functions:
        //...  string GetRow( string id ); //allows to get a document by its id in a json
        //...  string GetRows(string fiel = null); //allows to get the whole collection's documents in a json with true system_status
        //...  string GetRowsAll(string fiel = null); //allows to get the whole collection's documents in a json 
        //...  string GetRowsDeleted(string fiel = null); //allows to get the whole collection's documents in a json with false system_status
        //...  string Get(string key, string value, string fiel = null); //allows to get all the documents where key = value and system_status = true
        //...  string GetAll(string key, string value, string fiel = null); //allows to get all the documents where key = value
        //...  string GetDeleted(string key, string value, string fiel = null); //allows to get all the documents where key = value and system_status = false
        //...  string DeleteRow(string id); // allows to delete a document by its id, returns success or error
        //...  string SaveRow(string jsonData, string id); // allows to save or update a document, returns the saved document's id
        //.........

        protected MongoCollection collection;
        protected MongoConection conection;

        public MongoModel(String name, Dictionary<string,string> dbConf = null, Boolean authAgainstAdmin = false) 
        {
            conection = (MongoConection)Conection.getConection(dbConf);
            collection = conection.getCollection(name, authAgainstAdmin);
        }

        /// <summary>
        ///     Class that contains all the methods needed to join multiple collections
        /// </summary>
        /// <author>
        ///     Juan Bautista
        /// </author>
        internal class JoinCollections
        {
            private string collection;
            private List<string> fields;
            private List<JObject> joins = new List<JObject>();

            /// <summary>
            ///     Selects the main collection
            /// </summary>
            /// <param name="collection"> Name of the main collection </param>
            public JoinCollections Select(string collection)
            {
                this.collection = collection;
                return this;
            }

            /// <summary>
            ///     Join a new collection to the main collection
            /// </summary>
            /// <param name="collection"> Name of the collection to join </param>
            /// <param name="mainKey"> Name of the key field on the main collection </param>
            /// <param name="thisKey"> Name of the key field on the join collection </param>
            /// <param name="fields"> 
            ///     A string list with the fields required eg. "field, field2 => aliasName" 
            /// </param>
            /// <returns> An instance of JoinCollections </returns>
            public JoinCollections Join(string collection, string mainKey, string thisKey, string fields)
            {
                if (collection != "" && mainKey != "" && thisKey != "")
                {
                    JObject newJoin = new JObject();
                    newJoin.Add("collection", collection);
                    newJoin.Add("mainKey", mainKey);
                    newJoin.Add("thisKey", thisKey);

                    JArray fieldArray = new JArray();

                    //Parse the fields string
                    string[] fieldsArray = fields.Split(',');

                    foreach (string field in fieldsArray)
                    {
                        if (field.Contains("=>"))
                        {
                            string originalField = field.Substring(0, field.IndexOf('=')).Trim();
                            string renameFiel = field.Substring(field.IndexOf('>') + 1, field.Length - field.IndexOf('>') - 1).Trim();
                            JObject newField = new JObject();
                            newField.Add(originalField, renameFiel);

                            fieldArray.Add(newField);
                        }
                        else
                            fieldArray.Add(field.Trim());
                    }
                    newJoin.Add("fields", fieldArray);

                    //Add the new join
                    this.joins.Add(newJoin);
                }

                return this;
            }

             /// <summary>
            /// Gets the join result
            /// </summary>
            /// <returns> A string with the query result </returns>
            public string Find(IMongoQuery query = null)
            {
                var myQuery = "{}";

                if (query != null)
                    myQuery = query.ToJson();

                string queryFunction = "generalSearch ( \"" + this.collection + "\"" + ", " + myQuery + ", " + JsonConvert.SerializeObject(this.joins) + " )";
                BsonJavaScript JoinFunction = new BsonJavaScript(queryFunction);

                //Calling the stores Mongo Function
                MongoConection conection = (MongoConection)Conection.getConection();
                
                BsonArray result = conection.getDataBase().Eval(JoinFunction).AsBsonArray;
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in result)
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex) { }

                    documents.Add(document);
                }

                return documents.ToJson();
            }

        }
        public String GetMaxMinValue(string orderfield = null, string sortBy = "DESC", int limit = 0)
        {

            IMongoSortBy sorting = SortBy.Descending(orderfield);
            if (sortBy == "ASC")
            {
                sorting = SortBy.Ascending(orderfield);
            }
          try
            {
                MongoCursor cursor = null;
                
                cursor = collection.FindAllAs(typeof(BsonDocument)).SetFields(orderfield).SetSortOrder(sorting).SetLimit(1);
                
                List<BsonDocument> documents = new List<BsonDocument>();


                foreach (BsonDocument document in cursor)
                {
                    try
                    {

                        document.Set(orderfield, document.GetElement(orderfield).Value.ToString());

                        try
                        {
                            document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex) { }
                        try
                        {
                            document.Set("status", document.GetElement("status").Value.ToString());
                        }
                        catch (Exception ex) { }


                        documents.Add(document);
                    }
                    catch { }
                }
                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        /// <summary>
        ///     Allows to get all the documents in the collection
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a Json list with all the documents in the categories collection 
        ///     or null if there are no documents
        /// </returns>
        public String GetRows(string orderfield = null, string sortBy = "ASC", int limit = 0)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            var sorting = SortBy.Ascending(orderfield);
            if (sortBy == "DES")
                sorting = SortBy.Descending(orderfield);

            try
            {
                MongoCursor cursor = null;

                if( limit == 0)
                    cursor = collection.FindAllAs(typeof(BsonDocument));//.SetSortOrder(sorting);
                else
                    cursor = collection.FindAllAs(typeof(BsonDocument));//.SetSortOrder(sorting).SetLimit(limit);

                List<BsonDocument> documents = new List<BsonDocument>();
              

                foreach (BsonDocument document in cursor)
                {
                    try
                    {

                        document.Set("_id", document.GetElement("_id").Value.ToString());

                        try
                        {
                            document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex) { }
                        try
                        {
                            document.Set("status", document.GetElement("status").Value.ToString());
                            

                        }
                        catch (Exception ex) { }


                        documents.Add(document);
                    }
                    catch { }
                }
                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public JArray GetRowsnew(string orderfield = null, string sortBy = "ASC", int limit = 0)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            var sorting = SortBy.Ascending(orderfield);
            if (sortBy == "DES")
                sorting = SortBy.Descending(orderfield);

            try
            {
                MongoCursor cursor = null;

                if (limit == 0)
                    cursor = collection.FindAllAs(typeof(BsonDocument));//.SetSortOrder(sorting);
                else
                    cursor = collection.FindAllAs(typeof(BsonDocument));//.SetSortOrder(sorting).SetLimit(limit);

                List<BsonDocument> documents = new List<BsonDocument>();

                JArray data = new JArray();
                foreach (BsonDocument document in cursor)
                {
                    try
                    {
                        JObject row = new JObject();
                        document.Set("_id", document.GetElement("_id").Value.ToString());

                        try
                        {
                            document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex) { }
                        try
                        {
                            document.Set("status", document.GetElement("status").Value.ToString());
                        }
                        catch (Exception ex) { }

                        string doc = document.ToJson();
                        row = JsonConvert.DeserializeObject<JObject>((doc));
                        data.Add(row);
                        //documents.Add(document);
                    }
                    catch { }
                }
                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     Allows to get all the documents in the collection
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a Json list with all the documents in the categories collection 
        ///     or null if there are no documents
        /// </returns>
        public String GetRowsAll(string orderfield = null)
        {
            if (orderfield == null) orderfield = "name";

            try
            {
                var cursor = collection.FindAllAs(typeof(BsonDocument)).SetSortOrder(SortBy.Ascending(orderfield));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());

                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }
                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     Allows to get all the documents in the collection with status false
        /// </summary>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        ///     Abigail Rodriguez (edit)
        /// </author>
        /// <returns>
        ///     Returns a Json list with all the documents in the categories collection 
        ///     or null if there are no documents
        /// </returns>
        public String GetRowsDeleted(string orderfield = null)
        {
            if (orderfield == null) orderfield = "name";

            try
            {
                var query = Query.And(Query.EQ("system_status", false));
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));

                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }
                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     Gets a row specified by the objectId
        /// </summary>
        /// <param name="id">
        ///     A string with the idRow
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     A Json String with the document that matched, or null if no one was found
        /// </returns>
        public String GetRow(String id)
        {
            Object resultObject = null;
            try //trying to get the document, if an exception occurs there is not such document
            {
                resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(id));
                BsonDocument result = resultObject.ToBsonDocument();
                result.Set("_id", result.GetElement("_id").Value.ToString());

                try
                {
                    result.Set("CreatedTimeStamp",result.GetElement("CreatedTimeStamp").Value.ToString());
                }
                catch (Exception ex) { }
                if (collection.Name != "Demand") {
                    try
                {
                        result.Set("status", result.GetElement("status").Value.ToString());
                }
                    catch (Exception ex) { }
                }
                

                return result.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     Deletes a row from the collection
        /// </summary>
        /// <param name="id"></param>
        /// <author>
        ///     Juan Bautista
        /// </author>
        public string DeleteRow(String id, bool logic = false)
        {
            if (logic == false)
            {
                try
                {
                    var query = Query.And(Query.EQ("_id", new ObjectId(id)));
                    collection.Remove(query);
                    return "success";
                }
                catch (Exception e) { return "error"; }
            }
            else {
                Object resultObject = null;
                try //trying to get the document, if an exception occurs there is not such document
                {
                    resultObject = collection.FindOneByIdAs(typeof(BsonDocument), new BsonObjectId(id));
                    BsonDocument result = resultObject.ToBsonDocument();
                   
                    try
                    {
                        result.Set("system_status", false);
                    }
                    catch (Exception ex) { }
                    collection.Save(result);
                    return result.ToJson();
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     Deletes a row from the collection
        /// </summary>
        /// <param name="id"></param>
        /// <author>
        ///     Juan Bautista
        /// </author>
        public string DeleteRowPhysical(String id)
        {
            try
            {
                var query = Query.And(Query.EQ("_id", new ObjectId(id)));
                collection.Remove(query);
                return "success";
            }
            catch (Exception e) { return "error"; }
        }
        public String validSession(string ID_SESION, string DESCRIPCION, string NOMBRE_CONJUNTO, string FECHA_INICIO, string FECHA_FINALIZACION)
        {

            try
            {
                var query = Query.And(Query.EQ("ID_SESION", ID_SESION), Query.EQ("DESCRIPCION", DESCRIPCION), Query.EQ("NOMBRE_CONJUNTO", NOMBRE_CONJUNTO), Query.EQ("FECHA_INICIO", FECHA_INICIO), Query.EQ("FECHA_FINALIZACION", FECHA_FINALIZACION));
                var cursor = collection.FindAs(typeof(BsonDocument), query);
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public String GetIgnoreStatus(String key, String value, string orderfield = null, bool ignoreStatus = false)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            try
            {
                var query = Query.And(Query.EQ(key, value), Query.NE("system_status", false));
                if (ignoreStatus)
                    query = Query.And(Query.EQ(key, value));
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     This function generates a query, and returns all coincidences where key = value
        ///     and system_status = true
        /// </summary>
        /// <param name="key">
        ///     The collection's key value in the db
        /// </param>
        /// <param name="value">
        ///     The collection's value in the db
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        ///     Abigail Rodriguez (edit)
        /// </author> 
        /// <returns>
        ///     A json string with the matched documents
        /// </returns>
        public String Get(String key, String value, string orderfield = null)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            try
            {
                var query = Query.And(Query.EQ(key, value), Query.NE("system_status", false));
                
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     This function generates a query, and returns all coincidences where key = value
        /// </summary>
        /// <param name="key">
        ///     The collection's key value in the db
        /// </param>
        /// <param name="value">
        ///     The collection's value in the db
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author> 
        /// <returns>
        ///     A json string with the matched documents
        /// </returns>
        public String GetAll(String key, String value, string orderfield = null)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            try
            {
                var query = Query.And(Query.EQ(key, value));
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     This function generates a query, and returns all coincidences where key = value
        ///     and system_status = false
        /// </summary>
        /// <param name="key">
        ///     The collection's key value in the db
        /// </param>
        /// <param name="value">
        ///     The collection's value in the db
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        ///     Abigail Rodriguez (edit)
        /// </author> 
        /// <returns>
        ///     A json string with the matched documents
        /// </returns>
        public String GetDeleted(String key, String value, string orderfield = null)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            try
            {
                var query = Query.And(Query.EQ(key, value), Query.EQ("system_status", false));
                var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor)
                {
                    // if (isValidDocument(document))
                    document.Set("_id", document.GetElement("_id").Value.ToString());
                    try
                    {
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                    documents.Add(document);
                }

                return documents.ToJson();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        ///     Allows to create or modify a row
        /// </summary>
        /// <param name="jsonString">
        ///     The category json string with the document's information
        /// </param>
        /// <param name="id">
        ///     The modifying document's id
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns the id of the saved document
        /// </returns>
        /// 
        public String SaveRow(String jsonString, String id = null)
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
                // Set the TimeStamp
                try
                {
                    doc.Set("CreatedTimeStamp",Convert.ToInt64( BsonDocument.Parse(GetRow(id)).GetElement("CreatedTimeStamp").Value.ToString()));
                }
                catch (Exception e)
                {
                    doc.Set("CreatedTimeStamp", Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));
                }

                try
                {
                    doc.Set("Creator", BsonDocument.Parse(GetRow(id)).GetElement("Creator").Value);
                }
                catch (Exception e)
                {
                    try
                    {
                        doc.Set("Creator", HttpContext.Current.Session["id"].ToString());
                    }catch(Exception e2){
                        //do not set creator
                    }
                }
            }
            else //is a new user?
            {
                BsonValue valor1;
                if (!doc.TryGetValue("CreatedDate", out valor1))
                doc.Set("CreatedDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //setting the createdDate
                if (!doc.TryGetValue("CreatedTimeStamp", out valor1))
                doc.Set("CreatedTimeStamp", Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmmss")));

                try
                {
                    BsonValue valor;
                    if (!doc.TryGetValue("Creator", out valor))
                        doc.Set("Creator", HttpContext.Current.Session["_id"].ToString());
                }
                catch (Exception ex)
                {

                }
              
               if (!doc.TryGetValue("system_status", out valor1))
                  doc.Set("system_status", true);
            }
            doc.Set("LastmodDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")); //either it is a new user or an update we set the lastmodDate to this Day
            collection.Save(doc);
            
            return doc["_id"].ToString();
        }


        public String UpdateRow(String field, String data, String id)
        {
            BsonDocument updatedData = new BsonDocument();
            try
            {
                id = id.Trim();
                UpdateBuilder update = new UpdateBuilder();
                try
                {
                    updatedData = BsonDocument.Parse(data); 
                    update = Update.Set(field, updatedData);
                }
                catch(Exception e)
                {
                    try
                    {
                        JArray jArray = JsonConvert.DeserializeObject<JArray>(data);
                        var updatedArray = new BsonArray();

                        foreach (JObject jO in jArray)
                        {
                            updatedArray.Add(BsonDocument.Parse(jO.ToString()));
                        }
                        
                        update = Update.Set(field, updatedArray);
                    }
                    catch(Exception ex)
                    {
                        var updated = new BsonString(data);
                        update = Update.Set(field, updated);
                    }                    
                }
                if (id != null && id != "")
                {
                    var query = Query.EQ("_id", new ObjectId(id));
                    var lastMod = Update.Set(
                        "LastmodDate",
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
                    );
                    //Update data
                    collection.Update(query, update);
                    collection.Update(query, lastMod);
                }
            }
            catch(Exception e)
            {
                Error.Error.Log(e,"Updating data");
            }
            return "";
        }

    }
}