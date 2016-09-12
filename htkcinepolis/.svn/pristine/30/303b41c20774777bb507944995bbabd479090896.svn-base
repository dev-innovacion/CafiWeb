using RivkaAreas.User.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RivkaAreas.Design.Controllers;
using Rivka.Files;
using System.IO;
using System.util;
using System.Net;
using Rivka.Security;

namespace RivkaAreas.User.Controllers
{
    public class LoginController : Controller
    {
        // GET: /Login/
        protected DesignController Design = new DesignController();
        protected SystemSettingsTable systemSettingsTable;
        protected UserTable usertable;
        /// <summary>
        /// Login Window Show
        /// </summary>
        /// <returns>Login View</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        /// 
        public LoginController() {
            usertable = new UserTable();
        }

        public ActionResult Index()
        {
            //if (WebSecurity.IsAuthenticated == true)
            //{
                //return Redirect("~/Home");

            //}

            //usertable = new UserTable();
            List<string> backgrounds= Design.getBackgrounds();
            string text = Design.getText();
            string logologin = Design.getLogologin();
            ViewData["text"] = text;
            ViewData["logologin"] = logologin;

            try
            {
               

                if (Request.Cookies["_id2"] != null)
                {
                    string id = Request.Cookies["_id2"].Value;

                    string idusuario = usertable.getRowString(id);
                    if (idusuario != null)
                    {

                        FormsAuthentication.SetAuthCookie("User", false);

                        //Redirect Index
                        return Redirect("~/Home");
                    }
                    else
                    {
                        return View(backgrounds);
                    }
                }
                else
                {
                    return View(backgrounds);

                }

            }
            catch(Exception ex)
            {
                return View(backgrounds);
            }
           
        }


