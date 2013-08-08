using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using DocusignEntity;

namespace DocusignLib
{
    public class DocusignMethods
    {
        static string Username = string.Empty;
        static string Password = string.Empty;
        static string IntegratorKey = string.Empty;
        static string Url = string.Empty;
        static string TemplateId = string.Empty;
        static string envelopeUri = string.Empty;
        static string docListUri = string.Empty;
        static string errorMessage = string.Empty;
        static string authenticateStr = string.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="docusignAuth"></param>
        public DocusignMethods(DocusignAuth docusignAuth)
        {
            Username = docusignAuth.UserName;
            Password = docusignAuth.Password;
            IntegratorKey = docusignAuth.IntegratorKey;
            Url = docusignAuth.Url;
            TemplateId = docusignAuth.TemplateId;

            authenticateStr = "<DocuSignCredentials>" +
                "<Username>" + Username + "</Username>" +
                "<Password>" + Password + "</Password>" +
                "<IntegratorKey>" + IntegratorKey + "</IntegratorKey>" +
                "</DocuSignCredentials>";
        }

        /// <summary>
        /// RequestSignatureFromTemplate request signature using Template
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public string RequestSignatureFromTemplate(envelopeDefinition requestData)
        {
            string accountId = string.Empty;
            string baseURL = string.Empty;
            errorMessage = string.Empty;
            // 
            // STEP 1 - Login
            //
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }


                requestData.accountId = accountId;
                string requestBody = Helper.GetXMLFromObject(requestData);


                // append "/envelopes" to baseURL and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + "/envelopes");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.ContentLength = requestBody.Length;
                request.Accept = "application/xml";
                request.Method = "POST";
                // write the body of the request
                byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, requestBody.Length);
                dataStream.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr.Close();
                responseText = "";
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();

