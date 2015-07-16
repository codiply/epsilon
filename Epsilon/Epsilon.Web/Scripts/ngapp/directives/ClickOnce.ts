module Epsilon.NgApp.Directives {
    export interface ClickOnceScope extends ng.IScope {

    }

    export class ClickOnce implements ng.IDirective {
        constructor($timeout: ng.ITimeoutService) {
            return {
                restrict: 'A',
                link: (scope: ClickOnceScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new ClickOnceLink(scope, element, attributes, $timeout);
                }
            };
        }
    }

    export class ClickOnceLink {
        scope: ClickOnceScope;
        constructor(scope: ClickOnceScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes, $timeout: ng.ITimeoutService) {
            this.scope = scope;
            element.click(function () {
                $timeout(function () {
                    element.attr('disabled', 'true');
                }, 0);
            });
        }
    }
}