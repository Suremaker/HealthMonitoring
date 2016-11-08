var statusLegend = {};

statusLegend['notRun'] = 'Endpoint does not have a health status yet';
statusLegend['notExists'] = 'Endpoint does not exist, for example HTTP 404';
statusLegend['offline'] = 'Endpoint is offline or not accepting requests, for example HTTP 503';
statusLegend['healthy'] = 'Endpoint health status is successful';
statusLegend['faulty'] = 'Endpoint is not healthy, for example HTTP 500, a communication error occured or the endpoint definition is invalid';
statusLegend['unhealthy'] = 'Endpoint is working but it has performance issues or is degraded';
statusLegend['timedOut'] = 'Endpoint exceeded time out or monitor communication time out';