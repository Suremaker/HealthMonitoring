(function () {
    "use strict";

    var app = angular.module('app', ['angular.filter', 'advanced.filters']);
    app.config(function ($locationProvider) {
        $locationProvider.html5Mode(true);
    });

    var homeController = function ($scope, $location, healthmonitorEndpointService) {
        $scope.config = null;
        $scope.alerts = [];

        $scope.tagsFilterName = 'filter-tags';
        $scope.statusFilterName = 'filter-status';
        $scope.shouldGroupName = 'should-group';

        $scope.filters = {};
        $scope.filters[$scope.tagsFilterName] = [];
        $scope.filters[$scope.statusFilterName] = [];

        $scope.filterTagStyles = {};
        $scope.endpointTagStyles = {};
        $scope.statLegend = null;

        var statusesInOrderOfImportance = [
            "timedOut",
            "unhealthy",
            "faulty",
            "healthy",
            "offline",
            "notExists",
            "notRun"
        ];

        var endpointFrequency = getEndpointUpdatingFrequency($location);
        var configFrequency = getConfigUpdatingFrequency($location);

        $scope.displayHint = function (evnt) {
            if ($scope.statLegend === null) {
                $scope.statLegend = {
                    status: evnt.target.innerHTML,
                    description: statusLegend[evnt.target.innerHTML],
                    x: evnt.clientX + 5,
                    y: evnt.clientY + 5
                };
            }
        };

        $scope.getGroupColour = function (group) {
            for (var i = 0; i < statusesInOrderOfImportance.length; i++) {
                if (group[uppercaseFirstLetter(statusesInOrderOfImportance[i])] > 0) {
                    return statusesInOrderOfImportance[i];
                }
            }
            return null;
        };

        $scope.hideTooltip = function () {
            $scope.statLegend = null;
        };

        $scope.tagFilter = function (endpoint) {
            var tags = $scope.filters[$scope.tagsFilterName];
            for (var i = 0; i < tags.length; i++) {
                if (endpoint.Tags === null || endpoint.Tags.indexOf(tags[i]) < 0) {
                    return false;
                }
            }
            return true;
        };

        $scope.tagGroupedFilter = function (groupInfo) {
            var tagsAvailable = [];
            groupInfo.TagInfo.forEach(function (tagInfo) {
                tagsAvailable.push(tagInfo.Tag);
            });
            var tags = $scope.filters[$scope.tagsFilterName];
            for (var i = 0; i < tags.length; i++) {
                if (tagsAvailable.indexOf(tags[i]) < 0) {
                    return false;
                }
            }
            return true;
        };

        $scope.statusFilter = function (endpoint) {
            var statuses = $scope.filters[$scope.statusFilterName];
            if (statuses.length > 0) {
                if (statuses.indexOf(endpoint.Status) < 0)
                    return false;
            }
            return true;
        };

        $scope.statusGroupFilter = function (groupInfo) {
            var statuses = $scope.filters[$scope.statusFilterName];
            if (statuses.length === 0) {
                return true;
            }
            if (statuses.length > 0) {
                for (var i = 0; i < statuses.length; i++) {
                    if (groupInfo[uppercaseFirstLetter(statuses[i])] > 0) {
                        return true;
                    }
                }
            }
            return false;
        };

        $scope.addItemToFilter = function (item, filterName) {
            if ($scope.filters[filterName].indexOf(item) === -1)
                $scope.filters[filterName].push(item);
            $scope.updateLocationParams(filterName);
            setFilterTagStyles($scope);
        };

        $scope.removeItemFromFilter = function (item, filterName) {
            var index = $scope.filters[filterName].indexOf(item);
            $scope.filters[filterName].splice(index, 1);
            $scope.updateLocationParams(filterName);
            setFilterTagStyles($scope);
        };

        $scope.updateLocationParams = function (filterName) {
            $location.search(filterName, arrayToParamString($scope.filters[filterName]));
        };

        $scope.changeFilterTagColour = function (tag, hover) {
            $scope.filterTagStyles[tag] = { background: hashColour(tag, hover) };
        };

        $scope.changeTagColour = function (endpointId, tag, hover) {
            $scope.endpointTagStyles[endpointId][tag] = { background: hashColour(tag, hover) };
        };

        $scope.valueComparator = function(a, b) {
             return a === b;
        };

        $scope.resetGroupName = function() {
            $scope.nameFilter = "";
        };
        
        $scope.$on('$locationChangeSuccess',
            function () {
                initFiltersFromUrl($scope, $location);
            });

        $scope.toggleShoudGroup = function () {
            var searchItems = $location.search();
            if ($scope.groupGroups === true) {
                searchItems[$scope.shouldGroupName] = true;
            } else {
                searchItems[$scope.shouldGroupName] = false;
            }
            $location.search(searchItems);
        };
        
        $scope.parseDuration = parseDuration;
        $scope.formatDuration = formatDuration;

        var onEndpointsLoaded = function (data) {
            var setEndpointTagStyles = function () {
                angular.forEach($scope.endpoints,
                    function (endpoint) {
                        $scope.endpointTagStyles[endpoint.Id] = {};
                        angular.forEach(endpoint.Tags,
                            function (tag) {
                                $scope.endpointTagStyles[endpoint.Id][tag] = { background: hashColour(tag, false) };
                            });
                    });
            };

            $scope.endpoints = data;
            $scope.groupsInfo = getGroupsInfo(data);
            setEndpointTagStyles();
        };

        var onEndpointsFailed = function (error) {
            $scope.endpoints = null;
        };

        var onConfigLoaded = function (data) {
            $scope.config = data;
        };

        var onConfigFailed = function (error) {
            $scope.config = {
                Dashboard: { Title: "-- connection issue --" },
                Version: "-- connection issue --"
            };
        };
        
        var getEndpoints = function () {
            healthmonitorEndpointService.getEndpoints().then(onEndpointsLoaded, onEndpointsFailed);
        };

        var getConfig = function () {
            healthmonitorEndpointService.getConfig().then(onConfigLoaded, onConfigFailed);
        };

        getEndpoints();
        getConfig();

        setInterval(getEndpoints, endpointFrequency);
        setInterval(getConfig, configFrequency);
    };

    app.controller("HomeController", ["$scope", "$location", "healthmonitorEndpointService", homeController]);

}());

