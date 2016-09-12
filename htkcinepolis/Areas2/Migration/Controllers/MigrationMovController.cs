﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RivkaAreas.Inventory.Models;
using System.Data.SqlServerCe;
using System.Data;
using System.Net;
//using iTextSharp.text;
//using iTextSharp.text.html;
//using iTextSharp.text.pdf;
//using iTextSharp.text.xml;
//using iTextSharp.text.html.simpleparser;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.CustomXmlDataProperties;
using System.IO;
using System.Timers;
using Rivka.Mail;
//using System.Windows.Forms;
using System.IO.Compression;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Rivka.Db.MongoDb;
using Rivka.Mail;
using RivkaAreas.Rule;
using Rivka.Error;
using System.Reflection;
using System.Dynamic;

using Rivka.Form;
using Rivka.User;
using Rivka.Mail;
using System.Web.Security;
using System.Reflection;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using RivkaAreas.Inventory.Models;

namespace RivkaAreas.Migration.Controllers
{
    [Authorize]
    public class MigrationMovController : Controller
    {
        //
        // GET: /Migration/Migration/
        protected MongoModel objReals;
        protected MongoModel objReferencedb;
        protected LocationTable locationsdb;
        protected UserTable usersdb;
        protected MongoModel demanddb;
        protected MongoModel demanddesktopdb;
        protected Notifications Notificationsclass;
        protected MongoModel locationsProfilesdb;
        protected MongoModel movprofilesdb;
        protected RivkaAreas.Tags.Models.ObjectReal objectrealdb;
        protected Messenger push;
        public int countimer;
        public MigrationMovController()
        {
            objReals = new MongoModel("ObjectReal");
            locationsdb = new LocationTable();
            objReferencedb = new MongoModel("ReferenceObjects");
            Notificationsclass = new Notifications();
            usersdb = new UserTable();
            locationsProfilesdb = new MongoModel("LocationProfiles");
            demanddb = new MongoModel("Demand");
            demanddesktopdb = new MongoModel("DemandDesktop");
            objectrealdb = new RivkaAreas.Tags.Models.ObjectReal("ObjectReal");
            movprofilesdb = new MongoModel("MovementProfiles");
            push = new Messenger();
            countimer = 0;
        }
        public ActionResult Index()
        {
          /*  double datevalue=41201.7300810185;
            DateTime newDate = DateTime.FromOADate(datevalue);
            DateTime init=new DateTime(1899, 12, 30);
            init=init.AddDays(datevalue);
           string text = newDate.TimeOfDay.ToString();*/
            return View();
        }
        public string extractfiles(String url,String typefile){
            try{
               string folder = "dct_folio";

                switch (typefile)
                {
                    case "0":
                        folder = "dct_folio";
                        break;
                    case "1":
                        folder = "act_folio";
                        break;
                    case "2":
                        folder = "dct_planeada";
                        break;

                }
                string relativepath = "\\Uploads\\MigrationMov\\"+folder+"\\";
                string absolutepath = Server.MapPath(relativepath);
                if (!System.IO.Directory.Exists(absolutepath))
                {
                    System.IO.Directory.CreateDirectory(absolutepath);
                }

               
                try
                {
                    ZipFile.ExtractToDirectory(url, absolutepath);
                }
                catch
                {

                }
                return "success";
            }catch{
                return null;
            }
        }
        public String saveFile(HttpPostedFileBase excelFile)
        {
            try
            {

                //  JArray mails = JsonConvert.DeserializeObject<JArray>(rowArray);
                string url = "";
                string filename = "";
                if (excelFile != null)
                {
                    filename = excelFile.FileName.ToString();

                }
                string userid = "";
                try
                {
                    userid = Session["_id"].ToString();
                }
                catch (Exception e)
                {
                    userid = Request.Cookies["_id2"].Value;
                }

                string ext = null;
                string patch = "";
               
                var fecha = DateTime.Now.Ticks;
                patch = userid + fecha;
                string relativepath = @"\Uploads\MigrationMov\" + userid;
                string absolutepath = Server.MapPath(relativepath);
                string absolutepathdown = Server.MapPath(relativepath);

                if (System.IO.Directory.Exists(absolutepath))
                {
                    try
                    {
                        System.IO.Directory.Delete(absolutepath, true);
                    }
                    catch (Exception e)
                    {
                       
                    }
                }

                if (excelFile != null)
                {
                    ext = excelFile.FileName.Split('.').Last(); //getting the extension
                }
                if (excelFile != null)
                {
                    try
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }

                        excelFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;
                    }
                    catch (Exception ex)
                    {
                        if (!System.IO.Directory.Exists(absolutepath))
                        {
                            System.IO.Directory.CreateDirectory(absolutepath);
                        }
                        excelFile.SaveAs(absolutepath + "\\" + patch + "." + ext);
                        //  patch = relativepath + "\\" + patch + "." + ext;
                        url = absolutepath + "\\" + patch + "." + ext;

                    }

                   

                    //  string desingc = objReals.GetRows();


                }



