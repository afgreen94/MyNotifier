using MyNotifier.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.FileIOManager
{
    public interface IFileIOManagerFactory 
    {
        ICallResult<IFileIOManager> GetFileIOManager();
    }


}
