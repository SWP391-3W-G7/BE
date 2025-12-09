using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.IRepositories;
using DAL.Models;

namespace DAL.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly LostFoundTrackingSystemContext _context;
        public ImageRepository(LostFoundTrackingSystemContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Image image)
        {
            _context.Images.Add(image);
            await _context.SaveChangesAsync();
        }
    }
}
