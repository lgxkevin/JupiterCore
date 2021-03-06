﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using jupiterCore.jupiterContext;
using jupiterCore.Models;
using Jupiter.ActionFilter;
using Jupiter.Controllers;
using Jupiter.Models;
using Microsoft.AspNetCore.Authorization;

namespace jupiterCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectMediasController : BasicController
    {
        private readonly jupiterContext.jupiterContext _context;
        private readonly IMapper _mapper;

        public ProjectMediasController(jupiterContext.jupiterContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/ProjectMedias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectMedia>>> GetProjectMedia()
        {
            return await _context.ProjectMedia.Include(x=>x.Project).ToListAsync();
        }

        // GET: api/ProjectMedias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<ProjectMedia>>> GetProjectMedia(int id)
        {
            var projectMedia = await _context.ProjectMedia.Where(x => x.ProjectId == id).Select(x => x).ToListAsync();

            if (projectMedia == null)
            {
                return NotFound();
            }

            return projectMedia;
        }

        // PUT: api/ProjectMedias/5
        [CheckModelFilter]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProjectMedia(int id, ProjectMediaModel projectMediaModel)
        {
            var result = new Result<string>();
            Type projectMediaType = typeof(ProjectMedia);
            var updateProjectMedia = await _context.ProductMedia.Where(x=>x.Id == id).FirstOrDefaultAsync();
            if (updateProjectMedia == null)
            {
                return NotFound(DataNotFound(result));
            }
            UpdateTable(projectMediaModel,projectMediaType,updateProjectMedia);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }
            return Ok(result);
        }

        // POST: api/ProjectMedias
        [CheckModelFilter]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProjectMedia>> PostProjectMedia([FromForm] ProjectMediaModel projectMediaModel)
        {
            var requestForm = Request.Form;
            var file = requestForm.Files[0];
            var result = new Result<string>();
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var newFileName = $@"{Int32.Parse(projectMediaModel.ProjectId)}-{fileName}";
            try
            {
                // add image
                bool isStoreSuccess = await StoreImage("ProductImages", newFileName, file);
                if (!isStoreSuccess)
                {
                    throw new Exception("Store image locally failed.");
                }
                //add image name to db
                ProjectMedia projectMedia = new ProjectMedia { ProjectId = Int32.Parse(projectMediaModel.ProjectId), Url = $@"Images/GalleryImages/{newFileName}" };
                await _context.ProjectMedia.AddAsync(projectMedia);
                await _context.SaveChangesAsync();

                result.Data = $@"{fileName} successfully uploaded";
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                return BadRequest(result);
            }
            return Ok(result);

        }

        // DELETE: api/ProjectMedias/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ProjectMedia>> DeleteProjectMedia(int id)
        {
            var result = new Result<string>();
            var media = await _context.ProjectMedia.FindAsync(id);
            if (media == null)
            {
                return NotFound(DataNotFound(result));
            }
            try
            {
                //remove img from folder
                DeleteImage(media.Url);
            }
            catch (Exception e)
            {
                return BadRequest("unable to delete image");
            }
            _context.ProjectMedia.Remove(media);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
            }
            return Ok(result);
        }
    }
}
