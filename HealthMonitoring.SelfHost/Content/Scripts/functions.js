function parseDuration(timeSpan) {
    if (timeSpan == null) {
        return 0;
    }

    var parts = timeSpan.split('.');

    var ms = 0;
    if (parts.length > 1)
        ms = +(parts[1].substring(0, 3));

    parts = parts[0].split(':');

    var mul = 1000;
    for (var i = parts.length - 1; i >= 0; --i) {
        ms += (+(parts[i])) * mul;
        mul *= 60;
    }
    return ms;
}

function formatDate(dateString) {
    if (dateString == null) { return ""; }
    return new Date(dateString).toUTCString();
}

function formatDuration(timeSpan) {
    if (timeSpan == null) { return ""; }
    return parseDuration(timeSpan) + " ms";
}

function hashColour(str, skew) {
    var hash = 0;
    for (var i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }
    var colour = '#';
    for (var i = 0; i < 3; i++) {
        var value = (hash >> (i * 8)) & 0xFF;
        if (skew && i === 1) {
            var offset = 20;
            if (value > offset) value -= offset;
            else value += 20;
        }
        if (skew && i === 2) {
            var offset = 8;
            if (value < 255 - offset) value += offset;
            else value -= offset;
        }
        colour += ('00' + value.toString(16)).substr(-2);
    }
    return colour;
}

String.prototype.replaceAll = function (search, replacement) {
    return this.split(search).join(replacement);
};

Array.prototype.unique = function (comparator) {

    for (var i = 0; i < this.length; i++) {
        for (var j = i + 1; j < this.length; j++) {
            if (comparator(this[i], this[j])) {
                this.splice(j--, 1);
            }
        }
    }
    return this;
};

Array.prototype.merge = function (arr, comparator) {
    var result = [];

    for (var i = 0; i < this.length; i++) {
        for (var j = 0; j < arr.length; j++) {
            if (comparator(this[i], arr[j])) {
                result.push(this[i]);
                arr.splice(j, 1);
                break;
            }
        }
    }
    return result;
};

Array.prototype.remove = function(value, comparator) {
    for (var i = 0; i < this.length; i++) {
        if (comparator(value, this[i]))
            this.splice(i--,1);
    }

    return this;
};