                return responseText;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }
        }

        /// <summary>
        /// RequestSignatureFromDocument request signature using Document
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="requestData"></param>
        /// <returns></returns>
        public string RequestSignatureFromDocument(string filePath, envelopeDefinition requestData)
        {
            string accountId = string.Empty;
            string baseURL = string.Empty;
            errorMessage = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

               

                //
                // STEP 2 - Create an Envelope with one recipient and one tab and send
                //

                

                string envDef = Helper.GetXMLFromObject(requestData);


                // read contents of document into the request stream
                FileStream fileStream = File.OpenRead(@"C:\Users\jay\Desktop\test_document.pdf");

                // build the multipart request body
                string requestBodyStart = "\r\n\r\n--BOUNDARY\r\n" +
                        "Content-Type: application/xml\r\n" +
                        "Content-Disposition: form-data\r\n" +
                        "\r\n" +
                        envDef + "\r\n\r\n--BOUNDARY\r\n" + 	// our xml formatted envelopeDefinition
                        "Content-Type: application/pdf\r\n" +
                        "Content-Disposition: file; filename=\"test_document.pdf\"; documentId=1\r\n" +
                        "\r\n";

                string requestBodyEnd = "\r\n--BOUNDARY--\r\n\r\n";

                // use baseURL value + "/envelopes" for url of this request
                request = (HttpWebRequest)WebRequest.Create(baseURL + "/envelopes");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "multipart/form-data; boundary=BOUNDARY";
                request.Accept = "application/xml";
                request.ContentLength = requestBodyStart.ToString().Length + fileStream.Length + requestBodyEnd.ToString().Length;
                request.Method = "POST";
                // write the body of the request
                byte[] bodyStart = System.Text.Encoding.UTF8.GetBytes(requestBodyStart.ToString());
                byte[] bodyEnd = System.Text.Encoding.UTF8.GetBytes(requestBodyEnd.ToString());
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(bodyStart, 0, requestBodyStart.ToString().Length);

                // Read the file contents and write them to the request stream
                byte[] buf = new byte[4096];
                int len;
                while ((len = fileStream.Read(buf, 0, 4096)) > 0)
                {
                    dataStream.Write(buf, 0, len);
                }

                dataStream.Write(bodyEnd, 0, requestBodyEnd.ToString().Length);
                dataStream.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr.Close();
                responseText = "";
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();

                return responseText;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }

        }
        
        /// <summary>
        /// GetDocusignEnvelopeInformation fetches envelope information
        /// </summary>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public string GetDocusignEnvelopeInformation(string envelopeId)
        {
            string url = Url;
            string baseURL = "";	// we will retrieve this
            string accountId = "";	// will retrieve

            envelopeUri = "/envelopes/" + envelopeId;

            errorMessage = string.Empty;

            

            // 
            // STEP 1 - Login
            //
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

              

                //
                // STEP 2 - Get Envelope Info
                //
                // use baseURL value + envelopeUri for url of this request
                request = (HttpWebRequest)WebRequest.Create(baseURL + envelopeUri);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.Method = "GET";
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr.Close();
                responseText = "";
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();

                return responseText;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += errorMessage;
                    }
                }
                return errorMessage;
            }
        }

        /// <summary>
        /// GetEnvelopeRecipientStatus fetches envelope recipient status
        /// </summary>
        /// <param name="envelopeId"></param>
        /// <param name="includeTab"></param>
        /// <returns></returns>
        public string GetEnvelopeRecipientStatus(string envelopeId,bool includeTab)
        {
            string url = Url;
            string baseURL = "";	// we will retrieve this
            string accountId = "";	// will retrieve

            

            // 
            // STEP 1 - Login
            //
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

               

                //
                // STEP 2 - Get Envelope Recipient Info
                //

                // append "/envelopes/" + envelopeId + "/recipients" to baseUrl and use in the request
                url = baseURL + "/envelopes/" + envelopeId + "/recipients?include_tabs=" + includeTab;
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.Method = "GET";
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr.Close();
                responseText = "";
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                return responseText;
               
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }
        }

        /// <summary>
        /// GetEnvelopeStatus fetches envelope status
        /// </summary>
        /// <param name="date"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public string GetEnvelopeStatus(string date,from_to_status status)
        {
            string url = Url;
            string baseURL = "";	// we will retrieve this
            string accountId = "";	// will retrieve

           

            // 
            // STEP 1 - Login
            //
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

                //--- display results
                Console.WriteLine("accountId = " + accountId + "\nbaseUrl = " + baseURL);

                //
                // STEP 2 - Get Envelope Status(es)
                //

                // Append "/envelopes" and query string to baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + "/envelopes?from_date=" + date + "&from_to_status=" + status);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr.Close();
                responseText = "";
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();

                return responseText;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }
        }

        /// <summary>
        /// GetEnvelopeDocList gets the document in envelope and downloads them
        /// </summary>
        /// <param name="envelopeId"></param>
        /// <param name="download"></param>
        /// <param name="downloadToFolder"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public List<string> GetEnvelopeDocList(string envelopeId,bool download,string downloadToFolder,out string errorMsg)
        {
            errorMsg = string.Empty;
            string baseURL = "";	// we will retrieve this
            string accountId = "";	// will retrieve
            docListUri = "/envelopes/" + envelopeId + "/documents";
            List<string> uriList = new List<string>();
            

            // 
            // STEP 1 - Login
            //
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

               

                //
                // STEP 2 - Get Envelope Document List
                //

                // append docListUri to the baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + docListUri);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.Method = "GET";
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                // grab the document uris
                
                int cnt = 0;
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "uri"))
                        {
                            uriList.Add(reader.ReadString());
                          //  Console.WriteLine("document uri {0} is:  {1}", ++cnt, uriList[uriList.Count - 1]);
                        }
                    }
                }

                //
                // STEP 3 - Download the Document(s)
                //
                if (download)
                {
                    int fileId = 1;
                    foreach (string uri in uriList)
                    {
                        // append document uris to the baseUrl
                        request = (HttpWebRequest)WebRequest.Create(baseURL + uri);
                        request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                        request.Accept = "application/pdf";
                        request.Method = "GET";
                        // read the response
                        webResponse = (HttpWebResponse)request.GetResponse();
                        using (MemoryStream ms = new MemoryStream())
                        using (FileStream outfile = new FileStream(downloadToFolder + "/document_" + fileId++ + ".pdf", FileMode.Create))
                        {
                            webResponse.GetResponseStream().CopyTo(ms);
                            if (ms.Length > int.MaxValue)
                            {
                                throw new NotSupportedException("Cannot write a file larger than 2GB.");
                            }
                            outfile.Write(ms.GetBuffer(), 0, (int)ms.Length);
                        }
                    }
                }
                
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMsg = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMsg += text;
                    }
                }
            }
            return uriList;
        }

        /// <summary>
        /// EmbedSendingUX embeds the sending UX
        /// </summary>
        /// <param name="envData"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public string EmbedSendingUX(envelopeDefinition envData,string returnUrl)
        {
            string accountId = string.Empty;
            string baseURL = string.Empty;
            string envelopeId = string.Empty;
            string uri = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

               

                //
                // STEP 2 - Create an Envelope with an Embedded Recipient
                //

                // Construct an outgoing XML request body
                

                envData.accountId = accountId;
                string requestBody = Helper.GetXMLFromObject(envData);

                // append "/envelopes" to baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + "/envelopes");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.ContentLength = requestBody.Length;
                request.Method = "POST";
                // write the body of the request
                byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, requestBody.Length);
                dataStream.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "envelopeId"))
                            envelopeId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "uri"))
                            uri = reader.ReadString();
                    }
                }

                

                //
                // STEP 3 - Get the Embedded Console Send View
                //

                // construct another outgoing XML request body
                string reqBody = "<returnUrlRequest xmlns=\"http://www.docusign.com/restapi\">" +
                    "<authenticationMethod>email</authenticationMethod>" +
                    "<email>" + Username + "</email>" +	 				// NOTE: Use different email address if username provided in non-email format!
                    "<returnUrl>" + returnUrl + "</returnUrl>" +  // username can be in email format or an actual ID string
                    "<userName>Name</userName>" +
                    "<clientUserId>1</clientUserId>" +
                    "</returnUrlRequest>";

                // append uri + "/views/sender" to the baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + uri + "/views/sender");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.ContentLength = reqBody.Length;
                request.Method = "POST";
                // write the body of the request
                byte[] body2 = System.Text.Encoding.UTF8.GetBytes(reqBody);
                Stream dataStream2 = request.GetRequestStream();
                dataStream2.Write(body2, 0, reqBody.Length);
                dataStream2.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                return responseText;
                
                
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }
        }

        /// <summary>
        /// EmbedSigningUX embeds the signing UX
        /// </summary>
        /// <param name="envData"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public string EmbedSigningUX(envelopeDefinition envData,string returnUrl)
        {
            string accountId = string.Empty;
            string baseURL = string.Empty;
            string envelopeId = string.Empty;
            string uri = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

         

                //
                // STEP 2 - Request Envelope Result
                //

                // Construct an outgoing XML request body
                

                envData.accountId = accountId;
                string requestBody = Helper.GetXMLFromObject(envData);

                // append "/envelopes" to baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + "/envelopes");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.ContentLength = requestBody.Length;
                request.Method = "POST";
                // write the body of the request
                byte[] body = System.Text.Encoding.UTF8.GetBytes(requestBody);
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, requestBody.Length);
                dataStream.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "envelopeId"))
                            envelopeId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "uri"))
                            uri = reader.ReadString();
                    }
                }

               

                //
                // STEP 3 - Get the Embedded Console Sign View
                //

                // construct another outgoing XML request body
                string reqBody = "<recipientViewRequest xmlns=\"http://www.docusign.com/restapi\">" +
                    "<authenticationMethod>email</authenticationMethod>" +
                        "<email>" + Username + "</email>" +	 	// NOTE: Use different email address if username provided in non-email format!
                        "<returnUrl>" + returnUrl + "</returnUrl>" +  // username can be in email format or an actual ID string
                        "<clientUserId>1</clientUserId>" +
                        "<userName>Name</userName>" +
                        "</recipientViewRequest>";

                // append uri + "/views/recipient" to baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + uri + "/views/recipient");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.ContentLength = reqBody.Length;
                request.Method = "POST";
                // write the body of the request
                byte[] body2 = System.Text.Encoding.UTF8.GetBytes(reqBody);
                Stream dataStream2 = request.GetRequestStream();
                dataStream2.Write(body2, 0, reqBody.Length);
                dataStream2.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                return responseText;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }
        }

        /// <summary>
        /// EmbeddedDocusignConsoleView provides a Embedded Console View
        /// </summary>
        /// <returns></returns>
        public string EmbeddedDocusignConsoleView()
        {
            string accountId = string.Empty;
            string baseURL = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.Accept = "application/xml";
                request.Method = "GET";
                HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                string responseText = sr.ReadToEnd();
                using (XmlReader reader = XmlReader.Create(new StringReader(responseText)))
                {
                    while (reader.Read())
                    {	// Parse the xml response body
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "accountId"))
                            accountId = reader.ReadString();
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "baseUrl"))
                            baseURL = reader.ReadString();
                    }
                }

                

                //
                // STEP 2 - Launch the DocuSign Console in an authenticated view.
                //

                // Construct an outgoing XML request body
                StringBuilder xml = new StringBuilder();
                xml.Append("<consoleViewRequest xmlns=\"http://www.docusign.com/restapi\">");
                xml.Append("<accountId>" + accountId + "</accountId>");
                xml.Append("</consoleViewRequest>");

                // append "/views/console" to baseUrl and use in the request
                request = (HttpWebRequest)WebRequest.Create(baseURL + "/views/console");
                request.Headers.Add("X-DocuSign-Authentication", authenticateStr);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                request.ContentLength = xml.ToString().Length;
                request.Method = "POST";
                // write the body of the request
                byte[] body = System.Text.Encoding.UTF8.GetBytes(xml.ToString());
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, xml.ToString().Length);
                dataStream.Close();
                // read the response
                webResponse = (HttpWebResponse)request.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                responseText = sr.ReadToEnd();
                return responseText;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    errorMessage = "Error code:: " + httpResponse.StatusCode;
                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        errorMessage += text;
                    }
                }
                return errorMessage;
            }
        }
    }
}
