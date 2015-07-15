module Epsilon.NgApp.Directives {
    export class ClickOnce implements ng.IDirective {
        public restrict = 'A';

        public constructor(private $timeout: ng.ITimeoutService) {
        }

        public link($scope: ng.IScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) {
            alert("test");
            element.click(function () {
                this.$timeout(function () {
                    element.attr('disabled', 'true');
                }, 0);
            });
        }
    }
}