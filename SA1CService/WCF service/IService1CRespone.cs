using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SA1CService
{
    [ServiceContract]
    interface IControl
    {
        [OperationContract(IsOneWay = true)]
        void OnUnloadingFrom1C();

    }
}
