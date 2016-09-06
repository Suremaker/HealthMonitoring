// Karma configuration
// Generated on Tue Aug 30 2016 19:11:19 GMT+0300 (FLE Daylight Time)

var karmaFiles = require('./karma.files');

module.exports = function(config) {
    config.set({

        // base path that will be used to resolve all patterns (eg. files, exclude)
        basePath: '',


        // frameworks to use
        // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
        frameworks: ['jasmine'],


        // list of files / patterns to load in the browser
        files: karmaFiles.testFiles,


        // list of files to exclude
        exclude: [
        ],

        // test results reporter to use
        // possible values: 'dots', 'progress'
        // available reporters: https://npmjs.org/browse/keyword/karma-reporter
        reporters: ['progress', 'dots', 'junit', 'coverage', 'html'],
        
        htmlReporter: {
            outputDir: 'reports',
            reportName: 'UI_Unit_Tests',
            namedFiles: true
        },
        
        junitReporter: {
            outputDir: 'reports',
            outputFile: 'UI_Unit_Tests.xml',
            useBrowserName: false
        },

        preprocessors: karmaFiles.coverageFiles,

        coverageReporter: {
            dir: 'reports/ui-coverage',
            reporters: [
                // reporters not supporting the `file` property
                { type: 'html', subdir: 'report-html' },
                // reporters supporting the `file` property, use `subdir` to directly
                // output them in the `dir` directory
                { type: 'cobertura', subdir: '.', file: 'ui-coverage.xml' }
            ]
        },

        // web server port
        port: 9876,


        // enable / disable colors in the output (reporters and logs)
        colors: true,


        // level of logging
        // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
        logLevel: config.LOG_INFO,


        // enable / disable watching file and executing tests whenever any file changes
        autoWatch: true,


        // start these browsers
        // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
        browsers: ['ChromeNoSandbox', 'IE'],

        // fix for Chrome v52
        customLaunchers: {
            ChromeNoSandbox: {
                base: 'Chrome',
                flags: ['--no-sandbox']
            }
        },

        // Continuous Integration mode
        // if true, Karma captures browsers, runs the tests and exits
        singleRun: true,

        // Concurrency level
        // how many browser should be started simultaneous
        concurrency: Infinity
    });
}
