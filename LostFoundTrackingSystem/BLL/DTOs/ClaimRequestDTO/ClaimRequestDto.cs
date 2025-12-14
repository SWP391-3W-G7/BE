using BLL.DTOs; // Added to access ItemActionLogDto
﻿
﻿namespace BLL.DTOs.ClaimRequestDTO
﻿{
﻿    public class ClaimRequestDto
﻿    {
﻿        public int ClaimId { get; set; }
﻿        public DateTime? ClaimDate { get; set; }
﻿        public string? Status { get; set; }
﻿        public int? FoundItemId { get; set; }
﻿        public int? LostItemId { get; set; }
﻿        public string? FoundItemTitle { get; set; } 
﻿        public int? StudentId { get; set; }
﻿        public string? StudentName { get; set; }
﻿
﻿        public List<EvidenceDto> Evidences { get; set; } = new();
﻿        public List<ItemActionLogDto>? ActionLogs { get; set; } // Added
﻿    }
﻿
﻿    public class EvidenceDto
﻿    {
﻿        public int EvidenceId { get; set; }
﻿        public string? Title { get; set; }
﻿        public string? Description { get; set; }
﻿        public DateTime? CreatedAt { get; set; }
﻿        public List<string> ImageUrls { get; set; } = new();
﻿    }
﻿}
﻿