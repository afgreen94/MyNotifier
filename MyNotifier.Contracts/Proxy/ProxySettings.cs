using MyNotifier.Contracts.FileIOManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Contracts.Proxy
{

    public interface IProxySettings
    {
        public FileStorageProvider ProxyHost { get; }
        bool AllowRecreateProxyFileSystem { get; }
        FileStructureSettings FileStructure { get; }
    }

    public class ProxySettings : IProxySettings //: FileServerSettings //proxy file server settings  //encapsulate paths into proxySettings 
    {
        public FileStorageProvider ProxyHost { get; set; }
        public bool AllowRecreateProxyFileSystem { get; set; } = false;

        public FileStructureSettings FileStructure { get; set; }
    }

    public class FileStructureSettings
    {
        public FileSystemObjectNameAssertWrapper RootFolder { get; set; } = new() { Name = "MyNotifier", SemanticName = "Root", Assert = true };

        public FileSystemObjectNameAssertWrapper InterestFile { get; set; } = new() { Name = "Interests", SemanticName = "Interests", Assert = false };
        public FileSystemObjectNameAssertWrapper CatalogFile { get; set; } = new() { Name = "Catalog", SemanticName = "Catalog", Assert = true };

        public FileSystemObjectNameAssertWrapper NotificationsFolder { get; set; } = new() { Name = "Notifications", SemanticName = "Notifications", Assert = false };
        public FileSystemObjectNameAssertWrapper UpdatesFolder { get; set; } = new() { Name = "Updates", SemanticName = "Updates", Assert = false };
        public FileSystemObjectNameAssertWrapper CommandsFolder { get; set; } = new() { Name = "Commands", SemanticName = "Commands", Assert = false }; //command / CommandResult 
        public FileSystemObjectNameAssertWrapper ExceptionsFolder { get; set; } = new() { Name = "Exceptions", SemanticName = "Exceptions", Assert = false };

        public FileSystemObjectNameAssertWrapper LibraryFolder { get; set; } = new() { Name = "Library", SemanticName = "Library", Assert = true };
        public FileSystemObjectNameAssertWrapper DllsFolder { get; set; } = new() { Name = "Dlls", SemanticName = "Dlls", Assert = true };
        public FileSystemObjectNameAssertWrapper UpdaterDefinitionsFolder { get; set; } = new() { Name = "UpdaterDefinitions", SemanticName = "UpdaterDefinitions", Assert = true }; //assert here should be true, maybe proxy initializer checks for non-zero count 
        public FileSystemObjectNameAssertWrapper EventModuleDefinitionsFolder = new() { Name = "EventModuleDefinitions", SemanticName = "EventModuleDefinitions", Assert = true }; //assert here is conditional, true for now 
        public FileSystemObjectNameAssertWrapper EventModuleModelsFolder = new() { Name = "EventModuleModels", SemanticName = "EventModuleModels", Assert = true }; //assert here is conditional, true for now 

        public Paths BuildPaths(IFileIOManager.IWrapper wrapper)
        {
            var ret = new Paths() { RootFolder = new() { Path = this.RootFolder.Name, Assert = this.RootFolder.Assert } };

            ret.InterestsFile = new() { Path = wrapper.BuildAppendedPath(ret.RootFolder.Path, this.InterestFile.Name), Name = this.InterestFile.Name, SemanticName = this.InterestFile.SemanticName, Assert = this.InterestFile.Assert };
            ret.CatalogFile = new() { Path = wrapper.BuildAppendedPath(ret.RootFolder.Path, this.CatalogFile.Name), Name = this.CatalogFile.Name, SemanticName = this.CatalogFile.SemanticName, Assert = this.CatalogFile.Assert };

            ret.NotificationsFolder = new() { Path = wrapper.BuildAppendedPath(ret.RootFolder.Path, this.NotificationsFolder.Name), Name = this.NotificationsFolder.Name, SemanticName = this.NotificationsFolder.SemanticName, Assert = this.NotificationsFolder.Assert };
            ret.UpdatesFolder = new() { Path = wrapper.BuildAppendedPath(ret.NotificationsFolder.Path, this.UpdatesFolder.Name), Name = this.UpdatesFolder.Name, SemanticName = this.UpdatesFolder.SemanticName, Assert = this.UpdatesFolder.Assert };
            ret.CommandsFolder = new() { Path = wrapper.BuildAppendedPath(ret.NotificationsFolder.Path, this.CommandsFolder.Name), Name = this.CommandsFolder.Name, SemanticName = this.CommandsFolder.SemanticName, Assert = this.CommandsFolder.Assert };
            ret.ExceptionsFolder = new() { Path = wrapper.BuildAppendedPath(ret.NotificationsFolder.Path, this.ExceptionsFolder.Name), Name = this.ExceptionsFolder.Name, SemanticName = this.ExceptionsFolder.SemanticName, Assert = this.ExceptionsFolder.Assert };

            ret.LibraryFolder = new() { Path = wrapper.BuildAppendedPath(ret.RootFolder.Path, this.LibraryFolder.Name), Name = this.LibraryFolder.Name, SemanticName = this.LibraryFolder.SemanticName, Assert = this.LibraryFolder.Assert };
            ret.DllsFolder = new() { Path = wrapper.BuildAppendedPath(ret.LibraryFolder.Path, this.DllsFolder.Name), Name = this.DllsFolder.Name, SemanticName = this.DllsFolder.SemanticName, Assert = this.DllsFolder.Assert };
            ret.UpdaterDefinitionsFolder = new() { Path = wrapper.BuildAppendedPath(ret.LibraryFolder.Path, this.UpdaterDefinitionsFolder.Name), Name = this.UpdaterDefinitionsFolder.Name, SemanticName = this.UpdaterDefinitionsFolder.SemanticName, Assert = this.UpdaterDefinitionsFolder.Assert };
            ret.EventModuleDefinitionsFolder = new() { Path = wrapper.BuildAppendedPath(ret.LibraryFolder.Path, this.EventModuleDefinitionsFolder.Name), Name = this.EventModuleDefinitionsFolder.Name, SemanticName = this.EventModuleDefinitionsFolder.SemanticName, Assert = this.EventModuleDefinitionsFolder.Assert };
            ret.EventModuleModelsFolder = new() { Path = wrapper.BuildAppendedPath(ret.LibraryFolder.Path, this.EventModuleModelsFolder.Name), Name = this.EventModuleModelsFolder.Name, SemanticName = this.EventModuleModelsFolder.SemanticName, Assert = this.EventModuleModelsFolder.Assert };

            return ret;
        }
    }

    public class Paths //: IEnumerable<PathAssertWrapper>
    {
        public FileSystemObjectPathAssertWrapper RootFolder { get; set; }
        public FileSystemObjectPathAssertWrapper InterestsFile { get; set; }
        public FileSystemObjectPathAssertWrapper CatalogFile { get; set; }
        public FileSystemObjectPathAssertWrapper NotificationsFolder { get; set; }
        public FileSystemObjectPathAssertWrapper UpdatesFolder { get; set; }
        public FileSystemObjectPathAssertWrapper CommandsFolder { get; set; }
        public FileSystemObjectPathAssertWrapper ExceptionsFolder { get; set; }
        public FileSystemObjectPathAssertWrapper LibraryFolder { get; set; }
        public FileSystemObjectPathAssertWrapper DllsFolder { get; set; }
        public FileSystemObjectPathAssertWrapper UpdaterDefinitionsFolder { get; set; }
        public FileSystemObjectPathAssertWrapper EventModuleDefinitionsFolder { get; set; }
        public FileSystemObjectPathAssertWrapper EventModuleModelsFolder { get; set; }

        public IEnumerator<FileSystemObjectPathAssertWrapper> GetEnumerator() => (IEnumerator<FileSystemObjectPathAssertWrapper>)new FileSystemObjectPathAssertWrapper[]
        {
            this.RootFolder,
            this.InterestsFile,
            this.CatalogFile,
            this.NotificationsFolder,
            this.UpdatesFolder,
            this.CommandsFolder,
            this.ExceptionsFolder,
            this.LibraryFolder,
            this.DllsFolder,
            this.UpdaterDefinitionsFolder,
            this.EventModuleDefinitionsFolder,
            this.EventModuleModelsFolder
        }.GetEnumerator();
    }

    public class FileSystemObjectPathAssertWrapper : FileSystemObjectNameAssertWrapper
    {
        public string Path { get; set; }

    }

    public class FileSystemObjectNameAssertWrapper
    {
        public string Name { get; set; }
        public string SemanticName { get; set; }
        public bool Assert { get; set; }
    }

}
