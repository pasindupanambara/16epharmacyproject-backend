using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Pharmacy.Data;
using E_Pharmacy.Models;
using E_Pharmacy.Service;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace E_Pharmacy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=epharmacyservice;AccountKey=xq6HmKUkF4TaOjyg1y+2X9dxLUxBMdcBof/n7a+T8BkJv9zFAhw/V8OmvJGbY5VtF2RojAgOdQqo58Z2mg5TfA==;EndpointSuffix=core.windows.net");


        private readonly PharmacyDataContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public OrdersController(PharmacyDataContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;

        }

        

        /*// GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder()
        {
         return await _context.Order.ToListAsync();
         }*/

        //GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrder(string field, int value1, string value2, DateTime date)
        {/*
            if (field == "pharmacy" & value2 == "date")
            {
                return await _context.Order.Where(ord => ord.Pharmacy_id == value1 && ord.Date_time.Date == date).ToListAsync();
            }

            else if (field == "pharmacy" & value2 == null)
            {
                return await _context.Order.Where(ord => ord.Pharmacy_id == value1).ToListAsync();
            }

            else if (field == "customer" & value2 == "date")
            {
                return await _context.Order.Where(ord => ord.Customer_id == value1 && ord.Date_time.Date == date).ToListAsync();
            }

            else if (field == "customer" & value2 == null)
            {
                return await _context.Order.Where(ord => ord.Customer_id == value1).ToListAsync();
            }

            else if (field == "pharmacy" & value2 == "uncompleted")
            {
                return await _context.Order.Where(ord => ord.Pharmacy_id == value1 && ord.Status == "uncompleted").ToListAsync();
            }

            else if (field == "pharmacy" & value2 == "completed")
            {
                return await _context.Order.Where(ord => ord.Pharmacy_id == value1 && ord.Status2 == "completed").ToListAsync();
            }

            else if (field == "pharmacy" & value2 == "unseen")
            {
                return await _context.Order.Where(ord => ord.Pharmacy_id == value1 && ord.Status2 == "unseen").ToListAsync();
            }

            else if (field == "all")*/            
                return await _context.Order.ToListAsync();
            }


            /*

            return NotFound();
        }*/

            // GET: api/Orders
            [HttpGet("{field}/{value}")]
            public async Task<ActionResult<IEnumerable<Order>>> GetOrder(string field, int value)
            {
                if (field == "PharmacyId")
                {
                    return await _context.Order.Where(p => p.PharmacyId == value).ToListAsync();
                }

                if (field == "CustomerId")
                {
                    return await _context.Order.Where(p => p.CustomerId == value).ToListAsync();
                }




                else if (field == "all")
                {
                    return await _context.Order.ToListAsync();
                }

                return NotFound();
            }

            // GET: api/Orders/5
            [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Order.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderID)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Orders
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]

        public async Task<ActionResult<Order>> PostOrder([FromForm] Order order)
        {
            order.ImageName = await SaveImage(order.ImageFile); //save image

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            return StatusCode(201);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Order>> DeleteOrder(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            await _context.SaveChangesAsync();

            return order;
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderID == id);
        }


            [NonAction]

            public async Task<string> SaveImage(IFormFile imageFile)
            {
                string imageName = new string(Path.GetFileNameWithoutExtension(imageFile.FileName).ToArray());
                imageName = imageName  + Path.GetExtension(imageFile.FileName);
                var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", imageName);
               // using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                //await imageFile.CopyToAsync(fileStream);
                await UploadToAzureAsync(imageFile);
            }
                return imageName;
            }
        private async Task UploadToAzureAsync(IFormFile imageFile)
        {
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference("epharmacyimages");

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            }

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(imageFile.FileName);
            cloudBlockBlob.Properties.ContentType = imageFile.ContentType;

            await cloudBlockBlob.UploadFromStreamAsync(imageFile.OpenReadStream());
        }
    }
}
