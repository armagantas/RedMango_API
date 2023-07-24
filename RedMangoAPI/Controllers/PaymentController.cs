using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using Stripe;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly IConfiguration _congifuration;
        private readonly ApplicationDbContext _dbContext;
        public PaymentController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _response = new();
            _dbContext = dbContext;
            _congifuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _dbContext.ShoppingCarts.Include(u => u.CartItems).ThenInclude(u => u.MenuItem).FirstOrDefault(u => u.UserId == userId);

            if(shoppingCart == null || shoppingCart.CartItems == null || shoppingCart.CartItems.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            #region Create Payment Intent
            StripeConfiguration.ApiKey = _congifuration["StripeSettings:SecretKey"];
            shoppingCart.CartTotal= shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);

            PaymentIntentCreateOptions options = new ()
            {
                Amount = (int)(shoppingCart.CartTotal*100),
                Currency = "usd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            PaymentIntentService service = new();
            PaymentIntent response = service.Create(options);
            shoppingCart.StripePaymentIntentId = response.Id;
            shoppingCart.ClientSecret = response.ClientSecret;

            #endregion
            _response.Result = shoppingCart;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

    }
}
