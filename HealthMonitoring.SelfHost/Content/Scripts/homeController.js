(function () {
    "use strict";

    var app = angular.module('app', ['angular.filter', 'advanced.filters']);
    app.config(function ($locationProvider) {
        $locationProvider.html5Mode(true);
    });

    var homeController = function($scope, $location, healthmonitorEndpointService) {
        $scope.config = null;
        $scope.alerts = [];

        $scope.tagsFilterName = 'filter-tags';
        $scope.statusFilterName = 'filter-status';

        $scope.filters = {};
        $scope.filters[$scope.tagsFilterName] = [];
        $scope.filters[$scope.statusFilterName] = [];

        $scope.filterTagStyles = {};
        $scope.endpointTagStyles = {};
        $scope.statLegend = null;

        var endpointFrequency = getEndpointUpdatingFrequency($location);
        var configFrequency = getConfigUpdatingFrequency($location);

        $scope.displayHint = function(evnt) {
            if ($scope.statLegend === null) {
                $scope.statLegend = {
                    status: evnt.target.innerHTML,
                    description: statusLegend[evnt.target.innerHTML],
                    x: evnt.clientX + 5,
                    y: evnt.clientY + 5
                };
            }
        };

        $scope.hideTooltip = function() {
            $scope.statLegend = null;
        };

        $scope.tagFilter = function(endpoint) {
            var tags = $scope.filters[$scope.tagsFilterName];
            for (var i = 0; i < tags.length; i++) {
                if (endpoint.Tags === null || endpoint.Tags.indexOf(tags[i]) < 0) {
                    return false;
                }
            }
            return true;
        };

        $scope.statusFilter = function(endpoint) {
            var statuses = $scope.filters[$scope.statusFilterName];
            if (statuses.length > 0) {
                if (statuses.indexOf(endpoint.Status) < 0)
                    return false;
            }
            return true;
        };

        $scope.addItemToFilter = function(item, filterName) {
            if ($scope.filters[filterName].indexOf(item) === -1)
                $scope.filters[filterName].push(item);
            $scope.updateLocationParams(filterName);
            setFilterTagStyles($scope);
        };

        $scope.removeItemFromFilter = function(item, filterName) {
            var index = $scope.filters[filterName].indexOf(item);
            $scope.filters[filterName].splice(index, 1);
            $scope.updateLocationParams(filterName);
            setFilterTagStyles($scope);
        };

        $scope.updateLocationParams = function(filterName) {
            $location.search(filterName, arrayToParamString($scope.filters[filterName]));
        };

        $scope.changeFilterTagColour = function(tag, hover) {
            $scope.filterTagStyles[tag] = { background: hashColour(tag, hover) };
        };

        $scope.changeTagColour = function(endpointId, tag, hover) {
            $scope.endpointTagStyles[endpointId][tag] = { background: hashColour(tag, hover) };
        };

        $scope.valueComparator = function(a, b) { return a === b; };

        $scope.$on('$locationChangeSuccess',
            function() {
                initFiltersFromUrl($scope, $location);
            });

        $scope.parseDuration = parseDuration;
        $scope.formatDuration = formatDuration;

        var onEndpointsLoaded = function(data) {
            var setEndpointTagStyles = function() {
                angular.forEach($scope.endpoints,
                    function(endpoint) {
                        $scope.endpointTagStyles[endpoint.Id] = {};
                        angular.forEach(endpoint.Tags,
                            function(tag) {
                                $scope.endpointTagStyles[endpoint.Id][tag] = { background: hashColour(tag, false) };
                            });
                    });
            };

            $scope.endpoints = data;
            setEndpointTagStyles();
        };

        var onEndpointsFailed = function(error) {
            $scope.endpoints = null;
        };

        var onConfigLoaded = function(data) {
            $scope.config = data;
        };

        var onConfigFailed = function(error) {
            $scope.config = {
                Dashboard: { Title: "-- connection issue --" },
                Version: "-- connection issue --"
            };
        };

        var getEndpoints = function() {
            healthmonitorEndpointService.getEndpoints().then(onEndpointsLoaded, onEndpointsFailed);
        };

        var getConfig = function() {
            healthmonitorEndpointService.getConfig().then(onConfigLoaded, onConfigFailed);
        };

        getEndpoints();
        getConfig();

        setInterval(getEndpoints, endpointFrequency);
        setInterval(getConfig, configFrequency);
    };

    app.controller("HomeController", ["$scope", "$location", "healthmonitorEndpointService", homeController]);

}());


function getEndpointUpdatingFrequency(location) {
    var param = location.search()["endpoint-frequency"];
    var freq = parseInt(param);
    return freq >= 1000 ? freq : 5000;
}

function getConfigUpdatingFrequency(location) {
    var param = location.search()["config-frequency"];
    var freq = parseInt(param);
    return freq >= 5000 ? freq : 20000;
}

function setFilterTagStyles(scope) {
    angular.forEach(scope.filters[scope.tagsFilterName], function (tag) {
        scope.filterTagStyles[tag] = { background: hashColour(tag, false) };
    });
}

function initFiltersFromUrl(scope, location) {
    var tagsFilter = location.search()[scope.tagsFilterName];
    var statusFilter = location.search()[scope.statusFilterName];

    if (tagsFilter !== undefined && tagsFilter !== null)
        scope.filters[scope.tagsFilterName] = arrayFromParamString(tagsFilter);
    else
        scope.filters[scope.tagsFilterName] = [];

    if (statusFilter !== undefined && statusFilter !== null)
        scope.filters[scope.statusFilterName] = arrayFromParamString(statusFilter);
    else
        scope.filters[scope.statusFilterName] = [];

    setFilterTagStyles(scope);
}

function arrayToParamString(items) {
    var params = "";
    for (var i = 0; i < items.length; i++) {
        params += items[i] + ";";
    }
    return params;
}

function arrayFromParamString(str) {
    return str.split(";").filter(Boolean);
}