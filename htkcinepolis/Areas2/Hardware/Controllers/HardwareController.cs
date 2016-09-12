using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RivkaAreas.Hardware.Models;
using Rivka.Db;
using Rivka.Form;
using Rivka.Form.Field;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MongoDB.Bson;

namespace RivkaAreas.Hardware.Controllers
{
    public class HardwareController : Controller
    {
        //
        // GET: /Hardware/Hardware/
        protected HardwareTable _hardwareTable;
        protected HardwareReferenceTable _hardwareReferenceTable;
        protected HardwareCategory _hardwareCategoryTable;
        protected UserTable _userTable;

        public HardwareController()
        {
            _hardwareTable = new HardwareTable();
            _hardwareReferenceTable = new HardwareReferenceTable();
            _hardwareCategoryTable = new HardwareCategory();
            _userTable = new UserTable();
        }
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult getNodeContent2(String id)
        {
            if (id == "") id = "null";
            String categoriesString = _hardwareCategoryTable.Get("parentCategory", id); 
            
            if (categoriesString == null) return null; //there are no subcategories

            JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categoriesString);
          //  JArray objectObject = JsonConvert.DeserializeObject<JArray>(objectsString);

            //categoriesObject.Add(objectObject);

            JArray newobjs = new JArray();

            foreach (JObject obj in categoriesObject)
            {
                JObject obj1 = new JObject();
                obj1["id"] = obj["_id"];
                obj1["text"] = obj["name"];
                obj1["hasChildren"] = true;
                // obj1["items"] = new JArray();
                //  obj1["items"] = "[]";
                obj1["spriteCssClass"] = "folder";
                newobjs.Add(obj1);
            }


