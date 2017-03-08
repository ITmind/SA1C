
namespace SA1CService
{
    /// <summary>
    /// Задача
    /// </summary>
    public enum Job
    {
        Exchange= 0,
        	
        GetFileFromServer = 1,
        SendFileToServer = 2,
        
        RemoteSave = 3,
        RemoteLoad = 4,
        
        LocalLoad = 5,
        LocalSave = 6

    }
    
    /// <summary>
    /// Статус задачи
    /// </summary>
    public enum JobStatus{
    	Process=0,
    	Error=1,
    	Complite=2
    }
    	
    public static class TypeProtocol{
    	public static string TCP {
    		get{ return "net.tcp";}
    				
    	}
    	
    	public static string HTTP {
    		get{ return "http";}
    				
    	}

    }
}
