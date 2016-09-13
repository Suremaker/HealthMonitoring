///<reference path="~/../HealthMonitoring.SelfHost/Content/Scripts/angular.min.js"/>
///<reference path="~/Lib/angular-mocks.js"/>
///<reference path="~/../HealthMonitoring.SelfHost/Content/Scripts/advanced-directives.js"/>

describe("wgMultiselect directive test", function() {
    var rootScope,
        compile,
        directive,
        scope;

    function getDirectiveHtml() {
        return '<div class="wg-multiselect"> <div class="wg-selectBox" ng-click="toggleExpandSelect()"> </div> </div> ';
    }

    beforeEach(module('advanced.directives'));

    beforeEach(function () {
        inject(function ($compile, $rootScope, $templateCache) {
            rootScope = $rootScope.$new();
            compile = $compile;
            $templateCache.put("/static/templates/wg-multiselect.html", getDirectiveHtml());

            rootScope.items = ['1', '2', '3', '4', '5'];
            rootScope.afterSelect = jasmine.createSpy('afterSelect');
        });

        directive = getCompiledDirective();
        scope = directive.isolateScope();
    });

    function getCompiledDirective() {
        var compiledDirective = compile(angular
            .element('<wg-multiselect items="items" after-select="afterSelect" selected="selected"></wg-multiselect>'))(rootScope);
        rootScope.$digest();
        return compiledDirective;
    }

    it('directive should be defined', function () {
        expect(directive).toBeDefined();
    });

    it('should applied template', function () {
        expect(directive.html()).not.toEqual('');
    });
    
    it("scope.selected should be defined", function() {
        expect(scope.selected).toBeDefined();
    });

    it("scope.afterSelect() should be a function", function () {
        expect(typeof (scope.afterSelect)).toEqual('function');
    });

    it("scope.onSelect() should add not existed item to selected []", function () {
        scope.onSelect('1');

        expect(scope.selected.length).toEqual(1);
        expect(scope.selected[0]).toEqual('1');
    });

    it("scope.onSelect() should remove selected item", function () {
        scope.onSelect('1');
        scope.onSelect('1');
        expect(scope.selected.length).toEqual(0);
    });

    it("changeTitle() should change title if selected length changes", function () {
        var title1 = scope.title;

        scope.onSelect('1');
        var title2 = scope.title;

        scope.onSelect('2');
        var title3 = scope.title;

        scope.onSelect('3');
        var title4 = scope.title;

        expect(title1).toEqual('status...');
        expect(title2).toEqual('status: 1');
        expect(title3).toEqual('statuses: 1,2,...');
        expect(title4).toEqual('statuses: 1,2,...');
    });
});