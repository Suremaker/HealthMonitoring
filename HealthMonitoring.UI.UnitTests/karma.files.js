
module.exports = {
    testFiles: [
        '../HealthMonitoring.SelfHost/Content/Scripts/angular.min.js',
        'Lib/angular-mocks.js',

        '../HealthMonitoring.SelfHost/Content/Scripts/functions.js',
        'Spec/functions.spec.js',

        '../HealthMonitoring.SelfHost/Content/Scripts/advanced-directives.js',
        'Spec/advanced-directives.spec.js',

        '../HealthMonitoring.SelfHost/Content/Scripts/advanced-filters.js',
        'Spec/advanced-filters.spec.js'
    ],
    coverageFiles: {
        '../HealthMonitoring.SelfHost/Content/Scripts/functions.js': ['coverage'],
        '../HealthMonitoring.SelfHost/Content/Scripts/advanced-directives.js': ['coverage'],
        '../HealthMonitoring.SelfHost/Content/Scripts/advanced-filters.js': ['coverage']
    }

};