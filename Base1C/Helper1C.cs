using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace _1C
{
    public static class Helper1C
    {
        public static EnumVersion1C GetVersionFromFilePatch(string filePath)
        {
            int index = filePath.IndexOf(@"\1Cv81\");
            if (index != -1)
            {
                return EnumVersion1C.v81;
            }

            index = filePath.IndexOf(@"\1Cv8\");
            if (index != -1)
            {
                return EnumVersion1C.v8;
            }

            return EnumVersion1C.v82;
        }

        /// <summary>
        /// Определяем базы по записям в файлах *.v8i
        /// </summary>
        /// <returns></returns>
        public static List<BaseInfo> GetBasesFromAddData()
        {
            List<BaseInfo> baseList = new List<BaseInfo>();
            BaseInfoCompater compater = new BaseInfoCompater();
            string[] filesPach = null;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.None);
            try
            {
                filesPach = Directory.GetFiles(appData + "\\1c", "*.v8i", SearchOption.AllDirectories);
            }
            catch
            {
                //обработка ошибки
                return baseList;
            }

            foreach (string filePach in filesPach)
            {
                var file = File.OpenRead(filePach);
                var reader = new StreamReader(file);
                string nameBase = string.Empty;
                string connectString = string.Empty;
                string versionString = string.Empty;
                EnumVersion1C version = 0;
                BaseInfo _baseInfo = null;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (line.IndexOf('[') > -1)
                    {
                        if(_baseInfo != null)
                        {
                            if (_baseInfo.Version1C == EnumVersion1C.NA)
                            {
                                _baseInfo.Version1C = GetVersionFromFilePatch(filePach);
                            }
                            //сначала проверим есть ли такая база в списке и если нет то добавлем
                            if (!baseList.Contains(_baseInfo, compater))
                            {
                                baseList.Add(_baseInfo);
                            }
                        }

                        _baseInfo = new BaseInfo();
                        _baseInfo.Name = line.Substring(1, line.Length - 2);

                    }
                    
                    if (line.IndexOf("Version=") > -1)
                    {
                        versionString = line.Substring(8);
                        switch (versionString)
                        {
                            case "8.1": version = EnumVersion1C.v81;
                                break;
                            case "8.2": version = EnumVersion1C.v82;
                                break;
                            default: version = 0;
                                break;
                        }
                        if (version != 0)
                        {
                            _baseInfo.Version1C = version;
                        }
                        else
                        {
                            _baseInfo.Version1C = GetVersionFromFilePatch(filePach);
                        }
                    }
                                        
                    if (line.IndexOf("Connect=") > -1)
                    {
                        
                        connectString = line.Substring(8);
                        _baseInfo.ConnectionString = connectString;
                    }
                }

            }

            return baseList;
        }
    }


    class BaseInfoCompater : IEqualityComparer<BaseInfo>
    {

        public bool Equals(BaseInfo x, BaseInfo y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the properties are equal.
            return x.ConnectionString == y.ConnectionString;
        }

        public int GetHashCode(BaseInfo obj)
        {
            throw new NotImplementedException();
        }
    }
}
