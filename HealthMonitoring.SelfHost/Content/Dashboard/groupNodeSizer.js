var GroupNodeSizer = function (widthAspectRatio) {

    function findRowCount(count) {
        for (var i = 1;; i++) {
            var total = i * i * widthAspectRatio;
            if (total >= count)
                return i;
        }
    };

    function floor2dp(value) {
        return Math.floor(value * 100) / 100.0;
    }

    this.getNodeSize = function (nodeCount) {
        var rowCount = findRowCount(nodeCount);
        var percentHeight = floor2dp(100.0 / rowCount);
        var percentWidth = percentHeight / widthAspectRatio;
        return { percentWidth: percentWidth, percentHeight: percentHeight };
    }
}