using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.FileIOManager
{
    public class FileIOManagerFactory : IFileIOManagerFactory
    {
        public ICallResult<IFileIOManager> GetFileIOManager()
        {
            throw new NotImplementedException();
        }
    }
}
