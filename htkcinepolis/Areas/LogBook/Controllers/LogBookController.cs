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

        public Dictionary<string, string> mappingCollections()
        {
            try
            {
                Dictionary<string, string> collections = new Dictionary<string, string>();

                collections.Add("Alerts", "Alertas");
                collections.Add("Adjudicating", "Dictaminadores");
                collections.Add("AuthorizationMovement", "relacion Mov-Autorizadores");
                collections.Add("Categories", "Categorias");
                collections.Add("CircuitLocation", "Ubicacion de Circuitos");
                collections.Add("Circuits", "Circuitos");
                collections.Add("CustomFields", "Campos Custom");
                collections.Add("DashboardWidgets", "Wigets del Dashboard");
                collections.Add("Dashboard", "Configuracion del Dashboard");
                collections.Add("Demand", "Solicitudes");
                collections.Add("DemandAuthorization", "Autorizadores de solicitudes");
                collections.Add("DemandDesktop", "Movimientos migrados");
                collections.Add("Design", "Diseño");
                collections.Add("EPCController", "Control de Epc");
                collections.Add("FlowRules", "Reglas");
                collections.Add("Hardware", "Hardware");
                collections.Add("HardwareCategories", "Categorias de Hw");
                collections.Add("HardwareFields", "Campos de Hw");
                collections.Add("HardwareReference", "Hardware de referencia");
                collections.Add("HardwareRules", "Reglas de Hw");
                collections.Add("Inventory", "Inventarios");
                collections.Add("Limits", "Limites de cliente");
                collections.Add("Lists", "Listas");
                collections.Add("LocationCustomFields", "Campos de Ubicaciones");
                collections.Add("LocationProfiles", "Perfiles de Ubicaciones");
                collections.Add("LocationRules", "Reglas de ubicaciones");
                collections.Add("Locations", "Ubicaciones");
                collections.Add("Message", "Mensajes");
                collections.Add("MovementFields", "Campos de movimientos");
                collections.Add("MovementProfiles", "Perfiles de Movimientos");
                collections.Add("Notifications", "Notificaciones");
                collections.Add("ObjectAssignHistory", "Historial de Asignaciones de Objetos");
                collections.Add("ObjectFields", "Campos de objetos");
                collections.Add("ObjectReal", "Activos finales");
                collections.Add("ProccessRules", "Reglas de procesos");
                collections.Add("Proccesses", "Procesos");
                collections.Add("ProcessesDiagram", "Diagrama de procesos");
                collections.Add("Profiles", "Perfiles de usuarios");
                collections.Add("ReferenceObjects", "Objetos de referencia");
                collections.Add("Reports", "Reportes");
                collections.Add("Semaphore", "Semaforos");
                collections.Add("SystemSettings", "Configuracion del sistema");
                collections.Add("Tickets", "Tickets");
                collections.Add("userMessage", "Mensajes");
                collections.Add("Users", "Usuarios");
                collections.Add("UsersTickets", "Tickets");
                collections.Add("result", "Resultados");
                collections.Add("witnesses", "witnesses");

                return collections;
            }
            catch {

                return new Dictionary<string, string>();
            
            }
        }
        public ActionResult Index()
        {
            String rowArray = logBookTable.GetRows();
            JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);
            List<string> userslist = new List<string>();
            Dictionary<string, string> usersdict = new Dictionary<string, string>();
            try
            {
                userslist = (from user in objects select (string)user["userid"]).ToList();
                RivkaAreas.Reports.Models.ObjectsRealReport db = new RivkaAreas.Reports.Models.ObjectsRealReport("Users");
                JArray result = JsonConvert.DeserializeObject<JArray>(db.GetbyCustom("_id", userslist, "Users"));
                foreach (JObject item in result)
                {
                    try
                    {
                        usersdict.Add(item["_id"].ToString(), item["name"].ToString()+" "+item["lastname"].ToString());
                    }
                    catch { }
                }

            }
            catch
            {

            }
            JArray aux = new JArray();
            foreach (JObject obj in objects)
            {
               // String userarray = userTable.GetRow(obj["userid"].ToString());
                obj.Add("username", "");
                try
                {
                    if (obj["userid"].ToString() == "1")
                    {
                        obj["username"] = "Sistema";

                    }
                    else
                    {

                        obj["username"] = usersdict[obj["userid"].ToString()];
                    }
                }
                catch (Exception ex)
               {
                    
                }
                try
                {
                    string movtext = obj["movement"].ToString().ToLower();
                    if (movtext.Contains("insert"))
                    {
                        obj["movement"] = movtext.Replace("insert", "Alta");
                    }
                    else if(movtext.Contains("update")){
                    
                       obj["movement"] = movtext.Replace("update", "Actualización");
                    }else if(movtext.Contains("delete")){
                    
                       obj["movement"] = movtext.Replace("delete", "Baja");
                    }
                }
                catch { }
                try
                {
                    string collection = obj["collection"].ToString();
                    Dictionary<string, string> collections = mappingCollections();
                    obj["collection"] = collections[collection];
                }
                catch { }
                aux.Add(obj);

            }
            objects = aux;
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
