﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RivkaAreas.ObjectAdmin.Models;
using Rivka.Db;
using Rivka.Form;
using Rivka.Form.Field;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;
using Rivka.Security;
using Rivka.Error;
using System.Threading;
using System.Threading.Tasks;
namespace RivkaAreas.ObjectAdmin.Controllers
{
    [Authorize]
    public class ObjectAdminController : Controller
    {
        //
        // GET: /ObjectAdmin/ObjectAdmin/

        protected CategoryTable categoryTable;
        protected ObjectReal _objectTable;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;
        protected ObjectTable _objectReferenceTable;  
        protected ProfileTable _profileTable;
        protected LocationTable locationTable;
        protected UserProfileTable _userprofileTable;
        protected CustomFieldsTable fieldTable;
        protected UserTable userTable;
        protected LocationProfileTable locationProfileTable;
        protected validatePermissions validatepermissions;
        protected ObjectFieldsTable _objFieldsTable;
        protected ListTable _listTable;
        protected Dictionary<string, string> departs = new Dictionary<string, string>();

        public ObjectAdminController()
        {
            //this._common = new CommonFunctions();
            this._objectTable = new ObjectReal("ObjectReal");
            this._profileTable = new ProfileTable("MovementProfiles");
            this.categoryTable = new CategoryTable();
            this.locationTable = new LocationTable();
            this.userTable = new UserTable();
            this._objectReferenceTable = new ObjectTable();
            this._userprofileTable = new UserProfileTable();
            this.fieldTable = new CustomFieldsTable("ObjectFields");
            this.locationProfileTable = new LocationProfileTable();
            validatepermissions = new validatePermissions();
            _objFieldsTable = new ObjectFieldsTable();
            _listTable = new ListTable();
            this._logTable = new LogBook.Controllers.LogBookController();
        }

        public ActionResult Index()
        {
            //String dataPermissions = "";
            //try
            //{
            //    dataPermissions = Session["Permissions"].ToString();
            //}
            //catch (Exception e)
            //{
            //    return Redirect("~/Home");
            //}
            ////TODO: advanced search using Agregation.
            String dataPermissions;
            String dataPermissionsClient;
            try
            {
                dataPermissions = Session["Permissions"].ToString();
                dataPermissionsClient = Session["PermissionsClient"].ToString();
            }
            catch (Exception e) {
                return Redirect("~/Home");  
            }
            bool access = false;
            bool accessClient = false;
          
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                //var profiles = _profileTable.getRows();
                //return View(profiles);

                String rowArray = _objectTable.GetRows();
                JArray objects = JsonConvert.DeserializeObject<JArray>(rowArray);

                rowArray = _userprofileTable.GetRows();
                JArray users = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data = new Dictionary<string, string>();

                rowArray = userTable.GetRow(Session["_id"].ToString()); 
                JObject userlog = JsonConvert.DeserializeObject<JObject>(rowArray);
                rowArray = _userprofileTable.GetRow(userlog["profileId"].ToString());
                JObject userprofile = JsonConvert.DeserializeObject<JObject>(rowArray);

                foreach (JObject items in users)
                {
                    data.Add(items["_id"].ToString(), items["name"].ToString());
                }

                String fieldsString = fieldTable.GetRows();
                JArray fieldsArray = JsonConvert.DeserializeObject<JArray>(fieldsString);

                rowArray = _objectTable.GetObjectsReferences();
                JArray objsref = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data2 = new Dictionary<string, string>();

                foreach (JObject items in objsref)
                {
                    if (!data2.ContainsKey(items["_id"].ToString()))
                        data2.Add(items["_id"].ToString(), items["name"].ToString());
                }

               rowArray = locationTable.GetRows();
                JArray locat = JsonConvert.DeserializeObject<JArray>(rowArray);
                Dictionary<string, string> data3 = new Dictionary<string, string>();

                foreach (JObject items in locat)
                {
                    if (!data3.ContainsKey(items["_id"].ToString()))
                        data3.Add(items["_id"].ToString(), items["name"].ToString());
                }

                loadDepartments();
                loadproveedores();
                ViewData["locations"] = data3;

                ViewData["users"] = data;
                ViewData["fields"] = fieldsArray;
                ViewData["objsReference"] = data2;
                ViewData["userProfile"] = userprofile["name"].ToString();

                return View(objects);
            }
            else
            {

                return Redirect("~/Home");
            } 
        }

