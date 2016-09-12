using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Drawing;
using System.Globalization;
using Rivka.Db;
using Rivka.Db.MongoDb;
using System.Web;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace RivkaAreas.Reports.Models
{
    public class getReports : MongoModel
    {
        //
        // GET: /Reports/UsersReport/
        private MongoCollection collection;
        private MongoConection conection;
        private MongoCollection collection1;
        private MongoConection conection1;
        protected MongoModel modelMongo ;
        
        public getReports(string table)
            : base("Reports")
        {
            conection = (MongoConection)Conection.getConection();
            collection = conection.getCollection(table);
            //collection.EnsureIndex(IndexKeys.Ascending("_id"), IndexOptions.SetUnique(true));
           
        }


        public String GetRowsReports(string id,string type=null)
        {


            var orderfield = "name";




            try
            {

               

                List<BsonDocument> documents = new List<BsonDocument>();
                if (id != null )
                {
                    IMongoQuery query;
                    if (type != null)
                    {
                        query = Query.And(Query.EQ("UserId", id), Query.EQ("CategoryReport", type));
                    }
                    else
                    {
                        query = Query.And(Query.EQ("UserId", id));
                
                    }
                    var cursor = collection.FindAs(typeof(BsonDocument), query).SetSortOrder(SortBy.Ascending(orderfield));


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
                }
              
                
                return documents.ToJson();
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }

        public List<string> sublocations(string idlocation,string profile)
        {
            List<string> ids = new List<string>();
               
            try
            {
                ObjectsRealReport custom = new ObjectsRealReport("ObjectReal");
                MongoModel locationdb = new MongoModel("Locations");
               
                if (profile == "Region")
                {
                    try
                    {
                        JArray conjuntos = JsonConvert.DeserializeObject<JArray>(locationdb.Get("parent", idlocation));
                        ids = (from conj in conjuntos select (string)conj["_id"]).ToList();
                    }
                    catch { }
                }
                else
                {
                    ids.Add(idlocation);
                }

                try
                {
                    JArray locationsja = JsonConvert.DeserializeObject<JArray>(custom.GetbyCustom("parent", ids, "Locations"));
                    ids.AddRange((from loc in locationsja select (string)loc["_id"]).ToList());
                }
                catch { }

            }
            catch { }
            return ids;
        }
        public string GetObjectsTable(JObject options)
        {

            //Query needed to get the result
            IMongoQuery query=null,query2;
            List<IMongoQuery> listqueries=new List<IMongoQuery>();
            List<IMongoQuery> listqueries2=new List<IMongoQuery>();
            long dateinit=0;
            DateTime fecha = new DateTime();
            int cantidad;

            if (options["type"].ToString() != "summary") {
                int.TryParse(options["date"][0].ToString(), out cantidad);
                if (options["date"][1].ToString() == "minutes")
                {
                    fecha = DateTime.Now.AddMinutes(-cantidad);
                }
                if (options["date"][1].ToString() == "hours")
                {
                    fecha = DateTime.Now.AddHours(-cantidad);
                }
                if (options["date"][1].ToString() == "days")
                {
                    fecha = DateTime.Now.AddDays(-cantidad);
                }

                string cad1 = fecha.ToString("yyyyMMddhhmmss");
                long.TryParse(cad1, out dateinit);
            }

            JObject objfilter = JsonConvert.DeserializeObject<JObject>(options["filters"].ToString());
            JArray filters = JsonConvert.DeserializeObject<JArray>(objfilter["modules"].ToString());
            JArray valores= new JArray();
            JObject ObjectsReal= new JObject();
            ObjectsReal["Locations"]="location";
            ObjectsReal["ReferenceObjects"]="objectReference";
            ObjectsReal["Users"] = "Creator";
            MongoModel profdb = new MongoModel("LocationProfiles");
            MongoModel locationdb = new MongoModel("Locations");
            foreach (JObject filter in filters) {
                listqueries2.Clear();
                valores=JsonConvert.DeserializeObject<JArray>(filter["v"].ToString());
                
                if (valores.Count == 0) continue;
                foreach(String val in valores){
                    try
                    {
                        JObject location = JsonConvert.DeserializeObject<JObject>(locationdb.GetRow(val));
                        JObject profid = JsonConvert.DeserializeObject<JObject>(profdb.GetRow(location["profileId"].ToString()));
                        if (profid["name"].ToString() != "Ubicacion")
                        {
                            List<string> sublocationslist = sublocations(val, profid["name"].ToString());
                            foreach (string sub in sublocationslist)
                            {
                                try
                                {
                                    listqueries2.Add(Query.EQ(ObjectsReal[filter["mod"].ToString()].ToString(),sub));
           
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                    listqueries2.Add(Query.EQ(ObjectsReal[filter["mod"].ToString()].ToString(), val));
                }
                query2=Query.Or(listqueries2);
                listqueries.Add(query2);
            }
            if (options["type"].ToString() != "summary")
                listqueries.Add(Query.GTE("CreatedTimeStamp",dateinit));

            if (listqueries.Count > 0)
                query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join= Join.Select("ObjectReal");
            foreach (JObject filter in filters) {
                if (objfilter["group"].ToString() == ObjectsReal[filter["mod"].ToString()].ToString()) continue;
                Join=Join.Join(filter["mod"].ToString(), ObjectsReal[filter["mod"].ToString()].ToString(), "_id", "name => "+ObjectsReal[filter["mod"].ToString()].ToString());
            }
            string groupMod = "";
            foreach (KeyValuePair<String, JToken> token in ObjectsReal)
            {
                if (token.Value.ToString() == objfilter["group"].ToString())
                    groupMod = token.Key.ToString();
            }

            Join = Join.Join(groupMod, objfilter["group"].ToString(), "_id", "name => " + ObjectsReal[groupMod].ToString());

            return Join.Find(query);
        }

        public string GetLocationsTable(JObject options)
        {

            //Query needed to get the result
            IMongoQuery query=null, query2;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            List<IMongoQuery> listqueries2 = new List<IMongoQuery>();
            long dateinit=0;
            DateTime fecha = new DateTime();
            int cantidad;
            if (options["type"].ToString() != "summary") {
                int.TryParse(options["date"][0].ToString(), out cantidad);
                if (options["date"][1].ToString() == "minutes")
                {
                    fecha = DateTime.Now.AddMinutes(-cantidad);
                }
                if (options["date"][1].ToString() == "hours")
                {
                    fecha = DateTime.Now.AddHours(-cantidad);
                }
                if (options["date"][1].ToString() == "days")
                {
                    fecha = DateTime.Now.AddDays(-cantidad);
                }

                string cad1 = fecha.ToString("yyyyMMddhhmmss");
                long.TryParse(cad1, out dateinit);
            }

            JObject objfilter = JsonConvert.DeserializeObject<JObject>(options["filters"].ToString());
            JArray filters = JsonConvert.DeserializeObject<JArray>(objfilter["modules"].ToString());
            JArray valores = new JArray();
            JObject Locations = new JObject();
            Locations["LocationProfiles"] = "profileId";
            Locations["Users"] = "responsable";

            foreach (JObject filter in filters)
            {
                listqueries2.Clear();
                valores = JsonConvert.DeserializeObject<JArray>(filter["v"].ToString());
                if (valores.Count == 0) continue;
                foreach (String val in valores)
                {
                    listqueries2.Add(Query.EQ(Locations[filter["mod"].ToString()].ToString(), val));
                }
                query2 = Query.Or(listqueries2);
                listqueries.Add(query2);
            }
            if (options["type"].ToString() != "summary")
                listqueries.Add(Query.GTE("CreatedTimeStamp", dateinit));

            if (listqueries.Count > 0)
                query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join = Join.Select("Locations");
            foreach (JObject filter in filters)
            {
                if (objfilter["group"].ToString() == Locations[filter["mod"].ToString()].ToString()) continue;
                Join = Join.Join(filter["mod"].ToString(), Locations[filter["mod"].ToString()].ToString(), "_id", "name => " + Locations[filter["mod"].ToString()].ToString());
            }

            string groupMod = "";
            foreach (KeyValuePair<String, JToken> token in Locations)
            {
                if (token.Value.ToString() == objfilter["group"].ToString())
                    groupMod = token.Key.ToString();
            }

            Join = Join.Join(groupMod, objfilter["group"].ToString(), "_id", "name => " + Locations[groupMod].ToString());

            return Join.Find(query);
        }

        public string GetUsersTable(JObject options)
        {

            //Query needed to get the result
            IMongoQuery query = null, 
                query2;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            List<IMongoQuery> listqueries2 = new List<IMongoQuery>();
            long dateinit=0;
            DateTime fecha = new DateTime();
            int cantidad;
            if (options["type"].ToString() != "summary") {
                int.TryParse(options["date"][0].ToString(), out cantidad);
                if (options["date"][1].ToString() == "minutes")
                {
                    fecha = DateTime.Now.AddMinutes(-cantidad);
                }
                if (options["date"][1].ToString() == "hours")
                {
                    fecha = DateTime.Now.AddHours(-cantidad);
                }
                if (options["date"][1].ToString() == "days")
                {
                    fecha = DateTime.Now.AddDays(-cantidad);
                }

                string cad1 = fecha.ToString("yyyyMMddhhmmss");
                long.TryParse(cad1, out dateinit);
            }

            JObject objfilter = JsonConvert.DeserializeObject<JObject>(options["filters"].ToString());
            JArray filters = JsonConvert.DeserializeObject<JArray>(objfilter["modules"].ToString());
            JArray valores = new JArray();
            JObject Users = new JObject();
            Users["Profiles"] = "profileId";

            foreach (JObject filter in filters)
            {
                listqueries2.Clear();
                valores = JsonConvert.DeserializeObject<JArray>(filter["v"].ToString());
                if (valores.Count == 0) continue;
                foreach (String val in valores)
                {
                    listqueries2.Add(Query.EQ(Users[filter["mod"].ToString()].ToString(), val));
                }
                query2 = Query.Or(listqueries2);
                listqueries.Add(query2);
            }

            if (options["type"].ToString() != "summary")
                listqueries.Add(Query.GTE("CreatedTimeStamp", dateinit));
            
            if(listqueries.Count > 0)
                query = Query.And(listqueries);

            JoinCollections Join = new JoinCollections();
            Join = Join.Select("Users");
            foreach (JObject filter in filters)
            {
                if (objfilter["group"].ToString() == Users[filter["mod"].ToString()].ToString()) continue;
                Join = Join.Join(filter["mod"].ToString(), Users[filter["mod"].ToString()].ToString(), "_id", "name => " + Users[filter["mod"].ToString()].ToString());
            }

            string groupMod = "";
            foreach (KeyValuePair<String, JToken> token in Users)
            {
                if (token.Value.ToString() == objfilter["group"].ToString())
                    groupMod = token.Key.ToString();
            }

            Join = Join.Join(groupMod, objfilter["group"].ToString(), "_id", "name => " + Users[groupMod].ToString());


            return Join.Find(query);
        }

        public string GetMovementsTable(JObject options, string iduser=null)
        {

            //Query needed to get the result
            IMongoQuery query=null, query2;
            List<IMongoQuery> listqueries = new List<IMongoQuery>();
            List<IMongoQuery> listqueries2 = new List<IMongoQuery>();
            long dateinit = 0;
            DateTime fecha = new DateTime();
            int cantidad;
            if (options["type"].ToString() != "summary")
            {
                int.TryParse(options["date"][0].ToString(), out cantidad);
                if (options["date"][1].ToString() == "minutes")
                {
                    fecha = DateTime.Now.AddMinutes(-cantidad);
                }
                if (options["date"][1].ToString() == "hours")
                {
                    fecha = DateTime.Now.AddHours(-cantidad);
                }
                if (options["date"][1].ToString() == "days")
                {
                    fecha = DateTime.Now.AddDays(-cantidad);
                }

                string cad1 = fecha.ToString("yyyyMMddhhmmss");
                long.TryParse(cad1, out dateinit);
            }

            JObject objfilter = JsonConvert.DeserializeObject<JObject>(options["filters"].ToString());
            JArray filters = JsonConvert.DeserializeObject<JArray>(objfilter["modules"].ToString());
            JArray valores = new JArray();
            JObject moves = new JObject();
            moves["MovementProfiles"] = "movement";
            moves["Locations"] = "location";
            moves["Users"] = "Creator";
            moves["MovementStatus"] = "status";

            foreach (JObject filter in filters)
            {
                listqueries2.Clear();
                valores = JsonConvert.DeserializeObject<JArray>(filter["v"].ToString());
                if (valores.Count == 0) continue;
                foreach (String val in valores)
                {
                    BsonArray  status = new BsonArray();
                    if (filter["mod"].ToString() == "Locations")
                    {
                        listqueries2.Add(Query.EQ("objects.conjunto", val));
                    }
                    else 
                    if (filter["mod"].ToString() == "MovementStatus")
                    {
                        switch (val)
                        {
                            case "Aprobada":
                                status.Add(6);
                                break;
                            case "Cancelada":
                                status.Add(7);
                                break;
                            case "Pendiente":
                                status.Add(1);
                                status.Add(2);
                                status.Add(3);
                                status.Add(4);
                                status.Add(5);
                                status.Add(8);
                                status.Add(9);
                                break;
                        }
                     listqueries2.Add(Query.In(moves[filter["mod"].ToString()].ToString(), status));
                    }
                    else
                    {
                        listqueries2.Add(Query.EQ(moves[filter["mod"].ToString()].ToString(), val));
                    }
                }
                query2 = Query.Or(listqueries2);
                listqueries.Add(query2);
            }

            //if (options["type"].ToString() != "summary")
            //    listqueries.Add(Query.GTE("CreatedTimeStamp", dateinit));

            if (iduser != null && iduser != "null" && iduser != "")
            {
                listqueries.Add(Query.EQ("Creator", iduser));
            }

            if (listqueries.Count > 0)
                query = Query.And(listqueries);

            

            JoinCollections Join = new JoinCollections();
            Join = Join.Select("Demand");
            foreach (JObject filter in filters)
            {
                if (objfilter["group"].ToString() == moves[filter["mod"].ToString()].ToString() ||
                    moves[filter["mod"].ToString()].ToString() == "status"
                    ) continue;
                Join = Join.Join(filter["mod"].ToString(), moves[filter["mod"].ToString()].ToString(), "_id", "name => " + moves[filter["mod"].ToString()].ToString());
            }

            string groupMod="";

            if (objfilter["group"].ToString() != "status")
            {

                foreach (KeyValuePair<String, JToken> token in moves)
                {
                    if (token.Value.ToString() == objfilter["group"].ToString())
                        groupMod = token.Key.ToString();
                }

                Join = Join.Join(groupMod, objfilter["group"].ToString(), "_id", "name => " + moves[groupMod].ToString());
            }

            return Join.Find(query);
        }

        public string GetSubObjects(string locationId = null)
        {
            locationId = (locationId != null && locationId != "") ? locationId : "";

            string queryFunction = "getSubObjects ( \"" + locationId + "\")";
            BsonJavaScript SubFunction = new BsonJavaScript(queryFunction);

            //Calling the stores Mongo Function
            BsonArray result = conection.getDataBase().Eval(SubFunction).AsBsonArray;
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
}
