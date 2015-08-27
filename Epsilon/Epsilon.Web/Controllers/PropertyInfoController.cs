using Epsilon.Logic.Constants;
using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Resources.Common;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.PropertyInfo;
using Epsilon.Web.Models.ViewModels.Shared;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class PropertyInfoController : BaseMvcController
    {
        public const string MY_EXPLORED_PROPERTIES_SUMMARY_ACTION = "MyExploredPropertiesSummary";

        private readonly ICountryService _countryService;
        private readonly IAddressService _addressService;
        private readonly IPropertyInfoAccessService _propertyInfoAccessService;
        private readonly ITokenRewardService _tokenRewardService;

        public PropertyInfoController(
            ICountryService countryService,
            IAddressService addressService,
            IPropertyInfoAccessService propertyInfoAccessService,
            ITokenRewardService tokenRewardService)
        {
            _countryService = countryService;
            _addressService = addressService;
            _propertyInfoAccessService = propertyInfoAccessService;
            _tokenRewardService = tokenRewardService;
        }

        public ActionResult SearchProperty()
        {
            var availableCountries = _countryService.GetAvailableCountries();
            var model = new SearchPropertyViewModel
            {
                AvailableCountries = availableCountries
            };
            return View(model);
        }

        public async Task<ActionResult> GainAccess(Guid id)
        {
            var addressUniqueId = id;

            var hasCompletedSubmissions = await _addressService.AddressHasCompleteSubmissions(addressUniqueId);
            if (!hasCompletedSubmissions)
            {
                Danger(CommonResources.GenericInvalidActionMessage, true);
                return RedirectHome(false);
            }

            var entity = await _addressService.GetAddress(addressUniqueId);
            var existingAccessUniqueId = await _propertyInfoAccessService.GetExistingUnexpiredAccessUniqueId(GetUserId(), addressUniqueId);
            var model = new GainAccessViewModel
            {
                AccessUniqueId = Guid.NewGuid(),
                AddressDetails = AddressDetailsViewModel.FromEntity(entity),
                TokensCost = _tokenRewardService.GetCurrentReward(TokenRewardKey.SpendPerPropertyInfoAccess).AbsValue,
                ExistingUnexpiredAccessUniqueId = existingAccessUniqueId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GainAccessConfirmed(Guid accessUniqueId, Guid selectedAddressUniqueId)
        {
            var outcome = await _propertyInfoAccessService
                .Create(GetUserId(), GetUserIpAddress(), accessUniqueId, selectedAddressUniqueId);
            if (outcome.IsRejected)
            {
                Danger(outcome.RejectionReason, true);
                return RedirectHome(false);
            }
            else
            {
                PresentUiAlerts(outcome.UiAlerts, true);
                return RedirectToAction("ViewInfo", new { id = outcome.PropertyInfoAccessUniqueId, returnToSummary = false });
            }      
        }

        [HttpGet]
        public async Task<ActionResult> ViewInfo(Guid id, bool returnToSummary)
        {
            var accessUniqueId = id;
            var getInfoOutcome =
                await _propertyInfoAccessService.GetInfo(GetUserId(), accessUniqueId);

            if (getInfoOutcome.IsRejected)
            {
                Danger(getInfoOutcome.RejectionReason, true);
                return RedirectHome(returnToSummary);
            }

            var model = new ViewPropertyViewModel
            {
                PropertyInfo = getInfoOutcome.PropertyInfo,
                ReturnToSummary = returnToSummary
            };
            return View(model);
        }

        public ActionResult MyExploredPropertiesSummary()
        {
            return View();
        }

        private ActionResult RedirectHome(bool returnToSummary)
        {
            if (returnToSummary)
            {
                return RedirectToAction(MY_EXPLORED_PROPERTIES_SUMMARY_ACTION);
            }

            return RedirectToAction(
                AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }
    }
}