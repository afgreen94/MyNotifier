using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyNotifier.Base;
using MyNotifier.Contracts.Base;
using MyNotifier.Contracts.FileIOManager;
using MyNotifier.Contracts.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.FileIOManager
{
    public class Registrar
    {
        public Registrar() { }



        public void RegisterFileIOManagerServices(IServiceCollection services, FileStorageProvider provider/*, IConfiguration configuration*/)
        {
            switch (provider)
            {
                case FileStorageProvider.Local:
                    this.RegisterLocalFileIOManagerServices(services/*, configuration*/); break;
                case FileStorageProvider.GoogleDrive: 
                    this.RegisterGoogleDriveFileIOManagerServices(services/*, configuration*/); break;
                default: throw new Exception("Invalid FileStorageProvider."); //should never be reached
            }
        }


        //register all by specfic service derivative
        public void RegisterFileIOManagerServices(IServiceCollection services/*, IConfiguration configuration*/)
        {
            this.RegisterLocalFileIOManagerServices(services, true);
            this.RegisterGoogleDriveFileIOManagerServices(services, true);
        }


        private void RegisterLocalFileIOManagerServices(IServiceCollection services, /*IConfiguration configuration,*/ bool useSpecializedFileIOManagerDerivative = false)
        {
            services.AddSingleton<LocalDriveFileIOManager.IConfiguration, LocalDriveFileIOManager.Configuration>();
            services.AddScoped<ICallContext<LocalDriveFileIOManager>, CallContext<LocalDriveFileIOManager>>();           

            if (useSpecializedFileIOManagerDerivative)
                services.AddScoped<ILocalDriveFileIOManager, LocalDriveFileIOManager>();
            else
                services.AddScoped<IFileIOManager, LocalDriveFileIOManager>();
        }

        private void RegisterGoogleDriveFileIOManagerServices(IServiceCollection services/*, IConfiguration configuration*/, bool useSpecializedFileIOManagerDerivative = false)
        {
            services.AddSingleton<GoogleDriveFileIOManager.IConfiguration, GoogleDriveFileIOManager.Configuration>();
            services.AddScoped<ICallContext<GoogleDriveFileIOManager>, CallContext<GoogleDriveFileIOManager>>();

            if (useSpecializedFileIOManagerDerivative)
                services.AddScoped<IGoogleDriveFileIOManager, GoogleDriveFileIOManager>();
            else
                services.AddScoped<IFileIOManager, GoogleDriveFileIOManager>();
        } 


        private void RegisterFileIOManagerServices<FileIOManagerConfigurationServiceType, 
                                                   FileIOManagerConfigurationImplementationType, 
                                                   FileIOManagerImplementationType>(IServiceCollection services)
            where FileIOManagerConfigurationServiceType : class, FileIOManager.IConfiguration
            where FileIOManagerConfigurationImplementationType : FileIOManager.Configuration
            where FileIOManagerImplementationType : FileIOManager
        {
            //cache and configuration 

            services.AddTransient<ICallContext<FileIOManagerImplementationType>, CallContext<FileIOManagerImplementationType>>();
            services.AddTransient<IFileIOManager, FileIOManagerImplementationType>();
        }
    }
}
