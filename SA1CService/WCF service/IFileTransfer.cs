using System;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace SA1CService
{
    [ServiceContract]
    public interface IFileTransfer
    {
        [OperationContract]
        Stream LoadFile(string settingName, long offset = 0);

        [OperationContract]
        void UploadFile(FileData stream);
    }

    [MessageContract]
    public class FileData: IDisposable
    {
        [MessageHeader(MustUnderstand = true)]
        public string settingName;
        [MessageHeader(MustUnderstand = true)]
        public long length;
        [MessageBodyMember(Order = 1)]
        public Stream fileByteStream;

        public FileData()
        {
            settingName = "";
            length = 0;
            fileByteStream = null;
        }

    	
		public void Dispose()
		{
			fileByteStream.Close();
			fileByteStream.Dispose();
		}
    }
}