            return Json(JsonConvert.SerializeObject(newobjs), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getData(String parentCategory = "null")
        {
            bool access = getpermissions("objects", "r");
            if (access == true)
            {
                string idcatthis = parentCategory;

                if (parentCategory == "") parentCategory = "null";
                String categories = _hardwareCategoryTable.Get("parentCategory", parentCategory);
                String objectsString = _hardwareReferenceTable.Get("parentCategory", parentCategory); 

                //removing categories customFields
                if (categories!=null)
                {
                    JArray categoriesObject = JsonConvert.DeserializeObject<JArray>(categories);
                    foreach (JObject document in categoriesObject)
                    {
                        document.Remove("customFields");
                        try
                        { //trying to set the creator's name
                            String rowString = _userTable.GetRow(document["Creator"].ToString());
                            JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                            document["Creator"] = rowArray["name"];
                        }
                        catch (Exception e)
                        {
                            document["Creator"] = "";
                        }

                        try
                        {
                            if (document["ext"].ToString() != "")
                            {
                                document.Add("image", "/Uploads/Images/Hardware/" + document["_id"].ToString() + "." + document["ext"].ToString());
                            }
                            document.Remove("ext");
                        }
                        catch (Exception e) { /*Ignored*/ }

                        try //trying to set category's objects number
                        {
                            String objects = _hardwareReferenceTable.Get("parentCategory", document["_id"].ToString());
                            JArray childObject = JsonConvert.DeserializeObject<JArray>(objects);
                            document["objectCount"] = childObject.Count;
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    categories = JsonConvert.SerializeObject(categoriesObject);
                }

                if (objectsString != null) {
                    //doing changes to objects array
                    JArray objectsObject = JsonConvert.DeserializeObject<JArray>(objectsString);
                    String harwarestring = _hardwareTable.GetRows();
                    JArray hardwareobjects = JsonConvert.DeserializeObject<JArray>(harwarestring);
                    JArray hardobjs = new JArray();
                    foreach (JObject document in hardwareobjects)
                    {
                        int flag = 0;
                        JObject reference = new JObject();
                        foreach (JObject obj in objectsObject)
                        {
                            if (obj["_id"].ToString() == document["hardware_reference"].ToString())
                            {
                                flag = 1;
                                reference = obj;
                            }
                        }

                        if (flag == 1)
                        {
                            try
                            { //trying to set the creator's name
                                String rowString = _userTable.GetRow(reference["Creator"].ToString());
                                JObject rowArray = JsonConvert.DeserializeObject<JObject>(rowString);
                                document["Creator"] = rowArray["name"];
                            }
                            catch (Exception e)
                            {
                                document["Creator"] = "";
                            }

                            try
                            {
                                if (reference["ext"].ToString() != "")
                                    document.Add("image", "/Uploads/Images/Hardware/" + reference["_id"].ToString() + "." + reference["ext"].ToString());
                                document.Remove("ext");
                            }
                            catch (Exception e) { /*Ignored*/ }

                            //JObject profileFields = (JObject)document["profileFields"];
                            //JObject profileCopy = new JObject();
                            //foreach (string key in requiredValues)
                            //{

                            //    if (profileFields[key] == null)
                            //    {
                            //        profileCopy.Add(key, "");
                            //    }
                            //    else
                            //    {
                            //        profileCopy.Add(key, profileFields[key]);
                            //    }
                            //}
                            //document["profileFields"] = profileCopy;
                            document["name"] = reference["name"].ToString();
                            hardobjs.Add(document);

                        }

                    }
                    objectsString = JsonConvert.SerializeObject(hardobjs);
                }
                

                //Creating the route data
                JArray route = new JArray();
                while (parentCategory != "null" && categories!=null)
                {

                    String actualCategory = _hardwareCategoryTable.GetRow(parentCategory); 
                    if (actualCategory == null)
                    {
                        this.Response.StatusCode = 500;
                        return null;
                    }
                    JObject actualCatObject = JsonConvert.DeserializeObject<JObject>(actualCategory);
                    JObject categoryObject = new JObject();
                    categoryObject.Add("id", actualCatObject["_id"]);
                    categoryObject.Add("name", actualCatObject["name"]);
                    route.Add(categoryObject);
                    parentCategory = actualCatObject["parentCategory"].ToString();
                }

                JObject result = new JObject();
                result.Add("categories", categories);
                result.Add("objects", objectsString);
                result.Add("route", route);
              //  result.Add("prototype", prototype.ToJson());
                result.Add("idcat", idcatthis);
                return Json(JsonConvert.SerializeObject(result));
            }
            else
            {

                return null;
            }
        }

        public bool getpermissions(string permission, string type)
        {
            var datos = Session["Permissions"].ToString();

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

        public JsonResult getObject(String id)
        {
            string hardwarestring = _hardwareTable.GetRow(id);

            if (hardwarestring == null)
            {
                hardwarestring = _hardwareCategoryTable.GetRow(id); 
                JObject hardwareobject = JsonConvert.DeserializeObject<JObject>(hardwarestring);

                hardwareobject["profileFields"] = hardwareobject["customFields"];
                hardwareobject["idreference"] = hardwareobject["_id"].ToString();
                hardwareobject["parentid"] = hardwareobject["parentCategory"];
                hardwareobject["tipo"] = "category";
                hardwarestring = JsonConvert.SerializeObject(hardwareobject);
            }
            else {
                JObject hardwareobject = JsonConvert.DeserializeObject<JObject>(hardwarestring);

                string referencestring = _hardwareReferenceTable.GetRow(hardwareobject["hardware_reference"].ToString());
                JObject referenceobj = JsonConvert.DeserializeObject<JObject>(referencestring);

                hardwareobject["parentCategory"] = referenceobj["parentCategory"].ToString();
                hardwareobject["name"] = referenceobj["name"].ToString();
                hardwareobject["profileFields"] = referenceobj["profileFields"];
                hardwareobject["ext"] = referenceobj["ext"].ToString();
                hardwareobject["idreference"] = referenceobj["_id"].ToString();
                hardwareobject["parentid"] = hardwareobject["hardware_reference"];
                hardwareobject["tipo"] = "object";
                hardwarestring = JsonConvert.SerializeObject(hardwareobject);
            }
            return Json(hardwarestring); 
        }

        public ActionResult getObjectByCategory(string idCategory)
        {
            //dynamic objects = objectTable.get("categoryId", idCategory);
            //List<BsonDocument> categories = categoryTable.getRows();
            ViewBag.canDelete = true;
            //try
            //{
            //    if (_profileTable.getRow(idProfile).GetElement("name").Value.ToString() == "Básico")
            //        ViewBag.canDelete = false;
            //}
            //catch (Exception e)
            //{
            //    ViewBag.canDelete = false;
            //}
            //ViewBag.profiles = categories;
            ViewBag.idProfile = idCategory;

            return View(/*objects*/);
        }

        public String getObjectForm(String parentCategory)
        {
            String category = _hardwareCategoryTable.GetRow(parentCategory); 
            JObject categoryObject = JsonConvert.DeserializeObject<JObject>(category);
            String body = CustomForm.getFormView(categoryObject["customFields"].ToString(), "HardwareFields");
            String headers = CustomForm.getFormTitlesView(categoryObject["customFields"].ToString());
            JObject Result = new JObject();
            Result.Add("body", body);
            Result.Add("headers", headers);
            return JsonConvert.SerializeObject(Result);
        }

        public String saveObject(FormCollection data, HttpPostedFileBase file, IEnumerable<HttpPostedFileBase> files, String parentCategory = "null", String id = null)
        {
            if (id == "null" || id == "") //differents ways to receive null from javascript
            {
                id = null;
            }

            bool access = getpermissions("objects", "u");
            if (access == true)
            {
                data = CustomForm.unserialize(data);
                JObject objectArray = new JObject();
                try //name must be gived
                {
                    if (data["txtserie"] == null || data["txtserie"] == "")
                    {
                        return null; //name is wrong
                    }
                    objectArray.Add("serie", data["txtserie"].ToString());
                    data.Remove("txtserie");
                }
                catch (Exception e)
                { //what to do if name is wrong
                    return null;
                }

                objectArray.Add("hardware_reference", parentCategory);

                JObject profileFields = new JObject();

                //foreach element in the formData, let's append it to the jsonData in the profileFields
                foreach (String key in data.Keys)
                {
                    profileFields.Add(key, data[key]);
                }
                objectArray.Add("profileFields", profileFields);
                String jsonData = JsonConvert.SerializeObject(objectArray);

                String auxid = id;
                id = _hardwareTable.SaveRow(jsonData, id); 

                return null;
            }
            else { return null; }
        }
    }
}
