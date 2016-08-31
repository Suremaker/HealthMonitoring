

angular.module('advanced.directives', [])
    .directive('wgMultiselect',
        ['$timeout', function ($timeout) {
            return {
                replace: true,
                templateUrl: '/static/templates/wg-multiselect.html',
                scope: {
                    items: '=',
                    selected: '=',
                    afterSelect: '&'
                },
                link: function (scope, elem, attr) {
                    var prefixId = 'wg-selected-';
                    scope.title = 'status...';
                    scope.showCheckboxes = false;
                    scope.expanded = false;
                    scope.selected = scope.selected || [];

                    scope.toggleExpandSelect = function () {
                        scope.showCheckboxes = !scope.showCheckboxes;
                    };

                    scope.onSelect = function (item) {
                        var index;
                        if (!item) {
                            setCheckboxes(false, scope.selected);
                            scope.selected = [];
                        } else if (item === 'all') {
                            scope.selected = angular.copy(scope.items);
                            setCheckboxes(true, scope.selected);
                        } else {
                            index = scope.selected.indexOf(item);
                            if (index === -1) {
                                scope.selected.push(item);
                            } else {
                                scope.selected.splice(index, 1);
                            }
                        }

                        changeTitle();

                        // pass selected items to parent scope
                        scope.afterSelect()(scope.selected);
                    };

                    function changeTitle() {
                        var len = scope.selected.length;
                        if (!len)
                            scope.title = 'status...';
                        else if(len === 1)
                            scope.title = 'status: ' + scope.selected[0];
                        else
                            scope.title = 'statuses: ' + scope.selected[0] + ',' + scope.selected[1] + ',...';
                    }

                    function setCheckboxes(value, items) {
                        angular.forEach(items, function (item) {
                            document.getElementById(prefixId + item).checked = value;
                        });
                    }

                    changeTitle();

                    // after dom has finished rendering
                    $timeout(function () {
                        setCheckboxes(true, scope.selected);
                    });
                }
            }
        }]);
