using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Management;
using System.Threading.Tasks;
using Curupira.WindowsService.Model; // For parallel processing

namespace Curupira.WindowsService.Services
{
    public class ServiceService : IServiceService
    {
        public IEnumerable<ServiceModel> GetAllServices()
        {
            // Retrieve all services using ServiceController (quick operation)
            var serviceControllers = ServiceController.GetServices();

            // Perform a single WMI query to retrieve all necessary service details
            var wmiServiceData = GetAllWmiServiceData();

            // Use parallel processing to speed up the final service model creation
            var services = new List<ServiceModel>();
            Parallel.ForEach(serviceControllers, service =>
            {
                // Ensure the WMI data exists for this service
                if (wmiServiceData.ContainsKey(service.ServiceName))
                {
                    var wmiData = wmiServiceData[service.ServiceName];

                    // Create the service model and add it to the list
                    var serviceModel = new ServiceModel
                    {
                        ServiceName = service.ServiceName,
                        Status = service.Status.ToString(),
                        Description = wmiData.Description,
                        StartType = wmiData.StartType,
                        ServiceAccount = wmiData.ServiceAccount
                    };

                    lock (services) // Ensure thread-safe addition to the list
                    {
                        services.Add(serviceModel);
                    }
                }
            });

            return services.OrderBy(p => p.ServiceName);
        }

        // Perform a single WMI query to retrieve the description, start type, and account for all services at once
        private Dictionary<string, WmiServiceData> GetAllWmiServiceData()
        {
            var serviceDataDictionary = new Dictionary<string, WmiServiceData>();

            // WMI query to retrieve all services and their properties
            using (var searcher = new ManagementObjectSearcher("SELECT Name, Description, StartMode, StartName FROM Win32_Service"))
            {
                foreach (var obj in searcher.Get())
                {
                    var serviceName = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        var wmiData = new WmiServiceData
                        {
                            Description = obj["Description"]?.ToString() ?? "No description available.",
                            StartType = obj["StartMode"]?.ToString() ?? "Unknown",
                            ServiceAccount = obj["StartName"]?.ToString() ?? "Unknown"
                        };
                        serviceDataDictionary[serviceName] = wmiData;
                    }
                }
            }

            return serviceDataDictionary;
        }

        // Helper class to store WMI service data
        private class WmiServiceData
        {
            public string Description { get; set; }
            public string StartType { get; set; }
            public string ServiceAccount { get; set; }
        }
    }
}