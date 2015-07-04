module NgFilters {
    export function stringFormat() {
        return function (str) {
            if (!str || arguments.length <= 1) return str;
            var args = arguments;
            for (var i = 1; i < arguments.length; i++) {
                var reg = new RegExp("\\{" + (i - 1) + "\\}", "gm");
                str = str.replace(reg, arguments[i]);
            }
            return str;
        }
    }
}