(function () {
    var app = angular.module("app", ["ngRoute"]);

    app.config(function ($routeProvider) {
        $routeProvider
            .when("/dashboard/details", {
                templateUrl: "static/Content/Details/details.html",
                controller: "DetailsController"
            })
            .when("/dashboard", {
                templateUrl: "static/Content/Dashboard/dashboard.html",
                controller: "DashboardController"
            })

            .when("/repo/:username/:reponame", {
                templateUrl: "Content/html/repo.html",
                controller: "RepoController"
            })
            .otherwise({ redirectTo: "/main" });
    });
}());