module Epsilon.NgApp.Directives {
    export interface TooltipScope extends ng.IScope {
    }

    export class Tooltip implements ng.IDirective {
        constructor($timeout: ng.ITimeoutService) {
            return {
                restrict: 'A',
                link: (scope: TooltipScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) => {
                    new TooltipLink(scope, element, attributes);
                }
            };
        }
    }

    export class TooltipLink {
        scope: ClickOnceScope;
        constructor(scope: TooltipScope, element: ng.IAugmentedJQuery, attributes: ng.IAttributes) {
            $(element).hover(function () {
                // on mouseenter
                $(element).tooltip('show');
            }, function () {
                // on mouseleave
                $(element).tooltip('hide');
            });
        }
    }
}