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
    public partial class requestSignatureFromDocument : System.Web.UI.Page
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



            #region Document
            Document document=new Document();
            document.documentId="1";
            document.name="test_document.pdf";

            DocDetail docDetail = new DocDetail();
            docDetail.document = document;
            #endregion

            #region RecipientCreation
            SignHere objSignHere = new SignHere();
            objSignHere.xPosition = "100";
            objSignHere.yPosition = "100";
            objSignHere.documentId = "1";
            objSignHere.pageNumber = "1";

            SignHereTab objSignHereTab = new SignHereTab();
            objSignHereTab.signHere = objSignHere;

            Tab objTab = new Tab();
            objTab.signHereTabs = objSignHereTab;

            signer objSigner=new signer();
            objSigner.recipientId = "1";
            objSigner.name = "jay";
            objSigner.email = "jay3dec@gmail.com";
            objSigner.tabs = objTab;

            List<signer> signerList = new List<signer>();
            signerList.Add(objSigner);

            Recipients recip=new Recipients();
            recip.signers = signerList;

            #endregion

            envelopeDefinition data = new envelopeDefinition();
            data.emailSubject = "Hello";
            data.emailBlurb = "Hello";
            data.status = signatureStatus.sent;
            data.documents = docDetail;
            data.recipients = recip;
            
            string response = docusignMethods.RequestSignatureFromDocument(@"C:\Users\jay\Desktop\test_document.pdf", data);
        }
    }
}