        /// <summary>
        /// Check if username and password is valid, if it is, sets sessions and formauth to true
        /// </summary>
        /// <param name="username">The user name</param>
        /// <param name="password">The User password</param>
        /// <returns>Index View if valid user, Login view if invalid user</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        public ActionResult Login(string username, string password)
        {
            
            if (username == "" || password == "")
            {
                this.Redirect("/Login");
            }
            //Check the user on the database
            //usertable = new UserTable();
            
            BsonDocument doc = usertable.Login(username, password);
            ProfileTable profiletable = new ProfileTable();
            //If the return is null, is an invalid user


            if (doc != null)
            {
                //User Password time validation
                
                if (doc["_id"].AsObjectId.ToString() != "52e95ab907719e0d40637d96")
                {
                    if (username == password)
                    {

                        ViewBag.Message = "Default";
                        List<string> backgrounds = Design.getBackgrounds();
                        return View("Index", backgrounds);

                    }
                    JObject userInformation = JsonConvert.DeserializeObject<JObject>(usertable.GetRow(doc["_id"].AsObjectId.ToString()));
                    if (userInformation["lastChgPassword"] != null)
                    {
                        try
                        {
                            DateTime d1 = DateTime.ParseExact(userInformation["lastChgPassword"].ToString(), "dd/MM/yyyy HH:mm:ss", null);
                            DateTime d2 = DateTime.Now;
                            systemSettingsTable = new SystemSettingsTable();

                            JArray cantDays = JsonConvert.DeserializeObject<JArray>(systemSettingsTable.Get("name", "daysChangePassword"));
                            string days = (from mov in cantDays select (string)mov["days"]).First().ToString();

                            TimeSpan time = d2 - d1;
                            int NrOfDays = time.Days;

                            if (int.Parse(days) <= NrOfDays)
                            {
                                ViewBag.Message = "Timeout";
                                ViewData["timeout"] = "Timeout";

                                List<string> backgrounds = Design.getBackgrounds();
                                return View("Index", backgrounds);
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Error = true;
                            ViewBag.Message = ex.ToString();
                            ViewData["timeout"] = ex.ToString();
                            List<string> backgrounds = Design.getBackgrounds();
                            return View("Index", backgrounds);
                        }
                    }
                    else
                    {
                        try
                        {
                            JObject user = JsonConvert.DeserializeObject<JObject>(usertable.GetRow(doc["_id"].AsObjectId.ToString()));
                            user["lastChgPassword"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            usertable.saveRow(JsonConvert.SerializeObject(user), user["_id"].ToString());

                        }
                        catch (Exception ex)
                        {

                            // throw new Exception(ex.ToString());
                            //return Redirect("~/Home");
                        }
                        //END - User Password time validation
                    }
                }

                DataFileManager Filelimits;
                string filepath = "/App_Data/system.conf";
                string absolutedpath = Server.MapPath(filepath);
                Filelimits = new DataFileManager(absolutedpath, "juanin");

                if (!Filelimits.empty())
                {
                    //Set user name (to show on the upper right corner of the system)
                    this.Session["LoggedUser"] = "";
                   /* try
                    {
                        this.Session["Semaphores"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        HttpCookie aCookiesem = new HttpCookie("semaphores");
                        aCookiesem.Value = Session["Semaphores"].ToString();
                        aCookiesem.Expires = DateTime.Now.AddDays(10);
                        Response.Cookies.Add(aCookiesem);

                    }
                    catch (Exception ex)
                    {

                    }*/
                    this.Session["Username"] = "";
                    try
                    {
                        this.Session["Username"] = doc["name"].AsString;
                        this.Session["LoggedUser"] += doc["name"].AsString;
                        this.Session["User"] = doc["user"].AsString;
                        try
                        {
                            this.Session["LoggedUser"] += " " + doc["lastname"].AsString;

                        }
                        catch (Exception e) { /*Ignored*/ }
                    }
                    catch (Exception e) { /*Ignored*/ }
                    //If needed for user transactions
                    this.Session["_id"] = doc["_id"].AsObjectId;
                    
                    HttpCookie aCookie2 = new HttpCookie("_id2");
                    aCookie2.Value = doc["_id"].AsObjectId.ToString();
                    aCookie2.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(aCookie2);

                    //Check if there exist the image extension registry on database, if it exist, sets the relative
                    //path
                    try
                    {
                        if (!string.IsNullOrEmpty(doc["imgext"].ToString()))
                        {
                            //Relative path to save images
                            string relativepath = string.Format("\\Uploads\\Images\\{0}.{1}",
                                Session["_id"].ToString(), doc["imgext"].ToString());

                            //Check if profile picture file exists on the server
                            if (System.IO.File.Exists(Server.MapPath(relativepath)))
                            {
                                //if it exist, sets the profile picture url
                                this.Session["ProfilePicture"] = Url.Content(relativepath);
                                HttpCookie aCookie4 = new HttpCookie("_picture");
                                aCookie4.Value = Url.Content(relativepath);
                                aCookie4.Expires = DateTime.Now.AddDays(10);
                                Response.Cookies.Add(aCookie4);
                            }
                            else
                            {
                                //Set picture to default
                                this.Session["ProfilePicture"] = null;
                            }
                        }


                    }
                    catch (Exception e) { /*Ignored*/ }

                    string usuarioid = Session["_id"].ToString();
                    String profileid = usertable.getRowString(usuarioid);
                    JObject rowArray = JsonConvert.DeserializeObject<JObject>(profileid);
                    var jdatos = "";

                    if (rowArray["permissionsHTK"] != null)
                    {
                        string arraypermisos = rowArray["permissionsHTK"].ToString();
                        JObject allp = JsonConvert.DeserializeObject<JObject>(arraypermisos);

                        jdatos = JsonConvert.SerializeObject(allp);
                        this.Session["Permissions"] = jdatos.ToString();
                    }
                    else
                    {

                        string idpro = rowArray["profileId"].ToString();
                        String profiles = profiletable.GetRow(idpro);
                        JObject rowArraypro = JsonConvert.DeserializeObject<JObject>(profiles);
                        string arraypermisos = rowArraypro["permissionsHTK"].ToString();
                        JObject allp = JsonConvert.DeserializeObject<JObject>(arraypermisos);

                        jdatos = JsonConvert.SerializeObject(allp);

                        this.Session["Permissions"] = jdatos.ToString();
                    }
                    try
                    {

                        this.Session["PermissionsClient"] = Filelimits["scenario"]["modules"].ToString();
                        string filedata = Filelimits["scenario"]["modules"].ToString();
                        HttpCookie aCookiep = new HttpCookie("permissionsclient");
                        aCookiep.Value = Filelimits["scenario"]["modules"].ToString();
                        aCookiep.Expires = DateTime.Now.AddDays(10);
                        Response.Cookies.Add(aCookiep);
                    }
                    catch (Exception ex)
                    {
                        this.Session["PermissionsClient"] = "";
                    }
                    HttpCookie aCookie = new HttpCookie("permissions");
                    aCookie.Value = jdatos.ToString();
                    aCookie.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(aCookie);

                    HttpCookie aCookie1 = new HttpCookie("_loggeduser");
                    aCookie1.Value = Session["LoggedUser"].ToString();
                    aCookie1.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(aCookie1);

                    HttpCookie aCookie3 = new HttpCookie("_username");
                    aCookie3.Value = Session["Username"].ToString();
                    aCookie3.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(aCookie3);

                    HttpCookie aCookie5 = new HttpCookie("_user");
                    aCookie5.Value = Session["User"].ToString();
                    aCookie5.Expires = DateTime.Now.AddDays(10);
                    Response.Cookies.Add(aCookie5);

                    //Sets the login authorization
                    FormsAuthentication.SetAuthCookie("User", false);

                    //Redirect Index
                    return Redirect("~/Home");
                }
                else
                {
                    ViewBag.Error = true;
                    ViewBag.Message = "Error de Permisos";
                    List<string> backgrounds = Design.getBackgrounds();

                    return View("Index", backgrounds);
                }
            }
            else
            {
                //Set error and return to login page
                ViewBag.Error = true;
                ViewBag.Message = "Error de Login";
                List<string> backgrounds = Design.getBackgrounds();

                return View("Index", backgrounds);
            }
        }

        /// <summary>
        /// Logout the user
        /// </summary>
        /// <returns>Login View</returns>
        /// <author>Galaviz Alejos Luis Angel</author>
        public ActionResult Logout()
        {
            //Unset session variables
            this.Session["LoggedUser"] = null;
            this.Session["_id"] = null;

            //Unset auth cookie
            FormsAuthentication.SignOut();

           HttpCookie aCookie;
            string cookieName;
            int limit = Request.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;
                aCookie = new HttpCookie(cookieName);
                aCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(aCookie);
            }

            //Redirect to index
            return Redirect("~/User/Login");
        }

        public ActionResult ChgPassword(string username, string password, string newpassword)
        {
            BsonDocument doc = usertable.Login(username, password);
            if (doc != null)
            {
                JObject usuario = JsonConvert.DeserializeObject<JObject>(usertable.GetRow(doc["_id"].AsObjectId.ToString()));

                try
                {
                    foreach (JObject o1 in usuario["pwd_old"])
                    {
                        if (HashPassword.ValidatePassword(newpassword, o1["pwd"].ToString()))
                        {
                            return Content("RepeatedPass"); 
                        }
                    }
                }
                catch { }

                usuario["pwd"] = HashPassword.CreateHash(newpassword);
                usuario["lastChgPassword"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                usertable.SaveRow(JsonConvert.SerializeObject(usuario), usuario["_id"].ToString());

                return Content("/User/Login");
            }
            else {
                return Content("WrongPass");
            }
        }
   
    }
}
