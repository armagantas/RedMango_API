using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/shoppingCart")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly ApplicationDbContext _dbContext;
        public ShoppingCartController(ApplicationDbContext dbContext)
        {
            _response = new();
            _dbContext = dbContext;
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                ShoppingCart shoppingCart = _dbContext.ShoppingCarts.
                    Include(u => u.CartItems).ThenInclude(u => u.MenuItem).
                    FirstOrDefault(u => u.UserId == userId);

                if(shoppingCart.CartItems != null && shoppingCart.CartItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);
                }

                _response.Result = shoppingCart;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {

            //Metot, bir kullanıcı id, menü öğesi id ve miktarı güncelleme değeri (updateQuantityBy) alır.
            //İlk olarak, veritabanından ilgili kullanıcının alışveriş sepeti ve ilgili menü öğesi bulunur.

            ShoppingCart shoppingCart = _dbContext.ShoppingCarts.FirstOrDefault(u => u.UserId == userId);
            MenuItem menuItem = _dbContext.MenuItems.FirstOrDefault(u => u.Id == menuItemId);

            //Menü öğesinin bulunup bulunmadığı kontrol edilir. Bulunamazsa, "BadRequest" hatası döndürülür.
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            if (shoppingCart == null && updateQuantityBy > 0) //Eğer alışveriş sepeti bulunamaz ve updateQuantityBy değeri 0'dan büyükse, yeni bir alışveriş sepeti oluşturulur ve sepete yeni bir öğe eklenir.
            {
                ShoppingCart newCart = new() { UserId = userId };
                _dbContext.ShoppingCarts.Add(newCart);
                _dbContext.SaveChanges();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };
                _dbContext.CartItems.Add(newCartItem);
                _dbContext.SaveChanges();
            }
            else //Eğer alışveriş sepeti zaten varsa, belirli bir menü öğesinin sepet içinde olup olmadığı kontrol edilir.
            {
                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(u => u.MenuItemId == menuItemId);
                if(cartItemInCart == null) //Eğer menü öğesi sepete daha önce eklenmemişse ve updateQuantityBy değeri 0'dan büyükse, bu öğe sepete eklenir.
                {
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };
                    _dbContext.CartItems.Add(newCartItem);
                    _dbContext.SaveChanges();
                }
                else //Eğer menü öğesi zaten sepette bulunuyorsa ve updateQuantityBy değeri 0 veya yeni miktar 0'dan küçükse, bu öğe sepetten çıkarılır. Eğer bu öğe, alışveriş sepetindeki tek öğeyse, sepet de silinir.
                {
                    int newQuantity = cartItemInCart.Quantity + updateQuantityBy;
                    if(updateQuantityBy == 0 || newQuantity <= 0)
                    {
                        _dbContext.CartItems.Remove(cartItemInCart);
                        if(shoppingCart.CartItems.Count() == 1)
                        {
                            _dbContext.ShoppingCarts.Remove(shoppingCart);
                        }
                        _dbContext.SaveChanges();
                    }
                    else //Eğer menü öğesi zaten sepette bulunuyorsa ve yeni miktar 0'dan büyükse, bu öğenin miktarı güncellenir.
                    {
                        cartItemInCart.Quantity = newQuantity;
                        _dbContext.SaveChanges();
                    }
                }
            }
            return _response;
        }
    }
}
