using System;
using System.Text;

using System.IO;
using System.Web;

using Microsoft.Win32;


namespace CsHTTPServer
{
	/// <summary>
	/// Summary description for MyServer.
	/// </summary>
	public class MyServer : CsHTTPServer 
	{
		public string Folder;
		
		public MyServer(): base() 
		{
			this.Folder = "c:\\www";
		}
	
		public MyServer(int thePort, string theFolder): base(thePort) 
		{
			this.Folder = theFolder;
		}

		public override void OnResponse(ref HTTPRequestStruct rq, ref HTTPResponseStruct rp)
		{
			string path = this.Folder + "\\" + rq.URL.Replace("/","\\");

			if (Directory.Exists(path))
			{
				if (File.Exists(path + "default.htm"))
					path += "\\default.htm";
				else if(File.Exists(path + "index.html"))
				    path += "\\index.html";    
				else
				{
					string[] dirs = Directory.GetDirectories(path);
					string[] files = Directory.GetFiles(path);

					string bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
					bodyStr += "<HTML><HEAD>\n";
					bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
					bodyStr += "</HEAD>\n";
					bodyStr += "<BODY><p>Folder listing, to do not see this add a 'default.htm' document\n<p>\n";
					for ( int i = 0; i<dirs.Length; i++)
						bodyStr += "<br><a href = \"" + rq.URL + Path.GetFileName ( dirs[i] ) + "/\">[" +  Path.GetFileName( dirs[i] ) + "]</a>\n"; 
					for ( int i = 0; i<files.Length; i++)
						bodyStr += "<br><a href = \"" + rq.URL + Path.GetFileName( files[i] ) + "\">" +  Path.GetFileName( files[i] ) + "</a>\n"; 
					bodyStr += "</BODY></HTML>\n";

					rp.BodyData = Encoding.ASCII.GetBytes(bodyStr);
					return;
				}
			}

			if (File.Exists(path))
			{
				RegistryKey rk = Registry.ClassesRoot.OpenSubKey(Path.GetExtension(path), true);

				// Get the data from a specified item in the key.
				String s = (String)rk.GetValue("Content Type");

				// Open the stream and read it back.
				rp.fs = File.Open(path, FileMode.Open);
				if ( s != "")
					rp.Headers["Content-type"] = s;
			}
			else
			{
                
				rp.status = (int)RespState.NOT_FOUND;

				string bodyStr = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">\n";
				bodyStr += "<HTML><HEAD>\n";
				bodyStr += "<META http-equiv=Content-Type content=\"text/html; charset=windows-1252\">\n";
				bodyStr += "</HEAD>\n";
				bodyStr += "<BODY>File not found!!</BODY></HTML>\n";

				rp.BodyData = Encoding.ASCII.GetBytes(bodyStr);

			}

		}
	}
}
