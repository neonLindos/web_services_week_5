using TaskManagerClient.Models;
using WebServiceClients;

namespace TaskManagerClient.Services;

public class TaskApiService : ApiService<TaskItem>, ITaskApiService
{
    public TaskApiService(IHttpClientFactory factory)
        : base(factory.CreateClient("TaskApi"), "api/tasks", "Задача не найдена.") { }

    public Task<ApiResult<bool>> UpdateAsync(TaskItem item) => UpdateAsync(item.Id, item);
}
