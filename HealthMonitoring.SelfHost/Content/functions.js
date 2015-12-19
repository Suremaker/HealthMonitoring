function parseDuration(timeSpan) {
    if (timeSpan == null) {
        return 0;
    }

    var parts = timeSpan.split(".");

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