        public bool getpermissions(string permission, string type)
        {
            var datos = "";
            try
            {
                datos = Session["Permissions"].ToString();
            }
            catch (Exception ex)
            {
                if (Request.Cookies["permissions"] != null)
                {
                    Session["Permissions"] = Request.Cookies["permissions"].Value;
                    datos = Session["Permissions"].ToString();
                }

            }
            JObject allp = JsonConvert.DeserializeObject<JObject>(datos);

            if (allp[permission]["grant"].Count() > 0)
            {
                foreach (string x in allp[permission]["grant"])
                {
                    if (x.Contains(type))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        /// <summary>
        /// Sends the instructions to Edgeware for printing
        /// </summary>
        /// <param name="labels"></param>
        /// <returns></returns>
        public JsonResult PrintLabels(string labels) 
        {
            string result = "";



            return Json(result);
        }

        /// <summary>
        ///     This method allows to get the document's childs by id
        /// </summary>
        /// <param name="parentCategory">
        ///     The category's id that we want to find its children
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns an array with the information needed to represent a tree
        /// </returns>
        public JsonResult getNodeContent(String parentCategory)
        {
            if (parentCategory == "") parentCategory = "null";
            String categoriesString = locationTable.Get("parent", parentCategory);
            String objectsString = _objectTable.Get("parentCategory", parentCategory);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
            JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);

            //categoriesObject.Add(objectObject);

            foreach (JObject category in categoriesObject)
            {
                try
                { //try to remove customFields, if can't be removed it doesn't care
                    category.Remove("customFields");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("parentCategory");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("CreatedDate");
                }
                catch (Exception e) { /*Ignored*/ }

                try
                {
                    category.Remove("LastmodDate");
                }
                catch (Exception e) { /*Ignored*/ }
            }
            categoriesString = JsonConvert.SerializeObject(categoriesObject);

            return Json(categoriesString);
        }

        public JsonResult getObjectInfo(String idobject)
        {
            try
            {
                String rowString = _objectTable.GetRow(idobject); 
                JObject row = JsonConvert.DeserializeObject<JObject>(rowString);

                rowString = _objectReferenceTable.GetRow(row["objectReference"].ToString());
                if (rowString != null) {
                    JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);
                    row["profileFields"] = row1["profileFields"];
                    row["category"] = row1["parentCategory"].ToString();
                    row["ext"] = row1["ext"].ToString();

                    row["currentmove"] = _objectTable.GetdemandFolio(idobject);
                    row["allmoves"] = _objectTable.GetAlldemandsFolio(idobject);

                    try{
                        if (row["currentmove"].ToString() != " " && row["currentmove"].ToString() != "")
                        {
                            row["status"] = "En movimiento";

                        }
                        else {
                            if (row["system_status"].ToString() == "false" || row["system_status"].ToString() == "False")
                            {
                                row["status"] = "Dado de baja";
                            }
                            else {
                                row["status"] = "Está en tu conjunto";
                            }
                            
                        }

                    }
                    catch {
                        row["status"] = "Está en tu conjunto";
                    }
                    
                    try {
                        if (row["serie"].ToString() == "")
                        {
                            row["serie"] = row1["serie"].ToString();

                        } 
                    }
                    catch {
                        row["serie"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "serie") { 
                            if(row1["serie"].ToString()!="")
                               row["serie"] = row1["serie"].ToString();
                            }
                        }
                    
                    }
                    try {
                        if (row["price"].ToString() == "")
                        {
                            row["price"] = row1["precio"].ToString();
                        } 
                    }
                    catch {
                        row["price"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "precio")
                            {
                                if (row1["precio"].ToString() != "")
                                    row["price"] = row1["precio"].ToString();
                            }
                        }
                    }
                    try {
                        if (row["department"].ToString() == "")
                        {
                            row["department"] = row1["department"].ToString();
                        }
                    }
                    catch {
                        row["department"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "department")
                            {
                                if (row1["department"].ToString() != "")
                                    row["department"] = row1["department"].ToString();
                            }
                        }
                    }
                    //try
                    //{
                    //    row["marca"] = row1["marca"].ToString();
                    //}
                    //catch { 
                    
                    //}


                    try
                    {
                        if (row["marca"].ToString() == "")
                        {
                            row["marca"] = row1["marca"].ToString();
                        }
                    }
                    catch
                    {
                        row["marca"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "marca")
                            {
                                if (row1["marca"].ToString() != "")
                                    row["marca"] = row1["marca"].ToString();
                            }
                        }
                    }
                    try
                    {
                        if (row["modelo"].ToString() == "")
                        {
                            row["modelo"] = row1["modelo"].ToString();
                        } 
                    }
                    catch {
                        row["modelo"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "modelo")
                            {
                                if (row1["modelo"].ToString() != "")
                                    row["modelo"] = row1["modelo"].ToString();
                            }
                        }
                    }
                    try
                    {
                        if (row["perfil"].ToString() == "")
                        {
                            row["perfil"] = row1["perfil"].ToString();
                        }
                    }
                    catch {
                        row["perfil"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "perfil")
                            {
                                if (row1["perfil"].ToString() != "")
                                    row["perfil"] = row1["perfil"].ToString();
                            }
                        }
                    }
                   
                    try
                    {
                        if (row["object_id"].ToString() == "")
                        {
                            row["object_id"] = row1["object_id"].ToString();
                        } 
                    }
                    catch {
                        row["object_id"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "object_id")
                            {
                                if (row1["object_id"].ToString() != "")
                                    row["object_id"] = row1["object_id"].ToString();
                            }
                        }
                    }
                    
                    try
                    {
                        if (row["folio"].ToString() == "")
                        {
                            row["folio"] = row1["folio"].ToString();
                        }
                    }
                    catch
                    {
                        row["folio"] = "";
                        foreach (KeyValuePair<string, JToken> token in row1)
                        {
                            if (token.Key == "folio")
                            {
                                if (row1["folio"].ToString() != "")
                                    row["folio"] = row1["folio"].ToString();
                            }
                        }
                    }

                    //try
                    //{
                    //    if (row["folio"].ToString() == "")
                    //    {
                    //        row["folio"] = row1["folio"].ToString();
                    //    }
                    //}
                    //catch
                    //{
                    //    row["folio"] = "";
                    //    foreach (KeyValuePair<string, JToken> token in row1)
                    //    {
                    //        if (token.Key == "folio")
                    //        {
                    //            if (row1["folio"].ToString() != "")
                    //                row["folio"] = row1["folio"].ToString();
                    //        }
                    //    }
                    //}

                    row["idreference"] = row1["_id"].ToString();

                    row["locationRoute"] = getRoute2(row["location"].ToString());
                }


                rowString = locationTable.GetRow(row["location"].ToString());
                if (rowString != null)
                {
                    JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);

                    row["locationname"] = row1["name"].ToString();
                }
                try
                {
                    rowString = userTable.GetRow(row["Creator"].ToString());
                
                    if (rowString != null)
                    {
                        JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);

                        row["username"] = row1["name"].ToString()+" "+row1["lastname"].ToString();
                    }
                }
                catch (Exception ex) { row["username"] = ""; }


                try
                {
                    rowString = categoryTable.GetRow(row["category"].ToString()); 

                    if (rowString != null)
                    {
                        JObject row1 = JsonConvert.DeserializeObject<JObject>(rowString);

                        row["nameCategory"] = row1["name"].ToString();
                    }
                }
                catch (Exception ex) { row["namecategory"] = ""; }
                return Json(JsonConvert.SerializeObject(row));
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String loadLocationsRegion(String userid)
        {

            try
            {
                String region = "";
                String userstring = userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = locationProfileTable.Get("name", "Region"); 
                string idregion = "";
                JArray conjuntja = new JArray();
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                JArray locatList = new JArray();
                JObject locat = new JObject();
                JArray ele = new JArray();
                String rowArray;
                List<String> list1 = new List<String>();
                List<String> list2 = new List<String>();

               
                //******************************************************************************
                foreach (JObject ob in locats)
                {
                    rowArray = locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idregion)
                    {
                        region =  locat["_id"].ToString();
                        break;
                    }

                }

                return region;

            }
            catch (Exception ex)
            {
                return null;

            }
        }

        public String loadLocationsConjunto(String userid)
        {

            try
            {
                String region = "";
                String userstring = userTable.GetRow(userid);
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());

                string getconjunt = locationProfileTable.Get("name", "Conjunto");
                string idregion = "";
                JArray conjuntja = new JArray();
                try
                {
                    conjuntja = JsonConvert.DeserializeObject<JArray>(getconjunt);
                    idregion = (from mov in conjuntja select (string)mov["_id"]).First().ToString();
                }
                catch (Exception ex) { }

                JArray locatList = new JArray();
                JObject locat = new JObject();
                JArray ele = new JArray();
                String rowArray;
                List<String> list1 = new List<String>();
                List<String> list2 = new List<String>();


                //******************************************************************************
                foreach (JObject ob in locats)
                {
                    rowArray = locationTable.GetRow(ob["id"].ToString());
                    locat = JsonConvert.DeserializeObject<JObject>(rowArray);
                    if (locat["profileId"].ToString() == idregion)
                    {
                        region = locat["_id"].ToString();
                        break;
                    }

                }

                return region;

            }
            catch (Exception ex)
            {
                return null;

            }
        }

        public JsonResult getNodeContent2(String id , String userid=null)
        {
            JObject result = new JObject();
            if (id == "") id = "null";
             String categoriesString="";
            if (id=="null" && userid != null) {
                 String userstring = userTable.GetRow(userid); 
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);

                userstring = _userprofileTable.GetRow(userobj["profileId"].ToString());  
                JObject profilobj = JsonConvert.DeserializeObject<JObject>(userstring);
                if (profilobj["name"].ToString() != "Administrador de sistema" && profilobj["name"].ToString() != "Otro")
                {
                    if (profilobj["name"].ToString() == "Gerente regional")
                    {
                        id = loadLocationsRegion(userid);
                        
                    }
                    else if (profilobj["name"].ToString() == "Gerente de conjunto")
                    {
                        id = loadLocationsConjunto(userid);
                    }
                    else {
                        id = loadLocationsConjunto(userid);
                    }

                    result["id"] = id;
                    id=(id==null || id=="null")?"":id;
                    if (id == "") result["name"] = "";
                    else {
                        try
                        {
                            categoriesString = locationTable.GetRow(id);
                            result["name"] = (JsonConvert.DeserializeObject<JObject>(categoriesString))["name"].ToString();
                        }
                        catch {
                        
                        }
                        }
                    
                }
                else { id = "null";
                result["id"] = id;
                result["name"] = "Home";
                
                }
            }

            id = (id == "") ? "null" : id;
            categoriesString = locationTable.Get("parent", id);
          //  String objectsString = _objectTable.Get("parentCategory", id);

            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
           // JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);


            JArray newobjs = new JArray();

            foreach (JObject obj in categoriesObject)
            {
                JObject obj1 = new JObject();
                obj1["id"] = obj["_id"];
                obj1["text"] = obj["name"];
                obj1["hasChildren"] = true;

                //  obj1["items"] = "[]";
                obj1["spriteCssClass"] = "objectimg";
                newobjs.Add(obj1);
            }
            result["hijos"] = newobjs;
         
            //   return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
            return Json(JsonConvert.SerializeObject(result), JsonRequestBehavior.AllowGet);
        }
         public String generateIndex(int total,int take)
        {
          
        try{
                     

              System.Text.StringBuilder pagination = new System.Text.StringBuilder();

                    try
                    {
                    Dictionary<string,string> pairs=new Dictionary<string,string>();
                  
                        int skip=0;
                        int index=0;
                        if (total < take)
                        {
                            pairs.Add("0", total.ToString());
                        }
                        else
                        {
                            for (int i = 0; i < total; i++)
                            {

                                if (index == take)
                                {
                                    pairs.Add(((i + 1) - take).ToString(), i.ToString());


                                    if (total - i < take)
                                    {
                                        pairs.Add((i + 1).ToString(), total.ToString());
                                    }

                                    index = 0;
                                }

                                index++;

                            }
                        }

                        foreach (var dict in pairs)
                        {
                            try
                            {
                                pagination.Append(" <option value='" + dict.Key + "' data-skip='" + dict.Key + "' >" + dict.Key + "-" + dict.Value + "</option>");
                            }
                            catch { }
                        }

                      
                    }
                    catch
                    {
                       
                    }
                  
                    return pagination.ToString();
                
           }
          catch{
              return "<option value='0'>Todos</option>";
           }
         
        }
        /// <summary>
        ///     Allows to get the table elements to show in the object references section
        /// </summary>
        /// <param name="parentCategory">
        ///     Specifies which element to show
        /// </param>
        /// <author>
        ///     Luis Gonzalo Quijada Romero
        /// </author>
        /// <returns>
        ///     Returns a json with the elements to show
        /// </returns>
        //[HttpPost]
        public ActionResult getData(String parentCategory = "null", String vertodo="0",String skip="0")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            int skip1 = 0;
            try
            {
                skip1 = Convert.ToInt16(skip);
            }
            catch { }
            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);
            loadDepartments();
            bool isconjunt = false;
            try
            {
                JObject location1 = JsonConvert.DeserializeObject<JObject>(locationTable.GetRow(parentCategory));

                JObject categories = JsonConvert.DeserializeObject<JObject>(locationProfileTable.GetRow(location1["profileId"].ToString()));

                if (categories["name"].ToString().ToLower().Contains("conjunto") || categories["name"].ToString().ToLower().Contains("ubicacion"))
                {
                    isconjunt = true;
                }
               


            }
            catch { }
            JObject result = new JObject();
            if (access == true && accessClient == true)
            {
                if (parentCategory == "") parentCategory = "null";
                String objectsString="";
                String categoryString = "";
                JArray objectsObject = new JArray();
                JArray categories = new JArray();
                totalglobal=0;
                numActivos = 0;
                numBaja = 0;
                numMov = 0; 
                JArray paginationja = new JArray();
                if (isconjunt==false)
                {
                   
                    result.Add("objects", "[]");
                    result.Add("total", totalglobal);
                    result.Add("activos", numActivos);
                    result.Add("dadosbaja", numBaja);
                    result.Add("enMov", numMov);
                    result.Add("options", "null");
                    return Json(JsonConvert.SerializeObject(result));

                }else
                if (vertodo == "0")
                {
                    objectsString = _objectTable.GetObjects(parentCategory);
                    categoryString = categoryTable.GetRows();
                    //doing changes to objects array
                    objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                    categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                    Dictionary<string, string> listCategories = new Dictionary<string, string>();
                    foreach (JObject items in categories)
                    {
                        listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                    }
                    int take = 0;
                     totalglobal = objectsObject.Count();
                     List<string> idsact1 = new List<string>();
                       JArray getdemandin = new JArray();
                     JArray getdemandout = new JArray();
                     try
                     {
                         foreach (JObject o1 in objectsObject)
                         {
                             idsact1.Add(o1["_id"].ToString());
                         }
                         //idsact1 = (from id in objectsObject select (string)id).ToList();
                         try
                         {
                             getdemandin = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 0));
                         }
                         catch { }
                         try
                         {
                             getdemandout = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 1));

                         }
                         catch { }
                     }
                     catch
                     {

                     }
                     List<string> idsact = new List<string>();
                   // foreach (JObject document in objectsObject)

                     
                     Parallel.ForEach(objectsObject, (itemtoken, pls, indexfor) =>
                      {
                          JObject document = itemtoken as JObject;
                       /*   if (indexfor > skip1)
                          {
                              //continue;
                              return;
                          }
                          else
                          {
                              /*if (take == 5000)

                                  break;
                                take++;*/
                            /*  if (indexfor >= 5000)
                                  pls.Stop();

                          }*/
                          try
                          {
                              idsact.Add(document["_id"].ToString());
                          }
                          catch
                          {

                          }
                          if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                              document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                          document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();
                          try
                          {
                              //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                              document["currentmove"] = "";
                              foreach (JObject item in getdemandout)
                              {
                                  try
                                  {
                                      foreach (JObject obj in item["objects"])
                                      {
                                          try
                                          {
                                              if (obj["id"].ToString() == document["_id"].ToString())
                                              {
                                                  document["currentmove"] = item["folio"].ToString() + " " + item["namemov"].ToString();
                                                  break;
                                              }
                                          }
                                          catch
                                          {

                                          }
                                      }
                                  }
                                  catch
                                  {

                                  }
                              }
                          }
                          catch (Exception ex)
                          {
                              document["currentmove"] = "";
                          }

                          try
                          {
                              //  document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                              document["allmoves"] = "";
                              List<string> folioslist = new List<string>();
                              foreach (JObject item in getdemandin)
                              {
                                  try
                                  {
                                      foreach (JObject obj in item["objects"])
                                      {
                                          try
                                          {
                                              if (obj["id"].ToString() == document["_id"].ToString())
                                              {
                                                  folioslist.Add(item["folio"].ToString() + " " + item["namemov"].ToString());

                                              }
                                          }
                                          catch
                                          {

                                          }
                                      }
                                  }
                                  catch
                                  {

                                  }
                              }
                              document["allmoves"] = String.Join(",\n ", folioslist);
                          }
                          catch (Exception ex)
                          {
                              document["allmoves"] = "";
                          }
                          try
                          {
                              if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                              {
                                  document["status"] = "En movimiento";
                                  numMov++;
                              }
                              else
                              {
                                  if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                                  {
                                      document["status"] = "Dado de baja"; numBaja++;
                                  }
                                  else
                                  {
                                      document["status"] = "Está en tu conjunto"; numActivos++;
                                  }

                              }

                          }
                          catch
                          {
                              document["status"] = "Está en tu conjunto"; numActivos++;
                          }


                          try
                          {
                              if (document["ext"].ToString() != "")
                              {
                                  document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                              }
                          }
                          catch (Exception e) { /*Ignored*/ }
                          JToken aux;
                          if (!document.TryGetValue("etiquetado", out aux))
                          {
                              document.Add("etiquetado", "Normal");
                          }
                          if (document.TryGetValue("label", out aux))
                          {

                              if (document["label"].ToString() == "normal")
                              {
                                  document["etiquetado"] = "Normal";
                              }
                              else
                              {
                                  document["etiquetado"] = "No Etiquetable";
                              }

                          }
                          if (!document.TryGetValue("nameassetType", out aux))
                          {
                              document.Add("nameassetType", "");
                          }
                          if (document.TryGetValue("assetType", out aux))
                          {
                              if (document["assetType"].ToString().ToLower().Contains("system"))
                                  document["nameassetType"] = "Sistemas";
                              else if (document["assetType"].ToString().ToLower().Contains("maintenance"))
                                  document["nameassetType"] = "Mantenimiento";
                              else
                              {
                                  document["nameassetType"] = "Proyección y Sonido";
                              }
                          }


                          if (departs != null)
                          {
                              if (!document.TryGetValue("departmentName", out aux))
                              {
                                  document.Add("departmentName", "");
                              }
                              try
                              {
                                  if (departs.ContainsKey(document["department"].ToString()))
                                      document["departmentName"] = departs[document["department"].ToString()];
                              }
                              catch
                              {
                                  document["departmentName"] = "";
                              }

                          }
                          else
                          {
                              document["departmentName"] = "";
                          }

                          paginationja.Add(document);

                      });
                    //objectsObject = paginationja;
                    //try
                    //{
                    //    RivkaAreas.Reports.Models.ObjectsRealReport objdc = new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                    //    JArray actjo = JsonConvert.DeserializeObject<JArray>(objdc.GetbyCustom("_id", idsact, "ObjectReal"));
                    //    Dictionary<string, string> namesdict = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["name"]);
                    //    Dictionary<string, string> namesdict4 = actjo.ToDictionary(x => (string)x["_id"], x => (string)x["assetType"]);

                    //    JArray newja = new JArray();
                    //    foreach (JObject obj in objectsObject)
                    //    {
                    //        try
                    //        {

                    //            JToken tk;
                    //            if (namesdict.ContainsKey(obj["_id"].ToString()))
                    //            {
                    //                string name = namesdict[obj["_id"].ToString()];
                    //                if (name.Length > 0)
                    //                    obj["name"] = name;
                    //            }
                    //        }
                    //        catch { }
                    //        try
                    //        {


                    //            if (namesdict4.ContainsKey(obj["_id"].ToString()))
                    //            {
                    //                string nameasset = namesdict4[obj["_id"].ToString()];
                    //                if (nameasset.Length > 0) {
                    //                    if (nameasset.ToLower().Contains("system"))
                    //                        obj["nameassetType"] = "Sistemas";
                    //                    else if (nameasset.ToLower().Contains("maintenance"))
                    //                        obj["nameassetType"] = "Mantenimiento";
                    //                    else
                    //                    {
                    //                        obj["nameassetType"] = "Proyección y Sonido";
                    //                    }
                    //                }
                    //            }
                    //        }
                    //        catch { }

                    //        newja.Add(obj);
                    //    }
                    //    objectsObject = newja;
                    //}
                    //catch
                    //{

                    //}
               
                    objectsString = JsonConvert.SerializeObject(objectsObject);
                }
                else { //objects recursively

                   objectsString = GetAllSubObjects( parentCategory,skip1);
                }

                String options="";
                try
                {
                    options = generateIndex(totalglobal, 5000);
                }
                catch { }
               
                result.Add("objects", objectsString);
                result.Add("total", totalglobal);
                result.Add("activos", numActivos);
                result.Add("dadosbaja", numBaja);
                result.Add("enMov", numMov);
                result.Add("options", "");
                ViewData["resultjson"] = JsonConvert.SerializeObject(result);
               
                return View("search");
               // return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public String getFormView2(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    String document = categoryTable.GetRow(profile);
                    JObject row = JsonConvert.DeserializeObject<JObject>(document);
                    String formString = row["customFields"].ToString();
                    String response = CustomForm.getFormView(formString, "ObjectFields"); //we use the CustomForm class to generate the form's fiew
                    //return response.Replace("HTKField", "HTKFieldDetalles");
                    return response;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public String getFormTitlesView(String profile)
        {
            if (this.Request.IsAjaxRequest()) //only available with AJAX
            {
                try
                {
                    String document = categoryTable.GetRow(profile);
                    JObject row = JsonConvert.DeserializeObject<JObject>(document);
                    String formString = row["customFields"].ToString();
                    String response = CustomForm.getFormTitlesView(formString); //it use the CustomForm class to create the headers
                    return response;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return null;
        }

        public JsonResult getusers(String profileid)
        {
            String userstring;
            JArray users = new JArray();
            if (profileid == "null" || profileid == null)
            {
                userstring = userTable.GetRows();
            }
            else {
                userstring = userTable.Get("profileId", profileid);
            }
             
            users = JsonConvert.DeserializeObject<JArray>(userstring);
            return Json(JsonConvert.SerializeObject(users));

        }

        public JsonResult getObjectFields()
        {
            String objstring;
            objstring = _objFieldsTable.GetRows();
            return Json(objstring);

        }

        public void AsignarUsuario(String selectids, String iduser) {
            String objstring="";
            JObject objs = new JObject();
            JArray arreglo = JsonConvert.DeserializeObject<JArray>(selectids);
            String jsonData;
            foreach (String cad in arreglo)
            {
                objstring = _objectTable.GetRow(cad);
                objs = JsonConvert.DeserializeObject<JObject>(objstring);
                objs["userid"] = iduser;

                jsonData = JsonConvert.SerializeObject(objs);

                _objectTable.SaveRow(jsonData,cad);
                _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + cad, "ObjectReal", DateTime.Now.ToString());
                //guardar en usuario
                userTable.UpdateObjects(iduser, cad);

            }
        
        }
        public void AsignarSerie(String selectid, String serie)
        {
            String objstring = "";
            JObject objs = new JObject();
            String jsonData;

            objstring = _objectTable.GetRow(selectid);
            objs = JsonConvert.DeserializeObject<JObject>(objstring);
            JToken val = "";
            if (objs.TryGetValue("serie", out val))
            {
                objs["serie"] = serie;
            }
            else
            {
                objs.Add("serie", serie);
            }
            jsonData = JsonConvert.SerializeObject(objs);

            _objectTable.SaveRow(jsonData, selectid);
            _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + selectid, "ObjectReal", DateTime.Now.ToString());

        }
        public void AsignarUsuarioSelect(String selectid, String iduser)
        {
            String objstring = "";
            JObject objs = new JObject();
            String jsonData;

            objstring = _objectTable.GetRow(selectid);
            objs = JsonConvert.DeserializeObject<JObject>(objstring);
            objs["userid"] = iduser;

            jsonData = JsonConvert.SerializeObject(objs);

            _objectTable.SaveRow(jsonData, selectid);
            _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + selectid, "ObjectReal", DateTime.Now.ToString());
            //guardar en usuario
            userTable.UpdateObjects(iduser, selectid);
            
        }

        public int totalglobal;
        public int numActivos;
        public int numBaja;
        public int numMov; 
        /// <summary>
        /// Returns all objects from a location
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public String GetAllSubObjects(string locationId,int skip=0)
        {
            string results = "";
            String categoryString = "";
            JArray categories = new JArray();
            try
            {
                results = _objectTable.GetSubObjects(locationId);
                JArray objectList = JsonConvert.DeserializeObject<JArray>(results);
                categoryString = categoryTable.GetRows();
                //doing changes to objects array
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();
                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }
                int take = 0;
                totalglobal = objectList.Count();
                JArray paginationja = new JArray();
                List<string> idsact1 = new List<string>();
                Dictionary<string, string> actin = new Dictionary<string, string>();
                Dictionary<string, string> actout = new Dictionary<string, string>();
                JArray getdemandin = new JArray();
                JArray getdemandout = new JArray();
                try
                {
                    idsact1 = (from id in objectList select (string)id["_id"]).ToList();
                    try
                    {
                         getdemandin = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 0));
                      }
                    catch { }
                    try
                    {
                        getdemandout = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 1));
                        
                    }
                    catch { }
                }
                catch
                {

                }
                numActivos = 0;
                numBaja = 0;
                numMov = 0; 
                List<string> idsact=new List<string>();
               // foreach (JObject document in objectList) //for each profile we create an option element with id as value and the name as the text
                    Parallel.ForEach(objectList, (itemtoken, pls, indexfor) =>
                     {
                         JObject document = itemtoken as JObject;
                        // if (objectList.IndexOf(document) < skip)
                        /* if(indexfor>skip)
                         {
                             //continue;
                             return;
                         }
                         else
                         {
                             /*if (take == 5000)

                                 break;
                               take++;*/
                          /*   if (indexfor >= 5000)
                                 pls.Stop();

                         }*/
                         try
                         {
                             idsact.Add(document["_id"].ToString());
                         }
                         catch
                         {

                         }
                         if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                             document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);

                         document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();

                         try
                         {
                         //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                             document["currentmove"] = "";
                             foreach (JObject item in getdemandout)
                             {
                                 try
                                 {
                                     foreach (JObject obj in item["objects"])
                                     {
                                         try
                                         {
                                             if (obj["id"].ToString() == document["_id"].ToString())
                                             {
                                                 document["currentmove"] = item["folio"].ToString() + " " + item["namemov"].ToString();
                                                 break;
                                             }
                                         }
                                         catch
                                         {

                                         }
                                     }
                                 }
                                 catch
                                 {

                                 }
                             }
                         }
                         catch (Exception ex)
                         {
                             document["currentmove"] = "";
                         }

                         try
                         {
                           //  document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                             document["allmoves"] = "";
                             List<string> folioslist = new List<string>();
                             foreach (JObject item in getdemandin)
                             {
                                 try
                                 {
                                     foreach (JObject obj in item["objects"])
                                     {
                                         try
                                         {
                                             if (obj["id"].ToString() == document["_id"].ToString())
                                             {
                                                folioslist.Add(item["folio"].ToString() + " " + item["namemov"].ToString());
                                                break;
                                             }
                                         }
                                         catch
                                         {

                                         }
                                     }
                                 }
                                 catch
                                 {

                                 }
                             }
                             try
                             {
                                 document["allmoves"] = String.Join(",\n ", folioslist);
                             }
                             catch
                             {
                                 document["allmoves"] = ".";
                             }
                         }
                         catch (Exception ex)
                         {
                             document["allmoves"] = "";
                         }

                         try
                         {
                             if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                             {
                                 document["status"] = "En movimiento";
                                 numMov++;
                             }
                             else
                             {
                                 if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                                 {
                                     document["status"] = "Dado de baja"; numBaja++;
                                 }
                                 else
                                 {
                                     document["status"] = "Está en tu conjunto"; numActivos++;
                                 }

                             }

                         }
                         catch
                         {
                             document["status"] = "Está en tu conjunto"; numActivos++;
                         }

                         if (document["label"].ToString() == "normal")
                             document["etiquetado"] = "Normal";
                         else
                             document["etiquetado"] = "No Etiquetable";

                         if (document["assetType"].ToString().ToLower().Contains("system"))
                             document["nameassetType"] = "Sistemas";
                         else if (document["assetType"].ToString().ToLower().Contains("maintenance"))
                             document["nameassetType"] = "Mantenimiento";
                         else
                         {
                             document["nameassetType"] = "Proyección y Sonido";
                         }

                         if (departs != null)
                         {
                             try
                             {
                                 if (departs.ContainsKey(document["department"].ToString()))
                                     document["departmentName"] = departs[document["department"].ToString()];
                             }
                             catch
                             {
                                 document["departmentName"] = "";
                             }
                         }
                         else
                         {
                             document["departmentName"] = "";
                         }
                         if (document["ext"].ToString() != "")
                         {
                             document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                             document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
                         }

                         paginationja.Add(document);
                     });
              //  objectList = paginationja;
                //try
                //{
                //    JArray newja = new JArray();
                //    foreach (JObject obj in objectList)
                //    {
                //        newja.Add(obj);
                //    }
                //    objectList = newja;
                //}
                //catch
                //{

                //}
                  //  string cad = JsonConvert.SerializeObject(paginationja);
                    return objectList.ToString();
            }
            catch (Exception e)
            {
                Error.Log(e, "Trying to get all Objects");
            }


            return results;
        }

        /// <summary>
        /// Get all objects from a location
        /// </summary>
        /// <param name="locationid"></param>
        /// <returns></returns>
        public String GetAllObjects(string locationid)
        {
            String objectsString = "";
            String categoryString = "";
            JArray objectsObject = new JArray();

            String locationsString = "";
            JArray locationsObject = new JArray();
            JArray categories = new JArray();
            objectsString = _objectTable.GetObjects(locationid);
            categoryString = categoryTable.GetRows();
            //doing changes to objects array
            objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
            categories = JsonConvert.DeserializeObject<JArray>(categoryString);

            Dictionary<string, string> listCategories = new Dictionary<string, string>();
            foreach (JObject items in categories)
            {
                listCategories.Add(items["_id"].ToString(), items["name"].ToString());
            }
            totalglobal = objectsObject.Count();
            JArray paginationja = new JArray();
            List<string> idsact1 = new List<string>();
            Dictionary<string, string> actin = new Dictionary<string, string>();
            Dictionary<string, string> actout = new Dictionary<string, string>();
            JArray getdemandin = new JArray();
            JArray getdemandout = new JArray();
            try
            {
                idsact1 = (from id in objectsObject select (string)id["_id"]).ToList();
                try
                {
                    getdemandin = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 0));
                }
                catch { }
                try
                {
                    getdemandout = JsonConvert.DeserializeObject<JArray>(_objectTable.GetDemandByObj(idsact1, 1));

                }
                catch { }
            }
            catch
            {

            }
            numActivos = 0;
            numBaja = 0;
            numMov = 0;
            List<string> idsact = new List<string>();
            Parallel.ForEach(objectsObject, (itemtoken, pls, indexfor) =>
            {
                JObject document = itemtoken as JObject;
                // if (objectList.IndexOf(document) < skip)
                /* if(indexfor>skip)
                 {
                     //continue;
                     return;
                 }
                 else
                 {
                     /*if (take == 5000)

                         break;
                       take++;*/
                /*   if (indexfor >= 5000)
                       pls.Stop();

               }*/
                try
                {
                    idsact.Add(document["_id"].ToString());
                }
                catch
                {

                }
                if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                    document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);

                document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();

                try
                {
                    //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                    document["currentmove"] = "";
                    foreach (JObject item in getdemandout)
                    {
                        try
                        {
                            foreach (JObject obj in item["objects"])
                            {
                                try
                                {
                                    if (obj["id"].ToString() == document["_id"].ToString())
                                    {
                                        document["currentmove"] = item["folio"].ToString() + " " + item["namemov"].ToString();
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    document["currentmove"] = "";
                }

                try
                {
                    //  document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    document["allmoves"] = "";
                    List<string> folioslist = new List<string>();
                    foreach (JObject item in getdemandin)
                    {
                        try
                        {
                            foreach (JObject obj in item["objects"])
                            {
                                try
                                {
                                    if (obj["id"].ToString() == document["_id"].ToString())
                                    {
                                        folioslist.Add(item["folio"].ToString() + " " + item["namemov"].ToString());
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                    try
                    {
                        document["allmoves"] = String.Join(",\n ", folioslist);
                    }
                    catch
                    {
                        document["allmoves"] = ".";
                    }
                }
                catch (Exception ex)
                {
                    document["allmoves"] = "";
                }

                try
                {
                    if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                    {
                        document["status"] = "En movimiento";
                        numMov++;
                    }
                    else
                    {
                        if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                        {
                            document["status"] = "Dado de baja"; numBaja++;
                        }
                        else
                        {
                            document["status"] = "Está en tu conjunto"; numActivos++;
                        }

                    }

                }
                catch
                {
                    document["status"] = "Está en tu conjunto"; numActivos++;
                }

                if (document["label"].ToString() == "normal")
                    document["etiquetado"] = "Normal";
                else
                    document["etiquetado"] = "No Etiquetable";

                if (document["assetType"].ToString().ToLower().Contains("system"))
                    document["nameassetType"] = "Sistemas";
                else if (document["assetType"].ToString().ToLower().Contains("maintenance"))
                    document["nameassetType"] = "Mantenimiento";
                else
                {
                    document["nameassetType"] = "Proyección y Sonido";
                }

                if (departs != null)
                {
                    try
                    {
                        if (departs.ContainsKey(document["department"].ToString()))
                            document["departmentName"] = departs[document["department"].ToString()];
                    }
                    catch
                    {
                        document["departmentName"] = "";
                    }
                }
                else
                {
                    document["departmentName"] = "";
                }
                if (document["ext"].ToString() != "")
                {
                    document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"] + "." + document["ext"]);
                    document.Add("image_thumb", "/Uploads/Images/ObjectReferences/thumb_" + document["objectReference"] + "." + document["ext"]);
                }

                paginationja.Add(document);
            });

            objectsObject = paginationja;
            //foreach (JObject document in objectsObject)
            //{
            //    if (listCategories.ContainsKey(document["parentCategory"].ToString()))
            //        document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);

            //    document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();
            //    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
            //    try
            //    {
            //        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
            //    }
            //    try
            //    {
            //        if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
            //        {
            //            document["status"] = "En movimiento";
            //            numMov++;
            //        }
            //        else
            //        {
            //            if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
            //            {
            //                document["status"] = "Dado de baja";
            //            }
            //            else
            //            {
            //                document["status"] = "Está en tu conjunto";
            //            }

            //        }

            //    }
            //    catch
            //    {
            //        document["status"] = "Está en tu conjunto";
            //    }
            //    if (document["label"].ToString() == "normal")
            //        document["etiquetado"] = "Normal";
            //    else
            //        document["etiquetado"] = "No Etiquetable";

            //    if (document["assetType"].ToString() == "system")
            //        document["nameassetType"] = "Sistemas";
            //    else if (document["assetType"].ToString() == "maintenance")
            //        document["nameassetType"] = "Mantenimiento";
            //    else
            //    {
            //        document["nameassetType"] = "Proyección y Sonido";
            //    }

            //    if (departs != null)
            //    {
            //        try
            //        {
            //            if (departs.ContainsKey(document["department"].ToString()))
            //                document["departmentName"] = departs[document["department"].ToString()];
            //        }
            //        catch
            //        {
            //            document["departmentName"] = "";
            //        }

            //    }
            //    else
            //    {
            //        document["departmentName"] = "";
            //    }
            //    try
            //    {
            //        if (document["ext"].ToString() != "")
            //        {
            //            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
            //        }

            //    }
            //    catch (Exception e) { /*Ignored*/ }


            //}

            objectsString = JsonConvert.SerializeObject(objectsObject);
            string objs = "";
            locationsString = locationTable.Get("parent", locationid);
            locationsObject = JsonConvert.DeserializeObject<JArray>(locationsString);
            foreach(JObject obj in locationsObject){

                objs=GetAllObjects(obj["_id"].ToString());

                if (objs == "" || objs == "[]") continue;
                if (objectsString == "[]") objectsString = objectsString.Replace("]", "");
                else
                   objectsString= objectsString.Replace(']', ',');
                   objs= objs.Replace("[","");
                   objectsString +=objs;
                
                
            }


            return objectsString;
        }

        public String GetHowToShowObjects(String idlocation) {
            String result = "";
            idlocation = (idlocation == "") ? "null" : idlocation;
            String rowString="";
            JObject rowArray = new JObject();
            if (idlocation == "null" || idlocation == null)
            {
                rowString = locationTable.Get("_id","null");
                rowArray = JsonConvert.DeserializeObject<JArray>(rowString).First() as JObject;
            }
            else
            {
                rowString = locationTable.GetRow(idlocation);
                 rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
            }
           
            
            JObject obj= new JObject();
            rowString=locationProfileTable.GetRow(rowArray["profileId"].ToString());
            
            try {
                obj = JsonConvert.DeserializeObject<JObject>(rowString);
                result = obj["vertodo"].ToString();
            }
            catch (Exception) {
                result = "0";
            }

            return result;
        }

        public JsonResult getDataByObjectReference(String objectReference = "null")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (objectReference == "") objectReference = "null";
                String objectsString = "";
                String categoryString = "";
                JArray objectsObject = new JArray();
                JArray categories = new JArray();

                objectsString = _objectTable.GetObjects(null, objectReference);
                categoryString = categoryTable.GetRows();

                //doing changes to objects array
                objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();

                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }

                foreach (JObject document in objectsObject)
                {
                    if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                        document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);

                    document["nameCreator"] = document["nameCreator"].ToString() + " " + document["lastnameCreator"].ToString();
                    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                    try
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    catch (Exception ex)
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    try
                    {
                        if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                        {
                            document["status"] = "En movimiento";

                        }
                        else
                        {
                            if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                            {
                                document["status"] = "Dado de baja";
                            }
                            else
                            {
                                document["status"] = "Está en tu conjunto";
                            }

                        }

                    }
                    catch
                    {
                        document["status"] = "Está en tu conjunto";
                    }
                    if (document["label"].ToString() == "normal")
                        document["etiquetado"] = "Normal";
                    else
                        document["etiquetado"] = "No Etiquetable";

                    if (document["assetType"].ToString() == "system")
                        document["nameassetType"] = "Sistemas";
                    else if (document["assetType"].ToString() == "maintenance")
                        document["nameassetType"] = "Mantenimiento";
                    else
                    {
                        document["nameassetType"] = "Proyección y Sonido";
                    }

                    if (departs != null)
                    {
                        try
                        {
                            if (departs.ContainsKey(document["department"].ToString()))
                                document["departmentName"] = departs[document["department"].ToString()];
                        }
                        catch
                        {
                            document["departmentName"] = "";
                        }

                    }
                    else
                    {
                        document["departmentName"] = "";
                    }
                    try
                    {
                        if (document["ext"].ToString() != "")
                        {
                            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }
                }
                objectsString = JsonConvert.SerializeObject(objectsObject);
                

                JObject result = new JObject();
                result.Add("objects", objectsString);
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public List<BsonValue> GetObjectsReference(String texto) {
            List<BsonValue> valores = new List<BsonValue>();
            String objstring;
            JArray objs = new JArray();
            objstring = _objectReferenceTable.GetRows();
            objs = JsonConvert.DeserializeObject<JArray>(objstring);
            foreach (JObject ob in objs) {
                JObject obj1 = JsonConvert.DeserializeObject<JObject>(ob["profileFields"].ToString());
                foreach(KeyValuePair<String, JToken> token in obj1){
                    if (token.Value.ToString().Contains(texto)) {
                        valores.Add(ob["_id"].ToString());
                        break;
                    }
                }
            }
            return valores;
        
        }

        public JsonResult getDataByTexto2(String texto = "")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (texto == "") texto = "null";
                String objectsString = "";
                String categoryString = "";
                JArray objectsObject = new JArray();
                List<BsonValue> valores = GetObjectsReference(texto);
                JArray categories = new JArray();

              //  objectsString = _objectTable.GetObjectsByText(texto, valores);
                categoryString = categoryTable.GetRows();
                //doing changes to objects array
                objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();

                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }

                foreach (JObject document in objectsObject)
                {
                    if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                        document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                    document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                    try
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    catch (Exception ex)
                    {
                        document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                    }
                    try
                    {
                        if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                        {
                            document["status"] = "En movimiento";

                        }
                        else
                        {
                            if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                            {
                                document["status"] = "Dado de baja";
                            }
                            else
                            {
                                document["status"] = "Está en tu conjunto";
                            }

                        }

                    }
                    catch
                    {
                        document["status"] = "Está en tu conjunto";
                    }
                    if (document["label"].ToString() == "normal")
                        document["etiquetado"] = "Normal";
                    else
                        document["etiquetado"] = "No Etiquetable";

                    if (document["assetType"].ToString() == "system")
                        document["nameassetType"] = "Sistemas";
                    else if (document["assetType"].ToString() == "maintenance")
                        document["nameassetType"] = "Mantenimiento";
                    else
                    {
                        document["nameassetType"] = "Proyección y Sonido";
                    }

                    if (departs != null)
                    {
                        try
                        {
                            if (departs.ContainsKey(document["department"].ToString()))
                                document["departmentName"] = departs[document["department"].ToString()];
                        }
                        catch
                        {
                            document["departmentName"] = "";
                        }

                    }
                    else
                    {
                        document["departmentName"] = "";
                    }
                    try
                    {
                        if (document["ext"].ToString() != "")
                        {
                            document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                        }
                    }
                    catch (Exception e) { /*Ignored*/ }
                }
                objectsString = JsonConvert.SerializeObject(objectsObject);


                JObject result = new JObject();
                result.Add("objects", objectsString);
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {
                return null;
            }
        }

        public ActionResult getDataByTexto(String texto = "")
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("objects", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                String userstring = userTable.GetRow(Session["_id"].ToString());
                JObject userobj = JsonConvert.DeserializeObject<JObject>(userstring);
                List<String> listlocations= new List<String>();
                int FlagAll = 0;
                foreach (JObject u1 in userobj["userLocations"]) {
                    if (u1["id"].ToString() == "null") {
                        FlagAll = 1; break;
                    }
                    listlocations.Add(u1["id"].ToString());
                }

                if (texto == "") texto = "null";
                String objectsString = "";
                String categoryString = "";
                JArray objectsObject = new JArray();
                List<BsonValue> valores = new List<BsonValue>();//GetObjectsReference(texto);
                JArray categories = new JArray();
                List<string> locationsids = new List<string>();
                List<string> usersids = new List<string>();
                List<string> assetTypenames = new List<string>();
                List<string> refobjsids = new List<string>();
                //try
                //{
                //    JArray locs = JsonConvert.DeserializeObject<JArray>(locationTable.GetLocationsByText(texto));
                //    locationsids = (from loc in locs select (string)loc["_id"]).ToList();
                //    RivkaAreas.Reports.Models.ObjectsRealReport objdb= new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                //    JArray sublocations=JsonConvert.DeserializeObject<JArray>(objdb.GetbyCustom("parent",locationsids,"Locations"));
                //    List<String> sublocationsids = (from loc in sublocations select (string)loc["_id"]).ToList();
                // locationsids.AddRange(sublocationsids);
                //}
                //catch
                //{ }

                if (FlagAll == 0) {
                    try
                    {
                        RivkaAreas.Reports.Models.ObjectsRealReport objdb = new RivkaAreas.Reports.Models.ObjectsRealReport("ObjectReal");
                        JArray sublocations=new JArray();
                        JArray sub1 = new JArray();
                        List<String> l1 = listlocations;
                        do
                        {
                            sublocations = JsonConvert.DeserializeObject<JArray>(objdb.GetbyCustom("parent", listlocations, "Locations"));
                            foreach (JObject loc1 in sublocations)
                            {
                                sub1.Add(loc1);
                            }
                            listlocations = (from loc in sublocations select (string)loc["_id"]).ToList();
                        } while (sublocations.Count() > 0);
                        
                     //   JArray sub1 = JsonConvert.DeserializeObject<JArray>(objdb.GetbyCustom("parent", l1, "Locations"));
                        //foreach(JObject loc1 in sub1){
                        //    sublocations.Add(loc1);
                        //}

                        List<String> sublocationsids = (from loc in sub1 select (string)loc["_id"]).ToList();
                        locationsids.AddRange(sublocationsids);
                    }
                    catch
                    { }
                }
                JArray refobj = new JArray();
                try
                {
                    refobj = JsonConvert.DeserializeObject<JArray>(_objectTable.GetRefObjByText(texto));
                    refobjsids = (from refe in refobj select (string)refe["_id"]).ToList();
                }
                catch { }
                try
                {
                    JArray users = JsonConvert.DeserializeObject<JArray>(userTable.GetUserByText(texto));
                    usersids = (from userx in users select (string)userx["_id"]).ToList();
                }
                catch { }
                try
                {
                    JArray categor = JsonConvert.DeserializeObject<JArray>(categoryTable.GetCatByText(texto));
                    assetTypenames = (from cate in categor select (string)cate["name"]).ToList();
                    foreach (JObject category in categor)
                    {
                        switch (category["name"].ToString())
                        {
                            case "Sistemas":
                                assetTypenames.Add("system");
                                break;
                            case "Mantenimiento":
                                assetTypenames.Add("maintenance");
                                break;
                            case "Proyección y sonido":
                                assetTypenames.Add("sound");
                                break;

                        }
                        refobj = JsonConvert.DeserializeObject<JArray>(_objectReferenceTable.GetRows()); 
                        refobjsids = refobjsids.Union((from refe in refobj where (string)refe["parentCategory"] == category["_id"].ToString() select (string)refe["_id"]).ToList()).ToList<string>();
                    }
                }
                catch { }
                objectsString = _objectTable.GetObjectsByText(texto, valores, locationsids, usersids, assetTypenames, refobjsids);
                categoryString = categoryTable.GetRows();
                //doing changes to objects array
                objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                try
                {
                    foreach (JObject item in objectsObject)
                    {
                        try
                        {
                            item.Add("nameCreator", item["Creator"].ToString() + " " + item["lastname"].ToString());
                        }
                        catch { }
                    }
                }
                catch { }
                categories = JsonConvert.DeserializeObject<JArray>(categoryString);

                Dictionary<string, string> listCategories = new Dictionary<string, string>();

                foreach (JObject items in categories)
                {
                    listCategories.Add(items["_id"].ToString(), items["name"].ToString());
                }
                numActivos = 0;
                numBaja = 0;
                numMov = 0; 
                foreach (JObject document in objectsObject)
                {
                    JToken tk;
                    try
                    {
                        if (listCategories.ContainsKey(document["parentCategory"].ToString()))
                            document.Add("nameCategory", listCategories[document["parentCategory"].ToString()]);
                        document["currentmove"] = _objectTable.GetdemandFolio(document["_id"].ToString());
                        try
                        {
                            document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                        }
                        catch (Exception ex)
                        {
                            document["allmoves"] = _objectTable.GetAlldemandsFolio(document["_id"].ToString());
                        }
                        try
                        {
                            if (document["currentmove"].ToString() != " " && document["currentmove"].ToString() != "")
                            {
                                document["status"] = "En movimiento";
                                numMov++;
                            }
                            else
                            {
                                if (document["system_status"].ToString() == "false" || document["system_status"].ToString() == "False")
                                {
                                    document["status"] = "Dado de baja"; numBaja++;
                                }
                                else
                                {
                                    document["status"] = "Está en tu conjunto"; numActivos++;
                                }

                            }

                        }
                        catch
                        {
                            document["status"] = "Está en tu conjunto"; numActivos++;
                        }

                        if (!document.TryGetValue("status", out tk))
                        {
                            document.Add("status", "");
                        }
                        if (!document.TryGetValue("label", out tk))
                        {
                            document.Add("label", "normal");
                        }
                        if (!document.TryGetValue("etiquetado", out tk))
                        {
                            document.Add("etiquetado", "");
                        }
                        if (!document.TryGetValue("assetType", out tk))
                        {
                            document.Add("assetType", "");
                        }
                        if (!document.TryGetValue("nameassetType", out tk))
                        {
                            document.Add("nameassetType", "");
                        }
                        if (!document.TryGetValue("departmentName", out tk))
                        {
                            document.Add("departmentName", "");
                        }
                        if (!document.TryGetValue("department", out tk))
                        {
                            document.Add("department", "");
                        }

                        if (document["status"].ToString() == "") document["status"] = "Está en tu conjunto";

                        if (document["label"].ToString() == "normal")
                            document["etiquetado"] = "Normal";
                        else
                            document["etiquetado"] = "No Etiquetable";

                        if (document["assetType"].ToString() == "system")
                            document["nameassetType"] = "Sistemas";
                        else if (document["assetType"].ToString() == "maintenance")
                            document["nameassetType"] = "Mantenimiento";
                        else
                        {
                            document["nameassetType"] = "Proyección y Sonido";
                        }

                        if (departs != null)
                        {
                            try
                            {
                                if (departs.ContainsKey(document["department"].ToString()))
                                    document["departmentName"] = departs[document["department"].ToString()];
                            }
                            catch
                            {
                                document["departmentName"] = "";
                            }

                        }
                        else
                        {
                            document["departmentName"] = "";
                        }
                        try
                        {
                            if (document["ext"].ToString() != "")
                            {
                                document.Add("image", "/Uploads/Images/ObjectReferences/" + document["objectReference"].ToString() + "." + document["ext"].ToString());
                            }
                        }
                        catch (Exception e) { /*Ignored*/ }
                    }
                    catch
                    {

                    }
                }
               
                objectsString = JsonConvert.SerializeObject(objectsObject);
                totalglobal = objectsObject.Count();

                JObject result = new JObject();
                result.Add("objects", objectsString);
                result.Add("total", totalglobal);
                result.Add("activos", numActivos);
                result.Add("dadosbaja", numBaja);
                result.Add("enMov", numMov);
                ViewData["resultjson"] = JsonConvert.SerializeObject(result);
              //  return Json(JsonConvert.SerializeObject(result));
             return   View("search");
            }
            else
            {
                return null;
            }
            return null;
        }
        public JsonResult getRoute(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                String actualCategory = locationTable.GetRow(parentCategory);
                JObject actualCatObject = JsonConvert.DeserializeObject<JObject>(actualCategory);

                JObject categoryObject = new JObject();
                categoryObject.Add("id", actualCatObject["_id"].ToString());
                route.Add(categoryObject);
                parentCategory = actualCatObject["parent"].ToString();
            }

            JObject result = new JObject();
            result.Add("route", route);
            return Json(JsonConvert.SerializeObject(result));
        }


        public String updateLocation(string id, string newlocation)
        {
            if (newlocation == "") newlocation = "null";
            if (this.Request.IsAjaxRequest())
            {
                if (id != "")
                {
                    String obj = _objectTable.GetRow(id);
                    var newobj = JsonConvert.DeserializeObject<JObject>(obj);

                    newobj["location"] = newlocation;
                    _objectTable.SaveRow(JsonConvert.SerializeObject(newobj), id);
                    _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + id, "ObjectReal", DateTime.Now.ToString());
                    return "success";
                }

            }

            return null;
        }

        public string loadLocations(string conjunto)
        {

            try
            {
                String locationsOptions = "";
                String rowArray = locationTable.Get("parent", conjunto);
                JArray locatList = JsonConvert.DeserializeObject<JArray>(rowArray);

                locationsOptions += "<option value='null' selected> Seleccione Ubicacion</option>";

                foreach (JObject document in locatList) //for each profile we create an option element with id as value and the name as the text
                {
                    if (document["name"].ToString() != "")
                    {
                        locationsOptions += "<option value='" + document["_id"] + "'"; //setting the id as the value
                        locationsOptions += ">" + document["name"].ToString() + "</option>"; //setting the text as the name
                    }

                }


                return locationsOptions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public String getRoute2(String parentCategory = "null")
        {
            //Creating the route data
            String route = "";
            List<string> rutas = new List<string>();
            while (parentCategory != "null" && parentCategory != "")
            {

                string category = locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                rutas.Add(actualCategory["name"].ToString());
                parentCategory = actualCategory["parent"].ToString();
            }

            for (int i = rutas.Count; i > 0; i--) {
                route = route + rutas[i-1] + "/";
            }

                return route;
        }

        public JArray getRoute3(String parentCategory = "null")
        {
            //Creating the route data
            JArray route = new JArray();

            while (parentCategory != "null" && parentCategory != "")
            {

                string category = locationTable.GetRow(parentCategory);
                JObject actualCategory = JsonConvert.DeserializeObject<JObject>(category);

                route.Add(actualCategory["_id"].ToString());
                parentCategory = actualCategory["parent"].ToString();
            }

            return route;
        }


        public String ValidateLocationUser(String locationid, String userid) {
            String cadena = "";
            String userarray = userTable.GetRow(userid);
            JObject userobj = JsonConvert.DeserializeObject<JObject>(userarray);
            JArray ele = new JArray();
            List<String> list3 = new List<String>();
            List<String> list2 = new List<String>();

            ele = getRoute3(locationid);
            foreach (String ob in ele)
            {
                list3.Add(ob);
            }

            JObject positionUSer = JsonConvert.DeserializeObject<JObject>(_userprofileTable.GetRow(userobj["profileId"].ToString()));

            if (positionUSer["name"].ToString() == "Gerente de conjunto" || positionUSer["name"].ToString() == "Gerente regional")

            {
                JArray locats = JsonConvert.DeserializeObject<JArray>(userobj["userLocations"].ToString());
                foreach (JObject l in locats)
                    list2.Add(l["id"].ToString());
                      /* END: Patch*/
                if (list2.Intersect<string>(list3).ToList<string>().Count > 0)
                {
                    cadena = "1";                
                }
            }
            return cadena;
        }

        public void loadDepartments()
        {
            try
            {
                String DepartmentsOptions = "";
                String rowArray = _listTable.Get("name", "departments");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["unorder"].ToString());
                }
                DepartmentsOptions += "<option value='null' selected> Seleccione Departamento</option>";
                foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        DepartmentsOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        DepartmentsOptions += ">" + token.Value + "</option>"; //setting the text as the name
                        departs.Add(token.Key, token.Value.ToString());
                    }

                }
                ViewData["departList"] = new HtmlString(DepartmentsOptions);
            }
            catch (Exception e)
            {
                ViewData["departList"] = null;
            }
        }

        public void loadproveedores()
        {
            try
            {
                String DepartmentsOptions = "";
                String rowArray = _listTable.Get("name", "proveedores");
                JArray rowString = JsonConvert.DeserializeObject<JArray>(rowArray);
                JArray listas = new JArray();
                foreach (JObject obj in rowString)
                {
                    listas = JsonConvert.DeserializeObject<JArray>(obj["elements"]["order"].ToString());
                }
                DepartmentsOptions += "<option value='null' selected> Seleccione Proveedor</option>";
                foreach (JObject puesto in listas)
                {
                    foreach (KeyValuePair<string, JToken> token in puesto)
                    {
                        if (token.Key == "position") continue;
                        DepartmentsOptions += "<option value='" + token.Key + "'"; //setting the id as the value
                        DepartmentsOptions += ">" + token.Value + "</option>"; //setting the text as the name
                    }

                }

                ViewData["proveedorList"] = new HtmlString(DepartmentsOptions);
            }
            catch (Exception e)
            {
                ViewData["proveedorList"] = null;
            }
        }

        public String saveObject(string  objeto)
        {
            //if (id == "null" || id == "") //differents ways to receive null from javascript
            //{
            //    id = null;
            //}

            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("objects", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("objects", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                JObject datos = JsonConvert.DeserializeObject<JObject>(objeto);
                String objectID = (datos["objectID"].ToString() == "null" || datos["objectID"].ToString() == "") ? null : datos["objectID"].ToString(); //is this an insert or an update?, converting null in javascript to null in c#
                JObject newObject = new JObject();
                String obj = _objectTable.GetRow(objectID); 
                /*the gived id does not exists*/
                if (objectID != null && (objectID == null))
                {
                    return "El id especificado no existe";
                }
                if (obj != null) {
                    newObject = JsonConvert.DeserializeObject<JObject>(obj);
                }

                newObject["name"] = datos["name"].ToString();
              //  newObject["location"] = datos["idlocation"].ToString();
                newObject["price"] = datos["precio"].ToString();
                newObject["date"] = datos["fecha"].ToString();
                newObject["department"] = datos["departamento"].ToString();
                newObject["marca"] = datos["marca"].ToString();
                newObject["modelo"] = datos["modelo"].ToString();
                newObject["perfil"] = datos["perfil"].ToString();
                newObject["object_id"] = datos["object_id"].ToString();

                newObject["folio"] = datos["folio"].ToString();
                newObject["proveedor"] = datos["proveedor"].ToString();
                newObject["num_pedido"] = datos["pedido"].ToString();
                newObject["num_solicitud"] = datos["solicitud"].ToString();
                newObject["num_reception"] = datos["recepcion"].ToString();
                newObject["num_ERP"] = datos["erp"].ToString();
                newObject["serie"] = datos["serie"].ToString();
                newObject["factura"] = datos["factura"].ToString();
                newObject["RH"] = datos["RH"].ToString();


                //JObject profileFields = new JObject();

                ////foreach element in the formData, let's append it to the jsonData in the profileFields
                //foreach (String key in data.Keys)
                //{
                //    profileFields.Add(key, data[key]);
                //}
                //objectArray.Add("profileFields", profileFields);
                try
                {
                    newObject["assetType"] =datos["assetType"].ToString();
                }
                catch (Exception e) { }

                string id = _objectTable.SaveRow(JsonConvert.SerializeObject(newObject), objectID);
                _logTable.SaveLog(Session["_id"].ToString(), "Control de Activos", "Update: Activo _id:" + id, "ObjectReal", DateTime.Now.ToString());
                return id;
            }
            else { return null; }
        }

    }
}
