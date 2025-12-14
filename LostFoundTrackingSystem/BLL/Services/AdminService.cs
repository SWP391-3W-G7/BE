using BLL.DTOs.AdminDTO;
﻿using BLL.IServices;
﻿using DAL.IRepositories;
﻿using DAL.Models;
﻿
﻿namespace BLL.Services
﻿{
﻿    public class AdminService : IAdminService
﻿    {
﻿        private readonly ICampusRepository _campusRepo;
﻿        private readonly IUserRepository _userRepo;
﻿
﻿        public AdminService(ICampusRepository campusRepo, IUserRepository userRepo)
﻿        {
﻿            _campusRepo = campusRepo;
﻿            _userRepo = userRepo;
﻿        }
﻿
﻿        // 1. Chức năng tạo Campus
﻿        public async Task<Campus> CreateCampusAsync(CreateCampusRequest request)
﻿        {
﻿            var campus = new Campus
﻿            {
﻿                CampusName = request.CampusName,
﻿                Address = request.Address,
﻿                StorageLocation = request.StorageLocation
﻿            };
﻿
﻿            await _campusRepo.AddAsync(campus);
﻿            return campus;
﻿        }
﻿
﻿        public async Task AssignRoleAndCampusAsync(AssignRoleRequest request)
﻿        {
﻿            var user = await _userRepo.GetUserByIdAsync(request.UserId);
﻿            if (user == null) throw new Exception("User not found.");
﻿
﻿            var campus = await _campusRepo.GetByIdAsync(request.CampusId);
﻿            if (campus == null) throw new Exception("Campus not found.");
﻿
﻿            user.CampusId = request.CampusId;
﻿            user.RoleId = request.RoleId;
﻿
﻿            await _userRepo.UpdateAsync(user);
﻿        }
﻿    }
﻿}
﻿