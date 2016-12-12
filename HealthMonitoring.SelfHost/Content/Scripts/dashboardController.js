(function () {
    var app = angular.module('app', ['angular.filter', 'advanced.filters', 'advanced.directives']);

    app.config(function ($locationProvider) {
        $locationProvider.html5Mode(true);
    });

    var dashboardController = function ($scope, $location, healthmonitorEndpointService) {

        var groupClassNames = {};
        var groupNodeSizer = new GroupNodeSizer(3.0);
        var endpointFrequency = getEndpointUpdatingFrequency($location);
        var configFrequency = getConfigUpdatingFrequency($location);

        $scope.filter = {};
        $scope.tagsFilter = "";
        $scope.healthStatuses = [];
        $scope.selectedStatuses = [];

        initializeToolbar($scope, $location);

        $scope.endpoints = [];
        $scope.dashSettings = { Title: "Health Monitor" };

        var onConfigLoaded = function(data) {
            $scope.dashSettings = data.Dashboard;
            $scope.healthStatuses = data.HealthStatuses;
        }

        var onConfigFailed = function (error) {
            //do nothing
        }

        var onEndpointLoaded = function (data) {
            updateEndpointsInPlace($scope.endpoints, data);
        }

        var onEndpointFailed = function (error) {
            $scope.endpoints = [];
        }

        var getConfig = function () {
            healthmonitorEndpointService.getConfig().then(onConfigLoaded, onConfigFailed);
        }

        var update = function () {
            healthmonitorEndpointService.getEndpoints().then(onEndpointLoaded, onEndpointFailed);
        }

        getConfig();
        update();

        setInterval(update, endpointFrequency);
        setInterval(getConfig, configFrequency);

        $scope.getEndpointClassName = function (endpointGroup) {
            if (!groupClassNames[endpointGroup]) {
                groupClassNames[endpointGroup] = 'group' + (Object.keys(groupClassNames).length % 3 + 1);
            }
            return groupClassNames[endpointGroup];
        };

        $scope.formatDuration = formatDuration;

        $scope.findHighestStatus = function (endpoints) {
            var severities = { 'notRun': 0, 'healthy': 1, 'offline': 2, 'notExists': 3, 'unhealthy': 4, 'timedOut': 5, 'faulty': 6 };
            var severity = 0;
            for (var i = endpoints.length - 1; i >= 0; --i) {
                var current = severities[endpoints[i].Status];
                if (current > severity) {
                    severity = current;
                }
            }
            for (var prop in severities) {
                if (severities.hasOwnProperty(prop) && severities[prop] === severity) {
                    return prop;
                }
            }
            return 'notRun';
        }

        $scope.didAnyChanged = function (endpoints) {
            for (var i = endpoints.length - 1; i >= 0; --i) {
                if (endpoints[i].changed === true) {
                    return true;
                }
            }
            return false;
        }

        $scope.onStatusFilterChanged = function (statuses) {
            $scope.selectedStatuses = statuses;
            $scope.filter.Status = statuses.length ? statuses.join(';') : null;
            $location.search('filter-status', $scope.filter.Status);
        };

        $scope.onGroupFilterChanged = function () {
            var urlParam = $scope.filter.Group == "" ? null : $scope.filter.Group;
            $location.search('filter-group', urlParam);
        };

        $scope.onTextFilterChanged = function () {
            var urlParam = $scope.filter.$ == "" ? null : $scope.filter.$;
            $location.search('filter-text', urlParam);
        };

        $scope.onTagsFilterChanged = function () {
            var urlParam = $scope.tagsFilter || null;
            $location.search('filter-tags', urlParam);
        };

        $scope.getNodeSize = groupNodeSizer.getNodeSize;
        $scope.encodeURIComponent = encodeURIComponent;

        $scope.updateEndpointGrouping = function () {
            $location.search('group-view', $scope.endpointGrouping ? true : null);
        }
    };

    app.controller("DashboardController", ["$scope", "$location", "healthmonitorEndpointService", dashboardController]);
}());

function getEndpointUpdatingFrequency(location) {
    var param = location.search()["endpoint-frequency"];
    var freq = parseInt(param);
    return freq > 1000 ? freq : 1000;
}
function getConfigUpdatingFrequency(location) {
    var param = location.search()["config-frequency"];
    var freq = parseInt(param);
    return freq >= 5000 ? freq : 20000;
}

function updateEndpointsInPlace(endpoints, newEndpoints) {
    var newDict = {};
    var index;
    var newItem;

    //to dictionary
    for (index = newEndpoints.length - 1; index >= 0; --index) {
        var e = newEndpoints[index];
        newDict[e.Id] = e;
    }

    //update/delete
    for (index = endpoints.length - 1; index >= 0; --index) {
        var currentItem = endpoints[index];
        newItem = newDict[currentItem.Id];
        if (!newItem) {
            endpoints.splice(index, 1);
        }
        else if (currentItem.LastModifiedTime !== newItem.LastModifiedTime) {
            newItem.changed = true;
            endpoints[index] = newItem;
        } else if (currentItem.changed) {
            newItem.changed = false;
            endpoints[index] = newItem;
        }
        newDict[currentItem.Id] = null;
    }

    //insert
    for (var id in newDict) {
        if (newDict.hasOwnProperty(id)) {
            newItem = newDict[id];
            if (newItem != null) {
                newItem.changed = true;
                endpoints.push(newItem);
            }
        }
    }
}

function initializeToolbar(scope, location) {
    var textFilter = location.search()['filter-text'];
    var groupFilter = location.search()['filter-group'];
    var statusFilter = location.search()['filter-status'];
    var tagsFilter = location.search()['filter-tags'];
    var groupView = location.search()['group-view'];
    if (textFilter) {
        scope.filter.$ = textFilter;
    }
    if (groupFilter) {
        scope.filter.Group = groupFilter;
    }
    if (groupView) {
        scope.endpointGrouping = true;
    }
    if (statusFilter) {
        scope.filter.Status = statusFilter;
        scope.selectedStatuses = statusFilter.split(';');
    }
    if (tagsFilter) {
        scope.tagsFilter = tagsFilter;
    }
}