function getGroupsInfo(data) {

    var groupsInfo = {};

    angular.forEach(data, getGroupInfo);

    var groups = [];

    for (var key in groupsInfo) {
        if (groupsInfo.hasOwnProperty(key)) {
            groups.push(groupsInfo[key]);
        }
    }

    return groups;

    function getGroupInfo(item) {

        if (groupsInfo.hasOwnProperty(item.Group)) {
            groupsInfo[item.Group] = {
                Count: groupsInfo[item.Group].Count + 1,
                NotRun: item.Status === "notRun" ? groupsInfo[item.Group].NotRun + 1 : groupsInfo[item.Group].NotRun,
                NotExists: item.Status === "notExists" ? groupsInfo[item.Group].NotExists + 1 : groupsInfo[item.Group].NotExists,
                Offline: item.Status === "offline" ? groupsInfo[item.Group].Offline + 1 : groupsInfo[item.Group].Offline,
                Healthy: item.Status === "healthy" ? groupsInfo[item.Group].Healthy + 1 : groupsInfo[item.Group].Healthy,
                Faulty: item.Status === "faulty" ? groupsInfo[item.Group].Faulty + 1 : groupsInfo[item.Group].Faulty,
                Unhealthy: item.Status === "unhealthy" ? groupsInfo[item.Group].Unhealthy + 1 : groupsInfo[item.Group].Unhealthy,
                TimedOut: item.Status === "timedOut" ? groupsInfo[item.Group].TimedOut + 1 : groupsInfo[item.Group].TimedOut,
                Name: item.Group,
                TagInfo: groupsInfo[item.Group].TagInfo,
                LongestResponseTime: getLongestResponseTime(),
                ShortestResponseTime: getShortestResponseTime(),
                LastCheckUtc: item.LastCheckUtc
            }
        } else {
            groupsInfo[item.Group] = {
                Count: 1,
                NotRun: item.Status === "notRun" ? 1 : 0,
                NotExists: item.Status === "notExists" ? 1 : 0,
                Offline: item.Status === "offline" ? 1 : 0,
                Healthy: item.Status === "healthy" ? 1 : 0,
                Faulty: item.Status === "faulty" ? 1 : 0,
                Unhealthy: item.Status === "unhealthy" ? 1 : 0,
                TimedOut: item.Status === "timedOut" ? 1 : 0,
                Name: item.Group,
                TagInfo: [],
                LongestResponseTime: item.LastResponseTime,
                ShortestResponseTime: item.LastResponseTime,
                LastCheckUtc: item.LastCheckUtc
            }
        }

        angular.forEach(getNewTags(), addTagToGroupInfoIfDoesntExist);
        
        function getNewTags() {
            var newTags = [];

            angular.forEach(item.Tags, collateNewTags);

            return newTags;

            function collateNewTags(newTag) {
                newTags.push(newTag);
            }
        }

        function addTagToGroupInfoIfDoesntExist(newTag) {
            if (isUndefinedOrNullOrEmptyOrHasWhiteSpaces(newTag)) {
                return;
            }

            var currentTags = [];
            
            groupsInfo[item.Group].TagInfo.forEach(collateCurrentTags);
            
            if (currentTags.indexOf(newTag) === -1) {
                groupsInfo[item.Group].TagInfo.push({
                    Tag: newTag,
                    Id: item.Id
                });
            }

            function collateCurrentTags(tagInfo) {
                currentTags.push(tagInfo.Tag);
            }
        }

        function getLongestResponseTime() {
            return groupsInfo[item.Group].LongestResponseTime > item.LastResponseTime
                ? groupsInfo[item.Group].LongestResponseTime
                : item.LastResponseTime;
        }

        function getShortestResponseTime() {
            return groupsInfo[item.Group].ShortestResponseTime < item.LastResponseTime
                ? groupsInfo[item.Group].ShortestResponseTime
                : item.LastResponseTime;
        }
        
    }
}

function uppercaseFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}

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
    var shouldGroup = location.search()[scope.shouldGroupName];

    if (tagsFilter !== undefined && tagsFilter !== null) {
        scope.filters[scope.tagsFilterName] = arrayFromParamString(tagsFilter);
    } else {
        scope.filters[scope.tagsFilterName] = [];
    }

    if (statusFilter !== undefined && statusFilter !== null) {
        scope.filters[scope.statusFilterName] = arrayFromParamString(statusFilter);
    } else {
        scope.filters[scope.statusFilterName] = [];
    }

    if (shouldGroup === true) {
        scope.groupGroups = true;
    } else {
        scope.groupGroups = false;
    }

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

function isUndefinedOrNullOrEmptyOrHasWhiteSpaces(str) {
    return str === 'undefined' || str === null || str.match(/^ *$/) !== null;
}
