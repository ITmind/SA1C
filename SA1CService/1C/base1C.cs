using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

using NLog;

namespace _1C
{
	public abstract class Base1C : IDisposable
	{
		#region Fields

		private bool disposed = false;
		protected dynamic object1c    = null;
		protected dynamic connector = null;

		public BaseInfo baseInfo
		{
			get;
			set;
		}

		#endregion

		public Base1C()
		{
			//LoadSettings();
			//Connect();
		}

		public Base1C(BaseInfo settings)
		{
			baseInfo = settings;
			//Connect();
		}

		public abstract void Connect();

		public void ReleaseObject(ref object obj)
		{
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj); obj = null;
			}
		}

		public void Dispose()
		{
			//Logger logger = LogManager.GetCurrentClassLogger();
			//logger.Debug("start Dispose");
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Base1C()
		{
			//Logger logger = LogManager.GetCurrentClassLogger();
			//logger.Debug("start Destructor");
			Dispose(false);
		}

		// Implementing IDisposable.
		private void Dispose(bool disposing)
		{
			try{
				if (!disposed)
				{
					//logger.Debug("Отключаем СОМ объект");
					// Clean up unmanaged resources
					if (object1c != null)
					{
						//logger.Debug("object1c != null");
						int res = Marshal.FinalReleaseComObject(object1c);
						//logger.Debug("FinalReleaseComObject(object1c) = " + res.ToString());
						res = Marshal.FinalReleaseComObject(connector);
						
						//logger.Debug("FinalReleaseComObject(connector) = " + res.ToString());
					}

					object1c  = null;
					connector = null;
					//logger.Debug("object1c and connector = null");
					
					// Clean up managed resources
					if (disposing)
					{
						GC.Collect();
						//logger.Debug("GC.Collect()");
					}
				}
			}
			catch{
				Logger logger = LogManager.GetCurrentClassLogger();
				logger.Error("Ошибка отключения от 1С");
			}
			disposed = true;
		}

		public void LoadSettings()
		{
			if (File.Exists("set.xml"))
			{
				XmlSerializer mySerializer = new XmlSerializer(typeof(BaseInfo));
				FileStream myFileStream = new FileStream("set.xml", FileMode.Open);
				baseInfo = (BaseInfo)mySerializer.Deserialize(myFileStream);
				myFileStream.Close();
			}
			if (baseInfo == null)
			{
				baseInfo = new BaseInfo();
			}
		}

		public void SaveSettings()
		{
			XmlSerializer mySerializer = new XmlSerializer(typeof(BaseInfo));
			StreamWriter myWriter = new StreamWriter("set.xml");
			mySerializer.Serialize(myWriter, baseInfo);
			myWriter.Close();
		}

		public dynamic GetProperty(dynamic target, string name)
		{
			if (target == null) throw new ArgumentNullException();
			BindingFlags flags = BindingFlags.Public | BindingFlags.GetProperty;
			return target.GetType().InvokeMember(name, flags, null, target, null);
		}

		public object InvokeMethod(object target, string name, params object[] args)
		{
			if (target == null) throw new ArgumentNullException();
			BindingFlags flags = BindingFlags.Public | BindingFlags.InvokeMethod;
			return target.GetType().InvokeMember(name, flags, null, target, args);
		}

		public void SetProperty(object target, string name, object value)
		{
			if (target == null) throw new ArgumentNullException();
			BindingFlags flags = BindingFlags.Public | BindingFlags.SetProperty;
			target.GetType().InvokeMember(name, flags, null, target, new object[] { value });
		}

		public object GetEnumValue(string enumName, string valueName)
		{
			object value = null;

			object ENUMS = GetProperty(object1c, "Перечисления");

			object manager = GetProperty(ENUMS, enumName);

			try
			{
				value = GetProperty(manager, valueName);
			}
			catch { }

			Marshal.ReleaseComObject(ENUMS); ENUMS = null;
			Marshal.ReleaseComObject(manager); manager = null;

			return value;
		}

		public string EnumValueToString(string enumName, ref object enumValue)
		{
			string result = "";

			object METADATA = null;
			object metaENUMS = null;
			object metaENUM = null;
			object metaValues = null;
			object metaValue = null;

			object ENUMS = GetProperty(object1c, "Перечисления");

			object manager = GetProperty(ENUMS, enumName);

			try
			{
				int index = (int)InvokeMethod(manager, "Индекс", new object[] { enumValue });
				if (index >= 0)
				{
					METADATA = GetProperty(object1c, "Метаданные");
					metaENUMS = GetProperty(METADATA, "Перечисления");
					metaENUM = GetProperty(metaENUMS, enumName);
					metaValues = GetProperty(metaENUM, "ЗначенияПеречисления");
					metaValue = InvokeMethod(metaValues, "Получить", new object[] { index });
					result = (string)GetProperty(metaValue, "Имя");
				}
			}
			catch { }

			Marshal.ReleaseComObject(ENUMS); ENUMS = null;
			Marshal.ReleaseComObject(manager); manager = null;

			if (METADATA != null) { Marshal.ReleaseComObject(METADATA); METADATA = null; }
			if (metaENUMS != null) { Marshal.ReleaseComObject(metaENUMS); metaENUMS = null; }
			if (metaENUM != null) { Marshal.ReleaseComObject(metaENUM); metaENUM = null; }
			if (metaValues != null) { Marshal.ReleaseComObject(metaValues); metaValues = null; }
			if (metaValue != null) { Marshal.ReleaseComObject(metaValue); metaValue = null; }

			return result;
		}

	}
}
