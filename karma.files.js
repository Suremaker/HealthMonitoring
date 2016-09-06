
module.exports = {
    testFiles: [
        'HealthMonitoring.SelfHost/Content/Scripts/angular.min.js',
        'HealthMonitoring.SelfHost/Content/Scripts/angular-mocks.js',

        'HealthMonitoring.SelfHost/Content/Scripts/functions.js',
        'HealthMonitoring.UI.UnitTests/functions.spec.js',

        'HealthMonitoring.SelfHost/Content/Scripts/advanced-directives.js',
        'HealthMonitoring.UI.UnitTests/advanced-directives.spec.js',

        'HealthMonitoring.SelfHost/Content/Scripts/advanced-filters.js',
        'HealthMonitoring.UI.UnitTests/advanced-filters.spec.js'
    ],
    coverageFiles: {
        'HealthMonitoring.SelfHost/Content/Scripts/functions.js': ['coverage'],
        'HealthMonitoring.SelfHost/Content/Scripts/advanced-directives.js': ['coverage'],
        'HealthMonitoring.SelfHost/Content/Scripts/advanced-filters.js': ['coverage']
    }

};