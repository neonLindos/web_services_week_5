using TaskManagerClient.Models;
using WebServiceClients;

namespace TaskManagerClient.Services;

public interface ITaskApiService
{
    Task<ApiResult<List<TaskItem>>> GetAllAsync();
    Task<ApiResult<TaskItem>> GetByIdAsync(int id);
    Task<ApiResult<bool>> CreateAsync(TaskItem item);
    Task<ApiResult<bool>> UpdateAsync(TaskItem item);
    Task<ApiResult<bool>> DeleteAsync(int id);
}
