# Health Monitoring

This project aims to provide a tool for monitoring health of various components belonging to a bigger eco-system (like SOA or Microservice Architecture).

HealthMonitoring offers a standalone, self-hosted tool that:
* offers an WEB API for registering components and obtaining health information about them,
* monitors (with configurable time periods) registered components,
* offers a HTML dashboard to display the current status of monitored components,
* offers an endpoint detailed HTML page to display endpoint details and health history.

### Building

To build the project, please open a powershell console in project root folder and execute: ``PS> .\make\make_local.ps1``
After a successful build, the root folder would contain built nuget packages (*.nupkg)

### Installation

The Health Monitoring service is self hosted. 
The [HealthMonitoring.SelfHost](https://github.com/wongatech/HealthMonitoring/tree/master/HealthMonitoring.SelfHost) project is a console application that offers all the functionality.

The easiest way to install it is to:
1. install a **HealthMonitoring.Service-deploy** nuget package in target folder,
2. make a **monitors** directory in target folder (where *HealthMonitoring.SelfHost.exe* is),
3. install one or more **HealthMonitoring.Monitors.XXX-deploy** monitor packages in **monitors** directory,
4. (optionally) edit **HealthMonitoring.SelfHost.exe.config** to customise host settings, 
5. run **HealthMonitoring.SelfHost.exe** to start a console application or run **install_service.cmd** to register the health monitor as a **windows service**

### Running

If health monitor is installed with default settings and it is running, it's home page would be available at [http://localhost:9000/](http://localhost:9000/) address.
It will provide urls to:
* API documentation (with ability to execute API commands),
* to the dashboard.
 
#### Registering a new component

The component registration has to be done via WEB API and it could be done via API documentation page ([http://localhost:9000/swagger/ui/index](http://localhost:9000/swagger/ui/index))

A ``POST /api/endpoints/register`` method is doing registration.
Below there is an example request body to register a monitoring for http://google.com

```json
{
  "Name": "Google",
  "Address": "http://google.com",
  "MonitorType": "http",
  "Group": "My group"
}
```

Please note that **MonitorType** value has to be one of supported monitors (they are installed as plugins).
To list the currently supported monitors, please use ``GET /api/monitors``

### More details
To see more details, please visit the Wiki page of the project.
