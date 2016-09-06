///<reference path="~/HealthMonitoring.SelfHost/Content/Scripts/functions.js"/>

describe("functions test", function () {

    describe("String.replaceAll()", function () {
        it("String.replaceAll() should replace all occurences", function () {
            var str1 = "some*test* string"
                .replaceAll('*', '\\')
                .replaceAll('\\', '')
                .replaceAll(' ', '');

            var str2 = "test".replaceAll('', '_');

            var str3 = "%test%".replaceAll('%', '');

            expect(str1).toEqual("someteststring");
            expect(str2).toEqual("t_e_s_t");
            expect(str3).toEqual("test");
        });
    });
    
    describe("Array.unique()", function () {

        it("Array.unique() should remove duplicated numbers", function () {
            var arr = [1, 2, 1, 3, -1, -1, 0];

            var res = arr.unique(function (a, b) { return a === b });

            expect(res.length).toEqual(5);
            expect(res).toEqual(jasmine.arrayContaining([1, 2, 3, -1, 0]));    
        });

        it("Array.unique() should remove duplicated objects", function () {
            var arr = [{ id: 0 }, { id: 1 }, { id: 2 }, { id: 2 }, { id: 2 }];

            var res = arr.unique(function (a, b) { return a.id === b.id; });

            expect(res.length).toEqual(3);
            expect(res[0].id).toEqual(0);
            expect(res[1].id).toEqual(1);
            expect(res[2].id).toEqual(2);
        });
    });

    describe("Array.merge()", function () {

        it("Array.merge() should merge arrays with numbers", function () {
            var arr1 = [1, 2, 3];
            var arr2 = [2, 3, 4];

            var comparator = function (a, b) { return a === b; };

            var result = arr1.merge(arr2, comparator);

            expect(result.length).toEqual(2);
            expect(result[0]).toEqual(2);
            expect(result[1]).toEqual(3);
        });

        it("Array.merge() should merge arrays with objects", function () {
            var arr1 = [{ id: 0 }, { id: 1 }, { id: 2 }];
            var arr2 = [{ id: 1 }, { id: 2 }, { id: 4 }];

            var comparator = function (a, b) { return a.id === b.id; };

            var result = arr1.merge(arr2, comparator);

            expect(result.length).toEqual(2);
            expect(result[0].id).toEqual(1);
            expect(result[1].id).toEqual(2);
        });
    });
    

});