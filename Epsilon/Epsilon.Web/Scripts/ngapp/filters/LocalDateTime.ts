module Epsilon.NgApp.Filters {
    export function localDateTime() {
        return function (str) {
            if (!str || arguments.length < 1) return str;
            return new Date(str).toLocaleString();
        }
    }
}