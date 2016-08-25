
angular.module('wonga.filter', [])
    .filter('wildcardFilter', function ($filter) {

        /*  converts wildcard string to regex pattern
        @value - string to build regex pattern (with wildcards)
        @rules - regex replacement for wildcards. e.g [['*', ',*']]
        @strict - do we need strict comparing to value
        */
        function preparePattern(value, rules, strict) {
            var pattern = value.replace(/[&\/\\#,+()$~%.'":?<>{}\[\]]/g, ''),
                rules = rules || [];

            if (value == null || value == '')
                return new RegExp('^.*$', 'i', 'g');

            for (var i = 0; i < rules.length; i++) {
                pattern = value.replaceAll(rules[i][0], rules[i][1]);
            }

            if (!strict)
                return new RegExp('^.*' + pattern + '.*$', 'i', 'g');

            return new RegExp('^' + pattern + '$', 'i', 'g');
        }

        function filterByRegex(items, property, pattern) {
            var filtered = [];
            for (var i = 0; i < items.length; i++) {
                if (items[i].hasOwnProperty(property) &&
                    items[i][property] != null) {
                    if (pattern.test(items[i][property].toString())) {
                        filtered.push(items[i]);
                    }
                }
            }
            return filtered;
        }

        /*
        @items - items to be filtered (angular filter parameter)
        @filter - object which properties are used for filtering
        @strict - array of property names which should be strictly filtered
        @separator - char to split multiple search values
        */
        return function (items, filter, strict, rules) {
            var filtered = items,
                pattern,
                values,
                tmpBuffer,
                isStrictSearch;

            strict = strict || [];
            rules = rules || [['*', '.*']];

            var comparator = function (a, b) {
                return a.Id === b.Id;
            };

            if (!angular.isObject(filter) ||
               !Object.getOwnPropertyNames(filter).length)
                return filtered;

            for (var property in filter) {
                tmpBuffer = [];

                if (!angular.isString(filter[property]) ||
                    !filter.hasOwnProperty(property))
                    continue;

                if (property === '$') {
                    tmpBuffer = $filter('filter')(items, filter.$);
                } else {
                    values = filter[property].split(';');

                    for (var i = 0; i < values.length; i++) {
                        if (!values[i]) {
                            tmpBuffer = tmpBuffer.concat(filtered).unique(comparator);
                        } else {
                            isStrictSearch = strict.indexOf(property) != -1;
                            pattern = preparePattern(values[i], rules, isStrictSearch);

                            tmpBuffer = tmpBuffer
                              .concat(filterByRegex(items, property, pattern))
                              .unique(comparator);
                        }
                    }
                }

                filtered = filtered.merge(tmpBuffer, comparator);
            }

            return filtered;
        };
    })

    .filter('tagsFilter', function ($filter) {
        return function (endpoints, tags) {
            var filtered = [],
                tagProperty = 'Tags';

            tags = tags.split(';');

            angular.forEach(endpoints, function (endpoint) {
                var hasAllTags = true;
                if (endpoint.hasOwnProperty(tagProperty) &&
                    angular.isArray(endpoint[tagProperty])) {

                    angular.forEach(tags, function (tag) {
                        hasAllTags &= endpoint[tagProperty].indexOf(tag) !== -1 || !tag;
                    });

                    if (hasAllTags) filtered.push(endpoint);
                }
            });

            return filtered;

        };
    });