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
using MongoDB.Driver.Linq;


namespace RivkaAreas.Reports.Models
{
    public class DemandReport : MongoModel
    {
        //
        // GET: /Reports/DemandReport/

        private MongoCollection collection;
        private MongoConection conection;
        private MongoCollection collection1;
        private MongoConection conection1;
        private MongoConection conection2;

        
        public  DemandReport(string table):base("Demand") {
            conection = (MongoConection)Conection.getConection();
            conection1 = (MongoConection)Conection.getConection();
            conection2 = (MongoConection)Conection.getConection();
            
            collection = conection.getCollection(table);
          
          
        }
        public String getUsersByUsers(List<string> users = null)
        {

            try
            {
                //Query needed to get the result

                BsonArray bsonarray = new BsonArray();

                foreach (string item in users)
                {
                    bsonarray.Add(item);
                }
                var query = Query.And(Query.In("Creator", bsonarray));


                JoinCollections Join = new JoinCollections();
                Join.Select("Demand")
                    .Join("Users", "Creator", "_id", "name=>username");


                return Join.Find(query);


            }
            catch
            {
                return null;
            }
        }
        public String GetByStatus(String key, int value, string orderfield = null)
        {
            if (orderfield == null)
            {
                orderfield = "name";
            }
            try
            {
                IMongoQuery query = Query.And(Query.EQ(key, value), Query.NE("system_status", false));
                JoinCollections Join = new JoinCollections();
                Join.Select("Demand")
                .Join("Locations", "objects.location", "_id", "name=>nameLocation")
                .Join("Users", "Creator", "_id", "name=>nameUser")
                .Join("MovementProfiles", "movement", "_id", "name=>nameMovement,typeMovement=>typemov");


                return Join.Find(query);
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public String Mapreduced(Dictionary<string,string> fields, List<string> movement = null,List<string> objs=null,List<string> locations=null,List<string> users=null,Int64 start=0,Int64 end=0)
        {
         var orderfield = "folio";

        
            try
            {
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                 string[] arraymov = movement.ToArray();
                string[] arrayobjs = objs.ToArray();
                string[] arraylocations = locations.ToArray();
                string[] arrayusers = users.ToArray();
                
                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                BsonArray bsonarrayloc = new BsonArray();
                BsonArray bsonarrayusers = new BsonArray();
                for (int i = 0; i < arraymov.Length; i++)
                {
                    bsonarraymov.Add(arraymov[i]);
                }
                for (int i = 0; i < arrayobjs.Length; i++)
                {
                    bsonarrayobjs.Add(arrayobjs[i]);
                } for (int i = 0; i < arraylocations.Length; i++)
                {
                    bsonarrayloc.Add(arraylocations[i]);
                } for (int i = 0; i < arrayusers.Length; i++)
                {
                    bsonarrayusers.Add(arrayusers[i]);
                }

               
                List<BsonDocument> documents = new List<BsonDocument>();
               
                    var query = Query.And(Query.In("movement",bsonarraymov),Query.In("object",bsonarrayobjs),Query.In("location",bsonarrayloc),Query.In("Creator",bsonarrayusers), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
             
            /*Realiza la conexion */
            MongoCollection Demand = conection.getCollection("Demand");
            MongoCollection Movements = conection1.getCollection("MovementProfiles");
            MongoCollection Locations = conection2.getCollection("Locations");
           
            var Movementsfunction = @"function() {
                var key = this._id.valueOf();
                var value={movement:this.name};
                 emit(key,value);
                   }";
            var Locationsfunction = @"function() {
                var key = this._id.valueOf();
                var value={location:this.name};
                 emit(key,value);
                   }";
            var Demandfunction = @"function() {
                    var key = this.movement;
                     var value = {id:this._id.valueOf(),folio:this.folio,object:this.object,total:this.total,status:this.status,movementFields:this.movementFields,CreatedDate:this.CreatedDate,CreatedTimeStamp:this.CreatedTimeStamp,Creator:this.Creator,system_status:this.system_status};
                     emit(key,value);
                
                   }";
            var Demandfunction2 = @"function() {
                    var key = this.location;
                     var value = {id:this._id.valueOf(),folio:this.folio,object:this.object,total:this.total,status:this.status,movementFields:this.movementFields,CreatedDate:this.CreatedDate,CreatedTimeStamp:this.CreatedTimeStamp,Creator:this.Creator,system_status:this.system_status};
                     emit(key,value);
                
                   }";
           
            var reducefunction = @"function (key, values) {
               var reduced = {id:'',folio:'',movement:'',location:'',object:'',total:'',status:'',movementFields:'',CreatedDate:'',CreatedTimeStamp:'',Creator:'',system_status:''};
                var index=0;
              values.forEach(function(value){
              
          
                 
                    if(value.movement != null && reduced.movement ==''){
                    
                    reduced.movement = value.movement;
                  
                    }
                    if(value.location != null && reduced.location == ''){
                    
                    reduced.location = value.location;
                  
                    }
                   
                    if(value.id != null){
                       reduced.id= value.id;
                       reduced.folio = value.folio;
                      
                        reduced.object = value.object;
                        reduced.total = value.total;
                        reduced.status = value.status;
                        reduced.movementFields = value.movementFields;
                        reduced.CreatedDate = value.CreatedDate;
                        reduced.CreatedTimeStamp = value.CreatedTimeStamp;
                        reduced.Creator = value.Creator;
                        reduced.system_status = value.system_status;

                     
                    }
           
            });
           
            return reduced;
          }";

