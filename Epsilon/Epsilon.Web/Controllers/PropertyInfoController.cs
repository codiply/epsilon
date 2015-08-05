using Epsilon.Logic.Constants;
using Epsilon.Logic.Services.Interfaces;
using Epsilon.Web.Controllers.BaseControllers;
using Epsilon.Web.Models.ViewModels.PropertyInfo;
using Epsilon.Web.Models.ViewModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Epsilon.Web.Controllers
{
    public class PropertyInfoController : BaseMvcController
    {
        private readonly ICountryService _countryService;
        private readonly IAddressService _addressService;
        private readonly IPropertyInfoAccessService _propertyInfoAccessService;

        public PropertyInfoController(
            ICountryService countryService,
            IAddressService addressService,
            IPropertyInfoAccessService propertyInfoAccessService)
        {
            _countryService = countryService;
            _addressService = addressService;
            _propertyInfoAccessService = propertyInfoAccessService;
        }

        public ActionResult Search()
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
            var entity = await _addressService.GetAddress(addressUniqueId);
            var model = new GainAccessViewModel
            {
                AccessUniqueId = Guid.NewGuid(),
                AddressDetails = AddressDetailsViewModel.FromEntity(entity)
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
            }
            else
            {
                PresentUiAlerts(outcome.UiAlerts, true);
            }

            return RedirectToAction(
                AppConstant.AUTHENTICATED_USER_HOME_ACTION,
                AppConstant.AUTHENTICATED_USER_HOME_CONTROLLER);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult View(Guid accessUniqueId, bool returnToSummary)
        {
            throw new NotImplementedException();
        }

        public ActionResult MyExploredProperties()
        {
            return View();
        }
    }
}