                return url;
            }
            catch (Exception ex)
            {
                return "";
            }

        }

        public JObject CreateJson(int type)
        {
            JObject model = new JObject();
            if (type == 1)
                model = generateJson();
            else
                model = generateJson1();

            return model;
        }
        public JObject generateJson()
        {
            try
            {
                JObject model = new JObject();

                model.Add("TM_CONSEC", "");
                model.Add("TM_TIPO_MOVTO", "");
                model.Add("TM_DESCRIPCION", "");
                model.Add("TM_FECHA_CREACION", "");
                model.Add("TM_USUARIO_CREACION", "");
                model.Add("TM_STATUS", "");
                model.Add("TM_ID_CONUNTO", "");
                model.Add("TM_DESC_CONJUNTO", "");
                model.Add("TM_ID_DEPTO", "");
                model.Add("TM_DESC_DEPTO", "");
                model.Add("TM_ID_UBICA", "");
                model.Add("TM_DESC_UBICA", "");
                model.Add("TM_ID_ARTICULOS", "");
                model.Add("TM_DESC_ARTICULOS", "");
                model.Add("TM_ID_UNICOS", "");
                model.Add("TM_MARCAS", "");
                model.Add("TM_MODELOS", "");
                model.Add("TM_SERIES", "");
                model.Add("TM_FECHAS_COMPRA", "");
                model.Add("TM_PRECIOS_COMPRA", "");
                model.Add("TM_NUMS_PEDIDO", "");
                model.Add("TM_NUMS_RECEPCION", "");
                model.Add("TM_NUMS_SOLICITUD", "");
                model.Add("TM_CANTIDADES", "");
                model.Add("TM_TIPOS_ETIQUETADO	", "");
                model.Add("TM_ID_CONJUNTO_DEST", "");
                model.Add("TM_DESC_CONJUNTO_DEST", "");
                model.Add("TM_ID_DEPTO_DEST", "");
                model.Add("TM_DESC_DEPTO_DEST", "");
                model.Add("TM_ID_UBICA_DEST", "");
                model.Add("TM_DESC_UBICA_DEST", "");
                model.Add("TM_NUM_ESCRITURA", "");
                model.Add("TM_NUM_CONVENIO", "");
                model.Add("TM_AUT_JI", "");
                model.Add("TM_AUT_01", "");
                model.Add("TM_AUT_02", "");
                model.Add("TM_AUT_03", "");
                model.Add("TM_AUT_04", "");
                model.Add("TM_AUT_05", "");
                model.Add("TM_AUT_06", "");
                model.Add("TM_AUT_07", "");
                model.Add("TM_AUT_08", "");
                model.Add("TM_AUT_JI_FECHA", "");
                model.Add("TM_AUT_01_FECHA", "");
                model.Add("TM_AUT_02_FECHA", "");
                model.Add("TM_AUT_03_FECHA", "");
                model.Add("TM_AUT_04_FECHA", "");
                model.Add("TM_AUT_05_FECHA", "");
                model.Add("TM_AUT_06_FECHA", "");
                model.Add("TM_AUT_07_FECHA", "");
                model.Add("TM_AUT_08_FECHA", "");
                model.Add("TM_FECHA_LIBERACION", "");
                model.Add("TM_USUARIO_LIBERACION", "");
                model.Add("TM_OBSERVACIONES	", "");
                model.Add("TM_AUTORIZADOR", "");
                model.Add("TM_DOCTO_FOLIO", "");
                model.Add("TM_AUTORIZADO", "");
                model.Add("TM_FECHA_AUTORIZADO", "");
                model.Add("TM_SOLICITUD_CORRECTA", "");
                model.Add("TM_ACTIVOS_INCORRECTOS", "");
                model.Add("TM_ACTINC_OBS", "");
                model.Add("TM_TIPOS_OBSOLEC", "");
                model.Add("TM_RECHAZO_OBSERVACIONES", "");
                model.Add("TM_cantidades_mover", "");
                model.Add("TM2_TIPO_BAJA", "");
                model.Add("TM2_DESTINO_MOVTO", "");
                model.Add("TM2_PRECIO_SUGERIDO_VENTA", "");
                model.Add("TM2_COMPRADOR", "");
                model.Add("TM2_BENEFICIARIO_DONACION", "");
                model.Add("TM2_BENEFICIARIO_RFC", "");
                model.Add("TM2_DESTRUCC_SAT_NOTARIO", "");
                model.Add("TM2_BAJA_PLANEADA_FOLIO_ACTA", "");
                model.Add("TM2_VALOR_CONTABLE_ACTIVO", "");
                model.Add("TM2_SELECC_ACTA_SAT", "");
                model.Add("TM2_ADICIONALES_COMPLETOS", "");
                model.Add("TM2_DOCTOS_COMPLETOS", "");
                model.Add("TM2_AUTORIZADORES_COMPLETOS", "");
                model.Add("TM2_RECHAZADO_POR", "");
                model.Add("TM2_FECHA_RECHAZO", "");
                model.Add("TM3_SALIDA_TEMPORAL", "");

                return model;

            }
            catch
            {
                return new JObject();

            }
        }
        public JObject generateJson1()
        {
            try
            {
                JObject model = new JObject();

                model.Add("ID_SOLICITUD", "");
                model.Add("ID_CONJUNTO", "");
                model.Add("DESC_CONJUNTO", "");
                model.Add("ID_GERENTE", "");
                model.Add("DESC_GERENTE", "");
                model.Add("ID_REPORTE_EN_MAXIMO", "");
                model.Add("FECHA_SALIDA_CONJUNTO", "");
                model.Add("NUMERO_GUIA_ENVIO", "");
                model.Add("VD_NOMBRE", "");
                model.Add("VD_PUESTO", "");
                model.Add("SR_NOMBRE", "");
                model.Add("SR_FIRMA", "");
                model.Add("VOBO_NOMBRE", "");
                model.Add("VOBO_FIRMA", "");
                model.Add("VOBO_ID", "");
                model.Add("VOBO_AUTORIZADO", "");
                model.Add("FECHA_SOLICITUD", "");
                model.Add("USUARIO_SOLICITUD", "");
                model.Add("VOBO_FECHA_AUT", "");
                model.Add("REPARADO", "");
                model.Add("NO_REPARADO", "");
                model.Add("NO_REP_OBSERVACIONES", "");
                model.Add("REVISA_NOMBRE", "");
                model.Add("REVISA_EMPRESA", "");
                model.Add("FECHA_ENVIO", "");
                model.Add("FECHA_REINGRESO", "");
                model.Add("REINGRESO_NOMBRE", "");
                model.Add("REINGRESO_FIRMA", "");
                model.Add("STATUS_SOLICITUD	", "");
                model.Add("S_ID_ACTIVO1", "");
                model.Add("S_CANTIDAD1", "");
                model.Add("S_DESCRIPCION1", "");
                model.Add("S_MARCA1", "");
                model.Add("S_MODELO1", "");
                model.Add("S_SERIE1", "");
                model.Add("S_EPC1", "");
                model.Add("S_ID_ACTIVO2", "");
                model.Add("S_CANTIDAD2", "");
                model.Add("S_DESCRIPCION2", "");
                model.Add("S_MARCA2", "");
                model.Add("S_MODELO2", "");
                model.Add("S_SERIE2", "");
                model.Add("S_EPC2", "");
                model.Add("S_ID_ACTIVO3", "");
                model.Add("S_CANTIDAD3", "");
                model.Add("S_DESCRIPCION3", "");
                model.Add("S_MARCA3", "");
                model.Add("S_MODELO3", "");
                model.Add("S_SERIE3", "");
                model.Add("S_EPC3", "");
                model.Add("S_ID_ACTIVO4", "");
                model.Add("S_CANTIDAD4", "");
                model.Add("S_DESCRIPCION4", "");
                model.Add("S_MARCA4", "");
                model.Add("S_MODELO4", "");
                model.Add("S_SERIE4", "");
                model.Add("S_EPC4","");
                model.Add("S_ID_ACTIVO5", "");
                model.Add("S_CANTIDAD5", "");
                model.Add("S_DESCRIPCION5", "");
                model.Add("S_MARCA5", "");
                model.Add("S_MODELO5", "");
                model.Add("S_SERIE5", "");
                model.Add("S_EPC5", "");
                model.Add("R_ID_ACTIVO1", "");
                model.Add("R_CANTIDAD1", "");
                model.Add("R_DESCRIPCION1", "");
                model.Add("R_MARCA1", "");
                model.Add("R_MODELO1", "");
                model.Add("R_SERIE1", "");
                model.Add("R_EPC1", "");
                model.Add("R_REGRESO1", "");
                model.Add("R_ID_ACTIVO2", "");
                model.Add("R_CANTIDAD2", "");
                model.Add("R_DESCRIPCION2", "");
                model.Add("R_MARCA2", "");
                model.Add("R_MODELO2", "");
                model.Add("R_SERIE2", "");
                model.Add("R_EPC2", "");
                model.Add("R_REGRESO2", "");
                model.Add("R_ID_ACTIVO3", "");
                model.Add("R_CANTIDAD3", "");
                model.Add("R_DESCRIPCION3", "");
                model.Add("R_MARCA3", "");
                model.Add("R_MODELO3", "");
                model.Add("R_SERIE3", "");
                model.Add("R_EPC3", "");
                model.Add("R_REGRESO3", "");
                model.Add("R_ID_ACTIVO4", "");
                model.Add("R_CANTIDAD4", "");
                model.Add("R_DESCRIPCION4", "");
                model.Add("R_MARCA4", "");
                model.Add("R_MODELO4", "");
                model.Add("R_SERIE4", "");
                model.Add("R_EPC4", "");
                model.Add("R_REGRESO4", "");
                model.Add("R_ID_ACTIVO5", "");
                model.Add("R_CANTIDAD5", "");
                model.Add("R_DESCRIPCION5", "");
                model.Add("R_MARCA5", "");
                model.Add("R_MODELO5", "");
                model.Add("R_SERIE5", "");
                model.Add("R_EPC5", "");
                model.Add("R_REGRESO5", "");
                model.Add("MOVTOS_ADICIONALES", "");	

                return model;

            }
            catch
            {
                return new JObject();

            }
        }
        public Dictionary<int, string> pointer(JObject jo)
        {
            try
            {
              Dictionary<int,string> dictmodel=  new Dictionary<int, string>();
              int index = 0;
              foreach(JProperty item in jo as JToken)
              {

                  try
                  {
                      dictmodel.Add(index, item.Name.ToString());
                      index++;
                  }
                  catch { }

              }
              return dictmodel;
            }
            catch
            {
                return new Dictionary<int, string>();
            }

        }
        public List<CreateModel2> match(JArray ja,int type)
        {
            try
            {
                List<CreateModel2> listdata = new List<CreateModel2>();

                foreach (JToken jt in ja)
                {
                    try
                    {
                        CreateModel2 datamodel = new CreateModel2(type);
                        foreach (JProperty jp in jt)
                        {
                            try
                            {
                                ((IDictionary<string, object>)datamodel.model)[jp.Name.ToString()] = jp.Value.ToString();
                            }
                            catch { }
                        }

                        listdata.Add(datamodel);
                    }
                    catch
                    {

                    }
                }
                return listdata;
            }
            catch
            {
                return new List<CreateModel2>();
            }
        }
        public List<CreateModel2> getRows(String url,int type)
        {
            List<CreateModel2> ListData = new List<CreateModel2>();
            JArray DataJa = new JArray();
            try
            {
                Dictionary<string, int> orderCell = new Dictionary<string, int>();
                string[] arrayalf = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU", "BV", "BW", "BX", "BY", "BZ", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL", "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ","DA","DB","DC" };

                for (int i = 0; i < arrayalf.Length; i++)
                {
                    orderCell.Add(arrayalf[i], i);
                }
                DataSet ds = new DataSet();

                List<List<string>> tr = new List<List<string>>();
                Dictionary<int, string> dictpointer = new Dictionary<int, string>();
                JObject initjo = CreateJson(type);
                dictpointer = pointer(initjo);
               
                try{
                using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(url, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                        var httpCurrent = System.Web.HttpContext.Current;
                     //  string url5= httpCurrent.Request.Url.PathAndQuery.ToString();
                        int rowindex=0;
                     /*   Parallel.ForEach(sheetData.Elements<Row>(),( r,pls,indexfor) =>
                       {*/
                        foreach(Row r in sheetData.Elements<Row>()){
                           try
                           {
                               //pls.Break;

                               if (r.RowIndex == 1)
                                   continue;
                                  // return;
                               List<string> td = new List<string>();
                               int index = 0;
                               //CreateModel Dataobj = new CreateModel();
                               JObject Datajo = CreateJson(type);
                               
                               int point = 0;
                              
                               // dynamic Dataobj = CreateModel();
                               foreach (Cell c in r.Elements<Cell>())
                               {
                                   try
                                   {
                                       //styleSheet.CellFormat

                                       string cellIndex = "";
                                      // cellIndex=c.CellReference.ToString().Substring(0, 1);
                                       foreach (char t in c.CellReference.ToString())
                                       {
                                          
                                           if (Regex.IsMatch(t.ToString(), "[A-Z]"))
                                           {
                                               cellIndex += t.ToString();
                                           }
                                       }
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
                                               // td.Add("");
                                               // Dataobj.setValue(index, "");
                                                Datajo[dictpointer[point]] = "";
                                                point++;
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

                                                 
                                                  // Dataobj.setValue(point, item.Text.Text);
                                                   Datajo[dictpointer[point]] = item.Text.Text;

                                               }
                                               else if (item.InnerText != null)
                                               {


                                                   //Dataobj.setValue(point, item.InnerText);
                                                   Datajo[dictpointer[point]] = item.InnerText;
                                               }
                                               else if (item.InnerXml != null)
                                               {


                                                  // Dataobj.setValue(point, item.InnerXml);
                                                   Datajo[dictpointer[point]] = item.InnerXml;
                                               }
                                           }
                                           else
                                           {


                                               //Dataobj.setValue(point, c.CellValue.Text);
                                               Datajo[dictpointer[point]] = c.CellValue.Text;
                                           }
                                       }
                                       else
                                       {

                                           try
                                           {

                                               if (c.CellValue != null)
                                               {
                                                  // Dataobj.setValue(point, c.CellValue.Text);
                                                   Datajo[dictpointer[point]] = c.CellValue.Text;
                                               }
                                               else
                                               {
                                                   ///Dataobj.setValue(point, "");
                                                   Datajo[dictpointer[point]] ="";
                                               }
                                           }
                                           catch (Exception ex)
                                           {

                                              // Dataobj.setValue(point, "");
                                               Datajo[dictpointer[point]] = "";
                                           }

                                       }
                                       index++;
                                   }
                                   catch
                                   {

                                       string error = "Celda incorrecta";
                                   }
                                   point++;
                                   if (point >= 80)
                                       break;
                               }
                              // ListData.Add(Dataobj);
                             
                                DataJa.Add(Datajo);
                           }
                           catch
                           {

                           }
                          // indexfor++;
                       }
                        //);
                        try
                        {
                            spreadsheetDocument.Close();
                        }
                        catch { }
                    }
                    catch {  }
                }}catch{

                }

               // CreateModel DataModel = new CreateModel();

                ListData = match(DataJa,type);
                return ListData;
            }
            catch
            {
                ListData = match(DataJa,type);
                return ListData;
            }
        }
        public List<CreateModel2> getRows2(String url)
        {
            List<CreateModel2> ListData = new List<CreateModel2>();
            try
            {
                Dictionary<string, int> orderCell = new Dictionary<string, int>();
                string[] arrayalf = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

                for (int i = 0; i < arrayalf.Length; i++)
                {
                    orderCell.Add(arrayalf[i], i);
                }
                DataSet ds = new DataSet();

                List<List<string>> tr = new List<List<string>>();
               
                using (DocumentFormat.OpenXml.Packaging.SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(url, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


                        WorksheetPart worksheetPart = workbookPart.WorksheetParts.Last();
                        SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                        Parallel.ForEach(sheetData.Elements<Row>(), r =>
                       {
                           try
                           {
                               if (r.RowIndex == 1)
                                   return;
                               List<string> td = new List<string>();
                               int index = 0;
                               CreateModel2 Dataobj = new CreateModel2(1);

                               int point = 0;
                               // dynamic Dataobj = CreateModel();
                               foreach (Cell c in r.Elements<Cell>())
                               {
                                   try
                                   {

                                       string cellIndex = c.CellReference.ToString().Substring(0, 1);
                                       bool validate = false;
                                       int numcellx = 0;
                                       /* foreach (var x in orderCell)
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
                                    
                                               // Dataobj.setValue(index, "");
                                            }
                                            index = index + numcellx;

                                        }*/
                                       Int32 id = -1;


                                       if (c.DataType != null && c.DataType.Value == CellValues.SharedString)
                                       {
                                           if (Int32.TryParse(c.InnerText, out id))
                                           {
                                               SharedStringItem item = GetSharedStringItemById(workbookPart, id);
                                               if (item.Text != null)
                                               {

                                                 
                                                   Dataobj.setValue(point, item.Text.Text);

                                               }
                                               else if (item.InnerText != null)
                                               {


                                                   Dataobj.setValue(point, item.InnerText);
                                               }
                                               else if (item.InnerXml != null)
                                               {


                                                   Dataobj.setValue(point, item.InnerXml);
                                               }
                                           }
                                           else
                                           {


                                               Dataobj.setValue(point, c.CellValue.Text);
                                           }
                                       }
                                       else
                                       {

                                           try
                                           {

                                               if (c.CellValue != null)
                                               {
                                                   Dataobj.setValue(point, c.CellValue.Text);
                                               }
                                               else
                                               {
                                                   Dataobj.setValue(point, "");
                                               }
                                           }
                                           catch (Exception ex)
                                           {

                                               Dataobj.setValue(point, "");
                                            
                                           }

                                       }
                                       index++;
                                   }
                                   catch
                                   {

                                       string error = "Celda incorrecta";
                                   }
                                   point++;
                               }
                               ListData.Add(Dataobj);
                           }
                           catch
                           {

                           }
                       });
                        try
                        {
                            spreadsheetDocument.Close();
                        }
                        catch { }
                    }
                    catch {  }
                }
                return ListData;
            }
            catch
            {
                return ListData;
            }
        }
        public List<dynamic> duplicateEpcs(List<CreateModel2> Rows,dynamic Conjuntos)
        {
            try{
            List<dynamic> resultepcs = new List<dynamic>();
            List<string> Epcs = new List<string>();
            Epcs = (from row in Rows.Cast<dynamic>() select (string)row.AF_EPC_COMPLETO).ToList();
            JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.ValidEpcs(Epcs));
            // rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
            dynamic resultepcs1 = (from objectsx in objects select new { epc = (string)objectsx["EPC"], name = (string)objectsx["name"], serie = (string)objectsx["serie"], marca = (string)objectsx["marca"], modelo = (string)objectsx["modelo"], location = (string)objectsx["location"], location_name = (string)objectsx["location_name"], conjunto = (string)objectsx["conjunto"], number = "", conjuntoname = "" }).ToList();

            foreach (dynamic obj in resultepcs1)
            {
                dynamic myobj = new ExpandoObject();
                myobj.epc = obj.epc;
                myobj.name = obj.name;
                myobj.serie = obj.serie;
                myobj.marca = obj.marca;
                myobj.modelo = obj.modelo;
                myobj.location = obj.location;
                myobj.location_name = obj.location_name;
                myobj.conjunto = obj.conjunto;
                myobj.number = obj.number;
                myobj.conjuntoname = obj.conjuntoname;
                resultepcs.Add(myobj);

            }
            if (Conjuntos != null)
            {
                foreach (dynamic item in resultepcs)
                {
                    try
                    {
                        foreach (dynamic item2 in Conjuntos)
                        {
                            try
                            {
                                if (item.conjunto == item2.id)
                                {
                                    item.number = item2.number;
                                    item.conjuntoname = item2.name;
                                    break;
                                }
                            }
                            catch { }
                        }
                    }
                    catch
                    {

                    }
                }
            }

            return resultepcs;
        }catch{
            return new List<dynamic>();
       }

        }
        public String migrationInit(String url,string type,string typefile="0")
        {
            try
            {
                int typeint = 1;
                try
                {
                     typeint = Convert.ToInt16(type);
                }
                catch { }
                if (typeint == 3)
                    return extractfiles(url,typefile);
                try
                {
                    List<CreateModel2> Rows = getRows(url,typeint);
                    List<string> Epcs = new List<string>();
                    dynamic Conjuntos = null;
                    List<string> profileslist = new List<string>();
                    List<string> Idslist = new List<string>();
                    List<dynamic> resultepcs = new List<dynamic>();
                    List<dynamic> resultids = new List<dynamic>();
                   /* try
                    {

                        JArray profiles = JsonConvert.DeserializeObject<JArray>(locationsProfilesdb.Get("name", "Conjunto"));
                        profileslist = (from prof in profiles select (string)prof["_id"]).ToList();
                        JArray conjuntresult = JsonConvert.DeserializeObject<JArray>(objectrealdb.Getby("profileId", profileslist));
                        // rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
                        Conjuntos = (from conj in conjuntresult select new { id = (string)conj["_id"], name = (string)conj["name"], number = (string)conj["number"] }).ToList();
                    }
                    catch { }*/

                    
                       // List<dynamic> epcsDuplicate = duplicateEpcs(Rows,Conjuntos);
                      

                        return generateObjs(Rows,type);


                   
                   
                }
                catch(Exception ex)
                {
                    return ex.ToString();
                }

               
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public void timercount()
        {
            countimer = countimer + 10;
        }
        public  void timerEvent(object source, ElapsedEventArgs e){

            timercount();

        }
        public string generatefolio(string folio)
        {
            try
            {
                int num = folio.Length;
                string auxfolio="";
                for(int i=num;i<10;i++){
                    auxfolio+="0";
                }
                folio=auxfolio+folio;
                return folio;
            }
            catch
            {
                return folio;
            }

        }
        public string convertToDate(string datedouble)
        {
            try
            {
                  double datevalue=0.0;
                  string result = datedouble;
              try{
                  datedouble = datedouble.Replace(',', '.');
                  datevalue=Convert.ToDouble(datedouble);
                  DateTime newDate = DateTime.FromOADate(datevalue);
                  result = newDate.ToString("dd/MM/yyyy HH:mm:ss");
                 }catch{
                  try{
                         datedouble=datedouble.Replace('.',',');
                         datevalue=Convert.ToDouble(datedouble);
                         DateTime newDate = DateTime.FromOADate(datevalue);
                        
                         result = newDate.ToString("dd/MM/yyyy HH:mm:ss");

                   }catch{
                       return datedouble;
                  }

                 }


              return result;
            }
            catch
            {
                return datedouble;
            }
        }
         public string SaveBaja(CreateModel2 row, Dictionary<string, string> objidsdict,Dictionary<string, string> objidsdict2, JArray locationslist,Dictionary<string,string> movprof,Dictionary<string,string> userdict)
        {
            string idr = "";
              
            try
            {
                string heads1 = "";
                string folio = row.model.TM_CONSEC;
                folio = generatefolio(folio);
                string locations= row.model.TM_ID_UBICA;// row.ub_id_ubicacion;
                string desclocation = row.model.TM_DESC_UBICA;
                string numserie = row.model.TM_SERIES;
                string conjunto = row.model.TM_ID_CONUNTO;
                string conjuntodesc = row.model.TM_DESC_CONJUNTO;
                string namelocation = row.model.TM_DESC_UBICA;
                string createuser = row.model.TM_USUARIO_CREACION;
                string statustype = row.model.TM_STATUS;
                string typemov = row.model.TM_TIPO_MOVTO;
                string marca = row.model.TM_MARCAS;
                string modelo = row.model.TM_MODELOS;
                string desctypemov = row.model.TM_DESCRIPCION;
                string depto = row.model.TM_ID_DEPTO;
                string descdepto = row.model.TM_DESC_DEPTO;
                string numrecp=row.model.TM_NUMS_RECEPCION;
                string numsolicitud=row.model.TM_NUMS_RECEPCION;
                string quantity=row.model.TM_CANTIDADES;
                string label=row.model.TM_TIPOS_ETIQUETADO;
                string data_aproved=row.model.TM_FECHA_AUTORIZADO;
                string denynote=row.model.TM_RECHAZO_OBSERVACIONES;
                string deniers=row.model.TM2_RECHAZADO_POR;
                string denydate=row.model.TM2_FECHA_RECHAZO;
                string dctoplaneada = row.model.TM2_BAJA_PLANEADA_FOLIO_ACTA;
                string adjudicating=row.model.TM_AUTORIZADOR;
                string adjudicatingstatus=row.model.TM_AUTORIZADO;
                string adjudicatingdate=row.model.TM_FECHA_AUTORIZADO;


                string tipomov=row.model.TM_TIPO_MOVTO;
               /* try
                {
                    tipomov = movprof[tipomov];
                }
                catch { }*/
               //Baja
              
              
                string donation_benefit=row.model.TM2_BENEFICIARIO_DONACION;
                string rfc=row.model.TM2_BENEFICIARIO_RFC;
                string typeact=row.model.TM2_SELECC_ACTA_SAT;
               string destinomov=row.model.TM2_DESTINO_MOVTO;
                switch(destinomov){
                    case "1":
                        destinomov="venta";
                        break;
                    case "2":
                        destinomov="donacion";
                        break;
                    case "3":
                        destinomov="destruccion";
                        break;
                    case "4":
                        destinomov="robo";
                        break;
                    case "5":
                        destinomov="siniestro";
                        break;
                }
               string dctfolio=row.model.TM_DOCTO_FOLIO;
               string ActFolio=row.model.TM2_BAJA_PLANEADA_FOLIO_ACTA;
               string deleteType=(row.model.TM2_TIPO_BAJA=="1")?"planeada":"no planeada";
               // string conjuntdestiny=row.model.TM_ID_CONJUNTO_DEST;
               // string conjuntdestinydesc=row.model.TM_DESC_CONJUNTO_DEST;
               // string locationdestiny=row.model.TM_ID_UBICA_DEST;
               // string locationdestinydesc=row.model.TM_DESC_UBICA_DEST;
                string reader=row.model.TM_NUM_ESCRITURA;
                string convenio=row.model.TM_NUM_CONVENIO;
                List<string> autos=new List<string>();
                List<string> autosdate=new List<string>();
                autos.Add(row.model.TM_AUT_01);
                autos.Add(row.model.TM_AUT_02);
                autos.Add(row.model.TM_AUT_03);
                autos.Add(row.model.TM_AUT_04);
                autos.Add(row.model.TM_AUT_05);
                autos.Add(row.model.TM_AUT_06);
                autos.Add(row.model.TM_AUT_07);
                //autos.Add(row.model.TM_AUT_08);
                autosdate.Add(row.model.TM_AUT_01_FECHA);
                 autosdate.Add(row.model.TM_AUT_02_FECHA);
                  autosdate.Add(row.model.TM_AUT_03_FECHA);
                 autosdate.Add(row.model.TM_AUT_04_FECHA);
                 autosdate.Add(row.model.TM_AUT_05_FECHA);
                 autosdate.Add(row.model.TM_AUT_06_FECHA);
                 autosdate.Add(row.model.TM_AUT_07_FECHA);
                // autosdate.Add(row.model.TM_AUT_08_FECHA);

               

                //jarray authorizations 
                
                JArray authorizations=new JArray();
                bool flag=true;
                int i=0;
                while(flag){
                    try{
                      JObject autojo=new JObject();

                      if (autos.ElementAt(i) == "")
                      {
                          i++;
                          continue;
                      }
                    string user=autos.ElementAt(i).Split('|').First();
                       
                    string approved=autos.ElementAt(i).Split('|').Last();
                    string date1=autosdate.ElementAt(i);
                    string idus = "";
                    try
                    {
                        if (userdict.TryGetValue(user.ToLower(), out idus) || userdict.TryGetValue(user, out idus))
                        {
                            
                        }
                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(user.ToLower()))
                                {
                                    idus = userdi.Key;
                                }
                            }
                            catch { }
                        }
                         
                    }
                    catch
                    {

                    }
                     autojo.Add("id_auto","null");
                     idus = (idus == null) ? "" : idus;
                     autojo.Add("id_user", idus);
                     autojo.Add("user",user);
                    autojo.Add("name",user);
                    autojo.Add("lastname", user);
                     autojo.Add("approved",approved);
                     if (date1.Length > 0)
                     {
                         date1 = convertToDate(date1);
                     }
                     autojo.Add("date",date1);

                   authorizations.Add(autojo);
                    }catch{

                    }

                    i++;
                    if(i==8)
                    flag=false;
                   }
                 // in object array
                string pricedate = row.model.TM_FECHAS_COMPRA;
                string price = row.model.TM_PRECIOS_COMPRA;
                string numpedido = row.model.TM_NUMS_PEDIDO;
                string referenceobj = "null";
                string referenceobjdesc = row.model.TM_DESC_ARTICULOS;
                string referenceobjid = row.model.TM_ID_ARTICULOS;
                string idobjreal = row.model.TM_ID_UNICOS; //id_registro 
                List<string> quantitylist = new List<string>();
                List<string> marcalist = new List<string>();
                List<string> modelolist = new List<string>();
                List<string> serielist = new List<string>();
                List<string> idsref = new List<string>();
                List<string> idsrefdesc = new List<string>();
                List<string> idsobjreal = new List<string>();
                List<string> pricelist = new List<string>();
                Dictionary<string,string> idrefdict=new Dictionary<string,string>();
                Dictionary<string,string> idrefdescdict=new Dictionary<string,string>();
                Dictionary<string,string> idobjrealdict=new Dictionary<string,string>();
                Dictionary<string,string> pricedict=new Dictionary<string,string>();
                try
                {
                    marcalist = marca.Split('|').ToList();
                }
                catch { }
                try
                {
                    modelolist = modelo.Split('|').ToList();
                }
                catch { }
                try
                {
                    quantitylist = quantity.Split('|').ToList();
                }
                catch { }
               
                try
                {
                    serielist = numserie.Split('|').ToList();
                }
                catch { }


                try
                {
                    idsref = referenceobjid.Split('|').ToList();
                }
                catch { }
                    try
                {
                    idsobjreal = idobjreal.Split('|').ToList();
                }
                catch { }
                    try
                    {
                        pricelist = price.Split('|').ToList();
                    }
                    catch { }
                //get id real by id_registro
                    int countd = 0;
                foreach(string obj in idsobjreal){
                try
                {
                   idobjrealdict.Add(countd.ToString(),(from objid in objidsdict2 where objid.Value == obj select (string)objid.Key).First().ToString());
                   countd++;
                }
                catch
                {
                    try
                    {
                        idobjrealdict.Add(countd.ToString(), "");
                        countd++;
                    }
                    catch (Exception ex)
                    {
                        countd++;
                    }
                }
                }
                countd = 0;
                foreach(string refs in idsref){
                try
                {
                    idrefdict.Add(countd.ToString(), (from objid in objidsdict where objid.Value == refs select (string)objid.Key).First().ToString());
                    countd++;
                }
                catch
                {
                    try
                    {
                        idrefdict.Add(countd.ToString(), "");
                        countd++;
                    }
                    catch(Exception ex)
                    {
                        countd++;
                    }
                }
                }
                countd = 0;
                foreach(string refsd in idsrefdesc){
                try
                {
                    idrefdescdict.Add(countd.ToString(), refsd);
                    countd++;
                }
                catch
                {
                    try
                    {
                        idrefdescdict.Add(countd.ToString(), "");
                        countd++;
                    }
                    catch (Exception ex)
                    {
                        countd++;
                    }
                }
                }
                countd = 0;
                foreach(string price1 in pricelist){
                try
                {
                    string price2 = (price1.Contains("$")) ? price1 : "$" + price1;
                    pricedict.Add(countd.ToString(), price2);
                    countd++;
                }
                catch
                {
                    try
                    {
                        pricedict.Add(countd.ToString(), price1);
                        countd++;
                    }
                    catch (Exception ex)
                    {
                        countd++;
                    }
                }
                }
                JArray objects = new JArray();

                Int64 timestamp = 20150101000000;
                string datecreated = row.model.TM_FECHA_CREACION;
                if (datecreated.Length > 0)
                {
                    datecreated = convertToDate(datecreated);
                }
                try
                {
                    if (datecreated.Length < 14)
                    {
                        datecreated = datecreated.Replace(" ", "");
                        datecreated = datecreated + " 00:00:00";
                    }
                    DateTime dateCreated2 = DateTime.ParseExact(datecreated, "dd/MM/yyyy HH:mm:ss", null);
                    timestamp = Convert.ToInt64(dateCreated2.ToString("yyyyMMddHHmmss"));
                }
                catch
                {

                }
                try
                {
                    string locationf = "";
                    string conjuntoid = "";
                    try
                    {
                      //  conjunto = (from conj in locationslist where (string)conj["number"] == conjunto select (string)conj["_id"]).First().ToString();
                    }
                    catch
                    {
                        // locationf = "";
                    }
                    try
                    {
                        // JArray getlocations = (from loc in locationslist where (string)loc["parent"] == conjuntoid select loc) as JArray;
                    //    locationf = (from conj in locationslist where (string)conj["number"] == locations select (string)conj["_id"]).First().ToString();
                        locationf = locations;
                    }
                    catch
                    {

                    }

                    locations = (locationf != "") ? locationf : "null";
                }
                catch { }
                JObject objectJo = new JObject();
                JArray objreals = new JArray();
                List<string> conjuntolist = new List<string>();
                List<string> conjuntodesclist = new List<string>();
                List<string> locationclist = new List<string>();
                List<string> locationdesclist = new List<string>();
                try
                {
                    conjuntolist = conjunto.Split('|').ToList();
                }
                catch { }
                try
                {
                    conjuntodesclist = conjuntodesc.Split('|').ToList();
                }
                catch { }
                try
                {
                    locationclist = locations.Split('|').ToList();
                }
                catch { }
                try
                {
                    locationdesclist = namelocation.Split('|').ToList();
                }
                catch { }
                for (int x = 0; x < idobjrealdict.Count(); x++)
                {
                    try
                    {
                        JObject objectJoreal = new JObject();
                        try
                        {
                            objectJoreal.Add("id", idobjrealdict.ElementAt(x).Value);
                

                        }
                        catch { }
                        try{
                            objectJoreal.Add("id_registro", idsobjreal.ElementAt(x));
                        }catch{

                        }
                        try
                        {
                            objectJoreal.Add("objectReference", idrefdict.ElementAt(x).Value);
                        }
                        catch { }
                          try
                        {
                            objectJoreal.Add("price", pricedict.ElementAt(x).Value);
                        }
                        catch { objectJoreal.Add("price", ""); }
                        objectJoreal.Add("date", datecreated);
                        label = (label.ToLower() == "n") ? "normal" : "No Etiquetado";
                        objectJoreal.Add("label", label);
                        objectJoreal.Add("department", depto);
                        objectJoreal.Add("departmentText", descdepto);
                        try
                        {
                            if (conjuntolist.Count() == 1)
                            {
                                objectJoreal.Add("conjunto", conjunto);
                            }
                            else
                            {
                                objectJoreal.Add("conjunto", conjuntolist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjunto", conjunto); }
                        try
                        {
                            if (conjuntodesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoText", conjuntodesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoText", conjuntodesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoText", conjuntodesc); }

                        try
                        {
                            if (locationclist.Count() == 1)
                            {
                                objectJoreal.Add("location", locations);
                            }
                            else
                            {
                                objectJoreal.Add("location", locationclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("location", locations); }
                        try
                        {
                            if (locationdesclist.Count() == 1)
                            {
                                objectJoreal.Add("locationText", namelocation);
                            }
                            else
                            {
                                objectJoreal.Add("locationText", locationdesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("locationText", namelocation); }
                        try
                        {
                            objectJoreal.Add("serie", serielist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("serie", ""); }
                        try
                        {
                            objectJoreal.Add("quantity", quantitylist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("quantity", ""); }
                       
                        try
                        {
                            objectJoreal.Add("marca", marcalist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("marca", ""); }
                        try
                        {
                            objectJoreal.Add("modelo", modelolist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("modelo", ""); }
                       
                        objectJoreal.Add("donation_benefit", modelo);
                        objectJoreal.Add("rfc", modelo);
                        objectJoreal.Add("typeact", modelo);
                  
                        try
                        {
                            objectJoreal.Add("object_id", idsobjreal.ElementAt(x));
                        }
                        catch { }
                        objreals.Add(objectJoreal);
                    }
                    catch
                    {

                    }
                }

                    switch (statustype)
                    {
                        case "A":
                            statustype = "5";
                            break;
                        case "C":
                            statustype = "6";
                            break;
                        case "R":
                            statustype = "7";
                            break;
                        default:
                            statustype = "5";
                            break;
                    }

                    string voboinfo = row.model.TM_AUT_08;
                    string approvedvobo = voboinfo.Split('|').Last();
                    string voboid = "";
                    string namevobo = "";

                    string fechavobo = row.model.TM_AUT_08_FECHA;
                    if (fechavobo.Length > 0)
                    {
                        fechavobo = convertToDate(fechavobo);
                    }
                    int approvedstatus = (approvedvobo == "1") ? 1 : 0;
                    try
                    {
                        namevobo = voboinfo.Split('|').First();
                    if (userdict.TryGetValue(namevobo.ToLower(), out voboid) || userdict.TryGetValue(namevobo, out voboid)) { }
                    foreach (var userdi in userdict)
                    {
                        try
                        {
                            if (userdi.Value.ToLower().Contains(namevobo.ToLower()))
                            {
                                voboid = userdi.Key;
                            }
                        }
                        catch { }
                    }

                }catch{

                }

                    JArray approval = new JArray();
                    try
                    {
                        JObject vobo = new JObject();
                        if (row.model.TM_AUT_08 != "")
                        {
                            vobo.Add("id_auto", "null");
                            voboid = (voboid == null) ? "" : voboid;
                            vobo.Add("id_user", voboid);
                            vobo.Add("user", voboinfo.Split('|').First());
                            vobo.Add("name", voboinfo.Split('|').First());
                            vobo.Add("lastname", namevobo);
                            vobo.Add("date", fechavobo);
                            vobo.Add("approved", approvedstatus);
                            approval.Add(vobo);
                        }
            
                    }
                    catch {  }
                    string idusercreator = "";
                    try
                    {

                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(createuser.ToLower()))
                                {
                                    idusercreator = userdi.Key;
                                }
                            }
                            catch { }
                        }

                    }
                    catch
                    {

                    }
                objectJo.Add("folio", folio);
                objectJo.Add("objects", objreals);
                objectJo.Add("authorizations", authorizations);
                objectJo.Add("notifications", new JObject());
               objectJo.Add("approval", approval);
                objectJo.Add("status", Convert.ToInt16(statustype));
                objectJo.Add("movement", tipomov);
                objectJo.Add("deleteType", deleteType);
                objectJo.Add("dctFolio",dctfolio);
                objectJo.Add("ActFolio",ActFolio);
                objectJo.Add("destinyOptions",destinomov);
                objectJo.Add("baja_planeada_dcto", dctoplaneada);
                objectJo.Add("movementFields", new JObject());
                objectJo.Add("extras", new JObject());
                objectJo.Add("profileFields", new JObject());
                objectJo.Add("CreatedDate", datecreated);
                objectJo.Add("location", locations);
                objectJo.Add("conjunto", conjunto);
                objectJo.Add("locationText", desclocation);
                objectJo.Add("conjuntoText", conjuntodesc);
                objectJo.Add("adjudicating", adjudicating);
                objectJo.Add("adjudicatingstatus", adjudicatingstatus);
                objectJo.Add("dctDate", adjudicatingdate);
                
                objectJo.Add("tiposolicitud", row.model.TM_TIPO_MOVTO);
                objectJo.Add("tipomovimiento", row.model.TM_DESCRIPCION);
                objectJo.Add("Creatorname", createuser);
                objectJo.Add("Creator", idusercreator);
                objectJo.Add("temporal", false);
               
                objectJo.Add("CreatedTimeStamp",timestamp);

                if (data_aproved.Length > 2)
                {
                    if (data_aproved.Length > 0)
                    {
                        data_aproved = convertToDate(data_aproved);
                    }
                    objectJo.Add("AuthorizedDate", data_aproved);
                }
                if (denydate.Length>2)
                {
                    if (denydate.Length > 0)
                    {
                        denydate = convertToDate(denydate);
                    }
                    objectJo.Add("DenyDate", denydate);
                }
                if (deniers.Length > 2)
                {
                    objectJo.Add("Deniers", deniers);
                }
                // objectJo.Add("id_articulo", row.ID_REGISTRO);
                objectJo.Add("ApprovedDate", "");

                string item = JsonConvert.SerializeObject(objectJo);
               
              

                try
                {
                    idr = demanddesktopdb.SaveRow(item);
                   
                   
                      

                }
                catch { }


                bool ok = true;
            }
            catch
            {  }

            return idr;
       }
       

        public string SaveAlta(CreateModel2 row, Dictionary<string, string> objidsdict,Dictionary<string, string> objidsdict2, JArray locationslist,Dictionary<string,string> movprof,Dictionary<string,string> userdict)
        {
            string idr = "";
              
            try
            {
                string heads1 = "";
                string folio = row.model.TM_CONSEC;
                folio = generatefolio(folio);
                string locations= row.model.TM_ID_UBICA;// row.ub_id_ubicacion;
                string desclocation = row.model.TM_DESC_UBICA;
                string numserie = row.model.TM_SERIES;
                string conjunto = row.model.TM_ID_CONUNTO;
                string conjuntodesc = row.model.TM_DESC_CONJUNTO;
                string namelocation = row.model.TM_DESC_UBICA;
                string createuser = row.model.TM_USUARIO_CREACION;
                string statustype = row.model.TM_STATUS;
                string typemov = row.model.TM_TIPO_MOVTO;
                string marca = row.model.TM_MARCAS;
                string modelo = row.model.TM_MODELOS;
                string desctypemov = row.model.TM_DESCRIPCION;
                string depto = row.model.TM_ID_DEPTO;
                string descdepto = row.model.TM_DESC_DEPTO;
                string numrecp=row.model.TM_NUMS_RECEPCION;
                string numsolicitud=row.model.TM_NUMS_RECEPCION;
                string quantity=row.model.TM_CANTIDADES;
                string label=row.model.TM_TIPOS_ETIQUETADO;
                string data_aproved=row.model.TM_FECHA_AUTORIZADO;
                string denynote=row.model.TM_RECHAZO_OBSERVACIONES;
                string deniers=row.model.TM2_RECHAZADO_POR;
                string denydate=row.model.TM2_FECHA_RECHAZO;

                string tipomov=row.model.TM_TIPO_MOVTO;
             /*   try
                {
                    tipomov = movprof[tipomov];
                }
                catch { }*/
               //Baja
                //string tipobaja=row.model.TM2_TIPO_BAJA;
                //string destinomov=row.model.TM2_DESTINO_MOVTO;

               // string conjuntdestiny=row.model.TM_ID_CONJUNTO_DEST;
               // string conjuntdestinydesc=row.model.TM_DESC_CONJUNTO_DEST;
               // string locationdestiny=row.model.TM_ID_UBICA_DEST;
               // string locationdestinydesc=row.model.TM_DESC_UBICA_DEST;
                string reader=row.model.TM_NUM_ESCRITURA;
                string convenio=row.model.TM_NUM_CONVENIO;
                List<string> autos=new List<string>();
                List<string> autosdate=new List<string>();
                autos.Add(row.model.TM_AUT_01);
                autos.Add(row.model.TM_AUT_02);
                autos.Add(row.model.TM_AUT_03);
                autos.Add(row.model.TM_AUT_04);
                autos.Add(row.model.TM_AUT_05);
                autos.Add(row.model.TM_AUT_06);
                autos.Add(row.model.TM_AUT_07);
                //autos.Add(row.model.TM_AUT_08);
                autosdate.Add(row.model.TM_AUT_01_FECHA);
                autosdate.Add(row.model.TM_AUT_02_FECHA);
                autosdate.Add(row.model.TM_AUT_03_FECHA);
                autosdate.Add(row.model.TM_AUT_04_FECHA);
                autosdate.Add(row.model.TM_AUT_05_FECHA);
                autosdate.Add(row.model.TM_AUT_06_FECHA);
                autosdate.Add(row.model.TM_AUT_07_FECHA);
               // autosdate.Add(row.model.TM_AUT_08_FECHA);

               

                //jarray authorizations 
                
                JArray authorizations=new JArray();
                bool flag=true;
                int i=0;
                while(flag){
                    try{
                      JObject autojo=new JObject();

                      if (autos.ElementAt(i) == "")
                      {
                          i++;
                          continue;
                      }
                    string user=autos.ElementAt(i).Split('|').First();
                       
                    string approved=autos.ElementAt(i).Split('|').Last();
                    string date1=autosdate.ElementAt(i);
                    string idus = "";
                    try
                    {
                        if (userdict.TryGetValue(user.ToLower(), out idus) || userdict.TryGetValue(user, out idus))
                        {
                            
                        }
                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(user.ToLower()))
                                {
                                    idus = userdi.Key;
                                }
                            }
                            catch { }
                        }
                    }
                    catch
                    {

                    }
                     autojo.Add("id_auto","null");
                     idus = (idus == null) ? "" : idus;
                     autojo.Add("id_user", idus);
                     autojo.Add("user",user);
                    autojo.Add("name",user);
                    autojo.Add("lastname", user);
                     autojo.Add("approved",approved);
                     if (date1.Length > 0)
                     {
                         date1 = convertToDate(date1);
                     }
                     autojo.Add("date", date1);
                     

                   authorizations.Add(autojo);
                    }catch{

                    }

                    i++;
                    if(i==8)
                    flag=false;
                   }
                 // in object array
                string pricedate = row.model.TM_FECHAS_COMPRA;
                string price = row.model.TM_PRECIOS_COMPRA;
                string numpedido = row.model.TM_NUMS_PEDIDO;
                string referenceobj = "null";
                string referenceobjdesc = row.model.TM_DESC_ARTICULOS;
                string referenceobjid = row.model.TM_ID_ARTICULOS;
                string idobjreal = row.model.TM_ID_UNICOS; //id_registro 
                List<string> quantitylist = new List<string>();
                List<string> marcalist = new List<string>();
                List<string> modelolist = new List<string>();
                List<string> serielist = new List<string>();
                List<string> idsref = new List<string>();
                List<string> idsrefdesc = new List<string>();
                List<string> idsobjreal = new List<string>();
                List<string> pricelist = new List<string>();
                Dictionary<string,string> idrefdict=new Dictionary<string,string>();
                Dictionary<string,string> idrefdescdict=new Dictionary<string,string>();
                Dictionary<string,string> idobjrealdict=new Dictionary<string,string>();
                Dictionary<string,string> pricedict=new Dictionary<string,string>();
                try
                {
                    marcalist = marca.Split('|').ToList();
                }
                catch { }
                try
                {
                    modelolist = modelo.Split('|').ToList();
                }
                catch { }
                try
                {
                    quantitylist = quantity.Split('|').ToList();
                }
                catch { }
               
                try
                {
                    serielist = numserie.Split('|').ToList();
                }
                catch { }


                try
                {
                    idsref = referenceobjid.Split('|').ToList();
                }
                catch { }
                    try
                {
                    idsobjreal = idobjreal.Split('|').ToList();
                }
                catch { }
                    try
                    {
                        pricelist = price.Split('|').ToList();
                    }
                    catch { }
                //get id real by id_registro
                    int countd = 0;
                    foreach (string obj in idsobjreal)
                    {
                        try
                        {
                           // idobjrealdict.Add(countd.ToString(), (from objid in objidsdict2 where objid.Value == obj select (string)objid.Key).First().ToString());
                            idobjrealdict.Add(countd.ToString(), obj);
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                idobjrealdict.Add(countd.ToString(), obj);
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                    countd = 0;
                    foreach (string refs in idsref)
                    {
                        try
                        {
                         //   idrefdict.Add(countd.ToString(), (from objid in objidsdict where objid.Value == refs select (string)objid.Key).First().ToString());
                            idrefdict.Add(countd.ToString(), refs);
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                idrefdict.Add(countd.ToString(), refs);
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                    countd = 0;
                    foreach (string refsd in idsrefdesc)
                    {
                        try
                        {
                            idrefdescdict.Add(countd.ToString(), refsd);
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                idrefdescdict.Add(countd.ToString(), "");
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                    countd = 0;
                    foreach (string price1 in pricelist)
                    {
                        try
                        {
                            string price2 = (price1.Contains("$")) ? price1 : "$" + price1;
                            pricedict.Add(countd.ToString(), price2);
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                pricedict.Add(countd.ToString(), price1);
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                JArray objects = new JArray();

                Int64 timestamp = 20150101000000;
                string datecreated = row.model.TM_FECHA_CREACION;
                if (datecreated.Length > 0)
                {
                    datecreated = convertToDate(datecreated);
                }
                try
                {
                    if (datecreated.Length < 14)
                    {
                        datecreated = datecreated.Replace(" ", "");
                        datecreated = datecreated + " 00:00:00";
                    }
                    DateTime dateCreated2 = DateTime.ParseExact(datecreated, "dd/MM/yyyy HH:mm:ss", null);
                    timestamp = Convert.ToInt64(dateCreated2.ToString("yyyyMMddHHmmss"));
                }
                catch
                {

                }
                try
                {
                    string locationf = "";
                    string conjuntoid = "";
                    try
                    {
                      //  conjunto = (from conj in locationslist where (string)conj["number"] == conjunto select (string)conj["_id"]).First().ToString();
                    }
                    catch
                    {
                        // locationf = "";
                    }
                    try
                    {
                        // JArray getlocations = (from loc in locationslist where (string)loc["parent"] == conjuntoid select loc) as JArray;
                      //  locationf = (from conj in locationslist where (string)conj["number"] == locations select (string)conj["_id"]).First().ToString();
                        locationf = locations;
                    }
                    catch
                    {

                    }

                    locations = (locationf != "") ? locationf : "null";
                }
                catch { }
                JObject objectJo = new JObject();
                JArray objreals = new JArray();
                List<string> conjuntolist = new List<string>();
                List<string> conjuntodesclist = new List<string>();
                List<string> locationclist = new List<string>();
                List<string> locationdesclist = new List<string>();
                try
                {
                    conjuntolist = conjunto.Split('|').ToList();
                }
                catch { }
                try
                {
                    conjuntodesclist = conjuntodesc.Split('|').ToList();
                }
                catch { }
                try
                {
                    locationclist = locations.Split('|').ToList();
                }
                catch { }
                try
                {
                    locationdesclist = namelocation.Split('|').ToList();
                }
                catch { }
                for (int x = 0; x < idobjrealdict.Count(); x++)
                {
                    try
                    {
                        JObject objectJoreal = new JObject();
                        try
                        {
                          //  objectJoreal.Add("id", idobjrealdict.ElementAt(x).Value);
                            objectJoreal.Add("id", idrefdict.ElementAt(x).Value);
                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("objectReference", idrefdict.ElementAt(x).Value);
                        }
                        catch { }
                       
                        try
                        {
                            objectJoreal.Add("price", pricedict.ElementAt(x).Value);
                        }
                        catch { objectJoreal.Add("price", ""); }
                        objectJoreal.Add("date", datecreated);
                        label = (label.ToLower() == "n") ? "normal" : "no_etiquetable";
                        objectJoreal.Add("label", label);
                        objectJoreal.Add("department", depto);
                        objectJoreal.Add("departmentText", descdepto);
                        try
                        {
                            if (conjuntolist.Count() == 1)
                            {
                                objectJoreal.Add("conjunto", conjunto);
                            }
                            else
                            {
                                objectJoreal.Add("conjunto", conjuntolist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjunto",conjunto); }
                        try
                        {
                            if (conjuntodesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoText", conjuntodesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoText", conjuntodesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoText", conjuntodesc); }

                        try
                        {
                            if (locationclist.Count() == 1)
                            {
                                objectJoreal.Add("location", locations);
                            }
                            else
                            {
                                objectJoreal.Add("location", locationclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("location", locations); }
                        try
                        {
                            if (locationdesclist.Count() == 1)
                            {
                                objectJoreal.Add("locationText", namelocation);
                            }
                            else
                            {
                                objectJoreal.Add("locationText", locationdesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("locationText", namelocation); }
                       
                        try
                        {
                            objectJoreal.Add("serie", serielist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("serie", ""); }
                        try
                        {
                            objectJoreal.Add("quantity", quantitylist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("quantity", ""); }
                       
                        try
                        {
                            objectJoreal.Add("marca", marcalist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("marca", ""); }
                        try
                        {
                            objectJoreal.Add("modelo", modelolist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("modelo", ""); }
                       
                        try
                        {
                            objectJoreal.Add("object_id", idsref.ElementAt(x));
                        }
                        catch { }
                        objreals.Add(objectJoreal);
                    }
                    catch
                    {

                    }
                }

                    switch (statustype)
                    {
                        case "A":
                            statustype = "5";
                            break;
                        case "C":
                            statustype = "6";
                            break;
                        case "R":
                            statustype = "7";
                            break;
                        default:
                            statustype = "5";
                            break;
                    }

                    string voboinfo = row.model.TM_AUT_08;
                    string approvedvobo = voboinfo.Split('|').Last();
                    string voboid = "";
                    string namevobo = "";

                    string fechavobo = row.model.TM_AUT_08_FECHA;
                    if (fechavobo.Length > 0)
                    {
                        fechavobo = convertToDate(fechavobo);
                    }
                    int approvedstatus = (approvedvobo == "1") ? 1 : 0;
                    try
                    {
                        namevobo = voboinfo.Split('|').First();
                        if (userdict.TryGetValue(namevobo.ToLower(), out voboid) || userdict.TryGetValue(namevobo, out voboid)) { }
                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(namevobo.ToLower()))
                                {
                                    voboid = userdi.Key;
                                }
                            }
                            catch { }
                        }
                    }
                    catch
                    {

                    }

                    JArray approval = new JArray();
                    try
                    {
                        JObject vobo = new JObject();
                        if (row.model.TM_AUT_08 != "")
                        {
                            vobo.Add("id_auto", "null");
                            voboid = (voboid == null) ? "" : voboid;
                            vobo.Add("id_user", voboid);
                            vobo.Add("user", voboinfo.Split('|').First());
                            vobo.Add("name", voboinfo.Split('|').First());
                            vobo.Add("lastname", namevobo);
                            vobo.Add("date", fechavobo);
                            vobo.Add("approved", approvedstatus);
                            approval.Add(vobo);
                        }

                    }
                    catch { }
                    string idusercreator = "";
                    try
                    {

                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(createuser.ToLower()))
                                {
                                    idusercreator = userdi.Key;
                                }
                            }
                            catch { }
                        }

                    }
                    catch
                    {

                    }
                objectJo.Add("folio", folio);
                objectJo.Add("objects", objreals);
               
                objectJo.Add("authorizations", authorizations);
                objectJo.Add("notifications", new JObject());
                objectJo.Add("approval", approval);
                objectJo.Add("status", Convert.ToInt16(statustype));
                objectJo.Add("movement", tipomov);
                objectJo.Add("movementFields", new JObject());
                objectJo.Add("extras", new JObject());
                objectJo.Add("profileFields", new JObject());
                objectJo.Add("CreatedDate", datecreated);
                objectJo.Add("location", locations);
                objectJo.Add("conjunto", conjunto);
                objectJo.Add("locationText", desclocation);
                objectJo.Add("conjuntoText", conjuntodesc);
                objectJo.Add("tiposolicitud", row.model.TM_TIPO_MOVTO);
                objectJo.Add("tipomovimiento", row.model.TM_DESCRIPCION);
                objectJo.Add("Creatorname", createuser);
                objectJo.Add("Creator", idusercreator);
                objectJo.Add("temporal", false);
                 objectJo.Add("CreatedTimeStamp",timestamp);

                if (data_aproved.Length > 2)
                {
                    if (data_aproved.Length > 0)
                    {
                        data_aproved = convertToDate(data_aproved);
                    }
                    objectJo.Add("AuthorizedDate", data_aproved);
                }
                if (denydate.Length>2)
                {
                    if (denydate.Length > 0)
                    {
                        denydate = convertToDate(denydate);
                    }
                    objectJo.Add("DenyDate", denydate);
                }
                if (deniers.Length > 2)
                {
                    objectJo.Add("Deniers", deniers);
                }
                // objectJo.Add("id_articulo", row.ID_REGISTRO);
                objectJo.Add("ApprovedDate", "");

                string item = JsonConvert.SerializeObject(objectJo);
               
              

                try
                {
                    idr = demanddesktopdb.SaveRow(item);
                   
                   
                      

                }
                catch { }


                bool ok = true;
            }
            catch
            {  }

            return idr;
       }
          public string SaveTemp(CreateModel2 row, Dictionary<string, string> objidsdict,Dictionary<string, string> objidsdict2, JArray locationslist,Dictionary<string,string> movprof,Dictionary<string,string> userdict)
        {
            string idr = "";
              
            try
            {
                string heads1 = "";
                string folio = row.model.ID_SOLICITUD;
                folio = generatefolio(folio);
                string gerente=row.model.ID_GERENTE;
                string gerentenombre=row.model.DESC_GERENTE;
                string reportemax=row.model.ID_REPORTE_EN_MAXIMO;
                string createuser = row.model.USUARIO_SOLICITUD;
                string conjuntodest=row.model.ID_CONJUNTO;
                string conjuntdestinydesc=row.model.DESC_CONJUNTO;
               
               
                string referenceobj = "null";
             
                string orderNumber = row.model.NUMERO_GUIA_ENVIO; //id_registro 

                List<string> idsref = new List<string>();
                List<string> idsrefdesc = new List<string>();
                List<string> idsobjreal = new List<string>();
                try
                {
                    idsobjreal.Add(row.model.S_ID_ACTIVO1);
                    idsobjreal.Add(row.model.S_ID_ACTIVO2);
                    idsobjreal.Add(row.model.S_ID_ACTIVO3);
                    idsobjreal.Add(row.model.S_ID_ACTIVO4);
                    idsobjreal.Add(row.model.S_ID_ACTIVO5);
                }
                catch
                {

                }
                List<string> pricelist = new List<string>();
                Dictionary<string,string> idrefdict=new Dictionary<string,string>();
                Dictionary<string,string> idrefdescdict=new Dictionary<string,string>();
                Dictionary<string,string> idobjrealdict=new Dictionary<string,string>();
                Dictionary<string,string> pricedict=new Dictionary<string,string>();
                List<string> conjuntodestinylist = new List<string>();
                List<string> conjuntodestinydesclist = new List<string>();
               
                try
                {
                    conjuntodestinylist = conjuntodest.Split('|').ToList();
                }
                catch { }
                try
                {
                    conjuntodestinydesclist = conjuntdestinydesc.Split('|').ToList();
                }
                catch { }
              
               

                //get id real by id_registro
                int countd = 0;
                foreach(string obj in idsobjreal){
                try
                {
                   idobjrealdict.Add(countd.ToString(),(from objid in objidsdict2 where objid.Value == obj select (string)objid.Key).First().ToString());
                   countd++;
                }
                catch
                {
                    try
                    {
                        idobjrealdict.Add(countd.ToString(), ""); countd++;
                    }
                    catch (Exception ex)
                    {
                        countd++;
                    }
                }
                }
                countd = 0;
                foreach(string refs in idsref){
                try
                {
                    idrefdict.Add(countd.ToString(),(from objid in objidsdict where objid.Value == refs select (string)objid.Key).First().ToString());
                    countd++;
                }
                catch
                {
                    try
                    {
                        idrefdict.Add(countd.ToString(), ""); countd++;
                    }
                    catch (Exception ex) { countd++; }
                }
                }
               
               
                JArray objects = new JArray();

                Int64 timestamp = 10000101000000;
                string datecreated = row.model.FECHA_SALIDA_CONJUNTO;
                if (datecreated.Length > 0)
                {
                    datecreated = convertToDate(datecreated);
                }
                try
                {
                    if (datecreated.Length < 14)
                    {
                        datecreated = datecreated.Replace(" ", "");
                        datecreated = datecreated + " 00:00:00";
                    }
                    DateTime dateCreated2 = DateTime.ParseExact(datecreated, "dd/MM/yyyy HH:mm:ss", null);
                    timestamp = Convert.ToInt64(dateCreated2.ToString("yyyyMMddHHmmss"));
                }
                catch
                {

                }
                
                  try
                {
                    string locationfdest = "";
                    string conjuntoiddest = "";
                    try
                    {
                      //  conjuntodest = (from conjd in locationslist where (string)conjd["number"] == conjuntodest select (string)conjd["_id"]).First().ToString();
                    }
                    catch
                    {
                        // locationf = "";
                    }
                  
                }
                catch { }
                JObject objectJo = new JObject();
                JArray objreals = new JArray();
               ///object 1
               ///
                string act1=row.model.S_ID_ACTIVO1;
              //  if (act1.Length > 0) {
                    try
                    {
                        JObject objectJoreal = new JObject();
                        string entry = "0";
                        try
                        {
                            string id1 = "";
                            if (objidsdict2.TryGetValue(row.model.S_ID_ACTIVO1, out id1))
                            {

                            }
                            objectJoreal.Add("id", id1);

                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("objectReference", "");
                        }
                        catch { }
                        try
                        {
                            entry = row.model.R_ID_ACTIVO1;
                            entry = (entry.Length > 1) ? "1" : "0";
                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("serie_old", row.model.S_SERIE1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("serie", row.model.R_SERIE1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("quantity", row.model.S_CANTIDAD1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("quantity_r", row.model.R_CANTIDAD1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("name_old", row.model.S_DESCRIPCION1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("name", row.model.R_DESCRIPCION1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("marca", row.model.S_MARCA1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("marca_new", row.model.R_MARCA1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("modelo", row.model.S_MODELO1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("modelo_new", row.model.R_MODELO1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("EPC", row.model.S_EPC1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("EPC_new", row.model.S_EPC1);
                        }
                        catch
                        {

                        }
                        try
                        {
                            string repaired = (row.model.REPARADO == "TRUE") ? "1" : "0";
                            objectJoreal.Add("repaired", repaired);
                        }
                        catch
                        {

                        }
                        try
                        {

                            objectJoreal.Add("repaired_motive", row.model.NO_REP_OBSERVACIONES);
                        }
                        catch
                        {

                        }
                        objectJoreal.Add("date", datecreated);

                         try
                        {
                            if (conjuntodestinylist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodest);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodestinylist.ElementAt(0));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestiny",conjuntodest); }
                        try
                        {
                            if (conjuntodestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntodestinydesclist.ElementAt(0));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc); }
                        objectJoreal.Add("entry", entry);

                        try
                        {
                            objectJoreal.Add("object_id", idsobjreal.ElementAt(0));
                        }
                        catch { }
                        if (objectJoreal["object_id"].ToString() != "")
                            objreals.Add(objectJoreal);
                    }
                    catch
                    {

                    }
               // }
                //end object 1

                    ///object 2
                string act2 = row.model.S_ID_ACTIVO2;
             //   if (act2.Length > 0)  {
                    try
                    {
                        JObject objectJoreal = new JObject();
                        string entry = "0";
                        try
                        {
                            string id1 = "";
                            if (objidsdict2.TryGetValue(row.model.S_ID_ACTIVO2, out id1))
                            {

                            }
                            objectJoreal.Add("id", id1);

                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("objectReference", "");
                        }
                        catch { }
                        try
                        {
                            entry = row.model.R_ID_ACTIVO2;
                            entry = (entry.Length > 1) ? entry : "0";
                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("serie_old", row.model.S_SERIE2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("serie", row.model.R_SERIE2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("quantity", row.model.S_CANTIDAD2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("quantity_r", row.model.R_CANTIDAD2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("name_old", row.model.S_DESCRIPCION2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("name", row.model.R_DESCRIPCION2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("marca", row.model.S_MARCA2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("marca_new", row.model.R_MARCA2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("modelo", row.model.S_MODELO2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("modelo_new", row.model.R_MODELO2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("EPC", row.model.S_EPC2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("EPC_new", row.model.S_EPC2);
                        }
                        catch
                        {

                        }
                        try
                        {
                            string repaired = (row.model.REPARADO == "TRUE") ? "1" : "0";
                            objectJoreal.Add("repaired", repaired);
                        }
                        catch
                        {

                        }
                        try
                        {

                            objectJoreal.Add("repaired_motive", row.model.NO_REP_OBSERVACIONES);
                        }
                        catch
                        {

                        }
                        objectJoreal.Add("date", datecreated);


                          try
                        {
                            if (conjuntodestinylist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodest);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodestinylist.ElementAt(1));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestiny",conjuntodest); }
                        try
                        {
                            if (conjuntodestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntodestinydesclist.ElementAt(1));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc); }
                        objectJoreal.Add("entry", entry);

                        try
                        {
                            objectJoreal.Add("object_id", idsobjreal.ElementAt(1));
                        }
                        catch { }
                        if (objectJoreal["object_id"].ToString() != "")
                            objreals.Add(objectJoreal);
                    }
                    catch
                    {

                    }
              //  }
                    //end object 2
                    ///object 1
                string act3 = row.model.S_ID_ACTIVO3;
              //  if (act3.Length > 0) {
                    try
                    {
                        JObject objectJoreal = new JObject();
                        string entry = "0";
                        try
                        {
                            string id1 = "";
                            if (objidsdict2.TryGetValue(row.model.S_ID_ACTIVO3, out id1))
                            {

                            }
                            objectJoreal.Add("id", id1);

                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("objectReference", "");
                        }
                        catch { }
                        try
                        {
                            entry = row.model.R_ID_ACTIVO3;
                            entry = (entry.Length > 1) ? entry : "0";
                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("serie_old", row.model.S_SERIE3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("serie", row.model.R_SERIE3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("quantity", row.model.S_CANTIDAD3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("quantity_r", row.model.R_CANTIDAD3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("name_old", row.model.S_DESCRIPCION3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("name", row.model.R_DESCRIPCION3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("marca", row.model.S_MARCA3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("marca_new", row.model.R_MARCA3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("modelo", row.model.S_MODELO3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("modelo_new", row.model.R_MODELO3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("EPC", row.model.S_EPC3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            objectJoreal.Add("EPC_new", row.model.S_EPC3);
                        }
                        catch
                        {

                        }
                        try
                        {
                            string repaired = (row.model.REPARADO == "TRUE") ? "1" : "0";
                            objectJoreal.Add("repaired", repaired);
                        }
                        catch
                        {

                        }
                        try
                        {

                            objectJoreal.Add("repaired_motive", row.model.NO_REP_OBSERVACIONES);
                        }
                        catch
                        {

                        }
                        objectJoreal.Add("date", datecreated);


                          try
                        {
                            if (conjuntodestinylist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodest);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodestinylist.ElementAt(2));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestiny",conjuntodest); }
                        try
                        {
                            if (conjuntodestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntodestinydesclist.ElementAt(2));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc); }
                        objectJoreal.Add("entry", entry);

                        try
                        {
                            objectJoreal.Add("object_id", idsobjreal.ElementAt(2));
                        }
                        catch { }
                        if (objectJoreal["object_id"].ToString() != "")
                            objreals.Add(objectJoreal);
                    }
                    catch
                    {

                    }
              //  }
                    //end object 3
                    ///object 4
                    string act4=row.model.S_ID_ACTIVO4;
                //    if (act4.Length > 0){
                        try
                        {
                            JObject objectJoreal = new JObject();
                            string entry = "0";
                            try
                            {
                                string id1 = "";
                                if (objidsdict2.TryGetValue(row.model.S_ID_ACTIVO4, out id1))
                                {

                                }
                                objectJoreal.Add("id", id1);

                            }
                            catch { }
                            try
                            {
                                objectJoreal.Add("objectReference", "");
                            }
                            catch { }
                            try
                            {
                                entry = row.model.R_ID_ACTIVO4;
                                entry = (entry.Length > 1) ? entry : "0";
                            }
                            catch { }
                            try
                            {
                                objectJoreal.Add("serie_old", row.model.S_SERIE4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("serie", row.model.R_SERIE4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("quantity", row.model.S_CANTIDAD4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("quantity_r", row.model.R_CANTIDAD4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("name_old", row.model.S_DESCRIPCION4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("name", row.model.R_DESCRIPCION4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("marca", row.model.S_MARCA4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("marca_new", row.model.R_MARCA4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("modelo", row.model.S_MODELO4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("modelo_new", row.model.R_MODELO4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("EPC", row.model.S_EPC4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("EPC_new", row.model.S_EPC4);
                            }
                            catch
                            {

                            }
                            try
                            {
                                string repaired = (row.model.REPARADO == "TRUE") ? "1" : "0";
                                objectJoreal.Add("repaired", repaired);
                            }
                            catch
                            {

                            }
                            try
                            {

                                objectJoreal.Add("repaired_motive", row.model.NO_REP_OBSERVACIONES);
                            }
                            catch
                            {

                            }
                            objectJoreal.Add("date", datecreated);


                               try
                        {
                            if (conjuntodestinylist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodest);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodestinylist.ElementAt(3));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestiny",conjuntodest); }
                        try
                        {
                            if (conjuntodestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntodestinydesclist.ElementAt(3));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc); }
                            objectJoreal.Add("entry", entry);

                            try
                            {
                                objectJoreal.Add("object_id", idsobjreal.ElementAt(3));
                            }
                            catch { }
                            if (objectJoreal["object_id"].ToString() != "")
                                objreals.Add(objectJoreal);
                        }
                        catch
                        {

                        }
                 //   }
                    //end object 4
                    ///object 5
                    string act5 = row.model.S_ID_ACTIVO5;
                   // if (act5.Length > 0) {
                        try
                        {
                            JObject objectJoreal = new JObject();
                            string entry = "0";
                            try
                            {
                                string id1 = "";
                                if (objidsdict2.TryGetValue(row.model.S_ID_ACTIVO5, out id1))
                                {

                                }
                                objectJoreal.Add("id", id1);

                            }
                            catch { }
                            try
                            {
                                objectJoreal.Add("objectReference", "");
                            }
                            catch { }
                            try
                            {
                                entry = row.model.R_ID_ACTIVO5;
                                entry = (entry.Length > 1) ? entry : "0";
                            }
                            catch { }
                            try
                            {
                                objectJoreal.Add("serie_old", row.model.S_SERIE5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("serie", row.model.R_SERIE5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("quantity", row.model.S_CANTIDAD5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("quantity_r", row.model.R_CANTIDAD5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("name_old", row.model.S_DESCRIPCION5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("name", row.model.R_DESCRIPCION5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("marca", row.model.S_MARCA5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("marca_new", row.model.R_MARCA5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("modelo", row.model.S_MODELO5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("modelo_new", row.model.R_MODELO5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("EPC", row.model.S_EPC5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                objectJoreal.Add("EPC_new", row.model.S_EPC5);
                            }
                            catch
                            {

                            }
                            try
                            {
                                string repaired = (row.model.REPARADO == "TRUE") ? "1" : "0";
                                objectJoreal.Add("repaired", repaired);
                            }
                            catch
                            {

                            }
                            try
                            {

                                objectJoreal.Add("repaired_motive", row.model.NO_REP_OBSERVACIONES);
                            }
                            catch
                            {

                            }
                            objectJoreal.Add("date", datecreated);


                               try
                        {
                            if (conjuntodestinylist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodest);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodestinylist.ElementAt(4));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestiny",conjuntodest); }
                        try
                        {
                            if (conjuntodestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntodestinydesclist.ElementAt(4));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc); }
                            objectJoreal.Add("entry", entry);

                            try
                            {
                                objectJoreal.Add("object_id", idsobjreal.ElementAt(4));
                            }
                            catch { }
                            if(objectJoreal["object_id"].ToString()!="")
                              objreals.Add(objectJoreal);
                        }
                        catch
                        {

                        }
                    //}
                    //end object 5
                   

                string voboid="";
                string namevobo="";
                string fechavobo=row.model.VOBO_FECHA_AUT;
                if (fechavobo.Length > 0)
                {
                    fechavobo = convertToDate(fechavobo);
                }
                int approvedstatus=(fechavobo.Length>4)?1:0;
                try{
                    namevobo=row.model.VOBO_NOMBRE;
                    if(userdict.TryGetValue(row.model.VOBO_ID,out voboid) || userdict.TryGetValue(row.model.VOBO_ID,out voboid)){
                    };
                    foreach (var userdi in userdict)
                    {
                        try
                        {
                            if (userdi.Value.ToLower().Contains(namevobo.ToLower()))
                            {
                                voboid = userdi.Key;
                            }
                        }
                        catch { }
                    }

                }catch{

                }

                    JArray approval = new JArray();
                    try
                    {
                        JObject vobo = new JObject();
                        vobo.Add("id_auto","null");
                        vobo.Add("id_user",voboid);
                        vobo.Add("user",row.model.VOBO_ID);
                        vobo.Add("name",namevobo);
                        vobo.Add("lastname", namevobo);
                        vobo.Add("date",fechavobo);
                        vobo.Add("approved",approvedstatus);
                       approval.Add(vobo);
            
            
                    }
                    catch {  }
                    string idusercreator = "";
                    try
                    {

                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(createuser.ToLower()))
                                {
                                    idusercreator = userdi.Key;
                                }
                            }
                            catch { }
                        }

                    }
                    catch
                    {

                    }
                objectJo.Add("folio", folio);
                objectJo.Add("objects", objreals);
                objectJo.Add("authorizations", new JObject());
                objectJo.Add("notifications", new JObject());
                objectJo.Add("approval", approval);
                objectJo.Add("status", 5);
                objectJo.Add("movement","547604e56e576011a8356187" );
                objectJo.Add("movementFields", new JObject());
                objectJo.Add("extras", new JObject());
                objectJo.Add("profileFields", new JObject());
                objectJo.Add("CreatedDate", datecreated);
                objectJo.Add("conjuntoDestiny", conjuntodest);
                objectJo.Add("conjuntoDestinyText", conjuntdestinydesc);
                objectJo.Add("Creatorname", createuser);
                objectJo.Add("Creator", idusercreator);
                objectJo.Add("temporal", true);
                objectJo.Add("tipomovimiento", "Salida temporal");
                objectJo.Add("repaired", row.model.REPARADO);
                objectJo.Add("reingresdate", row.model.FECHA_REINGRESO);
                objectJo.Add("CreatedTimeStamp",timestamp);

                
                // objectJo.Add("id_articulo", row.ID_REGISTRO);
                objectJo.Add("ApprovedDate", fechavobo);

                string item = JsonConvert.SerializeObject(objectJo);
               
              

                try
                {
                    idr = demanddesktopdb.SaveRow(item);
                   
                   
                      

                }
                catch { }


                bool ok = true;
            }
            catch
            {  }

            return idr;
       }
          
       
         public string SaveTrans(CreateModel2 row, Dictionary<string, string> objidsdict,Dictionary<string, string> objidsdict2, JArray locationslist,Dictionary<string,string> movprof,Dictionary<string,string> userdict)
        {
            string idr = "";
              
            try
            {
                string heads1 = "";
                string folio = row.model.TM_CONSEC;
                folio = generatefolio(folio);
                string locations= row.model.TM_ID_UBICA;// row.ub_id_ubicacion;
                string desclocation = row.model.TM_DESC_UBICA;
                string numserie = row.model.TM_SERIES;
                string conjunto = row.model.TM_ID_CONUNTO;
                string conjuntodesc = row.model.TM_DESC_CONJUNTO;
                string namelocation = row.model.TM_DESC_UBICA;
                string createuser = row.model.TM_USUARIO_CREACION;
                string statustype = row.model.TM_STATUS;
                string typemov = row.model.TM_TIPO_MOVTO;
                string marca = row.model.TM_MARCAS;
                string modelo = row.model.TM_MODELOS;
                string desctypemov = row.model.TM_DESCRIPCION;
                string depto = row.model.TM_ID_DEPTO;
                string descdepto = row.model.TM_DESC_DEPTO;
                string numrecp=row.model.TM_NUMS_RECEPCION;
                string numsolicitud=row.model.TM_NUMS_RECEPCION;
                string quantity=row.model.TM_CANTIDADES;
                string label=row.model.TM_TIPOS_ETIQUETADO;
                string data_aproved=row.model.TM_FECHA_AUTORIZADO;
                string denynote=row.model.TM_RECHAZO_OBSERVACIONES;
                string deniers=row.model.TM2_RECHAZADO_POR;
                string denydate=row.model.TM2_FECHA_RECHAZO;

                string tipomov=row.model.TM_TIPO_MOVTO;
               /* try
                {
                    tipomov = movprof[tipomov];
                }
                catch { }*/
              
                //tranferencia
                string conjuntodest=row.model.TM_ID_CONJUNTO_DEST;
                string conjuntdestinydesc=row.model.TM_DESC_CONJUNTO_DEST;
                string locationsdest=row.model.TM_ID_UBICA_DEST;
                string locationdestinydesc=row.model.TM_DESC_UBICA_DEST;
                string reader=row.model.TM_NUM_ESCRITURA;
                string convenio=row.model.TM_NUM_CONVENIO;
                string quantity_new=row.model.TM_cantidades_mover;
                List<string> autos=new List<string>();
                List<string> autosdate=new List<string>();
                autos.Add(row.model.TM_AUT_01);
                autos.Add(row.model.TM_AUT_02);
                autos.Add(row.model.TM_AUT_03);
                autos.Add(row.model.TM_AUT_04);
                autos.Add(row.model.TM_AUT_05);
                autos.Add(row.model.TM_AUT_06);
                autos.Add(row.model.TM_AUT_07);
                //autos.Add(row.model.TM_AUT_08);
                autosdate.Add(row.model.TM_AUT_01_FECHA);
                 autosdate.Add(row.model.TM_AUT_02_FECHA);
                  autosdate.Add(row.model.TM_AUT_03_FECHA);
                 autosdate.Add(row.model.TM_AUT_04_FECHA);
                 autosdate.Add(row.model.TM_AUT_05_FECHA);
                 autosdate.Add(row.model.TM_AUT_06_FECHA);
                 autosdate.Add(row.model.TM_AUT_07_FECHA);
                // autosdate.Add(row.model.TM_AUT_08_FECHA);

               

                //jarray authorizations 
                
                JArray authorizations=new JArray();
                bool flag=true;
                int i=0;
                while(flag){
                    try{
                      JObject autojo=new JObject();

                      if (autos.ElementAt(i) == "")
                      {
                          i++;
                          continue;
                      }
                    string user=autos.ElementAt(i).Split('|').First();
                       
                    string approved=autos.ElementAt(i).Split('|').Last();
                    string date1=autosdate.ElementAt(i);
                    string idus = "";
                    try
                    {
                        if (userdict.TryGetValue(user.ToLower(), out idus) || userdict.TryGetValue(user, out idus))
                        {
                            
                        }
                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(user.ToLower()))
                                {
                                    idus = userdi.Key;
                                }
                            }
                            catch { }
                        }
                         
                    }
                    catch
                    {

                    }
                     autojo.Add("id_auto","null");
                     idus = (idus == null) ? "" : idus;
                     autojo.Add("id_user", idus);
                     autojo.Add("user",user);
                    autojo.Add("name",user);
                    autojo.Add("lastname", user);
                     autojo.Add("approved",approved);
                     if (date1.Length > 0)
                     {
                         date1 = convertToDate(date1);
                     }
                     autojo.Add("date",date1);

                   authorizations.Add(autojo);
                    }catch{

                    }

                    i++;
                    if(i==8)
                    flag=false;
                   }
                 // in object array
                string pricedate = row.model.TM_FECHAS_COMPRA;
                string price = row.model.TM_PRECIOS_COMPRA;
                string numpedido = row.model.TM_NUMS_PEDIDO;
                string referenceobj = "null";
                string referenceobjdesc = row.model.TM_DESC_ARTICULOS;
                string referenceobjid = row.model.TM_ID_ARTICULOS;
                string idobjreal = row.model.TM_ID_UNICOS; //id_registro 

                List<string> idsref = new List<string>();
                List<string> idsrefdesc = new List<string>();
                List<string> idsobjreal = new List<string>();
                List<string> pricelist = new List<string>();
                List<string> quantitylist = new List<string>();
                List<string> quantitynewlist = new List<string>();
                List<string> marcalist = new List<string>();
                List<string> modelolist = new List<string>();
                List<string> serielist = new List<string>();
                Dictionary<string,string> idrefdict=new Dictionary<string,string>();
                Dictionary<string,string> idrefdescdict=new Dictionary<string,string>();
                Dictionary<string,string> idobjrealdict=new Dictionary<string,string>();
                Dictionary<string,string> pricedict=new Dictionary<string,string>();
                try
                {
                    idsref = referenceobjid.Split('|').ToList();
                }
                catch { }
                try
                {
                    marcalist = marca.Split('|').ToList();
                }
                catch { }
                try
                {
                   modelolist = modelo.Split('|').ToList();
                }
                catch { }
                try
                {
                    quantitylist = quantity.Split('|').ToList();
                }
                catch { }
                try
                {
                    quantitynewlist = quantity_new.Split('|').ToList();
                }
                catch { }
                try
                {
                    serielist = numserie.Split('|').ToList();
                }
                catch { }
                    try
                {
                    idsobjreal = idobjreal.Split('|').ToList();
                }
                catch { }
                    try
                    {
                        pricelist = price.Split('|').ToList();
                    }
                    catch { }
                //get id real by id_registro
                    int countd = 0;
                    foreach (string obj in idsobjreal)
                    {
                        try
                        {
                            idobjrealdict.Add(countd.ToString(), (from objid in objidsdict2 where objid.Value == obj select (string)objid.Key).First().ToString());
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                idobjrealdict.Add(countd.ToString(), "");
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                    countd = 0;
                    foreach (string refs in idsref)
                    {
                        try
                        {
                            idrefdict.Add(countd.ToString(), (from objid in objidsdict where objid.Value == refs select (string)objid.Key).First().ToString());
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                idrefdict.Add(countd.ToString(), "");
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                    countd = 0;
                    foreach (string refsd in idsrefdesc)
                    {
                        try
                        {
                            idrefdescdict.Add(countd.ToString().ToString(), refsd);
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                idrefdescdict.Add(countd.ToString().ToString(), "");
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                    countd = 0;
                    foreach (string price1 in pricelist)
                    {
                        try
                        {
                            string price2 = (price1.Contains("$")) ? price1 : "$" + price1;
                            pricedict.Add(countd.ToString(), price2);
                            countd++;
                        }
                        catch
                        {
                            try
                            {
                                pricedict.Add(countd.ToString(), price1);
                                countd++;
                            }
                            catch (Exception ex)
                            {
                                countd++;
                            }
                        }
                    }
                JArray objects = new JArray();

                Int64 timestamp = 20150101000000;
                string datecreated = row.model.TM_FECHA_CREACION;
                if (datecreated.Length > 0)
                {
                    datecreated = convertToDate(datecreated);
                }
                try
                {
                    if (datecreated.Length < 14)
                    {
                        datecreated = datecreated.Replace(" ", "");
                        datecreated = datecreated + " 00:00:00";
                    }
                    DateTime dateCreated2 = DateTime.ParseExact(datecreated, "dd/MM/yyyy HH:mm:ss", null);
                    timestamp = Convert.ToInt64(dateCreated2.ToString("yyyyMMddHHmmss"));
                }
                catch
                {

                }
                try
                {
                    string locationf = "";
                    string conjuntoid = "";
                    try
                    {
                       // conjunto = (from conj in locationslist where (string)conj["number"] == conjunto select (string)conj["_id"]).First().ToString();
                    }
                    catch
                    {
                        // locationf = "";
                    }
                    try
                    {
                        // JArray getlocations = (from loc in locationslist where (string)loc["parent"] == conjuntoid select loc) as JArray;
                       // locationf = (from conj in locationslist where (string)conj["number"] == locations select (string)conj["_id"]).First().ToString();
                        locationf = locations;
                    }
                    catch
                    {

                    }

                    locations = (locationf != "") ? locationf : "null";
                }
                catch { }
                  try
                {
                    string locationfdest = "";
                    string conjuntoiddest = "";
                    try
                    {
                       // conjuntodest = (from conjd in locationslist where (string)conjd["number"] == conjuntodest select (string)conjd["_id"]).First().ToString();
                      
                    }
                    catch
                    {
                        // locationf = "";
                    }
                    try
                    {
                        // JArray getlocations = (from loc in locationslist where (string)loc["parent"] == conjuntoid select loc) as JArray;
                       // locationfdest = (from conjd in locationslist where (string)conjd["number"] == locationsdest select (string)conjd["_id"]).First().ToString();
                        locationfdest = locationsdest;
                    }
                    catch
                    {

                    }

                    locationsdest = (locationfdest != "") ? locationfdest : "null";
                }
                catch { }
                JObject objectJo = new JObject();
                JArray objreals = new JArray();
                List<string> conjuntolist = new List<string>();
                List<string> conjuntodesclist = new List<string>();
                List<string> locationclist = new List<string>();
                List<string> locationdesclist = new List<string>();
                List<string> conjuntodestinylist = new List<string>();
                List<string> conjuntodestinydesclist = new List<string>();
                List<string> locationcdestinylist = new List<string>();
                List<string> locationdestinydesclist = new List<string>();
                try
                {
                    conjuntolist = conjunto.Split('|').ToList();
                }
                catch { }
                try
                {
                    conjuntodesclist = conjuntodesc.Split('|').ToList();
                }
                catch { }
                try
                {
                    locationclist = locations.Split('|').ToList();
                }
                catch { }
                try
                {
                    locationdesclist = namelocation.Split('|').ToList();
                }
                catch { }
                try
                {
                    conjuntodestinylist = conjuntodest.Split('|').ToList();
                }
                catch { }
                try
                {
                    conjuntodestinydesclist = conjuntdestinydesc.Split('|').ToList();
                }
                catch { }
               try
                {
                    locationcdestinylist = locationsdest.Split('|').ToList();
                }
                catch { }
                 try
                {
                    locationdestinydesclist = locationdestinydesc.Split('|').ToList();
                }
                catch { }
                for (int x = 0; x < idobjrealdict.Count(); x++)
                {
                    try
                    {
                        JObject objectJoreal = new JObject();
                        try
                        {
                            objectJoreal.Add("id", idobjrealdict.ElementAt(x).Value);
                        }
                        catch { }
                        try
                        {
                            objectJoreal.Add("objectReference", idrefdict.ElementAt(x).Value);
                        }
                        catch { }
                     
                        try
                        {
                            objectJoreal.Add("price", pricedict.ElementAt(x).Value);
                        }
                        catch { objectJoreal.Add("price", ""); }
                      
                       objectJoreal.Add("date", datecreated);
                       label = (label.ToLower() == "n") ? "normal" : "No Etiquetado";
                        objectJoreal.Add("label", label);
                        objectJoreal.Add("department", depto);
                        objectJoreal.Add("departmentText", descdepto);
                         try
                        {
                            if (conjuntolist.Count() == 1)
                            {
                                objectJoreal.Add("conjunto", conjunto);
                            }
                            else
                            {
                                objectJoreal.Add("conjunto", conjuntolist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjunto",conjunto); }
                        try
                        {
                            if (conjuntodesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoText", conjuntodesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoText", conjuntodesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoText", conjuntodesc); }

                        try
                        {
                            if (locationclist.Count() == 1)
                            {
                                objectJoreal.Add("location", locations);
                            }
                            else
                            {
                                objectJoreal.Add("location", locationclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("location", locations); }
                        try
                        {
                            if (locationdesclist.Count() == 1)
                            {
                                objectJoreal.Add("locationText", namelocation);
                            }
                            else
                            {
                                objectJoreal.Add("locationText", locationdesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("locationText", namelocation); }
                         try
                        {
                            if (conjuntodestinylist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodest);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestiny", conjuntodestinylist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestiny",conjuntodest); }
                        try
                        {
                            if (conjuntodestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("conjuntoDestinyText", conjuntodestinydesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("conjuntoDestinyText", conjuntdestinydesc); }

                        try
                        {
                            if (locationcdestinylist.Count() == 1)
                            {
                                objectJoreal.Add("locationDestiny", locationsdest);
                            }
                            else
                            {
                                objectJoreal.Add("locationDestiny", locationcdestinylist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("locationDestiny", locationsdest); }
                        try
                        {
                            if (locationdestinydesclist.Count() == 1)
                            {
                                objectJoreal.Add("locationDestinyText", locationdestinydesc);
                            }
                            else
                            {
                                objectJoreal.Add("locationDestinyText", locationdestinydesclist.ElementAt(x));
                            }
                        }
                        catch { objectJoreal.Add("locationDestinyText", locationdestinydesc); }
                        
                      
                        
                        try
                        {
                            objectJoreal.Add("serie", serielist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("serie", ""); }
                        try
                        {
                            objectJoreal.Add("quantity", quantitylist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("quantity", ""); }
                        try
                        {
                            objectJoreal.Add("quantity_new", quantitynewlist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("quantity_new", ""); }
                        try
                        {
                            objectJoreal.Add("marca", marcalist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("marca", ""); }
                        try
                        {
                            objectJoreal.Add("modelo", modelolist.ElementAt(x));
                        }
                        catch { objectJoreal.Add("modelo", ""); }
                       
                        try
                        {
                            objectJoreal.Add("object_id", idsobjreal.ElementAt(x));
                        }
                        catch { }
                        objreals.Add(objectJoreal);
                    }
                    catch
                    {

                    }
                }

                    switch (statustype)
                    {
                        case "A":
                            statustype = "5";
                            break;
                        case "C":
                            statustype = "6";
                            break;
                        case "R":
                            statustype = "7";
                            break;
                        default:
                            statustype = "5";
                            break;
                    }

                    string voboinfo = row.model.TM_AUT_08;
                   string approvedvobo = voboinfo.Split('|').Last();
                    string voboid = "";
                    string namevobo = "";

                    string fechavobo = row.model.TM_AUT_08_FECHA;
                    if (fechavobo.Length > 0)
                    {
                        fechavobo = convertToDate(fechavobo);
                    }
                    int approvedstatus = (approvedvobo=="1") ? 1 : 0;
                    try
                    {
                        namevobo = voboinfo.Split('|').First();
                        if (userdict.TryGetValue(namevobo.ToLower(), out voboid) || userdict.TryGetValue(namevobo, out voboid)) { }
                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(namevobo.ToLower()))
                                {
                                    voboid = userdi.Key;
                                }
                            }
                            catch { }
                        }
                         
                    }
                    catch
                    {

                    }

                    JArray approval = new JArray();
                    try
                    {
                        JObject vobo = new JObject();
                        if (row.model.TM_AUT_08 != "")
                        {
                            vobo.Add("id_auto", "null");
                            voboid = (voboid == null) ? "" : voboid;
                            vobo.Add("id_user", voboid);
                            vobo.Add("user", voboinfo.Split('|').First());
                            vobo.Add("name", voboinfo.Split('|').First());
                            vobo.Add("lastname", namevobo);
                            vobo.Add("date", fechavobo);
                            vobo.Add("approved", approvedstatus);
                            approval.Add(vobo);
                        }


                    }
                    catch { }
                    string idusercreator = "";
                    try
                    {
                        
                        foreach (var userdi in userdict)
                        {
                            try
                            {
                                if (userdi.Value.ToLower().Contains(createuser.ToLower()))
                                {
                                    idusercreator = userdi.Key;
                                }
                            }
                            catch { }
                        }

                    }
                    catch
                    {

                    }
                objectJo.Add("folio", folio);
                objectJo.Add("objects", objreals);
                objectJo.Add("authorizations", authorizations);
                objectJo.Add("notifications", new JObject());
                objectJo.Add("approval", approval);
                objectJo.Add("status", Convert.ToInt16(statustype));
                objectJo.Add("movement", tipomov);
                objectJo.Add("movementFields", new JObject());
                objectJo.Add("extras", new JObject());
                objectJo.Add("profileFields", new JObject());
                objectJo.Add("CreatedDate", datecreated);
                objectJo.Add("location", locations);
                objectJo.Add("locationText", desclocation);
                objectJo.Add("conjuntoText",conjuntodesc);
                objectJo.Add("conjunto", conjunto);
                objectJo.Add("locationDestiny", locationsdest);
                objectJo.Add("locationDestinyText", locationdestinydesc);
                objectJo.Add("conjuntoDestinyText", conjuntdestinydesc);
                objectJo.Add("conjuntoDestiny", conjuntodest);
                objectJo.Add("tiposolicitud", row.model.TM_TIPO_MOVTO);
                objectJo.Add("tipomovimiento", row.model.TM_DESCRIPCION);
                objectJo.Add("Creatorname", createuser);
                objectJo.Add("temporal", false);
                objectJo.Add("Creator", idusercreator);
                objectJo.Add("CreatedTimeStamp",timestamp);
                if (data_aproved.Length > 2)
                {
                    if (data_aproved.Length > 0)
                    {
                        data_aproved = convertToDate(data_aproved);
                    }
                    objectJo.Add("AuthorizedDate", data_aproved);
                }
                if (denydate.Length > 2)
                {
                    if (denydate.Length > 0)
                    {
                        denydate = convertToDate(denydate);
                    }
                    objectJo.Add("DenyDate", denydate);
                }
                if (deniers.Length > 2)
                {
                    objectJo.Add("Deniers", deniers);
                }
                // objectJo.Add("id_articulo", row.ID_REGISTRO);
                objectJo.Add("ApprovedDate", "");

                string item = JsonConvert.SerializeObject(objectJo);
               
              

                try
                {
                    idr = demanddesktopdb.SaveRow(item);
                   
                   
                      

                }
                catch { }


                bool ok = true;
            }
            catch
            {  }

            return idr;
       }
       
        
        public String generateObjs(List<CreateModel2> rows,string type)
        {

            
            try
            {
               
                Task<string> task1 = Task<string>.Factory.StartNew(() => locationsdb.GetRows());
                Task<string> task2 = Task<string>.Factory.StartNew(() => objectrealdb.GetRows());
                Task<string> task3 = Task<string>.Factory.StartNew(() => usersdb.GetRows());
                
                List<string> listselects = new List<string>();
                List<dynamic> newObjs = new List<dynamic>();
                List<dynamic> editEpcs = new List<dynamic>();
                List<dynamic> editIds = new List<dynamic>();
               
                JObject datajo = new JObject();

                
                List<string> Epcs = new List<string>();
                List<string> Idslist = new List<string>();
                List<string> Idslist2 = new List<string>();
                List<string> objectsid = new List<string>();
                List<string> idusers = new List<string>();
                List<string> objectsid2 = new List<string>();
                Dictionary<string,string> objidsdict = new Dictionary<string,string>();
                Dictionary<string, string> objidsdict2 = new Dictionary<string, string>();
                Dictionary<string, string> userdict = new Dictionary<string, string>();
                Dictionary<string, string> assets = new Dictionary<string, string>();
                Dictionary<string,string> movprof=new Dictionary<string,string>();
                JArray movprofiles = new JArray();
                try
                {
                    movprofiles = JsonConvert.DeserializeObject<JArray>(movprofilesdb.GetRows());

                    foreach (JObject mov in movprofiles)
                    {
                        try
                        {
                            string value;
                            if (mov["name"].ToString().ToLower().Contains("alta"))
                            {

                                if (!movprof.TryGetValue("1", out value))
                                    movprof.Add("1", mov["_id"].ToString());
                            }
                            if (mov["name"].ToString().ToLower().Contains("baja"))
                            {

                                if (!movprof.TryGetValue("2", out value))
                                {
                                    movprof.Add("2", mov["_id"].ToString());
                                    movprof.Add("7", mov["_id"].ToString());
                                    movprof.Add("8", mov["_id"].ToString());
                                }
                            }
                            if (mov["name"].ToString().ToLower().Contains("transferencia"))
                            {

                                if (!movprof.TryGetValue("3", out value))
                                    movprof.Add("3", mov["_id"].ToString());
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
                try
                {
                    objectsid = (from row in rows.Cast<dynamic>() select (string)row.model.TM_ID_ARTICULOS).ToList();
                    JArray objects = JsonConvert.DeserializeObject<JArray>(objectrealdb.GetbyCustom("object_id", objectsid, "ReferenceObjects"));

                   // objidsdict = (from obj in objects select new { id = (string)obj["_id"], object_id = (string)obj["object_id"] }).ToList();
                    //  rows.Cast<dynamic>().Where(c => Epcs.Contains(c.af_epc_completo)).ToList().ForEach(cc => cc.analisis = "Activo con Epc ya Existente");
                    
                   // objidsdict = objects.ToDictionary(x => (string)x["_id"], x => (string)x["object_id"]);
                    foreach (JObject objid in objects)
                    {
                        try
                        {
                            objidsdict.Add((string)objid["_id"], (string)objid["object_id"]);
                        }
                        catch { objidsdict.Add((string)objid["_id"], ""); }
                        try
                        {
                            assets.Add((string)objid["_id"], (string)objid["assetType"]);
                        }
                        catch { assets.Add((string)objid["_id"], ""); }
                    }
                }
                catch { }
               
               

                try
                {

                    int countobjs = 0;
                    JObject idprof = new JObject();
                    try
                    {
                        String idslocs = locationsProfilesdb.Get("name", "Ubicacion");
                        idprof = JsonConvert.DeserializeObject<JArray>(idslocs).First() as JObject;
                                               
                    }
                    catch { }
                    int total = rows.Count();
                    JObject data=new JObject();
                    string iduser="";
                        try
                    {
                        iduser = Session["_id"].ToString();
                    }
                    catch (Exception e)
                    {
                        iduser = Request.Cookies["_id2"].Value;
                    }
                    data.Add("index","0");
                    data.Add("total",total.ToString());
                    data.Add("progress", "0");
                    data.Add("iduser", iduser);
                    data.Add("timer", "0");
                    int count=1;
                   
                    int indexx = 0;
                   // decimal porcent = total / 100;
                   // int lote = Convert.ToInt16(Math.Round(porcent, MidpointRounding.ToEven));

                    JArray locationslist = new JArray();
                    Task.WaitAll(task1,task2,task3);
                    try
                    {
                       
                        string locat = task1.Result;
                        locationslist = JsonConvert.DeserializeObject<JArray>(locat);
                    }
                    catch { }
                    try
                    {
                        // Idslist = (from row in rows.Cast<dynamic>() select (string)row.model.ID_REGISTRO).ToList();
                        JArray objects = JsonConvert.DeserializeObject<JArray>(task2.Result);
                        Idslist.Clear();
                        Idslist = (from obj in objects select (string)obj["id_registro"]).ToList();
                        // rows.Cast<dynamic>().Where(c => Idslist.Contains(c.id_registro)).ToList().ForEach(cc => cc.analisis = "Activo con Id de Registro ya Existente");
                        objidsdict2 = objects.ToDictionary(x => (string)x["_id"], x => (string)x["id_registro"]);

                    }
                    catch { }
                    try
                    {
                       
                        JArray objectsuser = JsonConvert.DeserializeObject<JArray>(task3.Result);

                        // rows.Cast<dynamic>().Where(c => Idslist.Contains(c.id_registro)).ToList().ForEach(cc => cc.analisis = "Activo con Id de Registro ya Existente");
                        userdict = objectsuser.ToDictionary(x => (string)x["_id"], x => (string)x["user"]);

                    }
                    catch { }
                    decimal totalm = Convert.ToDecimal(total);

                    System.Timers.Timer timer = new System.Timers.Timer();
                    countimer = 0;
                    timer.Elapsed += new ElapsedEventHandler(timerEvent);
                    timer.Interval = 10;
                    int flag = 0;
                    foreach (dynamic row in rows)
                    {
                        try
                        {
                            if (flag==0)
                            {
                                timer.Start();
                                flag = 1;
                            }




                            if (flag != 2)
                            {
                                decimal auxindex = Convert.ToDecimal(indexx);

                                decimal progress = Convert.ToDecimal((auxindex / totalm) * 100);
                                int lote = Convert.ToInt16(Math.Round(progress, MidpointRounding.ToEven));

                                if (Convert.ToDecimal(lote) <= progress)
                                {


                                    if (count < lote && flag == 1)
                                    {
                                        timer.Stop();
                                        data["index"] = indexx.ToString();
                                        data["progress"] = lote.ToString();
                                        data["timer"] = countimer.ToString();
                                        push.pushMessage(JsonConvert.SerializeObject(data), "progress");
                                        count = lote;
                                        flag = 2;
                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                       //count++;
                      
                            try
                            {
                                if (type == "1")
                                {
                                    if (row.model.TM_TIPO_MOVTO == "1")
                                    {
                                        string id = SaveAlta(row, objidsdict, objidsdict2, locationslist, movprof, userdict);
                                    }


                                    if (row.model.TM_TIPO_MOVTO == "2" || row.model.TM_TIPO_MOVTO == "7" || row.model.TM_TIPO_MOVTO == "8")
                                    {
                                        string id = SaveBaja(row, objidsdict, objidsdict2, locationslist, movprof, userdict);
                                    }
                                    if (row.model.TM_TIPO_MOVTO == "3")
                                    {
                                        string id = SaveTrans(row, objidsdict, objidsdict2, locationslist, movprof, userdict);
                                    }

                                }
                                else
                                {
                                    string id = SaveTemp(row, objidsdict, objidsdict2, locationslist, movprof, userdict);
                             
                                }
                            }
                            catch {

                               // string id = SaveTemp(row, objidsdict, objidsdict2, locationslist, movprof, userdict);
                                
                            }
                        
                        countobjs++;

                    }

                  //  Notificationsclass.saveNotification("Objects", "Create", "Se han creado " + countobjs + " Activos");



                }
                catch (Exception ex)
                {

                }


                //  modifyObjs(url);
                ViewData["news"] = newObjs;
                ViewData["epcs"] = editEpcs;
                ViewData["ids"] = editIds;
                return "Migración Correcta";



            }
            catch (Exception ex)
            {

                return ex.ToString() ;
            }

        }
        public static SharedStringItem GetSharedStringItemById(WorkbookPart workbookPart, Int32 id)
        {
            return workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
        }

    }
      public class CreateModel2 
        {
          public dynamic model; 
          public CreateModel2(int type){

              if (type == 1)
                  generateModel();
              else
                  generateModel1();
          }
          protected void generateModel()
          {
              try
              {
                  this.model = new ExpandoObject();

                  this.model.TM_CONSEC = "";
                  this.model.TM_TIPO_MOVTO = "";
                  this.model.TM_DESCRIPCION = "";
                  this.model.TM_FECHA_CREACION = "";
                  this.model.TM_USUARIO_CREACION = "";
                  this.model.TM_STATUS = "";
                  this.model.TM_ID_CONUNTO = "";
                  this.model.TM_DESC_CONJUNTO = "";
                  this.model.TM_ID_DEPTO = "";
                  this.model.TM_DESC_DEPTO = "";
                  this.model.TM_ID_UBICA = "";
                  this.model.TM_DESC_UBICA = "";
                  this.model.TM_ID_ARTICULOS = "";
                  this.model.TM_DESC_ARTICULOS = "";
                  this.model.TM_ID_UNICOS = "";
                  this.model.TM_MARCAS = "";
                  this.model.TM_MODELOS = "";
                  this.model.TM_SERIES = "";
                  this.model.TM_FECHAS_COMPRA = "";
                  this.model.TM_PRECIOS_COMPRA = "";
                  this.model.TM_NUMS_PEDIDO = "";
                  this.model.TM_NUMS_RECEPCION = "";
                  this.model.TM_NUMS_SOLICITUD = "";
                  this.model.TM_CANTIDADES = "";
                  this.model.TM_TIPOS_ETIQUETADO= "";
                  this.model.TM_ID_CONJUNTO_DEST = "";
                  this.model.TM_DESC_CONJUNTO_DEST = "";
                  this.model.TM_ID_DEPTO_DEST = "";
                  this.model.TM_DESC_DEPTO_DEST = "";
                  this.model.TM_ID_UBICA_DEST = "";
                  this.model.TM_DESC_UBICA_DEST = "";
                  this.model.TM_NUM_ESCRITURA = "";
                  this.model.TM_NUM_CONVENIO = "";
                  this.model.TM_AUT_JI = "";
                  this.model.TM_AUT_01 = "";
                  this.model.TM_AUT_02 = "";
                  this.model.TM_AUT_03 = "";
                  this.model.TM_AUT_04 = "";
                  this.model.TM_AUT_05 = "";
                  this.model.TM_AUT_06 = "";
                  this.model.TM_AUT_07 = "";
                  this.model.TM_AUT_08 = "";
                  this.model.TM_AUT_JI_FECHA = "";
                  this.model.TM_AUT_01_FECHA = "";
                  this.model.TM_AUT_02_FECHA = "";
                  this.model.TM_AUT_03_FECHA = "";
                  this.model.TM_AUT_04_FECHA = "";
                  this.model.TM_AUT_05_FECHA = "";
                  this.model.TM_AUT_06_FECHA = "";
                  this.model.TM_AUT_07_FECHA = "";
                  this.model.TM_AUT_08_FECHA = "";
                  this.model.TM_FECHA_LIBERACION = "";
                  this.model.TM_USUARIO_LIBERACION = "";
                  this.model.TM_OBSERVACIONES = "";
                  this.model.TM_AUTORIZADOR = "";
                  this.model.TM_DOCTO_FOLIO = "";
                  this.model.TM_AUTORIZADO = "";
                  this.model.TM_FECHA_AUTORIZADO = "";
                  this.model.TM_SOLICITUD_CORRECTA = "";
                  this.model.TM_ACTIVOS_INCORRECTOS = "";
                  this.model.TM_ACTINC_OBS = "";
                  this.model.TM_TIPOS_OBSOLEC = "";
                  this.model.TM_RECHAZO_OBSERVACIONES = "";
                  this.model.TM_cantidades_mover = "";
                  this.model.TM2_TIPO_BAJA = "";
                  this.model.TM2_DESTINO_MOVTO = "";
                  this.model.TM2_PRECIO_SUGERIDO_VENTA = "";
                  this.model.TM2_COMPRADOR = "";
                  this.model.TM2_BENEFICIARIO_DONACION = "";
                  this.model.TM2_BENEFICIARIO_RFC = "";
                  this.model.TM2_DESTRUCC_SAT_NOTARIO = "";
                  this.model.TM2_BAJA_PLANEADA_FOLIO_ACTA = "";
                  this.model.TM2_VALOR_CONTABLE_ACTIVO = "";
                  this.model.TM2_SELECC_ACTA_SAT = "";
                  this.model.TM2_ADICIONALES_COMPLETOS = "";
                  this.model.TM2_DOCTOS_COMPLETOS = "";
                  this.model.TM2_AUTORIZADORES_COMPLETOS = "";
                  this.model.TM2_RECHAZADO_POR = "";
                  this.model.TM2_FECHA_RECHAZO = "";
                  this.model.TM3_SALIDA_TEMPORAL = "";


              }
              catch
              {


              }
          }
          protected void generateModel1()
          {
              try
              {
                  this.model = new ExpandoObject();

                this.model.ID_SOLICITUD="";	
                this.model.ID_CONJUNTO="";		
                this.model.DESC_CONJUNTO="";		
                this.model.ID_GERENTE="";		
                this.model.DESC_GERENTE="";	
                this.model.ID_REPORTE_EN_MAXIMO	="";	
                this.model.FECHA_SALIDA_CONJUNTO="";		
                this.model.NUMERO_GUIA_ENVIO="";		
                this.model.VD_NOMBRE="";		
                this.model.VD_PUESTO="";		
                this.model.SR_NOMBRE="";		
                this.model.SR_FIRMA	="";	
                this.model.VOBO_NOMBRE="";		
                this.model.VOBO_FIRMA="";	
                this.model.VOBO_ID="";		
                this.model.VOBO_AUTORIZADO="";	
                this.model.FECHA_SOLICITUD="";		
                this.model.USUARIO_SOLICITUD="";		
                this.model.VOBO_FECHA_AUT="";		
                this.model.REPARADO	="";	
                this.model.NO_REPARADO="";		
                this.model.NO_REP_OBSERVACIONES="";		
                this.model.REVISA_NOMBRE="";		
                this.model.REVISA_EMPRESA="";
                this.model.FECHA_ENVIO = "";	
                this.model.FECHA_REINGRESO	="";	
                this.model.REINGRESO_NOMBRE	="";	
                this.model.REINGRESO_FIRMA="";		
                this.model.STATUS_SOLICITUD	="";	
                this.model.S_ID_ACTIVO1	="";	
                this.model.S_CANTIDAD1="";	
                this.model.S_DESCRIPCION1="";		
                this.model.S_MARCA1	="";	
                this.model.S_MODELO1="";		
                this.model.S_SERIE1="";		
                this.model.S_EPC1="";		
                this.model.S_ID_ACTIVO2	="";	
                this.model.S_CANTIDAD2="";		
                this.model.S_DESCRIPCION2="";		
                this.model.S_MARCA2	="";	
                this.model.S_MODELO2="";		
                this.model.S_SERIE2="";		
                this.model.S_EPC2	="";	
                this.model.S_ID_ACTIVO3	="";	
                this.model.S_CANTIDAD3	="";	
                this.model.S_DESCRIPCION3	="";	
                this.model.S_MARCA3	="";	
                this.model.S_MODELO3="";		
                this.model.S_SERIE3	="";	
                this.model.S_EPC3="";		
                this.model.S_ID_ACTIVO4="";		
                this.model.S_CANTIDAD4	="";	
                this.model.S_DESCRIPCION4	="";	
                this.model.S_MARCA4	="";	
                this.model.S_MODELO4="";		
                this.model.S_SERIE4	="";	
                this.model.S_EPC4="";	
                this.model.S_ID_ACTIVO5="";		
                this.model.S_CANTIDAD5="";		
                this.model.S_DESCRIPCION5="";	
                this.model.S_MARCA5	="";	
                this.model.S_MODELO5="";		
                this.model.S_SERIE5	="";	
                this.model.S_EPC5="";		
                this.model.R_ID_ACTIVO1="";		
                this.model.R_CANTIDAD1="";		
                this.model.R_DESCRIPCION1="";		
                this.model.R_MARCA1="";	
                this.model.R_MODELO1="";		
                this.model.R_SERIE1="";	
                this.model.R_EPC1="";	
                this.model.R_REGRESO1="";		
                this.model.R_ID_ACTIVO2="";	
                this.model.R_CANTIDAD2="";	
                this.model.R_DESCRIPCION2="";		
                this.model.R_MARCA2="";		
                this.model.R_MODELO2="";		
                this.model.R_SERIE2="";		
                this.model.R_EPC2="";		
                this.model.R_REGRESO2="";		
                this.model.R_ID_ACTIVO3	="";	
                this.model.R_CANTIDAD3="";		
                this.model.R_DESCRIPCION3="";		
                this.model.R_MARCA3="";	
                this.model.R_MODELO3="";		
                this.model.R_SERIE3="";	
                this.model.R_EPC3="";		
                this.model.R_REGRESO3="";		
                this.model.R_ID_ACTIVO4="";	
                this.model.R_CANTIDAD4="";		
                this.model.R_DESCRIPCION4="";		
                this.model.R_MARCA4="";	
                this.model.R_MODELO4="";		
                this.model.R_SERIE4="";		
                this.model.R_EPC4="";	
                this.model.R_REGRESO4="";		
                this.model.R_ID_ACTIVO5="";	
                this.model.R_CANTIDAD5="";		
                this.model.R_DESCRIPCION5="";		
                this.model.R_MARCA5="";		
                this.model.R_MODELO5="";		
                this.model.R_SERIE5="";	
                this.model.R_EPC5="";	
                this.model.R_REGRESO5="";		
                this.model.MOVTOS_ADICIONALES="";	


                 
              }
              catch
              {


              }
          }
          public void setValue(int index, String Value)
          {

              try{
                  int auxindex=0;
                  
                  foreach(dynamic item in this.model){
                      if(index==auxindex){
                        ((IDictionary<string,object>)this.model)[item.Key]=Value;
                        break;
                      }
                      auxindex++;
                  }
                     
                  
               }catch{

               }
          }
          public dynamic getValue(int index)
          {

              try
              {
                  int auxindex = 0;

                  foreach (dynamic item in model)
                  {
                      if (index == auxindex)
                      {
                        return ((IDictionary<string, object>)model)[item.Key];
                          
                      }
                      auxindex++;
                  }

                  return null;
              }
              catch
              {
                  return null;
              }
          }
        }
}
