using System.Web.Mvc;

namespace RivkaAreas.ManageSdf
{
    public class ManageSdfAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ManageSdf";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ManageSdf_default",
                "ManageSdf/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