            /*Establece las propiedades a seguir del mapreduce */
            MapReduceOptionsBuilder options = new MapReduceOptionsBuilder();
            options.SetOutput(MapReduceOutput.Inline);
            options.SetOutput(MapReduceOutput.Reduce("result",true));


            
            /* Realiza los mapreduce sobre el mismo resultado */
            MapReduceResult result =  Movements.MapReduce(Movementsfunction, reducefunction, options);
           // result = Locations.MapReduce(Locationsfunction, reducefunction, options);
          
                result = Demand.MapReduce(Demandfunction, reducefunction, options);
            //    result = Demand.MapReduce(Demandfunction2, reducefunction, options);
          
           
            /* resultado en json */
            string results = result.GetResults().ToJson();
            return results;
                }catch(Exception ex){
                    return null;
                }
        }
        public String GetRowsReportMasterDemand2(Dictionary<string, string> fields, Int64 start = 0, Int64 end = 0, List<string> movement = null, List<string> objs = null, List<int> status = null, string dict = null, string done = null, string mp = null, string destroy = null, string auto = null, string vobo = null, string photo = null, List<string> users = null, List<string> locs = null,string dep=null)
        {


            var orderfield = "folio";


            try
            {
                IMongoQuery dict1 = Query.Or(Query.Exists("dctFolio"), Query.NotExists("dctFolio"));
                IMongoQuery done1 = Query.Or(Query.Exists("receiptFile"), Query.NotExists("receiptFile"));
                IMongoQuery mp1 = Query.Or(Query.Or(Query.Exists("receiptFile"), Query.NotExists("receiptFile")), Query.EQ("destinyOptions", "robo"));
                IMongoQuery destroy1 = Query.Or(Query.Exists("ActFolio"), Query.NotExists("ActFolio"));
                IMongoQuery auto1 = Query.Or(Query.Exists("authorizations"), Query.NotExists("authorizations"));
                IMongoQuery vobo1 = Query.Or(Query.Exists("approval"), Query.NotExists("approval"));
                IMongoQuery photo1 = Query.Or(Query.Exists("objects.images"), Query.NotExists("objects.images"));

                List<IMongoQuery> listqueries = new List<IMongoQuery>();
                List<IMongoQuery> listqueries2 = new List<IMongoQuery>();
                if (dict != null)
                {
                    switch (dict)
                    {
                        case "1":
                            dict1 = Query.And(Query.Exists("dctFolio"));
                            listqueries.Add(dict1);
                            break;
                        case "2":
                            dict1 = Query.And(Query.NotExists("dctFolio"));
                            listqueries.Add(dict1);
                            break;
                        case "3":
                            dict1 = Query.And(Query.EQ("deleteType", "no_planeada"));
                            listqueries.Add(dict1);
                            break;
                    }
                }
                if (mp != null)
                {
                    switch (mp)
                    {
                        case "1":
                            mp1 = Query.And(Query.Exists("receiptFile"), Query.EQ("destinyOptions", "robo"));
                            listqueries.Add(mp1);
                            break;
                        case "2":
                            mp1 = Query.And(Query.NotExists("receiptFile"), Query.EQ("destinyOptions", "robo"));
                            listqueries.Add(mp1);
                            break;
                        case "3":
                            mp1 = Query.And(Query.EQ("deleteType", "planeada"));
                            listqueries.Add(mp1);
                            break;
                    }
                }
                if (destroy != null)
                {
                    switch (destroy)
                    {
                        case "1":
                            destroy1 = Query.And(Query.Exists("ActFolio"), Query.EQ("destinyOptions", "destruccion"));
                            listqueries.Add(destroy1);
                            break;
                        case "2":
                            destroy1 = Query.And(Query.NotExists("ActFolio"), Query.EQ("destinyOptions", "destruccion"));
                            listqueries.Add(destroy1);
                            break;
                        case "3":
                            destroy1 = Query.And(Query.NE("destinyOptions", "destruccion"));
                            listqueries.Add(destroy1);
                            break;
                    }
                }
                if (photo != null)
                {
                    switch (photo)
                    {
                        case "1":
                            photo1 = Query.And(Query.Exists("invoiceFile")); // Query.EQ("invoiceRequired", 1)
                            listqueries.Add(photo1);
                            break;
                        case "2":
                            photo1 = Query.And(Query.NotExists("invoiceFile"));
                            listqueries.Add(photo1);
                            break;
                        case "3":
                            photo1 = Query.And(Query.NE("destinyOptions", "venta"));
                            listqueries.Add(photo1);
                            break;
                    }
                }
                if (done != null)
                {
                    switch (done)
                    {
                        case "1":
                            done1 = Query.And(Query.Exists("receiptFile"), Query.EQ("destinyOptions", "siniestro"));
                            listqueries.Add(done1);
                            break;
                        case "2":
                            done1 = Query.And(Query.NotExists("receiptFile"), Query.EQ("destinyOptions", "siniestro"));
                            listqueries.Add(done1);
                            break;
                        case "3":
                            done1 = Query.And(Query.EQ("deleteType", "planeada"));
                            listqueries.Add(done1);
                            break;
                    }

                }
                if (auto != null)
                {
                    switch (auto)
                    {
                        case "1":
                            auto1 = Query.And(Query.Exists("authorizations.approved"), Query.Not(Query.EQ("authorizations.approved", "0")));
                            listqueries.Add(auto1);
                            break;
                        case "2":
                            auto1 = Query.And(Query.EQ("authorizations.approved", "0"));
                            listqueries.Add(auto1);
                            break;
                    }

                }
                if (vobo != null)
                {
                    switch (vobo)
                    {
                        case "1":
                            vobo1 = Query.And(Query.Exists("approval"), Query.EQ("status", 6));
                            listqueries.Add(vobo1);
                            break;
                        case "2":
                            vobo1 = Query.And(Query.Exists("approval"), Query.EQ("status", 5));
                            listqueries.Add(vobo1);
                            break;
                    }

                }
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                List<string> movlist = new List<string>();
                List<string> objslist = new List<string>();
                List<int> statuslist = new List<int>();
                try
                {
                    if (movement != null)
                    {
                        movlist = movement;
                    }
                }
                catch { }
                try
                {
                    if (objs != null)
                    {
                        objslist = objs;
                    }
                }
                catch { }
                try
                {
                    if (status != null)
                    {
                        statuslist = status;
                    }
                }
                catch { }
                string[] arraymov = movlist.ToArray();
                string[] arrayobjs = objslist.ToArray();
                int[] arraystatus = statuslist.ToArray();

                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                BsonArray bsonarraystatus = new BsonArray();
                BsonArray bsonarrayusers = new BsonArray();
                BsonArray bsonarraylocs = new BsonArray();
                try
                {
                    foreach (string userx in users)
                    {
                        bsonarrayusers.Add(userx);
                    }
                }
                catch
                {

                }
                try
                {
                    foreach (string locc in locs)
                    {
                        bsonarraylocs.Add(locc);
                    }
                }
                catch { }
                try
                {
                    for (int i = 0; i < arraymov.Length; i++)
                    {
                        bsonarraymov.Add(arraymov[i]);
                    }
                }
                catch { }
                try
                {
                    for (int i = 0; i < arrayobjs.Length; i++)
                    {
                        bsonarrayobjs.Add(new BsonObjectId(arrayobjs[i]));
                    }
                }
                catch { }
                try
                {
                    for (int i = 0; i < arraystatus.Length; i++)
                    {
                        bsonarraystatus.Add(arraystatus[i]);
                    }
                }
                catch { }
                JoinCollections Join = new JoinCollections();
                Join.Select("DemandDesktop")
                 .Join("Users", "Creatorname", "user", "name=>nameuser,lastname=>lastuser,user=>iduser");
                 
                IMongoQuery query = null;
                IMongoQuery querylocs = null;
                IMongoQuery querymovs = null;
                IMongoQuery queryusers = null;
                IMongoQuery queryobjs = null;
                IMongoQuery querystatus = null;
                if(locs.Count>0)
                    querylocs = Query.And(Query.Or(Query.In("objects.conjuntoText", bsonarraylocs), Query.In("objects.conjuntoDestinyText", bsonarraylocs)));

                if (arraymov.Length > 0)
                    querymovs = Query.And(Query.In("tipomovimiento", bsonarraymov));
                if (arraystatus.Length > 0)
                    querystatus = Query.And(Query.In("status", bsonarraystatus));
                if (users.Count > 0)
                    queryusers = Query.And(Query.In("Creatorname", bsonarrayusers));
                if (arrayobjs.Length > 0)
                    queryobjs = Query.And(Query.In("_id", bsonarrayobjs));
               
                    if (start == end)
                    {
                        query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end + 1000000));
                    }
                    else {
                        query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                    }
                    
          

                if (listqueries.Count() > 0)
                {
                    listqueries2.Add(Query.And(listqueries));
                }
                if(querylocs!=null)
                    listqueries2.Add(querylocs);
                if (querymovs != null)
                    listqueries2.Add(querymovs);
                if (querystatus != null)
                    listqueries2.Add(querystatus);
                if (queryusers != null)
                    listqueries2.Add(queryusers);
                if (queryobjs != null)
                    listqueries2.Add(queryobjs);

                listqueries2.Add(query);

                var queryfinal = Query.And(listqueries2); //new List<IMongoQuery>{query,dict1,done1,destroy1,auto1,vobo1,photo1}
                MongoCollection DemandDesktop = conection1.getCollection("DemandDesktop");
                var cursor = DemandDesktop.FindAs(typeof(BsonDocument), queryfinal);
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor) //putting the docuemnts into a list
                {
                    //firts we have to check if this document has a valid structure
                    try
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception e) { }
                    documents.Add(document);
                }
                return documents.ToJson(); 
                //return Join.Find(queryfinal);
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }

        public String GetRowsReportMasterDemand(Dictionary<string, string> fields, List<string> movement = null, List<string> objs = null, List<int> status = null, Int64 start = 0, Int64 end = 0, string dict = null, string done = null, string mp = null, string destroy = null, string auto = null, string vobo = null, string photo = null,string donative=null,string conta=null,string extra=null,string comprobante=null, List<string> users = null, List<string> locs = null,string dep=null)
        {


            var orderfield = "folio";
            

            try
            {
                IMongoQuery dict1 = Query.Or(Query.Exists("dctFolio"),Query.NotExists("dctFolio"));
                IMongoQuery done1 = Query.Or(Query.Exists("receiptFile"), Query.NotExists("receiptFile"));
                IMongoQuery mp1 = Query.Or(Query.Or(Query.Exists("receiptFile"), Query.NotExists("receiptFile")), Query.EQ("destinyOptions", "robo")); 
                IMongoQuery destroy1 = Query.Or(Query.Exists("ActFolio"), Query.NotExists("ActFolio"));
                IMongoQuery auto1 = Query.Or(Query.Exists("authorizations"), Query.NotExists("authorizations")); 
                IMongoQuery vobo1 = Query.Or(Query.Exists("approval"), Query.NotExists("approval"));
                IMongoQuery photo1 = Query.Or(Query.Exists("objects.images"), Query.NotExists("objects.images"));
                IMongoQuery donative1 = Query.Or(Query.Or(Query.Exists("receiptFile"), Query.NotExists("receiptFile")), Query.EQ("destinyOptions", "donacion"));
               // IMongoQuery conta1 = Query.Or(Query.Exists("ActFolio"), Query.NotExists("ActFolio"));
               
                List<IMongoQuery> listqueries = new List<IMongoQuery>();
                List<IMongoQuery> listqueries2 = new List<IMongoQuery>();
                IMongoQuery conta1 = null;
                IMongoQuery comprobante1 = null;
                IMongoQuery extra1 = null;
                IMongoQuery dep1 = null;
                if (conta != null)
                {
                    switch (conta)
                    {
                        case "1":
                            conta1 = Query.And(Query.Or(Query.Exists("objects.sold_price"), Query.Exists("objects.buyer"), Query.Exists("objects.propietario"), Query.Exists("objects.value_book"), Query.Exists("objects.deposit_account")), Query.Not(Query.EQ("status", 3)), Query.Not(Query.EQ("status", 2)), Query.Not(Query.EQ("status", 1)));
                             listqueries.Add(conta1);
                            break;
                        case "2":
                            conta1 = Query.And( Query.NotExists("objects.propietario"), Query.NotExists("objects.value_book"), Query.NotExists("objects.deposit_account"),Query.Or(Query.EQ("status", 3),Query.EQ("status", 2),  Query.EQ("status", 1)));

                            listqueries.Add(conta1);
                            break;
                        
                    }
                }
                if (extra != null)
                {
                    switch (extra)
                    {
                        case "1":
                            extra1 = Query.And(Query.Or(Query.Exists("objects.sold_price"), Query.Exists("objects.buyer"), Query.Exists("objects.propietario"), Query.Exists("objects.value_book"), Query.Exists("objects.deposit_account"), Query.Exists("objects.donation_benefit"), Query.Exists("objects.rfc"), Query.Exists("objects.imgDestrucion")), Query.And(Query.EQ("deleteType", "planeada"), Query.NE("status", 1),Query.NE("status", 2)));
                            listqueries.Add(extra1);
                            break;
                        case "2":
                            extra1 = Query.And(Query.NotExists("objects.sold_price"), Query.NotExists("objects.buyer"), Query.NotExists("objects.propietario"), Query.NotExists("objects.value_book"), Query.NotExists("objects.deposit_account"), Query.NotExists("objects.donation_benefit"), Query.NotExists("objects.rfc"), Query.NotExists("objects.imgDestrucion"), Query.EQ("deleteType", "planeada"), Query.Or(Query.EQ("status", 1), Query.EQ("status", 2)));

                            listqueries.Add(extra1);
                            break;

                    }
                }
                if (dict != null)
                {
                    switch (dict)
                    {
                        case "1":
                            dict1 = Query.And(Query.Exists("dctFolio"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(dict1);
                            break;
                        case "2":
                            dict1 = Query.And(Query.NotExists("dctFolio"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(dict1);
                            break;
                      /*  case "3":
                            dict1 = Query.And(Query.EQ("deleteType", "no_planeada"));
                            listqueries.Add(dict1);
                            break;*/
                    }
                }
                if (comprobante != null)
                {
                    switch (comprobante)
                    {
                        case "1":
                            comprobante1 = Query.And(Query.Or(Query.Exists("receiptFile"), Query.Exists("ActFolio")), Query.NE("status", 1), Query.NE("status", 2), Query.NE("status", 3), Query.NE("status", 4));
                            listqueries.Add(comprobante1);
                            break;
                        case "2":
                            comprobante1 = Query.And(Query.NotExists("receiptFile"), Query.NotExists("ActFolio"));
                            listqueries.Add(comprobante1);
                            break;
                      
                    }
                }
                if (dep != null)
                {
                    switch (dep)
                    {
                        case "1":
                            dep1 = Query.And(Query.Exists("receiptFile"), Query.EQ("destinyOptions", "venta"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(dep1);
                            break;
                        case "2":
                            dep1 = Query.And(Query.NotExists("receiptFile"), Query.EQ("destinyOptions", "venta"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(dep1);
                            break;
                        /* case "3":
                             mp1=Query.And(Query.EQ("deleteType", "planeada"));
                             listqueries.Add(mp1);
                             break;*/
                    }
                }
                if (mp != null) {
                    switch (mp)
                    {
                        case "1":
                            mp1 = Query.And(Query.Exists("receiptFile"), Query.EQ("destinyOptions", "robo"), Query.EQ("deleteType", "no_planeada"));
                            listqueries.Add(mp1);
                            break;
                        case "2":
                            mp1 = Query.And(Query.NotExists("receiptFile"), Query.EQ("destinyOptions", "robo"), Query.EQ("deleteType", "no_planeada"));
                            listqueries.Add(mp1);
                            break;
                       /* case "3":
                            mp1=Query.And(Query.EQ("deleteType", "planeada"));
                            listqueries.Add(mp1);
                            break;*/
                    }
                }
                if (destroy != null)
                {
                    switch (destroy)
                    {
                        case "1":
                            destroy1 = Query.And(Query.Exists("ActFolio"), Query.EQ("destinyOptions", "destruccion"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(destroy1);
                            break;
                        case "2":
                            destroy1 = Query.And(Query.NotExists("ActFolio"), Query.EQ("destinyOptions", "destruccion"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(destroy1);
                            break;
                       /* case "3":
                            destroy1 = Query.And(Query.NE("destinyOptions", "destruccion"));
                            listqueries.Add(destroy1);
                            break;*/
                    }
                }
                if (photo != null)
                {
                    switch (photo)
                    {
                        case "1":
                            photo1 = Query.And(Query.Exists("invoiceFile"), Query.EQ("destinyOptions", "venta"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(photo1);
                            break;
                        case "2":
                            photo1 = Query.And(Query.NotExists("invoiceFile"), Query.EQ("destinyOptions", "venta"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(photo1);
                            break;
                       /* case "3":
                            photo1 = Query.And(Query.NE("destinyOptions", "venta"));
                            listqueries.Add(photo1);
                            break;*/
                    }
                }
                if (donative != null)
                {
                    switch (donative)
                    {
                        case "1":
                            donative1 = Query.And(Query.Exists("receiptFile"), Query.EQ("destinyOptions", "donacion"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(donative1);
                            break;
                        case "2":
                            donative1 = Query.And(Query.NotExists("receiptFile"), Query.EQ("destinyOptions", "donacion"), Query.EQ("deleteType", "planeada"));
                            listqueries.Add(donative1);
                            break;
                        
                    }
                }
                 if(done!=null){
                    switch(done){
                        case "1":
                            done1 = Query.And(Query.Exists("receiptFile"), Query.EQ("destinyOptions", "siniestro"), Query.EQ("deleteType", "no_planeada"));
                            listqueries.Add(done1);
                            break;
                        case "2":
                            done1 = Query.And(Query.NotExists("receiptFile"), Query.EQ("destinyOptions", "siniestro"), Query.EQ("deleteType", "no_planeada"));
                            listqueries.Add(done1);
                            break;
                       /* case "3":
                            done1 = Query.And(Query.EQ("deleteType", "planeada"));
                            listqueries.Add(done1);
                            break;*/
                    }

                    }
                     if (auto != null)
                     {
                         switch (auto)
                         {
                             case "1":
                                 auto1 = Query.And(Query.Exists("authorizations.approved"), Query.Not(Query.EQ("authorizations.approved", "0")));
                                 listqueries.Add(auto1);
                                 break;
                             case "2":
                                 auto1 = Query.Or(Query.EQ("authorizations.approved", "0"), Query.EQ("authorizations.approved", "2"));
                                 listqueries.Add(auto1);
                                 break;
                         }

                     }
                     if (vobo != null)
                     {
                         switch (vobo)
                         {
                             case "1":
                                 vobo1 = Query.And(Query.Exists("approval.approved"),Query.Not(Query.EQ("authorizations.approved", "0")), Query.EQ("status", 6));
                                 listqueries.Add(vobo1);
                                 break;
                             case "2":
                                 vobo1 = Query.Or(Query.EQ("approval.approved", "0"),Query.EQ("approval.approved", "2"));//, Query.EQ("status", 5));
                                 listqueries.Add(vobo1);
                                 break;
                         }

                     }
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                string[] arraymov = movement.ToArray();
                string[] arrayobjs = objs.ToArray();
                  int[] arraystatus = status.ToArray();

                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                 BsonArray bsonarraystatus = new BsonArray();
                 BsonArray bsonarrayusers = new BsonArray();
                 BsonArray bsonarraylocs = new BsonArray();
                 foreach (string userx in users)
                 {
                     bsonarrayusers.Add(userx);
                 }
                 foreach (string locc in locs)
                 {
                     bsonarraylocs.Add(locc);
                 }
                for (int i = 0; i < arraymov.Length; i++)
                {
                    bsonarraymov.Add(arraymov[i]);
                }
                for (int i = 0; i < arrayobjs.Length; i++)
                {
                    bsonarrayobjs.Add(new BsonObjectId(arrayobjs[i]));
                }
                for (int i = 0; i < arraystatus.Length; i++)
                {
                    bsonarraystatus.Add( arraystatus[i]);
                }
            //    JoinCollections Join = new JoinCollections();
             //   Join.Select("Demand");
               // .Join("Locations", "objects.location", "_id", "name=>nameLocation,parent=>conjuntname");
               // .Join("Users", "Creator", "_id", "name=>nameUser,lastname=>lastuser,user=>iduser");
               // .Join("MovementProfiles", "movement", "_id", "name=>nameMovement,typeMovement=>typemov");

                IMongoQuery query = null;
                IMongoQuery querymov = null;
                IMongoQuery queryobjs = null;
                IMongoQuery querystatus = null;
                IMongoQuery queryusers = null;
                IMongoQuery querylocs = null;
               
                if (bsonarraymov.Count > 0)
                {
                    querymov=Query.And(Query.In("movement", bsonarraymov));
                }
                if (bsonarrayobjs.Count > 0)
                {
                    queryobjs = Query.And(Query.In("_id", bsonarrayobjs));
                }
                if (bsonarraystatus.Count > 0)
                {
                    querystatus = Query.And(Query.In("status", bsonarraystatus));
                }
                if (bsonarrayusers.Count > 0)
                {
                    queryusers = Query.And(Query.In("Creator", bsonarrayusers));
                }
                if (bsonarraylocs.Count > 0)
                {
                    querylocs = Query.And(Query.In("objects.location", bsonarraylocs));
                }
                if (start == end)
                {
                    query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end + 1000000));
                }
                else
                {
                    query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                }
                /*if (movement != null && movement.Count > 0)
                {
                    if (start == end)
                    {
                        if (bsonarrayobjs.Count() > 0)
                        {
                            query = Query.And( Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end + 1000000));
                        }
                        else
                        {
                            query = Query.And( Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end + 1000000));
                      
                        }
                        }
                    else
                    {
                        if (bsonarrayobjs.Count() > 0)
                        {
                            query = Query.And(Query.In("movement", bsonarraymov), Query.In("Creator", bsonarrayusers), Query.In("objects.location", bsonarraylocs), Query.In("_id", bsonarrayobjs), Query.In("status", bsonarraystatus), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                        }
                        else
                        {
                            query = Query.And(Query.In("movement", bsonarraymov), Query.In("Creator", bsonarrayusers), Query.In("objects.location", bsonarraylocs), Query.In("status", bsonarraystatus), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                       
                        }
                      }
                }
                else
                {
                    if (start == end)
                    {
                        query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end + 1000000));
                    }
                    else {
                        query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                    }
                }*/

                if (listqueries.Count() > 0) {
                    listqueries2.Add(Query.And(listqueries));
                }
                if (querymov != null)
                {
                    listqueries2.Add(Query.And(querymov));
                }
                if (queryobjs != null)
                {
                    listqueries2.Add(Query.And(queryobjs));
                }
                if (querystatus != null)
                {
                    listqueries2.Add(Query.And(querystatus));
                }
                if (queryusers != null)
                {
                    listqueries2.Add(Query.And(queryusers));
                }
                if (querylocs != null)
                {
                    listqueries2.Add(Query.And(querylocs));
                }
                listqueries2.Add(query);

                var queryfinal = Query.And(listqueries2); //new List<IMongoQuery>{query,dict1,done1,destroy1,auto1,vobo1,photo1}
                var cursor = collection.FindAs(typeof(BsonDocument), queryfinal);
                List<BsonDocument> documents = new List<BsonDocument>();
                foreach (BsonDocument document in cursor) //putting the docuemnts into a list
                {
                    //firts we have to check if this document has a valid structure
                    try
                    {
                        document.Set("_id", document.GetElement("_id").Value.ToString());
                        document.Set("CreatedTimeStamp", document.GetElement("CreatedTimeStamp").Value.ToString());
                    }
                    catch (Exception e) { }
                    documents.Add(document);
                }
                return documents.ToJson(); //getting the collection's cursor
                
               // return Join.Find(queryfinal); 
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }

        public String GetRowsReportDemand(Dictionary<string,string> fields, List<string> movement = null,List<string> objs=null,List<string> locations=null,List<string> users=null,List<int>status=null,Int64 start=0,Int64 end=0)
        {
           

            var orderfield = "folio";

        
            try
            {
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                 string[] arraymov = movement.ToArray();
                string[] arrayobjs = objs.ToArray();
                string[] arraylocations = locations.ToArray();
                string[] arrayusers = users.ToArray();
                int[] arraystatus = status.ToArray();
                
                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                BsonArray bsonarrayloc = new BsonArray();
                BsonArray bsonarrayusers = new BsonArray();
                BsonArray bsonarraystatus = new BsonArray();
              
                for (int i = 0; i < arraymov.Length; i++)
                {
                    bsonarraymov.Add(arraymov[i]);
                }
                for (int i = 0; i < arrayobjs.Length; i++)
                {
                    bsonarrayobjs.Add(arrayobjs[i]);
                } for (int i = 0; i < arraylocations.Length; i++)
                {
                    bsonarrayloc.Add(arraylocations[i]);
                } for (int i = 0; i < arrayusers.Length; i++)
                {
                    bsonarrayusers.Add(arrayusers[i]);
                }
                for (int i = 0; i < arraystatus.Length; i++)
                {
                    bsonarraystatus.Add(arraystatus[i]);
                }
                JoinCollections Join = new JoinCollections();
                Join.Select("Demand")
                .Join("Locations", "objects.location", "_id", "name=>nameLocation")
                .Join("Users", "Creator", "_id", "name=>nameUser")
                .Join("MovementProfiles", "movement", "_id", "name=>nameMovement");

                IMongoQuery query = null;
                if (movement != null && movement.Count>0)
                {
                    query = Query.And(Query.In("movement", bsonarraymov), Query.In("objects.objectReference", bsonarrayobjs), Query.In("objects.location", bsonarrayloc), Query.In("status", bsonarraystatus), Query.In("Creator", bsonarrayusers), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));
                    
                }
                else
                {
                    query = Query.And( Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                }

                return Join.Find(query);
            }
            catch (Exception e)
            {
            //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }
        public String GetRowsReportHistoryDemand(Dictionary<string, string> fields, List<string> movement = null, List<string> users = null,  Int64 start = 0, Int64 end = 0)
        {


            var orderfield = "folio";


            try
            {
                List<string> cols = new List<string>();
                foreach (var x in fields)
                {
                    cols.Add(x.Key);
                }
                cols.Add("CreatedTimeStamp");
                string[] arrayfields = cols.ToArray();
                string[] arraymov = movement.ToArray();
                 string[] arrayusers = users.ToArray();
              
                BsonArray bsonarraymov = new BsonArray();
                BsonArray bsonarrayobjs = new BsonArray();
                BsonArray bsonarrayloc = new BsonArray();
                BsonArray bsonarrayusers = new BsonArray();
                BsonArray bsonarraystatus = new BsonArray();

                for (int i = 0; i < arraymov.Length; i++)
                {
                    bsonarraymov.Add(arraymov[i]);
                }
               for (int i = 0; i < arrayusers.Length; i++)
                {
                    bsonarrayusers.Add(arrayusers[i]);
                }
               
                JoinCollections Join = new JoinCollections();
                Join.Select("Demand")
                .Join("Locations", "objects.location", "_id", "name=>nameLocation")
                .Join("Users", "Creator", "_id", "name=>nameUser")
                .Join("MovementProfiles", "movement", "_id", "name=>nameMovement");

                IMongoQuery query = null;
                if (movement != null && movement.Count > 0)
                {
                    query = Query.And(Query.In("movement", bsonarraymov) , Query.In("Creator", bsonarrayusers), Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                }
                else
                {
                    query = Query.And(Query.GTE("CreatedTimeStamp", start), Query.LTE("CreatedTimeStamp", end));

                }

                return Join.Find(query);
            }
            catch (Exception e)
            {
                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                return null;
            }
        }

        public string GetDemandsAuthorizefor(string userid = null)
        {
            userid = (userid != null && userid != "") ? userid : "";

            BsonArray bsonstatus = new BsonArray();
            bsonstatus.Add(3);
            IMongoQuery query;
            if (userid != null && userid != "")
                query = Query.And(Query.In("status", bsonstatus), Query.EQ("authorizations.id_user", userid));
            else {
                query = Query.And(Query.In("status", bsonstatus));
            }

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemove, typeMovement=>type,temporal=>temporal");

            return Join.Find(query);
        }

        public string GetDemandsApprovefor(string userid = null)
        {
            userid = (userid != null && userid != "") ? userid : "";

            BsonArray bsonstatus = new BsonArray();
            bsonstatus.Add(5);
            IMongoQuery query;
            if (userid != null && userid != "")
                query = Query.And(Query.In("status", bsonstatus), Query.EQ("approval.id_user", userid));
            else {
                query = Query.And(Query.In("status", bsonstatus));
            }
            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemove, typeMovement=>type, temporal=>temporal");

            return Join.Find(query);
        }

        public string GetDemandsAdjudicatefor(string userid = null)
        {
            userid = (userid != null && userid != "") ? userid : "";

            BsonArray bsonstatus = new BsonArray();
            bsonstatus.Add(1);
            IMongoQuery query;
            if (userid != null && userid != "")
                query = Query.And(Query.In("status", bsonstatus), Query.EQ("adjudicating", userid));
            else {
                query = Query.And(Query.In("status", bsonstatus));
            }

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemove, typeMovement=>type,temporal=>temporal");

            return Join.Find(query);
        }

        public string GetDemandsUpdatefor(string userid = null)
        {
            userid = (userid != null && userid != "") ? userid : "";

            BsonArray bsonstatus = new BsonArray();
            bsonstatus.Add(8);
            bsonstatus.Add(2);
            bsonstatus.Add(4);
            IMongoQuery query;
            if (userid != null && userid != "")
                query = Query.And(Query.In("status", bsonstatus), Query.EQ("Creator", userid));
            else {
                query = Query.And(Query.In("status", bsonstatus));
            }
            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemove, typeMovement=>type,temporal=>temporal");

            return Join.Find(query);
        }

        public string GetDemandsUpdateforContabilidad(string userid = null)
        {
            userid = (userid != null && userid != "") ? userid : "";

            BsonArray bsonstatus = new BsonArray();
            bsonstatus.Add(9);
            IMongoQuery query;
            if (userid != null && userid != "")
                query = Query.And(Query.In("status", bsonstatus), Query.EQ("contador.id_user", userid));
            else {
                query = Query.And(Query.In("status", bsonstatus));
            }

            JoinCollections Join = new JoinCollections();
            Join.Select("Demand")
                .Join("MovementProfiles", "movement", "_id", "name=>namemove, typeMovement=>type,temporal=>temporal");

            return Join.Find(query);
        }
    }
}
