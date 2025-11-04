using IlpRepoBackend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlpRepoBackend.Domain.Persistence
{
    public interface IFeedbackHeaderResponseRepository : IGenericRepository<FeedbackHeaderResponse>
    {
        //Task<IEnumerable<FeedbackHeaderResponse>> GetByFeedbackIdAsync(int feedbackId);
    }
}
