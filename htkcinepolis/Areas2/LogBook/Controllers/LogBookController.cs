using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.LogBook.Models;
using Rivka.Db;
using Rivka.Form;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using Rivka.Db.MongoDb;

namespace RivkaAreas.LogBook.Controllers
{
    public class LogBookController : Controller
    {
        //
        // GET: /LogBook/LogBook/

         protected RivkaAreas.LogBook.Models.LogBook logBookTable;
         protected UserTable userTable;

        public LogBookController(){
            logBookTable = new Models.LogBook();
            userTable = new UserTable();
        }

        public ActionResult Index()
        {
            String rowArray = logBookTable.GetRows();
            JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);

            //foreach (JObject obj in objects)
            //{
            //    String userarray = userTable.GetRow(obj["userid"].ToString());
            //    try
            //    {
            //        JObject obj1 = JsonConvert.DeserializeObject<JObject>(userarray);
            //        obj["username"] = obj1["name"].ToString() + " " + obj1["lastname"].ToString();
            //    }
            //    catch (Exception ex)
            //    {
            //        obj["username"] = "Sistema";
            //    }

            //}

            return View(objects);
        }

        public int SaveLog(String userid, String module, String move, String tabla, String fecha){
            int num = 0;
            JObject logobj = new JObject();
            logobj["userid"] = userid;
            logobj["module"] = module;
            logobj["movement"] = move;
            logobj["collection"] = tabla;
            logobj["movementDate"] = fecha;
            logBookTable.SaveRow(JsonConvert.SerializeObject(logobj));
            return num;
        }

        public ActionResult getData()
        {
            String rowArray = logBookTable.GetRows();
            JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);

            foreach (JObject obj in objects)
            {
                String userarray = userTable.GetRow(obj["userid"].ToString());
                try
                {
                    JObject obj1 = JsonConvert.DeserializeObject<JObject>(userarray);
                    obj["username"] = obj1["name"].ToString() + " " + obj1["lastname"].ToString();
                }
                catch (Exception ex)
                {
                    obj["username"] = "Sistema";
                }

            }
          
               
                //result.Add("objects", objectsString);
                //ViewData["resultjson"] = JsonConvert.SerializeObject(result);
               
                //return View();
                return Json(JsonConvert.SerializeObject(objects));
           
        }
    }
}
