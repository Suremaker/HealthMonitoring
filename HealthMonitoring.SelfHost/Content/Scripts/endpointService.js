(function () {
    var healthmonitorEndpointService = function ($http) {
        function getEndpoints() {
            return $http.get("/api/endpoints").then(function (response) {
                return response.data;
            });
        };

        function getConfig() {
            return $http.get("/api/config").then(function (response) {
                return response.data;
            });
        }

        function getDetails(id) {
            return $http.get("/api/endpoints/" + id)
                .then(function (response) {
                    return response.data;
                });
        }

        function getStats(id) {
            return $http.get("/api/endpoints/" + id + "/stats?limitDays=1")
                .then(function (response) {
                    return response.data;
                });
        }

        return {
            getEndpoints: getEndpoints,
            getConfig: getConfig,
            getDetails: getDetails,
            getStats: getStats
        };
    };

    var module = angular.module('app');

    module.factory('healthmonitorEndpointService', healthmonitorEndpointService);
}());