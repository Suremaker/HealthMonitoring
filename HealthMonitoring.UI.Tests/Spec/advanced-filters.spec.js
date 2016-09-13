///<reference path="~/../HealthMonitoring.SelfHost/Content/Scripts/angular.min.js"/>
///<reference path="~/Lib/angular-mocks.js"/>
///<reference path="~/../HealthMonitoring.SelfHost/Content/Scripts/functions.js"/>
///<reference path="~/../HealthMonitoring.SelfHost/Content/Scripts/advanced-filters.js"/>

describe("advanced.filters test",
    function() {
        var endpoints = [
                            {
                                "Id": "adbe75c3-abd4-4866-bc0c-e7b42bee053a",
                                "LastModifiedTime": "2016-08-31T14:53:31.9209014+00:00",
                                "Name": "failed",
                                "Address": "http://localhost:9008",
                                "MonitorType": "http",
                                "Group": "second-group",
                                "Status": "faulty",
                                "Tags": ["tag1", "tag2"]
                            },
                            {
                                "Id": "d3a7078c-f7e0-4bdb-8c88-d427337a8b81",
                                "LastModifiedTime": "2016-08-31T14:53:32.7049463+00:00",
                                "Name": "Microsoft",
                                "Address": "https://www.microsoft.com/uk-ua/",
                                "MonitorType": "http",
                                "Group": "second-group",
                                "Status": "healthy",
                                "Tags": ["tag1", "tag2", "tag3", "tag4"]
                            },
                            {
                                "Id": "a4a49e15-8b43-4a8c-9231-92d8b217e7c8",
                                "LastModifiedTime": "2016-08-31T14:53:31.6438856+00:00",
                                "Name": "Google",
                                "Address": "http://google.com",
                                "MonitorType": "http",
                                "Group": "first-group",
                                "Status": "healthy",
                                "Tags": ["tag2", "tag3", "tag4"]
                            },
                            {
                                "Id": "e0bca1b3-eeb2-4c1b-a9e6-21bb61512876",
                                "LastModifiedTime": "2016-08-31T14:53:31.7688927+00:00",
                                "Name": "ten-endpoint",
                                "Address": "http://localhost:9001",
                                "MonitorType": "http",
                                "Group": "third-group",
                                "Status": "faulty",
                                "Tags": ["tag5"]
                            },
                            {
                                "Id": "f0bc4feb-bcc4-4b79-a25f-02f3c45d6a73",
                                "LastModifiedTime": "2016-08-31T14:53:30.6168268+00:00",
                                "Name": "ksdjuh-endpoint",
                                "Address": "http://localhost:9003",
                                "MonitorType": "http",
                                "Group": "first-group",
                                "Status": "healthy",
                                "Tags": ["tag5"]
                            },
                            {
                                "Id": "c526ed01-854f-454a-9240-35b2615526ca",
                                "LastModifiedTime": "2016-08-31T14:53:29.0407367+00:00",
                                "Name": "test-endpoint",
                                "Address": "http://localhost:9002",
                                "MonitorType": "http",
                                "Group": "second-group",
                                "Status": "healthy",
                                "Tags": ["tag5"]
                            }
        ];

        describe("wildcardFilter test",
            function() {
                var filter,
                    criteria;

                beforeEach(function() {
                    module('advanced.filters');

                    inject(function($filter) {
                        filter = $filter;
                    });
                });

                it('Should return all items if group filtering is not strict',
                    function() {
                        // Arrange
                        criteria = {
                            Group: '*-group*'
                        };

                        // Act
                        var result = filter('wildcardFilter')(endpoints, criteria, null, null);

                        // Assert
                        expect(result.length).toEqual(endpoints.length);
                    });

                it('Should return only healthy endpoints if status is filtered strictly',
                    function() {
                        // Arrange
                        criteria = {
                            Status: 'healthy'
                        };

                        // Act
                        var result = filter('wildcardFilter')(endpoints, criteria, null, null);

                        // Assert
                        expect(result.length).toEqual(4);
                        expect(result[0].Status).toEqual('healthy');
                        expect(result[1].Status).toEqual('healthy');
                        expect(result[2].Status).toEqual('healthy');
                        expect(result[3].Status).toEqual('healthy');
                    });

                it('Should search one endpoint by $=Microsoft',
                    function() {
                        // Arrange
                        criteria = {
                            $: 'Microsoft'
                        };

                        // Act
                        var result = filter('wildcardFilter')(endpoints, criteria, null, null);

                        // Assert
                        expect(result.length).toEqual(1);
                        expect(result[0].Address).toEqual('https://www.microsoft.com/uk-ua/');
                    });

                it('Should return all endpoints if criteria property is not specified',
                    function() {
                        // Arrange
                        criteria = {
                            Group: ''
                        };

                        // Act
                        var result = filter('wildcardFilter')(endpoints, criteria, null, null);

                        // Assert
                        expect(result.length).toEqual(endpoints.length);
                    });

                it('Should filter by custom wildcard rules correctly',
                    function() {
                        // Arrange
                        var rules = [['%', '.']];
                        criteria = {
                            Status: 'fau%ty'
                        };

                        // Act
                        var result = filter('wildcardFilter')(endpoints, criteria, null, rules);

                        // Assert
                        expect(result.length).toEqual(2);
                        expect(result[0].Status).toEqual('faulty');
                        expect(result[1].Status).toEqual('faulty');
                    });

                it('Should filter by multiple parameters',
                    function() {
                        // Arrange
                        criteria = {
                            Status: '*lthy',
                            Group: 'second-group'
                        };
                        var strict = ['Group'];

                        // Act
                        var result = filter('wildcardFilter')(endpoints, criteria, strict, null);

                        // Assert
                        expect(result.length).toEqual(2);
                        expect(result[0]).toEqual(jasmine.objectContaining({
                            Status: 'healthy',
                            Group: 'second-group'
                        }));
                        expect(result[1]).toEqual(jasmine.objectContaining({
                            Status: 'healthy',
                            Group: 'second-group'
                        }));
                    });
            });


        describe("wildcardFilter test",
            function() {
                var filter;

                beforeEach(function() {
                    module('advanced.filters');

                    inject(function($filter) {
                        filter = $filter;
                    });
                });


                it('Should return all items if searching criteria is empty',
                    function () {
                        // Arrange
                        var tags = '';

                        // Act
                        var result = filter('tagsFilter')(endpoints, tags);

                        // Assert
                        expect(result.length).toEqual(endpoints.length);
                    });

                it('Should return only items with specified tags',
                    function () {
                        // Arrange
                        var tags = 'tag1;tag2';

                        // Act
                        var result = filter('tagsFilter')(endpoints, tags);

                        // Assert
                        expect(result.length).toEqual(2);
                        expect(result[0].Tags).toEqual(jasmine.arrayContaining(['tag2', 'tag1']));
                        expect(result[1].Tags).toEqual(jasmine.arrayContaining(['tag2', 'tag1']));
                    });

            });
    });