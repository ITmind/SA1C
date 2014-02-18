using System;
using System.Collections.Generic;
using System.Text;
using SA1CService;
using System.ServiceModel;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;


namespace DebugHost
{
	[System.ServiceModel.ServiceContract]
	public interface IHelloRest
	{
		[System.ServiceModel.OperationContract]
		[System.ServiceModel.Web.WebGet(UriTemplate = "helloto/{name}", ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		string Hello(string name);
		
		[System.ServiceModel.OperationContract]
		[System.ServiceModel.Web.WebGet(UriTemplate = "isalive/{animal}", ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json)]
		Animal CheckIfAlive(string animal);

		[System.ServiceModel.OperationContract]
		[System.ServiceModel.Web.WebInvoke(UriTemplate = "animals", ResponseFormat = System.ServiceModel.Web.WebMessageFormat.Xml)]
		Animal[] PostAnimal(Animal animal);

		[System.ServiceModel.OperationContract]
		[System.ServiceModel.Web.WebGet(UriTemplate = "gallery/{pictureId}")]
		System.IO.Stream GetPictureThumbnail(string pictureId);

	}
	[System.Runtime.Serialization.DataContract]
	public class Animal
	{
		[System.Runtime.Serialization.DataMember]
		public bool IsAlive { get; set; }
		[System.Runtime.Serialization.DataMember]
		public string Name { get; set; }
	}


	public class HelloRestService : IHelloRest
	{
		#region IHelloRest Members

		public string Hello(string name)
		{
			return String.Format("Hello:{0}", name);
		}
		public Animal CheckIfAlive(string name)
		{
			return new Animal { IsAlive = true, Name = name };
		}
		public Animal[] PostAnimal(Animal animal)
		{
			List<Animal> a = new List<Animal>();
			a.Add(new Animal { IsAlive = false, Name = "mamut" });
			a.Add(new Animal { IsAlive = true, Name = "dog" });
			a.Add(new Animal { IsAlive = true, Name = "ape" });
			return a.ToArray();
		}
		public System.IO.Stream GetPictureThumbnail(string pictureId)
		{
			System.IO.Stream stream = System.IO.File.Open(@"ow.jpg",System.IO.FileMode.Open);
			// set the Content-Type to image/jpeg
			System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = "image/jpeg";
			return stream;
		}
		#endregion
	}


	class Program
	{
		private static readonly string HOST_URL = "http://localhost:8000/jsonp";
		
		static void Main(string[] args)
		{
			//ServiceHost host  = new ServiceHost(typeof(ServiceSA1C));
			//host.Open();
			//Console.WriteLine("Create ok");
			//Console.ReadLine();
			//host.Close();
			
			try
			{
				using (var serviceHost = GetConfiguredServiceHost(typeof(HelloRestService)
				                                                  , typeof(IHelloRest)
				                                                  , new Uri(HOST_URL)))
				{
					serviceHost.Open();
					Console.WriteLine("WCF Service is running...");
					Console.WriteLine(HOST_URL);
					Console.ReadLine();
				}
				
				//using (WebServiceHost serviceHost = new WebServiceHost(typeof(HelloRestService)))
				//{
				//	serviceHost.Open();
				//	Console.WriteLine("WCF Service is running...");
				//	Console.ReadLine();
				//	serviceHost.Close();
				//}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.ReadLine();
			}
		}
		
		private static ServiceHost GetConfiguredServiceHost(Type typeService, Type typeContract, Uri uri)
		{
			var serviceHost = new WebServiceHost(typeService);

			var binding = new WebHttpBinding();
			// enabling jsonp output on ResponseFormat = WebMessageF
			
			binding.CrossDomainScriptAccessEnabled = true;

			serviceHost.AddServiceEndpoint(typeContract, binding, uri);
			Console.WriteLine(uri.AbsoluteUri);

			return serviceHost;
		}
	}
}
