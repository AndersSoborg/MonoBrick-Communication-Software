using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Utilities
{
		/*
	 *    HTTPGet.cs | C# .NET 2.0 HTTP GET Class
	 *    Copyright (c) 2008, Corey Goldberg
	 *
	 *    HTTPGet.cs is free software; you can redistribute it and/or modify
	 *    it under the terms of the GNU General Public License as published by
	 *    the Free Software Foundation; either version 3 of the License, or
	 *    (at your option) any later version.
    */
	public class HTTPGet
	{
	    private HttpWebRequest request;
	    private HttpWebResponse response; 
	    
	    private string responseBody;
	    private string escapedBody;
	    private int statusCode;
	    private double responseTime;

	    public string ResponseBody {get { return responseBody; }}
	    public string EscapedBody {get { return GetEscapedBody(); }}
	    public int StatusCode {get { return statusCode; }}
	    public double ResponseTime {get { return responseTime; }}
	    public string Headers {get { return GetHeaders(); }}
	    public string StatusLine {get { return GetStatusLine(); }}



	    public void Request(string url)
	    {
	        Stopwatch timer = new Stopwatch();
	        StringBuilder respBody = new StringBuilder();

	        this.request = (HttpWebRequest)WebRequest.Create(url);

	        try
	        {
	            timer.Start(); 
	            this.response = (HttpWebResponse)this.request.GetResponse();
	            byte[] buf = new byte[8192];
	            Stream respStream = this.response.GetResponseStream();
	            int count = 0;
	            do
	            {
	                count = respStream.Read(buf, 0, buf.Length);
	                if (count != 0)
	                    respBody.Append(Encoding.ASCII.GetString(buf, 0, count));
	            }
	            while (count > 0);
	            timer.Stop();

	            this.responseBody = respBody.ToString();
	            this.statusCode = (int)(HttpStatusCode)this.response.StatusCode;
	            this.responseTime = timer.ElapsedMilliseconds / 1000.0;
	        }
	        catch (WebException ex)
	        {
	            this.response = (HttpWebResponse)ex.Response;
	            this.responseBody = "No Server Response";
	            this.escapedBody = "No Server Response";
	            this.responseTime = 0.0;
	        }
	    }



	    private string GetEscapedBody()
	    {  // HTML escaped chars
	        string escapedBody = responseBody;
	        escapedBody = escapedBody.Replace("&", "&amp;");
	        escapedBody = escapedBody.Replace("<", "&lt;");
	        escapedBody = escapedBody.Replace(">", "&gt;");
	        escapedBody = escapedBody.Replace("'", "&apos;");
	        escapedBody = escapedBody.Replace("\"", "&quot;");
	        this.escapedBody = escapedBody;

	        return escapedBody;
	    }



	    private string GetHeaders()
	    {
	        if (response == null)
	            return "No Server Response";
	        else
	        {
	            StringBuilder headers = new StringBuilder();
	            for (int i = 0; i < this.response.Headers.Count; ++i)
	                headers.Append(String.Format("{0}: {1}\n",
	                    response.Headers.Keys[i], response.Headers[i]));

	            return headers.ToString();
	        }
	    }



	    private string GetStatusLine()
	    {
	        if (response == null)
	            return "No Server Response";
	        else
	            return String.Format("HTTP/{0} {1} {2}", response.ProtocolVersion, 
	                (int)response.StatusCode, response.StatusDescription);
	    }
	}

	public class NetworkHelper{
		public static string GetExternalIP(){

			HTTPGet req = new HTTPGet();
        	req.Request("http://checkip.dyndns.org");
        	string[] a = req.ResponseBody.Split(':');
        	string a2 = a[1].Substring(1);
        	string[] a3=a2.Split('<');
        	string a4 = a3[0];
			return a4;
		}

		public static string[] GetLocalIPAddress(){
			List<string> list = new List<string>();
			try{
				IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
				foreach(IPAddress ipAddress in ipv4Addresses)
				{
					list.Add(ipAddress.ToString());
				}
			}
			catch(Exception){
				string strHostName = System.Net.Dns.GetHostName();;
				IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
				IPAddress[] addr = ipEntry.AddressList;
				foreach(IPAddress ipAddress in addr)
				{
					list.Add(ipAddress.ToString());
				}
			}
			return list.ToArray();
		}

	}
}

