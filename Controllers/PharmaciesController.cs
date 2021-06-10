using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Pharmacy.Data;
using E_Pharmacy.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace E_Pharmacy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmaciesController : ControllerBase
    {
        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=epharmacyservice;AccountKey=xq6HmKUkF4TaOjyg1y+2X9dxLUxBMdcBof/n7a+T8BkJv9zFAhw/V8OmvJGbY5VtF2RojAgOdQqo58Z2mg5TfA==;EndpointSuffix=core.windows.net");


        private readonly PharmacyDataContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PharmaciesController(PharmacyDataContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this._hostEnvironment = hostEnvironment;
        }

        // GET: api/Pharmacies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pharmacy>>> GetPharmacy(string field, string value)
        {
            if (field == "district")
            {
                return await _context.Pharmacy.Where(p => p.District == value).ToListAsync();
            }

            else if (field == "name")
            {
                return await _context.Pharmacy.Where(p => p.Pharmacyname == value)
                     .Select(x => new Pharmacy()
                     {
                         Id = x.Id,
                         Pharmacyname = x.Pharmacyname,
                         Address = x.Address,
                         District = x.District,
                         TeleNo = x.TeleNo,
                         Email = x.Email,
                         RegNo = x.RegNo,
                         ImageSrc = String.Format("https://epharmacyservice.blob.core.windows.net/epharmacyimages/{0}",  x.Pharmacyimagename)
                     })
                     .ToListAsync();
            }

            else if (field == "Byid")
            {
                var intid = Convert.ToInt32(value);
                return await _context.Pharmacy.Where(p => p.Id == intid)
                     .Select(x => new Pharmacy()
                     {
                         Id = x.Id,
                         RegNo = x.RegNo,
                         Pharmacyname = x.Pharmacyname,
                         Address = x.Address,
                         District = x.District,
                         Email = x.Email,
                         TeleNo = x.TeleNo,
                         Password = x.Password,
                         Pharmacyimagename = x.Pharmacyimagename,
                         ImageSrc = String.Format("https://epharmacyservice.blob.core.windows.net/epharmacyimages/{0}", x.Pharmacyimagename)
                     })
                     .ToListAsync();


            }

            else if (field == "all")
            {
                return await _context.Pharmacy.ToListAsync();
            }

            return NotFound();
        }




        // GET: api/Pharmacies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pharmacy>> GetPharmacy(int id)
        {
            var pharmacy = await _context.Pharmacy.FindAsync(id);

            if (pharmacy == null)
            {
                return NotFound();
            }

            return pharmacy;
        }

        // PUT: api/Pharmacies/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPharmacy(int id, [FromForm] Pharmacy pharmacy)
        {
            if (id != pharmacy.Id)
            {
                return BadRequest();
            }

            if (pharmacy.Pharmacyimagefile != null)
            {
                DeleteImage(pharmacy.Pharmacyimagename);
                pharmacy.Pharmacyimagename = await SaveImage(pharmacy.Pharmacyimagefile);
            }

            _context.Entry(pharmacy).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PharmacyExists(id))
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

        // POST: api/Pharmacies
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Pharmacy>> PostPharmacy([FromForm]Pharmacy pharmacy)
        {

            var pharmacyWithSameEmail = _context.Pharmacy.FirstOrDefault(m => m.Email.ToLower() == pharmacy.Email.ToLower()); //check email already exit or not


            if (pharmacyWithSameEmail == null)
            {
                pharmacy.Pharmacyimagename = await SaveImage(pharmacy.Pharmacyimagefile);
                _context.Pharmacy.Add(pharmacy);
                await _context.SaveChangesAsync();

                return StatusCode(201);
                //return CreatedAtAction("GetPharmacy", new { id = pharmacy.Id }, pharmacy);

            }

            else
            {
                return BadRequest();
            }
        }

        // DELETE: api/Pharmacies/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Pharmacy>> DeletePharmacy(int id)
        {
            var pharmacy = await _context.Pharmacy.FindAsync(id);
            if (pharmacy == null)
            {
                return NotFound();
            }
            DeleteImage(pharmacy.Pharmacyimagename);
            _context.Pharmacy.Remove(pharmacy);
            await _context.SaveChangesAsync();

            return pharmacy;
        }

        private bool PharmacyExists(int id)
        {
            return _context.Pharmacy.Any(e => e.Id == id);
        }

        /*[NonAction]
        public async Task<string> SaveImage(IFormFile Pharmacyimagefile)
        {
            string PharmacyimageName = new string(PathString.GetFileNameWithoutExtension(Pharmacyimagefile.Filename).Take(10).ToArray()).Replace('', '-');
            PharmacyimageName = PharmacyimageName + DateTime.Now.Tostring("yymmssfff") + Path.GetExtension(Pharmacyimagefile.Filename);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", PharmacyimageName);
            using (var fileStream = new FileStream(imagePath, FileMode.create))
            {
                await Pharmacyimagefile.CopyToasync(filestream);
            }
            return PharmacyimageName;
        }*/

        [NonAction]
        public async Task<string> SaveImage(IFormFile Pharmacyimagefile)
        {
            string Pharmacyimagename = new String(Path.GetFileNameWithoutExtension(Pharmacyimagefile.FileName).ToArray());
            Pharmacyimagename = Pharmacyimagename +  Path.GetExtension(Pharmacyimagefile.FileName);
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", Pharmacyimagename);
           // using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                // await Pharmacyimagefile.CopyToAsync(fileStream);
                await UploadToAzureAsync(Pharmacyimagefile);
            }
            return Pharmacyimagename;
        }

        [NonAction]
        public void DeleteImage(string Pharmacyimagename)
        {
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Images", Pharmacyimagename);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }

            private async Task UploadToAzureAsync(IFormFile Pharmacyimagefile)
        {
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

        var cloudBlobContainer = cloudBlobClient.GetContainerReference("epharmacyimages");

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container
    });
            }

var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(Pharmacyimagefile.FileName);
cloudBlockBlob.Properties.ContentType = Pharmacyimagefile.ContentType;

await cloudBlockBlob.UploadFromStreamAsync(Pharmacyimagefile.OpenReadStream());
        }

    }
}
