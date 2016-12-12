(function() {
    var app = angular.module('app', ['advanced.filters']);

    app.config(function($locationProvider) {
        $locationProvider.html5Mode(true);
    });

    var detailsController = function ($scope, $location, healthmonitorEndpointService) {

        $scope.details = {};
        $scope.tagStyles = {};
        $scope.stats = { lines: [], paths: {} };
        $scope.graph = { width: 800, height: 400 };
        $scope.statsLoading = false;
        $scope.statLegend = statusLegend;

      

        $scope.tagClick = function(tag) {
            var url = "/#?filter-tags=" + tag;
            window.open(url, '_blank');
        };

        $scope.changeTagColour = function(tag, hover) {
            $scope.tagStyles[tag] = { background: hashColour(tag, hover) };
        }

        function buildPaths(lines, maxDuration, graphDims) {

            var paths = {};

            for (var i = 0; i < lines.length; ++i) {
                var point = lines[i];
                var path = paths[point.status];
                if (path == null) {
                    paths[point.status] = path = { data: "", last: -1 }
                };

                var ox = (i + 1) * graphDims.width / lines.length;
                var nx = i * graphDims.width / lines.length;
                var ny = point.value * graphDims.height / maxDuration;
                var oy = (i + 1 < lines.length) ? (lines[i + 1].value * graphDims.height / maxDuration) : ny;
                if (path.last !== ox) {
                    path.data += "M " + ox + " " + oy + " ";
                }
                path.data += "L " + nx + " " + ny + " ";
                path.last = nx;
            }
            return paths;
        }

        function convert(stats, graphDims) {
            var maxDuration = 1;
            var lines = [];
            var oldestTime = null;
            var newestTime = null;

            for (var i = stats.length - 1; i >= 0; --i) {
                var stat = stats[i];
                var duration = parseDuration(stat.ResponseTime);
                if (maxDuration < duration) {
                    maxDuration = duration;
                }
                lines.push({
                    status: stat.Status,
                    value: duration,
                    duration: stat.ResponseTime,
                    time: stat.CheckTimeUtc
                });
            }

            if (lines.length > 0) {
                newestTime = lines[0].time;
                oldestTime = lines[lines.length - 1].time;
            }

            return {
                lines: lines,
                maxDuration: maxDuration,
                newestTime: newestTime,
                oldestTime: oldestTime,
                paths: buildPaths(lines, maxDuration, graphDims)
            };
        }

        var onDetailsLoaded = function(data) {
            $scope.details = data;

            angular.forEach($scope.details.Tags,
                function (tag) {
                    $scope.tagStyles[tag] = { background: hashColour(tag, false) };
                });
        }

        var onDetailsFailed = function(error) {
            $scope.details = {};
        }

        var onStatsLoaded = function(data) {
            $scope.graph.width = Math.max(800, data.length);
            $scope.stats = convert(data, $scope.graph);
            $scope.statsLoading = false;
        }

        var onStatsFailed = function (error) {
            $scope.stats = { lines: [], paths: {}, maxDuration: 0 };
            $scope.statsLoading = false;
        }

        var updateDetails = function () {
            healthmonitorEndpointService.getDetails($location.search().id).then(onDetailsLoaded, onDetailsFailed);
        };

        var updateStats = function() {

            if ($scope.statsLoading)
                return;

            $scope.statsLoading = true;

            healthmonitorEndpointService.getStats($location.search().id).then(onStatsLoaded, onStatsFailed);
        };

        var update = function() {
            updateDetails();
            updateStats();
        }

        $scope.parseDuration = parseDuration;
        $scope.formatDuration = formatDuration;

        $scope.displayHint = function($evt) {
            var index = Math.floor($evt.offsetX * $scope.stats.lines.length / $scope.graph.width);
            $scope.currentStat = (index < $scope.stats.lines.length) ? $scope.stats.lines[index] : null;
            $scope.toolTipPosition = { x: $evt.clientX, y: $evt.clientY };
        };

        update();
        setInterval(update, 3000);
    }

    app.controller("DetailsController", ["$scope", "$location", "healthmonitorEndpointService", detailsController]);
}());
