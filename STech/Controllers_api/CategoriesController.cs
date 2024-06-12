using STech.DTO;
using STech.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace STech.Controllers_api
{
    public class CategoriesController : ApiController
    {
        public async Task<IEnumerable<DanhMucDTO>> Get()
        {
            using (DbEntities db = new DbEntities())
            {
                return await db.DanhMucs
                    .Select(d => new DanhMucDTO
                    {
                        MaDM = d.MaDM,
                        TenDM = d.TenDM,
                        HinhAnh = d.HinhAnh
                    }).ToListAsync();
            }
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}