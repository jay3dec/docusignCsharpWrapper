using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using DocusignLib;
using DocusignEntity;

namespace DocuSignApp
{
    public partial class getStatusOfEnvelopeWithRespectToDateTime : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DocusignAuth docusignObj = new DocusignAuth();
            docusignObj.UserName = ConfigurationManager.AppSettings["Username"].ToString();
            docusignObj.Password = ConfigurationManager.AppSettings["Password"].ToString();
            docusignObj.IntegratorKey = ConfigurationManager.AppSettings["IntegratorKey"].ToString();
            docusignObj.TemplateId = ConfigurationManager.AppSettings["TemplateId"].ToString();
            docusignObj.Url = ConfigurationManager.AppSettings["Url"].ToString();

            DocusignMethods docusignMethods = new DocusignMethods(docusignObj);

            string response = docusignMethods.GetEnvelopeStatus("08/02/2013 00:21", from_to_status.changed);
        }
    }
}