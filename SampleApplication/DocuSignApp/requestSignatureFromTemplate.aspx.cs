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
    public partial class _Default : System.Web.UI.Page
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

            templateRole tempRole = new templateRole();
            tempRole.accessCode = "123";
            tempRole.name = "jay";
            tempRole.role = "admin";
            tempRole.roleName = "admin";
            tempRole.email = "jay3dec@gmail.com";

            List<templateRole> tempList = new List<templateRole>();
            tempList.Add(tempRole);

            envelopeDefinition data = new envelopeDefinition();
            data.emailSubject = "Hello";
            data.emailBlurb = "Hello";
            data.status = signatureStatus.sent;
            data.templateRoles = tempList;
            data.templateId = "62AF9A9C-0794-40E1-822F-5F070151D790";

            

            string resp = docusignMethods.RequestSignatureFromTemplate(data);       
        }

        
    }
}
