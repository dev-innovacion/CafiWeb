using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RivkaAreas.User.Models;

using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Security;

using Rivka.Form.Field;
using Rivka.Security;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data;
using System.Text;
using System.IO.Compression;
using System.Globalization;
namespace RivkaAreas.User.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        protected ProfileTable _profileTable;
        protected UserTable _userTable;
        //protected CommonFunctions _common = new CommonFunctions();
        protected validatePermissions validatepermissions;
        protected RivkaAreas.LogBook.Controllers.LogBookController _logTable;

        public UserProfileController()
        {
            this._profileTable = new ProfileTable();
            this._userTable = new UserTable();
            validatepermissions = new validatePermissions();
            _logTable = new LogBook.Controllers.LogBookController();
        }

        //
        // GET: /UserProfile/

        public ActionResult Index()
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
            
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                //TODO: advanced search using Agregation.
                var profiles = _profileTable.getRows();
                ViewBag.userTable = _userTable;

                return View(profiles);
            }
            else
            {

                return Redirect("~/Home");
            }
        }
        public static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, Int32 id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }
        /// <summary>
        /// This method get the content of the Excel file
        /// </summary>
        /// <param name="file">Excel File</param>
        /// <returns>Table of the content of the Excel file</returns>
        /// <author>Edwin (Origin) - Abigail Rodriguez(Edit)</author>
        public ActionResult ImpExcel(HttpPostedFileBase file)
        {

            Dictionary<string, int> orderCell = new Dictionary<string, int>();
            string[] arrayalf = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            for (int i = 0; i < arrayalf.Length; i++)
            {
                orderCell.Add(arrayalf[i], i);
            }
            DataSet ds = new DataSet();

            List<List<string>> tr = new List<List<string>>();
            try
            {
                if (Request.Files["file"].ContentLength > 0/* && (idcategory != "0" && idcategory != null)*/)
                {
                    //ViewData["idcategory"] = idcategory;
                    string fileExtension =
                                         System.IO.Path.GetExtension(Request.Files["file"].FileName);

                    if (fileExtension == ".xls" || fileExtension == ".xlsx")
                    {
                        string fileLocation = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                        if (System.IO.File.Exists(fileLocation))
                        {

                            System.IO.File.Delete(fileLocation);
                        }
                        Request.Files["file"].SaveAs(fileLocation);
                    }

                    string fileLocation2 = Server.MapPath("~/Content/") + Request.Files["file"].FileName;
                    if (System.IO.File.Exists(fileLocation2))
                    {

                        System.IO.File.Delete(fileLocation2);
                    }
                    Request.Files["file"].SaveAs(fileLocation2);

                    using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileLocation2, false))
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>(); ;
                        foreach (Row r in sheetData.Elements<Row>())
                        {
                            List<string> td = new List<string>();
                            int index = 0;
                            foreach (Cell c in r.Elements<Cell>())
                            {

                                string cellIndex = c.CellReference.ToString().Substring(0, 1);
                                bool validate = false;
                                int numcellx = 0;
                                foreach (var x in orderCell)
                                {
                                    if (x.Key == cellIndex)
                                    {
                                        numcellx = x.Value;
                                    }
                                    if (x.Key == cellIndex && x.Value == index)
                                    {
                                        validate = true;
                                        break;
                                    }
                                }

                                if (validate == false)
                                {
                                    numcellx = numcellx - index;
                                    for (int i = 0; i < numcellx; i++)
                                    {
                                        td.Add("");
                                    }
                                    index = index + numcellx;

                                }
                                Int32 id = -1;


                                if (c.DataType != null && c.DataType.Value == CellValues.SharedString)
                                {
                                    if (Int32.TryParse(c.InnerText, out id))
                                    {
                                        SharedStringItem item = GetSharedStringItemById(workbookPart, id);
                                        if (item.Text != null)
                                        {
                                            td.Add(item.Text.Text);
                                        }
                                        else if (item.InnerText != null)
                                        {
                                            td.Add(item.InnerText);
                                        }
                                        else if (item.InnerXml != null)
                                        {
                                            td.Add(item.InnerXml);
                                        }
                                    }
                                    else
                                    {
                                        td.Add(c.CellValue.Text);
                                    }
                                }
                                else
                                {

                                    try
                                    {
                                        td.Add(c.CellValue.Text);
                                    }
                                    catch (Exception ex)
                                    {

                                        td.Add("");
                                    }

                                }
                                index++;
                            }
                            tr.Add(td);
                        }
                        spreadsheetDocument.Close();

                    }



                    /*string rowsArray = categoryTable.getRow(idcategory);
                    //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                    JObject categoryJarray = JsonConvert.DeserializeObject<JObject>(rowsArray);*/
                    List<List<string>> data = new List<List<string>>();
                    /* string idfield = "";
                     foreach (JObject items in categoryJarray["customFields"])
                     {
                         foreach (var customf in items["fields"])
                         {
                             List<string> rows = new List<string>();
                             idfield = objectsfields.GetRow(customf["fieldID"].ToString());
                             if (idfield != null)
                             {
                                 try
                                 {
                                     JObject fieldata = JsonConvert.DeserializeObject<JObject>(idfield);

                                     rows.Add(fieldata["_id"].ToString());
                                     rows.Add(fieldata["name"].ToString());
                                     rows.Add(customf["required"].ToString());

                                     data.Add(rows);
                                 }
                                 catch (Exception e)
                                 {

                                 }
                             }
                         }
                     }*/
                    ViewData["categoriedata"] = data;

                    ViewData["Filelocation"] = fileLocation2;
                    ViewData["Filerequest"] = file;
                    return View(tr);
                }
                else { return null; }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                // System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                //System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
                return null;
            }
        }
        public string getModulename(string mod)
        {

            Dictionary<string, string> modules = new Dictionary<string, string>();
            modules.Add("1", "users");
            modules.Add("2", "profiles");
            modules.Add("3", "custom_fields");
            modules.Add("4", "objects");
            modules.Add("5", "objectsreference");
            modules.Add("6", "demand");
            modules.Add("7", "hardware");
            modules.Add("8", "location");
            modules.Add("9", "processes");
            modules.Add("10", "circuits");
            modules.Add("11", "movement");
            modules.Add("12", "rules");
            modules.Add("13", "inventory");
            modules.Add("14", "lists");
            modules.Add("15", "messages");
            modules.Add("16", "tickets");
            modules.Add("17", "designs");
            modules.Add("18", "edgeware");
            modules.Add("19", "reports");
            modules.Add("20", "selectreports");
            modules.Add("21", "conexionext");
            modules.Add("22", "support");
            modules.Add("23", "help");
            string modulename = "";
            if (modules.TryGetValue(mod, out modulename))
                return modulename;
            else
                return "";

        }
        public JObject getPermission(String data, JObject generalPermissions)
        {
            try
            {

                JObject grant = new JObject();
                JArray grantarray = new JArray();
                string module = getModulename(data.Split('.')[0]);
                if (module.Length == 0)
                    return generalPermissions;

                List<string> access = data.Split('.')[1].Split(',').ToList();
                foreach (string p in access)
                {
                    grantarray.Add(p);
                }
                grant.Add("grant", grantarray);
                try
                {
                    generalPermissions.Add(module, grant);
                }
                catch { }
                return generalPermissions;
            }
            catch { return generalPermissions; }
        }
        public String saveImport(String data)
        {
            try
            {

               

                String dataimport = data.ToString();
                JArray dataimportarray = JsonConvert.DeserializeObject<JArray>(dataimport);
                int count = 0; int totalAdd = 0; int totalFail = 0; int totalalready = 0;
                JArray result = new JArray();
               
                foreach (JObject items in dataimportarray)
                {
                    count++; bool error = false; string getboss = "";
                   
                    string limit = "true";
                   //Section to get profile ID
                    string profileID = "";

                    JToken tk;
                    if (!items.TryGetValue("name", out tk))
                    {
                        error = true;
                    }
                    if (!items.TryGetValue("permissionsHTK", out tk))
                    {
                        error = true;
                    }
                    if (error) { totalFail++; continue; }
                    try{
                        JObject getperfil = JsonConvert.DeserializeObject<JArray>( _profileTable.Get("name",items["name"].ToString().ToLower().Trim())).First() as JObject;
                        totalalready++;
                        continue;
                    }catch{ 

                    }
                    //permissions
                    JObject permissions = new JObject();
                    try
                    {
                        List<string> listpermissions = items["permissionsHTK"].ToString().Split('|').ToList();

                        foreach (string permission in listpermissions)
                        {
                            try
                            {
                                permissions = getPermission(permission, permissions);
                            }
                            catch { }
                        }
                    }
                    catch { }
                    
                    //Change name representation
                    items["name"] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(items["name"].ToString().ToLower().Trim());
                    JObject datajo = new JObject();
                    datajo.Add("name", items["name"].ToString());
                    datajo.Add("permissionsHTK",permissions);
                    datajo.Add("customFields", new JArray());
                    //there are fields that we know that exists so we set them into the json
                  /*  String jsonData = "{'user':'" + items["user"].ToString() + "','pwd':'" + items["pwd"].ToString() + "','imgext':'"
                       + "','email':'" + userMail + "','name':'" + items["name"].ToString().Replace("+", " ")
                       + "','lastname':'" + items["lastname"].ToString().Replace("+", " ") + "', 'profileId':'" + profileID
                       + "', 'boss':'" + items["boss"].ToString() + "','userKey':'', 'userLocations':" + JsonConvert.SerializeObject(locationArray) + ", 'extra_emails':" + JsonConvert.SerializeObject(extraemailsArray) + ", 'permissionsHTK':" + JsonConvert.SerializeObject(permissions)
                       + ",'areaSelect':'" + items["areaSelect"].ToString() + "','departmentSelect':'" + items["departmentSelect"].ToString() + "','descripcionpuesto':'" + items["descripcionpuesto"].ToString() + "','alias':'" + items["alias"].ToString() + "'";
                    */


                    string id = _profileTable.SaveRow(JsonConvert.SerializeObject(datajo), null); //Save new profile
                    _logTable.SaveLog(Session["_id"].ToString(), "Perfiles", "Insert: " + items["name"].ToString(), "Profiles", DateTime.Now.ToString());
                   
                  

                    result.Add("{\"success\":\"Perfil guardado en registo: " + count + "\"}");
                    totalAdd++;

                }
               
                //Save imagen files
               
                //Get data for new users
              
                JObject finalResult = new JObject();
                finalResult.Add("perfilSuccess", totalAdd.ToString());
                finalResult.Add("perfilError", totalFail.ToString());
                finalResult.Add("details", JsonConvert.SerializeObject(result));
                finalResult.Add("perfilAlready", totalalready.ToString());

                return JsonConvert.SerializeObject(finalResult);
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        /// <summary>
        /// Gets all the profiles as a JsonResult
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProfilesTable()
        {
            if (Request.IsAjaxRequest())
            {
                string raw_profiles = _profileTable.getJoinRows();
               JArray profiles = JsonConvert.DeserializeObject<JArray>(raw_profiles);

                foreach (JObject profile in profiles)
                {
                    profile.Remove("customFields");
                    profile.Remove("permissionsHTK");
                    //Count the number of users with the specified profileId
                    profile["numUsers"] = JsonConvert.DeserializeObject<JArray>(_userTable.Get("profileId", profile["_id"].ToString())).Count;
                    
                }

                return Json(JsonConvert.SerializeObject(profiles));
            }            

            return Json("[]");
        }

        /// <summary>
        ///     Insert a new profile, if is set the idProfile, make an update of that idProfile
        /// </summary>
        /// <param name="idProfile">
        ///     Id of the profile to update.
        /// </param>
        /// <returns>
        ///     Returns the view to create a new profile
        /// </returns>
        public ActionResult newProfile(string idProfile = null)
        {


           
            bool edit = false;
            bool editclient=false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;
          //  access = getpermissions("users", "r");
            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            edit = validatepermissions.getpermissions("profiles", "u", dataPermissions);

            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);
            editclient = validatepermissions.getpermissions("profiles", "u", dataPermissionsClient);
    
            
           
            if (Profile != null && (edit == false || editclient==false))
            {
                access = false;
                accessClient = false;
            }

            if (access == true && accessClient==true)
            {
                CustomFieldsTable cft = new CustomFieldsTable("CustomFields");
                String fieldsArray = cft.GetRows();
                JArray fields = JsonConvert.DeserializeObject<JArray>(fieldsArray);

                if (idProfile != null && idProfile != "null" && idProfile != "")
                {
                    BsonDocument profile = _profileTable.getRow(idProfile);
                    if (profile != null)
                    {
                        profile.Set("_id", profile.GetElement("_id").Value.ToString());
                        try
                        {
                            profile.Set("CreatedTimeStamp", profile.GetElement("CreatedTimeStamp").Value.ToString());
                        }
                        catch (Exception ex)
                        {

                        }
                        string profileJson = profile.ToJson();
                        ViewData["profile"] = new HtmlString(profileJson);
                    }
                }

                List<BsonDocument> profiles = _profileTable.getRows();
                ViewBag.profiles = profiles;

                return View(fields);
            }
            else
            {
                return Redirect("~/Home");
            }
        }

        /// <summary>
        ///     Deletes a profile and changes the user's idProfile to the specified
        ///     in the users param, by default all users will be changed to a "basic" profile
        /// </summary>
        /// <param name="idProfile">
        ///     A string with de idProfile to delete
        /// </param>
        /// <param name="users">
        ///     A json string with an array of users to change its idProfile
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public void deleteProfile(string idProfile, string users)
        {
            bool access = false;
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
           
            bool accessClient = false;

            access = validatepermissions.getpermissions("profiles", "d", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "d", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                var userList = JsonConvert.DeserializeObject<UserList>(users);//User Class already exists its used in Security Auth

                if (idProfile != "null" && idProfile != null && idProfile != "")
                {
                    BsonDocument profile = _profileTable.getRow(idProfile);

                    if (profile != null)
                    {
                        foreach (var user in userList.users)
                        {
                            _userTable.updateProfile(user);
                        }
                        _profileTable.deleteProfile(idProfile);
                    }
                }
            }
        }


        /// <summary>
        ///     Get a profile specified by the idProfile
        /// </summary>
        /// <param name="idProfile"></param>
        /// <returns>
        ///     Returns a Json text with the found profile
        /// </returns>
        [HttpPost]
        public string getProfile(string idProfile)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("profiles", "r", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "r", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                if (idProfile != null && idProfile != "null" && idProfile != "")
                {
                    BsonDocument profiles = _profileTable.getRow(idProfile);
                    if (profiles != null)
                    {
                        profiles.Set("_id", profiles.GetElement("_id").Value.ToString());
                        string profileJson = profiles.ToJson();
                        return profileJson;
                    }
                }
                return "";
            }
            else { return ""; }
        }

        [HttpPost]
        public ActionResult getUserByProfile(string idProfile)
        {
            List<BsonDocument> users = _userTable.get("profileId", idProfile);
            List<BsonDocument> profiles = _profileTable.getRows();
            ViewBag.canDelete = true;

            try
            {
                if (_profileTable.getRow(idProfile).GetElement("name").Value.ToString() == "Básico")
                    ViewBag.canDelete = false;
            }
            catch (Exception e)
            {
                ViewBag.canDelete = false;
            }


            ViewBag.profiles = profiles;
            ViewBag.idProfile = idProfile;

            return View(users);
        }

        /// <summary>
        ///     Saves a new profile, if idProfile is not null, then edits that one.
        /// </summary>
        /// <param name="newProfile"></param>
        /// <param name="idProfile"></param>
        /// <returns>
        ///     
        /// </returns>
        public string saveProfile(string newProfile, string idProfile = null)
        {
            String dataPermissions = Session["Permissions"].ToString();
            String dataPermissionsClient = Session["PermissionsClient"].ToString();
            bool access = false;
            bool accessClient = false;

            access = validatepermissions.getpermissions("profiles", "u", dataPermissions);
            accessClient = validatepermissions.getpermissions("profiles", "u", dataPermissionsClient);

            if (access == true && accessClient == true)
            {
                idProfile = (_profileTable.getRow(idProfile) == null) ? null : idProfile;
                idProfile = _profileTable.SaveRow(newProfile, idProfile);
                try {
                    _logTable.SaveLog(Session["_id"].ToString(), "Perfiles de Usuario", "Insert/Update: se ha guardado un perfil de usuario", "Profiles", DateTime.Now.ToString());
                }
                catch (Exception ex) { }
                return idProfile;
            }
            else { return null; }
        }

        public bool getpermissions(string permission, string type)
        {
            var datos = "";
            if (Session["Permissions"] != null)
            {
                datos = Session["Permissions"].ToString();
                JObject allp2 = JsonConvert.DeserializeObject<JObject>(datos);

                if (allp2[permission]["grant"].Count() > 0)
                {
                    foreach (string x in allp2[permission]["grant"])
                    {
                        if (x.Contains(type))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            else
            {

                this.Session["LoggedUser"] = null;
                this.Session["_id"] = null;

                //Unset auth cookie
                FormsAuthentication.SignOut();


                //Redirect to index
                Response.Redirect("~/User/Login");
                return false;
            }



        }

    }
}
