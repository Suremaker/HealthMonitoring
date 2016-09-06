///<reference path="~/HealthMonitoring.SelfHost/Content/Scripts/angular.min.js"/>
///<reference path="~/HealthMonitoring.SelfHost/Content/Scripts/angular-mocks.js"/>
///<reference path="~/HealthMonitoring.SelfHost/Content/Scripts/advanced-directives.js"/>

describe("wgMultiselect directive test", function() {
    var rootScope,
        compile,
        directive,
        scope;

    function getDirectiveHtml() {
        return '<div class="wg-multiselect"> <style> .wg-multiselect { width: 15em; display: inline-block; cursor: pointer; }' +
     '.wg-selectBox { position: relative; }' +
     '.wg-selectBox select { width: 100%; height: 21px; background-color: #222222; color: #9CA99C; border-color: black; }' +
     '.wg-over-select { position: absolute; left: 0; right: 0; top: 0; bottom: 0; }' +
     '.wg-checkboxes { border: 1px #dadada solid; position: absolute; width: 15em; background-color: #222222; }' +
     '.wg-checkboxes label { display: block; }' +
     '#checkboxes label:hover { background-color: #1e90ff; }' +
     '.helper { display: inline-block; font-size: 0.8em; margin: 0.2em; border: 1px solid gray; border-radius: 0.3em; padding: 0 0.4em 0 0.4em; }' +
     '.wg-select-item-container { text-align: left; }' +
     '.pointer { cursor: pointer; }' +
     '.noselect { -webkit-touch-callout: none; -webkit-user-select: none; -khtml-user-select: none; -moz-user-select: none; -ms-user-select: none; user-select: none; }' +
    '</style><div class="wg-selectBox" ng-click="toggleExpandSelect()"> <select> <option class="noselect">{{title}}</option> ' +
    '</select> <div class="wg-over-select"></div> </div> <div class="wg-checkboxes" ng-show="showCheckboxes"> <div class="wg-select-item-container"> ' +
    '<span ng-if="selected.length !== items.length" ng-click="onSelect(\'all\')" class="helper">&#10003;&nbsp;&nbsp;Select All</span> ' +
    '<span ng-if="selected.length" ng-click="onSelect()" class="helper">&#10007;&nbsp;&nbsp;Clear All</span> </div> ' +
    '<div ng-repeat="item in items" class="wg-select-item-container pointer"> <label for="wg-selected-{{::item}}" class="pointer noselect"> ' +
    '<input type="checkbox" id="wg-selected-{{::item}}" class="pointer" ng-click="onSelect(item)"/>{{::item}} </label> </div> </div> </div> ';
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