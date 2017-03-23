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

        $scope.tagInfos = {};
        
        var groupClassNames = {};

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
        
        $scope.shortestResponseTime = function (endpointGroup) {
            if (endpointGroup.length <= 0) {
                return null;
            }

            var shortestResponseTime = endpointGroup[0].LastResponseTime;

            for (var i = 0; i < endpointGroup.length; i++) {
                if (shortestResponseTime > endpointGroup[i].LastResponseTime) {
                    shortestResponseTime = endpointGroup[i].LastResponseTime;
                }
            }

            return shortestResponseTime;
        };

        $scope.longestResponseTime = function (endpointGroup) {
            if (endpointGroup.length <= 0) {
                return null;
            }

            var longestResponseTime = endpointGroup[0].LastResponseTime;

            for (var i = 0; i < endpointGroup.length; i++) {
                if (longestResponseTime < endpointGroup[i].LastResponseTime) {
                    longestResponseTime = endpointGroup[i].LastResponseTime;
                }
            }

            return longestResponseTime;
        };

        $scope.LastCheckUtc = function (endpointGroup) {
            if (endpointGroup.length <= 0) {
                return null;
            }

            var lastCheckUtc = endpointGroup[0].LastCheckUtc;

            for (var i = 0; i < endpointGroup.length; i++) {
                if (lastCheckUtc < endpointGroup[i].LastCheckUtc) {
                    lastCheckUtc = endpointGroup[i].LastCheckUtc;
                }
            }

            return lastCheckUtc;
        };
        
        $scope.getEndpointClassName = function (endpointGroup) {
            if (!groupClassNames[endpointGroup]) {
                groupClassNames[endpointGroup] = 'group' + (Object.keys(groupClassNames).length % 3 + 1);
            }
            return groupClassNames[endpointGroup];
        };
        
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

        $scope.statusFilter = function (endpoint) {
            var statuses = $scope.filters[$scope.statusFilterName];
            if (statuses.length > 0) {
                if (statuses.indexOf(endpoint.Status) < 0)
                    return false;
            }
            return true;
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
            updateAllTagInfos();
            setEndpointTagStyles();

            function updateAllTagInfos() {

                angular.forEach(groupBy(data, 'Group'), updateTagInfos);

                function updateTagInfos(endpointGroup) {

                    var tagInfoSummary = {};

                    angular.forEach(endpointGroup, getTagInfoSummary);

                    angular.forEach(Object.keys(tagInfoSummary), setTagInfo);

                    function setTagInfo(key) {
                        if (tagInfoSummary.hasOwnProperty(key)) {
                            $scope.tagInfos[key] = tagInfoSummary[key];
                        }
                    }

                    function getTagInfoSummary(endpoint) {

                        var newTags = [];

                        angular.forEach(endpoint.Tags, collateNewTags);

                        angular.forEach(newTags, addTagToTagInfoSummaryIfDoesntExist);

                        function collateNewTags(newTag) {
                            newTags.push(newTag);
                        }

                        function addTagToTagInfoSummaryIfDoesntExist(newTag) {
                            if (isUndefinedOrNullOrEmptyOrHasWhiteSpaces(newTag)) {
                                return;
                            }

                            var currentTags = [];

                            if (tagInfoSummary.hasOwnProperty(endpoint.Group)) {
                                tagInfoSummary[endpoint.Group].forEach(collateCurrentTags);
                            } else {
                                tagInfoSummary[endpoint.Group] = [];
                            }

                            if (currentTags.indexOf(newTag) === -1) {
                                tagInfoSummary[endpoint.Group].push({
                                    Tag: newTag,
                                    Id: endpoint.Id
                                });
                            }

                            function collateCurrentTags(tagInfo) {
                                currentTags.push(tagInfo.Tag);
                            }
                        }
                    }

                };
            }
            
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


function groupBy(list, key) {
    return list.reduce(function (listGroup, item) {
        (listGroup[item[key]] = listGroup[item[key]] || []).push(item);
        return listGroup;
    }, {});
};

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
