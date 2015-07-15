module Epsilon.NgApp.Directives {
    export class ClickOnce implements ng.IDirective {
        public restrict = 'A';
        // TODO_PANOS: this is a workaround so that I can share the timeout service
        //             between the constructor and the link function.
        private static timeout: ng.ITimeoutService;

        public constructor(private $timeout: ng.ITimeoutService) {
            ClickOnce.timeout = $timeout;
        }

        public link($scope: ng.IScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) {
            element.click(function () {
                ClickOnce.timeout(function () {
                    element.attr('disabled', 'true');
                }, 0);
            });
        }

        static factory(): ng.IDirectiveFactory {
            const directive = ($timeout: ng.ITimeoutService) => {
                return new ClickOnce($timeout);
            }
            directive.$inject = ['$timeout'];
            return directive;
        }
    }
}