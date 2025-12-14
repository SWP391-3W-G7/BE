using BLL.DTOs.ClaimRequestDTO; // Added
﻿
﻿namespace BLL.DTOs.FoundItemDTO
﻿{
﻿    public class FoundItemDto
﻿    {
﻿        public int FoundItemId { get; set; }
﻿        public string? Title { get; set; }
﻿        public string? Description { get; set; }
﻿        public DateTime? FoundDate { get; set; }
﻿        public string? FoundLocation { get; set; }
﻿        public string? Status { get; set; }
﻿        public int? CampusId { get; set; }
﻿        public string? CampusName { get; set; }
﻿        public int? CategoryId { get; set; }
﻿        public string? CategoryName { get; set; }
﻿        public int? CreatedBy { get; set; }
﻿        public int? StoredBy { get; set; }
﻿
﻿        public List<string> ImageUrls { get; set; } = new();
﻿        public List<ClaimRequestDto>? ClaimRequests { get; set; } // Added
﻿    }
﻿}
﻿