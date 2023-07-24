using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.DTO;
using RedMangoAPI.Utils;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        protected ApiResponse _response;

        public OrderController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId)
        {
            try
            {
                var orderHeaders = _dbContext.OrderHeaders.Include(u => u.OrderDetails) // OrderHeaders tablosundaki tüm veriler çekilir,veriler OrderDetails ve MenuItem ile ilişkilendirilir.
                    .ThenInclude(u => u.MenuItem)
                    .OrderByDescending(u => u.OrderHeaderId); // Daha sonra veriler OrderHeaderId'ye göre azalan sıralama ile sıralanır.

                if (!string.IsNullOrEmpty(userId)) //eğer userId null ya da boş değilse, veriler userId'ye göre filtrelenir. Eğer userId boş ya da null ise tüm veriler çekilir.
                {
                    _response.Result = orderHeaders.Where(u => u.ApplicationUserId == userId);
                }
                else
                {
                    _response.Result = orderHeaders;
                }

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
            
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrderById(int id)
        {
            try
            {

                if(id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeaders = _dbContext.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .Where(u => u.OrderHeaderId == id);

                if (orderHeaders == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = orderHeaders;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;

        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                    PickupEmail = orderHeaderDTO.PickupEmail,
                    PickupName = orderHeaderDTO.PickupName,
                    PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber,
                    OrderTotal = orderHeaderDTO.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = orderHeaderDTO.StripePaymentIntentID,
                    TotalItems = orderHeaderDTO.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderDTO.Status) ? SD.status_pending : orderHeaderDTO.Status,
                };

                if (ModelState.IsValid)
                {
                    _dbContext.OrderHeaders.Add(order);
                    _dbContext.SaveChanges();
                    foreach(var orderDetailDTO in orderHeaderDTO.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity,
                        };
                        _dbContext.OrderDetails.Add(orderDetails);
                    }
                    _dbContext.SaveChanges();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }


            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDTO orderHeaderUpdateDTO)
        {
            try
            {
                if(orderHeaderUpdateDTO == null || id != orderHeaderUpdateDTO.OrderHeaderId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                OrderHeader orderFromDb = _dbContext.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == id);

                if(orderFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupName))
                {
                    orderFromDb.PickupName = orderHeaderUpdateDTO.PickupName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupPhoneNumber))
                {
                    orderFromDb.PickupPhoneNumber = orderHeaderUpdateDTO.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupEmail))
                {
                    orderFromDb.PickupEmail = orderHeaderUpdateDTO.PickupEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.Status))
                {
                    orderFromDb.Status = orderHeaderUpdateDTO.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.StripePaymentIntentID))
                {
                    orderFromDb.StripePaymentIntentID = orderHeaderUpdateDTO.StripePaymentIntentID;
                }

                _dbContext.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);

            }
            catch(Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }

            return _response;
        }
    }
}
