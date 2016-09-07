# Health Monitoring

This project aims to provide a tool for monitoring health of various components belonging to a bigger eco-system (like SOA or Microservice Architecture).

HealthMonitoring offers a standalone, self-hosted tool that:
* offers an WEB API for registering endpoints and obtaining health information about them,
* monitors (with configurable time periods) registered endpoints,
* offers a HTML dashboard to display the current status of monitored endpoints,
* offers an endpoint detailed HTML page to display endpoint details and health history.

### Building

To build the project, please open a powershell console in project root folder and execute: ``PS> .\make\make_local.ps1``
In order to successfuly run all the tests, a following dependencies has to be fulfiled on localhost:
* [RabbitMq](https://www.rabbitmq.com/download.html) has to be installed.

After a successful build, the root folder would contain built nuget packages (*.nupkg) that can be used to install Health Monitor or to integrate with it.

### Installation

The Health Monitoring service is self hosted and consists of 2 parts:
* the [HealthMonitoring.SelfHost](https://github.com/wongatech/HealthMonitoring/tree/master/HealthMonitoring.SelfHost) project is a console application that is responsible for:
  * providing management API,
  * providing HTML UI (web pages),
  * persisting Health Monitor data,
* the [HealthMonitoring.Monitors.SelfHost](https://github.com/wongatech/HealthMonitoring/tree/master/HealthMonitoring.Monitors.SelfHost) project is a console application that is responsible for running monitors.

The easiest way to install it is to:

1. install a **HealthMonitoring.Service-deploy** nuget package in target folder for the API/website,
  *  (optionally) edit **HealthMonitoring.SelfHost.exe.config** to customise API host settings
2. install a **HealthMonitoring.Monitors.Service-deploy** nuget package in target folder for the monitor process,
  * make a **monitors** directory in target folder where *HealthMonitoring.Monitors.SelfHost.exe* is,
  * install one or more **HealthMonitoring.Monitors.XXX-deploy** monitor packages in **monitors** directory,
  * edit **HealthMonitoring.Monitors.SelfHost.exe.config** and specify proper URL for *HealthMonitoringUrl* setting (if changed),
  * (optionally) edit **HealthMonitoring.Monitors.SelfHost.exe.config** and add monitor specific configuration,
3. run **HealthMonitoring.SelfHost.exe** to start a console application for API or run **install_service.cmd** (located in API instalation folder) to register the health monitor API as a **windows service**
4. run **HealthMonitoring.Monitors.SelfHost.exe** to start a console application for monitors or run **install_service.cmd** (located in monitor instalation folder) to register the health monitor API as a **windows service**

### Running

If health monitor is installed with default settings and it is running, it's home page would be available at [http://localhost:9000/](http://localhost:9000/) address.
It will display home page of the Health Monitor as well as provide urls to:
* API documentation (with ability to execute API commands),
* to the dashboard,
* to project site (this page).
 
#### Registering a new endpoint

The endpoint registration has to be done via WEB API and it can be done via API documentation page ([http://localhost:9000/swagger/ui/index](http://localhost:9000/swagger/ui/index)) or any tool (like powershell command, or curl) that allows to make HTTP requests.

A ``POST /api/endpoints/register`` operation allows to do that.
Below, there is an example request body to register a monitoring for http://google.com

```json
{
  "Name": "Google",
  "Address": "http://google.com",
  "MonitorType": "http",
  "Group": "My group"
}
```

Please note that **MonitorType** value has to be one of supported monitors (they are installed as plugins), so in this scenario the **HealthMonitoring.Monitors.Http** monitor plugin has to be installed.
To list the currently supported monitors, please use ``GET /api/monitors``

### Contributing

Any kind of contribution is welcome :)

To contribute please:
* fork the repo,
* write code, unit tests and acceptance tests (if applicable),
* add nuspec file for the new plugin (if applicable) and ensure that it is being packaged with ``PS> .\make\make_local.ps1``,
* ensure that ``PS> .\make\make_local.ps1`` finish successfuly,
* update README.MD if necesarry,
* make a pull request.

### More details
To see more details, please visit the Wiki page of the project.
